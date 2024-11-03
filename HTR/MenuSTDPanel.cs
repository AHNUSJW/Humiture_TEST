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
    public partial class MenuSTDPanel : UserControl
    {
        public MenuSTDPanel()
        {
            InitializeComponent();
        }

        private void MenuSTDPanel_Load(object sender, EventArgs e)
        {

        }

        #region 界面按钮事件

        #region 读取标准器列表

        //读取标准器列表
        private void button1_Click(object sender, EventArgs e)
        {
            groupBox1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            try
            {
                MyDefine.myXET.AddTraceInfo("读取");

                MyDefine.myXET.loadReportInfo();            //加载标准器数据表
                showSTDTable();                             //显示标准器数据表
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("读取失败：" + ex.ToString());
            }
        }

        #endregion

        #region 保存标准器列表

        //保存标准器列表
        private void button2_Click(object sender, EventArgs e)
        {
            groupBox1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            try
            {
                MyDefine.myXET.AddTraceInfo("保存");

                updateSTDTable();                                        //更新标准器数据表
                Boolean ret = MyDefine.myXET.saveReportInfo();           //保存标准器数据表
                if (ret) MyDefine.myXET.ShowCorrectMsg("保存成功！");
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("保存失败：" + ex.ToString());
            }
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
                MyDefine.myXET.ShowWrongMsg("OnResize失败：" + ex.ToString());
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
                MyDefine.myXET.ShowWrongMsg("界面尺寸变化失败：" + ex.ToString());
            }
        }

        #endregion

        #region 更新标准器数据表

        /// <summary>
        /// 更新标准器数据表
        /// </summary>
        /// <returns></returns>
        public void updateSTDTable()
        {
            try
            {
                //添加标准器表列信息
                MyDefine.myXET.meTblSTD = new dataTableClass();
                MyDefine.myXET.meTblSTD.addTableColumn(new string[] { "标准器名称", "标准器编号", "标准器测量范围", "溯源有效期至", "溯源单位" });

                //添加行信息
                if (textBox1.Text != "") MyDefine.myXET.meTblSTD.AddTableRow(new string[] { textBox1.Text, textBox2.Text, textBox3.Text, textBox4.Text, textBox5.Text });
                if (textBox6.Text != "") MyDefine.myXET.meTblSTD.AddTableRow(new string[] { textBox6.Text, textBox7.Text, textBox8.Text, textBox9.Text, textBox10.Text });
                if (textBox11.Text != "") MyDefine.myXET.meTblSTD.AddTableRow(new string[] { textBox11.Text, textBox12.Text, textBox13.Text, textBox14.Text, textBox15.Text });
                if (textBox16.Text != "") MyDefine.myXET.meTblSTD.AddTableRow(new string[] { textBox16.Text, textBox17.Text, textBox18.Text, textBox19.Text, textBox20.Text });
                if (textBox21.Text != "") MyDefine.myXET.meTblSTD.AddTableRow(new string[] { textBox21.Text, textBox22.Text, textBox23.Text, textBox24.Text, textBox25.Text });
                if (textBox26.Text != "") MyDefine.myXET.meTblSTD.AddTableRow(new string[] { textBox26.Text, textBox27.Text, textBox28.Text, textBox29.Text, textBox30.Text });
                if (textBox31.Text != "") MyDefine.myXET.meTblSTD.AddTableRow(new string[] { textBox31.Text, textBox32.Text, textBox33.Text, textBox34.Text, textBox35.Text });
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("更新标准器数据表失败：" + ex.ToString());
            }
        }

        #endregion

        #region 显示标准器数据表

        /// <summary>
        /// 显示标准器数据表
        /// </summary>
        /// <returns></returns>
        public void showSTDTable()
        {
            try
            {
                if (MyDefine.myXET.meTblSTD != null)
                {
                    //标准器1
                    if (MyDefine.myXET.meTblSTD.dataTable.Rows.Count > 0)
                    {
                        textBox1.Text = MyDefine.myXET.meTblSTD.GetCellValue(0, 0);
                        textBox2.Text = MyDefine.myXET.meTblSTD.GetCellValue(0, 1);
                        textBox3.Text = MyDefine.myXET.meTblSTD.GetCellValue(0, 2);
                        textBox4.Text = MyDefine.myXET.meTblSTD.GetCellValue(0, 3);
                        textBox5.Text = MyDefine.myXET.meTblSTD.GetCellValue(0, 4);
                    }

                    //标准器2
                    if (MyDefine.myXET.meTblSTD.dataTable.Rows.Count > 1)
                    {
                        textBox6.Text = MyDefine.myXET.meTblSTD.GetCellValue(1, 0);
                        textBox7.Text = MyDefine.myXET.meTblSTD.GetCellValue(1, 1);
                        textBox8.Text = MyDefine.myXET.meTblSTD.GetCellValue(1, 2);
                        textBox9.Text = MyDefine.myXET.meTblSTD.GetCellValue(1, 3);
                        textBox10.Text = MyDefine.myXET.meTblSTD.GetCellValue(1, 4);
                    }

                    //标准器3
                    if (MyDefine.myXET.meTblSTD.dataTable.Rows.Count > 2)
                    {
                        textBox11.Text = MyDefine.myXET.meTblSTD.GetCellValue(2, 0);
                        textBox12.Text = MyDefine.myXET.meTblSTD.GetCellValue(2, 1);
                        textBox13.Text = MyDefine.myXET.meTblSTD.GetCellValue(2, 2);
                        textBox14.Text = MyDefine.myXET.meTblSTD.GetCellValue(2, 3);
                        textBox15.Text = MyDefine.myXET.meTblSTD.GetCellValue(2, 4);
                    }

                    //标准器4
                    if (MyDefine.myXET.meTblSTD.dataTable.Rows.Count > 3)
                    {
                        textBox16.Text = MyDefine.myXET.meTblSTD.GetCellValue(3, 0);
                        textBox17.Text = MyDefine.myXET.meTblSTD.GetCellValue(3, 1);
                        textBox18.Text = MyDefine.myXET.meTblSTD.GetCellValue(3, 2);
                        textBox19.Text = MyDefine.myXET.meTblSTD.GetCellValue(3, 3);
                        textBox20.Text = MyDefine.myXET.meTblSTD.GetCellValue(3, 4);
                    }

                    //标准器5
                    if (MyDefine.myXET.meTblSTD.dataTable.Rows.Count > 4)
                    {
                        textBox21.Text = MyDefine.myXET.meTblSTD.GetCellValue(4, 0);
                        textBox22.Text = MyDefine.myXET.meTblSTD.GetCellValue(4, 1);
                        textBox23.Text = MyDefine.myXET.meTblSTD.GetCellValue(4, 2);
                        textBox24.Text = MyDefine.myXET.meTblSTD.GetCellValue(4, 3);
                        textBox25.Text = MyDefine.myXET.meTblSTD.GetCellValue(4, 4);
                    }

                    //标准器6
                    if (MyDefine.myXET.meTblSTD.dataTable.Rows.Count > 5)
                    {
                        textBox26.Text = MyDefine.myXET.meTblSTD.GetCellValue(5, 0);
                        textBox27.Text = MyDefine.myXET.meTblSTD.GetCellValue(5, 1);
                        textBox28.Text = MyDefine.myXET.meTblSTD.GetCellValue(5, 2);
                        textBox29.Text = MyDefine.myXET.meTblSTD.GetCellValue(5, 3);
                        textBox30.Text = MyDefine.myXET.meTblSTD.GetCellValue(5, 4);
                    }

                    //标准器7
                    if (MyDefine.myXET.meTblSTD.dataTable.Rows.Count > 6)
                    {
                        textBox31.Text = MyDefine.myXET.meTblSTD.GetCellValue(6, 0);
                        textBox32.Text = MyDefine.myXET.meTblSTD.GetCellValue(6, 1);
                        textBox33.Text = MyDefine.myXET.meTblSTD.GetCellValue(6, 2);
                        textBox34.Text = MyDefine.myXET.meTblSTD.GetCellValue(6, 3);
                        textBox35.Text = MyDefine.myXET.meTblSTD.GetCellValue(6, 4);
                    }
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("显示标准器数据表失败：" + ex.ToString());
            }
        }

        #endregion

        #region 文本框输入限制(禁止回车)

        /// <summary>
        /// 文本框输入限制
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
        }

        #endregion

    }
}
