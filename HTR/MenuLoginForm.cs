using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

namespace HTR
{
    public partial class MenuLoginForm : Form
    {
        #region 界面加载

        public MenuLoginForm()
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

        //帐号登录加载
        private void MenuAccountForm_Load(object sender, EventArgs e)
        {
            label8.Text = "软件版本：" + Constants.SW_Version;       //显示软件版本
            try
            {
                //窗口元素调整
                if (this.Text == "欢迎使用！")
                {
                    label10.Text = "账号登录";
                    MyDefine.myXET.meInterface = "登录界面";
                    MyDefine.myXET.CheckInitAccount();          //若账号个数为0，则创建初始账号和权限类别
                }
                else
                {
                    label10.Text = "账号切换";
                    //button3.Visible = true;
                }

                loadUserList();                             //用户列表加载及显示
                MyDefine.myXET.loadPremCatList();           //加载权限类别列表
                textBox3.Focus();
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        //最小化
        private void label3_Click(object sender, EventArgs e)
        {
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
        }

        //关闭窗口
        private void label6_Click(object sender, EventArgs e)
        {
            //关闭退出
            if (this.Text == "欢迎使用！")
            {
                MyDefine.myXET.AddTraceInfo("退出");
                MyDefine.myXET.SaveTraceRecords();
                System.Environment.Exit(0);
            }
            else
            {
                MyDefine.myXET.AddTraceInfo("关闭");
                MyDefine.myXET.meInterface = "用户管理";
                MyDefine.myXET.SaveTraceRecords();
                this.Close();
            }
        }

        #endregion

        #region 界面按钮事件

        //登录创建保存按钮- 登录
        private void login_button1_Click()
        {
            try
            {
                MyDefine.myXET.AddTraceInfo("登录：" + textBox3.Text);

                //工厂使用超级账号密码
                if ((textBox3.Text == "JDEGREE") && (textBox1.Text == "YANG"))
                {
                    int numSTEP = Enum.GetNames(typeof(STEP)).Length;               //获取权限总个数
                    //MyDefine.myXET.meLoginUser = "JDeGree;leiG;工厂;;隐藏权限;111111111111111111111111111111";
                    MyDefine.myXET.meLoginUser = "JDEGREE;YANG;工厂;;;;0000;隐藏权限;" + ("AAA").PadRight(numSTEP, 'A');      //使能所有权限
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    MyDefine.myXET.AddTraceInfo("登录成功");
                    this.Hide();
                    
                }
                else if ((textBox3.Text == "debug") && (textBox1.Text == "ella"))
                {
                    int numSTEP = Enum.GetNames(typeof(STEP)).Length;               //获取权限总个数
                    //MyDefine.myXET.meLoginUser = "debug;ella;程序开发;;隐藏权限;111111111111111111111111111111";
                    MyDefine.myXET.meLoginUser = "debug;ella;程序开发;;;;0000;隐藏权限;" + ("AAA").PadRight(numSTEP, 'A');     //使能所有权限
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    MyDefine.myXET.AddTraceInfo("登录成功");
                    this.Hide();
                }
                else                                                                //客户登录
                {
                    int ret = MyDefine.myXET.CheckUserAccount(textBox3.Text, textBox1.Text);       //核对用户名密码是否正确

                    if(ret == 1)        //核对正确
                    {
                        String isRembPSD = checkBox1.Checked ? "1" : "0";           //是否记住密码
                        this.DialogResult = System.Windows.Forms.DialogResult.OK;
                        MyDefine.myXET.SaveAsDefaultAccount(MyDefine.myXET.meLoginUser, isRembPSD);//将当前登录账号保存为默认账号
                        MyDefine.myXET.AddTraceInfo("登录成功");
                        this.Hide();
                    }
                    else if(ret == -1)
                    {
                        warning_NI("用户名不存在！");
                        MyDefine.myXET.AddTraceInfo("用户名不存在");
                    }
                    else if(ret == -2)
                    {
                        warning_NI("密码错误！");
                        MyDefine.myXET.AddTraceInfo("密码错误");
                    }
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("登录失败：" + ex.ToString());
            }
        }

        //登录创建保存按钮- 保存
        private void save_button1_Click()
        {
            
        }

        //登录创建保存按钮
        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "登 录")
            {
                login_button1_Click();
            }
            else if (button1.Text == "保 存")
            {
                save_button1_Click();
            }

        }

        //找回密码
        private void button4_Click(object sender, EventArgs e)
        {
            /*
            if (this.Text == "欢迎使用！")
            {
                System.Environment.Exit(0);
            }
            else
            {
                isSave = false;
                isNew = false;
                this.Hide();
            }
            */

            MyDefine.myXET.AddTraceInfo("找回密码");
            MessageBox.Show("请联系管理员查询修改！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        //时间控制
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            notifyIcon1.Visible = false;
        }

        #region 信息提示

        //报警提示
        private void warning_NI(string meErr)
        {
            try
            {
                timer1.Enabled = true;
                timer1.Interval = 3000;
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(3000, notifyIcon1.Text, meErr, ToolTipIcon.Info);
                label4.Text = ">>> " + meErr;
                label4.Visible = true;
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        #endregion

        #region 账号密码规则

        //账号密码规则
        private void psw_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                //不可以有以下特殊字符
                // \/:*?"<>|
                // \\
                // \|
                // ""
                Regex meRgx = new Regex(@"[\\/:*?""<>\|]");
                if (meRgx.IsMatch(e.KeyChar.ToString()))
                {
                    warning_NI("不能使用\\/:*?\"<>|");
                    e.Handled = true;
                }
                else if (e.KeyChar == 13)    //回车登陆
                {
                    button1_Click(null, null);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MessageBox.Show(ex.ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        #endregion

        #region 加载账号文件

        /// <summary>
        /// 加载用户名列表
        /// </summary>
        /// <returns></returns>
        public void loadUserList()
        {
            try
            {
                MyDefine.myXET.loadUserList();   //加载用户名列表
                ShowUserList();                  //显示用户名列表
            }
            catch (Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("用户列表加载失败：" + ex.ToString());
            }
        }

        /// <summary>
        /// 显示账号列表
        /// </summary>
        public void ShowUserList()
        {
            textBox3.AutoCompleteCustomSource.Clear();         //清空显示
            int total = MyDefine.myXET.meListUser.Count;
            int catnum = total > 10 ? 10 : total;
            for (int i = 0; i < catnum; i++)
            {
                Application.DoEvents();                       //防止界面加载时长时间卡顿
                string username = MyDefine.myXET.meListUser[i].Split(';')[(int)ACCOUNT.USER];       //账号
                string userpswd = MyDefine.myXET.meListUser[i].Split(';')[(int)ACCOUNT.PSWD];       //密码
                string islogin = MyDefine.myXET.meListUser[i].Split(';')[(int)ACCOUNT.DEFUSR];      //是否是上次登录的账号
                string isrember = MyDefine.myXET.meListUser[i].Split(';')[(int)ACCOUNT.REMPSD];     //是否记住当前账号的密码
                textBox3.AutoCompleteCustomSource.Add(username);        //将用户名添加进下拉列表

                if(islogin == "1")                  //将上次登录过的用户名自动显示在界面
                {
                    textBox3.Text = username;       //显示用户名
                    if (isrember == "1")            //已选择记住密码，将密码自动填入密码框
                    {
                        textBox1.Text = userpswd;
                        checkBox1.Checked = true;   //勾选记住密码
                    }
                }
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

        #region 核对用户名密码

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
                for (int i = 0; i < MyDefine.myXET.meListUser.Count; i++)               //遍历账号列表
                {
                    string myUserName = MyDefine.myXET.meListUser[i].Split(';')[(int)ACCOUNT.USER];    //用户名
                    string myCateCode = MyDefine.myXET.meListUser[i].Split(';')[(int)ACCOUNT.CATCODE]; //权限类别编号
                    if (myUserName.Equals(username))                                     //此账号存在
                    {
                        string mypassword = MyDefine.myXET.meListUser[i].Split(';')[(int)ACCOUNT.PSWD];
                        if (mypassword.Equals(password))                                 //账户名密码核对正确
                        {
                            MyDefine.myXET.meLoginUser = MyDefine.myXET.meListUser[i];   //"用户名;密码;部门;电话;权类编号;"
                            MyDefine.myXET.meLoginUser += GetUserPerm(myCateCode) + ";"; //"权限类别编号对应的权限类别名称"
                            MyDefine.myXET.meLoginUser += GetUserPerm(myCateCode);       //"权限类别编号对应的权限列表"
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

        /// <summary>
        /// 更新用户名对应权限列表
        /// </summary>
        /// <param name="username">用户输入的用户名</param>
        /// <returns>1=核对正确；0=程序异常；-1=用户名不存在；-2=密码错误</returns>
        public int UpdateUserAccount(string username)
        {
            try
            {
                for (int i = 0; i < MyDefine.myXET.meListUser.Count; i++)           //遍历账号列表
                {
                    string myUserName = MyDefine.myXET.meListUser[i].Split(';')[(int)ACCOUNT.USER];    //用户名
                    string myCateCode = MyDefine.myXET.meListUser[i].Split(';')[(int)ACCOUNT.CATCODE]; //权限类别编号
                    if (myUserName.Equals(username))                                 //此账号存在
                    {
                        MyDefine.myXET.meLoginUser = MyDefine.myXET.meListUser[i];   //"用户名;密码;部门;电话;权类编号;"
                        MyDefine.myXET.meLoginUser += GetUserPerm(myCateCode) + ";"; //"权限类别编号对应的权限类别名称"
                        MyDefine.myXET.meLoginUser += GetUserPerm(myCateCode);       //"权限类别编号对应的权限列表"
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

        /// <summary>
        /// 获取权限类别名称对应的权限列表
        /// </summary>
        /// <param name="catname"></param>
        /// <returns></returns>
        public string GetUserPerm(string catname)
        {
            int numSTEP = Enum.GetNames(typeof(STEP)).Length;               //获取权限总个数
            string myperm = ("BBB").PadRight(numSTEP, 'B');                 //默认禁止所有权限
            for (int i = 0; i < MyDefine.myXET.meListPermCat.Count; i++)    //遍历权限类别列表
            {
                if(catname == MyDefine.myXET.meListPermCat[i].Split(';')[1])
                {
                    myperm = MyDefine.myXET.meListPermCat[i].Split(';')[2];
                    break;
                }
            }

            return myperm;
        }

        #endregion

        #region 用户名文本框失去焦点时，自动显示/消除密码

        //用户名文本框失去焦点时，自动显示密码(如果账号之前选中了记住密码)
        private void textBox3_Validated(object sender, EventArgs e)
        {
            string username = textBox3.Text;                                //当前用户名
            int index = MyDefine.myXET.GetUserNameIndex(username);          //当前用户名在账号列表中的索引
            if (index != -1)
            {
                string userpswd = MyDefine.myXET.meListUser[index].Split(';')[(int)ACCOUNT.PSWD];       //密码
                string isrember = MyDefine.myXET.meListUser[index].Split(';')[(int)ACCOUNT.REMPSD];     //是否记住当前账号的密码
                if (isrember == "1")            //已选择记住密码，将密码自动填入密码框
                {
                    textBox1.Text = userpswd;
                    checkBox1.Checked = true;   //勾选记住密码
                }
                else
                {
                    textBox1.Text = "";
                    checkBox1.Checked = false;  //取消勾选记住密码
                }
            }
        }

        #endregion

        #region 窗体移动

        Point MouseOffset;

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            MouseOffset = new Point(-e.X, -e.Y);
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point Mpos = Control.MousePosition;
                Mpos.Offset(MouseOffset.X, MouseOffset.Y);
                Location = Mpos;
            }
        }

        #endregion

    }
}

//end