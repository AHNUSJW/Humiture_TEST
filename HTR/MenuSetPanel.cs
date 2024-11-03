using System;
using System.Drawing;
using System.Windows.Forms;

namespace HTR
{
    public partial class MenuSetPanel : UserControl
    {
        public MenuSetPanel()
        {
            InitializeComponent();
        }

        #region 界面加载

        private void MenuSetPanel_Load(object sender, EventArgs e)
        {
            try
            {
                dtpStartTime.CustomFormat = "yyyy-MM-dd HH:mm:ss";
                dtpStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
                dtpStartTime.ShowUpDown = true;
                dtpStartTime.MinDate = DateTime.Now;

                dtpCalDate.CustomFormat = "yyyy-MM-dd";
                dtpCalDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
                dtpCalDate.ShowUpDown = true;

                dtpRecalDate.CustomFormat = "yyyy-MM-dd";
                dtpRecalDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
                dtpRecalDate.ShowUpDown = true;

                comboBox1.SelectedIndex = 0;
                comboBox2.SelectedIndex = 0;
                comboBox3.SelectedIndex = 1;

            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("界面加载失败：" + ex.ToString());
            }
        }

        public void AddMyUpdateEvent()
        {
            try
            {
                //核对权限(不要弹框)
                button5.Visible = MyDefine.myXET.CheckPermission(STEP.数据修正, false) ? true : false;

                //只有最高权限才能给编辑PDF权限
                if (MyDefine.myXET.meLoginUser.Split(';')[(int)ACCOUNT.USER] != "JDEGREE")
                {
                    label34.Visible = false;
                    label35.Visible = false;
                    label36.Visible = false;
                    label37.Visible = false;
                }
                else
                {
                    label34.Visible = true;
                    label35.Visible = true;
                    label36.Visible = true;
                    label37.Visible = true;
                }

                button4_Click(null, null);      //刷新参数
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("界面参数刷新失败：" + ex.ToString());
            }
        }

        public void UpdateInterfaceInfo()
        {
            try
            {
                button4_Click(null, null);      //读取当前激活的产品参数，并刷新界面信息
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("界面参数刷新失败：" + ex.ToString());
            }
        }

        #endregion

        #region 界面按钮事件

        #region 读取参数并显示 -- 刷新

        //刷新按钮 -- 读取设备寄存器信息并显示
        private void button4_Click(object sender, EventArgs e)
        {
            label1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            if (!MyDefine.myXET.checkDeviceStatus()) return;    //设备未连接或繁忙

            Boolean ret = true;
            button4.Text = "读取中...";
            Application.DoEvents();
            MyDefine.myXET.meTask = TASKS.setting;  //设置串口为工作中
            MyDefine.myXET.meTips = "[DEVICE REFRESH]" + Environment.NewLine;
            MyDefine.myXET.AddTraceInfo("参数读取 " + MyDefine.myXET.meActiveJsn);

            try
            {
                #region 读取Device(设备批号/电池电量)

                if (ret)
                {
                    ret = MyDefine.myXET.readDevice();
                    if (!ret) MyDefine.myXET.ShowWrongMsg("参数读取失败：A01");

                    if (ret)            //读取成功，刷新界面信息
                    {
                        //label13.Text = MyDefine.myXET.meType.ToString();
                        label14.Text = MyDefine.myXET.meUID;                           //设备批号

                        //电池电量
                        if (Convert.ToInt32(MyDefine.myXET.meSWVer) >= 0x60 && MyDefine.myXET.meType == DEVICE.HTT)
                        {
                            if (MyDefine.myXET.meBat >= 3700) label16.Text = "100%";
                            else if (MyDefine.myXET.meBat <= 2750) label16.Text = "0%";
                            else label16.Text = Convert.ToInt32(MyDefine.myXET.meBat * (2.0 / 19) - (5500.0 / 19)).ToString() + "%";
                        }
                        else
                        {
                            if (MyDefine.myXET.meType == DEVICE.HTQ)
                            {
                                if (MyDefine.myXET.meBat <= 200) label16.Text = "USB供电";
                                else if (MyDefine.myXET.meBat <= 3160) label16.Text = "0%";
                                else if (MyDefine.myXET.meBat >= 4095) label16.Text = "100%";
                                else label16.Text = Convert.ToInt32(MyDefine.myXET.meBat * 0.107 - 338.12).ToString() + "%";
                            }
                            else
                            {
                                if (MyDefine.myXET.meBat <= 2290) label16.Text = "0%";
                                else if (MyDefine.myXET.meBat >= 2756) label16.Text = "100%";
                                else label16.Text = Convert.ToInt32(MyDefine.myXET.meBat * 0.213 - 486.87).ToString() + "%";
                            }
                        }
                    }
                }

                #endregion

                #region 读取Calendar(设备时间，不显示)

                //读取设备时间(不需要刷新界面)
                if (ret)
                {
                    ret = MyDefine.myXET.readCalendar();
                    if (!ret) MyDefine.myXET.ShowWrongMsg("参数读取失败：A02");

                    if (ret)
                    {
                        label35.Text = MyDefine.myXET.meDateCalendar.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                }

                #endregion

                #region 读取Time(计量校准时间/建议复校时间)

                //读取设备计量校准时间、换电池时间等
                if (ret)
                {
                    ret = MyDefine.myXET.readTime();
                    if (!ret) MyDefine.myXET.ShowWrongMsg("参数读取失败：A03");

                    if (ret)            //读取成功，刷新界面信息
                    {
                        label17.Text = MyDefine.myXET.meDateCal.ToString("yyyy-MM-dd");    //计量校准时间

                        DateTime dateReCal = MyDefine.myXET.meDateCal;
                        dateReCal = dateReCal.AddYears(1).AddDays(-1);
                        label29.Text = dateReCal.ToString("yyyy-MM-dd");                  //建议复校时间=校准时间+1年-1天

                        DateTime currentTime = DateTime.Now;                              //当前时间                   
                        DateTime calibrationTime = dateReCal;                             //建议复校时间
                        
                        if (calibrationTime.Year > 2005) {                                //复校时间大于2005年的设备提示校准
                            if (currentTime > calibrationTime)
                            {
                                //当前时间大于校准时间，弹窗提示需要校准
                                DialogResult result = MessageBox.Show("需要先校准后再设置使用", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            //判断距离校准时间是否小于等于一个月
                            else if (calibrationTime.Subtract(currentTime).TotalDays <= 30 && MyDefine.myXET.meRemindCail)
                            {
                                DialogResult result = MessageBox.Show("需要先校准后再设置使用，是否不再提醒？", "系统提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                                //判断用户是否勾选不再提醒
                                if (result == DialogResult.Yes)
                                {
                                    MyDefine.myXET.meRemindCail = false;
                                }
                                else
                                {
                                    MyDefine.myXET.meRemindCail = true;
                                }
                            }
                        }                     
                    }
                }

                #endregion

                #region 读取Jobset(开始/结束/间隔时间/时间单位/采样条数/工作状态)

                if (ret)
                {
                    //读取开始/结束/间隔时间/时间单位/采样条数
                    ret = MyDefine.myXET.readJobset();
                    if (!ret) MyDefine.myXET.ShowWrongMsg("参数读取失败：A04 \r\n" + "间隔时间单位：" +
                        MyDefine.myXET.meUnit + "\r\n测试间隔时间(单位：秒、分、时、天):" + MyDefine.myXET.meSpan +
                        "\r\n测试持续时间(秒):" + MyDefine.myXET.meDuration
                        );
                    if (!isCondenseStatus())
                    {
                        //更新设备工作状态
                        int status1, status2;
                        //status1 = MyDefine.myXET.meDateStart.CompareTo(DateTime.Now);
                        //status2 = MyDefine.myXET.meDateEnd.CompareTo(DateTime.Now);
                        status1 = MyDefine.myXET.meDateStart.CompareTo(MyDefine.myXET.meDateCalendar);  //将开始结束时间与设备时间做比较(注人为停止设备后，设备时间为日期最大值)
                        status2 = MyDefine.myXET.meDateEnd.CompareTo(MyDefine.myXET.meDateCalendar);    //将开始结束时间与设备时间做比较(注人为停止设备后，设备时间为日期最大值)
                        if ((status1 < 0 && status2 < 0)) label15.Text = "空闲中";
                        if ((status1 > 0 && status2 > 0)) label15.Text = "准备中";     //已设置好开始结束时间，等待开始
                        if ((status1 < 0 && status2 > 0)) label15.Text = "记录中";
                    }

                    //label15.Text = (status1 < 0 && status2 > 0) ? "工作中" : "空闲中";

                    //刷新界面信息：开始、结束、间隔时间
                    if (ret && label28.Text == "单次模式")
                    {
                        //必须定义myStart、myStop等变量再赋给labelxx.Text，否则有时候01-01-01时间更新不出来
                        string myStart = MyDefine.myXET.meDateStart.ToString("yyyy-MM-dd HH:mm:ss");
                        string myStop = MyDefine.myXET.meDateEnd.ToString("yyyy-MM-dd HH:mm:ss");
                        string myInterval = MyDefine.myXET.meSpan.ToString();
                        string myUnit = ((UNIT)MyDefine.myXET.meUnit).ToString();

                        string mySelectDur = MyDefine.myXET.meSelectDur.ToString();
                        string myDurUnit = ((UNIT)MyDefine.myXET.meDurUnit).ToString();

                        Double duration = (MyDefine.myXET.meDateEnd - MyDefine.myXET.meDateStart).TotalSeconds;//持续时间（差距的总秒数）

                        label32.Text = myStart;         //开始时间
                        if (duration > 0)
                        {
                            label38.Text = duration.ToString();
                            comboBox3.SelectedIndex = 0;
                            label39.Text = "秒";
                        }
                        if ((duration % 60) == 0)
                        {
                            label38.Text = (duration / 60).ToString();
                            comboBox3.SelectedIndex = 1;
                            label39.Text = "分";
                        }
                        if ((duration % 3600) == 0)
                        {
                            label38.Text = (duration / 3600).ToString();
                            comboBox3.SelectedIndex = 2;
                            label39.Text = "时";
                        }
                        if ((duration % 86400) == 0)
                        {
                            label38.Text = (duration / 86400).ToString();
                            comboBox3.SelectedIndex = 3;
                            label39.Text = "天";
                        }
                        if (duration == 0)
                        {
                            label38.Text = "";
                            comboBox3.SelectedIndex = 0;
                            label39.Text = "秒";
                        }

                        label25.Text = myInterval;      //时间间隔
                        label26.Text = myUnit;          //时间单位


                        //计算测试采样条数
                        UInt32 timePeriod = MyDefine.myXET.meSpan * MyDefine.myXET.meArrUnit[MyDefine.myXET.meUnit];
                        if (timePeriod != 0) label27.Text = (MyDefine.myXET.meDuration / timePeriod).ToString();
                    }

                }

                #endregion

                #region 读取DOT(标定数据，不显示)

                if (ret)
                {
                    //读取标定数据(不需要刷新界面) -- 温湿度采集器没有标定
                    if (MyDefine.myXET.meType == DEVICE.HTT)        //温度采集器
                    {
                        ret = MyDefine.myXET.readDOT(Constants.LEN_READ_DOT1);
                        if (!ret) MyDefine.myXET.ShowWrongMsg("参数读取失败：A05");
                    }
                    else if (MyDefine.myXET.meType == DEVICE.HTP)   //压力采集器
                    {
                        ret = MyDefine.myXET.readDOT(Constants.LEN_READ_DOT2);
                        if (!ret) MyDefine.myXET.ShowWrongMsg("参数读取失败：A06");
                    }
                }

                #endregion

                #region 读取JSN(设备型号/测量范围/产品编号)

                if (ret)
                {
                    //读取产品编号："设备型号,测量范围,产品编号"
                    ret = MyDefine.myXET.readJSN();
                    if (!ret) MyDefine.myXET.ShowWrongMsg("参数读取失败：A07");

                    if (ret)            //读取成功，刷新界面信息
                    {
                        label13.Text = MyDefine.myXET.meModel.Trim();      //设备型号
                        label18.Text = MyDefine.myXET.meRange.Trim();      //测量范围
                        label19.Text = MyDefine.myXET.meJSN.Trim().Length > 2 ? MyDefine.myXET.meJSN.Trim().Substring(2) : MyDefine.myXET.meJSN.Trim();        //产品编号
                    }
                    else
                    {
                        label13.Text = "";
                        label18.Text = "";
                        label19.Text = "";
                    }
                }

                #endregion

                #region 读取USN(管理编号)

                if (ret)
                {
                    //读取管理编号
                    ret = MyDefine.myXET.readUSN();
                    if (!ret) MyDefine.myXET.ShowWrongMsg("参数读取失败：A08");

                    if (ret)            //读取成功，刷新界面信息
                    {
                        label30.Text = MyDefine.myXET.meUSN.Trim();    //管理编号
                    }
                    else
                    {
                        label30.Text = "";
                    }
                }

                #endregion

                #region 读取UTXT(备注信息)

                if (ret)
                {
                    //读取备注信息
                    ret = MyDefine.myXET.readUTXT();
                    if (!ret) MyDefine.myXET.ShowWrongMsg("参数读取失败：A09");

                    if (ret)            //读取成功，刷新界面信息
                    {
                        label31.Text = MyDefine.myXET.meUTXT.Trim();   //备注信息
                    }
                    else
                    {
                        label31.Text = "";
                    }
                }

                #endregion

                #region 读取设备名称
                if (ret)
                {
                    //读取备注信息
                    ret = MyDefine.myXET.readName();

                    if (ret)            //读取成功，刷新界面信息
                    {
                        label12.Text = MyDefine.myXET.meName;   //设备名称
                    }
                    else
                    {
                        MyDefine.myXET.AddTraceInfo("读取失败");
                        label12.Text = ((PPRODUCT)MyDefine.myXET.meType).ToString();
                        ret = true;
                    }
                }
                #endregion

                //parameters_Display();       //将设备参数显示到界面
                showDebugInfo();              //将读取的各项参数添加进调试信息
                MyDefine.myXET.SaveCommunicationTips();  //将调试信息保存到操作日志

            }
            catch
            {
                showDebugInfo();              //将读取的各项参数添加进调试信息
                MyDefine.myXET.ShowWrongMsg("读取设备参数失败！");
                MyDefine.myXET.SaveCommunicationTips();  //将调试信息保存到操作日志
            }

            if (ret) MyDefine.myXET.AddTraceInfo("参数读取成功");

            switch (MyDefine.myXET.meType)
            {
                case DEVICE.HTQ:
                    button1.Enabled = true;
                    button3.Enabled = true;
                    break;
                case DEVICE.HTT:
                    button1.Enabled = false;
                    button3.Enabled = true;
                    break;
                case DEVICE.HTH:
                    button1.Enabled = true;
                    button3.Enabled = true;
                    break;
                case DEVICE.HTP:
                    button1.Enabled = false;
                    button3.Enabled = false;
                    break;
                case DEVICE.HTN:
                    button1.Enabled = false;
                    button3.Enabled = false;
                    break;
                default:
                    button1.Enabled = false;
                    button3.Enabled = false;
                    break;
            }

            button4.Text = "刷  新";
            Application.DoEvents();
            MyDefine.myXET.meTask = TASKS.run;
        }

        //将读到的寄存器参数显示到界面
        public void parameters_Display()
        {
            try
            {
                label12.Text = MyDefine.myXET.meName;   //设备名称
                //label13.Text = MyDefine.myXET.meType.ToString();
                label14.Text = MyDefine.myXET.meUID;
                if (!isCondenseStatus())
                {
                    //更新设备工作状态
                    int status1, status2;
                    status1 = MyDefine.myXET.meDateStart.CompareTo(MyDefine.myXET.meDateCalendar);  //将开始结束时间与设备时间做比较(注人为停止设备后，设备时间为日期最大值)
                    status2 = MyDefine.myXET.meDateEnd.CompareTo(MyDefine.myXET.meDateCalendar);    //将开始结束时间与设备时间做比较(注人为停止设备后，设备时间为日期最大值)
                    if ((status1 < 0 && status2 < 0)) label15.Text = "空闲中";
                    if ((status1 > 0 && status2 > 0)) label15.Text = "准备中";     //已设置好开始结束时间，等待开始
                    if ((status1 < 0 && status2 > 0)) label15.Text = "记录中";
                }

                //电池电量
                if (Convert.ToInt32(MyDefine.myXET.meSWVer) >= 0x60 && MyDefine.myXET.meType == DEVICE.HTT)
                {
                    if (MyDefine.myXET.meBat >= 3700) label16.Text = "100%";
                    else if (MyDefine.myXET.meBat <= 2750) label16.Text = "0%";
                    else label16.Text = Convert.ToInt32(MyDefine.myXET.meBat * (2.0 / 19) - (5500.0 / 19)).ToString() + "%";
                }
                else
                {
                    if (MyDefine.myXET.meType == DEVICE.HTQ)
                    {
                        if (MyDefine.myXET.meBat <= 200) label16.Text = "USB供电";
                        else if (MyDefine.myXET.meBat <= 3160) label16.Text = "0%";
                        else if (MyDefine.myXET.meBat >= 4095) label16.Text = "100%";
                        else label16.Text = Convert.ToInt32(MyDefine.myXET.meBat * 0.107 - 338.12).ToString() + "%";
                    }
                    else
                    {
                        if (MyDefine.myXET.meBat <= 2290) label16.Text = "0%";
                        else if (MyDefine.myXET.meBat >= 2756) label16.Text = "100%";
                        else label16.Text = Convert.ToInt32(MyDefine.myXET.meBat * 0.213 - 486.87).ToString() + "%";
                    }
                }

                label17.Text = MyDefine.myXET.meDateCal.ToString("yyyy-MM-dd");
                //label18.Text = MyDefine.myXET.meHWVer + MyDefine.myXET.meSWVer;
                label13.Text = MyDefine.myXET.meModel.Trim();
                label18.Text = MyDefine.myXET.meRange.Trim();
                label19.Text = MyDefine.myXET.meJSN.Trim().Length > 2 ? MyDefine.myXET.meJSN.Trim().Substring(2) : MyDefine.myXET.meJSN.Trim();        //产品编号
                label30.Text = MyDefine.myXET.meUSN.Trim();
                label31.Text = MyDefine.myXET.meUTXT.Trim();
                //MessageBox.Show(MyDefine.myXET.meUTXT + Environment.NewLine + MyDefine.myXET.meUTXT.Trim());

                if (label28.Text == "单次模式")
                {
                    string myStart = MyDefine.myXET.meDateStart.ToString("yyyy-MM-dd HH:mm:ss");
                    string myStop = MyDefine.myXET.meDateEnd.ToString("yyyy-MM-dd HH:mm:ss");
                    string myInterval = MyDefine.myXET.meSpan.ToString();
                    string myUnit = ((UNIT)MyDefine.myXET.meUnit).ToString();

                    string mySelectDur = MyDefine.myXET.meSelectDur.ToString();
                    string myDurUnit = ((UNIT)MyDefine.myXET.meDurUnit).ToString();

                    label32.Text = myStart;         //开始时间
                    label25.Text = myInterval;      //时间间隔
                    label26.Text = myUnit;          //时间单位

                    label38.Text = mySelectDur;     //持续时间
                    label39.Text = myDurUnit;       //持续时间单位

                    //计算测试间隔时间(秒）
                    UInt32 timePeriod = MyDefine.myXET.meSpan * MyDefine.myXET.meArrUnit[MyDefine.myXET.meUnit];
                    if (timePeriod != 0) label27.Text = (MyDefine.myXET.meDuration / timePeriod).ToString();
                }

                DateTime dateReCal = MyDefine.myXET.meDateCal;
                dateReCal = dateReCal.AddYears(1).AddDays(-1);
                label29.Text = dateReCal.ToString("yyyy-MM-dd");
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("参数显示失败：" + ex.ToString());
            }
        }

        public void showDebugInfo()
        {
            try
            {
                MyDefine.myXET.meTips += "meBat:" + MyDefine.myXET.meBat.ToString() + Environment.NewLine;
                MyDefine.myXET.meTips += "meTMax:" + MyDefine.myXET.meTMax.ToString() + Environment.NewLine;
                MyDefine.myXET.meTips += "meTMin:" + MyDefine.myXET.meTMin.ToString() + Environment.NewLine;
                MyDefine.myXET.meTips += "rtc:" + MyDefine.myXET.meRTC.ToString() + Environment.NewLine;
                MyDefine.myXET.meTips += "htt:" + MyDefine.myXET.meHTT.ToString() + Environment.NewLine;
                MyDefine.myXET.meTips += "meDateCalendar:" + MyDefine.myXET.meDateCalendar.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine;
                MyDefine.myXET.meTips += "meDateHW:" + MyDefine.myXET.meDateHW.ToString("yyyy-MM-dd") + Environment.NewLine;
                MyDefine.myXET.meTips += "meDateBat:" + MyDefine.myXET.meDateBat.ToString("yyyy-MM-dd") + Environment.NewLine;
                MyDefine.myXET.meTips += "dateTempSet:" + MyDefine.myXET.meDateDot.ToString("yyyy-MM-dd") + Environment.NewLine;
                MyDefine.myXET.meTips += "dateCalibration:" + MyDefine.myXET.meDateCal.ToString("yyyy-MM-dd") + Environment.NewLine;
                MyDefine.myXET.meTips += "meDateWake:" + MyDefine.myXET.meDateWake.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine;
                MyDefine.myXET.meTips += "dateStart:" + MyDefine.myXET.meDateStart.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine;
                MyDefine.myXET.meTips += "dateEnd:" + MyDefine.myXET.meDateEnd.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine;
                MyDefine.myXET.meTips += "meSpan:" + MyDefine.myXET.meSpan.ToString() + Environment.NewLine;
                MyDefine.myXET.meTips += "meDuration:" + MyDefine.myXET.meDuration.ToString() + Environment.NewLine;
                MyDefine.myXET.meTips += "meModel:" + MyDefine.myXET.meModel.Trim() + Environment.NewLine;
                MyDefine.myXET.meTips += "meRange:" + MyDefine.myXET.meRange.Trim() + Environment.NewLine;
                MyDefine.myXET.meTips += "meJSN:" + MyDefine.myXET.meJSN.Trim() + Environment.NewLine;
                MyDefine.myXET.meTips += "meUSN:" + MyDefine.myXET.meUSN.Trim() + Environment.NewLine;
                MyDefine.myXET.meTips += "meUTXT:" + MyDefine.myXET.meUTXT.Trim() + Environment.NewLine;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("showDebugInfo失败：" + ex.ToString());
            }
        }

        //判断是否为去冷凝状态
        private bool isCondenseStatus()
        {
            Boolean ret = true;
            if (MyDefine.myXET.meType == DEVICE.HTQ)
            {
                //读取去冷凝结束时间
                ret = MyDefine.myXET.readCondense();
                if (!ret) MyDefine.myXET.ShowWrongMsg("参数读取失败：A10");

                //更新设备工作状态

                if (ret)
                {
                    if (MyDefine.myXET.condenseTime > 0)
                    {
                        label15.Text = "去冷凝中";
                        return true;
                    }

                }
            }
            return false;
        }
        #endregion

        #region 写寄存器参数 -- 设置

        //设备设置 -- 将界面上的参数信息写入设备寄存器
        private void button6_Click(object sender, EventArgs e)
        {
            label1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            if (!MyDefine.myXET.checkDeviceStatus()) return;    //设备未连接或繁忙
            DateTime dateStart = Convert.ToDateTime(label32.Text);
            DateTime dateEnd = Convert.ToDateTime(label32.Text);
            if (label38.Text == "")
            {
                MessageBox.Show("请输入持续时间");
                return;
            }
            switch (comboBox3.Text)
            {
                case "秒": dateEnd = dateStart.AddSeconds(Double.Parse(label38.Text)); break;
                case "分": dateEnd = dateStart.AddMinutes(Double.Parse(label38.Text)); break;
                case "时": dateEnd = dateStart.AddHours(Double.Parse(label38.Text)); break;
                case "天": dateEnd = dateStart.AddDays(Double.Parse(label38.Text)); break;
                default: break;
            }

            if (isCondenseStatus())
            {
                int status1;
                status1 = MyDefine.myXET.meDateCondense.CompareTo(dateStart);  //将冷凝结束时间与开始时间做比较
                if (status1 > 0)
                {
                    if (DialogResult.Yes == MessageBox.Show("正在去冷凝中，确定要结束去冷凝吗？", "提示", MessageBoxButtons.YesNo))
                    {
                        //发送停止去冷凝命令
                        MyDefine.myXET.condenseTime = 0x00;
                        MyDefine.myXET.setREG_TIME_CONDENSE();       //停止去冷凝
                        label15.Text = "空闲中";
                    }
                    else
                    {
                        //设置时间为去冷凝结束时间 + 3分钟且以00结尾 （大于2分钟）
                        dateStart = (DateTime.Now.AddMinutes(3)).AddSeconds(-DateTime.Now.Second);
                        label32.Text = dateStart.ToString("yyyy-MM-dd HH:mm:ss");
                        dateEnd = dateStart.AddMinutes(2);
                        //label33.Text = dateEnd.ToString("yyyy-MM-dd HH:mm:ss");
                        return;
                    }
                }
            }

            //尚未设置完成
            if (label25.Text == "" || label25.Text == "0" || label27.Text == "" || label27.Text == "0")
            {
                MessageBox.Show("参数设置未完成！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //产品工作中，不允许重新设置参数，必须先停止
            if (label15.Text != "空闲中" && label15.Text != "去冷凝中")
            {
                //if (MessageBox.Show("设备工作中，是否重新设置产品参数？", "信息提示！", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel) return;
                MessageBox.Show("设备非空闲状态，无法重新设置参数！" + '\n' + "请停止设备后再设置参数。", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            //清卡前确认数据已保存
            if (MessageBox.Show("即将清卡，请确认测试数据已读出！" + Environment.NewLine + "单击确定，清卡并设置；" + Environment.NewLine + "单击取消，退出设置；", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel)
            {
                return;
            }

            //开始时间过快
            if (DateTime.Compare(dateStart, DateTime.Now.AddMinutes(2)) < 0)     //若开始时间 < 当前系统时间+2分钟，则开始时间 = 当前系统时间+2分钟
            {
                dateStart = (DateTime.Now.AddMinutes(3)).AddSeconds(-DateTime.Now.Second);
                label32.Text = dateStart.ToString("yyyy-MM-dd HH:mm:ss");
                switch (comboBox3.Text)
                {
                    case "秒":
                        dateEnd = dateStart.AddSeconds(Double.Parse(label38.Text));
                        break;
                    case "分":
                        dateEnd = dateStart.AddMinutes(Double.Parse(label38.Text));
                        break;
                    case "时":
                        dateEnd = dateStart.AddHours(Double.Parse(label38.Text));
                        break;
                    case "天":
                        dateEnd = dateStart.AddDays(Double.Parse(label38.Text));
                        break;
                    default: break;
                }
            }
            else
            {
                dateStart = dateStart.AddSeconds(-dateStart.Second);
                label32.Text = dateStart.ToString("yyyy-MM-dd HH:mm:ss");
                switch (comboBox3.Text)
                {
                    case "秒":
                        dateEnd = dateStart.AddSeconds(Double.Parse(label38.Text));
                        break;
                    case "分":
                        dateEnd = dateStart.AddMinutes(Double.Parse(label38.Text));
                        break;
                    case "时":
                        dateEnd = dateStart.AddHours(Double.Parse(label38.Text));
                        break;
                    case "天":
                        dateEnd = dateStart.AddDays(Double.Parse(label38.Text));
                        break;
                    default: break;
                }
            }
            

            //结束时间过快
            if (DateTime.Compare(dateEnd, dateStart.AddMinutes(2)) < 0)     //若结束时间 < 开始时间+2分钟，则结束时间 = 开始时间+2分钟
            {
                dateEnd = dateStart.AddMinutes(2);
            }

            Boolean ret = true;
            button6.Text = "设置中...";
            Application.DoEvents();
            MyDefine.myXET.meTask = TASKS.setting;  //设置串口为工作中
            MyDefine.myXET.meTips = "[DEVICE SETTING]" + Environment.NewLine;
            MyDefine.myXET.AddTraceInfo("设置 " + MyDefine.myXET.meActiveJsn);

            try
            {
                //清卡--清空测试数据
                if (ret)
                {
                    ret = MyDefine.myXET.setJobrec();
                    if (!ret) MyDefine.myXET.ShowWrongMsg("参数设置失败：B01");
                }

                #region 设置Calendar(设备时间)

                if (ret)
                {
                    //更新设备时间为当前系统时间
                    ret = MyDefine.myXET.setCalendar();
                    if (!ret) MyDefine.myXET.ShowWrongMsg("参数设置失败：B02");

                    if (ret)
                    {
                        label35.Text = MyDefine.myXET.meDateCalendar.ToString("yyyy-MM-dd HH:mm:ff");
                    }
                }

                if (ret)
                {
                    //读取设备时间(写完后立即读取，使设备的系统时间开始运行) -- 不读取可能造成设备过段时间后复位？？？
                    ret = MyDefine.myXET.readCalendar();
                    if (!ret) MyDefine.myXET.ShowWrongMsg("参数设置失败：B03");

                    if (ret)
                    {
                        label35.Text = MyDefine.myXET.meDateCalendar.ToString("yyyy-MM-dd HH:mm:ff");
                    }
                }

                #endregion

                if (ret)
                {
                    //更新设备开始时间、结束时间、间隔时间
                    UNIT myUnit = (UNIT)Enum.Parse(typeof(UNIT), label26.Text);     //根据字符串(枚举名称)生成枚举实例
                    MyDefine.myXET.meDateStart = Convert.ToDateTime(label32.Text);
                    MyDefine.myXET.meDateEnd = Convert.ToDateTime(label32.Text);
                    switch (comboBox3.Text)
                    {
                        case "秒": MyDefine.myXET.meDateEnd = Convert.ToDateTime(label32.Text).AddSeconds(Double.Parse(label38.Text)); break;
                        case "分": MyDefine.myXET.meDateEnd = Convert.ToDateTime(label32.Text).AddMinutes(Double.Parse(label38.Text)); break;
                        case "时": MyDefine.myXET.meDateEnd = Convert.ToDateTime(label32.Text).AddHours(Double.Parse(label38.Text)); break;
                        case "天": MyDefine.myXET.meDateEnd = Convert.ToDateTime(label32.Text).AddDays(Double.Parse(label38.Text)); break;
                        default: break;
                    }
                    MyDefine.myXET.meDateWake = MyDefine.myXET.meDateStart.AddSeconds(-40);            //提前40s唤醒设备
                    MyDefine.myXET.meUnit = (Byte)myUnit;
                    MyDefine.myXET.meSpan = Convert.ToUInt16(label25.Text);

                    UNIT myDurUnit = (UNIT)Enum.Parse(typeof(UNIT), label39.Text);
                    MyDefine.myXET.meDurUnit = (Byte)myDurUnit;
                    MyDefine.myXET.meSelectDur = Convert.ToUInt16(label38.Text);

                    //计算测试持续时间
                    TimeSpan totalTime = MyDefine.myXET.meDateEnd.Subtract(MyDefine.myXET.meDateStart);     //测试总时长
                    MyDefine.myXET.meDuration = (UInt32)(totalTime.TotalSeconds);     //测试总秒数

                    if (MyDefine.myXET.meDuration % (MyDefine.myXET.meSpan * MyDefine.myXET.meArrUnit[(int)myUnit]) == 0)
                    {
                        MyDefine.myXET.meDuration += MyDefine.myXET.meSpan * MyDefine.myXET.meArrUnit[(int)myUnit];
                    }
                    else
                    {
                        MyDefine.myXET.meDuration += MyDefine.myXET.meSpan * MyDefine.myXET.meArrUnit[(int)myUnit] - MyDefine.myXET.meDuration % (MyDefine.myXET.meSpan * MyDefine.myXET.meArrUnit[(int)myUnit]);
                    }

                    ret = MyDefine.myXET.setJobset();
                    if (!ret) MyDefine.myXET.ShowWrongMsg("参数设置失败：B05");
                }

                /*
                //将JSN写入设备
                if (label19.Text != "")
                {
                    MyDefine.myXET.meJSN = label19.Text;
                    ret = MyDefine.myXET.setJSN(label19.Text);
                    //if (ret == false) MessageBox.Show("JSN设置失败！");
                    textBox2.AppendText(MyDefine.myXET.meTips);
                }
                

                //将USN写入设备
                if (label30.Text != "")
                {
                    MyDefine.myXET.meUSN = label30.Text;
                    ret = MyDefine.myXET.setUSN(label30.Text);
                    //if (ret == false) MessageBox.Show("USN设置失败！");
                    textBox2.AppendText(MyDefine.myXET.meTips);
                }

                //将UTXT写入设备
                if (label31.Text != "")
                {
                    MyDefine.myXET.meUTXT = label31.Text;
                    ret = MyDefine.myXET.setUTXT(label31.Text);
                    //if (ret == false) MessageBox.Show("UTXT设置失败！");
                    textBox2.AppendText(MyDefine.myXET.meTips);
                }
                */

                showDebugInfo();              //将读取的各项参数添加进调试信息
                MyDefine.myXET.SaveCommunicationTips();  //将调试信息保存到操作日志
            }
            catch (Exception ex)
            {
                showDebugInfo();              //将读取的各项参数添加进调试信息
                MyDefine.myXET.ShowWrongMsg("设置失败：" + ex.ToString());
            }

            MyDefine.myXET.meTask = TASKS.run;

            //设置完后，刷新当前激活产品的参数
            button4_Click(null, null);

            if (ret) MyDefine.myXET.ShowCorrectMsg("设置成功!");
            button6.Text = "设  置";
            Application.DoEvents();
            //MessageBox.Show("设置完成！");
        }

        #endregion

        #region 写寄存器参数 -- 批量设置

        private void button8_Click(object sender, EventArgs e)
        {
            label1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            if (!MyDefine.myXET.checkDeviceStatus()) return;    //设备未连接或繁忙
            Boolean ret = true;
            DateTime dateStart = Convert.ToDateTime(label32.Text);
            DateTime dateEnd = Convert.ToDateTime(label32.Text);
            if (label38.Text == "")
            {
                MessageBox.Show("请输入持续时间");
                return;
            }
            switch (comboBox3.Text)
            {
                case "秒": dateEnd = dateStart.AddSeconds(Double.Parse(label38.Text)); break;
                case "分": dateEnd = dateStart.AddMinutes(Double.Parse(label38.Text)); break;
                case "时": dateEnd = dateStart.AddHours(Double.Parse(label38.Text)); break;
                case "天": dateEnd = dateStart.AddDays(Double.Parse(label38.Text)); break;
                default: break;
            }

            //尚未设置完成
            if (label25.Text == "" || label25.Text == "0" || label27.Text == "" || label27.Text == "0")
            {
                MessageBox.Show("参数设置未完成！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //清卡前确认数据已保存
            if (MessageBox.Show("即将清卡，请确认测试数据已读出！" + Environment.NewLine + "单击确定，清卡并设置；" + Environment.NewLine + "单击取消，退出设置；", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel)
            {
                return;
            }

            //开始时间过快
            if (DateTime.Compare(dateStart, DateTime.Now.AddMinutes(2)) < 0)     //若开始时间 < 当前系统时间+2分钟，则开始时间 = 当前系统时间+2分钟
            {
                dateStart = (DateTime.Now.AddMinutes(3)).AddSeconds(-DateTime.Now.Second);
                label32.Text = dateStart.ToString("yyyy-MM-dd HH:mm:ss");
                switch (comboBox3.Text)
                {
                    case "秒":
                        dateEnd = dateStart.AddSeconds(Double.Parse(label38.Text));
                        break;
                    case "分":
                        dateEnd = dateStart.AddMinutes(Double.Parse(label38.Text));
                        break;
                    case "时":
                        dateEnd = dateStart.AddHours(Double.Parse(label38.Text));
                        break;
                    case "天":
                        dateEnd = dateStart.AddDays(Double.Parse(label38.Text));
                        break;
                    default: break;
                }
            }
            else
            {
                dateStart = dateStart.AddSeconds(-dateStart.Second);
                label32.Text = dateStart.ToString("yyyy-MM-dd HH:mm:ss");
                switch (comboBox3.Text)
                {
                    case "秒":
                        dateEnd = dateStart.AddSeconds(Double.Parse(label38.Text));
                        break;
                    case "分":
                        dateEnd = dateStart.AddMinutes(Double.Parse(label38.Text));
                        break;
                    case "时":
                        dateEnd = dateStart.AddHours(Double.Parse(label38.Text));
                        break;
                    case "天":
                        dateEnd = dateStart.AddDays(Double.Parse(label38.Text));
                        break;
                    default: break;
                }
            }

            //结束时间过快
            if (DateTime.Compare(dateEnd, dateStart.AddMinutes(2)) < 0)     //若结束时间 < 开始时间+2分钟，则结束时间 = 开始时间+2分钟
            {
                dateEnd = dateStart.AddMinutes(2);
            }

            //Boolean ret = true;
            button8.Text = "设置中...";
            Application.DoEvents();
            MyDefine.myXET.meTask = TASKS.setting;  //设置串口为工作中
            Byte activeDUTAddress = MyDefine.myXET.meActiveAddr;   //保存当前正在连接的设备地址，批量设置完后再设置回来
            MyDefine.myXET.meTips = "[BATCH DEVICES SETTING]" + Environment.NewLine;
            MyDefine.myXET.AddTraceInfo("批量设置");

            try
            {
                for (int i = 0; i < MyDefine.myXET.meDUTAddrArr.Count; i++)
                {
                    //产品工作中，不允许重新设置参数，必须先停止
                    if(label15.Text == "空闲中")
                    {
                        if (!setWorking(i))
                        {
                            break;
                        }
                    }
                    else
                    {
                        //读取开始/结束/间隔时间/时间单位/采样条数
                        ret = MyDefine.myXET.readJobset();

                        if (!ret) MyDefine.myXET.ShowWrongMsg("参数读取失败：A04 \r\n" + "间隔时间单位：" +
                            MyDefine.myXET.meUnit + "\r\n测试间隔时间(单位：秒、分、时、天):" + MyDefine.myXET.meSpan +
                            "\r\n测试持续时间(秒):" + MyDefine.myXET.meDuration
                            );

                        if (isCondenseStatus())
                        {
                            if (DialogResult.Yes == MessageBox.Show("正在去冷凝中，确定要结束去冷凝吗？", "提示", MessageBoxButtons.YesNo))
                            {
                                //发送停止去冷凝命令
                                MyDefine.myXET.condenseTime = 0x00;
                                MyDefine.myXET.setREG_TIME_CONDENSE();       //停止去冷凝
                                if (!setWorking(i))
                                {
                                    break;
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            //更新设备工作状态
                            int status1, status2;
                            //status1 = MyDefine.myXET.meDateStart.CompareTo(DateTime.Now);
                            //status2 = MyDefine.myXET.meDateEnd.CompareTo(DateTime.Now);
                            status1 = MyDefine.myXET.meDateStart.CompareTo(MyDefine.myXET.meDateCalendar);  //将开始结束时间与设备时间做比较(注人为停止设备后，设备时间为日期最大值)
                            status2 = MyDefine.myXET.meDateEnd.CompareTo(MyDefine.myXET.meDateCalendar);    //将开始结束时间与设备时间做比较(注人为停止设备后，设备时间为日期最大值)
                            if ((status1 < 0 && status2 < 0))
                            {
                                if (!setWorking(i))
                                {
                                    break;
                                }
                            }
                            if ((status1 > 0 && status2 > 0) || (status1 < 0 && status2 > 0)) //设备在准备中或工作中
                            {
                                if (DialogResult.Yes == MessageBox.Show("设备已设置工作，确定要重新设置工作吗？", "提示", MessageBoxButtons.YesNo))
                                {
                                    if (!setWorking(i))
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("批量设置失败:" + ex.ToString());
            }

            MyDefine.myXET.meTask = TASKS.run;
            MyDefine.myXET.meActiveAddr = activeDUTAddress;        //将当前设备地址切换回批量设置前的已连接设备地址
            MyDefine.myXET.meTips += "ReadREG_Device:" + Environment.NewLine;
            MyDefine.myXET.readDevice();                            //重新读取当前设备型号等参数

            //批量设置完后，刷新当前激活产品的参数
            button4_Click(null, null);

            if (ret) MyDefine.myXET.AddTraceInfo("批量设置成功");
            MyDefine.myXET.SaveCommunicationTips();  //将调试信息保存到操作日志
            button8.Text = "批量设置";
            Application.DoEvents();
        }

        private Boolean setWorking(int i)
        {
            Boolean ret = true;

            MyDefine.myXET.AddTraceInfo("设置 " + MyDefine.myXET.meDUTJsnArr[i]);
            MyDefine.myXET.meActiveAddr = MyDefine.myXET.meDUTAddrArr[i];     //切换当前设备地址

            //清卡--清空测试数据
            if (ret)
            {
                ret = MyDefine.myXET.setJobrec();
                if (!ret) MyDefine.myXET.ShowWrongMsg("参数设置失败：C01");
            }

            #region 设置Calendar(设备系统时间)

            //更新设备时间为当前系统时间
            if (ret)
            {
                ret = MyDefine.myXET.setCalendar();
                if (!ret) MyDefine.myXET.ShowWrongMsg("参数设置失败：C03");
            }

            //读取设备时间(写完后立即读取，使设备的系统时间开始运行) -- 不读取可能造成设备过段时间后复位？？？
            if (ret)
            {
                ret = MyDefine.myXET.readCalendar();
                if (!ret) MyDefine.myXET.ShowWrongMsg("参数设置失败：C04");
            }

            #endregion

            //设置工作时间等参数(开始、结束、间隔)
            if (ret)
            {
                //更新设备开始时间、结束时间、间隔时间
                UNIT myUnit = (UNIT)Enum.Parse(typeof(UNIT), label26.Text);     //根据字符串(枚举名称)生成枚举实例
                MyDefine.myXET.meDateStart = Convert.ToDateTime(label32.Text);
                MyDefine.myXET.meDateEnd = Convert.ToDateTime(label32.Text);
                switch (comboBox3.Text)
                {
                    case "秒": MyDefine.myXET.meDateEnd = Convert.ToDateTime(label32.Text).AddSeconds(Double.Parse(label38.Text)); break;
                    case "分": MyDefine.myXET.meDateEnd = Convert.ToDateTime(label32.Text).AddMinutes(Double.Parse(label38.Text)); break;
                    case "时": MyDefine.myXET.meDateEnd = Convert.ToDateTime(label32.Text).AddHours(Double.Parse(label38.Text)); break;
                    case "天": MyDefine.myXET.meDateEnd = Convert.ToDateTime(label32.Text).AddDays(Double.Parse(label38.Text)); break;
                    default: break;
                }
                MyDefine.myXET.meDateWake = MyDefine.myXET.meDateStart.AddSeconds(-40);            //提前40s唤醒设备
                MyDefine.myXET.meUnit = (Byte)myUnit;
                MyDefine.myXET.meSpan = Convert.ToUInt16(label25.Text);

                UNIT myDurUnit = (UNIT)Enum.Parse(typeof(UNIT), label39.Text);
                MyDefine.myXET.meDurUnit = (Byte)myDurUnit;
                MyDefine.myXET.meSelectDur = Convert.ToUInt16(label38.Text);

                //计算测试持续时间
                TimeSpan totalTime = MyDefine.myXET.meDateEnd.Subtract(MyDefine.myXET.meDateStart);     //测试总时长
                MyDefine.myXET.meDuration = (UInt32)(totalTime.TotalSeconds);     //测试总秒数

                if (MyDefine.myXET.meDuration % (MyDefine.myXET.meSpan * MyDefine.myXET.meArrUnit[(int)myUnit]) == 0)
                {
                    MyDefine.myXET.meDuration += MyDefine.myXET.meSpan * MyDefine.myXET.meArrUnit[(int)myUnit];
                }
                else
                {
                    MyDefine.myXET.meDuration += MyDefine.myXET.meSpan * MyDefine.myXET.meArrUnit[(int)myUnit] - MyDefine.myXET.meDuration % (MyDefine.myXET.meSpan * MyDefine.myXET.meArrUnit[(int)myUnit]);
                }

                ret = MyDefine.myXET.setJobset();
                if (!ret) MyDefine.myXET.ShowWrongMsg("参数设置失败：C05");
            }

            if (MyDefine.myXET.meType == DEVICE.HTQ && ret)
            {
                ret = MyDefine.myXET.setScreenState();
            }

            if (!ret) return false;                            //设置失败，退出循环
            else return true;
        }

        #endregion

        #region 设备停止测试 -- 停止

        //停止 -- 使设备停止测试，恢复空闲状态
        private void button2_Click(object sender, EventArgs e)
        {
            label1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            if (!MyDefine.myXET.checkDeviceStatus()) return;    //设备未连接或繁忙

            if (MessageBox.Show("停止后产品不再记录测试数据，但当前测试数据保留，是否继续？", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                Boolean ret = true;
                button2.Text = "停止中...";
                Application.DoEvents();
                MyDefine.myXET.meTask = TASKS.setting;
                Byte activeDUTAddress = MyDefine.myXET.meActiveAddr;   //保存当前正在连接的设备地址，批量设置完后再设置回来
                MyDefine.myXET.meTips = "[BATCH DEVICES STOP]" + Environment.NewLine;
                MyDefine.myXET.AddTraceInfo("停止");
                DateTime currTime = DateTime.Now.AddSeconds(-1);           //当前时间（必须放在Calendar设置前）：减1s，防止重新设置的结束时间比设备Calendar时间大

                for (int i = 0; i < MyDefine.myXET.meDUTAddrArr.Count; i++)
                {
                    MyDefine.myXET.AddTraceInfo("停止 " + MyDefine.myXET.meDUTJsnArr[i]);
                    MyDefine.myXET.meActiveAddr = MyDefine.myXET.meDUTAddrArr[i];     //切换当前设备地址

                    #region 设置Jobset(重新设置Jobset会使后续读取数据时没有时间，弃用)
                    /*
                    if (ret)
                    {
                        ret = MyDefine.myXET.setJobset();
                        if (!ret) MyDefine.myXET.ShowWrongMsg("参数设置失败：D01");
                    }

                    //重新读取设备开始、结束等测试时间
                    if (ret)
                    {
                        ret = MyDefine.myXET.readJobset();
                        if (!ret) MyDefine.myXET.ShowWrongMsg("参数设置失败：D02");
                    }
                    */
                    #endregion

                    #region 设置Calendar(设备时间)(重新连接后会自动重新开始测试，弃用)
                    /*
                    if (ret)
                    {
                        //更新设备时间为时间最大值(超出设备停止时间，从而使设备停止)
                        ret = MyDefine.myXET.setMaxCalendar();
                        if (!ret) MyDefine.myXET.ShowWrongMsg("参数设置失败：D01");
                    }

                    if (ret)
                    {
                        //读取设备时间(写完后立即读取，使设备的系统时间开始运行) -- 不读取可能造成设备过段时间后复位？？？
                        ret = MyDefine.myXET.readCalendar();
                        if (!ret) MyDefine.myXET.ShowWrongMsg("参数设置失败：D02");
                    }
                    */
                    #endregion

                    #region 设置Jobset停止时间为当前系统时间

                    #region 设置Calendar(设备时间)

                    if (ret)
                    {
                        //更新设备时间为当前系统时间
                        ret = MyDefine.myXET.setCalendar();
                        if (!ret) MyDefine.myXET.ShowWrongMsg("参数设置失败：D01");
                    }

                    if (ret)
                    {
                        //读取设备时间(写完后立即读取，使设备的系统时间开始运行) -- 不读取可能造成设备过段时间后复位？？？
                        ret = MyDefine.myXET.readCalendar();
                        if (!ret) MyDefine.myXET.ShowWrongMsg("参数设置失败：D02");

                        if (ret)
                        {
                            label35.Text = MyDefine.myXET.meDateCalendar.ToString("yyyy-MM-dd HH:mm:ff");
                        }
                    }

                    #endregion

                    #region 读取readJobset

                    if (ret)    //读取当前设备的时间设置：唤醒时间、开始时间、停止时间、间隔时间、间隔单位、持续时间
                    {
                        ret = MyDefine.myXET.readJobset();
                        if (!ret) MyDefine.myXET.ShowWrongMsg("参数读取失败：D03");
                    }

                    #endregion

                    #region 根据工作状态(待机中/工作中/空闲中)更新Jobset参数

                    if (ret)    //更新时间设置相关参数
                    {
                        int status1 = MyDefine.myXET.meDateStart.CompareTo(currTime);
                        int status2 = MyDefine.myXET.meDateEnd.CompareTo(currTime);

                        if (status1 > 0 && status2 > 0)         //待机中，开始结束时间均设置为当前时间；
                        {
                            MyDefine.myXET.meDateWake = currTime;
                            MyDefine.myXET.meDateStart = currTime;
                            MyDefine.myXET.meDateEnd = currTime;
                            MyDefine.myXET.meDuration = 0;
                        }
                        else if (status1 < 0 && status2 < 0)    //空闲中，忽略，不做修改
                        {
                            //
                        }
                        else                                    //工作中，则保留开始时间，将结束时间设置为当前时间
                        {
                            //将停止时间替换为当前系统时间，使设备停止测试
                            MyDefine.myXET.meDateEnd = currTime;

                            //重新计算测试持续时间
                            TimeSpan totalTime = MyDefine.myXET.meDateEnd.Subtract(MyDefine.myXET.meDateStart);     //测试总时长
                            MyDefine.myXET.meDuration = (UInt32)(totalTime.TotalSeconds);     //测试总秒数
                        }
                    }

                    #endregion

                    #region 重新设置Jobset

                    if (ret)    //重新进行时间设置
                    {
                        ret = MyDefine.myXET.setJobset();
                        if (!ret) MyDefine.myXET.ShowWrongMsg("参数设置失败：D04");
                    }

                    #endregion

                    #endregion

                    if (!ret) break;                                    //设置失败，退出循环
                }

                //重新读取当前激活设备的参数
                MyDefine.myXET.meActiveAddr = activeDUTAddress;        //将当前设备地址切换回批量设置前的已连接设备地址
                MyDefine.myXET.meTips += "ReadREG_Jobset:" + Environment.NewLine;
                MyDefine.myXET.readJobset();
                MyDefine.myXET.meTips += "ReadREG_Device:" + Environment.NewLine;
                MyDefine.myXET.readDevice();                            //重新读取当前设备型号等参数

                parameters_Display();                                   //显示当前最新设备参数
                if (ret) MyDefine.myXET.AddTraceInfo("停止成功");
                MyDefine.myXET.meTask = TASKS.run;
                button2.Text = "停  止";
            }

            MyDefine.myXET.SaveCommunicationTips();  //将调试信息保存到操作日志
        }
        #endregion

        #region 设备去冷凝
        //去冷凝
        private void button1_Click(object sender, EventArgs e)
        {
            if (label15.Text != "空闲中" && label15.Text != "去冷凝中") return;
            if (!MyDefine.myXET.meScreenStatus)
            {
                MessageBox.Show("熄屏状态下不能去冷凝");
                return;
            }

            MyDefine.myXET.AddTraceInfo("设备去冷凝");
            MenuToCondense toCondense = new MenuToCondense();
            toCondense.ShowDialog();
            toCondense.Location = new Point(this.Location.X + this.Width / 2, this.Location.Y + this.Height / 2);
            if (toCondense.IsSave)
            {
                if (toCondense.updateTime)
                {
                    DateTime dateStart = DateTime.Now.AddMinutes(Convert.ToInt32(MyDefine.myXET.condenseTime) + 2);
                    DateTime dateEnd = dateStart.AddMinutes(2);
                    label32.Text = dateStart.ToString("yyyy-MM-dd HH:mm:ss");
                }
                label15.Text = "去冷凝中";
            }
        }
        #endregion

        #region 切换单位
        private void button3_Click(object sender, EventArgs e)
        {
            label1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果
            MyDefine.myXET.meTips = "[DEVICE REFRESH]" + Environment.NewLine;
            MyDefine.myXET.AddTraceInfo("切换单位 " + MyDefine.myXET.meActiveJsn);
            Boolean ret = MyDefine.myXET.setREG_UNIT();         //切换单位
            string newJsn = "";
            if (ret)
            {
                String[] str = MyDefine.myXET.meRange.Trim().Split(new char[] { '(', '~', ')' });      //测量范围
                String[] spString = MyDefine.myXET.meRange.Trim().Split(new char[] { ' ' });      //测量范围
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
                    MyDefine.myXET.meRange = "(" + data1 + "~" + data2 + ")" + MyDefine.myXET.temUnit;
                    if (spString.Length == 2)
                    {
                        MyDefine.myXET.meRange += " " + spString[1];
                    }

                    // 设置出厂编号:“设备型号,出厂编号,测量范围”
                    label18.Text = MyDefine.myXET.meRange.Trim();      //测量范围
                    newJsn = MyDefine.myXET.meModel + "," + MyDefine.myXET.meJSN + "," + MyDefine.myXET.meRange;
                    ret = MyDefine.myXET.setJSN(newJsn);

                    if (!ret) MessageBox.Show("测量范围更新失败");
                }
            }
        }
        #endregion

        #region 数据修正
        private void button5_Click(object sender, EventArgs e)
        {
            MyDefine.myXET.AddTraceInfo("数据修正");
            MenuDataCorrection dataCorrection = new MenuDataCorrection();
            dataCorrection.ShowDialog();
            dataCorrection.Location = new Point(this.Location.X + this.Width / 2, this.Location.Y + this.Height / 2);
        }
        #endregion

        #endregion

        #region 控件大小随窗体变化

        //控件大小随窗体变化
        private Boolean firstStart = true;
        private float X;//定义当前窗体的宽度
        private float Y;//定义当前窗体的高度

        /// <summary>
        /// 初始化所有控件尺寸及字体大小
        /// </summary>
        public void InitForm()
        {
            X = this.Width;//赋值初始窗体宽度
            Y = this.Height;//赋值初始窗体高度
            setTag(this);
        }

        /// <summary>
        /// 窗体尺寸变化时触发
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (firstStart) { firstStart = false; return; }     //OnResize()在Form_Load()之前触发,退出

            try
            {
                float newX = this.Width / X;//获取当前宽度与初始宽度的比例
                float newY = this.Height / Y;//获取当前高度与初始高度的比例
                setControls(newX, newY, this);
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("OnResize失败：" + ex.ToString());
            }
        }

        /// <summary>
        /// 获取控件的尺寸/位置/字体
        /// </summary>
        /// <param name="cons"></param>
        private void setTag(Control cons)
        {
            //遍历窗体中的控件
            foreach (Control con in cons.Controls)
            {
                con.Tag = con.Width + ":" + con.Height + ":" + con.Left + ":" + con.Top + ":" + con.Font.Size;

                //如果是容器控件，则递归继续纪录
                if (con.Controls.Count > 0)
                {
                    setTag(con);
                }
            }
        }

        /// <summary>
        /// 窗体尺寸变化时，按比例调整控件尺寸及字体
        /// </summary>
        /// <param name="newX"></param>
        /// <param name="newY"></param>
        /// <param name="cons"></param>
        public void setControls(float newX, float newY, Control cons)
        {
            try
            {
                //遍历窗体中的控件，重新设置控件的值
                foreach (Control con in cons.Controls)
                {
                    if (Convert.ToString(con.Tag) == string.Empty) continue;
                    string[] mytag = con.Tag.ToString().Split(new char[] { ':' });//获取控件的Tag属性值，并分割后存储字符串数组
                    if (mytag.Length < 5) return;

                    float a = Convert.ToSingle(mytag[0]) * newX;//根据窗体缩放比例确定控件的值，宽度//89*300
                    con.Width = (int)(a);//宽度

                    a = Convert.ToSingle(mytag[1]) * newY;//根据窗体缩放比例确定控件的值，高度//12*300
                    con.Height = (int)(a);//高度

                    a = Convert.ToSingle(mytag[2]) * newX;//根据窗体缩放比例确定控件的值，左边距离//
                    con.Left = (int)(a);//左边距离

                    a = Convert.ToSingle(mytag[3]) * newY;//根据窗体缩放比例确定控件的值，上边缘距离
                    con.Top = (int)(a);//上边缘距离

                    if (con is Panel == false && newY != 0)//Panel容器控件不改变字体--Panel字体变后，若panel调用了UserControl控件，则UserControl及其上控件的尺寸会出现不可控变化;newY=0时，字体设置会报错
                    {
                        Single currentSize = Convert.ToSingle(mytag[4]) * newY;//根据窗体缩放比例确定控件的值，字体大小
                        con.Font = new System.Drawing.Font(con.Font.Name, currentSize, con.Font.Style, con.Font.Unit);//字体大小
                    }

                    if (con.Controls.Count > 0)
                    {
                        setControls(newX, newY, con);
                    }

                    //Remarks：
                    //控件当前宽度：控件初始宽度=窗体当前宽度：窗体初始宽度
                    //控件当前宽度=控件初始宽度*(窗体当前宽度/窗体初始宽度)
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("界面尺寸变化失败：" + ex.ToString());
            }
        }

        #endregion

        #region label信息输入变更

        //双击label进行编辑
        private void label19_DoubleClick(object sender, EventArgs e)
        {
            textBox3.Text = label19.Text;
            textBox3.Visible = true;
            textBox3.BringToFront();
            textBox3.Focus();
        }

        //回车退出编辑状态
        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                label19.Text = textBox3.Text;
                textBox3.Visible = false;
            }
        }

        //焦点丢失退出编辑状态
        private void textBox3_Leave(object sender, EventArgs e)
        {
            label19.Text = textBox3.Text;
            textBox3.Visible = false;
        }

        private void label30_DoubleClick(object sender, EventArgs e)
        {
            textBox4.Text = label30.Text;
            textBox4.Visible = true;
            textBox4.BringToFront();
            textBox4.Focus();
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            int mycharNum = System.Text.Encoding.GetEncoding("GBK").GetBytes(e.KeyChar.ToString()).Length;
            if (mycharNum > 1)          //禁止输入中文及中文符号
            {
                //MessageBox.Show("请输入英文字符！");
                e.Handled = true;
                return;
            }

            if (e.KeyChar == 13)
            {
                label30.Text = textBox4.Text;
                textBox4.Visible = false;
            }
        }

        //焦点丢失退出编辑状态
        private void textBox4_Leave(object sender, EventArgs e)
        {
            MyDefine.myXET.AddTraceInfo("修改管理编号：" + textBox4.Text);
            label30.Text = textBox4.Text;
            textBox4.Visible = false;
        }

        private void label31_DoubleClick(object sender, EventArgs e)
        {
            textBox5.Text = label31.Text;
            textBox5.Visible = true;
            textBox5.BringToFront();
            textBox5.Focus();
        }

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                label31.Text = textBox5.Text;
                textBox5.Visible = false;
            }
        }

        //焦点丢失退出编辑状态
        private void textBox5_Leave(object sender, EventArgs e)
        {
            MyDefine.myXET.AddTraceInfo("修改备注信息：" + textBox5.Text);
            label31.Text = textBox5.Text;
            textBox5.Visible = false;
        }


        private void label25_DoubleClick(object sender, EventArgs e)
        {
            textBox6.Text = label25.Text;
            textBox6.Text = string.Empty;
            textBox6.Visible = true;
            textBox6.BringToFront();
            textBox6.Focus();
        }

        private void label25_TextChanged(object sender, EventArgs e)
        {
            MyDefine.myXET.AddTraceInfo("修改间隔时间为：" + label25.Text);
            calTotalTestTimes();    //计算采样条数
        }

        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            //只允许输入数字和删除键和回车键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != 8) && (e.KeyChar != 13))
            {
                e.Handled = true;
                return;
            }
            // 如果第一位为0，且输入的不是删除键，则不允许输入
            if ((e.KeyChar != 8) && (((TextBox)sender).Text == "0"))
            {
                e.Handled = true;
                return;
            }
            if (e.KeyChar == 13)
            {
                label25.Text = textBox6.Text;
                textBox6.Visible = false;
                //calTotalTestTimes();    //计算采样条数
            }
        }

        //焦点丢失退出编辑状态
        private void textBox6_Leave(object sender, EventArgs e)
        {
            label25.Text = textBox6.Text;
            textBox6.Visible = false;
            //calTotalTestTimes();    //计算采样条数
        }

        private void label17_DoubleClick(object sender, EventArgs e)
        {
            label17.Visible = false;
            dtpCalDate.Visible = true;
            //dtpCalDate.Value = DateTime.Now;
            dtpCalDate.BringToFront();
            dtpCalDate.Focus();
        }

        private void dtpCalDate_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                label17.Text = dtpCalDate.Value.ToString("yyyy-MM-dd");                   //校准时间
                label29.Text = dtpCalDate.Value.AddYears(1).AddDays(-1).ToString("yyyy-MM-dd");       //建议复校时间=校准时间+1年-1天
                dtpCalDate.Visible = false;
                label17.Visible = true;
            }
        }

        private void dtpCalDate_Leave(object sender, EventArgs e)
        {
            label17.Text = dtpCalDate.Value.ToString("yyyy-MM-dd");                   //校准时间
            label29.Text = dtpCalDate.Value.AddYears(1).ToString("yyyy-MM-dd");       //建议复校时间=校准时间+1年
            dtpCalDate.Visible = false;
            label17.Visible = true;
        }

        private void label29_DoubleClick(object sender, EventArgs e)
        {
            label29.Visible = false;
            dtpRecalDate.Visible = true;
            dtpRecalDate.BringToFront();
            dtpRecalDate.Focus();
        }

        private void dtpRecalDate_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void dtpRecalDate_Leave(object sender, EventArgs e)
        {
            label29.Text = dtpCalDate.Value.ToString("yyyy-MM-dd");
            dtpRecalDate.Visible = false;
            label29.Visible = true;
        }

        private void label32_DoubleClick(object sender, EventArgs e)
        {
            label32.Visible = false;
            dtpStartTime.Visible = true;
            dtpStartTime.Value = DateTime.Now;
            dtpStartTime.BringToFront();
            dtpStartTime.Focus();
        }

        private void label32_TextChanged(object sender, EventArgs e)
        {
            MyDefine.myXET.AddTraceInfo("修改开始时间为：" + label32.Text);
            calTotalTestTimes();    //计算采样条数
        }

        private void dtpStartTime_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                label32.Text = dtpStartTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                dtpStartTime.Visible = false;
                label32.Visible = true;
                //calTotalTestTimes();    //计算采样条数
            }
        }

        //焦点丢失退出编辑状态
        private void dtpStartTime_Leave(object sender, EventArgs e)
        {
            label32.Text = dtpStartTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
            dtpStartTime.Visible = false;
            label32.Visible = true;
            //calTotalTestTimes();    //计算采样条数
        }

        private void label26_DoubleClick(object sender, EventArgs e)
        {
            comboBox1.Visible = true;
            comboBox1.BringToFront();
            label26.Visible = false;
            //comboBox1.Focus();
        }

        private void label26_TextChanged(object sender, EventArgs e)
        {
            MyDefine.myXET.AddTraceInfo("修改间隔单位：" + label26.Text);
            calTotalTestTimes();    //计算采样条数
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            label26.Text = comboBox1.SelectedItem.ToString();
            comboBox1.Visible = false;
            label26.Visible = true;
            //calTotalTestTimes();    //计算采样条数
        }

        //焦点丢失退出编辑状态
        private void comboBox1_Leave(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1) comboBox1.SelectedIndex = 0;
            label26.Text = comboBox1.SelectedItem.ToString();
            comboBox1.Visible = false;
            label26.Visible = true;
            calTotalTestTimes();    //计算采样条数
        }

        private void label28_DoubleClick(object sender, EventArgs e)
        {
            comboBox2.Visible = true;
            comboBox2.BringToFront();
            label28.Visible = false;
            //comboBox1.Focus();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            MyDefine.myXET.AddTraceInfo("修改模式选择为：" + comboBox2.SelectedItem.ToString());
            label28.Text = comboBox2.SelectedItem.ToString();
            comboBox2.Visible = false;
            label28.Visible = true;
        }

        private void comboBox2_Leave(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex == -1) comboBox2.SelectedIndex = 0;
            label28.Text = comboBox2.SelectedItem.ToString();
            comboBox2.Visible = false;
            label28.Visible = true;
        }

        private void textBoxCTN_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                //只允许输入数字和删除键和回车键
                if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != 8) && (e.KeyChar != 13))
                {
                    e.Handled = true;
                    return;
                }
                // 如果第一位为0，且输入的不是删除键，则不允许输入
                if ((e.KeyChar != 8) && (((TextBox)sender).Text == "0"))
                {
                    e.Handled = true;
                    return;
                }
                if (e.KeyChar == 13)
                {
                    label38.Text = textBoxCTN.Text;
                    textBoxCTN.Visible = false;
                    //calTotalTestTimes();    //计算采样条数
                    if (label38.Text == "") return;
                    switch (comboBox3.Text)
                    {
                        case "秒":
                            if (Convert.ToDateTime(label32.Text).AddSeconds(Double.Parse(label38.Text)) < DateTime.Now)
                            {
                                dtpStartTime.Value = DateTime.Now;
                                label32.Text = dtpStartTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            MyDefine.myXET.AddTraceInfo("修改结束时间为：" + Convert.ToDateTime(label32.Text).AddSeconds(Double.Parse(label38.Text)));
                            break;
                        case "分":
                            if (Convert.ToDateTime(label32.Text).AddMinutes(Double.Parse(label38.Text)) < DateTime.Now)
                            {
                                dtpStartTime.Value = DateTime.Now;
                                label32.Text = dtpStartTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            MyDefine.myXET.AddTraceInfo("修改结束时间为：" + Convert.ToDateTime(label32.Text).AddMinutes(Double.Parse(label38.Text)));
                            break;
                        case "时":
                            if (Convert.ToDateTime(label32.Text).AddHours(Double.Parse(label38.Text)) < DateTime.Now)
                            {
                                dtpStartTime.Value = DateTime.Now;
                                label32.Text = dtpStartTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            MyDefine.myXET.AddTraceInfo("修改结束时间为：" + Convert.ToDateTime(label32.Text).AddHours(Double.Parse(label38.Text)));
                            break;
                        case "天":
                            if (Convert.ToDateTime(label32.Text).AddHours(Double.Parse(label38.Text)) < DateTime.Now)
                            {
                                dtpStartTime.Value = DateTime.Now;
                                label32.Text = dtpStartTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            MyDefine.myXET.AddTraceInfo("修改结束时间为：" + Convert.ToDateTime(label32.Text).AddDays(Double.Parse(label38.Text)));
                            break;
                        default: break;
                    }
                    calTotalTestTimes();    //计算采样条数
                }
            }
            catch (Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("输入数据错误：" + ex.ToString());
            }
        }

        //焦点丢失退出编辑状态
        private void textBoxCTN_Leave(object sender, EventArgs e)
        {
            try
            {
                label38.Text = textBoxCTN.Text;
                textBoxCTN.Visible = false;
                //calTotalTestTimes();    //计算采样条数
                if (label38.Text == "") return;
                switch (comboBox3.Text)
                {
                    case "秒":
                        if (Convert.ToDateTime(label32.Text).AddSeconds(Double.Parse(label38.Text)) < DateTime.Now)
                        {
                            dtpStartTime.Value = DateTime.Now;
                            label32.Text = dtpStartTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        MyDefine.myXET.AddTraceInfo("修改结束时间为：" + Convert.ToDateTime(label32.Text).AddSeconds(Double.Parse(label38.Text)));
                        break;
                    case "分":
                        if (Convert.ToDateTime(label32.Text).AddMinutes(Double.Parse(label38.Text)) < DateTime.Now)
                        {
                            dtpStartTime.Value = DateTime.Now;
                            label32.Text = dtpStartTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        MyDefine.myXET.AddTraceInfo("修改结束时间为：" + Convert.ToDateTime(label32.Text).AddMinutes(Double.Parse(label38.Text)));
                        break;
                    case "时":
                        if (Convert.ToDateTime(label32.Text).AddHours(Double.Parse(label38.Text)) < DateTime.Now)
                        {
                            dtpStartTime.Value = DateTime.Now;
                            label32.Text = dtpStartTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        MyDefine.myXET.AddTraceInfo("修改结束时间为：" + Convert.ToDateTime(label32.Text).AddHours(Double.Parse(label38.Text)));
                        break;
                    case "天":
                        if (Convert.ToDateTime(label32.Text).AddHours(Double.Parse(label38.Text)) < DateTime.Now)
                        {
                            dtpStartTime.Value = DateTime.Now;
                            label32.Text = dtpStartTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        MyDefine.myXET.AddTraceInfo("修改结束时间为：" + Convert.ToDateTime(label32.Text).AddDays(Double.Parse(label38.Text)));
                        break;
                    default: break;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("输入数据格式错误：" + ex.ToString());
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            label39.Text = comboBox3.SelectedItem.ToString();
            comboBox3.Visible = false;
            label39.Visible = true;
        }

        private void comboBox3_Leave(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex == -1) comboBox3.SelectedIndex = 1;
            label39.Text = comboBox3.SelectedItem.ToString();
            comboBox3.Visible = false;
            label39.Visible = true;
            calTotalTestTimes();
        }

        private void label38_DoubleClick(object sender, EventArgs e)
        {
            textBoxCTN.Text = label38.Text;
            textBoxCTN.Visible = true;
            textBoxCTN.BringToFront();
            textBoxCTN.Focus();
        }

        private void label38_TextChanged(object sender, EventArgs e)
        {
            MyDefine.myXET.AddTraceInfo("修改持续时间为：" + label25.Text);
            calTotalTestTimes();    //计算采样条数
        }

        private void label39_DoubleClick(object sender, EventArgs e)
        {
            comboBox3.Visible = true;
            comboBox3.BringToFront();
            label39.Visible = false;
        }

        private void label39_TextChanged(object sender, EventArgs e)
        {
            MyDefine.myXET.AddTraceInfo("修改持续时间单位：" + label39.Text);
            calTotalTestTimes();    //计算采样条数
        }

        /// <summary>
        /// 计算采样条数
        /// </summary>
        public void calTotalTestTimes()
        {
            try
            {
                if (label25.Text == "" || label26.Text == "" || label32.Text == "" || label38.Text == "" || label39.Text == "") return;

                UInt16 mySpan = Convert.ToUInt16(label25.Text);
                DateTime dateStart = Convert.ToDateTime(label32.Text);
                DateTime dateEnd = Convert.ToDateTime(label32.Text);
                switch (comboBox3.Text)
                {
                    case "秒": dateEnd = Convert.ToDateTime(label32.Text).AddSeconds(Double.Parse(label38.Text)); break;
                    case "分": dateEnd = Convert.ToDateTime(label32.Text).AddMinutes(Double.Parse(label38.Text)); break;
                    case "时": dateEnd = Convert.ToDateTime(label32.Text).AddHours(Double.Parse(label38.Text)); break;
                    case "天": dateEnd = Convert.ToDateTime(label32.Text).AddDays(Double.Parse(label38.Text)); break;
                    default: break;
                }

                UNIT myUnit = (UNIT)Enum.Parse(typeof(UNIT), label26.Text);     //根据字符串(枚举名称)生成枚举实例
                Int32 meUnit = (Byte)myUnit;
                TimeSpan totalTime = dateEnd.Subtract(dateStart);               //测试总时长
                ulong myDuration = (ulong)(totalTime.TotalSeconds);             //测试总秒数
                if (myDuration < 0) myDuration = 0;                             //测试总秒数不能为负

                //计算测试间隔时间(秒）
                UInt32 timePeriod = mySpan * MyDefine.myXET.meArrUnit[meUnit];


                //更新采样条数
                if (timePeriod <= 0)
                {
                    label27.Text = "0";
                }
                else
                {
                    label27.Text = (myDuration / timePeriod + 1).ToString();     //采样条数
                                                                                 //if (myDuration % timePeriod == 0)
                                                                                 //{
                                                                                 //label27.Text = (myDuration / timePeriod + 1).ToString();     //采样条数
                                                                                 //}
                                                                                 //else
                                                                                 //{
                                                                                 //    label27.Text = (myDuration / timePeriod).ToString();     //采样条数
                                                                                 //}
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("计算采样条数失败：" + ex.ToString());
            }
        }


        #endregion

    }
}

//end