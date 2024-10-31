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

using System.Collections.Generic;
using PlatformKit.Software.Internal.Exceptions;

namespace PlatformKit.Software.Abstractions
{
    public abstract class AbstractPackageManager
    {
        public string PackageManagerName { get; protected set; }
    
        public abstract bool DoesPackageManagerSupportThisOperatingSystem();

        public abstract bool IsPackageManagerInstalled();

        public abstract IEnumerable<AppModel> GetUpdatable();
        public abstract IEnumerable<AppModel> GetInstalled();

        private string CleanUpExecutableName(string executableName)
        {
            string newName = executableName;
        
            if (newName.EndsWith(".exe"))
            {
                newName = newName.Replace(".exe", string.Empty);
            }
            if (newName.EndsWith(".msi"))
            {
                newName = newName.Replace(".msi", string.Empty);
            }
            if (newName.EndsWith(".appx"))
            {
                newName = newName.Replace(".appx", string.Empty);
            }
            if (newName.EndsWith(".app"))
            {
                newName = newName.Replace(".app", string.Empty);
            }

            return newName;
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        /// <exception cref="PackageManagerNotInstalledException"></exception>
        /// <exception cref="PackageManagerNotSupportedException"></exception>
        public bool IsPackageInstalled(string packageName)
        {
            if (DoesPackageManagerSupportThisOperatingSystem())
            {
                if (!IsPackageManagerInstalled())
                {
                    throw new PackageManagerNotInstalledException(PackageManagerName);
                }

                bool foundPackage = false;

                string newPackageName = CleanUpExecutableName(packageName);
            
                foreach (AppModel app in GetInstalled())
                {
                    string tempAppName = CleanUpExecutableName(app.ExecutableName);
                
                    if (tempAppName.Equals(newPackageName.ToLower()))
                    {
                        foundPackage = true;
                    }
                }

                return foundPackage;
            }

            throw new PackageManagerNotSupportedException(PackageManagerName);
        }
    }
}