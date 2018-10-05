using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text.RegularExpressions;
using Nop.Core.Infrastructure;

namespace Nop.Web.Framework.Security
{
    public static class CurrentOSUser
    {
        #region Fields

        private static string _name;
        private static string _domainName;
        private static List<string> _groups;
        private static string _userId;

        #endregion

        #region Ctor

        static CurrentOSUser()
        {
            _name = System.Environment.UserName;

            _domainName = System.Environment.UserDomainName;

            switch (System.Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    {
                        PopulateWindowsUser();
                        break;
                    }
                case System.PlatformID.Unix:
                    {
                        PopulateLinuxUser();
                        break;
                    }
                default:
                    {
                        _userId = _name;
                        _groups = new List<string>();
                        break;
                    }
            }
        }

        #endregion

        #region Methods

        public static void PopulateWindowsUser()
        {
            _groups = WindowsIdentity.GetCurrent().Groups.Select(p=>p.Value).ToList();
            _userId = _name;
        }

        public static void PopulateLinuxUser()
        {
            var arg = "-c \" id -u ; id -G \"";
            var _p = new Process{
                StartInfo = new ProcessStartInfo{
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    FileName = "sh",
                    Arguments = arg
                }
            };
            _p.Start();
            _p.WaitForExit();


            var res = _p.StandardOutput.ReadToEnd();

            var respars = res.Split("\n");

            _userId = respars[0];
            _groups = respars[1].Split(" ").ToList();

        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns user name
        /// </summary>
        public static string Name => _name;

        /// <summary>
        /// Returns user domain name for Windows or group for Linux
        /// </summary>
        public static string DomainName => _domainName;

        /// <summary>
        /// Returns user groups
        /// </summary>
        public static List<string> Groups => _groups;

        /// <summary>
        /// Returns user name for Windows or user Id  for Linux like 1001
        /// </summary>
        public static string UserId => _userId;

        /// <summary>
        /// Returns full user name
        /// </summary>
        public static string FullName => $@"{_domainName}\{_name}";

        #endregion
    }
}