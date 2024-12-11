/*
       MIT License

    Copyright (c) 2024 Alastair Lundy

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
 */

using System;
using System.Collections.Generic;

#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif
using PlatformKit;
using PlatformKit.Software.Abstractions;
using PlatformKit.Software.Internal.Exceptions;

#if NETSTANDARD2_0 || NETSTANDARD2_1
using OperatingSystem = AlastairLundy.Extensions.Runtime.OperatingSystemExtensions;
#endif

namespace PlatformKit.Software.PackageManagers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class FlatpakPackageManager : AbstractPackageManager
    {
        public FlatpakPackageManager()
        {
            PackageManagerName = "Flatpak";
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PackageManagerNotInstalledException"></exception>
        /// <exception cref="PackageManagerNotSupportedException"></exception>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("freebsd")]
#endif
        public override IEnumerable<AppModel> GetUpdatable()
        {
            if (DoesPackageManagerSupportThisOperatingSystem())
            {
                if (IsPackageManagerInstalled() == false)
                {
                    throw new PackageManagerNotInstalledException(PackageManagerName);
                }
            
                List<AppModel> apps = new List<AppModel>();
            
                string[] flatpakResults = CommandRunner.RunCommandOnLinux("flatpak update -n")
                    .Split(Environment.NewLine);

                string installLocation = CommandRunner.RunCommandOnLinux("flatpak --installations");

                if (flatpakResults.Length > 1)
                {
                    for (int index = 1; index < flatpakResults.Length; index++)
                    {
                        string flatpakResult = flatpakResults[index];

                        if (flatpakResult.Equals(string.Empty) == false && flatpakResult.Contains('.') && flatpakResult.Contains("ID") == false)
                        {
                            string result = flatpakResult.Split(" ")[1];
                    
                            apps.Add(new AppModel(result, installLocation));
                        }
                    }
                }
                else
                {
                    apps.Clear();
                }

                return apps.ToArray();
            }

            throw new PackageManagerNotSupportedException(PackageManagerName);
        }

        /// <summary>
        /// Platforms Supported On: Linux and FreeBsd.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("freebsd")]
#endif
        public override IEnumerable<AppModel> GetInstalled()
        {
            if (DoesPackageManagerSupportThisOperatingSystem())
            {
                if (IsPackageManagerInstalled() == false)
                {
                    throw new PackageManagerNotInstalledException(PackageManagerName);
                }
            
                List<AppModel> apps = new List<AppModel>();
            
                string[] flatpakResults = CommandRunner.RunCommandOnLinux("flatpak list --columns=name")
                    .Split(Environment.NewLine);

                string installLocation = CommandRunner.RunCommandOnLinux("flatpak --installations");

                foreach (string flatpak in flatpakResults)
                {
                    apps.Add(new AppModel(flatpak, installLocation));
                }

                return apps.ToArray();
            }

            throw new PackageManagerNotSupportedException(PackageManagerName);
        }

        /// <summary>
        /// Determines whether the Flatpak package manager is installed or not.
        /// </summary>
        /// <returns></returns>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("freebsd")]
#endif
        public override bool IsPackageManagerInstalled()
        {
            if (DoesPackageManagerSupportThisOperatingSystem())
            {
                try
                {
                    string[] flatpakTest = CommandRunner.RunCommandOnLinux("flatpak --version").Split(' ');
                
                    if (flatpakTest[0].Contains("Flatpak"))
                    {
                        Version.Parse(flatpakTest[1]);

                        return true;
                    }
                }
                catch
                {
                    return false;
                }

                return false;
            }

            throw new PackageManagerNotSupportedException(PackageManagerName);
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool DoesPackageManagerSupportThisOperatingSystem()
        {
            return OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD();
        }
    }
}