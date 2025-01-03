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

#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using System.Text;
using System.Threading.Tasks;
using AlastairLundy.Extensions.System;

using CliRunner.Specializations.Commands;

using PlatformKit.Software.Abstractions;
using PlatformKit.Software.Internal.Exceptions;
using PlatformKit.Windows;

#if NETSTANDARD2_0 || NETSTANDARD2_1
using OperatingSystem = AlastairLundy.Extensions.Runtime.OperatingSystemExtensions;
#endif

namespace PlatformKit.Software.PackageManagers
{
    public class WingetPackageManager : AbstractPackageManager
    {
        public WingetPackageManager()
        {
            PackageManagerName = "Winget";
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PackageManagerNotInstalledException"></exception>
        /// <exception cref="PackageManagerNotSupportedException"></exception>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        public override async Task< IEnumerable<AppModel>> GetUpdatableAsync()
        {
            if (DoesPackageManagerSupportThisOperatingSystem())
            {
                if (await IsPackageManagerInstalledAsync() == false)
                {
                    throw new PackageManagerNotInstalledException(PackageManagerName);
                }
            
                List<AppModel> apps = new List<AppModel>();

                var command = await CmdCommand.Create()
                    .WithArguments("winget upgrade --source=winget")
                    .WithWorkingDirectory(Directory.GetCurrentDirectory())
                    .WithShellExecute(true)
                    .ExecuteBufferedAsync();
                
                string[] results = command.StandardOutput.Replace("-", string.Empty).Split(Environment.NewLine);
            
                string wingetLocation = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            
                int idPosition = results[0].IndexOf("Id", StringComparison.Ordinal);

                for (int index = 1; index < results.Length; index++)
                {
                    string line = results[index];

                    if (line.ToLower().Contains("upgrades available"))
                    {
                        break;
                    }
                
                    StringBuilder stringBuilder = new StringBuilder();

                    for (int charIndex = 0; charIndex < line.Length; charIndex++)
                    {
                        if (charIndex < (idPosition - 1))
                        {
                            stringBuilder.Append(line[charIndex]);
                        }
                        else
                        {
                            string appName = stringBuilder.ToString();
                        
                            if (appName.Contains("  "))
                            {
                                appName = appName.Replace("  ", string.Empty);
                            }
                            apps.Add(new AppModel(appName, wingetLocation));
                            break; 
                        }
                    }
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
        [SupportedOSPlatform("windows")]
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

                var command = await CmdCommand.Create()
                    .WithArguments("winget list --source=winget")
                    .WithWorkingDirectory(Directory.GetCurrentDirectory())
                    .WithShellExecute(true)
                    .ExecuteBufferedAsync();
                
                string[] results = command.StandardOutput.Replace("-", string.Empty).Split(Environment.NewLine);
            
                string wingetLocation = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

                int idPosition = results[0].IndexOf("Id", StringComparison.Ordinal);
            
                for (int index = 1; index < results.Length; index++)
                {
                    string line = results[index];
                
                    StringBuilder stringBuilder = new StringBuilder();

                    for (int charIndex = 0; charIndex < line.Length; charIndex++)
                    {
                        if (charIndex < (idPosition - 1))
                        {
                            stringBuilder.Append(line[charIndex]);
                        }
                        else
                        {
                            string appName = stringBuilder.ToString();
                        
                            if (appName.Contains("  "))
                            {
                                appName = appName.Replace("  ", string.Empty);
                            }
                            apps.Add(new AppModel(appName, wingetLocation));
                            break;
                        }
                    }
                }

                return apps.ToArray();
            }

            throw new PackageManagerNotSupportedException(PackageManagerName);
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        public override async Task<bool> IsPackageManagerInstalledAsync()
        {
            if (OperatingSystem.IsWindows())
            {
                if (DoesPackageManagerSupportThisOperatingSystem() == false)
                {
                    return false;
                }
            
                try
                {
                    var command = await CmdCommand.Create()
                        .WithArguments("winget")
                        .WithWorkingDirectory(Directory.GetCurrentDirectory())
                        .WithShellExecute(true)
                        .ExecuteBufferedAsync();
                    
                    string[] wingetTest = command.StandardOutput.Split(' ');
                    
                    if (wingetTest[0].Contains("Windows") && wingetTest[1].Contains("Package") && wingetTest[2].Contains("Manager"))
                    {
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool DoesPackageManagerSupportThisOperatingSystem()
        {
            if (OperatingSystem.IsWindows())
            {
                WindowsEdition edition = WindowsAnalyzer.GetWindowsEdition();
            
                if (WindowsAnalyzer.IsAtLeastVersion(WindowsVersion.Win10_v1809) &&
                    edition != WindowsEdition.Server && edition != WindowsEdition.Team)
                {
                    return true;
                }
                else
                {
                    if (WindowsAnalyzer.GetWindowsVersion().IsOlderThan(WindowsAnalyzer.GetWindowsVersionFromEnum(WindowsVersion.Win10_v1809)))
                    {
                        return false;
                    }
                    if (WindowsAnalyzer.GetWindowsVersionToEnum() == WindowsVersion.Win10_v1809 &&
                        WindowsAnalyzer.GetWindowsEdition() == WindowsEdition.Server)
                    {
                        return false;
                    }

                    return false;
                }
            }

            return false;
        }
    }
}