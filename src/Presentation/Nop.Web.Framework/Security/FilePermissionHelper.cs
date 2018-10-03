using System;
using System.Collections.Generic;
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
            if (Environment.OSVersion.Platform == System.PlatformID.Win32NT)
            {
                return CheckPermissionsInWindows(path, checkRead, checkWrite, checkModify, checkDelete);
            }

            if (Environment.OSVersion.Platform == System.PlatformID.Unix)
            {
                return CheckPermissionsInUnix(path, checkRead, checkWrite, checkModify, checkDelete);
            }

            return false;
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
        public static bool CheckPermissionsInWindows(string path, bool checkRead, bool checkWrite, bool checkModify, bool checkDelete)
        {
            var flag = false;
            var flag2 = false;
            var flag3 = false;
            var flag4 = false;
            var flag5 = false;
            var flag6 = false;
            var flag7 = false;
            var flag8 = false;

            var current = WindowsIdentity.GetCurrent();
            AuthorizationRuleCollection rules;
            try
            {
                var fileProvider = EngineContext.Current.Resolve<INopFileProvider>();
                rules = fileProvider.GetAccessControl(path).GetAccessRules(true, true, typeof(SecurityIdentifier));
            }
            catch
            {
                return true;
            }
            try
            {
                foreach (FileSystemAccessRule rule in rules)
                {
                    if (!current.User.Equals(rule.IdentityReference))
                    {
                        continue;
                    }
                    if (AccessControlType.Deny.Equals(rule.AccessControlType))
                    {
                        if ((FileSystemRights.Delete & rule.FileSystemRights) == FileSystemRights.Delete)
                            flag4 = true;
                        if ((FileSystemRights.Modify & rule.FileSystemRights) == FileSystemRights.Modify)
                            flag3 = true;

                        if ((FileSystemRights.Read & rule.FileSystemRights) == FileSystemRights.Read)
                            flag = true;

                        if ((FileSystemRights.Write & rule.FileSystemRights) == FileSystemRights.Write)
                            flag2 = true;

                        continue;
                    }
                    if (AccessControlType.Allow.Equals(rule.AccessControlType))
                    {
                        if ((FileSystemRights.Delete & rule.FileSystemRights) == FileSystemRights.Delete)
                        {
                            flag8 = true;
                        }
                        if ((FileSystemRights.Modify & rule.FileSystemRights) == FileSystemRights.Modify)
                        {
                            flag7 = true;
                        }
                        if ((FileSystemRights.Read & rule.FileSystemRights) == FileSystemRights.Read)
                        {
                            flag5 = true;
                        }
                        if ((FileSystemRights.Write & rule.FileSystemRights) == FileSystemRights.Write)
                        {
                            flag6 = true;
                        }
                    }
                }
                foreach (var reference in current.Groups)
                {
                    foreach (FileSystemAccessRule rule2 in rules)
                    {
                        if (!reference.Equals(rule2.IdentityReference))
                        {
                            continue;
                        }
                        if (AccessControlType.Deny.Equals(rule2.AccessControlType))
                        {
                            if ((FileSystemRights.Delete & rule2.FileSystemRights) == FileSystemRights.Delete)
                                flag4 = true;
                            if ((FileSystemRights.Modify & rule2.FileSystemRights) == FileSystemRights.Modify)
                                flag3 = true;
                            if ((FileSystemRights.Read & rule2.FileSystemRights) == FileSystemRights.Read)
                                flag = true;
                            if ((FileSystemRights.Write & rule2.FileSystemRights) == FileSystemRights.Write)
                                flag2 = true;
                            continue;
                        }
                        if (AccessControlType.Allow.Equals(rule2.AccessControlType))
                        {
                            if ((FileSystemRights.Delete & rule2.FileSystemRights) == FileSystemRights.Delete)
                                flag8 = true;
                            if ((FileSystemRights.Modify & rule2.FileSystemRights) == FileSystemRights.Modify)
                                flag7 = true;
                            if ((FileSystemRights.Read & rule2.FileSystemRights) == FileSystemRights.Read)
                                flag5 = true;
                            if ((FileSystemRights.Write & rule2.FileSystemRights) == FileSystemRights.Write)
                                flag6 = true;
                        }
                    }
                }
                var flag9 = !flag4 && flag8;
                var flag10 = !flag3 && flag7;
                var flag11 = !flag && flag5;
                var flag12 = !flag2 && flag6;
                var flag13 = true;
                if (checkRead)
                {
                    //flag13 = flag13 && flag11;
                    flag13 = flag11;
                }
                if (checkWrite)
                {
                    flag13 = flag13 && flag12;
                }
                if (checkModify)
                {
                    flag13 = flag13 && flag10;
                }
                if (checkDelete)
                {
                    flag13 = flag13 && flag9;
                }
                return flag13;
            }
            catch (System.IO.IOException)
            {
            }
            return false;
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
        public static bool CheckPermissionsInUnix(string path, bool checkRead, bool checkWrite, bool checkModify, bool checkDelete)
        {
            //read permissions
            int[] r = new int[]{5, 6, 7};

            //write permissions
            int[] w = new int[]{2, 3, 6, 7};

            var res = "";
            var linuxUserId = "";
            var linuxUserGroupIds = "";
            var linuxFilePermissions = new int[3];
            var linuxFileOwner = "";
            var linuxFileGroup = "";

            try
            {
                //Create bash command like
                //sh -c " id -u ; id -G ; stat -c '%a %u %g' <file>"
                //Result
                //1000                          - user ID
                //1000 4 24 27 30 46 116 126    - user groups
                //555 1000 1000                 - file permissions (555) | file owner ID (1000) | file group ID (1000)

                var arg = "-c \" id -u ; id -G ; stat -c '%a %u %g' " + path + "  \"";
                var _p = new System.Diagnostics.Process();
                _p.StartInfo.RedirectStandardInput = true;
                _p.StartInfo.RedirectStandardOutput = true;
                _p.StartInfo.UseShellExecute = false;
                _p.StartInfo.FileName = "sh";
                _p.StartInfo.Arguments = arg;
                _p.Start();
                _p.WaitForExit();
                res = _p.StandardOutput.ReadToEnd();

                var respars = res.Split("\n");
                linuxUserId = respars[0];
                linuxUserGroupIds = respars[1];

                var tmp = respars[2].Split(' ');
                linuxFilePermissions[0] = (int)Char.GetNumericValue(tmp[0][0]);
                linuxFilePermissions[1] = (int)Char.GetNumericValue(tmp[0][1]);
                linuxFilePermissions[2] = (int)Char.GetNumericValue(tmp[0][2]);
                linuxFileOwner = tmp[1];
                linuxFileGroup = tmp[2];
            }
            catch (System.Exception ex )
            {
                return true;
            }
            try
            {
                // if user is owner of file
                if (linuxUserId ==linuxFileOwner)
                {
                    if (checkRead & r.Contains(linuxFilePermissions[0]) )
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
                if (linuxUserGroupIds.Contains(linuxFileGroup))
                {
                    if (checkRead & r.Contains(linuxFilePermissions[1]) )
                    {
                        return true;
                    }

                    if ((checkWrite || checkModify || checkDelete) &  w.Contains(linuxFilePermissions[1]))
                    {
                        return true;
                    }

                    return false;

                }
                else // checking permissions for other
                {
                    if (checkRead & r.Contains(linuxFilePermissions[2]) )
                    {
                        return true;
                    }

                    if ((checkWrite || checkModify || checkDelete) &  w.Contains(linuxFilePermissions[2]))
                    {
                        return true;
                    }

                    return false;
                }
            }
            catch (System.IO.IOException)
            {
            }
            return false;
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
                fileProvider.Combine(rootDir, "plugins"),
                fileProvider.Combine(rootDir, "plugins\\bin"),
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
