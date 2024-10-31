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

using PlatformKit.Linux;
using PlatformKit.Linux.Enums;
using PlatformKit.Linux.Models;
using PlatformKit.Windows;

using PlatformKit.Software.PackageManagers;
using PlatformKit.Software.Enums;

#if NETSTANDARD2_0 || NETSTANDARD2_1
using OperatingSystem = PlatformKit.Extensions.OperatingSystem.OperatingSystemExtension;
#endif

// ReSharper disable RedundantIfElseBlock

namespace PlatformKit.Software
{
    public class PackageManagerDetector
    {

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public static PackageManager GetDefaultForPlatform()
        {
            if(OperatingSystem.IsLinux())
            {
                LinuxOsReleaseModel osRelease = LinuxOsReleaseRetriever.GetLinuxOsRelease();
                LinuxDistroBase distroBase = LinuxOsReleaseRetriever.GetDistroBase(osRelease);

                switch (distroBase)
                {
                    case LinuxDistroBase.Arch:
                        return PackageManager.Pacman;
                    case LinuxDistroBase.Manjaro:
                        return PackageManager.Pacman;
                    case LinuxDistroBase.Debian:
                        return PackageManager.APT;
                    case LinuxDistroBase.Ubuntu:
                        string osName = osRelease.PrettyName.ToLower();

                        if (osName.Contains("buntu"))
                        {
                            return PackageManager.Snap;
                        }
                        else
                        {
                            return PackageManager.APT;
                        }
                    case LinuxDistroBase.Fedora:
                        return PackageManager.Snap;
                    case LinuxDistroBase.RHEL:
                        return PackageManager.DNF;
                    default:
                        SnapPackageManager snap = new SnapPackageManager();
                        HomeBrewPackageManager homeBrew = new HomeBrewPackageManager();
                        FlatpakPackageManager flatpak = new FlatpakPackageManager();
                    
                        if(flatpak.IsPackageManagerInstalled())
                        {
                            return PackageManager.Flatpak;
                        }

                        if(snap.IsPackageManagerInstalled())
                        {
                            return PackageManager.Snap;
                        }
                        if(homeBrew.IsPackageManagerInstalled())
                        {
                            return PackageManager.Homebrew;
                        }

                        return PackageManager.NotDetected;
                }

            }
            if(OperatingSystem.IsMacOS())
            {
                HomeBrewPackageManager homeBrew = new HomeBrewPackageManager();
           
                if(homeBrew.IsPackageManagerInstalled()) 
                {
                    return PackageManager.Homebrew;
                }

                // TODO: Add Mac Ports support here


                return PackageManager.NotSupported;
            }
            if (OperatingSystem.IsWindows())
            {
                if (WindowsAnalyzer.IsAtLeastVersion(WindowsVersion.Win10_v1809) && WindowsAnalyzer.GetWindowsEdition() != WindowsEdition.Server)
                {
                    return PackageManager.Winget;
                }

                ChocolateyPackageManager chocolatey = new ChocolateyPackageManager();
            
                if (chocolatey.DoesPackageManagerSupportThisOperatingSystem())
                {
                    if (chocolatey.IsPackageManagerInstalled())
                    {
                        return PackageManager.Chocolatey;
                    }
                }

                return PackageManager.NotSupported;
            }

            throw new PlatformNotSupportedException();
        }
    }
}
