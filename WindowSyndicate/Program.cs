using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace WindowSyndicate
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (Environment.UserInteractive || (args.Select(s => s.ToUpper()).Contains("-console") || args.Select(s => s.ToUpper()).Contains("--console")))
            {
                var service = new WindowService();
                service.TestStartupAndStop(null);
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new WindowService()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
