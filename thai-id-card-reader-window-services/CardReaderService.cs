using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ThaiIDCardServices
{
    public partial class CardReaderService : ServiceBase
    {
        CardReader _cardReader;
        string _cardReaderName = ConfigurationManager.AppSettings["DEFAULT_CARD_READER_NAME"];

        public CardReaderService()
        {
            InitializeComponent();

            _cardReader = new CardReader(_cardReaderName);
        }

        protected override void OnStart(string[] args)
        {
            _cardReader.MonitorStart();   
        }

        protected override void OnStop()
        {
            _cardReader.MonitorStop();
        }

        public void OnDebug()
        {
            this.OnStart(null);
        }
    }
}
