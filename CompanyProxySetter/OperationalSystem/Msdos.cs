using CliWrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyProxySetter.OperationalSystem
{
    public static class Msdos
    {
        public static bool IsInstalled(string app)
        {
            try
            {
                Run(app, string.Empty);
                return true;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The system cannot find the file specified"))
                    return false;
                else
                    throw;
            }
        }

        public static void Run(string app, string param)
        {
            using (var cli = new Cli(app))
            {
                cli.ExecuteAndForget(param);
            }
        }
    }
}