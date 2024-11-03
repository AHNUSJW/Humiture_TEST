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
    public partial class MenuPermissionPanel : UserControl
    {
        public int status = 0;                            //记录操作状态：0=none;1=新建权限类别;2=删除权限类别;3=修改权限类别
        public string meSelectedCatCode = "";             //当前选中的权限类别的编号
        public string meSelectedCatName = "";             //当前选中的权限类别的名称
        public List<CheckBox> chbListPermItems = new List<CheckBox>();                //权限项
        public List<Label> labListPermCategory = new List<Label>();                   //权限类别

        #region 界面加载

        public MenuPermissionPanel()
        {
            InitializeComponent();
        }

        private void MenuPermissionPanel_Load(object sender, EventArgs e)
        {
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.WrapContents = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.MaximumSize = new Size(500, 300);

            BuildControlArray();        //创建控件数组
            loadPremCategory();         //加载权限类别
            InitPremItems();            //根据登录的账号初始化权限选项
        }

        public void AddMyUpdateEvent()
        {
            InitPremItems();            //根据登录的账号初始化权限选项
        }

        #endregion

        #region 界面按钮事件

        #region 新建权限类别

        //新建权限类别
        private void button1_Click(object sender, EventArgs e)
        {
            groupBox1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            status = 1;
            textBox1.Enabled = true;
            textBox1.Visible = true;
            button4.Visible = true;         //显示保存按钮
            MyDefine.myXET.AddTraceInfo("新建");

            //将界面上未使能的权限项设置为未选中状态，防止用户建立自己权限外的权限
            for (int i = 0; i < chbListPermItems.Count; i++)        //权限细项
            {
                if (chbListPermItems[i].Enabled == false) chbListPermItems[i].Checked = false;
            }
        }

        #endregion

        #region 删除权限类别

        //删除权限类别
        private void button2_Click(object sender, EventArgs e)
        {
            groupBox1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            if (textBox1.Text == "")
            {
                MessageBox.Show("请选中类别名称！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            status = 2;
            textBox1.Enabled = false;
            textBox1.Visible = false;
            button4.Visible = false;                //隐藏保存按钮

            if (MessageBox.Show("是否确认删除类别名称\"" + textBox1.Text + "\"?", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                MyDefine.myXET.AddTraceInfo("删除");
                DeletePremCategory(textBox1.Text);                      //删除类别名称
                loadPremCategory();                                     //加载权限类别列表
                MyDefine.myXET.switchMainPanel(25);                     //删除权限类别后，更新用户管理界面的权限类别下拉列表(跨窗体操作主界面控件)
            }
            status = 0;
        }

        #endregion

        #region 修改权限类别

        //修改权限类别
        private void button3_Click(object sender, EventArgs e)
        {
            groupBox1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            status = 3;
            textBox1.Enabled = true;
            textBox1.Visible = true;
            button4.Visible = true;         //显示保存按钮
            MyDefine.myXET.AddTraceInfo("修改");

            //将界面上未使能的权限项设置为未选中状态，防止用户建立自己权限外的权限
            for (int i = 0; i < chbListPermItems.Count; i++)        //权限细项
            {
                if (chbListPermItems[i].Enabled == false) chbListPermItems[i].Checked = false;
            }
        }

        #endregion

        #region 保存

        //保存
        private void button4_Click(object sender, EventArgs e)
        {
            groupBox1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            if (textBox1.Text == "")
            {
                MessageBox.Show("类别名称为空！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            //新建权限类别
            if (status == 1)
            {
                bool ret = CreatePremCategory(textBox1.Text);      //新建权限类别
                if (ret) MyDefine.myXET.switchMainPanel(25);        //新建权限类别后，更新用户管理界面的权限类别下拉列表(跨窗体操作主界面控件)
            }

            //修改权限类别
            if (status == 3)
            {
                bool ret = ModifyPremCategory(textBox1.Text);      //修改权限类别
                if (ret) MyDefine.myXET.switchMainPanel(25);        //新建权限类别后，更新用户管理界面的权限类别下拉列表(跨窗体操作主界面控件)
            }

            loadPremCategory();                                                     //加载权限类别列表
            textBox1.Enabled = false;
            textBox1.Visible = false;
            button4.Visible = false;                                //隐藏保存按钮
            status = 0;
            MyDefine.myXET.AddTraceInfo("保存");
        }

        #endregion

        #region 单击权限类别Label

        //单击权限类别Label
        private void labelCategory_Click(object sender, EventArgs e)
        {
            int index = labListPermCategory.IndexOf((Label)sender);               //获取控件在控件数组中的索引
            if (index >= MyDefine.myXET.meListPermCat.Count) return;              //权限类别为空

            meSelectedCatCode = MyDefine.myXET.meListPermCat[index].Split(';')[0];//保存当前选中的类别编号
            meSelectedCatName = MyDefine.myXET.meListPermCat[index].Split(';')[1];//保存当前选中的类别名称
            textBox1.Text = meSelectedCatName;                                    //显示类别名称
            groupBox1.Text = meSelectedCatName;                                   //显示类别名称
            ShowPremCategory(index);                                              //显示权限类别项
            textBox1.Enabled = false;
            textBox1.Visible = false;
            button4.Visible = false;                                              //隐藏保存按钮

            MyDefine.myXET.AddTraceInfo("选择权限" + meSelectedCatName);
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
            labListPermCategory.Add(label3);
            labListPermCategory.Add(label4);
            labListPermCategory.Add(label9);
            labListPermCategory.Add(label5);
            labListPermCategory.Add(label14);
            labListPermCategory.Add(label33);
            labListPermCategory.Add(label45);
            labListPermCategory.Add(label46);
            labListPermCategory.Add(label47);
            labListPermCategory.Add(label48);
            labListPermCategory.Add(label65);
            labListPermCategory.Add(label66);
            labListPermCategory.Add(label67);
            labListPermCategory.Add(label68);
            labListPermCategory.Add(label69);

            //权限项
            chbListPermItems.Add(checkBox1);
            chbListPermItems.Add(checkBox2);
            chbListPermItems.Add(checkBox3);
            chbListPermItems.Add(checkBox4);
            chbListPermItems.Add(checkBox5);
            chbListPermItems.Add(checkBox6);
            chbListPermItems.Add(checkBox7);
            chbListPermItems.Add(checkBox8);
            chbListPermItems.Add(checkBox9);
            chbListPermItems.Add(checkBox10);
            chbListPermItems.Add(checkBox11);
            chbListPermItems.Add(checkBox12);
            chbListPermItems.Add(checkBox13);
            chbListPermItems.Add(checkBox14);
            chbListPermItems.Add(checkBox15);
            chbListPermItems.Add(checkBox16);
            chbListPermItems.Add(checkBox17);
            chbListPermItems.Add(checkBox18);
            chbListPermItems.Add(checkBox19);
            chbListPermItems.Add(checkBox20);
            chbListPermItems.Add(checkBox21);
            chbListPermItems.Add(checkBox22);
            chbListPermItems.Add(checkBox23);
            chbListPermItems.Add(checkBox24);
            chbListPermItems.Add(checkBox25);
            chbListPermItems.Add(checkBox26);
            chbListPermItems.Add(checkBox27);
            chbListPermItems.Add(checkBox28);
            chbListPermItems.Add(checkBox29);
            chbListPermItems.Add(checkBox30);
            chbListPermItems.Add(checkBox31);
            chbListPermItems.Add(checkBox32);
            chbListPermItems.Add(checkBox33);
            chbListPermItems.Add(checkBox34);
            chbListPermItems.Add(checkBox35);
            chbListPermItems.Add(checkBox36);
        }

        #endregion

        #region 清空权限类别Label

        public void ClearLabPremCategory()
        {
            for (int i = 0; i < labListPermCategory.Count; i++)        //总权限类别
            {
                labListPermCategory[i].Text = "";
            }
        }

        #endregion

        #endregion

        #region 权限类别

        #region 初始化权限项

        /// <summary>
        /// 根据登录的账号初始化权限选项(仅登录账号拥有的权限项被使能)
        /// </summary>
        public void InitPremItems()
        {
            if (MyDefine.myXET.meLoginUser == "") return;
            String mycat = MyDefine.myXET.meLoginUser.Split(';')[(int)ACCOUNT.CATNAME];        //权限类别名称
            String myperm = MyDefine.myXET.meLoginUser.Split(';')[(int)ACCOUNT.CATLIST];       //权限类别列表

            flowLayoutPanel1.Controls.Clear();
            for (int i = 0; i < chbListPermItems.Count; i++)                //遍历权限细项
            {
                chbListPermItems[i].Enabled = false;
                chbListPermItems[i].Checked = myperm[i].Equals('A') ? true : false;
                if (chbListPermItems[i].Checked) chbListPermItems[i].Enabled = true;
                //if (chbListPermItems[i].Checked) flowLayoutPanel1.Controls.Add(chbListPermItems[i]);
                Application.DoEvents();
            }

            //只有最高权限才能给编辑PDF权限
            if (MyDefine.myXET.meLoginUser.Split(';')[(int)ACCOUNT.USER] != "JDEGREE")
            {
                label76.Visible = false;
                checkBox35.Visible = false;
            }
            else
            {
                label76.Visible = true;
                checkBox35.Visible = true;
            }
        }

        #endregion

        #region 新建权限类别

        public bool CreatePremCategory(string catname)
        {
            //禁止修改当前登录账号的权限类别
            string myLoginCat = MyDefine.myXET.meLoginUser.Split(';')[(int)ACCOUNT.CATNAME];       //当前登录账号的类别名称
            if (catname == myLoginCat)
            {
                MessageBox.Show("当前账号权限类别禁止编辑！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            //统计权限列表
            string mycode = GetRNDCode();                           //生成随机权限编号
            string myperm = mycode + ";" + catname + ";";           //权限编号;类别名称;
            for (int i = 0; i < chbListPermItems.Count; i++)        //权限列表
            {
                string sign = chbListPermItems[i].Checked ? "A" : "B";  //A=有权限；B=无权限。使用A/B而不是1/0是因为加密时1/0会显示固定且不同的字符，而A/B全部显示为口

                if (i == chbListPermItems.Count - 2) //验证编辑PDF权限
                {
                    if (MyDefine.myXET.meLoginUser.Split(';')[(int)ACCOUNT.USER] != "JDEGREE")//只有最高权限才能给编辑PDF权限
                    {
                        sign = "B";
                    }
                }
                myperm += sign;
            }

            for (int i = 0; i < MyDefine.myXET.meListPermCat.Count; i++)        //遍历权限类别
            {
                string myCatname = MyDefine.myXET.meListPermCat[i].Split(';')[1].ToUpper();
                if (myCatname.Equals(catname.ToUpper()))                        //此权限类别已存在
                {
                    /*
                    if (MessageBox.Show(catname + "已存在，是否覆盖？", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                        MyDefine.myXET.meListPermCat[i] = myperm;               //覆盖此类别名称原有信息
                        MyDefine.myXET.SavePermCatFile(catname, myperm);        //保存(覆盖)权限类别文件
                        MessageBox.Show(catname+ "覆盖成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    */

                    MessageBox.Show("[" + catname.ToUpper() + "]已存在，请重新输入权限类别名称！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }

            MyDefine.myXET.SavePermCatFile(catname, myperm);                    //新建权限类别文件
            MyDefine.myXET.meListPermCat.Add(myperm);                           //类别名称不存在，添加类别名称
            MessageBox.Show("新建权限类别成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }

        #endregion

        #region 修改权限类别

        /// <summary>
        /// 修改权限类别
        /// </summary>
        /// <param name="catname">权限类别名称</param>
        public bool ModifyPremCategory(string catname)
        {
            //禁止修改当前登录账号的权限类别
            string myLoginCatName = MyDefine.myXET.meLoginUser.Split(';')[(int)ACCOUNT.CATNAME];       //当前登录账号的类别名称
            if (catname == myLoginCatName)
            {
                MessageBox.Show("当前账号权限类别禁止编辑！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            //检查是否有重复权限类别名称
            //MessageBox.Show(catname + "," + meSelectedCatName);
            if (catname != meSelectedCatName && MyDefine.myXET.GetPermNameIndex(catname) != -1)     //类别名变更，且变更后的类别名已存在于列表中
            {
                MessageBox.Show("当前权限类别名称已存在！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            //生成权限类别编号+类别名称+权限列表
            string myperm = meSelectedCatCode + ";" + catname + ";";    //类别编号+类别名称
            for (int i = 0; i < chbListPermItems.Count; i++)            //权限列表
            {
                string sign = chbListPermItems[i].Checked ? "A" : "B";  //A=有权限；B=无权限。使用A/B而不是1/0是因为加密时1/0会显示固定且不同的字符，而A/B全部显示为口
                myperm += sign;
            }

            //遍历权限类别列表，查找对应权限类别
            for (int i = 0; i < MyDefine.myXET.meListPermCat.Count; i++)        //遍历权限类别
            {
                string myCatcode = MyDefine.myXET.meListPermCat[i].Split(';')[0];   //权限类别编号
                if (myCatcode.Equals(meSelectedCatCode))                        //找到当前权限类别所在位置
                {
                    MyDefine.myXET.DeletePermCatFile(meSelectedCatName);        //删除原权限类别文件
                    MyDefine.myXET.SavePermCatFile(catname, myperm);            //新建权限类别文件
                    MyDefine.myXET.meListPermCat[i] = myperm;                   //覆盖此类别名称原有信息
                    MessageBox.Show("修改权限类别成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 修改权限类别(修改类别名称和/或权限细则)
        /// </summary>
        /// <param name="oldname">修改前的类别名称</param>
        /// <param name="newname">修改后的类别名称</param>
        public void ModifyPremCategory0(string oldname, string newname)
        {
            //禁止修改当前登录账号的权限类别
            string myLoginCat = MyDefine.myXET.meLoginUser.Split(';')[(int)ACCOUNT.CATNAME];       //当前登录账号的类别名称
            if (oldname == myLoginCat)
            {
                MessageBox.Show("当前账号权限类别禁止编辑！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            string myperm = newname + ";";                          //类别名称
            for (int i = 0; i < chbListPermItems.Count; i++)        //权限细项
            {
                string sign = chbListPermItems[i].Checked ? "1" : "0";
                myperm += sign;
            }

            for (int i = 0; i < MyDefine.myXET.meListPermCat.Count; i++)        //遍历权限类别
            {
                string myCatname = MyDefine.myXET.meListPermCat[i].Split(';')[1];
                if (myCatname.Equals(oldname))                                  //找到当前权限类别所在位置
                {
                    if (newname != oldname) MyDefine.myXET.DeletePermCatFile(oldname);  //文件名称改变，先删除原有文件，在创建新文件
                    MyDefine.myXET.SavePermCatFile(newname, myperm);            //新建权限类别文件
                    MyDefine.myXET.meListPermCat[i] = myperm;                   //覆盖此类别名称原有信息
                    MessageBox.Show("修改成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
        }

        #endregion

        #region 删除权限类别

        /// <summary>
        /// 删除权限类别
        /// </summary>
        /// <param name="catname">要删除的权限类别名称</param>
        public void DeletePremCategory(string catname)
        {
            //禁止修改当前登录账号的权限类别
            string myLoginCat = MyDefine.myXET.meLoginUser.Split(';')[(int)ACCOUNT.CATNAME];       //当前登录账号的类别名称
            if (catname == myLoginCat)
            {
                MessageBox.Show("当前账号权限类别禁止编辑！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            for (int i = 0; i < MyDefine.myXET.meListPermCat.Count; i++)        //遍历权限类别
            {
                string myCatname = MyDefine.myXET.meListPermCat[i].Split(';')[1];
                if (myCatname.Equals(catname))                                  //此权限类别存在
                {
                    MyDefine.myXET.DeletePermCatFile(catname);                  //删除权限类别文件
                    MyDefine.myXET.meListPermCat.RemoveAt(i);                   //移除此类别名称
                    MessageBox.Show("删除成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            MessageBox.Show(catname + "不存在！", "系统提示", MessageBoxButtons.OK);//类别名称不存在
        }

        #endregion

        #region 保存权限类别

        /// <summary>
        /// 保存权限类别
        /// </summary>
        public void savePremCategory()
        {
            String filepath = MyDefine.myXET.userCFG + @"\category.cfg";
            FileStream meFS = new FileStream(filepath, FileMode.Create, FileAccess.Write);
            StreamWriter meWrite = new StreamWriter(meFS);

            try
            {
                foreach (string mycat in MyDefine.myXET.meListPermCat)
                {
                    meWrite.WriteLine(mycat);                  //保存权限类别
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("保存权限类别失败：" + ex.ToString());
            }
            finally
            {
                meWrite.Close();
                meFS.Close();
            }
        }

        #endregion

        #region 加载权限类别列表

        /// <summary>
        /// 加载权限类别列表
        /// </summary>
        /// <returns></returns>
        public void loadPremCategory()
        {
            try
            {
                MyDefine.myXET.loadPremCatList();   //加载权限类别列表
                ShowPremCategory();                 //显示权限类别列表
            }
            catch (Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("权限类别加载失败：" + ex.ToString());
            }
        }

        /// <summary>
        /// 显示权限类别列表
        /// </summary>
        public void ShowPremCategory()
        {
            ClearLabPremCategory();         //清空显示
            int total = MyDefine.myXET.meListPermCat.Count;
            int catnum = total > 10 ? 10 : total;
            for (int i = 0; i < catnum; i++)
            {
                labListPermCategory[i].Text = MyDefine.myXET.meListPermCat[i].Split(';')[1];
            }
        }
        #endregion

        #region 显示权限项选中状态

        /// <summary>
        /// 显示权限项选中状态
        /// </summary>
        /// <param name="idx">权限类别索引</param>
        public void ShowCategoryList(int idx)
        {
            //显示权限类别列表
            ClearLabPremCategory();                     //清空显示
            int total = labListPermCategory.Count;      //可显示的最大数目
            int catnum = MyDefine.myXET.meListPermCat.Count > total ? total : MyDefine.myXET.meListPermCat.Count;
            for (int i = 0; i < catnum; i++)
            {
                labListPermCategory[i].Text = MyDefine.myXET.meListPermCat[i].Split(';')[1];
            }

        }

        #endregion

        #region 显示权限项

        /// <summary>
        /// 显示权限项选中状态
        /// </summary>
        /// <param name="idx">权限类别索引</param>
        public void ShowPremCategory(int idx)
        {
            string mypermCat = MyDefine.myXET.meListPermCat[idx];
            string mypermName = mypermCat.Split(';')[1];              //类别名称
            string mypermItem = mypermCat.Split(';')[2];              //权限设置
            for (int i = 0; i < chbListPermItems.Count; i++)          //显示权限列表
            {
                //chbListPermItems[i].Enabled = false;
                chbListPermItems[i].Checked = mypermItem[i].Equals('A') ? true : false;
                //if (chbListPermItems[i].Checked) chbListPermItems[i].Enabled = true;
            }
        }

        #endregion

        #region 随机生成四位数的权限编号

        /// <summary>
        /// 随机生成四位数的权限编号
        /// </summary>
        public string GetRNDCode()
        {
            string mycode = "";
            Random RND = new Random();                                      //随机数
            for (int i = 0; i < 100; i++)
            {
                mycode = RND.Next(1000, 9999).ToString();                   //生成随机权限编号
                if (!MyDefine.myXET.meListPermCat.Contains(mycode)) break;  //无重复编号
            }

            return mycode;
        }

        #endregion

        #endregion
    }
}
