using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ProxyMySystem.OperationalSystem
{
    public static class Windows
    {
        public static bool IsAdministrator()
        {
            var currentUser = WindowsIdentity.GetCurrent();
            return new WindowsPrincipal(currentUser).IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static string GetCurrentUserFolder()
        {
            string path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            if (Environment.OSVersion.Version.Major >= 6)
            {
                path = Directory.GetParent(path).ToString();
            }

            return path;
        }
    }
}