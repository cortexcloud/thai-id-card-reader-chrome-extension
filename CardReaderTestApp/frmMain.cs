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
        string _cardReaderName = ConfigurationManager.AppSettings["DEFAULT_CARD_READER_NAME"];

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
            try
            {
                string[] cardReaderList = _idcard?.GetReaders();
                foreach (var reader in cardReaderList)
                {
                    lstCardReader.Items.Add(reader);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _idcard.MonitorStop(_cardReaderName);
            //_idcard.Close();
        }

        private void btnReadData_Click(object sender, EventArgs e)
        {
            if (lstCardReader.Items.Count > 0)
            {
                if (lstCardReader.SelectedIndex > -1)
                {
                    updateResult(string.Empty);

                    _cardReaderName = lstCardReader.SelectedItem.ToString();

                    _idcard.Open(_cardReaderName);

                    var personal = _idcard.readAll(false, _cardReaderName);

                    string jsonResponse = JsonConvert.SerializeObject(personal);

                    updateResult(jsonResponse);
                }
                else
                {
                    MessageBox.Show("Please select one card reader", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("No card reader.", "Error",MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            txtResult.Text = String.Empty;

            if(lstCardReader.Items.Count > 0)
            {
                lstCardReader.SelectedIndex = 0;
            }
        }
    }
}
