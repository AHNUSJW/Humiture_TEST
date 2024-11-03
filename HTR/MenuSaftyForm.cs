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
    public partial class MenuSaftyForm : Form
    {
        public MenuSaftyForm()
        {
            InitializeComponent();
        }

        private void MenuSaftyForm_Load(object sender, EventArgs e)
        {
            MyDefine.myXET.meInterface = "安全设置";
            updateDisplay();                        //更新界面信息
        }

        //恢复出厂设置
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!MyDefine.myXET.checkDeviceStatus()) return;    //设备未连接或繁忙
                button1.Text = "设置中...";
                Application.DoEvents();
                MyDefine.myXET.meTask = TASKS.setting;  //设置串口为工作中
                MyDefine.myXET.AddTraceInfo("恢复出厂设置");

                MyDefine.myXET.setREG_BAT_CL();         //写寄存器：清除重上电标志
                MyDefine.myXET.setREG_CTEMP_CL();       //写寄存器：清除设备最高、最低温度
                MyDefine.myXET.readDevice();            //读寄存器：读取重上电标志、设备最高、最低温度
                updateDisplay();                        //更新界面信息

                button1.Text = "恢复出厂设置";
                Application.DoEvents();
                MyDefine.myXET.meTask = TASKS.run;      //设置串口为空闲状态
                MyDefine.myXET.AddTraceInfo("恢复出厂设置成功");
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("恢复出厂设置失败：" + ex.ToString());
            }            
        }

        private void updateDisplay()
        {
            label11.Text = MyDefine.myXET.meJSN == "" ? "-" : MyDefine.myXET.meJSN;
            label12.Text = MyDefine.myXET.meHWVer == "" ? "-" : (MyDefine.myXET.meHWVer + MyDefine.myXET.meSWVer);

            label2.Text = MyDefine.myXET.meRTC == 1 ? "是" : "否";
            label4.Text = MyDefine.myXET.meHTT == 1 ? "是" : "否";
            label6.Text = (MyDefine.myXET.meTMax / 100.0).ToString("F2") + "℃";
            label8.Text = (MyDefine.myXET.meTMin / 100.0).ToString("F2") + "℃";
        }

        private void MenuSaftyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MyDefine.myXET.meInterface = "系统设置";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!MyDefine.myXET.checkDeviceStatus()) return;    //设备未连接或繁忙

            int colNum = 0;                         //实际需要标定的列数(点数)
            if (MyDefine.myXET.meType == DEVICE.HTT) colNum = 8;                   //温度采集器
            if (MyDefine.myXET.meType == DEVICE.HTP) colNum = 3;                   //压力采集器
            if (MyDefine.myXET.meType == DEVICE.HTQ) colNum = 16;

            if(colNum == 0)
            {
                MessageBox.Show("当前设备无标定功能！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //温度采集器出厂标定数据
            if (colNum == 8)
            {
                MyDefine.myXET.meTemp_CalPoints[0] = -8000;
                MyDefine.myXET.meTemp_CalPoints[1] = -5000;
                MyDefine.myXET.meTemp_CalPoints[2] = -2000;
                MyDefine.myXET.meTemp_CalPoints[3] = 500;
                MyDefine.myXET.meTemp_CalPoints[4] = 2000;
                MyDefine.myXET.meTemp_CalPoints[5] = 6000;
                MyDefine.myXET.meTemp_CalPoints[6] = 9000;
                MyDefine.myXET.meTemp_CalPoints[7] = 12000;

                MyDefine.myXET.meADC_CalPoints[0] = 56934;
                MyDefine.myXET.meADC_CalPoints[1] = 66751;
                MyDefine.myXET.meADC_CalPoints[2] = 76567;
                MyDefine.myXET.meADC_CalPoints[3] = 84748;
                MyDefine.myXET.meADC_CalPoints[4] = 91127;
                MyDefine.myXET.meADC_CalPoints[5] = 102203;
                MyDefine.myXET.meADC_CalPoints[6] = 111696;
                MyDefine.myXET.meADC_CalPoints[7] = 121190;
            }

            //压力采集器出厂标定数据
            if (colNum == 3)
            {
                MyDefine.myXET.meTemp_CalPoints[0] = 800;
                MyDefine.myXET.meTemp_CalPoints[1] = 10133;
                MyDefine.myXET.meTemp_CalPoints[2] = 60000;

                MyDefine.myXET.meADC_CalPoints[0] = 2230;
                MyDefine.myXET.meADC_CalPoints[1] = 18312;
                MyDefine.myXET.meADC_CalPoints[2] = 104243;
            }

            //温湿度采集器出厂标定数据
            if(colNum == 16)
            {
                //温度
                MyDefine.myXET.meTemp_CalPoints[0] = -4000;
                MyDefine.myXET.meTemp_CalPoints[1] = -2500;
                MyDefine.myXET.meTemp_CalPoints[2] = -1000;
                MyDefine.myXET.meTemp_CalPoints[3] = 500;
                MyDefine.myXET.meTemp_CalPoints[4] = 2500;
                MyDefine.myXET.meTemp_CalPoints[5] = 4500;
                MyDefine.myXET.meTemp_CalPoints[6] = 6500;
                MyDefine.myXET.meTemp_CalPoints[7] = 8500;

                MyDefine.myXET.meADC_CalPoints[0] = -4000;
                MyDefine.myXET.meADC_CalPoints[1] = -2500;
                MyDefine.myXET.meADC_CalPoints[2] = -1000;
                MyDefine.myXET.meADC_CalPoints[3] = 500;
                MyDefine.myXET.meADC_CalPoints[4] = 2500;
                MyDefine.myXET.meADC_CalPoints[5] = 4500;
                MyDefine.myXET.meADC_CalPoints[6] = 6500;
                MyDefine.myXET.meADC_CalPoints[7] = 8500;

                //湿度
                MyDefine.myXET.meHum_CalPoints[0] = 500;
                MyDefine.myXET.meHum_CalPoints[1] = 2000;
                MyDefine.myXET.meHum_CalPoints[2] = 3500;
                MyDefine.myXET.meHum_CalPoints[3] = 4500;
                MyDefine.myXET.meHum_CalPoints[4] = 5500;
                MyDefine.myXET.meHum_CalPoints[5] = 6500;
                MyDefine.myXET.meHum_CalPoints[6] = 8000;
                MyDefine.myXET.meHum_CalPoints[7] = 9500;

                MyDefine.myXET.meADC1_CalPoints[0] = 500;
                MyDefine.myXET.meADC1_CalPoints[1] = 2000;
                MyDefine.myXET.meADC1_CalPoints[2] = 3500;
                MyDefine.myXET.meADC1_CalPoints[3] = 4500;
                MyDefine.myXET.meADC1_CalPoints[4] = 5500;
                MyDefine.myXET.meADC1_CalPoints[5] = 6500;
                MyDefine.myXET.meADC1_CalPoints[6] = 8000;
                MyDefine.myXET.meADC1_CalPoints[7] = 9500;
            }
            //恢复出厂标定
            Boolean ret = true;
            MyDefine.myXET.meTask = TASKS.setting;  //设置串口为工作中
            MyDefine.myXET.meTips = "[RECOVER FACTORY CALIBRATION]" + Environment.NewLine;
            MyDefine.myXET.AddTraceInfo("恢复出厂标定");

            try
            {
                if (ret)
                {
                    MyDefine.myXET.meTips += "Read_Time:" + Environment.NewLine;
                    ret = MyDefine.myXET.readTime();                //读取硬件时间、标定时间等信息
                }

                if (ret)
                {
                    MyDefine.myXET.meTips += "SET_Time:" + Environment.NewLine;
                    MyDefine.myXET.meDateDot = DateTime.Now;        //将当前系统时间赋给标定日期变量
                    ret = MyDefine.myXET.setTime();                 //更新设备标定时间
                }

                if (ret)
                {
                    MyDefine.myXET.meTips += "SET_DOT:" + Environment.NewLine;
                    ret = MyDefine.myXET.setDOT(colNum);            //设备标定
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
                ret = false;
            }

            if (!ret) MyDefine.myXET.ShowWrongMsg("恢复出厂标定失败！");
            if (ret) MyDefine.myXET.ShowCorrectMsg("恢复出厂标定成功！");
            MyDefine.myXET.meTask = TASKS.run;
        }
    }
}
