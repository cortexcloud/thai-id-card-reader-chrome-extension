using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace thai_id_card_reader_window_services
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        private const string LogFile = @"Logs\log.txt";

        static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.Debug()
                   .WriteTo.Console()
                   .WriteTo.File(LogFile, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
                   .CreateLogger();

            var exitCode = HostFactory.Run(x =>
            {
                x.Service<CardReader>(y => 
                {
                    y.ConstructUsing(s => new CardReader());
                    y.WhenStarted(s => s.MonitorStart());
                    y.WhenStopped(s => s.MonitorStop());
                });

                x.RunAsLocalSystem();
                x.SetServiceName("HLABThaiIDCardReader");
                x.SetDisplayName("HLAB Thai ID Card Reader");
                x.SetDescription("Recieve thai id card information from card reader");
            });


            int exitCodeValue = ((int)exitCode);
            Environment.ExitCode = exitCodeValue;
//#if DEBUG
//            CardReaderService service = new CardReaderService();
//            service.OnDebug();
//            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
//#else
//            ServiceBase[] ServicesToRun;
//            ServicesToRun = new ServiceBase[]
//            {
//                new Service1()
//            };
//            ServiceBase.Run(ServicesToRun);
//#endif
        }
    }
}
