using System;
using System.Data;
using System.Windows.Forms;

namespace HTR
{
    public partial class MenuTracePanel : UserControl
    {
        public MenuTracePanel()
        {
            InitializeComponent();
        }

        #region 界面加载

        private void MenuDataPanel_Load(object sender, EventArgs e)
        {
            dtpStartTime.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            dtpStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            dtpStartTime.ShowUpDown = true;
            
            dtpStopTime.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            dtpStopTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            dtpStopTime.ShowUpDown = true;

            comboBox1.SelectedIndex = 0;
        }

        public void AddMyUpdateEvent()
        {
            /*
            if (MyDefine.myXET.meTraceTbl == null)
            {
                ShowBlankTable();           //生成并显示空表
            }
            */

            MyDefine.myXET.SaveTraceRecords();      //保存审计追踪记录
        }

        #endregion

        #region 表格初始化

        #region DataGridView初始化(生成空白表)

        //dataGridView加载
        public void InitDataGridView()
        {
            try
            {
                //设置表格为只读
                dataGridView(dataGridView1);    //dataGridView初始化必须放在添加列之前，行高设置才能起作用
                GenerateBlankTable();           //报告列表初始化，显示空的数据表
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("数据表初始化失败：" + ex.ToString());
            }
        }
        #endregion

        #region 生成并显示空白报告列表

        /// <summary>
        /// 尚未加载文件数据时，生成显示空的数据表
        /// </summary>
        public void GenerateBlankTable()
        {
            //生成审计追踪总表的表头
            MyDefine.myXET.meTraceTbl = new dataTableClass();
            MyDefine.myXET.meTraceTbl.addTableColumn("日期");
            MyDefine.myXET.meTraceTbl.addTableColumn("用户名");
            MyDefine.myXET.meTraceTbl.addTableColumn("操作类型");
            MyDefine.myXET.meTraceTbl.addTableColumn("日志");
            MyDefine.myXET.meTraceTbl.addTableColumn("计算机名/MAC");

            //生成审计追踪列表(显示在界面)的表头并添加空白行
            MyDefine.myXET.meTraceShowTbl = new dataTableClass();
            MyDefine.myXET.meTraceShowTbl.dataTable = MyDefine.myXET.meTraceTbl.CopyTable();

            ShowTheTable(MyDefine.myXET.meTraceShowTbl);     //显示空的数据表
        }

        #endregion

        #region 数据表显示

        public void ShowTheTable(dataTableClass mytable)
        {
            //数据表为空，显示空白表
            if (mytable == null) return;

            #region 制作空白表格

            int rowcount = mytable.dataTable.Rows.Count;
            int rownum = (rowcount < 20) ? 20 - rowcount : 5;

            dataTableClass tmpTbl = new dataTableClass();
            tmpTbl.dataTable = mytable.CopyTable();
            tmpTbl.ClearTableData();

            //添加空行
            for (int i = 0; i < rownum; i++)
            {
                tmpTbl.AddTableRow();
            }

            #endregion

            #region 显示数据表

            dataTableClass myShowTbl = new dataTableClass();
            myShowTbl.dataTable.Merge(mytable.dataTable);
            myShowTbl.dataTable.Merge(tmpTbl.dataTable);    //合并空白表格

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = myShowTbl.dataTable;
            dataGridView1.ClearSelection(); //清除单元格选中状态(放在添加列后才起作用)

            #endregion

        }

        #endregion

        #endregion

        #region 界面按钮事件

        #region 筛选按钮 -- 筛选并显示符合条件的审计追踪项

        public DateTime time_start = DateTime.MinValue;    //开始日期时间
        public DateTime time_stop = DateTime.MinValue;     //结束日期时间

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                groupBox1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

                #region 生成满足筛选条件的数据表

                MyDefine.myXET.AddTraceInfo("筛选");
                MyDefine.myXET.meTraceShowTbl.ClearTableData(); //筛选之前清除所有行，只保留表头

                String user = textBox1.Text.Trim();        //用户名
                String type = textBox2.Text.Trim();        //类型
                String cont = textBox3.Text.Trim();        //日志
                String comp = textBox4.Text.Trim();        //计算机名
                ReloadTraceRecord();                       //将审计追踪记录加载到数据表meTraceTbl(日期被改变才加载)

                string mycell;
                for (int i = 0; i < MyDefine.myXET.meTraceTbl.dataTable.Rows.Count; i++)
                {
                    #region 筛选项

                    //如果选中了自定义日期，则要筛选具体时间的记录 -- 精准筛选；注，加载文件时已经模糊筛选过一次日期
                    if (radioButton2.Checked)
                    {
                        mycell = MyDefine.myXET.meTraceTbl.GetCellValue(i, 0);
                        DateTime time = DateTime.ParseExact(mycell, "yyyy-MM-dd HH:mm:ss", null);    //字符串转为日期
                        if (time.CompareTo(time_start) < 0 || time.CompareTo(time_stop) > 0) continue;    //记录时间超出筛选范围
                    }

                    //用户名，模糊筛查
                    if (user != "")
                    {
                        mycell = MyDefine.myXET.meTraceTbl.GetCellValue(i, 1);
                        if (mycell.Contains(user) == false) continue;
                    }

                    //类型，模糊筛查
                    if (type != "")
                    {
                        mycell = MyDefine.myXET.meTraceTbl.GetCellValue(i, 2);
                        if (mycell.Contains(type) == false) continue;
                    }

                    //日志，模糊筛查
                    if (cont != "")
                    {
                        mycell = MyDefine.myXET.meTraceTbl.GetCellValue(i, 3);
                        if (mycell.Contains(cont) == false) continue;
                    }

                    //计算机名，模糊筛查
                    if (comp != "")
                    {
                        mycell = MyDefine.myXET.meTraceTbl.GetCellValue(i, 4);
                        if (mycell.Contains(comp) == false) continue;
                    }

                    #endregion

                    MyDefine.myXET.meTraceShowTbl.AddTableRow(MyDefine.myXET.meTraceTbl.dataTable.Rows[i]);   //将此行信息添加进表格
                }

                #endregion

                ShowTheTable(MyDefine.myXET.meTraceShowTbl);          //显示数据表
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("筛选失败：" + ex.ToString());
            }
        }

        /// <summary>
        /// 选择日期变化后，重新加载审计追踪文件
        /// </summary>
        public void ReloadTraceRecord()
        {
            DateTime start = DateTime.Now;
            DateTime stop = DateTime.Now;

            //更新日期
            if (radioButton1.Checked)           //3天/1周/1个月/3个月
            {
                int days = 0;
                if (comboBox1.SelectedIndex == 0) days = -3;
                if (comboBox1.SelectedIndex == 1) days = -7;
                if (comboBox1.SelectedIndex == 2) days = -30;
                if (comboBox1.SelectedIndex == 3) days = -90;
                start = DateTime.Now.AddDays(days).Date;        //注意，n天内是一个模糊时间，要从n天前的凌晨开始计算时间
                stop = DateTime.Now;
            }
            else                                 //自定义日期
            {
                start = dtpStartTime.Value;
                stop = dtpStopTime.Value;
            }

            if (start.CompareTo(time_start) == 0 && stop.CompareTo(time_stop) == 0) return;   //日期未变，不重新加载文件
            //MessageBox.Show(start.ToString() + Environment.NewLine + stop.ToString());
            time_start = start;
            time_stop = stop;
            MyDefine.myXET.LoadTraceRecords(time_start, time_stop);       //重新加载审计追踪文件
        }

        /// <summary>
        /// 切换日期选择方式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                comboBox1.Enabled = true;
                dtpStartTime.Enabled = false;
                dtpStopTime.Enabled = false;
            }
            else
            {
                comboBox1.Enabled = false;
                dtpStartTime.Enabled = true;
                dtpStopTime.Enabled = true;
            }
        }

        #endregion

        #region 导出按钮 -- 将筛选出的审计追踪项导出为excel文件

        private void button4_Click(object sender, EventArgs e)
        {
            groupBox1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            MyDefine.myXET.AddTraceInfo("导出");
            string filename = MyDefine.myXET.userOUT + "\\审计追踪" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            XET.ExportToCSVExcel(MyDefine.myXET.meTraceShowTbl.dataTable, filename, true);
        }

        #endregion

        #endregion

        #region dataGridView函数集

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
            dataGridViewCellStyle1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
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
            dataGridView.AllowUserToResizeColumns = true;   //允许用户改变列宽
            dataGridView.AllowUserToResizeRows = false;     //禁止用户改变行高
            dataGridView.AllowUserToOrderColumns = false;
            dataGridView.EnableHeadersVisualStyles = false;
            dataGridView.GridColor = System.Drawing.Color.WhiteSmoke;  //网格线颜色
            this.dataGridView1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;   //垂直滚动条

            dataGridView.ReadOnly = true;     //只读
            dataGridView.RowTemplate.Height = 32;       //设置行高
            dataGridView.RowHeadersVisible = false;     //加入此行后，行高就一直可以调整？？？
            //dataGridView.Height = dataGridView.RowTemplate.Height * dataGridView.Rows.Count + dataGridView.ColumnHeadersHeight;      //设置dataGridView总高度
            //dataGridView.MultiSelect = false;       //只能选中一个或一行单元格
            //dataGridView.RowTemplate.ReadOnly = true;
            //dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;   //单击选中整行
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

        #region dataGridView排序(实现带空白行的排序)

        #region dataGridView排序事件

        bool sortSign = true;           //排序方式(升序/降序)

        //dataGridView1排序后触发
        private void dataGridView1_Sorted(object sender, EventArgs e)
        {
            sortSign = !sortSign;               //排序方式(升序/降序)

            #region 对meTraceShowTbl手动排序

            string colname = dataGridView1.SortedColumn.Name;               //当前排序所依据的列的名称
            MyDefine.myXET.meTraceShowTbl.dataTable = TableSort1(colname);  //根据当前排序信息，手动对meTraceShowTbl排序后，生成新的表格

            #endregion

            #region 制作空白表格

            int rowcount = MyDefine.myXET.meTraceShowTbl.dataTable.Rows.Count - 1;
            int rownum = (rowcount < 20) ? 20 - rowcount : 5;

            dataTableClass tmpTbl = new dataTableClass();
            tmpTbl.dataTable = MyDefine.myXET.meTraceShowTbl.CopyTable();
            tmpTbl.ClearTableData();

            //添加空行
            for (int i = 0; i < rownum; i++)
            {
                tmpTbl.AddTableRow();
            }

            #endregion

            #region 合并meTraceShowTbl(排序后)与空白表并显示

            dataTableClass myShowTbl = new dataTableClass();
            myShowTbl.dataTable.Merge(MyDefine.myXET.meTraceShowTbl.dataTable);
            myShowTbl.dataTable.Merge(tmpTbl.dataTable);    //合并空白表格
            dataGridView1.DataSource = null;                                            //要先将数据源设置为null,否则有时候单元格宽度会异常
            dataGridView1.DataSource = myShowTbl.dataTable;

            #endregion

        }

        #endregion

        #region dataTable手动排序

        /// <summary>
        /// 对mePDFShowTbl进行手动排序
        /// </summary>
        /// <param name="colname">列名</param>
        /// <returns></returns>
        public DataTable TableSort1(string colname)
        {
            String sort = sortSign ? " ASC" : " DESC";            //注意字符串里面带一个空格

            DataView myview = new DataView(MyDefine.myXET.meTraceShowTbl.dataTable);
            myview.Sort = colname + sort;
            return myview.ToTable();
        }

        #endregion

        #endregion

    }
}
