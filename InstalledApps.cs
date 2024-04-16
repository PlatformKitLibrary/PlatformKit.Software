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


namespace PlatformKit.Software;

public class InstalledApps
{
    protected static AppModel[] GetOnMac()
    {
        if (OperatingSystem.IsMacOS())
        {
            List<AppModel> apps = new List<AppModel>();

            string[] binResult = CommandRunner.RunCommandOnLinux("ls -F /usr/bin | grep -v /").Split(Environment.NewLine);

            foreach (var app in binResult)
            {
                apps.Add(new AppModel(app, "/usr/bin"));
            }

            

            return apps.ToArray();
        }

        throw new PlatformNotSupportedException();
    }
    // ReSharper disable once IdentifierTypo
    protected static AppModel[] GetOnLinux(bool includeSnaps = false, bool includeFlatpaks = false)
    {
        if (OperatingSystem.IsLinux())
        {
#if NET5_0_OR_GREATER
            List<AppModel> apps = new List<AppModel>();

            string[] binResult = CommandRunner.RunCommandOnLinux("ls -F /usr/bin | grep -v /").Split(Environment.NewLine);
#else
            string[] binResult = CommandRunner.RunCommandOnLinux("ls -F /usr/bin | grep -v /").Split(Convert.ToChar(Environment.NewLine));
#endif

            foreach (var app in binResult)
            {
                apps.Add(new AppModel(app, "/usr/bin"));
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
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static AppModel[] Get()
    {
        if (OperatingSystem.IsWindows())
        {
            throw new NotImplementedException();
        }
        else if (OperatingSystem.IsMacOS())
        {
            throw new NotImplementedException();
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
        {
            return GetOnLinux(true, true);
        }

        throw new PlatformNotSupportedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="appModel"></param>
    /// <exception cref="PlatformNotSupportedException"></exception>
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