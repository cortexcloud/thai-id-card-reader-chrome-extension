using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThaiNationalIDCard;
using Newtonsoft.Json.Linq;
using System.IO;
using thai_id_card_reader_console_app.Models;
using System.Threading;

namespace thai_id_card_reader_console_app
{
    public class CardReader
    {
         private ThaiIDCard _idcard;
        private Model _resp;
        string _cardReaderName = ConfigurationManager.AppSettings["DEFAULT_CARD_READER_NAME"];
        string _cardStatus = "REMOVED"; //default;
        string _deviceStatus;

        enum CardStatus
        {
            INSERTED,
            REMOVED,
            LOADING,
            SUCCESS,
            EMPTY,
            FAIL,
        }

        enum DeviceStatus
        {
            AVAILABLE,
            INVALID,
            NOT_FOUND,
            ERROR
        }

        public CardReader()
        {
            _idcard = new ThaiIDCard();
            _resp = new Model();

            if (isAvailableCardReader())
            {
                _idcard.eventCardInserted += Idcard_eventCardInserted;
                _idcard.eventCardRemoved += Idcard_eventCardRemoved;
            }
        }

        private void Idcard_eventCardRemoved()
        {
            eventCardStatus(CardStatus.REMOVED);
        }

        private void Idcard_eventCardInserted(Personal personal)
        {

            eventCardStatus(CardStatus.INSERTED);

            Thread th = new Thread(new ThreadStart(() =>
            {
                try
                {
                    personal = _idcard.readAll();
                    eventCardStatus(CardStatus.SUCCESS, personal);
                }
                catch (Exception)
                {
                    eventCardStatus(CardStatus.FAIL);
                }

            }));

            th.Start();
            eventCardStatus(CardStatus.LOADING);
        }


        public bool isAvailableCardReader()
        {
            bool result = false;

            string[] cardReaderList;
            //check available card reader
            try
            {
                cardReaderList = _idcard.GetReaders();

                if (cardReaderList.Length > 0)
                {
                    //has card reader 
                    if (cardReaderList.Any(x => x == _cardReaderName))
                    {
                        //match card reader
                        _idcard.Open(_cardReaderName);
                        _deviceStatus = nameof(DeviceStatus.AVAILABLE);
                        result = true;
                    }
                    else
                    {
                        _deviceStatus = nameof(DeviceStatus.INVALID);
                    }
                }
                else
                {
                    _deviceStatus = nameof(DeviceStatus.NOT_FOUND);
                }
            }
            catch (Exception)
            {
                cardReaderList = new string[] { };
                _deviceStatus = nameof(DeviceStatus.ERROR);
            }

            return result;
        }

        public void MonitorStart()
        {
            _idcard.MonitorStart(_cardReaderName);
        }

        public void MonitorStop()
        {
            _idcard.MonitorStop(_cardReaderName);

            eventCardStatus(CardStatus.EMPTY, null);
        }

        private void SendToWeb(Model resp)
        {
            JObject o = JObject.Parse(JsonConvert.SerializeObject(resp));

            SendMessage(o);
        }

        public void SendMessage(JObject data)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data.ToString(Formatting.None));
            Stream stdout = Console.OpenStandardOutput();
            stdout.WriteByte((byte)((bytes.Length >> 0) & 0xFF));
            stdout.WriteByte((byte)((bytes.Length >> 8) & 0xFF));
            stdout.WriteByte((byte)((bytes.Length >> 16) & 0xFF));
            stdout.WriteByte((byte)((bytes.Length >> 24) & 0xFF));
            stdout.Write(bytes, 0, bytes.Length);
            stdout.Flush();
        }

        private void eventCardStatus(CardStatus status, Personal personal = null)
        {
            var value = (CardStatus)(int)status;
            _cardStatus = value.ToString();

            SendToWeb(new Model()
            {
                cardStatus = value.ToString(),
                deviceStatus = _deviceStatus,
                data = personal
            });
        }

        public string getCurrentStatus()
        {
            return _cardStatus;
        }
    }
}
