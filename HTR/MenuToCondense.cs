using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace HTR
{
    public partial class MenuToCondense : Form
    {
        public bool IsSave = false;
        public int maxTime;
        public bool updateTime = false; //是否更新主界面工作开始时间
        public MenuToCondense()
        {
            InitializeComponent();
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            Int32 dat;
            dat = Convert.ToInt32(textBox1.Text.Trim());
            if (dat < 0)
            {
                MessageBox.Show("设置的时间不能小于0", "提示");
            }
            else if (dat > maxTime)
            {
                if(maxTime != 150)
                {
                    MessageBox.Show("设置的时间最大为" + maxTime + ",若要设置的时间为" + dat + ",请先停止工作", "提示");
                }
                else
                {
                    MessageBox.Show("设置的时间最大只能为150", "提示");
                }
                dat = maxTime;
            }
            label1.Text = dat.ToString();
            textBox1.Visible = false;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            //只允许输入数字和删除键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != 8))
            {
                e.Handled = true;
                return;
            }

            //长度限制
            if (((System.Windows.Forms.TextBox)sender).Text.Length > 4)
            {
                e.Handled = true;
                return;
            }
        }

        private void label1_DoubleClick(object sender, EventArgs e)
        {
            textBox1.Text = label1.Text;
            textBox1.Text = string.Empty;
            textBox1.Visible = true;
            textBox1.BringToFront();
            textBox1.Focus();
        }
        #region 启动去冷凝
        private void button8_Click(object sender, EventArgs e)
        {
            if (MyDefine.myXET.meType == DEVICE.HTQ)
            {
                Int32 dat;
                Boolean ret;
                dat = Convert.ToInt32(label1.Text.Trim());
                //if (dat == 0) this.Close();
                //if (dat > maxTime) dat = maxTime;
                MyDefine.myXET.AddTraceInfo("设置去冷凝时长为" + dat.ToString());
                MyDefine.myXET.condenseTime = (Byte)dat;
                ret = MyDefine.myXET.setREG_TIME_CONDENSE();       //去冷凝
                if (ret) MyDefine.myXET.AddTraceInfo("设置去冷凝成功");
                else MyDefine.myXET.AddTraceInfo("设置去冷凝失败");
                IsSave = true;
            }
            else
            {
                MessageBox.Show("设备类型不匹配");
            }
            this.Close();
        }

        #endregion

        #region 取消设置
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion

        #region 启动加载
        private void MenuToCondense_Load(object sender, EventArgs e)
        {
            Boolean ret = true;
            //更新设备工作状态
            int status1, status2;
            //读取开始/结束/间隔时间/时间单位/采样条数
            ret = MyDefine.myXET.readJobset();
            if (!ret) MyDefine.myXET.ShowWrongMsg("参数读取失败：A04");
            else
            {
                
                //status1 = MyDefine.myXET.meDateStart.CompareTo(DateTime.Now);
                //status2 = MyDefine.myXET.meDateEnd.CompareTo(DateTime.Now);
                status1 = MyDefine.myXET.meDateStart.CompareTo(MyDefine.myXET.meDateCalendar);  //将开始结束时间与设备时间做比较(注人为停止设备后，设备时间为日期最大值)
                status2 = MyDefine.myXET.meDateEnd.CompareTo(MyDefine.myXET.meDateCalendar);    //将开始结束时间与设备时间做比较(注人为停止设备后，设备时间为日期最大值)
                if ((status1 < 0 && status2 < 0))   //空闲中
                {
                    maxTime = 150;
                    updateTime = true;
                }
                else if ((status1 > 0 && status2 > 0))  //准备中
                {
                    //已设置好开始结束时间，等待开始
                    TimeSpan ts = MyDefine.myXET.meDateStart.Subtract(DateTime.Now);
                    maxTime = (int)ts.TotalMinutes - 2;
                    if(maxTime < 0)
                    {
                        DialogResult result = MessageBox.Show("请先停止工作再去冷凝", "提示", MessageBoxButtons.OK);
                        if (result == DialogResult.OK)
                        {
                            this.Close();
                        }
                    }
                    else if(maxTime > 150)
                    {
                        maxTime = 150;
                    }
                    updateTime = false;
               }
                else if ((status1 < 0 && status2 > 0))  //记录中
                {
                    DialogResult result = MessageBox.Show("请先停止工作再去冷凝", "提示", MessageBoxButtons.OK);
                    if (result == DialogResult.OK)
                    {
                        this.Close();
                    }
                }
            }

            MyDefine.myXET.meInterface = "去冷凝设置";
        }

        #endregion

        #region 关闭界面
        private void MenuToCondense_FormClosing(object sender, FormClosingEventArgs e)
        {
            MyDefine.myXET.meInterface = "连接界面";
        }
        #endregion


        #region 停止去冷凝
        private void button2_Click(object sender, EventArgs e)
        {
            MyDefine.myXET.AddTraceInfo("停止去冷凝");
            MyDefine.myXET.condenseTime = (Byte)0x00;
            MyDefine.myXET.setREG_TIME_CONDENSE();       //去冷凝
        }
        #endregion

    }
}
