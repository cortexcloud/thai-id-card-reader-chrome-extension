using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThaiNationalIDCard;
using Serilog;
using Serilog.Sinks.SystemConsole;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog.Formatting.Compact;
using Newtonsoft.Json.Linq;
using System.IO;
using thai_id_card_reader_window_services.Models;
using System.Threading;

namespace thai_id_card_reader_window_services
{
    public class CardReader
    {
        private readonly ThaiIDCard _idcard;
        private ResponseModel _resp;
        string _cardReaderName = ConfigurationManager.AppSettings["DEFAULT_CARD_READER_NAME"];
        string _cardStatus;
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
            _resp = new ResponseModel();

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
                    eventCardStatus(CardStatus.SUCCESS,personal);
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

        public string ReadCard()
        {
            string jsonResult = "";
            try
            {

                Personal personal = _idcard.readAll();
                if (personal != null)
                {
                    jsonResult = JsonConvert.SerializeObject(personal, new JsonSerializerSettings() { Culture = new System.Globalization.CultureInfo("th-TH") });

                }
                else if (_idcard.ErrorCode() > 0)
                {
                    Console.WriteLine(_idcard.Error());
                }

            }
            catch (Exception ex)
            {
                //StopRead(ex);
            }

            return jsonResult;
        }


        public void MonitorStart()
        {
            _idcard.MonitorStart(_cardReaderName);
        }

        public void MonitorStop()
        {
            _idcard.MonitorStop(_cardReaderName);
        }

        private void SendToWeb(ResponseModel resp)
        {
            //string json = @"{
            //    name: 'cortex cloud'
            //    status : 
            //}";

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

            SendToWeb(new ResponseModel()
            {
                cardStatus = value.ToString(),
                deviceStatus = _deviceStatus,
                data = personal
            });
        }

    }
}
