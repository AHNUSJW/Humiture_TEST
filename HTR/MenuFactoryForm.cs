using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static iTextSharp.text.pdf.PdfDocument;

namespace HTR
{
    public partial class MenuFactoryForm : Form
    {
        public Boolean change = false;      //标记参数是否发生变化

        public MenuFactoryForm()
        {
            InitializeComponent();
        }

        #region 界面加载

        private void MenuFactoryForm_Load(object sender, EventArgs e)
        {
            try
            {
                MyDefine.myXET.meInterface = "出厂设置";
                //ShowCurrentSettings();                //读取并显示当前设定
                checkPermission();                    //核对权限
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("界面参数刷新失败：" + ex.ToString());
            }
        }

        //核对权限
        public void checkPermission()
        {
            textBox1.Enabled = MyDefine.myXET.CheckPermission(STEP.设备地址, false) ? true : false;
            textBox2.Enabled = MyDefine.myXET.CheckPermission(STEP.设备型号, false) ? true : false;
            textBox3.Enabled = MyDefine.myXET.CheckPermission(STEP.出厂编号, false) ? true : false;
            textBox4.Enabled = MyDefine.myXET.CheckPermission(STEP.测量范围, false) ? true : false;
            textBox5.Enabled = MyDefine.myXET.CheckPermission(STEP.校准日期, false) ? true : false;
            textBox6.Enabled = MyDefine.myXET.CheckPermission(STEP.公司电话, false) ? true : false;
            textBox7.Enabled = MyDefine.myXET.CheckPermission(STEP.设备名称, false) ? true : false;

            //超级账户才能改变是设备地址
            panel1.Visible = textBox1.Enabled;
            if (!panel1.Visible)
            {
                this.Size = new System.Drawing.Size(panel1.Location.X, this.Height);
            }
            else//加载文件
            {
                //所有用户加载
                if (Directory.Exists(MyDefine.myXET.userSAV))
                {
                    //存在
                    DirectoryInfo meDirectory = new DirectoryInfo(MyDefine.myXET.userSAV);
                    String meString = null;
                    foreach (FileInfo meFiles in meDirectory.GetFiles("*.txt"))
                    {
                        meString = meFiles.Name;
                        meString = meString.Replace(".txt", "");
                        listBox1.Items.Add(meString);
                    }
                }
                else
                {
                    //不存在则创建文件夹
                    Directory.CreateDirectory(MyDefine.myXET.userSAV);
                }
            }
        }

        #endregion

        #region 界面按钮事件

        //读取
        private void button2_Click(object sender, EventArgs e)
        {
            label3.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            ShowCurrentSettings();  //读取并显示当前设定
        }

        //保存
        private void button3_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(MyDefine.myXET.meTask.ToString());
            try
            {
                label3.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

                Boolean ret = true;
                MyDefine.myXET.AddTraceInfo("保存");

                //判断各项参数是否有权限
                Boolean perm1 = textBox1.Enabled;                                           //设备地址权限
                Boolean perm2 = textBox2.Enabled || textBox3.Enabled || textBox4.Enabled;   //设备型号/出厂编号/测量范围权限
                Boolean perm3 = textBox5.Enabled;                                           //校准日期权限
                Boolean perm4 = textBox6.Enabled;                                           //公司电话权限
                Boolean perm5 = textBox7.Enabled;                                           //设备名称

                //若所有权限都无，则禁止执行
                if ((perm1 || perm2 || perm3 || perm4 || perm5) == false)  
                {
                    MessageBox.Show("账号权限不足！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string mystr = MyDefine.myXET.meName;
                mystr += MyDefine.myXET.meActiveAddr.ToString();
                mystr += MyDefine.myXET.meModel;
                switch (MyDefine.myXET.meType)
                {
                    case DEVICE.HTT:
                        mystr += "TT" + MyDefine.myXET.meJSN;
                        break;
                    case DEVICE.HTP:
                        mystr += "TP" + MyDefine.myXET.meJSN;
                        break;
                    case DEVICE.HTH:
                        mystr += "TH" + MyDefine.myXET.meJSN;
                        break;
                    case DEVICE.HTQ:
                        mystr += "TQ" + MyDefine.myXET.meJSN;
                        break;
                    default:
                        mystr += MyDefine.myXET.meJSN;
                        break;
                }
                mystr += MyDefine.myXET.meRange;
                mystr += MyDefine.myXET.meDateCal;

                if(mystr != "")
                {
                    if (!MyDefine.myXET.checkDeviceStatus()) return;    //设备未连接或繁忙

                    if (MessageBox.Show("设备参数已存在，是否重新设置？", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
                    {
                        MyDefine.myXET.AddTraceInfo("取消保存");
                        return;
                    }

                    MyDefine.myXET.meTask = TASKS.setting;        //设置串口为工作中
                    if (ret && perm5) ret = SetDeviceName();      //设置设备名称
                    if (ret && perm1) ret = SetDeviceAddr();      //设置设备地址
                    if (ret && perm2) ret = SetFactoryJSN();      //设置出厂编号
                    if (ret && perm3) ret = SetJincalTime();      //设置校准日期
                    MyDefine.myXET.meTask = TASKS.run;            //设置串口为空闲状态
                }

                //保存公司电话
                if (ret && textBox6.Text != "" && perm4)
                {
                    MyDefine.myXET.compyPhone = textBox6.Text;
                    MyDefine.myXET.saveReportInfo();
                }

                if (ret)
                {
                    MyDefine.myXET.AddTraceInfo("保存成功");
                    warning_NI("设置成功！", Color.Green);
                    int num = 0;
                    if (textBox3.Text.Length > 3 && int.TryParse(textBox3.Text.Substring(textBox3.Text.Length - 3), out num))
                    {
                        textBox3.Text = textBox3.Text.Substring(0, textBox3.Text.Length - 3) + (num + 1).ToString().PadLeft(3,'0');
                    }
                    else if(int.TryParse(textBox3.Text, out num))
                    {
                        textBox3.Text = (num + 1).ToString().PadLeft(3, '0');
                    }

                }
            }
            catch (Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("保存失败：" + ex.ToString());
            }
            finally
            {
                MyDefine.myXET.meTask = TASKS.run;      //设置串口为空闲状态
            }
        }

        #endregion

        #region 设置设备名称

        /// <summary>
        /// 设置设备名称
        /// </summary>
        /// <returns></returns>
        private Boolean SetDeviceName()
        {
            try
            {
                //设置设备地址
                if (textBox7.Text != "")
                {
                    string name = textBox7.Text.Trim();
                    //设备名称已变更，设置设备名称
                    //if (name != MyDefine.myXET.meName)
                    //{
                        MyDefine.myXET.AddTraceInfo("设置设备地址:" + MyDefine.myXET.meActiveAddr + "->" + name);
                        Boolean ret = MyDefine.myXET.setName(name);
                        if (ret)
                        {
                            MyDefine.myXET.meName = name;
                            MyDefine.myXET.AddTraceInfo("设置成功");
                        }
                        else
                        {
                            MyDefine.myXET.AddTraceInfo("设置失败");
                            warning_NI("设备地址设置失败！", Color.Red);
                            return false;
                        }

                    //}
                }

                return true;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("设备地址设置失败：" + ex.ToString());
                return false;
            }
        }

        #endregion

        #region 设置设备地址

        /// <summary>
        /// 设置设备地址
        /// </summary>
        /// <returns></returns>
        private Boolean SetDeviceAddr()
        {
            try
            {
                //设置设备地址
                if (textBox1.Text != "")
                {
                    int address = 0;
                    Byte myAddress = 0;
                    if (int.TryParse(textBox1.Text, out address) == false)
                    {
                        MyDefine.myXET.AddTraceInfo(textBox1.Text + " 地址错误");
                        warning_NI("设备地址错误，正确地址为1-255！", Color.Red);
                        return false;
                    }

                    if (address < 1 || address > 255)
                    {
                        MyDefine.myXET.AddTraceInfo(address + " 地址错误");
                        warning_NI("设备地址错误，正确地址为1-255！", Color.Red);
                        return false;
                    }
                    else
                    {
                        myAddress = (Byte)address;
                    }

                    //设备地址已变更，设置设备地址
                    //if (myAddress != MyDefine.myXET.meActiveAddr)
                    //{
                        MyDefine.myXET.AddTraceInfo("设置设备地址:" + MyDefine.myXET.meActiveAddr + "->" + myAddress);
                        Boolean ret = MyDefine.myXET.setDevice(myAddress);
                        if (ret)
                        {
                            MyDefine.myXET.meActiveAddr = myAddress;
                            MyDefine.myXET.meDUTAddrArr[MyDefine.myXET.meActiveIdx] = myAddress;      //将修改后的地址更新至地址数组中
                            MyDefine.myXET.AddTraceInfo("设置成功");
                        }
                        else
                        {
                            MyDefine.myXET.AddTraceInfo("设置失败");
                            warning_NI("设备地址设置失败！", Color.Red);
                            return false;
                        }
                        
                    //}
                }

                return true;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("设备地址设置失败：" + ex.ToString());
                return false;
            }
        }

        #endregion

        #region 设置出厂编号

        /// <summary>
        /// 设置出厂编号:“设备型号,出厂编号,测量范围”
        /// 正确格式如“YZLT30,TH2101001,(-40~125)℃/(0~100)%RH”
        /// </summary>
        /// <returns></returns>
        private Boolean SetFactoryJSN()
        {
            if (textBox2.Text == "" && textBox3.Text == "" && textBox4.Text == "") return true;

            try
            {
                //出厂编号格式：“YZLT30,TT2101001,-80~150℃
                string newJsn = "";
                newJsn += ((textBox2.Text == "") ? MyDefine.myXET.meModel : textBox2.Text) + ",";
                switch (MyDefine.myXET.meType)
                {
                    case DEVICE.HTT:
                        newJsn += ((textBox3.Text == "") ? MyDefine.myXET.meJSN : "TT" + textBox3.Text) + ","; 
                        break;
                    case DEVICE.HTP:
                        newJsn += ((textBox3.Text == "") ? MyDefine.myXET.meJSN : "TP" + textBox3.Text) + ","; 
                        break;
                    case DEVICE.HTH:
                        newJsn += ((textBox3.Text == "") ? MyDefine.myXET.meJSN : "TH" + textBox3.Text) + ","; 
                        break;
                    case DEVICE.HTQ:
                        newJsn += ((textBox3.Text == "") ? MyDefine.myXET.meJSN : "TQ" + textBox3.Text) + ","; 
                        break;
                    default:
                        newJsn += ((textBox3.Text == "") ? MyDefine.myXET.meJSN : textBox3.Text) + ","; 
                        break;
                }
                newJsn += (textBox4.Text == "") ? MyDefine.myXET.meRange : textBox4.Text;

                string currJsn = "";
                currJsn += MyDefine.myXET.meModel + ",";
                currJsn += MyDefine.myXET.meJSN + ",";
                currJsn += MyDefine.myXET.meRange;

                //出厂编号已变更，设置出厂编号
                //if (newJsn != currJsn)
                //{
                    MyDefine.myXET.AddTraceInfo("设置出厂编号:" + currJsn + "->" + newJsn);
                    Boolean ret = MyDefine.myXET.setJSN(newJsn);
                    if (ret)
                    {
                        MyDefine.myXET.meJSN = textBox3.Text;
                        MyDefine.myXET.UpdateLabelJSN(MyDefine.myXET.meActiveIdx, MyDefine.myXET.meJSN);        //更新Main界面上的设备编号
                        MyDefine.myXET.AddTraceInfo("设置成功");
                    }
                    else
                    {
                        MyDefine.myXET.AddTraceInfo("设置失败");
                        warning_NI("出厂编号设置失败！", Color.Red);
                        return false;
                    }
                //}

                return true;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("出厂编号设置失败：" + ex.ToString());
                return false;
            }
        }

        #endregion

        #region 设置校准时间

        //更新计量校准时间
        public Boolean SetJincalTime()
        {
            Boolean ret = true;
                        
            if (textBox5.Text != string.Empty)
            {
                DateTime newDateCali;
                if (DateTime.TryParse(textBox5.Text, out newDateCali) == false)
                {
                    MyDefine.myXET.AddTraceInfo(textBox5.Text + "校准日期非时间格式！");
                    warning_NI("校准日期非时间格式！", Color.Red);
                    return false;
                }

                //if (newDateCali.CompareTo(MyDefine.myXET.meDateCal) != 0)               //计量校准时间发生变化，询问是否更新计量校准时间
                //{
                    //先读取
                    MyDefine.myXET.meTips += "Read_Time:" + Environment.NewLine;
                    MyDefine.myXET.AddTraceInfo("读取校准时间");
                    ret = MyDefine.myXET.readTime();                //读取硬件时间、标定时间等信息
                    if (!ret)
                    {
                        MyDefine.myXET.AddTraceInfo("读取失败");
                        warning_NI("校准时间读取失败！", Color.Red);
                        //return false;
                    }
                    else
                    {
                        MyDefine.myXET.AddTraceInfo("读取成功");
                    }

                    //后设置
                    MyDefine.myXET.meTips += "SET_Time:" + Environment.NewLine;
                    MyDefine.myXET.AddTraceInfo("设置校准时间:" + MyDefine.myXET.meDateCal.ToString() + "->" + newDateCali.ToString());
                    MyDefine.myXET.meDateCal = newDateCali;         //将新的校准时间赋给计量校准日期变量
                    ret = MyDefine.myXET.setTime();                 //更新计量校准时间
                    if (!ret)
                    {
                        MyDefine.myXET.AddTraceInfo("设置失败");
                        warning_NI("校准时间设置失败！", Color.Red);
                        return false;
                    }
                    else
                    {
                        MyDefine.myXET.AddTraceInfo("设置成功");
                    }
                //}
            }

            return ret;
        }

        #endregion

        #region 更新当前设置

        /// <summary>
        /// 更新当前设置
        /// </summary>
        private void ShowCurrentSettings()
        {
            try
            {
                MyDefine.myXET.AddTraceInfo("读取"); 
                warning_NI("读取中...", Color.Black);
                MyDefine.myXET.meTips += "readJSN:" + Environment.NewLine;
                MyDefine.myXET.AddTraceInfo("读取出厂编号");
                textBox1.Text = MyDefine.myXET.meActiveAddr.ToString();
                bool ret = MyDefine.myXET.readJSN();                     //读取设备编号
                if (!ret)
                {
                    MyDefine.myXET.AddTraceInfo("读取失败");
                    warning_NI("出厂编号读取失败！", Color.Red);
                }
                else
                {
                    MyDefine.myXET.AddTraceInfo("读取成功");
                    textBox2.Text = MyDefine.myXET.meModel;
                    textBox3.Text = MyDefine.myXET.meJSN.Length > 2 ? MyDefine.myXET.meJSN.Substring(2) : MyDefine.myXET.meJSN;
                    if (MyDefine.myXET.meRange == "")
                    {
                        switch (MyDefine.myXET.meType)
                        {
                            case DEVICE.HTT:
                                textBox4.Text = "(-90~150)℃";
                                break;
                            case DEVICE.HTH:
                                textBox4.Text = "(-30~70)℃ (0~100)%RH";
                                break;
                            case DEVICE.HTP:
                                textBox4.Text = "(0~600)KPa";
                                break;
                            case DEVICE.HTQ:
                                textBox4.Text = "(-40~85)℃ (0~100)%RH";
                                break;
                            default:
                                textBox4.Text = "";
                                break;
                        }
                    }
                    else
                    {
                        textBox4.Text = MyDefine.myXET.meRange;
                    }
                }

                if (ret)
                {
                    MyDefine.myXET.meTips += "readTime:" + Environment.NewLine;
                    ret = MyDefine.myXET.readTime();
                    if (!ret)
                    {
                        MyDefine.myXET.AddTraceInfo("读取失败");
                        warning_NI("校准日期读取失败！", Color.Red);
                    }
                    else
                    {
                        MyDefine.myXET.AddTraceInfo("读取成功");
                        textBox5.Text = MyDefine.myXET.meDateCal.ToString("yyyy-MM-dd");
                    }
                }

                if (ret)
                {
                    MyDefine.myXET.meTips += "readName:" + Environment.NewLine;
                    ret = MyDefine.myXET.readName();
                    if (!ret)
                    {
                        MyDefine.myXET.AddTraceInfo("读取失败");
                        textBox7.Enabled = false;
                        textBox7.Text = ((PPRODUCT)MyDefine.myXET.meType).ToString();
                        ret = true;
                    }
                    else
                    {
                        MyDefine.myXET.AddTraceInfo("读取成功");
                        textBox7.Text = MyDefine.myXET.meName;
                    }
                }

                textBox6.Text = MyDefine.myXET.compyPhone;
                if (ret) warning_NI("读取成功！", Color.Green);
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("读取失败：" + ex.ToString());
            }
        }

        #endregion

        #region 提示信息

        //报警提示
        private void warning_NI(string meErr, Color myColor)
        {
            label3.ForeColor = myColor;
            label3.Text = meErr;
            label3.Visible = true;
        }

        #endregion

        #region 文本框输入限制

        /// <summary>
        /// 判断十六进制字符串hex是否正确
        /// </summary>
        /// <param name="hex">十六进制字符串</param>
        /// <returns>true：不正确，false：正确</returns>
        public bool IsIllegalHexadecimal(string hex)
        {
            string PATTERN = @"([^A-Fa-f0-9]|\s+?)+";
            return !System.Text.RegularExpressions.Regex.IsMatch(hex, PATTERN);
        }

        /// <summary>
        /// 文本框输入限制(禁止输入中文及中文符号)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txt_KeyPress(object sender, KeyPressEventArgs e)
        {
            int mycharNum = System.Text.Encoding.GetEncoding("GBK").GetBytes(e.KeyChar.ToString()).Length;
            if (mycharNum > 1)          //禁止输入中文及中文符号
            {
                //MessageBox.Show("请输入英文字符！");
                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// 文件名规则
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void File_KeyPress(object sender, KeyPressEventArgs e)
        {
            //不可以有以下特殊字符
            // \/:*?"<>|
            // \\
            // \|
            // ""
            Regex meRgx = new Regex(@"[\\/:*?""<>\|]");
            if (meRgx.IsMatch(e.KeyChar.ToString()))
            {
                e.Handled = true;
            }
        }

        #endregion

        private void MenuFactoryForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MyDefine.myXET.meInterface = "系统设置";
        }

        #region 文件的导入导出

        /// <summary>
        /// 导出设备信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            //没有文件夹则创建
            if (!Directory.Exists(MyDefine.myXET.userSAV))
            {
                Directory.CreateDirectory(MyDefine.myXET.userSAV);
            }

            MyDefine.myXET.AddTraceInfo("导出设备配置");
            //保存设备名称： 设备型号.输入的名称_时间（年月日）
            String name = textBox2.Text + "." + textBox8.Text + "_" + System.DateTime.Now.ToString("yyyyMMdd");
            String meString = MyDefine.myXET.userSAV + "\\" + name + ".txt";      //保存在固定文件内
                                                                                  //覆盖
            if (File.Exists(meString))
            {
                if (MessageBox.Show("是否覆盖已有配置", "", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                {
                    SaveToCfg(name);
                }
            }
            //新增
            else
            {
                SaveToCfg(name);
                //
                listBox1.Items.Add(name);
            }
        }

        /// <summary>
        /// 选择不同文件导入文件数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(listBox1.SelectedItem != null)
            {
                String meString = MyDefine.myXET.userSAV + "\\" + listBox1.SelectedItem.ToString() + ".txt";
                if (File.Exists(meString))
                {
                    MyDefine.myXET.AddTraceInfo("导入配置文件：" + listBox1.SelectedItem.ToString());
                    if (!LoadFromCfg(listBox1.SelectedItem.ToString()))
                    {
                        MyDefine.myXET.AddTraceInfo("导入配置文件失败");
                        MessageBox.Show("导入失败");
                    }

                    MyDefine.myXET.AddTraceInfo("导入配置文件成功");
                }
            }
        }

        /// <summary>
        /// 删除设备信息文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                if (MessageBox.Show("是否删除选中配置", "", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                {
                    String meString = MyDefine.myXET.userSAV + "\\" + listBox1.SelectedItem.ToString() + ".txt";

                    MyDefine.myXET.AddTraceInfo("删除配置文件" + listBox1.SelectedItem.ToString());
                    if (File.Exists(meString))
                    {
                        File.Delete(meString);
                    }
                    listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                }
            }
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="myXET"></param>
        /// <param name="meName"></param>
        /// <returns></returns>
        public bool SaveToCfg(String meName)
        {
            //空
            if (MyDefine.myXET.userSAV == null)
            {
                return false;
            }
            //创建新路径
            else if (!Directory.Exists(MyDefine.myXET.userSAV))
            {
                Directory.CreateDirectory(MyDefine.myXET.userSAV);
            }

            //写入
            try
            {
                String filePath = MyDefine.myXET.userSAV + "\\" + meName + ".txt";
                FileStream meFS = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                TextWriter meWrite = new StreamWriter(meFS);
                //
                if(textBox7.Text != "")meWrite.WriteLine("设备名称=" + textBox7.Text);
                if(textBox1.Text != "")meWrite.WriteLine("设备地址=" + textBox1.Text);
                if(textBox2.Text != "")meWrite.WriteLine("设备型号=" + textBox2.Text);
                if(textBox3.Text != "")meWrite.WriteLine("出厂编号=" + textBox3.Text);
                if(textBox4.Text != "")meWrite.WriteLine("测量范围=" + textBox4.Text);
                if(textBox5.Text != "")meWrite.WriteLine("校准日期=" + textBox5.Text);
                if(textBox6.Text != "") meWrite.WriteLine("公司电话=" + textBox6.Text);
                //
                meWrite.Close();
                meFS.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 加载配置文档
        /// </summary>
        /// <param name="meName"></param>
        /// <returns></returns>
        public bool LoadFromCfg(String meName)
        {
            //空
            if (MyDefine.myXET.userSAV == null)
            {
                return false;
            }
            //创建新路径
            else if (!Directory.Exists(MyDefine.myXET.userSAV))
            {
                Directory.CreateDirectory(MyDefine.myXET.userSAV);
            }

            //配置文件
            String filePath = MyDefine.myXET.userSAV + "\\" + meName + ".txt";
            if (File.Exists(filePath))
            {
                String[] lines = File.ReadAllLines(filePath);
                if(lines.Length > 0)
                {
                    foreach(string line in lines)
                    {
                        switch(line.Substring(0, line.IndexOf('=')))
                        {
                             case "设备名称":  textBox7.Text = line.Substring(line.IndexOf('=') + 1); break;
                             case "设备地址":  textBox1.Text = line.Substring(line.IndexOf('=') + 1); break;
                             case "设备型号":  textBox2.Text = line.Substring(line.IndexOf('=') + 1); break;
                             case "出厂编号":  textBox3.Text = line.Substring(line.IndexOf('=') + 1); break;
                             case "测量范围":  textBox4.Text = line.Substring(line.IndexOf('=') + 1); break;
                             case "校准日期":  textBox5.Text = line.Substring(line.IndexOf('=') + 1); break;
                             case "公司电话":  textBox6.Text = line.Substring(line.IndexOf('=') + 1); break;
                        }
                    }
                    return true;
                }
                else
                {
                    MessageBox.Show("导入的文件为空");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        #endregion


    }
}
