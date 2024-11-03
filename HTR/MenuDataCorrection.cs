using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media;

namespace HTR
{
    public partial class MenuDataCorrection : Form
    {
        TextBox[] temp = new TextBox[16];//温度修正数据
        TextBox[] hum = new TextBox[16];//湿度修正数据
        Boolean isTwo = true;           //判断是否要修正两个数据
        Byte realLen = 0;               //实际接收数据长度
        int data_min = 0;
        int data_max = 0;

        public MenuDataCorrection()
        {
            InitializeComponent();
        }

        #region 窗体事件

        private void MenuDataCorrection_Load(object sender, EventArgs e)
        {
            //加载控件
            foreach(Control ct in this.Controls)
            {
                if (ct.Name.Contains("groupBox1"))
                {
                    foreach(Control ct1 in ct.Controls)
                    {
                        string ctName = ct1.Name;
                        if (ctName.Contains("textBox"))
                        {
                            ctName = ctName.Replace("textBox", "");
                            temp[Convert.ToInt32(ctName) - 1] = (TextBox)ct1;
                        }
                    }
                }
                else if (ct.Name.Contains("groupBox2"))
                {
                    foreach (Control ct1 in ct.Controls)
                    {
                        string ctName = ct1.Name;
                        if (ctName.Contains("textBox"))
                        {
                            ctName = ctName.Replace("textBox", "");
                            hum[Convert.ToInt32(ctName) - 17] = (TextBox)ct1;
                        }
                    }
                }
            }

            groupBox1.Text = "温度修正（℃）";
            switch (MyDefine.myXET.meType)
            {
                case DEVICE.HTT:
                    groupBox2.Visible = false;
                    isTwo = false;
                    realLen = 0x21;
                    data_max = 150;
                    data_min = -90;
                    break;
                case DEVICE.HTH:
                    groupBox2.Visible = true;
                    isTwo = true;
                    realLen = 0x42;
                    break;
                case DEVICE.HTP:
                    groupBox2.Visible = false;
                    groupBox1.Text = "压力修正（KPa）";
                    isTwo = false;
                    realLen = 0x21;
                    data_max = 600;
                    data_min = 8;
                    break;
                case DEVICE.HTQ:
                    groupBox2.Visible = true;
                    isTwo = true;
                    realLen = 0x42;
                    data_min = -40;
                    data_max = 85;
                    break;
            }

            //初始化控件
            set_TextBox();

            MyDefine.myXET.meInterface = "数据修正";
        }

        private void MenuDataCorrection_FormClosed(object sender, FormClosedEventArgs e)
        {
            MyDefine.myXET.meInterface = "连接界面";
        }

        #endregion
        
        #region 按钮事件

        //确定按钮
        private void button1_Click(object sender, EventArgs e)
        {
            Boolean ret;
            string data;
            Decimal meData;
            MyDefine.myXET.humCount = 0;
            MyDefine.myXET.tempCount = 0;
            //核对修正温度的数量和格式是否正确
            for (int i = 0; i < 16; i++)
            {
                if (temp[i].Text != null && temp[i].Text != "")
                {
                    data = temp[i].Text;                      //

                    if (Decimal.TryParse(data, out meData) == false)   //标准温度值非数字
                    {
                        temp[i].SelectAll();
                        MyDefine.myXET.ShowWrongMsg("修正失败：数据 \"" + data + "\" 格式错误！" + Environment.NewLine + data);
                        MyDefine.myXET.meTask = TASKS.run;
                        return;
                    }
                    if (meData >= data_min && meData <= data_max)
                    {
                        MyDefine.myXET.meData_Cor[i] = (Int32)(meData * 100);
                        MyDefine.myXET.tempCount++;
                        if(i % 2 == 1) MyDefine.myXET.AddTraceInfo("保存设备修正:" + MyDefine.myXET.meData_Cor[i - 1] + "->" + MyDefine.myXET.meData_Cor[i]);
                    }
                    else
                    {
                        temp[i].SelectAll();
                        MyDefine.myXET.ShowWrongMsg("修正失败：数据 \"" + data + "\" 超出范围！" + Environment.NewLine + data);
                    }
                }
                else
                {
                    MyDefine.myXET.meData_Cor[i] = 0;
                }
            }

            for (int i = 0; i < MyDefine.myXET.tempCount - 2; i += 2)
            {
                int x1 = MyDefine.myXET.meData_Cor[i];
                int x2 = MyDefine.myXET.meData_Cor[i + 2];
                if (x1 >= x2)       //温度点顺序排列错误
                {
                    MyDefine.myXET.ShowWrongMsg("修正失败：数据必须从小到大修正！" + Environment.NewLine);
                    MyDefine.myXET.meTask = TASKS.run;
                    return;
                }
            }

            if (isTwo)
            {
                //核对修正湿度的数量和格式是否正确
                for (int i = 0; i < 16; i++)
                {
                    if (hum[i].Text != null && hum[i].Text != "")
                    {
                        data = hum[i].Text;                      //

                        if (Decimal.TryParse(data, out meData) == false)   //标准温度值非数字
                        {
                            hum[i].SelectAll();
                            MyDefine.myXET.ShowWrongMsg("修正失败：数据 \"" + data + "\" 格式错误！" + Environment.NewLine + data);
                            MyDefine.myXET.meTask = TASKS.run;
                            return;
                        }
                        if (meData >= 0 && meData <= 100)
                        {
                            MyDefine.myXET.meData_Cor[i + 16] = (Int32)(meData * 100);
                            MyDefine.myXET.humCount++;
                            if (i % 2 == 1) MyDefine.myXET.AddTraceInfo("保存设备修正:" + MyDefine.myXET.meData_Cor[i - 1] + "->" + MyDefine.myXET.meData_Cor[i]);
                        }
                        else
                        {
                            hum[i].SelectAll();
                            MyDefine.myXET.ShowWrongMsg("修正失败：数据 \"" + data + "\" 超出范围！" + Environment.NewLine + data);
                        }
                    }
                    else
                    {
                        MyDefine.myXET.meData_Cor[i + 16] = 0;
                    }
                }

                //核对修正温度的温度点顺序是否正确：修正温度点必须从小到大排列，如-80，-50，-20，5，25
                for (int i = 0; i < MyDefine.myXET.humCount - 2; i += 2)
                {

                    if (isTwo)
                    {
                        int x1 = MyDefine.myXET.meData_Cor[i + 16];
                        int x2 = MyDefine.myXET.meData_Cor[i + 16 + 2];
                        if (x1 >= x2)       //湿度点顺序排列错误
                        {
                            MyDefine.myXET.ShowWrongMsg("修正失败：数据必须从小到大修正！" + Environment.NewLine);
                            MyDefine.myXET.meTask = TASKS.run;
                            return;
                        }
                    }
                }
            }

            MyDefine.myXET.meTips += "SET_CORDATA:" + Environment.NewLine;
            ret = MyDefine.myXET.setCorData(realLen);//数据修正

            if (!ret)
            {
                MyDefine.myXET.AddTraceInfo("保存数据修正失败");
                MyDefine.myXET.ShowWrongMsg("保存数据修正失败！");
            }

            if (ret)
            {
                MyDefine.myXET.AddTraceInfo("保存数据修正成功");
                MyDefine.myXET.AddTraceInfo("保存数据修正成功！");
            }

            MyDefine.myXET.SaveCommunicationTips();                //将调试信息保存到操作日志
            this.Close();
        }

        //取消按钮
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion

        #region 文本框事件

        //初始化文本框

        private void set_TextBox()
        {
            bool ret;
            MyDefine.myXET.meTips += "readCorData:" + Environment.NewLine;
           
            ret = MyDefine.myXET.readCorData(realLen);                 //读取设备数据校准

            //初始化文本框
            for(int i = 0; i < MyDefine.myXET.tempCount * 2; i++)
            {
                temp[i].Text = (MyDefine.myXET.meData_Cor[i] / 100.0).ToString();
            }
            for(int j = 0; j < MyDefine.myXET.humCount * 2; j++)
            {
                hum[j].Text = (MyDefine.myXET.meData_Cor[j + 16] / 100.0).ToString();
            }

            //初始化修正个数
            MyDefine.myXET.tempCount = 0;
            MyDefine.myXET.humCount = 0;
        }

        //文本框输入事件
        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            //只允许输入数字和删除键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != 8) && (e.KeyChar != 45) && (e.KeyChar != 46))
            {
                e.Handled = true;
                return;
            }

            //长度限制
            if (((TextBox)sender).Text.Length > 6)
            {
                e.Handled = true;
                return;
            }
        }

        #endregion
    }
}
