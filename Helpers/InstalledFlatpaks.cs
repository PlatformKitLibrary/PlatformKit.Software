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

namespace PlatformKit.Software
{

// ReSharper disable once ClassNeverInstantiated.Global
    public class InstalledFlatpaks
    {
        /// <summary>
        /// Platforms Supported On: Linux and FreeBsd.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public static AppModel[] Get()
        {
            List<AppModel> apps = new List<AppModel>();

            if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
            {
                if (IsFlatpakInstalled())
                {
                    string[] flatpakResults = CommandRunner.RunCommandOnLinux("flatpak list --columns=name")
                    .Split(Environment.NewLine);

                    string installLocation = CommandRunner.RunCommandOnLinux("flatpak --installations");

                    foreach (string flatpak in flatpakResults)
                    {
                        apps.Add(new AppModel(flatpak, installLocation));
                    }

                    return apps.ToArray();
                }

                apps.Clear();
                return apps.ToArray();
            }

            throw new PlatformNotSupportedException();
        }

        public static bool IsFlatpakInstalled()
        {
            string[] flatpakTest = CommandRunner.RunCommandOnLinux("flatpak --version").Split(' ');

            if (flatpakTest[0].Contains("Flatpak"))
            {
                Version.Parse(flatpakTest[1]);

                return true;
            }

            return false;
        }
    }
}