using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace CompanyProxySetter.OperationalSystem
{
    public static class Windows
    {
        public static bool IsAdministrator()
        {
            var currentUser = WindowsIdentity.GetCurrent();
            return new WindowsPrincipal(currentUser).IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}