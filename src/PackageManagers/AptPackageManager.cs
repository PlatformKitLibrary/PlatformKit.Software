﻿/*
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
using System.IO;
using System.Threading.Tasks;

using CliRunner;

#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using PlatformKit.Linux;

using PlatformKit.Software.Abstractions;
using PlatformKit.Software.Internal.Exceptions;

#if NETSTANDARD2_0 || NETSTANDARD2_1
using OperatingSystem = AlastairLundy.Extensions.Runtime.OperatingSystemExtensions;
#endif

namespace PlatformKit.Software.PackageManagers
{
    public class AptPackageManager : AbstractPackageManager
    {
        public AptPackageManager()
        {
            PackageManagerName = "apt";
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("linux")]
#endif
        public override bool DoesPackageManagerSupportThisOperatingSystem()
        {
            return OperatingSystem.IsLinux() && (LinuxOsReleaseRetriever.GetLinuxOsRelease().Identifier_Like.ToLower().Contains("debian") || 
                                                 LinuxOsReleaseRetriever.GetLinuxOsRelease().Identifier_Like.ToLower().Contains("ubuntu"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("linux")]
#endif
        public override async Task<bool> IsPackageManagerInstalledAsync()
        {
            if (DoesPackageManagerSupportThisOperatingSystem())
            {
                try
                {
                    var command = await Cli.Run("/usr/bin/apt")
                        .WithArguments("-v")
                        .WithWorkingDirectory(Directory.GetCurrentDirectory())
                        .WithShellExecute(true)
                        .ExecuteBufferedAsync();
                    
                    string[] infos = command.StandardOutput.Split(' ');

                    return infos[0].Equals("apt");
                }
                catch
                {
                    return false;
                }
            }

            throw new PackageManagerNotSupportedException(PackageManagerName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PackageManagerNotInstalledException"></exception>
        /// <exception cref="PackageManagerNotSupportedException"></exception>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("linux")]
#endif
        public override async Task<IEnumerable<AppModel>> GetUpdatableAsync()
        {
            if (DoesPackageManagerSupportThisOperatingSystem())
            {
                if (await IsPackageManagerInstalledAsync() == false)
                {
                    throw new PackageManagerNotInstalledException(PackageManagerName);
                }
                
                await Cli.Run("/usr/bin/apt")
                    .WithArguments("update")
                    .WithWorkingDirectory(Directory.GetCurrentDirectory())
                    .WithShellExecute(true)
                    .RequiresAdministrator(true)
                    .ExecuteAsync();
                
                var command = await Cli.Run("/usr/bin/apt")
                    .WithArguments("list --upgradable")
                    .WithWorkingDirectory(Directory.GetCurrentDirectory())
                    .WithShellExecute(true)
                    .RequiresAdministrator(true)
                    .ExecuteBufferedAsync();

                List<AppModel> apps = new List<AppModel>();

                string[] updatableApps = command.StandardOutput.Split(Environment.NewLine);

                if (updatableApps.Length > 1)
                {
                    for (int index = 1; index < updatableApps.Length; index++)
                    {
                        string[] updatableAppInfos = updatableApps[index].Split("/");
                        string updatableApp = updatableAppInfos[0];
                
                        apps.Add(new AppModel(updatableApp,
                            $"{Path.DirectorySeparatorChar}usr{Path.DirectorySeparatorChar}bin"));
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
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PackageManagerNotInstalledException"></exception>
        /// <exception cref="PackageManagerNotSupportedException"></exception>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("linux")]
#endif
        public override async Task<IEnumerable<AppModel>> GetInstalledAsync()
        {
            if (DoesPackageManagerSupportThisOperatingSystem())
            {
                if (await IsPackageManagerInstalledAsync() == false)
                {
                    throw new PackageManagerNotInstalledException(PackageManagerName);
                }

                List<AppModel> apps = new List<AppModel>();
                
                var command = await Cli.Run("/usr/bin/apt")
                    .WithArguments("list")
                    .WithWorkingDirectory(Directory.GetCurrentDirectory())
                    .WithShellExecute(true)
                    .RequiresAdministrator(true)
                    .ExecuteBufferedAsync();

                string[] installedApps = command.StandardOutput.Split(Environment.NewLine);

                for (int index = 1; index < installedApps.Length; index++)
                {
                    string[] appInfos = installedApps[index].Split("/");
                    string appName = appInfos[0];
                
                    apps.Add(new AppModel(appName,
                        $"{Path.DirectorySeparatorChar}usr{Path.DirectorySeparatorChar}bin"));
                }

                return apps.ToArray();
            }

            throw new PackageManagerNotSupportedException(PackageManagerName);
        }
    }
}