using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HTR
{
    public partial class MenuAccountPanel : UserControl
    {
        public int status = 0;           //记录操作状态：0=none;1=新建账号;2=注销账号;3=修改账号;4=切换账号;5=修改密码
        public List<Label> labListUser = new List<Label>();                   //账号列表
        public int selectedUserIndex = -1;

        #region 界面加载

        public MenuAccountPanel()
        {
            InitializeComponent();
        }

        private void MenuAccountPanel_Load(object sender, EventArgs e)
        {
            BuildControlArray();                //创建控件列表
            loadUserList();                     //加载用户列表
            loadPermCatList();                  //加载权限类别列表并更新其下拉列表

            //MyDefine.myXET.RunMyUpdateEvent();      //登录成功后更新主界面账户名等信息!!!!!!!!!!!!!!
        }

        public void AddMyUpdateEvent()
        {
            checkPermission();              //核对当前登录账户的权限
        }

        //核对权限
        public void checkPermission()
        {
            button1.Enabled = MyDefine.myXET.CheckPermission(STEP.新建账号, false) ? true : false;
            button2.Enabled = MyDefine.myXET.CheckPermission(STEP.注销账号, false) ? true : false;
            comboBox1.Enabled = MyDefine.myXET.CheckPermission(STEP.权限类别变更, false) ? true : false;
            button5.Enabled = MyDefine.myXET.CheckPermission(STEP.权限管理, false) ? true : false;
            checkBox1.Visible = MyDefine.myXET.CheckPermission(STEP.密码查看, false) ? true : false;
            if (checkBox1.Visible == false) checkBox1.Checked = false;
        }

        #endregion

        #region 界面按钮事件

        #region 新建账号

        //新建账号
        private void button1_Click(object sender, EventArgs e)
        {
            label1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            status = 1;
            ClearLabelInfos();              //清空界面信息
            EnableLabelEdit(true);          //允许编辑账号信息
            MyDefine.myXET.AddTraceInfo("新建账号");
        }

        #endregion

        #region 注销账号

        //注销账号
        private void button2_Click(object sender, EventArgs e)
        {
            label1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            if (textBox1.Text == "")
            {
                MessageBox.Show("用户名为空！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            status = 2;
            if (MessageBox.Show("是否确认删除账号\"" + textBox1.Text + "\"?", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                DeleteUserAccount(textBox1.Text);                           //删除账号
                loadUserList();                                             //重新加载账号列表
                ClearLabelInfos();                                          //清空界面信息
                MyDefine.myXET.AddTraceInfo("注销账号");
            }
            status = 0;
        }

        #endregion

        #region 修改账号(部门、电话、权限类别)

        //修改账号
        private void button3_Click(object sender, EventArgs e)
        {
            label1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            if (label17.Text == "")
            {
                MessageBox.Show("用户名为空，请选择账号！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            status = 3;
            EnableLabelEdit(true);             //允许编辑账号信息
            MyDefine.myXET.AddTraceInfo("修改账号");

            comboBox1.Enabled = MyDefine.myXET.CheckPermission(STEP.权限类别变更, false) ? true : false;  //核对有无修改权限类别的权限
            if (label17.Text == MyDefine.myXET.userName) comboBox1.Enabled = false;   //当前选中账号为登录账号，禁止修改自身权限类别
        }

        #endregion

        #region 切换账号

        //切换账号
        private void button4_Click(object sender, EventArgs e)
        {
            label1.Focus(); Application.DoEvents();           //将焦点从button上移走，使button每次单击都有点击效果

            status = 4; 
            MyDefine.myXET.switchMainPanel(8);               //切换到切换账号界面
        }

        #endregion

        #region 权限管理

        //权限管理
        private void button5_Click(object sender, EventArgs e)
        {
            label1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            MyDefine.myXET.switchMainPanel(9);               //切换到权限管理界面
        }

        #endregion

        #region 保存账号

        //保存账号
        private void button6_Click(object sender, EventArgs e)
        {
            label1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            if (textBox1.Text == "")
            {
                MessageBox.Show("用户名不能为空！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (status != 5 && comboBox1.Text == "")    //不是在修改密码的过程中
            {
                MessageBox.Show("权限类别不能为空！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (status != 5 && textBox2.Text == "" || textBox3.Text == "" || textBox4.Text == "")   //不是在修改密码的过程中
            {
                if (MessageBox.Show("密码、部门或联系电话为空，是否继续？", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
                {
                    return;
                }
            }
            if (status == 5 && textBox3.Text != textBox4.Text)         //修改密码，两个新密码不一致
            {
                MessageBox.Show("两次输入的新密码不一致！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (status == 1 || status == 3)
            {
                if (status == 1) CreateUserAccount();                       //新建账号
                if (status == 3) ModifyUserAccount();                       //修改账号
                loadUserList();                                             //重新加载账号列表

                status = 0;
                label17.Text = textBox1.Text;
                label18.Text = textBox2.Text;
                label19.Text = textBox3.Text;
                label20.Text = textBox4.Text;
                label21.Text = comboBox1.Text;
                if (!checkBox1.Checked) label18.Text = "***";               //隐藏密码
                EnableLabelEdit(false);                                     //不允许编辑账号信息
            }
            
            if (status == 5)                                                //修改密码
            {
                if (ModifyUserPassword(textBox1.Text, textBox2.Text, textBox3.Text))   //修改失败
                {
                    TurnToPSDEdit(false);       //恢复不修改密码时的界面
                }

                ShowUserAccount(selectedUserIndex);                     //显示当前选中账号的信息
                status = 0;
                return;
            }

            MyDefine.myXET.AddTraceInfo("保存");
        }

        #endregion

        #region 取消操作

        //取消(取消新建/修改等操作)
        private void button7_Click(object sender, EventArgs e)
        {
            label1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            //新建、修改账号
            if (status == 1 || status == 3)
            {
                status = 0;                                 //清除操作状态变量
                EnableLabelEdit(false);                     //不允许编辑账号信息
            }

            //修改密码
            if (status == 5)
            {
                status = 0;                                 //清除操作状态变量
                TurnToPSDEdit(false);                       //恢复不修改密码时的界面
                ShowUserAccount(selectedUserIndex);         //显示当前选中账号的信息
            }

            MyDefine.myXET.AddTraceInfo("取消");
        }

        #endregion

        #region 查看密码

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            /*
            if (checkBox1.Checked)
            {
                label18.Text = textBox2.Text;
                textBox2.PasswordChar = default(char);

            }
            else
            {
                label18.Text = "***";
                textBox2.PasswordChar = '*';
            }
            */

            label1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            if (status == 5)                //正在修改密码
            {
                textBox2.PasswordChar = default(char);
                textBox3.PasswordChar = default(char);
                textBox4.PasswordChar = default(char);
                if (!checkBox1.Checked) textBox2.PasswordChar = '*';
                if (!checkBox1.Checked) textBox3.PasswordChar = '*';
                if (!checkBox1.Checked) textBox4.PasswordChar = '*';
            }
            else                           //
            {
                label18.Text = textBox2.Text;
                textBox2.PasswordChar = default(char);
                if (!checkBox1.Checked)
                {
                    label18.Text = "***";
                    textBox2.PasswordChar = '*';
                }
            }
        }

        #endregion

        #region 修改密码

        private void button8_Click(object sender, EventArgs e)
        {
            label1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            if (label17.Text == "")
            {
                MessageBox.Show("用户名为空，请选择账号！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (label17.Text != MyDefine.myXET.userName)             //当前选中账号非登录账号，核对是否有权限修改其它账号的密码
            {
                if (!MyDefine.myXET.CheckPermission(STEP.修改密码,false ))        //核对权限
                {
                    MessageBox.Show("当前账号权限不足，无法修改其它账号密码！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }
            status = 5;
            TurnToPSDEdit(true);
            MyDefine.myXET.AddTraceInfo("修改密码");
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
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
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
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        #endregion

        #region 控件数组

        #region 创建并初始化控件数组

        /// <summary>
        /// 创建控件数组
        /// </summary>
        private void BuildControlArray()
        {
            //权限类别
            labListUser.Add(label2);
            labListUser.Add(label3);
            labListUser.Add(label4);
            labListUser.Add(label5);
            labListUser.Add(label6);
            labListUser.Add(label7);
            labListUser.Add(label8);
            labListUser.Add(label9);
            labListUser.Add(label10);
            labListUser.Add(label11);
            labListUser.Add(label33);
            labListUser.Add(label34);
            labListUser.Add(label35);
            labListUser.Add(label36);
            labListUser.Add(label37);
        }

        #endregion

        #region 清空账号列表Label

        public void ClearLabUsers()
        {
            for (int i = 0; i < labListUser.Count; i++)
            {
                labListUser[i].Text = "";
            }
        }

        #endregion

        #endregion

        #region 用户管理

        #region 加载账号列表

        /// <summary>
        /// 加载用户列表
        /// </summary>
        /// <returns></returns>
        public void loadUserList()
        {
            try
            {
                MyDefine.myXET.loadUserList();      //加载用户列表
                ShowUserList();                     //显示用户列表
            }
            catch (Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("用户列表加载失败：" + ex.ToString());
            }
        }

        #endregion

        #region 保存账号列表

        /// <summary>
        /// 加载用户列表
        /// </summary>
        /// <returns></returns>
        public void SavePremCategory()
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

                //显示用户列表
                ClearLabUsers();                    //清空显示
                int total = labListUser.Count;      //可显示的最大数目
                int catnum = myUsers.Count > total ? total : myUsers.Count;
                for (int i = 0; i < catnum; i++)
                {
                    labListUser[i].Text = myUsers[i].Split(';')[1];
                }

                MyDefine.myXET.meListUser = new List<string>(myUsers.ToArray());   //将列表赋给全局变量

            }
            catch (Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("用户列表加载失败：" + ex.ToString());
            }
        }

        #endregion

        #region 新建帐号

        /// <summary>
        /// 新建帐号(不保存具体权限列表，账号登录时根据权限类别列表分配，否则更新权限类别列表后涉及到更新全部账号)
        /// </summary>
        public void CreateUserAccount()
        {
            //统计账号及权限信息
            int myPermCat = comboBox1.SelectedIndex;      //权限类别
            String myUserAccount = "";
            String myUserName = textBox1.Text;
            String myCateCode = MyDefine.myXET.meListPermCat[myPermCat].Split(';')[0];  //权限类别编号
            //String myPerm = MyDefine.myXET.meListPermCat[myPermCat].Split(';')[2];    //根据权限类别获取具体权限
            myUserAccount += myUserName + ";";            //用户名
            myUserAccount += textBox2.Text + ";";         //密码
            myUserAccount += textBox3.Text + ";";         //部门
            myUserAccount += textBox4.Text + ";";         //电话
            myUserAccount += "0" + ";" + "0" + ";";       //是否默认登录账号(否)、是否记住密码(否)
            myUserAccount += myCateCode + ";";            //权限类别编号
            //myUserAccount += myPerm;                    //权限类别对应的具体权限

            for (int i = 0; i < MyDefine.myXET.meListUser.Count; i++)          //遍历账号列表
            {
                string username = MyDefine.myXET.meListUser[i].Split(';')[(int)ACCOUNT.USER].ToUpper(); //用户名
                if (username.Equals(myUserName.ToUpper()))                     //此账号已存在
                {
                    MessageBox.Show(username.ToUpper() + "已存在，请重新输入用户名！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }

            MyDefine.myXET.SaveUserFile(myUserName, myUserAccount);             //新建账号文件
            MyDefine.myXET.meListUser.Add(myUserAccount);                       //添加新账号
            MessageBox.Show("新建账号成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region 注销帐号

        /// <summary>
        /// 注销帐号
        /// </summary>
        public void DeleteUserAccount(string myUserName)
        {
            for (int i = 0; i < MyDefine.myXET.meListUser.Count; i++)          //遍历账号列表
            {
                string username = MyDefine.myXET.meListUser[i].Split(';')[(int)ACCOUNT.USER];   //用户名
                if (username.Equals(myUserName))                               //此账号已存在
                {
                    MyDefine.myXET.DeleteUserFile(myUserName);                 //删除账号文件
                    MyDefine.myXET.meListUser.RemoveAt(i); ;                   //移除此账号
                    MessageBox.Show("注销账号成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            MessageBox.Show(myUserName + "不存在！", "系统提示", MessageBoxButtons.OK);//账号不存在
        }

        #endregion

        #region 修改帐号(不包括密码)

        /// <summary>
        /// 修改帐号(部门、权限类别等)(不保存具体权限列表，账号登录时根据权限类别列表分配，否则更新权限类别列表后涉及到更新全部账号)
        /// </summary>
        public void ModifyUserAccount()
        {
            //统计账号及权限信息
            String myUserName = textBox1.Text;                                                       //用户名
            String myCateName = comboBox1.Text;                                                      //权限类别名称
            Int32 myUserIdx = MyDefine.myXET.GetUserNameIndex(myUserName);                           //用户名在用户列表的索引
            String isDefUsr = MyDefine.myXET.meListUser[myUserIdx].Split(';')[(int)ACCOUNT.DEFUSR];  //是否默认账号
            String isRemPsd = MyDefine.myXET.meListUser[myUserIdx].Split(';')[(int)ACCOUNT.REMPSD];  //是否记住密码
            String myCatCode = MyDefine.myXET.GetPermNameCode(myCateName);                           //新权限类别名称对应的权限类别编号
            String myUserAccount = myUserName + ";";      //用户名
            myUserAccount += textBox2.Text + ";";         //密码
            myUserAccount += textBox3.Text + ";";         //部门
            myUserAccount += textBox4.Text + ";";         //电话
            myUserAccount += isDefUsr + ";";              //是否默认账号
            myUserAccount += isRemPsd + ";";              //是否记住密码
            myUserAccount += myCatCode + ";";             //权限类别编号
            //myUserAccount += myPerm;                    //权限类别对应的具体权限

            for (int i = 0; i < MyDefine.myXET.meListUser.Count; i++)          //遍历账号列表
            {
                string username = MyDefine.myXET.meListUser[i].Split(';')[(int)ACCOUNT.USER];   //用户名
                if (username.Equals(myUserName))                               //此账号已存在
                {
                    MyDefine.myXET.SaveUserFile(myUserName, myUserAccount);    //保存账号文件
                    MyDefine.myXET.meListUser[i] = myUserAccount;              //覆盖原账号信息
                    MessageBox.Show("账号修改成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            if (MessageBox.Show(myUserName + "不存在，是否新建账号？", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                MyDefine.myXET.SaveUserFile(myUserName, myUserAccount);             //新建账号文件
                MyDefine.myXET.meListUser.Add(myUserAccount);                       //添加新账号
                MessageBox.Show("新建账号成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

        #region 修改密码

        /// <summary>
        /// 修改密码
        /// </summary>
        public bool ModifyUserPassword(string myUserName, string oldPsd, string newPsd)
        {
            for (int i = 0; i < MyDefine.myXET.meListUser.Count; i++)          //遍历账号列表
            {
                string userAccount = MyDefine.myXET.meListUser[i];
                string userName = userAccount.Split(';')[(int)ACCOUNT.USER];
                if (userName.Equals(myUserName))                               //此账号存在
                {
                    string password = userAccount.Split(';')[(int)ACCOUNT.PSWD];
                    if (oldPsd == password)                                    //密码核对正确
                    {
                        userAccount = userAccount.Replace(userName + ";" + password, userName + ";" + newPsd);   //将原密码替换为新密码(防止密码为空时无法替换)
                        //userAccount = userAccount.Replace(userName + ";" + password, userName + ";" + newPsd);   //将原密码替换为新密码(防止密码为空时无法替换)
                        MyDefine.myXET.SaveUserFile(userName, userAccount);    //保存账号文件
                        MyDefine.myXET.meListUser[i] = userAccount;            //覆盖原账号列表信息
                        MessageBox.Show("密码修改成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("原密码错误！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return false;
                    }
                }
            }

            MessageBox.Show("账号不存在！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return false;
        }

        #endregion

        #region 保存帐号文件

        /// <summary>
        /// 保存帐号文件
        /// </summary>
        public void SaveUserFile(String myUserName, String myUserAccount)
        {
            //保存账号及权限信息
            String filepath = MyDefine.myXET.userCFG + @"\user." + myUserName + ".cfg";
            FileStream meFS = new FileStream(filepath, FileMode.Create, FileAccess.Write);
            StreamWriter meWrite = new StreamWriter(meFS);

            try
            {
                meWrite.WriteLine(myUserAccount);                  //保存账号
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
        public void DeleteUserFile(String myUserName)
        {
            //保存账号及权限信息
            String filepath = MyDefine.myXET.userCFG + @"\user." + myUserName + ".cfg";
            FileStream meFS = new FileStream(filepath, FileMode.Create, FileAccess.Write);
            StreamWriter meWrite = new StreamWriter(meFS);

            try
            {
                if (File.Exists(filepath))
                {
                    File.SetAttributes(filepath, FileAttributes.Normal);
                    File.Delete(filepath);
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("删除账号文件失败：" + ex.ToString());
            }
            finally
            {
                meWrite.Close();
                meFS.Close();
            }
        }

        #endregion

        #region 显示账号列表

        /// <summary>
        /// 显示账号列表
        /// </summary>
        public void ShowUserList()
        {
            //显示权限类别列表
            ClearLabUsers();                    //清空显示
            int total = labListUser.Count;      //可显示的最大数目
            int catnum = MyDefine.myXET.meListUser.Count > total ? total : MyDefine.myXET.meListUser.Count;
            for (int i = 0; i < catnum; i++)
            {
                labListUser[i].Text = MyDefine.myXET.meListUser[i].Split(';')[(int)ACCOUNT.USER];   //用户名
            }
        }

        #endregion

        #endregion

        #region 加载权限类别列表并更新其下拉列表

        /// <summary>
        /// 加载权限类别列表并更新其下拉列表
        /// </summary>
        public void loadPermCatList()
        {
            MyDefine.myXET.loadPremCatList();   //加载权限类别列表
            ShowPermCatList();                  //更新权限类别下拉列表
        }

        /// <summary>
        /// 显示权限类别列表
        /// </summary>
        public void ShowPermCatList()
        {
            comboBox1.Items.Clear();
            for (int i = 0; i < MyDefine.myXET.meListPermCat.Count; i++)
            {
                comboBox1.Items.Add(MyDefine.myXET.meListPermCat[i].Split(';')[1]);
            }
        }

        #endregion

        #region 单击账户列表

        //单击账户列表
        private void labelUsers_Click(object sender, EventArgs e)
        {
            int index = labListUser.IndexOf((Label)sender);                  //获取控件在控件数组中的索引
            if (index >= MyDefine.myXET.meListUser.Count) return;            //权限类别为空
            selectedUserIndex = index;

            ShowUserAccount(index);
        }

        /// <summary>
        /// 显示账号信息
        /// </summary>
        /// <param name="index">账号索引</param>
        public void ShowUserAccount(int index)
        {
            string[] userInfos = MyDefine.myXET.meListUser[index].Split(';');
            string catcode = userInfos[(int)ACCOUNT.CATCODE];       //类别编号
            textBox1.Text = userInfos[(int)ACCOUNT.USER];
            textBox2.Text = userInfos[(int)ACCOUNT.PSWD];
            textBox3.Text = userInfos[(int)ACCOUNT.DEPT];
            textBox4.Text = userInfos[(int)ACCOUNT.TELE];
            comboBox1.SelectedItem = MyDefine.myXET.GetUserPermName(catcode);
            
            label17.Text = textBox1.Text;
            label18.Text = (checkBox1.Checked) ? textBox2.Text : "***";     //若查看密码被选中，显示密码，否则显示"***"
            label19.Text = textBox3.Text;
            label20.Text = textBox4.Text;
            label21.Text = comboBox1.Text;
            EnableLabelEdit(false);             //不允许编辑账号信息
        }

        #endregion

        #region 禁止/允许编辑界面信息

        //禁止/允许编辑界面信息
        public void EnableLabelEdit(Boolean allowEdit)
        {
            if(allowEdit)
            {
                textBox3.Visible = true;
                textBox4.Visible = true;
                comboBox1.Visible = true;
                comboBox1.Enabled = true;

                label19.Visible = false;
                label20.Visible = false;
                label21.Visible = false;
                button1.Visible = false;                         //隐藏新建按钮
                button2.Visible = false;                         //隐藏注销按钮
                button3.Visible = false;                         //隐藏修改按钮
                button4.Visible = false;                         //隐藏切换按钮
                button5.Visible = false;                         //隐藏权限按钮
                button6.Visible = true;                          //显示保存按钮
                button7.Visible = true;                          //显示取消按钮
                button8.Visible = false;                         //隐藏修改密码按钮

                if (status ==1)              //新建账户
                {
                    textBox1.Visible = true;                       //用户名编辑框
                    textBox2.Visible = true;                       //密码编辑框
                    label17.Visible = false;                       //用户名显示框
                    label18.Visible = false;                       //密码显示框
                }
            }
            else
            {
                textBox1.Visible = false;                        //用户名编辑框
                textBox2.Visible = false;                        //密码编辑框
                textBox3.Visible = false;
                textBox4.Visible = false;
                comboBox1.Visible = false;
                label17.Visible = true;                        //用户名显示框
                label18.Visible = true;                        //密码显示框
                label19.Visible = true;
                label20.Visible = true;
                label21.Visible = true;
                button1.Visible = true;                         //显示新建按钮
                button2.Visible = true;                         //显示注销按钮
                button3.Visible = true;                         //显示修改按钮
                button4.Visible = true;                         //显示切换按钮
                button5.Visible = true;                         //显示权限按钮
                button6.Visible = false;                        //隐藏保存按钮
                button7.Visible = false;                        //隐藏取消按钮
                button8.Visible = true;                         //显示修改密码按钮
            }
        }

        //清空界面信息
        public void ClearLabelInfos()
        {
            textBox1.Text = "";             
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            comboBox1.Text = "";
            label17.Text = "";
            label18.Text = "";
            label19.Text = "";
            label20.Text = "";
            label21.Text = "";
        }

        #endregion

        #region 修改密码界面变化

        //修改密码/取消修改密码
        public void TurnToPSDEdit(Boolean allowEdit)
        {
            if (allowEdit)                      //修改密码
            {
                label13.Text = "原 密 码";
                label14.Text = "新 密 码";
                label15.Text = "新 密 码";
                label16.Visible = false;                       //隐藏权限类别标签

                label17.Visible = true;                        //显示用户名显示框
                label18.Visible = false;                       //隐藏密码显示框
                label19.Visible = false;                       //隐藏部门显示框
                label20.Visible = false;                       //隐藏电话显示框
                label21.Visible = false;                       //隐藏权限类别显示框

                textBox2.Text = "";                            //清空内容
                textBox3.Text = "";                            //清空内容
                textBox4.Text = "";                            //清空内容
                textBox1.Visible = false;                      //隐藏用户名编辑框
                textBox2.Visible = true;                       //原密码编辑框
                textBox3.Visible = true;                       //新密码编辑框
                textBox4.Visible = true;                       //确认密码编辑框
                comboBox1.Visible = false;                     //隐藏权限类别编辑框

                textBox2.PasswordChar = default(char);
                textBox3.PasswordChar = default(char);
                textBox4.PasswordChar = default(char);
                if (!checkBox1.Checked) textBox2.PasswordChar = '*';
                if (!checkBox1.Checked) textBox3.PasswordChar = '*';
                if (!checkBox1.Checked) textBox4.PasswordChar = '*';

                button1.Visible = false;                         //隐藏新建按钮
                button2.Visible = false;                         //隐藏注销按钮
                button3.Visible = false;                         //隐藏修改按钮
                button4.Visible = false;                         //隐藏切换按钮
                button5.Visible = false;                         //隐藏权限按钮
                button6.Visible = true;                          //显示保存按钮
                button7.Visible = true;                          //显示取消按钮
                button8.Visible = false;                         //隐藏修改密码按钮

            }
            else                      //完成或取消修改密码
            {
                label13.Text = "密    码";
                label14.Text = "部    门";
                label15.Text = "联系电话";
                label16.Visible = true;                        //显示权限类别标签

                label17.Visible = true;                        //显示用户名显示框
                label18.Visible = true;                        //显示密码显示框
                label19.Visible = true;                        //显示部门显示框
                label20.Visible = true;                        //显示电话显示框
                label21.Visible = true;                        //显示权限类别显示框

                textBox1.Visible = false;                      //隐藏用户名编辑框
                textBox2.Visible = false;                      //隐藏密码编辑框
                textBox3.Visible = false;                      //隐藏部门编辑框
                textBox4.Visible = false;                      //隐藏电话编辑框
                comboBox1.Visible = false;                     //隐藏权限类别编辑框
                textBox2.PasswordChar = default(char);
                textBox3.PasswordChar = default(char);
                textBox4.PasswordChar = default(char);
                if (checkBox1.Checked) textBox2.PasswordChar = '*';

                button1.Visible = true;                         //显示新建按钮
                button2.Visible = true;                         //显示注销按钮
                button3.Visible = true;                         //显示修改按钮
                button4.Visible = true;                         //显示切换按钮
                button5.Visible = true;                         //显示权限按钮
                button6.Visible = false;                        //隐藏保存按钮
                button7.Visible = false;                        //隐藏取消按钮
                button8.Visible = true;                         //显示修改密码按钮
            }
        }

        #endregion

    }
}
