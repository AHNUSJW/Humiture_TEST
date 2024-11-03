using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Runtime.InteropServices;

namespace HTR
{
    public partial class MenuCalCurvePanel : UserControl
    {
        public  UInt16 total = 0;//控制进度条
        private String myPath = MyDefine.myXET.userDAT;
        private Int32 currentCurveIndex = 0;   //当前曲线数据的索引

        public MenuCalCurvePanel()
        {
            InitializeComponent();
        }

        #region 界面加载/关闭
        
        private void MenuCurvePanel_Load(object sender, EventArgs e)
        {
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
                MyDefine.myXET.meTblCurve = new dataTableClass();
                if (this.Name == "校准曲线图" && MyDefine.myXET.meTblPreCurve != null) MyDefine.myXET.meTblCurve.dataTable = MyDefine.myXET.meTblPreCurve.CopyTable();
                if (this.Name == "标定曲线图" && MyDefine.myXET.meTblCalCurve != null) MyDefine.myXET.meTblCurve.dataTable = MyDefine.myXET.meTblCalCurve.CopyTable();
               
                UpdateCurveInfo();                      //加载文件后更新曲线信息（曲线名称列表、曲线颜色列表）
                updateCurvelDrawing();
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }            
        }

        public void updateCurvelDrawing()
        {
            DrawFileDataLines();            //重新加载界面后重新绘制曲线
        }

        #endregion

        #region 界面按钮事件

        #region 加载文件按钮 -- 加载单个或多个文件数据(按钮未用)

        /// <summary>
        /// 加载文件后更新曲线信息（曲线名称列表、曲线颜色列表、曲线有效数据信息）
        /// </summary>
        public void UpdateCurveInfo()
        {
            UpdateCurveNameList();              //更新曲线名称列表
            setControlArrayAll();               //设置并显示所有曲线名称
            //ResetCurveInfo();                 //复位曲线信息、有效数据信息

            //DrawFileDataLines();              //绘制曲线
        }

        #endregion

        #region 保存图片按钮 -- 将曲线图保存为图片

        private void button7_Click(object sender, EventArgs e)
        {
            comboBox1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

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

            //button6.Visible = true;     //显示保存数据按钮
            //Application.DoEvents();
        }

        #endregion

        #region 选择曲线复选框 -- 选择单条或所有曲线后重新绘图

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (MyDefine.myXET.meDataTbl == null) return;                    //数据表未实例化
                currentCurveIndex = comboBox1.SelectedIndex;    //保存当前曲线索引(为0表示多条曲线)
                if (currentCurveIndex == 0) setControlArrayAll();                   //设置所有曲线为选中状态
                if (currentCurveIndex > 0) setControlArraySingle(currentCurveIndex - 1);      //选中单条曲线。comboBox1的曲线索引比myBoxArray的对应曲线索引大1，因为comboBox1的索引0为All_Lines

                DrawFileDataLines();                                                //绘制所有选中曲线

                /*
                if (currentCurveIndex == 0)                     //绘制所有数据曲线
                {
                    //button6.Enabled = false;                  //禁止保存数据按钮
                    //button6.ForeColor = Color.DimGray;
                    //resetCurveInfo();                         //将曲线信息复位为整条曲线的信息
                    setControlArrayAll();                       //设置所有曲线为选中状态
                    DrawFileDataLines();                        //绘制所有曲线
                    DrawSelectArea();                           //重新绘制可能存在的区域矩形
                    DrawValidLines();                           //重新绘制可能存在的开始或结束线条
                }
                else                                            //绘制选中曲线(单条曲线)
                {
                    //button6.Enabled = true;                     //是能保存数据按钮
                    //button6.ForeColor = Color.OliveDrab;
                    setControlArraySingle(currentCurveIndex - 1);      //comboBox1的曲线索引比myBoxArray的对应曲线索引大1，因为comboBox1的索引0为All_Lines
                    DrawFileDataLines();                        //绘制所有曲线
                    DrawSelectArea();                           //重新绘制可能存在的区域矩形
                    DrawValidLines();                           //重新绘制可能存在的开始或结束线条
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
        public List<Label> myLabAxisX = new List<Label>();
        public List<Label> myLabAxisY = new List<Label>();
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

            //X轴坐标，读数时间
            myLabAxisX.Add(label13);
            myLabAxisX.Add(label14);
            myLabAxisX.Add(label15);
            myLabAxisX.Add(label16);
            myLabAxisX.Add(label17);
            myLabAxisX.Add(label18);
            myLabAxisX.Add(label67);
            myLabAxisX.Add(label68);

            //Y轴坐标：左侧-温度/压力 0-4
            myLabAxisY.Add(label2);
            myLabAxisY.Add(label6);
            myLabAxisY.Add(label4);
            myLabAxisY.Add(label11);
            myLabAxisY.Add(label3);

            //Y轴坐标：右侧-湿度 5-9
            myLabAxisY.Add(label7);
            myLabAxisY.Add(label8);
            myLabAxisY.Add(label9);
            myLabAxisY.Add(label10);
            myLabAxisY.Add(label12);

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

            for (int i = 0; i < myLabAxisX.Count; i++)
            {
                //myLabAxisX[i].Text = "";
            }

            for (int i = 0; i < myLabAxisY.Count; i++)
            {
                //myLabAxisY[i].Text = "";
            }
        }

        #endregion

        #region 检查控件数组是否已创建

        //检查控件数组是否已创建
        public Boolean IsControlArrayReady()
        {
            if (myLabelColor.Count == 0) return false;
            if (myLabelArray.Count == 0) return false;
            if (myLabAxisX.Count == 0) return false;
            if (myLabAxisY.Count == 0) return false;
            return true;
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

            for (int i = 0; i < myLabAxisX.Count; i++)
            {
                myLabAxisX[i].Text = "";
            }

            for (int i = 0; i < myLabAxisY.Count; i++)
            {
                myLabAxisY[i].Text = "";
            }
        }

        #endregion

        #region 清空X轴坐标刻度

        public void ClearAxisX()
        {
            if (!IsControlArrayReady()) return;                     //控件数组未创建

            for (int i = 0; i < myLabAxisX.Count; i++)
            {
                myLabAxisX[i].Text = "";
            }
        }

        #endregion

        #region 清空Y轴坐标刻度

        public void ClearAxisY()
        {
            if (!IsControlArrayReady()) return;                     //控件数组未创建

            label1.Visible = false;                    //隐藏Y轴温度标题
            label20.Visible = false;                   //隐藏Y轴湿度标题
            label21.Visible = false;                   //隐藏Y轴压力标题

            for (int i = 0; i < myLabAxisY.Count; i++)
            {
                myLabAxisY[i].Text = "";
            }
        }

        #endregion

        #region 设置控件数组 -- 设置所有曲线为选中状态

        //设置所有曲线为选中状态
        public void setControlArrayAll()
        {
            if (!IsControlArrayReady()) return;                     //控件数组未创建
            if (MyDefine.myXET.meDataTbl == null) return;

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
            int idx = ((Label)sender).TabIndex;
            myLabelColor[idx].Text = myLabelColor[idx].Text == string.Empty ? "√" : string.Empty;   //选中状态翻转
            currentCurveIndex = getDisplayCurveIndex();           //保存当前曲线索引(若为多条曲线则为0)
            DrawFileDataLines();                                  //绘制单条或多条曲线
        }

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
            float ratioX = 0f;                          //屏幕横向缩放比例
            float ratioY = 0f;                          //屏幕竖向缩放比例
            GetDPIScale(ref ratioX, ref ratioY);        //获取当前屏幕缩放比例(100%/125%/150%)
            MyDefine.myXET.AddTraceInfo("屏幕比例：" + ratioX.ToString() + "," + ratioY.ToString());
            //MyDefine.myXET.AddToTraceRecords("数据曲线", "屏幕比例：" + ratioX.ToString() + "," + ratioY.ToString());

            Size desSize = myCon.Size;
            Point desPoint = myCon.PointToScreen(Point.Empty);         //获取控件在屏幕上的坐标(100%缩放比例时的坐标)
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

        #region 加载文件并绘制曲线

        #region 变量定义

        //dataTableClass meDataTbl;         //测试数据加载数据表
        //dataTableClass meInfoTbl;     //记录数据表参数信息(开始时间、结束时间等)
        //Color[] colors = new Color[] { Color.Green, Color.Peru, Color.Brown, Color.Tomato, Color.Cyan, Color.Gray, Color.Orange, Color.CadetBlue, Color.DarkSeaGreen, Color.CornflowerBlue, Color.ForestGreen, Color.Firebrick, Color.Salmon };
        //Color[] colors = new Color[] { Color.White, Color.YellowGreen, Color.LightSeaGreen, Color.Green, Color.CornflowerBlue, Color.RoyalBlue, Color.Orange, Color.Gold, Color.SlateBlue, Color.DeepSkyBlue, Color.Violet, Color.DarkGray, Color.LightCoral, Color.IndianRed, Color.Peru, Color.OrangeRed, Color.Goldenrod, Color.BlueViolet, Color.Magenta, Color.Crimson, Color.Pink };
        //Color[] colors = new Color[] { Color.White, Color.DarkGoldenrod, Color.Teal, Color.IndianRed, Color.DodgerBlue, Color.Magenta, Color.Orange, Color.DarkGreen, Color.SlateBlue, Color.DeepSkyBlue, Color.Violet, Color.SaddleBrown, Color.LightCoral, Color.SeaGreen, Color.DarkOrange, Color.OrangeRed, Color.DarkTurquoise, Color.BlueViolet, Color.Magenta, Color.Crimson, Color.RoyalBlue };
        Color[] colors = new Color[] { Color.RoyalBlue, Color.SaddleBrown, Color.ForestGreen, Color.Brown, Color.DodgerBlue, Color.SteelBlue, Color.DarkOrange, Color.DarkGreen, Color.BlueViolet, Color.Crimson, Color.OrangeRed, Color.DarkGoldenrod, Color.LightCoral, Color.LimeGreen, Color.DarkOrange, Color.Violet, Color.DarkTurquoise, Color.SlateBlue, Color.Magenta, Color.DeepSkyBlue, Color.RoyalBlue };
        Boolean drawHumCurve = true;

        #endregion

        #region 绘制多条曲线 -- 根据颜色条的选中状态

        private Point[] mPoints1 = new Point[0];        //坐标点集合

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

                if (MyDefine.myXET.meTblCurve != null)                //数据表不为空
                {
                    getDataLimits();                //计算温度值的绘图上下限tempSpan/meTMax
                    DrawPictureInfos(g);            //更新Y轴坐标信息

                    float x = 0;
                    int y1 = 0;
                    double myTemp1, minLimit = 0f;
                    int dataNum = 8;                                                    //数据总数
                    float unitHeight = 0f;                                              //每单位值在pictureBox1上的高度
                    float x_perGrid = (float)pictureBox1.Width / (dataNum - 1);         //若要显示所有数据，每个数据占的单位格数
                    float unitTmpHeight = (float)(pictureBox1.Height / tempSpan);       //每单位℃在pictureBox1上的高度
                    float unitHumHeight = (float)(pictureBox1.Height / humSpan);        //每单位℃在pictureBox1上的高度
                    float unitPsrHeight = (float)(pictureBox1.Height / psrSpan);        //每单位℃在pictureBox1上的高度

                    for (int i = 1; i < comboBox1.Items.Count; i++)                     //meTblCurve的0列是空列，comboBox1.Items的索引0是All_Line，所以从1开始
                    {
                        myLabelColor[i - 1].BackColor = colors[i % 20];                      //曲线条颜色
                        if (comboBox1.Items[i].ToString() == "标定后曲线") myLabelColor[i - 1].BackColor = Color.Red;      //将标定后曲线设置为红色
                        if (myLabelColor[i - 1].Text == string.Empty) continue;         //曲线为非选中状态，不绘制
                        if (i >= MyDefine.myXET.meCalTypeList.Count) continue;          //不明原因，有时候会出现超出meCalTypeList索引范围异常

                        x = y1 = 0;                     //一列数据一条曲线           
                        mPoints1 = new Point[0];        //坐标点集合

                        String myDeviceType = MyDefine.myXET.meCalTypeList[i];                 //产品类型
                        if (myDeviceType == "TT_T") { unitHeight = unitTmpHeight; minLimit = meTmpMin; }
                        if (myDeviceType == "TH_T") { unitHeight = unitTmpHeight; minLimit = meTmpMin; }
                        if (myDeviceType == "TH_H") { unitHeight = unitHumHeight; minLimit = meHumMin; }
                        if (myDeviceType == "TQ_T") { unitHeight = unitTmpHeight; minLimit = meTmpMin; }
                        if (myDeviceType == "TQ_H") { unitHeight = unitHumHeight; minLimit = meHumMin; }
                        if (myDeviceType == "TP_P") { unitHeight = unitPsrHeight; minLimit = mePrsMin; }

                        for (int j = 0; j < 8; j++, x = x + x_perGrid)
                        {
                            String myStr = MyDefine.myXET.meTblCurve.GetCellValue(j, i);
                            if (myStr == "") continue;                //校准，温度点可能不满8个(标定时，部分设备温度、读数时间可能为空)
                            //x = x + x_perGrid;

                            if (Double.TryParse(myStr, out myTemp1))  //数据格式正确
                            {
                                Array.Resize<Point>(ref mPoints1, mPoints1.Length + 1);
                                y1 = pictureBox1.Height - (int)((myTemp1 - minLimit) * unitHeight);
                                mPoints1[mPoints1.Length - 1] = new Point((int)x, y1);
                            }

                            //MessageBox.Show(unitHeight.ToString() + Environment .NewLine + myTemp1.ToString ()+ Environment.NewLine + meTmpMin.ToString());
                        }

                        if (mPoints1.Length >= 2)       //点数大于等于2
                        {
                            //画曲线
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            
                            if (comboBox1.Items[i].ToString() == "标定后曲线")                  //标定后曲线加粗红色显示
                            {
                                Pen mypen = new Pen(Color.Red, 2.00f);                          //定义背景网格画笔
                                mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;      //画虚线

                                myLabelColor[i - 1].BackColor = Color.Red;
                                g.DrawCurve(mypen, mPoints1, 0.3f);
                            }
                            else
                            {
                                g.DrawCurve(new Pen(colors[i % 20], 1.0f), mPoints1, 0.3f);           //画曲线
                            }

                            //画圆点
                            for (int m = 0; m < mPoints1.Length; m++)
                            {
                                g.FillEllipse(new SolidBrush(colors[i % 20]), (float)(mPoints1[m].X - 5), (mPoints1[m].Y - 4), 8, 8);//画圆点
                                if (comboBox1.Items[i].ToString() == "标定后曲线")               //标定后曲线加粗红色显示
                                {
                                    g.FillEllipse(new SolidBrush(Color.Red), (float)(mPoints1[m].X - 5), (mPoints1[m].Y - 4), 8, 8);
                                }
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
                gridnum = 7;
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

        #region 更新Y轴坐标信息

        //更新Y轴坐标值
        private void DrawPictureInfos(Graphics g)
        {
            try
            {
                if (MyDefine.myXET.meDataTbl == null) return;

                String MeasY;
                int idx = 0;                            //Y轴坐标索引
                int height = pictureBox1.Height;
                int width = pictureBox1.Width;
                ClearAxisY();                           //清空Y轴坐标

                //存在温度数据
                //if (MyDefine.myXET.meCalTypeList[1].Contains("_T"))     //TT_T、TH_T：存在温度列，绘图(温度或温湿度数据)
                if (this.tempSpan < Double.MaxValue - 10)    //温度跨度非最大值：存在温度列，绘图
                {
                    idx = 0;                                 //Y轴坐标索引初始值
                    double unitTmpHeight = height / tempSpan;//每单位℃在pictureBox1上的高度
                    label1.Visible = true;                   //显示Y轴温度标题

                    for (double i = 0; i <= 1; i += 0.25)
                    {
                        //计算坐标点所在位置的温度值：根据y1 = pictureBox1.Height - (int)((myTemp1 - meTmpMin) * unitTempHeight)反推的计算公式
                        MeasY = ((height - i * height) / unitTmpHeight + meTmpMin).ToString("F2");        //计算pictureBox1坐标0点对应的温度值
                        if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(2, (int)(i * height - 0)));
                        myLabAxisY[idx++].Text = MeasY;
                    }
                }

                //存在湿度数据
                //if (MyDefine.myXET.meCalTypeList[1].Contains("TH_H"))     //存在湿度列，绘图(温湿度数据)
                if (this.humSpan < Double.MaxValue - 10)    //湿度跨度非最大值：存在湿度列，绘图
                {
                    idx = 5;                                //Y轴坐标索引初始值
                    double unitHumHeight = height / humSpan;//每单位℃在pictureBox1上的高度
                    label20.Visible = true;                 //显示Y轴湿度标题

                    for (double i = 0; i <= 1; i += 0.25)
                    {
                        //计算坐标点所在位置的温度值：根据y1 = pictureBox1.Height - (int)((myTemp1 - meTmpMin) * unitTempHeight)反推的计算公式
                        MeasY = ((height - i * height) / unitHumHeight + meHumMin).ToString("F2");        //计算pictureBox1坐标0点对应的温度值
                        if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(width - GetSpace(MeasY), (int)(i * height - 0)));
                        myLabAxisY[idx++].Text = MeasY;
                    }
                }

                //存在压力数据
                //if (MyDefine.myXET.meCalTypeList[1].Contains("TP_P"))     //存在压力列，绘图(压力数据)
                if (this.psrSpan < Double.MaxValue - 10)    //压力跨度非最大值：存在压力列，绘图
                {
                    idx = 0;                                //Y轴坐标索引初始值
                    double unitPsrHeight = height / psrSpan;//每单位℃在pictureBox1上的高度
                    label21.Visible = true;                 //显示Y轴压力标题

                    for (double i = 0; i <= 1; i += 0.25)
                    {
                        //计算坐标点所在位置的温度值：根据y1 = pictureBox1.Height - (int)((myTemp1 - meTmpMin) * unitTempHeight)反推的计算公式
                        MeasY = ((height - i * height) / unitPsrHeight + mePrsMin).ToString("F2");        //计算pictureBox1坐标0点对应的温度值
                        if (MyDefine.myXET.meDebugMode) g.DrawString(MeasY, new Font("Arial", 8), Brushes.Gray, new PointF(2, (int)(i * height - 0)));
                        myLabAxisY[idx++].Text = MeasY;
                    }
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        //更新Y轴坐标值
        private void DrawPictureInfos0(Graphics g)
        {
            try
            {
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

                if (this.psrSpan < Double.MaxValue - 10)       //压力跨度非最大值：存在压力列，绘图
                {
                    //MessageBox.Show("1");
                    label21.Visible = true;
                    label7.Visible = true;
                    label8.Visible = true;
                    label9.Visible = true;
                    label10.Visible = true;
                    label12.Visible = true;

                    //=====更新湿度值Y轴信息================================================================================================================
                    double unitPsrHeight = pictureBox1.Height / psrSpan;     //每单位℃在pictureBox1上的高度

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

                if (this.humSpan < int.MaxValue - 10 && drawHumCurve == true)       //湿度跨度非最大值：存在湿度列且允许绘制湿度曲线(没有同时存在压力列)，绘图
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
                String mytpye = MyDefine.myXET.meTypeList[1];
                label7.Visible = mytpye.Contains("TH") ? true : false;
                label8.Visible = mytpye.Contains("TH") ? true : false;
                label9.Visible = mytpye.Contains("TH") ? true : false;
                label10.Visible = mytpye.Contains("TH") ? true : false;
                label12.Visible = mytpye.Contains("TH") ? true : false;
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
                if (!IsControlArrayReady()) return;         //控件数组未创建
                ClearAxisX();                               //清空X轴坐标信息

                String[] myReadTime = new String[8];
                if (this.Name == "校准曲线图") myReadTime = MyDefine.myXET.meTblPre1.GetRowArray(0, 1);
                if (this.Name == "标定曲线图") myReadTime = MyDefine.myXET.meTblCal1.GetRowArray(0, 1);
                for (int i = 0; i < 8; i++) 
                {
                    myLabAxisX[i].Text = myReadTime[i];
                }
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
        private double meTmpMin = 100;     //温度0点距离pictureBox1坐标0点(300℃)的跨度为300℃
        private double humSpan = 400;      //湿度范围
        private double meHumMin = 100;     //湿度0点距离pictureBox1坐标0点跨度
        private double psrSpan = 400;      //压力范围
        private double mePrsMin = 100;     //压力0点距离pictureBox1坐标0点跨度

        #region 查找测试数据最大、最小值(用于多条曲线绘制)

        //分别查找温度、湿度、压力的最大最小值
        public void getDataLimits()
        {
            try
            {
                if (MyDefine.myXET.meDataTbl == null) return;           //数据列表为空
                if (MyDefine.myXET.meTblCurve == null) return;          //曲线数据列表为空
                if (!IsControlArrayReady()) return;                     //控件数组未创建

                this.tempSpan = double.MaxValue;
                this.humSpan = double.MaxValue;
                this.psrSpan = double.MaxValue;

                Double max, min;
                Double maxTemp = double.MinValue;
                Double minTemp = double.MinValue;
                Double maxHum = double.MinValue;
                Double minHum = double.MinValue;
                Double maxPsr = double.MinValue;
                Double minPsr = double.MinValue;
                String myDeviceType;

                for (int i = 1; i < comboBox1.Items.Count; i++)               //comboBox1.Items.Count最大值为20
                {
                    //if (myBoxArray[i - 1].Checked == false) continue;       //曲线为非选中状态，不绘制
                    if (myLabelColor[i - 1].Text == string.Empty) continue;   //曲线为非选中状态，不绘制
                    if (i >= MyDefine.myXET.meCalTypeList.Count) continue;    //不明原因，有时候会出现超出meCalTypeList索引范围异常

                    myDeviceType = MyDefine.myXET.meCalTypeList[i];   //产品类型
                    max = MyDefine.myXET.meTblCurve.GetColumnMaxVal(i);
                    min = MyDefine.myXET.meTblCurve.GetColumnMinVal(i);

                    switch (myDeviceType)
                    {
                        case "TT_T":    //温度采集器
                            //MessageBox.Show("1");
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
                            //MessageBox.Show("2");
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

                        case "TQ_H":    //温湿度采集器
                            //MessageBox.Show("3");
                            drawHumCurve = true;           //允许绘制湿度曲线
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

                        case "TQ_T":    //温湿度采集器
                            //MessageBox.Show("2");
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
                            //MessageBox.Show("3");
                            drawHumCurve = true;           //允许绘制湿度曲线
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

                        default:
                            break;
                    }
                }

                if (maxTemp != double.MinValue)
                {
                    //计算温度坐标范围           
                    getCurveRange(ref minTemp, ref maxTemp);
                    //MessageBox.Show("2:" + maxTemp.ToString() + " " + minTemp.ToString());

                    this.tempSpan = maxTemp - minTemp;                              //温度跨度
                    this.meTmpMin = minTemp;                                        //温度最小值
                }

                if (maxHum != double.MinValue)
                {
                    //计算湿度坐标范围            
                    getCurveRange(ref minHum, ref maxHum);
                    //MessageBox.Show("3:" + maxHum.ToString() + " " + minHum.ToString());

                    this.humSpan = maxHum - minHum;                                //湿度跨度
                    this.meHumMin = minHum;                                          //湿度最小值
                }

                if (maxPsr != double.MinValue)
                {
                    //计算压力坐标范围      
                    getCurveRange(ref minPsr, ref maxPsr);
                    //MessageBox.Show("4:" + maxPsr.ToString() + " " + minPsr.ToString());

                    this.psrSpan = maxPsr - minPsr;                               //压力跨度
                    this.mePrsMin = minPsr;                                         //压力最小值
                }

                if (maxPsr != double.MinValue)     //存在压力数据时，不画湿度曲线
                {
                    //MessageBox.Show("4:");
                    drawHumCurve = false;
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
                if (MyDefine.myXET.meDataTbl == null) return;           //数据列表为空
                if (MyDefine.myXET.meTblCurve == null) return;          //曲线数据列表为空

                this.tempSpan = Double.MaxValue;
                this.humSpan = Double.MaxValue;
                this.psrSpan = Double.MaxValue;

                Double max, min;
                String myDeviceType;

                //for (int i = 1; i < meDataTbl.dataTable.Columns.Count; i++)
                {
                    myDeviceType = MyDefine.myXET.meTypeList[1];   //产品类型
                    max = MyDefine.myXET.meTblCurve.GetColumnMaxVal(idx);
                    min = MyDefine.myXET.meTblCurve.GetColumnMinVal(idx);
                    getCurveRange(ref min, ref max);

                    switch (myDeviceType)
                    {
                        case "TT_T":    //温度采集器
                            this.tempSpan = max - min;                                  //温度跨度
                            this.meTmpMin = min;                                         //温度最小值
                            break;

                        case "TH_T":    //温湿度采集器
                            this.tempSpan = max - min;                              //温度跨度
                            this.meTmpMin = min;                                     //温度最小值
                            break;

                        case "TH_H":    //温湿度采集器
                            this.humSpan = max - min;                               //湿度跨度
                            this.meHumMin = min;                                    //湿度最小值
                            drawHumCurve = true;                                    //允许绘制温度曲线
                            break;

                        case "TQ_T":    //温湿度采集器
                            this.tempSpan = max - min;                              //温度跨度
                            this.meTmpMin = min;                                     //温度最小值
                            break;

                        case "TQ_H":    //温湿度采集器
                            this.humSpan = max - min;                               //湿度跨度
                            this.meHumMin = min;                                    //湿度最小值
                            drawHumCurve = true;                                    //允许绘制温度曲线
                            break;

                        case "TP_P":    //压力采集器
                            this.psrSpan = max - min;                                  //压力跨度
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

        #region 计算曲线坐标范围

        //计算曲线坐标范围
        public void getCurveRange(ref Double minTemp, ref Double maxTemp)
        {
            //取较大绝对值的1/10作为坐标余量
            //int offsetY = Math.Abs(maxTemp) > Math.Abs(minTemp) ? (int)(Math.Abs(maxTemp) * 0.1) : (int)(Math.Abs(minTemp) * 0.1);
            Double offsetY = Math.Abs(maxTemp - minTemp) * 0.1;
            maxTemp += offsetY;
            minTemp -= offsetY;

            if (maxTemp - minTemp < 10)                                                 //Y轴跨度为<10,则调整为跨度=10
            {
                offsetY = (10 - (maxTemp - minTemp)) / 2;
                maxTemp = maxTemp + offsetY;
                minTemp = minTemp - offsetY;
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
                if (MyDefine.myXET.meTblCurve == null) return;    //数据表为空

                //清空曲线名称列表
                ClearCurveNameList();

                //添加曲线名称列表
                for (int i = 1; i < MyDefine.myXET.meTblCurve.dataTable.Columns.Count; i++)
                {
                    //if (i > 20) return;             //最多显示20条曲线

                    if (i > 20)
                    {
                        myLabelArray.Add(new Label());
                        myLabelColor.Add(new Label());
                    }
                    string curveName = MyDefine.myXET.meTblCurve.dataTable.Columns[i].ColumnName;      //温度：HTTxx_n；温湿度：HTHxx_n(温度)，HTH_Hxx_n(湿度)；压力：HTPxx_n
                    comboBox1.Items.Add(curveName);                                                    //xx_n,xx_n,xx_n
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

        #endregion

    }
}

//end


