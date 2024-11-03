using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.IO;

namespace HTR
{
    public partial class MainTest : Form
    {
        MenuLoginForm myAccountForm = new MenuLoginForm();
        MenuVerifyPanel myVerifyPanel = new MenuVerifyPanel();
        MenuReportPanel myReportPanel = new MenuReportPanel();
        MenuSettingPanel mySettingPanel = new MenuSettingPanel();
        MenuDataCurvePanel myReadPanel = new MenuDataCurvePanel();
        MenuCalPanel myCalPanel = new MenuCalPanel();
        MenuTracePanel myTracePanel = new MenuTracePanel();
        MenuDataCurvePanel myCurvePanel = new MenuDataCurvePanel();
        MenuDataPanel myDataPanel = new MenuDataPanel();
        MenuCalCurvePanel myCalCurvePanel = new MenuCalCurvePanel();             //数据曲线界面
        MenuPDFViewPanel myPDFViewPanel = new MenuPDFViewPanel();
        MenuPermissionPanel myPermissionPanel = new MenuPermissionPanel();
        MenuAccountPanel myAccountPanel = new MenuAccountPanel();
        MenuSTDPanel mySTDPanel = new MenuSTDPanel();

        public MainTest()
        {
            InitializeComponent();
        }

        private void MainTest_Load(object sender, EventArgs e)
        {
            //将当前主界面作为参数传递到XET.Function类，方便在其中做窗体切换
            MyDefine.myXET = new XET(this);
            MyDefine.myXET.meComputer = GetComputerInfo();  //获取电脑名

            InitForm();
            InitMidForm();

            int numSTEP = Enum.GetNames(typeof(STEP)).Length;               //获取权限总个数
            MyDefine.myXET.meLoginUser = "111;111;工厂;;;;0000;隐藏权限;" + ("AAA").PadRight(numSTEP, 'A');      //使能所有权限
            //MyDefine.myXET.meLoginUser = "debug;ella;程序开发;;隐藏权限;111111111111111111111111111111";
            MyDefine.myXET.loadReportInfo();                         //加载报告信息(报告编号+标准器列表)
            MyDefine.myXET.CheckInitAccount();          //若账号个数为0，则创建初始账号和权限类别
            loadUserList();                             //用户列表加载
            MyDefine.myXET.loadPremCatList();   //加载权限类别列表
            for(int i =0;i< MyDefine.myXET.meListPermCat.Count;i++)
            {
                textBox1.Text += MyDefine.myXET.meListPermCat[i] + '\n';
            }

        }

        #region 界面切换

        //验证按钮 -- 显示验证界面
        private void button1_Click(object sender, EventArgs e)
        {
            panel1.Controls.Add(myVerifyPanel);
            myVerifyPanel.BringToFront();
            myVerifyPanel.AddMyUpdateEvent();      //更新界面
            //myVerifyPanel.Dock = DockStyle.Fill;  //加入此指令，最大化后myVerifyPanel界面会超出边界，原因不明
        }

        private void button3_Click(object sender, EventArgs e)
        {
            myReportPanel.Name = "验证报告";
            panel1.Controls.Add(myReportPanel);
            myReportPanel.BringToFront();
            myReportPanel.AddMyUpdateEvent();      //更新界面
            //myReportPanel.Dock = DockStyle.Fill;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            panel1.Controls.Add(mySettingPanel);
            mySettingPanel.BringToFront();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            panel1.Controls.Add(myReadPanel);
            myReadPanel.BringToFront();
            myReadPanel.AddMyUpdateEvent();
        }
        #endregion

        #region 跨窗体控件调用

        public void switchToPanelTab(Byte panelIdx)
        {
            switch (panelIdx)
            {
                case 1:              //显示数据曲线界面
                    myCurvePanel.Name = "数据曲线";         //必须写在加载前，加载时才能进行名称判断
                    MyDefine.myXET.AddTraceInfo("生成曲线");
                    MyDefine.myXET.meInterface = "数据曲线";
                    panel1.Controls.Add(myCurvePanel);
                    myCurvePanel.BringToFront();
                    myCurvePanel.AddMyUpdateEvent();
                    //myCurvePanel.Dock = DockStyle.Fill;
                    break;

                case 2:              //显示标准器录入界面
                    mySTDPanel.Name = "标准器录入";         //必须写在加载前，加载时才能进行名称判断
                    MyDefine.myXET.AddTraceInfo("标准器录入");
                    MyDefine.myXET.meInterface = "标准器录入";
                    panel1.Controls.Add(mySTDPanel);
                    mySTDPanel.BringToFront();
                    //mySTDPanel.Dock = DockStyle.Fill;
                    break;

                case 3:              //显示校准曲线界面
                    myCalCurvePanel.Name = "校准曲线图";         //必须写在加载前，加载时才能进行名称判断
                    MyDefine.myXET.AddTraceInfo("生成曲线");
                    MyDefine.myXET.meInterface = "校准曲线图";
                    panel1.Controls.Add(myCalCurvePanel);
                    myCalCurvePanel.BringToFront();
                    myCalCurvePanel.AddMyUpdateEvent();
                    //myCalCurvePanel.Dock = DockStyle.Fill;
                    break;

                case 4:
                    myReportPanel.Name = "校准报告";         //必须写在加载前，加载时才能进行名称判断
                    MyDefine.myXET.AddTraceInfo("生成报告");
                    MyDefine.myXET.meInterface = "校准报告";
                    panel1.Controls.Add(myReportPanel);
                    myReportPanel.BringToFront();
                    myReportPanel.AddMyUpdateEvent();      //更新界面
                    //myReportPanel.Dock = DockStyle.Fill;
                    break;

                case 5:              //显示验证曲线界面
                    myCurvePanel.Name = "验证曲线";         //必须写在加载前，加载时才能进行名称判断
                    MyDefine.myXET.AddTraceInfo("生成曲线");
                    MyDefine.myXET.meInterface = "验证曲线";
                    panel1.Controls.Add(myCurvePanel);
                    myCurvePanel.BringToFront();
                    myCurvePanel.AddMyUpdateEvent();
                    //myCurvePanel.Dock = DockStyle.Fill;
                    break;

                case 6:              //显示验证报告界面
                    myReportPanel.Name = "验证报告";         //必须写在加载前，加载时才能进行名称判断
                    MyDefine.myXET.AddTraceInfo("生成报告");
                    MyDefine.myXET.meInterface = "验证报告";
                    panel1.Controls.Add(myReportPanel);
                    myReportPanel.BringToFront();
                    myReportPanel.AddMyUpdateEvent();      //更新界面
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
                    break;

                case 8:              //切换账号
                    this.Hide();
                    myAccountForm.Text = "切换账号";
                    MyDefine.myXET.AddTraceInfo("切换账号");
                    MyDefine.myXET.meInterface = "切换账号";

                    if (myAccountForm.ShowDialog() == DialogResult.OK)  //显示MenuAccountForm登陆界面
                    {
                        //核对当前登录账户的权限
                        myAccountPanel.checkPermission();

                        //保存当前用户名
                        MyDefine.myXET.userName = MyDefine.myXET.meLoginUser.Split(';')[(int)ACCOUNT.USER];
                        MyDefine.myXET.userPassword = MyDefine.myXET.meLoginUser.Split(';')[(int)ACCOUNT.PSWD];

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
                    break;

                case 10:             //显示审计追踪界面
                    myTracePanel.Name = "审计追踪";         //必须写在加载前，加载时才能进行名称判断
                    MyDefine.myXET.AddTraceInfo("审计追踪");
                    MyDefine.myXET.meInterface = "审计追踪";
                    panel1.Controls.Add(myTracePanel);
                    myTracePanel.BringToFront();
                    myTracePanel.AddMyUpdateEvent();      //更新界面
                    //myTracePanel.Dock = DockStyle.Fill;
                    break;

                case 11:             //显示标定界面
                    myCalPanel.Name = "标定";         //必须写在加载前，加载时才能进行名称判断
                    MyDefine.myXET.AddTraceInfo("出厂标定");
                    MyDefine.myXET.meInterface = "出厂标定";
                    panel1.Controls.Add(myCalPanel);
                    myCalPanel.BringToFront();
                    myCalPanel.AddMyUpdateEvent();      //没有此语句，标定时常常不触发myUpdate()函数
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
                    break;

                case 13:             //
                    break;

                case 14:             //
                    break;

                case 15:             //
                    break;

                case 23:             //数据曲线界面更新有效开始、结束时间后，同步更新数据处理界面有效开始、结束行的高亮状态
                    myDataPanel.ShowSelectedTable();        //重新显示当前数据表，从而复位各行背景色
                    myDataPanel.HighLightValidRows();       //高亮显示有效开始、结束行
                    myDataPanel.ShowValidRowText();         //在第一列显示"有效数据"文字
                    break;

                case 24:             //在选择界面单击按钮后，隐藏选择界面
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
        }

        #endregion

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
                setControls(newX, newY, this);

                if (this.panel1.Size.Width != 0 && this.panel1.Size.Height != 0)
                {
                    //触发子窗体OnResize()函数
                    if (myVerifyPanel != null) myVerifyPanel.Size = this.panel1.Size;
                    if (myReportPanel != null) myReportPanel.Size = this.panel1.Size;
                    if (myReadPanel != null) myReadPanel.Size = this.panel1.Size;
                    if (myPermissionPanel != null) myPermissionPanel.Size = this.panel1.Size;
                    if (myCalCurvePanel != null) myCalCurvePanel.Size = this.panel1.Size;
                    if (myTracePanel != null) myTracePanel.Size = this.panel1.Size;
                    if (myCalPanel != null) myCalPanel.Size = this.panel1.Size;
                    if (myReadPanel != null) myReadPanel.updateCurvelDrawing();   //在读取界面中绘制背景
                    if (myCalCurvePanel != null) myCalCurvePanel.updateCurvelDrawing();   //在读取界面中绘制背景
                    if (myPDFViewPanel != null) myPDFViewPanel.Size = this.panel1.Size;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
            myDataPanel.InitForm();
            myDataPanel.InitDataGridView();
            myTracePanel.InitForm();
            myTracePanel.InitDataGridView();
            myCurvePanel.InitForm();
            myCurvePanel.InitCurvePanel();          //提前创建界面上的控件数组，方便数据加载后直接绘图
            myCalCurvePanel.InitForm();
            myCalCurvePanel.InitCurvePanel();          //提前创建界面上的控件数组，方便数据加载后直接绘图
            myVerifyPanel.InitForm();
            myVerifyPanel.InitDataGridView();
            myReportPanel.InitForm();
            myPermissionPanel.InitForm();
            myReadPanel.InitForm();
            myCalPanel.InitForm();
            myCalPanel.InitDataGridView();
            myCalCurvePanel.InitForm();
            myPDFViewPanel.InitForm();
            myPDFViewPanel.InitDataGridView();


            //记录子窗体的初始尺寸
            myVerifyPanel.Tag = myVerifyPanel.Width + ":" + myVerifyPanel.Height + ":" + myVerifyPanel.Left + ":" + myVerifyPanel.Top + ":" + myVerifyPanel.Font.Size;
            myReportPanel.Tag = myReportPanel.Width + ":" + myReportPanel.Height + ":" + myReportPanel.Left + ":" + myReportPanel.Top + ":" + myReportPanel.Font.Size;
            myReadPanel.Tag = myReadPanel.Width + ":" + myReadPanel.Height + ":" + myReadPanel.Left + ":" + myReadPanel.Top + ":" + myReadPanel.Font.Size;
            myCalCurvePanel.Tag = myCalCurvePanel.Width + ":" + myCalCurvePanel.Height + ":" + myCalCurvePanel.Left + ":" + myCalCurvePanel.Top + ":" + myCalCurvePanel.Font.Size;
            myCalPanel.Tag = myCalPanel.Width + ":" + myCalPanel.Height + ":" + myCalPanel.Left + ":" + myCalPanel.Top + ":" + myCalPanel.Font.Size;
            myDataPanel.Tag = myDataPanel.Width + ":" + myDataPanel.Height + ":" + myDataPanel.Left + ":" + myDataPanel.Top + ":" + myDataPanel.Font.Size;
            myTracePanel.Tag = myTracePanel.Width + ":" + myTracePanel.Height + ":" + myTracePanel.Left + ":" + myTracePanel.Top + ":" + myTracePanel.Font.Size;
            myCurvePanel.Tag = myCurvePanel.Width + ":" + myCurvePanel.Height + ":" + myCurvePanel.Left + ":" + myCurvePanel.Top + ":" + myCurvePanel.Font.Size;
            myPermissionPanel.Tag = myPermissionPanel.Width + ":" + myPermissionPanel.Height + ":" + myPermissionPanel.Left + ":" + myPermissionPanel.Top + ":" + myPermissionPanel.Font.Size;
            myPDFViewPanel.Tag = myPDFViewPanel.Width + ":" + myPDFViewPanel.Height + ":" + myPDFViewPanel.Left + ":" + myPDFViewPanel.Top + ":" + myPDFViewPanel.Font.Size;

            //(如果子窗体尺寸≠父容器尺寸)触发子窗体OnResize函数，调整子窗体控件尺寸
            myVerifyPanel.Size = this.panel1.Size;
            myReportPanel.Size = this.panel1.Size;
            myReadPanel.Size = this.panel1.Size;
            myPermissionPanel.Size = this.panel1.Size;
            myCalCurvePanel.Size = this.panel1.Size;
            myCalPanel.Size = this.panel1.Size;
            myDataPanel.Size = this.panel1.Size;
            myTracePanel.Size = this.panel1.Size;
            myCurvePanel.Size = this.panel1.Size;
            myPDFViewPanel.Size = this.panel1.Size;

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

        #region 加载用户列表

        /// <summary>
        /// 加载用户列表
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
                    String filepath = MyDefine.myXET.userCFG + @"\" + myFile;
                    if (File.Exists(filepath))
                    {
                        String[] meLines = File.ReadAllLines(filepath);
                        if (meLines.Length != 0) myUsers.Add(meLines[0]);
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

        #region 加载权限类别

        /// <summary>
        /// 加载权限类别
        /// </summary>
        /// <returns></returns>
        public void loadPremCategory()
        {
            if (!Directory.Exists(MyDefine.myXET.userCFG))
            {
                Directory.CreateDirectory(MyDefine.myXET.userCFG);
            }

            try
            {
                //加载权限类别列表
                List<String> myPremCat = new List<string>();
                String filepath = MyDefine.myXET.userCFG + @"\category.cfg";
                if (File.Exists(filepath))
                {
                    String[] meLines = File.ReadAllLines(filepath);
                    if (meLines.Length != 0) myPremCat = new List<string>(meLines);
                }

                MyDefine.myXET.meListPermCat = new List<string>(myPremCat.ToArray());   //将列表赋给全局变量

            }
            catch (Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("权限类别加载失败：" + ex.ToString());
            }
        }

        #endregion

        private void button6_Click(object sender, EventArgs e)
        {
            MyDefine.myXET.meDebugMode = true;
            myCalPanel.Name = "校准";         //必须写在加载前，加载时才能进行名称判断
            panel1.Controls.Add(myCalPanel);
            myCalPanel.BringToFront();
            myCalPanel.AddMyUpdateEvent();      //没有此语句，标定时常常不触发myUpdate()函数
            //myCalPanel.Dock = DockStyle.Fill;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            MyDefine.myXET.meDebugMode = true;
            myCalPanel.Name = "标定";         //必须写在加载前，加载时才能进行名称判断
            panel1.Controls.Add(myCalPanel);
            myCalPanel.BringToFront();
            myCalPanel.AddMyUpdateEvent();      //没有此语句，标定时常常不触发myUpdate()函数
            //myCalPanel.Dock = DockStyle.Fill;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            panel1.Controls.Add(myDataPanel);
            myDataPanel.BringToFront();
            myDataPanel.AddMyUpdateEvent();      //没有此语句，标定时常常不触发myUpdate()函数
            //panel3.Visible = true;             //显示返回按钮
            //myDataPanel.Dock = DockStyle.Fill;
            MyDefine.myXET.AddToTraceRecords("主界面", "数据处理");
            MyDefine.myXET.meInterface = "数据处理";
            MyDefine.myXET.SaveTraceRecords();                          //保存追踪日志
        }

        private void MainTest_FormClosing(object sender, FormClosingEventArgs e)
        {
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
                MyDefine.myXET.AddToTraceRecords("退出", "退出");
                MyDefine.myXET.SaveTraceRecords();
                System.Environment.Exit(0);
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                MyDefine.myXET.AddToTraceRecords("退出", "系统失败：" + ex.ToString());
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            MyDefine.myXET.switchMainPanel(10);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            panel1.Controls.Add(myPDFViewPanel);
            myPDFViewPanel.BringToFront();
            myPDFViewPanel.AddMyUpdateEvent();
            //myPDFViewPanel.Dock = DockStyle.Fill;
            MyDefine.myXET.AddToTraceRecords("主界面", "报告查看");
            MyDefine.myXET.SaveTraceRecords();                          //保存追踪日志
            MyDefine.myXET.meInterface = "报告查看";
        }

        private void button10_Click(object sender, EventArgs e)
        {
            MyDefine.myXET.meInterface = "权限管理";
            panel1.Controls.Add(myPermissionPanel);
            myPermissionPanel.BringToFront();
            //myPermissionPanel.AddMyUpdateEvent();
            //myPermissionPanel.Dock = DockStyle.Fill;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            MyDefine.myXET.meInterface = "用户管理";
            panel1.Controls.Add(myAccountPanel);
            myAccountPanel.BringToFront();
            myAccountPanel.AddMyUpdateEvent();
            //myAccountPanel.Dock = DockStyle.Fill;
        }

        private void button14_Click(object sender, EventArgs e)
        {
            MenuSETValForm  mySETVal = new MenuSETValForm();
            mySETVal.ShowDialog();
            mySETVal.Location = new Point(this.Location.X + this.Width / 2, this.Location.Y + this.Height / 2);
        }
    }
}
