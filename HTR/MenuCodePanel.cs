using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HTR
{
    public partial class MenuCodePanel : UserControl
    {
        List<TextBox> txtJSNArray = new List<TextBox>();                   //产品编号数组
        List<TextBox> txtUSNArray = new List<TextBox>();                   //管理编号数组
        List<TextBox> txtUTXTArray = new List<TextBox>();                  //备注信息数组

        public MenuCodePanel()
        {
            InitializeComponent();
        }

        private void MenuCodePanel_Load(object sender, EventArgs e)
        {
            BuildControlArray();        //创建控件数组
        }

        public void AddMyUpdateEvent()
        {
            //核对是否有编辑管理编号的权限(不要弹窗)
            if (MyDefine.myXET.CheckPermission(STEP.管理编号,false))        //有权限
            {        
                SetUSNEnable(true);
            }
            else
            {                                                               //权限不足
                SetUSNEnable(false);
            }

            //核对是否有编辑备注信息的权限(不要弹窗)
            if (MyDefine.myXET.CheckPermission(STEP.备注信息, false))       //有权限
            {
                SetUTXTEnable(true);
            }
            else                                                            //权限不足
            {
                SetUTXTEnable(false);
            }
        }

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
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        #endregion

        #region 界面按钮事件

        #region 更新列表按钮 -- 批量读取JSN、USN、UTXT并更新界面

        /// <summary>
        /// 更新列表按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            label1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            if (!MyDefine.myXET.checkDeviceStatus()) return;    //设备未连接或繁忙

            Boolean ret = true;
            button1.Text = "更新中...";
            Application.DoEvents();
            MyDefine.myXET.meTask = TASKS.setting;  //设置串口为工作中
            MyDefine.myXET.meTips = "[CODE LIST REFRESH]" + Environment.NewLine;
            Byte activeDUTAddress = MyDefine.myXET.meActiveAddr;   //保存当前正在连接的设备地址，批量设置完后再设置回来
            ResetControlArray();                                   //复位界面显示
            MyDefine.myXET.AddTraceInfo("更新列表");

            try
            {
                for (int i = 0; i < MyDefine.myXET.meDUTAddrArr.Count; i++)
                {
                    MyDefine.myXET.meActiveAddr = MyDefine.myXET.meDUTAddrArr[i];     //切换当前设备地址

                    #region 读取JSN(设备型号/测量范围/产品编号)

                    //读取产品编号："设备型号,测量范围,产品编号"
                    ret = MyDefine.myXET.readJSN();
                    if (ret)            //读取成功，刷新界面信息
                    {
                        txtJSNArray[i].Text= MyDefine.myXET.meJSN.Length > 2 ? MyDefine.myXET.meJSN.Substring(2) : MyDefine.myXET.meJSN;        //产品编号
                    }
                    else
                    {
                        txtJSNArray[i].BackColor = Color.Red;
                    }

                    #endregion

                    #region 读取USN(管理编号)

                    //读取管理编号
                    ret = MyDefine.myXET.readUSN();

                    if (ret)            //读取成功，刷新界面信息
                    {
                        txtUSNArray[i].Text = MyDefine.myXET.meUSN.Trim();        //产品编号
                    }
                    else
                    {
                        txtUSNArray[i].BackColor = Color.Red;
                    }

                    #endregion

                    #region 读取UTXT(备注信息)

                    //读取备注信息
                    ret = MyDefine.myXET.readUTXT();

                    if (ret)            //读取成功，刷新界面信息
                    {
                        txtUTXTArray[i].Text = MyDefine.myXET.meUTXT.Trim();        //产品编号
                    }
                    else
                    {
                        txtUTXTArray[i].BackColor = Color.Red;
                    }

                    #endregion

                    MyDefine.myXET.SaveCommunicationTips();  //将调试信息保存到操作日志
                }
            }
            catch(Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("更新列表失败：" + ex.ToString());
            }

            if (ret) MyDefine.myXET.AddTraceInfo("更新列表成功");
            MyDefine.myXET.meTask = TASKS.run;
            MyDefine.myXET.meActiveAddr = activeDUTAddress;        //将当前设备地址切换回批量设置前的已连接设备地址
            MyDefine.myXET.meTips += "ReadREG_Device:" + Environment.NewLine;
            MyDefine.myXET.readDevice();                            //重新读取当前设备型号等参数

            button1.Text = "更新列表";
            Application.DoEvents();
        }

        #endregion

        #region 批量设置按钮 -- 批量设置JSN、USN、UTXT并更新界面

        /// <summary>
        /// 批量设置按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            label1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            if (!MyDefine.myXET.checkDeviceStatus()) return;    //设备未连接或繁忙

            Boolean ret = true;
            button2.Text = "设置中...";
            Application.DoEvents();
            MyDefine.myXET.meTask = TASKS.setting;  //设置串口为工作中
            MyDefine.myXET.meTips = "[CODE LIST SETTING]" + Environment.NewLine;
            Byte activeDUTAddress = MyDefine.myXET.meActiveAddr;   //保存当前正在连接的设备地址，批量设置完后再设置回来
            MyDefine.myXET.AddTraceInfo("批量设置");

            try
            {
                for (int i = 0; i < MyDefine.myXET.meDUTAddrArr.Count; i++)
                {
                    MyDefine.myXET.meActiveAddr = MyDefine.myXET.meDUTAddrArr[i];     //切换当前设备地址

                    #region 设置USN(管理编号)

                    ret = MyDefine.myXET.setUSN(txtUSNArray[i].Text);
                    if (!ret) txtUSNArray[i].BackColor = Color.Red;

                    /*
                    if (txtUSNArray[i].Text.Trim() != "")
                    {
                        ret = MyDefine.myXET.setUSN(txtUSNArray[i].Text.Trim());
                        if (!ret) txtUSNArray[i].BackColor = Color.Red;
                    }
                    else
                    {
                        ret = MyDefine.myXET.setUSN(" ");
                        if (!ret) txtUSNArray[i].BackColor = Color.Red;
                    }
                    */

                    #endregion

                    #region 设置UTXT(备注信息)

                    ret = MyDefine.myXET.setUTXT(txtUTXTArray[i].Text);
                    if (!ret) txtUTXTArray[i].BackColor = Color.Red;

                    /*
                    if (txtUTXTArray[i].Text.Trim() != "")
                    {
                        ret = MyDefine.myXET.setUTXT(txtUTXTArray[i].Text.Trim());
                        if (!ret) txtUTXTArray[i].BackColor = Color.Red;
                    }
                    else
                    {
                        ret = MyDefine.myXET.setUTXT(" ");
                        if (!ret) txtUTXTArray[i].BackColor = Color.Red;
                    }
                    */

                    #endregion

                    MyDefine.myXET.SaveCommunicationTips();  //将调试信息保存到操作日志
                }
            }
            catch(Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("批量设置失败:" + ex.ToString());
            }

            if (ret) MyDefine.myXET.ShowCorrectMsg("设置成功！");
            MyDefine.myXET.meTask = TASKS.run;
            MyDefine.myXET.meActiveAddr = activeDUTAddress;        //将当前设备地址切换回批量设置前的已连接设备地址
            MyDefine.myXET.meTips += "ReadREG_Device:" + Environment.NewLine;
            MyDefine.myXET.readDevice();                            //重新读取当前设备型号等参数

            button2.Text = "批量设置";
            Application.DoEvents();
        }

        #endregion

        #endregion

        #region 控件数组

        #region 创建并初始化控件数组

        /// <summary>
        /// 创建控件数组
        /// </summary>
        private void BuildControlArray()
        {
            //产品编号
            txtJSNArray.Add(textBox1);
            txtJSNArray.Add(textBox2);
            txtJSNArray.Add(textBox3);
            txtJSNArray.Add(textBox4);
            txtJSNArray.Add(textBox5);
            txtJSNArray.Add(textBox6);
            txtJSNArray.Add(textBox7);
            txtJSNArray.Add(textBox8);
            txtJSNArray.Add(textBox9);
            txtJSNArray.Add(textBox10);

            //管理编号
            txtUSNArray.Add(textBox11);
            txtUSNArray.Add(textBox12);
            txtUSNArray.Add(textBox13);
            txtUSNArray.Add(textBox14);
            txtUSNArray.Add(textBox15);
            txtUSNArray.Add(textBox16);
            txtUSNArray.Add(textBox17);
            txtUSNArray.Add(textBox18);
            txtUSNArray.Add(textBox19);
            txtUSNArray.Add(textBox20);

            //备注信息
            txtUTXTArray.Add(textBox21);
            txtUTXTArray.Add(textBox22);
            txtUTXTArray.Add(textBox23);
            txtUTXTArray.Add(textBox24);
            txtUTXTArray.Add(textBox25);
            txtUTXTArray.Add(textBox26);
            txtUTXTArray.Add(textBox27);
            txtUTXTArray.Add(textBox28);
            txtUTXTArray.Add(textBox29);
            txtUTXTArray.Add(textBox30);

            ResetControlArray();
        }

        #endregion

        #region 复位控件数组各项属性

        //复位控件数组各项属性
        public void ResetControlArray()
        {
            for (int i = 0; i < 10; i++)
            {
                txtJSNArray[i].Text = "";
                txtJSNArray[i].BackColor = Color.FromArgb(116, 170, 219);

                txtUSNArray[i].Text = "";
                txtUSNArray[i].BackColor = Color.FromArgb(116, 170, 219);

                txtUTXTArray[i].Text = "";
                txtUTXTArray[i].BackColor = Color.FromArgb(116, 170, 219);
            }
        }

        #endregion

        #region 使能或禁止USN

        //使能或禁止USN
        public void SetUSNEnable(Boolean enable)
        {
            for (int i = 0; i < 10; i++)
            {
                txtUSNArray[i].Enabled = (enable == true) ? true : false;
            }
        }

        #endregion

        #region 使能或禁止UTXT

        //使能或禁止UTXT
        public void SetUTXTEnable(Boolean enable)
        {
            for (int i = 0; i < 10; i++)
            {
                txtUTXTArray[i].Enabled = (enable == true) ? true : false;
            }
        }

        #endregion

        #endregion

        #region 文本框输入限制(禁止输入中文及中文符号)

        /// <summary>
        /// 文本框输入限制(禁止输入中文及中文符号)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txt_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)        //禁止输入回车
            {
                e.Handled = true;
                return;
            }

            int mycharNum = System.Text.Encoding.GetEncoding("GBK").GetBytes(e.KeyChar.ToString()).Length;
            if (mycharNum > 1)          //禁止输入中文及中文符号
            {
                //MessageBox.Show("请输入英文字符！");
                e.Handled = true;
                return;
            }
        }

        #endregion

    }
}
