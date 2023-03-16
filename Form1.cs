using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Reader;
using LitJson;

namespace RD91SWinForm
{
    public partial class Form1 : Form
    {
        public ReaderMethod reader = new ReaderMethod();

        volatile bool isLoop = false;

        string lastTag=string.Empty;
        public Form1()
        {
            InitializeComponent();
            reader.m_OnInventoryTag = onInventoryTag;
            reader.m_OnInventoryTagEnd = onInventoryTagEnd;
            reader.m_OnExeCMDStatus = onExeCMDStatus;
            reader.m_RefreshSetting = refreshSetting;
            reader.m_OnOperationTag = onOperationTag;
            reader.m_OnOperationTagEnd = onOperationTagEnd;
            reader.m_OnFastSwitchAntInventoryTagEnd = onFastSwitchAntInventoryTagEnd;
            reader.m_OnGetInventoryBufferTagCount = onGetInventoryBufferTagCount;
            reader.m_OnInventory6BTag = onInventory6BTag;
            reader.m_OnInventory6BTagEnd = onInventory6BTagEnd;
            reader.m_OnRead6BTag = onRead6BTag;
            reader.m_OnWrite6BTag = onWrite6BTag;
            reader.m_OnLock6BTag = onLock6BTag;
            reader.m_OnLockQuery6BTag = onLockQuery6BTag;
            reader.ReceiveCallback = onReceiveCallback;
        }

        void onReceiveCallback(byte[] btAryReceiveData)
        {
            string str = "";
            for (int i = 0; i < btAryReceiveData.Length; i++)
            {
                str += Convert.ToString(btAryReceiveData[i], 16) + "  ";
            }
            Console.WriteLine("cmd data ： " + str);
        }

        void refreshSetting(ReaderSetting readerSetting)
        {
            Console.WriteLine("Version:" + readerSetting.btMajor + "." + readerSetting.btMinor);
        }

        void onExeCMDStatus(byte cmd, byte status)
        {
            if (isLoop && (cmd == CMD.REAL_TIME_INVENTORY))
            {
                reader.InventoryReal((byte)0xFF, (byte)0xFF);
            }
            Console.WriteLine("CMD execute CMD:" + CMD.format(cmd) + "++Status code:" + ERROR.format(status));
        }

        void onInventoryTag(RXInventoryTag tag)
        {
            
            string res = tag.strEPC.Replace(" ", "").Substring(0,16);
            if (res.Length >= 16 && isWholeNumber(res))
            {

                if (tag.strEPC.Substring(1, 15) != null)
                {
                    string currentTag = res.Substring(1, 15);
                    if (currentTag!=lastTag)
                    {
                        try
                        {
                            
                            //先发送请求
                            string temp = NETTools.HttpGet(NETTools.targetAddress, "sampleCode=" + currentTag);
                            bool isSend = System.Convert.ToBoolean(temp);
                            if (isSend)
                            {
                             
                                //向编辑框发送文本消息
                                SendMsg sendMsg = new SendMsg();
                                sendMsg.SendText(res.Substring(1, 15));

                            }
                            else
                            {
                                MessageBox.Show("该样品无耳号信息！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);

                            }
                            

                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                        }
                        finally 
                        {
                            lastTag = currentTag;
                        }
                    }
                     
                }
            }
            Console.WriteLine("Inventory EPC:" + tag.strEPC);
            Console.WriteLine("Inventory Ant:" + tag.btAntId);
        }

        void onInventoryTagEnd(RXInventoryTagEnd tagEnd)
        {
            if (isLoop)
            {
                reader.InventoryReal((byte)0xFF, (byte)0xFF);
            }
        }

        void onFastSwitchAntInventoryTagEnd(RXFastSwitchAntInventoryTagEnd tagEnd)
        {
            Console.WriteLine("Fast Inventory end:" + tagEnd.mTotalRead);
        }

        void onInventory6BTag(byte nAntID, String strUID)
        {
            Console.WriteLine("Inventory 6B Tag:" + strUID);
        }

        void onInventory6BTagEnd(int nTagCount)
        {
            Console.WriteLine("Inventory 6B Tag:" + nTagCount);
        }

        void onRead6BTag(byte antID, String strData)
        {
            Console.WriteLine("Read 6B Tag:" + strData);
        }

        void onWrite6BTag(byte nAntID, byte nWriteLen)
        {
            Console.WriteLine("Write 6B Tag:" + nWriteLen);
        }

        void onLock6BTag(byte nAntID, byte nStatus)
        {
            Console.WriteLine("Lock 6B Tag:" + nStatus);
        }

        void onLockQuery6BTag(byte nAntID, byte nStatus)
        {
            Console.WriteLine("Lock query 6B Tag:" + nStatus);
        }

        void onGetInventoryBufferTagCount(int nTagCount)
        {
            Console.WriteLine("Get Inventory Buffer Tag Count" + nTagCount);
        }

        void onOperationTag(RXOperationTag tag)
        {
            Console.WriteLine("Operation Tag" + tag.strData);
        }

        void onOperationTagEnd(int operationTagCount)
        {
            Console.WriteLine("Operation Tag End" + operationTagCount);
        }
        /// 结束按钮
        private void endButton_Click(object sender, EventArgs e)
        {
            lastTag=string.Empty;
            isLoop=false;
            reader.CloseCom();
            endButton.Enabled = false;
            startButton.Enabled = true;
            Console.WriteLine("close Serial port!");
        }
        /// 开始
        private void startButton_Click(object sender, EventArgs e)
        {
           
            //Processing serial port to connect reader.
            string strException = string.Empty;
            string strComPort = cmbComPort.Text;
            int nBaudrate = 115200;

            int nRet = reader.OpenCom(strComPort, nBaudrate, out strException);
            if (nRet != 0)
            {
                string strLog = "链接失败: " + strException;
                Console.WriteLine(strLog);
                MessageBox.Show("连接失败：: " + strException);
                return;
            }
            else
            {
                //Console.WriteLine(reader.SetBeeperMode((byte)0xFF, (byte)0x02));
                // reader.SetBeeperMode(0x02, 0x02);
                reader.InventoryReal((byte)0xFF, (byte)0xFF);
             
                isLoop = true;
                string strLog = "连接成功：" + strComPort + "@" + nBaudrate.ToString();
                startButton.Enabled = false;
                endButton.Enabled = true;
                Console.WriteLine(strLog);
            }
        }


        /// <summary>
        /// 数字匹配
        /// </summary>
        /// <param name="strNumber"></param>
        /// <returns></returns>
        public static bool isWholeNumber(string strNumber)
        {
            System.Text.RegularExpressions.Regex g = new System.Text.RegularExpressions.Regex(@"^[0-9]\d*$");
            return g.IsMatch(strNumber);
        }

       
    }
}
