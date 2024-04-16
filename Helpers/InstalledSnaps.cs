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

using PlatformKit;


namespace PlatformKit.Software
{

    public class InstalledSnaps
    {
        /// <summary>
        /// Detect what Snap packages (if any) are installed on a linux distribution or on macOS.
        /// </summary>
        /// <returns>Returns a list of installed snaps. Returns an empty array if no Snaps are installed.</returns>
        /// <exception cref="PlatformNotSupportedException">Throws an exception if run on a Platform other than Linux, macOS, and FreeBsd.</exception>
        public static AppModel[] Get()
        {
            List<AppModel> apps = new List<AppModel>();

            if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
            {
                bool useSnap = IsSnapInstalled();

                if (useSnap)
                {
                    string[] snapResults = CommandRunner.RunCommandOnLinux("ls " + Path.DirectorySeparatorChar + "snap" + Path.DirectorySeparatorChar + "bin").Split(' ');

                    foreach (string snap in snapResults)
                    {
                        apps.Add(new AppModel(snap, Path.DirectorySeparatorChar + "snap" + Path.DirectorySeparatorChar + "bin"));
                    }

                    return apps.ToArray();
                }

                apps.Clear();
                return apps.ToArray();
            }

            throw new PlatformNotSupportedException();
        }

        public static bool IsSnapInstalled()
        {
            if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
            {
                return  Directory.Exists(Path.DirectorySeparatorChar + "snap" + Path.DirectorySeparatorChar + "bin");
            }

            throw new PlatformNotSupportedException();
        }
        
        public static bool IsPackageInstalled(string packageName)
        {
            foreach (AppModel app in Get())
            {
                if (app.ExecutableName.Equals(packageName))
                {
                    return true;
                }       
            }

            return false;
        }
    }
}