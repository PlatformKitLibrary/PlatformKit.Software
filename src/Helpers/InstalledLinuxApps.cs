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

#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using PlatformKit.Software.PackageManagers;


#if NETSTANDARD2_0 || NETSTANDARD2_1
using OperatingSystem = AlastairLundy.Extensions.Runtime.OperatingSystemExtensions;
#endif


namespace PlatformKit.Software
{
    public class InstalledLinuxApps
    {

        // ReSharper disable once IdentifierTypo
        #if NET5_0_OR_GREATER
        [SupportedOSPlatform("linux")]
        #endif
        public static async Task<IEnumerable<AppModel>> GetInstalled(bool includeBrewCasks = true)
        {
            if (OperatingSystem.IsLinux())
            {
                List<AppModel> apps = new List<AppModel>();

                string[] binResult = CommandRunner.RunCommandOnLinux("ls -F /usr/bin | grep -v /").Split(Environment.NewLine);

                foreach (string app in binResult)
                {
                    apps.Add(new AppModel(app, $"{Path.DirectorySeparatorChar}usr{Path.DirectorySeparatorChar}bin"));
                }

                if (includeBrewCasks)
                {
                    HomeBrewPackageManager homeBrew = new HomeBrewPackageManager();
                
                    foreach (AppModel app in await homeBrew.GetInstalledAsync())
                    {
                        apps.Add(app);
                    }
                }

                return apps.ToArray();
            }

            throw new PlatformNotSupportedException();
        }

    }
}
