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
    public partial class MenuSETValForm : Form
    {
        public MenuSETValForm()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Boolean ret = true;
            int Pn = MyDefine.myXET.meValidStageNum;              //有效阶段数量
            int tmpNum = MyDefine.myXET.meTMPNum;                 //温度列数量
            int humNum = MyDefine.myXET.meHUMNum;                 //湿度列数量
            int prsNum = MyDefine.myXET.mePRSNum;                 //压力列数量
            MyDefine.myXET.meTmpSETList.Clear();
            MyDefine.myXET.meHumSETList.Clear();
            MyDefine.myXET.mePrsSETList.Clear();

            #region 有效阶段P1设定值

            if (Pn >= 1 && ret == true)
            {
                if (textBox1.Text == "" && textBox2.Text == "" && textBox3.Text == "")
                {
                    MyDefine.myXET.ShowWrongMsg("设定值P1为空！");
                    ret = false;
                }

                try
                {
                    double tmpSET = int.MinValue;                 //温度设定值
                    double humSET = int.MinValue;                 //湿度设定值
                    double prsSET = int.MinValue;                 //压力设定值

                    if (tmpNum > 0) tmpSET = Convert.ToDouble(textBox1.Text);                 //温度设定值
                    if (humNum > 0) humSET = Convert.ToDouble(textBox2.Text);                 //湿度设定值
                    if (prsNum > 0) prsSET = Convert.ToDouble(textBox3.Text);                 //压力设定值
                    if (tmpNum > 0) MyDefine.myXET.meTmpSETList.Add(tmpSET);                 //温度设定值
                    if (humNum > 0) MyDefine.myXET.meHumSETList.Add(humSET);                 //湿度设定值
                    if (prsNum > 0) MyDefine.myXET.mePrsSETList.Add(prsSET);                 //压力设定值
                }
                catch
                {
                    MyDefine.myXET.ShowWrongMsg("设定值P1格式错误或为空！");
                    ret = false;
                }
            }

            #endregion

            #region 有效阶段P2设定值

            if (Pn >= 2 && ret == true)
            {
                if (textBox4.Text == "" && textBox5.Text == "" && textBox6.Text == "")
                {
                    MyDefine.myXET.ShowWrongMsg("设定值P2为空！");
                    ret = false;
                }

                try
                {
                    double tmpSET = int.MinValue;                 //温度设定值
                    double humSET = int.MinValue;                 //湿度设定值
                    double prsSET = int.MinValue;                 //压力设定值

                    if (tmpNum > 0) tmpSET = Convert.ToDouble(textBox4.Text);                 //温度设定值
                    if (humNum > 0) humSET = Convert.ToDouble(textBox5.Text);                 //湿度设定值
                    if (prsNum > 0) prsSET = Convert.ToDouble(textBox6.Text);                 //压力设定值
                    if (tmpNum > 0) MyDefine.myXET.meTmpSETList.Add(tmpSET);                 //温度设定值
                    if (humNum > 0) MyDefine.myXET.meHumSETList.Add(humSET);                 //湿度设定值
                    if (prsNum > 0) MyDefine.myXET.mePrsSETList.Add(prsSET);                 //压力设定值
                }
                catch
                {
                    MyDefine.myXET.ShowWrongMsg("设定值P2格式错误或为空！");
                    ret = false;
                }
            }

            #endregion

            #region 有效阶段P3设定值

            if (Pn >= 3 && ret == true)
            {
                if (textBox7.Text == "" && textBox8.Text == "" && textBox9.Text == "")
                {
                    MyDefine.myXET.ShowWrongMsg("设定值P3为空！");
                    ret = false;
                }

                try
                {
                    double tmpSET = int.MinValue;                 //温度设定值
                    double humSET = int.MinValue;                 //湿度设定值
                    double prsSET = int.MinValue;                 //压力设定值

                    if (tmpNum > 0) tmpSET = Convert.ToDouble(textBox7.Text);                 //温度设定值
                    if (humNum > 0) humSET = Convert.ToDouble(textBox8.Text);                 //湿度设定值
                    if (prsNum > 0) prsSET = Convert.ToDouble(textBox9.Text);                 //压力设定值
                    if (tmpNum > 0) MyDefine.myXET.meTmpSETList.Add(tmpSET);                 //温度设定值
                    if (humNum > 0) MyDefine.myXET.meHumSETList.Add(humSET);                 //湿度设定值
                    if (prsNum > 0) MyDefine.myXET.mePrsSETList.Add(prsSET);                 //压力设定值
                }
                catch
                {
                    MyDefine.myXET.ShowWrongMsg("设定值P3格式错误或为空！");
                    ret = false;
                }
            }

            #endregion

            #region 有效阶段P4设定值

            if (Pn >= 4 && ret == true)
            {
                if (textBox10.Text == "" && textBox11.Text == "" && textBox12.Text == "")
                {
                    MyDefine.myXET.ShowWrongMsg("设定值P4为空！");
                    ret = false;
                }

                try
                {
                    double tmpSET = int.MinValue;                 //温度设定值
                    double humSET = int.MinValue;                 //湿度设定值
                    double prsSET = int.MinValue;                 //压力设定值

                    if (tmpNum > 0) tmpSET = Convert.ToDouble(textBox10.Text);                 //温度设定值
                    if (humNum > 0) humSET = Convert.ToDouble(textBox11.Text);                 //湿度设定值
                    if (prsNum > 0) prsSET = Convert.ToDouble(textBox12.Text);                 //压力设定值
                    if (tmpNum > 0) MyDefine.myXET.meTmpSETList.Add(tmpSET);                 //温度设定值
                    if (humNum > 0) MyDefine.myXET.meHumSETList.Add(humSET);                 //湿度设定值
                    if (prsNum > 0) MyDefine.myXET.mePrsSETList.Add(prsSET);                 //压力设定值
                }
                catch
                {
                    MyDefine.myXET.ShowWrongMsg("设定值P5格式错误或为空！");
                    ret = false;
                }
            }

            #endregion

            #region 有效阶段P5设定值

            if (Pn >= 5 && ret == true)
            {
                if (textBox13.Text == "" && textBox14.Text == "" && textBox15.Text == "")
                {
                    MyDefine.myXET.ShowWrongMsg("设定值P5为空！");
                    ret = false;
                }

                try
                {
                    double tmpSET = int.MinValue;                 //温度设定值
                    double humSET = int.MinValue;                 //湿度设定值
                    double prsSET = int.MinValue;                 //压力设定值

                    if (tmpNum > 0) tmpSET = Convert.ToDouble(textBox13.Text);                 //温度设定值
                    if (humNum > 0) humSET = Convert.ToDouble(textBox14.Text);                 //湿度设定值
                    if (prsNum > 0) prsSET = Convert.ToDouble(textBox15.Text);                 //压力设定值
                    if (tmpNum > 0) MyDefine.myXET.meTmpSETList.Add(tmpSET);                 //温度设定值
                    if (humNum > 0) MyDefine.myXET.meHumSETList.Add(humSET);                 //湿度设定值
                    if (prsNum > 0) MyDefine.myXET.mePrsSETList.Add(prsSET);                 //压力设定值
                }
                catch
                {
                    MyDefine.myXET.ShowWrongMsg("设定值P5格式错误或为空！");
                    ret = false;
                }
            }

            #endregion

            #region 有效阶段P6设定值

            if (Pn >= 6 && ret == true)
            {
                if (textBox16.Text == "" && textBox17.Text == "" && textBox18.Text == "")
                {
                    MyDefine.myXET.ShowWrongMsg("设定值P6为空！");
                    ret = false;
                }

                try
                {
                    double tmpSET = int.MinValue;                 //温度设定值
                    double humSET = int.MinValue;                 //湿度设定值
                    double prsSET = int.MinValue;                 //压力设定值

                    if (tmpNum > 0) tmpSET = Convert.ToDouble(textBox16.Text);                 //温度设定值
                    if (humNum > 0) humSET = Convert.ToDouble(textBox17.Text);                 //湿度设定值
                    if (prsNum > 0) prsSET = Convert.ToDouble(textBox18.Text);                 //压力设定值
                    if (tmpNum > 0) MyDefine.myXET.meTmpSETList.Add(tmpSET);                 //温度设定值
                    if (humNum > 0) MyDefine.myXET.meHumSETList.Add(humSET);                 //湿度设定值
                    if (prsNum > 0) MyDefine.myXET.mePrsSETList.Add(prsSET);                 //压力设定值
                }
                catch
                {
                    MyDefine.myXET.ShowWrongMsg("设定值P6格式错误或为空！");
                    ret = false;
                }
            }

            #endregion

            #region 有效阶段P7设定值

            if (Pn >= 7 && ret == true)
            {
                if (textBox19.Text == "" && textBox20.Text == "" && textBox21.Text == "")
                {
                    MyDefine.myXET.ShowWrongMsg("设定值P7为空！");
                    ret = false;
                }

                try
                {
                    double tmpSET = int.MinValue;                 //温度设定值
                    double humSET = int.MinValue;                 //湿度设定值
                    double prsSET = int.MinValue;                 //压力设定值

                    if (tmpNum > 0) tmpSET = Convert.ToDouble(textBox19.Text);                 //温度设定值
                    if (humNum > 0) humSET = Convert.ToDouble(textBox20.Text);                 //湿度设定值
                    if (prsNum > 0) prsSET = Convert.ToDouble(textBox21.Text);                 //压力设定值
                    if (tmpNum > 0) MyDefine.myXET.meTmpSETList.Add(tmpSET);                 //温度设定值
                    if (humNum > 0) MyDefine.myXET.meHumSETList.Add(humSET);                 //湿度设定值
                    if (prsNum > 0) MyDefine.myXET.mePrsSETList.Add(prsSET);                 //压力设定值
                }
                catch
                {
                    MyDefine.myXET.ShowWrongMsg("设定值P7格式错误或为空！");
                    ret = false;
                }
            }

            #endregion

            #region 有效阶段P8设定值

            if (Pn >= 8 && ret == true)
            {
                if (textBox22.Text == "" && textBox23.Text == "" && textBox24.Text == "")
                {
                    MyDefine.myXET.ShowWrongMsg("设定值P8为空！");
                    ret = false;
                }

                try
                {
                    double tmpSET = int.MinValue;                 //温度设定值
                    double humSET = int.MinValue;                 //湿度设定值
                    double prsSET = int.MinValue;                 //压力设定值

                    if (tmpNum > 0) tmpSET = Convert.ToDouble(textBox22.Text);                 //温度设定值
                    if (humNum > 0) humSET = Convert.ToDouble(textBox23.Text);                 //湿度设定值
                    if (prsNum > 0) prsSET = Convert.ToDouble(textBox24.Text);                 //压力设定值
                    if (tmpNum > 0) MyDefine.myXET.meTmpSETList.Add(tmpSET);                 //温度设定值
                    if (humNum > 0) MyDefine.myXET.meHumSETList.Add(humSET);                 //湿度设定值
                    if (prsNum > 0) MyDefine.myXET.mePrsSETList.Add(prsSET);                 //压力设定值
                }
                catch
                {
                    MyDefine.myXET.ShowWrongMsg("设定值P8格式错误或为空！");
                    ret = false;
                }
            }

            #endregion

            if (ret == true) MyDefine.myXET.ShowCorrectMsg("设定值设定完成！");
        }

        private void MenuSETValForm_Load(object sender, EventArgs e)
        {
            EnableSETGroups(MyDefine.myXET.meValidStageNum);
        }

        public void EnableSETGroups(int Pn)
        {
            int num = 0;
            int tmpNum = MyDefine.myXET.meTMPNum;                 //温度列数量
            int humNum = MyDefine.myXET.meHUMNum;                 //湿度列数量
            int prsNum = MyDefine.myXET.mePRSNum;                 //压力列数量
            groupBox1.Enabled = false;
            groupBox2.Enabled = false;
            groupBox3.Enabled = false;
            groupBox4.Enabled = false;
            groupBox5.Enabled = false;
            groupBox6.Enabled = false;
            groupBox7.Enabled = false;
            groupBox8.Enabled = false;

            if (Pn >= 1)
            {
                num = 1;
                groupBox1.Enabled = true;
                textBox1.Enabled = (tmpNum > 0) ? true : false;
                textBox2.Enabled = (humNum > 0) ? true : false;
                textBox3.Enabled = (prsNum > 0) ? true : false;
                if (MyDefine.myXET.meTmpSETList.Count >= num) textBox1.Text = MyDefine.myXET.meTmpSETList[num - 1].ToString();
                if (MyDefine.myXET.meHumSETList.Count >= num) textBox2.Text = MyDefine.myXET.meHumSETList[num - 1].ToString();
                if (MyDefine.myXET.mePrsSETList.Count >= num) textBox3.Text = MyDefine.myXET.mePrsSETList[num - 1].ToString();
                if (textBox1.Enabled == false) textBox1.Text = "";
                if (textBox2.Enabled == false) textBox2.Text = "";
                if (textBox3.Enabled == false) textBox3.Text = "";
            }

            if (Pn >= 2)
            {
                num = 2;
                groupBox2.Enabled = true;
                textBox4.Enabled = (tmpNum > 0) ? true : false;
                textBox5.Enabled = (humNum > 0) ? true : false;
                textBox6.Enabled = (prsNum > 0) ? true : false;
                if (MyDefine.myXET.meTmpSETList.Count >= num) textBox4.Text = MyDefine.myXET.meTmpSETList[num - 1].ToString();
                if (MyDefine.myXET.meHumSETList.Count >= num) textBox5.Text = MyDefine.myXET.meHumSETList[num - 1].ToString();
                if (MyDefine.myXET.mePrsSETList.Count >= num) textBox6.Text = MyDefine.myXET.mePrsSETList[num - 1].ToString();
                if (textBox4.Enabled == false) textBox4.Text = "";
                if (textBox5.Enabled == false) textBox5.Text = "";
                if (textBox6.Enabled == false) textBox6.Text = "";
            }

            if (Pn >= 3)
            {
                num = 3;
                groupBox3.Enabled = true;
                textBox7.Enabled = (tmpNum > 0) ? true : false;
                textBox8.Enabled = (humNum > 0) ? true : false;
                textBox9.Enabled = (prsNum > 0) ? true : false;
                if (MyDefine.myXET.meTmpSETList.Count >= num) textBox7.Text = MyDefine.myXET.meTmpSETList[num - 1].ToString();
                if (MyDefine.myXET.meHumSETList.Count >= num) textBox8.Text = MyDefine.myXET.meHumSETList[num - 1].ToString();
                if (MyDefine.myXET.mePrsSETList.Count >= num) textBox9.Text = MyDefine.myXET.mePrsSETList[num - 1].ToString();
            }

            if (Pn >= 4)
            {
                num = 4;
                groupBox4.Enabled = true;
                textBox10.Enabled = (tmpNum > 0) ? true : false;
                textBox11.Enabled = (humNum > 0) ? true : false;
                textBox12.Enabled = (prsNum > 0) ? true : false;
                if (MyDefine.myXET.meTmpSETList.Count >= num) textBox10.Text = MyDefine.myXET.meTmpSETList[num - 1].ToString();
                if (MyDefine.myXET.meHumSETList.Count >= num) textBox11.Text = MyDefine.myXET.meHumSETList[num - 1].ToString();
                if (MyDefine.myXET.mePrsSETList.Count >= num) textBox12.Text = MyDefine.myXET.mePrsSETList[num - 1].ToString();
            }

            if (Pn >= 5)
            {
                num = 5;
                groupBox5.Enabled = true;
                textBox13.Enabled = (tmpNum > 0) ? true : false;
                textBox14.Enabled = (humNum > 0) ? true : false;
                textBox15.Enabled = (prsNum > 0) ? true : false;
                if (MyDefine.myXET.meTmpSETList.Count >= num) textBox13.Text = MyDefine.myXET.meTmpSETList[num - 1].ToString();
                if (MyDefine.myXET.meHumSETList.Count >= num) textBox14.Text = MyDefine.myXET.meHumSETList[num - 1].ToString();
                if (MyDefine.myXET.mePrsSETList.Count >= num) textBox15.Text = MyDefine.myXET.mePrsSETList[num - 1].ToString();
            }

            if (Pn >= 6)
            {
                num = 6;
                groupBox6.Enabled = true;
                textBox16.Enabled = (tmpNum > 0) ? true : false;
                textBox17.Enabled = (humNum > 0) ? true : false;
                textBox18.Enabled = (prsNum > 0) ? true : false;
                if (MyDefine.myXET.meTmpSETList.Count >= num) textBox16.Text = MyDefine.myXET.meTmpSETList[num - 1].ToString();
                if (MyDefine.myXET.meHumSETList.Count >= num) textBox17.Text = MyDefine.myXET.meHumSETList[num - 1].ToString();
                if (MyDefine.myXET.mePrsSETList.Count >= num) textBox18.Text = MyDefine.myXET.mePrsSETList[num - 1].ToString();
            }

            if (Pn >= 7)
            {
                num = 7;
                groupBox7.Enabled = true;
                textBox19.Enabled = (tmpNum > 0) ? true : false;
                textBox20.Enabled = (humNum > 0) ? true : false;
                textBox21.Enabled = (prsNum > 0) ? true : false;
                if (MyDefine.myXET.meTmpSETList.Count >= num) textBox19.Text = MyDefine.myXET.meTmpSETList[num - 1].ToString();
                if (MyDefine.myXET.meHumSETList.Count >= num) textBox20.Text = MyDefine.myXET.meHumSETList[num - 1].ToString();
                if (MyDefine.myXET.mePrsSETList.Count >= num) textBox21.Text = MyDefine.myXET.mePrsSETList[num - 1].ToString();
            }

            if (Pn >= 8)
            {
                num = 8;
                groupBox8.Enabled = true;
                textBox22.Enabled = (tmpNum > 0) ? true : false;
                textBox23.Enabled = (humNum > 0) ? true : false;
                textBox24.Enabled = (prsNum > 0) ? true : false;
                if (MyDefine.myXET.meTmpSETList.Count >= num) textBox22.Text = MyDefine.myXET.meTmpSETList[num - 1].ToString();
                if (MyDefine.myXET.meHumSETList.Count >= num) textBox23.Text = MyDefine.myXET.meHumSETList[num - 1].ToString();
                if (MyDefine.myXET.mePrsSETList.Count >= num) textBox24.Text = MyDefine.myXET.mePrsSETList[num - 1].ToString();
            }

            if (textBox1.Enabled == false) textBox1.Text = "";
            if (textBox2.Enabled == false) textBox2.Text = "";
            if (textBox3.Enabled == false) textBox3.Text = "";
            if (textBox4.Enabled == false) textBox4.Text = "";
            if (textBox5.Enabled == false) textBox5.Text = "";
            if (textBox6.Enabled == false) textBox6.Text = "";
            if (textBox7.Enabled == false) textBox7.Text = "";
            if (textBox8.Enabled == false) textBox8.Text = "";
            if (textBox9.Enabled == false) textBox9.Text = "";
            if (textBox10.Enabled == false) textBox10.Text = "";
            if (textBox11.Enabled == false) textBox11.Text = "";
            if (textBox12.Enabled == false) textBox12.Text = "";
            if (textBox13.Enabled == false) textBox13.Text = "";
            if (textBox14.Enabled == false) textBox14.Text = "";
            if (textBox15.Enabled == false) textBox15.Text = "";
            if (textBox16.Enabled == false) textBox16.Text = "";
            if (textBox17.Enabled == false) textBox17.Text = "";
            if (textBox18.Enabled == false) textBox18.Text = "";
            if (textBox19.Enabled == false) textBox19.Text = "";
            if (textBox20.Enabled == false) textBox20.Text = "";
            if (textBox21.Enabled == false) textBox21.Text = "";
            if (textBox22.Enabled == false) textBox22.Text = "";
            if (textBox23.Enabled == false) textBox23.Text = "";
            if (textBox24.Enabled == false) textBox24.Text = "";
        }
    }
}
