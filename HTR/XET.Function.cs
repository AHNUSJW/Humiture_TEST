using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace HTR
{
    public partial class XET : SCT
    {
        public Form myMainFrom;                       //将Main窗体定义为Form类型，需跨线程调用Main中的函数
        public Main meMainFrom;                       //将Main窗体定义为Main类型，可直接调用Main中的函数
        public String password = "EE";                //文件加密的密码(只能用简单的密码，如单个字母或数字等，否则会出错)
        public double totalmax = double.MinValue;     //最大值列的最大值
        public double totalmin = double.MaxValue;     //最小值列的最小值
        public double colmax = double.MinValue;       //每一列的最大值
        public double colmin = double.MaxValue;       //每一列的最小值
        public double totalsum = 0;                   //数据表中所有值的和
        public double number = 0;                     //数据表中有效数据的数量

        #region XET初始化

        public XET()
        {

            userName = "admin";
            userPassword = "";
            userCFG = Application.StartupPath + @"\cfg";
            userDAT = Application.StartupPath + @"\dat";
            userOUT = Application.StartupPath + @"\out";
            userSAV = Application.StartupPath + @"\sav";
            userLOG = Application.StartupPath + @"\log";
            userPIC = Application.StartupPath + @"\pic";
            userHEL = Application.StartupPath + @"\hel";
            userCFGPATH = userCFG + @"\dataPath";

            if (!Directory.Exists(userCFG)) Directory.CreateDirectory(userCFG);
            if (!Directory.Exists(userDAT)) Directory.CreateDirectory(userDAT);
            if (!Directory.Exists(userOUT)) Directory.CreateDirectory(userOUT);
            if (!Directory.Exists(userSAV)) Directory.CreateDirectory(userSAV);
            if (!Directory.Exists(userLOG)) Directory.CreateDirectory(userLOG);
            if (!Directory.Exists(userPIC)) Directory.CreateDirectory(userPIC);
            if (!Directory.Exists(userHEL)) Directory.CreateDirectory(userHEL);
            if (!Directory.Exists(userCFGPATH)) Directory.CreateDirectory(userCFGPATH);

            //串口初始化
            mePort.PortName = "COM1";
            mePort.BaudRate = Convert.ToInt32(BAUT.B9600); //波特率固定
            mePort.DataBits = Convert.ToInt32("8"); //数据位固定
            mePort.StopBits = StopBits.One; //停止位固定
            mePort.Parity = Parity.None; //校验位固定
            mePort.ReceivedBytesThreshold = 1; //接收即通知
            mePort.DataReceived += new SerialDataReceivedEventHandler(mePort_DataReceived);//串口接收数据

            //
            meTask = TASKS.disconnected;
            Array.Clear(meTXD, 0, meTXD.Length);
            myPC = 0;
        }

        public XET(Form myFrom)     //public XET(Main myMainFrom) meMainFrom = myMainFrom;
        {
            myMainFrom = myFrom;

            userName = "admin";
            userPassword = "";
            userCFG = Application.StartupPath + @"\cfg";
            userDAT = Application.StartupPath + @"\dat";
            userOUT = Application.StartupPath + @"\out";
            userSAV = Application.StartupPath + @"\sav";
            userLOG = Application.StartupPath + @"\log";
            userPIC = Application.StartupPath + @"\pic";
            userHEL = Application.StartupPath + @"\hel";
            userCFGPATH = userCFG + @"\dataPath";

            if (!Directory.Exists(userCFG)) Directory.CreateDirectory(userCFG);
            if (!Directory.Exists(userDAT)) Directory.CreateDirectory(userDAT);
            if (!Directory.Exists(userOUT)) Directory.CreateDirectory(userOUT);
            if (!Directory.Exists(userSAV)) Directory.CreateDirectory(userSAV);
            if (!Directory.Exists(userLOG)) Directory.CreateDirectory(userLOG);
            if (!Directory.Exists(userPIC)) Directory.CreateDirectory(userPIC);
            if (!Directory.Exists(userHEL)) Directory.CreateDirectory(userHEL);
            if (!Directory.Exists(userCFGPATH)) Directory.CreateDirectory(userCFGPATH);

            //串口初始化
            mePort.PortName = "COM1";
            mePort.BaudRate = Convert.ToInt32(BAUT.B9600); //波特率固定
            mePort.DataBits = Convert.ToInt32("8"); //数据位固定
            mePort.StopBits = StopBits.One; //停止位固定
            mePort.Parity = Parity.None; //校验位固定
            mePort.ReceivedBytesThreshold = 1; //接收即通知
            mePort.DataReceived += new SerialDataReceivedEventHandler(mePort_DataReceived);

            //
            meTask = TASKS.disconnected;
            Array.Clear(meTXD, 0, meTXD.Length);
            myPC = 0;
        }

        #endregion

        #region 串口操作

        #region 串口设置

        public void setSerialPort(string portName, string baud)
        {
            mePort.PortName = portName;
            mePort.BaudRate = Convert.ToInt32(baud); //Convert.ToInt32(BAUT.B9600); //波特率固定
            mePort.DataBits = Convert.ToInt32("8"); //数据位固定
            mePort.StopBits = StopBits.One; //停止位固定
            mePort.Parity = Parity.Even; //校验位固定
            //mePort.ReceivedBytesThreshold = 1; //接收即通知
        }

        public void setSerialPort(string portName, int timeout)
        {
            mePort.PortName = portName;
            mePort.BaudRate = Convert.ToInt32(BAUT.B9600); //波特率固定
            mePort.DataBits = Convert.ToInt32("8"); //数据位固定
            mePort.StopBits = StopBits.One; //停止位固定
            mePort.Parity = Parity.Even; //校验位固定
            //mePort.ReceivedBytesThreshold = 1; //接收即通知
            mePort.ReadTimeout = timeout;   //设置读取超时时间
        }

        public Boolean PortOpen()
        {
            try
            {
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;
                return true;
            }
            catch (Exception ex)
            {
                meTips += "打开端口失败:" + ex.ToString() + Environment.NewLine;
                return false;
            }
        }

        #endregion

        #region 设备连接

        /// <summary>
        /// 检查设备连接
        /// </summary>
        /// <param name="address">设备地址: 0x00-0xFF</param>
        /// <returns></returns>
        public Boolean CheckDevice(Byte address)
        {
            try
            {
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复
                meTips += "readDevice:" + address.ToString("X2") + Environment.NewLine;
                mePort.DiscardInBuffer();
                mePort.DiscardOutBuffer();
                mePort_Send_ReadIDN(address);   //读取设备寄存器

                byte[] RXData = new Byte[3];    //定义数组长度为3
                Boolean ret = ReadACKCommandWithTimeout(ref RXData, 100); //RXData已在函数中被重置长度
                if (ret == true)    //应答指令接成功
                {
                    meUID = RXData[3].ToString("X2") + RXData[4].ToString("X2") + RXData[5].ToString("X2") + RXData[6].ToString("X2");
                    meHWVer = RXData[7].ToString("X2");
                    meSWVer = RXData[8].ToString();
                    meType = (DEVICE)RXData[9];
                    meBat = (ushort)getValue(RXData[12], RXData[13]);
                    meTMax = (short)getValue(RXData[14], RXData[15]);
                    meTMin = (short)getValue(RXData[16], RXData[17]);
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;                 //应答指令接收成功或失败
            }
            catch (Exception ex)
            {
                //MessageBox.Show("CheckDevice timeout:" + ex.ToString());
                meTips += "CheckDevice timeout:" + ex.ToString() + Environment.NewLine;
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }

        #endregion

        #region 读设备寄存器

        #region 读Device寄存器

        public Boolean readDevice()
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "readDevice:" + Environment.NewLine;
                while (comm_repeat-- > 0)   //若读取失败则重复读取
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_ReadREG(RTCOM.COM_READ_DEVICE, Constants.REG_DEVICE, Constants.LEN_READ_DEVICE);

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData); //RXData已在函数中被重置长度

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        meUID = RXData[3].ToString("X2") + RXData[4].ToString("X2") + RXData[5].ToString("X2") + RXData[6].ToString("X2");
                        meHWVer = RXData[7].ToString("X2");
                        meSWVer = RXData[8].ToString();
                        meType = (DEVICE)RXData[9];
                        meBat = (ushort)getValue(RXData[12], RXData[13]);
                        meTMax = (short)getValue(RXData[14], RXData[15]);
                        meTMin = (short)getValue(RXData[16], RXData[17]);
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "读取超时:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("readDevice：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }

        #endregion

        #region 读Calendar寄存器

        public Boolean readCalendar()
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "readCalendar:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_ReadREG(RTCOM.COM_READ_CALENDAR, Constants.REG_CALENDAR, Constants.LEN_READ_CALENDAR);

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        if ((RXData[3] != 0))   //RXData[3]为年，其值为0时，dateTime变量无法识别会报错
                        {
                            //如果接收到的十六进制字节为21 07 11 15 43 55，表示2021-07-11 15:43:55，要先把16进制字节转换为字符串，再将字符串转换为10进制数字
                            int myYear = 2000 + Convert.ToByte(RXData[3].ToString("x8"), 10);
                            Byte myMon = Convert.ToByte(RXData[4].ToString("x8"), 10);
                            Byte myDay = Convert.ToByte(RXData[5].ToString("x8"), 10);
                            Byte myHour = Convert.ToByte(RXData[6].ToString("x8"), 10);
                            Byte myMin = Convert.ToByte(RXData[7].ToString("x8"), 10);
                            Byte mySec = Convert.ToByte(RXData[8].ToString("x8"), 10);
                            meDateCalendar = new DateTime(myYear, myMon, myDay, myHour, myMin, mySec);
                            meRTC = RXData[9];
                            meHTT = RXData[10];
                        }
                        else
                        {
                            meDateCalendar = DateTime.MinValue;
                        }
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "读取超时:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("readCalendar：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }

        #endregion

        #region 读去冷凝状态
        public Boolean readCondense()
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "readCondense:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_ReadREG(RTCOM.COM_READ_CALENDAR, Constants.REG_TIME_CONDENSE, 0X01);//读去冷凝状态

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        condenseTime = RXData[3];
                        meDateCondense = DateTime.Now.AddMinutes(condenseTime);
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "读取超时:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("readCalendar：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }
        #endregion

        #region 读Time寄存器

        public Boolean readTime()
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "readTime:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_ReadREG(RTCOM.COM_READ_TIME, Constants.REG_TIME, Constants.LEN_READ_TIME);

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        meDateHW = DateTime.ParseExact(getValue(RXData[3], RXData[4], RXData[5], RXData[6]).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                        meDateBat = DateTime.ParseExact(getValue(RXData[7], RXData[8], RXData[9], RXData[10]).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                        meDateDot = DateTime.ParseExact(getValue(RXData[11], RXData[12], RXData[13], RXData[14]).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                        meDateCal = DateTime.ParseExact(getValue(RXData[15], RXData[16], RXData[17], RXData[18]).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "读取超时:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("readTime：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }

        #endregion

        #region 读Jobset寄存器

        public Boolean readJobset()
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "readJobset:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_ReadREG(RTCOM.COM_READ_JOBSET, Constants.REG_JOBSET, Constants.LEN_READ_JOBSET);

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        if ((RXData[3] != 0))    //RXData[3]为年，firmware初始值为0，dateTime变量无法识别会报错
                        {
                            int myYear = 2000 + Convert.ToByte(RXData[3].ToString("x8"), 10);
                            Byte myMon = Convert.ToByte(RXData[4].ToString("x8"), 10);
                            Byte myDay = Convert.ToByte(RXData[5].ToString("x8"), 10);
                            Byte myHour = Convert.ToByte(RXData[6].ToString("x8"), 10);
                            Byte myMin = Convert.ToByte(RXData[7].ToString("x8"), 10);
                            Byte mySec = Convert.ToByte(RXData[8].ToString("x8"), 10);
                            meDateWake = new DateTime(myYear, myMon, myDay, myHour, myMin, mySec);

                            myYear = 2000 + Convert.ToByte(RXData[9].ToString("x8"), 10);
                            myMon = Convert.ToByte(RXData[10].ToString("x8"), 10);
                            myDay = Convert.ToByte(RXData[11].ToString("x8"), 10);
                            myHour = Convert.ToByte(RXData[12].ToString("x8"), 10);
                            myMin = Convert.ToByte(RXData[13].ToString("x8"), 10);
                            mySec = Convert.ToByte(RXData[14].ToString("x8"), 10);
                            meDateStart = new DateTime(myYear, myMon, myDay, myHour, myMin, mySec);

                            myYear = 2000 + Convert.ToByte(RXData[15].ToString("x8"), 10);
                            myMon = Convert.ToByte(RXData[16].ToString("x8"), 10);
                            myDay = Convert.ToByte(RXData[17].ToString("x8"), 10);
                            myHour = Convert.ToByte(RXData[18].ToString("x8"), 10);
                            myMin = Convert.ToByte(RXData[19].ToString("x8"), 10);
                            mySec = Convert.ToByte(RXData[20].ToString("x8"), 10);
                            meDateEnd = new DateTime(myYear, myMon, myDay, myHour, myMin, mySec);
                        }
                        else
                        {
                            meDateWake = DateTime.MinValue;
                            meDateStart = DateTime.MinValue;
                            meDateEnd = DateTime.MinValue;
                        }

                        meUnit = RXData[21];
                        meSpan = (UInt16)getValue(RXData[22], RXData[23]);
                        meDuration = (UInt32)getValue(RXData[24], RXData[25], RXData[26], RXData[27]);
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;   //CRC校验失败
            }
            catch (Exception ex)
            {
                meTips += "读取超时:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("readJobset：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }

        #endregion

        #region 读Jobrec寄存器

        public Boolean readJobrec(UInt32 address, ref int myDataNum)
        {
            try
            {
                myDataNum = -1;
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "readJobrec:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_ReadJOBREC(address);    //发送读取测试数据指令

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);     //读取全部应答指令(应答字节数=RXData.Length)

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        int dataByte, offset, idx, myGroupNum;

                        dataByte = (meType == DEVICE.HTT) ? 2 : 4;     //温湿度采集器，4个字节为一组数据(温度+湿度)；温度或压力采集器，2个字节为一组数据
                        myDataNum = RXData.Length - 5 - 4;                  //计算应答指令中包含的数据长度 = 数据总长度-5个功能字节-4个地址字节
                        myGroupNum = (int)Math.Ceiling((System.Double)(myDataNum / (SZ.CHA * dataByte * 1.0)));     //数据共myGroupNum组(向上取整，注意必须用1.0进行小数计算)，每组SZ.CHA个温度值

                        for (int m = 0; m < myGroupNum; m++)    //组
                        {
                            mtp = new TMP();
                            corMtp = new TMP();
                            offset = m * SZ.CHA * dataByte + 7;             //dataByte个字节代表一个温度值==============================

                            for (int n = 0; n < SZ.CHA; n++)    //通道
                            {
                                idx = n * dataByte + offset;

                                if (idx >= RXData.Length - 2)  //索引超出读到的数据长度(数据个数不是16的整数)
                                {
                                    //mtp.OUT[n] = mtp.OUT[n - 1];
                                    //if (meType == DEVICE.HTH) mtp.HUM[n] = mtp.HUM[n - 1];     //温湿度传感器，另外存储湿度值
                                    continue;
                                }

                                switch (meType)
                                {
                                    case DEVICE.HTT:    //温度采集器
                                        mtp.OUT[n] = (Int16)getValue(RXData[idx], RXData[idx + 1]);//温度值
                                        CorData(mtp.OUT[n], 0, -9000, 15000, n);
                                        break;

                                    case DEVICE.HTH:    //温湿度采集器
                                        mtp.OUT[n] = (Int16)getValue(RXData[idx], RXData[idx + 1]);//温度值
                                        mtp.HUM[n] = (UInt16)getValue(RXData[idx + 2], RXData[idx + 3]);//湿度值
                                        //CorData(mtp.OUT[n], mtp.HUM[n], n);
                                        break;

                                    case DEVICE.HTP:    //压力采集器
                                        mtp.OUT[n] = (Int32)getValue_INT32(RXData[idx], RXData[idx + 1], RXData[idx + 2], RXData[idx + 3]);//压力值
                                        CorData(mtp.OUT[n], 0, 800, 60000, n);
                                        break;
                                    case DEVICE.HTQ:   //温湿度采集器
                                        mtp.OUT[n] = (Int16)getValue(RXData[idx], RXData[idx + 1]);//温度值
                                        mtp.HUM[n] = (UInt16)getValue(RXData[idx + 2], RXData[idx + 3]);//湿度值
                                        if (mtp.HUM[n] > 10000)
                                        {
                                            mtp.HUM[n] = 10000;
                                        }
                                        CorData(mtp.OUT[n], mtp.HUM[n], -4000, 8500, n);
                                        break;
                                    default:
                                        break;
                                }
                            }

                            myMem.Add(new TMP(mtp));
                            //存储修正后的数据
                            corMem.Add(new TMP(corMtp));

                            if (myMem.Count > 0x20000)
                            {
                                myMem.RemoveAt(0);
                                corMem.RemoveAt(0);
                            }
                        }
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;   //CRC校验失败
            }
            catch (Exception ex)
            {
                myDataNum = -2;
                meTips += "读取超时:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("readJobset：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }

        /*
        int m, n, dataByte, offset, idx, myGroupNum;
                meTips += "Jobrec:数据个数为" + DataLen.ToString() + Environment.NewLine;
                dataByte = (meType == DEVICE.HTH) ? 4 : 2;     //温湿度采集器，4个字节为一组数据(温度+湿度)；温度或压力采集器，2个字节为一组数据
                myGroupNum = (int)Math.Ceiling((System.Double)((DataLen - 4) / (SZ.CHA * dataByte * 1.0)));     //数据共myGroupNum组(向上取整，注意必须用1.0进行小数计算)，每组SZ.CHA个温度值===========================

                for (m = 0; m < myGroupNum; m++)//组
                {
                    offset = m * SZ.CHA * dataByte + 7;        //2个字节代表一个温度值==============================

                    for (n = 0; n < SZ.CHA; n++)//通道
                    {
                        idx = n * dataByte + offset;

                        if (idx >= ACKLen - 2)  //索引超出读到的数据长度(数据个数不是15的整数)
                        {
                            mtp.OUT[n] = mtp.OUT[n - 1];
                            if (meType == DEVICE.HTH) mtp.HUM[n] = mtp.HUM[n - 1];     //温湿度传感器，另外存储湿度值
                            continue;
                        }

                        switch (meType)
                        {
                            case DEVICE.HTT:    //温度采集器
                                mtp.OUT[n] = (Int16)getValue(rxBuff((short)idx), rxBuff((short)(idx + 1)));//温度值
                                break;

                            case DEVICE.HTH:    //温湿度采集器
                                mtp.OUT[n] = (Int16)getValue(rxBuff((short)idx), rxBuff((short)(idx + 1)));//温度值
                                mtp.HUM[n] = (UInt16)getValue(rxBuff((short)(idx + 2)), rxBuff((short)(idx + 3)));//湿度值
                                break;

                            case DEVICE.HTP:    //压力采集器
                                mtp.OUT[n] = (UInt16)getValue(rxBuff((short)idx), rxBuff((short)(idx + 1)));//压力值
                                break;

                            default:
                                break;
                        }
                    }

                    myMem.Add(new TMP(mtp));
        */

        #region 修正后的数据
        public void CorData(int temp, UInt16 hum, int min, int max, int n)
        {
            double k;

            bool temp_bl = false;
            bool hum_bl = false;

            int tempCount1 = tempCount * 2;
            int humCount1 = humCount * 2;
            //温度修正
            if (tempCount > 0)
            {
                if (temp < meData_Cor[0])
                {
                    k = 1.0 * (meData_Cor[1] - min) / (meData_Cor[0] - min);
                    corMtp.OUT[n] = Convert.ToInt32(k * (temp - min) + min);
                    temp_bl = true;
                }
                else if (tempCount1 == 16 && temp > meData_Cor[14])
                {
                    k = 1.0 * (max - meData_Cor[15]) / (max - meData_Cor[14]);
                    corMtp.OUT[n] = Convert.ToInt32(k * (temp - meData_Cor[14]) + meData_Cor[15]);
                    temp_bl = true;
                }
                else
                {
                    for (int i = 2; i < tempCount1; i += 2)
                    {
                        if (temp < meData_Cor[i])
                        {
                            k = 1.0 * (meData_Cor[i + 1] - meData_Cor[i - 1]) / (meData_Cor[i] - meData_Cor[i - 2]);
                            corMtp.OUT[n] = Convert.ToInt32(k * (temp - meData_Cor[i - 2]) + meData_Cor[i - 1]);
                            temp_bl = true;
                            break;
                        }
                    }
                }

                if (!temp_bl)
                {
                    k = 1.0 * (max - meData_Cor[tempCount1 - 1]) / (max - meData_Cor[tempCount1 - 2]);
                    corMtp.OUT[n] = Convert.ToInt32(k * (temp - meData_Cor[tempCount1 - 2]) + meData_Cor[tempCount1 - 1]);
                }

            }
            else
            {
                corMtp.OUT[n] = temp;
            }

            //湿度修正
            if (humCount > 0)
            {
                if (hum < meData_Cor[0 + 16])
                {
                    k = 1.0 * (meData_Cor[1 + 16] - 0) / (meData_Cor[0 + 16] - 0);
                    corMtp.HUM[n] = Convert.ToUInt16(k * (hum - 0) + 0);
                    hum_bl = true;
                }
                else if (humCount1 == 16 && hum > meData_Cor[14 + 16])
                {
                    k = 1.0 * (10000 - meData_Cor[15 + 16]) / (10000 - meData_Cor[14 + 16]);
                    corMtp.HUM[n] = Convert.ToUInt16(k * (hum - meData_Cor[14 + 16]) + meData_Cor[15 + 16]);
                    hum_bl = true;
                }
                else
                {
                    for (int i = 2; i < humCount1; i += 2)
                    {
                        if (hum < meData_Cor[i + 16])
                        {
                            k = 1.0 * (meData_Cor[i + 1 + 16] - meData_Cor[i - 1 + 16]) / (meData_Cor[i + 16] - meData_Cor[i - 2 + 16]);
                            corMtp.HUM[n] = Convert.ToUInt16(k * (hum - meData_Cor[i - 2 + 16]) + meData_Cor[i - 1 + 16]);
                            hum_bl = true;
                            break;
                        }
                    }
                }

                if (!hum_bl)
                {
                    k = 1.0 * (10000 - meData_Cor[humCount1 - 1 + 16]) / (10000 - meData_Cor[humCount1 - 2 + 16]);
                    corMtp.HUM[n] = Convert.ToUInt16(k * (hum - meData_Cor[humCount1 - 2 + 16]) + meData_Cor[humCount1 - 1 + 16]);
                }

                if (corMtp.HUM[n] > 10000)
                {
                    corMtp.HUM[n] = 10000;
                }
            }
            else
            {
                corMtp.HUM[n] = hum;
            }
        }
        #endregion


        #endregion

        #region 读DOT寄存器

        public Boolean readDOT(byte datalen)
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                int calNum = 8;     //默认标定数量为8：温度采集器
                if (meType == DEVICE.HTP) calNum = 3;  //压力采集器，标定数量为3
                meTips += "readDOT:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    Array.Clear(meTemp_CalPoints, 0, meTemp_CalPoints.Length);
                    mePort_Send_ReadREG(RTCOM.COM_READ_DOT, Constants.REG_DOT, datalen);

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        int idx = 3;
                        for (int i = 0; i < calNum; i++)
                        {
                            if (meType == DEVICE.HTT)
                            {
                                meTemp_CalPoints[i] = (Int16)((RXData[(Int16)(idx + 1)] << 8) + RXData[(Int16)idx]);    //标定温度(±)

                                int a = (Int32)RXData[(Int16)(idx + 4)];
                                int b = (Int32)RXData[(Int16)(idx + 3)];
                                int c = (Int32)RXData[(Int16)(idx + 2)];
                                meADC_CalPoints[i] = ((Int32)((a << 24) + (b << 16) + (c << 8))) / 256;

                                idx += 5;
                            }
                            else if (meType == DEVICE.HTP)
                            {
                                meTemp_CalPoints[i] = (Int32)getValue_INT32(RXData[idx], RXData[idx + 1], RXData[idx + 2], RXData[idx + 3]);//标定压力(+)
                                //(((RXData[(Int32)(idx + 1)] << 24) + RXData[(Int32)idx] << 16) / 65536);

                                int a = (Int32)RXData[(Int16)(idx + 6)];
                                int b = (Int32)RXData[(Int16)(idx + 5)];
                                int c = (Int32)RXData[(Int16)(idx + 4)];
                                meADC_CalPoints[i] = ((Int32)((a << 24) + (b << 16) + (c << 8))) / 256;

                                idx += 7;
                            }
                            else
                            {
                                meTemp_CalPoints[i] = (Int16)((RXData[(Int16)(idx + 1)] << 8) + RXData[(Int16)idx]);    //标定温度(±)
                                meHum_CalPoints[i] = (Int16)((RXData[(Int16)(idx + 1 + 32)] << 8) + RXData[(Int16)idx + 32]);//标定湿度

                                int a = (Int32)RXData[(Int16)(idx + 3)];
                                int b = (Int32)RXData[(Int16)(idx + 2)];
                                meADC_CalPoints[i] = ((Int32)((a << 24) + (b << 16)) / 65536);
                                a = (Int32)RXData[(Int16)(idx + 3 + 32)];
                                b = (Int32)RXData[(Int16)(idx + 2 + 32)];
                                meADC1_CalPoints[i] = ((Int32)((a << 24) + (b << 16)) / 65536);

                                idx += 4;
                            }
                        }

                        for (int i = 0; i < calNum - 1; i++)
                        {
                            meV_Slope[i] = ((float)(meADC_CalPoints[i + 1] - meADC_CalPoints[i]) / (float)(meTemp_CalPoints[i + 1] - meTemp_CalPoints[i]));
                            if (meType != DEVICE.HTT && meType != DEVICE.HTP)
                            {
                                meV1_Slope[i] = ((float)(meADC1_CalPoints[i + 1] - meADC1_CalPoints[i]) / (float)(meHum_CalPoints[i + 1] - meHum_CalPoints[i]));
                            }
                        }
                        meV_Slope[calNum - 1] = meV_Slope[calNum - 2];      //meV_Slope[7] = meV_Slope[6];
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "读取超时:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("readDOT：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }

        #endregion

        #region 读JSN寄存器

        public Boolean readJSN()
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "readJSN:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    meModel = meJSN = meRange = "blank";
                    mePort_Send_ReadREG(RTCOM.COM_READ_JSN, Constants.REG_JSN, Constants.LEN_READ_JSN);

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        string mychar1 = System.Text.Encoding.Default.GetString(new Byte[] { 0xFF });       //无效字符
                        string mychar2 = System.Text.Encoding.Default.GetString(new Byte[] { 0x00 });       //空字符null
                        string meJSNCode = System.Text.Encoding.Default.GetString(RXData, 3, RXData.Length - 5);
                        meJSNCode = meJSNCode.Replace(mychar1, "");    //滤除无法识别的空字符
                        meJSNCode = meJSNCode.Replace(mychar2, "");    //滤除无法识别的空字符
                        if (meJSNCode.Contains(","))                   //设备型号,设备编号，测量范围
                        {
                            meModel = meJSNCode.Split(',')[0];     //设备型号
                            meJSN = meJSNCode.Split(',')[1];       //设备编号
                            meRange = meJSNCode.Split(',')[2];     //测量范围
                        }
                        else                                         //(备用项，后面应该用不到)
                        {
                            meModel = string.Empty;                //设备型号
                            meJSN = meJSNCode;                     //设备编号
                            meRange = string.Empty;                //测量范围
                        }
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;   //CRC校验失败
            }
            catch (Exception ex)
            {
                meTips += "读取超时:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("readJSN：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }

        #endregion

        #region 读设备名称
        public Boolean readName()
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "readName:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_ReadREG(RTCOM.COM_READ_NAME, Constants.REG_NAME, Constants.LEN_READ_NAME);

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        string mychar1 = System.Text.Encoding.Default.GetString(new Byte[] { 0xFF });       //无效字符
                        string mychar2 = System.Text.Encoding.Default.GetString(new Byte[] { 0x00 });       //空字符null
                        string meJSNCode = System.Text.Encoding.Default.GetString(RXData, 3, RXData.Length - 5);
                        meJSNCode = meJSNCode.Replace(mychar1, "");    //滤除无法识别的空字符
                        meJSNCode = meJSNCode.Replace(mychar2, "");    //滤除无法识别的空字符
                        meName = meJSNCode;                            //设置设备名称
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;   //CRC校验失败
            }
            catch (Exception ex)
            {
                meTips += "读取超时:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("readJSN：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }
        #endregion
        #region 读USN寄存器

        public Boolean readUSN()
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "readUSN:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_ReadREG(RTCOM.COM_READ_USN, Constants.REG_USN, Constants.LEN_READ_USN);

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        string mychar1 = System.Text.Encoding.Default.GetString(new Byte[] { 0xFF, 0xFF });
                        string mychar2 = System.Text.Encoding.Default.GetString(new Byte[] { 0x00 });       //空字符null
                        meUSN = System.Text.Encoding.Default.GetString(RXData, 3, RXData.Length - 5);
                        meUSN = meUSN.Replace(mychar1, "");    //滤除无法识别的空字符
                        meUSN = meUSN.Replace(mychar2, "");    //滤除无法识别的空字符
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "读取超时:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("readUSN：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }

        #endregion

        #region 读UTXT寄存器

        public Boolean readUTXT()
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "readUTXT:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_ReadREG(RTCOM.COM_READ_UTXT, Constants.REG_UTXT, Constants.LEN_READ_UTXT);

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        string mychar = System.Text.Encoding.GetEncoding("GBK").GetString(new Byte[] { 0xFF, 0xFF });
                        meUTXT = System.Text.Encoding.GetEncoding("GBK").GetString(RXData, 3, RXData.Length - 5);
                        meUTXT = meUTXT.Replace(mychar, "");    //滤除无法识别的空字符
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "读取超时:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("readUTXT：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }

        #endregion

        #region 读数据修正
        public Boolean readCorData(byte length)
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    Array.Clear(meTemp_CalPoints, 0, meTemp_CalPoints.Length);
                    mePort_Send_ReadREG(RTCOM.COM_READ_DOT, Constants.REG_COR_DATE, length);

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        if (length == 0x21)
                        {
                            int idx = 4;
                            tempCount = Convert.ToInt32(RXData[3]);
                            humCount = 0;
                            for (int i = 0; i < tempCount * 2; i++)
                            {
                                meData_Cor[i] = (Int32)(((RXData[idx + 1] << 24) + (RXData[idx] << 16)) / 65536);   //修正数据(±)
                                idx += 2;
                            }
                        }
                        else
                        {
                            int idx = 5;
                            tempCount = Convert.ToInt32(RXData[3]);
                            humCount = Convert.ToInt32(RXData[4]);
                            for (int i = 0; i < tempCount * 2; i++)
                            {
                                meData_Cor[i] = (Int32)(((RXData[idx + 1] << 24) + (RXData[idx] << 16)) / 65536);    //修正数据(±)
                                idx += 2;
                            }
                            idx = 32 + 5;
                            for (int i = 16; i < humCount * 2 + 16; i++)
                            {
                                meData_Cor[i] = (Int32)(((RXData[idx + 1] << 24) + (RXData[idx] << 16)) / 65536);    //修正数据(±)
                                idx += 2;
                            }
                        }
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "读取超时:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("readDOT：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }
        #endregion

        #region 读熄屏状态
        public Boolean readScreenState()
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "readScreenState:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_ReadREG(RTCOM.COM_READ_SCREEN, Constants.REG_COR_DATE, Constants.REG_SCREEN_STATE);

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        MyDefine.myXET.meScreenStatus = RXData[3] == 0x00 ? true : false;
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "读取超时:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("readScreenState：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }
        #endregion

        #endregion

        #region 写设备寄存器

        #region 写设备名称
        //设置设备地址
        public Boolean setName(string name)
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "setName:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_SetName(name);   //设置新的设备名称

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "设置失败:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("setDevice：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }
        #endregion

        #region 写Device寄存器

        //设置设备地址
        public Boolean setDevice(Byte newAddress)
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "setDevice:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_SetDevice(newAddress);   //设置新的设备地址

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "设置失败:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("setDevice：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }

        #endregion

        #region 写Calendar寄存器

        public Boolean setCalendar()
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                DateTime time = DateTime.Now;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "setCalendar:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_SetCalendar(time);   //设置设备时间为当前系统时间

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        if ((RXData[3] != 0))   //RXData[3]为年，其值为0时，dateTime变量无法识别会报错
                        {
                            //如果接收到的十六进制字节为21 07 11 15 43 55，表示2021-07-11 15:43:55，要先把16进制字节转换为字符串，再将字符串转换为10进制数字
                            int myYear = 2000 + Convert.ToByte(RXData[3].ToString("x8"), 10);
                            Byte myMon = Convert.ToByte(RXData[4].ToString("x8"), 10);
                            Byte myDay = Convert.ToByte(RXData[5].ToString("x8"), 10);
                            Byte myHour = Convert.ToByte(RXData[6].ToString("x8"), 10);
                            Byte myMin = Convert.ToByte(RXData[7].ToString("x8"), 10);
                            Byte mySec = Convert.ToByte(RXData[8].ToString("x8"), 10);
                            DateTime reTime = new DateTime(myYear, myMon, myDay, myHour, myMin, mySec);
                            if (reTime.ToString("yyyy-MM-dd HH:mm:ss") != time.ToString("yyyy-MM-dd HH:mm:ss"))
                            {
                                ret = false;
                            }
                        }
                        else
                        {
                            meDateCalendar = DateTime.MinValue;
                        }
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "设置失败:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("setCalendar：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }

        //写入一个设备时间(时间最大值)，使当前设备停止采样
        public Boolean setMaxCalendar()
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "setCalendar:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_SetCalendar(DateTime.MaxValue);   //设置设备时间为最大时间(使设备停止测试)

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "设置失败:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("setCalendar：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }

        #endregion

        #region 写Time寄存器

        public Boolean setTime()
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "setTime:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_SetTime();         //将设备标定时间修改为当前系统时间

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "设置失败:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("setCalendar：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }

        #endregion

        #region 写Jobset寄存器

        public Boolean setJobset()
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "setJobset:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_SetJobset();   //设置开始、结束、间隔单位、间隔时间

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "设置失败:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("setJobset：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }

        #endregion

        #region 写JSN寄存器

        public Boolean setJSN(string myText)
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "setJSN:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_SetJSN(myText);   //设置JSN

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "设置失败:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("setJSN：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }

        #endregion

        #region 写USN寄存器

        public Boolean setUSN(string myText)
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "setUSN:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_SetUSN(myText);   //设置USN

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "设置失败:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("setUSN：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }

        #endregion

        #region 写UTXT寄存器

        public Boolean setUTXT(string myText)
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "setUTXT:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_SetUTXT(myText);   //设置UTXT

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "设置失败:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("setUTXT：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }

        #endregion

        #region 写Jobrec寄存器 -- 清卡

        //清卡
        public Boolean setJobrec()
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "setJobrec:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_SetJobrec();   //清卡

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "设置失败:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("setJobrec：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }

        #endregion

        #region 写DOT寄存器 -- 标定

        //标定
        //HTT: calNum = 8,标定8组数据；HTP：calNum = 3,标定3组数据
        public Boolean setDOT(int calNum = 8)
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "setDOT:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_SetDOT(calNum);   //标定（标定数据已提前放入全局变量）

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "设置失败:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("setDOT：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }

        #endregion

        #region 写REG_BAT_CL寄存器 -- 清除电池重上电标志

        //清除电池重上电标志
        public Boolean setREG_BAT_CL()
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "setREG_BAT_CL:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_SetREG_BAT_CL();    //清除电池重上电标志

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "设置失败:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("setJobrec：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }

        #endregion

        #region 写REG_CTEMP_CL寄存器 -- 清除设备最高最低温度寄存器

        //清除设备最高最低温度寄存器
        public Boolean setREG_CTEMP_CL()
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "setREG_CTEMP_CL:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_SetREG_CTEMP_CL();   //清除设备最高最低温度

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "设置失败:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("setJobrec：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }

        #endregion

        #region 写REG_UNIT寄存器 -- 切换单位

        //切换单位
        public Boolean setREG_UNIT()
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "setREG_UNIT:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_SetREG_UNIT();    //切换单位

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        if (RXData[3] == 0X01)
                        {
                            MyDefine.myXET.temUnit = "℃";
                            MessageBox.Show("单位切换为摄氏度（℃）");
                        }
                        else if (RXData[3] == 0x02)
                        {
                            MyDefine.myXET.temUnit = "℉";
                            MessageBox.Show("单位切换为华氏度（℉）");
                        }
                        ret = true;
                        break;
                    }
                }

                if (!ret)
                {
                    MessageBox.Show("切换单位失败");
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "设置失败:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("setJobrec：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }

        #endregion

        #region 写REG_TIME_CONDENSE寄存器 -- 去冷凝

        //去冷凝
        public Boolean setREG_TIME_CONDENSE()
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "setREG_TIME_CONDENSE:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_SetTIME_CONDENSE();   //发送去冷凝指令

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "设置失败:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("setJobrec：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }

        #endregion

        #region 写寄存器 -- 数据修正
        //数据修正
        public Boolean setCorData(byte length)
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "setCorData:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_SetData(length);   //数据修正（将修正数据已提前放入全局变量）

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "设置失败:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("setDOT：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }
        #endregion

        #region 写熄屏状态
        public Boolean setScreenState()
        {
            try
            {
                Boolean ret = false;
                int comm_repeat = Constants.REPEAT;
                if (mePort.IsOpen == false) mePort.Open();
                if (mePort.IsOpen == false) return false;    //端口打开失败(端口被占用)
                meAutoReceive = false;     //禁止自动接收回复

                meTips += "setScreenState:" + Environment.NewLine;
                while (comm_repeat-- > 0)
                {
                    mePort.DiscardInBuffer();
                    mePort.DiscardOutBuffer();
                    mePort_Send_SetScreenState();

                    byte[] RXData = new Byte[3];    //定义数组长度为3
                    Boolean result = ReadACKCommandWithTimeout(ref RXData);

                    if (result == false)    //应答指令接收失败，重新发送指令
                    {
                        continue;
                    }
                    else                    //应答指令读取成功
                    {
                        ret = true;
                        break;
                    }
                }

                meAutoReceive = true;     //使能自动接收字节
                return ret;
            }
            catch (Exception ex)
            {
                meTips += "设置失败:" + ex.ToString() + Environment.NewLine;
                //MessageBox.Show("setScreenState：" + Environment.NewLine + ex.ToString());
                meAutoReceive = true;     //使能自动接收字节
                return false;
            }
        }
        #endregion

        #endregion

        #region 指令发送

        #region 发送寄存器设置指令

        #region 设置设备名称
        //发送--设置Device寄存器
        public bool mePort_Send_SetName(string name)
        {
            try
            {
                Byte num = 0;
                Encoding gb = System.Text.Encoding.GetEncoding("gb2312");
                byte[] arrASCII = gb.GetBytes(name);
                //Array.Resize<Byte>(ref arrASCII, Constants.LEN_SET_JSN);  //重新定义数组长度，并保留其原有值
                //if (myJSN == null) arrASCII = new Byte[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                //串口发送
                if (mePort.IsOpen == true)
                {
                    meTXD[num++] = meActiveAddr;           //设备地址
                    meTXD[num++] = Constants.SET_REG;       //设置寄存器
                    meTXD[num++] = Constants.REG_NAME;  //JOBREC寄存器地址
                    meTXD[num++] = (Byte)arrASCII.Length;  //数据长度

                    for (int i = 0; i < arrASCII.Length; i++)
                    {
                        meTXD[num++] = arrASCII[i];
                    }

                    UInt16 myCRC = CRC16(meTXD, num);    //CRC校验
                    meTXD[num++] = (System.Byte)(myCRC >> 8);       //CRC高八位
                    meTXD[num++] = (System.Byte)(myCRC & 0x00FF);   //CRC低八位
                                                                    //MessageBox.Show(meTXD[10].ToString ("X") + meTXD[11].ToString ("X"));
                    mePort.Write(meTXD, 0, num);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }
        #endregion

        #region 设置Device寄存器

        //发送--设置Device寄存器
        public bool mePort_Send_SetDevice(Byte newAddress)
        {
            try
            {
                Byte num = 0;
                DateTime currentTime = new DateTime();
                currentTime = DateTime.Now;

                //串口发送
                if (mePort.IsOpen == true)
                {
                    meTXD[num++] = meActiveAddr;           //设备地址
                    meTXD[num++] = Constants.SET_REG;       //设置寄存器
                    meTXD[num++] = Constants.REG_DEVICE;    //设备寄存器地址
                    meTXD[num++] = Constants.LEN_SET_DEVICE;//数据长度
                    meTXD[num++] = 0x03;                    //校验方式, 固定0x03
                    meTXD[num++] = newAddress;              //新的设备地址

                    UInt16 myCRC = CRC16(meTXD, num);    //CRC校验
                    meTXD[num++] = (System.Byte)(myCRC >> 8);       //CRC高八位
                    meTXD[num++] = (System.Byte)(myCRC & 0x00FF);   //CRC低八位
                                                                    //MessageBox.Show(meTXD[num-2].ToString("X") + meTXD[num-1].ToString("X"));
                    mePort.Write(meTXD, 0, num);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }

        #endregion

        #region 设置Calendar寄存器

        //发送--设置Calendar寄存器
        public bool mePort_Send_SetCalendar(DateTime calTime)
        {
            try
            {
                Byte num = 0;

                //串口发送
                if (mePort.IsOpen == true)
                {
                    meTXD[num++] = meActiveAddr;           //设备地址
                    meTXD[num++] = Constants.SET_REG;       //设置寄存器
                    meTXD[num++] = Constants.REG_CALENDAR;  //日历寄存器地址
                    meTXD[num++] = Constants.LEN_SET_CALENDAR;  //数据长度

                    //meTXD[num++] = Convert.ToByte(currentTime.ToString("yy"));  //年
                    //meTXD[num++] = (byte)(currentTime.Month);
                    //meTXD[num++] = (byte)(currentTime.Day);
                    //meTXD[num++] = (byte)(currentTime.Hour);
                    //meTXD[num++] = (byte)(currentTime.Minute);
                    //meTXD[num++] = (byte)(currentTime.Second);

                    //十六进制转十进制
                    meTXD[num++] = Convert.ToByte(calTime.ToString("yy"), 16);  //年
                    meTXD[num++] = Convert.ToByte(calTime.Month.ToString(), 16); //月
                    meTXD[num++] = Convert.ToByte(calTime.Day.ToString(), 16);  //日
                    meTXD[num++] = Convert.ToByte(calTime.Hour.ToString(), 16);
                    meTXD[num++] = Convert.ToByte(calTime.Minute.ToString(), 16);
                    meTXD[num++] = Convert.ToByte(calTime.Second.ToString(), 16);

                    UInt16 myCRC = CRC16(meTXD, num);    //CRC校验
                    meTXD[num++] = (System.Byte)(myCRC >> 8);       //CRC高八位
                    meTXD[num++] = (System.Byte)(myCRC & 0x00FF);   //CRC低八位
                                                                    //MessageBox.Show(meTXD[num-2].ToString("X") + meTXD[num-1].ToString("X"));
                    mePort.Write(meTXD, 0, num);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }

        #endregion

        #region 设置Time寄存器

        //发送--设置Time寄存器
        public bool mePort_Send_SetTime()
        {
            try
            {
                Byte num = 0;

                //串口发送
                if (mePort.IsOpen == true)
                {
                    meTXD[num++] = meActiveAddr;           //设备地址
                    meTXD[num++] = Constants.SET_REG;       //设置寄存器
                    meTXD[num++] = Constants.REG_TIME;      //Time寄存器地址
                    meTXD[num++] = Constants.LEN_SET_TIME;  //数据长度

                    //硬件制造日期
                    Int32 myIntDate = Convert.ToInt32(meDateHW.ToString("yyyyMMdd"), 10);      //将日期转换为十进制数字格式，如20210815
                    meTXD[num++] = (Byte)myIntDate;
                    meTXD[num++] = (Byte)(myIntDate >> 8);
                    meTXD[num++] = (Byte)(myIntDate >> 16);
                    meTXD[num++] = (Byte)(myIntDate >> 24);

                    //更换电池日期
                    myIntDate = Convert.ToInt32(meDateBat.ToString("yyyyMMdd"), 10);      //将日期转换为十进制数字格式，如20210815
                    meTXD[num++] = (Byte)myIntDate;
                    meTXD[num++] = (Byte)(myIntDate >> 8);
                    meTXD[num++] = (Byte)(myIntDate >> 16);
                    meTXD[num++] = (Byte)(myIntDate >> 24);

                    //温度标定日期
                    myIntDate = Convert.ToInt32(meDateDot.ToString("yyyyMMdd"), 10);          //将标定日期转换为十进制数字格式，如20210815
                    meTXD[num++] = (Byte)myIntDate;
                    meTXD[num++] = (Byte)(myIntDate >> 8);
                    meTXD[num++] = (Byte)(myIntDate >> 16);
                    meTXD[num++] = (Byte)(myIntDate >> 24);

                    //计量校准日期
                    myIntDate = Convert.ToInt32(meDateCal.ToString("yyyyMMdd"), 10);         //将计量校准日期转换为十进制数字格式，如20210815
                    meTXD[num++] = (Byte)myIntDate;
                    meTXD[num++] = (Byte)(myIntDate >> 8);
                    meTXD[num++] = (Byte)(myIntDate >> 16);
                    meTXD[num++] = (Byte)(myIntDate >> 24);

                    UInt16 myCRC = CRC16(meTXD, num);    //CRC校验
                    meTXD[num++] = (System.Byte)(myCRC >> 8);       //CRC高八位
                    meTXD[num++] = (System.Byte)(myCRC & 0x00FF);   //CRC低八位

                    mePort.Write(meTXD, 0, num);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }

        #endregion

        #region 设置Jobset寄存器

        //发送--设置JOBSET寄存器
        public bool mePort_Send_SetJobset()
        {
            try
            {
                Byte num = 0;
                //meDateWake = (dateStart == DateTime.MinValue) ? DateTime.MinValue : dateStart.AddSeconds(-40);        //提前40s唤醒设备

                //串口发送
                if (mePort.IsOpen == true)
                {
                    meTXD[num++] = meActiveAddr;           //设备地址
                    meTXD[num++] = Constants.SET_REG;       //设置寄存器
                    meTXD[num++] = Constants.REG_JOBSET;    //日历寄存器地址
                    meTXD[num++] = Constants.LEN_SET_JOBSET;  //数据长度

                    //十六进制转十进制
                    meTXD[num++] = Convert.ToByte(meDateWake.ToString("yy"), 16);  //年
                    meTXD[num++] = Convert.ToByte(meDateWake.Month.ToString(), 16); //月
                    meTXD[num++] = Convert.ToByte(meDateWake.Day.ToString(), 16);  //日
                    meTXD[num++] = Convert.ToByte(meDateWake.Hour.ToString(), 16);
                    meTXD[num++] = Convert.ToByte(meDateWake.Minute.ToString(), 16);
                    meTXD[num++] = Convert.ToByte(meDateWake.Second.ToString(), 16);

                    meTXD[num++] = Convert.ToByte(meDateStart.ToString("yy"), 16);  //年
                    meTXD[num++] = Convert.ToByte(meDateStart.Month.ToString(), 16); //月
                    meTXD[num++] = Convert.ToByte(meDateStart.Day.ToString(), 16);  //日
                    meTXD[num++] = Convert.ToByte(meDateStart.Hour.ToString(), 16);
                    meTXD[num++] = Convert.ToByte(meDateStart.Minute.ToString(), 16);
                    meTXD[num++] = Convert.ToByte(meDateStart.Second.ToString(), 16);

                    meTXD[num++] = Convert.ToByte(meDateEnd.ToString("yy"), 16);  //年
                    meTXD[num++] = Convert.ToByte(meDateEnd.Month.ToString(), 16); //月
                    meTXD[num++] = Convert.ToByte(meDateEnd.Day.ToString(), 16);  //日
                    meTXD[num++] = Convert.ToByte(meDateEnd.Hour.ToString(), 16);
                    meTXD[num++] = Convert.ToByte(meDateEnd.Minute.ToString(), 16);
                    meTXD[num++] = Convert.ToByte(meDateEnd.Second.ToString(), 16);

                    meTXD[num++] = (Byte)meUnit;            //间隔单位：秒
                    meTXD[num++] = (Byte)meSpan;  //间隔时间低八位
                    meTXD[num++] = (Byte)(meSpan >> 8);   //间隔时间高八位

                    meTXD[num++] = (Byte)meDuration;
                    meTXD[num++] = (Byte)(meDuration >> 8);
                    meTXD[num++] = (Byte)(meDuration >> 16);
                    meTXD[num++] = (Byte)(meDuration >> 24);

                    UInt16 myCRC = CRC16(meTXD, num);    //CRC校验
                    meTXD[num++] = (System.Byte)(myCRC >> 8);       //CRC高八位
                    meTXD[num++] = (System.Byte)(myCRC & 0x00FF);   //CRC低八位
                                                                    //MessageBox.Show(num .ToString ());
                                                                    //MessageBox.Show(meTXD[29].ToString ("X") + meTXD[30].ToString ("X"));
                    mePort.Write(meTXD, 0, num);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }

        //发送--设置JOBSET寄存器
        public bool mePort_Send_SetJobset(DateTime startTime, DateTime endTime, Byte meUnit, UInt16 testInterval)
        {
            try
            {
                Byte num = 0;
                DateTime currentTime = new DateTime();
                currentTime = DateTime.Now;
                TimeSpan totalTime = endTime.Subtract(startTime);     //测试总时长
                Int32 totalDuartion = (Int32)(totalTime.TotalSeconds);     //测试总秒数
                DateTime meDateWake = (meDateStart == DateTime.MinValue) ? DateTime.MinValue : meDateStart.AddSeconds(-40);        //提前40s唤醒设备

                //串口发送
                if (mePort.IsOpen == true)
                {
                    meTXD[num++] = meActiveAddr;           //设备地址
                    meTXD[num++] = Constants.SET_REG;       //设置寄存器
                    meTXD[num++] = Constants.REG_JOBSET;  //日历寄存器地址
                    meTXD[num++] = Constants.LEN_SET_JOBSET;  //数据长度

                    /*
                    meTXD[num++] = Convert.ToByte(wakeTime.ToString("yy"));  //年
                    meTXD[num++] = (Byte)(wakeTime.Month);
                    meTXD[num++] = (Byte)(wakeTime.Day);
                    meTXD[num++] = (Byte)(wakeTime.Hour);
                    meTXD[num++] = (Byte)(wakeTime.Minute);
                    meTXD[num++] = (Byte)(wakeTime.Second);

                    meTXD[num++] = Convert.ToByte(startTime.ToString("yy"));  //年
                    meTXD[num++] = (Byte)(startTime.Month);
                    meTXD[num++] = (Byte)(startTime.Day);
                    meTXD[num++] = (Byte)(startTime.Hour);
                    meTXD[num++] = (Byte)(startTime.Minute);
                    meTXD[num++] = (Byte)(startTime.Second);

                    meTXD[num++] = Convert.ToByte(endTime.ToString("yy"));  //年
                    meTXD[num++] = (Byte)(endTime.Month);
                    meTXD[num++] = (Byte)(endTime.Day);
                    meTXD[num++] = (Byte)(endTime.Hour);
                    meTXD[num++] = (Byte)(endTime.Minute);
                    meTXD[num++] = (Byte)(endTime.Second);
                    */

                    //十六进制转十进制
                    meTXD[num++] = Convert.ToByte(meDateWake.ToString("yy"), 16);  //年
                    meTXD[num++] = Convert.ToByte(meDateWake.Month.ToString(), 16); //月
                    meTXD[num++] = Convert.ToByte(meDateWake.Day.ToString(), 16);  //日
                    meTXD[num++] = Convert.ToByte(meDateWake.Hour.ToString(), 16);
                    meTXD[num++] = Convert.ToByte(meDateWake.Minute.ToString(), 16);
                    meTXD[num++] = Convert.ToByte(meDateWake.Second.ToString(), 16);

                    meTXD[num++] = Convert.ToByte(startTime.ToString("yy"), 16);  //年
                    meTXD[num++] = Convert.ToByte(startTime.Month.ToString(), 16); //月
                    meTXD[num++] = Convert.ToByte(startTime.Day.ToString(), 16);  //日
                    meTXD[num++] = Convert.ToByte(startTime.Hour.ToString(), 16);
                    meTXD[num++] = Convert.ToByte(startTime.Minute.ToString(), 16);
                    meTXD[num++] = Convert.ToByte(startTime.Second.ToString(), 16);

                    meTXD[num++] = Convert.ToByte(endTime.ToString("yy"), 16);  //年
                    meTXD[num++] = Convert.ToByte(endTime.Month.ToString(), 16); //月
                    meTXD[num++] = Convert.ToByte(endTime.Day.ToString(), 16);  //日
                    meTXD[num++] = Convert.ToByte(endTime.Hour.ToString(), 16);
                    meTXD[num++] = Convert.ToByte(endTime.Minute.ToString(), 16);
                    meTXD[num++] = Convert.ToByte(endTime.Second.ToString(), 16);

                    meTXD[num++] = (Byte)meUnit;            //间隔单位：秒
                    meTXD[num++] = (Byte)testInterval;  //间隔时间低八位
                    meTXD[num++] = (Byte)(testInterval >> 8);   //间隔时间高八位

                    meTXD[num++] = (Byte)totalDuartion;
                    meTXD[num++] = (Byte)(totalDuartion >> 8);
                    meTXD[num++] = (Byte)(totalDuartion >> 16);
                    meTXD[num++] = (Byte)(totalDuartion >> 24);

                    UInt16 myCRC = CRC16(meTXD, num);    //CRC校验
                    meTXD[num++] = (System.Byte)(myCRC >> 8);       //CRC高八位
                    meTXD[num++] = (System.Byte)(myCRC & 0x00FF);   //CRC低八位
                                                                    //MessageBox.Show(num .ToString ());
                                                                    //MessageBox.Show(meTXD[29].ToString ("X") + meTXD[30].ToString ("X"));
                    mePort.Write(meTXD, 0, num);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }

        #endregion

        #region 清空JOBREC寄存器

        //发送--清空JOBREC寄存器
        public bool mePort_Send_SetJobrec()
        {
            try
            {
                Byte num = 0;

                //串口发送
                if (mePort.IsOpen == true)
                {
                    meTXD[num++] = meActiveAddr;           //设备地址
                    meTXD[num++] = Constants.SET_REG;       //设置寄存器
                    meTXD[num++] = Constants.REG_JOBREC;  //JOBREC寄存器地址
                    meTXD[num++] = Constants.LEN_SET_JOBREC;  //数据长度

                    meTXD[num++] = 0x55;
                    meTXD[num++] = 0xAA;

                    UInt16 myCRC = CRC16(meTXD, num);    //CRC校验
                    meTXD[num++] = (System.Byte)(myCRC >> 8);       //CRC高八位
                    meTXD[num++] = (System.Byte)(myCRC & 0x00FF);   //CRC低八位
                                                                    //MessageBox.Show(meTXD[10].ToString ("X") + meTXD[11].ToString ("X"));
                    mePort.Write(meTXD, 0, num);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }

        #endregion

        #region 写REG_BAT_CL寄存器

        //发送--写REG_BAT_CL寄存器
        public bool mePort_Send_SetREG_BAT_CL()
        {
            try
            {
                Byte num = 0;

                //串口发送
                if (mePort.IsOpen == true)
                {
                    //rtCOM = RTCOM.COM_SET_JOBREC;
                    meTXD[num++] = meActiveAddr;           //设备地址
                    meTXD[num++] = Constants.SET_REG;       //设置寄存器
                    meTXD[num++] = Constants.REG_BAT_CL;    //REG_BAT_CL寄存器地址
                    meTXD[num++] = Constants.LEN_SET_BAT_CL;  //数据长度

                    //meTXD[num++] = 0x00;
                    //meTXD[num++] = 0x00;
                    //meTXD[num++] = 0x00;
                    //meTXD[num++] = 0x00;

                    UInt16 myCRC = CRC16(meTXD, num);    //CRC校验
                    meTXD[num++] = (System.Byte)(myCRC >> 8);       //CRC高八位
                    meTXD[num++] = (System.Byte)(myCRC & 0x00FF);   //CRC低八位
                                                                    //MessageBox.Show(meTXD[10].ToString ("X") + meTXD[11].ToString ("X"));
                    mePort.Write(meTXD, 0, num);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }

        #endregion

        #region 写REG_CTEMP_CL寄存器

        //发送--写REG_CTEMP_CL寄存器
        public bool mePort_Send_SetREG_CTEMP_CL()
        {
            try
            {
                Byte num = 0;

                //串口发送
                if (mePort.IsOpen == true)
                {
                    //rtCOM = RTCOM.COM_SET_JOBREC;
                    meTXD[num++] = meActiveAddr;           //设备地址
                    meTXD[num++] = Constants.SET_REG;       //设置寄存器
                    meTXD[num++] = Constants.REG_CTEMP_CL;  //REG_BAT_CL寄存器地址
                    meTXD[num++] = Constants.LEN_SET_CTEMP_CL;  //数据长度

                    //meTXD[num++] = 0x00;
                    //meTXD[num++] = 0x00;
                    //meTXD[num++] = 0x00;
                    //meTXD[num++] = 0x00;

                    UInt16 myCRC = CRC16(meTXD, num);    //CRC校验
                    meTXD[num++] = (System.Byte)(myCRC >> 8);       //CRC高八位
                    meTXD[num++] = (System.Byte)(myCRC & 0x00FF);   //CRC低八位
                                                                    //MessageBox.Show(meTXD[10].ToString ("X") + meTXD[11].ToString ("X"));
                    mePort.Write(meTXD, 0, num);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }

        #endregion

        #region 写REG_UNIT
        //发送--写REG_BAT_CL寄存器
        public bool mePort_Send_SetREG_UNIT()
        {
            try
            {
                Byte num = 0;

                //串口发送
                if (mePort.IsOpen == true)
                {
                    //rtCOM = RTCOM.COM_SET_JOBREC;
                    meTXD[num++] = meActiveAddr;           //设备地址
                    meTXD[num++] = Constants.SET_REG;       //设置寄存器
                    meTXD[num++] = Constants.REG_UNIT;    //REG_UNIT寄存器地址
                    meTXD[num++] = Constants.LEN_SET_UNIT_CL;  //数据长度
                    meTXD[num++] = 0X00;

                    UInt16 myCRC = CRC16(meTXD, num);    //CRC校验
                    meTXD[num++] = (System.Byte)(myCRC >> 8);       //CRC高八位
                    meTXD[num++] = (System.Byte)(myCRC & 0x00FF);   //CRC低八位
                                                                    //MessageBox.Show(meTXD[10].ToString ("X") + meTXD[11].ToString ("X"));
                    mePort.Write(meTXD, 0, num);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }
        #endregion

        #region 写REG_UNIT
        //发送--写REG_BAT_CL寄存器
        public bool mePort_Send_SetTIME_CONDENSE()
        {
            try
            {
                Byte num = 0;

                //串口发送
                if (mePort.IsOpen == true)
                {
                    //rtCOM = RTCOM.COM_SET_JOBREC;
                    meTXD[num++] = meActiveAddr;           //设备地址
                    meTXD[num++] = Constants.SET_REG;       //设置寄存器
                    meTXD[num++] = Constants.REG_TIME_CONDENSE;    //REG_UNIT寄存器地址
                    meTXD[num++] = 0x01;  //数据长度
                    meTXD[num++] = condenseTime;

                    UInt16 myCRC = CRC16(meTXD, num);    //CRC校验
                    meTXD[num++] = (System.Byte)(myCRC >> 8);       //CRC高八位
                    meTXD[num++] = (System.Byte)(myCRC & 0x00FF);   //CRC低八位
                                                                    //MessageBox.Show(meTXD[10].ToString ("X") + meTXD[11].ToString ("X"));
                    mePort.Write(meTXD, 0, num);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }
        #endregion

        #region 设置JSN寄存器

        //发送--设置JSN寄存器
        public bool mePort_Send_SetJSN(string myJSN)
        {
            try
            {
                Byte num = 0;
                Byte[] arrASCII = System.Text.Encoding.Default.GetBytes(myJSN);
                //Array.Resize<Byte>(ref arrASCII, Constants.LEN_SET_JSN);  //重新定义数组长度，并保留其原有值
                //if (myJSN == null) arrASCII = new Byte[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                //串口发送
                if (mePort.IsOpen == true)
                {
                    meTXD[num++] = meActiveAddr;           //设备地址
                    meTXD[num++] = Constants.SET_REG;       //设置寄存器
                    meTXD[num++] = Constants.REG_JSN;  //JOBREC寄存器地址
                    meTXD[num++] = (Byte)arrASCII.Length;  //数据长度

                    for (int i = 0; i < arrASCII.Length; i++)
                    {
                        meTXD[num++] = arrASCII[i];
                    }

                    UInt16 myCRC = CRC16(meTXD, num);    //CRC校验
                    meTXD[num++] = (System.Byte)(myCRC >> 8);       //CRC高八位
                    meTXD[num++] = (System.Byte)(myCRC & 0x00FF);   //CRC低八位
                                                                    //MessageBox.Show(meTXD[10].ToString ("X") + meTXD[11].ToString ("X"));
                    mePort.Write(meTXD, 0, num);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }

        #endregion

        #region 设置USN寄存器

        //发送--设置USN寄存器
        public bool mePort_Send_SetUSN(string myUSN)
        {
            try
            {
                Byte num = 0;
                Byte[] arrASCII = System.Text.Encoding.Default.GetBytes(myUSN);
                //Array.Resize<Byte>(ref arrASCII, Constants.LEN_SET_USN);  //重新定义数组长度，并保留其原有值
                //if (myUSN == null) arrASCII = new Byte[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                //串口发送
                if (mePort.IsOpen == true)
                {
                    meTXD[num++] = meActiveAddr;            //设备地址
                    meTXD[num++] = Constants.SET_REG;       //设置寄存器
                    meTXD[num++] = Constants.REG_USN;       //USN寄存器地址
                    meTXD[num++] = (Byte)arrASCII.Length;   //数据长度

                    for (int i = 0; i < arrASCII.Length; i++)
                    {
                        meTXD[num++] = arrASCII[i];
                    }

                    UInt16 myCRC = CRC16(meTXD, num);      //CRC校验
                    meTXD[num++] = (System.Byte)(myCRC >> 8);       //CRC高八位
                    meTXD[num++] = (System.Byte)(myCRC & 0x00FF);   //CRC低八位
                                                                    //MessageBox.Show(meTXD[10].ToString ("X") + meTXD[11].ToString ("X"));
                    mePort.Write(meTXD, 0, num);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }

        #endregion

        #region 设置UTXT寄存器

        //发送--设置UTXT寄存器
        public bool mePort_Send_SetUTXT(string myUTXT)
        {
            try
            {
                Byte num = 0;
                Byte[] arrASCII = System.Text.Encoding.GetEncoding("GBK").GetBytes(myUTXT);
                //Array.Resize<Byte>(ref arrASCII, Constants.LEN_SET_UTXT);  //重新定义数组长度，并保留其原有值
                //if (myUTXT == null) arrASCII = new Byte[1] { 0 };

                //串口发送
                if (mePort.IsOpen == true)
                {
                    meTXD[num++] = meActiveAddr;           //设备地址
                    meTXD[num++] = Constants.SET_REG;       //设置寄存器
                    meTXD[num++] = Constants.REG_UTXT;      //UTXT寄存器地址
                    meTXD[num++] = (Byte)arrASCII.Length;   //数据长度
                                                            //MessageBox.Show(arrASCII.Length.ToString());

                    for (int i = 0; i < arrASCII.Length; i++)
                    {
                        meTXD[num++] = arrASCII[i];
                    }

                    UInt16 myCRC = CRC16(meTXD, num);    //CRC校验
                    meTXD[num++] = (System.Byte)(myCRC >> 8);       //CRC高八位
                    meTXD[num++] = (System.Byte)(myCRC & 0x00FF);   //CRC低八位
                                                                    //MessageBox.Show(meTXD[10].ToString ("X") + meTXD[11].ToString ("X"));
                    mePort.Write(meTXD, 0, num);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }

        #endregion

        #region 设置DOT寄存器

        //发送--设置DOT寄存器
        //HTT: calNum = 8,标定8组数据；HTP：calNum = 3,标定3组数据
        public bool mePort_Send_SetDOT(int calNum = 8)
        {
            try
            {
                Byte num = 0;

                //串口发送
                if (mePort.IsOpen == true)
                {
                    meTXD[num++] = meActiveAddr;           //设备地址
                    meTXD[num++] = Constants.SET_REG;       //设置寄存器
                    meTXD[num++] = Constants.REG_DOT;       //DOT寄存器地址
                    if (calNum == 3)                          //数据长度
                    {
                        meTXD[num++] = Constants.LEN_READ_DOT2;
                        for (int i = 0; i < calNum; i++)
                        {
                            //标定压力值
                            meTXD[num++] = (Byte)meTemp_CalPoints[i];            //低八位
                            meTXD[num++] = (Byte)(meTemp_CalPoints[i] >> 8);     //高八位
                            meTXD[num++] = (Byte)(meTemp_CalPoints[i] >> 16);
                            meTXD[num++] = (Byte)(meTemp_CalPoints[i] >> 24);

                            //标定ADC
                            meTXD[num++] = (Byte)meADC_CalPoints[i];             //低八位
                            meTXD[num++] = (Byte)(meADC_CalPoints[i] >> 8);
                            meTXD[num++] = (Byte)(meADC_CalPoints[i] >> 16);     //高八位
                        }
                    }
                    else if (calNum == 16)
                    {
                        meTXD[num++] = Constants.LEN_READ_DOT3;

                        for (int i = 0; i < 8; i++)
                        {
                            //标定温度值或标定压力值
                            meTXD[num++] = (Byte)meTemp_CalPoints[i];            //低八位
                            meTXD[num++] = (Byte)(meTemp_CalPoints[i] >> 8);     //高八位

                            //标定ADC
                            meTXD[num++] = (Byte)meADC_CalPoints[i];             //低八位
                            meTXD[num++] = (Byte)(meADC_CalPoints[i] >> 8);
                        }

                        for (int i = 0; i < 8; i++)
                        {
                            //标定温度值或标定压力值
                            meTXD[num++] = (Byte)meHum_CalPoints[i];            //低八位
                            meTXD[num++] = (Byte)(meHum_CalPoints[i] >> 8);     //高八位

                            //标定ADC
                            meTXD[num++] = (Byte)meADC1_CalPoints[i];             //低八位
                            meTXD[num++] = (Byte)(meADC1_CalPoints[i] >> 8);
                        }
                    }
                    else
                    {
                        meTXD[num++] = Constants.LEN_SET_DOT;

                        for (int i = 0; i < calNum; i++)
                        {
                            //标定温度值或标定压力值
                            meTXD[num++] = (Byte)meTemp_CalPoints[i];            //低八位
                            meTXD[num++] = (Byte)(meTemp_CalPoints[i] >> 8);     //高八位

                            //标定ADC
                            meTXD[num++] = (Byte)meADC_CalPoints[i];             //低八位
                            meTXD[num++] = (Byte)(meADC_CalPoints[i] >> 8);
                            meTXD[num++] = (Byte)(meADC_CalPoints[i] >> 16);     //高八位
                        }
                    }

                    UInt16 myCRC = CRC16(meTXD, num);    //CRC校验
                    meTXD[num++] = (Byte)(myCRC >> 8);       //CRC高八位
                    meTXD[num++] = (Byte)(myCRC);   //CRC低八位
                                                    //MessageBox.Show(meTXD[10].ToString ("X") + meTXD[11].ToString ("X"));
                    mePort.Write(meTXD, 0, num);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }

        #endregion

        #region 设置数据修正
        //发送--修正的数据
        public bool mePort_Send_SetData(byte length)
        {
            try
            {
                Byte num = 0;
                int len = 0;
                //串口发送
                if (mePort.IsOpen == true)
                {
                    meTXD[num++] = meActiveAddr;           //设备地址
                    meTXD[num++] = Constants.SET_REG;       //设置寄存器
                    meTXD[num++] = Constants.REG_COR_DATE;       //DOT寄存器地址
                    meTXD[num++] = length;          //数据长度

                    if (length == 0x21)
                    {
                        meTXD[num++] = Convert.ToByte(tempCount / 2);
                        len = 16;
                    }
                    else
                    {
                        meTXD[num++] = Convert.ToByte(tempCount / 2);
                        meTXD[num++] = Convert.ToByte(humCount / 2);
                        len = 32;
                    }
                    for (int i = 0; i < len; i++)
                    {
                        //修正前数值
                        meTXD[num++] = (Byte)meData_Cor[i];            //低八位
                        meTXD[num++] = (Byte)(meData_Cor[i] >> 8);     //高八位
                    }

                    UInt16 myCRC = CRC16(meTXD, num);    //CRC校验
                    meTXD[num++] = (Byte)(myCRC >> 8);       //CRC高八位
                    meTXD[num++] = (Byte)(myCRC);   //CRC低八位
                                                    //MessageBox.Show(meTXD[10].ToString ("X") + meTXD[11].ToString ("X"));
                    mePort.Write(meTXD, 0, num);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }
        #endregion

        #region 设置熄屏状态
        //发送--修正的数据
        public bool mePort_Send_SetScreenState()
        {
            try
            {
                Byte num = 0;

                //串口发送
                if (mePort.IsOpen == true)
                {
                    //rtCOM = RTCOM.COM_SET_JOBREC;
                    meTXD[num++] = meActiveAddr;           //设备地址
                    meTXD[num++] = Constants.SET_REG;       //设置寄存器
                    meTXD[num++] = Constants.REG_SCREEN_STATE;    //REG_SCREEN_STATE寄存器地址
                    meTXD[num++] = Constants.LEN_READ_SCREEN;  //数据长度
                    meTXD[num++] = (byte)(meScreenStatus ? 0 : 1);
                    meTXD[num++] = 0x00;
                    meTXD[num++] = 0x00;
                    meTXD[num++] = 0x00;
                    meTXD[num++] = 0x00;
                    meTXD[num++] = 0x00;
                    meTXD[num++] = 0x00;
                    meTXD[num++] = 0x00;

                    UInt16 myCRC = CRC16(meTXD, num);    //CRC校验
                    meTXD[num++] = (System.Byte)(myCRC >> 8);       //CRC高八位
                    meTXD[num++] = (System.Byte)(myCRC & 0x00FF);   //CRC低八位
                                                                    //MessageBox.Show(meTXD[10].ToString ("X") + meTXD[11].ToString ("X"));
                    mePort.Write(meTXD, 0, num);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }
        #endregion

        #endregion

        #region 发送寄存器读取指令

        //发送
        public bool mePort_Send_ReadIDN(Byte myActiveAddr)
        {
            try
            {
                Byte num = 0;

                //串口发送
                if (mePort.IsOpen == true)
                {
                    meTXD[num++] = myActiveAddr;                     //设备地址
                    meTXD[num++] = Constants.READ_REG;               //读寄存器
                    meTXD[num++] = Constants.REG_DEVICE;             //寄存器地址
                    meTXD[num++] = Constants.LEN_READ_DEVICE;        //读取数据长度

                    UInt16 myCRC = CRC16(meTXD, num);       //CRC校验
                    meTXD[num++] = (System.Byte)(myCRC >> 8);       //CRC高八位
                    meTXD[num++] = (System.Byte)(myCRC & 0x00FF);   //CRC低八位
                    mePort.Write(meTXD, 0, num);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }

        //发送
        public bool mePort_Send_ReadREG(RTCOM myRTCOM, Byte REG_Address, Byte REG_DataLen)
        {
            try
            {
                Byte num = 0;

                //串口发送
                if (mePort.IsOpen == true)
                {
                    meTXD[num++] = meActiveAddr;           //设备地址
                    meTXD[num++] = Constants.READ_REG;      //读寄存器
                    meTXD[num++] = REG_Address;             //寄存器地址
                    meTXD[num++] = REG_DataLen;             //读取数据长度

                    UInt16 myCRC = CRC16(meTXD, num);       //CRC校验
                    meTXD[num++] = (System.Byte)(myCRC >> 8);       //CRC高八位
                    meTXD[num++] = (System.Byte)(myCRC & 0x00FF);   //CRC低八位
                    mePort.Write(meTXD, 0, num);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }

        //发送
        public bool mePort_Send_ReadJOBREC(UInt32 address)
        {
            try
            {
                Byte num = 0;

                //串口发送
                if (mePort.IsOpen == true)
                {
                    meTXD[num++] = meActiveAddr;           //设备地址
                    meTXD[num++] = Constants.READ_DATA;     //读寄存器
                    meTXD[num++] = Constants.REG_JOBREC;    //寄存器地址
                    meTXD[num++] = (Byte)Constants.REC_ONEPAGE;   //读取数据长度(固定读取一页128字节，但是返回的不一定是128字节数据)

                    meTXD[num++] = (Byte)address;            //地址低八位
                    meTXD[num++] = (Byte)(address >> 8);     //地址
                    meTXD[num++] = (Byte)(address >> 16);    //地址
                    meTXD[num++] = (Byte)(address >> 24);    //地址高八位

                    UInt16 myCRC = CRC16(meTXD, num);    //CRC校验
                    meTXD[num++] = (System.Byte)(myCRC >> 8);       //CRC高八位
                    meTXD[num++] = (System.Byte)(myCRC & 0x00FF);   //CRC低八位
                    mePort.Write(meTXD, 0, num);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }

        //发送
        public bool mePort_Send_ReadJobrec(UInt32 address, UInt32 dataLen)
        {
            try
            {
                Byte num = 0;

                //串口发送
                if (mePort.IsOpen == true)
                {
                    meTXD[num++] = meActiveAddr;           //设备地址
                    meTXD[num++] = Constants.READ_DATA;     //读寄存器
                    meTXD[num++] = Constants.REG_JOBREC;    //寄存器地址
                    meTXD[num++] = (Byte)dataLen;           //读取数据长度

                    meTXD[num++] = (System.Byte)address;            //地址低八位
                    meTXD[num++] = (System.Byte)(address >> 8);     //地址
                    meTXD[num++] = (System.Byte)(address >> 16);    //地址
                    meTXD[num++] = (System.Byte)(address >> 24);    //地址高八位

                    UInt16 myCRC = CRC16(meTXD, num);    //CRC校验
                    meTXD[num++] = (System.Byte)(myCRC >> 8);       //CRC高八位
                    meTXD[num++] = (System.Byte)(myCRC & 0x00FF);   //CRC低八位
                    mePort.Write(meTXD, 0, num);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }

        #endregion

        #endregion

        #region DataReceived串口自动接收

        //接收触发函数,实际会由串口线程创建
        private void mePort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            meTips += "mePort_DataReceived event." + Environment.NewLine;
            if (meAutoReceive == false)    //禁止自动接收
            {
                meFirstByte = true;   //已读取到串口数据，用于手动接收
                return;
            }
        }

        #endregion

        #region CRC校验

        public Boolean checkCRC16(byte[] RXData)
        {
            try
            {
                UInt16 myCRC = CRC16(RXData, RXData.Length - 2);            //计算得到的CRC
                UInt16 rxCRC = (UInt16)(RXData[RXData.Length - 2] << 8 | RXData[RXData.Length - 1]);    //接收到的CRC校验字节
                if (rxCRC != myCRC)
                {
                    meTips += "CRC校验失败!" + Environment.NewLine;
                    meTips += "myCRC:" + myCRC.ToString("X2") + Environment.NewLine;
                    meTips += "rxCRC:" + rxCRC.ToString("X2") + Environment.NewLine;
                    return false;
                }

                meTips += "CRC校验成功!" + Environment.NewLine;
                return true;
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }

        #region CRC计算

        /// <summary>
        /// CRC校验
        /// </summary>
        /// <param name="data">校验的字节数组</param>
        /// <param name="length">校验的数组长度</param>
        /// <returns>该字节数组的奇偶校验字节</returns>
        public UInt16 CRC16(byte[] data, int arrayLength)
        {
            byte CRCHigh = 0xFF;
            byte CRCLow = 0xFF;
            byte index;
            int i = 0;
            while (arrayLength-- > 0)
            {
                index = (System.Byte)(CRCHigh ^ data[i++]);
                CRCHigh = (System.Byte)(CRCLow ^ ArrayCRCHigh[index]);
                CRCLow = checkCRCLow[index];
            }

            return (UInt16)(CRCHigh << 8 | CRCLow);
        }

        /// <summary>
        /// CRC校验
        /// </summary>
        /// <param name="data">校验的字节数组</param>
        /// <param name="length">校验的数组长度</param>
        /// <returns>该字节数组的奇偶校验字节</returns>
        public UInt16 CRC16(byte[] data, int startIdx, int arrayLength)
        {
            byte CRCHigh = 0xFF;
            byte CRCLow = 0xFF;
            byte index;
            int i = startIdx;
            //meTips = "";
            while (arrayLength-- > 0)
            {
                //meTips += data[i].ToString("X2") + " ";
                if (i >= SZ.RxSize) i -= SZ.RxSize;  //索引超出数组边界
                index = (System.Byte)(CRCHigh ^ data[i++]);
                CRCHigh = (System.Byte)(CRCLow ^ ArrayCRCHigh[index]);
                CRCLow = checkCRCLow[index];
            }
            //MessageBox.Show( meTips.ToString());

            return (UInt16)(CRCHigh << 8 | CRCLow);
        }

        /// <summary>
        /// CRC高位校验码checkCRCHigh
        /// </summary>
        static byte[] ArrayCRCHigh =
        {
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
        0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
        0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
        0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
        0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
        0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
        0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
        0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
        0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
        0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
        0x80, 0x41, 0x00, 0xC1, 0x81, 0x40
        };

        /// <summary>
        /// CRC地位校验码checkCRCLow
        /// </summary>
        static byte[] checkCRCLow =
        {
        0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06,
        0x07, 0xC7, 0x05, 0xC5, 0xC4, 0x04, 0xCC, 0x0C, 0x0D, 0xCD,
        0x0F, 0xCF, 0xCE, 0x0E, 0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09,
        0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9, 0x1B, 0xDB, 0xDA, 0x1A,
        0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC, 0x14, 0xD4,
        0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
        0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3,
        0xF2, 0x32, 0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4,
        0x3C, 0xFC, 0xFD, 0x3D, 0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A,
        0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38, 0x28, 0xE8, 0xE9, 0x29,
        0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF, 0x2D, 0xED,
        0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
        0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60,
        0x61, 0xA1, 0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67,
        0xA5, 0x65, 0x64, 0xA4, 0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F,
        0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 0x69, 0xA9, 0xA8, 0x68,
        0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA, 0xBE, 0x7E,
        0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
        0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71,
        0x70, 0xB0, 0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92,
        0x96, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C,
        0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E, 0x5A, 0x9A, 0x9B, 0x5B,
        0x99, 0x59, 0x58, 0x98, 0x88, 0x48, 0x49, 0x89, 0x4B, 0x8B,
        0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
        0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42,
        0x43, 0x83, 0x41, 0x81, 0x80, 0x40
        };

        #endregion

        #endregion

        #region 检测设备当前状态 -- 未连接、工作中、空闲

        public Boolean checkDeviceStatus()
        {
            if (meTask == TASKS.disconnected)
            {
                MessageBox.Show("设备尚未连接！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.DoEvents();
                return false;
            }

            if (meTask == TASKS.setting || meTask == TASKS.reading)
            {
                MessageBox.Show("设备繁忙，请稍后再试！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.DoEvents();
                return false;
            }

            return true;
        }

        #endregion

        #region 检测数据文件是否加载

        public Boolean checkDataFileStatus()
        {
            if (meTypeList.Count == 0)
            {
                MessageBox.Show("尚未加载数据！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        #endregion

        #region 检测数据文件数量与当前连接产品数量是否一致

        public Boolean checkDataFileNumber()
        {
            if (MyDefine.myXET.meDUTAddrArr.Count != MyDefine.myXET.meDUTNum)
            {
                MessageBox.Show("加载文件数量与当前设备数量不符！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        #endregion

        #region 检测数据文件类型是否唯一

        public Boolean checkDataFileType(Boolean showmsg = true)
        {
            int typenum = 0;
            if (meTypeList.Count == 0) return true;
            if (meTypeList.Contains("TT_T")) typenum++;
            if (meTypeList.Contains("TH_T")) typenum++;
            if (meTypeList.Contains("TP_P")) typenum++;
            if (meTypeList.Contains("TQ_T")) typenum++;
            if (typenum > 1)
            {
                //MessageBox.Show("原始数据类型不一致，请重新加载原始数据", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (showmsg) MessageBox.Show("原始数据类型不一致，包含温度、湿度、压力类型中的两种或以上！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        #endregion

        #region 等待串口数据接收(带超时检测)

        /// <summary>
        /// 等待串口读取(超时时间)
        /// </summary>
        /// <param name="milliseconds">超时时间</param>
        /// <returns></returns>
        public Boolean WaitFirstByteWithTimeout(int milliseconds)
        {
            try
            {
                Boolean ret = false;
                long startTicks = DateTime.Now.Ticks;
                long endTicks = DateTime.Now.Ticks;
                long runTicks = (endTicks - startTicks) / 10000;

                meFirstByte = false;  //全局变量，串口自动接收函数触发后置位
                while (meFirstByte == false && runTicks <= milliseconds)
                {
                    endTicks = DateTime.Now.Ticks;
                    runTicks = (endTicks - startTicks) / 10000;
                    System.Threading.Thread.Sleep(1);
                    //Application.DoEvents();
                }

                meTips += "等待第一个字节：" + runTicks.ToString() + "ms" + Environment.NewLine;
                if (meFirstByte == true) ret = true;
                meFirstByte = false;

                return ret;
            }
            catch (Exception ex)
            {
                //捕获异常
                meTips += "ReadFirstByte timeout:" + ex.ToString() + Environment.NewLine;
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }

        /// <summary>
        /// 等待串口接收到足够字节
        /// </summary>
        /// <param name="len">等待接收的字节数</param>
        /// <param name="milliseconds">超时时间ms</param>
        /// <returns>串口已接收到的字节数</returns>
        public int WaitDataReadyWithTimeout(int myLen, int milliseconds)
        {
            try
            {
                int byteNum = 0;
                long startTicks = DateTime.Now.Ticks;
                long endTicks = DateTime.Now.Ticks;
                long runTicks = (endTicks - startTicks) / 10000;

                while (byteNum < myLen && runTicks <= milliseconds)   //数据长度不足且未超时则继续等待
                {
                    byteNum = mePort.BytesToRead;
                    endTicks = DateTime.Now.Ticks;
                    runTicks = (endTicks - startTicks) / 10000;
                    System.Threading.Thread.Sleep(1);
                    //Application.DoEvents();
                }

                meTips += runTicks.ToString() + "ms" + Environment.NewLine;
                return byteNum;
            }
            catch (Exception ex)
            {
                //捕获异常
                meTips += "WaitDataReady timeout:" + ex.ToString() + Environment.NewLine;
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return 0;
            }
        }

        /// <summary>
        /// 接收下位机应答指令
        /// </summary>
        /// <param name="rxData">应答指令储存数组（地址传递）</param>
        /// <returns></returns>
        public Boolean ReadACKCommandWithTimeout(ref Byte[] rxData, int timeout = 300)
        {
            try
            {
                meTips += "Send command..." + Environment.NewLine;
                MyDefine.myXET.SaveLogTips();  //===========
                Boolean rec = WaitFirstByteWithTimeout(timeout);//等待串口接收第一个应答字节(串口自动接收函数触发)
                if (rec == false)       //未接收到回复，读取应答指令失败
                {
                    //MessageBox.Show("未接收到回复，读取应答指令失败");
                    meTips += "Timeout：no data received." + Environment.NewLine;
                    return false;
                }

                int readNum = mePort.BytesToRead;           //读取当前已接收到的字节数
                meTips += "接收字节数=" + readNum.ToString() + Environment.NewLine;
                MyDefine.myXET.SaveLogTips();  //===========

                if (readNum < 3)
                {
                    meTips += "等待前3字节就绪：";
                    WaitDataReadyWithTimeout(3, timeout);     //等待前3个字节就绪，超时时间100ms
                    readNum = mePort.BytesToRead;           //读取当前已接收到的字节数
                    meTips += "接收字节数=" + readNum.ToString() + Environment.NewLine;
                    MyDefine.myXET.SaveLogTips();  //===========
                }

                if (readNum >= 3)                   //读取前三个字节，获取应答指令中的数据长度RXData[2]
                {
                    mePort.Read(rxData, 0, 3);      //读取3个应答字节
                    int ACKLen = rxData[2] + 5;     //获取应答指令总长度
                    Array.Resize<Byte>(ref rxData, ACKLen);  //重新定义数组长度，并保留其原有值
                    meTips += "接收前3个字节：" + rxData[0].ToString("X2") + " " + rxData[1].ToString("X2") + " " + rxData[2].ToString("X2") + " " + Environment.NewLine;
                    MyDefine.myXET.SaveLogTips();  //===========

                    meTips += "应读字节数=" + ACKLen.ToString() + Environment.NewLine;
                    meTips += "等待全部字节就绪：";
                    int recNum = WaitDataReadyWithTimeout(ACKLen - 3, timeout);     //等待其余指令字节就绪，超时时间100ms
                    meTips += "已读字节数=" + (recNum + 3).ToString() + Environment.NewLine;
                    if (recNum < ACKLen - 3)                                    //未接收到足够应答字节，读取应答指令失败
                    {
                        meTips += "timeout：应答字节长度不符，重新发送指令." + Environment.NewLine;
                        return false;
                    }

                    meTips += "读取全部字节：" + Environment.NewLine;
                    MyDefine.myXET.SaveLogTips();  //===========
                    mePort.Read(rxData, 3, ACKLen - 3);                         //读取其余应答字节

                    //显示接收到的数据到调试窗口
                    for (int i = 0; i < rxData.Length; i++)
                    {
                        meTips += rxData[i].ToString("X2") + " ";
                    }
                    meTips += Environment.NewLine;
                    MyDefine.myXET.SaveLogTips();  //===========

                    if (checkCRC16(rxData)) return true;    //CRC校验
                }

                return false;
            }
            catch (Exception ex)
            {
                meTips += "ReadACKCommand timeout:" + ex.ToString() + Environment.NewLine;
                MyDefine.myXET.SaveCommunicationTips();  //===========
                return false;
            }
        }

        #endregion

        #region 16进制转10进制

        private UInt32 getValue(byte b1, byte b2, byte b3, byte b4)
        {
            UInt32 d1 = b1;//低位
            UInt32 d2 = b2;
            UInt32 d3 = b3;
            UInt32 d4 = b4;

            d2 = d2 << 8;
            d3 = d3 << 16;
            d4 = d4 << 24;

            return (d1 + d2 + d3 + d4);
        }

        public UInt32 getValue(byte b1, byte b2)
        {
            UInt32 d1 = b1;//低位
            UInt32 d2 = b2;

            return ((d2 << 8) + d1);
        }

        public Int32 getValue_INT32(byte b1, byte b2, byte b3, byte b4)
        {
            Int32 d1 = b1;//低位
            Int32 d2 = b2;
            Int32 d3 = b3;
            Int32 d4 = b4;

            return ((d4 << 24) + (d3 << 16) + (d2 << 8) + d1);
        }
        #endregion

        #endregion

        #region 文件操作

        #region 账号操作

        #region 保存帐号

        //保存帐号
        public bool SaveToDat()
        {
            //空
            if (userCFG == null)
            {
                return false;
            }
            //创建新路径
            else if (!Directory.Exists(userCFG))
            {
                Directory.CreateDirectory(userCFG);
            }

            //写入
            try
            {
                String mePath = userCFG + @"\user." + userName + ".cfg";
                if (File.Exists(mePath))
                {
                    System.IO.File.SetAttributes(mePath, FileAttributes.Normal);
                }
                FileStream meFS = new FileStream(mePath, FileMode.Create, FileAccess.Write);
                BinaryWriter meWrite = new BinaryWriter(meFS);
                //
                meWrite.Write(userName);
                meWrite.Write(userPassword);
                meWrite.Write(userCFG);
                //
                meWrite.Close();
                meFS.Close();
                System.IO.File.SetAttributes(mePath, FileAttributes.ReadOnly);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region 注销帐号

        //注销帐号
        public bool FileDelete(string myUserName)
        {
            //空
            if (userCFG == null)
            {
                return false;
            }
            //创建新路径
            else if (!Directory.Exists(userCFG))
            {
                Directory.CreateDirectory(userCFG);
            }

            //删除帐户
            try
            {
                String mePath = userCFG + @"\user." + myUserName + ".cfg";
                if (File.Exists(mePath))
                {
                    File.SetAttributes(mePath, FileAttributes.Normal);
                    File.Delete(mePath);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("账户注销异常: " + ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }
        #endregion

        #endregion

        #region 原始数据保存/加载/导出csv

        #region 保存数据文件 -- 保存从设备寄存器读取的数据

        //保存数据 -- 保存从设备寄存器读取的数据
        public bool mem_SaveToLog()
        {
            String jsn = meJSN.Length > 2 ? meJSN.Substring(2) : meJSN;
            String myPath = MyDefine.myXET.userDAT;
            //String DUTname = (meUSN == "") ? jsn : meUSN;//用户自定义编号
            String DUTname = jsn;
            String DUTtype = meType.ToString().Substring(1, 2);             //TT/TH/TP/TQ
            //String DUTcode = DUTtype;     //若出厂编号不包含TT/TH/TP等字符(如为空时)，则显示#TT/#TH/#TP/#
            String DUTcode = meJSN == "" ? DUTtype : meJSN;     //若出厂编号不包含TT/TH/TP等字符(如为空时)，则显示#TT/#TH/#TP/#
            // String DUTcode = jsn.Contains(DUTtype) ? jsn : DUTtype;     //若出厂编号不包含TT/TH/TP等字符(如为空时)，则显示#TT/#TH/#TP/#TQ
            String fileName = meType.ToString() + "." + meUID + "." + DUTname + "." + meDateStart.ToString("yyMMddHHmmss") + ".tmp";
            myPath = myPath + "\\" + fileName;      //文件路径

            if (myPath == null) return false;       //文件路径为空
            //if (myMem.Count == 0) return false;     //已读取数据个数为0

            //写入
            try
            {
                /*
                 if (File.Exists(mePath))
                 {
                     if (MessageBox.Show(System.IO.Path.GetFileName(mePath) + "文件已存在。" + Environment.NewLine + "是否替换？", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel)
                     {
                         return false;
                     }
                     System.IO.File.SetAttributes(mePath, FileAttributes.Normal);
                 }
                 */

                //如果文件存在则直接覆盖
                if (File.Exists(myPath)) System.IO.File.SetAttributes(myPath, FileAttributes.Normal);   //取消只读模式
                FileStream meFS = new FileStream(myPath, FileMode.Create, FileAccess.Write);
                TextWriter meWrite = new StreamWriter(meFS);

                //string msg = "";
                String mStr = String.Empty;
                String strHashSource = string.Empty;                        //要计算哈希值的数据源
                DateTime testDate = meDateStart;                            //测试时间
                UInt32 testSpan = meSpan * meArrUnit[meUnit];               //测试间隔(秒)
                meWrite.WriteLine(";" + System.DateTime.Now.ToString());
                meWrite.WriteLine(";===============================================================");
                meWrite.WriteLine("Time".PadRight(18) + ("#" + MyDefine.myXET.meJSN).PadRight(10));
                //MessageBox.Show("0:" + System.BitConverter.ToString(Encoding.Default.GetBytes(meJSN)) + Environment.NewLine + "1:" + System.BitConverter.ToString(Encoding.Default.GetBytes("JD222")));

                //保存测试数据
                for (int i = 0; i < myMem.Count; i++)
                {
                    for (int k = 0; k < SZ.CHA; k++)
                    {
                        if (i == myMem.Count - 1 && myMem[i].OUT[k] == 0) break; //如果最后一组中出现0，则认为是空数据，不进行保存

                        mStr = testDate.ToString("yy-MM-dd HH:mm:ss");          //记录测试时间
                        switch (meType)
                        {
                            case DEVICE.HTT:    //温度采集器
                                mStr += "," + (myMem[i].OUT[k] / 100.0).ToString("F2");       //记录温度值
                                break;

                            case DEVICE.HTH:    //温湿度采集器
                                mStr += "," + (myMem[i].OUT[k] / 100.0).ToString("F2");       //记录温度值
                                mStr += "," + (myMem[i].HUM[k] / 100.0).ToString("F2");       //记录湿度值
                                break;

                            case DEVICE.HTP:        //压力采集器
                                mStr += "," + (myMem[i].OUT[k] / 100.0).ToString("F2");       //记录压力值
                                break;

                            case DEVICE.HTQ:    //温湿度采集器
                                mStr += "," + (myMem[i].OUT[k] / 100.0).ToString("F2");       //记录温度值
                                mStr += "," + (myMem[i].HUM[k] / 100.0).ToString("F2");       //记录湿度值
                                break;

                            default:
                                break;
                        }

                        //msg += myMem[i].OUT[k].ToString() + " ";
                        testDate = testDate.AddSeconds(testSpan);
                        strHashSource += mStr + Environment.NewLine;                         //将行数据加入哈希值数据源
                        meWrite.WriteLine(mStr);
                    }
                    //msg += Environment.NewLine;
                }

                //MessageBox.Show(msg);

                String mem_date = meDateStart.ToString("yyyy-MM-dd");        //测试日期
                String mem_wake = meDateWake.ToString("yy-MM-dd HH:mm:ss");  //测试唤醒时间
                String mem_start = meDateStart.ToString("yy-MM-dd HH:mm:ss");//测试开始时间(设定值)
                String mem_stop = meDateEnd.ToString("yy-MM-dd HH:mm:ss");   //测试结束时间(设定值)
                String mem_read = DateTime.Now.ToString("yy-MM-dd HH:mm:ss");//数据读取时间
                String mem_duration = meDuration.ToString();                 //测试持续时间(秒)(设定值)
                String mem_Dstart = meDateStart.ToString("yy-MM-dd HH:mm:ss");//测试开始时间(实际值)
                String mem_Dstop = meDateEnd.ToString("yy-MM-dd HH:mm:ss");   //测试结束时间(实际值)
                String mem_Dduration = meDuration.ToString();                 //测试持续时间(秒)(实际值)
                String mem_interval = meSpan.ToString();                      //测试间隔时间(单位：秒、分、时、天)
                String mem_unit = ((UNITNAME)meUnit).ToString();              //测试间隔单位
                String mem_span = testSpan.ToString();                        //测试间隔时间
                String mem_ID = meUID;                                      //设备ID
                String mem_Model = meModel;                                 //设备类型(YZLT30)
                String mem_JSN = DUTcode;                                   //出厂编号(TT20210101,若出厂编号为空则显示TT/TH/TP)
                String mem_Range = meRange;                                 //测量范围(-80~150℃)
                String mem_Cal = meDateCal.ToString("yyyyMMdd");            //设备校准日期
                String mem_Rec = meDateCal.AddYears(1).AddDays(-1).ToString("yyyyMMdd");   //设备复校日期
                String mem_Type = meType.ToString();                        //设备类型(HTT)
                String mem_USN = meUSN;                                     //用户管理编号
                String mem_UTXT = meUTXT;                                   //备注信息
                String mem_Tmax = (meTMax / 100.0).ToString("F2") + "℃";   //内核最高温度
                String mem_Tmin = (meTMin / 100.0).ToString("F2") + "℃";   //内核最低温度
                String mem_Addr = meActiveAddr.ToString("X2");              //当前设备的地址
                String mem_DateHW = meDateHW.ToString("yyyyMMdd");          //硬件制造日期
                String mem_DateBat = meDateBat.ToString("yyyyMMdd");        //更换电池日期
                String mem_DateDot = meDateDot.ToString("yyyyMMdd");        //温度标定日期
                String mem_DateCal = meDateCal.ToString("yyyyMMdd");        //计量校准日期
                String mem_Bat = String.Empty;                              //电池电压


                //计算电池电量
                if (Convert.ToInt32(meSWVer) >= 0x60 && meType == DEVICE.HTT)
                {
                    if (meBat >= 3700) mem_Bat = "100%";
                    else if (meBat <= 2750) mem_Bat = "0%";
                    else mem_Bat = Convert.ToInt32(meBat * (2.0 / 19) - (5500.0 / 19)).ToString() + "%";
                }
                else
                {
                    if (meType == DEVICE.HTQ)
                    {
                        if (meBat <= 200) mem_Bat = "USB供电";
                        else if (meBat <= 3415) mem_Bat = "0%";
                        else if (meBat >= 4095) mem_Bat = "100%";
                        else mem_Bat = Convert.ToInt32(meBat * 0.107 - 338.12).ToString() + "%";
                    }
                    else
                    {
                        if (meBat <= 2290) mem_Bat = "0%";
                        else if (meBat >= 2756) mem_Bat = "100%";
                        else mem_Bat = Convert.ToInt32(meBat * 0.213 - 486.87).ToString() + "%";
                    }
                }

                if (meDateEnd.CompareTo(DateTime.Now) > 0)             //设备还在工作中,以最后一个测试数据都测试时间作为测试结束时间，并计算总测试时间
                {
                    DateTime testingNow = DateTime.Now;
                    //memstop = testingNow.ToString("yy-MM-dd HH:mm:ss");                               //实际测试结束时间
                    mem_Dstop = mStr.Split(',')[0];                                                     //最后一个测试数据的实际测试时间
                    mem_Dduration = ((Int32)(testingNow.Subtract(meDateStart).TotalSeconds)).ToString();//实际测试持续时间(秒)
                }

                /*
                meWrite.WriteLine(";---------------------------------------------------------------");
                meWrite.WriteLine("date=" + memdate);
                meWrite.WriteLine("time=" + memtime);
                meWrite.WriteLine("stop=" + memstop);
                meWrite.WriteLine("span=" + memspan);
                meWrite.WriteLine("duration=" + memrun);
                meWrite.WriteLine("id=" + mem_ID);
                meWrite.WriteLine("model=" + mem_Model);
                meWrite.WriteLine("code=" + mem_Code);
                meWrite.WriteLine("range=" + mem_Range);
                meWrite.WriteLine("cal=" + mem_Cal);
                meWrite.WriteLine("rec=" + mem_Rec);
                meWrite.WriteLine("bat=" + mem_Bat);

                meWrite.WriteLine("wake=" + meDateWake.ToString("yy-MM-dd HH:mm:ss"));
                meWrite.WriteLine("meHWVer=" + meHWVer);
                meWrite.WriteLine("meSWVer=" + meSWVer);
                meWrite.WriteLine("TYPE=" + mem_Type);
                meWrite.WriteLine("ADDR=" + meActiveAddr.ToString("X2"));
                meWrite.WriteLine("USN=" + mem_USN);
                meWrite.WriteLine("maxcore=" + mem_Tmax);
                meWrite.WriteLine("mincore=" + mem_Tmin);
                meWrite.WriteLine("rtc=" + meRTC);
                meWrite.WriteLine("htt=" + meHTT);
                meWrite.WriteLine("wdt=" + meWDT);
                meWrite.WriteLine("luk=" + meLUK);
                meWrite.WriteLine("hash=" + getHashMD5(strHashSource));     //将哈希值写入文件末尾
                */

                meWrite.WriteLine(";---------------------------------------------------------------");
                meWrite.WriteLine(Constants.CON_UID + "=" + mem_ID);
                meWrite.WriteLine(Constants.CON_VERHW + "=" + meHWVer);
                meWrite.WriteLine(Constants.CON_VERSW + "=" + meSWVer);
                meWrite.WriteLine(Constants.CON_TYPE + "=" + mem_Type);
                meWrite.WriteLine(Constants.CON_ADDR + "=" + mem_Addr);
                meWrite.WriteLine(Constants.CON_battery + "=" + mem_Bat);
                meWrite.WriteLine(Constants.CON_maxcore + "=" + mem_Tmax);
                meWrite.WriteLine(Constants.CON_mincore + "=" + mem_Tmin);
                meWrite.WriteLine(Constants.CON_rtc + "=" + meRTC);
                meWrite.WriteLine(Constants.CON_htr + "=" + meHTT);
                meWrite.WriteLine(Constants.CON_wdt + "=" + meWDT);
                meWrite.WriteLine(Constants.CON_luk + "=" + meLUK);
                meWrite.WriteLine(Constants.CON_mantime + "=" + mem_DateHW);
                meWrite.WriteLine(Constants.CON_battime + "=" + mem_DateBat);
                meWrite.WriteLine(Constants.CON_caltime + "=" + mem_DateDot);
                meWrite.WriteLine(Constants.CON_jintime + "=" + mem_DateCal);
                meWrite.WriteLine(Constants.CON_JSN + "=" + mem_JSN);
                meWrite.WriteLine(Constants.CON_USN + "=" + mem_USN);
                meWrite.WriteLine(Constants.CON_UTXT + "=" + mem_UTXT);
                meWrite.WriteLine(Constants.CON_date + "=" + mem_date);
                meWrite.WriteLine(Constants.CON_wake + "=" + mem_wake);
                meWrite.WriteLine(Constants.CON_start + "=" + mem_start);
                meWrite.WriteLine(Constants.CON_stop + "=" + mem_stop);
                meWrite.WriteLine(Constants.CON_read + "=" + mem_read);
                meWrite.WriteLine(Constants.CON_interval + "=" + mem_interval);
                meWrite.WriteLine(Constants.CON_unit + "=" + mem_unit);
                meWrite.WriteLine(Constants.CON_span + "=" + mem_span);
                meWrite.WriteLine(Constants.CON_duration + "=" + mem_duration);
                meWrite.WriteLine(Constants.CON_mode + "=" + mem_Model);
                meWrite.WriteLine(Constants.CON_range + "=" + mem_Range);
                meWrite.WriteLine(Constants.CON_cal + "=" + mem_Cal);
                meWrite.WriteLine(Constants.CON_rec + "=" + mem_Rec);
                meWrite.WriteLine(Constants.CON_Dstart + "=" + mem_Dstart);
                meWrite.WriteLine(Constants.CON_Dstop + "=" + mem_Dstop);
                meWrite.WriteLine(Constants.CON_Dduration + "=" + mem_Dduration);
                meWrite.WriteLine(Constants.CON_hash + "=" + getHashMD5(strHashSource));     //将哈希值写入文件末尾

                //
                meWrite.Close();
                meFS.Close();
                System.IO.File.SetAttributes(myPath, FileAttributes.ReadOnly);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据保存失败: " + ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }

        #endregion

        #region 保存修正的数据文件 -- 保存从设备寄存器读取的数据
        //保存数据 -- 保存从设备寄存器读取的数据
        public bool mem_SaveToCorLog()
        {
            String jsn = meJSN.Length > 2 ? meJSN.Substring(2) : meJSN;
            String myPath = MyDefine.myXET.userDAT;
            String DUTname = (meUSN == "") ? jsn : meUSN;
            String DUTtype = meType.ToString().Substring(1, 2);             //TT/TH/TP/TQ
            String DUTcode = meJSN == "" ? DUTtype : meJSN;     //若出厂编号不包含TT/TH/TP等字符(如为空时)，则显示#TT/#TH/#TP/#
            String fileName = meType.ToString() + "." + meUID + "." + DUTname + "." + meDateStart.ToString("yyMMddHHmmss") + ".Cor" + ".tmp";
            myPath = myPath + "\\" + fileName;      //文件路径

            if (myPath == null) return false;       //文件路径为空

            //写入
            try
            {
                //如果文件存在则直接覆盖
                if (File.Exists(myPath)) System.IO.File.SetAttributes(myPath, FileAttributes.Normal);   //取消只读模式
                FileStream meFS = new FileStream(myPath, FileMode.Create, FileAccess.Write);
                TextWriter meWrite = new StreamWriter(meFS);

                //string msg = "";
                String mStr = String.Empty;
                String strHashSource = string.Empty;                        //要计算哈希值的数据源
                DateTime testDate = meDateStart;                            //测试时间
                UInt32 testSpan = meSpan * meArrUnit[meUnit];               //测试间隔(秒)
                meWrite.WriteLine(";" + System.DateTime.Now.ToString());
                meWrite.WriteLine(";===============================================================");
                meWrite.WriteLine("Time".PadRight(18) + ("#" + MyDefine.myXET.meJSN + ".Cor").PadRight(10));
                //MessageBox.Show("0:" + System.BitConverter.ToString(Encoding.Default.GetBytes(meJSN)) + Environment.NewLine + "1:" + System.BitConverter.ToString(Encoding.Default.GetBytes("JD222")));

                //保存测试数据
                for (int i = 0; i < corMem.Count; i++)
                {
                    for (int k = 0; k < SZ.CHA; k++)
                    {
                        if (i == corMem.Count - 1 && corMem[i].OUT[k] == 0) break; //如果最后一组中出现0，则认为是空数据，不进行保存

                        mStr = testDate.ToString("yy-MM-dd HH:mm:ss");          //记录测试时间
                        switch (meType)
                        {
                            case DEVICE.HTT:    //温度采集器
                                mStr += "," + (corMem[i].OUT[k] / 100.0).ToString("F2");       //记录温度值
                                break;

                            case DEVICE.HTH:    //温湿度采集器
                                mStr += "," + (corMem[i].OUT[k] / 100.0).ToString("F2");       //记录温度值
                                mStr += "," + (corMem[i].HUM[k] / 100.0).ToString("F2");       //记录湿度值
                                break;

                            case DEVICE.HTP:        //压力采集器
                                mStr += "," + (corMem[i].OUT[k] / 100.0).ToString("F2");       //记录压力值
                                break;

                            case DEVICE.HTQ:    //温湿度采集器
                                mStr += "," + (corMem[i].OUT[k] / 100.0).ToString("F2");       //记录温度值
                                mStr += "," + (corMem[i].HUM[k] / 100.0).ToString("F2");       //记录湿度值
                                break;

                            default:
                                break;
                        }

                        //msg += myMem[i].OUT[k].ToString() + " ";
                        testDate = testDate.AddSeconds(testSpan);
                        strHashSource += mStr + Environment.NewLine;                         //将行数据加入哈希值数据源
                        meWrite.WriteLine(mStr);
                    }
                    //msg += Environment.NewLine;
                }

                //MessageBox.Show(msg);

                String mem_date = meDateStart.ToString("yyyy-MM-dd");        //测试日期
                String mem_wake = meDateWake.ToString("yy-MM-dd HH:mm:ss");  //测试唤醒时间
                String mem_start = meDateStart.ToString("yy-MM-dd HH:mm:ss");//测试开始时间(设定值)
                String mem_stop = meDateEnd.ToString("yy-MM-dd HH:mm:ss");   //测试结束时间(设定值)
                String mem_read = DateTime.Now.ToString("yy-MM-dd HH:mm:ss");//数据读取时间
                String mem_duration = meDuration.ToString();                 //测试持续时间(秒)(设定值)
                String mem_Dstart = meDateStart.ToString("yy-MM-dd HH:mm:ss");//测试开始时间(实际值)
                String mem_Dstop = meDateEnd.ToString("yy-MM-dd HH:mm:ss");   //测试结束时间(实际值)
                String mem_Dduration = meDuration.ToString();                 //测试持续时间(秒)(实际值)
                String mem_interval = meSpan.ToString();                      //测试间隔时间(单位：秒、分、时、天)
                String mem_unit = ((UNITNAME)meUnit).ToString();              //测试间隔单位
                String mem_span = testSpan.ToString();                        //测试间隔时间
                String mem_ID = meUID;                                      //设备ID
                String mem_Model = meModel;                                 //设备类型(YZLT30)
                String mem_JSN = DUTcode;                                   //出厂编号(TT20210101,若出厂编号为空则显示TT/TH/TP)
                String mem_Range = meRange;                                 //测量范围(-80~150℃)
                String mem_Cal = meDateCal.ToString("yyyyMMdd");            //设备校准日期
                String mem_Rec = meDateCal.AddYears(1).AddDays(-1).ToString("yyyyMMdd");   //设备复校日期
                String mem_Type = meType.ToString();                        //设备类型(HTT)
                String mem_USN = meUSN;                                     //用户管理编号
                String mem_UTXT = meUTXT;                                   //备注信息
                String mem_Tmax = (meTMax / 100.0).ToString("F2") + "℃";   //内核最高温度
                String mem_Tmin = (meTMin / 100.0).ToString("F2") + "℃";   //内核最低温度
                String mem_Addr = meActiveAddr.ToString("X2");              //当前设备的地址
                String mem_DateHW = meDateHW.ToString("yyyyMMdd");          //硬件制造日期
                String mem_DateBat = meDateBat.ToString("yyyyMMdd");        //更换电池日期
                String mem_DateDot = meDateDot.ToString("yyyyMMdd");        //温度标定日期
                String mem_DateCal = meDateCal.ToString("yyyyMMdd");        //计量校准日期
                String mem_Bat = String.Empty;                              //电池电压


                if (Convert.ToInt32(meSWVer) >= 60 && meType == DEVICE.HTT)
                {
                    if (meBat >= 3700) mem_Bat = "100%";
                    else if (meBat <= 2750) mem_Bat = "0%";
                    else mem_Bat = Convert.ToInt32(meBat * (2.0 / 19) - (5500.0 / 19)).ToString() + "%";
                }
                else
                {
                    if (meType == DEVICE.HTQ)
                    {
                        if (meBat <= 200) mem_Bat = "USB供电";
                        else if (meBat <= 3415) mem_Bat = "0%";
                        else if (meBat >= 4095) mem_Bat = "100%";
                        else mem_Bat = Convert.ToInt32(meBat * 0.147 - 502.01).ToString() + "%";
                    }
                    else
                    {
                        if (meBat <= 2290) mem_Bat = "0%";
                        else if (meBat >= 2756) mem_Bat = "100%";
                        else mem_Bat = Convert.ToInt32(meBat * 0.213 - 486.87).ToString() + "%";
                    }
                }

                if (meDateEnd.CompareTo(DateTime.Now) > 0)             //设备还在工作中,以最后一个测试数据都测试时间作为测试结束时间，并计算总测试时间
                {
                    DateTime testingNow = DateTime.Now;
                    //memstop = testingNow.ToString("yy-MM-dd HH:mm:ss");                               //实际测试结束时间
                    mem_Dstop = mStr.Split(',')[0];                                                     //最后一个测试数据的实际测试时间
                    mem_Dduration = ((Int32)(testingNow.Subtract(meDateStart).TotalSeconds)).ToString();//实际测试持续时间(秒)
                }

                meWrite.WriteLine(";---------------------------------------------------------------");
                meWrite.WriteLine(Constants.CON_UID + "=" + mem_ID);
                meWrite.WriteLine(Constants.CON_VERHW + "=" + meHWVer);
                meWrite.WriteLine(Constants.CON_VERSW + "=" + meSWVer);
                meWrite.WriteLine(Constants.CON_TYPE + "=" + mem_Type);
                meWrite.WriteLine(Constants.CON_ADDR + "=" + mem_Addr);
                meWrite.WriteLine(Constants.CON_battery + "=" + mem_Bat);
                meWrite.WriteLine(Constants.CON_maxcore + "=" + mem_Tmax);
                meWrite.WriteLine(Constants.CON_mincore + "=" + mem_Tmin);
                meWrite.WriteLine(Constants.CON_rtc + "=" + meRTC);
                meWrite.WriteLine(Constants.CON_htr + "=" + meHTT);
                meWrite.WriteLine(Constants.CON_wdt + "=" + meWDT);
                meWrite.WriteLine(Constants.CON_luk + "=" + meLUK);
                meWrite.WriteLine(Constants.CON_mantime + "=" + mem_DateHW);
                meWrite.WriteLine(Constants.CON_battime + "=" + mem_DateBat);
                meWrite.WriteLine(Constants.CON_caltime + "=" + mem_DateDot);
                meWrite.WriteLine(Constants.CON_jintime + "=" + mem_DateCal);
                meWrite.WriteLine(Constants.CON_JSN + "=" + mem_JSN);
                meWrite.WriteLine(Constants.CON_USN + "=" + mem_USN);
                meWrite.WriteLine(Constants.CON_UTXT + "=" + mem_UTXT);
                meWrite.WriteLine(Constants.CON_date + "=" + mem_date);
                meWrite.WriteLine(Constants.CON_wake + "=" + mem_wake);
                meWrite.WriteLine(Constants.CON_start + "=" + mem_start);
                meWrite.WriteLine(Constants.CON_stop + "=" + mem_stop);
                meWrite.WriteLine(Constants.CON_read + "=" + mem_read);
                meWrite.WriteLine(Constants.CON_interval + "=" + mem_interval);
                meWrite.WriteLine(Constants.CON_unit + "=" + mem_unit);
                meWrite.WriteLine(Constants.CON_span + "=" + mem_span);
                meWrite.WriteLine(Constants.CON_duration + "=" + mem_duration);
                meWrite.WriteLine(Constants.CON_mode + "=" + mem_Model);
                meWrite.WriteLine(Constants.CON_range + "=" + mem_Range);
                meWrite.WriteLine(Constants.CON_cal + "=" + mem_Cal);
                meWrite.WriteLine(Constants.CON_rec + "=" + mem_Rec);
                meWrite.WriteLine(Constants.CON_Dstart + "=" + mem_Dstart);
                meWrite.WriteLine(Constants.CON_Dstop + "=" + mem_Dstop);
                meWrite.WriteLine(Constants.CON_Dduration + "=" + mem_Dduration);
                meWrite.WriteLine(Constants.CON_hash + "=" + getHashMD5(strHashSource));     //将哈希值写入文件末尾

                //
                meWrite.Close();
                meFS.Close();
                System.IO.File.SetAttributes(myPath, FileAttributes.ReadOnly);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据保存失败: " + ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }
        #endregion

        #region 保存数据文件 -- 保存从曲线截取的局部数据

        /// <summary>
        /// 保存数据 -- 保存从曲线截取的局部数据
        /// </summary>
        /// <param name="meDataTbl">数据表</param>
        /// <param name="meInfoTbl">信息表</param>
        /// <param name="idx">第几阶段</param>
        /// <param name="colIdx">需保存的数据列</param>
        /// <param name="startIdx">数据列开始行数</param>
        /// <param name="stopIdx">数据列结束行数</param>
        /// <param name="curveNum">保存曲线数量(1或2)-HTT/HTP保存1条曲线；HTH保存2条曲线</param>
        /// <returns></returns>
        public bool Curve_SaveToLog(DataTable meDataTbl, DataTable meInfoTbl, int idx, int colIdx, int startIdx, int stopIdx, int curveNum)
        {
            //写入
            try
            {
                if (meDataTbl == null) return false;
                String fristData = meDataTbl.Rows[startIdx][colIdx].ToString();
                //if (fristData == string.Empty) return true;

                String mStr = string.Empty;
                String strHashSource = string.Empty;                            //要计算哈希值的数据源
                String deviceName = meDataTbl.Columns[colIdx].ColumnName;       //出厂编号：如TT20220101

                //需保存的参数
                int rownum = 0;
                String mem_date = meInfoTbl.Rows[rownum++][colIdx].ToString();     //测试日期
                String mem_start = meInfoTbl.Rows[rownum++][colIdx].ToString();    //测试开始时间
                String mem_stop = meInfoTbl.Rows[rownum++][colIdx].ToString();     //测试结束时间
                String mem_span = meInfoTbl.Rows[rownum++][colIdx].ToString();     //测试间隔时间
                String mem_duration = meInfoTbl.Rows[rownum++][colIdx].ToString(); //测试持续时间(秒)
                String mem_ID = meInfoTbl.Rows[rownum++][colIdx].ToString();       //设备ID
                String mem_Model = meInfoTbl.Rows[rownum++][colIdx].ToString();    //设备类型
                String mem_JSN = meInfoTbl.Rows[rownum++][colIdx].ToString();      //出厂编号
                String mem_Range = meInfoTbl.Rows[rownum++][colIdx].ToString();    //测量范围
                String mem_Cal = meInfoTbl.Rows[rownum++][colIdx].ToString();      //设备校准日期
                String mem_Rec = meInfoTbl.Rows[rownum++][colIdx].ToString();      //设备复校日期
                String mem_Bat = meInfoTbl.Rows[rownum++][colIdx].ToString();      //设备电池电量
                String mem_Type = meInfoTbl.Rows[rownum++][colIdx].ToString();     //设备类型(HTT等)
                String mem_USN = meInfoTbl.Rows[rownum++][colIdx].ToString();      //设备管理编号
                String mem_UTXT = meInfoTbl.Rows[rownum++][colIdx].ToString();     //设备管理编号
                String mem_HWver = meInfoTbl.Rows[rownum++][colIdx].ToString();
                String mem_SWver = meInfoTbl.Rows[rownum++][colIdx].ToString();
                String mem_Addr = meInfoTbl.Rows[rownum++][colIdx].ToString();
                String mem_Tmax = meInfoTbl.Rows[rownum++][colIdx].ToString();
                String mem_Tmin = meInfoTbl.Rows[rownum++][colIdx].ToString();
                String mem_rtc = meInfoTbl.Rows[rownum++][colIdx].ToString();
                String mem_htr = meInfoTbl.Rows[rownum++][colIdx].ToString();
                String mem_wdt = meInfoTbl.Rows[rownum++][colIdx].ToString();
                String mem_luk = meInfoTbl.Rows[rownum++][colIdx].ToString();
                String mem_DateHW = meInfoTbl.Rows[rownum++][colIdx].ToString();
                String mem_DateBat = meInfoTbl.Rows[rownum++][colIdx].ToString();
                String mem_DateDot = meInfoTbl.Rows[rownum++][colIdx].ToString();
                String mem_DateCal = meInfoTbl.Rows[rownum++][colIdx].ToString();
                String mem_read = meInfoTbl.Rows[rownum++][colIdx].ToString();
                String mem_wake = meInfoTbl.Rows[rownum++][colIdx].ToString();
                String mem_interval = meInfoTbl.Rows[rownum++][colIdx].ToString();
                String mem_unit = meInfoTbl.Rows[rownum++][colIdx].ToString();
                String mem_hash = meInfoTbl.Rows[rownum++][colIdx].ToString();

                String mem_Dstart = meDataTbl.Rows[startIdx][0].ToString(); //测试开始时间(实际值)
                String mem_Dstop = meDataTbl.Rows[stopIdx][0].ToString();   //测试结束时间(实际值)
                String mem_Dduration = ((stopIdx - startIdx) * Convert.ToInt32(mem_span)).ToString(); //测试持续时间(秒)(实际值)

                DateTime time = Convert.ToDateTime(mem_Dstart);          //截取曲线的开始时间
                String myPath = MyDefine.myXET.userDAT;
                //String DUTname = (mem_USN == "") ? mem_JSN : mem_USN;
                String DUTname = (mem_JSN.Length > 2) ? mem_JSN.Substring(2) : mem_JSN;
                String fileName = mem_Type + "." + mem_ID + "." + DUTname + "." + time.ToString("yyMMddHHmmss") + ".s" + idx + ".tmp";
                myPath = myPath + "\\" + fileName;      //文件路径

                /*
                if (File.Exists(myPath))
                {
                    if (MessageBox.Show(System.IO.Path.GetFileName(myPath) + "文件已存在。" + Environment.NewLine + "是否替换？", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel)
                    {
                        return false;
                    }
                    System.IO.File.SetAttributes(myPath, FileAttributes.Normal);
                }
                */

                //如果文件存在则直接覆盖
                if (File.Exists(myPath)) System.IO.File.SetAttributes(myPath, FileAttributes.Normal);   //取消只读模式
                FileStream meFS = new FileStream(myPath, FileMode.Create, FileAccess.Write);
                TextWriter meWrite = new StreamWriter(meFS);


                //System.Security.AccessControl.FileSecurity mySecurity = meFS.GetAccessControl();
                //mySecurity.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule("Everyone", System.Security.AccessControl.FileSystemRights.Read, System.Security.AccessControl.AccessControlType.Allow));
                //meFS.SetAccessControl(mySecurity);

                meWrite.WriteLine(";" + System.DateTime.Now.ToString());
                meWrite.WriteLine(";===============================================================");
                meWrite.WriteLine("Time".PadRight(18) + ("#" + mem_JSN.PadRight(15)));
                //meWrite.WriteLine("Time".PadRight(18) + ("#" + deviceName.Substring(0, deviceName.Length - 2).PadRight(10)));
                //MessageBox.Show("2:" + System.BitConverter.ToString(Encoding.Default.GetBytes(deviceName)) + Environment.NewLine + "3:" + System.BitConverter.ToString(Encoding.Default.GetBytes("HTHJD22222A")));

                //保存测试数据
                if (startIdx < 0) startIdx = 0;
                if (stopIdx >= meDataTbl.Rows.Count) stopIdx = meDataTbl.Rows.Count - 1;

                for (int i = startIdx; i <= stopIdx; i++)
                {
                    mStr = meDataTbl.Rows[i][0] + "," + meDataTbl.Rows[i][colIdx];      //测试时间+测试数据
                    if (curveNum == 2) mStr += "," + meDataTbl.Rows[i][colIdx + 1];   //保存第二条曲线(湿度值)
                    strHashSource += mStr + Environment.NewLine;                    //将行数据加入哈希值数据源
                    meWrite.WriteLine(mStr);
                }
                /*
                meWrite.WriteLine(";---------------------------------------------------------------");
                meWrite.WriteLine("date=" + memdate);
                meWrite.WriteLine("time=" + memstart);
                meWrite.WriteLine("stop=" + memstop);
                meWrite.WriteLine("span=" + memspan);
                meWrite.WriteLine("duration=" + memduration);
                meWrite.WriteLine("id=" + mem_ID);
                meWrite.WriteLine("model=" + mem_Model);
                meWrite.WriteLine("code=" + mem_JSN);
                meWrite.WriteLine("range=" + mem_Range);
                meWrite.WriteLine("cal=" + mem_Cal);
                meWrite.WriteLine("rec=" + mem_Rec);
                meWrite.WriteLine("bat=" + mem_Bat);
                meWrite.WriteLine("TYPE=" + mem_Type);
                meWrite.WriteLine("USN=" + mem_USN);
                meWrite.WriteLine("hash=" + getHashMD5(strHashSource));     //将哈希值写入文件末尾
                */

                meWrite.WriteLine(";---------------------------------------------------------------");
                meWrite.WriteLine(Constants.CON_UID + "=" + mem_ID);
                meWrite.WriteLine(Constants.CON_VERHW + "=" + mem_HWver);
                meWrite.WriteLine(Constants.CON_VERSW + "=" + mem_SWver);
                meWrite.WriteLine(Constants.CON_TYPE + "=" + mem_Type);
                meWrite.WriteLine(Constants.CON_ADDR + "=" + mem_Addr);
                meWrite.WriteLine(Constants.CON_battery + "=" + mem_Bat);
                meWrite.WriteLine(Constants.CON_maxcore + "=" + mem_Tmax);
                meWrite.WriteLine(Constants.CON_mincore + "=" + mem_Tmin);
                meWrite.WriteLine(Constants.CON_rtc + "=" + mem_rtc);
                meWrite.WriteLine(Constants.CON_htr + "=" + mem_htr);
                meWrite.WriteLine(Constants.CON_wdt + "=" + mem_wdt);
                meWrite.WriteLine(Constants.CON_luk + "=" + mem_luk);
                meWrite.WriteLine(Constants.CON_mantime + "=" + mem_DateHW);
                meWrite.WriteLine(Constants.CON_battime + "=" + mem_DateBat);
                meWrite.WriteLine(Constants.CON_caltime + "=" + mem_DateDot);
                meWrite.WriteLine(Constants.CON_jintime + "=" + mem_DateCal);
                meWrite.WriteLine(Constants.CON_JSN + "=" + mem_JSN);
                meWrite.WriteLine(Constants.CON_USN + "=" + mem_USN);
                meWrite.WriteLine(Constants.CON_UTXT + "=" + mem_UTXT);
                meWrite.WriteLine(Constants.CON_date + "=" + mem_date);
                meWrite.WriteLine(Constants.CON_wake + "=" + mem_wake);
                meWrite.WriteLine(Constants.CON_start + "=" + mem_start);
                meWrite.WriteLine(Constants.CON_stop + "=" + mem_stop);
                meWrite.WriteLine(Constants.CON_read + "=" + mem_read);
                meWrite.WriteLine(Constants.CON_interval + "=" + mem_interval);
                meWrite.WriteLine(Constants.CON_unit + "=" + mem_unit);
                meWrite.WriteLine(Constants.CON_span + "=" + mem_span);
                meWrite.WriteLine(Constants.CON_duration + "=" + mem_duration);
                meWrite.WriteLine(Constants.CON_mode + "=" + mem_Model);
                meWrite.WriteLine(Constants.CON_range + "=" + mem_Range);
                meWrite.WriteLine(Constants.CON_cal + "=" + mem_Cal);
                meWrite.WriteLine(Constants.CON_rec + "=" + mem_Rec);
                meWrite.WriteLine(Constants.CON_Dstart + "=" + mem_Dstart);
                meWrite.WriteLine(Constants.CON_Dstop + "=" + mem_Dstop);
                meWrite.WriteLine(Constants.CON_Dduration + "=" + mem_Dduration);
                meWrite.WriteLine(Constants.CON_hash + "=" + getHashMD5(strHashSource));     //将哈希值写入文件末尾

                //
                meWrite.Close();
                meFS.Close();
                System.IO.File.SetAttributes(myPath, FileAttributes.ReadOnly);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("曲线数据保存失败: " + ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }

        public bool AllCurves_SaveToLog(DataTable meDataTbl, DataTable meInfoTbl, int startIdx, int stopIdx)
        {
            try
            {
                if (meDataTbl == null) return false;
                for (int i = 1; i < meDataTbl.Columns.Count; i++)
                {
                    string colType = MyDefine.myXET.meTypeList[i];
                    if (colType == "TT_T" || colType == "TP_P")
                    {
                        Curve_SaveToLog(meDataTbl, meInfoTbl, 0, i, startIdx, stopIdx, 1);
                    }
                    else if (colType == "TH_T")
                    {
                        Curve_SaveToLog(meDataTbl, meInfoTbl, 0, i, startIdx, stopIdx, 2);
                    }
                    else if (colType == "TH_H")
                    {
                        continue;
                    }
                    else
                    {
                        continue;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("曲线数据保存失败！" + Environment.NewLine + ex.ToString());
                return false;
            }
        }

        //计算字符串哈希值
        public string getHashMD5(string strSource)
        {
            try
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                Byte[] hashSource = ASCIIEncoding.ASCII.GetBytes(strSource);                    //将字符串转换为数组
                Byte[] hashBytes = md5.ComputeHash(hashSource);                                 //计算字节数组的哈希值
                String strHashHex = System.BitConverter.ToString(hashBytes).Replace("-", "");   //将字节数组转换为十六进制字符串
                return strHashHex;
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return string.Empty;
            }
        }

        #endregion

        #region 加载数据文件(一个或多个文件)

        #region 加载文件数据并校验文件哈希值(校验失败则代表文件已被修改，终止加载)

        //加载数据文件
        public Boolean loadDataSource(String[] arrFileNames)
        {
            if (!Directory.Exists(MyDefine.myXET.userDAT))
            {
                Directory.CreateDirectory(MyDefine.myXET.userDAT);
            }

            try
            {
                MyDefine.myXET.processValue = 0;
                String colName = "";
                String testspan = "";                                       //第一个文件记录的测试间隔(判断所有文件的测试间隔是否一致)
                Boolean firstFile = true;
                DEVICE myDeviceType = DEVICE.HTT;
                int colNum = 0, rowNum = -1, fileNum = 0;                   //记录当前列索引、行索引
                int allFileNum = arrFileNames.Length; //数据文件总数

                String myDataUnit = String.Empty;                           //记录数据单位(℃/%RH/kPa)的变量
                dataTableClass DataTbl = new dataTableClass();
                dataTableClass InfoTbl = new dataTableClass();
                DataTbl.addTableColumn("时间");                             //为数据表添加时间列
                InfoTbl.addTableColumn("Blank");                            //为信息表添加空列(无作用，只是为了与meDataTbl列一一对应)
                InfoTbl.AddTableRow(Constants.InfoTblRowNum);               //根据数据文件中包含的参数个数，添加相应行数

                #region 统计所有文件的测试开始时间，并找出开始时间最早的文件的索引

                //遍历所有文件，查找测试日期最早的文件的索引(以便对其第一个加载)，并记录所有文件的测试开始时间arrStartTimes
                int MinStartFileIdx = -1;                                           //最早开始测试的文件的索引
                //String[] arrFileNames = fileDialog.FileNames;                       //所有文件路径
                DateTime MinStartTime = DateTime.MaxValue;                          //记录最早的开始时间
                DateTime[] arrStartTimes = new DateTime[arrFileNames.Length];       //记录每个文件的测试开始时间

                for (int i = 0; i < arrFileNames.Length; i++)                             //遍历所有文件，查找测试日期最早的文件
                {
                    //将原有的ReadAllLines改为逐行读取，提速
                    using (StreamReader sr = new StreamReader(arrFileNames[i]))
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            sr.ReadLine();                                                //跳过前三行
                        }
                        string line = sr.ReadLine();                                      //读取第四行，获取应该是第一个数据测试时间的字符串
                        if (line != null)
                        {
                            string strtime = line.Split(',')[0];                          //提取时间字符串
                            if (DateTime.TryParse(strtime, out arrStartTimes[i]) == false) continue;     //记录此文件的测试开始时间(若字符串无法转换为时间格式，则可能是空文件)

                            //此文件测试开始时间小于已记录的最小开始时间，更新MinStartTime并记录此文件的索引
                            if (DateTime.Compare(MinStartTime, arrStartTimes[i]) > 0)
                            {
                                MinStartTime = arrStartTimes[i];
                                MinStartFileIdx = i;                     //将此文件作为要读取的第一个文件(其数据开始时间最早)
                            }
                        }
                    }
                }

                //全部是空文件，退出
                if (MinStartFileIdx == -1)
                {
                    MyDefine.myXET.ShowWrongMsg("文件加载失败：所加载的文件数据为空！");
                    return false;
                }

                #endregion

                #region 将开始时间最早的文件置换为第一个文件，以便先加载此文件

                //如果开始时间最早的文件不是第一个文件，则将其与第一个文件互换位置(记录的测试时间同样需要互换)
                string fileName0 = arrFileNames[0];                 //文件0路径
                DateTime fileTimes0 = arrStartTimes[0];             //文件0的测试开始时间
                if (fileName0 != arrFileNames[MinStartFileIdx])     //将要读取的第一个文件替换到索引0处
                {
                    arrFileNames[0] = arrFileNames[MinStartFileIdx];
                    arrFileNames[MinStartFileIdx] = fileName0;

                    arrStartTimes[0] = arrStartTimes[MinStartFileIdx];
                    arrStartTimes[MinStartFileIdx] = fileTimes0;
                }

                #endregion

                #region 以第一个文件为基准，根据每个文件的测试开始时间计算其在数据表中的起始行

                //用rowIndex记录每个文件的第一个数据在表格中的行索引
                Boolean isFindAll = true;                                       //是否所有文件均已找到开始点了
                int[] rowIndex = new int[arrFileNames.Length];                  //每个文件的数据开始索引(行)
                String[] myLines = File.ReadAllLines(arrFileNames[0]);          //读取第一个文件(最早开始测试的文件)
                for (int i = 0; i < rowIndex.Length; i++) rowIndex[i] = -2;     //rowIndex赋初值

                for (int i = 3; i < myLines.Length; i++)                //遍历第一个文件的数据行(从第3行开始是数据行)
                {
                    if (myLines[i].Contains(";")) continue;             //注释行
                    if (myLines[i].Contains("=")) continue;             //参数行
                    if (!myLines[i].Contains(",")) continue;            //标题行

                    isFindAll = true;                                   //默认所有文件均已找到开始点了(必须放在以上3个continue语句后面)
                    DateTime mytime = DateTime.MaxValue;                //当前数据行的测试时间
                    if (DateTime.TryParse(myLines[i].Split(',')[0], out mytime) == false) continue;     //提取数据行的测试时间
                    for (int j = 0; j < arrFileNames.Length; j++)       //遍历所有文件，判断当前数据行的测试时间是否达到文件的初始测试时间
                    {
                        if (rowIndex[j] == -2) isFindAll = false;       //只要有一个文件还没找到开始点，isFindAll会为false
                        if (rowIndex[j] != -2) continue;                //当前文件已经找到开始点了
                        if (DateTime.Compare(mytime, arrStartTimes[j]) >= 0)    //找到了当前文件的开始点
                        {
                            rowIndex[j] = i - 3 - 1;
                        }
                    }

                    if (isFindAll) break;                               //已经找到所有文件的开始点了
                }

                //文件的测试时间不连贯(比如一个文件是昨天的测试数据，另一个是今天的)，退出
                if (isFindAll == false)
                {
                    MyDefine.myXET.ShowWrongMsg("文件加载失败：所加载的文件测试时间不连贯！");
                    return false;
                }

                #endregion

                #region 加载所有文件

                MyDefine.myXET.processValue += 1;                            //更新进度
                List<StringBuilder> tableRow = new List<StringBuilder>();    //记录最终显示的表每一行的数值
                for (int i = 0; i < allFileNum; i++)
                {
                    string myfile = arrFileNames[i];
                    MyDefine.myXET.AddTraceInfo("加载文件：" + myfile);
                    DateTime myCreationTime = File.GetCreationTime(myfile);
                    DateTime myModificationTime = File.GetLastWriteTime(myfile);
                    //MessageBox.Show(myCreationTime.ToString() + Environment .NewLine + myModificationTime.ToString());

                    String strInfo = string.Empty;                              //
                    String fileHash = string.Empty;                             //记录文件里的哈希值
                    String strHashSource = string.Empty;                        //要计算哈希值的数据源
                    String[] meLines = File.ReadAllLines(myfile);
                    if (meLines[meLines.Length - 1] == "hash=D41D8CD98F00B204E9800998ECF8427E") continue;           //文件包含数据个数为0，不加载此文件
                    rowNum = rowIndex[i];               //为当前文件指定数据加载的起始行数
                    fileNum++;

                    #region 加载单个文件
                    foreach (String line in meLines)
                    {
                        if (line.Contains(";"))                 //注释行
                        {
                        }
                        else if (line.Contains("="))            //参数行
                        {
                            #region 加载所有参数

                            strInfo = line.Substring(line.IndexOf('=') + 1);
                            switch (line.Substring(0, line.IndexOf('=')))
                            {
                                case Constants.CON_date:        //日期
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 0, colNum, strInfo);
                                    break;
                                case Constants.CON_start:       //起始时刻
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 1, colNum, strInfo);
                                    break;
                                case Constants.CON_stop:        //停止时刻
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 2, colNum, strInfo);
                                    break;
                                case Constants.CON_span:        //间隔时间(秒)
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 3, colNum, strInfo);
                                    if (firstFile) testspan = strInfo;
                                    if (!firstFile && testspan != strInfo)
                                    {
                                        MyDefine.myXET.ShowWrongMsg("文件加载失败：所加载的文件测试间隔不一致！");
                                        return false;
                                    }
                                    break;
                                case Constants.CON_duration:    //持续时间
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 4, colNum, strInfo);
                                    break;
                                case Constants.CON_UID:         //设备ID
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 5, colNum, strInfo);
                                    break;
                                case Constants.CON_mode:        //设备类型（LT30）
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 6, colNum, strInfo);
                                    break;
                                case Constants.CON_JSN:         //出厂编号
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 7, colNum, strInfo);
                                    break;
                                case Constants.CON_range:
                                    //测量范围单位切换
                                    String[] str = strInfo.Trim().Split(new char[] { '（', '(', '~', ')', '）' });      //测量范围
                                    String[] spString = strInfo.Trim().Split(new char[] { ' ' });      //测量范围
                                    if (MyDefine.myXET.temUnit != str[3].Trim())
                                    {
                                        double data1 = 0;
                                        double data2 = 0;
                                        if (double.TryParse(str[1], out data1) && double.TryParse(str[2], out data2))
                                        {
                                            switch (MyDefine.myXET.temUnit)
                                            {
                                                case "℃":
                                                    data1 = (data1 - 32) / 1.8;
                                                    data2 = (data2 - 32) / 1.8;
                                                    break;
                                                case "℉":
                                                    data1 = 32 + data1 * 1.8;
                                                    data2 = 32 + data2 * 1.8;
                                                    break;
                                            }
                                        }
                                        strInfo = "(" + data1 + "~" + data2 + ")" + MyDefine.myXET.temUnit;
                                        if (spString.Length == 2)
                                        {
                                            strInfo += " " + spString[1];
                                        }
                                    }
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 8, colNum, strInfo);
                                    break;
                                case Constants.CON_cal:
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 9, colNum, strInfo);
                                    break;
                                case Constants.CON_rec:
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 10, colNum, strInfo);
                                    break;
                                case Constants.CON_battery:
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 11, colNum, strInfo);
                                    break;
                                case Constants.CON_TYPE:
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 12, colNum, strInfo);
                                    break;
                                case Constants.CON_USN:
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 13, colNum, strInfo);
                                    break;
                                case Constants.CON_UTXT:
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 14, colNum, strInfo);
                                    break;
                                case Constants.CON_VERHW:
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 15, colNum, strInfo);
                                    break;
                                case Constants.CON_VERSW:
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 16, colNum, strInfo);
                                    break;
                                case Constants.CON_ADDR:
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 17, colNum, strInfo);
                                    break;
                                case Constants.CON_maxcore:
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 18, colNum, strInfo);
                                    break;
                                case Constants.CON_mincore:
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 19, colNum, strInfo);
                                    break;
                                case Constants.CON_rtc:
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 20, colNum, strInfo);
                                    break;
                                case Constants.CON_htr:
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 21, colNum, strInfo);
                                    break;
                                case Constants.CON_wdt:
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 22, colNum, strInfo);
                                    break;
                                case Constants.CON_luk:
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 23, colNum, strInfo);
                                    break;
                                case Constants.CON_mantime:
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 24, colNum, strInfo);
                                    break;
                                case Constants.CON_battime:
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 25, colNum, strInfo);
                                    break;
                                case Constants.CON_caltime:
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 26, colNum, strInfo);
                                    break;
                                case Constants.CON_jintime:
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 27, colNum, strInfo);
                                    break;
                                case Constants.CON_read:
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 28, colNum, strInfo);
                                    break;
                                case Constants.CON_wake:
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 29, colNum, strInfo);
                                    break;
                                case Constants.CON_interval:
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 30, colNum, strInfo);
                                    break;
                                case Constants.CON_unit:
                                    addValToInfoTable(ref InfoTbl, myDeviceType, 31, colNum, strInfo);
                                    break;
                                case Constants.CON_hash:    //hash永远应该是最后一个参数
                                    if (firstFile) firstFile = false;                    //第一个文件读取完成
                                    fileHash = line.Substring(line.IndexOf('=') + 1);    //保存文件的哈希值
                                    addValToInfoTable(ref InfoTbl, myDeviceType, Constants.InfoTblRowNum - 1, colNum, strInfo);
                                    break;
                                default:
                                    break;
                            }

                            #endregion
                        }
                        else
                        {
                            if (!line.Contains(","))            //标题行
                            {
                                #region 加载标题行(数据表列信息)

                                colName = line.Split('#')[1].Trim();        //出厂编号，如TT20220101
                                myDeviceType = (DEVICE)(Enum.Parse(typeof(DEVICE), "H" + colName.Substring(0, 2)));   //产品类型HTT/HTH/HTP/HTQ

                                if (myDeviceType == DEVICE.HTT)
                                {
                                    //仅添加一个数据列
                                    DataTbl.addTableColumn(colName.Substring(2) + "T");           //添加一个数据列(加空白字符，保持所有名称格式一致：HTx(_H)+JSN+1*char)
                                    InfoTbl.addTableColumn(colName.Substring(2) + "T");           //添加一个数据列(加空白字符，保持所有名称格式一致：HTx(_H)+JSN+1*char)
                                    colNum += 1;
                                }
                                else if (myDeviceType == DEVICE.HTP)
                                {
                                    //仅添加一个数据列
                                    DataTbl.addTableColumn(colName.Substring(2) + "P");           //添加一个数据列(加空白字符，保持所有名称格式一致：HTx(_H)+JSN+1*char)
                                    InfoTbl.addTableColumn(colName.Substring(2) + "P");           //添加一个数据列(加空白字符，保持所有名称格式一致：HTx(_H)+JSN+1*char)
                                    colNum += 1;
                                }
                                else if (myDeviceType == DEVICE.HTH || myDeviceType == DEVICE.HTQ)
                                {
                                    //添加第一列：温度值
                                    DataTbl.addTableColumn(colName.Substring(2) + "T");//           //添加一个数据列
                                    InfoTbl.addTableColumn(colName.Substring(2) + "T");            //添加一个数据列
                                    colNum += 1;

                                    //添加第二列：湿度值
                                    DataTbl.addTableColumn(colName.Substring(2) + "H");            //添加一个数据列
                                    InfoTbl.addTableColumn(colName.Substring(2) + "H");            //添加一个数据列
                                    colNum += 1;
                                }

                                #endregion
                            }
                            else                                //数据行
                            {
                                #region 加载数据行(数据表行信息)

                                strHashSource += line + Environment.NewLine;        //将数据行加入哈希值数据源
                                rowNum = rowNum + 1;                                //记录当前数据对应的数据表行数(索引，从0开始)
                                string[] dataArr = line.Split(',');                 //"测试时间,测试数据,测试数据"
                                int myRowNum;                                       //当前数据表的行数
                                if (myDeviceType == DEVICE.HTQ)
                                {
                                    myRowNum = tableRow.Count();
                                }
                                else
                                {
                                    myRowNum = DataTbl.dataTable.Rows.Count;
                                }

                                if (myRowNum <= rowNum)                             //当前数据超出数据表列数，则添加新行，并写入测试时间"Time"
                                {
                                    switch (myDeviceType)
                                    {
                                        case DEVICE.HTQ:
                                            StringBuilder sb = new StringBuilder(dataArr[0].Trim());
                                            tableRow.Add(sb);                                       //加到对应行
                                            break;
                                        default:
                                            DataTbl.AddTableRow();                                  //添加新行
                                            DataTbl.SetCellValue(rowNum, 0, dataArr[0].Trim());     //添加测试时间"Time"
                                            break;
                                    }
                                }
                                double tem;
                                switch (myDeviceType)
                                {
                                    case DEVICE.HTT:    //温度采集器
                                        tem = Convert.ToDouble(dataArr[1]);
                                        if (temUnit == "℃")
                                        {
                                            if (!myDataUnit.Contains("℃")) myDataUnit += "/℃";      //添加文件数据单位(用于出报告)
                                        }
                                        else
                                        {
                                            if (!myDataUnit.Contains("℉")) myDataUnit += "/℉";      //添加文件数据单位(用于出报告)
                                            dataArr[1] = (9 * tem / 5 + 32).ToString();
                                        }
                                        DataTbl.SetCellValue(rowNum, colNum, dataArr[1].Trim());       //添加温度值
                                        break;

                                    case DEVICE.HTH:    //温湿度采集器
                                        tem = Convert.ToDouble(dataArr[1]);
                                        if (temUnit == "℃")
                                        {
                                            if (!myDataUnit.Contains("℃")) myDataUnit += "/℃";      //添加文件数据单位(用于出报告)
                                        }
                                        else
                                        {
                                            if (!myDataUnit.Contains("℉")) myDataUnit += "/℉";      //添加文件数据单位(用于出报告)
                                            dataArr[1] = (9 * tem / 5 + 32).ToString();
                                        }
                                        DataTbl.SetCellValue(rowNum, colNum - 1, dataArr[1].Trim());   //添加温度值
                                        DataTbl.SetCellValue(rowNum, colNum, dataArr[2].Trim());       //添加湿度值
                                        if (!myDataUnit.Contains("%RH")) myDataUnit += "/%RH";         //添加文件数据单位(用于出报告)
                                        break;

                                    case DEVICE.HTP:    //压力采集器
                                        DataTbl.SetCellValue(rowNum, colNum, dataArr[1].Trim());       //添加压力值
                                        if (!myDataUnit.Contains("kPa")) myDataUnit += "/kPa";         //添加文件数据单位(用于出报告)
                                        break;

                                    case DEVICE.HTQ:    //温湿度采集器
                                        tem = Convert.ToDouble(dataArr[1]);
                                        if (temUnit == "℃")
                                        {
                                            if (!myDataUnit.Contains("℃")) myDataUnit += "/℃";      //添加文件数据单位(用于出报告)
                                        }
                                        else
                                        {
                                            if (!myDataUnit.Contains("℉")) myDataUnit += "/℉";      //添加文件数据单位(用于出报告)
                                            dataArr[1] = (9 * tem / 5 + 32).ToString();
                                        }
                                        tableRow[rowNum].Append(",");
                                        tableRow[rowNum].Append(dataArr[1].Trim());
                                        tableRow[rowNum].Append(",");
                                        tableRow[rowNum].Append(dataArr[2].Trim());
                                        if (!myDataUnit.Contains("%RH")) myDataUnit += "/%RH";         //添加文件数据单位(用于出报告)
                                        break;

                                    default:
                                        break;
                                }

                                #endregion
                            }
                        }
                    }

                    #endregion

                    #region 校验文件哈希值

                    //校验每个文件的哈希值
                    if (!fileHash.Equals(MyDefine.myXET.getHashMD5(strHashSource)))  //哈希值校验失败
                    {
                        MyDefine.myXET.ShowWrongMsg("文件加载失败：" + Path.GetFileName(myfile) + "文件被修改，无法加载!");
                        return false;
                    }

                    MyDefine.myXET.processValue = (int)(1 + 93 * i / allFileNum);  //更新进度

                    #endregion

                }

                //将按行加入到DataTb1
                if (myDeviceType == DEVICE.HTQ)
                {
                    int RowNum = tableRow.Count;
                    foreach (StringBuilder sb in tableRow)
                    {
                        string[] tableRowItem = sb.ToString().Split(',');
                        DataTbl.AddTableRow(tableRowItem);
                    }
                }

                MyDefine.myXET.processValue += 1;  //更新进度


                #endregion

                #region 加载的都是空文件，加载失败

                //加载的都是空文件，只有初始时间列
                if (DataTbl.dataTable.Columns.Count == 1)
                {
                    MyDefine.myXET.ShowWrongMsg("文件加载失败：所加载的文件数据为空！");
                    return false;
                }

                #endregion

                #region 加载成功，更新全局变量

                //文件加载成功，将数据表赋给全局变量
                MyDefine.myXET.homunit = myDataUnit;
                MyDefine.myXET.meDataTbl = new dataTableClass();
                MyDefine.myXET.meDataTbl.dataTable = DataTbl.CopyTable();
                MyDefine.myXET.meInfoTbl = new dataTableClass();
                MyDefine.myXET.meInfoTbl.dataTable = InfoTbl.CopyTable();

                //文件加载成功，更新相关信息(必须放在全局数据表赋值后面)
                UpdateGlobalParameters();       //文件加载成功后，更新文件相关参数的全局变量
                getDataTypeList();              //数据表各列数据类型标记（TT_T / TH_T / TH_H / TP_P）
                getDataLimits();                //计算温度、湿度、压力的最大最小值(用于验证表计算)
                MyDefine.myXET.processValue += 2;  //更新进度
                DataDealing();                  //将数据表按温度、湿度、压力分成3个表
                MyDefine.myXET.processValue += 2;  //更新进度
                #endregion

                MyDefine.myXET.AddTraceInfo("数据加载成功，加载文件数：" + fileNum);
                //MyDefine.myXET.AddToTraceRecords("数据处理", "数据加载成功，加载文件数：" + fileNum);
                MyDefine.myXET.processValue = 100;
                MessageBox.Show("文件加载成功，已载入文件个数：" + fileNum, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                if (ex.GetType() == typeof(DuplicateNameException))
                {
                    MyDefine.myXET.ShowWrongMsg("文件加载失败：加载文件中存在重复设备编号！");
                }
                else
                {
                    MyDefine.myXET.ShowWrongMsg("文件加载失败：" + ex.ToString());
                }
                return false;
            }
        }
        #endregion

        #region 加载文件数据并校验文件哈希值(校验失败则代表文件已被修改，终止加载)0

        //加载数据文件
        public Boolean loadDataSource1(String[] arrFileNames)
        {
            if (!Directory.Exists(MyDefine.myXET.userDAT))
            {
                Directory.CreateDirectory(MyDefine.myXET.userDAT);
            }

            try
            {
                String colName = "";
                String testspan = "";                       //第一个文件记录都测试间隔(判断所有文件的测试间隔是否一致)
                Boolean firstFile = true;
                DEVICE myDeviceType = DEVICE.HTT;
                int colNum = 0, rowNum = -1, maxRowNum = 0, fileNum = 0;   //记录当前列索引、行索引

                System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
                fileDialog.Title = "请选择数据";
                fileDialog.Filter = "数据(*.tmp)|*.tmp";
                fileDialog.Multiselect = true;                  //允许选择多文件
                fileDialog.RestoreDirectory = true;
                fileDialog.InitialDirectory = MyDefine.myXET.userDAT;

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    String myDataUnit = String.Empty;                       //记录数据单位(℃/%RH/kPa)的变量
                    dataTableClass DataTbl = new dataTableClass();
                    dataTableClass InfoTbl = new dataTableClass();
                    DataTbl.addTableColumn("时间");        //为数据表添加时间列
                    InfoTbl.addTableColumn("Blank");       //为信息表添加空列(无作用，只是为了与meDataTbl列一一对应)
                    InfoTbl.AddTableRow(Constants.InfoTblRowNum);  //根据数据文件中包含的参数个数，添加相应行数

                    #region 统计所有文件的测试开始时间，并找出开始时间最早的文件的索引

                    //遍历所有文件，查找测试日期最早的文件的索引(以便对其第一个加载)，并记录所有文件的测试开始时间arrStartTimes
                    int MinStartFileIdx = -1;                                           //最早开始测试的文件的索引
                    //String[] arrFileNames = fileDialog.FileNames;                       //所有文件路径
                    DateTime MinStartTime = DateTime.MaxValue;                          //记录最早的开始时间
                    DateTime[] arrStartTimes = new DateTime[arrFileNames.Length];       //记录每个文件的测试开始时间
                    for (int i = 0; i < arrFileNames.Length; i++)                       //遍历所有文件，查找测试日期最早的文件
                    {
                        String[] meLines = File.ReadAllLines(arrFileNames[i]);          //读取文件
                        String strtime = meLines[3].Split(',')[0];                      //获取应该是第一个数据测试时间的字符串
                        if (DateTime.TryParse(strtime, out arrStartTimes[i]) == false) continue;     //记录此文件的测试开始时间(若字符串无法转换为时间格式，则可能是空文件)

                        //此文件测试开始时间小于已记录的最小开始时间，更新MinStartTime并记录此文件的索引
                        if (DateTime.Compare(MinStartTime, arrStartTimes[i]) > 0)
                        {
                            MinStartTime = arrStartTimes[i];
                            MinStartFileIdx = i;                     //将此文件作为要读取的第一个文件(其数据开始时间最早)
                        }
                    }

                    //全部是空文件，退出
                    if (MinStartFileIdx == -1)
                    {
                        MyDefine.myXET.ShowWrongMsg("文件加载失败：所加载的文件数据为空！");
                        return false;
                    }

                    #endregion

                    #region 将开始时间最早的文件置换为第一个文件，以便先加载此文件

                    //如果开始时间最早的文件不是第一个文件，则将其与第一个文件互换位置(记录的测试时间同样需要互换)
                    string fileName0 = arrFileNames[0];                 //文件0路径
                    DateTime fileTimes0 = arrStartTimes[0];             //文件0的测试开始时间
                    if (fileName0 != arrFileNames[MinStartFileIdx])     //将要读取的第一个文件替换到索引0处
                    {
                        arrFileNames[0] = arrFileNames[MinStartFileIdx];
                        arrFileNames[MinStartFileIdx] = fileName0;

                        arrStartTimes[0] = arrStartTimes[MinStartFileIdx];
                        arrStartTimes[MinStartFileIdx] = fileTimes0;
                    }

                    #endregion

                    #region 以第一个文件为基准，根据每个文件的测试开始时间计算其在数据表中的起始行

                    //用rowIndex记录每个文件的第一个数据在表格中的行索引
                    Boolean isFindAll = true;                                       //是否所有文件均已找到开始点了
                    int[] rowIndex = new int[arrFileNames.Length];                  //每个文件的数据开始索引(行)
                    String[] myLines = File.ReadAllLines(arrFileNames[0]);          //读取第一个文件(最早开始测试的文件)
                    for (int i = 0; i < rowIndex.Length; i++) rowIndex[i] = -2;     //rowIndex赋初值

                    for (int i = 3; i < myLines.Length; i++)                //遍历第一个文件的数据行(从第3行开始是数据行)
                    {
                        if (myLines[i].Contains(";")) continue;             //注释行
                        if (myLines[i].Contains("=")) continue;             //参数行
                        if (!myLines[i].Contains(",")) continue;            //标题行

                        isFindAll = true;                                   //默认所有文件均已找到开始点了(必须放在以上3个continue语句后面)
                        DateTime mytime = DateTime.MaxValue;                //当前数据行的测试时间
                        if (DateTime.TryParse(myLines[i].Split(',')[0], out mytime) == false) continue;     //提取数据行的测试时间
                        for (int j = 0; j < arrFileNames.Length; j++)       //遍历所有文件，判断当前数据行的测试时间是否达到文件的初始测试时间
                        {
                            if (rowIndex[j] == -2) isFindAll = false;       //只要有一个文件还没找到开始点，isFindAll会为false
                            if (rowIndex[j] != -2) continue;                //当前文件已经找到开始点了
                            if (DateTime.Compare(mytime, arrStartTimes[j]) >= 0)    //找到了当前文件的开始点
                            {
                                rowIndex[j] = i - 3 - 1;
                            }
                        }

                        if (isFindAll) break;                               //已经找到所有文件的开始点了
                    }

                    //文件的测试时间不连贯(比如一个文件是昨天的测试数据，另一个是今天的)，退出
                    if (isFindAll == false)
                    {
                        MyDefine.myXET.ShowWrongMsg("文件加载失败：所加载的文件测试时间不连贯！");
                        return false;
                    }

                    #endregion

                    #region 加载所有文件

                    for (int i = 0; i < arrFileNames.Length; i++)
                    {
                        string myfile = arrFileNames[i];
                        DateTime myCreationTime = File.GetCreationTime(myfile);
                        DateTime myModificationTime = File.GetLastWriteTime(myfile);
                        //MessageBox.Show(myCreationTime.ToString() + Environment .NewLine + myModificationTime.ToString());

                        String strInfo = string.Empty;                              //
                        String fileHash = string.Empty;                             //记录文件里的哈希值
                        String strHashSource = string.Empty;                        //要计算哈希值的数据源
                        String[] meLines = File.ReadAllLines(myfile);
                        if (meLines[meLines.Length - 1] == "hash=D41D8CD98F00B204E9800998ECF8427E") continue;           //文件包含数据个数为0，不加载此文件
                        rowNum = rowIndex[i];               //为当前文件指定数据加载的起始行数
                        fileNum++;

                        #region 加载单个文件

                        foreach (String line in meLines)
                        {
                            if (line.Contains(";"))                 //注释行
                            {
                            }
                            else if (line.Contains("="))            //参数行
                            {
                                #region 加载所有参数

                                strInfo = line.Substring(line.IndexOf('=') + 1);
                                switch (line.Substring(0, line.IndexOf('=')))
                                {
                                    case Constants.CON_date:        //日期
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 0, colNum, strInfo);

                                        break;
                                    case Constants.CON_start:       //起始时刻
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 1, colNum, strInfo);
                                        break;
                                    case Constants.CON_stop:        //停止时刻
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 2, colNum, strInfo);
                                        break;
                                    case Constants.CON_span:        //间隔时间(秒)
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 3, colNum, strInfo);
                                        if (firstFile) testspan = strInfo;
                                        if (!firstFile && testspan != strInfo)
                                        {
                                            MyDefine.myXET.ShowWrongMsg("文件加载失败：所加载的文件测试间隔不一致！");
                                            return false;
                                        }
                                        break;
                                    case Constants.CON_duration:    //持续时间
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 4, colNum, strInfo);
                                        break;
                                    case Constants.CON_UID:         //设备ID
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 5, colNum, strInfo);
                                        break;
                                    case Constants.CON_mode:        //设备类型（LT30）
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 6, colNum, strInfo);
                                        break;
                                    case Constants.CON_JSN:         //出厂编号
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 7, colNum, strInfo);
                                        break;
                                    case Constants.CON_range:
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 8, colNum, strInfo);
                                        break;
                                    case Constants.CON_cal:
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 9, colNum, strInfo);
                                        break;
                                    case Constants.CON_rec:
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 10, colNum, strInfo);
                                        break;
                                    case Constants.CON_battery:
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 11, colNum, strInfo);
                                        break;
                                    case Constants.CON_TYPE:
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 12, colNum, strInfo);
                                        break;
                                    case Constants.CON_USN:
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 13, colNum, strInfo);
                                        break;
                                    case Constants.CON_UTXT:
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 14, colNum, strInfo);
                                        break;
                                    case Constants.CON_VERHW:
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 15, colNum, strInfo);
                                        break;
                                    case Constants.CON_VERSW:
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 16, colNum, strInfo);
                                        break;
                                    case Constants.CON_ADDR:
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 17, colNum, strInfo);
                                        break;
                                    case Constants.CON_maxcore:
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 18, colNum, strInfo);
                                        break;
                                    case Constants.CON_mincore:
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 19, colNum, strInfo);
                                        break;
                                    case Constants.CON_rtc:
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 20, colNum, strInfo);
                                        break;
                                    case Constants.CON_htr:
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 21, colNum, strInfo);
                                        break;
                                    case Constants.CON_wdt:
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 22, colNum, strInfo);
                                        break;
                                    case Constants.CON_luk:
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 23, colNum, strInfo);
                                        break;
                                    case Constants.CON_mantime:
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 24, colNum, strInfo);
                                        break;
                                    case Constants.CON_battime:
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 25, colNum, strInfo);
                                        break;
                                    case Constants.CON_caltime:
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 26, colNum, strInfo);
                                        break;
                                    case Constants.CON_jintime:
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 27, colNum, strInfo);
                                        break;
                                    case Constants.CON_read:
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 28, colNum, strInfo);
                                        break;
                                    case Constants.CON_wake:
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 29, colNum, strInfo);
                                        break;
                                    case Constants.CON_interval:
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 30, colNum, strInfo);
                                        break;
                                    case Constants.CON_unit:
                                        addValToInfoTable(ref InfoTbl, myDeviceType, 31, colNum, strInfo);
                                        break;
                                    case Constants.CON_hash:    //hash永远应该是最后一个参数
                                        if (firstFile)          //第一个文件读取完成
                                        {
                                            firstFile = false;
                                            maxRowNum = rowNum; //将第一个文件的数据行数定义为允许加载的最大行数
                                        }

                                        //rowNum = -1;                                         //复位文件当前行数
                                        fileHash = line.Substring(line.IndexOf('=') + 1);    //保存文件的哈希值
                                        addValToInfoTable(ref InfoTbl, myDeviceType, Constants.InfoTblRowNum - 1, colNum, strInfo);
                                        break;
                                    default:
                                        break;
                                }

                                #endregion
                            }
                            else
                            {
                                if (!line.Contains(","))            //标题行
                                {
                                    #region 加载标题行(数据表列信息)

                                    colName = line.Split('#')[1].Trim();        //出厂编号，如TT20220101
                                    myDeviceType = (DEVICE)(Enum.Parse(typeof(DEVICE), "H" + colName.Substring(0, 2)));   //产品类型HTT/HTH/HTP

                                    if (myDeviceType == DEVICE.HTT || myDeviceType == DEVICE.HTP)
                                    {
                                        //仅添加一个数据列
                                        DataTbl.addTableColumn(colName + " ");           //添加一个数据列(加空白字符，保持所有名称格式一致：HTx(_H)+JSN+1*char)
                                        InfoTbl.addTableColumn(colName + " ");           //添加一个数据列(加空白字符，保持所有名称格式一致：HTx(_H)+JSN+1*char)
                                        colNum += 1;
                                    }
                                    else if (myDeviceType == DEVICE.HTH || myDeviceType == DEVICE.HTQ)
                                    {
                                        //添加第一列：温度值
                                        DataTbl.addTableColumn(colName + "T");            //添加一个数据列
                                        InfoTbl.addTableColumn(colName + "T");            //添加一个数据列
                                        colNum += 1;

                                        //添加第二列：湿度值
                                        DataTbl.addTableColumn(colName + "H");            //添加一个数据列
                                        InfoTbl.addTableColumn(colName + "H");            //添加一个数据列
                                        colNum += 1;
                                    }

                                    #endregion
                                }
                                else                                //数据行
                                {
                                    #region 加载数据行(数据表行信息)

                                    strHashSource += line + Environment.NewLine;        //将数据行加入哈希值数据源
                                    rowNum = rowNum + 1;                                //记录当前数据对应的数据表行数(索引，从0开始)
                                    string[] dataArr = line.Split(',');                 //"测试时间,测试数据,测试数据"
                                    int myRowNum = DataTbl.dataTable.Rows.Count;        //当前数据表的行数
                                    if (myRowNum <= rowNum)                             //当前数据超出数据表列数，则添加新行，并写入测试时间"Time"
                                    {
                                        DataTbl.AddTableRow();                          //添加新行
                                        DataTbl.SetCellValue(rowNum, 0, dataArr[0].Trim());     //添加测试时间"Time"
                                    }

                                    switch (myDeviceType)
                                    {
                                        case DEVICE.HTT:    //温度采集器
                                            DataTbl.SetCellValue(rowNum, colNum, dataArr[1].Trim());       //添加温度值
                                            if (!myDataUnit.Contains("℃")) myDataUnit += "/℃";           //添加文件数据单位(用于出报告)
                                            break;

                                        case DEVICE.HTH:    //温湿度采集器
                                            DataTbl.SetCellValue(rowNum, colNum - 1, dataArr[1].Trim());   //添加温度值
                                            DataTbl.SetCellValue(rowNum, colNum, dataArr[2].Trim());       //添加湿度值
                                            if (!myDataUnit.Contains("℃")) myDataUnit += "/℃";           //添加文件数据单位(用于出报告)
                                            if (!myDataUnit.Contains("%RH")) myDataUnit += "/%RH";         //添加文件数据单位(用于出报告)
                                            break;

                                        case DEVICE.HTP:    //压力采集器
                                            DataTbl.SetCellValue(rowNum, colNum, dataArr[1].Trim());       //添加压力值
                                            if (!myDataUnit.Contains("kPa")) myDataUnit += "/kPa";         //添加文件数据单位(用于出报告)
                                            break;

                                        case DEVICE.HTQ:    //温湿度采集器
                                            DataTbl.SetCellValue(rowNum, colNum - 1, dataArr[1].Trim());   //添加温度值
                                            DataTbl.SetCellValue(rowNum, colNum, dataArr[2].Trim());       //添加湿度值
                                            if (!myDataUnit.Contains("℃")) myDataUnit += "/℃";           //添加文件数据单位(用于出报告)
                                            if (!myDataUnit.Contains("%RH")) myDataUnit += "/%RH";         //添加文件数据单位(用于出报告)
                                            break;

                                        default:
                                            break;
                                    }

                                    #endregion
                                }
                            }
                        }

                        #endregion

                        #region 校验文件哈希值

                        //校验每个文件的哈希值
                        if (!fileHash.Equals(MyDefine.myXET.getHashMD5(strHashSource)))  //哈希值校验失败
                        {
                            MyDefine.myXET.ShowWrongMsg("文件加载失败：" + Path.GetFileName(myfile) + "文件被修改，无法加载!");
                            return false;
                        }

                        #endregion

                    }

                    #endregion

                    #region 加载的都是空文件，加载失败

                    //加载的都是空文件，只有初始时间列
                    if (DataTbl.dataTable.Columns.Count == 1)
                    {
                        MyDefine.myXET.ShowWrongMsg("文件加载失败：所加载的文件数据为空！");
                        return false;
                    }

                    #endregion

                    #region 加载成功，更新全局变量

                    //文件加载成功，将数据表赋给全局变量
                    MyDefine.myXET.homunit = myDataUnit;
                    MyDefine.myXET.meDataTbl = new dataTableClass();
                    MyDefine.myXET.meDataTbl.dataTable = DataTbl.CopyTable();
                    MyDefine.myXET.meInfoTbl = new dataTableClass();
                    MyDefine.myXET.meInfoTbl.dataTable = InfoTbl.CopyTable();

                    //文件加载成功，更新相关信息(必须放在全局数据表赋值后面)
                    UpdateGlobalParameters();       //文件加载成功后，更新文件相关参数的全局变量
                    getDataTypeList();              //数据表各列数据类型标记（TT_T / TH_T / TH_H / TP_P）
                    getDataLimits();                //计算温度、湿度、压力的最大最小值(用于验证表计算)
                    DataDealing();                  //将数据表按温度、湿度、压力分成3个表

                    #endregion

                    MyDefine.myXET.AddTraceInfo("数据加载成功，加载文件数：" + fileNum);
                    //MyDefine.myXET.AddToTraceRecords("数据处理", "数据加载成功，加载文件数：" + fileNum);
                    MessageBox.Show("文件加载成功，已载入文件个数：" + fileNum, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
                else
                {
                    MyDefine.myXET.AddTraceInfo("取消加载");
                    return false;
                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                if (ex.GetType() == typeof(DuplicateNameException))
                {
                    MyDefine.myXET.ShowWrongMsg("文件加载失败：加载文件中存在重复设备编号！");
                }
                else
                {
                    MyDefine.myXET.ShowWrongMsg("文件加载失败：" + ex.ToString());
                }
                return false;
            }
        }

        #endregion

        #region 加载文件数据并校验文件哈希值(校验失败则代表文件已被修改，终止加载)0

        //加载数据文件
        public Boolean loadDataSource0()
        {
            if (!Directory.Exists(MyDefine.myXET.userDAT))
            {
                Directory.CreateDirectory(MyDefine.myXET.userDAT);
            }

            try
            {
                String colName = "";
                Boolean firstFile = true;
                DEVICE myDeviceType = DEVICE.HTT;
                //String myJSNCode = "";              //产品编号
                int colNum = 0, rowNum = -1, maxRowNum = 0, fileNum = 0;   //记录当前列索引、行索引

                System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
                fileDialog.Title = "请选择数据";
                fileDialog.Filter = "数据(*.tmp)|*.tmp";
                fileDialog.Multiselect = true;                  //允许选择多文件
                fileDialog.RestoreDirectory = true;
                fileDialog.InitialDirectory = MyDefine.myXET.userDAT;

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    MyDefine.myXET.homunit = string.Empty;             //清空记录数据单位(℃/%RH/kPa)的变量
                    MyDefine.myXET.meDataTbl = new dataTableClass();
                    MyDefine.myXET.meInfoTbl = new dataTableClass();
                    MyDefine.myXET.meDataTbl.addTableColumn("Time");        //为数据表添加时间列
                    MyDefine.myXET.meInfoTbl.addTableColumn("Blank");       //为信息表添加空列(无作用，只是为了与meDataTbl列一一对应)
                    MyDefine.myXET.meInfoTbl.AddTableRow(Constants.InfoTblRowNum);  //根据数据文件中包含的参数个数，添加相应行数

                    foreach (string myfile in fileDialog.FileNames)
                    {
                        DateTime myCreationTime = File.GetCreationTime(myfile);
                        DateTime myModificationTime = File.GetLastWriteTime(myfile);
                        //MessageBox.Show(myCreationTime.ToString() + Environment .NewLine + myModificationTime.ToString());

                        fileNum++;
                        //if (fileNum > 10) break;      //最多加载10个文件
                        String strInfo = string.Empty;                              //
                        String fileHash = string.Empty;                             //记录文件里的哈希值
                        String strHashSource = string.Empty;                        //要计算哈希值的数据源
                        String[] meLines = File.ReadAllLines(myfile);
                        if (meLines[meLines.Length - 1] == "hash=D41D8CD98F00B204E9800998ECF8427E") continue;           //文件包含数据个数为0，不加载此文件
                        if (firstFile) MyDefine.myXET.homsave = meLines[0].Substring(meLines[0].IndexOf('=') + 1);

                        foreach (String line in meLines)
                        {
                            if (line.Contains(";"))
                            {
                            }
                            else if (line.Contains("="))
                            {
                                strInfo = line.Substring(line.IndexOf('=') + 1);
                                switch (line.Substring(0, line.IndexOf('=')))
                                {
                                    case Constants.CON_date:     //日期
                                        if (firstFile) MyDefine.myXET.homdate = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 0, colNum, strInfo);
                                        break;
                                    case Constants.CON_start:     //起始时刻
                                        if (firstFile) MyDefine.myXET.homstart = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 1, colNum, strInfo);
                                        break;
                                    case Constants.CON_stop:    //停止时刻
                                        if (firstFile) MyDefine.myXET.homstop = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 2, colNum, strInfo);
                                        break;
                                    case Constants.CON_span:    //间隔时间(秒)
                                        if (firstFile) MyDefine.myXET.homspan = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 3, colNum, strInfo);
                                        break;
                                    case Constants.CON_duration:    //持续时间
                                        if (firstFile) MyDefine.myXET.homrun = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 4, colNum, strInfo);
                                        break;
                                    case Constants.CON_UID: //设备ID
                                        if (firstFile) MyDefine.myXET.hom_UID = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 5, colNum, strInfo);
                                        break;
                                    case Constants.CON_mode:    //设备型号HTT/HTH/HTP
                                        if (firstFile) MyDefine.myXET.hom_Type = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 6, colNum, strInfo);
                                        break;
                                    case Constants.CON_JSN:    //出厂编号
                                        if (firstFile) MyDefine.myXET.hom_JSN = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 7, colNum, strInfo);
                                        break;
                                    case Constants.CON_range:
                                        if (firstFile) MyDefine.myXET.hom_Range = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 8, colNum, strInfo);
                                        break;
                                    case Constants.CON_cal:
                                        if (firstFile) MyDefine.myXET.hom_Cal = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 9, colNum, strInfo);
                                        break;
                                    case Constants.CON_rec:
                                        if (firstFile) MyDefine.myXET.hom_Rec = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 10, colNum, strInfo);
                                        break;
                                    case Constants.CON_battery:
                                        if (firstFile) MyDefine.myXET.hom_Bat = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 11, colNum, strInfo);
                                        break;
                                    case Constants.CON_TYPE:
                                        if (firstFile) MyDefine.myXET.hom_Type = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 12, colNum, strInfo);
                                        break;
                                    case Constants.CON_USN:
                                        if (firstFile) MyDefine.myXET.hom_USN = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 13, colNum, strInfo);
                                        break;
                                    case Constants.CON_UTXT:
                                        if (firstFile) MyDefine.myXET.hom_UTXT = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 14, colNum, strInfo);
                                        break;
                                    case Constants.CON_VERHW:
                                        //if (firstFile) MyDefine.myXET.hom_Bat = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 15, colNum, strInfo);
                                        break;
                                    case Constants.CON_VERSW:
                                        //if (firstFile) MyDefine.myXET.hom_Bat = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 16, colNum, strInfo);
                                        break;
                                    case Constants.CON_ADDR:
                                        //if (firstFile) MyDefine.myXET.hom_Bat = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 17, colNum, strInfo);
                                        break;
                                    case Constants.CON_maxcore:
                                        //if (firstFile) MyDefine.myXET.hom_Bat = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 18, colNum, strInfo);
                                        break;
                                    case Constants.CON_mincore:
                                        //if (firstFile) MyDefine.myXET.hom_Bat = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 19, colNum, strInfo);
                                        break;
                                    case Constants.CON_rtc:
                                        //if (firstFile) MyDefine.myXET.hom_Bat = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 20, colNum, strInfo);
                                        break;
                                    case Constants.CON_htr:
                                        //if (firstFile) MyDefine.myXET.hom_Bat = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 21, colNum, strInfo);
                                        break;
                                    case Constants.CON_wdt:
                                        //if (firstFile) MyDefine.myXET.hom_Bat = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 22, colNum, strInfo);
                                        break;
                                    case Constants.CON_luk:
                                        //if (firstFile) MyDefine.myXET.hom_Bat = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 23, colNum, strInfo);
                                        break;
                                    case Constants.CON_mantime:
                                        //if (firstFile) MyDefine.myXET.hom_Bat = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 24, colNum, strInfo);
                                        break;
                                    case Constants.CON_battime:
                                        //if (firstFile) MyDefine.myXET.hom_Bat = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 25, colNum, strInfo);
                                        break;
                                    case Constants.CON_caltime:
                                        //if (firstFile) MyDefine.myXET.hom_Bat = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 26, colNum, strInfo);
                                        break;
                                    case Constants.CON_jintime:
                                        //if (firstFile) MyDefine.myXET.hom_Bat = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 27, colNum, strInfo);
                                        break;
                                    case Constants.CON_read:
                                        //if (firstFile) MyDefine.myXET.hom_Bat = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 28, colNum, strInfo);
                                        break;
                                    case Constants.CON_wake:
                                        //if (firstFile) MyDefine.myXET.hom_Bat = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 29, colNum, strInfo);
                                        break;
                                    case Constants.CON_interval:
                                        //if (firstFile) MyDefine.myXET.hom_Bat = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 30, colNum, strInfo);
                                        break;
                                    case Constants.CON_unit:
                                        //if (firstFile) MyDefine.myXET.hom_Bat = line.Substring(line.IndexOf('=') + 1);
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, 31, colNum, strInfo);
                                        break;
                                    case Constants.CON_hash:    //hash永远应该是最后一个参数
                                        if (firstFile)          //第一个文件读取完成
                                        {
                                            firstFile = false;
                                            maxRowNum = rowNum; //将第一个文件的数据行数定义为允许加载的最大行数
                                            MyDefine.myXET.homFileName = myfile;
                                        }
                                        rowNum = -1;                                         //复位文件当前行数
                                        fileHash = line.Substring(line.IndexOf('=') + 1);    //保存文件的哈希值
                                        addValToInfoTable(ref MyDefine.myXET.meInfoTbl, myDeviceType, Constants.InfoTblRowNum - 1, colNum, strInfo);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                if (!line.Contains(","))    //标题行
                                {
                                    colName = line.Split('#')[1].Trim();        //出厂编号，如TT20220101
                                    myDeviceType = (DEVICE)(Enum.Parse(typeof(DEVICE), "H" + colName.Substring(0, 2)));   //产品类型HTT/HTH/HTP

                                    if (myDeviceType == DEVICE.HTT || myDeviceType == DEVICE.HTP)
                                    {
                                        //仅添加一个数据列
                                        MyDefine.myXET.meDataTbl.addTableColumn(colName + " ");           //添加一个数据列(加空白字符，保持所有名称格式一致：HTx(_H)+JSN+1*char)
                                        MyDefine.myXET.meInfoTbl.addTableColumn(colName + " ");           //添加一个数据列(加空白字符，保持所有名称格式一致：HTx(_H)+JSN+1*char)
                                        colNum += 1;
                                    }
                                    else if (myDeviceType == DEVICE.HTH || myDeviceType == DEVICE.HTQ)
                                    {
                                        //添加第一列：温度值
                                        MyDefine.myXET.meDataTbl.addTableColumn(colName + "T");            //添加一个数据列
                                        MyDefine.myXET.meInfoTbl.addTableColumn(colName + "T");            //添加一个数据列
                                        colNum += 1;

                                        //添加第二列：湿度值
                                        MyDefine.myXET.meDataTbl.addTableColumn(colName + "H");            //添加一个数据列
                                        MyDefine.myXET.meInfoTbl.addTableColumn(colName + "H");            //添加一个数据列
                                        colNum += 1;
                                    }
                                }
                                else                                                    //数据行
                                {
                                    strHashSource += line + Environment.NewLine;        //将数据行加入哈希值数据源
                                    rowNum = rowNum + 1;                                //记录当前数据对应的数据表行数(索引，从0开始)
                                    string[] dataArr = line.Split(',');                 //"测试时间,测试数据,测试数据"
                                    int myRowNum = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;   //当前数据表的行数
                                    if (myRowNum <= rowNum)                             //当前数据超出数据表列数，则添加新行，并写入测试时间"Time"
                                    {
                                        MyDefine.myXET.meDataTbl.AddTableRow();         //添加新行
                                        MyDefine.myXET.meDataTbl.SetCellValue(rowNum, 0, dataArr[0].Trim());     //添加测试时间"Time"
                                    }

                                    switch (myDeviceType)
                                    {
                                        case DEVICE.HTT:    //温度采集器
                                            MyDefine.myXET.meDataTbl.SetCellValue(rowNum, colNum, dataArr[1].Trim());       //添加温度值
                                            if (!MyDefine.myXET.homunit.Contains("℃")) MyDefine.myXET.homunit += "/℃";  //添加文件数据单位(用于出报告)
                                            break;

                                        case DEVICE.HTH:    //温湿度采集器
                                            MyDefine.myXET.meDataTbl.SetCellValue(rowNum, colNum - 1, dataArr[1].Trim());   //添加温度值
                                            MyDefine.myXET.meDataTbl.SetCellValue(rowNum, colNum, dataArr[2].Trim());       //添加湿度值
                                            if (!MyDefine.myXET.homunit.Contains("℃")) MyDefine.myXET.homunit += "/℃";  //添加文件数据单位(用于出报告)
                                            if (!MyDefine.myXET.homunit.Contains("%RH")) MyDefine.myXET.homunit += "/%RH";//添加文件数据单位(用于出报告)
                                            break;

                                        case DEVICE.HTP:    //压力采集器
                                            MyDefine.myXET.meDataTbl.SetCellValue(rowNum, colNum, dataArr[1].Trim());       //添加压力值
                                            if (!MyDefine.myXET.homunit.Contains("kPa")) MyDefine.myXET.homunit += "/kPa";//添加文件数据单位(用于出报告)
                                            break;

                                        case DEVICE.HTQ:    //温湿度采集器
                                            MyDefine.myXET.meDataTbl.SetCellValue(rowNum, colNum - 1, dataArr[1].Trim());   //添加温度值
                                            MyDefine.myXET.meDataTbl.SetCellValue(rowNum, colNum, dataArr[2].Trim());       //添加湿度值
                                            if (!MyDefine.myXET.homunit.Contains("℃")) MyDefine.myXET.homunit += "/℃";  //添加文件数据单位(用于出报告)
                                            if (!MyDefine.myXET.homunit.Contains("%RH")) MyDefine.myXET.homunit += "/%RH";//添加文件数据单位(用于出报告)
                                            break;

                                        default:
                                            break;
                                    }
                                }
                            }
                        }

                        //校验每个文件的哈希值
                        if (!fileHash.Equals(MyDefine.myXET.getHashMD5(strHashSource)))  //哈希值校验失败
                        {
                            MyDefine.myXET.ShowWrongMsg("文件加载失败：" + Path.GetFileName(myfile) + "文件被修改，无法加载!");
                            MyDefine.myXET.meDataTbl = null;
                            MyDefine.myXET.meInfoTbl = null;
                            return false;
                        }

                    }

                    if (MyDefine.myXET.meDataTbl.dataTable.Columns.Count == 1)  //加载的都是空文件，只有初始时间列
                    {
                        MyDefine.myXET.ShowWrongMsg("文件加载失败：所加载的文件数据为空！");
                        MyDefine.myXET.meDataTbl = null;
                        MyDefine.myXET.meInfoTbl = null;
                        return false;
                    }

                    //加载成功
                    getDataTypeList();              //数据表各列数据类型标记（TT_T / TH_T / TH_H / TP_P）
                    getDataLimits();                //计算温度、湿度、压力的最大最小值(用于验证表计算)
                    DataDealing();                  //将数据表按温度、湿度、压力分成3个表
                    MyDefine.myXET.homunit = MyDefine.myXET.homunit.Trim('/');      //去掉头尾多长的字符'/'

                    MyDefine.myXET.AddTraceInfo("数据加载成功，加载文件数：" + fileNum);
                    //MyDefine.myXET.AddToTraceRecords("数据处理", "数据加载成功，加载文件数：" + fileNum);
                    MessageBox.Show("文件加载成功，已载入文件个数：" + fileNum, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
                else
                {
                    MyDefine.myXET.AddTraceInfo("取消加载");
                    return false;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                if (ex.GetType() == typeof(DuplicateNameException))
                {
                    MyDefine.myXET.ShowWrongMsg("文件加载失败：加载文件中存在重复设备编号！");
                }
                else
                {
                    MyDefine.myXET.ShowWrongMsg("文件加载失败：" + ex.ToString());
                }
                return false;
            }
        }

        #endregion

        #region 记录文件信息(开始时间、结束时间、持续时间等信息)

        private void addValToInfoTable(ref dataTableClass mTable, DEVICE model, int row, int col, string mStr)
        {
            if (mTable == null) return;     //数据表为空

            try
            {
                switch (model)
                {
                    case DEVICE.HTT:    //温度采集器
                        mTable.SetCellValue(row, col, mStr);       //添加温度信息
                        break;

                    case DEVICE.HTH:    //温湿度采集器
                        mTable.SetCellValue(row, col - 1, mStr);   //添加温度信息
                        mTable.SetCellValue(row, col, mStr);       //添加湿度信息
                        break;

                    case DEVICE.HTP:    //压力采集器
                        mTable.SetCellValue(row, col, mStr);       //添加压力信息
                        break;

                    case DEVICE.HTQ:    //温湿度采集器
                        mTable.SetCellValue(row, col - 1, mStr);   //添加温度信息
                        mTable.SetCellValue(row, col, mStr);       //添加湿度信息
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        #endregion

        #region 文件加载成功后，更新文件相关参数的全局变量

        //文件加载成功后，更新文件相关参数的全局变量
        public void UpdateGlobalParameters()
        {
            //以第一列即第一个文件的参数为基准
            MyDefine.myXET.homdate = MyDefine.myXET.meInfoTbl.GetCellValue(0, 1);            //测试日期
            MyDefine.myXET.homstart = MyDefine.myXET.meInfoTbl.GetCellValue(1, 1);           //起始时刻
            MyDefine.myXET.homstop = MyDefine.myXET.meInfoTbl.GetCellValue(2, 1);            //停止时刻
            MyDefine.myXET.homspan = MyDefine.myXET.meInfoTbl.GetCellValue(3, 1);            //间隔时间(秒)
            MyDefine.myXET.homrun = MyDefine.myXET.meInfoTbl.GetCellValue(4, 1);             //持续时间
            MyDefine.myXET.hom_UID = MyDefine.myXET.meInfoTbl.GetCellValue(5, 1);            //设备ID
            MyDefine.myXET.hom_Model = MyDefine.myXET.meInfoTbl.GetCellValue(6, 1);          //设备类型(YZLT30等)
            MyDefine.myXET.hom_JSN = MyDefine.myXET.meInfoTbl.GetCellValue(7, 1);            //出厂编号
            MyDefine.myXET.hom_Range = MyDefine.myXET.meInfoTbl.GetCellValue(8, 1);          //测量范围
            MyDefine.myXET.hom_Cal = MyDefine.myXET.meInfoTbl.GetCellValue(9, 1);            //校准日期
            MyDefine.myXET.hom_Rec = MyDefine.myXET.meInfoTbl.GetCellValue(10, 1);           //复校日期
            MyDefine.myXET.hom_Bat = MyDefine.myXET.meInfoTbl.GetCellValue(11, 1);           //电池电量
            MyDefine.myXET.hom_Type = MyDefine.myXET.meInfoTbl.GetCellValue(12, 1);          //设备型号HTT/HTH/HTP
            MyDefine.myXET.hom_USN = MyDefine.myXET.meInfoTbl.GetCellValue(13, 1);           //管理编号
            MyDefine.myXET.hom_UTXT = MyDefine.myXET.meInfoTbl.GetCellValue(14, 1);          //备注信息


            MyDefine.myXET.homunit = MyDefine.myXET.homunit.Trim('/');                      //去掉头尾多长的字符'/'
        }

        #endregion

        #region 数据表数据类型划分（TT_T / TH_T / TH_H / TP_P）

        /// <summary>
        /// 数据表各列数据类型划分,把所有数据列数据类型划分为四种： TT_T / TH_T / TH_H / TP_P
        /// </summary>
        public void getDataTypeList()
        {
            if (MyDefine.myXET.meInfoTbl == null) return;         //文件信息表为空，退出

            int tmpNum = 0;                 //温度列数量
            int t_humNum = 0;               //温湿度温度列数量
            int humNum = 0;                 //湿度列数量
            int prsNum = 0;                 //压力列数量
            int DUTNum = 0;                 //产品总数
            List<String> codelist = new List<String>(); //出厂编号列表
            List<String> typeList = new List<String>(); //数据类型列表TT_T / TH_T / TH_H / TP_P
            List<int> codeNumlist = new List<int>();    //数量编号列表，指明当前产品是第几个产品(HTH产品有两列，但两列是一个产品) -- 应该用不到

            codelist.Add("0");              //添加空值，对应meDataTbl的Time列
            codeNumlist.Add(0);             //添加0，对应meDataTbl的Time列
            typeList.Add("0");              //添加空值，对应meDataTbl的Time列

            //计算各类型标准器数量
            for (int idx = 1; idx < MyDefine.myXET.meInfoTbl.dataTable.Columns.Count; idx++)        //meInfoTbl第一列为空数据列
            {
                String myCode = MyDefine.myXET.meInfoTbl.GetCellValue(7, idx);      //出厂编号(注意不能用ColumnName，因为HTH分别在列名后面加了T、H)
                //String myCode = MyDefine.myXET.meInfoTbl.dataTable.Columns[idx].ColumnName;       //出厂编号(注意不能用ColumnName，因为HTH分别在列名后面加了T、H)
                if (!codelist.Contains(myCode.Substring(2))) //出厂编号尚未保存，产品数+1
                {
                    DUTNum++;                   //产品数+1
                    codeNumlist.Add(DUTNum);    //idx列数据对应的是第deviceNum个产品
                    codelist.Add(myCode.Substring(2));       //将产品编号存入列表

                    //计算标准器数量(各类型设备的数量)
                    if (myCode.Substring(0, 2).Contains("TT")) { tmpNum++; typeList.Add("TT_T"); }               //温度采集器的温度数据
                    else if (myCode.Substring(0, 2).Contains("TH")) { tmpNum++; t_humNum++; typeList.Add("TH_T"); }               //温湿度采集器的温度数据
                    else if (myCode.Substring(0, 2).Contains("TP")) { prsNum++; typeList.Add("TP_P"); }               //压力采集器的压力数据
                    else if (myCode.Substring(0, 2).Contains("TQ")) { tmpNum++; t_humNum++; typeList.Add("TQ_T"); }
                }
                else                            //出厂编号已存在，产品数不变
                {
                    codeNumlist.Add(DUTNum);    //idx列数据对应的是第deviceNum个产品
                    codelist.Add(myCode.Substring(2));       //将产品编号存入列表
                    if (myCode.Substring(0, 2).Contains("TT")) { tmpNum++; typeList.Add("TT_T"); }               //温度采集器的温度数据
                    else if (myCode.Substring(0, 2).Contains("TP")) { prsNum++; typeList.Add("TP_P"); }               //压力采集器的压力数据
                    else
                    {
                        if (humNum == t_humNum)
                        {
                            tmpNum++;
                            t_humNum++;
                            if (myCode.Substring(0, 2).Contains("TH")) typeList.Add("TH_T");
                            if (myCode.Substring(0, 2).Contains("TQ")) typeList.Add("TQ_T");     //温湿度采集器的湿度数据
                        }
                        else
                        {
                            humNum++;                   //湿度采集器+1
                            if (myCode.Substring(0, 2).Contains("TH")) typeList.Add("TH_H");
                            if (myCode.Substring(0, 2).Contains("TQ")) typeList.Add("TQ_H");     //温湿度采集器的湿度数据
                        }
                    }

                }
            }

            string mystr = "";
            for (int i = 0; i < typeList.Count; i++)
            {
                mystr += typeList[i] + ";";
            }

            //添加空值，对应meDataTbl的Time列
            MyDefine.myXET.meTmpList.Clear();
            MyDefine.myXET.meHumList.Clear();
            MyDefine.myXET.mePrsList.Clear();
            MyDefine.myXET.meTmpList.Add("0");
            MyDefine.myXET.meHumList.Add("0");
            MyDefine.myXET.mePrsList.Add("0");

            foreach (string str in typeList)
            {
                switch (str)
                {
                    case "TT_T":
                    case "TH_T":
                    case "TQ_T":
                        MyDefine.myXET.meTmpList.Add(str);
                        break;
                    case "TH_H":
                    case "TQ_H":
                        MyDefine.myXET.meHumList.Add(str);
                        break;
                    case "TP_P":
                        MyDefine.myXET.mePrsList.Add(str);
                        break;
                }
            }

            //更新全局变量
            MyDefine.myXET.meTMPNum = tmpNum;
            MyDefine.myXET.meHUMNum = humNum;
            MyDefine.myXET.mePRSNum = prsNum;
            MyDefine.myXET.meDUTNum = DUTNum;
            MyDefine.myXET.meJSNList = codelist;
            MyDefine.myXET.meTypeList = typeList;
            MyDefine.myXET.meAllList = typeList;
            MyDefine.myXET.codeNumlist = codeNumlist;

            MyDefine.myXET.meJSNListAll = codelist;
            if (typeList[1] == "TQ_T" || typeList[1] == "TH_T")
            {
                MyDefine.myXET.meJSNListPart.Clear();

                for (int i = 0; i < codelist.Count; i += 2)
                {
                    MyDefine.myXET.meJSNListPart.Add(codelist[i]);
                }
            }
            else
            {
                MyDefine.myXET.meJSNListPart = codelist;
            }
        }

        #endregion

        #region 计算温度、湿度、压力的最大最小值(用于验证表计算)

        //计算温度、湿度、压力的最大最小值
        public void getDataLimits()
        {
            try
            {
                if (MyDefine.myXET.meDataTbl == null) return;        //数据列表为空

                Double maxTmp = double.MinValue;
                Double minTmp = double.MaxValue;
                Double maxHum = double.MinValue;
                Double minHum = double.MaxValue;
                Double maxPsr = double.MinValue;
                Double minPsr = double.MaxValue;

                for (int i = 1; i < MyDefine.myXET.meDataTbl.dataTable.Columns.Count; i++)
                {
                    if (i >= MyDefine.myXET.meTypeList.Count) continue;       //不明原因，有时候会出现超出meTypeList索引范围异常
                    Double max = MyDefine.myXET.meDataTbl.GetColumnMaxVal(i);
                    Double min = MyDefine.myXET.meDataTbl.GetColumnMinVal(i);

                    switch (meTypeList[i])   //产品类型
                    {
                        case "TT_T":    //温度采集器
                            if (maxTmp < max) maxTmp = max;
                            if (minTmp > min) minTmp = min;
                            break;

                        case "TH_T":    //温湿度采集器
                            if (maxTmp < max) maxTmp = max;
                            if (minTmp > min) minTmp = min;
                            break;

                        case "TH_H":    //温湿度采集器
                            if (maxHum < max) maxHum = max;
                            if (minHum > min) minHum = min;
                            break;

                        case "TP_P":    //压力采集器
                            if (maxPsr < max) maxPsr = max;
                            if (minPsr > min) minPsr = min;
                            break;

                        default:
                            break;
                    }
                }

                MyDefine.myXET.meTMPMax = maxTmp;                //所有温度数据中的最大值
                MyDefine.myXET.meTMPMin = minTmp;                //所有温度数据中的最小值
                MyDefine.myXET.meHUMMax = maxHum;                //所有湿度数据中的最大值
                MyDefine.myXET.meHUMMin = minHum;                //所有湿度数据中的最小值
                MyDefine.myXET.mePRSMax = maxPsr;                //所有压力数据中的最大值
                MyDefine.myXET.mePRSMin = minPsr;                //所有压力数据中的最小值
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString().Split('\n')[0]);
            }
        }

        #endregion

        #region 数据文件处理(分类计算：将原始数据分为温度、湿度、压力三个表格，并分别计算最大、最小值)

        public void DataDealing()
        {
            if (meDataTbl == null) return;         //文件信息表为空，退出
            try
            {
                #region 添加列

                //添加列(复制原始数据表后，删除非数据表对应类型的列)
                meDataAllTbl = new dataTableClass();
                meDataTmpTbl = new dataTableClass();
                meDataHumTbl = new dataTableClass();
                meDataPrsTbl = new dataTableClass();

                meDataTblAll = new dataTableClass();
                meDataTblTmp = new dataTableClass();
                meDataTblHum = new dataTableClass();
                meDataTblPrs = new dataTableClass();

                meInfoAllTbl = new dataTableClass();
                meInfoTmpTbl = new dataTableClass();
                meInfoHumTbl = new dataTableClass();
                meInfoPrsTbl = new dataTableClass();

                meDataAllTbl.dataTable = meDataTbl.CopyTable();
                meDataTmpTbl.dataTable = meDataTbl.CopyTable();
                meDataHumTbl.dataTable = meDataTbl.CopyTable();
                meDataPrsTbl.dataTable = meDataTbl.CopyTable();

                meDataTblAll.dataTable = meDataTbl.CopyTable();
                meDataTblTmp.dataTable = meDataTbl.CopyTable();
                meDataTblHum.dataTable = meDataTbl.CopyTable();
                meDataTblPrs.dataTable = meDataTbl.CopyTable();

                meInfoAllTbl.dataTable = meInfoTbl.CopyTable();
                meInfoTmpTbl.dataTable = meInfoTbl.CopyTable();
                meInfoHumTbl.dataTable = meInfoTbl.CopyTable();
                meInfoPrsTbl.dataTable = meInfoTbl.CopyTable();

                //删除不需要的列(需要的测试数据被保留在了表中)
                for (int i = 0; i < meDataTbl.dataTable.Columns.Count; i++)
                {
                    if (i >= MyDefine.myXET.meTypeList.Count) continue;
                    string colname = meDataTbl.dataTable.Columns[i].ColumnName;     //列名
                    switch (MyDefine.myXET.meTypeList[i])
                    {
                        case "TT_T":
                        case "TH_T":
                        case "TQ_T":
                            meDataHumTbl.DeleteTableColumn(colname);
                            meDataPrsTbl.DeleteTableColumn(colname);
                            meDataTblHum.DeleteTableColumn(colname);
                            meDataTblPrs.DeleteTableColumn(colname);
                            break;

                        case "TH_H":
                        case "TQ_H":
                            meDataTmpTbl.DeleteTableColumn(colname);
                            meDataPrsTbl.DeleteTableColumn(colname);
                            meDataTblTmp.DeleteTableColumn(colname);
                            meDataTblPrs.DeleteTableColumn(colname);
                            break;

                        case "TP_P":
                            meDataTmpTbl.DeleteTableColumn(colname);
                            meDataHumTbl.DeleteTableColumn(colname);
                            meDataTblTmp.DeleteTableColumn(colname);
                            meDataTblHum.DeleteTableColumn(colname);
                            break;
                    }
                }

                //删除不需要的列(需要的测试数据被保留在了表中)
                for (int i = 0; i < meInfoTbl.dataTable.Columns.Count; i++)
                {
                    if (i >= meTypeList.Count) continue;
                    string colname = meDataTbl.dataTable.Columns[i].ColumnName;     //列名
                    if (MyDefine.myXET.meTypeList[i].EndsWith("T"))
                    {
                        meInfoHumTbl.DeleteTableColumn(colname);
                        meInfoPrsTbl.DeleteTableColumn(colname);
                    }
                    else if (MyDefine.myXET.meTypeList[i].EndsWith("H"))
                    {
                        meInfoTmpTbl.DeleteTableColumn(colname);
                        meInfoPrsTbl.DeleteTableColumn(colname);
                    }
                    else if (MyDefine.myXET.meTypeList[i].EndsWith("P"))
                    {
                        meInfoTmpTbl.DeleteTableColumn(colname);
                        meInfoHumTbl.DeleteTableColumn(colname);
                    }
                }

                //添加统计列(尚无数据)
                meDataAllTbl.addTableColumn("采样次数", 0);      //在索引0处插入列
                meDataAllTbl.addTableColumn("阶段", 0);          //在索引0处插入列

                meDataTmpTbl.addTableColumn("采样次数", 0);      //在索引0处插入列
                meDataTmpTbl.addTableColumn("阶段", 0);          //在索引0处插入列
                meDataTmpTbl.addTableColumn("最大值");
                meDataTmpTbl.addTableColumn("最小值");
                meDataTmpTbl.addTableColumn("最大-最小");
                meDataTmpTbl.addTableColumn("平均值");

                meDataHumTbl.addTableColumn("采样次数", 0);      //在索引0处插入列
                meDataHumTbl.addTableColumn("阶段", 0);          //在索引0处插入列
                meDataHumTbl.addTableColumn("最大值");
                meDataHumTbl.addTableColumn("最小值");
                meDataHumTbl.addTableColumn("最大-最小");
                meDataHumTbl.addTableColumn("平均值");

                meDataPrsTbl.addTableColumn("采样次数", 0);      //在索引0处插入列
                meDataPrsTbl.addTableColumn("阶段", 0);          //在索引0处插入列
                meDataPrsTbl.addTableColumn("最大值");
                meDataPrsTbl.addTableColumn("最小值");
                meDataPrsTbl.addTableColumn("最大-最小");
                meDataPrsTbl.addTableColumn("平均值");

                #endregion

                #region 添加数据行，并计算各行的最大最小值

                //分别计算每行温度、湿度、压力最大最小值，并添加进表格
                for (int i = 0; i < MyDefine.myXET.meDataTbl.dataTable.Rows.Count; i++)
                {
                    //写入采样次数
                    meDataAllTbl.SetCellValue(i, "采样次数", (i + 1).ToString());
                    meDataTmpTbl.SetCellValue(i, "采样次数", (i + 1).ToString());
                    meDataHumTbl.SetCellValue(i, "采样次数", (i + 1).ToString());
                    meDataPrsTbl.SetCellValue(i, "采样次数", (i + 1).ToString());

                    //计算各行最大最小值
                    Double myTMPMax = meDataTmpTbl.GetRowMaxVal(i, 3);      //行温度最大值
                    Double myTMPMin = meDataTmpTbl.GetRowMinVal(i, 3);      //行温度最小值
                    Double myTMPAvr = meDataTmpTbl.GetRowAvrVal(i, 3);      //行温度平均值
                    Double myHUMMax = meDataHumTbl.GetRowMaxVal(i, 3);      //行湿度最大值
                    Double myHUMMin = meDataHumTbl.GetRowMinVal(i, 3);      //行湿度最小值
                    Double myHUMAvr = meDataHumTbl.GetRowAvrVal(i, 3);      //行湿度平均值
                    Double myPRSMax = meDataPrsTbl.GetRowMaxVal(i, 3);      //行压力最大值
                    Double myPRSMin = meDataPrsTbl.GetRowMinVal(i, 3);      //行压力最小值
                    Double myPRSAvr = meDataPrsTbl.GetRowAvrVal(i, 3);      //行压力平均值

                    if (meTMPNum > 0 && myTMPMax != double.MinValue)        //若本行温度数据均为空，则myTMPMax=double.MinValue
                    {
                        //生成温度表新行
                        meDataTmpTbl.SetCellValue(i, "最大值", myTMPMax.ToString("F2"));
                        meDataTmpTbl.SetCellValue(i, "最小值", myTMPMin.ToString("F2"));
                        meDataTmpTbl.SetCellValue(i, "最大-最小", (myTMPMax - myTMPMin).ToString("F2"));
                        meDataTmpTbl.SetCellValue(i, "平均值", myTMPAvr.ToString("F2"));
                    }

                    if (meHUMNum > 0 && myHUMMax != double.MinValue)        //若本行湿度数据均为空，则myHUMMax=double.MinValue
                    {
                        //生成湿度表新行
                        meDataHumTbl.SetCellValue(i, "最大值", myHUMMax.ToString("F2"));
                        meDataHumTbl.SetCellValue(i, "最小值", myHUMMin.ToString("F2"));
                        meDataHumTbl.SetCellValue(i, "最大-最小", (myHUMMax - myHUMMin).ToString("F2"));
                        meDataHumTbl.SetCellValue(i, "平均值", myHUMAvr.ToString("F2"));
                    }

                    if (mePRSNum > 0 && myPRSMax != double.MinValue)        //若本行压力数据均为空，则myPRSMax=double.MinValue
                    {
                        //生成压力表新行
                        meDataPrsTbl.SetCellValue(i, "最大值", myPRSMax.ToString("F2"));
                        meDataPrsTbl.SetCellValue(i, "最小值", myPRSMin.ToString("F2"));
                        meDataPrsTbl.SetCellValue(i, "最大-最小", (myPRSMax - myPRSMin).ToString("F2"));
                        meDataPrsTbl.SetCellValue(i, "平均值", myPRSAvr.ToString("F2"));
                    }
                }

                #endregion

                #region 添加统计行，并计算各列的最大最小值

                if (meTMPNum > 0)             //存在温度数据，则各列的和、平均值
                {
                    int rownum = MyDefine.myXET.meDataTmpTbl.dataTable.Rows.Count - 1;
                    meDataTmpTbl.AddTableRow(new String[] { "", "", "最大值" });
                    meDataTmpTbl.AddTableRow(new String[] { "", "", "最小值" });
                    meDataTmpTbl.AddTableRow(new String[] { "", "", "最大-最小" });
                    meDataTmpTbl.AddTableRow(new String[] { "", "", "平均值" });

                    for (int i = 3; i < meDataTmpTbl.dataTable.Columns.Count - 4; i++)   //第一列为阶段名称，第二列为序号，第三列为时间，后四列为统计值（最大最小等）
                    {
                        double mymax = MyDefine.myXET.meDataTmpTbl.GetColumnMaxVal(i);        //求列的最大值
                        double mymin = MyDefine.myXET.meDataTmpTbl.GetColumnMinVal(i);        //求列的最小值
                        double myavr = MyDefine.myXET.meDataTmpTbl.GetColumnAvrVal(i);        //求列的平均值
                        double mysum = MyDefine.myXET.meDataTmpTbl.GetColumnSumVal(i);        //求列的总和
                        meDataTmpTbl.SetCellValue(rownum + 1, i, mymax.ToString("F2"));
                        meDataTmpTbl.SetCellValue(rownum + 2, i, mymin.ToString("F2"));
                        meDataTmpTbl.SetCellValue(rownum + 3, i, (mymax - mymin).ToString("F2"));
                        meDataTmpTbl.SetCellValue(rownum + 4, i, myavr.ToString("F2"));
                        colmax = Math.Max(mymax, colmax);
                        colmin = Math.Min(mymin, colmin);
                        totalsum += mysum;
                        for (int j = 0; j < rownum + 1; j++)
                        {
                            if (meDataTmpTbl.IsCellEmpty(j, i) == false)
                            {
                                number++;
                            }
                        }
                    }
                    double myMaxcolmax = MyDefine.myXET.meDataTmpTbl.GetColumnMaxVal(meDataTmpTbl.dataTable.Columns.Count - 4);        //求最大值列的最大值
                    totalmax = Math.Max(myMaxcolmax, totalmax);

                    double myMincolmin = MyDefine.myXET.meDataTmpTbl.GetColumnMinVal(meDataTmpTbl.dataTable.Columns.Count - 3);        //求最小值列的最小值
                    totalmin = Math.Min(myMincolmin, totalmin);

                    meDataTmpTbl.SetCellValue(rownum + 1, meDataTmpTbl.dataTable.Columns.Count - 4, (Math.Max(totalmax, colmax)).ToString("F2"));
                    meDataTmpTbl.SetCellValue(rownum + 2, meDataTmpTbl.dataTable.Columns.Count - 3, (Math.Min(totalmin, colmin)).ToString("F2"));
                    meDataTmpTbl.SetCellValue(rownum + 3, meDataTmpTbl.dataTable.Columns.Count - 2, (Math.Max(totalmax, colmax) - Math.Min(totalmin, colmin)).ToString("F2"));
                    meDataTmpTbl.SetCellValue(rownum + 4, meDataTmpTbl.dataTable.Columns.Count - 1, (totalsum / number).ToString("F2"));

                    totalmax = double.MinValue;
                    totalmin = double.MaxValue;
                    colmax = double.MinValue;
                    colmin = double.MaxValue;
                    totalsum = 0;
                    number = 0;
                }

                if (meHUMNum > 0)             //存在湿度数据，则各列的和、平均值
                {
                    int rownum = MyDefine.myXET.meDataHumTbl.dataTable.Rows.Count - 1;
                    meDataHumTbl.AddTableRow(new String[] { "", "", "最大值" });
                    meDataHumTbl.AddTableRow(new String[] { "", "", "最小值" });
                    meDataHumTbl.AddTableRow(new String[] { "", "", "最大-最小" });
                    meDataHumTbl.AddTableRow(new String[] { "", "", "平均值" });

                    for (int i = 3; i < meDataHumTbl.dataTable.Columns.Count - 4; i++)   //第一列为阶段名称，第二列为序号，第三列为时间，后四列为统计值（最大最小等）
                    {
                        double mymax = meDataHumTbl.GetColumnMaxVal(i);        //求列的最大值
                        double mymin = meDataHumTbl.GetColumnMinVal(i);        //求列的最小值
                        double myavr = meDataHumTbl.GetColumnAvrVal(i);        //求列的平均值
                        double mysum = meDataHumTbl.GetColumnSumVal(i);        //求列的总和
                        meDataHumTbl.SetCellValue(rownum + 1, i, mymax.ToString("F2"));
                        meDataHumTbl.SetCellValue(rownum + 2, i, mymin.ToString("F2"));
                        meDataHumTbl.SetCellValue(rownum + 3, i, (mymax - mymin).ToString("F2"));
                        meDataHumTbl.SetCellValue(rownum + 4, i, myavr.ToString("F2"));
                        colmax = Math.Max(mymax, colmax);
                        colmin = Math.Min(mymin, colmin);
                        totalsum += mysum;
                        for (int j = 0; j < rownum + 1; j++)
                        {
                            if (meDataHumTbl.IsCellEmpty(j, i) == false)
                            {
                                number++;
                            }
                        }
                    }

                    double myMaxcolmax = MyDefine.myXET.meDataHumTbl.GetColumnMaxVal(meDataHumTbl.dataTable.Columns.Count - 4);        //求最大值列的最大值
                    totalmax = Math.Max(myMaxcolmax, totalmax);

                    double myMincolmin = MyDefine.myXET.meDataHumTbl.GetColumnMinVal(meDataHumTbl.dataTable.Columns.Count - 3);        //求最小值列的最小值
                    totalmin = Math.Min(myMincolmin, totalmin);

                    meDataHumTbl.SetCellValue(rownum + 1, meDataHumTbl.dataTable.Columns.Count - 4, (Math.Max(totalmax, colmax)).ToString("F2"));
                    meDataHumTbl.SetCellValue(rownum + 2, meDataHumTbl.dataTable.Columns.Count - 3, (Math.Min(totalmin, colmin)).ToString("F2"));
                    meDataHumTbl.SetCellValue(rownum + 3, meDataHumTbl.dataTable.Columns.Count - 2, (Math.Max(totalmax, colmax) - Math.Min(totalmin, colmin)).ToString("F2"));
                    meDataHumTbl.SetCellValue(rownum + 4, meDataHumTbl.dataTable.Columns.Count - 1, (totalsum / number).ToString("F2"));

                    totalmax = double.MinValue;
                    totalmin = double.MaxValue;
                    colmax = double.MinValue;
                    colmin = double.MaxValue;
                    totalsum = 0;
                    number = 0;
                }

                if (mePRSNum > 0)             //存在压力数据，则各列的和、平均值
                {
                    int rownum = MyDefine.myXET.meDataPrsTbl.dataTable.Rows.Count - 1;
                    meDataPrsTbl.AddTableRow(new String[] { "", "", "最大值" });
                    meDataPrsTbl.AddTableRow(new String[] { "", "", "最小值" });
                    meDataPrsTbl.AddTableRow(new String[] { "", "", "最大-最小" });
                    meDataPrsTbl.AddTableRow(new String[] { "", "", "平均值" });

                    for (int i = 3; i < meDataPrsTbl.dataTable.Columns.Count - 4; i++)   //第一列为阶段名称，第二列为序号，第三列为时间，后四列为统计值（最大最小等）
                    {
                        double mymax = meDataPrsTbl.GetColumnMaxVal(i);        //求列的最大值
                        double mymin = meDataPrsTbl.GetColumnMinVal(i);        //求列的最小值
                        double myavr = meDataPrsTbl.GetColumnAvrVal(i);        //求列的平均值
                        double mysum = meDataPrsTbl.GetColumnSumVal(i);        //求列的总和
                        meDataPrsTbl.SetCellValue(rownum + 1, i, mymax.ToString("F2"));
                        meDataPrsTbl.SetCellValue(rownum + 2, i, mymin.ToString("F2"));
                        meDataPrsTbl.SetCellValue(rownum + 3, i, (mymax - mymin).ToString("F2"));
                        meDataPrsTbl.SetCellValue(rownum + 4, i, myavr.ToString("F2"));
                        colmax = Math.Max(mymax, colmax);
                        colmin = Math.Min(mymin, colmin);
                        totalsum += mysum;
                        for (int j = 0; j < rownum + 1; j++)
                        {
                            if (meDataPrsTbl.IsCellEmpty(j, i) == false)
                            {
                                number++;
                            }
                        }
                    }

                    double myMaxcolmax = MyDefine.myXET.meDataPrsTbl.GetColumnMaxVal(meDataPrsTbl.dataTable.Columns.Count - 4);        //求最大值列的最大值
                    totalmax = Math.Max(myMaxcolmax, totalmax);

                    double myMincolmin = MyDefine.myXET.meDataPrsTbl.GetColumnMinVal(meDataPrsTbl.dataTable.Columns.Count - 3);        //求最小值列的最小值
                    totalmin = Math.Min(myMincolmin, totalmin);

                    meDataPrsTbl.SetCellValue(rownum + 1, meDataPrsTbl.dataTable.Columns.Count - 4, (Math.Max(totalmax, colmax)).ToString("F2"));
                    meDataPrsTbl.SetCellValue(rownum + 2, meDataPrsTbl.dataTable.Columns.Count - 3, (Math.Min(totalmin, colmin)).ToString("F2"));
                    meDataPrsTbl.SetCellValue(rownum + 3, meDataPrsTbl.dataTable.Columns.Count - 2, (Math.Max(totalmax, colmax) - Math.Min(totalmin, colmin)).ToString("F2"));
                    meDataPrsTbl.SetCellValue(rownum + 4, meDataPrsTbl.dataTable.Columns.Count - 1, (totalsum / number).ToString("F2"));

                    totalmax = double.MinValue;
                    totalmin = double.MaxValue;
                    colmax = double.MinValue;
                    colmin = double.MaxValue;
                    totalsum = 0;
                    number = 0;
                }

                #endregion

            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("原始数据表分类生成失败：" + ex.ToString());
                //MyDefine.myXET.meDataTmpTbl = null;
                //MyDefine.myXET.meDataHumTbl = null;
                //MyDefine.myXET.meDataPrsTbl = null;
            }
        }

        #endregion

        #region 数据文件处理(分类计算：将原始数据分为温度、湿度、压力三个表格，并分别计算最大、最小值)0

        public void DataDealing0()
        {
            if (MyDefine.myXET.meDataTbl == null) return;         //文件信息表为空，退出
            try
            {
                #region 变量

                Double myVal = 0;               //测试值
                int tmpMaxIdx = 0;              //行温度最大值的列索引
                int tmpMinIdx = 0;              //行温度最小值的列索引
                int humMaxIdx = 0;              //行湿度最大值的列索引
                int humMinIdx = 0;              //行湿度最小值的列索引
                int prsMaxIdx = 0;              //行压力最大值的列索引
                int prsMinIdx = 0;              //行压力最小值的列索引

                #endregion

                #region 添加列

                //添加列
                MyDefine.myXET.meDataTmpTbl = new dataTableClass();
                MyDefine.myXET.meDataTmpTbl.addTableColumn("采样次数");            //第0栏
                MyDefine.myXET.meDataTmpTbl.addTableColumn("时间");                //第1栏

                MyDefine.myXET.meDataHumTbl = new dataTableClass();
                MyDefine.myXET.meDataHumTbl.addTableColumn("采样次数");            //第0栏
                MyDefine.myXET.meDataHumTbl.addTableColumn("时间");                //第1栏

                MyDefine.myXET.meDataPrsTbl = new dataTableClass();
                MyDefine.myXET.meDataPrsTbl.addTableColumn("采样次数");            //第0栏
                MyDefine.myXET.meDataPrsTbl.addTableColumn("时间");                //第1栏

                for (int i = 0; i < meTypeList.Count; i++)
                {
                    switch (meTypeList[i])
                    {
                        case "TT_T":
                        case "TH_T":
                            MyDefine.myXET.meDataTmpTbl.addTableColumn(meJSNList[i]);
                            break;

                        case "TH_H":
                            MyDefine.myXET.meDataHumTbl.addTableColumn(meJSNList[i]);
                            break;

                        case "TP_P":
                            MyDefine.myXET.meDataPrsTbl.addTableColumn(meJSNList[i]);
                            break;
                    }
                }

                MyDefine.myXET.meDataTmpTbl.addTableColumn("最大值");
                MyDefine.myXET.meDataTmpTbl.addTableColumn("最小值");
                MyDefine.myXET.meDataTmpTbl.addTableColumn("最大-最小");
                MyDefine.myXET.meDataTmpTbl.addTableColumn("平均值");

                MyDefine.myXET.meDataHumTbl.addTableColumn("最大值");
                MyDefine.myXET.meDataHumTbl.addTableColumn("最小值");
                MyDefine.myXET.meDataHumTbl.addTableColumn("最大-最小");
                MyDefine.myXET.meDataHumTbl.addTableColumn("平均值");

                MyDefine.myXET.meDataPrsTbl.addTableColumn("最大值");
                MyDefine.myXET.meDataPrsTbl.addTableColumn("最小值");
                MyDefine.myXET.meDataPrsTbl.addTableColumn("最大-最小");
                MyDefine.myXET.meDataPrsTbl.addTableColumn("平均值");

                #endregion

                #region 添加数据行，并计算各行的最大最小值

                //计算数据表MyDefine.myXET.meDataTbl每行的最大最小值，并添加进表格
                for (int i = 0; i < MyDefine.myXET.meDataTbl.dataTable.Rows.Count; i++)
                {
                    Double meTMPAvr = 0;                    //行温度平均值
                    Double meTMPMax = Double.MinValue;      //行温度最大值
                    Double meTMPMin = Double.MaxValue;      //行温度最小值
                    Double meHUMAvr = 0;                    //行温度平均值
                    Double meHUMMax = Double.MinValue;      //行湿度最大值
                    Double meHUMMin = Double.MaxValue;      //行湿度最小值
                    Double mePRSAvr = 0;                    //行温度平均值
                    Double mePRSMax = Double.MinValue;      //行压力最大值
                    Double mePRSMin = Double.MaxValue;      //行压力最小值
                    String myTime = MyDefine.myXET.meDataTbl.GetCellValue(i, 0);         //测试时间
                    MyDefine.myXET.meDataTmpTbl.AddTableRow(new String[] { (i + 1).ToString(), myTime });//添加新行，写入采样次数和时间
                    MyDefine.myXET.meDataHumTbl.AddTableRow(new String[] { (i + 1).ToString(), myTime });//添加新行，写入采样次数和时间
                    MyDefine.myXET.meDataPrsTbl.AddTableRow(new String[] { (i + 1).ToString(), myTime });//添加新行，写入采样次数和时间

                    //计算每一行的温度/湿度/压力的最大最小值
                    for (int j = 1; j < MyDefine.myXET.meDataTbl.dataTable.Columns.Count; j++)        //meDataTbl第0列为Time
                    {
                        if (MyDefine.myXET.meDataTbl.GetCellValue(i, j) == "") continue;              //空数据
                        myVal = Convert.ToDouble(MyDefine.myXET.meDataTbl.GetCellValue(i, j));        //测试值

                        //计算行最大最小值
                        switch (meTypeList[j])
                        {
                            case "TT_T":
                            case "TH_T":
                                if (meTMPMax < myVal) { meTMPMax = myVal; tmpMaxIdx = j; }
                                if (meTMPMin > myVal) { meTMPMin = myVal; tmpMinIdx = j; }
                                MyDefine.myXET.meDataTmpTbl.SetCellValue(i, meJSNList[j], myVal.ToString());
                                meTMPAvr += myVal;
                                break;

                            case "TH_H":
                                if (meHUMMax < myVal) { meHUMMax = myVal; humMaxIdx = j; }
                                if (meHUMMin > myVal) { meHUMMin = myVal; humMinIdx = j; }
                                MyDefine.myXET.meDataHumTbl.SetCellValue(i, meJSNList[j], myVal.ToString());
                                meHUMAvr += myVal;
                                break;

                            case "TP_P":
                                if (mePRSMax < myVal) { mePRSMax = myVal; prsMaxIdx = j; }
                                if (mePRSMin > myVal) { mePRSMin = myVal; prsMinIdx = j; }
                                MyDefine.myXET.meDataPrsTbl.SetCellValue(i, meJSNList[j], myVal.ToString());
                                mePRSAvr += myVal;
                                break;
                        }
                    }

                    if (meTMPNum > 0 && meTMPMax != double.MinValue)        //若本行温度数据均为空，则meTMPMax=double.MinValue
                    {
                        //生成温度表新行
                        String max = meTMPMax.ToString("F2");
                        String maxIdx = codeNumlist[tmpMaxIdx].ToString();
                        String min = meTMPMin.ToString("F2");
                        String minIdx = codeNumlist[tmpMinIdx].ToString();
                        String avr = (meTMPAvr / meTMPNum).ToString("F2");
                        String max_min = (meTMPMax - meTMPMin).ToString("F2");

                        MyDefine.myXET.meDataTmpTbl.SetCellValue(i, "最大值", max.ToString());
                        MyDefine.myXET.meDataTmpTbl.SetCellValue(i, "最小值", min.ToString());
                        MyDefine.myXET.meDataTmpTbl.SetCellValue(i, "最大-最小", max_min.ToString());
                        MyDefine.myXET.meDataTmpTbl.SetCellValue(i, "平均值", avr.ToString());
                    }

                    if (meHUMNum > 0 && meHUMMax != double.MinValue)        //若本行湿度数据均为空，则meHUMMax=double.MinValue
                    {
                        //生成湿度表新行
                        String max = meHUMMax.ToString("F2");
                        String maxIdx = codeNumlist[humMaxIdx].ToString();
                        String min = meHUMMin.ToString("F2");
                        String minIdx = codeNumlist[humMinIdx].ToString();
                        String avr = (meHUMAvr / meHUMNum).ToString("F2");
                        String max_min = (meHUMMax - meHUMMin).ToString("F2");

                        MyDefine.myXET.meDataHumTbl.SetCellValue(i, "最大值", max.ToString());
                        MyDefine.myXET.meDataHumTbl.SetCellValue(i, "最小值", min.ToString());
                        MyDefine.myXET.meDataHumTbl.SetCellValue(i, "最大-最小", max_min.ToString());
                        MyDefine.myXET.meDataHumTbl.SetCellValue(i, "平均值", avr.ToString());
                    }

                    if (mePRSNum > 0 && mePRSMax != double.MinValue)        //若本行压力数据均为空，则mePRSMax=double.MinValue
                    {
                        //生成压力表新行
                        String max = mePRSMax.ToString("F2");
                        String maxIdx = codeNumlist[prsMaxIdx].ToString();
                        String min = mePRSMin.ToString("F2");
                        String minIdx = codeNumlist[prsMinIdx].ToString();
                        String avr = (mePRSAvr / mePRSNum).ToString("F2");
                        String max_min = (mePRSMax - mePRSMin).ToString("F2");

                        MyDefine.myXET.meDataPrsTbl.SetCellValue(i, "最大值", max.ToString());
                        MyDefine.myXET.meDataPrsTbl.SetCellValue(i, "最小值", min.ToString());
                        MyDefine.myXET.meDataPrsTbl.SetCellValue(i, "最大-最小", max_min.ToString());
                        MyDefine.myXET.meDataPrsTbl.SetCellValue(i, "平均值", avr.ToString());
                    }
                }

                #endregion

                #region 添加统计行，并计算各列的最大最小值

                if (meTMPNum > 0)             //存在温度数据，则各列的和、平均值
                {
                    int rownum = MyDefine.myXET.meDataTmpTbl.dataTable.Rows.Count - 1;
                    MyDefine.myXET.meDataTmpTbl.AddTableRow(new String[] { "", "最大值" });
                    MyDefine.myXET.meDataTmpTbl.AddTableRow(new String[] { "", "最小值" });
                    MyDefine.myXET.meDataTmpTbl.AddTableRow(new String[] { "", "最大-最小" });
                    MyDefine.myXET.meDataTmpTbl.AddTableRow(new String[] { "", "平均值" });

                    for (int i = 2; i < meDataTmpTbl.dataTable.Columns.Count - 4; i++)   //第1列为序号，第二列为时间，后四列为统计值（最大最小等）
                    {
                        double mymax = MyDefine.myXET.meDataTmpTbl.GetColumnMaxVal(i);        //求列的最大值
                        double mymin = MyDefine.myXET.meDataTmpTbl.GetColumnMinVal(i);        //求列的最小值
                        double myavr = MyDefine.myXET.meDataTmpTbl.GetColumnAvrVal(i);        //求列的平均值
                        MyDefine.myXET.meDataTmpTbl.SetCellValue(rownum + 1, i, mymax.ToString("F2"));
                        MyDefine.myXET.meDataTmpTbl.SetCellValue(rownum + 2, i, mymin.ToString("F2"));
                        MyDefine.myXET.meDataTmpTbl.SetCellValue(rownum + 3, i, (mymax - mymin).ToString("F2"));
                        MyDefine.myXET.meDataTmpTbl.SetCellValue(rownum + 4, i, myavr.ToString("F2"));
                    }
                }

                if (meHUMNum > 0)             //存在湿度数据，则各列的和、平均值
                {
                    int rownum = MyDefine.myXET.meDataHumTbl.dataTable.Rows.Count - 1;
                    MyDefine.myXET.meDataHumTbl.AddTableRow(new String[] { "", "最大值" });
                    MyDefine.myXET.meDataHumTbl.AddTableRow(new String[] { "", "最小值" });
                    MyDefine.myXET.meDataHumTbl.AddTableRow(new String[] { "", "最大-最小" });
                    MyDefine.myXET.meDataHumTbl.AddTableRow(new String[] { "", "平均值" });

                    for (int i = 2; i < meDataHumTbl.dataTable.Columns.Count - 4; i++)   //第1列为序号，第二列为时间，后四列为统计值（最大最小等）
                    {
                        double mymax = MyDefine.myXET.meDataHumTbl.GetColumnMaxVal(i);        //求列的最大值
                        double mymin = MyDefine.myXET.meDataHumTbl.GetColumnMinVal(i);        //求列的最小值
                        double myavr = MyDefine.myXET.meDataHumTbl.GetColumnAvrVal(i);        //求列的平均值
                        MyDefine.myXET.meDataHumTbl.SetCellValue(rownum + 1, i, mymax.ToString("F2"));
                        MyDefine.myXET.meDataHumTbl.SetCellValue(rownum + 2, i, mymin.ToString("F2"));
                        MyDefine.myXET.meDataHumTbl.SetCellValue(rownum + 3, i, (mymax - mymin).ToString("F2"));
                        MyDefine.myXET.meDataHumTbl.SetCellValue(rownum + 4, i, myavr.ToString("F2"));
                    }
                }

                if (mePRSNum > 0)             //存在压力数据，则各列的和、平均值
                {
                    int rownum = MyDefine.myXET.meDataPrsTbl.dataTable.Rows.Count - 1;
                    MyDefine.myXET.meDataPrsTbl.AddTableRow(new String[] { "", "最大值" });
                    MyDefine.myXET.meDataPrsTbl.AddTableRow(new String[] { "", "最小值" });
                    MyDefine.myXET.meDataPrsTbl.AddTableRow(new String[] { "", "最大-最小" });
                    MyDefine.myXET.meDataPrsTbl.AddTableRow(new String[] { "", "平均值" });

                    for (int i = 2; i < meDataPrsTbl.dataTable.Columns.Count - 4; i++)   //第1列为序号，第二列为时间，后四列为统计值（最大最小等）
                    {
                        double mymax = MyDefine.myXET.meDataPrsTbl.GetColumnMaxVal(i);        //求列的最大值
                        double mymin = MyDefine.myXET.meDataPrsTbl.GetColumnMinVal(i);        //求列的最小值
                        double myavr = MyDefine.myXET.meDataPrsTbl.GetColumnAvrVal(i);        //求列的平均值
                        MyDefine.myXET.meDataPrsTbl.SetCellValue(rownum + 1, i, mymax.ToString("F2"));
                        MyDefine.myXET.meDataPrsTbl.SetCellValue(rownum + 2, i, mymin.ToString("F2"));
                        MyDefine.myXET.meDataPrsTbl.SetCellValue(rownum + 3, i, (mymax - mymin).ToString("F2"));
                        MyDefine.myXET.meDataPrsTbl.SetCellValue(rownum + 4, i, myavr.ToString("F2"));
                    }
                }

                #endregion

            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("原始数据表分类生成失败：" + ex.ToString());
                //MyDefine.myXET.meDataTmpTbl = null;
                //MyDefine.myXET.meDataHumTbl = null;
                //MyDefine.myXET.meDataPrsTbl = null;
            }
        }

        #endregion

        #endregion

        #region 整体导出.csv格式的Excel数据(DataTable导出到csv)

        //将DataTable数据导出到Excel -- 导出后缀为.csv格式的Excel，数据以","分隔
        public static void ExportToCSVExcel(DataTable dataTable, string fileName, bool isOpen = false)
        {
            try
            {
                //将所有时间列后面加上"\t"(按文本格式显示)，否则导出的csv中时间列不显示秒数据
                for (int i = 0; i < dataTable.Columns.Count; i++)         //遍历所有列
                {
                    string colname = dataTable.Columns[i].ColumnName;
                    if (colname.Contains("时间"))                         //列名中包含时间二字
                    {
                        for (int j = 0; j < dataTable.Rows.Count; j++)    //遍历所有行
                        {
                            dataTable.Rows[j][colname] = dataTable.Rows[j][colname].ToString() + '\t';
                        }
                    }
                }

                //导出CSV
                var lines = new List<string>();
                string[] columnNames = dataTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();
                var header = string.Join(",", columnNames);
                lines.Add(header);

                var valueLines = dataTable.AsEnumerable().Select(row => string.Join(",", row.ItemArray));         //日期无法完整显示，数字前导0无法显示
                //var valueLines = dataTable.AsEnumerable().Select(row => string.Join("\t" + ",", row.ItemArray));    //在单元格内容后加入制表符"\t"，这样所有单元格都是按文本格式显示，较长数字和日期都可以完整显示
                lines.AddRange(valueLines);

                File.WriteAllLines(fileName, lines, System.Text.Encoding.Default);      //不加System.Text.Encoding.Default，则导出的中文是乱码
                if (isOpen) Process.Start(fileName);

                MyDefine.myXET.ShowCorrectMsg("导出成功！");
                MyDefine.myXET.AddTraceInfo("导出成功");
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.AddTraceInfo("导出失败");
                MyDefine.myXET.ShowWrongMsg("导出Excel失败：" + ex.ToString());
            }
        }

        //将DataTable数据导出到Excel -- 导出后缀为.csv格式的Excel，数据以","分隔
        //不在每次导出文件后显示“导出成功”
        public static void ExportToCSVExcelTypeTwo(DataTable dataTable, string fileName, bool isOpen = false)
        {
            try
            {
                //将所有时间列后面加上"\t"(按文本格式显示)，否则导出的csv中时间列不显示秒数据
                for (int i = 0; i < dataTable.Columns.Count; i++)         //遍历所有列
                {
                    string colname = dataTable.Columns[i].ColumnName;
                    if (colname.Contains("时间"))                         //列名中包含时间二字
                    {
                        for (int j = 0; j < dataTable.Rows.Count; j++)    //遍历所有行
                        {
                            dataTable.Rows[j][colname] = dataTable.Rows[j][colname].ToString() + '\t';
                        }
                    }
                }

                //导出CSV
                var lines = new List<string>();
                string[] columnNames = dataTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();
                var header = string.Join(",", columnNames);
                lines.Add(header);

                var valueLines = dataTable.AsEnumerable().Select(row => string.Join(",", row.ItemArray));         //日期无法完整显示，数字前导0无法显示
                                                                                                                  //var valueLines = dataTable.AsEnumerable().Select(row => string.Join("\t" + ",", row.ItemArray));    //在单元格内容后加入制表符"\t"，这样所有单元格都是按文本格式显示，较长数字和日期都可以完整显示
                lines.AddRange(valueLines);

                File.WriteAllLines(fileName, lines, System.Text.Encoding.Default);      //不加System.Text.Encoding.Default，则导出的中文是乱码

                if (isOpen) Process.Start(fileName);

                MyDefine.myXET.AddTraceInfo("导出成功");
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.AddTraceInfo("Excel导出失败");
                MyDefine.myXET.ShowWrongMsg("Excel导出失败：" + ex.ToString());
            }
        }

        public static void ExportToExcel(DataTable dt, string fileName)
        {
            try
            {
                //创建一个工作簿
                IWorkbook workbook = new HSSFWorkbook();

                //创建一个 sheet 表
                ISheet sheet = workbook.CreateSheet("数据表");

                //创建一行
                IRow rowH = sheet.CreateRow(0);

                //创建一个单元格
                ICell cell = null;

                //创建单元格样式
                ICellStyle cellStyle = workbook.CreateCellStyle();

                //创建格式
                IDataFormat dataFormat = workbook.CreateDataFormat();

                //设置为两位小数
                cellStyle.DataFormat = dataFormat.GetFormat("0.00");

                //设置列名
                foreach (DataColumn col in dt.Columns)
                {
                    //创建单元格并设置单元格内容
                    rowH.CreateCell(col.Ordinal).SetCellValue(col.Caption);

                    //设置单元格格式
                    rowH.Cells[col.Ordinal].CellStyle = cellStyle;
                }

                //写入数据
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //跳过第一行，第一行为列名
                    IRow row = sheet.CreateRow(i + 1);

                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        cell = row.CreateCell(j);
                        cell.SetCellValue(dt.Rows[i][j].ToString());
                        cell.CellStyle = cellStyle;
                    }
                }

                //创建文件
                FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write);

                //创建一个 IO 流
                MemoryStream ms = new MemoryStream();

                //写入到流
                workbook.Write(ms);

                //转换为字节数组
                byte[] bytes = ms.ToArray();

                file.Write(bytes, 0, bytes.Length);
                file.Flush();

                //释放资源
                bytes = null;

                ms.Close();
                ms.Dispose();

                file.Close();
                file.Dispose();

                // workbook.();
                sheet = null;
                workbook = null;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.AddTraceInfo("Excel导出失败");
                MyDefine.myXET.ShowWrongMsg("Excel导出失败：" + ex.ToString());
            }
        }


        #endregion

        #endregion

        #region 串口通信日志

        /// <summary>
        /// 保存通信日志，用于问题分析
        /// </summary>
        public void SaveCommunicationTips()
        {
            String filePath = MyDefine.myXET.userLOG + "\\" + DateTime.Now.ToString("yyyyMMdd");

            //没有文件夹则创建
            if (!Directory.Exists(MyDefine.myXET.userLOG))
            {
                Directory.CreateDirectory(MyDefine.myXET.userLOG);
            }

            //使用FileMode.Append，若文件不存在则新建；若文件存在则在文件末尾追加文本
            using (FileStream file = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                using (StreamWriter meWrite = new StreamWriter(file))
                {
                    meWrite.WriteLine("=======================" + DateTime.Now.ToString("yyyyMMdd HH:mm:ss") + "=======================");
                    meWrite.WriteLine(MyDefine.myXET.meTips);
                    MyDefine.myXET.meTips = "";
                }
            }
        }

        /// <summary>
        /// 保存通信日志，用于程序调试
        /// </summary>
        public void SaveLogTips(Boolean flag = false)
        {
            String filePath = MyDefine.myXET.userLOG + "\\" + DateTime.Now.ToString("yyyyMMdd");

            //没有文件夹则创建
            if (!Directory.Exists(MyDefine.myXET.userLOG))
            {
                Directory.CreateDirectory(MyDefine.myXET.userLOG);
            }

            //使用FileMode.Append，若文件不存在则新建；若文件存在则在文件末尾追加文本
            using (FileStream file = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                using (StreamWriter meWrite = new StreamWriter(file))
                {
                    if (flag) meWrite.WriteLine("=================" + DateTime.Now.ToString("yyyyMMdd HH:mm:ss") + "================");
                    meWrite.WriteLine(MyDefine.myXET.meTips);
                    MyDefine.myXET.meTips = "";
                }
            }
        }

        /// <summary>
        /// 添加通信记录
        /// </summary>
        /// <param name="type"></param>
        /// <param name="step"></param>
        public void AddToCOMMRecords(string msg)
        {
            meTips += msg + Environment.NewLine;
        }

        #endregion

        #region 审计追踪日志

        #region 添加审计追踪日志

        /// <summary>
        /// 添加审计追踪记录
        /// </summary>
        /// <param name="type">类型：界面名称</param>
        /// <param name="step"></param>
        public void AddToTraceRecords(string type, string info)
        {
            info = info.Replace(",", "");                   //导出csv文件时，英文的逗号会被识别为单元格分隔符
            info = info.Replace(Environment.NewLine, "");   //去掉换行符，否则加载时会抛出异常
            string msg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ";";
            msg += userName + ";";
            msg += type + ";";
            msg += info + ";";
            msg += meComputer + Environment.NewLine;

            MyDefine.myXET.meRecords.Append(msg);
        }

        /// <summary>
        /// 添加审计追踪记录
        /// </summary>
        /// <param name="info">审计追踪信息</param>
        public void AddTraceInfo(string info)
        {
            string type = meInterface;
            info = info.Replace(",", "");                   //导出csv文件时，英文的逗号会被识别为单元格分隔符
            info = info.Replace(Environment.NewLine, "");   //去掉换行符，否则加载时会抛出异常
            string msg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ";";
            msg += userName + ";";
            msg += type + ";";
            msg += info + ";";
            msg += meComputer + Environment.NewLine;

            MyDefine.myXET.meRecords.Append(msg);
        }

        #endregion

        #region 保存审计追踪日志(加密)

        /// <summary>
        /// 加密并保存
        /// </summary>
        public void SaveTraceRecords()
        {
            String filePath = MyDefine.myXET.userLOG + "\\" + DateTime.Now.ToString("yyyyMMdd") + "rcd";

            //没有文件夹则创建
            if (!Directory.Exists(MyDefine.myXET.userLOG))
            {
                Directory.CreateDirectory(MyDefine.myXET.userLOG);
            }

            //使用FileMode.Append，若文件不存在则新建；若文件存在则在文件末尾追加文本
            using (FileStream file = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                using (StreamWriter meWrite = new StreamWriter(file))
                {
                    meWrite.Write(TextEncrypt(MyDefine.myXET.meRecords.ToString(), password));     //加密并保存(只能用简单的密码，单个字母或数字等，否则会出错)
                }
            }

            MyDefine.myXET.meRecords.Clear();
        }

        /// <summary>
        /// 仅保存
        /// </summary>
        public void SaveTraceRecords0()
        {
            String filePath = MyDefine.myXET.userLOG + "\\" + DateTime.Now.ToString("yyyyMMdd") + "rcd";

            //没有文件夹则创建
            if (!Directory.Exists(MyDefine.myXET.userLOG))
            {
                Directory.CreateDirectory(MyDefine.myXET.userLOG);
            }

            //使用FileMode.Append，若文件不存在则新建；若文件存在则在文件末尾追加文本
            using (FileStream file = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                using (StreamWriter meWrite = new StreamWriter(file))
                {
                    meWrite.Write(MyDefine.myXET.meRecords);     //保存
                }
            }

            MyDefine.myXET.meRecords.Clear();
        }

        #endregion

        #region 加载审计追踪日志(解密)

        /// <summary>
        /// 加载并解密
        /// </summary>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        public void LoadTraceRecords(DateTime start, DateTime stop)
        {
            try
            {
                //加载之前清除所有行，只保留表头
                meTraceTbl.ClearTableData();

                String mypath = userLOG;
                String[] myfiles = Directory.GetFiles(mypath);
                String[] contents = new String[0];
                List<String> fileList = new List<String>();
                if (myfiles.Length == 0) return;

                foreach (string myfile in myfiles)
                {
                    if (myfile.Contains("rcd") == false) continue;          //非追踪文件
                    if (isInDate(myfile, start, stop) == false) continue;   //文件创建时间超出范围
                    fileList.Add(myfile);
                }

                if (fileList.Count > 0)
                {
                    foreach (string myfile in fileList)
                    {
                        String AllText = File.ReadAllText(myfile);      //读取所有内容并解密
                        AllText = TextDecrypt(AllText, password);       //解密
                        String[] myLines = AllText.Split('\n');         //按行分隔
                        if (myLines.Length == 0) continue;
                        /*
                        //从后往前加载追踪列表(最后生成的追踪信息显示在最前面)
                        for (int i = myLines.Length - 1; i >= 0; i--)
                        {
                            if (myLines[i] == string.Empty) continue;
                            contents = myLines[i].Split(';');
                            meTraceTbl.AddTableRow(contents);
                        }
                        */
                        //从前往后加载追踪列表(最后生成的追踪信息显示在最后面)
                        for (int i = 0; i <= myLines.Length - 1; i++)
                        {
                            if (myLines[i] == string.Empty) continue;
                            contents = myLines[i].Split(';');
                            meTraceTbl.AddTableRow(contents);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// 仅加载
        /// </summary>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        public void LoadTraceRecords0(DateTime start, DateTime stop)
        {
            try
            {
                //加载之前清除所有行，只保留表头
                meTraceTbl.ClearTableData();

                String mypath = userLOG;
                String[] myfiles = Directory.GetFiles(mypath);
                String[] contents = new String[0];
                List<String> fileList = new List<String>();
                if (myfiles.Length == 0) return;

                foreach (string myfile in myfiles)
                {
                    if (myfile.Contains("rcd") == false) continue;          //非追踪文件
                    if (isInDate(myfile, start, stop) == false) continue;   //文件创建时间超出范围
                    fileList.Add(myfile);
                }

                if (fileList.Count > 0)
                {
                    foreach (string myfile in fileList)
                    {
                        String[] myLines = File.ReadAllLines(myfile);
                        if (myLines.Length == 0) continue;

                        //从后往前加载追踪列表(最后生成的追踪信息显示在最前面)
                        for (int i = myLines.Length - 1; i >= 0; i--)
                        {
                            if (myLines[i] == string.Empty) continue;
                            contents = myLines[i].Split(';');
                            meTraceTbl.AddTableRow(contents);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// 文件名日期是否包含在范围内
        /// </summary>
        /// <param name="myfile"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        public Boolean isInDate(string myfile, DateTime start, DateTime stop)
        {
            try
            {
                String filename = Path.GetFileName(myfile).Substring(0, 8);     //获取文件名里的日期 “20220101rcd.tmp” -> “20220101”
                DateTime filedate = DateTime.ParseExact(filename, "yyyyMMdd", null);    //文件名转为日期
                if (filedate.CompareTo(start.Date) < 0 || filedate.CompareTo(stop.Date) > 0) return false;    //文件创建时间超出范围(只比较日期，不比较具体时间)
                return true;
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }

        #endregion

        #endregion

        #region PDF报告列表

        #region 保存PDF报告列表

        /// <summary>
        /// 保存PDF报告列表
        /// </summary>
        public void SavePDFList(String pdfinfo)
        {
            //没有文件夹则创建
            if (!Directory.Exists(MyDefine.myXET.userSAV))
            {
                Directory.CreateDirectory(MyDefine.myXET.userSAV);
            }

            //使用FileMode.Append，若文件不存在则新建；若文件存在则在文件末尾追加文本
            String filePath = MyDefine.myXET.userSAV + "\\rptlist.sav";      //保存在固定文件内
            using (FileStream file = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                using (StreamWriter meWrite = new StreamWriter(file))
                {
                    meWrite.WriteLine(pdfinfo);
                }
            }
        }

        /// <summary>
        /// 保存PDF报告列表
        /// </summary>
        public void SavePDFList0(String pdfinfo)
        {
            String filePath = MyDefine.myXET.userSAV + "\\" + DateTime.Now.ToString("yyyyMMdd") + "sss";

            //没有文件夹则创建
            if (!Directory.Exists(MyDefine.myXET.userOUT))
            {
                Directory.CreateDirectory(MyDefine.myXET.userOUT);
            }

            //使用FileMode.Append，若文件不存在则新建；若文件存在则在文件末尾追加文本
            using (FileStream file = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                using (StreamWriter meWrite = new StreamWriter(file))
                {
                    meWrite.WriteLine(pdfinfo);
                }
            }
        }

        #endregion

        #region 加载PDF报告列表

        public void LoadPDFList()
        {
            try
            {
                //加载之前清除所有行，只保留表头
                mePDFTbl.ClearTableData();
                String filePath = MyDefine.myXET.userSAV + "\\rptlist.sav";

                //文件存在则加载
                if (File.Exists(filePath))
                {
                    int linenum = 1;
                    String[] contents = new String[0];
                    String[] myLines = File.ReadAllLines(filePath);
                    if (myLines.Length == 0) return;

                    //从后往前加载报告列表(最后生成的PDF信息显示在最前面)
                    for (int i = myLines.Length - 1; i >= 0; i--)
                    {
                        if (myLines[i] == string.Empty) continue;
                        String myline = (linenum++).ToString() + ";" + myLines[i];
                        contents = myline.Split(';');
                        mePDFTbl.AddTableRow(contents);
                    }
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        public void LoadPDFList0(DateTime start, DateTime stop)
        {
            try
            {
                //加载之前清除所有行，只保留表头
                mePDFTbl.ClearTableData();

                String mypath = userOUT;
                String[] myfiles = Directory.GetFiles(mypath);
                String[] contents = new String[0];
                List<String> fileList = new List<String>();
                if (myfiles.Length == 0) return;

                foreach (string myfile in myfiles)
                {
                    if (myfile.Contains("sss") == false) continue;          //非追踪文件
                    if (isInDate(myfile, start, stop) == false) continue;   //文件创建时间超出范围
                    fileList.Add(myfile);
                }

                if (fileList.Count > 0)
                {
                    foreach (string myfile in fileList)
                    {
                        int linenum = 1;
                        String[] myLines = File.ReadAllLines(myfile);
                        if (myLines.Length == 0) continue;

                        //从后往前加载报告列表(最后生成的PDF信息显示在最前面)
                        for (int i = myLines.Length - 1; i >= 0; i--)
                        {
                            if (myLines[i] == string.Empty) continue;
                            String myline = (linenum++).ToString() + ";" + myLines[i];
                            contents = myline.Split(';');
                            mePDFTbl.AddTableRow(contents);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        #endregion

        #region 保存报告信息(报告编号+标准器列表)

        /// <summary>
        /// 保存报告信息(报告编号+标准器列表)
        /// </summary>
        /// <returns></returns>
        public Boolean saveReportInfo()
        {
            String filepath = MyDefine.myXET.userSAV + @"\stdtbl.sav";
            FileStream meFS = new FileStream(filepath, FileMode.Create, FileAccess.Write);
            StreamWriter meWrite = new StreamWriter(meFS);

            try
            {
                meWrite.WriteLine(MyDefine.myXET.repcode);                  //保存报告编号
                meWrite.WriteLine(MyDefine.myXET.compyPhone);               //保存公司电话

                if (MyDefine.myXET.meTblSTD != null)
                {
                    string tablelines = MyDefine.myXET.meTblSTD.joinToString();
                    meWrite.WriteLine(tablelines);                          //保存标准器数据表
                }
                return true;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("保存报告编号及标准器数据表失败：" + ex.ToString());
                return false;
            }
            finally
            {
                meWrite.Close();
                meFS.Close();
            }
        }

        #endregion

        #region 加载报告信息(报告编号+标准器列表)

        /// <summary>
        /// 加载报告信息(报告编号+标准器列表)
        /// </summary>
        /// <returns></returns>
        public Boolean loadReportInfo()
        {
            if (!Directory.Exists(MyDefine.myXET.userSAV))
            {
                Directory.CreateDirectory(MyDefine.myXET.userSAV);
            }

            Boolean ret = true;
            string myline = string.Empty;
            string flowcode = string.Empty;
            String filepath = MyDefine.myXET.userSAV + @"\stdtbl.sav";

            try
            {
                if (File.Exists(filepath))
                {
                    //读取用户信息
                    FileStream meFS = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                    StreamReader meRead = new StreamReader(meFS);

                    if (!meRead.EndOfStream)            //不是空文件
                    {
                        MyDefine.myXET.repcode = meRead.ReadLine();     //加载报告编号：JDYZ20211109xxx
                    }

                    if (!meRead.EndOfStream)            //未到文件末尾
                    {
                        MyDefine.myXET.compyPhone = meRead.ReadLine();  //加载公司电话
                    }

                    if (!meRead.EndOfStream)            //未到文件末尾
                    {
                        myline = meRead.ReadLine();     //加载标准器表列信息：标准器名称,标准器编号,xxx

                        //添加标准器表列信息
                        MyDefine.myXET.meTblSTD = new dataTableClass();
                        MyDefine.myXET.meTblSTD.addTableColumn(myline.Split(','));
                    }

                    while (!meRead.EndOfStream)         //未到文件末尾
                    {
                        //添加标准器表行信息
                        myline = meRead.ReadLine();
                        MyDefine.myXET.meTblSTD.AddTableRow(myline.Split(','));
                    }

                    meRead.Close();
                    meRead.Dispose();
                    meFS.Close();
                    meFS.Dispose();
                }
                else
                {
                    ret = false;
                    //if(meInterface == "标准器录入") MyDefine.myXET.ShowWrongMsg("读取失败：stdtbl.sav文件不存在！");
                }

                ret = true;
                if (meInterface == "标准器录入") MyDefine.myXET.ShowWrongMsg("读取成功！");

                return ret;
            }
            catch (Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("标准器数据加载失败：" + ex.ToString());
                return false;
            }
        }

        #endregion

        #endregion

        #region 校准/标定表(手动输入表)

        #region 保存校准/标定表1(手动输入表)

        /// <summary>
        /// 程序关闭前保存校准/标定表1(不显示弹窗)
        /// </summary>
        /// <returns></returns>
        public void savePreCalTable()
        {
            savePreTable(false);
            saveCalTable(false);
        }

        /// <summary>
        /// 保存校准表1(手动输入表)
        /// </summary>
        /// <returns></returns>
        public void savePreTable(Boolean showbox = true)
        {
            if (MyDefine.myXET.meTblPre1 == null)
            {
                MyDefine.myXET.ShowWrongMsg("保存失败：当前数据表为空！", showbox);
                return;
            }

            if (MyDefine.myXET.meTblPre1.IsRowEmpty(0, 1))   //判断第0行读数时间有没有值，若均为空，则不保存
            {
                MyDefine.myXET.ShowWrongMsg("保存失败：当前数据表为空！", showbox);
                return;
            }

            String filepath = MyDefine.myXET.userSAV + @"\pretbl.sav";
            FileStream meFS = new FileStream(filepath, FileMode.Create, FileAccess.Write);
            StreamWriter meWrite = new StreamWriter(meFS);

            try
            {
                meWrite.WriteLine("[校准表]");                                      //校准表
                string tablelines = MyDefine.myXET.meTblPre1.joinToString();        //保存校准表
                meWrite.Write(tablelines);                                          //保存校准表

                MyDefine.myXET.ShowCorrectMsg("保存成功！", showbox);
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("校准表保存失败：" + ex.ToString());
            }
            finally
            {
                meWrite.Close();
                meFS.Close();
            }
        }

        /// <summary>
        /// 保存标定表1(手动输入表)
        /// </summary>
        /// <returns></returns>
        public void saveCalTable(Boolean showbox = true)
        {
            if (MyDefine.myXET.meTblCal1 == null)
            {
                MyDefine.myXET.ShowWrongMsg("保存失败：当前数据表为空！", showbox);
                return;
            }

            if (MyDefine.myXET.meTblCal1.IsRowEmpty(0, 1))   //判断第0行读数时间有没有值，若均为空，则不保存
            {
                MyDefine.myXET.ShowWrongMsg("保存失败：当前数据表为空！", showbox);
                return;
            }

            String filepath = MyDefine.myXET.userSAV + @"\caltbl.sav";
            FileStream meFS = new FileStream(filepath, FileMode.Create, FileAccess.Write);
            StreamWriter meWrite = new StreamWriter(meFS);

            try
            {
                meWrite.WriteLine("[标定表]");                                      //标定表
                string tablelines = MyDefine.myXET.meTblCal1.joinToString();        //保存标定表
                meWrite.Write(tablelines);                                          //保存标定表

                MyDefine.myXET.ShowCorrectMsg("保存成功！", showbox);
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("标定表保存失败：" + ex.ToString());
            }
            finally
            {
                meWrite.Close();
                meFS.Close();
            }
        }

        /// <summary>
        /// 保存校准/标定表1(手动输入表)
        /// </summary>
        /// <returns></returns>
        public void savePreCalTable0()
        {
            String filepath = MyDefine.myXET.userSAV + @"\caltbl.sav";
            FileStream meFS = new FileStream(filepath, FileMode.Create, FileAccess.Write);
            StreamWriter meWrite = new StreamWriter(meFS);

            try
            {
                meWrite.WriteLine("[校准表]");                                     //校准表
                if (MyDefine.myXET.meTblPre1 != null)
                {
                    string tablelines = MyDefine.myXET.meTblPre1.joinToString();   //保存校准表
                    meWrite.Write(tablelines);                                     //保存校准表
                }

                meWrite.WriteLine("[标定表]");                                     //标定表
                if (MyDefine.myXET.meTblCal1 != null)
                {
                    string tablelines = MyDefine.myXET.meTblCal1.joinToString();   //保存标定表
                    meWrite.Write(tablelines);                                     //保存标定表
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("保存报告编号及标准器数据表失败：" + ex.ToString());
            }
            finally
            {
                meWrite.Close();
                meFS.Close();
            }
        }

        #endregion

        #region 加载校准/标定表1(手动输入表)

        /// <summary>
        /// 加载校准/标定表1(手动输入表)
        /// </summary>
        /// <returns></returns>
        public void loadPreCalTable()
        {
            loadPreTable();
            loadCalTable();
        }

        /// <summary>
        /// 加载校准1(手动输入表)
        /// </summary>
        /// <returns></returns>
        public Boolean loadPreTable()
        {
            if (!Directory.Exists(MyDefine.myXET.userSAV))
            {
                Directory.CreateDirectory(MyDefine.myXET.userSAV);
            }

            int rownum = 0;
            string myline = string.Empty;
            string flowcode = string.Empty;
            String filepath = MyDefine.myXET.userSAV + @"\pretbl.sav";

            try
            {
                if (File.Exists(filepath))
                {
                    //读取用户信息
                    FileStream meFS = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                    StreamReader meRead = new StreamReader(meFS);

                    myline = meRead.ReadLine();         //"[校准表]"

                    //加载校准表头
                    if (!meRead.EndOfStream)            //未到文件末尾
                    {
                        myline = meRead.ReadLine();
                    }

                    //添加校准表行信息
                    while (!meRead.EndOfStream)         //未到文件末尾
                    {
                        myline = meRead.ReadLine();

                        if (MyDefine.myXET.meTblPre1.dataTable.Rows.Count > rownum)
                            MyDefine.myXET.meTblPre1.SetRowArray(rownum++, 1, myline.Split(','));
                    }

                    meRead.Close();
                    meRead.Dispose();
                    meFS.Close();
                    meFS.Dispose();
                }
                else
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("加载校准表1失败：" + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 加载标定表1(手动输入表)
        /// </summary>
        /// <returns></returns>
        public Boolean loadCalTable()
        {
            if (!Directory.Exists(MyDefine.myXET.userSAV))
            {
                Directory.CreateDirectory(MyDefine.myXET.userSAV);
            }

            int rownum = 0;
            string myline = string.Empty;
            string flowcode = string.Empty;
            String filepath = MyDefine.myXET.userSAV + @"\caltbl.sav";

            try
            {
                if (File.Exists(filepath))
                {
                    //读取用户信息
                    FileStream meFS = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                    StreamReader meRead = new StreamReader(meFS);

                    myline = meRead.ReadLine();         //"[标定表]"

                    //加载标定表头
                    if (!meRead.EndOfStream)            //未到文件末尾
                    {
                        myline = meRead.ReadLine();
                    }

                    //添加标定表行信息
                    while (!meRead.EndOfStream)         //未到文件末尾
                    {
                        myline = meRead.ReadLine();

                        if (MyDefine.myXET.meTblCal1.dataTable.Rows.Count > rownum)
                            MyDefine.myXET.meTblCal1.SetRowArray(rownum++, 1, myline.Split(','));
                    }

                    meRead.Close();
                    meRead.Dispose();
                    meFS.Close();
                    meFS.Dispose();

                }
                else
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("加载标定表1失败：" + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 加载校准/标定表1(手动输入表)
        /// </summary>
        /// <returns></returns>
        public Boolean loadPreCalTable0()
        {
            if (!Directory.Exists(MyDefine.myXET.userSAV))
            {
                Directory.CreateDirectory(MyDefine.myXET.userSAV);
            }

            int rownum = 0;
            string myline = string.Empty;
            string flowcode = string.Empty;
            String filepath = MyDefine.myXET.userSAV + @"\caltbl.sav";

            try
            {
                if (File.Exists(filepath))
                {
                    //读取用户信息
                    FileStream meFS = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                    StreamReader meRead = new StreamReader(meFS);

                    myline = meRead.ReadLine();         //"[校准表]"

                    //加载校准表头
                    if (!meRead.EndOfStream)            //未到文件末尾
                    {
                        myline = meRead.ReadLine();
                        //MyDefine.myXET.meTblPre1 = new dataTableClass();
                        //MyDefine.myXET.meTblPre1.addTableColumn(myline.Split(','));
                    }

                    //添加校准表行信息
                    while (!meRead.EndOfStream)         //未到文件末尾
                    {
                        myline = meRead.ReadLine();
                        if (myline == "[标定表]") { rownum = 0; break; }

                        //MyDefine.myXET.meTblPre1.AddTableRow(myline.Split(','));
                        if (MyDefine.myXET.meTblPre1.dataTable.Rows.Count > rownum)
                            MyDefine.myXET.meTblPre1.SetRowArray(rownum++, 1, myline.Split(','));
                    }

                    //加载标定表头
                    if (!meRead.EndOfStream)            //未到文件末尾
                    {
                        myline = meRead.ReadLine();
                        //MyDefine.myXET.meTblCal1 = new dataTableClass();
                        //MyDefine.myXET.meTblCal1.addTableColumn(myline.Split(','));
                    }

                    //添加标定表行信息
                    while (!meRead.EndOfStream)         //未到文件末尾
                    {
                        myline = meRead.ReadLine();
                        //MyDefine.myXET.meTblCal1.AddTableRow(myline.Split(','));
                        if (MyDefine.myXET.meTblCal1.dataTable.Rows.Count > rownum)
                            MyDefine.myXET.meTblCal1.SetRowArray(rownum++, 1, myline.Split(','));
                    }

                    meRead.Close();
                    meRead.Dispose();
                    meFS.Close();
                    meFS.Dispose();
                }

                return true;
            }
            catch (Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("加载校准表1失败：" + ex.ToString());
                return false;
            }
        }

        #endregion

        #endregion

        #region 创建初始账号和权限类别

        /// <summary>
        /// 创建初始账号和权限类别
        /// </summary>
        public void CheckInitAccount()
        {
            if (!Directory.Exists(MyDefine.myXET.userCFG))
            {
                Directory.CreateDirectory(MyDefine.myXET.userCFG);
            }

            try
            {
                DirectoryInfo meDirectory = new DirectoryInfo(MyDefine.myXET.userCFG);
                int myUserNum = meDirectory.GetFiles("user.*.cfg").Length;      //账号个数
                int myCateNum = meDirectory.GetFiles("cate.*.cfg").Length;      //权限类别个数

                //账号个数为0，创建初始账号
                if (myUserNum == 0)
                {
                    SaveUserFile("admin", "admin;admin;;;1;0;0001;");        //账号admin，密码admin，是否默认账号，是否记住密码，权限类别编号0000(admin)
                }

                //权限类别个数为0，创建初始权限类别
                if (myCateNum == 0)
                {
                    SavePermCatFile("admin", "0001;admin;AAAAAAAAAAAAAAAAAAABBAAAAAABBBBAABBB");     //权类编号;权类名称;权类列表
                }
            }
            catch (Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("用户列表加载失败：" + ex.ToString());
            }
        }

        #endregion

        #region 账号和密码操作

        #region 登录--核对用户名密码是否正确

        /// <summary>
        /// 检查用户名密码是否正确
        /// </summary>
        /// <param name="username">用户输入的用户名</param>
        /// <param name="password">用户输入的密码</param>
        /// <returns>1=核对正确；0=程序异常；-1=用户名不存在；-2=密码错误</returns>
        public int CheckUserAccount(string username, string password)
        {
            try
            {
                for (int i = 0; i < MyDefine.myXET.meListUser.Count; i++)                   //遍历账号列表
                {
                    string myUserName = MyDefine.myXET.meListUser[i].Split(';')[(int)ACCOUNT.USER];    //用户名
                    string myCateCode = MyDefine.myXET.meListUser[i].Split(';')[(int)ACCOUNT.CATCODE]; //权限类别编号
                    if (myUserName.Equals(username))                                        //此账号存在
                    {
                        string mypassword = MyDefine.myXET.meListUser[i].Split(';')[(int)ACCOUNT.PSWD];
                        if (mypassword.Equals(password))                                    //账户名密码核对正确
                        {
                            MyDefine.myXET.meLoginUser = MyDefine.myXET.meListUser[i];       //"用户名;密码;部门;电话;权类编号;"
                            MyDefine.myXET.meLoginUser += GetUserPermName(myCateCode) + ";"; //"权类编号对应的权限类别名称"
                            MyDefine.myXET.meLoginUser += GetUserPermList(myCateCode);       //"权类编号对应的权限列表"(如果权限不存在，则返回BBB...)
                            return 1;
                        }

                        return -2;                                                 //密码错误
                    }
                }

                return -1;                                                         //账号名不存在

            }
            catch (Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("用户名密码核对失败：" + ex.ToString());
                return 0;           //发生异常
            }
        }

        #endregion

        #region 登录成功--保存当前登录账号为默认账号

        /// <summary>
        /// 保存当前登录账号为默认账号
        /// </summary>
        /// <param name="loginuser">当前登录账号</param>
        /// <param name="rempsd">是否保存账号密码</param>
        public void SaveAsDefaultAccount(string loginuser, string rempsd)
        {
            string isdefult = loginuser.Split(';')[(int)ACCOUNT.DEFUSR];              //当前账号是否默认账号
            string isrember = loginuser.Split(';')[(int)ACCOUNT.REMPSD];              //当前账号是否记住密码
            if (isdefult == "1" && isrember == rempsd) return;                        //当前账号已是默认账号，且是否记住密码没有变化，则不再保存账号信息

            //将原账号列表中的默认账号修改为非默认账号(meListUser:账号;密码;部门;电话;是否默认账号;是否记住密码;权限类别编号)
            for (int i = 0; i < MyDefine.myXET.meListUser.Count; i++)                   //遍历账号列表
            {
                string myIsDef = MyDefine.myXET.meListUser[i].Split(';')[(int)ACCOUNT.DEFUSR];      //当前账号是否默认账号
                //MessageBox.Show(MyDefine.myXET.meListUser[i] + "," + myIsDef);
                if (myIsDef == "1")                                                                 //此账号为默认账号，将其修改为非默认账号
                {
                    string myUserName = MyDefine.myXET.meListUser[i].Split(';')[(int)ACCOUNT.USER]; //用户名
                    string[] arrAccount = MyDefine.myXET.meListUser[i].Split(';');
                    arrAccount[(int)ACCOUNT.DEFUSR] = "0";
                    string account = string.Join(";", arrAccount);
                    SaveUserFile(myUserName, account);                                              //保存为非默认账号
                }
            }

            //保存当前账号为默认账号(loginuser:账号;密码;部门;电话;是否默认账号;是否记住密码;权限类别编号;权限类别名称;权限类别列表)
            //string username = loginuser.Split(';')[(int)ACCOUNT.USER];                //用户名
            //string[] arrDefault = loginuser.Split(';');
            //arrDefault[(int)ACCOUNT.DEFUSR] = "1";                                    //是否默认登录账号（是）
            //arrDefault[(int)ACCOUNT.REMPSD] = rempsd;                                 //是否记住密码
            //string defaultUser = string.Join(";", arrDefault);
            //SaveUserFile(username, defaultUser);                                      //保存默认账号

            //保存当前账号为默认账号(loginuser:账号;密码;部门;电话;是否默认账号;是否记住密码;权限类别编号;权限类别名称;权限类别列表)
            List<string> listDefault = loginuser.Split(';').ToList();                  //将当前登录账号转换为List
            listDefault[(int)ACCOUNT.DEFUSR] = "1";                                    //是否默认登录账号（是）
            listDefault[(int)ACCOUNT.REMPSD] = rempsd;                                 //是否记住密码

            //移除权限类别名称和列表
            string catName = listDefault[(int)ACCOUNT.CATNAME];                        //权限类别名称
            string catList = listDefault[(int)ACCOUNT.CATLIST];                        //权限类别列表
            listDefault.RemoveAt((int)ACCOUNT.CATNAME);                                //移除权限类别名称
            listDefault.RemoveAt((int)ACCOUNT.CATNAME);                                //移除权限类别列表

            //获得更改后的账号字符串，并写入到文件
            string defaultUser = string.Join(";", listDefault.ToArray()) + ";";        //更改后的默认账号字符串
            string username = listDefault[(int)ACCOUNT.USER];                          //用户名
            SaveUserFile(username, defaultUser);                                       //保存默认账号

            /*
            string username = loginuser.Split(';')[(int)ACCOUNT.USER];                //用户名
            string defaultUser = username + ";";                                      //默认账号信息
            defaultUser += loginuser.Split(';')[(int)ACCOUNT.PSWD] + ";";             //密码
            defaultUser += loginuser.Split(';')[(int)ACCOUNT.DEPT] + ";";             //部门
            defaultUser += loginuser.Split(';')[(int)ACCOUNT.TELE] + ";";             //电话
            defaultUser += "1" + ";";                                                 //是否默认登录账号（是）
            defaultUser += rempsd + ";";                                              //是否记住密码
            defaultUser += loginuser.Split(';')[(int)ACCOUNT.CATCODE] + ";";          //权限类别编号
            SaveUserFile(username, defaultUser);                                      //保存默认账号
            */
        }

        /// <summary>
        /// 更新用户名对应权限列表
        /// </summary>
        /// <returns>1=核对正确；0=程序异常；-1=用户名不存在</returns>
        public int UpdateLoginAccount()
        {
            try
            {
                string username = MyDefine.myXET.meLoginUser.Split(';')[(int)ACCOUNT.USER];             //当前登录的账号的用户名
                for (int i = 0; i < MyDefine.myXET.meListUser.Count; i++)                               //遍历账号列表
                {
                    string myUserName = MyDefine.myXET.meListUser[i].Split(';')[(int)ACCOUNT.USER];    //用户名
                    string myCateCode = MyDefine.myXET.meListUser[i].Split(';')[(int)ACCOUNT.CATCODE]; //权限类别编号
                    if (myUserName.Equals(username))                                     //此账号存在
                    {
                        MyDefine.myXET.meLoginUser = MyDefine.myXET.meListUser[i];       //"用户名;密码;部门;电话;权类编号;"
                        MyDefine.myXET.meLoginUser += GetUserPermName(myCateCode) + ";"; //"权类编号对应的权限类别名称"
                        MyDefine.myXET.meLoginUser += GetUserPermList(myCateCode);       //"权类编号对应的权限列表"(如果权限不存在，则返回BBB...)
                        return 1;
                    }
                }

                return -1;                                                         //账号名不存在

            }
            catch (Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("用户名密码核对失败：" + ex.ToString());
                return 0;           //发生异常
            }
        }

        #endregion

        #region 保存帐号文件(加密)

        /// <summary>
        /// 保存帐号文件(注意创建文件时，文件名称不会区分大小写。分别创建aaa或AAA，后者会直接覆盖前者)
        /// </summary>
        public void SaveUserFile(String myUserName, String myUserAccount)
        {
            //保存账号及权限信息
            String filepath = MyDefine.myXET.userCFG + @"\user." + myUserName + ".cfg";
            FileStream meFS = new FileStream(filepath, FileMode.Create, FileAccess.Write);
            StreamWriter meWrite = new StreamWriter(meFS);

            try
            {
                meWrite.Write(TextEncrypt(myUserAccount, password));     //加密并保存(只能用简单的密码，单个字母或数字等，否则后续添加的可能变成乱码)
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("保存账号文件失败：" + ex.ToString());
            }
            finally
            {
                meWrite.Close();
                meFS.Close();
            }
        }

        #endregion

        #region 删除帐号文件

        /// <summary>
        /// 删除帐号文件
        /// </summary>
        public bool DeleteUserFile(String myUserName)
        {
            try
            {
                String mePath = userCFG + @"\user." + myUserName + ".cfg";
                if (File.Exists(mePath))
                {
                    File.SetAttributes(mePath, FileAttributes.Normal);
                    File.Delete(mePath);
                }
                return true;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("删除账号文件失败：" + ex.ToString());
                return false;
            }
        }

        #endregion

        #region 加载账号文件(到meListUser列表)(解密)

        /// <summary>
        /// 加载用户名列表
        /// </summary>
        /// <returns></returns>
        public void loadUserList()
        {
            if (!Directory.Exists(MyDefine.myXET.userCFG))
            {
                Directory.CreateDirectory(MyDefine.myXET.userCFG);
            }

            try
            {
                List<String> myUsers = new List<string>();      //用户列表
                DirectoryInfo meDirectory = new DirectoryInfo(MyDefine.myXET.userCFG);
                foreach (FileInfo myFile in meDirectory.GetFiles("user.*.cfg"))
                {
                    Application.DoEvents();
                    String filepath = MyDefine.myXET.userCFG + @"\" + myFile;
                    if (File.Exists(filepath))
                    {
                        String[] meLines = File.ReadAllLines(filepath);
                        if (meLines.Length != 0) myUsers.Add(TextDecrypt(meLines[0], password));    //解密
                    }
                }

                MyDefine.myXET.meListUser = new List<string>(myUsers.ToArray());   //将列表赋给全局变量
            }
            catch (Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("用户列表加载失败：" + ex.ToString());
            }
        }

        #endregion

        #region 获取账号名称对应的索引

        /// <summary>
        /// 获取账号名称对应的索引
        /// </summary>
        /// <param name="username">账号名称</param>
        /// <returns></returns>
        public int GetUserNameIndex(string username)
        {
            int myUserIndex = -1;
            for (int i = 0; i < MyDefine.myXET.meListUser.Count; i++)    //遍历账号列表
            {
                string myUserName = MyDefine.myXET.meListUser[i].Split(';')[(int)ACCOUNT.USER];
                if (username.Equals(myUserName))
                {
                    myUserIndex = i;
                    break;
                }
            }

            return myUserIndex;
        }

        #endregion

        #endregion

        #region 权限类别操作

        #region 保存权限类别文件(加密)

        /// <summary>
        /// 保存权限类别文件
        /// </summary>
        /// <param name="myCatName">权限类别名称</param>
        /// <param name="myPermCat">权限类别：名称+权限列表</param>
        public void SavePermCatFile(String myCatName, String myPermCat)
        {
            String filepath = MyDefine.myXET.userCFG + @"\cate." + myCatName + ".cfg";
            FileStream meFS = new FileStream(filepath, FileMode.Create, FileAccess.Write);
            StreamWriter meWrite = new StreamWriter(meFS);

            try
            {
                meWrite.Write(TextEncrypt(myPermCat, password));     //加密并保存(只能用简单的密码，单个字母或数字等，否则后续添加的可能变成乱码)
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("保存账号文件失败：" + ex.ToString());
            }
            finally
            {
                meWrite.Close();
                meFS.Close();
            }
        }

        #endregion

        #region 删除权限类别文件

        /// <summary>
        /// 删除权限类别文件
        /// </summary>
        public bool DeletePermCatFile(String myCatName)
        {
            try
            {

                String mePath = userCFG + @"\cate." + myCatName + ".cfg";
                if (File.Exists(mePath))
                {
                    File.SetAttributes(mePath, FileAttributes.Normal);
                    File.Delete(mePath);
                }
                return true;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("删除权限类别列表失败：" + ex.ToString());
                return false;
            }
        }

        #endregion

        #region 加载权限类别文件(到meListPermCat列表)(解密)

        /// <summary>
        /// 加载权限类别列表
        /// </summary>
        /// <returns></returns>
        public void loadPremCatList()
        {
            if (!Directory.Exists(MyDefine.myXET.userCFG))
            {
                Directory.CreateDirectory(MyDefine.myXET.userCFG);
            }

            try
            {
                List<String> myPremCats = new List<string>();      //权限类别列表
                DirectoryInfo meDirectory = new DirectoryInfo(MyDefine.myXET.userCFG);
                foreach (FileInfo myFile in meDirectory.GetFiles("cate.*.cfg"))
                {
                    String filepath = MyDefine.myXET.userCFG + @"\" + myFile;
                    if (File.Exists(filepath))
                    {
                        String[] meLines = File.ReadAllLines(filepath);

                        if (meLines.Length != 0)
                        {
                            string decryptedLine = TextDecrypt(meLines[0], password);   //解密

                            string[] parts = decryptedLine.Split(';');
                            //判断权限文件是否与当前权限数量一致，不一致则将cfg文件缺失的新权限赋值为B
                            if (parts.Length >= 2 && parts[2].Length < Enum.GetValues(typeof(STEP)).Length)
                            {
                                parts[2] = parts[2].PadRight(Enum.GetValues(typeof(STEP)).Length, 'B');
                                decryptedLine = string.Join(";", parts);
                            }

                            myPremCats.Add(decryptedLine);
                        }
                    }
                }

                MyDefine.myXET.meListPermCat = new List<string>(myPremCats.ToArray());   //将列表赋给全局变量
            }
            catch (Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("权限类别列表加载失败：" + ex.ToString());
            }
        }

        #endregion

        #region 获取权限类别编号对应的索引/权类名称/权类列表

        /// <summary>
        /// 获取权限类别名称对应的索引
        /// </summary>
        /// <param name="catname">权限类别名称</param>
        /// <returns></returns>
        public int GetPermNameIndex(string catname)
        {
            int myCatIndex = -1;
            for (int i = 0; i < MyDefine.myXET.meListPermCat.Count; i++)    //遍历权限类别列表
            {
                string myCateName = MyDefine.myXET.meListPermCat[i].Split(';')[1];
                if (catname.Equals(myCateName))
                {
                    myCatIndex = i;
                    break;
                }
            }

            return myCatIndex;
        }

        /// <summary>
        /// 获取权限类别名称对应的权限类别编号
        /// </summary>
        /// <param name="catname">权限类别名称</param>
        /// <returns></returns>
        public string GetPermNameCode(string catname)
        {
            string catcode = "0000";
            for (int i = 0; i < MyDefine.myXET.meListPermCat.Count; i++)    //遍历权限类别列表
            {
                string myCateCode = MyDefine.myXET.meListPermCat[i].Split(';')[0];
                string myCateName = MyDefine.myXET.meListPermCat[i].Split(';')[1];
                if (catname.Equals(myCateName))
                {
                    catcode = myCateCode;
                    break;
                }
            }

            return catcode;
        }

        /// <summary>
        /// 获取权限类别编号对应的权限类别名称
        /// </summary>
        /// <param name="catcode"></param>
        /// <returns></returns>
        public string GetUserPermName(string catcode)
        {
            string mypermName = "null";
            for (int i = 0; i < MyDefine.myXET.meListPermCat.Count; i++)    //遍历权限类别列表
            {
                if (catcode == MyDefine.myXET.meListPermCat[i].Split(';')[0])
                {
                    mypermName = MyDefine.myXET.meListPermCat[i].Split(';')[1];
                    break;
                }
            }

            return mypermName;
        }

        /// <summary>
        /// 获取权限类别编号对应的权限列表
        /// </summary>
        /// <param name="catcode"></param>
        /// <returns></returns>
        public string GetUserPermList(string catcode)
        {
            int numSTEP = Enum.GetNames(typeof(STEP)).Length;               //获取权限总个数
            string myperm = ("BBB").PadRight(numSTEP, 'B');                 //默认禁止所有权限
            //MessageBox.Show("2:" + myperm);
            for (int i = 0; i < MyDefine.myXET.meListPermCat.Count; i++)    //遍历权限类别列表
            {
                if (catcode == MyDefine.myXET.meListPermCat[i].Split(';')[0])
                {
                    myperm = MyDefine.myXET.meListPermCat[i].Split(';')[2];
                    //MessageBox.Show("3:" + myperm);
                    break;
                }
            }

            return myperm;
        }

        /// <summary>
        /// 获取类别编号所在索引
        /// </summary>
        public int GetUserPermIndex(String catcode)
        {
            try
            {
                for (int i = 0; i < MyDefine.myXET.meListUser.Count; i++)          //遍历账号列表
                {
                    string myCatCode = MyDefine.myXET.meListUser[i].Split(';')[(int)ACCOUNT.CATCODE];  //权限类别编号
                    if (myCatCode.Equals(catcode)) return i;                       //此账号存在
                }

                return -1;                                                         //账号名不存在
            }
            catch (Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("获取类别编号所在索引失败：" + ex.ToString());
                return 0;           //发生异常
            }
        }

        #endregion

        #endregion

        #endregion

        #region 跨线程调用窗体

        //控制主窗体panel切换(跨线程调用窗体函数) -- 注意mymainFrom是Form类型的
        public void switchMainPanel(Byte panelIdx)
        {
            try
            {
                var m = myMainFrom.GetType().GetMethod("switchToPanelTab");

                if (m != null)
                {
                    m.Invoke(myMainFrom, new object[] { panelIdx });

                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        //出厂设置中更新JSN后，同步更新主界面上的已连接产品的出厂编号
        public void UpdateLabelJSN(int labelIdx, string code)
        {
            try
            {
                var m = myMainFrom.GetType().GetMethod("setLabelJSN");

                if (m != null)
                {
                    m.Invoke(myMainFrom, new object[] { labelIdx, code });

                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        #endregion

        #region 提示信息(弹窗可选)

        /// <summary>
        /// 错误弹窗(弹窗可选)
        /// </summary>
        /// <param name="msg">要显示的信息</param>
        /// <param name="showbox">是否显示弹窗</param>
        public void ShowWrongMsg(string msg, bool showbox = true)
        {
            AddToCOMMRecords("系统提示:" + msg);
            AddToTraceRecords(meInterface, msg);
            if (showbox) MessageBox.Show(msg, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        /// <summary>
        /// 正确弹窗(弹窗可选)
        /// </summary>
        /// <param name="msg">要显示的信息</param>
        /// <param name="showbox">是否显示弹窗</param>
        public void ShowCorrectMsg(string msg, bool showbox = true)
        {
            AddToCOMMRecords("系统提示:" + msg);
            AddToTraceRecords(meInterface, msg);
            if (showbox) MessageBox.Show(msg, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region 核对权限(弹窗可选)

        /// <summary>
        /// 检查是否有权限
        /// </summary>
        /// <param name="mystep">正在执行的操作</param>
        /// <param name="showbox">是否显示弹窗</param>
        /// <returns></returns>
        public Boolean CheckPermission(STEP mystep, bool showbox = true)
        {
            //A=有权限；B=无权限。使用A/B而不是1/0是因为加密时1/0会显示固定且不同的字符，而A/B全部显示为口
            string myperm = MyDefine.myXET.meLoginUser.Split(';')[(int)ACCOUNT.CATLIST];
            if (myperm[(int)mystep].Equals('A')) return true;               //有权限
            if (showbox) MessageBox.Show("权限不足！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return false;
        }

        #endregion

        #region 字符串加密/解密(异或加密)

        //注：加密解密函数其实是完全一样的，字符串与密码异或后完成加密，再次异或密码则完成解密。

        /// <summary>
        /// 字符串异或加密
        /// </summary>
        /// <param name="content">要加密的字符串</param>
        /// <param name="password">密码字符串</param>
        /// <returns>加密后的字符串</returns>
        public string TextEncrypt(string content, string password)
        {
            if (password == "") return content;
            char[] data = content.ToCharArray();
            char[] key = password.ToCharArray();
            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= key[i % key.Length];
            }

            return new string(data);
        }

        /// <summary>
        /// 字符串异或解密
        /// </summary>
        /// <param name="content">要解密的字符串</param>
        /// <param name="password">密码字符串</param>
        /// <returns>解密后的字符串</returns>
        public string TextDecrypt(string content, string password)
        {
            if (password == "") return content;
            char[] data = content.ToCharArray();
            char[] key = password.ToCharArray();

            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= key[i % key.Length];
            }

            return new string(data);
        }

        #endregion

    }
}

//end


