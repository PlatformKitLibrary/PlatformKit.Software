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

#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlatformKit;

#if NETSTANDARD2_0 || NETSTANDARD2_1
using OperatingSystem = AlastairLundy.Extensions.Runtime.OperatingSystemExtensions;
#endif

namespace PlatformKit.Software
{
    public class InstalledApps
    {

        /// <summary>
        /// Determine whether an app is installed or not.
        /// </summary>
        /// <param name="appName"></param>
        /// <returns></returns>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("macos")]
#endif
        public static async Task<bool> IsInstalled(string appName)
        {
            return (await Get()).Any(app => app.ExecutableName.Equals(appName));
        }

        /// <summary>
        /// Gets a collection of apps and programs installed on this device.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("macos")]
#endif
        public static async Task<IEnumerable<AppModel>> Get()
        {
            if (OperatingSystem.IsWindows())
            {
                return await InstalledWindowsApps.GetInstalled(true);
            }
            else if (OperatingSystem.IsMacOS())
            {
                return await InstalledMacApps.GetInstalled();
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
            {
                return await InstalledLinuxApps.GetInstalled(true);
            }

            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Opens the specified app or program.
        /// </summary>
        /// <param name="appModel"></param>
        /// <exception cref="PlatformNotSupportedException"></exception>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("macos")]
#endif
        public static void Open(AppModel appModel)
        {
            if (OperatingSystem.IsWindows())
            {
                ProcessRunner.RunProcessOnWindows(appModel.InstallLocation, appModel.ExecutableName);
            }
            else if (OperatingSystem.IsMacOS())
            {
                ProcessRunner.RunProcessOnMac(appModel.InstallLocation, appModel.ExecutableName);
            }
            else if (OperatingSystem.IsLinux())
            {
                ProcessRunner.RunProcessOnLinux(appModel.InstallLocation, appModel.ExecutableName);
            }
            else if (OperatingSystem.IsFreeBSD())
            {
                ProcessRunner.RunProcessOnFreeBsd(appModel.InstallLocation, appModel.ExecutableName);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }
    }
}