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

        public static bool IsInstalled(string app, string param = null)
        {
            try
            {
                logger.Debug($"Find for {app}");
                Run(app, param, false);
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

        public static ExecutionResult Run(string app, string param, bool log = true)
        {
            var paths = Environment.GetEnvironmentVariable("PATH")?.Split(';');

            foreach (var path in paths)
            {
                var exec = Path.Combine(path, app);

                // Execute like a program
                if (File.Exists(exec))
                {
                    var result = Cli.Wrap(Path.GetFileName(exec))
                        .EnableExitCodeValidation(false)
                        .SetWorkingDirectory(Path.GetDirectoryName(exec))
                        .SetArguments(param ?? "")
                        .Execute();

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

            // Execute like command
            var resultCommand = Cli.Wrap(app)
                .EnableExitCodeValidation(false)
                .Execute();

            if (log)
            {
                logger.Debug($"Configure command: {app}");

                if (!string.IsNullOrEmpty(resultCommand.StandardOutput))
                    logger.Debug(resultCommand.StandardOutput);

                if (!string.IsNullOrEmpty(resultCommand.StandardOutput))
                    logger.Error(resultCommand.StandardError);
            }

            return resultCommand;
        }
    }
}