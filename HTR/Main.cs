using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;

namespace HTR
{
    public partial class Main : Form
    {
        private Boolean isRun = false;//使能导出和取消后停止通讯保存
        public UInt16 total = 0;//控制进度条
        private String myPath = MyDefine.myXET.userDAT;

        #region 子窗体定义

        MenuEnterForm myEnterForm = new MenuEnterForm();
        MenuLoginForm myLoginForm = new MenuLoginForm();
        MenuAccountPanel myAccountPanel = new MenuAccountPanel();
        MenuSetPanel mySetPanel = new MenuSetPanel();
        MenuCodePanel myCodePanel = new MenuCodePanel();
        MenuDataCurvePanel myDataCurvePanel = new MenuDataCurvePanel();             //数据曲线界面
        MenuCalCurvePanel myCalCurvePanel = new MenuCalCurvePanel();             //数据曲线界面
        MenuCalPanel myCalPanel = new MenuCalPanel();                   //校准标定界面
        MenuVerifyPanel myVerifyPanel = new MenuVerifyPanel();
        MenuReportPanel myReportPanel = new MenuReportPanel();
        MenuDataPanel myDataPanel = new MenuDataPanel();
        MenuTracePanel myTracePanel = new MenuTracePanel();
        MenuPDFViewPanel myPDFViewPanel = new MenuPDFViewPanel();
        MenuSettingPanel mySettingPanel = new MenuSettingPanel();
        MenuSelectPanel mySelectPanel = new MenuSelectPanel();
        MenuPermissionPanel myPermissionPanel = new MenuPermissionPanel();
        MenuSTDPanel mySTDPanel = new MenuSTDPanel();

        List<PictureBox> myPictureArray = new List<PictureBox>();       //产品图片数组
        List<Label> myLabelArray = new List<Label>();                   //产品编号数组
        List<Label> myReadNumArray = new List<Label>();                 //产品读数总数数组
        List<ProgressBar> myProBarArray = new List<ProgressBar>();      //产品读数时进度条数组

        #endregion

        #region 窗体加载/关闭

        public Main()
        {
            InitializeComponent();
            
            #region 实时显示系统时间

            new Thread(() =>        //实时显示时间
            {
                while (true)
                {
                    try
                    {
                        label1.BeginInvoke(new MethodInvoker(() =>
                        label1.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    }
                    catch { }
                    Thread.Sleep(1000);
                }
            })
            { IsBackground = true }.Start();

            #endregion 

        }

        //程序加载
        private void Main_Load(object sender, EventArgs e)      
        {
            try
            {
                //将当前主界面作为参数传递到XET.Function类，方便在其中做窗体切换
                MyDefine.myXET = new XET(this);                 //必须放在最前面，定义时里面的全局变量会被重置
                MyDefine.myXET.meComputer = GetComputerInfo();  //获取电脑名

                this.Hide();
                //InitializationRegister();     //注册
                //CheckForIllegalCrossThreadCalls = false;    //取消控件跨线程检测（不推荐有时会出现一些莫名奇妙的错误如控件不能加载等问题）
                
                Application.EnableVisualStyles();
                //Application.SetCompatibleTextRenderingDefault(false);
                //if (myEnterForm.ShowDialog() == DialogResult.OK)        //显示进入界面
                //{
                    //login_Account("欢迎使用！");
                //}

                myEnterForm.Show();
                Application.DoEvents();
                System.Threading.Thread.Sleep(1000);
                myEnterForm.Close();

                if (myLoginForm.ShowDialog() == DialogResult.OK)  //显示MenuAccountForm登陆界面
                {
                    //保存当前用户名
                    MyDefine.myXET.userName = MyDefine.myXET.meLoginUser.Split(';')[(int)ACCOUNT.USER];
                    MyDefine.myXET.userPassword = MyDefine.myXET.meLoginUser.Split(';')[(int)ACCOUNT.PSWD];

                    //显示选择界面
                    panel6.BringToFront();
                    panel6.Controls.Add(mySelectPanel);
                    mySelectPanel.Dock = DockStyle.Fill;
                    mySelectPanel.BringToFront();

                    //显示用户名
                    panel4.Visible = true;                                   //显示左侧按钮panel
                    label7.Text = "当前登录用户：" + MyDefine.myXET.userName;      //显示当前登陆用户名
                    label8.Text = "软件版本：" + Constants.SW_Version;       //显示软件版本

                    InitForm();                                              //各界面控件尺寸初始化
                    InitMidForm();                                           //记录各子界面的控件尺寸初始值
                    BuildPictureArray();                                     //创建控件数组(注创建控件数组必须放在InitForm()后面，因为二者都用都来控件的Tag属性)
                    MyDefine.myXET.loadReportInfo();                         //加载报告信息(报告编号+标准器列表)

                    //界面最大化
                    this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                    this.Show();
                    comboBox1.SelectedIndex = 0;
                    comboBox3.SelectedIndex = 0;
                    MyDefine.myXET.meInterface = "连接界面";
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("主界面加载失败：" + ex.ToString());
            }
        }

        //程序退出
        private void Main_FormClosing(object sender, FormClosingEventArgs e)    
        {
            //确认是否退出
            if (MessageBox.Show("是否确认退出此应用程序？", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            try
            {
                //关闭串口
                while (MyDefine.myXET.mePort.IsOpen)
                {
                    MyDefine.myXET.mePort.Close();
                }

                //保存校准表、标定表的读数时间行
                MyDefine.myXET.savePreCalTable();

                //退出所有窗口
                MyDefine.myXET.AddToTraceRecords("主界面", "退出");
                MyDefine.myXET.SaveTraceRecords();
                System.Environment.Exit(0);
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("退出失败：" + ex.ToString());
            }            
        }

        #endregion

        #region 界面按钮事件

        #region 返回主页

        //关闭串口
        private void closeSeriport()
        {
            MyDefine.myXET.meTask = TASKS.disconnected;
            MyDefine.myXET.meDotComplete = false;           //重新连接设备后，默认未标定过，不显示标定后曲线(标定成功后置位)
            ResetDevicePictures();      //复位设备连接图片
            Application.DoEvents();
            if (MyDefine.myXET.mePort.IsOpen)
            {
                if (MyDefine.myXET.mePort.IsOpen) MyDefine.myXET.mePort.Close();
            }
        }

        //返回主页
        private void button1_Click(object sender, EventArgs e)
        {
            panel6.Visible = true;
            panel6.Controls.Clear();
            panel6.BringToFront();
            panel6.Controls.Add(mySelectPanel);
            mySelectPanel.Dock = DockStyle.Fill;
            mySelectPanel.BringToFront();
            MyDefine.myXET.AddToTraceRecords("主界面", "返回主页");
            closeSeriport();
            MyDefine.myXET.SaveTraceRecords();                          //保存追踪日志
            label29.Visible = false;
            checkBox1.Visible = false;
        }

        //返回主页
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            panel6.Visible = true;
            panel6.Controls.Clear();
            panel6.BringToFront();
            panel6.Controls.Add(mySelectPanel);
            mySelectPanel.Dock = DockStyle.Fill;
            mySelectPanel.BringToFront();
            MyDefine.myXET.AddToTraceRecords("主界面", "返回主页");
            closeSeriport();
            MyDefine.myXET.SaveTraceRecords();                          //保存追踪日志
        }

        #endregion

        #region 切换账号

        //切换账号
        private void button2_Click(object sender, EventArgs e)
        {
            groupBox1.Focus(); Application.DoEvents();       //将焦点从button上移走，使button每次单击都有点击效果

            MyDefine.myXET.switchMainPanel(8);               //切换到切换账号界面
        }

        //切换账号
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            groupBox1.Focus(); Application.DoEvents();       //将焦点从button上移走，使button每次单击都有点击效果

            MyDefine.myXET.switchMainPanel(8);               //切换到切换账号界面
        }

        #endregion

        #region 帮助说明

        //帮助说明
        private void btnHelp_Click(object sender, EventArgs e)
        {
            if (MyDefine.myXET.meTask == TASKS.disconnected)
            {
                System.Diagnostics.Process.Start(Application.StartupPath + @"\hel\无线验证系统产品说明书1.1.pdf");//设备未连接或繁忙打开无线验证系统产品说明书 
                Application.DoEvents();
            }
            else
            {
                switch (MyDefine.myXET.meType)
                {
                    //case DEVICE.HTT: System.Diagnostics.Process.Start(Application.StartupPath + @"\hel\说明书-HTT1.pdf"); break;
                    //case DEVICE.HTH: System.Diagnostics.Process.Start(Application.StartupPath + @"\hel\说明书-HTH1.pdf"); break;
                    //case DEVICE.HTP: System.Diagnostics.Process.Start(Application.StartupPath + @"\hel\说明书-HTP1.pdf"); break;
                    case DEVICE.HTQ: System.Diagnostics.Process.Start(Application.StartupPath + @"\hel\说明书-HTQ1.pdf"); break;
                    default: MessageBox.Show("当前型号产品暂无说明书，请补充放置" + Application.StartupPath + @"\hel文件夹下"); break;
                }
            }
        }

        #endregion

        #region 连接界面 -- 左侧切换按钮

        private void btnDevice_Click(object sender, EventArgs e)
        {
            if (MyDefine.myXET.meInterface == "连接界面") return;        //已处于当前界面，忽略

            ClearAllUserPanels();
            ResetLeftButtons();                 //复位左侧按钮图片为未选中状态
            btnDevice.BackgroundImage = global::HTR.Properties.Resources.连接界面_1;    //设置按钮为选中状态
            MyDefine.myXET.AddToTraceRecords("主界面", "连接界面");
            MyDefine.myXET.meInterface = "连接界面";
            MyDefine.myXET.SaveTraceRecords();                          //保存追踪日志
        }

        #endregion

        #region 设备连接

        public Boolean flag_Connecting = false;     //是否连接中

        //设备连接
        private void btnConnect_Click(object sender, EventArgs e)
        {
            //创建载入数据的路径记录文件
            if (!Directory.Exists(MyDefine.myXET.userCFGPATH))
            {
                Directory.CreateDirectory(MyDefine.myXET.userCFGPATH);
            }

            if (!System.IO.File.Exists(MyDefine.myXET.userCFGPATH + @"\DataSavePathLog.txt"))
            {
                FileStream fs1 = new FileStream(MyDefine.myXET.userCFGPATH + @"\DataSavePathLog.txt", FileMode.Create);//创建写入文件 
                fs1.Close();
            }

            Console.WriteLine(pictureBox1.Size.ToString());
            groupBox1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            if (!MyDefine.myXET.CheckPermission(STEP.设备连接)) return;       //核对权限

            if (MyDefine.myXET.meTask == TASKS.reading)
            {
                MessageBox.Show("正在读取设备，请稍后再试！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (flag_Connecting == false)           //单击连接，置位连接中标记
            {
                flag_Connecting = true;
                //MyDefine.myXET.AddToTraceRecords("连接界面", "设备连接");
                MyDefine.myXET.AddTraceInfo("设备连接");
            }
            else                                    //设备连接过程中再次单击连接，则取消连接
            {
                flag_Connecting = false;
                //MyDefine.myXET.AddToTraceRecords("连接界面", "取消连接");
                MyDefine.myXET.AddTraceInfo("取消连接");
                return;
            }

            try
            {
                Boolean ret = true;
                btnConnect.Text = " 连接中";
                Application.DoEvents();
                chbManual.Enabled = false;
                btnConnect.TextAlign = ContentAlignment.MiddleLeft;
                MyDefine.myXET.meTask = TASKS.disconnected;
                MyDefine.myXET.meDotComplete = false;           //重新连接设备后，默认未标定过，不显示标定后曲线(标定成功后置位)
                ResetDevicePictures();      //复位设备连接图片
                Application.DoEvents();

                if (chbManual.Checked)
                {
                    ret = ManualConnection();
                }
                else
                {
                    ret = AutoConnection();
                }
                
                btnConnect.Text = "设备连接";
                chbManual.Enabled = true;
                btnConnect.TextAlign = ContentAlignment.MiddleCenter;

                //循环连接
                //if(flag_Connecting == true)
                //{
                //    flag_Connecting = false;
                //    btnConnect_Click(null, null);
                //}

                //直接切换图片：图片较清晰
                if (ret == false)   //连接失败
                {
                    MyDefine.myXET.meTask = TASKS.disconnected;
                    if (flag_Connecting)
                    {
                        MyDefine.myXET.ShowWrongMsg("设备连接失败！");
                    }
                    else
                    {
                        MyDefine.myXET.ShowWrongMsg("设备连接已取消！");
                    }
                    flag_Connecting = false;
                    MyDefine.myXET.AddTraceInfo("连接失败");
                }
                else                //连接成功
                {
                    flag_Connecting = false;
                    MyDefine.myXET.meTask = TASKS.run;     //串口准备就绪
                    //MyDefine.myXET.AddToTraceRecords("连接界面", "连接成功");
                    MyDefine.myXET.AddTraceInfo("连接成功");
                    //mySetPanel.UpdateInterfaceInfo();  //更新设备设置界面中的部分参数

                    //仅HTQ会显示激活屏幕控件
                    if (MyDefine.myXET.meType == DEVICE.HTQ)
                    {
                        label29.Visible = true;
                        checkBox1.Visible = true;
                    }
                    else
                    {
                        label29.Visible = false;
                        checkBox1.Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("设备连接失败：" + ex.ToString());
            }  
        }

        #endregion

        #region 设备设置

        //设备设置 -- 显示设置界面
        private void btnDeviceSet_Click(object sender, EventArgs e)
        {
            groupBox1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            if (!MyDefine.myXET.CheckPermission(STEP.设备设置)) return;       //核对权限

            if (ClearAllUserPanels() == false) return;

            panel1.Controls.Add(mySetPanel);
            mySetPanel.BringToFront();
            mySetPanel.AddMyUpdateEvent();
            panel3.Visible = true;
            //mySetPanel.Dock = DockStyle.Fill;
            //MyDefine.myXET.AddToTraceRecords("连接界面", "设备设置");
            MyDefine.myXET.AddTraceInfo("设备设置");
            MyDefine.myXET.meInterface = "设备设置";
        }
        #endregion

        #region 编号管理

        //编号管理 -- 显示编号管理界面
        private void btnCodes_Click(object sender, EventArgs e)
        {
            groupBox1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            if (ClearAllUserPanels() == false) return;

            panel1.Controls.Add(myCodePanel);
            myCodePanel.BringToFront();
            myCodePanel.AddMyUpdateEvent();
            panel3.Visible = true;
            //mySetPanel.Dock = DockStyle.Fill;
            //MyDefine.myXET.AddToTraceRecords("连接界面", "编号管理");
            MyDefine.myXET.AddTraceInfo("编号管理");
            MyDefine.myXET.meInterface = "编号管理";
        }
        #endregion

        #region 设备读取

        //设备读取(数据导出) -- 显示设备读取界面
        private void btnDeviceRead_Click0(object sender, EventArgs e)
        {
            if (!MyDefine.myXET.CheckPermission(STEP.设备读取)) return;       //核对权限
            if (ClearAllUserPanels() == false) return;
            MyDefine.myXET.AddTraceInfo("设备读取");

            panel1.Controls.Add(myDataCurvePanel);
            myDataCurvePanel.BringToFront();
            myDataCurvePanel.AddMyUpdateEvent();
            //myDataCurvePanel.Dock = DockStyle.Fill;
            panel3.Visible = true;
        }

        //设备读取(数据导出)
        private void btnDeviceRead_Click(object sender, EventArgs e)
        {
            try
            {
                groupBox1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

                if (!MyDefine.myXET.checkDeviceStatus()) return;    //设备未连接或繁忙
                //不存在则创建文件夹dat
                if (!Directory.Exists(MyDefine.myXET.userDAT))
                {
                    Directory.CreateDirectory(MyDefine.myXET.userDAT);
                }
                //存在文件夹dat创建默认保存路径文件夹
                else
                {
                    String filepath = MyDefine.myXET.userCFGPATH + @"\DataSavePathLog.txt";
                    StreamReader sr = new StreamReader(filepath);
                    string str = sr.ReadToEnd();
                    sr.Close();
                    if (str != "")
                    {
                        MyDefine.myXET.userDAT = str;
                    }
                    else
                    {
                        MessageBox.Show("尚未选择保存路径，请重新选择");
                        return;
                    }

                }
                
                /*
                //保存文件弹窗
                System.Windows.Forms.SaveFileDialog DialogSave = new System.Windows.Forms.SaveFileDialog();
                DialogSave.Filter = "文件(*.tmp)|*.tmp|所有文件(*.*)|*.*";
                DialogSave.InitialDirectory = MyDefine.myXET.meDatPath;                     //设置默认路径
                if (DialogSave.ShowDialog() == DialogResult.Cancel) return;                 //取消保存
                MyDefine.myXET.meDatPath = Path.GetDirectoryName(DialogSave.FileName);      //保存当前选中路径
                */

                isRun = true;
                btnCancel.Visible = true;                       //显示取消按钮
                btnDeviceRead.Visible = false;                  //隐藏读出按钮
                btnCancel.Location = btnDeviceRead.Location;    //将取消按钮显示在原先批量读取按钮的位置
                progressBar1.Value = progressBar1.Minimum + 3;  //进度条显示一点值，防止数据太多时好长时间看不到进度条            
                MyDefine.myXET.meTask = TASKS.reading;
                MyDefine.myXET.meTips = "[DEVICE READ]" + Environment.NewLine;
                //MyDefine.myXET.AddToTraceRecords("连接界面", "设备读取");
                MyDefine.myXET.AddTraceInfo("设备读取");
                ShowProBarArr();                                //显示全部已连接设备的进度条
                ResetReadNumArr();                              //清空读取数量控件数组

                //新建线程读取设备数据
                ThreadStart ts = new ThreadStart(readDataFromDevices);
                Thread t = new Thread(ts);
                t.Start();
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("读取失败：" + ex.ToString());
            }
        }

        /// <summary>
        /// 取消读取
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            isRun = false;
            btnCancel.Visible = false;
            btnDeviceRead.Visible = true;
            MyDefine.myXET.meTask = TASKS.run;
            HideProBarArr();                    //隐藏全部读取进度条
            //MyDefine.myXET.AddToTraceRecords("连接界面", "取消读取");
            MyDefine.myXET.AddTraceInfo("取消读取");
        }

        #region 读取单个/多个设备的测试数据

        //读取设备的测试数据
        public void readDataFromDevices()
        {
            try
            {
                Boolean ret = true;
                Int32 totalTimes = 0;                                  //总共需要读取的次数
                String message = Environment.NewLine;
                Int32 probarMax = progressBar1.Maximum;
                Int32 deviceNum = MyDefine.myXET.meDUTAddrArr.Count;   //已读取设备的数目
                Byte activeDUTAddress = MyDefine.myXET.meActiveAddr;    //保存当前正在连接的设备地址，批量设置完后再设置回来
                MyDefine.myXET.meTips = "[BATCH DEVICES READ]" + Environment.NewLine;

                for (int i = 0; i < MyDefine.myXET.meDUTAddrArr.Count; i++)            //遍历当前设备
                {
                    if (isRun == false) break;                                          //取消读取,则直接退出
                    MyDefine.myXET.meActiveAddr = MyDefine.myXET.meDUTAddrArr[i];      //切换当前设备地址
                    MyDefine.myXET.myMem.Clear();                                       //清空数据数组
                    MyDefine.myXET.corMem.Clear();                                      //清空修正数据数组
                    total = 0;
                    uint myAddress = 0;
                    int dataNum = 0;    //发送一条指令后，读到的数据字节个数
                    int readNum = 0;    //已读取数据个数

                    MyDefine.myXET.meTips += "ReadREG_Device:" + Environment.NewLine;
                    ret = MyDefine.myXET.readDevice();          //先读取设备型号等参数

                    if (ret)
                    {
                        MyDefine.myXET.meTips += "readJSN:" + Environment.NewLine;
                        ret = MyDefine.myXET.readJSN();                     //读取设备编号
                    }
                    else
                    {
                        MessageBox.Show("读取设备型号错误");
                        continue;
                    }

                    if (ret)
                    {
                        MyDefine.myXET.meTips += "readUSN:" + Environment.NewLine;
                        ret = MyDefine.myXET.readUSN();                     //读取管理编号
                    }
                    else
                    {
                        MessageBox.Show("读取设备型号,设备编号，测量范围错误");
                        continue;
                    }

                    if (ret)
                    {
                        MyDefine.myXET.meTips += "readUTXT:" + Environment.NewLine;
                        ret = MyDefine.myXET.readUTXT();                     //读取用户备注信息
                    }
                    else
                    {
                        MessageBox.Show("读取用户定义的编号错误");
                        continue;
                    }

                    if (ret)
                    {
                        MyDefine.myXET.meTips += "readTime:" + Environment.NewLine;
                        ret = MyDefine.myXET.readTime();                    //读取设备计量校准时间、换电池时间等
                    }
                    else
                    {
                        MessageBox.Show("读取用户备注信息错误");
                        continue;
                    }

                    if (ret)
                    {
                        MyDefine.myXET.meTips += "readCorData:" + Environment.NewLine;
                        if(MyDefine.myXET.meType == DEVICE.HTT || MyDefine.myXET.meType == DEVICE.HTP)
                        { 
                            ret = MyDefine.myXET.readCorData(0x21);                 //读取设备数据校准
                        }
                        else
                        {
                            ret = MyDefine.myXET.readCorData(0x42);                 //读取设备数据校准
                        }
                    }
                    else
                    {
                        MessageBox.Show("读取设备计量校准时间、换电池时间等错误");
                        continue;
                    }

                    if (ret)
                    {
                        MyDefine.myXET.meTips += "ReadREG_Jobset:" + Environment.NewLine;
                        ret = MyDefine.myXET.readJobset();                  //读取设备开始、结束等参数

                        //计算测试采样条数及读取总次数
                        UInt32 timePeriod = MyDefine.myXET.meSpan * MyDefine.myXET.meArrUnit[MyDefine.myXET.meUnit];        //采样间隔（秒）
                        timePeriod = (timePeriod == 0) ? 1 : timePeriod;
                        Int32 totalData = (int)(MyDefine.myXET.meDuration / timePeriod);                                    //采样条数
                        totalTimes = totalData * 4 / 128 + 1;                                                               //需要读取的次数(1条数据最多占4个字节(HTH),每次可读取128个字节)
                        totalTimes = (totalTimes <= 5) ? 100 : totalTimes;                                                  //需要读取的总次数
                    }
                    else
                    {
                        MessageBox.Show("读取数据修正错误");
                        continue;
                    }

                    if (!ret)           //以上参数未全部读取成功，不再继续读取数据
                    {
                        MessageBox.Show("参数读取失败：A04  \r\n 读取设备开始、结束时间、间隔时间单位、测试间隔时间(单位：秒、分、时、天)、测试持续时间(秒)错误");
                        message += "设备" + (i + 1).ToString() + "参数读取失败！" + Environment.NewLine;
                        UpdateProgressBar(i, probarMax);            //把进度条拉满
                        continue;
                    }

                    if (ret)
                    {
                        MyDefine.myXET.meTips += "REG_JOBREC:" + Environment.NewLine;
                        for (int j = 0; j < totalTimes; j++, total++)
                        {
                            myAddress = Constants.REC_ADDRESS + Constants.REC_ONEPAGE * total;  //更新地址
                            ret = MyDefine.myXET.readJobrec(myAddress, ref dataNum);            //发送指令并读取测试数据
                            //if (total == 100) total = 99;                                       //防止进度条在数据读取中途被拉满
                            UpdateProgressBar(i, 3 + (probarMax - 3) * total / totalTimes);     //更新进度条

                            if (ret) readNum += (MyDefine.myXET.meType != DEVICE.HTT) ? dataNum / 4 : dataNum / 2;//当前已读取数据个数
                            if (dataNum == -1)
                            {
                                MessageBox.Show("连续读取10次均未成功");
                                break;                       //连续读取10次均未成功
                            }
                            if (dataNum == -2)
                            {
                                MessageBox.Show("读取过程中抛出异常");
                                break;                       //读取过程中抛出异常
                            }
                            if (dataNum < Constants.REC_ONEPAGE)
                            {
                                break;     //读到的字节不满128个，测试数据已读完
                            }

                            if (isRun == false)                             //取消读取,则直接退出
                            {
                                deviceNum = i + 1;
                                break;
                            }
                        }
                    }

                    if (dataNum < 0) dataNum = 0;
                    UpdateProgressBar(i, probarMax);                        //把进度条拉满
                    ShowReadNumArr(i, readNum);                             //显示已读取数量
                    ret = MyDefine.myXET.mem_SaveToLog();                   //保存测试数据(不存在则创建文件)

                    if (ret)
                    {
                        string msg = isRun ? "读取成功，已读取数据个数：" : "读取终断，已读取数据个数：";
                        message += "设备" + (i + 1).ToString() + msg + readNum + Environment.NewLine;
                    }
                    else
                    {
                        MessageBox.Show("保存测试数据异常");
                        message += "设备" + (i + 1).ToString() + "读取失败，已读取数据个数：" + readNum + Environment.NewLine;
                    }

                    if (MyDefine.myXET.tempCount > 0 || MyDefine.myXET.humCount > 0)
                    {
                        ret = MyDefine.myXET.mem_SaveToCorLog();            //保存修正数据（不存在则创建文件）
                        MyDefine.myXET.tempCount = 0;
                        MyDefine.myXET.humCount = 0;
                    }

                    if (!ret)
                    {
                        MessageBox.Show("保存修正数据异常");
                        continue;
                    }
                }

                Application.DoEvents();
                System.Threading.Thread.Sleep(800);                    //让最后一个进度条显示完全
                MyDefine.myXET.meActiveAddr = activeDUTAddress;        //将当前设备地址切换回批量设置前的已连接设备地址
                MyDefine.myXET.meTips += "ReadREG_Device:" + Environment.NewLine;
                MyDefine.myXET.readDevice();                            //重新读取当前设备型号等参数

                readDataComplete();                                     //数据读取并保存完成，复位界面控件
                MessageBox.Show("读取完成，读取设备数：" + deviceNum + message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                MyDefine.myXET.SaveCommunicationTips();                  //将调试信息保存到操作日志
                MyDefine.myXET.AddTraceInfo("读取完成");
            }
            catch (Exception ex)
            {
                MyDefine.myXET.AddTraceInfo("读取失败");
                MyDefine.myXET.ShowWrongMsg("读取失败：" + ex.ToString());
            }
        }

        #endregion

        #region 读取测试数据后，跨线程更新界面信息

        //更新进度条
        private void UpdateProgressBar(int probarIdx, int val)
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    if (val <= myProBarArray[probarIdx].Maximum)
                    {
                        myProBarArray[probarIdx].Value = val;  //更新进度条
                        System.Windows.Forms.Application.DoEvents();
                    }
                    else
                    {
                        myProBarArray[probarIdx].Value = myProBarArray[probarIdx].Maximum;
                        System.Windows.Forms.Application.DoEvents();
                    }
                }));
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("跨线程更新界面信息失败：" + ex.ToString());
            }
        }

        /// <summary>
        /// 显示第n个设备的测试数据读取数量
        /// </summary>
        /// <param name="idx">设备索引</param>
        /// <param name="datanum">读取数量</param>
        public void ShowReadNumArr(int idx, int datanum)
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    myReadNumArray[idx].Text = datanum.ToString(); 
                    System.Windows.Forms.Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("显示读取数量失败：" + ex.ToString());
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            
        }

        //数据读取结束，更新进度条、按钮信息
        private void readDataComplete()
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    btnCancel.Visible = false;
                    btnDeviceRead.Visible = true;       //显示读取按钮
                    MyDefine.myXET.meTask = TASKS.run;
                    HideProBarArr();                    //隐藏全部读取进度条
                }));
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("跨线程更新界面信息失败：" + ex.ToString());
            }
        }

        #endregion

        #endregion

        #region 保存路径
        private void btnSavePath_Click(object sender, EventArgs e)
        {
            //if (!MyDefine.myXET.checkDeviceStatus()) return;    //设备未连接或繁忙

            FolderSelectDialog dialog = new FolderSelectDialog();
            if (myPath == MyDefine.myXET.userDAT)
            {
                dialog.InitialDirectory = myPath;//设置此次默认目录为上一次选中目录
            }
            else
            {
                dialog.InitialDirectory = Application.StartupPath + @"\dat";
            }

            if (dialog.ShowDialog(this.Handle))
            {
                myPath = dialog.FileName;
                String filepath = MyDefine.myXET.userCFGPATH + @"\DataSavePathLog.txt";//在账户的dataPath文件夹保存存储路径
                System.IO.File.WriteAllText(filepath, myPath.ToString());//在cfg文件中写入存储路径
                MyDefine.myXET.userDAT = dialog.FileName;
            }
        }
        #endregion

        #region 数据处理 -- 左侧切换按钮

        //数据加载按钮 -- 显示数据加载界面
        private void btnData_Click(object sender, EventArgs e)
        {
            if (MyDefine.myXET.meInterface == "数据处理") return;        //已处于当前界面，忽略
            if (ClearAllUserPanels() == false) return;

            ResetLeftButtons();                 //复位左侧按钮图片为未选中状态
            btnData.BackgroundImage = global::HTR.Properties.Resources.数据处理_1;

            panel1.Controls.Add(myDataPanel);
            myDataPanel.BringToFront();
            myDataPanel.AddMyUpdateEvent();      //没有此语句，标定时常常不触发myUpdate()函数
            //panel3.Visible = true;             //显示返回按钮
            //myDataPanel.Dock = DockStyle.Fill;
            MyDefine.myXET.AddToTraceRecords("主界面", "数据处理");
            MyDefine.myXET.meInterface = "数据处理";
            MyDefine.myXET.SaveTraceRecords();                          //保存追踪日志
        }

        #endregion

        #region 校准界面 -- 左侧切换按钮

        //校准按钮 -- 显示校准界面
        private void btnCalibration_Click(object sender, EventArgs e)
        {
            if (MyDefine.myXET.meInterface == "校准界面") return;        //已处于当前界面，忽略
            if (ClearAllUserPanels() == false) return;

            ResetLeftButtons();                 //复位左侧按钮图片为未选中状态
            btnCalibration.BackgroundImage = global::HTR.Properties.Resources.校准界面_1;    //设置按钮为选中状态

            myCalPanel.Name = "校准";         //必须写在加载前，加载时才能进行名称判断
            panel1.Controls.Add(myCalPanel);
            myCalPanel.BringToFront();
            myCalPanel.AddMyUpdateEvent();      //没有此语句，标定时常常不触发myUpdate()函数
            //panel3.Visible = true;             //显示返回按钮
            //myCalPanel.Dock = DockStyle.Fill;
            //myCalPanel.HideCalibrationFunction();
            MyDefine.myXET.AddToTraceRecords("主界面", "校准界面");
            MyDefine.myXET.meInterface = "校准界面";
            MyDefine.myXET.SaveTraceRecords();                          //保存追踪日志

            /*
            if (ClearAllUserPanels() == false) return;

            myCalPanel.Name = "后校验";         //必须写在加载前，加载时才能进行名称判断
            panel1.Controls.Add(myCalPanel);
            myCalPanel.BringToFront();
            myCalPanel.AddMyUpdateEvent();      //没有此语句，标定时常常不触发myUpdate()函数
            panel3.Visible = true;
            myCalPanel.Dock = DockStyle.Fill;
            //myCalPanel.HideCalibrationFunction();
            */
        }

        #endregion

        #region 验证界面 -- 左侧切换按钮

        //验证按钮 -- 显示验证界面
        private void btnVerify_Click(object sender, EventArgs e)
        {
            if (MyDefine.myXET.meInterface == "验证界面") return;        //已处于当前界面，忽略
            if (ClearAllUserPanels() == false) return;

            ResetLeftButtons();                 //复位左侧按钮图片为未选中状态
            btnVerify.BackgroundImage = global::HTR.Properties.Resources.验证界面_1;    //设置按钮为选中状态

            panel1.Controls.Add(myVerifyPanel);
            myVerifyPanel.BringToFront();
            myVerifyPanel.AddMyUpdateEvent();      //更新界面
            //panel3.Visible = true;
            //myVerifyPanel.Dock = DockStyle.Fill;
            MyDefine.myXET.AddToTraceRecords("主界面", "验证界面");
            MyDefine.myXET.meInterface = "验证界面";
            MyDefine.myXET.SaveTraceRecords();                          //保存追踪日志
        }

        #endregion

        #region 报告查看 -- 左侧切换按钮

        //PDF报告按钮 -- 显示报告界面
        private void btnPDFReport_Click(object sender, EventArgs e)
        {
            if (MyDefine.myXET.meInterface == "报告查看") return;        //已处于当前界面，忽略
            if (ClearAllUserPanels() == false) return;

            ResetLeftButtons();                 //复位左侧按钮图片为未选中状态
            btnPDFReport.BackgroundImage = global::HTR.Properties.Resources.报告查看_1;    //设置按钮为选中状态

            //panel1.Controls.Add(myReportPanel);
            //myReportPanel.BringToFront();
            //myReportPanel.AddMyUpdateEvent();
            //panel3.Visible = true;
            //myReportPanel.Dock = DockStyle.Fill;

            panel1.Controls.Add(myPDFViewPanel);
            myPDFViewPanel.BringToFront();
            myPDFViewPanel.AddMyUpdateEvent();
            //panel3.Visible = true;             //显示返回按钮
            //myPDFViewPanel.Dock = DockStyle.Fill;
            MyDefine.myXET.AddToTraceRecords("主界面", "报告查看");
            MyDefine.myXET.meInterface = "报告查看";
            MyDefine.myXET.SaveTraceRecords();                          //保存追踪日志
        }

        #endregion

        #region 系统设置 -- 左侧切换按钮

        //设置按钮 -- 显示设置界面
        private void btnSetting_Click(object sender, EventArgs e)
        {
            if (MyDefine.myXET.meInterface == "系统设置") return;        //已处于当前界面，忽略
            if (ClearAllUserPanels() == false) return;

            ResetLeftButtons();                 //复位左侧按钮图片为未选中状态
            btnSetting.BackgroundImage = global::HTR.Properties.Resources.系统设置_1;    //设置按钮为选中状态

            panel1.Controls.Add(mySettingPanel);
            mySettingPanel.BringToFront();
            //panel3.Visible = true;             //显示返回按钮
            MyDefine.myXET.AddToTraceRecords("主界面", "系统设置");
            MyDefine.myXET.meInterface = "系统设置";
            MyDefine.myXET.SaveTraceRecords();                          //保存追踪日志
        }

        #endregion

        #region 返回按钮

        //返回按钮
        private void btnReturn_Click(object sender, EventArgs e)
        {
            try
            {
                //MenuReadPanel页面正在读取设备测试数据：Cancel则不会执行任何操作，OK则移除MenuReadPanel页面，触发其ParentChanged事件
                if (MyDefine.myXET.meTask == TASKS.reading)
                {
                    //确认是否取消切换页面操作
                    if (MessageBox.Show("正在读取设备，切换页面将会取消读取操作，是否继续？", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
                    {
                        return;   //取消切换页面操作
                    }
                }

                //三级界面，移除后退出函数
                if (panel1.Controls.Contains(myPermissionPanel))      //返回用户管理界面(移除PermissionPanel必须放在移除AccountPanel前面)
                {
                    MyDefine.myXET.meInterface = "用户管理";
                    panel1.Controls.Remove(myPermissionPanel);
                    return;
                }

                //二级界面，移除后回到主界面(一级界面)并显示返回按钮
                if (panel1.Controls.Contains(myCalCurvePanel) && myCalCurvePanel.Name == "标定曲线图")    //返回标定界面
                {
                    MyDefine.myXET.meInterface = "标定界面";
                    panel1.Controls.Remove(myCalCurvePanel);
                    return;
                }

                //二级界面，移除后回到主界面(一级界面)并显示返回按钮
                if (panel1.Controls.Contains(mySetPanel))    //返回设备连接界面
                {
                    MyDefine.myXET.meInterface = "设备连接";
                    panel1.Controls.Remove(mySetPanel);
                    //return;
                }

                //二级界面，移除后回到主界面(一级界面)并显示返回按钮
                if (panel1.Controls.Contains(myCodePanel))    //返回设备连接界面
                {
                    MyDefine.myXET.meInterface = "设备连接";
                    panel1.Controls.Remove(myCodePanel);
                    //return;
                }

                //二级界面，移除后回到主界面(一级界面)并显示返回按钮
                if (panel1.Controls.Contains(myDataCurvePanel))    //返回数据处理界面
                {
                    MyDefine.myXET.meInterface = "数据处理";
                    panel1.Controls.Remove(myDataCurvePanel);
                    //return;
                }

                //二级界面，移除后回到主界面(一级界面)并显示返回按钮
                if (panel1.Controls.Contains(myCalCurvePanel))    //返回校准界面
                {
                    MyDefine.myXET.meInterface = "校准界面";
                    panel1.Controls.Remove(myCalCurvePanel);
                    //return;
                }

                //二级界面，移除后回到主界面(一级界面)并显示返回按钮
                if (panel1.Controls.Contains(mySTDPanel))       //返回校准界面
                {
                    MyDefine.myXET.meInterface = "校准界面";
                    panel1.Controls.Remove(mySTDPanel);
                    //return;
                }

                //二级界面，移除后回到主界面(一级界面)并显示返回按钮
                if (panel1.Controls.Contains(myReportPanel))      //返回校准/验证界面
                {
                    MyDefine.myXET.meInterface = (myReportPanel.Name == "校准报告") ? "校准界面" : "验证界面";
                    panel1.Controls.Remove(myReportPanel);
                    //return;
                }

                //二级界面，移除后回到主界面(一级界面)并显示返回按钮
                if (panel1.Controls.Contains(myAccountPanel))    //返回setting界面
                {
                    MyDefine.myXET.meInterface = "系统设置";
                    panel1.Controls.Remove(myAccountPanel);
                    //return;
                }

                //二级界面，移除后回到主界面(一级界面)并显示返回按钮
                if (panel1.Controls.Contains(myTracePanel))    //返回setting界面
                {
                    MyDefine.myXET.meInterface = "系统设置";
                    panel1.Controls.Remove(myTracePanel);
                    //return;
                }

                //二级界面，移除后回到主界面(一级界面)并显示返回按钮
                if (panel1.Controls.Contains(myCalPanel) && myCalPanel.Name == "标定")    //返回setting界面
                {
                    MyDefine.myXET.meInterface = "系统设置";
                    panel1.Controls.Remove(myCalPanel);
                    //return;
                }

                //if (panel1.Controls.Contains(mySetPanel)) panel1.Controls.Remove(mySetPanel);
                //if (panel1.Controls.Contains(myCodePanel)) panel1.Controls.Remove(myCodePanel);
                //if (panel1.Controls.Contains(myAccountPanel)) panel1.Controls.Remove(myAccountPanel);
                //if (panel1.Controls.Contains(mySTDPanel)) panel1.Controls.Remove(mySTDPanel);
                //if (panel1.Controls.Contains(myDataCurvePanel)) panel1.Controls.Remove(myDataCurvePanel);
                //if (panel1.Controls.Contains(myCalCurvePanel)) panel1.Controls.Remove(myCalCurvePanel);
                //if (panel1.Controls.Contains(myCalPanel) && myCalPanel.Name == "标定") panel1.Controls.Remove(myCalPanel);
                //if (panel1.Controls.Contains(myTracePanel)) panel1.Controls.Remove(myTracePanel);

                //以下为主界面(一级界面)，不再移除
                //if (panel1.Controls.Contains(mySettingPanel)) panel1.Controls.Remove(mySettingPanel);
                //if (panel1.Controls.Contains(myCalPanel)) panel1.Controls.Remove(myCalPanel);
                //if (panel1.Controls.Contains(myVerifyPanel)) panel1.Controls.Remove(myVerifyPanel);
                //if (panel1.Controls.Contains(myDataPanel)) panel1.Controls.Remove(myDataPanel);
                //if (panel1.Controls.Contains(myPDFViewPanel)) panel1.Controls.Remove(myPDFViewPanel);
                panel3.Visible = false;                     //显示返回按钮

            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("返回失败：" + ex.ToString());
            }
        }

        //清除所有panel1中的子panel
        private Boolean ClearAllUserPanels()
        {
            try
            {
                //MenuReadPanel页面正在读取设备测试数据：Cancel则不会执行任何操作，OK则移除MenuReadPanel页面，触发其ParentChanged事件
                if (MyDefine.myXET.meTask == TASKS.reading)
                {
                    //确认是否取消切换页面操作
                    if (MessageBox.Show("正在读取设备，切换页面将会取消读取操作，是否继续？", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
                    {
                        return false;   //取消切换页面操作
                    }
                }

                //panel1.Controls.Clear();  //此指令会把panel1上本来的label、按钮等控件也清空
                if (panel1.Controls.Contains(myAccountPanel)) panel1.Controls.Remove(myAccountPanel);
                if (panel1.Controls.Contains(mySettingPanel)) panel1.Controls.Remove(mySettingPanel);

                if (panel1.Controls.Contains(mySetPanel)) panel1.Controls.Remove(mySetPanel);
                if (panel1.Controls.Contains(myCodePanel)) panel1.Controls.Remove(myCodePanel);
                if (panel1.Controls.Contains(mySTDPanel)) panel1.Controls.Remove(mySTDPanel);
                if (panel1.Controls.Contains(myDataCurvePanel)) panel1.Controls.Remove(myDataCurvePanel);
                if (panel1.Controls.Contains(myCalCurvePanel)) panel1.Controls.Remove(myCalCurvePanel);
                if (panel1.Controls.Contains(myCalPanel)) panel1.Controls.Remove(myCalPanel);
                if (panel1.Controls.Contains(myReportPanel)) panel1.Controls.Remove(myReportPanel);
                if (panel1.Controls.Contains(myVerifyPanel)) panel1.Controls.Remove(myVerifyPanel);
                if (panel1.Controls.Contains(myDataPanel)) panel1.Controls.Remove(myDataPanel);
                if (panel1.Controls.Contains(myPermissionPanel)) panel1.Controls.Remove(myPermissionPanel);
                if (panel1.Controls.Contains(myPDFViewPanel)) panel1.Controls.Remove(myPDFViewPanel);
                if (panel1.Controls.Contains(myTracePanel)) panel1.Controls.Remove(myTracePanel);
                panel3.Visible = false;

                return true;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("清除所有Panel失败：" + ex.ToString());
                return false;
            }            
        }

        #endregion

        #region 跨窗体控件调用

        public void switchToPanelTab(Byte panelIdx)
        {
            switch (panelIdx)
            {
                case 1:              //显示数据曲线界面
                    myDataCurvePanel.Name = "数据曲线";         //必须写在加载前，加载时才能进行名称判断
                    MyDefine.myXET.AddTraceInfo("生成曲线");
                    MyDefine.myXET.meInterface = "数据曲线";
                    panel1.Controls.Add(myDataCurvePanel);
                    myDataCurvePanel.BringToFront();
                    myDataCurvePanel.AddMyUpdateEvent();
                    //myDataCurvePanel.Dock = DockStyle.Fill;
                    panel3.Visible = true;
                    break;

                case 2:              //显示标准器录入界面
                    mySTDPanel.Name = "标准器录入";         //必须写在加载前，加载时才能进行名称判断
                    MyDefine.myXET.AddTraceInfo("标准器录入");
                    MyDefine.myXET.meInterface = "标准器录入";
                    panel1.Controls.Add(mySTDPanel);
                    mySTDPanel.BringToFront();
                    //mySTDPanel.Dock = DockStyle.Fill;
                    panel3.Visible = true;
                    break;

                case 3:              //显示校准曲线界面
                    myCalCurvePanel.Name = "校准曲线图";         //必须写在加载前，加载时才能进行名称判断
                    MyDefine.myXET.AddTraceInfo("生成曲线");
                    MyDefine.myXET.meInterface = "校准曲线图";
                    panel1.Controls.Add(myCalCurvePanel);
                    myCalCurvePanel.BringToFront();
                    myCalCurvePanel.AddMyUpdateEvent();
                    //myCalCurvePanel.Dock = DockStyle.Fill;
                    panel3.Visible = true;
                    break;

                case 4:
                    myReportPanel.Name = "校准报告";         //必须写在加载前，加载时才能进行名称判断
                    MyDefine.myXET.AddTraceInfo("生成报告");
                    MyDefine.myXET.meInterface = "校准报告";
                    panel1.Controls.Add(myReportPanel);
                    myReportPanel.BringToFront();
                    myReportPanel.AddMyUpdateEvent();      //更新界面
                    panel3.Visible = true;
                    //myReportPanel.Dock = DockStyle.Fill;
                    break;

                case 5:              //显示验证曲线界面
                    myDataCurvePanel.Name = "验证曲线";         //必须写在加载前，加载时才能进行名称判断
                    MyDefine.myXET.AddTraceInfo("生成曲线");
                    MyDefine.myXET.meInterface = "验证曲线";
                    panel1.Controls.Add(myDataCurvePanel);
                    myDataCurvePanel.BringToFront();
                    myDataCurvePanel.AddMyUpdateEvent();
                    //myDataCurvePanel.Dock = DockStyle.Fill;
                    panel3.Visible = true;
                    break;

                case 6:              //显示验证报告界面
                    myReportPanel.Name = "验证报告";         //必须写在加载前，加载时才能进行名称判断
                    MyDefine.myXET.AddTraceInfo("生成报告");
                    MyDefine.myXET.meInterface = "验证报告";
                    panel1.Controls.Add(myReportPanel);
                    myReportPanel.BringToFront();
                    myReportPanel.AddMyUpdateEvent();      //更新界面
                    panel3.Visible = true;
                    //myReportPanel.Dock = DockStyle.Fill;
                    break;

                case 7:              //显示用户管理界面
                    myAccountPanel.Name = "用户管理";         //必须写在加载前，加载时才能进行名称判断
                    MyDefine.myXET.myUpdate += new freshHandler(login_Success);
                    MyDefine.myXET.AddTraceInfo("用户管理");
                    MyDefine.myXET.meInterface = "用户管理";
                    panel1.Controls.Add(myAccountPanel);
                    myAccountPanel.BringToFront();
                    myAccountPanel.AddMyUpdateEvent();
                    //myAccountPanel.Dock = DockStyle.Fill;
                    panel3.Visible = true;
                    break;

                case 8:              //切换账号
                    this.Hide();
                    myLoginForm.Text = "切换账号";
                    MyDefine.myXET.AddTraceInfo("切换账号");
                    MyDefine.myXET.meInterface = "切换账号";

                    if (myLoginForm.ShowDialog() == DialogResult.OK)  //显示MenuAccountForm登陆界面
                    {
                        //核对当前登录账户的权限
                        myAccountPanel.checkPermission();

                        //保存当前用户名
                        MyDefine.myXET.userName = MyDefine.myXET.meLoginUser.Split(';')[(int)ACCOUNT.USER];
                        MyDefine.myXET.userPassword = MyDefine.myXET.meLoginUser.Split(';')[(int)ACCOUNT.PSWD];

                        //显示用户名
                        panel4.Visible = true;                                   //显示左侧按钮panel
                        label7.Text = "当前登录用户：" + MyDefine.myXET.userName;      //显示当前登陆用户名
                    }
                    this.Show();
                    break;

                case 9:              //显示权限管理
                    myPermissionPanel.Name = "权限管理";         //必须写在加载前，加载时才能进行名称判断
                    MyDefine.myXET.AddTraceInfo("权限管理");
                    MyDefine.myXET.meInterface = "权限管理";
                    panel1.Controls.Add(myPermissionPanel);
                    myPermissionPanel.BringToFront();
                    myPermissionPanel.AddMyUpdateEvent();
                    //myPermissionPanel.Dock = DockStyle.Fill;
                    panel3.Visible = true;
                    break;

                case 10:             //显示审计追踪界面
                    myTracePanel.Name = "审计追踪";         //必须写在加载前，加载时才能进行名称判断
                    MyDefine.myXET.AddTraceInfo("审计追踪");
                    MyDefine.myXET.meInterface = "审计追踪";
                    panel1.Controls.Add(myTracePanel);
                    myTracePanel.BringToFront();
                    myTracePanel.AddMyUpdateEvent();      //更新界面
                    panel3.Visible = true;
                    //myTracePanel.Dock = DockStyle.Fill;
                    break;

                case 11:             //显示标定界面
                    myCalPanel.Name = "标定";         //必须写在加载前，加载时才能进行名称判断
                    MyDefine.myXET.AddTraceInfo("出厂标定");
                    MyDefine.myXET.meInterface = "出厂标定";
                    panel1.Controls.Add(myCalPanel);
                    myCalPanel.BringToFront();
                    myCalPanel.AddMyUpdateEvent();      //没有此语句，标定时常常不触发myUpdate()函数
                    panel3.Visible = true;
                    //myCalPanel.Dock = DockStyle.Fill;
                    //myCalPanel.ShowCalibrationFunction();
                    break;

                case 12:             //显示标定曲线界面
                    myCalCurvePanel.Name = "标定曲线图";         //必须写在加载前，加载时才能进行名称判断
                    MyDefine.myXET.AddTraceInfo("生成曲线");
                    MyDefine.myXET.meInterface = "标定曲线图";
                    panel1.Controls.Add(myCalCurvePanel);
                    myCalCurvePanel.BringToFront();
                    myCalCurvePanel.AddMyUpdateEvent();
                    //myCalCurvePanel.Dock = DockStyle.Fill;
                    panel3.Visible = true;
                    break;

                case 13:             //
                    break;

                case 14:             //
                    break;

                case 15:             //
                    break;

                case 22:
                    myDataCurvePanel.ResetValidList();
                    myDataPanel.ShowSelectedTable();        //重新显示当前数据表，从而复位各行背景色
                    myDataPanel.HighLightValidRows();       //高亮显示有效开始、结束行
                    myDataPanel.ShowValidRowText();         //在第一列显示"有效数据"文字
                    break;

                case 23:             //数据曲线界面更新有效开始、结束时间后，同步更新数据处理界面有效开始、结束行的高亮状态
                    myDataPanel.ShowSelectedTable();        //重新显示当前数据表，从而复位各行背景色
                    myDataPanel.HighLightValidRows();       //高亮显示有效开始、结束行
                    myDataPanel.ShowValidRowText();         //在第一列显示"有效数据"文字
                    break;

                case 24:             //在选择界面单击按钮后，隐藏选择界面
                    panel6.Controls.Clear();
                    panel6.Visible = false;
                    break;

                case 25:            //更新权限类别后，更新用户管理界面中的权限类别下拉列表
                    myAccountPanel.loadPermCatList();       //加载权限类别列表并更新其下拉列表
                    //MyDefine.myXET.UpdateLoginAccount();    //更新用户名对应权限列表
                    //myAccountPanel.checkPermission();       //核对当前登录账户的权限
                    break;

                default:
                    break;
            }
        }

        #region 登录处理

        //登录成功 -- 显示用户名和返回按钮
        private void login_Success()
        {
            //MessageBox.Show("2");
            label7.Text = "当前登录用户：" + MyDefine.myXET.userName;
            panel4.Visible = true;
        }

        #endregion

        #endregion

        #region 复位左侧按钮图片为未选中状态

        /// <summary>
        /// 复位左侧按钮图片为未选中状态
        /// </summary>
        public void ResetLeftButtons()
        {
            btnDevice.BackgroundImage = global::HTR.Properties.Resources.连接界面_2;
            btnData.BackgroundImage = global::HTR.Properties.Resources.数据处理_2;
            btnCalibration.BackgroundImage = global::HTR.Properties.Resources.校准界面_2;
            btnVerify.BackgroundImage = global::HTR.Properties.Resources.验证界面_2;
            btnPDFReport.BackgroundImage = global::HTR.Properties.Resources.报告查看_2;
            btnSetting.BackgroundImage = global::HTR.Properties.Resources.系统设置_2;
        }

        #endregion

        #endregion

        #region 串口自动连接

        //自动连接，遍历端口，连接所有设备
        public Boolean AutoConnection()
        {
            try
            {
                int num = 0;    //记录已连接设备数
                Boolean ret = false;
                Boolean result = false;
                string[] myComPorts = SerialPort.GetPortNames();
                if (myComPorts.Length == 0) return false;

                int totalNum = Convert.ToInt32(comboBox1.SelectedItem.ToString());      //待连接产品总数
                MyDefine.myXET.meDUTAddrArr.Clear();           //清空设备地址列表
                MyDefine.myXET.meDUTJsnArr.Clear();            //清空设备出厂编号列表
                MyDefine.myXET.meTips = "[DEVICE AUTO CONNECTION]" + Environment.NewLine;
                MyDefine.myXET.SaveLogTips(true);  //===========
                RefreshCOMList();                               //刷新端口列表

                //for (int i = 0; i < myComPorts.Length; i++)
                for (int i = myComPorts.Length - 1; i >= 0; i--)
                {
                    if (num >= totalNum) break;             //连接所有待连接产品后退出连接循环
                    if (flag_Connecting == false) break;    //人为取消连接

                    MyDefine.myXET.meTips += "Select COMPort: " + myComPorts[i];//=========
                    MyDefine.myXET.SaveLogTips();  //===========
                    if (MyDefine.myXET.mePort.IsOpen) MyDefine.myXET.mePort.Close();
                    MyDefine.myXET.setSerialPort(myComPorts[i],comboBox3.Text);
                    if (MyDefine.myXET.PortOpen() == false) continue;   //端口被占用
                    comboBox2.SelectedIndex = i;            //显示当前端口
                    MyDefine.myXET.meTips += myComPorts[i] + " available.";//=========
                    MyDefine.myXET.SaveLogTips();  //===========
                    Application.DoEvents();

                    for (Byte address = 0x01; address <= 0xFF; address++)
                    {
                        //if (num >= 10) break;                 //连接10个产品后退出连接循环
                        if (num >= totalNum) break;             //连接所有待连接产品后退出连接循环
                        if (flag_Connecting == false) break;    //人为取消连接

                        MyDefine.myXET.meTips += "CheckAddress: " + address.ToString("X2");//=========
                        MyDefine.myXET.SaveLogTips();  //===========
                        ret = MyDefine.myXET.CheckDevice(address);

                        MyDefine.myXET.meTips += "CheckAddressResult: " + ret.ToString();//=========
                        MyDefine.myXET.SaveLogTips();  //===========
                        if (ret)    //读取数据成功且CRC校验成功
                        {
                            result = true;
                            //myLabelArray[num].TabIndex = address;             //添加已连接产品的地址
                            MyDefine.myXET.meDUTAddrArr.Add(address);          //添加已连接产品的地址
                            myPictureArray[num].Cursor = System.Windows.Forms.Cursors.Hand;

                            MyDefine.myXET.meTips += "readJSN: ";//=========
                            MyDefine.myXET.SaveLogTips();  //===========
                            MyDefine.myXET.meActiveAddr = address;
                            ret = MyDefine.myXET.readJSN();     //读取设备编号
                            MyDefine.myXET.meTips += "readJSNResult: " + ret.ToString();//=========
                            MyDefine.myXET.SaveLogTips();  //===========
                            if (ret)
                            {
                                //myLabelArray[num].MinimumSize = MinimumSize = new System.Drawing.Size(myPictureArray[num].Width, myLabelArray[num].Size.Height);    //为使编号文字居中显示，强制改变label宽度
                                //myLabelArray[num].Text = "TT" + MyDefine.myXET.meJSN.Trim();      //温度(TT)设备编号
                                myLabelArray[num].Text = MyDefine.myXET.meJSN.Trim().Length > 2 ? MyDefine.myXET.meJSN.Trim().Substring(2) : MyDefine.myXET.meJSN.Trim();      //温度(TT)设备编号
                                if (myLabelArray[num].Text == "") myLabelArray[num].Text = "blank";
                                myLabelArray[num].BackColor = Color.SteelBlue;
                                MyDefine.myXET.meDUTJsnArr.Add(myLabelArray[num].Text);      //添加已连接产品的出厂编号
                            }
                            else
                            {
                                myLabelArray[num].Text = "blank";
                                myLabelArray[num].BackColor = Color.SteelBlue;
                                MyDefine.myXET.meDUTJsnArr.Add(myLabelArray[num].Text);      //添加已连接产品的出厂编号
                            }

                            switch (MyDefine.myXET.meType)
                            {
                                case DEVICE.HTT:
                                    myPictureArray[num].BackgroundImage = global::HTR.Properties.Resources.探头;
                                    break;
                                case DEVICE.HTH:
                                    //myPictureArray[num].BackgroundImage = global::HTR.Properties.Resources._4_界面_中间_湿度模型_blue;
                                    break;
                                case DEVICE.HTP:
                                    //myPictureArray[num].BackgroundImage = global::HTR.Properties.Resources._4_界面_中间_压力模型_blue;
                                    break;
                                case DEVICE.HTQ:
                                    myPictureArray[num].BackgroundImage = global::HTR.Properties.Resources.HTQ模型_open;
                                    break;
                                default:
                                    break;
                            }

                            num++;
                        }

                        //btnConnect.Text += "●";
                        //if (btnConnect.Text == "连接中●●●●") btnConnect.Text = "连接中";
                        btnConnect.Text += ".";
                        if (btnConnect.Text == " 连接中......") btnConnect.Text = " 连接中";
                        Application.DoEvents();
                        if (address == 0xFF) break;
                    }
                }

                if (num > 0)                    //已连接产品数>0,默认激活第一个产品
                {
                    MyDefine.myXET.meActiveIdx = 0;                                    //保存当前激活产品的索引值
                    MyDefine.myXET.meActiveAddr = MyDefine.myXET.meDUTAddrArr[0];      //保存当前激活产品的地址
                    MyDefine.myXET.meActiveJsn = MyDefine.myXET.meDUTJsnArr[0];        //保存当前激活产品的出厂编号

                    if (num < totalNum)        //实际连接产品数量<客户要连接的产品数量，连接成功，仅给出数量不符提示
                    {
                        MessageBox.Show("实连设备数量：" + num.ToString() + Environment.NewLine + "待连设备数量：" + totalNum.ToString() + Environment.NewLine + "连接失败：实连设备数量 < 待连设备数量", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }

                //textBox1.AppendText(MyDefine.myXET.meTips);
                MyDefine.myXET.AddTraceInfo("设备连接数量：" + num.ToString());
                foreach (string str in MyDefine.myXET.meDUTJsnArr)
                {
                    MyDefine.myXET.AddTraceInfo("连接设备的出厂编号：" + str);
                }
                MyDefine.myXET.SaveCommunicationTips();  //将调试信息保存到操作日志
                return result;  
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("串口自动连接失败：" + ex.ToString());
                return false;
            }            
        }

        //手动连接，指定COM口
        public Boolean ManualConnection()
        {
            try
            {
                int num = 0;    //记录已连接设备数
                Boolean ret = false;
                Boolean result = false;
                string myComport = comboBox2.Text;
                if (myComport == "") return false;

                byte[] recData = new byte[20];
                int totalNum = Convert.ToInt32(comboBox1.SelectedItem.ToString());      //待连接产品总数
                MyDefine.myXET.meDUTAddrArr.Clear();           //清空设备地址列表
                MyDefine.myXET.meDUTJsnArr.Clear();            //清空设备出厂编号列表
                MyDefine.myXET.meTips = "[DEVICE MANUAL CONNECTION]" + Environment.NewLine;

                if (MyDefine.myXET.mePort.IsOpen) MyDefine.myXET.mePort.Close();
                MyDefine.myXET.setSerialPort(myComport,comboBox3.Text);
                if (MyDefine.myXET.PortOpen() == false)    //端口被占用
                {
                    MessageBox.Show("端口被占用!", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }

                for (Byte address = 0x01; address <= 0xFF; address++)
                {
                    if (num >= totalNum) break;             //连接所有待连接产品后退出连接循环
                    if (flag_Connecting == false) break;    //人为取消连接
                    ret = MyDefine.myXET.CheckDevice(address);
                    if (ret)    //读取数据成功且CRC校验成功
                    {
                        result = true;
                        //myLabelArray[num].TabIndex = address;             //添加已连接产品的地址
                        MyDefine.myXET.meDUTAddrArr.Add(address);          //添加已连接产品的地址
                        myPictureArray[num].Cursor = System.Windows.Forms.Cursors.Hand;

                        MyDefine.myXET.meActiveAddr = address;
                        ret = MyDefine.myXET.readJSN();                     //读取设备编号
                        if (ret)
                        {
                            //myLabelArray[num].MinimumSize = MinimumSize = new System.Drawing.Size(myPictureArray[num].Width, myLabelArray[num].Size.Height);    //为使编号文字居中显示，强制改变label宽度
                            //myLabelArray[num].Text = "TT" + MyDefine.myXET.meJSN.Trim();      //温度(TT)设备编号
                            myLabelArray[num].Text = MyDefine.myXET.meJSN.Trim().Length > 2 ? MyDefine.myXET.meJSN.Trim().Substring(2) : MyDefine.myXET.meJSN.Trim();      //温度(TT)设备编号
                            if (myLabelArray[num].Text == "") myLabelArray[num].Text = "blank";
                            myLabelArray[num].BackColor = Color.SteelBlue;
                            MyDefine.myXET.meDUTJsnArr.Add(myLabelArray[num].Text);      //添加已连接产品的出厂编号
                        }
                        else
                        {
                            myLabelArray[num].Text = "blank";
                            myLabelArray[num].BackColor = Color.SteelBlue;
                            MyDefine.myXET.meDUTJsnArr.Add(myLabelArray[num].Text);      //添加已连接产品的出厂编号
                        }

                        switch (MyDefine.myXET.meType)
                        {
                            case DEVICE.HTT:
                                myPictureArray[num].BackgroundImage = global::HTR.Properties.Resources.探头;
                                break;
                            case DEVICE.HTH:
                                //myPictureArray[num].BackgroundImage = global::HTR.Properties.Resources._4_界面_中间_湿度模型_blue;
                                break;
                            case DEVICE.HTP:
                                //myPictureArray[num].BackgroundImage = global::HTR.Properties.Resources._4_界面_中间_压力模型_blue;
                                break;
                            case DEVICE.HTQ:
                                myPictureArray[num].BackgroundImage = global::HTR.Properties.Resources.HTQ模型_open;
                                break;
                            default:
                                break;
                        }

                        num++;
                    }

                    //btnConnect.Text += "●";
                    //if (btnConnect.Text == "连接中●●●●") btnConnect.Text = "连接中";
                    btnConnect.Text += ".";
                    if (btnConnect.Text == " 连接中......") btnConnect.Text = " 连接中";
                    Application.DoEvents();
                    if (address == 0xFF) break;
                }

                if (num > 0)                    //已连接产品数>0,默认激活第一个产品
                {
                    MyDefine.myXET.meActiveIdx = 0;                                    //保存当前激活产品的索引值
                    MyDefine.myXET.meActiveAddr = MyDefine.myXET.meDUTAddrArr[0];      //保存当前激活产品的地址
                    MyDefine.myXET.meActiveJsn = MyDefine.myXET.meDUTJsnArr[0];        //保存当前激活产品的出厂编号

                    if (num < totalNum)        //实际连接产品数量<客户要连接的产品数量，连接成功，仅给出数量不符提示
                    {
                        MessageBox.Show("实连设备数量：" + num.ToString() + Environment.NewLine + "待连设备数量：" + totalNum.ToString() + Environment.NewLine + "连接失败：实连设备数量 < 待连设备数量", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }

                //textBox1.AppendText(MyDefine.myXET.meTips);
                MyDefine.myXET.AddTraceInfo("设备连接数量：" + num.ToString());
                foreach(string str in MyDefine.myXET.meDUTJsnArr)
                {
                    MyDefine.myXET.AddTraceInfo("连接设备的出厂编号：" + str);
                }
                MyDefine.myXET.SaveCommunicationTips();  //将调试信息保存到操作日志
                return result;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("串口手动连接失败：" + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 更新端口列表
        /// </summary>
        public void RefreshCOMList()
        {
            comboBox2.Items.Clear();
            string[] myComPorts = SerialPort.GetPortNames();

            MyDefine.myXET.meTips += "RefreshCOMList:" + Environment.NewLine;        //=========
            for (int i = 0; i < myComPorts.Length; i++)
            {
                comboBox2.Items.Add(myComPorts[i]);
                MyDefine.myXET.meTips += myComPorts[i] + Environment.NewLine;        //=========
            }
        }

        /// <summary>
        /// 切换手动/自动连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chbManual_CheckedChanged(object sender, EventArgs e)
        {
            if (chbManual.Checked)
            {
                RefreshCOMList();
                comboBox2.Enabled = true;
                if (comboBox2.Items.Count > 0) comboBox2.SelectedIndex = 0;
            }
            else
            {
                comboBox2.Enabled = false;
                comboBox2.SelectedIndex = -1;
            }
        }

        #endregion

        #region 注册软件

        private void InitializationRegister()
        {
            //验证MAC地址
            Int64 net_Mac = 0;
            Int64 net_Var = 0;
            //验证regedit
            Int64 reg_Mac = 0;
            Int64 reg_Var = 0;
            //验证C盘文件
            Int64 sys_Mac = 0;
            Int64 sys_Var = 0;
            Int32 sys_num = 0;
            //验证本地文件
            Int64 use_Mac = 0;
            Int64 use_Var = 0;
            Int32 use_num = 0;

            //验证MAC地址
            string macAddress = "";
            Process myProcess = null;
            StreamReader reader = null;
            try
            {
                ProcessStartInfo start = new ProcessStartInfo("cmd.exe");

                start.FileName = "ipconfig";
                start.Arguments = "/all";
                start.CreateNoWindow = true;
                start.RedirectStandardOutput = true;
                start.RedirectStandardInput = true;
                start.UseShellExecute = false;
                myProcess = Process.Start(start);
                reader = myProcess.StandardOutput;
                string line = reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    if (line.ToLower().IndexOf("physical address") > 0 || line.ToLower().IndexOf("物理地址") > 0)
                    {
                        int index = line.IndexOf(":");
                        index += 2;
                        macAddress = line.Substring(index);
                        macAddress = macAddress.Replace('-', ':');
                        break;
                    }
                    line = reader.ReadLine();
                }
            }
            catch
            {

            }
            finally
            {
                if (myProcess != null)
                {
                    reader.ReadToEnd();
                    myProcess.WaitForExit();
                    myProcess.Close();
                }
                if (reader != null)
                {
                    reader.Close();
                }
            }

            if (macAddress.Length == 17)
            {
                macAddress = macAddress.Replace(":", "");
                net_Mac = Convert.ToInt64(macAddress, 16);
                net_Var = net_Mac;
                while ((net_Var % 2) == 0)
                {
                    net_Var = net_Var / 2;
                }
                while ((net_Var % 3) == 0)
                {
                    net_Var = net_Var / 3;
                }
                while ((net_Var % 5) == 0)
                {
                    net_Var = net_Var / 5;
                }
                while ((net_Var % 7) == 0)
                {
                    net_Var = net_Var / 7;
                }
            }

            //验证regedit
            RegistryKey myKey = Registry.LocalMachine.OpenSubKey("software");
            string[] names = myKey.GetSubKeyNames();
            foreach (string keyName in names)
            {
                if (keyName == "WinES")
                {
                    myKey = Registry.LocalMachine.OpenSubKey("software\\WinES");
                    reg_Mac = Convert.ToInt64(myKey.GetValue("input").ToString());
                    reg_Var = Convert.ToInt64(myKey.GetValue("ouput").ToString());
                }
            }
            myKey.Close();

            //验证C盘文件
            if (!File.Exists("C:\\Windows\\user.cfg"))
            {
                if (File.Exists(MyDefine.myXET.userCFG + @"\user.num"))
                {
                    File.Copy((MyDefine.myXET.userCFG + @"\user.num"), ("C:\\Windows\\user.cfg"), true);
                }
            }
            if (File.Exists("C:\\Windows\\user.cfg"))
            {
                //读取用户信息
                FileStream meFS = new FileStream("C:\\Windows\\user.cfg", FileMode.Open, FileAccess.Read);
                BinaryReader meRead = new BinaryReader(meFS);
                if (meFS.Length > 0)
                {
                    //有内容文件
                    sys_Mac = meRead.ReadInt64();
                    sys_Var = meRead.ReadInt64();
                    sys_num = meRead.ReadInt32();
                }
                meRead.Close();
                meFS.Close();
            }


            //验证本地文件
            if (!File.Exists(MyDefine.myXET.userCFG + @"\user.num"))
            {
                if (File.Exists("C:\\Windows\\user.cfg"))
                {
                    File.Copy(("C:\\Windows\\user.cfg"), (MyDefine.myXET.userCFG + @"\user.num"), true);
                }
            }
            if (File.Exists(MyDefine.myXET.userCFG + @"\user.num"))
            {
                //读取用户信息
                FileStream meFS = new FileStream((MyDefine.myXET.userCFG + @"\user.num"), FileMode.Open, FileAccess.Read);
                BinaryReader meRead = new BinaryReader(meFS);
                if (meFS.Length > 0)
                {
                    //有内容文件
                    use_Mac = meRead.ReadInt64();
                    use_Var = meRead.ReadInt64();
                    use_num = meRead.ReadInt32();
                }
                meRead.Close();
                meFS.Close();
            }

            //注册分析
            if ((net_Mac == reg_Mac) && (net_Var == reg_Var) && (sys_Mac == use_Mac) && (sys_Var == use_Var) && (net_Mac == use_Mac) && (net_Var == use_Var))
            {
                MyDefine.myXET.myPC = 1;
                MyDefine.myXET.myMac = sys_Mac;
                MyDefine.myXET.myVar = sys_Var;
            }
            else
            {
                this.Text += " - no signed";
                MyDefine.myXET.myPC = 1;  //20200629加调试功能给杨磊
                MyDefine.myXET.myMac = sys_Mac;  //20200629加调试功能给杨磊
                MyDefine.myXET.myVar = sys_Var;  //20200629加调试功能给杨磊
            }
        }

        #endregion

        #region 控件大小随窗体变化

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
                //this.Width = (int)(this.Width / newX);
                if (newX < 0 || newY < 0.9  ) return;                      //界面尺寸太小时不调整控件，如最小化(防止界面卡顿)
                setControls(newX, newY, this);
                //MessageBox.Show(newX.ToString() + "," + newY.ToString());
                //MyDefine.myXET.AddTraceInfo("窗口尺寸变化：" + this.Width.ToString() + "," + this.Height.ToString());


                if (this.panel1.Size.Width != 0 && this.panel1.Size.Height != 0)
                {
                    //触发子窗体OnResize()函数
                    if (mySetPanel != null) mySetPanel.Size = this.panel1.Size;
                    if (myCodePanel != null) myCodePanel.Size = this.panel1.Size;
                    if (myDataPanel != null) myDataPanel.Size = this.panel1.Size;
                    if (myPDFViewPanel != null) myPDFViewPanel.Size = this.panel1.Size;
                    if (myTracePanel != null) myTracePanel.Size = this.panel1.Size;
                    if (myDataCurvePanel != null) myDataCurvePanel.Size = this.panel1.Size;
                    if (myCalCurvePanel != null) myCalCurvePanel.Size = this.panel1.Size;
                    if (myCalPanel != null) myCalPanel.Size = this.panel1.Size;
                    if (myVerifyPanel != null) myVerifyPanel.Size = this.panel1.Size;
                    if (myReportPanel != null) myReportPanel.Size = this.panel1.Size;
                    if (mySettingPanel != null) mySettingPanel.Size = this.panel1.Size;
                    if (mySelectPanel != null) mySelectPanel.Size = this.panel1.Size;
                    if (myAccountPanel != null) myAccountPanel.Size = this.panel1.Size;
                    if (myPermissionPanel != null) myPermissionPanel.Size = this.panel1.Size;
                    if (mySTDPanel != null) mySTDPanel.Size = this.panel1.Size;

                    if (myDataCurvePanel != null) myDataCurvePanel.updateCurvelDrawing();   //在读取界面中绘制背景
                    if (myCalCurvePanel != null) myCalCurvePanel.updateCurvelDrawing();   //在读取界面中绘制背景
                }
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
                    Application.DoEvents();
                    if (con is UserControl) continue;
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

                    if (con is Panel == false)            //容器控件Panel不改变字体--Panel字体变后，若panel调用了UserControl控件，则UserControl及其上的控件的尺寸会出现不可控变化
                    {
                        Single currentSize = Convert.ToSingle(mytag[4]) * newY;//根据窗体缩放比例确定控件的值，字体大小
                        con.Font = new Font(con.Font.Name, currentSize, con.Font.Style, con.Font.Unit);//字体大小
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
                MyDefine.myXET.ShowWrongMsg("串口自动连接失败：" + ex.ToString());
            }
        }

        /// <summary>
        /// 窗口尺寸变化时，按比例调整pictureBox的尺寸
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            foreach(PictureBox pictureBox in myPictureArray)
            {
                pictureBox.Width = (int)(60 * pictureBox.Height / 110);
            }
        }

        #endregion

        #region 子窗体初始化(子窗体大小随主窗体变化)

        /// <summary>
        /// 记录子窗体各控件尺寸
        /// </summary>
        private void InitMidForm()
        {
            //记录子窗体界面中各控件的初始尺寸
            mySetPanel.InitForm();
            mySTDPanel.InitForm();
            myCodePanel.InitForm();
            myDataPanel.InitForm();
            myTracePanel.InitForm();
            myCalPanel.InitForm();
            myCalCurvePanel.InitForm();
            myDataCurvePanel.InitForm();
            myVerifyPanel.InitForm();
            myCalCurvePanel.InitCurvePanel();          //提前创建界面上的控件数组，方便数据加载后直接绘图
            myDataCurvePanel.InitCurvePanel();          //提前创建界面上的控件数组，方便数据加载后直接绘图
            myCalPanel.InitDataGridView();
            myDataPanel.InitDataGridView();
            myTracePanel.InitDataGridView();
            myVerifyPanel.InitDataGridView();
            myReportPanel.InitForm();
            mySelectPanel.InitForm();
            mySettingPanel.InitForm();
            myAccountPanel.InitForm();
            myPermissionPanel.InitForm();
            myPDFViewPanel.InitForm();
            myPDFViewPanel.InitDataGridView();

            //记录子窗体的初始尺寸
            mySetPanel.Tag = mySetPanel.Width + ":" + mySetPanel.Height + ":" + mySetPanel.Left + ":" + mySetPanel.Top + ":" + mySetPanel.Font.Size;
            myCalPanel.Tag = myCalPanel.Width + ":" + myCalPanel.Height + ":" + myCalPanel.Left + ":" + myCalPanel.Top + ":" + myCalPanel.Font.Size;
            mySTDPanel.Tag = mySTDPanel.Width + ":" + mySTDPanel.Height + ":" + mySTDPanel.Left + ":" + mySTDPanel.Top + ":" + mySTDPanel.Font.Size;
            myCodePanel.Tag = myCodePanel.Width + ":" + myCodePanel.Height + ":" + myCodePanel.Left + ":" + myCodePanel.Top + ":" + myCodePanel.Font.Size;
            myDataPanel.Tag = myDataPanel.Width + ":" + myDataPanel.Height + ":" + myDataPanel.Left + ":" + myDataPanel.Top + ":" + myDataPanel.Font.Size;
            myTracePanel.Tag = myTracePanel.Width + ":" + myTracePanel.Height + ":" + myTracePanel.Left + ":" + myTracePanel.Top + ":" + myTracePanel.Font.Size;
            myCalCurvePanel.Tag = myCalCurvePanel.Width + ":" + myCalCurvePanel.Height + ":" + myCalCurvePanel.Left + ":" + myCalCurvePanel.Top + ":" + myCalCurvePanel.Font.Size;
            myDataCurvePanel.Tag = myDataCurvePanel.Width + ":" + myDataCurvePanel.Height + ":" + myDataCurvePanel.Left + ":" + myDataCurvePanel.Top + ":" + myDataCurvePanel.Font.Size;
            myPermissionPanel.Tag = myPermissionPanel.Width + ":" + myPermissionPanel.Height + ":" + myPermissionPanel.Left + ":" + myPermissionPanel.Top + ":" + myPermissionPanel.Font.Size;
            myVerifyPanel.Tag = myVerifyPanel.Width + ":" + myVerifyPanel.Height + ":" + myVerifyPanel.Left + ":" + myVerifyPanel.Top + ":" + myVerifyPanel.Font.Size;
            myReportPanel.Tag = myReportPanel.Width + ":" + myReportPanel.Height + ":" + myReportPanel.Left + ":" + myReportPanel.Top + ":" + myReportPanel.Font.Size;
            mySelectPanel.Tag = mySelectPanel.Width + ":" + mySelectPanel.Height + ":" + mySelectPanel.Left + ":" + mySelectPanel.Top + ":" + mySelectPanel.Font.Size;
            mySettingPanel.Tag = mySettingPanel.Width + ":" + mySettingPanel.Height + ":" + mySettingPanel.Left + ":" + mySettingPanel.Top + ":" + mySettingPanel.Font.Size;
            myAccountPanel.Tag = myAccountPanel.Width + ":" + myAccountPanel.Height + ":" + myAccountPanel.Left + ":" + myAccountPanel.Top + ":" + myAccountPanel.Font.Size;
            myPDFViewPanel.Tag = myPDFViewPanel.Width + ":" + myPDFViewPanel.Height + ":" + myPDFViewPanel.Left + ":" + myPDFViewPanel.Top + ":" + myPDFViewPanel.Font.Size;

            //(如果子窗体尺寸≠父容器尺寸)触发子窗体OnResize函数，调整子窗体控件尺寸
            mySetPanel.Size = this.panel1.Size;
            myCalPanel.Size = this.panel1.Size;
            mySTDPanel.Size = this.panel1.Size;
            myCodePanel.Size = this.panel1.Size;
            myDataPanel.Size = this.panel1.Size;
            myTracePanel.Size = this.panel1.Size;
            myCalCurvePanel.Size = this.panel1.Size;
            myDataCurvePanel.Size = this.panel1.Size;
            myPermissionPanel.Size = this.panel1.Size;
            myVerifyPanel.Size = this.panel1.Size;
            myReportPanel.Size = this.panel1.Size;
            mySelectPanel.Size = this.panel1.Size;
            mySettingPanel.Size = this.panel1.Size;
            myAccountPanel.Size = this.panel1.Size;
            myPDFViewPanel.Size = this.panel1.Size;
        }

        #endregion

        #endregion

        #region 未用函数

        /// <summary>
        /// 跨线程操作
        /// </summary>
        public void callDoCrossThread()
        {
            Thread thread1 = new Thread(DoCrossThread);//可以省略线程的委托类型ParameterizedThreadStart
            thread1.Start();
        }

        public void DoCrossThread()
        {
            this.Invoke(new MethodInvoker(() => {
                //UI操作代码
                panel6.Visible = false;
            }));
        }
        #endregion

        #region 控件数组

        #region 创建并初始化控件数组

        /// <summary>
        /// 创建控件数组
        /// </summary>
        private void BuildPictureArray()
        {
            //产品图片
            myPictureArray.Add(pictureBox1);
            myPictureArray.Add(pictureBox2);
            myPictureArray.Add(pictureBox3);
            myPictureArray.Add(pictureBox6);
            myPictureArray.Add(pictureBox7);
            myPictureArray.Add(pictureBox8);
            myPictureArray.Add(pictureBox9);
            myPictureArray.Add(pictureBox10);
            myPictureArray.Add(pictureBox11);
            myPictureArray.Add(pictureBox12);

            //产品编号
            myLabelArray.Add(label10);
            myLabelArray.Add(label11);
            myLabelArray.Add(label12);
            myLabelArray.Add(label13);
            myLabelArray.Add(label14);
            myLabelArray.Add(label15);
            myLabelArray.Add(label16);
            myLabelArray.Add(label17);
            myLabelArray.Add(label18);
            myLabelArray.Add(label19);

            //产品读取数量
            myReadNumArray.Add(label3);
            myReadNumArray.Add(label4);
            myReadNumArray.Add(label6);
            myReadNumArray.Add(label20);
            myReadNumArray.Add(label21);
            myReadNumArray.Add(label22);
            myReadNumArray.Add(label23);
            myReadNumArray.Add(label24);
            myReadNumArray.Add(label25);
            myReadNumArray.Add(label26);

            //产品读取时进度条显示
            myProBarArray.Add(progressBar1);
            myProBarArray.Add(progressBar2);
            myProBarArray.Add(progressBar3);
            myProBarArray.Add(progressBar4);
            myProBarArray.Add(progressBar5);
            myProBarArray.Add(progressBar6);
            myProBarArray.Add(progressBar7);
            myProBarArray.Add(progressBar8);
            myProBarArray.Add(progressBar9);
            myProBarArray.Add(progressBar10);

            //初始化控件数组的一些属性
            for (int i = 0; i < 10; i++)
            {
                myPictureArray[i].Tag += ":" + i.ToString();        //初始化myPictureArray控件数组的索引值，以后都不变了
                myPictureArray[i].BackgroundImage = global::HTR.Properties.Resources.加载模式链条;
                myPictureArray[i].Cursor = System.Windows.Forms.Cursors.Default;

                myLabelArray[i].Text = "";
                //myLabelArray[i].TabIndex = 0;
                myLabelArray[i].BackColor = Color.White;

                myReadNumArray[i].Text = "";
                myProBarArray[i].Visible = false;
                myProBarArray[i].Value = myProBarArray[i].Minimum;
            }
        }

        #endregion

        #region 复位控件数组各项属性

        //复位控件数组各项属性
        public void ResetDevicePictures()
        {
            for (int i = 0; i < 10; i++)
            {
                myPictureArray[i].BackgroundImage = global::HTR.Properties.Resources.加载模式链条;//_4_界面_中间_温度模型_gray
                myPictureArray[i].Cursor = System.Windows.Forms.Cursors.Default;
                myLabelArray[i].Text = "";
                //myLabelArray[i].TabIndex = 0;
                myLabelArray[i].BackColor = Color.White;

                myReadNumArray[i].Text = "";
                myProBarArray[i].Visible = false;
            }
        }

        #endregion

        #region 隐藏控件数组中的进度条

        //复位控件数组各项属性
        public void HideProBarArr()
        {
            for (int i = 0; i < 10; i++)
            {
                myProBarArray[i].Visible = false;
                myProBarArray[i].Value = myProBarArray[i].Minimum;
            }
        }

        #endregion

        #region 显示控件数组中已连接设备的进度条

        public void ShowProBarArr()
        {
            for (int i = 0; i < MyDefine.myXET.meDUTAddrArr.Count; i++)
            {
                myProBarArray[i].Visible = true;
                myProBarArray[i].Value = myProBarArray[i].Minimum;
            }
        }

        #endregion

        #region 复位设备读取数量

        /// <summary>
        /// 复位所有读取数量Label
        /// </summary>
        public void ResetReadNumArr()
        {
            for (int i = 0; i < 10; i++)
            {
                myReadNumArray[i].Text = "";
            }
        }

        #endregion

        #region 设置单个产品编号

        /// <summary>
        /// 设置单个产品编号(注意，此函数在MenuFactoryForm中有引用到[跨窗体引用])
        /// </summary>
        /// <param name="idx">产品索引</param>
        /// <param name="code">产品编号</param>
        public void setLabelJSN(int idx, string code)
        {
            myLabelArray[idx].Text = code;
        }

        /// <summary>
        /// 设置单个产品标签颜色(选中状态)
        /// </summary>
        /// <param name="idx">产品索引</param>
        /// <param name="code">产品编号</param>
        public void setLabelColor(int idx, Color color)
        {
            //将已连接产品的标签全部复位为蓝色
            for (int i = 0; i < myLabelArray.Count; i++)
            {
                if (myLabelArray[i].Text != "")                     
                {
                    myLabelArray[i].BackColor = Color.SteelBlue;
                }
            }

            //将被选中产品的标签设置为橘色
            myLabelArray[idx].BackColor = color;
        }

        #endregion

        #region 控件数组单击事件

        private void pictureBox_Click(object sender, EventArgs e)
        {
            try
            {
                int idx = Convert.ToInt32(((PictureBox)sender).Tag.ToString().Split(':')[5]);   //获取选中产品的索引
                if (myLabelArray[idx].Text == "") return;                   //当前单击的设备尚未连接，退出
                setLabelColor(idx, Color.DarkOrange);                       //将被选中产品设置为橘色
                if (MyDefine.myXET.meTask == TASKS.disconnected) return;    //设备未连接，或设备连接中，则退出

                MyDefine.myXET.AddTraceInfo("单击产品：" + (idx + 1).ToString() + "/" + MyDefine.myXET.meDUTAddrArr.Count.ToString());
                MyDefine.myXET.meActiveAddr = MyDefine.myXET.meDUTAddrArr[idx];       //保存当前激活产品的地址
                MyDefine.myXET.meActiveJsn = MyDefine.myXET.meDUTJsnArr[idx];         //保存当前激活产品的出厂编号
                MyDefine.myXET.meActiveIdx = idx;                                     //保存当前激活产品的索引值
                                                                                      
                btnDeviceSet_Click(null, null);                             //打开设置界面
                mySetPanel.UpdateInterfaceInfo();                           //更新设备设置界面中的部分参数
                //myProBarArray[idx].Visible = false ;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("单击产品失败：" + ex.ToString());
            }
        }

        #endregion

        #endregion

        #region 获取电脑名+MAC

        public String GetComputerInfo()
        {
            string info = Environment.UserDomainName + @"\" + Environment.UserName + @"\";

            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    info += BitConverter.ToString(adapter.GetPhysicalAddress().GetAddressBytes());
                }
            }

            return info;
        }

        #endregion

        #region 显示label信息
        private void label_MouseHover(object sender, EventArgs e)
        {
            Label label = (Label)sender;
            toolTip1.SetToolTip(label, label.Text);
        }
        #endregion

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked)
            {
                MyDefine.myXET.meScreenStatus = true;
                MyDefine.myXET.setScreenState();
            }
            else
            {
                MyDefine.myXET.meScreenStatus = false;
                MyDefine.myXET.setScreenState();
            }
        }
    }
}