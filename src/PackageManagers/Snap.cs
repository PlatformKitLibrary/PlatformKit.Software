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

using PlatformKit.Software.Abstractions;
using PlatformKit.Software.Exceptions;

namespace PlatformKit.Software.PackageManagers;

public class Snap : AbstractPackageManager
{
    public Snap()
    {
        PackageManagerName = "Snap";
    }
    
    /// <summary>
    /// Gets the names of updatable Snap packages.
    /// </summary>
    /// <returns>the updatable Snap packages as AppModel objects.</returns>
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("freebsd")]
    public override IEnumerable<AppModel> GetUpdatable()
    {
        if (DoesPackageManagerSupportThisOperatingSystem())
        {
            if (!IsPackageManagerInstalled())
            {
                throw new PackageManagerNotInstalledException(PackageManagerName);
            }
            
            List<AppModel> apps = new List<AppModel>();

            string[] snapUpdates = CommandRunner.RunCommandOnLinux("snap refresh --list").Split(Environment.NewLine);

            if (snapUpdates.Length > 1)
            {
                for (int i = 1; i < snapUpdates.Length; i++)
                {
                    string[] snapInfos = snapUpdates[i].Split(" ");
                    string snap = snapInfos[0];
                
                    apps.Add(new AppModel(snap,
                        $"{Path.DirectorySeparatorChar}snap{Path.DirectorySeparatorChar}bin"));
                }
            }
            else
            {
                apps.Clear();
                return apps.ToArray();
            }

            return apps.ToArray();
        }

        throw new PackageManagerNotSupportedException(PackageManagerName);
    }
    
    /// <summary>
    /// Detect what Snap packages (if any) are installed on a linux distribution or on macOS.
    /// </summary>
    /// <returns>Returns a list of installed snaps. Returns an empty array if no Snaps are installed.</returns>
    /// <exception cref="PlatformNotSupportedException">Throws an exception if run on a Platform other than Linux, macOS, and FreeBsd.</exception>
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("freebsd")]
    public override IEnumerable<AppModel> GetInstalled()
    {
        if (DoesPackageManagerSupportThisOperatingSystem())
        {
            if (!IsPackageManagerInstalled())
            {
                throw new PackageManagerNotInstalledException(PackageManagerName);
            }
            
            List<AppModel> apps = new List<AppModel>();

            string[] snapResults = CommandRunner.RunCommandOnLinux(
                    $"ls {Path.DirectorySeparatorChar}snap{Path.DirectorySeparatorChar}bin").Split(' ');

            foreach (string snap in snapResults)
            {
                apps.Add(new AppModel(snap, 
                    $"{Path.DirectorySeparatorChar}snap{Path.DirectorySeparatorChar}bin"));
            }

            return apps.ToArray();
        }

        throw new PackageManagerNotSupportedException(PackageManagerName);
    }

    public override bool DoesPackageManagerSupportThisOperatingSystem()
    {
        return OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD();
    }

    /// <summary>
    /// Detect if the Snap package manager is installed.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("freebsd")]
    public override bool IsPackageManagerInstalled()
    {
        if (DoesPackageManagerSupportThisOperatingSystem())
        {
            return Directory.Exists($"{Path.DirectorySeparatorChar}snap{Path.DirectorySeparatorChar}bin");
        }

        throw new PackageManagerNotInstalledException(PackageManagerName);
    }
}