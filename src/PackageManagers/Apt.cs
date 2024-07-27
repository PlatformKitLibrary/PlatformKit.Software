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

using System.Runtime.Versioning;
using PlatformKit.Linux;
using PlatformKit.Software.Abstractions;
using PlatformKit.Software.Internal.Exceptions;

namespace PlatformKit.Software.PackageManagers;

public class Apt : AbstractPackageManager
{
    public Apt()
    {
        PackageManagerName = "apt";
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [SupportedOSPlatform("linux")]
    public override bool DoesPackageManagerSupportThisOperatingSystem()
    {
        return OperatingSystem.IsLinux() && (LinuxOsReleaseRetriever.GetLinuxOsRelease().Identifier_Like.ToLower().Contains("debian") || 
         LinuxOsReleaseRetriever.GetLinuxOsRelease().Identifier_Like.ToLower().Contains("ubuntu"));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [SupportedOSPlatform("linux")]
    public override bool IsPackageManagerInstalled()
    {
        if (DoesPackageManagerSupportThisOperatingSystem())
        {
            try
            {
                string[] infos = CommandRunner.RunCommandOnLinux("apt -v").Split(" ");

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
    [SupportedOSPlatform("linux")]
    public override IEnumerable<AppModel> GetUpdatable()
    {
        if (DoesPackageManagerSupportThisOperatingSystem())
        {
            if (!IsPackageManagerInstalled())
            {
                throw new PackageManagerNotInstalledException(PackageManagerName);
            }

            CommandRunner.RunCommandOnLinux("apt update", true);

            List<AppModel> apps = new List<AppModel>();

            string[] updatableApps = CommandRunner.RunCommandOnLinux("apt list --upgradable").Split(Environment.NewLine);

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
    [SupportedOSPlatform("linux")]
    public override IEnumerable<AppModel> GetInstalled()
    {
        if (DoesPackageManagerSupportThisOperatingSystem())
        {
            if (!IsPackageManagerInstalled())
            {
                throw new PackageManagerNotInstalledException(PackageManagerName);
            }

            List<AppModel> apps = new List<AppModel>();

            string[] installedApps = CommandRunner.RunCommandOnLinux("apt list").Split(Environment.NewLine);

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