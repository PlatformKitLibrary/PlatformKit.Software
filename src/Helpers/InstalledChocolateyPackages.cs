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

namespace PlatformKit.Software;

public class InstalledChocolateyPackages
{
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    [SupportedOSPlatform("windows")]
    public static IEnumerable<AppModel> Get()
    {
        if (IsChocolateyInstalled())
        {
            List<AppModel> apps = new List<AppModel>();
            
            string[] chocoResults = CommandRunner.RunCmdCommand("choco list -l -r --id-only").Split(Environment.NewLine);

            string chocolateyLocation = CommandRunner.RunPowerShellCommand("$env:ChocolateyInstall");
            
            foreach (string package in chocoResults)
            {
                apps.Add(new AppModel(package, chocolateyLocation));
            }

            return apps.ToArray();
        }

        throw new PlatformNotSupportedException();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    public static bool IsChocolateyInstalled()
    {
        if (OperatingSystem.IsWindows())
        {
            try
            {
                string[] chocoTest = CommandRunner.RunCmdCommand("choco info").Split(' ');
                    
                if (chocoTest[0].Contains("Chocolatey"))
                {
                    Version.Parse(chocoTest[1]);

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
}