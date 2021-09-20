using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using thai_id_card_reader_window_app.Models;
using ThaiNationalIDCard;

namespace thai_id_card_reader_window_app
{
    public partial class frmMain : Form
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


        public frmMain()
        {
            InitializeComponent();

            _idcard = new ThaiIDCard();
           
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.
            Opacity = 0;

            if (isAvailableCardReader())
            {
                _idcard.eventCardInserted += Idcard_eventCardInserted;
                _idcard.eventCardRemoved += Idcard_eventCardRemoved;
                _idcard.MonitorStart(_cardReaderName);
            }
            
        }

        private void Idcard_eventCardRemoved()
        {
            eventCardStatus(CardStatus.REMOVED);

            updateResult("");

        }

        private void Idcard_eventCardInserted(Personal personal)
        {
            eventCardStatus(CardStatus.INSERTED);

            Thread th = new Thread(new ThreadStart(() =>
            {
                try
                {
                    personal = _idcard.readAll();

                    string jsonResponse = JsonConvert.SerializeObject(personal);

                    updateResult(jsonResponse);

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


        public void updateResult(string result)
        {
            if (txtResult.InvokeRequired)
            {
                Action safeWrite = delegate { updateResult(result); };
                txtResult.Invoke(safeWrite);
            }
            else
            {
                if (string.IsNullOrEmpty(result) || result.Trim().Length <= 0)
                {
                    txtResult.Text = string.Empty;
                }
                else
                {
                    txtResult.AppendText(result + Environment.NewLine);
                }
            }
        }

        private void eventCardStatus(CardStatus status, Personal personal = null)
        {
            var value = (CardStatus)(int)status;
            string jsonResponse = JsonConvert.SerializeObject(personal);


            SendToWeb(new ResponseModel()
            {
                cardStatus = value.ToString(),
                deviceStatus = _deviceStatus,
                data = personal
            });
        }

        private void SendToWeb(ResponseModel resp)
        {
            string jsonResponse = JsonConvert.SerializeObject(resp);

            JObject o = JObject.Parse(jsonResponse);

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

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _idcard.MonitorStop(_cardReaderName);
            //_idcard.Close();
        }
    }
}
