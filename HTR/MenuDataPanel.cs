using HZH_Controls;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace HTR
{
    public partial class MenuDataPanel : UserControl
    {
        public MenuDataPanel()
        {
            InitializeComponent();

            //设置窗体的双缓冲
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            //利用反射设置DataGridView的双缓冲
            Type dgvType = this.dataGridView1.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(this.dataGridView1, true, null);
        }

        #region 界面加载

        private void MenuDataPanel_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
        }

        //界面关闭(被移除出panel)
        private void MenuDataPanel_ParentChanged(object sender, EventArgs e)
        {
            if (this.Parent == null)
            {
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

        public void AddMyUpdateEvent()
        {
            if (MyDefine.myXET.meDataTbl == null)
            {
                ShowBlankDataTable();           //生成并显示空的数据表
            }

            //核对权限(不要弹框)
            button1.Visible = MyDefine.myXET.CheckPermission(STEP.数据载入, false) ? true : false;
            button3.Visible = MyDefine.myXET.CheckPermission(STEP.数据导出, false) ? true : false;
            button2.Visible = MyDefine.myXET.CheckPermission(STEP.数据曲线, false) ? true : false;
        }

        #endregion

        #region 界面按钮事件

        #region 数据加载按钮 -- 加载单个或多个文件数据

        private void button1_Click(object sender, EventArgs e)
        {
            MyDefine.myXET.AddTraceInfo("数据加载");
            MenuDataImport dataImport = new MenuDataImport();
            dataImport.ShowDialog();
            dataImport.Location = new Point(this.Location.X + this.Width / 2, this.Location.Y + this.Height / 2);


            if (dataImport.ret)                                            //文件加载成功
            {
                MyDefine.myXET.meDataCurveUpdating = true;      //新的数据表已加载，数据曲线信息需更新
                MyDefine.myXET.meCalCurveUpdating = true;       //新的数据表已加载，需要更新校准、标定曲线信息
                MyDefine.myXET.meCalTableUpdating = true;       //重新加载了文件, 需要更新校准、标定表

                //更新有效数据开始结束时间
                int rowNum = MyDefine.myXET.meDataTbl.dataTable.Rows.Count - 1;
                DateTime startTime = Convert.ToDateTime(MyDefine.myXET.meDataTbl.GetCellValue(0, 0));         //整条曲线的开始时间
                DateTime stopTime = Convert.ToDateTime(MyDefine.myXET.meDataTbl.GetCellValue(rowNum, 0));     //整条曲线的结束时间
                //MyDefine.myXET.meValidStartIdx = 0;                                                           //默认有效数据开始索引为0
                //MyDefine.myXET.meValidStopIdx = rowNum;                                                       //默认有效数据结束索引为数据行最大值
                //MyDefine.myXET.meValidStartTime = startTime;                                                  //默认有效数据开始时间为测试开始时间
                //MyDefine.myXET.meValidStopTime = stopTime;                                                    //默认有效数据结束时间为测试结束时间
                MyDefine.myXET.meStartIdx = 0;                                                                //记录总数据开始索引
                MyDefine.myXET.meStopIdx = rowNum;                                                            //记录总数据结束索引
                MyDefine.myXET.meStartTime = startTime;                                                       //记录总数据开始时间
                MyDefine.myXET.meStopTime = stopTime;                                                         //记录总数据结束时间
                MyDefine.myXET.meSpanTime = Convert.ToInt32(MyDefine.myXET.homspan);                          //间隔时间(秒)
                MyDefine.myXET.meValidStageNum = 1;

                //复位有效数据索引
                MyDefine.myXET.meActivePn = 0;
                MyDefine.myXET.meValidIdxList.Clear();
                MyDefine.myXET.meValidTimeList.Clear();
                MyDefine.myXET.meValidNameList.Clear();
                MyDefine.myXET.meValidSetValueList.Clear();
                MyDefine.myXET.meValidUpperList.Clear();
                MyDefine.myXET.meValidLowerList.Clear();
                MyDefine.myXET.meValidIdxList.Add(0);
                MyDefine.myXET.meValidIdxList.Add(rowNum);
                MyDefine.myXET.meValidTimeList.Add(MyDefine.myXET.meStartTime);
                MyDefine.myXET.meValidTimeList.Add(MyDefine.myXET.meStopTime);
                MyDefine.myXET.meValidNameList.Add("有效数据");
                MyDefine.myXET.meValidSetValueList.Add(double.MinValue);
                MyDefine.myXET.meValidSetValueList.Add(double.MinValue);
                MyDefine.myXET.meValidUpperList.Add(double.MinValue);
                MyDefine.myXET.meValidUpperList.Add(double.MinValue);
                MyDefine.myXET.meValidLowerList.Add(double.MinValue);
                MyDefine.myXET.meValidLowerList.Add(double.MinValue);

                comboBox1.SelectedIndex = 0;
                ShowTheTable(MyDefine.myXET.meDataAllTbl);      //在当前界面显示加载的数据表

                updateFullPeriodMaxMin();        //载入数据时，需要获取全段数据的上下限
                保存阶段ToolStripMenuItem_Click(sender, e);

                ShowValidRowText();                                   //如果有效开始、结束均设置完毕，则在每行前面显示"有效数据"
            }
            else                                 //文件加载失败
            {

            }
            dataGridView1.Focus();      //将焦点从button上移走，使button每次单击都有点击效果
        }

        #endregion

        #region 数据导出按钮 -- 将当前界面表格导出为csv文件

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                dataGridView1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

                #region 判断数据源

                string type = "";
                DataTable mytable = new DataTable();
                if (comboBox1.SelectedIndex == 0 && MyDefine.myXET.meDataAllTbl != null)
                {
                    type = "原始数据";
                    mytable = MyDefine.myXET.meDataAllTbl.dataTable;
                }
                if (comboBox1.SelectedIndex == 1 && MyDefine.myXET.meDataTmpTbl != null)
                {
                    type = "原始温度数据";
                    mytable = MyDefine.myXET.meDataTmpTbl.dataTable;
                }
                if (comboBox1.SelectedIndex == 2 && MyDefine.myXET.meDataHumTbl != null)
                {
                    type = "原始湿度数据";
                    mytable = MyDefine.myXET.meDataHumTbl.dataTable;
                }
                if (comboBox1.SelectedIndex == 3 && MyDefine.myXET.meDataPrsTbl != null)
                {
                    type = "原始压力数据";
                    mytable = MyDefine.myXET.meDataPrsTbl.dataTable;
                }

                if (type == "")
                {
                    MyDefine.myXET.ShowWrongMsg("数据表尚未加载！");
                    return;
                }

                #endregion

                #region 数据导出

                System.Windows.Forms.SaveFileDialog DialogSave = new System.Windows.Forms.SaveFileDialog();
                DialogSave.Filter = "Excel(*.xls)|*.xls|所有文件(*.*)|*.*";
                DialogSave.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);     //默认桌面
                DialogSave.FileName = type;
                string selectedFolderPath = "";

                if (DialogSave.ShowDialog() == DialogResult.OK)
                {
                    MyDefine.myXET.AddTraceInfo("数据导出");

                    //导出到CSV文件
                    XET.ExportToExcel(mytable, DialogSave.FileName);

                    selectedFolderPath = Path.GetDirectoryName(DialogSave.FileName);
                } 
                else
                {
                    return;
                }

                //打开保存路径目录
                Process.Start(selectedFolderPath.Replace(type, ""));

                #endregion

            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("导出失败：" + ex.ToString());
            }
        }

        #endregion

        #region 生成曲线按钮 -- 切换到数据曲线界面(跨窗体控制)

        /// <summary>
        /// 切换到数据曲线界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            dataGridView1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            MyDefine.myXET.switchMainPanel(1);                      //切换到数据曲线界面
        }

        #endregion

        #region 切换数据表(全部/温度/湿度/压力)

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowSelectedTable();            //显示被选中的表格
            HighLightValidRows();           //高亮显示有效开始、结束行
        }

        //显示被选中的表格
        public void ShowSelectedTable()
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    //ShowDataTable();
                    MyDefine.myXET.AddTraceInfo("切换数据表为：全部");
                    ShowTheTable(MyDefine.myXET.meDataAllTbl);
                    MyDefine.myXET.meJSNList = MyDefine.myXET.meJSNListAll;
                    MyDefine.myXET.meDataTbl = MyDefine.myXET.meDataTblAll;
                    MyDefine.myXET.meTypeList = MyDefine.myXET.meAllList;
                    MyDefine.myXET.meInfoTbl = MyDefine.myXET.meInfoAllTbl;
                    MyDefine.myXET.meTMPNum = MyDefine.myXET.meTmpList.Count - 1;
                    MyDefine.myXET.meHUMNum = MyDefine.myXET.meHumList.Count - 1;
                    MyDefine.myXET.mePRSNum = MyDefine.myXET.mePrsList.Count - 1;
                    break;
                case 1:
                    //ShowDataTmpTable();
                    MyDefine.myXET.AddTraceInfo("切换数据表为：温度");
                    ShowTheTable(MyDefine.myXET.meDataTmpTbl);
                    MyDefine.myXET.meJSNList = MyDefine.myXET.meJSNListPart;
                    MyDefine.myXET.meDataTbl = MyDefine.myXET.meDataTblTmp;
                    MyDefine.myXET.meTypeList = MyDefine.myXET.meTmpList;
                    MyDefine.myXET.meInfoTbl = MyDefine.myXET.meInfoTmpTbl;
                    MyDefine.myXET.meTMPNum = MyDefine.myXET.meTmpList.Count - 1;
                    MyDefine.myXET.meHUMNum = 0;
                    MyDefine.myXET.mePRSNum = 0;
                    break;
                case 2:
                    //ShowDataHumTable();
                    MyDefine.myXET.AddTraceInfo("切换数据表为：湿度");
                    ShowTheTable(MyDefine.myXET.meDataHumTbl);
                    MyDefine.myXET.meJSNList = MyDefine.myXET.meJSNListPart;
                    MyDefine.myXET.meDataTbl = MyDefine.myXET.meDataTblHum;
                    MyDefine.myXET.meTypeList = MyDefine.myXET.meHumList;
                    MyDefine.myXET.meInfoTbl = MyDefine.myXET.meInfoHumTbl;
                    MyDefine.myXET.meTMPNum = 0;
                    MyDefine.myXET.meHUMNum = MyDefine.myXET.meHumList.Count - 1;
                    MyDefine.myXET.mePRSNum = 0;
                    break;
                case 3:
                    //ShowDataPrsTable();
                    MyDefine.myXET.AddTraceInfo("切换数据表为：压力");
                    ShowTheTable(MyDefine.myXET.meDataPrsTbl);
                    MyDefine.myXET.meJSNList = MyDefine.myXET.meJSNListPart;
                    MyDefine.myXET.meDataTbl = MyDefine.myXET.meDataTblPrs;
                    MyDefine.myXET.meTypeList = MyDefine.myXET.mePrsList;
                    MyDefine.myXET.meInfoTbl = MyDefine.myXET.meInfoPrsTbl;
                    MyDefine.myXET.meTMPNum = 0;
                    MyDefine.myXET.meHUMNum = 0;
                    MyDefine.myXET.mePRSNum = MyDefine.myXET.mePrsList.Count - 1;
                    break;
            }
        }

        #endregion

        #endregion

        #region 显示数据表(显示在界面)

        #region 验证数据表初始化(显示空白表)

        //dataGridView加载
        public void InitDataGridView()
        {
            try
            {
                //设置表格为只读
                dataGridView(dataGridView1);    //dataGridView初始化必须放在添加列之前，行高设置才能起作用
                ShowBlankDataTable();           //尚未加载文件数据时，显示空的数据表
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }
        #endregion

        #region 生成并显示空白表

        /// <summary>
        /// 尚未加载文件数据时，生成显示空的数据表
        /// </summary>
        public void ShowBlankDataTable()
        {

            //添加列
            dataTableClass myDataTable = new dataTableClass();
            myDataTable.addTableColumn("序号");                   //第0栏
            myDataTable.addTableColumn("设备1");                  //第1栏
            myDataTable.addTableColumn("设备2");                  //第2栏
            myDataTable.addTableColumn("设备3");                  //第3栏
            myDataTable.addTableColumn("设备4");                  //第4栏
            myDataTable.addTableColumn("设备5");                  //第5栏


            //添加20个空行
            for (int i = 0; i < 20; i++)
            {
                myDataTable.AddTableRow();
            }

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = myDataTable.dataTable;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;       //自动填满整个dataGridView宽度
            dataGridView1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;            //垂直滚动条
            dataGridView1.ClearSelection(); //清除单元格选中状态(放在添加列后才起作用)

            //禁止列排序
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            dataGridView1.ClearSelection(); //清除单元格选中状态(放在添加列后才起作用)
        }

        #endregion

        #region 数据表显示

        public void ShowTheTable(dataTableClass mytable)
        {
            //数据表为空，显示空白表
            if (mytable == null)
            {
                ShowBlankDataTable();
                return;
            }

            //行数不满20则添加空白行
            int rowcount = mytable.dataTable.Rows.Count;
            if (rowcount < 20)
            {
                //添加空行
                for (int i = rowcount; i < 20; i++)
                {
                    mytable.AddTableRow();
                }
            }

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = mytable.dataTable;
            dataGridView1.ScrollBars = System.Windows.Forms.ScrollBars.Both;                //水平、垂直滚动条
            dataGridView1.ClearSelection(); //清除单元格选中状态(放在添加列后才起作用)

            //禁止列排序、设置最小列宽、自动填满整个dataGridView宽度(超出则显示滚动条)
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                dataGridView1.Columns[i].MinimumWidth = 120;                                    //设置最小列宽
                dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;     //禁止列排序
                dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;    //自动填满整个dataGridView宽度，超出则显示滚动条
            }

            /*
            //列数过多，则设置列宽并显示横向滚动条
            if (dataGridView1.Columns.Count > 6)
            {
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;   //不自动填满整个dataGridView宽度
                dataGridView1.ScrollBars = System.Windows.Forms.ScrollBars.Both;            //水平、垂直滚动条

                //设置列宽
                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    dataGridView1.Columns[i].Width = 120;
                }
            }
            */
        }

        #endregion

        #endregion

        #region dataGridView操作

        #region dataGridView函数集

        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();   //标题栏格式
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();   //所有行格式
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();   //偶数行格式
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();   //失效行格式

        private void dataGridView(DataGridView dataGridView)
        {
            //System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();   //标题栏格式
            //System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();   //所有行格式
            //System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();   //偶数行格式

            //标题栏设置
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;  //标题栏文字中心对齐
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(68, 114, 196);       //背景色
            dataGridViewCellStyle1.Font = new System.Drawing.Font("宋体", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.WhiteSmoke;                   //文字颜色
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;    //禁止改变列高度
            dataGridView.ColumnHeadersHeight = 60;  //设置列高

            //行设置
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(207, 213, 234);    //行背景色
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;  //行文字中心对齐
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Black;                 //文字颜色
            dataGridViewCellStyle2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridView.RowsDefaultCellStyle = dataGridViewCellStyle2;

            //偶数行设置
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(233, 235, 245);
            //dataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle3;

            //失效行设置
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.LightGray;

            //总体设置
            dataGridView.BackgroundColor = System.Drawing.Color.White;     //总体背景色
            dataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;       //无边框
            dataGridView.AllowUserToAddRows = false;        //禁止用户添加行
            dataGridView.AllowUserToDeleteRows = false;     //禁止用户删除行
            dataGridView.AllowUserToResizeColumns = false;  //禁止用户改变列宽
            dataGridView.AllowUserToResizeRows = false;     //禁止用户改变行高
            dataGridView.AllowUserToOrderColumns = false;
            dataGridView.EnableHeadersVisualStyles = false;
            dataGridView.GridColor = System.Drawing.Color.WhiteSmoke;  //网格线颜色
            this.dataGridView1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;   //垂直滚动条

            dataGridView.ReadOnly = true;     //只读
            dataGridView.RowTemplate.Height = 32;       //设置行高
            dataGridView.RowHeadersVisible = false;     //加入此行后，行高就一直可以调整？？？
            //dataGridView.Height = dataGridView.RowTemplate.Height * dataGridView.Rows.Count + dataGridView.ColumnHeadersHeight;      //设置dataGridView总高度
            dataGridView.MultiSelect = false;       //只能选中一个或一行单元格
            //dataGridView.RowTemplate.ReadOnly = true;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;   //单击选中整行
            dataGridView.ClearSelection(); //清除单元格选中状态

            //3D网格
            //dataGridView.AdvancedColumnHeadersBorderStyle.All = DataGridViewAdvancedCellBorderStyle.InsetDouble;
            //dataGridView.AdvancedCellBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.InsetDouble;
            //dataGridView.AdvancedCellBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.Inset;
            //dataGridView.AdvancedCellBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Inset;
            //dataGridView.AdvancedCellBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.InsetDouble;

            dataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;   //自动填满整个dataGridView宽度
            //dataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;   //行高自适应行文字（使能后，最大化时行高不会按比例变化）

            //dataGridView.EditMode = DataGridViewEditMode.EditOnEnter;   //单击即可编辑(如果不设置此项，要双击才出现光标可编辑，设置后单击单元格即出现光标)

        }

        //添加一栏数据
        private void AddDataGridViewColumn(DataGridView myDGV, string colName, string colText, Boolean colReadOnly)
        {
            DataGridViewColumn dgvColumn = new DataGridViewTextBoxColumn();

            dgvColumn.Name = colName;
            dgvColumn.HeaderText = colText;
            dgvColumn.ReadOnly = colReadOnly;
            dgvColumn.SortMode = DataGridViewColumnSortMode.NotSortable;    //禁止排序
            //dgvColumn.ValueType = typeof(int);
            myDGV.Columns.AddRange(dgvColumn);
        }

        //添加一栏数据
        private void AddDataGridViewColumn(DataGridView myDGV, string colName, string colText, int colWidth, Boolean colReadOnly)
        {
            DataGridViewColumn dgvColumn = new DataGridViewTextBoxColumn();

            dgvColumn.Name = colName;
            dgvColumn.HeaderText = colText;
            dgvColumn.Width = colWidth;
            dgvColumn.ReadOnly = colReadOnly;
            dgvColumn.SortMode = DataGridViewColumnSortMode.NotSortable;    //禁止排序
            //dgvColumn.ValueType = typeof(int);
            myDGV.Columns.AddRange(dgvColumn);
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

                    if (con is Panel == false && newY != 0 && con is DataGridView == false)//Panel容器控件不改变字体--Panel字体变后，若panel调用了UserControl控件，则UserControl及其上控件的尺寸会出现不可控变化;newY=0时，字体设置会报错
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

        #region 设置有效数据

        public int selectedRowIdx = -1;
        public DateTime selectedTime = DateTime.MinValue;
        public Boolean validStartSeclected = false;
        public Boolean validStopSeclected = false;

        #region 右键显示下拉菜单

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            bool isSelect = false;
            //选中单元格
            selectedRowIdx = dataGridView1.CurrentRow.Index;

            //选中单元格大于数据行
            if (MyDefine.myXET.meDataTbl == null || selectedRowIdx >= MyDefine.myXET.meDataTbl.Count)
            {
                return;
            }

            selectedTime = Convert.ToDateTime(MyDefine.myXET.meDataTbl.GetCellValue(selectedRowIdx, 0));

            for (int i = 0; i < MyDefine.myXET.meValidIdxList.Count; i += 2)
            {
                if (selectedRowIdx > MyDefine.myXET.meValidIdxList[i])
                {
                    if (selectedRowIdx < MyDefine.myXET.meValidIdxList[i + 1])
                    {
                        MyDefine.myXET.meActivePn = i / 2;

                        //添加阶段启用
                        if (MyDefine.myXET.meValidNameList.Count != MyDefine.myXET.meActivePn + 1)
                        {
                            addStageToolStripMenuItem.Visible = false;
                        }
                        isSelect = true;
                    }
                }
            }

            if (!isSelect)
            {
                MyDefine.myXET.meActivePn = MyDefine.myXET.meValidNameList.Count - 1;
            }

            if (e.Button == MouseButtons.Right)
            {
                if (MyDefine.myXET.meDataTbl == null) return;                      //数据列表为空
                if (dataGridView1.SelectedCells.Count == 0) return;                //未选中单元格

                //初始化阶段名称文本框
                if (MyDefine.myXET.meValidNameList[MyDefine.myXET.meActivePn] == "有效数据" || ToolStripTb_setName.Text == "")
                {
                    ToolStripTb_setName.Text = "阶段名称";
                    ToolStripTb_setName.ForeColor = Color.LightGray;
                }
                else
                {
                    ToolStripTb_setName.Text = MyDefine.myXET.meValidNameList[MyDefine.myXET.meActivePn];
                    ToolStripTb_setName.ForeColor = Color.Black;
                }

                //在鼠标右击的位置显示下拉菜单(下拉菜单位置是相对全屏的)
                contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);           //注意MousePosition.X&Y为鼠标的屏幕坐标
                UnlockValidStageList();                                             //根据当前有效列表解锁必要的阶段菜单
            }
        }

        #endregion

        #region 添加新阶段
        private void 添加阶段ToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
            }
            else
            {
                --MyDefine.myXET.meActivePn;
            }

            addStageToolStripMenuItem.Visible = false;
        }
        #endregion

        #region 设置阶段名称

        //点击回车键隐藏输入框
        private void tb_setName_KeyUp(object sender, KeyEventArgs e)
        {
            //设置阶段名称
            int index = MyDefine.myXET.meActivePn;
            if (ToolStripTb_setName.Text != "" && ToolStripTb_setName.Text != null)
            {
                MyDefine.myXET.meValidNameList[index] = ToolStripTb_setName.Text;

                //同步更新数据处理界面有效开始、结束行的高亮状态
                MyDefine.myXET.switchMainPanel(23);
            }
        }

        //阶段名称
        private void tb_setName_Click(object sender, EventArgs e)
        {
            ToolStripTb_setName.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            int index = MyDefine.myXET.meActivePn;
            if (MyDefine.myXET.meValidNameList[index] == "有效数据" || ToolStripTb_setName.Text == "")
            {
                ToolStripTb_setName.Text = "";
                ToolStripTb_setName.ForeColor = Color.Black;
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
            dataGridView1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            if (this.Name == "验证曲线") return;                         //验证曲线禁止手动设置开始、结束时间

            if (MyDefine.myXET.meDataTbl == null)                        //数据列表为空
            {
                MessageBox.Show("尚未加载数据，设置无效！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //计算当前设置点在数据表中的索引
            Int32 listIdx = MyDefine.myXET.meActivePn * 2;                            //当前开始、结束索引分别为listIdx、listIdx+1
            Int32 dataNum = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;            //数据总个
            Int32 startIndex = selectedRowIdx;                                        //有效开始索引(当前鼠标所在位置)
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
            MyDefine.myXET.AddTraceInfo("设置起点P" + MyDefine.myXET.meActivePn);
            if (MyDefine.myXET.meValidIdxList[listIdx] != -1) dataGridView1.Rows[MyDefine.myXET.meValidIdxList[listIdx]].DefaultCellStyle.BackColor = Color.FromArgb(207, 213, 234);   //清除之前有效开始行的背景色
            MyDefine.myXET.meValidIdxList[listIdx] = startIndex;         //记录阶段n有效数据开始索引
            MyDefine.myXET.meValidTimeList[listIdx] = startTime;         //记录阶段n有效数据开始时间
            dataGridView1.ClearSelection();

            ShowValidRowText();                                   //如果有效开始、结束均设置完毕，则在每行前面显示"有效数据"
            UnlockValidStageList();                               //根据有效列表解锁必要的阶段列表Pn

            //更新区域最高最低值
            if (stopIndex != -1)
            {
                updateMaxMin();

                //自动保存阶段
                保存阶段ToolStripMenuItem_Click(sender, e);
            }
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
            dataGridView1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            if (this.Name == "验证曲线") return;                         //验证曲线禁止手动设置开始、结束时间

            if (MyDefine.myXET.meDataTbl == null)                        //数据列表为空
            {
                MessageBox.Show("尚未加载数据，设置无效！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //计算当前设置点在数据表中的索引
            Int32 listIdx = MyDefine.myXET.meActivePn * 2;                            //当前开始、结束索引分别为listIdx、listIdx+1
            Int32 dataNum = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;
            Int32 startIndex = MyDefine.myXET.meValidIdxList[listIdx];                //有效开始索引
            Int32 stopIndex = selectedRowIdx;    //有效结束索引(当前鼠标所在位置)
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
            MyDefine.myXET.AddTraceInfo("设置终点P" + MyDefine.myXET.meActivePn);
            if (MyDefine.myXET.meValidIdxList[listIdx + 1] != -1) dataGridView1.Rows[MyDefine.myXET.meValidIdxList[listIdx + 1]].DefaultCellStyle.BackColor = Color.FromArgb(207, 213, 234);   //清除之前有效开始行的背景色
            MyDefine.myXET.meValidIdxList[listIdx + 1] = stopIndex;      //记录阶段n有效数据结束索引
            MyDefine.myXET.meValidTimeList[listIdx + 1] = stopTime;      //记录阶段n有效数据结束时间
            dataGridView1.ClearSelection();

            ShowValidRowText();                                   //如果有效开始、结束均设置完毕，则在每行前面显示"有效数据"
            UnlockValidStageList();                               //根据有效列表解锁必要的阶段列表Pn

            //更新区域最高最低值
            if (startIndex != -1)
            {
                updateMaxMin();

                //自动保存阶段
                保存阶段ToolStripMenuItem_Click(sender, e);
            }
        }

        //更新区域最高最低值
        public void updateMaxMin()
        {
            #region 变量定义

            Double myVal = 0;                        //测试值
            String myType = "";                      //数据类型
            int index = MyDefine.myXET.meActivePn;  //当前正在编辑的阶段Pn
            Boolean isPsr = false;                  //是否存在压力数据

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

            //初始化数据
            MyDefine.myXET.drawTemCurve = false;     //是否画温度曲线
            MyDefine.myXET.drawHumCurve = false;     //是否画湿度曲线
            MyDefine.myXET.drawPrsCurve = false;    //是否画压力曲线

            for (int i = MyDefine.myXET.meValidIdxList[2 * index]; i <= MyDefine.myXET.meValidIdxList[2 * index + 1]; i++)
            {
                //遍历每一列，计算本行的温度/湿度/压力的最大最小值
                for (int j = 1; j < MyDefine.myXET.meDataTbl.dataTable.Columns.Count; j++)        //meDataTbl第0列为Time
                {
                    if (MyDefine.myXET.meDataTbl.GetCellValue(i, j) == "") continue;       //空数据
                    myType = MyDefine.myXET.meTypeList[j];                                     //数据类型
                    myVal = Convert.ToDouble(MyDefine.myXET.meDataTbl.GetCellValue(i, j)); //测试值

                    //计算最大最小值
                    switch (myType)
                    {
                        case "TT_T":
                        case "TH_T":
                        case "TQ_T":
                            MyDefine.myXET.drawTemCurve = true;
                            if (myTMPMax < myVal) { myTMPMax = myVal; }
                            if (myTMPMin > myVal) { myTMPMin = myVal; }
                            break;

                        case "TH_H":
                        case "TQ_H":
                            MyDefine.myXET.drawHumCurve = true;
                            if (myHUMMax < myVal) { myHUMMax = myVal; }
                            if (myHUMMin > myVal) { myHUMMin = myVal; }
                            break;

                        case "TP_P":
                            isPsr = true;
                            MyDefine.myXET.drawPrsCurve = true;
                            if (myPRSMax < myVal) { myPRSMax = myVal; }
                            if (myPRSMin > myVal) { myPRSMin = myVal; }
                            break;
                    }
                }
            }
            #endregion

            #region 阶段数据最大最小值
            if (myTMPMax != double.MinValue)//温度
            {
                MyDefine.myXET.meLeftMaxMinList[2 * index] = myTMPMax;
                MyDefine.myXET.meLeftMaxMinList[2 * index + 1] = myTMPMin;
            }
            if (isPsr)  //压力
            {
                MyDefine.myXET.meRightMaxMinList[2 * index] = myPRSMax;
                MyDefine.myXET.meRightMaxMinList[2 * index + 1] = myPRSMin;
            }
            else    //湿度
            {
                MyDefine.myXET.meRightMaxMinList[2 * index] = myHUMMax;
                MyDefine.myXET.meRightMaxMinList[2 * index + 1] = myHUMMin;
            }
            #endregion
        }

        //更新全段和区域最高最低值
        public void updateFullPeriodMaxMin()
        {
            #region 变量定义

            Double myVal = 0;                        //测试值
            String myType = "";                      //数据类型
            int index = MyDefine.myXET.meActivePn;  //当前正在编辑的阶段Pn
            Boolean isPsr = false;                  //是否存在压力数据

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

            //初始化数据
            MyDefine.myXET.drawTemCurve = false;     //是否画温度曲线
            MyDefine.myXET.drawHumCurve = false;     //是否画湿度曲线
            MyDefine.myXET.drawPrsCurve = false;    //是否画压力曲线

            for (int i = MyDefine.myXET.meValidIdxList[2 * index]; i <= MyDefine.myXET.meValidIdxList[2 * index + 1]; i++)
            {
                //遍历每一列，计算本行的温度/湿度/压力的最大最小值
                for (int j = 1; j < MyDefine.myXET.meDataTbl.dataTable.Columns.Count; j++)        //meDataTbl第0列为Time
                {
                    if (MyDefine.myXET.meDataTbl.GetCellValue(i, j) == "") continue;       //空数据
                    myType = MyDefine.myXET.meTypeList[j];                                     //数据类型
                    myVal = Convert.ToDouble(MyDefine.myXET.meDataTbl.GetCellValue(i, j)); //测试值

                    //计算最大最小值
                    switch (myType)
                    {
                        case "TT_T":
                        case "TH_T":
                        case "TQ_T":
                            MyDefine.myXET.drawTemCurve = true;
                            if (myTMPMax < myVal) { myTMPMax = myVal; }
                            if (myTMPMin > myVal) { myTMPMin = myVal; }
                            break;

                        case "TH_H":
                        case "TQ_H":
                            MyDefine.myXET.drawHumCurve = true;
                            if (myHUMMax < myVal) { myHUMMax = myVal; }
                            if (myHUMMin > myVal) { myHUMMin = myVal; }
                            break;

                        case "TP_P":
                            isPsr = true;
                            MyDefine.myXET.drawPrsCurve = true;
                            if (myPRSMax < myVal) { myPRSMax = myVal; }
                            if (myPRSMin > myVal) { myPRSMin = myVal; }
                            break;
                    }
                }
            }
            #endregion

            #region 阶段数据最大最小值
            if (myTMPMax != double.MinValue)//温度
            {
                MyDefine.myXET.meLeftMaxMinList[2 * index] = myTMPMax;
                MyDefine.myXET.meLeftMaxMinList[2 * index + 1] = myTMPMin;
                MyDefine.myXET.leftLimit[0] = Math.Round(myTMPMax + 5, 2);
                MyDefine.myXET.leftLimit[1] = Math.Round(myTMPMin - 5, 2);
            }
            if (isPsr)  //压力
            {
                MyDefine.myXET.meRightMaxMinList[2 * index] = myPRSMax;
                MyDefine.myXET.meRightMaxMinList[2 * index + 1] = myPRSMin;
                MyDefine.myXET.rightLimit[0] = Math.Round(myPRSMax + 5, 2);
                MyDefine.myXET.rightLimit[1] = Math.Round(myPRSMin - 5, 2);
            }
            else    //湿度
            {
                MyDefine.myXET.meRightMaxMinList[2 * index] = myHUMMax;
                MyDefine.myXET.meRightMaxMinList[2 * index + 1] = myHUMMin;
                MyDefine.myXET.rightLimit[0] = Math.Round(myHUMMax + 5, 2);
                MyDefine.myXET.rightLimit[1] = Math.Round(myHUMMin - 5, 2);
            }
            #endregion
        }

        #endregion

        #region 保存阶段设置
        private void 保存阶段ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            int index = 2 * MyDefine.myXET.meActivePn;

            #region 设置左右轴阶段性上下限、设定值
            if (MyDefine.myXET.meLeftMaxMinList[index] != Double.MinValue)
            {
                //添加阶段信息
                MyDefine.myXET.meValidSetValueList[index] = Math.Round((MyDefine.myXET.meLeftMaxMinList[index] + MyDefine.myXET.meLeftMaxMinList[index + 1]) / 2, 2);
                MyDefine.myXET.meValidUpperList[index] = Math.Round(MyDefine.myXET.meLeftMaxMinList[index] + 5, 2);
                MyDefine.myXET.meValidLowerList[index] = Math.Round(MyDefine.myXET.meLeftMaxMinList[index + 1] - 5, 2);
            }

            if (MyDefine.myXET.meRightMaxMinList[index] != Double.MinValue)
            {
                //添加阶段信息
                MyDefine.myXET.meValidSetValueList[index + 1] = Math.Round((MyDefine.myXET.meRightMaxMinList[index] + MyDefine.myXET.meRightMaxMinList[index + 1]) / 2, 2);
                MyDefine.myXET.meValidUpperList[index + 1] = Math.Round(MyDefine.myXET.meRightMaxMinList[index] + 5, 2);
                MyDefine.myXET.meValidLowerList[index + 1] = Math.Round(MyDefine.myXET.meRightMaxMinList[index + 1] - 5, 2);
            }

            #endregion

            //添加阶段启用
            if (MyDefine.myXET.meValidNameList.Count == MyDefine.myXET.meActivePn + 1)
            {
                addStageToolStripMenuItem.Visible = true;
            }
        }
        #endregion

        #region 删除阶段
        private void 删除阶段ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            int index = MyDefine.myXET.meActivePn;
            if (index < MyDefine.myXET.meValidStageNum)
            {
                if (MessageBox.Show("是否删除" + MyDefine.myXET.meValidNameList[index] + "阶段", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    #region 更新dataGridView1显示
                    dataGridView1.Rows[MyDefine.myXET.meValidIdxList[index * 2]].DefaultCellStyle.BackColor = Color.FromArgb(207, 213, 234);   //清除之前有效开始行的背景色
                    dataGridView1.Rows[MyDefine.myXET.meValidIdxList[index * 2 + 1]].DefaultCellStyle.BackColor = Color.FromArgb(207, 213, 234);   //清除之前有效开始行的背景色
                                                                                                                                                   //标记有效数据列
                    for (int j = MyDefine.myXET.meValidIdxList[index * 2]; j <= MyDefine.myXET.meValidIdxList[index * 2 + 1]; j++)
                    {
                        MyDefine.myXET.meDataAllTbl.SetCellValue(j, 0, "");//"P" + (i / 2 + 1).ToString()
                    }
                    #endregion

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
                    }
                    #endregion

                    #region 更新界面
                    MyDefine.myXET.meActivePn = --MyDefine.myXET.meActivePn < 0 ? 0 : MyDefine.myXET.meActivePn;

                    UnlockValidStageList();                               //根据有效列表解锁必要的阶段列表Pn
                    #endregion

                }
            }
        }
        #endregion

        #region 设置有效开始、结束行背景色

        //高亮显示有效开始、结束行
        public void HighLightValidRows()
        {
            for (int i = 0; i < MyDefine.myXET.meValidIdxList.Count; i++)
            {
                int validIdx = MyDefine.myXET.meValidIdxList[i];
                if (validIdx != -1) dataGridView1.Rows[validIdx].DefaultCellStyle.BackColor = Color.DarkKhaki;      //设置有效开始/结束行的背景色
            }
        }

        #endregion

        #region 设置有效开始、结束行文字

        //显示有效数据行文字
        public void ShowValidRowText()
        {
            try
            {
                if (MyDefine.myXET.meDataTbl == null) return;

                //清空所有"有效数据"文字
                MyDefine.myXET.meDataAllTbl.ClearTableColumn(0);
                MyDefine.myXET.meDataTmpTbl.ClearTableColumn(0);
                MyDefine.myXET.meDataHumTbl.ClearTableColumn(0);
                MyDefine.myXET.meDataPrsTbl.ClearTableColumn(0);

                for (int i = 0; i < MyDefine.myXET.meValidIdxList.Count; i = i + 2)         //
                {
                    int validIdx1 = MyDefine.myXET.meValidIdxList[i];
                    int validIdx2 = MyDefine.myXET.meValidIdxList[i + 1];
                    if (validIdx1 != -1 && validIdx2 != -1)
                    {
                        //标记有效数据列
                        for (int j = validIdx1; j <= validIdx2; j++)
                        {
                            MyDefine.myXET.meDataAllTbl.SetCellValue(j, 0, MyDefine.myXET.meValidNameList[i / 2]);//"P" + (i / 2 + 1).ToString()
                            MyDefine.myXET.meDataTmpTbl.SetCellValue(j, 0, MyDefine.myXET.meValidNameList[i / 2]);
                            MyDefine.myXET.meDataHumTbl.SetCellValue(j, 0, MyDefine.myXET.meValidNameList[i / 2]);
                            MyDefine.myXET.meDataPrsTbl.SetCellValue(j, 0, MyDefine.myXET.meValidNameList[i / 2]);
                        }
                    }

                    if (validIdx1 != -1) dataGridView1.Rows[validIdx1].DefaultCellStyle.BackColor = Color.DarkKhaki;      //设置有效开始行的背景色
                    if (validIdx2 != -1) dataGridView1.Rows[validIdx2].DefaultCellStyle.BackColor = Color.DarkKhaki;      //设置有效结束行的背景色
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("显示有效数据行文字失败：" + ex.ToString());
            }
        }

        #endregion

        #region 解锁有效阶段菜单列表

        //解锁必要的有效阶段(右键菜单)
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
        }

        #endregion

        #endregion

        /// <summary>
        /// 临时解决多设备（500）导入数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {
            e.Column.FillWeight = 10;
        }
    }
}
