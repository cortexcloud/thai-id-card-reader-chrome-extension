using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
//using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using thai_id_card_reader_console_app.Models;
using ThaiNationalIDCard;

namespace thai_id_card_reader_console_app
{
    class Program
    {
        static int _countingTimeout = 0;

        static int _timeout = int.Parse(ConfigurationManager.AppSettings["DEFAULT_LIMIT_TIMEOUT"]);
        static string _cardReaderName = ConfigurationManager.AppSettings["DEFAULT_CARD_READER_NAME"];

        static ThaiIDCard _idcard = new ThaiIDCard();
        static CardStatus _currentCardStatus = CardStatus.REMOVED;
        static DeviceStatus _currentDeviceStatus = DeviceStatus.NOT_FOUND;

        static JObject data;
        static Thread _thread;

        enum CardStatus
        {
            INSERTED,
            REMOVED,
            LOADING,
            SUCCESS,
            TIMEOUT,
            FAIL,
        }

        enum DeviceStatus
        {
            AVAILABLE,
            INVALID,
            NOT_FOUND,
            ERROR
        }


        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);

            try
            {
                if (isAvailableCardReader())
                {
                    _idcard.eventCardInserted += Idcard_eventCardInserted;
                    _idcard.eventCardRemoved += Idcard_eventCardRemoved;
                    _idcard.MonitorStart(_cardReaderName);

                    //check card inserted before app start then warning user to remove card first
                    if (isCardInserted())
                    {
                        var resp = new ResponseModel()
                        {
                            cardStatus = nameof(CardStatus.FAIL),
                            deviceStatus = nameof(DeviceStatus.AVAILABLE)
                        };

                        SendToWeb(resp);
                    }
                    //else
                    //{
                    //    //not yet card insert then waiting user action
                    //    waitingInsertCardTimeout();
                    //}



                }
            }
            catch (Exception ex)
            {
                var resp = new ResponseModel()
                {
                    cardStatus = nameof(CardStatus.FAIL),
                    deviceStatus = nameof(DeviceStatus.ERROR),
                    data = ex.Message
                };

                SendToWeb(resp);
            }


            //long live listening
            while ((data = Read()) != null)
            {
                var processed = ProcessMessage(data);
                SendMessage(data);
                if (processed == "exit")
                {
                    return;
                }
            }

        }

        //console self listening
        private static string ProcessMessage(JObject data)
        {
            var message = data["text"].Value<string>();
            switch (message)
            {
                case "test":
                    return "testing!";
                case "exit":
                    return "exit";
                default:
                    return "echo: " + message;
            }
        }

        private static JObject Read()
        {
            var stdin = Console.OpenStandardInput();
            var length = 0;

            var lengthBytes = new byte[4];
            stdin.Read(lengthBytes, 0, 4);
            length = BitConverter.ToInt32(lengthBytes, 0);

            var buffer = new char[length];
            using (var reader = new StreamReader(stdin))
            {
                while (reader.Peek() >= 0)
                {
                    reader.Read(buffer, 0, buffer.Length);
                }
            }

            return (JObject)JsonConvert.DeserializeObject<JObject>(new string(buffer));
        }

        private static void SendToWeb(ResponseModel resp)
        {
            JObject o = JObject.Parse(JsonConvert.SerializeObject(resp));

            SendMessage(o);
        }

        private static void SendMessage(JObject data)
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

        private static void eventCardStatus(CardStatus status, Personal personal = null)
        {
            //update current card status
            _currentCardStatus = status;

            var cardStatusValue = (CardStatus)(int)status;
            var deviceStatusValue = (DeviceStatus)(int)_currentDeviceStatus;

            var resp = new ResponseModel()
            {
                cardStatus = cardStatusValue.ToString(),
                deviceStatus = deviceStatusValue.ToString(),
                data = personal
            };

            SendToWeb(resp);
        }

        #region === WAITING INSERT CARD TIMEOUT (NOT USE) ===

        private static void validateWaitingInsertCardTimeout()
        {
            //new thread after card remove then waiting insert card timeout
            _thread = new Thread(waitingInsertCardTimeout);
            _thread.Start();
        }

        private static void waitingInsertCardTimeout()
        {
            while (_currentCardStatus != CardStatus.TIMEOUT)
            {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                Thread.Sleep(1000);
                _countingTimeout++;


                if (_currentCardStatus == CardStatus.REMOVED && _countingTimeout <= _timeout)
                {
                    string jsonResult = JsonConvert.SerializeObject(new MessageModel() { text = "Counting " + _countingTimeout });
                    JObject jObject = JObject.Parse(jsonResult);
                    SendMessage(jObject);
                }
                else if (_currentCardStatus != CardStatus.REMOVED && _countingTimeout <= _timeout)
                {
                    _countingTimeout = 0;
                    _thread.Abort();
                    //string jsonResult = JsonConvert.SerializeObject(new MessageModel() { text = "Counting " + _counting });
                    //JObject jObject = JObject.Parse(jsonResult);
                    //SendMessage(jObject);
                }
                else
                {
                    if (Thread.CurrentThread.ThreadState == System.Threading.ThreadState.Running)
                    {
                        _countingTimeout = 0;
                        eventCardStatus(CardStatus.TIMEOUT);
                        _thread.Abort();
                    }
                }
            }
        }

        #endregion

        #region === LOADING CARD TIMEOUT ===

        private static void validateLoadingCardTimeout()
        {
            _thread = new Thread(loadingTimeout);
            _thread.Start();
        }

        private static void loadingTimeout()
        {
            while (_currentCardStatus != CardStatus.TIMEOUT)
            {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                Thread.Sleep(1000);
                _countingTimeout++;


                if (_currentCardStatus == CardStatus.LOADING && _countingTimeout > _timeout)
                {
                    if (Thread.CurrentThread.ThreadState == System.Threading.ThreadState.Running)
                    {
                        eventCardStatus(CardStatus.TIMEOUT);
                        _countingTimeout = 0;
                        _thread.Abort();
                    }
                }
            }
        }

        #endregion

        private static void Idcard_eventCardRemoved()
        {
            eventCardStatus(CardStatus.REMOVED);
        }

        private static void Idcard_eventCardInserted(Personal personal)
        {
            eventCardStatus(CardStatus.INSERTED);

            //interval delay
            Thread.Sleep(1000);

            eventCardStatus(CardStatus.LOADING);

            validateLoadingCardTimeout();

            try
            {
                personal = _idcard.readAll();
                if (_currentCardStatus != CardStatus.TIMEOUT)
                {
                    eventCardStatus(CardStatus.SUCCESS, personal);
                }
            }
            catch
            {
                eventCardStatus(CardStatus.FAIL);
                _thread.Abort();
            }

            //interval delay
            Thread.Sleep(1000);
        }

        private static bool isAvailableCardReader()
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
                        _currentDeviceStatus = DeviceStatus.AVAILABLE;
                        result = true;
                    }
                    else
                    {
                        _currentDeviceStatus = DeviceStatus.INVALID;
                    }
                }
                else
                {
                    _currentDeviceStatus = DeviceStatus.NOT_FOUND;
                }
            }
            catch (Exception)
            {
                cardReaderList = new string[] { };
                _currentDeviceStatus = DeviceStatus.ERROR;
            }

            return result;
        }


        private static bool isCardInserted()
        {
            try
            {
                var personal = _idcard.readCitizenid();
                if (personal != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            _idcard.MonitorStop(_cardReaderName);
            Environment.Exit(1);
        }

    }
}
