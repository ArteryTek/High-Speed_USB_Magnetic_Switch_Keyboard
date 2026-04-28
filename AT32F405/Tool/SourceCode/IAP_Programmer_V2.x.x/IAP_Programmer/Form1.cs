using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Runtime.InteropServices;

namespace IAP_Demo
{
    public partial class IAPDemo : Form
    {
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
        private static extern uint timeBeginPeriod(uint uMilliseconds);

        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
        private static extern uint timeEndPeriod(uint uMilliseconds);

        public enum PortType
        {
            PORT_TYPE_RS232 = 0,
            PORT_TYPE_USB = 1,
            PORT_TYPE_CAN = 2,
            PORT_TYPE_SPI = 3,
            PORT_TYPE_I2C = 4,
        }

        public class FILEPARTINFO
        {
            public uint PartStartAddress;
            public int PartEndAddress;
            public int PartSize;
            public int PartChecksum;
            public byte[] PartData;
        }

        public static string[][] BuadRate = new string[4][]
        {
            //RS232
            new string[]
            {
                "110",
                "300",
                "600",
                "1200",
                "2400",
                "4800",
                "9600",
                "14400",
                "19200",
                "38400",
                "57600",
                "115200",
                "128000",
                "230400",
                "256000",
                "460800",
                "921600",
                "1228800",
                "1382400"
            },     
            //CAN
            new string[]
            {
                "1Mbit",
                "500Kbit",
                "250Kbit",
                "125Kbit"
            },    
            //SPI
            new string[]
            {
                "13.5MHz",
                "6.75MHz",
                "3.375MHz",
                "1.6875MHz",
                "0.84375MHz"
            },    
            //I2C
            new string[]
            {
                "1M",
                "400K",
                "200K",
                "100K",
                "50K",
                "10K"
            },
        };

        SerialPort sp = new SerialPort();

        delegate void dShowForm();
        ProgressBarForm frm_Progress = new ProgressBarForm();

        public static object lockIniDevice = new object();
        public bool m_InitFlag = false;

        public int m_PortNameSelectUSB = 0;
        public int m_PortNameSelectUART = 0;

        public uint I2CAddress = 0;

        Thread newth = null;

        public Hid oSp = new Hid();
        ushort uPID = 0xAF01;
        ushort uVID = 0x2E3C;
        ushort uBridgeDevicePID = 0xFF01;

        public const byte USBCOMMOND_MSB = 0x5A;
        public const byte USBCOMMOND_IAPMODE = 0xA0;
        public const byte USBCOMMOND_BEGINDOWN = 0xA1;
        public const byte USBCOMMOND_ADDRESS = 0xA2;
        public const byte USBCOMMOND_SENDDATA = 0xA3;
        public const byte USBCOMMOND_SENDEND = 0xA4;
        public const byte USBCOMMOND_CRC = 0xA5;
        public const byte USBCOMMOND_JUMPAPP = 0xA6;
        public const byte USBCOMMOND_GETAPPADDR = 0xA7;

        public const byte USBCOMMOND_ACK_MSB = 0xFF;
        public const byte USBCOMMOND_ACK_LSB = 0x00;
        public const byte USBCOMMOND_NACK_MSB = 0x00;
        public const byte USBCOMMOND_NACK_LSB = 0xFF;

        public const int USB_PART_LENGTH = 1024;

        static public uint[] crc32table = { 0x00000000, 0x04c11db7, 0x09823b6e, 0x0d4326d9, 0x130476dc, 0x17c56b6b,
                                     0x1a864db2, 0x1e475005, 0x2608edb8, 0x22c9f00f, 0x2f8ad6d6, 0x2b4bcb61,
                                     0x350c9b64, 0x31cd86d3, 0x3c8ea00a, 0x384fbdbd, 0x4c11db70, 0x48d0c6c7,
                                     0x4593e01e, 0x4152fda9, 0x5f15adac, 0x5bd4b01b, 0x569796c2, 0x52568b75,
                                     0x6a1936c8, 0x6ed82b7f, 0x639b0da6, 0x675a1011, 0x791d4014, 0x7ddc5da3,
                                     0x709f7b7a, 0x745e66cd, 0x9823b6e0, 0x9ce2ab57, 0x91a18d8e, 0x95609039,
                                     0x8b27c03c, 0x8fe6dd8b, 0x82a5fb52, 0x8664e6e5, 0xbe2b5b58, 0xbaea46ef,
                                     0xb7a96036, 0xb3687d81, 0xad2f2d84, 0xa9ee3033, 0xa4ad16ea, 0xa06c0b5d,
                                     0xd4326d90, 0xd0f37027, 0xddb056fe, 0xd9714b49, 0xc7361b4c, 0xc3f706fb,
                                     0xceb42022, 0xca753d95, 0xf23a8028, 0xf6fb9d9f, 0xfbb8bb46, 0xff79a6f1,
                                     0xe13ef6f4, 0xe5ffeb43, 0xe8bccd9a, 0xec7dd02d, 0x34867077, 0x30476dc0,
                                     0x3d044b19, 0x39c556ae, 0x278206ab, 0x23431b1c, 0x2e003dc5, 0x2ac12072,
                                     0x128e9dcf, 0x164f8078, 0x1b0ca6a1, 0x1fcdbb16, 0x018aeb13, 0x054bf6a4,
                                     0x0808d07d, 0x0cc9cdca, 0x7897ab07, 0x7c56b6b0, 0x71159069, 0x75d48dde,
                                     0x6b93dddb, 0x6f52c06c, 0x6211e6b5, 0x66d0fb02, 0x5e9f46bf, 0x5a5e5b08,
                                     0x571d7dd1, 0x53dc6066, 0x4d9b3063, 0x495a2dd4, 0x44190b0d, 0x40d816ba,
                                     0xaca5c697, 0xa864db20, 0xa527fdf9, 0xa1e6e04e, 0xbfa1b04b, 0xbb60adfc,
                                     0xb6238b25, 0xb2e29692, 0x8aad2b2f, 0x8e6c3698, 0x832f1041, 0x87ee0df6,
                                     0x99a95df3, 0x9d684044, 0x902b669d, 0x94ea7b2a, 0xe0b41de7, 0xe4750050,
                                     0xe9362689, 0xedf73b3e, 0xf3b06b3b, 0xf771768c, 0xfa325055, 0xfef34de2,
                                     0xc6bcf05f, 0xc27dede8, 0xcf3ecb31, 0xcbffd686, 0xd5b88683, 0xd1799b34,
                                     0xdc3abded, 0xd8fba05a, 0x690ce0ee, 0x6dcdfd59, 0x608edb80, 0x644fc637,
                                     0x7a089632, 0x7ec98b85, 0x738aad5c, 0x774bb0eb, 0x4f040d56, 0x4bc510e1,
                                     0x46863638, 0x42472b8f, 0x5c007b8a, 0x58c1663d, 0x558240e4, 0x51435d53,
                                     0x251d3b9e, 0x21dc2629, 0x2c9f00f0, 0x285e1d47, 0x36194d42, 0x32d850f5,
                                     0x3f9b762c, 0x3b5a6b9b, 0x0315d626, 0x07d4cb91, 0x0a97ed48, 0x0e56f0ff,
                                     0x1011a0fa, 0x14d0bd4d, 0x19939b94, 0x1d528623, 0xf12f560e, 0xf5ee4bb9,
                                     0xf8ad6d60, 0xfc6c70d7, 0xe22b20d2, 0xe6ea3d65, 0xeba91bbc, 0xef68060b,
                                     0xd727bbb6, 0xd3e6a601, 0xdea580d8, 0xda649d6f, 0xc423cd6a, 0xc0e2d0dd,
                                     0xcda1f604, 0xc960ebb3, 0xbd3e8d7e, 0xb9ff90c9, 0xb4bcb610, 0xb07daba7,
                                     0xae3afba2, 0xaafbe615, 0xa7b8c0cc, 0xa379dd7b, 0x9b3660c6, 0x9ff77d71,
                                     0x92b45ba8, 0x9675461f, 0x8832161a, 0x8cf30bad, 0x81b02d74, 0x857130c3,
                                     0x5d8a9099, 0x594b8d2e, 0x5408abf7, 0x50c9b640, 0x4e8ee645, 0x4a4ffbf2,
                                     0x470cdd2b, 0x43cdc09c, 0x7b827d21, 0x7f436096, 0x7200464f, 0x76c15bf8,
                                     0x68860bfd, 0x6c47164a, 0x61043093, 0x65c52d24, 0x119b4be9, 0x155a565e,
                                     0x18197087, 0x1cd86d30, 0x029f3d35, 0x065e2082, 0x0b1d065b, 0x0fdc1bec,
                                     0x3793a651, 0x3352bbe6, 0x3e119d3f, 0x3ad08088, 0x2497d08d, 0x2056cd3a,
                                     0x2d15ebe3, 0x29d4f654, 0xc5a92679, 0xc1683bce, 0xcc2b1d17, 0xc8ea00a0,
                                     0xd6ad50a5, 0xd26c4d12, 0xdf2f6bcb, 0xdbee767c, 0xe3a1cbc1, 0xe760d676,
                                     0xea23f0af, 0xeee2ed18, 0xf0a5bd1d, 0xf464a0aa, 0xf9278673, 0xfde69bc4,
                                     0x89b8fd09, 0x8d79e0be, 0x803ac667, 0x84fbdbd0, 0x9abc8bd5, 0x9e7d9662,
                                     0x933eb0bb, 0x97ffad0c, 0xafb010b1, 0xab710d06, 0xa6322bdf, 0xa2f33668,
                                     0xbcb4666d, 0xb8757bda, 0xb5365d03, 0xb1f740b4
                                     };
        
        void ShowForm()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new dShowForm(this.ShowForm));
            }
            else
            {
                frm_Progress.ShowDialog(this);
            }
        }

        public IAPDemo()
        {
            InitializeComponent();

            timeBeginPeriod(1);

            if (File.Exists(ShareParameter.ConfigFilePath))
            {
                XmlDocument xmlDoc = OperateXML.LoadXMLFile(ShareParameter.ConfigFilePath);
                XmlElement root = xmlDoc.DocumentElement;
                OperateXML.LoadValue(xmlDoc);
                LoadSettrings();
            }
            else
            {
                XmlDocument xmlDoc = OperateXML.CreateXMLFile();
                XmlElement root = xmlDoc.DocumentElement;
                OperateXML.AddInfo(xmlDoc);
                OperateXML.SaveXMLDoc(xmlDoc, ShareParameter.ConfigFilePath);
                OperateXML.LoadValue(xmlDoc);
                LoadSettrings();
            }

            m_InitFlag = true;

            Control.CheckForIllegalCrossThreadCalls = false;
            newth = new System.Threading.Thread(new System.Threading.ThreadStart(InitDeviceThread));
            newth.CurrentCulture = Thread.CurrentThread.CurrentCulture;
            newth.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
            newth.Start();
        }

        private void LoadSettrings()
        {
            int index = 0;

            //PortType
            try
            {
                index = int.Parse(ShareParameter.PortType);
            }
            catch
            {
                index = 0;
            }
            if (index < comboBox_PortType.Items.Count)
                comboBox_PortType.SelectedIndex = index;
            else
                comboBox_PortType.SelectedIndex = 0;

            //PortName
            try
            {
                index = int.Parse(ShareParameter.COMPort);
            }
            catch
            {
                index = -1;
            }
            m_PortNameSelectUSB = index;
            m_PortNameSelectUART = index;
            //boud rate
            for (index = 0; index < comboBoxBaudRate.Items.Count; index++)
            {
                if (comboBoxBaudRate.Items[index].ToString() == ShareParameter.BoudRate)
                {
                    comboBoxBaudRate.SelectedIndex = index;
                    break;
                }
            }
            if (index == comboBoxBaudRate.Items.Count)
            {
                comboBoxBaudRate.Text = ShareParameter.BoudRate;
            }

            //crc
            if (ShareParameter.CRC.ToUpper() == "FALSE")
                checkBox_CRC.Checked = false;
            else
                checkBox_CRC.Checked = true;

            //APP
            textBox_AppAddress.Text = ShareParameter.AppAddress;
            textBox_I2CAddress.Text = ShareParameter.I2CAddress;
            textBoxFile1Path.Text = ShareParameter.AppDownloadFile;
            textBox_VID.Text = ShareParameter.VID;
            textBox_PID.Text = ShareParameter.PID;
        }

        private void SetShareParameter()
        {
            if (File.Exists(ShareParameter.ConfigFilePath))
            {
                XmlDocument xmlDoc = OperateXML.LoadXMLFile(ShareParameter.ConfigFilePath);
                OperateXML.SetXMLNodeInnerText(xmlDoc, @"Configuration/PortType", comboBox_PortType.SelectedIndex.ToString());
                //                if(comboBoxPortName.SelectedIndex !=-1)
                OperateXML.SetXMLNodeInnerText(xmlDoc, @"Configuration/COMPort", comboBoxPortName.SelectedIndex.ToString());
                //                 else
                //                     OperateXML.SetXMLNodeInnerText(xmlDoc, @"Configuration/COMPort", "-1");
                OperateXML.SetXMLNodeInnerText(xmlDoc, @"Configuration/BoudRate", comboBoxBaudRate.Text);
                if (checkBox_CRC.Checked)
                    OperateXML.SetXMLNodeInnerText(xmlDoc, @"Configuration/CRC", "true");
                else
                    OperateXML.SetXMLNodeInnerText(xmlDoc, @"Configuration/CRC", "false");
                OperateXML.SetXMLNodeInnerText(xmlDoc, @"Configuration/AppAddress", textBox_AppAddress.Text);
                OperateXML.SetXMLNodeInnerText(xmlDoc, @"Configuration/I2CAddress", textBox_I2CAddress.Text);
                OperateXML.SetXMLNodeInnerText(xmlDoc, @"Configuration/AppDownloadFile", textBoxFile1Path.Text);
                OperateXML.SetXMLNodeInnerText(xmlDoc, @"Configuration/VID", textBox_VID.Text);
                OperateXML.SetXMLNodeInnerText(xmlDoc, @"Configuration/PID", textBox_PID.Text);

                OperateXML.SaveXMLDoc(xmlDoc, ShareParameter.ConfigFilePath);
            }
        }

        private void InitDeviceThread()
        {
            while (m_InitFlag)
            {
                InitDevice();
                Thread.Sleep(1000);
            }
        }

        private void buttonOpenfile1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Supported Files(*.bin,*.hex)|*.bin;*.hex|Intel BIN Files(*.bin)|*.bin|Hes Files(*.hex)|*.hex|All FIles(*.*)|*.*";
            ofd.Title = "打开文件";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string temp3 = ofd.FileName.Substring(ofd.FileName.LastIndexOf(@"\"));
                string tempType = temp3.ToUpper();

                if (tempType.Substring(tempType.Length - 3, 3) == "HEX")
                {
                    List<FILEPARTINFO> FIleInfo = null;
                    uint StartAddr = 0;
                    if (!GetFileInfo(out StartAddr, out FIleInfo, ofd.FileName))
                    {
                        MessageBox.Show("The download file open error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, 0);
                        return;
                    }
                    textBox_AppAddress.Text = FIleInfo[0].PartStartAddress.ToString("X8");
                    //textBox_AppAddress.Enabled = false;
                    //label1.Enabled = false;
                }
                else if (tempType.Substring(tempType.Length - 3, 3) == "BIN")
                {
                    //textBox_AppAddress.Enabled = true;
                    //label1.Enabled = true;
                }
                else
                {
                    MessageBox.Show("The download file format error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, 0);
                    return;
                }
                textBoxFile1Path.Text = ofd.FileName;
            }

        }

        private void buttonBeginTest_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxFile1Path.Text))
            {
                MessageBox.Show("Can't open the download file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_RS232)
            {
                if (!OpenSerialPort())
                {
                    MessageBox.Show("Open RS232 port error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Control.CheckForIllegalCrossThreadCalls = false;
                Thread newth = new System.Threading.Thread(new System.Threading.ThreadStart(DownloadThreadRS232));
                newth.CurrentCulture = Thread.CurrentThread.CurrentCulture;
                newth.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
                newth.Start();
            }
            else if (comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_USB)
            {
                if (string.IsNullOrEmpty(comboBoxPortName.Text))
                {
                    MessageBox.Show("No USB IAP device!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Control.CheckForIllegalCrossThreadCalls = false;
                Thread newth = new System.Threading.Thread(new System.Threading.ThreadStart(DownloadThreadUSB));
                newth.CurrentCulture = Thread.CurrentThread.CurrentCulture;
                newth.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
                newth.Start();
            }
            else if (comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_CAN)
            {
                if (!OpenSerialPort())
                {
                    MessageBox.Show("Open port error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Control.CheckForIllegalCrossThreadCalls = false;
                Thread newth = new System.Threading.Thread(new System.Threading.ThreadStart(DownloadThreadCAN));
                newth.CurrentCulture = Thread.CurrentThread.CurrentCulture;
                newth.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
                newth.Start();
            }
            else if (comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_SPI)
            {
                if (!OpenSerialPort())
                {
                    MessageBox.Show("Open port error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Control.CheckForIllegalCrossThreadCalls = false;
                Thread newth = new System.Threading.Thread(new System.Threading.ThreadStart(DownloadThreadSPI));
                newth.CurrentCulture = Thread.CurrentThread.CurrentCulture;
                newth.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
                newth.Start();
            }
            else if (comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_I2C)
            {
                if (!OpenSerialPort())
                {
                    MessageBox.Show("Open port error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Control.CheckForIllegalCrossThreadCalls = false;
                Thread newth = new System.Threading.Thread(new System.Threading.ThreadStart(DownloadThreadI2C));
                newth.CurrentCulture = Thread.CurrentThread.CurrentCulture;
                newth.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
                newth.Start();
            }
        }

        private void DownloadThreadRS232()
        {
            lock (lockIniDevice)
            {
                List<FILEPARTINFO> FIleInfo = null;

                new System.Threading.Thread(new System.Threading.ThreadStart(ShowForm)).Start();
                frm_Progress.SetOprationInfo("Downloading.......");
                frm_Progress.SetProgress(100, 0);

                uint StartAddr = 0;
                if (!GetFileInfo(out StartAddr, out FIleInfo, textBoxFile1Path.Text))
                {
                    frm_Progress.CloseBar();
                    sp.Close();
                    return;
                }

                byte[] sendbuff = new byte[2];
                byte[] recvbuff = new byte[2];

                //send 5AA5
                sendbuff[0] = 0x5A;
                sendbuff[1] = 0xA5;
                recvbuff[0] = 0x00;
                recvbuff[1] = 0x00;
                if (!PortCommunicationRS232Special(sendbuff, recvbuff))
                {
                    MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    frm_Progress.CloseBar();
                    sp.Close();
                    return;
                }
                else
                {
                    if ((recvbuff[0] != 0xCC) || (recvbuff[1] != 0xDD))
                    {
                        MessageBox.Show("Download error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        frm_Progress.CloseBar();
                        sp.Close();
                        return;
                    }
                }
                frm_Progress.SetProgress(100, 5);
                Thread.Sleep(800);
                //send 5A01
                sendbuff[0] = 0x5A;
                sendbuff[1] = 0x01;
                recvbuff[0] = 0x00;
                recvbuff[1] = 0x00;
                if (!PortCommunicationRS232(sendbuff, recvbuff))
                {
                    MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    frm_Progress.CloseBar();
                    sp.Close();
                    return;
                }
                else
                {
                    if ((recvbuff[0] != 0xCC) || (recvbuff[1] != 0xDD))
                    {
                        MessageBox.Show("Download error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        frm_Progress.CloseBar();
                        sp.Close();
                        return;
                    }
                }
                frm_Progress.SetProgress(100, 10);
                //send file data
                string strSend = string.Empty;
                byte[] DataSendBuff = new byte[2054];
                uint checksum = 0;
                for (int i = 0; i < FIleInfo.Count; i++)
                {
                    DataSendBuff[0] = 0x31;
                    checksum = 0;
                    uint address = StartAddr + FIleInfo[i].PartStartAddress;
                    DataSendBuff[1] = (byte)(address >> 24);
                    checksum += DataSendBuff[1];
                    DataSendBuff[2] = (byte)((address >> 16) & 0x00FF);
                    checksum += DataSendBuff[2];
                    DataSendBuff[3] = (byte)((address >> 8) & 0x0000FF);
                    checksum += DataSendBuff[3];
                    DataSendBuff[4] = (byte)((address >> 0) & 0x000000FF);
                    checksum += DataSendBuff[4];

                    for (int k = 0; k < 2048; k++)
                    {
                        DataSendBuff[5 + k] = FIleInfo[i].PartData[k];
                        checksum += DataSendBuff[5 + k];
                    }

                    DataSendBuff[2053] = (byte)checksum;

                    recvbuff[0] = 0x00;
                    recvbuff[1] = 0x00;
                    if (!PortCommunicationRS232(DataSendBuff, recvbuff))
                    {
                        MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        frm_Progress.CloseBar();
                        sp.Close();
                        return;
                    }
                    else
                    {
                        if ((recvbuff[0] != 0xCC) || (recvbuff[1] != 0xDD))
                        {
                            MessageBox.Show("Download error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                            frm_Progress.CloseBar();
                            sp.Close();
                            return;
                        }
                    }
                    double kkk = 10 + (i + 1) * 1.0 / FIleInfo.Count * 85;
                    frm_Progress.SetProgress(100, (int)kkk);
                }

                //send 5A02
                sendbuff[0] = 0x5A;
                sendbuff[1] = 0x02;
                recvbuff[0] = 0x00;
                recvbuff[1] = 0x00;
                if (!PortCommunicationRS232(sendbuff, recvbuff))
                {
                    MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    frm_Progress.CloseBar();
                    sp.Close();
                    return;
                }
                else
                {
                    if ((recvbuff[0] != 0xCC) || (recvbuff[1] != 0xDD))
                    {
                        MessageBox.Show("Download error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        frm_Progress.CloseBar();
                        sp.Close();
                        return;
                    }
                }

                frm_Progress.SetProgress(100, 100);
                MessageBox.Show("Download succeed!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

                sp.Close();
            }
        }

        private void DownloadThreadCAN()
        {
            lock (lockIniDevice)
            {
                List<FILEPARTINFO> FIleInfo = null;

                new System.Threading.Thread(new System.Threading.ThreadStart(ShowForm)).Start();
                frm_Progress.SetOprationInfo("Downloading.......");
                frm_Progress.SetProgress(100, 0);

                uint StartAddr = 0;
                if (!GetFileInfo(out StartAddr, out FIleInfo, textBoxFile1Path.Text))
                {
                    frm_Progress.CloseBar();
                    sp.Close();
                    return;
                }

                byte[] sendbuff = new byte[7];
                byte[] recvbuff = new byte[8];
                bool bGetInfoSuccess = false;
                for (int i = 0; i < 10; i++)
                {
                    //Get Info
                    sendbuff[0] = 0x00;     //帧ID(4字节)
                    sendbuff[1] = 0x00;
                    sendbuff[2] = 0x00;
                    sendbuff[3] = 0x00;
                    sendbuff[4] = 0x00;     //ID类型
                    sendbuff[5] = 0x00;     //帧类型
                    sendbuff[6] = 0x00;     //帧长
                    if (!PortCommunicationRS232(sendbuff, recvbuff))
                    {
                        MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                               MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        frm_Progress.CloseBar();
                        sp.Close();
                        return;
                    }
                    else
                    {
                        if ((recvbuff[7] != 0x79))
                        {
                            Thread.Sleep(1000);
                            continue;
                        }
                    }
                    bGetInfoSuccess = true;
                    break;
                }

                if (!bGetInfoSuccess)
                {
                    if ((recvbuff[7] != 0x79))
                    {
                        MessageBox.Show("Download error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        frm_Progress.CloseBar();
                        sp.Close();
                        return;
                    }
                }
                frm_Progress.SetProgress(100, 5);
                Thread.Sleep(800);

                //Upgrade Start
                sendbuff[0] = 0x00;
                sendbuff[1] = 0x00;
                sendbuff[2] = 0x00;
                sendbuff[3] = 0x01;
                sendbuff[4] = 0x00;
                sendbuff[5] = 0x00;
                sendbuff[6] = 0x00;
                recvbuff = new byte[8];
                if (!PortCommunicationRS232(sendbuff, recvbuff))
                {
                    MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    frm_Progress.CloseBar();
                    sp.Close();
                    return;
                }
                else
                {
                    if ((recvbuff[7] != 0x79))
                    {
                        MessageBox.Show("Download error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        frm_Progress.CloseBar();
                        sp.Close();
                        return;
                    }
                }
                frm_Progress.SetProgress(100, 10);

                //Write Memory
                byte[] HeadSendBuff = new byte[13];
                for (int i = 0; i < FIleInfo.Count; i++)
                {
                    HeadSendBuff[0] = 0x00;
                    HeadSendBuff[1] = 0x00;
                    HeadSendBuff[2] = 0x00;
                    HeadSendBuff[3] = 0x31;
                    HeadSendBuff[4] = 0x00;
                    HeadSendBuff[5] = 0x00;
                    HeadSendBuff[6] = 0x06;
                    uint address = StartAddr + FIleInfo[i].PartStartAddress;
                    HeadSendBuff[7] = (byte)(address >> 24);
                    HeadSendBuff[8] = (byte)((address >> 16) & 0x00FF);
                    HeadSendBuff[9] = (byte)((address >> 8) & 0x0000FF);
                    HeadSendBuff[10] = (byte)((address >> 0) & 0x000000FF);
                    HeadSendBuff[11] = (byte)((FIleInfo[i].PartSize >> 8) & 0x0000FF);
                    HeadSendBuff[12] = (byte)((FIleInfo[i].PartSize >> 0) & 0x000000FF);
                    if (!PortCommunicationRS232(HeadSendBuff, recvbuff))
                    {
                        MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        frm_Progress.CloseBar();
                        sp.Close();
                        return;
                    }
                    else
                    {
                        if ((recvbuff[7] != 0x79))
                        {
                            MessageBox.Show("Download error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                               MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                            frm_Progress.CloseBar();
                            sp.Close();
                            return;
                        }
                    }

                    byte[] DataSendBuff = new byte[15];
                    DataSendBuff[0] = 0x00;
                    DataSendBuff[1] = 0x00;
                    DataSendBuff[2] = 0x00;
                    DataSendBuff[3] = 0x31;
                    DataSendBuff[4] = 0x00;
                    DataSendBuff[5] = 0x00;
                    DataSendBuff[6] = 0x08;
                    int nIndex = 0;
                    for (int k = 0; k < 2048; k++)
                    {
                        DataSendBuff[7 + nIndex] = FIleInfo[i].PartData[k];
                        nIndex++;
                        if (nIndex >= 8  || k == 2048 - 1)
                        {
                            nIndex = 0;
                            if (!PortCommunicationRS232Write(DataSendBuff))
                            {
                                MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                                   MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                                frm_Progress.CloseBar();
                                sp.Close();
                                return;
                            }
                        }
                    }

                    if (!PortCommunicationRS232Read(recvbuff, 8))
                    {
                        MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        frm_Progress.CloseBar();
                        sp.Close();
                        return;
                    }
                    else
                    {
                        if ((recvbuff[7] != 0x79))
                        {
                            MessageBox.Show("Download error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                               MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                            frm_Progress.CloseBar();
                            sp.Close();
                            return;
                        }
                    }

                    double kkk = 10 + (i + 1) * 1.0 / FIleInfo.Count * 85;
                    frm_Progress.SetProgress(100, (int)kkk);
                }

                //Upgrade Finish
                sendbuff[0] = 0x00;
                sendbuff[1] = 0x00;
                sendbuff[2] = 0x00;
                sendbuff[3] = 0xBD;
                sendbuff[4] = 0x00;
                sendbuff[5] = 0x00;
                sendbuff[6] = 0x00;
                recvbuff = new byte[8];
                if (!PortCommunicationRS232(sendbuff, recvbuff))
                {
                    MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    frm_Progress.CloseBar();
                    sp.Close();
                    return;
                }
                else
                {
                    if ((recvbuff[7] != 0x79))
                    {
                        MessageBox.Show("Download error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        frm_Progress.CloseBar();
                        sp.Close();
                        return;
                    }
                }

                //CRC Check
                if (checkBox_CRC.Checked)
                {
                    frm_Progress.SetOprationInfo("CRC Verify.......");

                    if (!CRCVerifyCAN(StartAddr, FIleInfo))
                    {
                        frm_Progress.CloseBar();
                        sp.Close();
                        return;
                    }
                }

                frm_Progress.SetOprationInfo("Jump to App.......");
                //send jump
                uint AppAddr = StartAddr + FIleInfo[0].PartStartAddress;
                if (!JumpToAppCAN(AppAddr))
                {
                    frm_Progress.CloseBar();
                    sp.Close();
                    return;
                }

                frm_Progress.SetProgress(100, 100);
                MessageBox.Show("Download succeed!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                sp.Close();
            }
        }

        private void DownloadThreadUSB()
        {
            lock (lockIniDevice)
            {
                List<FILEPARTINFO> FIleInfo = null;

                new System.Threading.Thread(new System.Threading.ThreadStart(ShowForm)).Start();
                frm_Progress.SetOprationInfo("Downloading.......");
                frm_Progress.SetProgress(100, 0);

                uint StartAddr = 0;
                if (!GetFileInfo(out StartAddr, out FIleInfo, textBoxFile1Path.Text))
                {
                    frm_Progress.CloseBar();
                    return;
                }

                bool res = false;
                byte[] recvbuff = null;
                uint fileStartAddr = 0;
                for (int k = 0; k < 5; k++)
                {
                    Hid.HID_RETURN rv = oSp.OpenDevice(StringToUInt16(textBox_VID.Text, 16), StringToUInt16(textBox_PID.Text, 16), StringToByte(textBox_InterfaceNumber.Text, 16), comboBoxPortName.Text);
                    if (rv != Hid.HID_RETURN.SUCCESS)
                    {
                        MessageBox.Show("Can't open the usb device", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                               MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        frm_Progress.CloseBar();
                        return;
                    }

                    //Enter IAP Mode
                    if (!EnterIAPModeUSB())
                    {
                        frm_Progress.CloseBar();
                        oSp.CloseDevice();
                        return;
                    }
                    Thread.Sleep(2000);

                    //check download address
                    fileStartAddr = StartAddr + FIleInfo[0].PartStartAddress;
                    res = GetAppAddrFromIAP(fileStartAddr, out recvbuff);
                    if (res)
                    {
                        break;
                    }

                    oSp.CloseDevice();
                }

                if (!IsAppAddrSuccess(res, fileStartAddr, recvbuff))
                {
                    frm_Progress.CloseBar();
                    oSp.CloseDevice();
                    return;
                }

                //Begin download
                if (!BeginDownUSB())
                {
                    frm_Progress.CloseBar();
                    oSp.CloseDevice();
                    return;
                }

                frm_Progress.SetProgress(100, 5);

                //send file data
                if (!DownloadDataToDeviceUSB(StartAddr, FIleInfo))
                {
                    frm_Progress.CloseBar();
                    oSp.CloseDevice();
                    return;
                }

                //send download end
                if (!DownloadEndUSB())
                {
                    frm_Progress.CloseBar();
                    oSp.CloseDevice();
                    return;
                }

                //CRC Check
                if (checkBox_CRC.Checked)
                {
                    frm_Progress.SetOprationInfo("CRC Verify.......");

                    if (!CRCVerifyUSB(StartAddr, FIleInfo))
                    {
                        frm_Progress.CloseBar();
                        oSp.CloseDevice();
                        return;
                    }
                }

                frm_Progress.SetOprationInfo("Jump to App.......");
                //send jump
                if (!JumpToAppUSB())
                {
                    frm_Progress.CloseBar();
                    oSp.CloseDevice();
                    return;
                }

                frm_Progress.SetProgress(100, 100);
                if (checkBox_CRC.Checked)
                {
                    MessageBox.Show("Download and CRC Verify succeed!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information,
                                                               MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                }
                else
                {
                    MessageBox.Show("Download succeed!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information,
                                                               MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                }

                oSp.CloseDevice();
            }
        }

        private bool SPI_CyclicRecvData(byte cmdNo, byte[] sendbuff, byte[] recvbuff, int sendTimes = 10)
        {
            for (int i = 0; i < sendTimes; i++)
            {
                if (!PortCommunicationRS232(sendbuff, recvbuff))
                {
                    return false;
                }
                else
                {
                    if ((recvbuff[0] != 0x55) || (recvbuff[1] != cmdNo) || (recvbuff[2] != 0x79))
                    {
                        Thread.Sleep(1);
                        continue;
                    }
                    else
                        return true;
                }
            }
            return false;
        }

        private void DownloadThreadSPI()
        {
            lock (lockIniDevice)
            {
                List<FILEPARTINFO> FIleInfo = null;

                new System.Threading.Thread(new System.Threading.ThreadStart(ShowForm)).Start();
                frm_Progress.SetOprationInfo("Downloading.......");
                frm_Progress.SetProgress(100, 0);

                uint StartAddr = 0;
                uint SectorSize = 0;
                if (!GetFileInfo(out StartAddr, out FIleInfo, textBoxFile1Path.Text))
                {
                    frm_Progress.CloseBar();
                    sp.Close();
                    return;
                }
                
                byte[] sendbuff = new byte[2];
                byte[] recvbuff = new byte[2];
                bool bGetInfoSuccess = false;
                for (int i = 0; i < 10; i++)
                {
                    //Get Info
                    sendbuff = new byte[2];
                    recvbuff = new byte[2];
                    sendbuff[0] = 0x55;
                    sendbuff[1] = 0x00;
                    if (!PortCommunicationRS232(sendbuff, recvbuff))
                    {
                        MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                               MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        frm_Progress.CloseBar();
                        sp.Close();
                        return;
                    }

                    sendbuff = new byte[15];
                    for (int j = 0; j < 15; j++)
                    {
                        sendbuff[j] = 0xFF;
                    }
                    recvbuff = new byte[15];

                    if (!SPI_CyclicRecvData(0x00,sendbuff, recvbuff))
                    {
                        //MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        //                                       MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        //frm_Progress.CloseBar();
                        //sp.Close();
                        //return;
                        Thread.Sleep(20);
                        continue;
                    }
                    SectorSize = ((uint)recvbuff[11] << 24) + ((uint)recvbuff[12] << 16) + ((uint)recvbuff[13] << 8) + (uint)recvbuff[14];
                    bGetInfoSuccess = true;
                    break;
                }

                if (!bGetInfoSuccess)
                {
                    if ((recvbuff[2] != 0x79))
                    {
                        MessageBox.Show("Download error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        frm_Progress.CloseBar();
                        sp.Close();
                        return;
                    }
                }
                frm_Progress.SetProgress(100, 5);
                Thread.Sleep(800);

                //Upgrade Start
                sendbuff = new byte[2];
                recvbuff = new byte[2];
                sendbuff[0] = 0x55;
                sendbuff[1] = 0x01;
                if (!PortCommunicationRS232(sendbuff, recvbuff))
                {
                    MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    frm_Progress.CloseBar();
                    sp.Close();
                    return;
                }

                sendbuff = new byte[3];
                for (int j = 0; j < 3; j++)
                {
                    sendbuff[j] = 0xFF;
                }
                recvbuff = new byte[3];
                if (!SPI_CyclicRecvData(0x01, sendbuff, recvbuff))
                {
                    MessageBox.Show("Download error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                       MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    frm_Progress.CloseBar();
                    sp.Close();
                    return;
                }          
                frm_Progress.SetProgress(100, 10);
    
                for (int i = 0; i < FIleInfo.Count; i++)
                {
                    //Erase Sector
                    byte[] SendBuff = new byte[8];
                    recvbuff = new byte[8];
                    SendBuff[0] = 0x55;
                    SendBuff[1] = 0x41;
                    uint address = StartAddr + FIleInfo[i].PartStartAddress;
                    SendBuff[2] = (byte)(address >> 24);
                    SendBuff[3] = (byte)((address >> 16) & 0x00FF);
                    SendBuff[4] = (byte)((address >> 8) & 0x0000FF);
                    SendBuff[5] = (byte)((address >> 0) & 0x000000FF);
                    uint sectorNum = 0;
                    if (FIleInfo[i].PartSize % SectorSize == 0)
                        sectorNum = (uint)FIleInfo[i].PartSize / SectorSize;
                    else
                        sectorNum = (uint)FIleInfo[i].PartSize / SectorSize + 1;
                    SendBuff[6] = (byte)((sectorNum >> 8) & 0x0000FF);
                    SendBuff[7] = (byte)((sectorNum >> 0) & 0x000000FF);
                    if (!PortCommunicationRS232(SendBuff, recvbuff))
                    {
                        MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        frm_Progress.CloseBar();
                        sp.Close();
                        return;
                    }

                    sendbuff = new byte[3];
                    for (int j = 0; j < 3; j++)
                    {
                        sendbuff[j] = 0xFF;
                    }
                    recvbuff = new byte[3];
                    if (!SPI_CyclicRecvData(0x41, sendbuff, recvbuff, 1000))
                    {
                        MessageBox.Show("Download error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        frm_Progress.CloseBar();
                        sp.Close();
                        return;
                    }

                    //Write Memory
                    byte[] DataSendBuff = new byte[489];
                    recvbuff = new byte[489];
                    DataSendBuff[0] = 0x55;
                    DataSendBuff[1] = 0x31;
                    address = StartAddr + FIleInfo[i].PartStartAddress;
                    
                    int nIndex = 0;
                    byte Checksum = 0x00;
                    for (int k = 0; k < 2048; k++)
                    {
                        DataSendBuff[8 + nIndex] = FIleInfo[i].PartData[k];
                        Checksum = (byte)(Checksum ^ FIleInfo[i].PartData[k]);
                        nIndex++;
                        if (nIndex >= 480 || k == 2048 - 1)
                        {
                            DataSendBuff[2] = (byte)(address >> 24);
                            DataSendBuff[3] = (byte)((address >> 16) & 0x00FF);
                            DataSendBuff[4] = (byte)((address >> 8) & 0x0000FF);
                            DataSendBuff[5] = (byte)((address >> 0) & 0x000000FF);
                            DataSendBuff[6] = (byte)((nIndex >> 8) & 0x0000FF);
                            DataSendBuff[7] = (byte)((nIndex >> 0) & 0x000000FF);
                            DataSendBuff[8 + nIndex] = Checksum;
                            address += (uint)nIndex;
                            nIndex = 0;
                            Checksum = 0;
                            if (!PortCommunicationRS232(DataSendBuff, recvbuff))
                            {
                                MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                                   MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                                frm_Progress.CloseBar();
                                sp.Close();
                                return;
                            }

                            sendbuff = new byte[3];
                            for (int j = 0; j < 3; j++)
                            {
                                sendbuff[j] = 0xFF;
                            }
                            recvbuff = new byte[3];
                            if (!SPI_CyclicRecvData(0x31, sendbuff, recvbuff))
                            {
                                MessageBox.Show("Download error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                                                  MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                                frm_Progress.CloseBar();
                                sp.Close();
                                return;
                            }
                        }
                    }

                    double kkk = 10 + (i + 1) * 1.0 / FIleInfo.Count * 85;
                    frm_Progress.SetProgress(100, (int)kkk);
                }

                //CRC Check
                if (checkBox_CRC.Checked)
                {
                    frm_Progress.SetOprationInfo("CRC Verify.......");

                    if (!CRCVerifySPI(StartAddr, FIleInfo))
                    {
                        frm_Progress.CloseBar();
                        sp.Close();
                        return;
                    }
                }

                //Upgrade Finish
                sendbuff = new byte[2];
                sendbuff[0] = 0x55;
                sendbuff[1] = 0xBD;
                recvbuff = new byte[2];
                if (!PortCommunicationRS232(sendbuff, recvbuff))
                {
                    MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    frm_Progress.CloseBar();
                    sp.Close();
                    return;
                }

                sendbuff = new byte[3];
                for (int j = 0; j < 3; j++)
                {
                    sendbuff[j] = 0xFF;
                }
                recvbuff = new byte[3];
                if (!SPI_CyclicRecvData(0xBD, sendbuff, recvbuff))
                {
                    MessageBox.Show("Download error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                       MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    frm_Progress.CloseBar();
                    sp.Close();
                    return;
                }
                
                frm_Progress.SetOprationInfo("Jump to App.......");
                //send jump
                uint AppAddr = StartAddr + FIleInfo[0].PartStartAddress;
                if (!JumpToAppSPI(AppAddr))
                {
                    frm_Progress.CloseBar();
                    sp.Close();
                    return;
                }

                frm_Progress.SetProgress(100, 100);
                MessageBox.Show("Download succeed!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                sp.Close();
            }
        }

        private void DownloadThreadI2C()
        {
            lock (lockIniDevice)
            {
                List<FILEPARTINFO> FIleInfo = null;

                new System.Threading.Thread(new System.Threading.ThreadStart(ShowForm)).Start();
                frm_Progress.SetOprationInfo("Downloading.......");
                frm_Progress.SetProgress(100, 0);

                uint StartAddr = 0;
                uint SectorSize = 0;
                if (!GetFileInfo(out StartAddr, out FIleInfo, textBoxFile1Path.Text))
                {
                    frm_Progress.CloseBar();
                    sp.Close();
                    return;
                }

                byte[] sendbuff = new byte[6];
                byte[] recvbuff = new byte[19];
                bool bGetInfoSuccess = false;
                for (int i = 0; i < 10; i++)
                {
                    //Get Info
                    sendbuff = new byte[6];
                    sendbuff[0] = 0x55;
                    sendbuff[1] = (byte)((I2CAddress >> 8) & 0x0000FF);
                    sendbuff[2] = (byte)((I2CAddress >> 0) & 0x000000FF);
                    sendbuff[3] = 0x00;
                    sendbuff[4] = 0x01;
                    sendbuff[5] = 0x00;
                    if (!PortCommunicationRS232Write(sendbuff))
                    {
                        MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                               MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        frm_Progress.CloseBar();
                        sp.Close();
                        return;
                    }

                    sendbuff = new byte[5];
                    sendbuff[0] = 0xAA;
                    sendbuff[1] = (byte)((I2CAddress >> 8) & 0x0000FF);
                    sendbuff[2] = (byte)((I2CAddress >> 0) & 0x000000FF);
                    sendbuff[3] = 0x00;
                    sendbuff[4] = 0x0E;
                    if (!PortCommunicationRS232(sendbuff, recvbuff))
                    {
                        MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                               MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        frm_Progress.CloseBar();
                        sp.Close();
                        return;
                    }
                    else
                    {
                        if ((recvbuff[6] == 0x1F))
                        {
                            Thread.Sleep(200);
                            continue;
                        }
                        else if ((recvbuff[6] != 0x79))
                        {
                            MessageBox.Show("Download error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                               MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                            frm_Progress.CloseBar();
                            sp.Close();
                            return;
                        }
                    }

                    SectorSize = ((uint)recvbuff[15] << 24) + ((uint)recvbuff[16] << 16) + ((uint)recvbuff[17] << 8) + (uint)recvbuff[18];
                    bGetInfoSuccess = true;
                    break;
                }

                if (!bGetInfoSuccess)
                {
                    if ((recvbuff[6] != 0x79))
                    {
                        MessageBox.Show("Download error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        frm_Progress.CloseBar();
                        sp.Close();
                        return;
                    }
                }
                frm_Progress.SetProgress(100, 5);
                Thread.Sleep(800);

                //Upgrade Start
                sendbuff = new byte[6];
                sendbuff[0] = 0x55;
                sendbuff[1] = (byte)((I2CAddress >> 8) & 0x0000FF);
                sendbuff[2] = (byte)((I2CAddress >> 0) & 0x000000FF);
                sendbuff[3] = 0x00;
                sendbuff[4] = 0x01;
                sendbuff[5] = 0x01;
                if (!PortCommunicationRS232Write(sendbuff))
                {
                    MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    frm_Progress.CloseBar();
                    sp.Close();
                    return;
                }

                sendbuff = new byte[5];
                recvbuff = new byte[7];
                sendbuff[0] = 0xAA;
                sendbuff[1] = (byte)((I2CAddress >> 8) & 0x0000FF);
                sendbuff[2] = (byte)((I2CAddress >> 0) & 0x000000FF);
                sendbuff[3] = 0x00;
                sendbuff[4] = 0x02;
                if (!PortCommunicationRS232(sendbuff, recvbuff))
                {
                    MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    frm_Progress.CloseBar();
                    sp.Close();
                    return;
                }
                else
                {
                    if ((recvbuff[6] != 0x79))
                    {
                        MessageBox.Show("Download error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        frm_Progress.CloseBar();
                        sp.Close();
                        return;
                    }
                }
                frm_Progress.SetProgress(100, 10);

                byte[] HeadSendBuff = new byte[12];
                for (int i = 0; i < FIleInfo.Count; i++)
                {
                    //Erase Sector
                    sendbuff = new byte[12];
                    sendbuff[0] = 0x55;
                    sendbuff[1] = (byte)((I2CAddress >> 8) & 0x0000FF);
                    sendbuff[2] = (byte)((I2CAddress >> 0) & 0x000000FF);
                    sendbuff[3] = 0x00;
                    sendbuff[4] = 0x07;
                    sendbuff[5] = 0x41;
                    uint address = StartAddr + FIleInfo[i].PartStartAddress;
                    sendbuff[6] = (byte)(address >> 24);
                    sendbuff[7] = (byte)((address >> 16) & 0x00FF);
                    sendbuff[8] = (byte)((address >> 8) & 0x0000FF);
                    sendbuff[9] = (byte)((address >> 0) & 0x000000FF);
                    uint sectorNum = 0;
                    if (FIleInfo[i].PartSize % 2048 == 0)
                        sectorNum = (uint)FIleInfo[i].PartSize / 2048;
                    else
                        sectorNum = (uint)FIleInfo[i].PartSize / 2048 + 1;
                    sendbuff[10] = (byte)((sectorNum >> 8) & 0x0000FF);
                    sendbuff[11] = (byte)((sectorNum >> 0) & 0x000000FF);
                    if (!PortCommunicationRS232Write(sendbuff))
                    {
                        MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        frm_Progress.CloseBar();
                        sp.Close();
                        return;
                    }                

                    sendbuff = new byte[5];
                    recvbuff = new byte[7];
                    sendbuff[0] = 0xAA;
                    sendbuff[1] = (byte)((I2CAddress >> 8) & 0x0000FF);
                    sendbuff[2] = (byte)((I2CAddress >> 0) & 0x000000FF);
                    sendbuff[3] = 0x00;
                    sendbuff[4] = 0x02;
                    if (!PortCommunicationRS232(sendbuff, recvbuff))
                    {
                        MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                               MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        frm_Progress.CloseBar();
                        sp.Close();
                        return;
                    }
                    else
                    {
                        if ((recvbuff[6] != 0x79))
                        {
                            MessageBox.Show("Download error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                               MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                            frm_Progress.CloseBar();
                            sp.Close();
                            return;
                        }
                    }

                    //Write Memory
                    byte[] DataSendBuff = new byte[493];
                    DataSendBuff[0] = 0x55;
                    DataSendBuff[1] = (byte)((I2CAddress >> 8) & 0x0000FF);
                    DataSendBuff[2] = (byte)((I2CAddress >> 0) & 0x000000FF);
                    DataSendBuff[5] = 0x31;
                    address = StartAddr + FIleInfo[i].PartStartAddress;
                    DataSendBuff[10] = (byte)((480 >> 8) & 0x0000FF);
                    DataSendBuff[11] = (byte)((480 >> 0) & 0x000000FF);
                    int nIndex = 0;
                    byte Checksum = 0;
                    for (int k = 0; k < 2048; k++)
                    {
                        DataSendBuff[12 + nIndex] = FIleInfo[i].PartData[k];
                        Checksum = (byte)(Checksum ^ FIleInfo[i].PartData[k]);
                        nIndex++;
                        if (nIndex >= 480 || k == 2048 - 1)
                        {
                            DataSendBuff[3] = (byte)(((nIndex + 1 + 7) >> 8) & 0x0000FF);
                            DataSendBuff[4] = (byte)(((nIndex + 1 + 7) >> 0) & 0x0000FF);
                            DataSendBuff[6] = (byte)(address >> 24);
                            DataSendBuff[7] = (byte)((address >> 16) & 0x00FF);
                            DataSendBuff[8] = (byte)((address >> 8) & 0x0000FF);
                            DataSendBuff[9] = (byte)((address >> 0) & 0x000000FF);
                            DataSendBuff[10] = (byte)((nIndex >> 8) & 0x0000FF);
                            DataSendBuff[11] = (byte)((nIndex >> 0) & 0x000000FF);
                            DataSendBuff[12 + nIndex] = Checksum;
                            address += (uint)nIndex;
                            nIndex = 0;
                            Checksum = 0;
                            if (!PortCommunicationRS232Write(DataSendBuff))
                            {
                                MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                                   MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                                frm_Progress.CloseBar();
                                sp.Close();
                                return;
                            }
                            
                            sendbuff = new byte[5];
                            recvbuff = new byte[7];
                            sendbuff[0] = 0xAA;
                            sendbuff[1] = (byte)((I2CAddress >> 8) & 0x0000FF);
                            sendbuff[2] = (byte)((I2CAddress >> 0) & 0x000000FF);
                            sendbuff[3] = 0x00;
                            sendbuff[4] = 0x02;
                            if (!PortCommunicationRS232(sendbuff, recvbuff))
                            {
                                MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                                       MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                                frm_Progress.CloseBar();
                                sp.Close();
                                return;
                            }
                            else
                            {
                                if ((recvbuff[6] != 0x79))
                                {
                                    MessageBox.Show("Download error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                                       MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                                    frm_Progress.CloseBar();
                                    sp.Close();
                                    return;
                                }
                            }
                        }
                    }
                   
                    double kkk = 10 + (i + 1) * 1.0 / FIleInfo.Count * 85;
                    frm_Progress.SetProgress(100, (int)kkk);
                }

                //CRC Check
                if (checkBox_CRC.Checked)
                {
                    frm_Progress.SetOprationInfo("CRC Verify.......");

                    if (!CRCVerifyI2C(StartAddr, FIleInfo))
                    {
                        frm_Progress.CloseBar();
                        sp.Close();
                        return;
                    }
                }

                //Upgrade Finish
                sendbuff = new byte[6];
                sendbuff[0] = 0x55;
                sendbuff[1] = (byte)((I2CAddress >> 8) & 0x0000FF);
                sendbuff[2] = (byte)((I2CAddress >> 0) & 0x000000FF);
                sendbuff[3] = 0x00;
                sendbuff[4] = 0x01;
                sendbuff[5] = 0xBD;
                if (!PortCommunicationRS232Write(sendbuff))
                {
                    MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    frm_Progress.CloseBar();
                    sp.Close();
                    return;
                }

                sendbuff = new byte[5];
                recvbuff = new byte[7];
                sendbuff[0] = 0xAA;
                sendbuff[1] = (byte)((I2CAddress >> 8) & 0x0000FF);
                sendbuff[2] = (byte)((I2CAddress >> 0) & 0x000000FF);
                sendbuff[3] = 0x00;
                sendbuff[4] = 0x02;
                if (!PortCommunicationRS232(sendbuff, recvbuff))
                {
                    MessageBox.Show("Serial port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    frm_Progress.CloseBar();
                    sp.Close();
                    return;
                }
                else
                {
                    if ((recvbuff[6] != 0x79))
                    {
                        MessageBox.Show("Download error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        frm_Progress.CloseBar();
                        sp.Close();
                        return;
                    }
                }

                frm_Progress.SetOprationInfo("Jump to App.......");
                //send jump
                uint AppAddr = StartAddr + FIleInfo[0].PartStartAddress;
                if (!JumpToAppI2C(AppAddr))
                {
                    frm_Progress.CloseBar();
                    sp.Close();
                    return;
                }

                frm_Progress.SetProgress(100, 100);
                MessageBox.Show("Download succeed!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                sp.Close();
            }
        }

        private bool GetFileInfo(out uint StartAddr, out List<FILEPARTINFO> FIleInfo, string filePath)
        {
            StartAddr = 0;
            FIleInfo = null;
            if (filePath == string.Empty)
                return false;
            string temp3 = filePath.Substring(filePath.LastIndexOf(@"\"));
            string tempType = temp3.ToUpper();

            if (tempType.Substring(tempType.Length - 3, 3) == "HEX")
            {
                FIleInfo = AddValueHex(filePath);
                StartAddr = 0;
            }
            else if (tempType.Substring(tempType.Length - 3, 3) == "BIN")
            {
                FIleInfo = AddValueBin(filePath);
                string strAddr = textBox_AppAddress.Text;
                StartAddr = StringToUInt32(strAddr, 16);
            }
            else
            {
                MessageBox.Show("The download file format error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                       MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                frm_Progress.CloseBar();
                return false;
            }

            if (FIleInfo == null)
            {
                MessageBox.Show("The download file format error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                       MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                frm_Progress.CloseBar();
                return false;
            }
            if (FIleInfo.Count == 0)
            {
                MessageBox.Show("The download file format error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                       MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                frm_Progress.CloseBar();
                return false;
            }
            return true;
        }

        private bool GetAppAddrFromIAP(uint fileAddr, out byte[] recvbuff)
        {
            byte[] sendbuff = new byte[2];
            recvbuff = new byte[8];
            sendbuff[0] = USBCOMMOND_MSB;
            sendbuff[1] = USBCOMMOND_GETAPPADDR;

            bool res = false;
            res = PortCommunicationUSB(sendbuff, recvbuff);
            return res;
        }

        private bool IsAppAddrSuccess(bool res, uint fileAddr, byte[] recvbuff)
        {
            if (!res)
            {
                MessageBox.Show("USB port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                       MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                return false;
            }
            else
            {
                if ((recvbuff[0] != USBCOMMOND_MSB) || (recvbuff[1] != USBCOMMOND_GETAPPADDR) || (recvbuff[2] != USBCOMMOND_ACK_MSB) || (recvbuff[3] != USBCOMMOND_ACK_LSB))
                {
                    MessageBox.Show("USB device receive NACK", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                       MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    return false;
                }
                else
                {
                    uint appAddr = ((uint)recvbuff[4] << 24) + ((uint)recvbuff[5] << 16) + ((uint)recvbuff[6] << 8) + (uint)recvbuff[7];
                    if (fileAddr != appAddr)
                    {
                        MessageBox.Show("File download address is different from the APP address of IAP", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                      MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        return false;
                    }
                }
            }
            return true;
        }

        private bool EnterIAPModeUSB()
        {
            byte[] sendbuff = new byte[2];
            byte[] recvbuff = new byte[4];
            sendbuff[0] = USBCOMMOND_MSB;
            sendbuff[1] = USBCOMMOND_IAPMODE;
            if (!PortCommunicationUSB(sendbuff, recvbuff))
            {
                MessageBox.Show("USB port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                       MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                return false;
            }
            else
            {
                if ((recvbuff[0] != USBCOMMOND_MSB) || (recvbuff[1] != USBCOMMOND_IAPMODE) || (recvbuff[2] != USBCOMMOND_ACK_MSB) || (recvbuff[3] != USBCOMMOND_ACK_LSB))
                {
                    MessageBox.Show("USB device receive NACK", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                       MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    return false;
                }
            }
            return true;
        }

        private bool BeginDownUSB()
        {
            byte[] sendbuff = new byte[2];
            byte[] recvbuff = new byte[4];
            sendbuff[0] = USBCOMMOND_MSB;
            sendbuff[1] = USBCOMMOND_BEGINDOWN;
            if (!PortCommunicationUSB(sendbuff, recvbuff))
            {
                MessageBox.Show("USB port communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                       MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                return false;
            }
            else
            {
                if ((recvbuff[0] != USBCOMMOND_MSB) || (recvbuff[1] != USBCOMMOND_BEGINDOWN) || (recvbuff[2] != USBCOMMOND_ACK_MSB) || (recvbuff[3] != USBCOMMOND_ACK_LSB))
                {
                    MessageBox.Show("USB device receive NACK", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                       MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    return false;
                }
            }
            return true;
        }

        private bool CRCVerifyUSB(uint StartAddr, List<FILEPARTINFO> FIleInfo)
        {
            for (int i = 0; i < FIleInfo.Count; i++)
            {
                uint address = StartAddr + FIleInfo[i].PartStartAddress;
                uint pageNum = 2;

                byte[] sendCRCbuff = new byte[8];
                byte[] recvCRCbuff = new byte[8];
                sendCRCbuff[0] = USBCOMMOND_MSB;
                sendCRCbuff[1] = USBCOMMOND_CRC;
                sendCRCbuff[2] = (byte)(address >> 24);
                sendCRCbuff[3] = (byte)((address >> 16) & 0x00FF);
                sendCRCbuff[4] = (byte)((address >> 8) & 0x0000FF);
                sendCRCbuff[5] = (byte)((address >> 0) & 0x000000FF);
                sendCRCbuff[6] = (byte)((pageNum >> 8) & 0x0000FF);
                sendCRCbuff[7] = (byte)((pageNum >> 0) & 0x000000FF);

                recvCRCbuff[0] = 0x00;
                recvCRCbuff[1] = 0x00;
                if (!PortCommunicationUSB(sendCRCbuff, recvCRCbuff))
                {
                    MessageBox.Show("USB device communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    return false;
                }
                else
                {
                    if ((recvCRCbuff[0] != USBCOMMOND_MSB) || (recvCRCbuff[1] != USBCOMMOND_CRC) || (recvCRCbuff[2] != USBCOMMOND_ACK_MSB) || (recvCRCbuff[3] != USBCOMMOND_ACK_LSB))
                    {
                        MessageBox.Show("USB device receive NACK", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        return false;
                    }
                    else
                    {
                        //CRC Value right?
                        uint fileCRCCode = GetLoadFileCRCCode(FIleInfo[i].PartData, FIleInfo[i].PartData.Length);

                        uint fwCRCCode = ((uint)recvCRCbuff[4] << 24) + ((uint)recvCRCbuff[5] << 16) + ((uint)recvCRCbuff[6] << 8) + (uint)recvCRCbuff[7];
                        if (fwCRCCode == fileCRCCode)
                        {
                            continue;
                        }
                        else
                        {
                            MessageBox.Show("USB device CRC verify failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                          MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private bool DownloadEndUSB()
        {
            byte[] sendbuff = new byte[2];
            byte[] recvbuff = new byte[4];

            sendbuff[0] = USBCOMMOND_MSB;
            sendbuff[1] = USBCOMMOND_SENDEND;
            recvbuff[0] = 0x00;
            recvbuff[1] = 0x00;
            if (!PortCommunicationUSB(sendbuff, recvbuff))
            {
                MessageBox.Show("USB device communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                       MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                return false;
            }
            else
            {
                if ((recvbuff[0] != USBCOMMOND_MSB) || (recvbuff[1] != USBCOMMOND_SENDEND) || (recvbuff[2] != USBCOMMOND_ACK_MSB) || (recvbuff[3] != USBCOMMOND_ACK_LSB))
                {
                    MessageBox.Show("USB device receive NACK", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                       MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    return false;
                }
            }
            return true;
        }

        private bool JumpToAppUSB()
        {
            byte[] sendbuff = new byte[2];
            byte[] recvbuff = new byte[4];
            sendbuff[0] = USBCOMMOND_MSB;
            sendbuff[1] = USBCOMMOND_JUMPAPP;
            recvbuff[0] = 0x00;
            recvbuff[1] = 0x00;
            if (!PortCommunicationUSB(sendbuff, recvbuff))
            {
                MessageBox.Show("USB device communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                       MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                return false;
            }
            else
            {
                if ((recvbuff[0] != USBCOMMOND_MSB) || (recvbuff[1] != USBCOMMOND_JUMPAPP) || (recvbuff[2] != USBCOMMOND_ACK_MSB) || (recvbuff[3] != USBCOMMOND_ACK_LSB))
                {
                    MessageBox.Show("USB device receive NACK", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                       MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    return false;
                }
            }
            return true;
        }

        private bool CRCVerifyCAN(uint StartAddr, List<FILEPARTINFO> FIleInfo)
        {
            for (int i = 0; i < FIleInfo.Count; i++)
            {
                uint address = StartAddr + FIleInfo[i].PartStartAddress;
                uint length = (uint)FIleInfo[i].PartSize;

                byte[] sendCRCbuff = new byte[15];
                byte[] recvCRCbuff = new byte[19];
                sendCRCbuff[0] = 0x00;
                sendCRCbuff[1] = 0x00;
                sendCRCbuff[2] = 0x00;
                sendCRCbuff[3] = 0xAC;
                sendCRCbuff[4] = 0x00;
                sendCRCbuff[5] = 0x00;
                sendCRCbuff[6] = 0x08;      //长度
                sendCRCbuff[7] = (byte)(address >> 24);
                sendCRCbuff[8] = (byte)((address >> 16) & 0x00FF);
                sendCRCbuff[9] = (byte)((address >> 8) & 0x0000FF);
                sendCRCbuff[10] = (byte)((address >> 0) & 0x000000FF);
                sendCRCbuff[11] = (byte)(length >> 24);
                sendCRCbuff[12] = (byte)((length >> 16) & 0x00FF);
                sendCRCbuff[13] = (byte)((length >> 8) & 0x0000FF);
                sendCRCbuff[14] = (byte)((length >> 0) & 0x000000FF);

                if (!PortCommunicationRS232(sendCRCbuff, recvCRCbuff))
                {
                    MessageBox.Show("USB device communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    return false;
                }
                else
                {
                    if (recvCRCbuff[7] != 0x79)
                    {
                        MessageBox.Show("CAN device receive NACK", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        return false;
                    }
                    else
                    {
                        //CRC Value right?
                        uint fileCRCCode = GetLoadFileCRCCode(FIleInfo[i].PartData, FIleInfo[i].PartData.Length);

                        uint fwCRCCode = ((uint)recvCRCbuff[15] << 24) + ((uint)recvCRCbuff[16] << 16) + ((uint)recvCRCbuff[17] << 8) + (uint)recvCRCbuff[18];
                        if (fwCRCCode == fileCRCCode)
                        {
                            continue;
                        }
                        else
                        {
                            MessageBox.Show("CAN device CRC verify failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                          MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private bool CRCVerifySPI(uint StartAddr, List<FILEPARTINFO> FIleInfo)
        {
            for (int i = 0; i < FIleInfo.Count; i++)
            {
                uint address = StartAddr + FIleInfo[i].PartStartAddress;
                uint length = (uint)FIleInfo[i].PartSize;

                byte[] sendCRCbuff = new byte[10];
                byte[] recvCRCbuff = new byte[10];
                sendCRCbuff[0] = 0x55;
                sendCRCbuff[1] = 0xAC;
                sendCRCbuff[2] = (byte)(address >> 24);
                sendCRCbuff[3] = (byte)((address >> 16) & 0x00FF);
                sendCRCbuff[4] = (byte)((address >> 8) & 0x0000FF);
                sendCRCbuff[5] = (byte)((address >> 0) & 0x000000FF);
                sendCRCbuff[6] = (byte)(length >> 24);
                sendCRCbuff[7] = (byte)((length >> 16) & 0x00FF);
                sendCRCbuff[8] = (byte)((length >> 8) & 0x0000FF);
                sendCRCbuff[9] = (byte)((length >> 0) & 0x000000FF);
                if (!PortCommunicationRS232(sendCRCbuff, recvCRCbuff))
                {
                    MessageBox.Show("SPI device communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    return false;
                }

                sendCRCbuff = new byte[7];
                for (int j = 0; j < 7; j++)
                {
                    sendCRCbuff[j] = 0xFF;
                }
                recvCRCbuff = new byte[7];
                if (!SPI_CyclicRecvData(0xAC, sendCRCbuff, recvCRCbuff))
                {
                    MessageBox.Show("SPI device communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    return false;
                }

                //CRC Value right?
                uint fileCRCCode = GetLoadFileCRCCode(FIleInfo[i].PartData, FIleInfo[i].PartData.Length);

                uint fwCRCCode = ((uint)recvCRCbuff[3] << 24) + ((uint)recvCRCbuff[4] << 16) + ((uint)recvCRCbuff[5] << 8) + (uint)recvCRCbuff[6];
                if (fwCRCCode == fileCRCCode)
                {
                    continue;
                }
                else
                {
                    MessageBox.Show("SPI device CRC verify failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                  MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    return false;
                }
            }
            return true;
        }

        private bool CRCVerifyI2C(uint StartAddr, List<FILEPARTINFO> FIleInfo)
        {
            for (int i = 0; i < FIleInfo.Count; i++)
            {
                uint address = StartAddr + FIleInfo[i].PartStartAddress;
                uint length = (uint)FIleInfo[i].PartSize;

                byte[] sendCRCbuff = new byte[14];
                byte[] recvCRCbuff = new byte[11];
                sendCRCbuff[0] = 0x55;
                sendCRCbuff[1] = (byte)((I2CAddress >> 8) & 0x0000FF);
                sendCRCbuff[2] = (byte)((I2CAddress >> 0) & 0x000000FF);
                sendCRCbuff[3] = 0x00;
                sendCRCbuff[4] = 0x09;
                sendCRCbuff[5] = 0xAC;
                sendCRCbuff[6] = (byte)(address >> 24);
                sendCRCbuff[7] = (byte)((address >> 16) & 0x00FF);
                sendCRCbuff[8] = (byte)((address >> 8) & 0x0000FF);
                sendCRCbuff[9] = (byte)((address >> 0) & 0x000000FF);
                sendCRCbuff[10] = (byte)(length >> 24);
                sendCRCbuff[11] = (byte)((length >> 16) & 0x00FF);
                sendCRCbuff[12] = (byte)((length >> 8) & 0x0000FF);
                sendCRCbuff[13] = (byte)((length >> 0) & 0x000000FF);
                if (!PortCommunicationRS232Write(sendCRCbuff))
                {
                    MessageBox.Show("I2C device communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    return false;
                }

                sendCRCbuff = new byte[5];
                sendCRCbuff[0] = 0xAA;
                sendCRCbuff[1] = (byte)((I2CAddress >> 8) & 0x0000FF);
                sendCRCbuff[2] = (byte)((I2CAddress >> 0) & 0x000000FF);
                sendCRCbuff[3] = 0x00;
                sendCRCbuff[4] = 0x06;
                if (!PortCommunicationRS232(sendCRCbuff, recvCRCbuff))
                {
                    MessageBox.Show("I2C device communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    return false;
                }
                else
                {
                    if (recvCRCbuff[6] != 0x79)
                    {
                        MessageBox.Show("I2C device receive NACK", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        return false;
                    }
                    else
                    {
                        //CRC Value right?
                        uint fileCRCCode = GetLoadFileCRCCode(FIleInfo[i].PartData, FIleInfo[i].PartData.Length);

                        uint fwCRCCode = ((uint)recvCRCbuff[7] << 24) + ((uint)recvCRCbuff[8] << 16) + ((uint)recvCRCbuff[9] << 8) + (uint)recvCRCbuff[10];
                        if (fwCRCCode == fileCRCCode)
                        {
                            continue;
                        }
                        else
                        {
                            MessageBox.Show("I2C device CRC verify failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                          MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private bool JumpToAppCAN(uint address)
        {
            byte[] sendbuff = new byte[11];
            byte[] recvbuff = new byte[8];
            sendbuff[0] = 0x00;
            sendbuff[1] = 0x00;
            sendbuff[2] = 0x00;
            sendbuff[3] = 0x21;
            sendbuff[4] = 0x00;
            sendbuff[5] = 0x00;
            sendbuff[6] = 0x04;
            sendbuff[7] = (byte)(address >> 24);
            sendbuff[8] = (byte)((address >> 16) & 0x00FF);
            sendbuff[9] = (byte)((address >> 8) & 0x0000FF);
            sendbuff[10] = (byte)((address >> 0) & 0x000000FF);
            if (!PortCommunicationRS232(sendbuff, recvbuff))
            {
                MessageBox.Show("CAN device communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                       MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                return false;
            }
            else
            {
                if (recvbuff[7] != 0x79)
                {
                    MessageBox.Show("CAN device receive NACK", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                       MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    return false;
                }
            }
            return true;
        }
        
        private bool JumpToAppSPI(uint address)
        {
            byte[] sendbuff = new byte[6];
            byte[] recvbuff = new byte[6];
            sendbuff[0] = 0x55;
            sendbuff[1] = 0x21;
            sendbuff[2] = (byte)(address >> 24);
            sendbuff[3] = (byte)((address >> 16) & 0x00FF);
            sendbuff[4] = (byte)((address >> 8) & 0x0000FF);
            sendbuff[5] = (byte)((address >> 0) & 0x000000FF);
            if (!PortCommunicationRS232(sendbuff, recvbuff))
            {
                MessageBox.Show("SPI device communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                       MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                return false;
            }

            sendbuff = new byte[3];
            for (int i = 0; i < 3; i++)
            {
                sendbuff[i] = 0xFF;
            }
            recvbuff = new byte[3];
            if (!SPI_CyclicRecvData(0x21, sendbuff, recvbuff))
            {
                MessageBox.Show("SPI device receive NACK", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                   MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                return false;
            }

            return true;
        }

        private bool JumpToAppI2C(uint address)
        {
            byte[] sendbuff = new byte[10];
            byte[] recvbuff = new byte[7];
            sendbuff[0] = 0x55;
            sendbuff[1] = (byte)((I2CAddress >> 8) & 0x0000FF);
            sendbuff[2] = (byte)((I2CAddress >> 0) & 0x000000FF);
            sendbuff[3] = 0x00;
            sendbuff[4] = 0x05;
            sendbuff[5] = 0x21;
            sendbuff[6] = (byte)(address >> 24);
            sendbuff[7] = (byte)((address >> 16) & 0x00FF);
            sendbuff[8] = (byte)((address >> 8) & 0x0000FF);
            sendbuff[9] = (byte)((address >> 0) & 0x000000FF);
            if (!PortCommunicationRS232Write(sendbuff))
            {
                MessageBox.Show("I2C device communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                       MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                return false;
            }
            sendbuff = new byte[5];
            sendbuff[0] = 0xAA;
            sendbuff[1] = (byte)((I2CAddress >> 8) & 0x0000FF);
            sendbuff[2] = (byte)((I2CAddress >> 0) & 0x000000FF);
            sendbuff[3] = 0x00;
            sendbuff[4] = 0x02;
            if (!PortCommunicationRS232(sendbuff, recvbuff))
            {
                MessageBox.Show("I2C device communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                       MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                return false;
            }
            else
            {
                if (recvbuff[6] != 0x79)
                {
                    MessageBox.Show("I2C device receive NACK", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                       MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    return false;
                }
            }
            return true;
        }

        private bool DownloadDataToDeviceUSB(uint StartAddr, List<FILEPARTINFO> FIleInfo)
        {
            byte[] sendbuff = new byte[2];
            byte[] recvbuff = new byte[4];
            string strSend = string.Empty;
            byte[] DataSendBuff = new byte[oSp.OutputReportLength];
            byte[] AddrBuff = new byte[6];

            for (int i = 0; i < FIleInfo.Count; i++)
            {
                for (uint j = 0; j < FIleInfo[i].PartData.Length / USB_PART_LENGTH; j++)
                {
                    //send addr
                    uint address = StartAddr + FIleInfo[i].PartStartAddress + j * USB_PART_LENGTH;
                    AddrBuff[0] = USBCOMMOND_MSB;
                    AddrBuff[1] = USBCOMMOND_ADDRESS;
                    AddrBuff[2] = (byte)(address >> 24);
                    AddrBuff[3] = (byte)((address >> 16) & 0x00FF);
                    AddrBuff[4] = (byte)((address >> 8) & 0x0000FF);
                    AddrBuff[5] = (byte)((address >> 0) & 0x000000FF);
                    if (!PortCommunicationUSB(AddrBuff, recvbuff))
                    {
                        MessageBox.Show("USB device communication error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                               MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        return false;
                    }
                    else
                    {
                        if ((recvbuff[0] != USBCOMMOND_MSB) || (recvbuff[1] != USBCOMMOND_ADDRESS) || (recvbuff[2] != USBCOMMOND_ACK_MSB) || (recvbuff[3] != USBCOMMOND_ACK_LSB))
                        {
                            MessageBox.Show("USB device receive NACK", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                               MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                            return false;
                        }
                    }

                    int sendDataLen = oSp.OutputReportLength - 5; //1byte  00, 2 bytes command, 2 bytes length
                    int sendTimes = 0;
                    if (USB_PART_LENGTH % sendDataLen == 0)
                        sendTimes = USB_PART_LENGTH / sendDataLen;
                    else
                        sendTimes = USB_PART_LENGTH / sendDataLen + 1;

                    DataSendBuff[0] = USBCOMMOND_MSB;
                    DataSendBuff[1] = USBCOMMOND_SENDDATA;

                    int realLen = 0;
                    for (int k = 0; k < sendTimes; k++)
                    {
                        if ((k + 1) * sendDataLen > USB_PART_LENGTH)
                        {
                            realLen = USB_PART_LENGTH - k * sendDataLen;
                        }
                        else
                        {
                            realLen = sendDataLen;
                        }
                        DataSendBuff[2] = (byte)((realLen >> 8) & 0x0000FF);
                        DataSendBuff[3] = (byte)((realLen >> 0) & 0x000000FF);

                        for (int m = 0; m < realLen; m++)
                            DataSendBuff[m + 4] = FIleInfo[i].PartData[j * USB_PART_LENGTH + k * sendDataLen + m];

                        if (!PortSendUSB(DataSendBuff))
                        {
                            MessageBox.Show("USB device send data error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                                   MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                            return false;
                        }
                    }

                    if (!PortReadUSB(recvbuff))
                    {
                        MessageBox.Show("USB device receive data error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                               MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        return false;
                    }
                    else
                    {
                        if ((recvbuff[0] != USBCOMMOND_MSB) || (recvbuff[1] != USBCOMMOND_SENDDATA) || (recvbuff[2] != USBCOMMOND_ACK_MSB) || (recvbuff[3] != USBCOMMOND_ACK_LSB))
                        {
                            MessageBox.Show("USB device receive NACK", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                               MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                            return false;
                        }
                    }
                }

                double kkk = 5 + (i + 1) * 1.0 / FIleInfo.Count * 90;
                frm_Progress.SetProgress(100, (int)kkk);
            }

            return true;
        }

        private uint GetLoadFileCRCCode(byte[] pFileAllData, int reallength)
        {
            int kk = reallength;

            int i;
            UInt32 crc = 0xFFFFFFFF;

            for (i = 0; i < kk; i++)
                crc = (crc << 8) ^ crc32table[((crc >> 24) ^ pFileAllData[i]) & 0xFF];

            return crc;
        }

        private bool PortCommunicationUSB(byte[] sendBuff, byte[] recvBuff)
        {
            if ((sendBuff == null) || (recvBuff == null))
                return false;

            try
            {
                Hid.HID_RETURN hdrtn = oSp.Write(new report(0, sendBuff));

                if (hdrtn != Hid.HID_RETURN.SUCCESS)
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

            int timeout = 0;
            while (true)
            {
                try
                {
                    Hid.HID_RETURN re = oSp.Read(recvBuff, recvBuff.Length);
                    if (re == Hid.HID_RETURN.SUCCESS)
                        return true;
                    else
                    {
                        Thread.Sleep(10);
                        timeout++;
                        if (timeout >= 500)
                            return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        private bool PortSendUSB(byte[] sendBuff)
        {
            if (sendBuff == null)
                return false;

            try
            {
                Hid.HID_RETURN hdrtn = oSp.Write(new report(0, sendBuff));

                if (hdrtn != Hid.HID_RETURN.SUCCESS)
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        private bool PortReadUSB(byte[] recvBuff)
        {
            if (recvBuff == null)
                return false;

            int timeout = 0;
            while (true)
            {
                try
                {
                    Hid.HID_RETURN re = oSp.Read(recvBuff, recvBuff.Length);
                    if (re == Hid.HID_RETURN.SUCCESS)
                        return true;
                    else
                    {
                        Thread.Sleep(10);
                        timeout++;
                        if (timeout >= 500)
                            return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }


        private long GetCurrentTimeSeconds()
        {
            long currentTicks = System.DateTime.Now.Ticks;
            System.DateTime dtFrom = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            long currentMillis = (currentTicks - dtFrom.Ticks) / 1000 / 1000 / 10;
            return currentMillis;
        }

        private bool PortCommunicationRS232Read(byte[] recvBuff, int length)
        {
            if (!sp.IsOpen)
                return false;

            if (recvBuff == null)
                return false;

            int count = 0;
            int timeout = 0;
            while (true)
            {
                try
                {
                    recvBuff[count] = (byte)sp.ReadByte();
                    count++;
                    if (count >= length)
                        break;
                }
                catch
                {
                    timeout++;
                    if (timeout >= 5)
                        return false;
                    else
                        continue;
                }
            }

            return true;
        }

        private bool PortCommunicationRS232Write(byte[] sendBuff)
        {
            if (!sp.IsOpen)
                return false;

            if (sendBuff == null)
                return false;

            try
            {
                sp.DiscardInBuffer();
            }
            catch
            {
                ;
            }


            try
            {
                sp.Write(sendBuff, 0, sendBuff.Length);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private bool PortCommunicationRS232(byte[] sendBuff, byte[] recvBuff)
        {
            if (!sp.IsOpen)
                return false;

            if ((sendBuff == null) || (recvBuff == null))
                return false;

            try
            {
                sp.DiscardInBuffer();
            }
            catch
            {
                ;
            }


            try
            {
                sp.Write(sendBuff, 0, sendBuff.Length);
            }
            catch
            {
                return false;
            }

            int count = 0;
            int timeout = 0;
            while (true)
            {
                try
                {
                    recvBuff[count] = (byte)sp.ReadByte();
                    count++;
                    if (count >= recvBuff.Length)
                        break;
                }
                catch
                {
                    timeout++;
                    if (timeout >= 5)
                        return false;
                    else
                        continue;
                }
            }

            return true;
        }

        private bool PortCommunicationRS232Special(byte[] sendBuff, byte[] recvBuff)
        {
            if (!sp.IsOpen)
                return false;

            if ((sendBuff == null) || (recvBuff == null))
                return false;

            if (recvBuff.Length < 2)
                return false;

            try
            {
                sp.DiscardInBuffer();
            }
            catch
            {
                ;
            }


            try
            {
                sp.Write(sendBuff, 0, sendBuff.Length);
            }
            catch
            {
                return false;
            }

            int timeout = 0;
            byte recvByte = 0;
            recvBuff[1] = 0x00;
            recvBuff[0] = 0x00;

            long beginTimes = GetCurrentTimeSeconds();
            while (true)
            {
                try
                {
                    recvByte = (byte)sp.ReadByte();
                    if (recvByte == 0xCC)
                        recvBuff[0] = recvByte;
                    else if (recvByte == 0xDD)
                    {
                        recvBuff[1] = recvByte;
                    }
                    else
                    {
                        recvBuff[1] = 0x00;
                        recvBuff[0] = 0x00;
                    }

                    if ((recvBuff[0] == 0xCC) && (recvBuff[1] == 0xDD))
                        break;

                    long currentTimes = GetCurrentTimeSeconds();
                    if (currentTimes > beginTimes + 5)
                        break;
                }
                catch
                {
                    timeout++;
                    if (timeout >= 5)
                        return false;
                    else
                        continue;
                }
            }

            return true;
        }

        private void InitRS232PortName()
        {
            comboBoxPortName.Items.Clear();
            string[] str = SerialPort.GetPortNames();
      
            if (str.Length == 0)
            {
                //MessageBox.Show("No SerialPort！", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                foreach (string s in str)
                {
                    comboBoxPortName.Items.Add(s);
                }
            }
            if (m_PortNameSelectUART < comboBoxPortName.Items.Count)
                comboBoxPortName.SelectedIndex = m_PortNameSelectUART;
            else
                comboBoxPortName.SelectedIndex = -1;
        }

        private void InitBaudRate()
        {
            comboBoxBaudRate.Items.Clear();
            if (comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_RS232)
            {
                for (int i = 0; i < BuadRate[0].Count(); i++)
                {
                    comboBoxBaudRate.Items.Add(BuadRate[0][i]);
                }
                //boud rate
                int index = 0;
                for (index = 0; index < comboBoxBaudRate.Items.Count; index++)
                {
                    if (comboBoxBaudRate.Items[index].ToString() == ShareParameter.BoudRate)
                    {
                        comboBoxBaudRate.SelectedIndex = index;
                        break;
                    }
                }
                if (index == comboBoxBaudRate.Items.Count)
                {
                    comboBoxBaudRate.Text = "115200";
                }
            }
            else if (comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_CAN)
            {
                for (int i = 0; i < BuadRate[1].Count(); i++)
                {
                    comboBoxBaudRate.Items.Add(BuadRate[1][i]);
                }
                //boud rate
                int index = 0;
                for (index = 0; index < comboBoxBaudRate.Items.Count; index++)
                {
                    if (comboBoxBaudRate.Items[index].ToString() == ShareParameter.BoudRate)
                    {
                        comboBoxBaudRate.SelectedIndex = index;
                        break;
                    }
                }
                if (index == comboBoxBaudRate.Items.Count)
                {
                    comboBoxBaudRate.Text = "500Kbit";
                }
            }

            else if (comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_SPI)
            {
                for (int i = 0; i < BuadRate[2].Count(); i++)
                {
                    comboBoxBaudRate.Items.Add(BuadRate[2][i]);
                }
                //boud rate
                int index = 0;
                for (index = 0; index < comboBoxBaudRate.Items.Count; index++)
                {
                    if (comboBoxBaudRate.Items[index].ToString() == ShareParameter.BoudRate)
                    {
                        comboBoxBaudRate.SelectedIndex = index;
                        break;
                    }
                }
                if (index == comboBoxBaudRate.Items.Count)
                {
                    comboBoxBaudRate.Text = "1.6875MHz";
                }
            }
            else if (comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_I2C)
            {
                for (int i = 0; i < BuadRate[3].Count(); i++)
                {
                    comboBoxBaudRate.Items.Add(BuadRate[3][i]);
                }
                //boud rate
                int index = 0;
                for (index = 0; index < comboBoxBaudRate.Items.Count; index++)
                {
                    if (comboBoxBaudRate.Items[index].ToString() == ShareParameter.BoudRate)
                    {
                        comboBoxBaudRate.SelectedIndex = index;
                        break;
                    }
                }
                if (index == comboBoxBaudRate.Items.Count)
                {
                    comboBoxBaudRate.Text = "100K";
                }
            }

        }

        private bool OpenSerialPort()
        {
            if (string.IsNullOrEmpty(comboBoxPortName.SelectedItem.ToString()))
            {
                MessageBox.Show("No RS232 port", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            List<string> deviceList = null;
            oSp.GetHIDDeviceList(uVID, uBridgeDevicePID, out deviceList);
            if ((deviceList != null) && (deviceList.Count > 0))
            {
                Hid.HID_RETURN rv = oSp.OpenDevice(uVID, uBridgeDevicePID, deviceList[0]);
                if (rv != Hid.HID_RETURN.SUCCESS)
                {
                    MessageBox.Show("Can't open the usb device", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    frm_Progress.CloseBar();
                    return false;
                }

                byte[] DataSendBuff = new byte[2];
                DataSendBuff[0] = 0xA1;
                if (comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_RS232)
                {
                    DataSendBuff[1] = 0x00;
                }
                else if (comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_CAN)
                {
                    DataSendBuff[1] = 0x01;
                }
                else if (comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_SPI)
                {
                    DataSendBuff[1] = 0x02;
                }
                else if (comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_I2C)
                {
                    DataSendBuff[1] = 0x03;
                }

                if (!PortSendUSB(DataSendBuff))
                {
                    MessageBox.Show("USB device send data error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                           MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    oSp.CloseDevice();
                    return false;
                }

                oSp.CloseDevice();
            }

            Thread.Sleep(100);

            try
            {
                sp.BaudRate = BaudRateFromComboBox()/*int.Parse(comboBoxBaudRate.Text)*/;
                sp.PortName = comboBoxPortName.SelectedItem.ToString();
                sp.DataBits = 8;
                sp.Parity = Parity.None;
                sp.StopBits = StopBits.One;
                sp.ReadTimeout = 1 * 1000;
                sp.WriteTimeout = 1 * 500;
                sp.Open();
                
            }
            catch
            {
                return false;
            }

            if (sp.IsOpen)
            {
                return true;
            }
            return false;
        }

        private int BaudRateFromComboBox()
        {
            int BaudRate = 0;
            if (comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_RS232)
            {
                BaudRate = int.Parse(comboBoxBaudRate.Text);
            }
            else if (comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_CAN)
            {
                if (comboBoxBaudRate.Text == "1Mbit")
                {
                    BaudRate = 1000000;
                }
                else if (comboBoxBaudRate.Text == "500Kbit")
                {
                    BaudRate = 500000;
                }
                else if (comboBoxBaudRate.Text == "250Kbit")
                {
                    BaudRate = 250000;
                }
                else if (comboBoxBaudRate.Text == "125Kbit")
                {
                    BaudRate = 125000;
                }
            }
            else if (comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_SPI)
            {
                if (comboBoxBaudRate.Text == "13.5MHz")
                {
                    BaudRate = 13500000;
                }
                else if (comboBoxBaudRate.Text == "6.75MHz")
                {
                    BaudRate = 6750000;
                }
                else if (comboBoxBaudRate.Text == "3.375MHz")
                {
                    BaudRate = 3375000;
                }
                else if (comboBoxBaudRate.Text == "1.6875MHz")
                {
                    BaudRate = 1687500;
                }
                else if (comboBoxBaudRate.Text == "0.84375MHz")
                {
                    BaudRate = 843750;
                }
            }
            else if (comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_I2C)
            {
                if (comboBoxBaudRate.Text == "1M")
                {
                    BaudRate = 1000000;
                }
                else if (comboBoxBaudRate.Text == "400K")
                {
                    BaudRate = 400000;
                }
                else if (comboBoxBaudRate.Text == "200K")
                {
                    BaudRate = 200000;
                }
                else if (comboBoxBaudRate.Text == "100K")
                {
                    BaudRate = 100000;
                }
                else if (comboBoxBaudRate.Text == "50K")
                {
                    BaudRate = 50000;
                }
                else if (comboBoxBaudRate.Text == "10K")
                {
                    BaudRate = 10000;
                }
            }
            return BaudRate;
        }

        private ArrayList SplitLength(string sourceString, int length)
        {
            ArrayList list = new ArrayList();
            for (int i = 0; i < sourceString.Trim().Length; i += length)
            {
                if (sourceString.Trim().Length - i >= length)
                {
                    list.Add(sourceString.Trim().Substring(i, length));
                }
                else
                {
                    list.Add(sourceString.Trim().Substring(i, sourceString.Trim().Length - i));
                }
            }
            return list;
        }

        public List<FILEPARTINFO> AddValueBin(string FilePath)
        {
            if (!File.Exists(FilePath))
            {
                return null;
            }

            List<FILEPARTINFO> partInfo;
            partInfo = new List<FILEPARTINFO>();

            FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            int fullLength = (int)fs.Length;

            if (fullLength == 0)
                return null;

            int partLength = 0;
            FILEPARTINFO tempInfo = new FILEPARTINFO();
            tempInfo.PartStartAddress = 0;
            tempInfo.PartEndAddress = 0;
            tempInfo.PartSize = 2048;
            tempInfo.PartData = new byte[2048];
            for (int k = 0; k < 2048; k++)
                tempInfo.PartData[k] = 0xFF;
            partInfo.Add(tempInfo);
            partLength = 0;

            for (int i = 0; i < fullLength; i++)
            {
                partInfo[partInfo.Count - 1].PartData[partLength] = br.ReadByte();
                partLength++;
                if ((partLength == 0x0800) && (i + 1 < fullLength))
                {
                    tempInfo = new FILEPARTINFO();
                    tempInfo.PartStartAddress = partInfo[partInfo.Count - 1].PartStartAddress + 0x0800;
                    tempInfo.PartEndAddress = 0;
                    tempInfo.PartSize = 2048;
                    tempInfo.PartData = new byte[2048];
                    for (int k = 0; k < 2048; k++)
                        tempInfo.PartData[k] = 0xFF;
                    partInfo.Add(tempInfo);
                    partLength = 0;
                }
            }

            return partInfo;
        }

        public List<FILEPARTINFO> AddValueHex(string FilePath)
        {
            if (!File.Exists(FilePath))
            {
                return null;
            }

            List<FILEPARTINFO> partInfo;
            partInfo = new List<FILEPARTINFO>();

            ArrayList arr1 = new ArrayList();

            string dizhi = "";

            string[] readtxt = File.ReadAllLines(FilePath);
            string firstvalue = readtxt[0];
            int qq3 = 0;
            int q1 = 0;
            int partLength = 0;

            for (int qq = 0; qq < readtxt.Length; qq++)
            {
                if (readtxt[qq].Substring(7, 2) == "04")
                {
                    int q = Convert.ToInt32(readtxt[qq].Substring(9, 4), 16);//16进制转10进制，取04后面的数据0800，作为基地址
                    q1 = Convert.ToInt32(q * Math.Pow((double)2, (double)16));

                    for (int k = 1; k < readtxt.Length - qq; k++)
                    {
                        if (string.IsNullOrEmpty(readtxt[qq + k]))
                            continue;
                        if (readtxt[qq + k].Substring(7, 2) == "00")
                        {
                            qq3 = Convert.ToInt32(readtxt[qq + k].Substring(3, 4), 16);//取第一行数据的地址
                            break;
                        }
                        if (readtxt[qq + k].Substring(7, 2) == "04")
                            return partInfo;
                    }

                    int thisPartBenginAddr = qq3 + q1;
                    int lastPartEndAddr = 0;
                    if (dizhi == string.Empty)
                        lastPartEndAddr = 0 + partLength - 16;
                    else
                        lastPartEndAddr = int.Parse(dizhi, System.Globalization.NumberStyles.HexNumber) + partLength - 16;

                    if ((thisPartBenginAddr - lastPartEndAddr > 0x10) || (partLength % 16 != 0) || (partLength == 0x800)) //非连续地址或者上个part地址未写满16字节，才算下一个part开始
                    {
                        dizhi = (qq3 + q1).ToString("X8");//转16进制

                        FILEPARTINFO tempInfo = new FILEPARTINFO();
                        tempInfo.PartStartAddress = uint.Parse(dizhi, System.Globalization.NumberStyles.HexNumber);
                        tempInfo.PartEndAddress = 0;
                        tempInfo.PartSize = 2048;
                        tempInfo.PartData = new byte[2048];
                        for (int k = 0; k < 2048; k++)
                            tempInfo.PartData[k] = 0xFF;
                        partInfo.Add(tempInfo);
                        partLength = 0;
                    }
                }
                else if ((readtxt[qq].Substring(7, 2) == "01") || (qq + 1 == readtxt.Length))
                {
                    break;
                }
                else if (readtxt[qq].Substring(7, 2) != "00")
                {
                    continue;
                }
                else
                {
                    string[] str4 = Regex.Split(readtxt[qq], ":");
                    string str5 = str4[1];
                    string strlength = str5.Substring(0, 2);
                    string str6 = str5.Substring(8, str5.Length - 2 - 8);
                    int ilength = int.Parse(strlength, System.Globalization.NumberStyles.HexNumber);
                    arr1 = SplitLength(str6, 2);

                    if (arr1.Count != ilength)
                    {
                        MessageBox.Show("Error", "File error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }

                    if (partLength == 0x0800)
                    {
                        FILEPARTINFO tempInfo = new FILEPARTINFO();
                        tempInfo.PartStartAddress = partInfo[partInfo.Count - 1].PartStartAddress + 0x0800;
                        tempInfo.PartEndAddress = 0;
                        tempInfo.PartSize = 2048;
                        tempInfo.PartData = new byte[2048];
                        for (int k = 0; k < 2048; k++)
                            tempInfo.PartData[k] = 0xFF;
                        partInfo.Add(tempInfo);
                        partLength = 0;
                    }
                    else if (partLength + ilength > 0x0800)
                    {
                        int iRemainLen = 0x0800 - partLength;
                        if (partInfo.Count > 0)
                        {
                            for (int i = 0; i < iRemainLen; i++)
                            {
                                partInfo[partInfo.Count - 1].PartData[partLength] = Convert.ToByte(arr1[i].ToString(), 16);
                                partLength++;
                            }
                        }

                        FILEPARTINFO tempInfo = new FILEPARTINFO();
                        tempInfo.PartStartAddress = partInfo[partInfo.Count - 1].PartStartAddress + 0x0800;
                        tempInfo.PartEndAddress = 0;
                        tempInfo.PartSize = 2048;
                        tempInfo.PartData = new byte[2048];
                        for (int k = 0; k < 2048; k++)
                            tempInfo.PartData[k] = 0xFF;
                        partInfo.Add(tempInfo);
                        partLength = 0;

                        if (partInfo.Count > 0)
                        {
                            for (int i = iRemainLen; i < arr1.Count; i++)
                            {
                                partInfo[partInfo.Count - 1].PartData[partLength] = Convert.ToByte(arr1[i].ToString(), 16);
                                partLength++;
                            }
                        }
                        continue;
                    }


                    if (partInfo.Count > 0)
                    {
                        for (int i = 0; i < arr1.Count; i++)
                        {
                            partInfo[partInfo.Count - 1].PartData[partLength] = Convert.ToByte(arr1[i].ToString(), 16);
                            partLength++;
                        }
                    }
                }
            }

            return partInfo;
        }

        private void comboBox_PortType_SelectedIndexChanged(object sender, EventArgs e)
        {
            InitDevice();
            InitBaudRate();
        }

        private void InitDevice()
        {
            lock (lockIniDevice)
            {
                if (comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_RS232)
                {
                    label2.Text = "Port Name";
                    comboBoxBaudRate.Enabled = true;
                    checkBox_CRC.Enabled = false;
                    label6.Enabled = true;
                    label4.Enabled = false;
                    textBox_I2CAddress.Enabled = false;
                    label_PID.Enabled = false;
                    label_VID.Enabled = false;
                    textBox_PID.Enabled = false;
                    textBox_VID.Enabled = false;
                    InitRS232PortName();
                }
                else if (comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_USB)
                {
                    label2.Text = "USB Device Serial No.";
                    comboBoxBaudRate.Enabled = false;
                    checkBox_CRC.Enabled = true;
                    label6.Enabled = false;
                    label4.Enabled = false;
                    textBox_I2CAddress.Enabled = false;
                    label_PID.Enabled = true;
                    label_VID.Enabled = true;
                    textBox_PID.Enabled = true;
                    textBox_VID.Enabled = true;

                    comboBoxPortName.Items.Clear();
                    List<string> deviceList = null;
                    oSp.GetHIDDeviceList(StringToUInt16(textBox_VID.Text, 16), StringToUInt16(textBox_PID.Text, 16), StringToByte(textBox_InterfaceNumber.Text, 16), out deviceList);
                    if ((deviceList != null) && (deviceList.Count > 0))
                    {
                        foreach (string str in deviceList)
                        {
                            comboBoxPortName.Items.Add(str);
                        }

                        if (m_PortNameSelectUSB < comboBoxPortName.Items.Count)
                            comboBoxPortName.SelectedIndex = m_PortNameSelectUSB;
                        else
                            comboBoxPortName.SelectedIndex = -1;
                    }
                    else
                    {
                        //MessageBox.Show("No USB IAP device!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else if (comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_CAN)
                {
                    label2.Text = "Port Name";
                    comboBoxBaudRate.Enabled = true;
                    checkBox_CRC.Enabled = true;
                    label6.Enabled = true;
                    label4.Enabled = false;
                    textBox_I2CAddress.Enabled = false;
                    label_PID.Enabled = false;
                    label_VID.Enabled = false;
                    textBox_PID.Enabled = false;
                    textBox_VID.Enabled = false;
                    InitRS232PortName();
                }
                else if (comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_SPI)
                {
                    label2.Text = "Port Name";
                    comboBoxBaudRate.Enabled = true;
                    checkBox_CRC.Enabled = true;
                    label6.Enabled = true;
                    label4.Enabled = false;
                    textBox_I2CAddress.Enabled = false;
                    label_PID.Enabled = false;
                    label_VID.Enabled = false;
                    textBox_PID.Enabled = false;
                    textBox_VID.Enabled = false;
                    InitRS232PortName();
                }
                else if (comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_I2C)
                {
                    label2.Text = "Port Name";
                    comboBoxBaudRate.Enabled = true;
                    checkBox_CRC.Enabled = true;
                    label6.Enabled = true;
                    label4.Enabled = true;
                    textBox_I2CAddress.Enabled = true;
                    label_PID.Enabled = false;
                    label_VID.Enabled = false;
                    textBox_PID.Enabled = false;
                    textBox_VID.Enabled = false;
                    InitRS232PortName();

                    I2CAddress = StringToUInt32(textBox_I2CAddress.Text, 16); 
                }
            }
        }

        public static uint StringToUInt32(string value, int fromBase)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }
            else
            {
                try
                {
                    return Convert.ToUInt32(value, fromBase);
                }
                catch
                {
                    return 0;
                }
            }
        }
        public static ushort StringToUInt16(string value, int fromBase)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }
            else
            {
                try
                {
                    return Convert.ToUInt16(value, fromBase);
                }
                catch
                {
                    return 0;
                }
            }
        }
        public static byte StringToByte(string value, int fromBase)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }
            else
            {
                try
                {
                    return Convert.ToByte(value, fromBase);
                }
                catch
                {
                    return 0;
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
        }

        private void IAPDemo_FormClosing(object sender, FormClosingEventArgs e)
        {
            timeEndPeriod(1);

            SetShareParameter();
            m_InitFlag = false;
            if (newth != null)
            {
                if (newth.IsAlive)
                {
                    newth.Abort();
                    newth = null;
                }
            }
        }

        private void comboBoxPortName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_RS232 ||
                comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_CAN ||
                comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_SPI ||
                comboBox_PortType.SelectedIndex == (int)PortType.PORT_TYPE_I2C)
            {
                m_PortNameSelectUART = comboBoxPortName.SelectedIndex;
            }
            else if (comboBox_PortType.SelectedIndex == 1)
            {
                m_PortNameSelectUSB = comboBoxPortName.SelectedIndex;
            }
        }

        private void textBox_AppAddress_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = "0123456789ABCDEFabcdef\b".IndexOf(char.ToUpper(e.KeyChar)) < 0;
        }

        private void comboBoxBaudRate_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = "0123456789\b".IndexOf(char.ToUpper(e.KeyChar)) < 0;
        }

    }
}
