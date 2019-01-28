using CliWrap;
using CliWrap.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyAtWork.OperationalSystem
{
    public static class Msdos
    {
        public static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static bool IsInstalled(string app)
        {
            try
            {
                logger.Debug($"Find for {app}");
                Run(app, string.Empty, false);
                return true;
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                if (ex.NativeErrorCode == 2)
                    return false;

                logger.Error(ex);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        public static ExecutionOutput Run(string app, string param, bool log = true)
        {
            
            var paths = Environment.GetEnvironmentVariable("PATH")?.Split(';');


            foreach (var path in paths)
            {
                var exec = Path.Combine(path, app);

                // Execute like a program
                if (File.Exists(exec))
                {
                    var cliSettings = new CliSettings()
                    {
                        WorkingDirectory = path
                    };

                    using (var cli = new Cli(app, cliSettings))
                    {
                        var result = cli.Execute(param);

                        if (log)
                        {
                            logger.Debug($"Configure application: {app} in {path}");

                            if (!string.IsNullOrEmpty(result.StandardOutput))
                                logger.Debug(result.StandardOutput);

                            if (!string.IsNullOrEmpty(result.StandardError))
                                logger.Error(result.StandardError);
                        }
                            
                        return result;
                    }
                }
            }

            // Execute like command
            using (var cli = new Cli(app))
            {
                var result = cli.Execute(param);

                if (log)
                {
                    logger.Debug($"Configure command: {app}");

                    if (!string.IsNullOrEmpty(result.StandardOutput))
                        logger.Debug(result.StandardOutput);

                    if (!string.IsNullOrEmpty(result.StandardOutput))
                        logger.Error(result.StandardError);
                }

                return result;
            }
        }
    }
}