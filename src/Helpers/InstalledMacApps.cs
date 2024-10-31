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
using System.Runtime.Versioning;

using PlatformKit;
using PlatformKit.Software.PackageManagers;

#if NETSTANDARD2_0 || NETSTANDARD2_1
using OperatingSystem = PlatformKit.Extensions.OperatingSystem.OperatingSystemExtension;
#endif


namespace PlatformKit.Software
{
    public class InstalledMacApps
    {
  
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("macos")]
#endif
        public static IEnumerable<AppModel> GetInstalled()
        {
            if (OperatingSystem.IsMacOS())
            {
                List<AppModel> apps = new List<AppModel>();

                string binDirectory = $"{Path.DirectorySeparatorChar}usr{Path.DirectorySeparatorChar}bin";

                string listFilesStart = "ls -F";
                string listFilesEnd = " | grep -v /";

                string[] binResult = CommandRunner.RunCommandOnLinux($"{listFilesStart} {binDirectory} {listFilesEnd}").Split(Environment.NewLine);

                foreach (string app in binResult)
                {
                    apps.Add(new AppModel(app, binDirectory));
                }

                string applicationsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Programs);

                string[] appResults = CommandRunner
                    .RunCommandOnMac($"{listFilesStart} {applicationsFolder} {listFilesEnd}")
                    .Split(Environment.NewLine);

                foreach (string app in appResults)
                {
                    apps.Add(new AppModel(app, applicationsFolder));
                }

                HomeBrewPackageManager homeBrew = new HomeBrewPackageManager();
            
                if (homeBrew.IsPackageManagerInstalled())
                {
                    foreach (AppModel app in homeBrew.GetInstalled())
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