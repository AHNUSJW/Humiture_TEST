using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace HTR
{
    public partial class MenuDataImport : Form
    {
        List<String> names = new List<string>();       //存储选中文件的出厂编号
        public delegate void datagridviewcheckboxHeaderEventHander(object sender, datagridviewCheckboxHeaderEventArgs e);

        public Boolean ret = false;
        DataView dv;
        private Boolean isShowCalendar = false;
        public int currentRowIndex = 0;
        private String myPath = MyDefine.myXET.userDAT;
        private DirectoryInfo dir;

        List<string> fileLoad = new List<string>(); //读取文件

        System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();

        public MenuDataImport()
        {
            InitializeComponent();
        }
        #region 窗体事件

        //启动加载
        private void MenuDataImport_Load(object sender, EventArgs e)
        {
            DataTable dt = CreateDataTable();

            DataGridViewCheckBoxColumn checkColum = new DataGridViewCheckBoxColumn();

            //System.IO.DirectoryInfo dir;
            String filepath = MyDefine.myXET.userCFGPATH + @"\DataSavePathLog.txt";//在账户的dataPath文件夹保存存储路径
            if (!Directory.Exists(MyDefine.myXET.userCFGPATH) || !File.Exists(filepath))
            {
                Directory.CreateDirectory(MyDefine.myXET.userCFGPATH);
                StreamWriter sw = File.CreateText(filepath);
                sw.Close();
                FolderSelectDialog dialog = new FolderSelectDialog();
                dialog.InitialDirectory = myPath;//设置此次默认目录为上一次选中目录

                if (dialog.ShowDialog(this.Handle))
                {
                    myPath = dialog.FileName;
                    MyDefine.myXET.userDAT = myPath;
                }

                dir = new System.IO.DirectoryInfo(myPath);

                System.IO.File.WriteAllText(filepath, myPath.ToString());//在cfg文件中写入存储路径
            }
            else
            {
                StreamReader sr = new StreamReader(filepath);
                string str = sr.ReadToEnd();
                sr.Close();
                if (str == "")
                {
                    FolderSelectDialog dialog = new FolderSelectDialog();
                    dialog.InitialDirectory = myPath;//设置此次默认目录为上一次选中目录

                    if (dialog.ShowDialog(this.Handle))
                    {
                        myPath = dialog.FileName;
                        MyDefine.myXET.userDAT = myPath;
                    }

                    System.IO.File.WriteAllText(filepath, myPath.ToString());//在cfg文件中写入存储路径
                    StreamReader sr1 = new StreamReader(filepath);
                    string str1 = sr1.ReadToEnd();
                    sr1.Close();
                    dir = new System.IO.DirectoryInfo(str1);
                }
                else
                {
                    dir = new System.IO.DirectoryInfo(str);
                }
            }

            FileInfo[] subDirs = dir.GetFiles("*.tmp");
            SortAsFileCreationTime(ref subDirs);
            foreach (System.IO.FileInfo subDir in subDirs)
            {
                dt.Rows.Add(new object[] { false, subDir.LastWriteTime.ToString(), subDir.Name });
            }
            dv = dt.DefaultView;
            dataGridView1.DataSource = dv;

            dataGridView1.AllowUserToAddRows = false;//解决用户点击复选框，表格自动增加一行
            datagridviewCheckboxHeaderCell ch = new datagridviewCheckboxHeaderCell();
            ch.OnCheckBoxClicked += new datagridviewCheckboxHeaderCell.HeaderEventHander(ch_OnCheckBoxClicked);
            DataGridViewCheckBoxColumn checkboxCol = dataGridView1.Columns["是否选择"] as DataGridViewCheckBoxColumn;
            checkboxCol.HeaderCell = ch;
            checkboxCol.HeaderCell.Value = "";//消除列头checkbox旁出现的文字

            for (int i = 0; i < this.dataGridView1.Columns.Count; i++)
            {
                dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            pl_dgv_extend.Visible = false;      //初始话下标不可见
        }

        //关闭窗口
        private void MenuDataImport_FormClosed(object sender, FormClosedEventArgs e)
        {
            MyDefine.myXET.AddTraceInfo("取消加载");

            timer1.Stop();
        }

        #endregion

        #region 表处理

        /// <summary>
        /// 按创建的时间排序（倒序）
        /// </summary>
        /// <param name="arrFi">待排序数组</param>
        private void SortAsFileCreationTime(ref FileInfo[] arrFi)
        {
            Array.Sort(arrFi, delegate (FileInfo x, FileInfo y) { return x.LastWriteTime.CompareTo(y.LastWriteTime); });
        }

        //创建表头
        private DataTable CreateDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("是否选择", System.Type.GetType("System.Boolean"));
            dt.Columns.Add("Date",System.Type.GetType("System.String"));
            dt.Columns.Add(new DataColumn("FileName"));
            return dt;
        }
        #endregion

        #region 表多选
        void ch_OnCheckBoxClicked(object sender, datagridviewCheckboxHeaderEventArgs e)
        {
            dataGridView1.SuspendLayout();//暂停布局更新
            dataGridView1.BeginEdit(false);//批量操作开始，避免更改值时触发额外的事件和验证
            foreach (DataGridViewRow dgvRow in dataGridView1.Rows)
            {
                if (e.CheckedState)
                {
                    dgvRow.Cells[0].Value = true;
                }
                else
                {
                    dgvRow.Cells[0].Value = false;
                }
            }
            dataGridView1.EndEdit();//批量操作结束
            dataGridView1.ResumeLayout();//恢复布局更新
        }

        public class datagridviewCheckboxHeaderEventArgs : EventArgs
        {
            private bool checkedState = false;
            public bool CheckedState
            {
                get { return checkedState; }
                set { checkedState = value; }
            }
        }

        //定义继承于DataGridViewColumnHeaderCell的类，用于绘制checkbox，定义checkbox鼠标单击事件
        public class datagridviewCheckboxHeaderCell : DataGridViewColumnHeaderCell
        {
            public delegate void HeaderEventHander(object sender, datagridviewCheckboxHeaderEventArgs e);
            public event HeaderEventHander OnCheckBoxClicked;
            Point checkBoxLocation;
            Size checkBoxSize;
            bool _checked = false;
            Point _cellLocation = new Point();
            System.Windows.Forms.VisualStyles.CheckBoxState _cbState =
                System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal;

            //绘制列头checkbox
            protected override void Paint(System.Drawing.Graphics graphics,
               System.Drawing.Rectangle clipBounds,
               System.Drawing.Rectangle cellBounds,
               int rowIndex,
               DataGridViewElementStates dataGridViewElementState,
               object value,
               object formattedValue,
               string errorText,
               DataGridViewCellStyle cellStyle,
               DataGridViewAdvancedBorderStyle advancedBorderStyle,
               DataGridViewPaintParts paintParts)
            {
                base.Paint(graphics, clipBounds, cellBounds, rowIndex,
                    dataGridViewElementState, value,
                    formattedValue, errorText, cellStyle,
                    advancedBorderStyle, paintParts);
                Point p = new Point();
                Size s = CheckBoxRenderer.GetGlyphSize(graphics,
                System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal);
                p.X = cellBounds.Location.X +
                    (cellBounds.Width / 2) - (s.Width / 2) - 1;//列头checkbox的X坐标
                p.Y = cellBounds.Location.Y +
                    (cellBounds.Height / 2) - (s.Height / 2);//列头checkbox的Y坐标
                _cellLocation = cellBounds.Location;
                checkBoxLocation = p;
                checkBoxSize = s;
                if (_checked)
                    _cbState = System.Windows.Forms.VisualStyles.
                        CheckBoxState.CheckedNormal;
                else
                    _cbState = System.Windows.Forms.VisualStyles.
                        CheckBoxState.UncheckedNormal;
                CheckBoxRenderer.DrawCheckBox
                (graphics, checkBoxLocation, _cbState);
            }

            /// <summary>
            /// 点击列头checkbox单击事件
            /// </summary>
            /// <param name="e"></param>
            protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
            {
                Point p = new Point(e.X + _cellLocation.X, e.Y + _cellLocation.Y);
                if (p.X >= checkBoxLocation.X && p.X <=
                    checkBoxLocation.X + checkBoxSize.Width
                && p.Y >= checkBoxLocation.Y && p.Y <=
                    checkBoxLocation.Y + checkBoxSize.Height)
                {
                    _checked = !_checked;
                    //获取列头checkbox的选择状态
                    datagridviewCheckboxHeaderEventArgs ex = new datagridviewCheckboxHeaderEventArgs();
                    ex.CheckedState = _checked;

                    object sender = new object();//此处不代表选择的列头checkbox，只是作为参数传递。应该列头checkbox是绘制出来的，无法获得它的实例

                    if (OnCheckBoxClicked != null)
                    {
                        OnCheckBoxClicked(sender, ex);//触发单击事件
                        this.DataGridView.InvalidateCell(this);
                    }
                }
                base.OnMouseClick(e);
            }
        }

        //结束编辑状态
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (isShowCalendar && (e.RowIndex != -1 || e.ColumnIndex != 1))
            {
                isShowCalendar = false;
                pl_dgv_extend.Visible = false;
            }

            if (e.RowIndex != -1)//&& e.ColumnIndex == 0
            {
                dataGridView1.EndEdit();
                if (e.ColumnIndex != 0)
                {
                    dataGridView1.Rows[e.RowIndex].Cells[0].Value = !Convert.ToBoolean(dataGridView1.Rows[e.RowIndex].Cells[0].Value);
                }
            }
        }

        #endregion

        #region 打开文件
        private void button1_Click(object sender, EventArgs e)
        {
            int count = 0;//选择文件数

            //清空存储选中文件的出厂编号
            names.Clear();

            string filePath = MyDefine.myXET.userCFGPATH + @"\DataSavePathLog.txt";
            StreamReader sr = new StreamReader(filePath);
            string str = sr.ReadToEnd();
            sr.Close();
            foreach (DataGridViewRow dgvRow in dataGridView1.Rows)
            {
                string s = str + @"\" + dgvRow.Cells[2].Value;// userDAT = Application.StartupPath + @"\dat";
                if (Convert.ToBoolean(dgvRow.Cells[0].Value))
                {
                    string name = dgvRow.Cells[2].Value.ToString().Split(new char[] { '.' })[2];
                    if (names.Contains(name))
                    {
                        MessageBox.Show("设备名称为" + name + "存在两个或两个以上");
                        return;
                    }
                    else
                    {
                        names.Add(name);
                        fileLoad.Add(s);
                        count++;
                    }

                }
            }
            if (count > 0)
            {
                if (radioButton1.Checked)
                {
                    MyDefine.myXET.temUnit = "℃";
                }
                else
                {
                    MyDefine.myXET.temUnit = "℉";
                }
                //打开文件

                timer1.Interval = 500;
                timer1.Tick += Timer1_Tick;
                timer1.Start();

                button1.Enabled = false;

                Thread thread = new Thread(DoLoadDataSource);
                thread.Start();
            }
            else
            {
                MessageBox.Show("未选择任何文件", "提示");
            }
        }

        //读取文件生成表格
        private void DoLoadDataSource()
        {
            ret = MyDefine.myXET.loadDataSource(fileLoad.ToArray());      //加载一个或多个文件

            button1.Invoke((MethodInvoker)delegate
            {
                MyDefine.myXET.processValue = 0;

                this.Close();
            });
        }

        //进度条变化
        private void Timer1_Tick(object sender, EventArgs e)
        {
            ucProcessLine1.Value = MyDefine.myXET.processValue;
        }

        #endregion

        #region 表下拉框处理
        //点击dgv列标题，panel显示，并根据列标题的不同位置，对应显示到相应的位置
        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int dLeft, dTop;

            if (e.ColumnIndex != 1)
            {
                if (isShowCalendar)
                {
                    isShowCalendar = false;
                    pl_dgv_extend.Visible = false;
                }
                return;
            }

            //获取dgv列标题位置相对坐标
            Rectangle range = dataGridView1.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
            //计算pl_dgv_extend位置坐标
            dLeft = range.Left + dataGridView1.Left;
            dTop = range.Top + dataGridView1.Top + range.Height;

            //设置pl_dgv_extend位置，超出框体宽度时，和dgv右边对齐
            if (dLeft + pl_dgv_extend.Width > this.Width)
            {
                pl_dgv_extend.SetBounds(pl_dgv_extend.Width - pl_dgv_extend.Width, dTop, pl_dgv_extend.Width, pl_dgv_extend.Height);
            }
            else
            {
                pl_dgv_extend.SetBounds(dLeft, dTop, pl_dgv_extend.Width, pl_dgv_extend.Height);
            }

            ////设置cb_condition下拉菜单内容
            //cb_condition.Items.Clear();
            //DateTime dateTime;
            //for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            //{
            //    bool isfind = false;
            //    dateTime = Convert.ToDateTime(dataGridView1.Rows[i].Cells[e.ColumnIndex].Value.ToString());

            //    for (int j = 0; j < cb_condition.Items.Count; j++)
            //    {
            //        if (cb_condition.Items[j].ToString() == dateTime.ToString("yyyy/M/d"))
            //        {
            //            isfind = true;
            //            j = cb_condition.Items.Count;//break
            //        }
            //    }
            //    if (!isfind)
            //    {
            //        cb_condition.Items.Add(dateTime.ToString("yyyy/M/d"));
            //    }
            //}
            isShowCalendar = true;
            pl_dgv_extend.Visible = true;
            monthCalendar1.TodayDate = DateTime.Now;
            ////初始化选择项
            //cb_operator.SelectedIndex = 0;
            //cb_condition.Text = "";
            ////存储现有的筛选条件选项
            //if (ltb_condition.Items.Count == 0)
            //{
            //    str_ltb = null;
            //}
            //else
            //{
            //    str_ltb = new string[ltb_condition.Items.Count];
            //    ltb_condition.Items.CopyTo(str_ltb, 0);
            //}
        }

        ////升序排列
        //private void bt_asc_Click(object sender, EventArgs e)
        //{
        //    dv.Sort = "Date asc";

        //    pl_dgv_extend.Visible = false;
        //}
        ////降序排列
        //private void bt_desc_Click(object sender, EventArgs e)
        //{
        //    dv.Sort = "Date desc";
        //    pl_dgv_extend.Visible = false;
        //}

        ////添加筛选条件
        //private void bt_add_Click(object sender, EventArgs e)
        //{
        //    //空值判断
        //    if (cb_operator.SelectedItem == null || cb_condition.Text.Trim() == "")
        //    {
        //        MessageBox.Show("请选择条件！");
        //        return;
        //    }
        //    if (cb_andor.SelectedItem == null && ltb_condition.Items.Count > 0)
        //    {
        //        MessageBox.Show("请选择条件！");
        //        return;
        //    }
        //    //文字转运算符
        //    string str_andor = "", str_operator = "";
        //    if (ltb_condition.Items.Count > 0)
        //    {
        //        if (cb_andor.SelectedItem == null) str_andor = "";
        //        else if (cb_andor.SelectedItem.ToString() == "或") str_andor = "or ";
        //        else str_andor = "and ";
        //    }
        //    if (cb_operator.SelectedItem.ToString() == "等于") str_operator = "like ";
        //    else if (cb_operator.SelectedItem.ToString() == "大于") str_operator = "> ";
        //    else if (cb_operator.SelectedItem.ToString() == "小于") str_operator = "< ";

        //    //筛选条件添加到listbox
        //    if (cb_operator.SelectedItem.ToString() == "等于")
        //    {
        //        ltb_condition.Items.Add(str_andor + "Date " + str_operator + "'*" + cb_condition.Text.Trim() + "*'");
        //    }
        //    else
        //    {
        //        ltb_condition.Items.Add(str_andor + "Date " + str_operator + "#" + cb_condition.Text.Trim() + "#");
        //    }
        //}

        ////移除筛选条件
        //private void bt_remove_Click(object sender, EventArgs e)
        //{
        //    if (ltb_condition.SelectedItem == null)
        //    {
        //        MessageBox.Show("请选择需要移除的项！");
        //        return;
        //    }
        //    int rownum = ltb_condition.SelectedIndex;
        //    ltb_condition.Items.RemoveAt(rownum);
        //    //如果移除了第一项并且移除后listbox项数目大于0
        //    //把第一项开头的and或者or去掉
        //    if (ltb_condition.Items.Count > 0 && rownum == 0)
        //    {
        //        string first = ltb_condition.Items[0].ToString();
        //        string result;
        //        if (first.Substring(0, 2) == "or")
        //        {
        //            result = first.Substring(2, first.Length - 3);
        //        }
        //        else
        //        {
        //            result = first.Substring(3, first.Length - 4);
        //        }
        //        ltb_condition.Items.RemoveAt(0);
        //        ltb_condition.Items.Insert(0, result);
        //    }
        //}
        ////清除所有筛选条件
        //private void bt_clear_Click(object sender, EventArgs e)
        //{
        //    if (MessageBox.Show("是否要移除所有筛选条件？", "警告", MessageBoxButtons.YesNo) == DialogResult.Yes)
        //    {
        //        ltb_condition.Items.Clear();
        //    }
        //    dv.RowFilter = ""; //清空筛选条件
        //    pl_dgv_extend.Visible = false;

        //}
        ////确认，执行筛选程序
        //private void bt_ok_Click(object sender, EventArgs e)
        //{
        //    string search_line = "";
        //    if (ltb_condition.Items.Count == 0)
        //    {
        //        dv.RowFilter = "";
        //        return;
        //    }
        //    else
        //    {
        //        for (int i = 0; i < ltb_condition.Items.Count; i++)
        //        {
        //            if (i == 0) search_line = ltb_condition.Items[0].ToString();
        //            else search_line += " " + ltb_condition.Items[i].ToString();
        //        }
        //        dv.RowFilter = search_line;
        //    }
        //    pl_dgv_extend.Visible = false;


        //    string str = "";
        //    foreach (DataGridViewRow dgvRow in dataGridView1.Rows)
        //    {
        //        if (Convert.ToBoolean(dgvRow.Cells[0].Value))
        //        {
        //            str += dgvRow.Cells[1].Value.ToString() + "\r\n";
        //        }
        //    }
        //    MessageBox.Show(str);
        //}

        ////取消，回滚操作
        //private void bt_cancle_Click(object sender, EventArgs e)
        //{//回滚操作
        //    ltb_condition.Items.Clear();
        //    if (str_ltb != null) ltb_condition.Items.AddRange(str_ltb);
        //    pl_dgv_extend.Visible = false;
        //}
        #endregion

        #region 日历事件

        /// <summary>
        /// 离开日历控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void monthCalendar1_Leave(object sender, EventArgs e)
        {
            isShowCalendar = false;
            pl_dgv_extend.Visible = false;
        }

        /// <summary>
        /// 选择日历时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void monthCalendar1_DateSelected(object sender, DateRangeEventArgs e)
        {
            string starttime = monthCalendar1.SelectionStart.AddDays(1).ToString("yyyy/M/d");
            string endtime = monthCalendar1.SelectionStart.ToString("yyyy/M/d");
            string search_line = "Date < #" + starttime + "# and Date > #" + endtime + "#";
            dv.RowFilter = search_line;

            MyDefine.myXET.AddTraceInfo("选择日期：" + monthCalendar1.SelectionStart.ToString("yyyy/M/d"));
        }

        #endregion

        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (dataGridView1.Rows.Count != 0)
            {
                if (e.Button == MouseButtons.Left && !(Control.ModifierKeys == Keys.Shift))
                {
                    currentRowIndex = this.dataGridView1.CurrentRow.Index;
                }
            }
        }

        private void dataGridView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.dataGridView1.SelectedCells.Count > 0 && e.KeyData == Keys.ShiftKey)
            {
                int endrow = this.dataGridView1.CurrentRow.Index;
                if (currentRowIndex <= endrow)
                {
                    //正序选时
                    for (int i = currentRowIndex; i <= endrow; i++)
                    {
                        this.dataGridView1.Rows[i].Cells[0].Value = true;
                    }

                    for (int j = endrow + 1; j < this.dataGridView1.Rows.Count; j++)
                    {
                        this.dataGridView1.Rows[j].Cells[0].Value = false;
                    }

                    for (int k = 0; k < currentRowIndex; k++)
                    {
                        this.dataGridView1.Rows[k].Cells[0].Value = false;
                    }
                }
                else
                {
                    //倒序选时
                    for (int i = endrow; i <= currentRowIndex; i++)
                    {
                        this.dataGridView1.Rows[i].Cells[0].Value = true;
                    }

                    for (int j = 0; j < endrow; j++)
                    {
                        this.dataGridView1.Rows[j].Cells[0].Value = false;
                    }

                    for (int k = currentRowIndex + 1; k < this.dataGridView1.Rows.Count; k++)
                    {
                        this.dataGridView1.Rows[k].Cells[0].Value = false;
                    }
                }
            }
        }

    }
}
