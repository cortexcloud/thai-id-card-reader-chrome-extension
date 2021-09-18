using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace thai_id_card_reader_console_app
{
    class Program
    {
        static void Main(string[] args)
        {

            var _cardReader = new CardReader();

            _cardReader.MonitorStart();
            //var exitCode = HostFactory.Run(x =>
            //{
            //    x.Service<CardReader>(y =>
            //    {
            //        y.ConstructUsing(s => new CardReader());
            //        y.WhenStarted(s => s.MonitorStart());
            //        y.WhenStopped(s => s.MonitorStop());
            //    });

            //    x.RunAsLocalSystem();
            //    x.SetServiceName("HLABThaiIDCardReader");
            //    x.SetDisplayName("HLAB Thai ID Card Reader");
            //    x.SetDescription("Recieve thai id card information from card reader");
            //});


            //int exitCodeValue = ((int)exitCode);
            //Environment.ExitCode = exitCodeValue;
        }
    }
}
