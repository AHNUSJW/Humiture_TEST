using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace HTR
{
    public partial class MenuCalPanel : UserControl
    {
        public float[] colWidth = new float[] { 11, 11, 11, 11, 11, 11, 11, 11, 11 };        //设置列宽比例dataGridView1

        public MenuCalPanel()
        {
            InitializeComponent();
        }

        #region 界面加载

        private void MenuCalPanel_Load(object sender, EventArgs e)
        {
            //dataGridView2.ColumnHeadersVisible = false;     //隐藏表2的列头
            dataGridView1.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
            dataGridView2.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
        }

        public void AddMyUpdateEvent()
        {
            try
            {
                //重新加载文件后新建表格
                if (MyDefine.myXET.meCalTableUpdating == true)    //有新的数据表文件加载
                {
                    MyDefine.myXET.meCalTableUpdating = false;
                    GenerateTable();            //根据已加载的文件数据，生成对应的检索表
                }

                switchDataTables();     //切换数据表
                SetTableReadOnly();     //设置只读列
                DisableGridViewSort();  //禁止列排序
                checkPermission();      //核对权限(需放在切换数据表后面)
              
                dadataGridViewAutoResize();         //重新分配dadataGridView行高列宽
            }
            catch (Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("界面加载失败：" + ex.ToString());
            }
        }

        public void switchDataTables()
        {
            try
            {
                switch (this.Name)
                {
                    case "标定":
                        dataGridView1.DataSource = null;
                        dataGridView1.DataSource = MyDefine.myXET.meTblCal1.dataTable;
                        dataGridView2.DataSource = null;
                        dataGridView2.DataSource = MyDefine.myXET.meTblCal2.dataTable;
                        ShowFunctionButtons();          //根据界面名称显示需要的按钮
                        break;

                    case "校准":
                        dataGridView1.DataSource = null;
                        dataGridView1.DataSource = MyDefine.myXET.meTblPre1.dataTable;
                        dataGridView2.DataSource = null;
                        dataGridView2.DataSource = MyDefine.myXET.meTblPre2.dataTable;
                        ShowFunctionButtons();          //根据界面名称显示需要的按钮
                       
                        //隐藏标定值列
                        for (int i = 0; i < dataGridView2.Rows.Count; i++)
                        {
                            String rowname = dataGridView2.Rows[i].Cells[0].Value.ToString();
                            if (rowname == "标定值") dataGridView2.Rows[i].Visible = false;
                        }
                        break;

                    default:
                        break;
                }

                //重新设置带出厂编号行的背景色
                if (MyDefine.myXET.meTypeList.Count > 0)
                {
                    for (int i = 0; i < MyDefine.myXET.meTypeList.Count - 1; i++)
                    {
                        dataGridView2.Rows[i * 6].DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(177, 183, 204);    //行背景色
                    }
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("切换数据表失败：" + ex.ToString());
            }
        }

        //根据最新文件类型更新标定点数并设置只读列
        public void SetTableReadOnly()
        {
            dataGridView1.Columns[0].ReadOnly = true;   //标题列只读

            //表1 -- 校准界面全部可编辑
            if (this.Name == "校准")
            {
                for (int i = 1; i < dataGridView1.Columns.Count; i++)
                {
                    dataGridView1.Columns[i].ReadOnly = false;
                }
            }

            //表1 -- 标定界面部分可编辑
            if (this.Name == "标定")
            {
                MyDefine.myXET.meDotNum = 0;         //标定点数
                if (MyDefine.myXET.meTypeList.Contains("TP_P")) MyDefine.myXET.meDotNum = 3;
                if (MyDefine.myXET.meTypeList.Contains("TT_T")) MyDefine.myXET.meDotNum = 8;
                if (MyDefine.myXET.meTypeList.Contains("TQ_T")) MyDefine.myXET.meDotNum = 8;
                if (!MyDefine.myXET.checkDataFileType(false)) MyDefine.myXET.meDotNum = 0;           //原始数据类型不一致(包含温度、湿度、压力数据中的一种以上)

                for (int i = 1; i < dataGridView1.Columns.Count; i++)        //设置dataGridView1只读列
                {
                    //if (i == 0) dataGridView1.Columns[i].ReadOnly = true;
                    if (i < MyDefine.myXET.meDotNum) dataGridView1.Columns[i].ReadOnly = false;
                    //if (i > 0 && i < MyDefine.myXET.meDotNum) dataGridView1.Columns[i].ReadOnly = false;
                    if (i > MyDefine.myXET.meDotNum) dataGridView1.Columns[i].ReadOnly = true;
                }
            }

            //表2为只读
            dataGridView2.ReadOnly = true;
        }

        #region 禁止DataGridView列排序

        //禁止列排序
        public void DisableGridViewSort()
        {
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                dataGridView2.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

        }

        #endregion

        #region 根据界面名称显示需要的按钮

        //根据界面名称显示需要的按钮
        public void ShowFunctionButtons()
        {
            if(this.Name == "标定")
            {
                button3.Visible = true;         //显示数据检索按钮
                button6.Visible = true;         //显示读取标定按钮
                button1.Visible = true;         //显示批量标定按钮
                button11.Visible = true;        //显示生成曲线按钮
                button12.Visible = false;       //隐藏标准器录入按钮
                button10.Visible = false;       //隐藏生成报告按钮

                //调整生成曲线按钮的显示位置(标定的时候[曲线]按钮显示在原[生成报告]按钮的位置])
                this.button11.Location = this.button10.Location;      //调整生成曲线按钮位置
                this.button11.Tag = this.button10.Tag;                //必须将tag一并调整，否则窗体Resize时控件会回到原始位置
            }
            else if (this.Name == "校准")
            {
                button3.Visible = true;         //显示数据检索按钮
                button12.Visible = true;        //显示标准器录入按钮
                button11.Visible = true;        //显示生成曲线按钮
                button10.Visible = true;        //显示生成报告按钮
                button6.Visible = true;         //隐藏读取标定按钮
                button1.Visible = true;         //隐藏批量标定按钮

                //调整生成曲线按钮的显示位置(校准的时候[曲线]按钮显示在原[批量标定]按钮的位置])
                this.button11.Location = this.button1.Location;      //调整生成曲线按钮位置
                this.button11.Tag = this.button1.Tag;                //必须将tag一并调整，否则窗体Resize时控件会回到原始位置
            }
        }

        #endregion

        #region 核对权限

        public void checkPermission()
        {
            //核对权限(不要弹框)
            if (this.Name == "校准")
            {
                button6.Visible = false;        //读取标定按钮
                button1.Visible = false;        //批量标定按钮
                button12.Visible = MyDefine.myXET.CheckPermission(STEP.标准器录入, false) ? true : false;
                button3.Visible = MyDefine.myXET.CheckPermission(STEP.数据检索, false) ? true : false;
                button11.Visible = MyDefine.myXET.CheckPermission(STEP.校准曲线, false) ? true : false;
                button10.Visible = MyDefine.myXET.CheckPermission(STEP.校准报告, false) ? true : false;
            }

            //核对权限(不要弹框)
            if (this.Name == "标定")
            {
                button12.Visible = false;        //标准器录入按钮
                button10.Visible = false;        //校准报告按钮
                button6.Visible = MyDefine.myXET.CheckPermission(STEP.出厂标定, false) ? true : false;  //读取标定按钮
                button3.Visible = MyDefine.myXET.CheckPermission(STEP.出厂标定, false) ? true : false;  //数据检索按钮
                button1.Visible = MyDefine.myXET.CheckPermission(STEP.出厂标定, false) ? true : false;  //批量标定按钮
                button11.Visible = MyDefine.myXET.CheckPermission(STEP.出厂标定, false) ? true : false; //曲线图按钮
            }

        }

        #endregion

        #endregion

        #region dataGridView加载

        //dataGridView加载
        public void InitDataGridView()
        {
            try
            {
                //初始化
                dataGridView(dataGridView1);    //dataGridView初始化必须放在添加列之前，行高设置才能起作用
                dataGridView(dataGridView2);    //dataGridView初始化必须放在添加列之前，行高设置才能起作用

                //添加列
                MyDefine.myXET.meTblCal1 = new dataTableClass();
                MyDefine.myXET.meTblCal1.addTableColumn("标定点数");            //第0栏
                MyDefine.myXET.meTblCal2 = new dataTableClass();
                MyDefine.myXET.meTblCal2.addTableColumn("标定点数");            //第0栏

                //添加列
                for (int i = 1; i <= 8; i++)
                {
                    MyDefine.myXET.meTblCal1.addTableColumn(i.ToString());      //第i栏
                    MyDefine.myXET.meTblCal2.addTableColumn(i.ToString());      //第i栏
                }

                //添加行
                MyDefine.myXET.meTblCal1.AddTableRow("读数时间");
                MyDefine.myXET.meTblCal1.AddTableRow("设定温度");
                MyDefine.myXET.meTblCal1.AddTableRow("标准温度");
                MyDefine.myXET.meTblCal2.AddTableRow("设备温度");
                MyDefine.myXET.meTblCal2.AddTableRow("标定值");
                MyDefine.myXET.meTblCal2.AddTableRow("波动度");
                MyDefine.myXET.meTblCal2.AddTableRow("上偏差");
                MyDefine.myXET.meTblCal2.AddTableRow("下偏差");

                MyDefine.myXET.meTblPre1 = new dataTableClass();
                MyDefine.myXET.meTblPre2 = new dataTableClass();
                MyDefine.myXET.meTblPre1.dataTable = MyDefine.myXET.meTblCal1.CopyTable();
                MyDefine.myXET.meTblPre2.dataTable = MyDefine.myXET.meTblCal2.CopyTable();
                MyDefine.myXET.meTblPre1.dataTable.Columns[0].ColumnName = "校准点数";
                MyDefine.myXET.meTblPre2.dataTable.Columns[0].ColumnName = "校准点数";
                //dataGridView1.DataSource = MyDefine.myXET.meTblCal1.dataTable;
                //dataGridView2.DataSource = MyDefine.myXET.meTblCal2.dataTable;
                //dataGridView1.ReadOnly = true;
                //dataGridView2.ReadOnly = true;


                DisableGridViewSort();          //禁止列排序
                dataGridView1.ClearSelection(); //清除单元格选中状态(放在添加列后才起作用)
                dataGridView2.ClearSelection(); //清除单元格选中状态(放在添加列后才起作用)
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        #endregion

        #region 界面按钮操作

        #region 保存数据表按钮 -- 保存当前界面上的数据表1

        //保存数据表按钮
        private void button4_Click(object sender, EventArgs e)
        {
            dataGridView1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            try
            {
                MyDefine.myXET.AddTraceInfo("保存数据表");
                if (this.Name == "校准") MyDefine.myXET.savePreTable();
                if (this.Name == "标定") MyDefine.myXET.saveCalTable();
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("保存数据表失败：" + ex.ToString());
            }
        }

        //清空数据表按钮
        private void button4_Click0(object sender, EventArgs e)
        {
            dataGridView1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            try
            {
                MyDefine.myXET.AddTraceInfo("清空数据表");

                //清空表1
                for (int irow = 0; irow < dataGridView1.Rows.Count; irow++)
                {
                    for (int jcol = 1; jcol < dataGridView1.Columns.Count; jcol++)      //第一列不清空
                    {
                        dataGridView1.Rows[irow].Cells[jcol].Value = "";
                    }
                }

                //清空表2
                for (int irow = 0; irow < dataGridView2.Rows.Count; irow++)
                {
                    for (int jcol = 1; jcol < dataGridView2.Columns.Count; jcol++)      //第一列不清空
                    {
                        if ((irow % 6 == 0)) continue;                                  //带出厂编号的行不清空
                        dataGridView2.Rows[irow].Cells[jcol].Value = "";
                    }
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("清空数据表失败：" + ex.ToString());
            }
        }

        #endregion

        #region 加载数据表按钮 -- 加载之前保存到文件中的校准表1或标定表1

        //加载数据表
        private void button2_Click(object sender, EventArgs e)
        {
            dataGridView1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            try
            {
                MyDefine.myXET.AddTraceInfo("加载数据表");
                if(this.Name == "校准")
                {
                    if (!MyDefine.myXET.meTblPre1.IsRowEmpty(0, 1))             //当前校准表第一行读数时间不为空
                    {
                        if (MessageBox.Show("是否覆盖当前表格数据？", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
                        {
                            return;
                        }
                    }

                    //加载校准表
                    Boolean ret = MyDefine.myXET.loadPreTable();
                    if (ret)
                    {
                        MyDefine.myXET.ShowCorrectMsg("加载成功！");
                    }
                    else
                    {
                        MyDefine.myXET.ShowWrongMsg("加载失败：文件不存在！");
                    }
                }

                if (this.Name == "标定")
                {
                    if (!MyDefine.myXET.meTblCal1.IsRowEmpty(0, 1))             //当前校准表第一行读数时间不为空
                    {
                        if (MessageBox.Show("是否覆盖当前表格数据？", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
                        {
                            return;
                        }
                    }

                    //加载标定表
                    Boolean ret = MyDefine.myXET.loadCalTable();
                    if(ret)
                    {
                        MyDefine.myXET.ShowCorrectMsg("加载成功！");
                        
                        //将标定点数之外的列数据清空
                        for (int irow = 0; irow < MyDefine.myXET.meTblCal1.dataTable.Rows.Count; irow++)
                        {
                            for (int jcol = MyDefine.myXET.meDotNum + 1; jcol < 8; jcol++)
                            {
                                if (jcol == 0) continue;                                    //第一列为标题列，不清空
                                MyDefine.myXET.meTblCal1.SetCellValue(irow, jcol, "");
                            }
                        }
                    }
                    else
                    {
                        MyDefine.myXET.ShowWrongMsg("加载失败：文件不存在！");
                    }
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("加载数据表失败：" + ex.ToString());
            }
        }

        #endregion

        #region 标准器录入按钮 -- 切换到标准器录入界面

        //标准器录入
        private void button12_Click(object sender, EventArgs e)
        {
            dataGridView1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            MyDefine.myXET.switchMainPanel(2);                      //切换到标准器录入界面
        }

        #endregion

        #region 读取标定按钮 -- 读取并显示设备当前已标定值

        //读取标定值
        private void button6_Click(object sender, EventArgs e)
        {
            dataGridView1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            if (!MyDefine.myXET.checkDataFileStatus()) return;  //尚未加载数据
            if (!MyDefine.myXET.checkDataFileNumber()) return;  //数据文件数量与当前产品数量是否一致
            if (!MyDefine.myXET.checkDataFileType()) return;    //原始数据类型不一致(包含温度、湿度、压力数据中的一种以上)
            if (!MyDefine.myXET.checkDeviceStatus()) return;    //设备未连接或繁忙
            MyDefine.myXET.meTask = TASKS.setting;  //设置串口为工作中
            Byte activeDUTAddress = MyDefine.myXET.meActiveAddr;   //保存当前正在连接的设备地址，批量设置完后再设置回来
            MyDefine.myXET.meTips = "[READ CALIBRATION]" + Environment.NewLine;
            MyDefine.myXET.AddTraceInfo("读取标定");
            button6.Text = "读取中...";
            Boolean ret = true;

            try
            {
                //读取并显示产品当前标定数据
                for (int i = 0; i < MyDefine.myXET.meDUTAddrArr.Count; i++)
                {
                    if (!ret) break;
                    MyDefine.myXET.meActiveAddr = MyDefine.myXET.meDUTAddrArr[i];     //切换当前设备地址

                    //读取产品编号
                    ret = MyDefine.myXET.readJSN();

                    //获取当前产品JSN对应的文件索引
                    int culomnNum = 0;                      //需要标定的列数(点数)
                    string jsn = MyDefine.myXET.meJSN.Substring(2);
                    int idx = MyDefine.myXET.meJSNList.IndexOf(jsn);
                    if(idx >= 1)    //MyDefine.myXET.meTypeList[0]为"0",有效索引从1开始
                    {
                        //读取标定值
                        String mytype = MyDefine.myXET.meTypeList[idx];
                        if (mytype == "TT_T")               //温度采集器
                        {
                            ret = MyDefine.myXET.readDOT(Constants.LEN_READ_DOT1);
                            if (ret) culomnNum = 8;
                        }
                        else if (mytype == "TP_P")          //压力采集器
                        {
                            ret = MyDefine.myXET.readDOT(Constants.LEN_READ_DOT2);
                            if (ret) culomnNum = 3;
                        }
                        else                                //温湿度采集器没有标定
                        {
                            ret = MyDefine.myXET.readDOT(Constants.LEN_READ_DOT3);
                            if (ret) culomnNum = 8;
                        }

                        //显示标定值
                        for (int j = 0; j < culomnNum; j++)
                        {
                            dataGridView1.Rows[2].Cells[j + 1].Value = MyDefine.myXET.meTemp_CalPoints[j] / 100.0;          //显示设备标准温度
                            dataGridView2.Rows[(idx - 1) * 6 + 2].Cells[j + 1].Value = MyDefine.myXET.meADC_CalPoints[j];   //显示设备标定ADC
                            if(mytype != "TT_T" && mytype != "TP_P")
                            {
                                dataGridView1.Rows[2 + 2].Cells[j + 1].Value = MyDefine.myXET.meHum_CalPoints[j] / 100.0;          //显示设备标准温度
                                dataGridView2.Rows[(idx - 1) * 6 + 2 + 6].Cells[j + 1].Value = MyDefine.myXET.meADC1_CalPoints[j];   //显示设备标定ADC
                            }
                        }
                    }
                    else if (idx == -1)
                    {
                        MyDefine.myXET.ShowWrongMsg("设备" + MyDefine.myXET.meJSN + "未加载对应文件数据！");
                        MyDefine.myXET.meTask = TASKS.run;
                        button6.Text = "读取标定";
                        dataGridView1.Focus();      //将焦点从button上移走，使button每次单击都有点击效果
                        return;
                    }
                }

                if (!ret) MyDefine.myXET.ShowWrongMsg("读取标定失败，请检查设备是否连接！");
                dataGridView1.Focus();      //将焦点从button上移走，使button每次单击都有点击效果
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("读取标定失败:" + ex.ToString());
            }

            if (ret) MyDefine.myXET.ShowCorrectMsg("读取标定成功！");

            MyDefine.myXET.meTask = TASKS.run;
            MyDefine.myXET.meActiveAddr = activeDUTAddress;        //将当前设备地址切换回批量设置前的已连接设备地址
            MyDefine.myXET.meTips += "ReadREG_Device:" + Environment.NewLine;
            MyDefine.myXET.readDevice();                           //重新读取当前设备型号等参数
            MyDefine.myXET.SaveCommunicationTips();  //将调试信息保存到操作日志

            button6.Text = "读取标定";
            dataGridView1.Focus();      //将焦点从button上移走，使button每次单击都有点击效果
            return;
        }

        #endregion

        #region 批量标定按钮 -- 将标定数据写入设备

        //将（标准温度，标定ADC）写入设备
        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView1.Focus(); Application.DoEvents();      //将焦点从button上移走，使button每次单击都有点击效果

            MyDefine.myXET.meDotComplete = false;               //标定未完成，不显示标定后曲线
            if (!MyDefine.myXET.checkDataFileStatus()) return;  //尚未加载数据
            if (!MyDefine.myXET.checkDataFileNumber()) return;  //数据文件数量与当前产品数量是否一致
            if (!MyDefine.myXET.checkDataFileType()) return;    //原始数据类型不一致(包含温度、湿度、压力数据中的一种以上)
            if (!MyDefine.myXET.checkDeviceStatus()) return;    //设备未连接或繁忙

            MyDefine.myXET.meTask = TASKS.setting;  //设置串口为工作中
            Byte activeDUTAddress = MyDefine.myXET.meActiveAddr;//保存当前正在连接的设备地址，批量设置完后再设置回来
            MyDefine.myXET.meTips = "[DEVICE CALIBRATION]" + Environment.NewLine;
            MyDefine.myXET.AddTraceInfo("批量标定");
            button1.Text = "标定中...";

            try
            {
                Boolean ret = true;
                String myStr1, myStr2;
                Decimal myTemp;
                Int32 myADC;

                //核对标准温度的数量和格式是否正确
                for (int i = 0; i < MyDefine.myXET.meDotNum; i++)
                {
                    myStr1 = Convert.ToString(dataGridView1.Rows[2].Cells[i + 1].Value);                        //设备标准温度

                    if (Decimal.TryParse(myStr1, out myTemp) == false)   //标准温度值非数字
                    {
                        dataGridView1.ClearSelection();
                        dataGridView1.Rows[2].Cells[i + 1].Selected = true;
                        MyDefine.myXET.ShowWrongMsg("标定失败：标定数据 \"" + myStr1 + "\" 格式错误！" + Environment.NewLine + myStr1);
                        //MessageBox.Show("标定数据 \"" + myStr1 + "\" 格式错误！" + Environment.NewLine + myStr1, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        MyDefine.myXET.meTask = TASKS.run;
                        button1.Text = "设备标定";
                        return;
                    }
                    MyDefine.myXET.meTemp_CalPoints[i] = (Int32)(myTemp * 100);
                }

                //核对标准温度的温度点顺序是否正确：标定温度点必须从小到大排列，如-80，-50，-20，5，25
                for (int i = 0; i < MyDefine.myXET.meDotNum - 1; i++)
                {
                    int x1 = MyDefine.myXET.meTemp_CalPoints[i];
                    int x2 = MyDefine.myXET.meTemp_CalPoints[i + 1];
                    if (x1 >= x2)       //温度点顺序排列错误
                    {
                        dataGridView1.ClearSelection();
                        dataGridView1.Rows[2].Cells[i + 1].Selected = true;
                        dataGridView1.Rows[2].Cells[i + 2].Selected = true;
                        MyDefine.myXET.ShowWrongMsg("标定失败：标准温度/压力必须从小到大排列！" + Environment.NewLine);
                        MyDefine.myXET.meTask = TASKS.run;
                        button1.Text = "设备标定";
                        return;
                    }
                }

                if(MyDefine.myXET.meType == DEVICE.HTQ)
                {
                    //核对标准湿度的数量和格式是否正确
                    for (int i = 0; i < MyDefine.myXET.meDotNum; i++)
                    {
                        myStr1 = Convert.ToString(dataGridView1.Rows[4].Cells[i + 1].Value);                        //设备标准温度

                        if (Decimal.TryParse(myStr1, out myTemp) == false)   //标准温度值非数字
                        {
                            dataGridView1.ClearSelection();
                            dataGridView1.Rows[2].Cells[i + 1].Selected = true;
                            MyDefine.myXET.ShowWrongMsg("标定失败：标定数据 \"" + myStr1 + "\" 格式错误！" + Environment.NewLine + myStr1);
                            //MessageBox.Show("标定数据 \"" + myStr1 + "\" 格式错误！" + Environment.NewLine + myStr1, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            MyDefine.myXET.meTask = TASKS.run;
                            button1.Text = "设备标定";
                            return;
                        }
                        MyDefine.myXET.meHum_CalPoints[i] = (Int32)(myTemp * 100);
                    }

                    //核对标准湿度的温度点顺序是否正确：标定湿度点必须从小到大排列，如-80，-50，-20，5，25
                    for (int i = 0; i < MyDefine.myXET.meDotNum - 1; i++)
                    {
                        int x1 = MyDefine.myXET.meTemp_CalPoints[i];
                        int x2 = MyDefine.myXET.meTemp_CalPoints[i + 1];
                        if (x1 >= x2)       //温度点顺序排列错误
                        {
                            dataGridView1.ClearSelection();
                            dataGridView1.Rows[2].Cells[i + 1].Selected = true;
                            dataGridView1.Rows[2].Cells[i + 2].Selected = true;
                            MyDefine.myXET.ShowWrongMsg("标定失败：标准湿度必须从小到大排列！" + Environment.NewLine);
                            MyDefine.myXET.meTask = TASKS.run;
                            button1.Text = "设备标定";
                            return;
                        }
                    }
                }
                //核对设备温度的数量和格式是否正确
                for (int i = 0; i < MyDefine.myXET.meDUTAddrArr.Count; i++)
                {
                    if (!ret) break;
                    MyDefine.myXET.meActiveAddr = MyDefine.myXET.meDUTAddrArr[i];     //切换当前设备地址

                    //读取产品编号
                    ret = MyDefine.myXET.readJSN();
                    if (!ret) break;

                    int colNum = 0;                         //实际需要标定的列数(点数)
                    string jsn = MyDefine.myXET.meJSN.Substring(2);
                    int idx = MyDefine.myXET.meJSNList.IndexOf(jsn);   //获取当前产品JSN对应的文件索引
                    if (idx >= 1)                           //MyDefine.myXET.meTypeList[0]为"0",有效索引从1开始
                    {
                        String mytype = MyDefine.myXET.meTypeList[idx];     //获取产品类型
                        if (mytype == "TT_T") colNum = 8;                   //温度采集器
                        if (mytype == "TP_P") colNum = 3;                   //压力采集器
                        if (mytype == "TQ_T") colNum = 16;                   //温湿度采集器

                        //根据界面信息更新标定点数据
                        if(colNum == 16)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                myStr2 = Convert.ToString(dataGridView2.Rows[(idx - 1) * 6 + 2].Cells[j + 1].Value);            //设备标定ADC
                                if (Int32.TryParse(myStr2, out myADC) == false)     //标定ADC值非整数
                                {
                                    dataGridView2.ClearSelection();
                                    dataGridView2.Rows[(idx - 1) * 6 + 2].Cells[j + 1].Selected = true;
                                    MyDefine.myXET.ShowWrongMsg("标定失败：标定数据 \"" + myStr2 + "\" 格式错误！" + Environment.NewLine + myStr2);
                                    //MessageBox.Show("标定数据 \"" + myStr2 + "\" 格式错误！" + Environment.NewLine + myStr2, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    button1.Text = "设备标定";
                                    return;
                                }
                                MyDefine.myXET.meADC_CalPoints[j] = myADC;

                                //湿度
                                myStr2 = Convert.ToString(dataGridView2.Rows[(idx - 1) * 6 + 2 + 6].Cells[j + 1].Value);            //设备标定ADC
                                if (Int32.TryParse(myStr2, out myADC) == false)     //标定ADC值非整数
                                {
                                    dataGridView2.ClearSelection();
                                    dataGridView2.Rows[(idx - 1) * 6 + 4].Cells[j + 1].Selected = true;
                                    MyDefine.myXET.ShowWrongMsg("标定失败：标定数据 \"" + myStr2 + "\" 格式错误！" + Environment.NewLine + myStr2);
                                    //MessageBox.Show("标定数据 \"" + myStr2 + "\" 格式错误！" + Environment.NewLine + myStr2, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    button1.Text = "设备标定";
                                    return;
                                }
                                MyDefine.myXET.meADC1_CalPoints[j] = myADC;
                            }
                        }
                        else
                        {
                            for (int j = 0; j < colNum; j++)
                            {
                                myStr2 = Convert.ToString(dataGridView2.Rows[(idx - 1) * 6 + 2].Cells[j + 1].Value);            //设备标定ADC
                                if (Int32.TryParse(myStr2, out myADC) == false)     //标定ADC值非整数
                                {
                                    dataGridView2.ClearSelection();
                                    dataGridView2.Rows[(idx - 1) * 6 + 2].Cells[j + 1].Selected = true;
                                    MyDefine.myXET.ShowWrongMsg("标定失败：标定数据 \"" + myStr2 + "\" 格式错误！" + Environment.NewLine + myStr2);
                                    //MessageBox.Show("标定数据 \"" + myStr2 + "\" 格式错误！" + Environment.NewLine + myStr2, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    button1.Text = "设备标定";
                                    return;
                                }
                                MyDefine.myXET.meADC_CalPoints[j] = myADC;
                            }
                        }

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
                            ret = MyDefine.myXET.setDOT(colNum);
                        }
                    }
                    else if (idx == -1)
                    {
                        MyDefine.myXET.ShowWrongMsg("设备" + MyDefine.myXET.meJSN + "未加载对应文件数据！");
                        MyDefine.myXET.meTask = TASKS.run;
                        button1.Text = "设备标定";
                        return;
                    }
                }

                MyDefine.myXET.meTask = TASKS.run;
                MyDefine.myXET.meActiveAddr = activeDUTAddress;        //将当前设备地址切换回批量设置前的已连接设备地址
                MyDefine.myXET.meTips += "ReadREG_Device:" + Environment.NewLine;
                MyDefine.myXET.readDevice();                           //重新读取当前设备型号等参数

                if (!ret) MyDefine.myXET.ShowWrongMsg("批量标定失败！");
                if (ret) MyDefine.myXET.ShowCorrectMsg("批量标定成功！");
                if (ret) MyDefine.myXET.meDotComplete = true;               //标定成功，显示标定后曲线
                MyDefine.myXET.SaveCommunicationTips();                //将调试信息保存到操作日志
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }

            MyDefine.myXET.meTask = TASKS.run;
            button1.Text = "设备标定";
            dataGridView1.Focus();      //将焦点从button上移走，使button每次单击都有点击效果
        }

        #endregion

        #region 数据检索按钮 -- 根据读数时间检索对应的设备温度，并计算波动度、上下偏差

        private void button3_Click(object sender, EventArgs e)
        {
            dataGridView1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            if (!MyDefine.myXET.checkDataFileStatus()) return;  //尚未加载数据
            //if (!MyDefine.myXET.checkDataFileNumber()) return;  //数据文件数量与当前产品数量是否一致
            if (!MyDefine.myXET.checkDataFileType()) return;    //原始数据类型不一致(包含温度、湿度、压力数据中的一种以上)
            MyDefine.myXET.AddTraceInfo("数据检索");

            button3.Text = "检索中...";
            Application.DoEvents();

            //GenerateTable();    //根据加载的数据表重新生成表格
            dataRetrieval();    //数据检索

            button3.Text = "数据检索";
            dataGridView1.Focus();      //将焦点从button上移走，使button每次单击都有点击效果
        }

        #endregion

        #region 生成校准报告按钮 -- 切换到生成报告界面(跨窗体控制)

        //生成校准报告按钮
        private void button10_Click(object sender, EventArgs e)
        {
            dataGridView1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            MyDefine.myXET.switchMainPanel(4);                      //切换到校准报告界面
        }

        #endregion

        #region 生成曲线按钮 -- 切换到校准/标定曲线界面(跨窗体控制)

        private void button11_Click(object sender, EventArgs e)
        {
            dataGridView1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            GenerateCurveData();        //生成曲线数据表

            if (this.Name == "校准")
            {
                MyDefine.myXET.switchMainPanel(3);                    //切换到校准曲线图界面
            }

            if (this.Name == "标定")
            {
                MyDefine.myXET.switchMainPanel(12);                  //切换到标定曲线图界面
            }
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
            dataGridViewCellStyle1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.WhiteSmoke;                   //文字颜色
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;    //禁止改变列高度
            dataGridView.ColumnHeadersHeight = 40;  //设置列高

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
            //this.dataGridView1.ScrollBars = System.Windows.Forms.ScrollBars.None;   //禁止滚动条

            //dataGridView.ReadOnly = true;     //只读
            //dataGridView.RowTemplate.Height = 32;       //设置行高
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

        #region 单元格手动编辑触发函数 - 仅针对读数时间
        private void Tb_Enter(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentCell.RowIndex == 0)
            {
                if (((TextBox)sender).Text == "" || ((TextBox)sender).Text == null)
                {
                    ((TextBox)sender).Text = MyDefine.myXET.homstart;
                }
            }
        }
        #endregion

        #region 单元格手动编辑结束触发函数 - 仅针对读数时间
        //若单元格编辑结束，则触发(程序引起的内容变化不会触发)
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                string myStr = Convert.ToString(dataGridView1.CurrentCell.Value).Trim();
                if (myStr == "" || myStr == "Invalid") return;      //单元格为空、空格或Invalid

                if (e.RowIndex >= 1)  //设定/标准温度，判断是否是数字
                {
                    double myDouble;
                    if (Double.TryParse(myStr, out myDouble) == false)      //输入数字非整数或浮点数
                    {
                        dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.ForeColor = Color.Red;
                        return;
                    }
                    else
                    {
                        dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.ForeColor = Color.Black;
                    }
                }

                if (dataGridView1.CurrentCell.RowIndex == 0) //手动写入读数时间后，检查是否是时间格式，并将手动输入的日期改为标准年-月-日+时间格式
                {
                    DateTime myTime;
                    if (DateTime.TryParse(myStr, out myTime) == false)  //输入数据非时间格式
                    {
                        dataGridView1.CurrentCell.Style.ForeColor = Color.Red;
                        return;
                    }
                    else if (myStr.Contains("-") == false)              //仅输入了测试时间(无日期)，自动补齐原始数据的测试日期
                    {
                        string newTime = MyDefine.myXET.homdate + " " + myTime.ToString("HH:mm:ss");       //生成带测试日期的新时间字符串
                        //myTime = DateTime.ParseExact(newTime, "yyyy-MM-dd HH:mm:ss", null);                //将新日期转换为日期格式的字符串
                        DateTime.TryParse(newTime, out myTime);
                        dataGridView1.CurrentCell.Value = myTime.ToString("yy-MM-dd HH:mm:ss");            //将手动输入的日期改为标准年月日+时间格式
                        dataGridView1.CurrentCell.Style.ForeColor = Color.Black;
                    }
                    else                                                //手动输入了完整的日期+时间，则自动调整为统一的格式
                    {
                        dataGridView1.CurrentCell.Style.ForeColor = Color.Black;
                        dataGridView1.CurrentCell.Value = myTime.ToString("yy-MM-dd HH:mm:ss");    //将手动输入的日期改为标准年月日+时间格式
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

        #region 单元格内容变化结束触发函数 -- 针对设定温度、标准温度、设备温度
        //设定温度变化：检查数据格式是否正确
        //标准温度变化：将当前系统时间写入读数时间栏
        //设备温度变化：反推标定值(ADC值)

        //若单元格内容变化，则触发(程序引起的内容变化也会触发)
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
                                                                                                                                                                                                                                                                                                                                                                                                                                     try
            {
                var dgv = (DataGridView)sender;
                if (Convert.ToString(dgv.CurrentCell.Value).Contains("--")) dgv.CurrentCell.Value = dgv.CurrentCell.Value.ToString().Replace("--", "-");

                string myStr = Convert.ToString(dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value).Trim();
                if (myStr == "" || myStr == "Invalid") return;      //单元格为空、空格或Invalid

                if (e.RowIndex >= 1)                                //设定/标准温度写入，格式检查
                {
                    double myDouble1;
                    if (Double.TryParse(myStr, out myDouble1) == false)      //输入数字非整数或浮点数
                    {
                        dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.ForeColor = Color.Red;
                        return;
                    }
                    else
                    {
                        dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.ForeColor = Color.Black;
                    }

                    //dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = String.Format("{0:N3}", Convert.ToDouble(myStr));  //保留2位小数
                    //dataGridView1.Rows[e.RowIndex].Cells[3].Value = DateTime.Now.ToString("HH:mm:ss");   //将当前系统时间写入读数时间栏
                }

                /*
                if (e.ColumnIndex == 2)  //标准温度变化，将当前系统时间写入读数时间
                {
                    //myStr = Convert.ToString(dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                    double myDouble;
                    if (Double.TryParse(myStr, out myDouble) == false)      //输入数字非整数或浮点数
                    {
                        //MessageBox.Show("输入数字非整数或浮点数");
                        //dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "Invalid";
                        dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.ForeColor = Color.Red;
                        return;
                    }
                    else
                    {
                        dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.ForeColor = Color.Black;
                    }

                    //dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = String.Format("{0:N3}", Convert.ToDouble(myStr));  //保留2位小数
                    dataGridView1.Rows[e.RowIndex].Cells[3].Value = DateTime.Now.ToString("yy-MM-dd HH:mm:ss");   //将当前系统时间写入读数时间栏
                }
                */
                /*
                if (e.ColumnIndex == 4) //设备温度值变化,根据温度值反推ADC值
                {
                    int myTemp;
                    //string myStr = Convert.ToString(dataGridView1.Rows[e.RowIndex].Cells[4].Value);     //必须用Convert.ToString，用Value.ToString()会报错
                    if (Int32.TryParse(myStr, out myTemp) == false)  //输入数据非整数格式
                    {
                        //MessageBox.Show("输入数据非整数格式");
                        //dataGridView1.Rows[e.RowIndex].Cells[4].Value = "Invalid";
                        dataGridView1.Rows[e.RowIndex].Cells[4].Style.ForeColor = Color.Red;
                        return;
                    }
                    else
                    {
                        dataGridView1.Rows[e.RowIndex].Cells[4].Style.ForeColor = Color.Black;
                    }

                    dataGridView1.Rows[e.RowIndex].Cells[5].Value = calculateADCFromTemp(myTemp);   //根据设备温度反推标定ADC
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

        #region DataGridView数据输入限制

        //数据输入
        private void dataGridViewTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            //MessageBox.Show(e.KeyChar.ToString ());           
            //if (e.KeyChar == '：')
            //{
            //    e.KeyChar = ':';  //时间符号改为英文
            //}

            //只允许输入数字,负号,小数点和删除键,和空格(时间栏需要)
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != '.') && (e.KeyChar != ':') && (e.KeyChar != '-') && (e.KeyChar != 8) && (e.KeyChar != ' '))
            {
                e.Handled = true;
                return;
            }

            /*//时间栏里需要多次输入“-”号/ ':'号
            //时间符号只能输入一次
            if ((e.KeyChar == ':') && (((TextBox)sender).Text.IndexOf(':') != -1))
            {
                e.Handled = true;
                return;
            }
                        
            //负号只能输入一次
            if ((e.KeyChar == '-') && (((TextBox)sender).Text.IndexOf('-') != -1))
            {
                e.Handled = true;
                return;
            }
            */

            //第一位不能为小数点
            if ((e.KeyChar == '.') && (((TextBox)sender).Text.Length == 0))
            {
                e.Handled = true;
                return;
            }

            //小数点只能输入一次
            if ((e.KeyChar == '.') && (((TextBox)sender).Text.IndexOf('.') != -1))
            {
                e.Handled = true;
                return;
            }

            /*
            //正数第一位是0,第二位必须为小数点
            if ((e.KeyChar != '.') && (e.KeyChar != 8) && (((TextBox)sender).Text == "0"))
            {
                e.Handled = true;
                return;
            }
            */
        }

        #endregion

        #region DataGridView按键触发事件

        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            //MessageBox.Show(sender.ToString());
            var cb = e.Control as ComboBox;

            if (cb != null)
            {
                cb.MaxLength = 2;
                cb.DropDownStyle = ComboBoxStyle.DropDown;
                cb.AutoCompleteMode = AutoCompleteMode.Suggest;
                cb.AutoCompleteSource = AutoCompleteSource.ListItems;
            }

            if (e.Control is DataGridViewTextBoxEditingControl)
            {
                var dgv = (DataGridView)sender;

                var tb = (DataGridViewTextBoxEditingControl)e.Control;

                //解除事件
                tb.KeyPress -= dataGridViewTextBox_KeyPress;

                if(dgv.CurrentCell.RowIndex == 0)
                {
                    tb.Enter += Tb_Enter;
                }

                //需要添加事件的行(>= 0 全部添加事件)
                if (dgv.CurrentCell.RowIndex >= 0)
                {
                    //事件追加
                    tb.KeyPress += dataGridViewTextBox_KeyPress;
                }

                //需要添加事件的列
                //if (dgv.CurrentCell.ColumnIndex == 2 || dgv.CurrentCell.ColumnIndex == 3)
                //{
                    //事件追加
                //    tb.KeyPress += dataGridViewTextBox_KeyPress;
                //}
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

        #region dataGridView随窗体大小变化

        private void dataGridView1_SizeChanged(object sender, EventArgs e)
        {
            dadataGridViewAutoResize();         //重新分配dadataGridView行高列宽
        }

        /// <summary>
        /// dataGridView自动分配行高列宽
        /// </summary>
        public void dadataGridViewAutoResize()
        {
            dataGridView1.ClearSelection(); //清除单元格选中状态(放在添加列后才起作用)  
            dataGridView2.ClearSelection(); //清除单元格选中状态(放在添加列后才起作用)   
            SetColumnWidthScale(dataGridView1, colWidth);    //设置列宽比例(必须设置dataGridView1的列宽，否则最大最小化时列宽会异常)
            //SetColumnWidthScale(dataGridView2, colWidth);    //设置列宽比例(不设置dataGridView2的列宽，否则最大最小化时列宽会异常)
            dataGridViewHeightResize();     //根据dataGridView的高度重新分配行高
        }

        //设置列宽比例(only works in fill mode)
        private void SetColumnWidthScale(DataGridView myDGV, float[] widthScaleArray)
        {
            myDGV.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;   //自动填满整个dataGridView宽度
            int colNum = widthScaleArray.Length >= myDGV.Columns.Count ? myDGV.Columns.Count : widthScaleArray.Length;  //select the smaller one

            for (int i = 0; i < colNum; i++)    //set the columns' width scale
            {
                myDGV.Columns[i].FillWeight = widthScaleArray[i];
            }
        }

        //根据dataGridView的高度重新分配行高
        private void dataGridViewHeightResize()
        {
            try
            {
                int newHeight = Convert.ToInt32(dataGridView1.Height / (dataGridView1.Rows.Count + 2));  //标题栏占2倍行高
                int headerHeight = dataGridView1.Height - newHeight * dataGridView1.Rows.Count;  //设置标题栏高度
                //dataGridView1.RowTemplate.Height = newHeight;
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    dataGridView1.Rows[i].Height = newHeight;   //设置行高
                }
                if (headerHeight >= 4) dataGridView1.ColumnHeadersHeight = headerHeight;    //设置标题栏高度(高度<4会报错)
                //dataGridView1.Height = newHeight * (dataGridView1.Rows.Count+1);      //设置dataGridView总高度
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }            
        }

        #endregion

        #region 数据检索

        private void dataRetrieval()
        {
            try
            {
                if (MyDefine.myXET.meDataTbl == null)
                {
                    MessageBox.Show("尚未加载数据！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                #region 变量定义
                
                string myStr;
                DateTime myStartTime = MyDefine.myXET.meStartTime;
                DateTime myStopTime = MyDefine.myXET.meStopTime;
                Int32 mySpanTime = MyDefine.myXET.meSpanTime;       //测试间隔时间(秒)，用于计算读数时间对应的设备温度的index
                List<List<Double>> RowList = new List<List<Double>>();
                List<DateTime> ReadTimeList = new List<DateTime>();
                ReadTimeList.Add(DateTime.MinValue);                //给以下ReadTimeList添加一个空值，使其与meTypeList、meDataTbl等结构一致
                dataGridView1.ClearSelection();

                #endregion

                #region 核对输入信息正确性

                //=====核对读数时间正确性--根据读数时间检索设备温度====================================================================================
                for (int i = 1; i <= 8; i++)        //共8个标定点
                {
                    DateTime myReadTime = DateTime.MinValue;
                    myStr = Convert.ToString(dataGridView1.Rows[0].Cells[i].Value).Trim();     //读数时间

                    if (myStr != "")           //如果是空的则忽略，不为空则报错并退出
                    {
                        if (DateTime.TryParse(myStr, out myReadTime) == false)  //读数时间非时间格式  -- 将读数时间转换为时间格式并保存在myTime中
                        {
                            dataGridView1.Rows[0].Cells[i].Selected = true;     //选中当前单元格
                            MyDefine.myXET.ShowWrongMsg("Error：读数时间非时间格式！");
                            return;
                        }

                        if (DateTime.Compare(myReadTime, myStartTime) < 0 || DateTime.Compare(myReadTime, myStopTime) > 0)   //测试时间小于0或大于最大测试持续时间
                        {
                            string msg = "开始时间：" + myStartTime.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine;
                            msg += "结束时间：" + myStopTime.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine;
                            msg += "读数时间：" + myReadTime.ToString("yyyy -MM-dd HH:mm:ss") + Environment.NewLine;
                            MyDefine.myXET.ShowWrongMsg(msg + "Error：读数时间超出开始-结束时间范围！");
                            //MessageBox.Show(msg + "Error：读数时间超出开始-结束时间范围！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            dataGridView1.Rows[0].Cells[i].Selected = true;     //选中当前单元格
                            return;
                        }

                        if (DateTime.Compare(myReadTime, DateTime.Now) > 0)   //读数时间大于当前系统时间
                        {
                            MyDefine.myXET.ShowWrongMsg("Error：读数时间大于当前系统时间！");
                            //MessageBox.Show("Error：读数时间大于当前系统时间！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            dataGridView1.Rows[0].Cells[i].Selected = true;     //选中当前单元格
                            return;
                        }
                    }
                    ReadTimeList.Add(myReadTime);
                }

                //=====核对设定/标准值正确性--设定值用于计算波动度等，标准值用于设备标定====================================================================================
                for (int i = 1; i < dataGridView1.Rows.Count; i++)      //设定/标准温湿度
                {
                    List<Double> myList = new List<Double>();           //一行数据一个list
                    myList.Add(0);                                      //给myList添加一个空值，使其与meTypeList、meDataTbl等结构一致
                    for (int j = 1; j <= 8; j++)                        //共8个标定点
                    {
                        myStr = Convert.ToString(dataGridView1.Rows[i].Cells[j].Value).Trim();     //设定温度或标准温度
                        if (myStr == "") { myList.Add(double.MinValue); continue; }

                        double temp = double.MinValue;                  //设定温度或标准温度
                        if (double.TryParse(myStr, out temp) == false)  //非数字格式  -- 将温度转换为double格式并保存在temp中
                        {
                            MyDefine.myXET.ShowWrongMsg("单元格内容非数字格式！");
                            //MessageBox.Show("单元格内容非数字格式！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            dataGridView1.Rows[i].Cells[j].Selected = true;     //选中当前单元格
                            return;
                        }
                        myList.Add(temp);
                    }
                    RowList.Add(myList);
                }

                #endregion

                #region 数据检索

                MyDefine.myXET.meTips = "[DATA RETRIEVAL]" + Environment.NewLine;
                for (int i = 1; i < MyDefine.myXET.meTypeList.Count; i++)
                {
                    int row = i - 1;
                    for (int j = 1; j <= 8; j++)        //共8个标定点
                    {
                        if (ReadTimeList[j] == DateTime.MinValue) continue;             //读数时间为空，略过

                        //根据读数时间检索相应的设备温度===================================================================================
                        TimeSpan totalTime = ReadTimeList[j].Subtract(myStartTime);     //开始测试到客户读数时间的测试总时长
                        Int32 totalDuartion = (Int32)(totalTime.TotalSeconds);          //测试开始到读数时的总秒数
                        Int32 index = (Int32)(totalDuartion / mySpanTime);              //计算数据在meDataTbl中的索引

                        string mydata = MyDefine.myXET.meDataTbl.GetCellValue(index, i);//当前读数时间对应的设备温度
                        if (mydata == "") continue;                                     //当前单元格无数据，忽略

                        dataGridView2.Rows[row * 6 + 1].Cells[j].Value = mydata;                          //将检索到的值填入单元格
                        if (this.Name == "标定")          //非温湿度采集器，计算标定值
                        {
                            dataGridView2.Rows[row * 6 + 2].Cells[j].Value = calculateADCFromTemp(mydata, MyDefine.myXET.meTypeList[i]);//根据设备温度反推标定ADC
                        }

                        //根据设定温度计算波动度等数据=====================================================================================
                        //当前index向前30分钟时间段内的测试数据波动、最大值、最小值
                        int desRow = 1, stdRow;                                                     //设定(标准)温度或压力所在行
                        if (MyDefine.myXET.meTypeList[i].Contains("TH_H")) { desRow = 3; stdRow = 4; }  //设定(标准)湿度所在行
                        if (RowList[desRow][j] <= double.MinValue + 10) continue;                       //设定值为空，不计算波动度

                        MyDefine.myXET.meTips += "序号" + (i + 1) + "：" + Environment.NewLine;
                        Int32 dataNum = 30 * 60 / mySpanTime;    //30分钟内的读数个数
                        Int32 idxNum = dataNum / SZ.CHA;         //30分钟数据注myMem中的索引数
                        Double dataMax = double.MinValue;
                        Double dataMin = double.MaxValue;

                        //检索最大值和最小值
                        for (int m = index; m > index - dataNum; m--)
                        {
                            if (m < 0) break;   //超出索引范围
                            
                            for (int n = 0; n < SZ.CHA; n++)
                            {
                                string strdata = MyDefine.myXET.meDataTbl.GetCellValue(m, i);
                                if (strdata == "") continue;

                                Double data = Convert.ToDouble(strdata);
                                if (data > dataMax) dataMax = data;
                                if (data < dataMin) dataMin = data;
                                //MyDefine.myXET.meTips += data.ToString();     //加此语句后，执行检索后dataGridView1会非常卡，原因不明
                            }
                        }

                        MyDefine.myXET.meTips += "共检索数据：" + dataNum.ToString() + Environment.NewLine;
                        MyDefine.myXET.meTips += "Tmax = " + dataMax.ToString() + ",Tmin = " + dataMin.ToString() + Environment.NewLine + Environment.NewLine;

                        //MessageBox.Show(((dataMax - dataMin) / 2).ToString() + "," +dataMax.ToString() + "," + dataMin.ToString());
                        //dataGridView1.Rows[i].Cells[6].Value = ((double)(dataMax - dataMin) / 2).ToString("±" + ("#0.#0") + "%");   //波动度=(实测最大温度-实测最小温度)/2*100%
                        dataGridView2.Rows[row * 6 + 3].Cells[j].Value = ((dataMax - dataMin) / 2).ToString("±" + ("#0.#0"));   //波动度=(实测最大温度-实测最小温度)/2
                        dataGridView2.Rows[row * 6 + 4].Cells[j].Value = (dataMax - RowList[desRow][j]).ToString("F2");   //上偏差=实测温度最大值dataMax-设备设定温度
                        dataGridView2.Rows[row * 6 + 5].Cells[j].Value = (dataMin - RowList[desRow][j]).ToString("F2");   //上偏差=实测温度最小值dataMin-设备设定温度
                    }
                }

                #endregion

                //DrawCurve();   //绘图
                //GenerateCurveData();                     //生成绘图数据表
                MyDefine.myXET.ShowCorrectMsg("数据检索成功");
                MyDefine.myXET.SaveCommunicationTips();  //将调试信息保存到操作日志
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("数据检索失败：" + ex.ToString());
                MyDefine.myXET.SaveCommunicationTips();  //将调试信息保存到操作日志
            }
        }

        //根据设备温度/压力反推需标定的ADC值
        public int calculateADCFromTemp(String temp, String type)
        {
            Int32 myTemp = Convert.ToInt32(Convert.ToDouble(temp) * 100);
            Int32 myADC = 0;
            switch (MyDefine.myXET.meType)
            {
                case DEVICE.HTT:
                    myADC = getADCVal_HTT(myTemp);
                    break;
                case DEVICE.HTH:
                    break;
                case DEVICE.HTP:
                    myADC = getADCVal_HTP(myTemp);
                    break;
                case DEVICE.HTQ:
                    if(type == "TQ_T") myADC = getADCVal_HTT(myTemp);
                    else if(type == "TQ_H") myADC = getADCVal_HTQ(myTemp);
                    break;
                default:
                    break;
            }

            return myADC;
        }

        /****************************************************************************************
         * 
         * [根据温度值反推温度标定值(ADC值)]
         * 
         * 产品出厂默认标定值如下：
         *                           [0]    [1]   [2]    [3]     [4]    [5]     [6]     [7]     
         * meTemp_CalPoints[0]->[7]：-8000  -5000  -2000  0℃    2500   6000    9000    12000
         *  meADC_CalPoints[0]->[7]：56934  66751  76567  84748  91127  102203  111696  121190
         *  
         * 假设设备在-80℃时的实测温度为-73，标准器温度为-79.73，则对-80℃标定过程如下：
         * a. 两点确定一条曲线：(x1,y1)=(-8000,56934)  
         *                      (x2,y2)=(-5000,66751)
         *                      则，斜率k=(y2-y1)/(x2-x1)=(66751-56934)/(-5000-(-8000))=3.27233
         *                      则，曲线y=k(x-x1)+y1
         * 则-73℃对应的ADC值=k(x-x1)+y1=3.27233*(-7300-(-8000))+56934=59224.6333≈59225
         * 则-80℃时的标定数据应为(-7973,59225)
         * 
        *****************************************************************************************/
        public int getADCVal_HTT(int myTemp)
        {
            try
            {
                Int32 myADC = 0;
                if (myTemp > MyDefine.myXET.meTemp_CalPoints[7])    // >120度
                {
                    myADC = Convert.ToInt32(MyDefine.myXET.meADC_CalPoints[7] + (myTemp - MyDefine.myXET.meTemp_CalPoints[7]) * MyDefine.myXET.meV_Slope[7]);
                }
                else if (myTemp > MyDefine.myXET.meTemp_CalPoints[6])    //90-120
                {
                    myADC = Convert.ToInt32(MyDefine.myXET.meADC_CalPoints[6] + (myTemp - MyDefine.myXET.meTemp_CalPoints[6]) * MyDefine.myXET.meV_Slope[6]);
                }
                else if (myTemp > MyDefine.myXET.meTemp_CalPoints[5])   //60-90
                {
                    myADC = Convert.ToInt32(MyDefine.myXET.meADC_CalPoints[5] + (myTemp - MyDefine.myXET.meTemp_CalPoints[5]) * MyDefine.myXET.meV_Slope[5]);
                }
                else if (myTemp > MyDefine.myXET.meTemp_CalPoints[4])   //25-60
                {
                    myADC = Convert.ToInt32(MyDefine.myXET.meADC_CalPoints[4] + (myTemp - MyDefine.myXET.meTemp_CalPoints[4]) * MyDefine.myXET.meV_Slope[4]);
                }
                else if (myTemp > MyDefine.myXET.meTemp_CalPoints[3])   //5-25
                {
                    myADC = Convert.ToInt32(MyDefine.myXET.meADC_CalPoints[3] + (myTemp - MyDefine.myXET.meTemp_CalPoints[3]) * MyDefine.myXET.meV_Slope[3]);
                }
                else if (myTemp > MyDefine.myXET.meTemp_CalPoints[2])   //-20~5
                {
                    myADC = Convert.ToInt32(MyDefine.myXET.meADC_CalPoints[2] + (myTemp - MyDefine.myXET.meTemp_CalPoints[2]) * MyDefine.myXET.meV_Slope[2]);
                }
                else if (myTemp > MyDefine.myXET.meTemp_CalPoints[1])   //-50~-20
                {
                    myADC = Convert.ToInt32(MyDefine.myXET.meADC_CalPoints[1] + (myTemp - MyDefine.myXET.meTemp_CalPoints[1]) * MyDefine.myXET.meV_Slope[1]);
                }
                else if (myTemp > MyDefine.myXET.meTemp_CalPoints[0])   //-80~-50
                {
                    myADC = Convert.ToInt32(MyDefine.myXET.meADC_CalPoints[0] + (myTemp - MyDefine.myXET.meTemp_CalPoints[0]) * MyDefine.myXET.meV_Slope[0]);
                }
                else                                                    //<-80度
                {
                    myADC = Convert.ToInt32(MyDefine.myXET.meADC_CalPoints[0] + (myTemp - MyDefine.myXET.meTemp_CalPoints[0]) * MyDefine.myXET.meV_Slope[0]);
                }

                return myADC;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
                return -1;
            }
        }

        //根据湿度值反推湿度标定值(ADC值)
        public int getADCVal_HTQ(int myTemp)
        {
            try
            {
                Int32 myADC = 0;
                if (myTemp > MyDefine.myXET.meHum_CalPoints[7])    // >120度
                {
                    myADC = Convert.ToInt32(MyDefine.myXET.meADC1_CalPoints[7] + (myTemp - MyDefine.myXET.meHum_CalPoints[7]) * MyDefine.myXET.meV1_Slope[7]);
                }
                else if (myTemp > MyDefine.myXET.meHum_CalPoints[6])    //90-120
                {
                    myADC = Convert.ToInt32(MyDefine.myXET.meADC1_CalPoints[6] + (myTemp - MyDefine.myXET.meHum_CalPoints[6]) * MyDefine.myXET.meV1_Slope[6]);
                }
                else if (myTemp > MyDefine.myXET.meHum_CalPoints[5])   //60-90
                {
                    myADC = Convert.ToInt32(MyDefine.myXET.meADC1_CalPoints[5] + (myTemp - MyDefine.myXET.meHum_CalPoints[5]) * MyDefine.myXET.meV1_Slope[5]);
                }
                else if (myTemp > MyDefine.myXET.meHum_CalPoints[4])   //25-60
                {
                    myADC = Convert.ToInt32(MyDefine.myXET.meADC1_CalPoints[4] + (myTemp - MyDefine.myXET.meHum_CalPoints[4]) * MyDefine.myXET.meV1_Slope[4]);
                }
                else if (myTemp > MyDefine.myXET.meHum_CalPoints[3])   //5-25
                {
                    myADC = Convert.ToInt32(MyDefine.myXET.meADC1_CalPoints[3] + (myTemp - MyDefine.myXET.meHum_CalPoints[3]) * MyDefine.myXET.meV1_Slope[3]);
                }
                else if (myTemp > MyDefine.myXET.meHum_CalPoints[2])   //-20~5
                {
                    myADC = Convert.ToInt32(MyDefine.myXET.meADC1_CalPoints[2] + (myTemp - MyDefine.myXET.meHum_CalPoints[2]) * MyDefine.myXET.meV1_Slope[2]);
                }
                else if (myTemp > MyDefine.myXET.meHum_CalPoints[1])   //-50~-20
                {
                    myADC = Convert.ToInt32(MyDefine.myXET.meADC1_CalPoints[1] + (myTemp - MyDefine.myXET.meHum_CalPoints[1]) * MyDefine.myXET.meV1_Slope[1]);
                }
                else if (myTemp > MyDefine.myXET.meHum_CalPoints[0])   //-80~-50
                {
                    myADC = Convert.ToInt32(MyDefine.myXET.meADC1_CalPoints[0] + (myTemp - MyDefine.myXET.meHum_CalPoints[0]) * MyDefine.myXET.meV1_Slope[0]);
                }
                else                                                    //<-80度
                {
                    myADC = Convert.ToInt32(MyDefine.myXET.meADC1_CalPoints[0] + (myTemp - MyDefine.myXET.meHum_CalPoints[0]) * MyDefine.myXET.meV1_Slope[0]);
                }

                return myADC;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
                return -1;
            }
        }

        //根据压力值反推压力标定值(ADC值)
        public int getADCVal_HTP(int myTemp)
        {
            try
            {
                Int32 myADC = 0;
                if (myTemp > MyDefine.myXET.meTemp_CalPoints[2])   //5-25
                {
                    myADC = Convert.ToInt32(MyDefine.myXET.meADC_CalPoints[2] + (myTemp - MyDefine.myXET.meTemp_CalPoints[2]) * MyDefine.myXET.meV_Slope[2]);
                }
                else if (myTemp > MyDefine.myXET.meTemp_CalPoints[1])   // -20~5
                {
                    myADC = Convert.ToInt32(MyDefine.myXET.meADC_CalPoints[1] + (myTemp - MyDefine.myXET.meTemp_CalPoints[1]) * MyDefine.myXET.meV_Slope[1]);
                }
                else if (myTemp > MyDefine.myXET.meTemp_CalPoints[0])   //-50~-20
                {
                    myADC = Convert.ToInt32(MyDefine.myXET.meADC_CalPoints[0] + (myTemp - MyDefine.myXET.meTemp_CalPoints[0]) * MyDefine.myXET.meV_Slope[0]);
                }
                else                  //-80~-50
                {
                    myADC = Convert.ToInt32(MyDefine.myXET.meADC_CalPoints[0] + (myTemp - MyDefine.myXET.meTemp_CalPoints[0]) * MyDefine.myXET.meV_Slope[0]);
                }

                return myADC;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
                return -1;
            }            
        }
        #endregion

        #region 生成新的空检索表(根据原始文件类型列表)

        public void GenerateTable()
        {
            if (MyDefine.myXET.meTypeList.Count == 0) return;

            #region 生成表1
            
            MyDefine.myXET.meTblCal1.ClearTableData();
            if(MyDefine.myXET.meTypeList[1] == "TH_T" || MyDefine.myXET.meTypeList[1] == "TQ_T")
            {
                MyDefine.myXET.meTblCal1.AddTableRow("读数时间");
                MyDefine.myXET.meTblCal1.AddTableRow("设定温度（" + MyDefine.myXET.temUnit + "）");
                MyDefine.myXET.meTblCal1.AddTableRow("标准温度（" + MyDefine.myXET.temUnit + "）");
                MyDefine.myXET.meTblCal1.AddTableRow("设定湿度（%RH）");
                MyDefine.myXET.meTblCal1.AddTableRow("标准湿度（%RH）");
            }
            else if (MyDefine.myXET.meTypeList[1] == "TT_T")
            {
                MyDefine.myXET.meTblCal1.AddTableRow("读数时间");
                MyDefine.myXET.meTblCal1.AddTableRow("设定温度（" + MyDefine.myXET.temUnit + "）");
                MyDefine.myXET.meTblCal1.AddTableRow("标准温度（" + MyDefine.myXET.temUnit + "）");
            }
            else if (MyDefine.myXET.meTypeList[1] == "TP_P")
            {
                MyDefine.myXET.meTblCal1.AddTableRow("读数时间");
                MyDefine.myXET.meTblCal1.AddTableRow("设定压力（KPa）");
                MyDefine.myXET.meTblCal1.AddTableRow("标准压力（KPa）");
            }

            MyDefine.myXET.meTblPre1.dataTable = MyDefine.myXET.meTblCal1.CopyTable();  //校准表
            MyDefine.myXET.meTblPre1.dataTable.Columns[0].ColumnName = "校准点数";
            dataGridView1.DataSource = null;                                            //要先将数据源设置为null,否则有时候单元格宽度会异常
            dataGridView1.DataSource = MyDefine.myXET.meTblCal1.dataTable;

            #endregion

            #region 生成表2

            MyDefine.myXET.meTblCal2.ClearTableData();
            dataTableClass blankTbl = new dataTableClass();
            blankTbl.dataTable = MyDefine.myXET.meTblCal2.CopyTable();

            for (int i = 1; i < MyDefine.myXET.meTypeList.Count; i++)   //meTypeList[0]为空
            {
                String type = "设备温度（" + MyDefine.myXET.temUnit + "）";
                if (MyDefine.myXET.meTypeList[i] == "TH_H" || MyDefine.myXET.meTypeList[i] == "TQ_H") type = "设备湿度（%RH）";
                if (MyDefine.myXET.meTypeList[i] == "TP_P") type = "设备压力（KPa）";

                String code = MyDefine.myXET.meJSNList[i];
                dataTableClass mytable = new dataTableClass();
                mytable.dataTable = blankTbl.CopyTable();
                //mytable.AddTableRow(new String[] { code, "1", "2", "3", "4", "5", "6", "7", "8" });
                mytable.AddTableRow(new String[] { code, "P1", "P2", "P3", "P4", "P5", "P6", "P7", "P8" });
                //mytable.AddTableRow(code);
                mytable.AddTableRow(type);
                mytable.AddTableRow("标定值");
                mytable.AddTableRow("波动度");
                mytable.AddTableRow("上偏差");
                mytable.AddTableRow("下偏差");
                MyDefine.myXET.meTblCal2.dataTable.Merge(mytable.dataTable);    //合并数据表
                //MyDefine.myXET.meDataSetCal.Tables.Add(mytable.dataTable);      //添加进数据表集合
            }

            MyDefine.myXET.meTblPre2.dataTable = MyDefine.myXET.meTblCal2.CopyTable();  //校准表
            MyDefine.myXET.meTblPre2.dataTable.Columns[0].ColumnName = "校准点数";
            dataGridView2.DataSource = null;                                            //要先将数据源设置为null,否则有时候单元格宽度会异常
            dataGridView2.DataSource = MyDefine.myXET.meTblCal2.dataTable;

            #endregion

            DisableGridViewSort();  //禁止列排序

            //重新设置带出厂编号行的背景色
            for (int i = 0; i < MyDefine.myXET.meTypeList.Count - 1; i++) 
            {
                dataGridView2.Rows[i * 6].DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(177, 183, 204);    //行背景色
            }

            if (this.Name == "校准")
            {
                //隐藏标定值列
                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                {
                    String rowname = dataGridView2.Rows[i].Cells[0].Value.ToString();
                    if (rowname == "标定值") dataGridView2.Rows[i].Visible = false;
                }
            }

            dataGridView2.ClearSelection(); //清除单元格选中状态(放在添加列后才起作用)
        }

        #endregion

        #region 生成曲线数据表(根据当前界面信息)

        public void GenerateCurveData()
        {
            try
            {
                String myStr;
                Int32 colnum = 0;
                MyDefine.myXET.meTblPreCurve = new dataTableClass();    //创建曲线数据表对象(表可以为空，但必须有实列，否则进入画图界面会出现异常)
                MyDefine.myXET.meTblCalCurve = new dataTableClass();    //创建曲线数据表对象(表可以为空，但必须有实列，否则进入画图界面会出现异常)
                if (MyDefine.myXET.meDataTbl == null) return;

                #region 生成曲线原始数据

                //曲线数据表与原始数据表的表头一致，在此基础上增加标准值列(标定成功后再增加标定后列--其数据同标准值列)
                dataTableClass mytable = new dataTableClass();
                mytable.dataTable = MyDefine.myXET.meDataTbl.CopyTable();//第0列为空，在最后增加标准数据列，使其结构与meDataTbl一致
                List<String> myList = new List<String>(MyDefine.myXET.meTypeList.ToArray());  //存储各数据列的数据类型(首先将meTypeList整个复制过去，其次根据原始数据类型添加1个或2个标准数据的类型)

                mytable.ClearTableData();                               //清空数据，只保留表头
                if (MyDefine.myXET.meTypeList[1].Contains("TH"))        //温湿度验证设备
                {
                    myList.Add("TH_H");
                    myList.Add("TH_T");
                    mytable.addTableColumn("标准湿度");
                    mytable.addTableColumn("标准温度");
                }
                else if (MyDefine.myXET.meTypeList[1].Contains("TQ_T"))
                {
                    myList.Add("TQ_H");
                    myList.Add("TQ_T");
                    mytable.addTableColumn("标准湿度");
                    mytable.addTableColumn("标准温度");
                }
                else if (MyDefine.myXET.meTypeList[1].Contains("TT_T")) //温度验证设备
                {
                    myList.Add("TT_T");
                    mytable.addTableColumn("标准温度");
                }
                else if (MyDefine.myXET.meTypeList[1].Contains("TP_P")) //压力验证设备
                {
                    myList.Add("TP_P");
                    mytable.addTableColumn("标准压力");
                }

                //添加标准温度/压力列的数据(最后一列)
                colnum = mytable.dataTable.Columns.Count - 1;
                for (int i = 1; i <= 8; i++)                            //注意第一列是行名称
                {
                    myStr = Convert.ToString(dataGridView1.Rows[2].Cells[i].Value);     //标准温度/压力
                    if (double.TryParse(myStr, out double temp) == false) myStr = "";   //单元格内容非数字，按空字符串处理

                    mytable.AddTableRow();                                              //添加新行
                    mytable.SetCellValue(i - 1, colnum, myStr);                         //将值写入数据表
                }

                //添加标准湿度列的数据(倒数第二列)
                colnum = mytable.dataTable.Columns.Count - 2;
                if (MyDefine.myXET.meTypeList[1].Contains("TH") || MyDefine.myXET.meTypeList[1].Contains("TQ_T"))
                {
                    for (int i = 1; i <= 8; i++)                            //注意第一列是行名称
                    {
                        myStr = Convert.ToString(dataGridView1.Rows[4].Cells[i].Value);     //标准温度/压力
                        if (double.TryParse(myStr, out double temp) == false) myStr = "";   //单元格内容非数字，按空字符串处理

                        mytable.SetCellValue(i - 1, colnum, myStr);                         //将值写入数据表
                    }
                }

                //添加各设备的设备测试数据列(检索值)
                for (int i = 1; i < MyDefine.myXET.meTypeList.Count; i++)
                {
                    for (int j = 1; j <= 8; j++)
                    {
                        myStr = Convert.ToString(dataGridView2.Rows[(i - 1) * 6 + 1].Cells[j].Value);    //设备温度
                        if (double.TryParse(myStr, out double temp) == false) myStr = "";   //单元格内容非数字，按空字符串处理

                        mytable.SetCellValue(j - 1, i, myStr);                              //将值写入数据表
                    }
                }

                #endregion

                #region 标定成功后，增加标定后曲线

                //标定成功后，增加标定后曲线
                if (this.Name == "标定" && MyDefine.myXET.meDotComplete == true)
                {
                    myList.Add(myList[1]);
                    mytable.addTableColumn("标定后曲线");

                    //添加标准温度/压力列的数据(最后一列)
                    colnum = mytable.dataTable.Columns.Count - 1;
                    for (int i = 1; i <= 8; i++)                            //注意第一列是行名称
                    {
                        myStr = Convert.ToString(dataGridView1.Rows[2].Cells[i].Value);     //标准温度/压力
                        if (double.TryParse(myStr, out double temp) == false) myStr = "";   //单元格内容非数字，按空字符串处理

                        mytable.AddTableRow();                                              //添加新行
                        mytable.SetCellValue(i - 1, colnum, myStr);                         //将值写入数据表
                    }
                    if(MyDefine.myXET.meTypeList[1].Contains("TH") || MyDefine.myXET.meTypeList[1].Contains("TQ_T"))
                    {
                        myList.Add(myList[2]);
                        mytable.setColumnName(5, "温度标定后曲线");
                        mytable.addTableColumn("湿度标定后曲线");

                        //添加标准温度/压力列的数据(最后一列)
                        colnum = mytable.dataTable.Columns.Count - 1;
                        for (int i = 1; i <= 8; i++)                            //注意第一列是行名称
                        {
                            myStr = Convert.ToString(dataGridView1.Rows[4].Cells[i].Value);     //标准湿度
                            if (double.TryParse(myStr, out double temp) == false) myStr = "";   //单元格内容非数字，按空字符串处理

                            mytable.AddTableRow();                                              //添加新行
                            mytable.SetCellValue(i - 1, colnum, myStr);                         //将值写入数据表
                        }
                    }
                }

                #endregion

                MyDefine.myXET.meCalTypeList = myList;        //生成信息数据类型列表，用于绘制曲线(比meTypeList多1或两个标准数据的类型)
                if (this.Name == "校准") MyDefine.myXET.meTblPreCurve.dataTable = mytable.CopyTable();
                if (this.Name == "标定") MyDefine.myXET.meTblCalCurve.dataTable = mytable.CopyTable();

            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        public void GenerateCurveData0()
        {
            try
            {
                int colnum = 0;
                String myStr;

                //曲线数据表与原始数据表的表头一致
                dataTableClass mytable = new dataTableClass();
                mytable.dataTable = MyDefine.myXET.meDataTbl.CopyTable();//第0列为空，在最后增加标准数据列，使其结构与meDataTbl一致
                List<String> myList = new List<String>(MyDefine.myXET.meTypeList.ToArray());  //存储各数据列的数据类型(首先将meTypeList整个复制过去，其次根据原始数据类型添加1个或2个标准数据的类型)

                if (MyDefine.myXET.meTypeList[1].Contains("TH") || MyDefine.myXET.meTypeList[1].Contains("TQ_T"))
                {
                    myList.Add("TH_T");
                    myList.Add("TH_H");
                    mytable.addTableColumn("标准温度");
                    mytable.addTableColumn("标准湿度");
                }
                else if (MyDefine.myXET.meTypeList[1].Contains("TT_T"))
                {
                    myList.Add("TT_T");
                    mytable.addTableColumn("标准温度");
                }
                else if (MyDefine.myXET.meTypeList[1].Contains("TP_P"))
                {
                    myList.Add("TP_P");
                    mytable.addTableColumn("标准压力");
                }
                mytable.ClearTableData();           //清空数据，只保留表头

                //添加标准温度/压力列的数据(最后一列)
                colnum = mytable.dataTable.Columns.Count - 1;
                for (int i = 1; i <= 8; i++)                            //注意第一列是行名称
                {
                    myStr = Convert.ToString(dataGridView1.Rows[2].Cells[i].Value);     //标准温度/压力
                    mytable.AddTableRow();                              //添加新行
                    mytable.SetCellValue(i - 1, colnum, myStr);             //将值写入数据表
                }

                //添加标准湿度列的数据(倒数第二列)
                if (MyDefine.myXET.meTypeList[1].Contains("TH") || MyDefine.myXET.meTypeList[1].Contains("TQ_T"))
                {
                    colnum = mytable.dataTable.Columns.Count - 2;
                    for (int i = 1; i <= 8; i++)                            //注意第一列是行名称
                    {
                        myStr = Convert.ToString(dataGridView1.Rows[4].Cells[i].Value);     //标准温度/压力
                        mytable.SetCellValue(i - 1, colnum, myStr);             //将值写入数据表
                    }
                }

                //添加各设备的设备温度数据列
                for (int i = 1; i < MyDefine.myXET.meTypeList.Count; i++)
                {
                    for (int j = 1; j <= 8; j++)
                    {
                        myStr = Convert.ToString(dataGridView2.Rows[(i - 1) * 6 + 1].Cells[j].Value);    //设备温度
                        mytable.SetCellValue(j - 1, i, myStr);                          //将值写入数据表
                    }
                }

                if (this.Name == "校准")
                {
                    MyDefine.myXET.meTblPreCurve = new dataTableClass();
                    MyDefine.myXET.meTblPreCurve.dataTable = mytable.CopyTable();
                    MyDefine.myXET.meCalTypeList = myList;                      //生成信息数据类型列表，用于绘制曲线(比meTypeList多1或两个标准数据的类型)
                }
                if (this.Name == "标定")
                {
                    MyDefine.myXET.meTblCalCurve = new dataTableClass();
                    MyDefine.myXET.meTblCalCurve.dataTable = mytable.CopyTable();
                    MyDefine.myXET.meCalTypeList = myList;                      //生成信息数据类型列表，用于绘制曲线(比meTypeList多1或两个标准数据的类型)
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        #endregion

    }
}
