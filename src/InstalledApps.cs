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
using Microsoft.VisualBasic;

namespace PlatformKit.Software;

public class InstalledApps
{

    [SupportedOSPlatform("windows")]
    protected static AppModel[] ExpandWinSpecialFolderPath(string directory)
    {
        List<AppModel> apps = new List<AppModel>();

        if (OperatingSystem.IsWindows())
        {
            var files = directory.Split(Environment.NewLine);
        
            for (int programIndex = 0; programIndex < files.Length; programIndex++)
            {
                string item = files[programIndex];
                
                if (item.Contains("<DIR>"))
                {
                    AppModel[] programs = InstalledWinPrograms.Get(item);

                    foreach (AppModel program in programs)
                    {
                        apps.Add(program);
                    }
                }
            }

            return apps.ToArray();
        }

        throw new PlatformNotSupportedException();
    }
    
    [SupportedOSPlatform("windows")]
    protected static AppModel[] GetOnWindows(bool includeWindowsPrograms)
    {
        if (OperatingSystem.IsWindows())
        {
            List<AppModel> apps = new List<AppModel>();

            string programFiles = CommandRunner.RunCmdCommand("dir " + Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            
            string programFilesX86 = CommandRunner.RunCmdCommand("dir " + Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));

            string appData = CommandRunner
                .RunCmdCommand("dir " + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

            string winPrograms = CommandRunner
                .RunCmdCommand(Environment.GetFolderPath(Environment.SpecialFolder.System));

            foreach (AppModel program in ExpandWinSpecialFolderPath(programFiles))
            {
                apps.Add(program);
            }
            foreach (AppModel program in ExpandWinSpecialFolderPath(programFilesX86))
            {
                apps.Add(program);
            }
            foreach (AppModel program in ExpandWinSpecialFolderPath(appData))
            {
                apps.Add(program);
            }

            if (includeWindowsPrograms)
            {
                foreach (AppModel program in ExpandWinSpecialFolderPath(winPrograms))
                {
                    apps.Add(program);
                }
            }

            return apps.ToArray();
        }

        throw new PlatformNotSupportedException();
    }
    
    [SupportedOSPlatform("macos")]
    protected static AppModel[] GetOnMac()
    {
        if (OperatingSystem.IsMacOS())
        {
            List<AppModel> apps = new List<AppModel>();

            string binDirectory = Path.DirectorySeparatorChar + "usr" + Path.DirectorySeparatorChar + "bin";
            
            string listFilesStart = "ls -F";
            string listFilesEnd = " | grep -v /";
            
            string[] binResult = CommandRunner.RunCommandOnLinux(listFilesStart + " " + binDirectory + " " + listFilesEnd).Split(Environment.NewLine);

            foreach (string app in binResult)
            {
                apps.Add(new AppModel(app, binDirectory));
            }

            string applicationsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Programs);

            string[] appResults = CommandRunner
                .RunCommandOnMac(listFilesStart + " " + applicationsFolder + " " + listFilesEnd)
                .Split(Environment.NewLine);

            foreach (string app in appResults)
            {
                apps.Add(new AppModel(app, applicationsFolder));
            }
            
            return apps.ToArray();
        }

        throw new PlatformNotSupportedException();
    }

    // ReSharper disable once IdentifierTypo
    [SupportedOSPlatform("linux")]
    protected static AppModel[] GetOnLinux(bool includeSnaps = false, bool includeFlatpaks = false)
    {
        if (OperatingSystem.IsLinux())
        {
            List<AppModel> apps = new List<AppModel>();

            string[] binResult = CommandRunner.RunCommandOnLinux("ls -F /usr/bin | grep -v /").Split(Environment.NewLine);

            foreach (var app in binResult)
            {
                apps.Add(new AppModel(app, Path.DirectorySeparatorChar + "usr" + Path.DirectorySeparatorChar + "bin"));
            }
            
            if (includeSnaps)
            {
                foreach (AppModel snap in InstalledSnaps.Get())
                {
                    apps.Add(snap);
                }
            }
            if(includeFlatpaks){
                foreach (AppModel flatpak in InstalledFlatpaks.Get())
                {
                    apps.Add(flatpak);
                }
            }

            return apps.ToArray();
        }

        throw new PlatformNotSupportedException();
    }

    /// <summary>
    /// Determine whether an app is installed or not.
    /// </summary>
    /// <param name="appName"></param>
    /// <returns></returns>
    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    public bool IsInstalled(string appName)
    {
        foreach (AppModel app in Get())
        {
            if (app.ExecutableName.Equals(appName))
            {
                return true;
            }
        }

        return false;
    }
    
    /// <summary>
    /// Gets a collection of apps and programs installed on this device.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    public static IEnumerable<AppModel> Get()
    {
        if (OperatingSystem.IsWindows())
        {
            return GetOnWindows(true);
        }
        else if (OperatingSystem.IsMacOS())
        {
            return GetOnMac();
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
        {
            return GetOnLinux(true, true);
        }

        throw new PlatformNotSupportedException();
    }
    
    /// <summary>
    /// Opens the specified app or program.
    /// </summary>
    /// <param name="appModel"></param>
    /// <exception cref="PlatformNotSupportedException"></exception>
    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
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