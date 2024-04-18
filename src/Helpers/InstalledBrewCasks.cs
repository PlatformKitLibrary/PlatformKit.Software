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

namespace PlatformKit.Software;

public class InstalledBrewCasks
{
    /// <summary>
    /// Returns all the detected installed brew casks.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    [SupportedOSPlatform("macos")]
    [SupportedOSPlatform("linux")]
    public static IEnumerable<AppModel> Get()
    {
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            List<AppModel> apps = new List<AppModel>();

            if (IsHomeBrewInstalled())
            {
                string[] casks = CommandRunner.RunCommandOnMac("ls -l bin").Split(Environment.NewLine);

                foreach (string cask in casks)
                {
                    string[] nameArr = cask.Replace("->", string.Empty).Replace(" ", string.Empty).Split(" ");

                    if (MacOsAnalyzer.IsAppleSiliconMac())
                    {
                        apps.Add(new AppModel(nameArr[0].Replace("bin/", string.Empty), nameArr[1].Replace("..", "/opt/homebrew")));
                    }
                    else
                    {
                        apps.Add(new AppModel(nameArr[0].Replace("bin/", string.Empty), nameArr[1]));
                    }
                }

                return apps;
            }
            
            apps.Clear();
            return apps.ToArray();
        }

        throw new PlatformNotSupportedException();
    }

    /// <summary>
    /// Determines whether the Homebrew package manager is installed or not.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    public static bool IsHomeBrewInstalled()
    {
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            string[] brewTest = CommandRunner.RunCommandOnLinux("brew -v").Split(' ');

            try
            {
                if (brewTest[0].Contains("Flatpak"))
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

        throw new PlatformNotSupportedException();
    }

    /// <summary>
    /// Determines whether the specified brew cask is installed or not.
    /// </summary>
    /// <param name="caskName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static bool IsCaskInstalled(string caskName)
    {
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            if (IsHomeBrewInstalled())
            {
                foreach (AppModel app in Get())
                {
                    if (app.ExecutableName.Equals(caskName))
                    {
                        return true;
                    }
                }

                return false;
            }

            throw new ArgumentException();
        }

        throw new PlatformNotSupportedException();
    }
}