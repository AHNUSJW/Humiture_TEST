using NPOI.HSSF.UserModel;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace HTR
{
    public partial class MenuDataCurvePanel : UserControl
    {
        public UInt16 total = 0;//控制进度条
        private String myPath = MyDefine.myXET.userDAT;
        private int myTimeSpan = 0;         //测试间隔时间(秒)
        private DateTime myStartTime = new DateTime();
        private DateTime myStopTime = new DateTime();
        private Int32 myTestSeconds = 0;     //总测试时间
        private Int32 currentCurveIndex = 0;   //当前曲线数据的索引
        private Boolean picSaving = false;     //保存图片过程中，曲线图上不显示测试值
        private Boolean isPsr = false;         //是否存在压力数据
        private Int32 indexStart;
        private Int32 indexEnd;
        private Boolean drawPicture = false;
        private Point leave_Panel1;

        public MenuDataCurvePanel()
        {
            InitializeComponent();
        }

        #region 界面加载/关闭

        private void MenuCurvePanel_Load(object sender, EventArgs e)
        {
            label1.Text = "温度/" + MyDefine.myXET.temUnit;
            this.DoubleBuffered = true;
            //InitCurvePanel();
        }

        /// <summary>
        /// CurvePanel初始化
        /// </summary>
        public void InitCurvePanel()
        {
            try
            {
                buildControlArrary();
                initalLabelArray();
                comboBox1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        public void AddMyUpdateEvent()
        {
            try
            {
                //查看验证曲线时，禁止保存数据及切换曲线
                button6.Visible = (this.Name == "数据曲线") ? true : false;     //保存数据按钮
                label41.Visible = (this.Name == "数据曲线") ? true : false;     //选择曲线标签
                comboBox1.Visible = (this.Name == "数据曲线") ? true : false;   //曲线切换框
                groupBox2.Visible = (this.Name == "数据曲线") ? true : false;   //上下限输入框
                tabControl1.Visible = (this.Name == "数据曲线") ? true : false; //阶段数据输入
                label68.Visible = (this.Name == "数据曲线") ? true : false;     //选择阶段标签
                cb_selectStage.Visible = (this.Name == "数据曲线") ? true : false;   //阶段切换框
                leave_Panel1 = panel1.PointToScreen(Point.Empty);

                if (MyDefine.myXET.meDataCurveUpdating == true)                 //有新的数据表文件加载
                {
                    MyDefine.myXET.meDataCurveUpdating = false;
                    ZoomScale = 1.0f;                                           //曲线放大比例为1.0
                    drawPicture = false;                                        //关闭曲线的放大
                    Zoom = 1.0f;

                    //初始化阶段选择
                    if (this.Name == "数据曲线")
                    {
                        tb_setValueLeft.Text = "";
                        tb_upperValueLeft.Text = "";
                        tb_lowerValueLeft.Text = "";
                        tb_setValueRight.Text = "";
                        tb_upperValueRight.Text = "";
                        tb_lowerValueRight.Text = "";
                        if (MyDefine.myXET.meTMPNum > 0)//温度列数量
                        {
                            tabPageLeft.Tag = true;
                            tabControl1.SelectedIndex = 0;
                            tb_setValueLeft.Text = MyDefine.myXET.meValidSetValueList[0].ToString();               //显示阶段n的设定温度
                            tb_upperValueLeft.Text = MyDefine.myXET.meValidUpperList[0].ToString();                  //显示阶段n的纵坐标上限温度
                            tb_lowerValueLeft.Text = MyDefine.myXET.meValidLowerList[0].ToString();                  //显示阶段n的纵坐标下限温度
                        }
                        else
                        {
                            tabPageLeft.Tag = false;
                            tabControl1.SelectedIndex = 1;
                        }

                        if (MyDefine.myXET.meHUMNum > 0 || MyDefine.myXET.mePRSNum > 0)//湿度列数量或压力列数量大于0
                        {
                            tabPageRight.Tag = true;
                            tb_setValueRight.Text = MyDefine.myXET.meValidSetValueList[1].ToString();               //显示阶段n的设定温度
                            tb_upperValueRight.Text = MyDefine.myXET.meValidUpperList[1].ToString();                  //显示阶段n的纵坐标上限温度
                            tb_lowerValueRight.Text = MyDefine.myXET.meValidLowerList[1].ToString();                  //显示阶段n的纵坐标下限温度
                        }
                        else
                        {
                            tabPageRight.Tag = false;
                        }
                    }
                }

                if (this.Name == "数据曲线")
                {
                    UpdateCurveInfo();                                          //加载文件后更新曲线信息（曲线名称列表、曲线颜色列表、曲线有效数据信息）

                    cb_selectStage.Items.Clear();
                    foreach (string str in MyDefine.myXET.meValidNameList)
                    {
                        cb_selectStage.Items.Add(str);
                    }

                    if (MyDefine.myXET.meTMPNum > 0)//温度列数量
                    {
                        tabPageLeft.Tag = true;
                        tabControl1.SelectedIndex = 0;
                    }
                    else
                    {
                        tabPageLeft.Tag = false;
                        tabControl1.SelectedIndex = 1;
                    }

                    if (MyDefine.myXET.meHUMNum > 0 || MyDefine.myXET.mePRSNum > 0)//湿度列数量或压力列数量大于0
                    {
                        tabPageRight.Tag = true;
                        tabControl1.SelectedIndex = 1;
                    }
                    else
                    {
                        tabPageRight.Tag = false;
                    }
                }

                updateCurvelDrawing();                                          //绘制曲线、有效数据线并更新右侧信息框
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        //界面关闭(被移除出panel)
        private void MenuCurvePanel_ParentChanged(object sender, EventArgs e)
        {
            if (this.Parent == null)
            {
                //保存图片
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                MyDefine.myXET.reportPicName = System.DateTime.Now.ToString("yyMMddHHmmssfff");
                saveFileDialog.FileName = MyDefine.myXET.userPIC + @"\" + MyDefine.myXET.reportPicName + ".gif";
                try
                {
                    Bitmap bitmap = GetFormCaptureLeave(panel1);
                    bitmap.Save(saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    //捕获异常
                    MyDefine.myXET.ShowWrongMsg("图片保存失败：" + ex.ToString());
                }

                //if (button1.Visible == true) button1_Click(null, null); //单击取消按钮 -- 取消正在读取的设备数据
                MyDefine.myXET.meTmpSETList.Clear();
                MyDefine.myXET.meHumSETList.Clear();
                MyDefine.myXET.mePrsSETList.Clear();

                for (int i = 0; i < MyDefine.myXET.meValidStageNum; i++)
                {
                    if (MyDefine.myXET.drawTemCurve)
                    {
                        MyDefine.myXET.meTmpSETList.Add(MyDefine.myXET.meValidSetValueList[i * 2]);//温度设定值
                    }
                    if (MyDefine.myXET.drawHumCurve)
                    {
                        MyDefine.myXET.meHumSETList.Add(MyDefine.myXET.meValidSetValueList[i * 2 + 1]);//湿度设定值
                    }
                    if (MyDefine.myXET.drawPrsCurve)
                    {
                        MyDefine.myXET.mePrsSETList.Add(MyDefine.myXET.meValidSetValueList[i * 2 + 1]);//压力设定值
                    }
                }
            }
            else
            {
            }
        }

        public void updateCurvelDrawing()
        {
            UnlockValidStageList();             //根据有效列表解锁必要的阶段列表Pn
            DrawFileDataLines();           //重新加载界面后重新绘制曲线
            ShowValidLines();                   //显示可能存在的有效数据竖直线
            ShowValidInfo();                    //显示右侧有效数据信息
        }

        /// <summary>
        /// 加载文件后更新曲线信息（曲线名称列表、曲线颜色列表、曲线有效数据信息）
        /// </summary>
        public void UpdateCurveInfo()
        {
            UpdateCurveNameList();           //更新曲线名称列表                
            setControlArrayAll();            //设置并显示所有曲线名称
            //ResetValidList();                //复位有效数据列表

            int listIdx = MyDefine.myXET.meActivePn * 2;
            myStartTime = MyDefine.myXET.meStartTime;                           //有效数据开始时间
            myStopTime = MyDefine.myXET.meStopTime;                             //有效数据结束时间
            myTimeSpan = Convert.ToInt32(MyDefine.myXET.homspan);               //测试间隔(秒)
            myTestSeconds = (Int32)(myStopTime.Subtract(myStartTime).TotalSeconds);     //总测试时间(秒)
        }

        #endregion

        #region 界面按钮事件

        #region 保存图片按钮 -- 将曲线图保存为图片

        private void button7_Click(object sender, EventArgs e)
        {
            groupBox1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            picSaving = true;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            MyDefine.myXET.reportPicName = System.DateTime.Now.ToString("yyMMddHHmmssfff");
            saveFileDialog.FileName = MyDefine.myXET.userPIC + @"\" + MyDefine.myXET.reportPicName + ".gif";
            saveFormImage(saveFileDialog.FileName);

            //System.Windows.Forms.SaveFileDialog DialogSave = new System.Windows.Forms.SaveFileDialog();
            //DialogSave.Filter = "图片(*.bmp)|*.bmp|图片(*.jpeg)|*.jpeg|图片(*.gif)|*.gif|所有文件(*.*)|*.*";
            //DialogSave.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);     //默认桌面
            ////DialogSave.InitialDirectory = MyDefine.myXET.userPIC;

            //if (DialogSave.ShowDialog() == DialogResult.OK)
            //{
            //    MyDefine.myXET.AddTraceInfo("保存图片");
            //    Application.DoEvents();
            //    Thread.Sleep(1000);                      //等待保存窗口关闭，防止保存窗口被截图到图片中

            //    saveFormImage(DialogSave.FileName);
            //}

            picSaving = false;
            //button6.Visible = true;     //显示保存数据按钮
            //Application.DoEvents();
        }

        #endregion

        #region 保存数据按钮 -- 保存客户选中的局部曲线数据(仅在显示单条数据时使能)

        //保存数据按钮
        private void button6_Click(object sender, EventArgs e)
        {
            groupBox1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            SaveAllSelectedCurveData();        //保存所选曲线数据至txt文件、csv文件
        }

        #endregion

        #region 选择曲线复选框 -- 选择单条或所有曲线后重新绘图

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (MyDefine.myXET.meDataTbl == null) return;                       //数据表未实例化
                currentCurveIndex = comboBox1.SelectedIndex;                        //保存当前曲线索引(为0表示多条曲线)
                if (currentCurveIndex == 0) setControlArrayAll();                   //设置所有曲线为选中状态
                if (currentCurveIndex > 0) setControlArraySingle(currentCurveIndex - 1);      //选中单条曲线。comboBox1的曲线索引比myBoxArray的对应曲线索引大1，因为comboBox1的索引0为All_Lines

                if (drawPicture)
                {
                    drawPointMessage();      //绘制截取曲线
                }
                else
                {
                    DrawFileDataLines();     //绘制所有选中曲线
                }
                ShowValidLines();                                                   //显示可能存在的有效数据竖直线
                ShowValidInfo();                                                    //显示右侧有效数据信息
                //DrawSelectArea();                                                   //重新绘制可能存在的区域矩形
                //DrawDataValidLines();                                               //重新绘制可能存在的开始或结束线条

                /*
                if (currentCurveIndex == 0)                     //绘制所有数据曲线
                {
                    //button6.Enabled = false;                  //禁止保存数据按钮
                    //button6.ForeColor = Color.DimGray;
                    //resetCurveInfo();                         //将曲线信息复位为整条曲线的信息
                    setControlArrayAll();                       //设置所有曲线为选中状态
                    DrawFileDataLines();                        //绘制所有曲线
                    DrawSelectArea();                           //重新绘制可能存在的区域矩形
                    DrawDataValidLines();                       //重新绘制可能存在的开始或结束线条
                }
                else                                            //绘制选中曲线(单条曲线)
                {
                    //button6.Enabled = true;                   //是能保存数据按钮
                    //button6.ForeColor = Color.OliveDrab;
                    setControlArraySingle(currentCurveIndex - 1);      //comboBox1的曲线索引比myBoxArray的对应曲线索引大1，因为comboBox1的索引0为All_Lines
                    DrawFileDataLines();                        //绘制所有曲线
                    //DrawFileDataLine(currentCurveIndex);        //绘制单条曲线
                    DrawSelectArea();                           //重新绘制可能存在的区域矩形
                    DrawDataValidLines();                       //重新绘制可能存在的开始或结束线条
                }
                */
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        #endregion

        #region 控件数组 -- 显示曲线对应的颜色、名称信息

        #region 建立控件数组

        public List<Label> myLabelArray = new List<Label>();
        public List<Label> myLabelColor = new List<Label>();
        public List<CheckBox> myBoxArray = new List<CheckBox>();
        public void buildControlArrary()
        {
            myLabelArray.Add(label22);      //曲线1
            myLabelArray.Add(label24);      //曲线2
            myLabelArray.Add(label26);      //曲线3
            myLabelArray.Add(label28);      //曲线4
            myLabelArray.Add(label30);      //曲线5
            myLabelArray.Add(label32);      //曲线6
            myLabelArray.Add(label37);      //曲线7
            myLabelArray.Add(label36);      //曲线8
            myLabelArray.Add(label40);      //曲线9
            myLabelArray.Add(label38);      //曲线10
            myLabelArray.Add(label46);      //曲线11
            myLabelArray.Add(label55);      //曲线12
            myLabelArray.Add(label52);      //曲线13
            myLabelArray.Add(label60);      //曲线14
            myLabelArray.Add(label56);      //曲线15
            myLabelArray.Add(label43);      //曲线16
            myLabelArray.Add(label54);      //曲线17
            myLabelArray.Add(label53);      //曲线18
            myLabelArray.Add(label61);      //曲线19
            myLabelArray.Add(label57);      //曲线20

            myLabelColor.Add(label19);
            myLabelColor.Add(label23);
            myLabelColor.Add(label25);
            myLabelColor.Add(label27);
            myLabelColor.Add(label29);
            myLabelColor.Add(label31);
            myLabelColor.Add(label34);
            myLabelColor.Add(label33);
            myLabelColor.Add(label39);
            myLabelColor.Add(label35);
            myLabelColor.Add(label44);
            myLabelColor.Add(label50);
            myLabelColor.Add(label45);
            myLabelColor.Add(label58);
            myLabelColor.Add(label49);
            myLabelColor.Add(label42);
            myLabelColor.Add(label48);
            myLabelColor.Add(label47);
            myLabelColor.Add(label59);
            myLabelColor.Add(label51);
        }

        #endregion

        #region 检查控件数组是否已创建

        //检查控件数组是否已创建
        public Boolean IsControlArrayReady()
        {
            if (myLabelColor.Count == 0) return false;
            if (myLabelArray.Count == 0) return false;
            return true;
        }

        #endregion

        #region 初始化控件数组

        public void initalLabelArray()
        {
            if (!IsControlArrayReady()) return;                     //控件数组未创建

            for (int i = 0; i < myLabelArray.Count; i++)
            {
                //myBoxArray[i].Checked = false;
                //myBoxArray[i].ForeColor = Color.DimGray;

                myLabelColor[i].TabIndex = i;                 //初始化索引编号
                myLabelColor[i].Text = string.Empty;          //初始为未选中状态
                myLabelColor[i].ForeColor = Color.Black;
                myLabelArray[i].Text = "曲线" + (i + 1).ToString();

                //myLabelArray[i].Font = new System.Drawing.Font("宋体", 8.5F);
                //myLabelArray[i].Visible = false;
                //myLabelColor[i].Visible = false;
            }
        }

        #endregion

        #region 复位控件数组

        //复位并隐藏所有曲线名称
        public void resetControlArrayAll()
        {
            if (!IsControlArrayReady()) return;                     //控件数组未创建

            for (int i = 0; i < myLabelArray.Count; i++)
            {
                //myBoxArray[i].Checked = false;
                //myBoxArray[i].Visible = false;

                myLabelColor[i].Visible = false;
                myLabelArray[i].Visible = false;
                myLabelColor[i].Text = string.Empty;          //复位为未选中状态
                myLabelArray[i].Text = "曲线" + (i + 1).ToString();
            }
        }

        #endregion

        #region 设置控件数组 -- 设置所有曲线为选中状态

        //设置所有曲线为选中状态
        public void setControlArrayAll()
        {
            resetControlArrayAll();
            for (int i = 0; i < comboBox1.Items.Count - 1; i++)
            {
                //myBoxArray[i].Checked = true;
                //myBoxArray[i].Visible = true;
                if (comboBox1.Items.Count > 21)
                {
                    myLabelColor[i].Visible = false;
                    myLabelArray[i].Visible = false;
                }
                else
                {
                    myLabelColor[i].Visible = true;
                    myLabelArray[i].Visible = true;
                }
                myLabelColor[i].Text = "√";                 //设置为选中状态
                myLabelArray[i].Text = comboBox1.Items[i + 1].ToString();
            }
        }

        #endregion

        #region 设置控件数组 -- 设置单条曲线为选中状态

        //设置单条曲线为选中状态
        public void setControlArraySingle(int selectIdx)
        {
            if (!IsControlArrayReady()) return;                     //控件数组未创建

            for (int i = 0; i < myLabelColor.Count; i++)
            {
                if (i == selectIdx)
                {
                    //myBoxArray[i].Checked = true;
                    myLabelColor[i].Text = "√";                 //设置为选中状态
                }
                else
                {
                    //myBoxArray[i].Checked = false;
                    myLabelColor[i].Text = string.Empty;          //设置为未选中状态
                }
            }
        }

        #endregion

        #region 单击事件 -- 单击某个颜色label(共20个颜色label)

        //共20个颜色label，当其选中状态发生变化时重新绘制所有曲线
        private void labelColor_Click(object sender, EventArgs e)
        {
            if (this.Name == "验证曲线") return;
            if (!IsControlArrayReady()) return;                     //控件数组未创建

            int idx = ((Label)sender).TabIndex;
            myLabelColor[idx].Text = myLabelColor[idx].Text == string.Empty ? "√" : string.Empty;   //选中状态翻转
            currentCurveIndex = getDisplayCurveIndex();          //保存当前曲线索引(若为多条曲线则为0)
            /*
            if (currentCurveIndex == -1)                         //没有显示曲线
            {
                button6.Enabled = false;                          //禁止保存数据按钮
                button6.ForeColor = Color.DimGray;
            }
            else                                                  //当前显示单条或多条曲线
            {
                button6.Enabled = true;                           //使能保存数据按钮
                button6.ForeColor = Color.OliveDrab;
            }
            */

            if (drawPicture)
            {
                drawPointMessage();      //绘制截取曲线
            }
            else
            {
                DrawFileDataLines();     //绘制所有选中曲线
            }
            ShowValidLines();                                     //显示可能存在的有效数据竖直线
            ShowValidInfo();                                      //显示右侧有效数据信息
            //DrawSelectArea();                                     //重新绘制可能存在的区域矩形
            //DrawDataValidLines();                                 //重新绘制可能存在的开始或结束线条
        }

        #endregion

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

        #region 保存曲线图
        /*
        //保存图片
        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            System.Windows.Forms.SaveFileDialog DialogSave = new System.Windows.Forms.SaveFileDialog();
            DialogSave.Filter = "图片(*.bmp)|*.bmp|图片(*.jpeg)|*.jpeg|图片(*.gif)|*.gif|所有文件(*.*)|*.*";
            DialogSave.InitialDirectory = MyDefine.myXET.userPIC;

            if (DialogSave.ShowDialog() == DialogResult.OK)
            {
                saveFormImage(DialogSave.FileName);
            }
            
        }
        */
        #region 截取屏幕图像并保存
        private Bitmap GetScreenCapture()
        {
            Rectangle tScreenRect = new Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Bitmap tSrcBmp = new Bitmap(tScreenRect.Width, tScreenRect.Height); // 用于屏幕原始图片保存
            Graphics gp = Graphics.FromImage(tSrcBmp);
            gp.CopyFromScreen(0, 0, 0, 0, tScreenRect.Size);
            gp.DrawImage(tSrcBmp, 0, 0, tScreenRect, GraphicsUnit.Pixel);
            return tSrcBmp;
        }

        private void saveScreenImage(string path)
        {
            Bitmap bitmap = GetScreenCapture();
            bitmap.Save(path);
        }
        #endregion

        #region 截取软件图像并保存

        private void saveFormImage(string path)
        {
            try
            {
                Bitmap bitmap = GetFormCapture(panel1);
                bitmap.Save(path);
                MyDefine.myXET.ShowCorrectMsg("保存成功！");
                //MyDefine.myXET.AddToTraceRecords("数据曲线", "保存成功");
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("图片保存失败：" + ex.ToString());
            }
        }

        private Bitmap GetFormCapture(Control myCon)
        {
            int scNum = Screen.AllScreens.Count();                  //获取屏幕数量(可能存在扩展屏)
            Screen CurrentScreen = Screen.FromControl(this);        //窗体所在屏幕
            //Screen[] screens = Screen.AllScreens;                 //获取屏幕集合

            float ratioX = 0f;                          //屏幕横向缩放比例
            float ratioY = 0f;                          //屏幕竖向缩放比例
            GetDPIScale(ref ratioX, ref ratioY);        //获取当前屏幕缩放比例(100%/125%/150%)

            //如果程序显示在扩展屏上，则缩放为100%
            if (scNum > 1 && !CurrentScreen.Primary)    //存在扩展屏 且 当前窗体所在屏幕非主屏(次屏缩放一直是100%？)
            {
                ratioX = 1;
                ratioY = 1;
            }

            Size desSize = myCon.Size;
            Point desPoint = myCon.PointToScreen(Point.Empty);         //获取控件在屏幕上的坐标(100%缩放比例时的坐标)
            //Point desPoint2 = myCon.PointToScreen(new Point (100,100));//获取控件在屏幕上的坐标(100%缩放比例时的坐标)
            desSize = new Size((int)(desSize.Width * ratioX - 4), (int)(desSize.Height * ratioY - 4));  //根据屏幕缩放比例调整截取范围
            desPoint = new Point((int)(desPoint.X * ratioX + 2), (int)(desPoint.Y * ratioY + 2));       //根据屏幕缩放比例调整截取起始点

            Bitmap picBmp = new Bitmap(desSize.Width, desSize.Height); // 用于pictureBox1图片保存
            Graphics gp = Graphics.FromImage(picBmp);
            gp.CopyFromScreen(desPoint, Point.Empty, desSize);    //将pictureBox1坐标转换为屏幕坐标，并截取picturebox位置的图片

            return picBmp;
        }

        private Bitmap GetFormCaptureLeave(Control myCon)
        {
            int scNum = Screen.AllScreens.Count();                  //获取屏幕数量(可能存在扩展屏)
            Screen CurrentScreen = Screen.FromControl(this);        //窗体所在屏幕
            //Screen[] screens = Screen.AllScreens;                 //获取屏幕集合

            float ratioX = 0f;                          //屏幕横向缩放比例
            float ratioY = 0f;                          //屏幕竖向缩放比例
            GetDPIScale(ref ratioX, ref ratioY);        //获取当前屏幕缩放比例(100%/125%/150%)

            //如果程序显示在扩展屏上，则缩放为100%
            if (scNum > 1 && !CurrentScreen.Primary)    //存在扩展屏 且 当前窗体所在屏幕非主屏(次屏缩放一直是100%？)
            {
                ratioX = 1;
                ratioY = 1;
            }

            Size desSize = myCon.Size;
            Point desPoint = leave_Panel1;         //获取控件在屏幕上的坐标(100%缩放比例时的坐标)
            //Point desPoint2 = myCon.PointToScreen(new Point (100,100));//获取控件在屏幕上的坐标(100%缩放比例时的坐标)
            desSize = new Size((int)(desSize.Width * ratioX - 4), (int)(desSize.Height * ratioY - 4));  //根据屏幕缩放比例调整截取范围
            desPoint = new Point((int)(desPoint.X * ratioX + 2), (int)(desPoint.Y * ratioY + 2));       //根据屏幕缩放比例调整截取起始点

            Bitmap picBmp = new Bitmap(desSize.Width, desSize.Height); // 用于pictureBox1图片保存
            Graphics gp = Graphics.FromImage(picBmp);
            gp.CopyFromScreen(desPoint, Point.Empty, desSize);    //将pictureBox1坐标转换为屏幕坐标，并截取picturebox位置的图片

            return picBmp;
        }

        private Bitmap GetFormCapture0(Control myCon)
        {
            float ratioX = 0f;                          //屏幕横向缩放比例
            float ratioY = 0f;                          //屏幕竖向缩放比例
            GetDPIScale(ref ratioX, ref ratioY);        //获取当前屏幕缩放比例(100%/125%/150%)

            Size desSize = myCon.Size;
            Point desPoint = myCon.PointToScreen(Point.Empty);              //获取控件在屏幕上的坐标(100%缩放比例时的坐标)
            //Point desPointxx = myCon.PointToScreen(new Point(100, 100));    //获取控件上一点在屏幕上的坐标(100%缩放比例时的坐标)
            desSize = new Size((int)(desSize.Width * ratioX - 4), (int)(desSize.Height * ratioY - 4));  //根据屏幕缩放比例调整截取范围
            desPoint = new Point((int)(desPoint.X * ratioX + 2), (int)(desPoint.Y * ratioY + 2));       //根据屏幕缩放比例调整截取起始点

            Bitmap picBmp = new Bitmap(desSize.Width, desSize.Height); // 用于pictureBox1图片保存
            Graphics gp = Graphics.FromImage(picBmp);
            gp.CopyFromScreen(desPoint, Point.Empty, desSize);    //将pictureBox1坐标转换为屏幕坐标，并截取picturebox位置的图片

            return picBmp;
        }

        #endregion

        #region 获取屏幕缩放比率(100%/125%/150%)

        #region Dll引用

        [DllImport("User32.dll", EntryPoint = "GetDC")]
        private extern static IntPtr GetDC(IntPtr hWnd);

        [DllImport("User32.dll", EntryPoint = "ReleaseDC")]
        private extern static int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("User32.dll")]
        public static extern int GetSystemMetrics(int hWnd);

        const int DESKTOPVERTRES = 117;
        const int DESKTOPHORZRES = 118;

        const int SM_CXSCREEN = 0;
        const int SM_CYSCREEN = 1;

        #endregion

        #region 获取屏幕缩放比率

        /// <summary>
        /// 获取DPI缩放比例
        /// </summary>
        /// <param name="dpiscalex"></param>
        /// <param name="dpiscaley"></param>
        public static void GetDPIScale(ref float dpiscalex, ref float dpiscaley)
        {
            int x = GetSystemMetrics(SM_CXSCREEN);
            int y = GetSystemMetrics(SM_CYSCREEN);
            IntPtr hdc = GetDC(IntPtr.Zero);
            int w = GetDeviceCaps(hdc, DESKTOPHORZRES);
            int h = GetDeviceCaps(hdc, DESKTOPVERTRES);
            ReleaseDC(IntPtr.Zero, hdc);
            dpiscalex = (float)w / x;
            dpiscaley = (float)h / y;

            //MessageBox.Show(dpiscalex.ToString() + "," + dpiscaley.ToString());
        }

        #endregion

        #endregion

        #endregion

        #region 设置有效数据开始、结束时间

        #region 动态选择有效数据区域 -- 动态绘制区域矩形、绘制测量线(暂时未用，勿删)

        private Bitmap MeasBitmap;//= new Bitmap(pictureBox1.Width , pictureBox1.Height);                      //画布
        private Boolean leftButtonRelease = true;     //默认鼠标左键未按下
        private Point firstPoint = Point.Empty;       //记录区域按下的第一个点
        private Point areaPoint1 = Point.Empty;       //记录区域绘制的第一个点
        private Point areaPoint2 = Point.Empty;       //记录区域绘制的第二个点

        #region 动态绘制区域矩形、测量线(根据鼠标移动轨迹)0

        private void pictureBox1_MouseMove0(object sender, MouseEventArgs e)
        {

        }

        #endregion

        #region 检测鼠标释放(鼠标在pictureBox1之外释放，区域矩形绘制完成)

        //动态选择局部曲线时，鼠标在pictureBox1范围外且Panel1范围内释放时触发
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!leftButtonRelease && e.Button == MouseButtons.None)
            {
                leftButtonRelease = true;           //标记鼠标为未按下状态
                //UpdateValidCurveInfo();             //处于动态选泽状态且鼠标释放，更新所选区域信息
            }
        }

        #endregion

        #region 鼠标左键双击 -- 清除区域矩形0

        //左键双击清除区域矩形
        private void pictureBox1_MouseDoubleClick0(object sender, MouseEventArgs e)
        {

        }

        #endregion

        #region 鼠标右键单击 -- 调整区域矩形0

        //右键单击微调区域矩形
        private void pictureBox1_MouseClick0(object sender, MouseEventArgs e)
        {

        }

        #endregion

        #region 鼠标离开pictureBox1后测量线消失

        //鼠标离开pictureBox1后测量线消失
        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            //if (!MyDefine.myXET.meDebugMode) return;  //非调试模式，不绘制测量线
            //pictureBox1.Refresh();
            toolTip1.Hide(pictureBox1);
        }

        #endregion

        #region 计算鼠标所在位置对应的曲线XY坐标(用于绘制测量线)

        //将鼠标在pictureBox1上的坐标转换为曲线数据的XY坐标字符串
        private String getAxisVal(int x, int y)
        {
            try
            {
                if (MyDefine.myXET.meDataTbl == null) return "";

                string strMeas = "";
                //Int32 dataNum = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;              //数据总个数
                //Int32 index = (int)((1.0 * x / pictureBox1.Width) * dataNum);               //鼠标当前位置对应的数据索引
                //if (index > dataNum) return "";
                Int32 dataNum = indexEnd - indexStart;              //数据总个数
                Int32 index = Convert.ToInt32((1.0 * x / pictureBox1.Width) * dataNum) + indexStart;               //鼠标当前位置对应的数据索引
                if (index > indexEnd) return "";
                DateTime mouseX = Convert.ToDateTime(MyDefine.myXET.meDataTbl.GetCellValue(index, 0));    //当前鼠标所在日期
                string MeasX = mouseX.ToString("yy-MM-dd HH:mm:ss");  //X轴--时间轴
                //string MeasX = myStartTime.AddSeconds(myTestSeconds * x / pictureBox1.Width).ToString("yy-MM-dd HH:mm:ss");  //X轴--时间轴
                if (drawPicture)
                {
                    if (this.tempSpan != double.MaxValue)
                    {
                        double unitTempHeight = pictureBox1.Height / tempSpanZS;     //每单位℃在pictureBox1上的高度
                        double MeasY = (pictureBox1.Height - y) / unitTempHeight + meTmpMinZS; //Y轴--温度轴 //计算鼠标所在位置的温度值：根据y1 = pictureBox1.Height - (int)((myTemp1 - meTmpMin) * unitTempHeight)反推的计算公式
                        strMeas += "(" + MeasX + ", " + MeasY.ToString("F2") + " ℃)" + Environment.NewLine;
                    }

                    if (this.humSpan != double.MaxValue)
                    {
                        double unitHumHeight = pictureBox1.Height / humSpanZS;     //每单位℃在pictureBox1上的高度
                        double MeasY = (pictureBox1.Height - y) / unitHumHeight + meHumMinZS; //Y轴--温度轴 //计算鼠标所在位置的温度值：根据y1 = pictureBox1.Height - (int)((myTemp1 - meTmpMin) * unitTempHeight)反推的计算公式
                        strMeas += "(" + MeasX + ", " + MeasY.ToString("F2") + " %)" + Environment.NewLine;
                    }

                    if (this.prsSpan != double.MaxValue)
                    {
                        double unitPrsHeight = pictureBox1.Height / prsSpanZS;     //每单位℃在pictureBox1上的高度
                        double MeasY = (pictureBox1.Height - y) / unitPrsHeight + mePrsMinZS; //Y轴--温度轴 //计算鼠标所在位置的温度值：根据y1 = pictureBox1.Height - (int)((myTemp1 - meTmpMin) * unitTempHeight)反推的计算公式
                        strMeas += "(" + MeasX + ", " + MeasY.ToString("F2") + " kPa)" + Environment.NewLine;
                    }
                }
                else
                {
                    if (this.tempSpan != double.MaxValue)
                    {
                        double unitTempHeight = pictureBox1.Height / tempSpan;     //每单位℃在pictureBox1上的高度
                        double MeasY = (pictureBox1.Height - y) / unitTempHeight + meTmpMin; //Y轴--温度轴 //计算鼠标所在位置的温度值：根据y1 = pictureBox1.Height - (int)((myTemp1 - meTmpMin) * unitTempHeight)反推的计算公式
                        strMeas += "(" + MeasX + ", " + MeasY.ToString("F2") + " ℃)" + Environment.NewLine;
                    }

                    if (this.humSpan != double.MaxValue)
                    {
                        double unitHumHeight = pictureBox1.Height / humSpan;     //每单位℃在pictureBox1上的高度
                        double MeasY = (pictureBox1.Height - y) / unitHumHeight + meHumMin; //Y轴--温度轴 //计算鼠标所在位置的温度值：根据y1 = pictureBox1.Height - (int)((myTemp1 - meTmpMin) * unitTempHeight)反推的计算公式
                        strMeas += "(" + MeasX + ", " + MeasY.ToString("F2") + " %)" + Environment.NewLine;
                    }

                    if (this.prsSpan != double.MaxValue)
                    {
                        double unitPrsHeight = pictureBox1.Height / prsSpan;     //每单位℃在pictureBox1上的高度
                        double MeasY = (pictureBox1.Height - y) / unitPrsHeight + mePrsMin; //Y轴--温度轴 //计算鼠标所在位置的温度值：根据y1 = pictureBox1.Height - (int)((myTemp1 - meTmpMin) * unitTempHeight)反推的计算公式
                        strMeas += "(" + MeasX + ", " + MeasY.ToString("F2") + " kPa)" + Environment.NewLine;
                    }
                }


                #region 变量定义

                Double myVal = 0;               //测试值
                String myType = "";             //数据类型
                int tmpMaxIdx = 0;              //行温度最大值的列索引
                int tmpMinIdx = 0;              //行温度最小值的列索引
                int humMaxIdx = 0;              //行湿度最大值的列索引
                int humMinIdx = 0;              //行湿度最小值的列索引
                int prsMaxIdx = 0;              //行压力最大值的列索引
                int prsMinIdx = 0;              //行压力最小值的列索引

                Double myTMPMax = Double.MinValue;      //行温度最大值
                Double myTMPMin = Double.MaxValue;      //行温度最小值
                Double myHUMMax = Double.MinValue;      //行湿度最大值
                Double myHUMMin = Double.MaxValue;      //行湿度最小值
                Double myPRSMax = Double.MinValue;      //行压力最大值
                Double myPRSMin = Double.MaxValue;      //行压力最小值
                String myTime = MyDefine.myXET.meDataTbl.GetCellValue(index, 0);         //测试时间

                #endregion

                #region 计算行温度(/湿度/压力)最大最小值

                //遍历每一列，计算本行的温度/湿度/压力的最大最小值
                for (int j = 1; j < MyDefine.myXET.meDataTbl.dataTable.Columns.Count; j++)        //meDataTbl第0列为Time
                {
                    if (myLabelColor[j - 1].Text == string.Empty) continue;                    //曲线为非选中状态，不列入最大最小值计算
                    if (MyDefine.myXET.meDataTbl.GetCellValue(index, j) == "") continue;       //空数据
                    myType = MyDefine.myXET.meTypeList[j];                                     //数据类型
                    myVal = Convert.ToDouble(MyDefine.myXET.meDataTbl.GetCellValue(index, j)); //测试值

                    //计算最大最小值
                    switch (myType)
                    {
                        case "TT_T":
                        case "TH_T":
                        case "TQ_T":
                            if (myTMPMax < myVal) { myTMPMax = myVal; tmpMaxIdx = j; }
                            if (myTMPMin > myVal) { myTMPMin = myVal; tmpMinIdx = j; }
                            break;

                        case "TH_H":
                        case "TQ_H":
                            if (myHUMMax < myVal) { myHUMMax = myVal; humMaxIdx = j; }
                            if (myHUMMin > myVal) { myHUMMin = myVal; humMinIdx = j; }
                            break;

                        case "TP_P":
                            if (myPRSMax < myVal) { myPRSMax = myVal; prsMaxIdx = j; }
                            if (myPRSMin > myVal) { myPRSMin = myVal; prsMinIdx = j; }
                            break;
                    }
                }

                #endregion

                #region 添加温度(/湿度/压力)最大最小值文字

                if (this.tempSpan != double.MaxValue)
                {
                    strMeas += "温度最大值：" + myTMPMax + ", 探头编号" + MyDefine.myXET.meJSNList[tmpMaxIdx] + Environment.NewLine;
                    strMeas += "温度最小值：" + myTMPMin + ", 探头编号" + MyDefine.myXET.meJSNList[tmpMinIdx] + Environment.NewLine;
                }

                if (this.humSpan != double.MaxValue)
                {
                    strMeas += "湿度最大值：" + myHUMMax + ", 探头编号" + MyDefine.myXET.meJSNList[humMaxIdx] + Environment.NewLine;
                    strMeas += "湿度最小值：" + myHUMMin + ", 探头编号" + MyDefine.myXET.meJSNList[humMinIdx] + Environment.NewLine;
                }

                if (this.prsSpan != double.MaxValue)
                {
                    strMeas += "压力最大值：" + myPRSMax + ", 探头编号" + MyDefine.myXET.meJSNList[prsMaxIdx] + Environment.NewLine;
                    strMeas += "压力最小值：" + myPRSMin + ", 探头编号" + MyDefine.myXET.meJSNList[prsMinIdx] + Environment.NewLine;
                }

                #endregion

                return strMeas.Trim('\n');
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
                return string.Empty;
            }
        }

        #endregion

        #region 绘制半透明区域矩形

        //绘制透明的矩形选择区域
        public void DrawSelectArea()
        {
            try
            {
                if (areaPoint1 == Point.Empty || areaPoint2 == Point.Empty) return;     //没有选中区域，退出


                //(==========勿删==========)
                //必须先调整Label位置再绘图，否则可能出现小白框(lable原先所在的位置阻止的绘图呈现)
                //====================================================================
                //显示文字：有效数据开始、有效数据结束
                label62.Visible = true;
                label63.Visible = true;

                //文字显示矩形框内侧
                //label62.Location = new Point(pictureBox1.Location.X + areaPoint1.X + 2, panel1.Location.Y + pictureBox1.Height - label62.Size.Height);
                //label63.Location = new Point(pictureBox1.Location.X + areaPoint2.X - label63.Size.Width, panel1.Location.Y + pictureBox1.Height - label63.Size.Height);

                //文字显示矩形框外侧
                label62.Location = new Point(pictureBox1.Location.X + areaPoint1.X - label62.Size.Width, panel1.Location.Y + pictureBox1.Height - label62.Size.Height);
                label63.Location = new Point(pictureBox1.Location.X + areaPoint2.X + 2, panel1.Location.Y + pictureBox1.Height - label63.Size.Height);


                //====================================================================
                //画布
                //AreaBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                Graphics myArea = pictureBox1.CreateGraphics();
                pictureBox1.Refresh();

                //画矩形选择区域①：有效数据区域为透明蓝矩形框(矩形框高度为鼠标绘制高度)
                //myArea.DrawRectangle(new Pen(Color.SkyBlue), new Rectangle(areaPoint1, new Size(areaPoint2.X - areaPoint1.X, areaPoint2.Y - areaPoint1.Y)));
                //myArea.FillRectangle(new SolidBrush(Color.FromArgb(125, Color.LightBlue)), new RectangleF(areaPoint1, new Size(areaPoint2.X - areaPoint1.X, areaPoint2.Y - areaPoint1.Y)));

                //画矩形选择区域②：有效数据区域为透明蓝矩形框(矩形框高度为pictureBox1高度)
                myArea.DrawRectangle(new Pen(Color.LightBlue), new Rectangle(areaPoint1.X, -1, areaPoint2.X - areaPoint1.X, pictureBox1.Height + 1));
                myArea.FillRectangle(new SolidBrush(Color.FromArgb(60, Color.LightBlue)), new Rectangle(areaPoint1.X, -1, areaPoint2.X - areaPoint1.X, pictureBox1.Height + 1));

                ////画矩形选择区域③：有效数据区域为白色，周围为透明灰
                //myArea.FillRectangle(new SolidBrush(Color.FromArgb(60, Color.LightGray)), new Rectangle(0, -1, areaPoint1.X, pictureBox1.Height + 1));
                //myArea.FillRectangle(new SolidBrush(Color.FromArgb(80, Color.LightGray)), new Rectangle(areaPoint2.X, -1, pictureBox1 .Width - areaPoint2.X, pictureBox1.Height + 1));
                //myArea.DrawRectangle(new Pen(Color.Gray), new Rectangle(areaPoint1.X, -1, areaPoint2.X - areaPoint1.X, pictureBox1.Height + 1));

            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        #endregion

        #region 更新曲线信息、有效数据信息(更新为区域矩形内的曲线信息：开始时间、结束时间、持续时间)0

        //更新所选区域信息
        private void UpdateSelectedCurveInfo0()
        {
            try
            {
                if (areaPoint1 == Point.Empty || areaPoint2 == Point.Empty) return;     //没有选中区域，退出
                if (MyDefine.myXET.meDataTbl == null) return;                             //meDataTbl为空，退出

                firstPoint = new Point(0, 0);
                areaPoint1.X = (areaPoint1.X < 0) ? 0 : areaPoint1.X;
                areaPoint1.X = (areaPoint1.X > pictureBox1.Width) ? pictureBox1.Width : areaPoint1.X;
                areaPoint2.X = (areaPoint2.X < 0) ? 0 : areaPoint2.X;
                areaPoint2.X = (areaPoint2.X > pictureBox1.Width) ? pictureBox1.Width : areaPoint2.X;

                if (areaPoint2.X - areaPoint1.X >= 10)   //防抖，防止双击时出现提示框
                {
                    //if (MessageBox.Show("是否保存选中区域内的曲线数据？", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                        //更新有效数据开始结束索引
                        int rowNum = MyDefine.myXET.meDataTbl.dataTable.Rows.Count - 1;
                        int startIdx = (int)((1.0 * areaPoint1.X / pictureBox1.Width) * MyDefine.myXET.meDataTbl.dataTable.Rows.Count);
                        int stopIdx = (int)((1.0 * areaPoint2.X / pictureBox1.Width) * MyDefine.myXET.meDataTbl.dataTable.Rows.Count);
                        if (stopIdx > rowNum) stopIdx = rowNum;

                        DateTime startTime = Convert.ToDateTime(MyDefine.myXET.meDataTbl.GetCellValue(startIdx, 0));
                        DateTime stopTime = Convert.ToDateTime(MyDefine.myXET.meDataTbl.GetCellValue(stopIdx, 0));
                        MyDefine.myXET.meValidStartIdx = startIdx;        //记录有效数据开始索引
                        MyDefine.myXET.meValidStopIdx = stopIdx;          //记录有效数据结束索引
                        MyDefine.myXET.meValidStartTime = startTime;      //记录有效数据开始时间
                        MyDefine.myXET.meValidStopTime = stopTime;        //记录有效数据结束时间

                        label64.Text = "开始时间：" + startTime.ToString("MM-dd HH:mm:ss");
                        label65.Text = "结束时间：" + stopTime.ToString("MM-dd HH:mm:ss");
                        label66.Text = "持续时间：" + ((stopTime.Subtract(startTime).TotalSeconds) / 60.0).ToString("F2") + "min";

                        //label64.Text = "开始：" + startTime.ToString("MM-dd HH:mm:ss");
                        //label65.Text = "结束：" + stopTime.ToString("MM-dd HH:mm:ss");
                        //label66.Text = "持续时间：" + ((stopTime.Subtract(startTime).TotalSeconds) / 60.0).ToString("F2") + " min";

                    }
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        #endregion

        #endregion

        #region 右键设置有效区域

        private int mouseX = 0;                 //有效数据设定点的X坐标
        private int mouseY = 0;
        private String[] stage = new String[8];

        #region 添加新阶段
        private void 添加阶段ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MyDefine.myXET.AddTraceInfo("添加阶段");

            int num = ++MyDefine.myXET.meActivePn;
            if (MyDefine.myXET.meValidNameList.Count <= num)
            {
                MyDefine.myXET.meValidNameList.Add("有效数据");          //添加空的阶段n的名称
                MyDefine.myXET.meValidSetValueList.Add(double.MinValue);               //添加空的阶段n的设定温度
                MyDefine.myXET.meValidSetValueList.Add(double.MinValue);
                MyDefine.myXET.meValidUpperList.Add(double.MinValue);                  //添加空的阶段n的纵坐标上限温度
                MyDefine.myXET.meValidUpperList.Add(double.MinValue);
                MyDefine.myXET.meValidLowerList.Add(double.MinValue);                  //添加空的阶段n的纵坐标下限温度
                MyDefine.myXET.meValidLowerList.Add(double.MinValue);
                cb_selectStage.Items.Add("有效数据P" + (num + 1));
            }
            else
            {
                --MyDefine.myXET.meActivePn;
            }
            //初始化变量
            tb_lowerValueLeft.Text = "";
            tb_upperValueLeft.Text = "";
            tb_setValueLeft.Text = "";
            tb_lowerValueRight.Text = "";
            tb_upperValueRight.Text = "";
            tb_setValueRight.Text = "";

            addStageToolStripMenuItem.Visible = false;
        }
        #endregion

        #region 设置阶段名称

        //点击回车键隐藏输入框
        private void tb_setName_KeyUp(object sender, KeyEventArgs e)
        {
            //设置阶段名称
            int index = MyDefine.myXET.meActivePn;
            if (tb_setName.Text != "" && tb_setName.Text != null)
            {
                MyDefine.myXET.meValidNameList[index] = tb_setName.Text;

                //显示可能存在的有效数据竖直线
                ShowValidLines();

                //添加选择阶段信息
                cb_selectStage.Items.Insert(index, tb_setName.Text);
                cb_selectStage.Items.RemoveAt(index + 1);

                //同步更新数据处理界面有效开始、结束行的高亮状态
                MyDefine.myXET.switchMainPanel(23);
            }
        }

        //阶段名称
        private void tb_setName_Click(object sender, EventArgs e)
        {
            tb_setName.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            MyDefine.myXET.AddTraceInfo("设置阶段名称");
            int index = MyDefine.myXET.meActivePn;
            if (MyDefine.myXET.meValidNameList[index] == "有效数据" || tb_setName.Text == "")
            {
                tb_setName.Text = "";
                tb_setName.ForeColor = Color.Black;
            }
        }
        #endregion

        #region 设置起点

        /// <summary>
        /// 设置起点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 设为起点ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            groupBox1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            if (this.Name == "验证曲线") return;                         //验证曲线禁止手动设置开始、结束时间

            if (MyDefine.myXET.meDataTbl == null)                        //数据列表为空
            {
                MessageBox.Show("尚未加载数据，设置无效！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //计算当前设置点在数据表中的索引
            Int32 listIdx = MyDefine.myXET.meActivePn * 2;                            //当前开始、结束索引分别为listIdx、listIdx+1
            Int32 dataNum = indexEnd - indexStart; //MyDefine.myXET.meDataTbl.dataTable.Rows.Count;            //数据总个数
            Int32 startIndex = Convert.ToInt32((1.0 * mouseX / pictureBox1.Width) * dataNum) + indexStart;   //有效开始索引(当前鼠标所在位置)
            Int32 stopIndex = MyDefine.myXET.meValidIdxList[listIdx + 1];             //有效结束索引
            DateTime startTime = Convert.ToDateTime(MyDefine.myXET.meDataTbl.GetCellValue(startIndex, 0));    //当前鼠标所在日期

            if (stopIndex != -1 && startIndex >= stopIndex)     //结束索引已设置，且开始索引大于结束索引
            {
                MessageBox.Show("有效数据开始时间大于结束时间，设置无效！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            for (int i = 0; i < MyDefine.myXET.meValidIdxList.Count; i += 2)
            {
                if (listIdx == i)
                {
                    continue;
                }
                if (startIndex > MyDefine.myXET.meValidIdxList[i])
                {
                    if (startIndex < MyDefine.myXET.meValidIdxList[i + 1])
                    {
                        MessageBox.Show("阶段不能嵌套，设置无效！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                }
            }

            //更新有效开始数据
            MyDefine.myXET.AddTraceInfo("设置起点");
            MyDefine.myXET.meValidIdxList[listIdx] = startIndex;         //记录阶段n有效数据开始索引
            MyDefine.myXET.meValidTimeList[listIdx] = startTime;         //记录阶段n有效数据开始时间
            //MyDefine.myXET.meValidStartIdx = startIndex;         //记录阶段n有效数据开始时间
            //MyDefine.myXET.meValidStartTime = startTime;         //记录有效数据开始时间

            pictureBox1.Refresh();
            UnlockValidStageList();                               //根据有效列表解锁必要的阶段列表Pn
            ShowValidLines();                                     //显示可能存在的有效数据竖直线
            ShowValidInfo();                                      //显示右侧有效数据信息
            MyDefine.myXET.switchMainPanel(23);                   //同步更新数据处理界面有效开始、结束行的高亮状态

            //更新区域最高最低值
            if (stopIndex != -1)
            {
                updateMaxMin();

                //自动保存阶段
                保存阶段ToolStripMenuItem_Click(sender, e);
            }

            //DrawDataValidLines();
            //MyDefine.myXET.AddTraceInfo("设置起点");
        }

        #endregion

        #region 设置终点

        /// <summary>
        /// 设置终点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 设为终点ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            groupBox1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            if (this.Name == "验证曲线") return;                         //验证曲线禁止手动设置开始、结束时间

            if (MyDefine.myXET.meDataTbl == null)                        //数据列表为空
            {
                MessageBox.Show("尚未加载数据，设置无效！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //计算当前设置点在数据表中的索引
            Int32 listIdx = MyDefine.myXET.meActivePn * 2;                            //当前开始、结束索引分别为listIdx、listIdx+1
            Int32 dataNum = indexEnd - indexStart;//MyDefine.myXET.meDataTbl.dataTable.Rows.Count;
            Int32 startIndex = MyDefine.myXET.meValidIdxList[listIdx];                //有效开始索引
            Int32 stopIndex = Convert.ToInt32((1.0 * mouseX / pictureBox1.Width) * dataNum) + indexStart;    //有效结束索引(当前鼠标所在位置)
            DateTime stopTime = Convert.ToDateTime(MyDefine.myXET.meDataTbl.GetCellValue(stopIndex, 0));

            if (startIndex != -1 && startIndex >= stopIndex)     //结束索引已设置，且开始索引大于结束索引
            {
                MessageBox.Show("有效数据开始时间大于结束时间，设置无效！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            for (int i = 0; i < MyDefine.myXET.meValidIdxList.Count; i += 2)
            {
                if (listIdx == i)
                {
                    continue;
                }

                if (stopIndex > MyDefine.myXET.meValidIdxList[i])
                {
                    if (stopIndex < MyDefine.myXET.meValidIdxList[i + 1])
                    {
                        MessageBox.Show("阶段不能嵌套，设置无效！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                }
            }

            //更新有效结束数据
            MyDefine.myXET.AddTraceInfo("设置终点");
            MyDefine.myXET.meValidIdxList[listIdx + 1] = stopIndex;      //记录阶段n有效数据结束索引
            MyDefine.myXET.meValidTimeList[listIdx + 1] = stopTime;      //记录阶段n有效数据结束时间
            //MyDefine.myXET.meValidStopIdx = stopIndex;          //记录有效数据结束索引
            //MyDefine.myXET.meValidStopTime = stopTime;          //记录有效数据结束时间

            pictureBox1.Refresh();
            UnlockValidStageList();                               //根据有效列表解锁必要的阶段列表Pn
            ShowValidLines();                                     //显示可能存在的有效数据竖直线
            ShowValidInfo();                                      //显示右侧有效数据信息
            MyDefine.myXET.switchMainPanel(23);                   //同步更新数据处理界面有效开始、结束行的高亮状态

            //更新区域最高最低值
            if (startIndex != -1)
            {
                updateMaxMin();

                //自动保存阶段
                保存阶段ToolStripMenuItem_Click(sender, e);
            }
            //DrawDataValidLines();
            //MyDefine.myXET.AddTraceInfo("设置终点");
        }

        //更新区域最高最低值
        public void updateMaxMin()
        {
            #region 变量定义

            Double myVal = 0;               //测试值
            String myType = "";             //数据类型
            int index = MyDefine.myXET.meActivePn;//当前正在编辑的阶段Pn

            Double myTMPMax = Double.MinValue;      //行温度最大值
            Double myTMPMin = Double.MaxValue;      //行温度最小值
            Double myHUMMax = Double.MinValue;      //行湿度最大值
            Double myHUMMin = Double.MaxValue;      //行湿度最小值
            Double myPRSMax = Double.MinValue;      //行压力最大值
            Double myPRSMin = Double.MaxValue;      //行压力最小值

            #endregion

            #region 添加新阶段左右轴的最大最小值
            if (MyDefine.myXET.meLeftMaxMinList.Count < MyDefine.myXET.meValidStageNum * 2)
            {
                MyDefine.myXET.meLeftMaxMinList.Add(Double.MinValue);
                MyDefine.myXET.meLeftMaxMinList.Add(Double.MaxValue);
                MyDefine.myXET.meRightMaxMinList.Add(Double.MinValue);
                MyDefine.myXET.meRightMaxMinList.Add(Double.MaxValue);
            }
            #endregion

            #region 计算行温度(/湿度/压力)最大最小值
            for (int i = MyDefine.myXET.meValidIdxList[2 * index]; i <= MyDefine.myXET.meValidIdxList[2 * index + 1]; i++)
            {
                //遍历每一列，计算本行的温度/湿度/压力的最大最小值
                for (int j = 1; j < MyDefine.myXET.meDataTbl.dataTable.Columns.Count; j++)        //meDataTbl第0列为Time
                {
                    if (myLabelColor[j - 1].Text == string.Empty) continue;                    //曲线为非选中状态，不列入最大最小值计算
                    if (MyDefine.myXET.meDataTbl.GetCellValue(i, j) == "") continue;       //空数据
                    myType = MyDefine.myXET.meTypeList[j];                                     //数据类型
                    myVal = Convert.ToDouble(MyDefine.myXET.meDataTbl.GetCellValue(i, j)); //测试值

                    //计算最大最小值
                    switch (myType)
                    {
                        case "TT_T":
                        case "TH_T":
                        case "TQ_T":
                            if (myTMPMax < myVal) { myTMPMax = myVal; }
                            if (myTMPMin > myVal) { myTMPMin = myVal; }
                            break;

                        case "TH_H":
                        case "TQ_H":
                            if (myHUMMax < myVal) { myHUMMax = myVal; }
                            if (myHUMMin > myVal) { myHUMMin = myVal; }
                            break;

                        case "TP_P":
                            if (myPRSMax < myVal) { myPRSMax = myVal; }
                            if (myPRSMin > myVal) { myPRSMin = myVal; }
                            break;
                    }
                }
            }
            #endregion

            #region 阶段数据最大最小值
            tabPageLeft.Tag = true;
            tabPageRight.Tag = true;
            if (myTMPMax != double.MinValue)//温度
            {
                tabPageLeft.Tag = true;
                tabControl1.SelectedIndex = 0;
                MyDefine.myXET.meLeftMaxMinList[2 * index] = myTMPMax;
                MyDefine.myXET.meLeftMaxMinList[2 * index + 1] = myTMPMin;
            }
            else
            {
                tabPageLeft.Tag = false;
                tabControl1.SelectedIndex = 1;
            }
            if (isPsr)  //压力
            {
                if (myPRSMax != double.MinValue) tabPageRight.Tag = true;
                else tabPageRight.Tag = false;
                MyDefine.myXET.meRightMaxMinList[2 * index] = myPRSMax;
                MyDefine.myXET.meRightMaxMinList[2 * index + 1] = myPRSMin;
            }
            else    //湿度
            {
                if (myHUMMax != double.MinValue) tabPageRight.Tag = true;
                else tabPageRight.Tag = false;
                MyDefine.myXET.meRightMaxMinList[2 * index] = myHUMMax;
                MyDefine.myXET.meRightMaxMinList[2 * index + 1] = myHUMMin;
            }
            #endregion
        }

        /// <summary>
        /// 阶段自定义左右轴上下限，page页是否可用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPage.Tag is Boolean)
            {
                if (e.TabPage.Tag != null && !(bool)e.TabPage.Tag) e.Cancel = true;
            }
        }
        #endregion

        #region 保存阶段设置
        private void 保存阶段ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            groupBox1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            int index = 2 * MyDefine.myXET.meActivePn;

            #region 设置左右轴阶段性上下限、设定值
            if ((bool)tabPageLeft.Tag)
            {
                //左轴
                tb_upperValueLeft.Text = (MyDefine.myXET.meLeftMaxMinList[index] + 5).ToString("F2");
                tb_lowerValueLeft.Text = (MyDefine.myXET.meLeftMaxMinList[index + 1] - 5).ToString("F2");
                tb_setValueLeft.Text = ((Convert.ToDouble(tb_upperValueLeft.Text) + Convert.ToDouble(tb_lowerValueLeft.Text)) / 2).ToString("F2");

                //添加阶段信息
                MyDefine.myXET.meValidSetValueList[index] = Convert.ToDouble(tb_setValueLeft.Text);
                MyDefine.myXET.meValidUpperList[index] = Convert.ToDouble(tb_upperValueLeft.Text);
                MyDefine.myXET.meValidLowerList[index] = Convert.ToDouble(tb_lowerValueLeft.Text);
            }

            if ((bool)tabPageRight.Tag)
            {
                tb_upperValueRight.Text = (MyDefine.myXET.meRightMaxMinList[index] + 5).ToString("F2");
                tb_lowerValueRight.Text = (MyDefine.myXET.meRightMaxMinList[index + 1] - 5).ToString("F2");
                tb_setValueRight.Text = ((Convert.ToDouble(tb_upperValueRight.Text) + Convert.ToDouble(tb_lowerValueRight.Text)) / 2).ToString("F2");

                //添加阶段信息
                MyDefine.myXET.meValidSetValueList[index + 1] = Convert.ToDouble(tb_setValueRight.Text);
                MyDefine.myXET.meValidUpperList[index + 1] = Convert.ToDouble(tb_upperValueRight.Text);
                MyDefine.myXET.meValidLowerList[index + 1] = Convert.ToDouble(tb_lowerValueRight.Text);
            }

            #endregion

            //添加选择阶段信息
            MyDefine.myXET.AddTraceInfo("保存阶段");
            cb_selectStage.Items.Insert(index / 2, MyDefine.myXET.meValidNameList[index / 2]);
            cb_selectStage.Items.RemoveAt(index / 2 + 1);

            //添加阶段启用
            if (MyDefine.myXET.meValidNameList.Count == MyDefine.myXET.meActivePn + 1)
            {
                addStageToolStripMenuItem.Visible = true;
            }
        }
        #endregion

        #region 修改阶段设定值、上下限值

        /// <summary>
        /// 阶段左轴设定值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb_setValueLeft_Leave(object sender, EventArgs e)
        {
            int index = 2 * MyDefine.myXET.meActivePn;

            if (tb_setValueLeft.Text == "")
            {
                return;
            }

            if (tb_lowerValueLeft.Text == "" || tb_upperValueLeft.Text == "")
            {
                MessageBox.Show("请先输入上下限值");
                tb_setValueLeft.Text = "";
                return;
            }

            if (Convert.ToDouble(tb_lowerValueLeft.Text) > Convert.ToDouble(tb_setValueLeft.Text) || Convert.ToDouble(tb_upperValueLeft.Text) < Convert.ToDouble(tb_setValueLeft.Text))
            {
                MessageBox.Show("左轴设定值输入有误，要求:上限值 > 设定值 > 下限值");
                tb_setValueLeft.Text = "";
            }
            else
            {
                //添加阶段信息
                MyDefine.myXET.meValidSetValueList[index] = Convert.ToDouble(tb_setValueLeft.Text);
            }
        }

        /// <summary>
        /// 阶段左轴上限值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb_upperValueLeft_Leave(object sender, EventArgs e)
        {
            int index = 2 * MyDefine.myXET.meActivePn;

            if (tb_upperValueLeft.Text == "")
            {
                return;
            }

            if (Convert.ToDouble(tb_upperValueLeft.Text) < MyDefine.myXET.meLeftMaxMinList[index])
            {
                MessageBox.Show("左轴上限值输入有误，要求数值大于该阶段所有数据的最大值");
                tb_upperValueLeft.Text = "";
            }
            else
            {
                //添加阶段信息
                MyDefine.myXET.meValidUpperList[index] = Convert.ToDouble(tb_upperValueLeft.Text);
            }
        }

        /// <summary>
        /// 阶段左轴下限值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb_lowerValueLeft_Leave(object sender, EventArgs e)
        {
            int index = 2 * MyDefine.myXET.meActivePn;

            if (tb_lowerValueLeft.Text == "")
            {
                return;
            }

            if (Convert.ToDouble(tb_lowerValueLeft.Text) > MyDefine.myXET.meLeftMaxMinList[index + 1])
            {
                MessageBox.Show("左轴下限值输入有误，要求数值大于该阶段所有数据的最大值");
                tb_lowerValueLeft.Text = "";
            }
            else
            {
                //添加阶段信息
                MyDefine.myXET.meValidLowerList[index] = Convert.ToDouble(tb_lowerValueLeft.Text);
            }
        }

        /// <summary>
        /// 阶段右轴设定值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb_setValueRight_Leave(object sender, EventArgs e)
        {
            int index = 2 * MyDefine.myXET.meActivePn;

            if (tb_setValueRight.Text == "")
            {
                return;
            }

            if (tb_lowerValueRight.Text == "" || tb_upperValueRight.Text == "")
            {
                MessageBox.Show("请先输入上下限值");
                tb_setValueRight.Text = "";
                return;
            }

            if (Convert.ToDouble(tb_lowerValueRight.Text) > Convert.ToDouble(tb_setValueRight.Text) || Convert.ToDouble(tb_upperValueRight.Text) < Convert.ToDouble(tb_setValueRight.Text))
            {
                MessageBox.Show("右轴设定值输入有误，要求:上限值 > 设定值 > 下限值");
                tb_setValueRight.Text = "";
            }
            else
            {
                //添加阶段信息
                MyDefine.myXET.meValidSetValueList[index + 1] = Convert.ToDouble(tb_setValueRight.Text);
            }
        }

        /// <summary>
        /// 阶段右轴上限值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb_upperValueRight_Leave(object sender, EventArgs e)
        {
            int index = 2 * MyDefine.myXET.meActivePn;

            if (tb_upperValueRight.Text == "")
            {
                return;
            }

            if (Convert.ToDouble(tb_upperValueRight.Text) < MyDefine.myXET.meRightMaxMinList[index])
            {
                MessageBox.Show("右轴上限值输入有误，要求数值大于该阶段所有数据的最大值");
                tb_upperValueRight.Text = "";
            }
            else
            {
                //添加阶段信息
                MyDefine.myXET.meValidUpperList[index + 1] = Convert.ToDouble(tb_upperValueRight.Text);
            }
        }

        /// <summary>
        /// 阶段右轴下限值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb_lowerValueRight_Leave(object sender, EventArgs e)
        {
            int index = 2 * MyDefine.myXET.meActivePn;

            if (tb_lowerValueRight.Text == "")
            {
                return;
            }

            if (Convert.ToDouble(tb_lowerValueRight.Text) > MyDefine.myXET.meRightMaxMinList[index + 1])
            {
                MessageBox.Show("右轴下限值输入有误，要求数值大于该阶段所有数据的最大值");
                tb_lowerValueRight.Text = "";
            }
            else
            {
                //添加阶段信息
                MyDefine.myXET.meValidLowerList[index + 1] = Convert.ToDouble(tb_lowerValueRight.Text);
            }
        }

        #endregion

        #region 删除阶段
        private void 删除阶段ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            groupBox1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            int index = MyDefine.myXET.meActivePn;
            if (index < MyDefine.myXET.meValidStageNum)
            {
                if (MessageBox.Show("是否删除" + cb_selectStage.Text + "阶段", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    #region 删除数据
                    MyDefine.myXET.meValidNameList.RemoveAt(index);          //添加空的阶段n的名称
                    MyDefine.myXET.meValidSetValueList.RemoveAt(index * 2);               //添加空的阶段n的设定温度
                    MyDefine.myXET.meValidSetValueList.RemoveAt(index * 2);
                    MyDefine.myXET.meValidUpperList.RemoveAt(index * 2);                  //添加空的阶段n的纵坐标上限温度
                    MyDefine.myXET.meValidUpperList.RemoveAt(index * 2);
                    MyDefine.myXET.meValidLowerList.RemoveAt(index * 2);                  //添加空的阶段n的纵坐标下限温度
                    MyDefine.myXET.meValidLowerList.RemoveAt(index * 2);
                    MyDefine.myXET.meLeftMaxMinList.RemoveAt(index * 2);                  //左轴有效区域最大最小值列表
                    MyDefine.myXET.meLeftMaxMinList.RemoveAt(index * 2);
                    MyDefine.myXET.meRightMaxMinList.RemoveAt(index * 2);                 //右轴有效区域最大最小值列表
                    MyDefine.myXET.meRightMaxMinList.RemoveAt(index * 2);

                    MyDefine.myXET.meValidIdxList.RemoveAt(index * 2);      //记录阶段n有效数据开始索引
                    MyDefine.myXET.meValidIdxList.RemoveAt(index * 2);      //记录阶段n有效数据结束索引
                    MyDefine.myXET.meValidTimeList.RemoveAt(index * 2);     //记录阶段n有效数据开始时间
                    MyDefine.myXET.meValidTimeList.RemoveAt(index * 2);     //记录阶段n有效数据结束时间
                    MyDefine.myXET.meValidStageNum--;
                    cb_selectStage.Items.RemoveAt(index);                                   //移除列表里的阶段信息
                    #endregion

                    #region 判断是否还有阶段，没有阶段则初始化阶段

                    //复位有效数据索引
                    if (MyDefine.myXET.meValidStageNum <= 0)
                    {
                        MyDefine.myXET.meActivePn = 0;
                        MyDefine.myXET.meValidIdxList.Clear();
                        MyDefine.myXET.meValidTimeList.Clear();
                        MyDefine.myXET.meValidNameList.Clear();
                        MyDefine.myXET.meValidSetValueList.Clear();
                        MyDefine.myXET.meValidUpperList.Clear();
                        MyDefine.myXET.meValidLowerList.Clear();
                        MyDefine.myXET.meValidIdxList.Add(0);
                        MyDefine.myXET.meValidIdxList.Add(MyDefine.myXET.meStopIdx);
                        MyDefine.myXET.meValidTimeList.Add(MyDefine.myXET.meStartTime);
                        MyDefine.myXET.meValidTimeList.Add(MyDefine.myXET.meStopTime);
                        MyDefine.myXET.meValidNameList.Add("有效数据");
                        MyDefine.myXET.meValidSetValueList.Add(double.MinValue);
                        MyDefine.myXET.meValidSetValueList.Add(double.MinValue);
                        MyDefine.myXET.meValidUpperList.Add(double.MinValue);
                        MyDefine.myXET.meValidUpperList.Add(double.MinValue);
                        MyDefine.myXET.meValidLowerList.Add(double.MinValue);
                        MyDefine.myXET.meValidLowerList.Add(double.MinValue);

                        cb_selectStage.Items.Add("有效阶段P1");
                        tb_setValueLeft.Text = "";
                        tb_upperValueLeft.Text = "";
                        tb_lowerValueLeft.Text = "";
                        tb_setValueRight.Text = "";
                        tb_upperValueRight.Text = "";
                        tb_lowerValueRight.Text = "";
                    }
                    #endregion

                    #region 更新界面
                    MyDefine.myXET.meActivePn = --MyDefine.myXET.meActivePn < 0 ? 0 : MyDefine.myXET.meActivePn;

                    if (index < MyDefine.myXET.meValidStageNum)//更新右侧文本框
                    {
                        tb_setValueLeft.Text = MyDefine.myXET.meValidSetValueList[index * 2].ToString();
                        tb_upperValueLeft.Text = MyDefine.myXET.meValidUpperList[index * 2].ToString();
                        tb_lowerValueLeft.Text = MyDefine.myXET.meValidLowerList[index * 2].ToString();
                        tb_setValueRight.Text = MyDefine.myXET.meValidSetValueList[index * 2 + 1].ToString();
                        tb_upperValueRight.Text = MyDefine.myXET.meValidUpperList[index * 2 + 1].ToString();
                        tb_lowerValueRight.Text = MyDefine.myXET.meValidLowerList[index * 2 + 1].ToString();
                    }


                    MyDefine.myXET.AddTraceInfo("删除阶段");
                    UnlockValidStageList();                               //根据有效列表解锁必要的阶段列表Pn
                    ShowValidLines();                                     //显示可能存在的有效数据竖直线
                    ShowValidInfo();                                      //显示右侧有效数据信息
                    MyDefine.myXET.switchMainPanel(23);                   //同步更新数据处理界面有效开始、结束行的高亮状态
                    #endregion

                }
            }
        }
        #endregion

        #region 阶段选择Pn
        //阶段选择Pn
        private void cb_selectStage_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = cb_selectStage.SelectedIndex * 2;
            MyDefine.myXET.meActivePn = cb_selectStage.SelectedIndex;
            ShowValidInfo();

            if (MyDefine.myXET.meActivePn < MyDefine.myXET.meValidStageNum)
            {
                tabPageLeft.Tag = true;
                tabPageRight.Tag = true;
                if (MyDefine.myXET.meValidSetValueList[index] != double.MinValue)
                {
                    tabPageLeft.Tag = true;
                    tabControl1.SelectedIndex = 0;
                    tb_setValueLeft.Text = MyDefine.myXET.meValidSetValueList[index].ToString();               //显示阶段n的设定温度
                    tb_upperValueLeft.Text = MyDefine.myXET.meValidUpperList[index].ToString();                  //显示阶段n的纵坐标上限温度
                    tb_lowerValueLeft.Text = MyDefine.myXET.meValidLowerList[index].ToString();                  //显示阶段n的纵坐标下限温度
                }
                else
                {
                    tabPageLeft.Tag = false;
                    tabControl1.SelectedIndex = 1;
                    tb_setValueLeft.Text = "";
                    tb_upperValueLeft.Text = "";
                    tb_lowerValueLeft.Text = "";
                }
                if (MyDefine.myXET.meValidSetValueList[index + 1] != double.MinValue)
                {
                    tabPageRight.Tag = true;
                    tb_setValueRight.Text = MyDefine.myXET.meValidSetValueList[index + 1].ToString();               //显示阶段n的设定温度
                    tb_upperValueRight.Text = MyDefine.myXET.meValidUpperList[index + 1].ToString();                  //显示阶段n的纵坐标上限温度
                    tb_lowerValueRight.Text = MyDefine.myXET.meValidLowerList[index + 1].ToString();                  //显示阶段n的纵坐标下限温度
                }
                else
                {
                    tabPageRight.Tag = false;
                    tb_setValueRight.Text = "";
                    tb_upperValueRight.Text = "";
                    tb_lowerValueRight.Text = "";
                }
            }
            else
            {
                tb_setValueLeft.Text = "";
                tb_upperValueLeft.Text = "";
                tb_lowerValueLeft.Text = "";
                tb_setValueRight.Text = "";
                tb_upperValueRight.Text = "";
                tb_lowerValueRight.Text = "";
            }
        }

        #endregion

        #region 解锁阶段列表

        //根据有效列表解锁必要的阶段列表Pn
        public void UnlockValidStageList()
        {
            if (MyDefine.myXET.meValidIdxList.Count == 0) return;

            //若本组有效数据设置完成，则新添加一组空的有效数据
            int listNum = MyDefine.myXET.meValidIdxList.Count;
            int validIdx1 = MyDefine.myXET.meValidIdxList[listNum - 2];
            int validIdx2 = MyDefine.myXET.meValidIdxList[listNum - 1];
            if (validIdx1 != -1 && validIdx2 != -1)         //如果最后一组有效开始结束已设置完成，则添加下一组元素
            {
                MyDefine.myXET.meValidIdxList.Add(-1);
                MyDefine.myXET.meValidIdxList.Add(-1);
                MyDefine.myXET.meValidTimeList.Add(DateTime.MinValue);
                MyDefine.myXET.meValidTimeList.Add(DateTime.MinValue);
                //MyDefine.myXET.meValidNameList.Add("有效数据");          //添加空的阶段n的名称
                //MyDefine.myXET.meValidSetValueList.Add(0);               //添加空的阶段n的设定温度
                //MyDefine.myXET.meValidUpperList.Add(0);                  //添加空的阶段n的纵坐标上限温度
                //MyDefine.myXET.meValidLowerList.Add(0);                  //添加空的阶段n的纵坐标下限温度
            }

            //记录当前已设置的有效阶段数量(有效开始、结束均已设置的阶段)
            MyDefine.myXET.meValidStageNum = listNum / 2;
            if (validIdx1 == -1 || validIdx2 == -1) MyDefine.myXET.meValidStageNum -= 1;    //有效开始或有效结束未设置

            //根据当前已设置的有效阶段数量，使能/禁止Pn阶段的设置
            //radioButton2.Enabled = MyDefine.myXET.meValidIdxList.Count > 2 ? true : false;
            //radioButton3.Enabled = MyDefine.myXET.meValidIdxList.Count > 4 ? true : false;
            //radioButton4.Enabled = MyDefine.myXET.meValidIdxList.Count > 6 ? true : false;
            //radioButton5.Enabled = MyDefine.myXET.meValidIdxList.Count > 8 ? true : false;
            //radioButton6.Enabled = MyDefine.myXET.meValidIdxList.Count > 10 ? true : false;
            //radioButton7.Enabled = MyDefine.myXET.meValidIdxList.Count > 12 ? true : false;
            //radioButton8.Enabled = MyDefine.myXET.meValidIdxList.Count > 14 ? true : false;
            label1.Text = "温度/" + MyDefine.myXET.temUnit;
        }

        #endregion

        #region 绘制有效数据开始、结束线条(数据曲线)0

        public void DrawDataValidLines()
        {
            if (MyDefine.myXET.meDataTbl == null) return;

            Graphics gph = pictureBox1.CreateGraphics();
            gph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            pictureBox1.Refresh();

            string mystr1 = "有" + '\n' + "效" + '\n' + "数" + '\n' + "据" + '\n' + "开" + '\n' + "始";
            string mystr2 = "有" + '\n' + "效" + '\n' + "数" + '\n' + "据" + '\n' + "结" + '\n' + "束";

            //if (point1 != Point.Empty)
            if (MyDefine.myXET.meValidStartIdx != MyDefine.myXET.meStartIdx)
            {
                //画线
                int total = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;          //总点数
                int index = MyDefine.myXET.meValidStartIdx;                         //有效开始索引
                int pointX = (int)(1.0 * index / total * pictureBox1.Width);        //当前索引在图片上的位置(由数据索引值计算其点坐标)
                gph.DrawLine(new Pen(Color.Black, 1), new Point(pointX, 0), new Point(pointX, pictureBox1.Height));
                gph.DrawString(mystr1, new Font("Arial", 8), Brushes.Blue, new Point(pointX, pictureBox1.Height - 80));

                //显示文字：有效数据开始
                //label62.Visible = true;
                //label62.Location = new Point(pictureBox1.Location.X + point1.X - label62.Size.Width, panel1.Location.Y + pictureBox1.Height - label62.Size.Height);

                //显示提示信息
                //String msg = (point2 == Point.Empty) ? "右击选择终点" : "";
                //SetToolTips(msg);
            }

            //if (point2 != Point.Empty)
            if (MyDefine.myXET.meValidStopIdx != MyDefine.myXET.meStopIdx)
            {
                //画线
                int total = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;          //总点数
                int index = MyDefine.myXET.meValidStopIdx;                          //有效结束索引
                int pointX = (int)(1.0 * index / total * pictureBox1.Width);        //当前索引在图片上的位置(由数据索引值计算其点坐标)
                gph.DrawLine(new Pen(Color.Black, 1), new Point(pointX, 0), new Point(pointX, pictureBox1.Height));
                gph.DrawString(mystr2, new Font("Arial", 8), Brushes.Blue, new Point(pointX, pictureBox1.Height - 80));

                //显示文字：有效数据结束
                //label63.Visible = true;
                //label63.Location = new Point(pictureBox1.Location.X + point2.X + 2, panel1.Location.Y + pictureBox1.Height - label63.Size.Height);

                //显示提示信息
                //String msg = (point1 == Point.Empty) ? "右击选择起点" : "";
                //SetToolTips(msg);
            }
        }

        #endregion

        #region 绘制有效数据开始、结束线条(验证曲线，可能有多组有效区域)0

        public void DrawVerValidLines()
        {
            if (MyDefine.myXET.meDataTbl == null) return;
            if (MyDefine.myXET.meF0ValidList.Count == 0) return;    //尚无F0有效数据

            Graphics gph = pictureBox1.CreateGraphics();
            gph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            pictureBox1.Refresh();

            for (int i = 0; i < MyDefine.myXET.meF0ValidList.Count; i++)
            {
                int total = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;          //总点数
                int index = MyDefine.myXET.meF0ValidList[i];                        //当前索引
                int pointX = (int)(1.0 * index / total * pictureBox1.Width);        //当前索引在图片上的位置(由数据索引值计算其点坐标)
                string mystr1 = "有" + '\n' + "效" + '\n' + "数" + '\n' + "据" + '\n' + "开" + '\n' + "始";
                string mystr2 = "有" + '\n' + "效" + '\n' + "数" + '\n' + "据" + '\n' + "结" + '\n' + "束";

                gph.DrawLine(new Pen(Color.Black, 1), new Point(pointX, 0), new Point(pointX, pictureBox1.Height));
                if (i % 2 == 0) gph.DrawString(mystr1, new Font("Arial", 8), Brushes.Blue, new Point(pointX, pictureBox1.Height - 80));
                if (i % 2 == 1) gph.DrawString(mystr2, new Font("Arial", 8), Brushes.Blue, new Point(pointX, pictureBox1.Height - 80));
            }

            int startIdx = MyDefine.myXET.meF0ValidList[0];
            int stopIdx = MyDefine.myXET.meF0ValidList[1];
            DateTime startTime = Convert.ToDateTime(MyDefine.myXET.meDataTbl.GetCellValue(startIdx, 0));
            DateTime stopTime = Convert.ToDateTime(MyDefine.myXET.meDataTbl.GetCellValue(stopIdx, 0));
            label64.Text = "有效开始：" + startTime.ToString("MM-dd HH:mm:ss");
            label65.Text = "有效结束：" + stopTime.ToString("MM-dd HH:mm:ss");
            label66.Text = "持续时间：" + ((stopTime.Subtract(startTime).TotalSeconds) / 60.0).ToString("F2") + " min";

        }

        #endregion

        #region 双击刷新界面

        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.Name == "验证曲线") return;                         //验证曲线禁止手动设置开始、结束时间

            MyDefine.myXET.AddTraceInfo("双击刷新");
            pictureBox1.Refresh();

            ResetValidList();                                           //复位有效数据索引
            ShowValidLines();                                           //显示可能存在的有效数据竖直线
            ShowValidInfo();                                            //显示右侧有效数据信息
            MyDefine.myXET.switchMainPanel(23);                         //同步更新数据处理界面有效开始、结束行的高亮状态
            //SetToolTips("");
        }

        #endregion

        #region 右键显示下拉菜单

        /// <summary>
        /// 单击右键显示下拉菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            groupBox1.Focus(); Application.DoEvents();

            if (this.Name == "验证曲线") return;                         //验证曲线禁止手动设置开始、结束时间

            if (e.Button == MouseButtons.Left)      //选择阶段
            {
                Int32 dataNum = indexEnd - indexStart;              //数据总个数
                Int32 index = (int)((1.0 * e.X / pictureBox1.Width) * dataNum) + indexStart;               //鼠标当前位置对应的数据索引

                for (int i = 0; i < MyDefine.myXET.meValidIdxList.Count; i += 2)
                {
                    if (index > MyDefine.myXET.meValidIdxList[i])
                    {
                        if (index < MyDefine.myXET.meValidIdxList[i + 1])
                        {
                            cb_selectStage.SelectedIndex = i / 2;

                            //添加阶段启用
                            if (MyDefine.myXET.meValidNameList.Count != MyDefine.myXET.meActivePn + 1)
                            {
                                addStageToolStripMenuItem.Visible = false;
                            }
                            return;
                        }
                    }
                }

                MyDefine.myXET.meActivePn = MyDefine.myXET.meValidNameList.Count - 1;

                if (MyDefine.myXET.meActivePn + 1 == MyDefine.myXET.meValidStageNum)
                {
                    cb_selectStage.SelectedIndex = MyDefine.myXET.meActivePn;
                }
                //添加阶段启用
                if (MyDefine.myXET.meValidNameList.Count == MyDefine.myXET.meActivePn + 1)
                {
                    addStageToolStripMenuItem.Visible = true;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                //MyDefine.myXET.AddTraceInfo("右键单击");
                contextMenuStrip1.Visible = true;
                mouseX = e.X;           //e.X为鼠标在pictureBox1上的坐标
                mouseY = e.Y;           //e.Y为鼠标在pictureBox1上的坐标

                //初始化阶段名称文本框
                if (MyDefine.myXET.meValidNameList[MyDefine.myXET.meActivePn] == "有效数据" || tb_setName.Text == "")
                {
                    tb_setName.Text = "阶段名称";
                    tb_setName.ForeColor = Color.LightGray;
                }
                else
                {
                    tb_setName.Text = MyDefine.myXET.meValidNameList[MyDefine.myXET.meActivePn];
                    tb_setName.ForeColor = Color.Black;
                }

                //是否显示恢复原图像
                if (drawPicture)
                {
                    恢复原图像ToolStripMenuItem.Visible = true;
                }
                else
                {
                    恢复原图像ToolStripMenuItem.Visible = false;
                }

                //在鼠标右击的位置显示下拉菜单(下拉菜单位置是相对全屏的)
                contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);       //注意MousePosition.X&Y为鼠标的屏幕坐标
            }
        }

        #endregion

        #region 恢复原图像1.0比例

        /// <summary>
        /// 图像放大后恢复1.0比例
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 恢复原图像ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            drawPicture = false;
            Zoom = 1.0f;
            pictureBox1.Cursor = Cursors.Default;
            DrawFileDataLines();                //重新加载界面后重新绘制曲线
            ShowValidLines();                   //显示可能存在的有效数据竖直线
        }
        #endregion 

        #endregion

        #region 鼠标事件

        #region 设置并显示提示信息toolTip1

        /// <summary>
        /// 鼠标悬停在pictureBox1上后，显示提示信息（注，一个toolTip可以为多个控件设置不同的提示信息）
        /// </summary>
        /// <param name="msg"></param>
        public void SetToolTips(string msg)
        {
            toolTip1.AutoPopDelay = 5000;   //显示停留5秒        
            toolTip1.InitialDelay = 200;    //1秒后显示        
            toolTip1.ReshowDelay = 200;     //从一个控件移到另一个控件0.5秒后显示        
            toolTip1.ShowAlways = true;     //在窗口不活动时也显示
            toolTip1.SetToolTip(pictureBox1, msg);
        }

        /// <summary>
        /// 提示信息随鼠标移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (MyDefine.myXET.meDataTbl == null) return;
            if (pictureBox1.Image == null) return;
            if (picSaving == true) return;
            //if (this.Name == "验证曲线") return;

            try
            {
                String meas = getAxisVal(e.X, e.Y);
                if (toolTip1.GetToolTip(pictureBox1) != meas)
                {
                    //显示信息
                    toolTip1.SetToolTip(pictureBox1, meas);
                    toolTip1.Show(toolTip1.GetToolTip(pictureBox1), pictureBox1, new Point(e.X + 5, e.Y + 5));

                }
            }
            catch
            {
            }
        }

        #endregion

        #endregion

        #region 保存区域矩形内的曲线数据(单条曲线)

        //保存局部曲线数据
        private void SaveSelectedCurveData()
        {
            try
            {
                if (areaPoint1 == Point.Empty || areaPoint2 == Point.Empty)      //没有选中区域，退出
                {
                    MessageBox.Show("请选择需要保存的区域！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (areaPoint1.X == areaPoint2.X)           //没有可保存的数据
                {
                    MessageBox.Show("请选择需要保存的区域！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (MyDefine.myXET.meDataTbl == null)                        //数据列表为空
                {
                    MessageBox.Show("尚未加载数据！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int index = getDisplayCurveIndex();         //获取当前曲线索引
                if (index == -1 || index == 0) return;      //当前未显示曲线，或显示的是多条曲线，不进行数据保存

                int saveNum = 1;                            //需保存的曲线数量(HTH保存两条曲线)
                int startIdx = (int)((1.0 * areaPoint1.X / pictureBox1.Width) * MyDefine.myXET.meDataTbl.dataTable.Rows.Count);
                int stopIdx = (int)((1.0 * areaPoint2.X / pictureBox1.Width) * MyDefine.myXET.meDataTbl.dataTable.Rows.Count);
                //string mDeviceTypel = MyDefine.myXET.meDataTbl.dataTable.Columns[index].ColumnName; //comboBox1.Items[index].ToString();
                string mDeviceTypel = MyDefine.myXET.meTypeList[index]; //comboBox1.Items[index].ToString();

                if (mDeviceTypel.Contains("TH"))                             //HTH需保存温度和湿度两组数据，否则加载的时候会出错
                {
                    saveNum = 2;                                            //保存温湿度两条曲线
                    if (mDeviceTypel == "TH_H") index -= 1;                 //当前为湿度曲线, 曲线索引-1     
                }

                //保存曲线局部数据
                Boolean ret = MyDefine.myXET.Curve_SaveToLog(MyDefine.myXET.meDataTbl.dataTable, MyDefine.myXET.meInfoTbl.dataTable, 0, index, startIdx, stopIdx, saveNum);
                //pictureBox1.Refresh();

                if (ret)
                {
                    MessageBox.Show("数据保存成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MyDefine.myXET.ShowWrongMsg("数据保存失败！");
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("数据保存失败:" + ex.ToString());
            }
        }

        #endregion

        #region 保存区域矩形内的曲线数据(所有被选中的曲线)

        //保存局部曲线数据
        private void SaveAllSelectedCurveData()
        {
            try
            {
                if (!IsControlArrayReady()) return;                     //控件数组未创建

                if (MyDefine.myXET.meDataTbl == null)                        //数据列表为空
                {
                    MessageBox.Show("尚未加载数据！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (currentCurveIndex == -1)                                //没有显示曲线
                {
                    MessageBox.Show("请选择数据曲线！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                //没有选中区域，退出
                if (MyDefine.myXET.meValidStageNum == 0)
                {
                    MessageBox.Show("请选择需要保存的区域！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Boolean ret = true;
                MyDefine.myXET.AddTraceInfo("保存数据");

                //导出excel
                string type = "";
                Boolean isPart = true;
                DataTable mytable = new DataTable();
                //获取原始表格
                if (MyDefine.myXET.meTypeList == MyDefine.myXET.meAllList && MyDefine.myXET.meDataAllTbl != null)
                {
                    type = "原始数据";
                    isPart = false;
                    mytable = MyDefine.myXET.meDataAllTbl.dataTable;
                }
                if (MyDefine.myXET.meTypeList == MyDefine.myXET.meTmpList && MyDefine.myXET.meDataTmpTbl != null)
                {
                    type = "原始温度数据";
                    mytable = MyDefine.myXET.meDataTmpTbl.dataTable;
                }
                if (MyDefine.myXET.meTypeList == MyDefine.myXET.meHumList && MyDefine.myXET.meDataHumTbl != null)
                {
                    type = "原始湿度数据";
                    mytable = MyDefine.myXET.meDataHumTbl.dataTable;
                }
                if (MyDefine.myXET.meTypeList == MyDefine.myXET.mePrsList && MyDefine.myXET.meDataPrsTbl != null)
                {
                    type = "原始压力数据";
                    mytable = MyDefine.myXET.meDataPrsTbl.dataTable;
                }

                List<DataTable> myPeriodDataTables = new List<DataTable>();        //用于存储要保存的数据
                int rowCount = mytable.Rows.Count;                                 //获取原始表格的总行数
                for (int idx = 0; idx < MyDefine.myXET.meValidStageNum * 2; idx = idx + 2)      //注意，每个阶段的有效设置是成对的
                {
                    if (ret == false) break;
                    int startIdx = MyDefine.myXET.meValidIdxList[idx];             //有效数据开始索引
                    int stopIdx = MyDefine.myXET.meValidIdxList[idx + 1];          //有效数据结束索引

                    if (idx == 0 && startIdx == 0 && getIsPeriodSelected(type, stopIdx, rowCount))
                    {
                        //未选择阶段且阶段为整个阶段
                        break;
                    }

                    myPeriodDataTables.Add(getSelectedPeriodTable(mytable, startIdx, stopIdx));
                }

                if (type == "")
                {
                    MyDefine.myXET.ShowWrongMsg("数据表尚未加载！");
                    return;
                }

                SaveFileDialog DialogSave = new SaveFileDialog();
                DialogSave.Filter = "Excel(*.xls)|*.xls|所有文件(*.*)|*.*";
                DialogSave.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);     //默认桌面
                DialogSave.FileName = type;
                string selectedFilePath = "";                                                                   //excel文件存储路径
                string selectedFolderPath = "";

                if (DialogSave.ShowDialog() == DialogResult.OK)
                {
                    MyDefine.myXET.AddTraceInfo("数据导出");

                    XET.ExportToExcel(mytable, DialogSave.FileName);

                    selectedFilePath = DialogSave.FileName.Replace(".xls", "");

                    selectedFolderPath = Path.GetDirectoryName(DialogSave.FileName);
                }
                else
                {
                    return;
                }

                //导出各分段数据
                foreach (DataTable dataTable in myPeriodDataTables)
                {
                    //分段csv文件名加起止时间信息便于区分
                    string peirodFileName = ConvertFileNameFormat(selectedFilePath + "_" + dataTable.Rows[0][2].ToString() + "_" + dataTable.Rows[dataTable.Rows.Count - 1][2].ToString() + ".xls");

                    //导出分段数据
                    XET.ExportToExcel(dataTable, peirodFileName);
                }
                MyDefine.myXET.AddTraceInfo("Excel导出成功");

                //保存tmp
                string selectCurveType = "";                                            //存储选择的曲线类型
                for (int idx = 0; idx < MyDefine.myXET.meValidStageNum * 2; idx = idx + 2)      //注意，每个阶段的有效设置是成对的
                {
                    if (ret == false) break;
                    int startIdx = MyDefine.myXET.meValidIdxList[idx];             //有效数据开始索引
                    int stopIdx = MyDefine.myXET.meValidIdxList[idx + 1];          //有效数据结束索引
                    List<String> mylist = new List<String>();                      //存储被选中曲线对应的产品出厂编号

                    if (idx == 0 && startIdx == 0 && getIsPeriodSelected(type, stopIdx, rowCount))
                    {
                        //未选择阶段且阶段为整个阶段
                        break;
                    }

                    for (int i = 1; i < comboBox1.Items.Count; i++)
                    {
                        if (myLabelColor[i - 1].Text == "√")             //曲线为被选中状态
                        {
                            String strtype = MyDefine.myXET.meTypeList[i];          //当前曲线的数据类型
                            String strcode = MyDefine.myXET.meJSNList[i];           //当前产品的出厂编号

                            if (!selectCurveType.Contains(strtype[3]))                   //获取选择的曲线类型
                            {
                                selectCurveType += strtype[3];
                            }

                            if (mylist.Contains(strcode))                           //此产品的曲线已经被保存过了(如湿度采集器，两条曲线均为选中状态，遍历到其温度曲线时就已经保存好，到湿度曲线无需重复保存)
                            {
                                continue;
                            }
                            else                                                    //此产品的曲线未被保存过
                            {
                                mylist.Add(strcode);                                //添加出厂编号
                                int data = i;
                                if (isPart)
                                {
                                    data = data * 2 - 1;
                                }

                                if (strtype == "TH_T")                              //温湿度采集器的温度数据，需保存两条曲线
                                {
                                    ret = MyDefine.myXET.Curve_SaveToLog(MyDefine.myXET.meDataTblAll.dataTable, MyDefine.myXET.meInfoAllTbl.dataTable, idx / 2 + 1, data, startIdx, stopIdx, 2);
                                }
                                else if (strtype == "TH_H")                         //温湿度采集器的湿度数据，需保存两条曲线(索引-1)
                                {
                                    ret = MyDefine.myXET.Curve_SaveToLog(MyDefine.myXET.meDataTblAll.dataTable, MyDefine.myXET.meInfoAllTbl.dataTable, idx / 2 + 1, data - 1, startIdx, stopIdx, 2);
                                }
                                else if (strtype == "TT_T" || strtype == "TP_P")    //保存单条数据
                                {
                                    data = i;
                                    ret = MyDefine.myXET.Curve_SaveToLog(MyDefine.myXET.meDataTblAll.dataTable, MyDefine.myXET.meInfoAllTbl.dataTable, idx / 2 + 1, data, startIdx, stopIdx, 1);
                                }
                                else if (strtype == "TQ_T")                              //温湿度采集器的温度数据，需保存两条曲线
                                {
                                    ret = MyDefine.myXET.Curve_SaveToLog(MyDefine.myXET.meDataTblAll.dataTable, MyDefine.myXET.meInfoAllTbl.dataTable, idx / 2 + 1, data, startIdx, stopIdx, 2);
                                }
                                else if (strtype == "TQ_H")                         //温湿度采集器的湿度数据
                                {
                                    ret = MyDefine.myXET.Curve_SaveToLog(MyDefine.myXET.meDataTblAll.dataTable, MyDefine.myXET.meInfoAllTbl.dataTable, idx / 2 + 1, data, startIdx, stopIdx, 2);
                                }
                                else
                                {
                                    continue;
                                }

                            }
                        }
                    }
                }

                //打开保存路径目录
                Process.Start(selectedFolderPath.Replace(type, ""));

                if (ret)
                {
                    MyDefine.myXET.ShowCorrectMsg("保存成功！");
                }
                else
                {
                    MyDefine.myXET.ShowWrongMsg("数据保存失败！");
                }

            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("数据保存失败:" + ex.ToString());
            }
        }

        private string ConvertFileNameFormat(string input)
        {
            string pattern = @"(\d{2})-(\d{2})-(\d{2}) (\d{2}):(\d{2}):(\d{2})";
            string replacement = "$1$2$3$4$5$6";
            string output = Regex.Replace(input, pattern, replacement);
            output = output.Replace("\t", "");
            return output;
        }

        //获取阶段表格
        private DataTable getSelectedPeriodTable(DataTable myDataTable, int startIdx, int stopIdx)
        {
            //克隆表结构，保留表头
            DataTable periodDataTable = myDataTable.Clone();
            for (int i = startIdx; i < stopIdx + 1; i++)
            {
                //将指定行的数据导入到阶段表中
                periodDataTable.ImportRow(myDataTable.Rows[i]);
            }
            return periodDataTable;
        }

        //判断已选择的阶段是否为整个阶段
        private bool getIsPeriodSelected(string type, int stopIdx, int rowCount)
        {
            switch (type)
            {
                case "原始温度数据":
                    if (MyDefine.myXET.meTMPNum > 0)
                    {
                        if (stopIdx == rowCount - 5)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (stopIdx == rowCount - 1)
                        {
                            return true;
                        }
                    }
                    break;
                case "原始湿度数据":
                    if (MyDefine.myXET.meHUMNum > 0)
                    {
                        if (stopIdx == rowCount - 5)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (stopIdx == rowCount - 1)
                        {
                            return true;
                        }
                    }
                    break;
                case "原始压力数据":
                    if (MyDefine.myXET.mePRSNum > 0)
                    {
                        if (stopIdx == rowCount - 5)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (stopIdx == rowCount - 1)
                        {
                            return true;
                        }
                    }
                    break;
                default:
                    if (stopIdx == rowCount - 1)
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }

        #endregion

        #region 返回当前显示的单条曲线的索引值(若显示多条曲线则返回0)

        /// <summary>
        /// 获取当前显示的单条曲线的索引，若显示多条曲线则返回0(0与comboBox1的索引0对应，代表当前显示的是多条曲线)
        /// </summary>
        /// <returns></returns>
        public int getDisplayCurveIndex()
        {
            try
            {
                if (!IsControlArrayReady()) return -1;                     //控件数组未创建

                int ret = -1;
                int displayCurveIdx = 0;
                int displayCurveNum = 0;
                for (int i = 1; i < comboBox1.Items.Count; i++)
                {
                    if (myLabelColor[i - 1].Text == "√")
                    {
                        displayCurveNum++;                              //统计当前显示曲线的条数
                        displayCurveIdx = i;                            //当前显示曲线的索引值
                    }
                }

                if (displayCurveNum == 0) ret = -1;                     //没有曲线被选中
                if (displayCurveNum == 1) ret = displayCurveIdx;        //仅一条曲线被选中
                if (displayCurveNum > 1) ret = 0;                       //代表多条曲线被选中

                return ret;                                             //返回当前显示曲线的索引值(0代表有多条曲线被选中)
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
                return 0;
            }
        }

        #endregion

        #region 获得绘制字符需要的空间大小

        //获得绘制字符需要的空间大小
        private int getStringDrawingSpace(string msg)
        {
            return msg.Length * 7;  //7 -- 一个英文字符需要的单位坐标空间 
        }

        #endregion

        #region 绘制有效数据竖线并显示右侧有效信息

        #region 绘制有效数据竖线

        /// <summary>
        /// 绘制有效数据竖线
        /// </summary>
        public void ShowValidLines()
        {
            pictureBox1.Refresh();
            if (MyDefine.myXET.meDataTbl == null) return;

            Graphics gph = pictureBox1.CreateGraphics();
            gph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            pictureBox1.Refresh();

            #region 数据曲线

            //绘制数据曲线有效数据分割线
            if (this.Name == "数据曲线")
            {
                for (int i = 0, j = 0; i < MyDefine.myXET.meValidIdxList.Count; i++, j = (i + 1) / 2)
                {
                    if (MyDefine.myXET.meValidIdxList[i] == -1) continue;

                    //MessageBox.Show(MyDefine.myXET.meF0ValidList[i].ToString());
                    //int total = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;          //总点数
                    //int index = MyDefine.myXET.meValidIdxList[i] + 1;                   //当前索引（索引从0开始，所以+1）
                    //if (index > total || index < 0) continue;
                    int total = indexEnd - indexStart;          //总点数
                    int index = MyDefine.myXET.meValidIdxList[i] - indexStart;                   //当前索引（索引从0开始，所以+1）
                    if (index > total || index < 0) continue;
                    int pointX = (int)(1.0 * index / total * pictureBox1.Width);        //当前索引在图片上的位置(由数据索引值计算其点坐标)

                    //画竖直线
                    if (pointX <= 0) pointX = 1;                                        //竖线位置超出绘图区域
                    if (pointX >= pictureBox1.Width) pointX = pictureBox1.Width - 2;    //竖线位置超出绘图区域
                    gph.DrawLine(new Pen(Color.Black, 1), new Point(pointX, 0), new Point(pointX, pictureBox1.Height));

                    if (pointX > pictureBox1.Width - 20) pointX = pointX - 20;          //如果竖线位置过于靠近右边，则把字写在竖线左侧
                    if (i % 2 == 0)
                    {
                        //写字：有效数据开始 P1
                        String str1 = InsertFormat(MyDefine.myXET.meValidNameList[j], 1, "\n") + '\n' + "开" + '\n' + "始" + '\n' + " " + '\n' + "P" + '\n' + (i / 2 + 1).ToString();
                        gph.DrawString(str1, new Font("Arial", 8), Brushes.Blue, new Point(pointX + 3, pictureBox1.Height - 120));
                    }
                    else if (i % 2 == 1)
                    {
                        //写字：有效数据结束 P1
                        String str2 = InsertFormat(MyDefine.myXET.meValidNameList[j - 1], 1, "\n") + '\n' + "结" + '\n' + "束" + '\n' + " " + '\n' + "P" + '\n' + (i / 2 + 1).ToString();
                        gph.DrawString(str2, new Font("Arial", 8), Brushes.Blue, new Point(pointX + 3, pictureBox1.Height - 120));
                    }

                }
            }

            #endregion

            #region 验证曲线

            //绘制F0有效数据分割线
            if (this.Name == "验证曲线" && MyDefine.myXET.meF0ValidList.Count > 0)
            {
                for (int i = 0, j = 0; i < MyDefine.myXET.meF0ValidList.Count; i++, j = (i + 1) / 2)
                {
                    //MessageBox.Show(MyDefine.myXET.meF0ValidList[i].ToString());
                    int total = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;          //总点数
                    int index = MyDefine.myXET.meF0ValidList[i];                        //当前索引（索引从0开始，所以+1）
                    int pointX = (int)(1.0 * index / total * pictureBox1.Width);        //当前索引在图片上的位置(由数据索引值计算其点坐标)

                    //画竖直线
                    if (pointX <= 0) pointX = 1;                                        //竖线位置超出绘图区域
                    if (pointX >= pictureBox1.Width) pointX = pictureBox1.Width - 2;    //竖线位置超出绘图区域
                    gph.DrawLine(new Pen(Color.Black, 1), new Point(pointX, 0), new Point(pointX, pictureBox1.Height));

                    //写字：有效数据开始、有效数据结束
                    if (pointX > pictureBox1.Width - 20) pointX = pointX - 20;          //如果竖线位置过于靠近右边，则把字写在竖线左侧
                    if (i % 2 == 0)
                    {
                        //写字：有效数据开始 P1
                        String str1 = InsertFormat("F0有效", 1, "\n") + '\n' + "开" + '\n' + "始";
                        gph.DrawString(str1, new Font("Arial", 8), Brushes.Blue, new Point(pointX + 3, pictureBox1.Height - 80));
                    }
                    if (i % 2 == 1)
                    {
                        //写字：有效数据结束 P1
                        String str2 = InsertFormat("F0有效", 1, "\n") + '\n' + "结" + '\n' + "束";
                        gph.DrawString(str2, new Font("Arial", 8), Brushes.Blue, new Point(pointX + 3, pictureBox1.Height - 80));
                    }

                }
            }

            #endregion

        }

        //字符串间隔插入字符
        private string InsertFormat(string input, int interval, string value)
        {
            for (int i = interval; i < input.Length; i += interval + 1)
                input = input.Insert(i, value);
            return input;
        }
        #endregion

        #region 显示右侧有效信息

        /// <summary>
        /// 显示右侧有效信息
        /// </summary>
        public void ShowValidInfo()
        {
            int startIdx = int.MinValue;
            int stopIdx = int.MinValue;
            DateTime startTime = DateTime.MinValue;
            DateTime stopTime = DateTime.MinValue;
            int index = MyDefine.myXET.meActivePn;
            #region 数据曲线

            if (this.Name == "数据曲线" && MyDefine.myXET.meDataTbl != null)
            {
                int listIdx = index * 2;
                if (MyDefine.myXET.meValidIdxList.Count < listIdx + 2) return;

                //有效数据开始
                startIdx = MyDefine.myXET.meValidIdxList[listIdx];
                startTime = MyDefine.myXET.meValidTimeList[listIdx];

                //有效数据结束
                stopIdx = MyDefine.myXET.meValidIdxList[listIdx + 1];
                stopTime = MyDefine.myXET.meValidTimeList[listIdx + 1];
            }

            #endregion

            #region 验证曲线

            if (this.Name == "验证曲线" && MyDefine.myXET.meDataTbl != null)
            {
                //有效索引数量
                int validnum = MyDefine.myXET.meF0ValidList.Count;

                //有效数据开始
                startIdx = (validnum == 0) ? MyDefine.myXET.meStartIdx : MyDefine.myXET.meF0ValidList[0];
                startTime = Convert.ToDateTime(MyDefine.myXET.meDataTbl.GetCellValue(startIdx, 0));

                //有效数据结束
                stopIdx = (validnum == 0) ? MyDefine.myXET.meStopIdx : MyDefine.myXET.meF0ValidList[1];
                stopTime = Convert.ToDateTime(MyDefine.myXET.meDataTbl.GetCellValue(stopIdx, 0));
            }

            #endregion

            label64.Text = "有效开始：" + startTime.ToString("MM-dd HH:mm:ss");
            label65.Text = "有效结束：" + stopTime.ToString("MM-dd HH:mm:ss");
            label66.Text = "持续时间：" + ((stopTime.Subtract(startTime).TotalSeconds) / 60.0).ToString("F2") + " min";
        }

        #endregion

        #region 复位有效数据索引设定点(清除有效区域的选择)

        public void ResetValidList()
        {
            try
            {
                if (MyDefine.myXET.meDataTbl == null) return;

                //清除选中区域
                //label62.Visible = false;
                //label63.Visible = false;
                firstPoint = Point.Empty;
                areaPoint1 = Point.Empty;
                areaPoint2 = Point.Empty;
                MyDefine.myXET.meF0ValidList.Clear();       //清除F0有效区域

                //复位有效数据索引
                //MyDefine.myXET.meValidStartIdx = MyDefine.myXET.meStartIdx;
                //MyDefine.myXET.meValidStartTime = MyDefine.myXET.meStartTime;
                //MyDefine.myXET.meValidStopIdx = MyDefine.myXET.meStopIdx;
                //MyDefine.myXET.meValidStopTime = MyDefine.myXET.meStopTime;

                //复位有效数据索引
                MyDefine.myXET.meActivePn = 0;
                MyDefine.myXET.meValidIdxList.Clear();
                MyDefine.myXET.meValidTimeList.Clear();
                MyDefine.myXET.meValidNameList.Clear();
                MyDefine.myXET.meValidSetValueList.Clear();
                MyDefine.myXET.meValidUpperList.Clear();
                MyDefine.myXET.meValidLowerList.Clear();
                MyDefine.myXET.meValidIdxList.Add(-1);
                MyDefine.myXET.meValidIdxList.Add(-1);
                MyDefine.myXET.meValidTimeList.Add(DateTime.MinValue);
                MyDefine.myXET.meValidTimeList.Add(DateTime.MinValue);
                MyDefine.myXET.meValidNameList.Add("有效数据");
                MyDefine.myXET.meValidSetValueList.Add(double.MinValue);
                MyDefine.myXET.meValidSetValueList.Add(double.MinValue);
                MyDefine.myXET.meValidUpperList.Add(double.MinValue);
                MyDefine.myXET.meValidUpperList.Add(double.MinValue);
                MyDefine.myXET.meValidLowerList.Add(double.MinValue);
                MyDefine.myXET.meValidLowerList.Add(double.MinValue);
                cb_selectStage.Items.Clear();
                cb_selectStage.Items.Add("无");
                //复位阶段选择
                //radioButton1.Checked = true;
                //radioButton2.Enabled = false;
                //radioButton3.Enabled = false;
                //radioButton4.Enabled = false;
                //radioButton5.Enabled = false;
                //radioButton6.Enabled = false;
                //radioButton7.Enabled = false;
                //radioButton8.Enabled = false;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }

        }

        #endregion

        #endregion

        #endregion

        #region 绘制曲线

        #region 变量定义

        //dataTableClass meDataTbl;         //测试数据加载数据表
        //dataTableClass meInfoTbl;     //记录数据表参数信息(开始时间、结束时间等)
        //Color[] colors = new Color[] { Color.Green, Color.Peru, Color.Brown, Color.Tomato, Color.Cyan, Color.Gray, Color.Orange, Color.CadetBlue, Color.DarkSeaGreen, Color.CornflowerBlue, Color.ForestGreen, Color.Firebrick, Color.Salmon };
        Color[] colors = new Color[] { Color.RoyalBlue, Color.SaddleBrown, Color.ForestGreen, Color.Brown, Color.DodgerBlue, Color.SteelBlue, Color.DarkOrange, Color.DarkGreen, Color.BlueViolet, Color.Crimson, Color.OrangeRed, Color.DarkGoldenrod, Color.LightCoral, Color.LimeGreen, Color.DarkOrange, Color.Violet, Color.DarkTurquoise, Color.SlateBlue, Color.Magenta, Color.DeepSkyBlue, Color.RoyalBlue };
        float ZoomScale = 1.0f;                //曲线图放大比例

        #endregion

        #region 绘制多条曲线 -- 根据颜色条的选中状态

        public void DrawFileDataLines()
        {
            try
            {
                if (!IsControlArrayReady()) return;                     //控件数组未创建
                Bitmap img = new Bitmap(pictureBox1.Width, pictureBox1.Height);     //层图
                Graphics g = Graphics.FromImage(img);       //绘制
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.Clear(Color.White);    //必须先清除绘图，否则DrawString()写的字会有重影(无意中发现此方法可解决重影问题)
                DrawPictureGrid(g);      //画背景网格线            

                if (MyDefine.myXET.meDataTbl != null)                //数据表不为空
                {
                    indexStart = 0;
                    indexEnd = MyDefine.myXET.meDataTbl.dataTable.Rows.Count - 1;

                    myStartTime = MyDefine.myXET.meStartTime;                           //有效数据开始时间
                    myStopTime = MyDefine.myXET.meStopTime;                             //有效数据结束时间
                    myTestSeconds = (Int32)(myStopTime.Subtract(myStartTime).TotalSeconds);     //总测试时间(秒)

                    getDataLimits();                //计算温度值的绘图上下限tempSpan/meTMax
                    DrawPictureInfos(g);            //更新Y轴坐标信息

                    float x = 0;
                    int y1 = 0, y2 = 0;
                    int dataNum = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;          //数据总数
                    float x_perGrid = (float)pictureBox1.Width / (dataNum - 1);         //若要显示所有数据，每个数据占的单位格数
                    float unitTempHeight = (float)(pictureBox1.Height / tempSpan);      //每单位℃在pictureBox1上的高度
                    float unitHumHeight = (float)(pictureBox1.Height / humSpan);        //每单位℃在pictureBox1上的高度
                    float unitPsrHeight = (float)(pictureBox1.Height / prsSpan);        //每单位℃在pictureBox1上的高度

                    for (int i = 1; i < comboBox1.Items.Count; i++)                     //meDataTbl.dataTable的0列是时间列，comboBox1.Items的索引0是All_Line，所以从1开始
                    {
                        myLabelColor[i - 1].BackColor = colors[i % 20];                      //曲线条颜色
                        //if (myBoxArray[i - 1].Checked == false) continue;             //曲线为非选中状态，不绘制
                        if (myLabelColor[i - 1].Text == string.Empty) continue;         //曲线为非选中状态，不绘制
                        if (i >= MyDefine.myXET.meTypeList.Count) continue;             //不明原因，有时候会出现超出meTypeList索引范围异常

                        x = y1 = y2 = 0;        //一列数据一条曲线                
                        String myDeviceType = MyDefine.myXET.meTypeList[i];   //产品类型

                        for (int j = 0; j < MyDefine.myXET.meDataTbl.dataTable.Rows.Count - 1; j++, x = x + x_perGrid)
                        {
                            if (MyDefine.myXET.meDataTbl.dataTable.Rows[j][i].ToString() == "" || MyDefine.myXET.meDataTbl.dataTable.Rows[j + 1][i].ToString() == "") continue;//空数据
                            Double mydata1 = Convert.ToDouble(MyDefine.myXET.meDataTbl.dataTable.Rows[j][i]);
                            Double mydata2 = Convert.ToDouble(MyDefine.myXET.meDataTbl.dataTable.Rows[j + 1][i]);
                            if (x > pictureBox1.Width) x = pictureBox1.Width;

                            switch (myDeviceType)
                            {
                                case "TT_T":    //温度采集器
                                    y1 = pictureBox1.Height - (int)((mydata1 - meTmpMin) * unitTempHeight);
                                    y2 = pictureBox1.Height - (int)((mydata2 - meTmpMin) * unitTempHeight);
                                    //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[i], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    break;

                                case "TH_T":    //温湿度采集器
                                    y1 = pictureBox1.Height - (int)((mydata1 - meTmpMin) * unitTempHeight);
                                    y2 = pictureBox1.Height - (int)((mydata2 - meTmpMin) * unitTempHeight);
                                    //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[i], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    break;

                                case "TH_H":    //温湿度采集器
                                    if (!MyDefine.myXET.drawHumCurve) continue;        //存在压力数据时，不画湿度值
                                    y1 = pictureBox1.Height - (int)((mydata1 - meHumMin) * unitHumHeight);
                                    y2 = pictureBox1.Height - (int)((mydata2 - meHumMin) * unitHumHeight);
                                    //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[i], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    break;

                                case "TQ_T":    //温湿度采集器
                                    y1 = pictureBox1.Height - (int)((mydata1 - meTmpMin) * unitTempHeight);
                                    y2 = pictureBox1.Height - (int)((mydata2 - meTmpMin) * unitTempHeight);
                                    //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[i], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    break;

                                case "TQ_H":    //温湿度采集器
                                    if (!MyDefine.myXET.drawHumCurve) continue;        //存在压力数据时，不画湿度值
                                    y1 = pictureBox1.Height - (int)((mydata1 - meHumMin) * unitHumHeight);
                                    y2 = pictureBox1.Height - (int)((mydata2 - meHumMin) * unitHumHeight);
                                    //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[i], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    break;

                                case "TP_P":    //压力采集器
                                    y1 = pictureBox1.Height - (int)((mydata1 - mePrsMin) * unitPsrHeight);
                                    y2 = pictureBox1.Height - (int)((mydata2 - mePrsMin) * unitPsrHeight);
                                    //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[i], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    break;

                                default:
                                    break;
                            }

                        }
                        //pictureBox1.Image = img;
                        //MessageBox.Show(meDataTbl.dataTable.Columns[i].Caption.ToString());
                    }

                    //更新X轴时间           
                    updateAxisLabel();
                }

                pictureBox1.Image = img;    //铺图
                g.Dispose();
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        #endregion

        #region 重绘曲线

        public void RedrawFileDataLines()
        {
            try
            {
                if (!IsControlArrayReady()) return;                     //控件数组未创建
                Bitmap img = new Bitmap(pictureBox1.Width, pictureBox1.Height);     //层图
                Graphics g = Graphics.FromImage(img);       //绘制
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.Clear(Color.White);    //必须先清除绘图，否则DrawString()写的字会有重影(无意中发现此方法可解决重影问题)
                DrawPictureGrid(g);      //画背景网格线            

                if (MyDefine.myXET.meDataTbl != null)                //数据表不为空
                {
                    //    getDataLimits();                //计算温度值的绘图上下限tempSpan/meTMax
                    //    DrawPictureInfos(g);            //更新Y轴坐标信息

                    float x = 0;
                    int y1 = 0, y2 = 0;
                    int dataNum = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;          //数据总数
                    float x_perGrid = (float)pictureBox1.Width / (dataNum - 1);         //若要显示所有数据，每个数据占的单位格数
                    float unitTempHeight = (float)(pictureBox1.Height / tempSpan);      //每单位℃在pictureBox1上的高度
                    float unitHumHeight = (float)(pictureBox1.Height / humSpan);        //每单位℃在pictureBox1上的高度
                    float unitPsrHeight = (float)(pictureBox1.Height / prsSpan);        //每单位℃在pictureBox1上的高度

                    for (int i = 1; i < comboBox1.Items.Count; i++)                     //meDataTbl.dataTable的0列是时间列，comboBox1.Items的索引0是All_Line，所以从1开始
                    {
                        myLabelColor[i - 1].BackColor = colors[i % 20];                      //曲线条颜色
                        //if (myBoxArray[i - 1].Checked == false) continue;             //曲线为非选中状态，不绘制
                        if (myLabelColor[i - 1].Text == string.Empty) continue;         //曲线为非选中状态，不绘制
                        if (i >= MyDefine.myXET.meTypeList.Count) continue;             //不明原因，有时候会出现超出meTypeList索引范围异常

                        x = y1 = y2 = 0;        //一列数据一条曲线                
                        String myDeviceType = MyDefine.myXET.meTypeList[i];   //产品类型

                        for (int j = 0; j < MyDefine.myXET.meDataTbl.dataTable.Rows.Count - 1; j++, x = x + x_perGrid)
                        {
                            if (MyDefine.myXET.meDataTbl.dataTable.Rows[j][i].ToString() == "" || MyDefine.myXET.meDataTbl.dataTable.Rows[j + 1][i].ToString() == "") continue;//空数据
                            Double mydata1 = Convert.ToDouble(MyDefine.myXET.meDataTbl.dataTable.Rows[j][i]);
                            Double mydata2 = Convert.ToDouble(MyDefine.myXET.meDataTbl.dataTable.Rows[j + 1][i]);
                            if (x > pictureBox1.Width) x = pictureBox1.Width;

                            switch (myDeviceType)
                            {
                                case "TT_T":    //温度采集器
                                    y1 = pictureBox1.Height - (int)((mydata1 - meTmpMin) * unitTempHeight);
                                    y2 = pictureBox1.Height - (int)((mydata2 - meTmpMin) * unitTempHeight);
                                    //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[i], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    break;

                                case "TH_T":    //温湿度采集器
                                    y1 = pictureBox1.Height - (int)((mydata1 - meTmpMin) * unitTempHeight);
                                    y2 = pictureBox1.Height - (int)((mydata2 - meTmpMin) * unitTempHeight);
                                    //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[i], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    break;

                                case "TH_H":    //温湿度采集器
                                    if (!MyDefine.myXET.drawHumCurve) continue;        //存在压力数据时，不画湿度值
                                    y1 = pictureBox1.Height - (int)((mydata1 - meHumMin) * unitHumHeight);
                                    y2 = pictureBox1.Height - (int)((mydata2 - meHumMin) * unitHumHeight);
                                    //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[i], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    break;

                                case "TQ_T":    //温湿度采集器
                                    y1 = pictureBox1.Height - (int)((mydata1 - meTmpMin) * unitTempHeight);
                                    y2 = pictureBox1.Height - (int)((mydata2 - meTmpMin) * unitTempHeight);
                                    //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[i], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    break;

                                case "TQ_H":    //温湿度采集器
                                    if (!MyDefine.myXET.drawHumCurve) continue;        //存在压力数据时，不画湿度值
                                    y1 = pictureBox1.Height - (int)((mydata1 - meHumMin) * unitHumHeight);
                                    y2 = pictureBox1.Height - (int)((mydata2 - meHumMin) * unitHumHeight);
                                    //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[i], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    break;

                                case "TP_P":    //压力采集器
                                    y1 = pictureBox1.Height - (int)((mydata1 - mePrsMin) * unitPsrHeight);
                                    y2 = pictureBox1.Height - (int)((mydata2 - mePrsMin) * unitPsrHeight);
                                    //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[i], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    break;

                                default:
                                    break;
                            }

                        }
                        //pictureBox1.Image = img;
                        //MessageBox.Show(meDataTbl.dataTable.Columns[i].Caption.ToString());
                    }

                    //更新X轴时间           
                    // updateAxisLabel();
                }

                pictureBox1.Image = img;    //铺图
                g.Dispose();
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        #endregion

        #region 绘制阶段曲线 -- 根据颜色条的选中状态

        public void DrawStageDataLines()
        {
            try
            {
                if (!IsControlArrayReady()) return;                     //控件数组未创建
                Bitmap img = new Bitmap(pictureBox1.Width, pictureBox1.Height);     //层图
                Graphics g = Graphics.FromImage(img);       //绘制
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.Clear(Color.White);    //必须先清除绘图，否则DrawString()写的字会有重影(无意中发现此方法可解决重影问题)
                DrawPictureGrid(g);      //画背景网格线            

                if (MyDefine.myXET.meDataTbl != null)                //数据表不为空
                {
                    getDataLimits();                //计算温度值的绘图上下限tempSpan/meTMax
                    DrawPictureInfos(g);            //更新Y轴坐标信息

                    float x = 0;
                    int y1 = 0, y2 = 0;
                    int dataNum = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;          //数据总数
                    float x_perGrid = (float)pictureBox1.Width / (dataNum - 1);         //若要显示所有数据，每个数据占的单位格数
                    float unitTempHeight = (float)(pictureBox1.Height / tempSpan);      //每单位℃在pictureBox1上的高度
                    float unitHumHeight = (float)(pictureBox1.Height / humSpan);        //每单位℃在pictureBox1上的高度
                    float unitPsrHeight = (float)(pictureBox1.Height / prsSpan);        //每单位℃在pictureBox1上的高度

                    for (int i = 1; i < comboBox1.Items.Count; i++)                     //meDataTbl.dataTable的0列是时间列，comboBox1.Items的索引0是All_Line，所以从1开始
                    {
                        myLabelColor[i - 1].BackColor = colors[i];                      //曲线条颜色
                        //if (myBoxArray[i - 1].Checked == false) continue;             //曲线为非选中状态，不绘制
                        if (myLabelColor[i - 1].Text == string.Empty) continue;         //曲线为非选中状态，不绘制
                        if (i >= MyDefine.myXET.meTypeList.Count) continue;             //不明原因，有时候会出现超出meTypeList索引范围异常

                        x = y1 = y2 = 0;        //一列数据一条曲线                
                        String myDeviceType = MyDefine.myXET.meTypeList[i];   //产品类型

                        for (int j = 0; j < MyDefine.myXET.meDataTbl.dataTable.Rows.Count - 1; j++, x = x + x_perGrid)
                        {
                            if (MyDefine.myXET.meDataTbl.dataTable.Rows[j][i].ToString() == "" || MyDefine.myXET.meDataTbl.dataTable.Rows[j + 1][i].ToString() == "") continue;//空数据
                            Double mydata1 = Convert.ToDouble(MyDefine.myXET.meDataTbl.dataTable.Rows[j][i]);
                            Double mydata2 = Convert.ToDouble(MyDefine.myXET.meDataTbl.dataTable.Rows[j + 1][i]);
                            if (x > pictureBox1.Width) x = pictureBox1.Width;

                            switch (myDeviceType)
                            {
                                case "TT_T":    //温度采集器
                                    y1 = pictureBox1.Height - (int)((mydata1 - meTmpMin) * unitTempHeight);
                                    y2 = pictureBox1.Height - (int)((mydata2 - meTmpMin) * unitTempHeight);
                                    //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[i], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    g.DrawLine(new Pen(colors[i], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    break;

                                case "TH_T":    //温湿度采集器
                                    y1 = pictureBox1.Height - (int)((mydata1 - meTmpMin) * unitTempHeight);
                                    y2 = pictureBox1.Height - (int)((mydata2 - meTmpMin) * unitTempHeight);
                                    //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[i], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    g.DrawLine(new Pen(colors[i], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    break;

                                case "TH_H":    //温湿度采集器
                                    if (!MyDefine.myXET.drawHumCurve) continue;        //存在压力数据时，不画湿度值
                                    y1 = pictureBox1.Height - (int)((mydata1 - meHumMin) * unitHumHeight);
                                    y2 = pictureBox1.Height - (int)((mydata2 - meHumMin) * unitHumHeight);
                                    //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[i], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    g.DrawLine(new Pen(colors[i], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    break;

                                case "TQ_T":    //温湿度采集器
                                    y1 = pictureBox1.Height - (int)((mydata1 - meTmpMin) * unitTempHeight);
                                    y2 = pictureBox1.Height - (int)((mydata2 - meTmpMin) * unitTempHeight);
                                    //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[i], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    g.DrawLine(new Pen(colors[i], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    break;

                                case "TQ_H":    //温湿度采集器
                                    if (!MyDefine.myXET.drawHumCurve) continue;        //存在压力数据时，不画湿度值
                                    y1 = pictureBox1.Height - (int)((mydata1 - meHumMin) * unitHumHeight);
                                    y2 = pictureBox1.Height - (int)((mydata2 - meHumMin) * unitHumHeight);
                                    //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[i], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    g.DrawLine(new Pen(colors[i], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    break;

                                case "TP_P":    //压力采集器
                                    y1 = pictureBox1.Height - (int)((mydata1 - mePrsMin) * unitPsrHeight);
                                    y2 = pictureBox1.Height - (int)((mydata2 - mePrsMin) * unitPsrHeight);
                                    //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[i], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    g.DrawLine(new Pen(colors[i], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    break;

                                default:
                                    break;
                            }

                        }
                        //pictureBox1.Image = img;
                        //MessageBox.Show(meDataTbl.dataTable.Columns[i].Caption.ToString());
                    }

                    //更新X轴时间           
                    updateAxisLabel();
                }

                pictureBox1.Image = img;    //铺图
                g.Dispose();
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        #endregion

        #region 绘制单条曲线

        //绘制单条曲线
        public void DrawFileDataLine(int idx)
        {
            try
            {
                Bitmap img = new Bitmap(pictureBox1.Width, pictureBox1.Height);     //层图
                Graphics g = Graphics.FromImage(img);       //绘制
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.Clear(Color.White);    //必须先清除绘图，否则DrawString()写的字会有重影(无意中发现此方法可解决重影问题)
                DrawPictureGrid(g);     //画背景网格线            

                if (MyDefine.myXET.meDataTbl != null)                //数据表不为空
                {
                    getDataLimit(idx);              //计算xx值的绘图上下限tempSpan/meTMax
                    DrawPictureInfos(g);            //更新Y轴坐标信息

                    float x = 0;
                    int y1 = 0, y2 = 0;
                    int dataNum = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;        //数据总数
                    float x_perGrid = (float)pictureBox1.Width / (dataNum - 1);       //若要显示所有数据，每个数据占的单位格数
                    float unitTempHeight = (float)(pictureBox1.Height / tempSpan);    //每单位℃(*100)在pictureBox1上的高度
                    float unitHumHeight = (float)(pictureBox1.Height / humSpan);      //每单位℃(*100)在pictureBox1上的高度
                    float unitPsrHeight = (float)(pictureBox1.Height / prsSpan);      //每单位℃(*100)在pictureBox1上的高度

                    //for (int i = 1; i < meDataTbl.dataTable.Columns.Count; i++)   //0列是时间列，所以从1开始
                    {
                        x = y1 = y2 = 0;  //一列数据一条曲线
                        String myDeviceType = MyDefine.myXET.meTypeList[idx];   //产品类型

                        for (int j = 0; j < MyDefine.myXET.meDataTbl.dataTable.Rows.Count - 1; j++, x = x + x_perGrid)
                        {
                            if (MyDefine.myXET.meDataTbl.dataTable.Rows[j][idx].ToString() == "" || MyDefine.myXET.meDataTbl.dataTable.Rows[j + 1][idx].ToString() == "") continue;//空数据
                            Double mydata1 = Convert.ToDouble(MyDefine.myXET.meDataTbl.dataTable.Rows[j][idx]);
                            Double mydata2 = Convert.ToDouble(MyDefine.myXET.meDataTbl.dataTable.Rows[j + 1][idx]);
                            if (x > pictureBox1.Width) x = pictureBox1.Width;

                            switch (myDeviceType)
                            {
                                case "TT_T":    //温度采集器

                                    y1 = pictureBox1.Height - (int)((mydata1 - meTmpMin) * unitTempHeight);
                                    y2 = pictureBox1.Height - (int)((mydata2 - meTmpMin) * unitTempHeight);
                                    //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[idx], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    g.DrawLine(new Pen(colors[idx], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    break;

                                case "TH_T":    //温湿度采集器
                                    y1 = pictureBox1.Height - (int)((mydata1 - meTmpMin) * unitTempHeight);
                                    y2 = pictureBox1.Height - (int)((mydata2 - meTmpMin) * unitTempHeight);
                                    //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[idx], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    g.DrawLine(new Pen(colors[idx], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    break;

                                case "TH_H":    //温湿度采集器
                                    y1 = pictureBox1.Height - (int)((mydata1 - meHumMin) * unitHumHeight);
                                    y2 = pictureBox1.Height - (int)((mydata2 - meHumMin) * unitHumHeight);
                                    //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[idx], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    g.DrawLine(new Pen(colors[idx], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    break;

                                case "TP_P":    //压力采集器
                                    y1 = pictureBox1.Height - (int)((mydata1 - mePrsMin) * unitPsrHeight);
                                    y2 = pictureBox1.Height - (int)((mydata2 - mePrsMin) * unitPsrHeight);
                                    //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[idx], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    g.DrawLine(new Pen(colors[idx], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    break;

                                default:
                                    break;
                            }

                        }
                    }

                    //更新X轴时间           
                    updateAxisLabel();
                }

                pictureBox1.Image = img;    //铺图
                g.Dispose();
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        #endregion

        #region 画背景网格线

        //画pictureBox1的网格线(勿删)
        private void DrawPictureGrid(Graphics g)
        {
            try
            {
                int x1 = 0;
                int y1 = 0;
                int gridnum = 0;
                Pen mypen = new Pen(Color.Silver, 1.00f);   //定义背景网格画笔

                //画X轴网格线
                gridnum = 20;
                for (int i = 0; i <= gridnum; i++)
                {
                    y1 = (int)(pictureBox1.Height * i / gridnum * 1.0) - 1;
                    if (y1 < 0) y1 = 0;

                    mypen = new Pen(Color.Gainsboro, 1.00f);   //定义背景网格画笔
                    mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;      //画虚线

                    //画5条实线
                    if (i % 5 == 0)
                    {
                        mypen = new Pen(Color.DarkGray, 1.00f);   //定义背景网格画笔
                        mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;      //画实线
                    }

                    g.DrawLine(mypen, new Point(0, y1), new Point(pictureBox1.Width, y1));      //画X轴网格
                }

                //画Y轴网格线
                gridnum = 5;
                for (int i = 0; i <= gridnum; i++)
                {
                    mypen = new Pen(Color.Gainsboro, 1.00f);   //定义背景网格画笔
                    mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;      //画虚线

                    x1 = (int)(pictureBox1.Width * i / gridnum * 1.0) - 1;
                    if (x1 < 0) x1 = 0;

                    mypen = new Pen(Color.Gainsboro, 1.00f);   //定义背景网格画笔
                    mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;      //画虚线

                    if (i % gridnum == 0)
                    {
                        mypen = new Pen(Color.DarkGray, 1.00f);   //定义背景网格画笔
                        mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;      //画实线
                    }

                    g.DrawLine(mypen, new Point(x1, 0), new Point(x1, pictureBox1.Height - 1));      //画Y轴网格线
                }

                //g.DrawLine(new Pen(Color.Silver, 1.00f), new Point(0, 0), new Point(0, pictureBox1.Height - 1));      //画Y轴实线
                //g.DrawLine(new Pen(Color.Silver, 1.00f), new Point(pictureBox1.Width - 1, 0), new Point(pictureBox1.Width - 1, pictureBox1.Height - 1));      //画Y轴实线
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        //画pictureBox1的网格线
        private void DrawPictureGrid0(Graphics g)
        {
            try
            {
                int y1 = 0;
                for (int j = 0; j <= 8; j++)
                {
                    y1 = (int)(pictureBox1.Height * j / 8.0) - 1;
                    if (y1 < 0) y1 = 0;
                    g.DrawLine(new Pen(Color.Silver, 1.00f), new Point(0, y1), new Point(pictureBox1.Width, y1));      //画网格
                }
                g.DrawLine(new Pen(Color.Silver, 1.00f), new Point(0, 0), new Point(0, pictureBox1.Height - 1));      //画Y轴
                g.DrawLine(new Pen(Color.Silver, 1.00f), new Point(pictureBox1.Width - 1, 0), new Point(pictureBox1.Width - 1, pictureBox1.Height - 1));      //画Y轴
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }


        #endregion

        #region 计算曲线坐标范围

        //计算曲线坐标范围
        public void getCurveRange(ref Double minTemp, ref Double maxTemp, int num)//num = 0 为左轴；num = 1 为右轴
        {
            //取较大绝对值的1/10作为坐标余量
            //int offsetY = Math.Abs(maxTemp) > Math.Abs(minTemp) ? (int)(Math.Abs(maxTemp) * 0.1) : (int)(Math.Abs(minTemp) * 0.1);
            if (MyDefine.myXET.CustomAxes[num])
            {
                if (num == 0)
                {
                    maxTemp = MyDefine.myXET.leftLimit[0];
                    minTemp = MyDefine.myXET.leftLimit[1];
                }
                else
                {
                    maxTemp = MyDefine.myXET.rightLimit[0];
                    minTemp = MyDefine.myXET.rightLimit[1];
                }
                return;
            }

            Double offsetY = Math.Abs(maxTemp - minTemp) * 0.1;
            maxTemp += offsetY;
            minTemp -= offsetY;

            if (maxTemp - minTemp < 10)                                                 //Y轴跨度为<10,则调整为跨度=10
            {
                offsetY = (10 - (maxTemp - minTemp)) / 2;
                maxTemp = maxTemp + offsetY;
                minTemp = minTemp - offsetY;
            }

            if (num == 0)
            {
                MyDefine.myXET.leftLimit[0] = maxTemp;
                MyDefine.myXET.leftLimit[1] = minTemp;
            }
            else
            {
                MyDefine.myXET.rightLimit[0] = maxTemp;
                MyDefine.myXET.rightLimit[1] = minTemp;
            }
            /*
            maxTemp = maxTemp > 0 ? (int)(maxTemp * 1.1) : (int)(maxTemp * 0.9);    //范围跨度应略大于最大最小值
            minTemp = minTemp > 0 ? (int)(minTemp * 0.9) : (int)(minTemp * 1.1);    //范围跨度应略大于最大最小值
            if (maxTemp == 0) maxTemp += 10;
            if (minTemp == 0) minTemp -= 10;
            if (maxTemp == minTemp)                                                 //防止数值全一样时，跨度为0
            {
                maxTemp = maxTemp + 10;
                minTemp = minTemp - 10;
            }*/
        }

        #endregion

        #region 更新Y轴坐标信息

        //更新Y轴坐标值
        private void DrawPictureInfos(Graphics g)
        {
            try
            {
                if (MyDefine.myXET.meDataTbl == null) return;

                String MeasY;

                if (this.tempSpan < int.MaxValue - 10)       //温度跨度非最大值：存在温度列，绘图
                {
                    label1.Visible = true;
                    label2.Visible = true;
                    label6.Visible = true;
                    label4.Visible = true;
                    label11.Visible = true;
                    label3.Visible = true;

                    //=====更新温度/压力值Y轴信息================================================================================================================
                    double unitTempHeight = pictureBox1.Height / tempSpan;     //每单位℃在pictureBox1上的高度

                    //计算坐标点所在位置的温度值：根据y1 = pictureBox1.Height - (int)((myTemp1 - meTmpMin) * unitTempHeight)反推的计算公式
                    MeasY = ((pictureBox1.Height - 0.00 * pictureBox1.Height) / unitTempHeight + meTmpMin).ToString("F2");        //计算pictureBox1坐标0点对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(2, (int)(0.00 * pictureBox1.Height - 0)));
                    label2.Text = MeasY;
                    //label2.Location = new System.Drawing.Point(label2.Location.X, (int)(0.00 * pictureBox1.Height - 8 + 15)); 

                    MeasY = ((pictureBox1.Height - 0.25 * pictureBox1.Height) / unitTempHeight + meTmpMin).ToString("F2");        //计算pictureBox1坐标1/4处对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(2, (int)(0.25 * pictureBox1.Height - 8)));
                    label6.Text = MeasY;
                    //label6.Location = new System.Drawing.Point(label6.Location.X, (int)(0.25 * pictureBox1.Height - 8 + 15));

                    MeasY = ((pictureBox1.Height - 0.5 * pictureBox1.Height) / unitTempHeight + meTmpMin).ToString("F2");        //计算pictureBox1坐标2/4处对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(2, (int)(0.5 * pictureBox1.Height - 8)));
                    label4.Text = MeasY;
                    //label4.Location = new System.Drawing.Point(label4.Location.X, (int)(0.5 * pictureBox1.Height - 8 + 15));

                    MeasY = ((pictureBox1.Height - 0.75 * pictureBox1.Height) / unitTempHeight + meTmpMin).ToString("F2");        //计算pictureBox1坐标3/4处对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(2, (int)(0.75 * pictureBox1.Height - 8)));
                    label11.Text = MeasY;
                    //label11.Location = new System.Drawing.Point(label11.Location.X, (int)(0.75 * pictureBox1.Height - 8 + 15));

                    MeasY = ((pictureBox1.Height - 1.00 * pictureBox1.Height) / unitTempHeight + meTmpMin).ToString("F2");        //计算pictureBox1坐标4/4处对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(2, pictureBox1.Height - 16));
                    label3.Text = MeasY;
                    //label3.Location = new System.Drawing.Point(label3.Location.X, (int)(pictureBox1.Height - 8 + 15));
                }
                else
                {
                    label1.Visible = false;
                    label2.Visible = false;
                    label6.Visible = false;
                    label4.Visible = false;
                    label11.Visible = false;
                    label3.Visible = false;
                }

                //默认隐藏Y轴右侧坐标
                label20.Visible = false;
                label21.Visible = false;
                label7.Visible = false;
                label8.Visible = false;
                label9.Visible = false;
                label10.Visible = false;
                label12.Visible = false;

                if (this.prsSpan < int.MaxValue - 10)       //压力跨度非最大值：存在压力列，绘图
                {
                    //MessageBox.Show("1");
                    label21.Visible = true;
                    label7.Visible = true;
                    label8.Visible = true;
                    label9.Visible = true;
                    label10.Visible = true;
                    label12.Visible = true;

                    //=====更新湿度值Y轴信息================================================================================================================
                    double unitPsrHeight = pictureBox1.Height / prsSpan;     //每单位℃在pictureBox1上的高度

                    //计算坐标点所在位置的温度值：根据y1 = pictureBox1.Height - (int)((myTemp1 - meTmpMin) * unitTempHeight)反推的计算公式
                    MeasY = ((pictureBox1.Height - 0.00 * pictureBox1.Height) / unitPsrHeight + mePrsMin).ToString("F2");        //计算pictureBox1坐标0点对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), (int)(0.00 * pictureBox1.Height - 0)));
                    label7.Text = MeasY;

                    MeasY = ((pictureBox1.Height - 0.25 * pictureBox1.Height) / unitPsrHeight + mePrsMin).ToString("F2");        //计算pictureBox1坐标1/4处对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), (int)(0.25 * pictureBox1.Height - 8)));
                    label8.Text = MeasY;

                    MeasY = ((pictureBox1.Height - 0.5 * pictureBox1.Height) / unitPsrHeight + mePrsMin).ToString("F2");        //计算pictureBox1坐标2/4处对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), (int)(0.5 * pictureBox1.Height - 8)));
                    label9.Text = MeasY;

                    MeasY = ((pictureBox1.Height - 0.75 * pictureBox1.Height) / unitPsrHeight + mePrsMin).ToString("F2");        //计算pictureBox1坐标3/4处对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), (int)(0.75 * pictureBox1.Height - 8)));
                    label10.Text = MeasY;

                    MeasY = ((pictureBox1.Height - 1.00 * pictureBox1.Height) / unitPsrHeight + mePrsMin).ToString("F2");        //计算pictureBox1坐标4/4处对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), pictureBox1.Height - 16));
                    label12.Text = MeasY;
                }

                if (this.humSpan < int.MaxValue - 10 && MyDefine.myXET.drawHumCurve == true)       //湿度跨度非最大值：存在湿度列且允许绘制湿度曲线(没有同时存在压力列)，绘图
                {
                    //MessageBox.Show("2");
                    label20.Visible = true;
                    label7.Visible = true;
                    label8.Visible = true;
                    label9.Visible = true;
                    label10.Visible = true;
                    label12.Visible = true;

                    //=====更新湿度值Y轴信息================================================================================================================
                    double unitHumHeight = pictureBox1.Height / humSpan;     //每单位℃在pictureBox1上的高度

                    //计算坐标点所在位置的温度值：根据y1 = pictureBox1.Height - (int)((myTemp1 - meTmpMin) * unitTempHeight)反推的计算公式
                    MeasY = ((pictureBox1.Height - 0.00 * pictureBox1.Height) / unitHumHeight + meHumMin).ToString("F2");        //计算pictureBox1坐标0点对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), (int)(0.00 * pictureBox1.Height - 0)));
                    label7.Text = MeasY;

                    MeasY = ((pictureBox1.Height - 0.25 * pictureBox1.Height) / unitHumHeight + meHumMin).ToString("F2");        //计算pictureBox1坐标1/4处对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), (int)(0.25 * pictureBox1.Height - 8)));
                    label8.Text = MeasY;

                    MeasY = ((pictureBox1.Height - 0.5 * pictureBox1.Height) / unitHumHeight + meHumMin).ToString("F2");        //计算pictureBox1坐标2/4处对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), (int)(0.5 * pictureBox1.Height - 8)));
                    label9.Text = MeasY;

                    MeasY = ((pictureBox1.Height - 0.75 * pictureBox1.Height) / unitHumHeight + meHumMin).ToString("F2");        //计算pictureBox1坐标3/4处对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), (int)(0.75 * pictureBox1.Height - 8)));
                    label10.Text = MeasY;

                    MeasY = ((pictureBox1.Height - 1.00 * pictureBox1.Height) / unitHumHeight + meHumMin).ToString("F2");        //计算pictureBox1坐标4/4处对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), pictureBox1.Height - 16));
                    label12.Text = MeasY;
                }


                //如果不是温湿度传感器则隐藏右侧坐标信息
                //label7.Visible = MyDefine.myXET.meType == DEVICE.HTH ? true : false;
                //label8.Visible = MyDefine.myXET.meType == DEVICE.HTH ? true : false;
                //label9.Visible = MyDefine.myXET.meType == DEVICE.HTH ? true : false;
                //label10.Visible = MyDefine.myXET.meType == DEVICE.HTH ? true : false;
                //label12.Visible = MyDefine.myXET.meType == DEVICE.HTH ? true : false;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        //更新Y轴坐标值(放大曲线)
        private void DrawPictureInfosZoom(Graphics g)
        {
            try
            {
                if (MyDefine.myXET.meDataTbl == null) return;

                String MeasY;

                if (this.tempSpan < int.MaxValue - 10)       //温度跨度非最大值：存在温度列，绘图
                {
                    label1.Visible = true;
                    label2.Visible = true;
                    label6.Visible = true;
                    label4.Visible = true;
                    label11.Visible = true;
                    label3.Visible = true;

                    //=====更新温度/压力值Y轴信息================================================================================================================
                    double unitTempHeight = pictureBox1.Height / tempSpanZS;     //每单位℃在pictureBox1上的高度

                    //计算坐标点所在位置的温度值：根据y1 = pictureBox1.Height - (int)((myTemp1 - meTmpMin) * unitTempHeight)反推的计算公式
                    MeasY = ((pictureBox1.Height - 0.00 * pictureBox1.Height) / unitTempHeight + meTmpMinZS).ToString("F2");        //计算pictureBox1坐标0点对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(2, (int)(0.00 * pictureBox1.Height - 0)));
                    label2.Text = MeasY;
                    //label2.Location = new System.Drawing.Point(label2.Location.X, (int)(0.00 * pictureBox1.Height - 8 + 15)); 

                    MeasY = ((pictureBox1.Height - 0.25 * pictureBox1.Height) / unitTempHeight + meTmpMinZS).ToString("F2");        //计算pictureBox1坐标1/4处对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(2, (int)(0.25 * pictureBox1.Height - 8)));
                    label6.Text = MeasY;
                    //label6.Location = new System.Drawing.Point(label6.Location.X, (int)(0.25 * pictureBox1.Height - 8 + 15));

                    MeasY = ((pictureBox1.Height - 0.5 * pictureBox1.Height) / unitTempHeight + meTmpMinZS).ToString("F2");        //计算pictureBox1坐标2/4处对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(2, (int)(0.5 * pictureBox1.Height - 8)));
                    label4.Text = MeasY;
                    //label4.Location = new System.Drawing.Point(label4.Location.X, (int)(0.5 * pictureBox1.Height - 8 + 15));

                    MeasY = ((pictureBox1.Height - 0.75 * pictureBox1.Height) / unitTempHeight + meTmpMinZS).ToString("F2");        //计算pictureBox1坐标3/4处对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(2, (int)(0.75 * pictureBox1.Height - 8)));
                    label11.Text = MeasY;
                    //label11.Location = new System.Drawing.Point(label11.Location.X, (int)(0.75 * pictureBox1.Height - 8 + 15));

                    MeasY = ((pictureBox1.Height - 1.00 * pictureBox1.Height) / unitTempHeight + meTmpMinZS).ToString("F2");        //计算pictureBox1坐标4/4处对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(2, pictureBox1.Height - 16));
                    label3.Text = MeasY;
                    //label3.Location = new System.Drawing.Point(label3.Location.X, (int)(pictureBox1.Height - 8 + 15));
                }
                else
                {
                    label1.Visible = false;
                    label2.Visible = false;
                    label6.Visible = false;
                    label4.Visible = false;
                    label11.Visible = false;
                    label3.Visible = false;
                }

                //默认隐藏Y轴右侧坐标
                label20.Visible = false;
                label21.Visible = false;
                label7.Visible = false;
                label8.Visible = false;
                label9.Visible = false;
                label10.Visible = false;
                label12.Visible = false;

                if (this.prsSpan < int.MaxValue - 10)       //压力跨度非最大值：存在压力列，绘图
                {
                    //MessageBox.Show("1");
                    label21.Visible = true;
                    label7.Visible = true;
                    label8.Visible = true;
                    label9.Visible = true;
                    label10.Visible = true;
                    label12.Visible = true;

                    //=====更新湿度值Y轴信息================================================================================================================
                    double unitPsrHeight = pictureBox1.Height / prsSpanZS;     //每单位℃在pictureBox1上的高度

                    //计算坐标点所在位置的温度值：根据y1 = pictureBox1.Height - (int)((myTemp1 - meTmpMin) * unitTempHeight)反推的计算公式
                    MeasY = ((pictureBox1.Height - 0.00 * pictureBox1.Height) / unitPsrHeight + mePrsMinZS).ToString("F2");        //计算pictureBox1坐标0点对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), (int)(0.00 * pictureBox1.Height - 0)));
                    label7.Text = MeasY;

                    MeasY = ((pictureBox1.Height - 0.25 * pictureBox1.Height) / unitPsrHeight + mePrsMinZS).ToString("F2");        //计算pictureBox1坐标1/4处对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), (int)(0.25 * pictureBox1.Height - 8)));
                    label8.Text = MeasY;

                    MeasY = ((pictureBox1.Height - 0.5 * pictureBox1.Height) / unitPsrHeight + mePrsMinZS).ToString("F2");        //计算pictureBox1坐标2/4处对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), (int)(0.5 * pictureBox1.Height - 8)));
                    label9.Text = MeasY;

                    MeasY = ((pictureBox1.Height - 0.75 * pictureBox1.Height) / unitPsrHeight + mePrsMinZS).ToString("F2");        //计算pictureBox1坐标3/4处对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), (int)(0.75 * pictureBox1.Height - 8)));
                    label10.Text = MeasY;

                    MeasY = ((pictureBox1.Height - 1.00 * pictureBox1.Height) / unitPsrHeight + mePrsMinZS).ToString("F2");        //计算pictureBox1坐标4/4处对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), pictureBox1.Height - 16));
                    label12.Text = MeasY;
                }

                if (this.humSpan < int.MaxValue - 10 && MyDefine.myXET.drawHumCurve == true)       //湿度跨度非最大值：存在湿度列且允许绘制湿度曲线(没有同时存在压力列)，绘图
                {
                    //MessageBox.Show("2");
                    label20.Visible = true;
                    label7.Visible = true;
                    label8.Visible = true;
                    label9.Visible = true;
                    label10.Visible = true;
                    label12.Visible = true;

                    //=====更新湿度值Y轴信息================================================================================================================
                    double unitHumHeight = pictureBox1.Height / humSpanZS;     //每单位℃在pictureBox1上的高度

                    //计算坐标点所在位置的温度值：根据y1 = pictureBox1.Height - (int)((myTemp1 - meTmpMin) * unitTempHeight)反推的计算公式
                    MeasY = ((pictureBox1.Height - 0.00 * pictureBox1.Height) / unitHumHeight + meHumMinZS).ToString("F2");        //计算pictureBox1坐标0点对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), (int)(0.00 * pictureBox1.Height - 0)));
                    label7.Text = MeasY;

                    MeasY = ((pictureBox1.Height - 0.25 * pictureBox1.Height) / unitHumHeight + meHumMinZS).ToString("F2");        //计算pictureBox1坐标1/4处对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), (int)(0.25 * pictureBox1.Height - 8)));
                    label8.Text = MeasY;

                    MeasY = ((pictureBox1.Height - 0.5 * pictureBox1.Height) / unitHumHeight + meHumMinZS).ToString("F2");        //计算pictureBox1坐标2/4处对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), (int)(0.5 * pictureBox1.Height - 8)));
                    label9.Text = MeasY;

                    MeasY = ((pictureBox1.Height - 0.75 * pictureBox1.Height) / unitHumHeight + meHumMinZS).ToString("F2");        //计算pictureBox1坐标3/4处对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), (int)(0.75 * pictureBox1.Height - 8)));
                    label10.Text = MeasY;

                    MeasY = ((pictureBox1.Height - 1.00 * pictureBox1.Height) / unitHumHeight + meHumMinZS).ToString("F2");        //计算pictureBox1坐标4/4处对应的温度值
                    if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), pictureBox1.Height - 16));
                    label12.Text = MeasY;
                }


                //如果不是温湿度传感器则隐藏右侧坐标信息
                //label7.Visible = MyDefine.myXET.meType == DEVICE.HTH ? true : false;
                //label8.Visible = MyDefine.myXET.meType == DEVICE.HTH ? true : false;
                //label9.Visible = MyDefine.myXET.meType == DEVICE.HTH ? true : false;
                //label10.Visible = MyDefine.myXET.meType == DEVICE.HTH ? true : false;
                //label12.Visible = MyDefine.myXET.meType == DEVICE.HTH ? true : false;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        //更新Y轴坐标值
        private void DrawPictureInfo(Graphics g)
        {
            try
            {
                String MeasY;

                //=====更新温度/压力值Y轴信息================================================================================================================
                double unitTempHeight = pictureBox1.Height / tempSpan;     //每单位℃在pictureBox1上的高度

                //计算坐标点所在位置的温度值：根据y1 = pictureBox1.Height - (int)((myTemp1 - meTmpMin) * unitTempHeight)反推的计算公式
                MeasY = ((pictureBox1.Height - 0.00 * pictureBox1.Height) / unitTempHeight + meTmpMin).ToString("F0");        //计算pictureBox1坐标0点对应的温度值
                if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(2, (int)(0.00 * pictureBox1.Height - 0)));
                label2.Text = MeasY;

                MeasY = ((pictureBox1.Height - 0.25 * pictureBox1.Height) / unitTempHeight + meTmpMin).ToString("F0");        //计算pictureBox1坐标1/4处对应的温度值
                if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(2, (int)(0.25 * pictureBox1.Height - 8)));
                label6.Text = MeasY;

                MeasY = ((pictureBox1.Height - 0.5 * pictureBox1.Height) / unitTempHeight + meTmpMin).ToString("F0");        //计算pictureBox1坐标2/4处对应的温度值
                if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(2, (int)(0.5 * pictureBox1.Height - 8)));
                label4.Text = MeasY;

                MeasY = ((pictureBox1.Height - 0.75 * pictureBox1.Height) / unitTempHeight + meTmpMin).ToString("F0");        //计算pictureBox1坐标3/4处对应的温度值
                if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(2, (int)(0.75 * pictureBox1.Height - 8)));
                label11.Text = MeasY;

                MeasY = ((pictureBox1.Height - 1.00 * pictureBox1.Height) / unitTempHeight + meTmpMin).ToString("F0");        //计算pictureBox1坐标4/4处对应的温度值
                if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(2, pictureBox1.Height - 16));
                label3.Text = MeasY;

                //=====更新湿度值Y轴信息================================================================================================================
                double unitHumHeight = pictureBox1.Height / humSpan;     //每单位℃在pictureBox1上的高度

                //计算坐标点所在位置的温度值：根据y1 = pictureBox1.Height - (int)((myTemp1 - meTmpMin) * unitTempHeight)反推的计算公式
                MeasY = ((pictureBox1.Height - 0.00 * pictureBox1.Height) / unitHumHeight + meHumMin).ToString("F0");        //计算pictureBox1坐标0点对应的温度值
                if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), (int)(0.00 * pictureBox1.Height - 0)));
                label7.Text = MeasY;

                MeasY = ((pictureBox1.Height - 0.25 * pictureBox1.Height) / unitHumHeight + meHumMin).ToString("F0");        //计算pictureBox1坐标1/4处对应的温度值
                if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), (int)(0.25 * pictureBox1.Height - 8)));
                label8.Text = MeasY;

                MeasY = ((pictureBox1.Height - 0.5 * pictureBox1.Height) / unitHumHeight + meHumMin).ToString("F0");        //计算pictureBox1坐标2/4处对应的温度值
                if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), (int)(0.5 * pictureBox1.Height - 8)));
                label9.Text = MeasY;

                MeasY = ((pictureBox1.Height - 0.75 * pictureBox1.Height) / unitHumHeight + meHumMin).ToString("F0");        //计算pictureBox1坐标3/4处对应的温度值
                if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), (int)(0.75 * pictureBox1.Height - 8)));
                label10.Text = MeasY;

                MeasY = ((pictureBox1.Height - 1.00 * pictureBox1.Height) / unitHumHeight + meHumMin).ToString("F0");        //计算pictureBox1坐标4/4处对应的温度值
                if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(pictureBox1.Width - GetSpace(MeasY), pictureBox1.Height - 16));
                label12.Text = MeasY;

                //如果不是温湿度传感器则隐藏右侧坐标信息
                label7.Visible = ((MyDefine.myXET.meType == DEVICE.HTH) || (MyDefine.myXET.meType == DEVICE.HTQ)) ? true : false;
                label8.Visible = ((MyDefine.myXET.meType == DEVICE.HTH) || (MyDefine.myXET.meType == DEVICE.HTQ)) ? true : false;
                label9.Visible = ((MyDefine.myXET.meType == DEVICE.HTH) || (MyDefine.myXET.meType == DEVICE.HTQ)) ? true : false;
                label10.Visible = ((MyDefine.myXET.meType == DEVICE.HTH) || (MyDefine.myXET.meType == DEVICE.HTQ)) ? true : false;
                label12.Visible = ((MyDefine.myXET.meType == DEVICE.HTH) || (MyDefine.myXET.meType == DEVICE.HTQ)) ? true : false;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        //获得绘制字符串需要的宽度
        private int GetSpace(string mystr)
        {
            return (mystr.Length + 1) * 6;
        }

        #endregion

        #region 更新X轴坐标信息

        //更新X轴时间信息
        private void updateAxisLabel()
        {
            try
            {
                label13.Text = "01-01-01 00:00:00";
                label14.Text = "01-01-01 00:00:00";
                label15.Text = "01-01-01 00:00:00";
                label16.Text = "01-01-01 00:00:00";
                label17.Text = "01-01-01 00:00:00";
                label18.Text = "01-01-01 00:00:00";

                float unitTimeGridX = (float)(myTestSeconds * 1.0 / 5);             //将测试时间分为5份
                label13.Text = myStartTime.ToString("yy-MM-dd HH:mm:ss");
                label14.Text = myStartTime.AddSeconds(unitTimeGridX * 1).ToString("yy-MM-dd HH:mm:ss");
                label15.Text = myStartTime.AddSeconds(unitTimeGridX * 2).ToString("yy-MM-dd HH:mm:ss");
                label16.Text = myStartTime.AddSeconds(unitTimeGridX * 3).ToString("yy-MM-dd HH:mm:ss");
                label17.Text = myStartTime.AddSeconds(unitTimeGridX * 4).ToString("yy-MM-dd HH:mm:ss");
                label18.Text = myStartTime.AddSeconds(myTestSeconds).ToString("yy-MM-dd HH:mm:ss");
                //label18.Text = myStopTime.ToString("yy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        #endregion

        #region 计算曲线数据的limits(用于动态调整绘图上下限)

        private double tempSpan = 400;     //温度范围-100~300℃，跨度400℃
        private double meTmpMin = 100;       //温度0点距离pictureBox1坐标0点(300℃)的跨度为300℃
        private double humSpan = 400;      //湿度范围
        private double meHumMin = 100;     //湿度0点距离pictureBox1坐标0点跨度
        private double prsSpan = 400;      //压力范围
        private double mePrsMin = 100;       //压力0点距离pictureBox1坐标0点跨度

        #region 查找测试数据最大、最小值(用于多条曲线绘制)

        //分别查找温度、湿度、压力的最大最小值
        public void getDataLimits()
        {
            try
            {
                if (MyDefine.myXET.meDataTbl == null) return;           //数据列表为空
                if (!IsControlArrayReady()) return;                     //控件数组未创建

                this.tempSpan = double.MaxValue;
                this.humSpan = double.MaxValue;
                this.prsSpan = double.MaxValue;

                Double max, min;
                Double maxTemp = double.MinValue;
                Double minTemp = double.MinValue;
                Double maxHum = double.MinValue;
                Double minHum = double.MinValue;
                Double maxPsr = double.MinValue;
                Double minPsr = double.MinValue;
                String myDeviceType;

                //初始化数据
                MyDefine.myXET.drawTemCurve = false;     //是否画温度曲线
                MyDefine.myXET.drawHumCurve = false;     //是否画湿度曲线
                MyDefine.myXET.drawPrsCurve = false;    //是否画压力曲线 

                for (int i = 1; i < comboBox1.Items.Count; i++)               //comboBox1.Items.Count最大值为20
                {
                    //if (myBoxArray[i - 1].Checked == false) continue;       //曲线为非选中状态，不绘制
                    if (myLabelColor[i - 1].Text == string.Empty) continue;   //曲线为非选中状态，不绘制
                    if (i >= MyDefine.myXET.meTypeList.Count) continue;       //不明原因，有时候会出现超出meTypeList索引范围异常

                    myDeviceType = MyDefine.myXET.meTypeList[i];   //产品类型
                    max = MyDefine.myXET.meDataTbl.GetColumnMaxVal(i);
                    min = MyDefine.myXET.meDataTbl.GetColumnMinVal(i);
                    //MessageBox.Show("1:" + max.ToString() + " " + min.ToString());

                    switch (myDeviceType)
                    {
                        case "TT_T":    //温度采集器
                            MyDefine.myXET.drawTemCurve = true;
                            if (maxTemp == double.MinValue)    //将第一个温度值赋给最大最小温度变量
                            {
                                maxTemp = max;
                                minTemp = min;
                            }
                            else
                            {
                                if (maxTemp < max) maxTemp = max;
                                if (minTemp > min) minTemp = min;
                            }
                            break;

                        case "TH_T":    //温湿度采集器
                            MyDefine.myXET.drawTemCurve = true;
                            if (maxTemp == double.MinValue)    //将第一个温度值赋给最大最小温度变量
                            {
                                maxTemp = max;
                                minTemp = min;
                            }
                            else
                            {
                                if (maxTemp < max) maxTemp = max;
                                if (minTemp > min) minTemp = min;
                            }
                            break;

                        case "TH_H":    //温湿度采集器
                            MyDefine.myXET.drawHumCurve = true;           //允许绘制湿度曲线
                            if (maxHum == double.MinValue)    //将第一个湿度值赋给最大最小湿度变量
                            {
                                maxHum = max;
                                minHum = min;
                            }
                            else
                            {
                                if (maxHum < max) maxHum = max;
                                if (minHum > min) minHum = min;
                            }
                            break;

                        case "TP_P":    //压力采集器
                            MyDefine.myXET.drawPrsCurve = true;
                            if (maxPsr == double.MinValue)    //将第一个压力值赋给最大最小压力变量
                            {
                                maxPsr = max;
                                minPsr = min;
                            }
                            else
                            {
                                if (maxPsr < max) maxPsr = max;
                                if (minPsr > min) minPsr = min;
                            }
                            break;

                        case "TQ_T"://温湿度采集器
                            MyDefine.myXET.drawTemCurve = true;
                            if (maxTemp == double.MinValue)    //将第一个温度值赋给最大最小温度变量
                            {
                                maxTemp = max;
                                minTemp = min;
                            }
                            else
                            {
                                if (maxTemp < max) maxTemp = max;
                                if (minTemp > min) minTemp = min;
                            }
                            break;

                        case "TQ_H"://温湿度采集器
                            MyDefine.myXET.drawHumCurve = true;           //允许绘制湿度曲线
                            if (maxHum == double.MinValue)    //将第一个湿度值赋给最大最小湿度变量
                            {
                                maxHum = max;
                                minHum = min;
                            }
                            else
                            {
                                if (maxHum < max) maxHum = max;
                                if (minHum > min) minHum = min;
                            }
                            break;

                        default:
                            break;
                    }
                }

                if (maxTemp != double.MinValue)
                {
                    //记录温度数据最大最小值
                    MyDefine.myXET.recordMaxMin[0] = maxTemp;
                    MyDefine.myXET.recordMaxMin[1] = minTemp;

                    //左轴
                    rb_left.Enabled = true;

                    //计算温度坐标范围           
                    getCurveRange(ref minTemp, ref maxTemp, 0);
                    //MessageBox.Show("2:" + maxTemp.ToString() + " " + minTemp.ToString());

                    double span = maxTemp - minTemp;                                 //温度跨度
                    this.tempSpan = span / ZoomScale;                                //放大缩小后的温度跨度
                    this.meTmpMin = minTemp + (span - tempSpan) / 2;                 //温度最小值

                    tempSpanZS = this.tempSpan;
                    meTmpMinZS = this.meTmpMin;
                }

                if (maxHum != double.MinValue)
                {
                    //记录湿度数据最大最小值
                    MyDefine.myXET.recordMaxMin[2] = maxHum;
                    MyDefine.myXET.recordMaxMin[3] = minHum;

                    //左轴
                    rb_right.Enabled = true;

                    //计算湿度坐标范围            
                    getCurveRange(ref minHum, ref maxHum, 1);
                    //MessageBox.Show("3:" + maxHum.ToString() + " " + minHum.ToString());

                    double span = maxHum - minHum;                                  //湿度跨度
                    this.humSpan = span / ZoomScale;                                //湿度跨度
                    this.meHumMin = minHum + (span - humSpan) / 2;                  //湿度最小值

                    humSpanZS = this.humSpan;
                    meHumMinZS = this.meHumMin;
                }

                if (maxPsr != double.MinValue)
                {
                    //记录压力数据最大最小值
                    MyDefine.myXET.recordMaxMin[2] = maxPsr;
                    MyDefine.myXET.recordMaxMin[3] = minPsr;

                    //左轴
                    rb_right.Enabled = true;

                    //计算压力坐标范围      
                    getCurveRange(ref minPsr, ref maxPsr, 1);
                    //MessageBox.Show("4:" + maxPsr.ToString() + " " + minPsr.ToString());

                    double span = maxPsr - minPsr;                                  //压力跨度
                    this.prsSpan = span / ZoomScale;                                //压力跨度
                    this.mePrsMin = minPsr + (span - prsSpan) / 2;                  //压力最小值

                    prsSpanZS = this.prsSpan;
                    mePrsMinZS = this.mePrsMin;
                }

                if (maxPsr != double.MinValue)     //存在压力数据时，不画湿度曲线
                {
                    //MessageBox.Show("4:");
                    isPsr = true;
                    MyDefine.myXET.drawHumCurve = false;
                    MyDefine.myXET.drawPrsCurve = true;
                }
                //MessageBox.Show("Temp: " + maxTemp.ToString() + "," + minTemp.ToString());
                //MessageBox.Show("Hum: " + maxHum.ToString() + "," + minHum.ToString());
                //MessageBox.Show("Psr: " + maxPsr.ToString() + "," + minPsr.ToString());
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        #endregion

        #region 查找测试数据最大、最小值(用于单条曲线绘制)

        //查找某列数据的最大最小值
        public void getDataLimit(int idx)
        {
            try
            {
                if (MyDefine.myXET.meDataTbl == null) return;        //数据列表为空

                this.tempSpan = Double.MaxValue;
                this.humSpan = Double.MaxValue;
                this.prsSpan = Double.MaxValue;

                Double max, min;
                String myDeviceType;

                //for (int i = 1; i < meDataTbl.dataTable.Columns.Count; i++)
                {
                    myDeviceType = MyDefine.myXET.meTypeList[idx];   //产品类型
                    max = MyDefine.myXET.meDataTbl.GetColumnMaxVal(idx);
                    min = MyDefine.myXET.meDataTbl.GetColumnMinVal(idx);
                    //getCurveRange(ref min, ref max);

                    switch (myDeviceType)
                    {
                        case "TT_T":    //温度采集器
                            this.tempSpan = max - min;                              //温度跨度
                            this.meTmpMin = min;                                    //温度最小值
                            break;

                        case "TH_T":    //温湿度采集器
                            this.tempSpan = max - min;                              //温度跨度
                            this.meTmpMin = min;                                    //温度最小值
                            break;

                        case "TH_H":    //温湿度采集器
                            this.humSpan = max - min;                               //湿度跨度
                            this.meHumMin = min;                                    //湿度最小值
                            MyDefine.myXET.drawHumCurve = true;                     //允许绘制湿度曲线
                            break;

                        case "TQ_T":    //温湿度采集器
                            this.tempSpan = max - min;                              //温度跨度
                            this.meTmpMin = min;                                     //温度最小值
                            break;

                        case "TQ_H":    //温湿度采集器
                            this.humSpan = max - min;                               //湿度跨度
                            this.meHumMin = min;                                    //湿度最小值
                            MyDefine.myXET.drawHumCurve = true;                     //允许绘制湿度曲线
                            break;

                        case "TP_P":    //压力采集器
                            this.prsSpan = max - min;                                  //压力跨度
                            this.mePrsMin = min;                                         //压力最小值
                            break;

                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        #endregion

        #endregion

        #region 更新ComboBox1曲线名称列表

        //清空ComboBox1曲线名称列表
        public void ClearCurveNameList()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.Add("All_Lines");
        }

        //更新ComboBox1曲线列表
        public void UpdateCurveNameList()
        {
            try
            {
                if (MyDefine.myXET.meDataTbl == null)     //数据表为空
                {
                    ClearCurveNameList();
                    return;
                }

                //清空曲线名称列表
                ClearCurveNameList();

                //添加曲线名称列表
                for (int i = 1; i < MyDefine.myXET.meDataTbl.dataTable.Columns.Count; i++)
                {
                    //if (i > 20) return;             //最多显示20条曲线

                    if (i > 20)
                    {
                        myLabelArray.Add(new Label());
                        myLabelColor.Add(new Label());
                    }
                    string curveName = MyDefine.myXET.meDataTbl.dataTable.Columns[i].ColumnName;       //温度：HTTxx_n；温湿度：HTHxx_n(温度)，HTH_Hxx_n(湿度)；压力：HTPxx_n
                    comboBox1.Items.Add(curveName);                                                    //xx_n,xx_n,xx_n

                    /*
                    //加载名称曲线列表--温度：TTxx_n；温湿度：THxx_n(温度)，RHxx_n(湿度)；压力：TPxx_n
                    if (curveName.Contains("HTH_H"))                
                    {
                        //comboBox1.Items.Add("RH" + curveName.Substring(5)); //RHxx_n
                        comboBox1.Items.Add(curveName.Substring(5));          //xxH
                    }
                    else
                    {
                        //comboBox1.Items.Add(curveName.Substring(1));        //TTxx_n,THxx_n,TPxx_n
                        comboBox1.Items.Add(curveName.Substring(3));          //xx_n,xx_n,xx_n
                    }
                    */
                }

                comboBox1.SelectedIndex = 0;        //默认选择All_Lines，显示所有曲线
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        #endregion

        #region 放大缩小曲线(滚轮事件)

        //添加PictureBox1滚轮事件：放大缩小曲线

        float Zoom = 1.0f;
        private double tempSpanZS = 400;     //温度范围-100~300℃，跨度400℃
        private double meTmpMinZS = 100;       //温度0点距离pictureBox1坐标0点(300℃)的跨度为300℃
        private double humSpanZS = 400;      //湿度范围
        private double meHumMinZS = 100;     //湿度0点距离pictureBox1坐标0点跨度
        private double prsSpanZS = 400;      //压力范围
        private double mePrsMinZS = 100;       //压力0点距离pictureBox1坐标0点跨度
        private Point startPoint;

        /// <summary>
        /// 滚轮滚动事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            drawPicture = true;
            pictureBox1.Cursor = Cursors.Hand;
            if (e.Delta > 0)    //鼠标滚动轴往上滚
            {
                Zoom *= 1.1f;
                if (Zoom > 15) Zoom = 15f;
            }
            else                // 鼠标滚动轴 往下滚
            {
                Zoom /= 1.1f;
                if (Zoom < 1f)
                {
                    Zoom = 1f;
                    drawPicture = false;
                    pictureBox1.Cursor = Cursors.Default;
                }
            }
            Zoom = (float)Math.Round(Zoom, 2);

            getPictureMessage(e.X, e.Y);
            drawPointMessage();                     //重新绘制放大后的图像
                                                    // DrawFileDataLines();                //重新加载界面后重新绘制曲线
            ShowValidLines();                   //显示可能存在的有效数据竖直线
            //ShowValidInfo();                    //显示右侧有效数据信息
        }

        /// <summary>
        /// 鼠标点击时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            startPoint = e.Location;
        }

        /// <summary>
        /// 鼠标释放时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (drawPicture)
            {
                int x = e.X - startPoint.X;
                int y = e.Y - startPoint.Y;
                Int32 nowNum = indexEnd - indexStart + 1;              //picture中数据总个数
                float x_perGrid = (float)pictureBox1.Width / (nowNum - 1);         //若要显示所有数据，每个数据占的单位格数
                Int32 num = (int)(x / x_perGrid);                       //移动的数据个数

                indexStart -= num;
                indexEnd -= num;
                if (indexStart < 0)
                {
                    indexEnd -= indexStart;
                    indexStart = 0;
                }
                if (indexEnd >= MyDefine.myXET.meDataTbl.dataTable.Rows.Count)
                {
                    indexStart -= indexEnd - MyDefine.myXET.meDataTbl.dataTable.Rows.Count + 1;
                    indexEnd = MyDefine.myXET.meDataTbl.dataTable.Rows.Count - 1;
                }

                myStartTime = Convert.ToDateTime(MyDefine.myXET.meDataTbl.GetCellValue(indexStart, 0));    //开始鼠标所在日期
                myStopTime = Convert.ToDateTime(MyDefine.myXET.meDataTbl.GetCellValue(indexEnd, 0));    //结束鼠标所在日期
                myTestSeconds = (Int32)(myStopTime.Subtract(myStartTime).TotalSeconds);     //总测试时间(秒)

                if (this.tempSpan != double.MaxValue)
                {
                    double unitTempHeight = pictureBox1.Height / tempSpanZS;      //每单位℃在pictureBox1上的高度
                    this.meTmpMinZS += y / unitTempHeight;

                    if (meTmpMinZS < meTmpMin)
                    {
                        meTmpMinZS = meTmpMin;
                    }
                    else if (meTmpMinZS > meTmpMin + tempSpan - tempSpanZS)
                    {
                        meTmpMinZS = meTmpMin + tempSpan - tempSpanZS;
                    }
                }
                if (this.humSpan != double.MaxValue)
                {
                    double unitHumHeight = pictureBox1.Height / humSpanZS;        //每单位℃在pictureBox1上的高度
                    this.meHumMinZS += y / unitHumHeight;

                    if (meHumMinZS < meHumMin)
                    {
                        meHumMinZS = meHumMin;
                    }
                    else if (meHumMinZS > meHumMin + humSpan - humSpanZS)
                    {
                        meHumMinZS = meHumMin + humSpan - humSpanZS;
                    }
                }
                if (this.prsSpan != double.MaxValue)
                {
                    double unitPsrHeight = pictureBox1.Height / prsSpanZS;        //每单位℃在pictureBox1上的高度
                    this.mePrsMinZS += y / unitPsrHeight;

                    if (mePrsMinZS < mePrsMin)
                    {
                        mePrsMinZS = mePrsMin;
                    }
                    else if (mePrsMinZS > mePrsMin + prsSpan - prsSpanZS)
                    {
                        mePrsMinZS = mePrsMin + prsSpan - prsSpanZS;
                    }
                }

                drawPointMessage();                  //重新绘制放大后的图像
                ShowValidLines();                   //显示可能存在的有效数据竖直线
            }
        }

        //获取放大图线信息
        private void getPictureMessage(int x, int y)
        {
            try
            {
                if (MyDefine.myXET.meDataTbl == null) return;

                double unitTempHeight;     //每单位℃在pictureBox1上的高度
                double unitHumHeight;        //每单位℃在pictureBox1上的高度
                double unitPsrHeight;
                Int32 nowNum = indexEnd - indexStart + 1;              //数据总个数
                int index = (int)((1.0 * x / pictureBox1.Width) * nowNum) + indexStart;               //鼠标位置对应的数据索引
                Int32 dataNum = MyDefine.myXET.meDataTbl.dataTable.Rows.Count - 1;
                int num = (int)(dataNum / Zoom);
                indexStart = index - num * index / dataNum;
                indexEnd = num + indexStart;

                myStartTime = Convert.ToDateTime(MyDefine.myXET.meDataTbl.GetCellValue(indexStart, 0));    //开始鼠标所在日期
                myStopTime = Convert.ToDateTime(MyDefine.myXET.meDataTbl.GetCellValue(indexEnd, 0));    //结束鼠标所在日期
                myTestSeconds = (Int32)(myStopTime.Subtract(myStartTime).TotalSeconds);     //总测试时间(秒)

                if (this.tempSpan != double.MaxValue)
                {
                    unitTempHeight = pictureBox1.Height / tempSpanZS;     //每单位℃在pictureBox1上的高度
                    double MeasY1 = (pictureBox1.Height - y) / unitTempHeight + meTmpMinZS; //Y轴--温度轴 //计算鼠标所在位置的温度值：根据y1 = pictureBox1.Height - (int)((myTemp1 - meTmpMin) * unitTempHeight)反推的计算公式

                    this.tempSpanZS = tempSpan / Zoom;                                //温度跨度
                    this.meTmpMinZS = MeasY1 - (MeasY1 - meTmpMin) / Zoom;//温度最小值
                }

                if (this.humSpan != double.MaxValue)
                {
                    unitHumHeight = pictureBox1.Height / humSpanZS;     //每单位℃在pictureBox1上的高度
                    double MeasY1 = (pictureBox1.Height - y) / unitHumHeight + meHumMinZS; //Y轴--温度轴 //计算鼠标所在位置的温度值：根据y1 = pictureBox1.Height - (int)((myTemp1 - meTmpMin) * unitTempHeight)反推的计算公式

                    this.humSpanZS = humSpan / Zoom;                                //温度跨度
                    this.meHumMinZS = MeasY1 - (MeasY1 - meHumMin) / Zoom;//温度最小值
                }

                if (this.prsSpan != double.MaxValue)
                {
                    double unitPrsHeight = pictureBox1.Height / prsSpan;     //每单位℃在pictureBox1上的高度
                    double MeasY1 = (pictureBox1.Height - y) / unitPrsHeight + mePrsMin; //Y轴--温度轴 //计算鼠标所在位置的温度值：根据y1 = pictureBox1.Height - (int)((myTemp1 - meTmpMin) * unitTempHeight)反推的计算公式

                    this.prsSpanZS = prsSpan / Zoom;                                //温度跨度
                    this.mePrsMinZS = MeasY1 - (MeasY1 - mePrsMin) * prsSpanZS / prsSpan;//温度最小值
                }
            }
            catch { }
        }

        //绘制放大后图线
        private void drawPointMessage()
        {
            try
            {
                if (MyDefine.myXET.meDataTbl == null) return;

                double unitTempHeight;     //每单位℃在pictureBox1上的高度
                double unitHumHeight;        //每单位℃在pictureBox1上的高度
                double unitPsrHeight;
                Int32 dataNum;
                float x = 0;
                int y1 = 0, y2 = 0;

                #region 画图
                dataNum = indexEnd - indexStart + 1;          //数据总数
                float x_perGrid = (float)pictureBox1.Width / (dataNum - 1);         //若要显示所有数据，每个数据占的单位格数
                unitTempHeight = pictureBox1.Height / tempSpanZS;      //每单位℃在pictureBox1上的高度
                unitHumHeight = pictureBox1.Height / humSpanZS;        //每单位℃在pictureBox1上的高度
                unitPsrHeight = pictureBox1.Height / prsSpanZS;        //每单位℃在pictureBox1上的高度

                Bitmap img = new Bitmap(pictureBox1.Width, pictureBox1.Height);     //层图
                Graphics g = Graphics.FromImage(img);       //绘制
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.Clear(Color.White);    //必须先清除绘图，否则DrawString()写的字会有重影(无意中发现此方法可解决重影问题)
                DrawPictureGrid(g);      //画背景网格线  

                DrawPictureInfosZoom(g);            //更新Y轴坐标信息
                updateAxisLabel();             //更新X轴时间

                for (int i = 1; i < comboBox1.Items.Count; i++)                     //meDataTbl.dataTable的0列是时间列，comboBox1.Items的索引0是All_Line，所以从1开始
                {
                    myLabelColor[i - 1].BackColor = colors[i % 20];                      //曲线条颜色
                                                                                         //if (myBoxArray[i - 1].Checked == false) continue;             //曲线为非选中状态，不绘制
                    if (myLabelColor[i - 1].Text == string.Empty) continue;         //曲线为非选中状态，不绘制
                    if (i >= MyDefine.myXET.meTypeList.Count) continue;             //不明原因，有时候会出现超出meTypeList索引范围异常

                    x = y1 = y2 = 0;        //一列数据一条曲线                
                    String myDeviceType = MyDefine.myXET.meTypeList[i];   //产品类型

                    for (int j = indexStart; j <= indexEnd - 1; j++, x = x + x_perGrid)
                    {
                        if (MyDefine.myXET.meDataTbl.dataTable.Rows[j][i].ToString() == "" || MyDefine.myXET.meDataTbl.dataTable.Rows[j + 1][i].ToString() == "") continue;//空数据
                        Double mydata1 = Convert.ToDouble(MyDefine.myXET.meDataTbl.dataTable.Rows[j][i]);
                        Double mydata2 = Convert.ToDouble(MyDefine.myXET.meDataTbl.dataTable.Rows[j + 1][i]);
                        if (x > pictureBox1.Width) x = pictureBox1.Width;

                        switch (myDeviceType)
                        {
                            case "TT_T":    //温度采集器
                                y1 = pictureBox1.Height - (int)((mydata1 - meTmpMinZS) * unitTempHeight);
                                y2 = pictureBox1.Height - (int)((mydata2 - meTmpMinZS) * unitTempHeight);
                                //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[i], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                break;

                            case "TH_T":    //温湿度采集器
                                y1 = pictureBox1.Height - (int)((mydata1 - meTmpMinZS) * unitTempHeight);
                                y2 = pictureBox1.Height - (int)((mydata2 - meTmpMinZS) * unitTempHeight);
                                //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[i], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                break;

                            case "TH_H":    //温湿度采集器
                                if (!MyDefine.myXET.drawHumCurve) continue;        //存在压力数据时，不画湿度值
                                y1 = pictureBox1.Height - (int)((mydata1 - meHumMinZS) * unitHumHeight);
                                y2 = pictureBox1.Height - (int)((mydata2 - meHumMinZS) * unitHumHeight);
                                //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[i], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                break;

                            case "TQ_T":    //温湿度采集器
                                y1 = pictureBox1.Height - (int)((mydata1 - meTmpMinZS) * unitTempHeight);
                                y2 = pictureBox1.Height - (int)((mydata2 - meTmpMinZS) * unitTempHeight);
                                //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[i], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                break;

                            case "TQ_H":    //温湿度采集器
                                if (!MyDefine.myXET.drawHumCurve) continue;        //存在压力数据时，不画湿度值
                                y1 = pictureBox1.Height - (int)((mydata1 - meHumMinZS) * unitHumHeight);
                                y2 = pictureBox1.Height - (int)((mydata2 - meHumMinZS) * unitHumHeight);
                                //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[i], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                break;

                            case "TP_P":    //压力采集器
                                y1 = pictureBox1.Height - (int)((mydata1 - mePrsMinZS) * unitPsrHeight);
                                y2 = pictureBox1.Height - (int)((mydata2 - mePrsMinZS) * unitPsrHeight);
                                //if (x < pictureBox1.Width) g.DrawLine(new Pen(colors[i], 1.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                break;

                            default:
                                break;
                        }

                    }
                }

                pictureBox1.Image = img;    //铺图
                g.Dispose();

                #endregion
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }
        #endregion

        #endregion

        #region 手动更改左右轴上下限
        //选择轴
        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_left.Checked == false && rb_right.Checked == false)
            {
                return;
            }
            if (tb_upperLimit.Text == "" || tb_downLimit.Text == "")
            {
                MessageBox.Show("请先输入上下限");
                rb_left.Checked = false;
                rb_right.Checked = false;
                return;
            }
            double upperValue = Convert.ToDouble(tb_upperLimit.Text);
            double limitValue = Convert.ToDouble(tb_downLimit.Text);
            if (rb_left.Checked)
            {
                MyDefine.myXET.AddTraceInfo("设置左轴上下限");
                if (upperValue < MyDefine.myXET.recordMaxMin[0])
                {
                    MessageBox.Show("上限值不能小于数据的最大值");
                    rb_left.Checked = false;
                    return;
                }
                if (limitValue > MyDefine.myXET.recordMaxMin[1])
                {
                    MessageBox.Show("下限值不能大于数据的最大值");
                    rb_left.Checked = false;
                    return;
                }
                MyDefine.myXET.CustomAxes[0] = true;
                MyDefine.myXET.leftLimit[0] = upperValue;
                MyDefine.myXET.leftLimit[1] = limitValue;
                rb_left.Checked = false;
            }
            else if (rb_right.Checked)
            {
                MyDefine.myXET.AddTraceInfo("设置右轴上下限");
                if (upperValue < MyDefine.myXET.recordMaxMin[2])
                {
                    MessageBox.Show("上限值不能小于数据的最大值");
                    rb_right.Checked = false;
                    return;
                }
                if (limitValue > MyDefine.myXET.recordMaxMin[3])
                {
                    MessageBox.Show("下限值不能大于数据的最大值");
                    rb_right.Checked = false;
                    return;
                }
                MyDefine.myXET.CustomAxes[1] = true;
                MyDefine.myXET.rightLimit[0] = upperValue;
                MyDefine.myXET.rightLimit[1] = limitValue;
                rb_right.Checked = false;
            }
            updateCurvelDrawing();
        }

        //上下限输入限制
        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            //只允许输入数字和删除键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != 8) && (e.KeyChar != '.') && (e.KeyChar != '-'))
            {
                e.Handled = true;
                return;
            }

            //长度限制
            if (((System.Windows.Forms.TextBox)sender).Text.Length > 7)
            {
                e.Handled = true;
                return;
            }
        }

        #endregion

    }
}

//end


