using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;

namespace Nop.Web.Framework.Security
{
    /// <summary>
    /// File permission helper
    /// </summary>
    public static class FilePermissionHelper
    {
        /// <summary>
        /// Check permissions
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="checkRead">Check read</param>
        /// <param name="checkWrite">Check write</param>
        /// <param name="checkModify">Check modify</param>
        /// <param name="checkDelete">Check delete</param>
        /// <returns>Result</returns>
        public static bool CheckPermissions(string path, bool checkRead, bool checkWrite, bool checkModify, bool checkDelete)
        {
            var result = false;

            switch (Environment.OSVersion.Platform)
            {
                    case PlatformID.Win32NT:
                        result = CheckPermissionsInWindows(path, checkRead, checkWrite, checkModify, checkDelete);
                        break;
                    case PlatformID.Unix:
                        result = CheckPermissionsInUnix(path, checkRead, checkWrite, checkModify, checkDelete);
                        break;
            }

            return result;
        }

        private static void CheckAccessRule(FileSystemAccessRule rule, ref bool deleteIsDeny, ref bool modifyIsDeny,
            ref bool readIsDeny, ref bool writeIsDeny, ref bool deleteIsAllow, ref bool modifyIsAllow, ref bool readIsAllow,
            ref bool writeIsAllow)
        {
            bool CheckAccessRule(FileSystemAccessRule fileSystemAccessRule, FileSystemRights fileSystemRights)
            {
                return (fileSystemRights & fileSystemAccessRule.FileSystemRights) == fileSystemRights;
            }

            switch (rule.AccessControlType)
            {
                case AccessControlType.Deny:
                    if (CheckAccessRule(rule, FileSystemRights.Delete))
                        deleteIsDeny = true;

                    if (CheckAccessRule(rule, FileSystemRights.Modify))
                        modifyIsDeny = true;

                    if (CheckAccessRule(rule, FileSystemRights.Read))
                        readIsDeny = true;

                    if (CheckAccessRule(rule, FileSystemRights.Write))
                        writeIsDeny = true;

                    return;
                case AccessControlType.Allow:
                    if (CheckAccessRule(rule, FileSystemRights.Delete))
                        deleteIsAllow = true;

                    if (CheckAccessRule(rule, FileSystemRights.Modify))
                        modifyIsAllow = true;

                    if (CheckAccessRule(rule, FileSystemRights.Read))
                        readIsAllow = true;

                    if (CheckAccessRule(rule, FileSystemRights.Write))
                        writeIsAllow = true;
                    break;
            }
        }

        /// <summary>
        /// Check permissions
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="checkRead">Check read</param>
        /// <param name="checkWrite">Check write</param>
        /// <param name="checkModify">Check modify</param>
        /// <param name="checkDelete">Check delete</param>
        /// <returns>Result</returns>
        private static bool CheckPermissionsInWindows(string path, bool checkRead, bool checkWrite, bool checkModify, bool checkDelete)
        {
            var permissionsAreGranted = true;

            try
            {
                var fileProvider = EngineContext.Current.Resolve<INopFileProvider>();

                if (!(fileProvider.FileExists(path) || fileProvider.DirectoryExists(path)))
                {
                    return true;
                }

                var current = WindowsIdentity.GetCurrent();

                var readIsDeny = false;
                var writeIsDeny = false;
                var modifyIsDeny = false;
                var deleteIsDeny = false;

                var readIsAllow = false;
                var writeIsAllow = false;
                var modifyIsAllow = false;
                var deleteIsAllow = false;
                
                var rules = fileProvider.GetAccessControl(path).GetAccessRules(true, true, typeof(SecurityIdentifier))
                    .Cast<FileSystemAccessRule>()
                    .ToList();

                foreach (var rule in rules.Where(rule => current.User?.Equals(rule.IdentityReference) ?? false))
                {
                    CheckAccessRule(rule, ref deleteIsDeny, ref modifyIsDeny, ref readIsDeny, ref writeIsDeny, ref deleteIsAllow, ref modifyIsAllow, ref readIsAllow, ref writeIsAllow);
                }

                if (current.Groups != null)
                {
                    foreach (var reference in current.Groups)
                    {
                        foreach (var rule in rules.Where(rule => reference.Equals(rule.IdentityReference)))
                        {
                            CheckAccessRule(rule, ref deleteIsDeny, ref modifyIsDeny, ref readIsDeny, ref writeIsDeny, ref deleteIsAllow, ref modifyIsAllow, ref readIsAllow, ref writeIsAllow);
                        }
                    }
                }

                deleteIsAllow = !deleteIsDeny && deleteIsAllow;
                modifyIsAllow = !modifyIsDeny && modifyIsAllow;
                readIsAllow = !readIsDeny && readIsAllow;
                writeIsAllow = !writeIsDeny && writeIsAllow;

                if (checkRead)
                    permissionsAreGranted = readIsAllow;

                if (checkWrite)
                    permissionsAreGranted = permissionsAreGranted && writeIsAllow;

                if (checkModify)
                    permissionsAreGranted = permissionsAreGranted && modifyIsAllow;

                if (checkDelete)
                    permissionsAreGranted = permissionsAreGranted && deleteIsAllow;
            }
            catch (IOException)
            {
                return false;
            }
            catch
            {
                return true;
            }

            return permissionsAreGranted;
        }

        /// <summary>
        /// Check permissions
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="checkRead">Check read</param>
        /// <param name="checkWrite">Check write</param>
        /// <param name="checkModify">Check modify</param>
        /// <param name="checkDelete">Check delete</param>
        /// <returns>Result</returns>
        private static bool CheckPermissionsInUnix(string path, bool checkRead, bool checkWrite, bool checkModify, bool checkDelete)
        {
            //read permissions
            int[] r = { 5, 6, 7 };

            //write permissions
            int[] w = { 2, 3, 6, 7 };

            try
            {
                //Create bash command like
                //sh -c "stat -c '%a %u %g' <file>"
                //Result
                //555 1000 1000  - file permissions (555) | file owner ID (1000) | file group ID (1000)

                var arg = "-c \" stat -c '%a %u %g' " + path + "  \"";

                var process = new Process 
                {
                    StartInfo = new ProcessStartInfo 
                    {
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        FileName = "sh",
                        Arguments = arg
                    }
                };
                process.Start();
                process.WaitForExit();
                var res = process.StandardOutput.ReadToEnd();

                var respars = res.Trim('\n').Split(' ');

                var linuxFilePermissions = new[] 
                {
                    (int)char.GetNumericValue(respars[0][0]),
                    (int)char.GetNumericValue(respars[0][1]),
                    (int)char.GetNumericValue(respars[0][2])
                };

                var linuxFileOwnerId = respars[1];
                var linuxFileGroup = respars[2];

                // if user is owner of file
                if (CurrentOSUser.UserId == linuxFileOwnerId)
                {
                    if (checkRead & r.Contains(linuxFilePermissions[0]))
                    {
                        return true;
                    }

                    if ((checkWrite || checkModify || checkDelete) & w.Contains(linuxFilePermissions[0]))
                    {
                        return true;
                    }

                    return false;
                }

                // if user is in same group as file
                if (CurrentOSUser.Groups.Contains(linuxFileGroup))
                {
                    if (checkRead & r.Contains(linuxFilePermissions[1]))
                    {
                        return true;
                    }

                    if ((checkWrite || checkModify || checkDelete) & w.Contains(linuxFilePermissions[1]))
                    {
                        return true;
                    }

                    return false;
                }

                // checking permissions for other
                if (checkRead & r.Contains(linuxFilePermissions[2]))
                {
                    return true;
                }

                if ((checkWrite || checkModify || checkDelete) & w.Contains(linuxFilePermissions[2]))
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a list of directories (physical paths) which require write permission
        /// </summary>
        /// <returns>Result</returns>
        public static IEnumerable<string> GetDirectoriesWrite()
        {
            var fileProvider = EngineContext.Current.Resolve<INopFileProvider>();

            var rootDir = fileProvider.MapPath("~/");

            var dirsToCheck = new List<string>
            {
                fileProvider.Combine(rootDir, "App_Data"),
                fileProvider.Combine(rootDir, "bin"),
                fileProvider.Combine(rootDir, "log"),
                fileProvider.Combine(rootDir, "Plugins"),
                fileProvider.Combine(rootDir, "Plugins\\bin"),
                fileProvider.Combine(rootDir, "wwwroot\\bundles"),
                fileProvider.Combine(rootDir, "wwwroot\\db_backups"),
                fileProvider.Combine(rootDir, "wwwroot\\files\\exportimport"),
                fileProvider.Combine(rootDir, "wwwroot\\images"),
                fileProvider.Combine(rootDir, "wwwroot\\images\\thumbs"),
                fileProvider.Combine(rootDir, "wwwroot\\images\\uploaded")
            };

            return dirsToCheck;
        }

        /// <summary>
        /// Gets a list of files (physical paths) which require write permission
        /// </summary>
        /// <returns>Result</returns>
        public static IEnumerable<string> GetFilesWrite()
        {
            var fileProvider = EngineContext.Current.Resolve<INopFileProvider>();

            return new List<string>
            {
                fileProvider.MapPath(NopPluginDefaults.InstalledPluginsFilePath),
                fileProvider.MapPath(NopDataSettingsDefaults.FilePath)
            };
        }
    }
}
