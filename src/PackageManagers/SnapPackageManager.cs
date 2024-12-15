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
using System.IO;
using System.Threading.Tasks;

#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using PlatformKit.Software.Abstractions;
using PlatformKit.Software.Internal.Exceptions;

#if NETSTANDARD2_0 || NETSTANDARD2_1
using OperatingSystem = AlastairLundy.Extensions.Runtime.OperatingSystemExtensions;
#endif

namespace PlatformKit.Software.PackageManagers
{
    public class SnapPackageManager : AbstractPackageManager
    {
        public SnapPackageManager()
        {
            PackageManagerName = "Snap";
        }
    
        /// <summary>
        /// Gets the names of updatable Snap packages.
        /// </summary>
        /// <returns>the updatable Snap packages as AppModel objects.</returns>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("freebsd")]
#endif
        public override async Task<IEnumerable<AppModel>> GetUpdatableAsync()
        {
            if (DoesPackageManagerSupportThisOperatingSystem())
            {
                if (await IsPackageManagerInstalledAsync() == false)
                {
                    throw new PackageManagerNotInstalledException(PackageManagerName);
                }
            
                List<AppModel> apps = new List<AppModel>();

                string[] snapUpdates = CommandRunner.RunCommandOnLinux("snap refresh --list").Split(Environment.NewLine);

                if (snapUpdates.Length > 1)
                {
                    for (int i = 1; i < snapUpdates.Length; i++)
                    {
                        string[] snapInfos = snapUpdates[i].Split(" ");
                        string snap = snapInfos[0];
                
                        apps.Add(new AppModel(snap,
                            $"{Path.DirectorySeparatorChar}snap{Path.DirectorySeparatorChar}bin"));
                    }
                }
                else
                {
                    apps.Clear();
                    return apps.ToArray();
                }

                return apps.ToArray();
            }

            throw new PackageManagerNotSupportedException(PackageManagerName);
        }
    
        /// <summary>
        /// Detect what Snap packages (if any) are installed on a linux distribution or on macOS.
        /// </summary>
        /// <returns>Returns a list of installed snaps. Returns an empty array if no Snaps are installed.</returns>
        /// <exception cref="PlatformNotSupportedException">Throws an exception if run on a Platform other than Linux, macOS, and FreeBsd.</exception>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("freebsd")]
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

                string[] snapResults = CommandRunner.RunCommandOnLinux(
                    $"ls {Path.DirectorySeparatorChar}snap{Path.DirectorySeparatorChar}bin").Split(' ');

                foreach (string snap in snapResults)
                {
                    apps.Add(new AppModel(snap, 
                        $"{Path.DirectorySeparatorChar}snap{Path.DirectorySeparatorChar}bin"));
                }

                return apps.ToArray();
            }

            throw new PackageManagerNotSupportedException(PackageManagerName);
        }

        public override bool DoesPackageManagerSupportThisOperatingSystem()
        {
            return OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD();
        }

        /// <summary>
        /// Detect if the Snap package manager is installed.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("freebsd")]
#endif
        public override async Task<bool> IsPackageManagerInstalledAsync()
        {
            if (DoesPackageManagerSupportThisOperatingSystem())
            {
                return await Task.FromResult(
                    Directory.Exists($"{Path.DirectorySeparatorChar}snap{Path.DirectorySeparatorChar}bin"));
            }

            throw new PackageManagerNotInstalledException(PackageManagerName);
        }
    }
}