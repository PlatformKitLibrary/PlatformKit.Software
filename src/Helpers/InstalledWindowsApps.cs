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

using PlatformKit;

#if NETSTANDARD2_0 || NETSTANDARD2_1
using OperatingSystem = PlatformKit.Extensions.OperatingSystem.OperatingSystemExtension;
#endif

namespace PlatformKit.Software
{
    public class InstalledWindowsApps
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="includeWindowsSystemPrograms"></param>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        public static IEnumerable<AppModel> GetInstalled(bool includeWindowsSystemPrograms)
        {
            if (OperatingSystem.IsWindows())
            {
                List<AppModel> apps = new List<AppModel>();

                string programFiles = CommandRunner.RunCmdCommand(
                    $"dir {Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}");

                string programFilesX86 = CommandRunner.RunCmdCommand(
                    $"dir {Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}");

                string appData = CommandRunner
                    .RunCmdCommand($"dir {Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}");

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

                if (includeWindowsSystemPrograms)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        protected static IEnumerable<AppModel> ExpandWinSpecialFolderPath(string directory)
        {
            List<AppModel> apps = new List<AppModel>();

            if (OperatingSystem.IsWindows())
            {
                string[] files = directory.Split(Environment.NewLine);

                for (int programIndex = 0; programIndex < files.Length; programIndex++)
                {
                    string item = files[programIndex];

                    if (item.Contains("<DIR>"))
                    {
                        IEnumerable<AppModel> programs = GetExecutablesInDirectory(item);

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        public static IEnumerable<AppModel> GetExecutablesInDirectory(string folderPath)
        {
            if (OperatingSystem.IsWindows())
            {
                List<AppModel> apps = new List<AppModel>();
                string[] directories = Directory.GetDirectories(folderPath);

                for (int directoryIndex = 0; directoryIndex < directories.Length; directoryIndex++)
                {
                    string[] programs = Directory.GetFiles(directories[directoryIndex]);

                    foreach (var program in programs)
                    {
                        if (program.EndsWith(".exe") || program.EndsWith(".appx") || program.EndsWith(".msi"))
                        {
                            apps.Add(new AppModel(program, directories[directoryIndex]));
                        }
                    }
                }

                return apps.ToArray();
            }

            throw new PlatformNotSupportedException();
        }
    }
}
