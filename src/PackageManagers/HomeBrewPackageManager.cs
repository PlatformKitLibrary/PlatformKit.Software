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

using PlatformKit.Mac;

using PlatformKit.Software.Abstractions;
using PlatformKit.Software.Internal.Exceptions;

namespace PlatformKit.Software.PackageManagers;

public class HomeBrewPackageManager : AbstractPackageManager
{
    public HomeBrewPackageManager()
    {
        PackageManagerName = "HomeBrew";
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PackageManagerNotInstalledException"></exception>
    /// <exception cref="PackageManagerNotSupportedException"></exception>
    [SupportedOSPlatform("macos")]
    [SupportedOSPlatform("maccatalyst")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("freebsd")]
    public override IEnumerable<AppModel> GetUpdatable()
    {
        if (DoesPackageManagerSupportThisOperatingSystem())
        {
            if (!IsPackageManagerInstalled())
            {
                throw new PackageManagerNotInstalledException("HomeBrew");
            }
            
            List<AppModel> apps = new List<AppModel>();

            string[] caskUpdates = CommandRunner.RunCommandOnMac("brew outdated").Split(Environment.NewLine);

            foreach (string caskUpdate in caskUpdates)
            {
                if (caskUpdate.Equals(string.Empty) == true)
                {
                    string[] nameArr = caskUpdate.Replace("->", string.Empty).Replace(" ", string.Empty).Split(" ");

                    string installLocation;
                    string executableName = nameArr[0].Replace("bin/", string.Empty);

                    if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
                    {
                        if (MacOsAnalyzer.IsAppleSiliconMac())
                        {
                            installLocation = nameArr[1].Replace("..", "/opt/homebrew");
                        }
                        else
                        {
                            installLocation = nameArr[1];
                        }
                    }
                    else
                    {
                        installLocation = nameArr[1];
                    }
                
                    apps.Add(new AppModel(executableName, installLocation));
                }
            }

            return apps.ToArray();
        }

        throw new PackageManagerNotSupportedException("HomeBrew");
    }
    
    /// <summary>
    /// Returns all the detected installed brew casks.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PackageManagerNotInstalledException"></exception>
    /// <exception cref="PackageManagerNotSupportedException"></exception>
    [SupportedOSPlatform("macos")]
    [SupportedOSPlatform("maccatalyst")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("freebsd")]
    public override IEnumerable<AppModel> GetInstalled()
    {
        if (DoesPackageManagerSupportThisOperatingSystem())
        {
            if (IsPackageManagerInstalled())
            {
                List<AppModel> apps = new List<AppModel>();
                string[] casks = CommandRunner.RunCommandOnMac("ls -l bin").Split(Environment.NewLine);

                foreach (string cask in casks)
                {
                    string[] nameArr = cask.Replace("->", string.Empty).Replace(" ", string.Empty).Split(" ");

                    string executableName = nameArr[0].Replace("bin/", string.Empty);
                    string installLocation;
                    
                    if (OperatingSystem.IsMacOS())
                    {
                        if(MacOsAnalyzer.IsAppleSiliconMac())
                        {
                            installLocation = nameArr[1].Replace("..", "/opt/homebrew");
                        }
                        else
                        {
                            installLocation = nameArr[1];
                        }
                    }
                    else
                    {
                        installLocation = nameArr[1];
                    }
                    
                    apps.Add(new AppModel(executableName, installLocation));
                }

                return apps;
            }
            else
            {
                throw new PackageManagerNotInstalledException(PackageManagerName);
            }
        }

        throw new PackageManagerNotSupportedException("HomeBrew");
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [SupportedOSPlatform("macos")]
    [SupportedOSPlatform("maccatalyst")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("freebsd")]
    public override bool DoesPackageManagerSupportThisOperatingSystem()
    {
        return OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsFreeBSD();
    }

    /// <summary>
    /// Determines whether the Homebrew package manager is installed or not.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PackageManagerNotSupportedException"></exception>
    [SupportedOSPlatform("macos")]
    [SupportedOSPlatform("maccatalyst")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("freebsd")]
    public override bool IsPackageManagerInstalled()
    {
        if (DoesPackageManagerSupportThisOperatingSystem())
        {
            try
            {
                string[] brewTest = CommandRunner.RunCommandOnLinux("brew -v").Split(' ');
                
                if (brewTest[0].Contains("brew"))
                {
                    Version.Parse(brewTest[1]);

                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        throw new PackageManagerNotSupportedException("HomeBrew");
    }
}