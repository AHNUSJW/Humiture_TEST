using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Windows.Forms;

namespace HTR
{
    public partial class MenuVerifyPanel : UserControl
    {
        MenuSETValForm mySETVal = new MenuSETValForm();

        public MenuVerifyPanel()
        {
            InitializeComponent();
        }

        #region 界面加载

        private void MenuVerifyPanel_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
        }

        public void AddMyUpdateEvent()
        {
            if (MyDefine.myXET.meTblVer == null)
            {
                createBlankDataTable();           //生成并显示空的验证数据表
            }

            checkPermission();                    //核对权限

        }

        #region 核对权限

        public void checkPermission()
        {
            button1.Visible = MyDefine.myXET.CheckPermission(STEP.查看曲线, false) ? true : false;
            button2.Visible = MyDefine.myXET.CheckPermission(STEP.验证报表, false) ? true : false;
            button3.Visible = MyDefine.myXET.CheckPermission(STEP.验证报告, false) ? true : false;
            button4.Visible = MyDefine.myXET.CheckPermission(STEP.导出报表, false) ? true : false;
        }

        #endregion

        #endregion

        #region 界面按钮事件

        #region 选择F0值是否计算

        /// <summary>
        /// F0值计算公式设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox6.Enabled = true;

                MyDefine.myXET.isF0Checked = true;
            }
            else
            {
                textBox6.Enabled = false;

                MyDefine.myXET.isF0Checked = false;
            }
        }

        #endregion

        #region 生成验证报表按钮 -- 计算并生成多个验证数据表

        /// <summary>
        /// 生成验证表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            groupBox2.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            if (MyDefine.myXET.meDataTbl == null)               //测试数据表为空，退出
            {
                MessageBox.Show("尚未加载测试数据，请加载数据文件！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Boolean ret = true;
            MyDefine.myXET.AddTraceInfo("生成验证报表");
            MyDefine.myXET.meTblVer = null;
            MyDefine.myXET.meTblVer1 = null;
            MyDefine.myXET.meTblVer2 = null;
            MyDefine.myXET.meTblVer3 = null;
            MyDefine.myXET.meTblVer4 = null;
            MyDefine.myXET.meTblVer5 = null;
            MyDefine.myXET.meTblVer6 = null;
            MyDefine.myXET.meTblVer7 = null;
            MyDefine.myXET.meTblVer8 = null;

            //计算各验证表
            if (ret) ret = createVerifyDataTable();        //生成并显示验证数据表
            if (ret) ret = createDataTable2();
            if (ret) ret = createDataTable3();
            if (ret) ret = createDataTable4();
            if (ret) ret = createDataTable5();
            if (ret) ret = createDataTable8();
            if (ret && checkBox1.Checked) ret = createDataTable6();     //计算F0值
            else if (ret) ret = createDataTable7();      //计算F0值
            if (ret) MyDefine.myXET.AddTraceInfo("生成验证报表成功");

            //让表格闪一下，表示已重新计算
            dataGridView1.DataSource = null;
            Application.DoEvents();

            //显示验证数据表
            comboBox1.SelectedIndex = 0;                    //默认显示验证数据表
            ShowTheTable(MyDefine.myXET.meTblVer);          //显示验证数据表
        }

        #endregion

        #region 生成报告按钮 -- 切换到生成报告界面(跨窗体控制)

        //生成校准报告按钮
        private void button3_Click(object sender, EventArgs e)
        {
            groupBox2.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            MyDefine.myXET.switchMainPanel(6);                  //切换到验证报告界面
        }

        #endregion

        #region 生成曲线按钮 -- 切换到验证曲线界面(跨窗体控制)

        private void button1_Click(object sender, EventArgs e)
        {
            groupBox2.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            MyDefine.myXET.switchMainPanel(5);                  //切换到验证曲线界面
        }

        #endregion

        #region 导出报表按钮 -- 将报表导出为csv文件

        private void button4_Click(object sender, EventArgs e)
        {
            groupBox2.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            #region 判断数据源

            string type = "";
            DataTable mytable = new DataTable();
            if (comboBox1.SelectedIndex == 0 && MyDefine.myXET.meTblVer != null)
            {
                type = "验证数据表";
                mytable = MyDefine.myXET.meTblVer.dataTable;
            }
            if (comboBox1.SelectedIndex == 1 && MyDefine.myXET.meTblVer8 != null)
            {
                type = "有效数据汇总表";
                mytable = MyDefine.myXET.meTblVer8.dataTable;
            }
            if (comboBox1.SelectedIndex == 2 && MyDefine.myXET.meTblVer2 != null)
            {
                type = "有效数据横向汇总表-温度";
                mytable = MyDefine.myXET.meTblVer2.dataTable;
            }
            if (comboBox1.SelectedIndex == 3 && MyDefine.myXET.meTblVer3 != null)
            {
                type = "有效数据横向汇总表-湿度";
                mytable = MyDefine.myXET.meTblVer3.dataTable;
            }
            if (comboBox1.SelectedIndex == 4 && MyDefine.myXET.meTblVer4 != null)
            {
                type = "有效数据横向汇总表-压力";
                mytable = MyDefine.myXET.meTblVer4.dataTable;
            }
            if (comboBox1.SelectedIndex == 5 && MyDefine.myXET.meTblVer5 != null)
            {
                type = "有效数据纵向汇总表";
                mytable = MyDefine.myXET.meTblVer5.dataTable;
            }
            if (comboBox1.SelectedIndex == 6 && MyDefine.myXET.meTblVer6 != null)
            {
                type = "关键参数汇总表";
                mytable = MyDefine.myXET.meTblVer6.dataTable;
            }
            if (comboBox1.SelectedIndex == 7 && MyDefine.myXET.meTblVer7 != null)
            {
                type = "F0值计算表";
                mytable = MyDefine.myXET.meTblVer7.dataTable;
            }

            if (type == "")
            {
                MyDefine.myXET.ShowWrongMsg("数据表尚未生成！");
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

        #endregion

        #endregion

        #region 各验证表格生成及显示

        #region 验证数据表初始化(显示空白表)

        //dataGridView加载
        public void InitDataGridView()
        {
            try
            {
                //设置表格为只读
                dataGridView(dataGridView1);    //dataGridView初始化必须放在添加列之前，行高设置才能起作用
                createBlankDataTable();           //尚未加载文件数据时，显示空的验证数据表
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("验证数据表初始化失败：" + ex.ToString());
            }
        }
        #endregion

        #region 生成并显示空白表

        /// <summary>
        /// 尚未加载文件数据时，生成显示空的验证数据表
        /// </summary>
        public void createBlankDataTable()
        {

            //添加列
            dataTableClass myTblVerify = new dataTableClass();
            myTblVerify = new dataTableClass();
            myTblVerify.addTableColumn("序号");                 //第0栏
            myTblVerify.addTableColumn("列1");                  //第1栏
            myTblVerify.addTableColumn("列2");                  //第2栏
            myTblVerify.addTableColumn("列3");                  //第3栏
            myTblVerify.addTableColumn("列4");                  //第4栏
            myTblVerify.addTableColumn("列5");                  //第5栏

            //添加20个空行
            for (int i = 0; i < 20; i++)
            {
                myTblVerify.AddTableRow();
            }

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = myTblVerify.dataTable;     //显示空的验证数据表
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

        #region 生成并显示验证数据表

        /// <summary>
        /// 生成并显示验证数据表(HTH分两行显示，两行的测量范围不同)
        /// </summary>
        public Boolean createVerifyDataTable()
        {
            if (MyDefine.myXET.meInfoTbl == null) return false;         //文件信息表为空，退出
            try
            {

                Int32 intNO = 0;                //序号
                String myNO = "";               //序号
                String myType = "";             //设备类型
                String myCode = "";             //出厂编号
                String myRange = "";            //测量范围
                String myCalDate = "";          //校准日期
                String myRecDate = "";          //复校日期
                String myBattery = "";          //剩余电量

                #region 初始化列表
                //添加列
                MyDefine.myXET.meTblVer = new dataTableClass();
                MyDefine.myXET.meTblVer.addTableColumn("序号");                 //第0栏
                MyDefine.myXET.meTblVer.addTableColumn("验证设备类型");         //第1栏
                MyDefine.myXET.meTblVer.addTableColumn("验证设备编号");         //第2栏
                MyDefine.myXET.meTblVer.addTableColumn("测量范围");             //第3栏
                MyDefine.myXET.meTblVer.addTableColumn("溯源有效期至");         //第4栏
                MyDefine.myXET.meTblVer.addTableColumn("剩余电量");             //第5栏
                MyDefine.myXET.meTemVer = new dataTableClass();
                MyDefine.myXET.meTemVer.dataTable = MyDefine.myXET.meTblVer.CopyTable();
                MyDefine.myXET.meHumVer = new dataTableClass();
                MyDefine.myXET.meHumVer.dataTable = MyDefine.myXET.meTblVer.CopyTable();
                MyDefine.myXET.mePrsVer = new dataTableClass();
                MyDefine.myXET.mePrsVer.dataTable = MyDefine.myXET.meTblVer.CopyTable();

                #endregion
                getDataTypeList();      //获取数据表每列的数据类型：TT_T / TH_T / TH_H / TP_P

                //遍历产品信息表
                for (int idx = 1; idx < MyDefine.myXET.meInfoTbl.dataTable.Columns.Count; idx++)        //meInfoTbl第一列为空数据列
                {
                    myCode = MyDefine.myXET.meInfoTbl.GetCellValue(7, idx).Substring(2);       //出厂编号
                    myRange = MyDefine.myXET.meInfoTbl.GetCellValue(8, idx);      //测量范围
                    myCalDate = MyDefine.myXET.meInfoTbl.GetCellValue(9, idx);    //校准日期
                    myRecDate = MyDefine.myXET.meInfoTbl.GetCellValue(10, idx);   //复校日期（溯源有效期至）
                    myBattery = MyDefine.myXET.meInfoTbl.GetCellValue(11, idx);   //剩余电量

                    if (typeList[idx] == "TT_T")                                    //温度采集器
                    {
                        myNO = (++intNO).ToString();                                    //序号+1
                        myType = "温度(" + MyDefine.myXET.temUnit + ")";                                            //产品类型
                        MyDefine.myXET.meTemVer.AddTableRow(new string[] { myNO, myType, myCode, myRange, myRecDate, myBattery });    //添加行
                    }

                    if (typeList[idx] == "TH_T")                                        //温湿度采集器
                    {
                        myNO = (++intNO).ToString();                                    //序号+1
                        myType = "温湿度(" + MyDefine.myXET.temUnit + ")";                                      //产品类型
                        MyDefine.myXET.meTemVer.AddTableRow(new string[] { myNO, myType, myCode, myRange, myRecDate, myBattery });    //添加行
                    }

                    if (typeList[idx] == "TH_H")                                        //温湿度采集器
                    {
                        myNO = (++intNO).ToString();                                    //序号+1
                        myType = "温湿度(%RH)";                                      //产品类型
                        MyDefine.myXET.meHumVer.AddTableRow(new string[] { myNO, myType, myCode, myRange, myRecDate, myBattery });    //添加行
                    }

                    if (typeList[idx] == "TP_P")                                        //压力采集器
                    {
                        myNO = (++intNO).ToString();                                    //序号+1
                        myType = "压力(kPa)";                                            //产品类型
                        MyDefine.myXET.mePrsVer.AddTableRow(new string[] { myNO, myType, myCode, myRange, myRecDate, myBattery });    //添加行
                    }

                    if (typeList[idx] == "TQ_T")
                    {
                        myNO = (++intNO).ToString();                                    //序号+1
                        myType = "温湿度(" + MyDefine.myXET.temUnit + ")";                                            //产品类型
                        MyDefine.myXET.meTemVer.AddTableRow(new string[] { myNO, myType, myCode, myRange, myRecDate, myBattery });    //添加行
                    }

                    if (typeList[idx] == "TQ_H")
                    {
                        myNO = (++intNO).ToString();                                    //序号+1
                        myType = "温湿度(%RH)";                                            //产品类型
                        MyDefine.myXET.meHumVer.AddTableRow(new string[] { myNO, myType, myCode, myRange, myRecDate, myBattery });    //添加行
                    }
                }
                MyDefine.myXET.meTblVer.AddTable(MyDefine.myXET.meTemVer.dataTable);
                MyDefine.myXET.meTblVer.AddTable(MyDefine.myXET.meHumVer.dataTable);
                MyDefine.myXET.meTblVer.AddTable(MyDefine.myXET.mePrsVer.dataTable);

                MyDefine.myXET.AddTraceInfo("生成验证数据表成功");
                return true;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("生成验证数据表失败：" + ex.ToString());
                MyDefine.myXET.AddTraceInfo("生成验证数据表失败");
                MyDefine.myXET.meTblVer = null;
                return false;
            }
        }

        #endregion

        #region 报告有效数据汇总

        /// <summary>
        /// 报告有效数据汇总
        /// </summary>
        public Boolean createDataTable2()
        {
            if (MyDefine.myXET.meInfoTbl == null) return false;         //文件信息表为空，退出
            if (MyDefine.myXET.meValidStageNum == 0) return false;      //有效数据阶段未设定

            try
            {
                #region 初始化列表
                //添加列
                MyDefine.myXET.meTblVer1 = new dataTableClass();
                MyDefine.myXET.meTblVer1.addTableColumn("有效阶段");         //第0栏
                MyDefine.myXET.meTblVer1.addTableColumn("设备数量");         //第1栏
                MyDefine.myXET.meTblVer1.addTableColumn("设备类型");         //第2栏
                MyDefine.myXET.meTblVer1.addTableColumn("记录间隔");         //第3栏
                MyDefine.myXET.meTblVer1.addTableColumn("记录数");           //第4栏
                MyDefine.myXET.meTblVer1.addTableColumn("启动时间");         //第5栏
                MyDefine.myXET.meTblVer1.addTableColumn("结束时间");         //第6栏
                MyDefine.myXET.meTemVer1 = new dataTableClass();
                MyDefine.myXET.meTemVer1.dataTable = MyDefine.myXET.meTblVer1.CopyTable();
                MyDefine.myXET.meHumVer1 = new dataTableClass();
                MyDefine.myXET.meHumVer1.dataTable = MyDefine.myXET.meTblVer1.CopyTable();
                MyDefine.myXET.mePrsVer1 = new dataTableClass();
                MyDefine.myXET.mePrsVer1.dataTable = MyDefine.myXET.meTblVer1.CopyTable();
                #endregion
                getDataTypeList();      //获取数据表每列的数据类型：TT_T / TH_T / TH_H / TP_P

                for (int Pn = 0; Pn < MyDefine.myXET.meValidStageNum; Pn++)    //遍历有效阶段
                {
                    int listIdx = Pn * 2;                                                                      //阶段Pn在meValidIdxList中的存储开始索引
                    int validIdx1 = MyDefine.myXET.meValidIdxList[listIdx];                                    //有效开始索引
                    int validIdx2 = MyDefine.myXET.meValidIdxList[listIdx + 1];                                //有效结束索引
                    String myStartTime = MyDefine.myXET.meValidTimeList[listIdx].ToString("MM-dd HH:mm:ss");   //有效数据开始时间
                    String myStopTime = MyDefine.myXET.meValidTimeList[listIdx + 1].ToString("MM-dd HH:mm:ss");//有效数据结束时间
                    String myDateNum = (validIdx2 - validIdx1 + 1).ToString();                                 //有效数据个数(记录数)
                    String myStage = MyDefine.myXET.meValidNameList[Pn]; //有效阶段

                    //间隔时间
                    int intspan = MyDefine.myXET.meSpanTime;
                    String mySpanTime = intspan.ToString() + "s";
                    if (intspan >= 60) mySpanTime = (intspan / 60.0).ToString("F2") + "min";
                    if (intspan >= 3600) mySpanTime = (intspan / 3600.0).ToString("F2") + "h";

                    //添加阶段Pn的温度行
                    if (typeList.Contains("TT_T") || typeList.Contains("TH_T") || typeList.Contains("TQ_T"))
                    {
                        String myType = "温度(" + MyDefine.myXET.temUnit + ")";                                                             //产品类型
                        String myNum = tmpNum.ToString();                                                       //产品数量
                        MyDefine.myXET.meTemVer1.AddTableRow(new string[] { myStage, myNum, myType, mySpanTime, myDateNum, myStartTime, myStopTime });
                    }
                    //添加阶段Pn的湿度行
                    if (typeList.Contains("TH_H") || typeList.Contains("TQ_H"))
                    {
                        String myType = "湿度(%RH)";                                                            //产品类型
                        String myNum = humNum.ToString();                                                       //产品数量
                        MyDefine.myXET.meHumVer1.AddTableRow(new string[] { myStage, myNum, myType, mySpanTime, myDateNum, myStartTime, myStopTime });
                    }
                    //添加阶段Pn的压力行
                    if (typeList.Contains("TP_P"))
                    {
                        String myType = "压力(kPa)";                                                            //产品类型
                        String myNum = prsNum.ToString();                                                       //产品数量
                        MyDefine.myXET.mePrsVer1.AddTableRow(new string[] { myStage, myNum, myType, mySpanTime, myDateNum, myStartTime, myStopTime });
                    }
                }
                MyDefine.myXET.meTblVer1.AddTable(MyDefine.myXET.meTemVer1.dataTable);
                MyDefine.myXET.meTblVer1.AddTable(MyDefine.myXET.meHumVer1.dataTable);
                MyDefine.myXET.meTblVer1.AddTable(MyDefine.myXET.mePrsVer1.dataTable);

                MyDefine.myXET.AddTraceInfo("生成有效数据汇总表成功");
                return true;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("生成有效数据汇总表失败:" + ex.ToString());
                MyDefine.myXET.AddTraceInfo("生成有效数据汇总表失败");
                MyDefine.myXET.meTblVer1 = null;
                return false;
            }
        }

        #endregion

        #region 有效数据横向汇总表（表格*3）

        /// <summary>
        /// 详细数据横向汇总表(共三个表格)(有效数据)
        /// </summary>
        public Boolean createDataTable3()
        {
            if (MyDefine.myXET.meDataTbl == null) return false;         //文件信息表为空，退出
            if (MyDefine.myXET.meValidStageNum == 0) return false;      //有效数据阶段未设定

            try
            {
                #region 变量定义

                Double myVal = 0;               //测试值
                int tmpMaxIdx = 0;              //行温度最大值的列索引
                int tmpMinIdx = 0;              //行温度最小值的列索引
                int humMaxIdx = 0;              //行湿度最大值的列索引
                int humMinIdx = 0;              //行湿度最小值的列索引
                int prsMaxIdx = 0;              //行压力最大值的列索引
                int prsMinIdx = 0;              //行压力最小值的列索引

                String myType = "";             //数据类型
                String max = "";
                String maxIdx = "";
                String min = "";
                String minIdx = "";
                String max_min = "";
                string mychar1 = System.Text.Encoding.Default.GetString(new Byte[] { 0x00 });       //空字符null
                string mychar2 = System.Text.Encoding.Default.GetString(new Byte[] { 0x01 });       //空字符null

                #endregion

                #region 表格初始化

                //添加列
                MyDefine.myXET.meTblVer2 = new dataTableClass();
                MyDefine.myXET.meTblVer2.addTableColumn("时间");            //第0栏
                MyDefine.myXET.meTblVer2.addTableColumn("最大值");          //第1栏
                MyDefine.myXET.meTblVer2.addTableColumn("最大值设备编号");       //第2栏
                MyDefine.myXET.meTblVer2.addTableColumn("最小值");          //第3栏
                MyDefine.myXET.meTblVer2.addTableColumn("最小值设备编号");       //第4栏
                MyDefine.myXET.meTblVer2.addTableColumn("差值");       //第5栏
                MyDefine.myXET.meTblVer2.addTableColumn("平均值");         //第6栏
                MyDefine.myXET.meTblVer2.addTableColumn("采样次数");        //第7栏
                MyDefine.myXET.meTblVer2.addTableColumn("设备数量");        //第8栏

                MyDefine.myXET.meTblVer3 = new dataTableClass();
                MyDefine.myXET.meTblVer3.dataTable = MyDefine.myXET.meTblVer2.CopyTable();
                MyDefine.myXET.meTblVer4 = new dataTableClass();
                MyDefine.myXET.meTblVer4.dataTable = MyDefine.myXET.meTblVer2.CopyTable();

                //临时表，用于存放各个阶段的温度、湿度、压力表，最后合并到meTblVer2/3/4中
                dataTableClass myTmpTbl = new dataTableClass();
                dataTableClass myHumTbl = new dataTableClass();
                dataTableClass myPrsTbl = new dataTableClass();
                myTmpTbl.dataTable = MyDefine.myXET.meTblVer2.CopyTable();
                myHumTbl.dataTable = MyDefine.myXET.meTblVer2.CopyTable();
                myPrsTbl.dataTable = MyDefine.myXET.meTblVer2.CopyTable();

                getDataTypeList();      //获取数据表每列的数据类型：TT_T / TH_T / TH_H / TP_P

                #endregion

                #region 生成所有有效阶段的横向汇总表（表格*3）

                for (int Pn = 0; Pn < MyDefine.myXET.meValidStageNum; Pn++)    //遍历有效阶段
                {
                    int listIdx = Pn * 2;                                                                      //阶段Pn在meValidIdxList中的存储开始索引
                    int validIdx1 = MyDefine.myXET.meValidIdxList[listIdx];                                    //有效开始索引
                    int validIdx2 = MyDefine.myXET.meValidIdxList[listIdx + 1];                                //有效结束索引

                    myTmpTbl.ClearTableData();      //清空数据，只保留表头
                    myHumTbl.ClearTableData();      //清空数据，只保留表头
                    myPrsTbl.ClearTableData();      //清空数据，只保留表头
                    if (tmpNum > 0) myTmpTbl.AddTableRow(new string[] { MyDefine.myXET.meValidNameList[Pn] + ":", "", "", "", "", "", "", "" });
                    if (humNum > 0) myHumTbl.AddTableRow(new string[] { MyDefine.myXET.meValidNameList[Pn] + ":", "", "", "", "", "", "", "" });
                    if (prsNum > 0) myPrsTbl.AddTableRow(new string[] { MyDefine.myXET.meValidNameList[Pn] + ":", "", "", "", "", "", "", "" });

                    #region 生成当前有效阶段对应的温度、湿度、压力横向汇总表

                    //计算数据表MyDefine.myXET.meDataTbl每行的最大最小值，并添加进表格
                    for (int i = validIdx1; i <= validIdx2; i++)            //有效数据开始行-有效数据结束行
                    {
                        #region 变量定义

                        Double myTMPMax = Double.MinValue;      //行温度最大值
                        Double myTMPMin = Double.MaxValue;      //行温度最小值
                        Double myHUMMax = Double.MinValue;      //行湿度最大值
                        Double myHUMMin = Double.MaxValue;      //行湿度最小值
                        Double myPRSMax = Double.MinValue;      //行压力最大值
                        Double myPRSMin = Double.MaxValue;      //行压力最小值
                        Double myTMPAvg = 0;                    //行温度平均值
                        Double myHUMAvg = 0;                    //行湿度平均值
                        Double myPRSAvg = 0;                    //行压力平均值
                        int tmpNum = 0;                         //行湿度计数
                        int humNum = 0;                         //行压力计数
                        int prsNum = 0;                         //行压力计数
                        String myTime = MyDefine.myXET.meDataTbl.GetCellValue(i, 0);         //测试时间

                        #endregion

                        #region 计算行温度(/湿度/压力)最大最小值

                        //计算每一行的温度/湿度/压力的最大最小值
                        for (int j = 1; j < MyDefine.myXET.meDataTbl.dataTable.Columns.Count; j++)        //meDataTbl第0列为Time
                        {
                            if (MyDefine.myXET.meDataTbl.GetCellValue(i, j) == "") continue;              //空数据
                            myType = typeList[j];                                                         //数据类型
                            myVal = Convert.ToDouble(MyDefine.myXET.meDataTbl.GetCellValue(i, j));        //测试值

                            //计算最大最小值
                            switch (myType)
                            {
                                case "TT_T":
                                case "TH_T":
                                case "TQ_T":
                                    if (myTMPMax < myVal) { myTMPMax = myVal; tmpMaxIdx = j; }
                                    if (myTMPMin > myVal) { myTMPMin = myVal; tmpMinIdx = j; }
                                    tmpNum++;
                                    myTMPAvg += myVal;
                                    break;

                                case "TH_H":
                                case "TQ_H":
                                    if (myHUMMax < myVal) { myHUMMax = myVal; humMaxIdx = j; }
                                    if (myHUMMin > myVal) { myHUMMin = myVal; humMinIdx = j; }
                                    humNum++;
                                    myHUMAvg += myVal;
                                    break;

                                case "TP_P":
                                    if (myPRSMax < myVal) { myPRSMax = myVal; prsMaxIdx = j; }
                                    if (myPRSMin > myVal) { myPRSMin = myVal; prsMinIdx = j; }
                                    prsNum++;
                                    myPRSAvg += myVal;
                                    break;
                            }
                        }

                        myTMPAvg = myTMPAvg / (double)tmpNum;
                        myHUMAvg = myHUMAvg / (double)humNum;
                        myPRSAvg = myPRSAvg / (double)prsNum;

                        #endregion

                        #region 向数据表中添加一行数据

                        if (tmpNum > 0 && myTMPMax != double.MinValue)
                        {
                            //生成温度表新行
                            max = myTMPMax.ToString("F2");
                            maxIdx = codeNumlist[tmpMaxIdx].ToString();
                            min = myTMPMin.ToString("F2");
                            minIdx = codeNumlist[tmpMinIdx].ToString();
                            max_min = (myTMPMax - myTMPMin).ToString("F2");
                            myTmpTbl.AddTableRow(new string[] { myTime, max, codelist[tmpMaxIdx], min, codelist[tmpMinIdx], max_min, myTMPAvg.ToString("F2"), (i - MyDefine.myXET.meValidStartIdx + 1).ToString(), tmpNum.ToString() });
                        }

                        if (humNum > 0 && myHUMMax != double.MinValue)
                        {
                            //生成湿度表新行
                            max = myHUMMax.ToString("F2");
                            maxIdx = codeNumlist[humMaxIdx].ToString();
                            min = myHUMMin.ToString("F2");
                            minIdx = codeNumlist[humMinIdx].ToString();
                            max_min = (myHUMMax - myHUMMin).ToString("F2");
                            myHumTbl.AddTableRow(new string[] { myTime, max, codelist[humMaxIdx], min, codelist[humMinIdx], max_min, myHUMAvg.ToString("F2"), (i - MyDefine.myXET.meValidStartIdx + 1).ToString(), humNum.ToString() });
                        }

                        if (prsNum > 0 && myPRSMax != double.MinValue)
                        {
                            //生成压力表新行
                            max = myPRSMax.ToString("F2");
                            maxIdx = codeNumlist[prsMaxIdx].ToString();
                            min = myPRSMin.ToString("F2");
                            minIdx = codeNumlist[prsMinIdx].ToString();
                            max_min = (myPRSMax - myPRSMin).ToString("F2");
                            myPrsTbl.AddTableRow(new string[] { myTime, max, codelist[prsMaxIdx], min, codelist[prsMinIdx], max_min, myPRSAvg.ToString("F2"), (i - MyDefine.myXET.meValidStartIdx + 1).ToString(), prsNum.ToString() });
                        }

                        #endregion
                    }

                    #region 计算数据表中"最大-最小"列的和、平均值

                    if (tmpNum > 0)             //存在温度数据，则计算其"最大-最小"列的和、平均值
                    {
                        double sum = myTmpTbl.GetColumnSumVal(5);        //求"最大-最小"列的和
                        double avr = myTmpTbl.GetColumnAvrVal(5);        //求"最大-最小"列的平均值
                        myTmpTbl.AddTableRow(new string[] { "", "", "", "", "求和：", sum.ToString("F2"), "", "" });
                        myTmpTbl.AddTableRow(new string[] { "", "", "", "", "平均值：", avr.ToString("F2"), "", "" });
                    }

                    if (humNum > 0)             //存在湿度数据，则计算其"最大-最小"列的和、平均值
                    {
                        double sum = myHumTbl.GetColumnSumVal(5);        //求"最大-最小"列的和
                        double avr = myHumTbl.GetColumnAvrVal(5);        //求"最大-最小"列的平均值
                        myHumTbl.AddTableRow(new string[] { "", "", "", "", "求和：", sum.ToString("F2"), "", "" });
                        myHumTbl.AddTableRow(new string[] { "", "", "", "", "平均值：", avr.ToString("F2"), "", "" });
                    }

                    if (prsNum > 0)             //存在压力数据，则计算其"最大-最小"列的和、平均值
                    {
                        double sum = myPrsTbl.GetColumnSumVal(5);        //求"最大-最小"列的和
                        double avr = myPrsTbl.GetColumnAvrVal(5);        //求"最大-最小"列的平均值
                        myPrsTbl.AddTableRow(new string[] { "", "", "", "", "求和：", sum.ToString("F2"), "", "" });
                        myPrsTbl.AddTableRow(new string[] { "", "", "", "", "平均值：", avr.ToString("F2"), "", "" });
                    }

                    #endregion

                    #endregion

                    //将当前阶段的横向汇总表添加到总汇总表中
                    MyDefine.myXET.meTblVer2.dataTable.Merge(myTmpTbl.dataTable);
                    MyDefine.myXET.meTblVer3.dataTable.Merge(myHumTbl.dataTable);
                    MyDefine.myXET.meTblVer4.dataTable.Merge(myPrsTbl.dataTable);
                }

                #endregion

                MyDefine.myXET.AddTraceInfo("生成横向汇总表成功");
                return true;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("生成横向汇总表失败：" + ex.ToString());
                MyDefine.myXET.AddTraceInfo("生成横向汇总表失败");
                MyDefine.myXET.meTblVer2 = null;
                MyDefine.myXET.meTblVer3 = null;
                MyDefine.myXET.meTblVer4 = null;
                return false;
            }
        }

        #endregion

        #region 有效数据纵向汇总表

        /// <summary>
        /// 详细数据纵向汇总表(有效数据)
        /// </summary>
        public Boolean createDataTable4()
        {
            if (MyDefine.myXET.meInfoTbl == null) return false;         //文件信息表为空，退出
            if (MyDefine.myXET.meValidStageNum == 0) return false;      //有效数据阶段未设定

            try
            {
                #region 变量定义

                String myType = "";               //数据类型
                String myCode = "";               //出厂编号
                String myMaxVal = "";             //列最大值
                String myMinVal = "";             //列最小值
                String myAvrVal = "";             //列平均值
                String max_min = "";              //最大-最小
                Double max;                       //列最大值
                Double min;                       //列最小值

                //添加列
                MyDefine.myXET.meTblVer5 = new dataTableClass();
                MyDefine.myXET.meTblVer5.addTableColumn("有效阶段");           //第0栏
                MyDefine.myXET.meTblVer5.addTableColumn("设备编号");           //第1栏(出厂编号)
                MyDefine.myXET.meTblVer5.addTableColumn("开始时间");           //第2栏
                MyDefine.myXET.meTblVer5.addTableColumn("结束时间");           //第3栏
                MyDefine.myXET.meTblVer5.addTableColumn("设备类型");           //第4栏
                MyDefine.myXET.meTblVer5.addTableColumn("最大值");             //第5栏
                MyDefine.myXET.meTblVer5.addTableColumn("最小值");             //第6栏
                MyDefine.myXET.meTblVer5.addTableColumn("差值");               //第7栏
                MyDefine.myXET.meTblVer5.addTableColumn("平均值");             //第8栏
                MyDefine.myXET.meTemVer5 = new dataTableClass();
                MyDefine.myXET.meTemVer5.dataTable = MyDefine.myXET.meTblVer5.CopyTable();
                MyDefine.myXET.meHumVer5 = new dataTableClass();
                MyDefine.myXET.meHumVer5.dataTable = MyDefine.myXET.meTblVer5.CopyTable();
                MyDefine.myXET.mePrsVer5 = new dataTableClass();
                MyDefine.myXET.mePrsVer5.dataTable = MyDefine.myXET.meTblVer5.CopyTable();

                getDataTypeList();      //获取数据表每列的数据类型：TT_T / TH_T / TH_H / TP_P

                #endregion

                #region 生成所有有效阶段的纵向汇总表
                for (int Pn = 0; Pn < MyDefine.myXET.meValidStageNum; Pn++)    //遍历有效阶段
                {
                    int listIdx = Pn * 2;                                                                      //阶段Pn在meValidIdxList中的存储开始索引
                    int validIdx1 = MyDefine.myXET.meValidIdxList[listIdx];                                    //有效开始索引
                    int validIdx2 = MyDefine.myXET.meValidIdxList[listIdx + 1];                                //有效结束索引
                    String myStartTime = MyDefine.myXET.meValidTimeList[listIdx].ToString("MM-dd HH:mm:ss");   //有效数据开始时间
                    String myStopTime = MyDefine.myXET.meValidTimeList[listIdx + 1].ToString("MM-dd HH:mm:ss");//有效数据结束时间
                    String myDateNum = (validIdx2 - validIdx1 + 1).ToString();                                 //有效数据个数(记录数)
                    String myStage = MyDefine.myXET.meValidNameList[Pn];                                                //有效阶段

                    //截取有效数据段，放入临时表validTbl
                    dataTableClass validTbl = new dataTableClass();
                    validTbl.dataTable = MyDefine.myXET.meDataTbl.CopyTable(validIdx1, validIdx2);

                    #region 生成当前有效阶段的纵向汇总表
                    //先添加所有温度行
                    for (int idx = 1; idx < validTbl.dataTable.Columns.Count; idx++)        //meInfoTbl第一列为空数据列
                    {
                        if (typeList[idx] == "TT_T" || typeList[idx] == "TH_T" || typeList[idx] == "TQ_T")                                           //此列为温度数据
                        {
                            myType = "温度(" + MyDefine.myXET.temUnit + ")";                                                                          //数据类型
                            myCode = MyDefine.myXET.meInfoTbl.GetCellValue(7, idx).Substring(2);                                                                         //出厂编号
                            myMaxVal = validTbl.GetColumnMaxVal(idx).ToString("F2");                                      //列最大值
                            myMinVal = validTbl.GetColumnMinVal(idx).ToString("F2");                                      //列最小值
                            myAvrVal = validTbl.GetColumnAvrVal(idx).ToString("F2");                                      //列平均值
                            if (Double.TryParse(myMaxVal, out max) && Double.TryParse(myMinVal, out min))
                            {
                                max_min = (max - min).ToString("F2");                                                    //最大-最小
                            }
                            else
                            {
                                myMaxVal = "";
                                myMinVal = "";
                                myAvrVal = "";
                                max_min = "";
                            }
                            MyDefine.myXET.meTemVer5.AddTableRow(new string[] { myStage, myCode, myStartTime, myStopTime, myType, myMaxVal, myMinVal, max_min, myAvrVal });
                        }
                    }
                    //再添加所有湿度行
                    for (int idx = 1; idx < validTbl.dataTable.Columns.Count; idx++)        //meInfoTbl第一列为空数据列
                    {
                        if (typeList[idx] == "TH_H" || typeList[idx] == "TQ_H")                                                                    //此列为湿度数据
                        {
                            myType = "湿度(%RH)";                                                                       //数据类型
                            myCode = MyDefine.myXET.meInfoTbl.GetCellValue(7, idx).Substring(2);                                     //出厂编号
                            myMaxVal = validTbl.GetColumnMaxVal(idx).ToString("F2");                                    //列最大值
                            myMinVal = validTbl.GetColumnMinVal(idx).ToString("F2");                                    //列最小值
                            myAvrVal = validTbl.GetColumnAvrVal(idx).ToString("F2");                                    //列平均值
                            if (Double.TryParse(myMaxVal, out max) && Double.TryParse(myMinVal, out min))
                            {
                                max_min = (max - min).ToString("F2");                                                   //最大-最小
                            }
                            else
                            {
                                myMaxVal = "";
                                myMinVal = "";
                                myAvrVal = "";
                                max_min = "";
                            }

                            MyDefine.myXET.meHumVer5.AddTableRow(new string[] { myStage, myCode, myStartTime, myStopTime, myType, myMaxVal, myMinVal, max_min, myAvrVal });
                        }
                    }
                    //最后添加所有压力行
                    for (int idx = 1; idx < validTbl.dataTable.Columns.Count; idx++)        //meInfoTbl第一列为空数据列
                    {
                        if (typeList[idx] == "TP_P")                                                                    //此列为压力数据
                        {
                            myType = "压力(kPa)";                                                                       //数据类型
                            myCode = MyDefine.myXET.meInfoTbl.GetCellValue(7, idx).Substring(2);                                     //出厂编号
                            myMaxVal = validTbl.GetColumnMaxVal(idx).ToString("F2");                                    //列最大值
                            myMinVal = validTbl.GetColumnMinVal(idx).ToString("F2");                                    //列最小值
                            myAvrVal = validTbl.GetColumnAvrVal(idx).ToString("F2");                                    //列平均值
                            if (Double.TryParse(myMaxVal, out max) && Double.TryParse(myMinVal, out min))
                            {
                                max_min = (max - min).ToString("F2");                                                   //最大-最小
                            }
                            else
                            {
                                myMaxVal = "";
                                myMinVal = "";
                                myAvrVal = "";
                                max_min = "";
                            }

                            MyDefine.myXET.mePrsVer5.AddTableRow(new string[] { myStage, myCode, myStartTime, myStopTime, myType, myMaxVal, myMinVal, max_min, myAvrVal });
                        }
                    }
                    #endregion
                }
                MyDefine.myXET.meTblVer5.AddTable(MyDefine.myXET.meTemVer5.dataTable);
                MyDefine.myXET.meTblVer5.AddTable(MyDefine.myXET.meHumVer5.dataTable);
                MyDefine.myXET.meTblVer5.AddTable(MyDefine.myXET.mePrsVer5.dataTable);
                #endregion
                MyDefine.myXET.AddTraceInfo("生成纵向汇总表成功");
                return true;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("生成纵向汇总表失败：" + ex.ToString());
                MyDefine.myXET.AddTraceInfo("生成纵向汇总表失败");
                MyDefine.myXET.meTblVer5 = null;
                return false;
            }
        }

        #endregion

        #region 关键参数汇总(有效数据)

        /// <summary>
        /// 关键参数汇总(有效数据)
        /// </summary>
        public Boolean createDataTable5()
        {
            try
            {
                if (MyDefine.myXET.meDataTbl == null) return false;         //文件信息表为空，退出
                if (MyDefine.myXET.meValidStageNum == 0) return false;      //有效数据阶段未设定

                #region 设定值检查

                int PnNum = MyDefine.myXET.meValidStageNum;

                if (tmpNum > 0 && MyDefine.myXET.meTmpSETList.Count < PnNum)                  //温度设定值
                {
                    MyDefine.myXET.ShowWrongMsg("温度设定值为空或部分缺失，关键参数汇总表创建失败！");
                    return false;
                }

                if (humNum > 0 && MyDefine.myXET.meHumSETList.Count < PnNum)                  //湿度设定值
                {
                    MyDefine.myXET.ShowWrongMsg("湿度设定值为空或部分缺失，关键参数汇总表创建失败！");
                    return false;
                }

                if (prsNum > 0 && MyDefine.myXET.mePrsSETList.Count < PnNum)                  //压力设定值
                {
                    MyDefine.myXET.ShowWrongMsg("压力设定值为空或部分缺失，关键参数汇总表创建失败！");
                    return false;
                }

                #endregion

                #region 验证表初始化

                //添加列
                MyDefine.myXET.meTblVer6 = new dataTableClass();
                MyDefine.myXET.meTblVer6.addTableColumn("有效阶段");              //第0栏
                MyDefine.myXET.meTblVer6.addTableColumn("标准器类型");            //第1栏
                MyDefine.myXET.meTblVer6.addTableColumn("均匀度");                //第2栏
                MyDefine.myXET.meTblVer6.addTableColumn("波动度");                //第3栏
                MyDefine.myXET.meTblVer6.addTableColumn("上偏差");                //第4栏
                MyDefine.myXET.meTblVer6.addTableColumn("下偏差");                //第5栏
                //添加列
                MyDefine.myXET.meTemVer6 = new dataTableClass();
                MyDefine.myXET.meTemVer6.dataTable = MyDefine.myXET.meTblVer6.CopyTable();
                //添加列
                MyDefine.myXET.meHumVer6 = new dataTableClass();
                MyDefine.myXET.meHumVer6.dataTable = MyDefine.myXET.meTblVer6.CopyTable();
                //添加列
                MyDefine.myXET.mePrsVer6 = new dataTableClass();
                MyDefine.myXET.mePrsVer6.dataTable = MyDefine.myXET.meTblVer6.CopyTable();
                #endregion

                #region 生成所有有效阶段的关键参数汇总

                for (int Pn = 0; Pn < MyDefine.myXET.meValidStageNum; Pn++)    //遍历有效阶段
                {
                    int listIdx = Pn * 2;                                                                      //阶段Pn在meValidIdxList中的存储开始索引
                    int validIdx1 = MyDefine.myXET.meValidIdxList[listIdx];                                    //有效开始索引
                    int validIdx2 = MyDefine.myXET.meValidIdxList[listIdx + 1];                                //有效结束索引
                    int myDateNum = validIdx2 - validIdx1 + 1;                                                 //有效数据个数
                    String myStage = MyDefine.myXET.meValidNameList[Pn];                                       //有效阶段

                    //截取当前阶段有效数据，放入临时表validTbl
                    dataTableClass validTbl = new dataTableClass();
                    validTbl.dataTable = MyDefine.myXET.meDataTbl.CopyTable(validIdx1, validIdx2);

                    #region 变量初始化

                    List<double> tmpDiffList = new List<double>();       //数据表每一行的温度最大-最小值的和(有效数据)，用于计算均匀度
                    List<double> humDiffList = new List<double>();       //数据表每一行的湿度最大-最小值的和(有效数据)，用于计算均匀度
                    List<double> prsDiffList = new List<double>();       //数据表每一行的压力最大-最小值的和(有效数据)，用于计算均匀度
                    List<double> tmpMaxList = new List<double>();        //数据表每一列的温度最大值(有效数据)，用于计算上下偏差里面的最大温度
                    List<double> humMaxList = new List<double>();        //数据表每一列的湿度最大值(有效数据)，用于计算上下偏差里面的最大湿度
                    List<double> prsMaxList = new List<double>();        //数据表每一列的压力最大值(有效数据)，用于计算上下偏差里面的最大压力
                    List<double> tmpMinList = new List<double>();        //数据表每一列的温度最小值(有效数据)，用于计算上下偏差里面的最小温度
                    List<double> humMinList = new List<double>();        //数据表每一列的湿度最小值(有效数据)，用于计算上下偏差里面的最小湿度
                    List<double> prsMinList = new List<double>();        //数据表每一列的压力最小值(有效数据)，用于计算上下偏差里面的最小压力
                    List<double> tmpVolList = new List<double>();        //数据表每一列的温度最大-最小值(有效数据)，用于计算波动度
                    List<double> humVolList = new List<double>();        //数据表每一列的湿度最大-最小值(有效数据)，用于计算波动度
                    List<double> prsVolList = new List<double>();        //数据表每一列的压力最大-最小值(有效数据)，用于计算波动度
                    string mychar1 = System.Text.Encoding.Default.GetString(new Byte[] { 0x00 });       //空字符null
                    string mychar2 = System.Text.Encoding.Default.GetString(new Byte[] { 0x01 });       //空字符null

                    #endregion

                    #region 遍历有效数据行，统计所有行的各数据类型的最大-最小值，用于计算均匀度

                    for (int i = 0; i < myDateNum; i++)       //遍历有效数据表
                    {
                        Double TMPMax = Double.MinValue;      //行温度最大值
                        Double TMPMin = Double.MaxValue;      //行温度最小值
                        Double HUMMax = Double.MinValue;      //行湿度最大值
                        Double HUMMin = Double.MaxValue;      //行湿度最小值
                        Double PRSMax = Double.MinValue;      //行压力最大值
                        Double PRSMin = Double.MaxValue;      //行压力最小值
                        String myTime = validTbl.GetCellValue(i, 0);    //测试时间

                        //计算每一行的温度/湿度/压力的最大最小值
                        for (int j = 1; j < validTbl.dataTable.Columns.Count; j++)        //meDataTbl第0列为Time
                        {
                            if (validTbl.GetCellValue(i, j) == "") continue;              //空数据
                            Double myVal = Convert.ToDouble(validTbl.GetCellValue(i, j)); //测试值

                            //计算最大最小值
                            switch (typeList[j])    //数据类型
                            {
                                case "TT_T":
                                case "TH_T":
                                case "TQ_T":
                                    if (TMPMax < myVal) TMPMax = myVal;
                                    if (TMPMin > myVal) TMPMin = myVal;
                                    break;

                                case "TH_H":
                                case "TQ_H":
                                    if (HUMMax < myVal) HUMMax = myVal;
                                    if (HUMMin > myVal) HUMMin = myVal;
                                    break;

                                case "TP_P":
                                    if (PRSMax < myVal) PRSMax = myVal;
                                    if (PRSMin > myVal) PRSMin = myVal;
                                    break;
                            }
                        }

                        if (tmpNum > 0)
                        {
                            //生成温度表新行
                            double max_min = TMPMax - TMPMin;
                            tmpDiffList.Add(max_min);
                        }

                        if (humNum > 0)
                        {
                            //生成湿度表新行
                            double max_min = HUMMax - HUMMin;
                            humDiffList.Add(max_min);
                        }

                        if (prsNum > 0)
                        {
                            //生成压力表新行
                            double max_min = PRSMax - PRSMin;
                            prsDiffList.Add(max_min);
                        }
                    }

                    #endregion

                    #region 统计每列的最大值最小值(用于计算上下偏差)，并计算每列的最大-最小值(用于计算波动度)

                    //计算所有列的波动度(取其中最大波动度输出到报告)
                    for (int idx = 1; idx < validTbl.dataTable.Columns.Count; idx++)        //meInfoTbl第一列为空数据列
                    {
                        if (typeList[idx] == "TT_T" || typeList[idx] == "TH_T" || typeList[idx] == "TQ_T")//此列为温度数据
                        {
                            Double myMaxVal = validTbl.GetColumnMaxVal(idx);                      //列最大值
                            Double myMinVal = validTbl.GetColumnMinVal(idx);                      //列最小值
                            Double myColVol = myMaxVal - myMinVal;                                //计算此列数据的最大-最小值(波动度)
                            tmpVolList.Add(myColVol);
                            tmpMaxList.Add(myMaxVal);
                            tmpMinList.Add(myMinVal);
                        }

                        if (typeList[idx] == "TH_H" || typeList[idx] == "TQ_H")                   //此列为湿度数据
                        {
                            Double myMaxVal = validTbl.GetColumnMaxVal(idx);                      //列最大值
                            Double myMinVal = validTbl.GetColumnMinVal(idx);                      //列最小值
                            Double myColVol = myMaxVal - myMinVal;                                //计算此列数据的最大-最小值(波动度)
                            humVolList.Add(myColVol);
                            humMaxList.Add(myMaxVal);
                            humMinList.Add(myMinVal);
                        }

                        if (typeList[idx] == "TP_P")                                              //此列为压力数据
                        {
                            Double myMaxVal = validTbl.GetColumnMaxVal(idx);                      //列最大值
                            Double myMinVal = validTbl.GetColumnMinVal(idx);                      //列最小值
                            Double myColVol = myMaxVal - myMinVal;                                //计算此列数据的最大-最小值(波动度)
                            prsVolList.Add(myColVol);
                            prsMaxList.Add(myMaxVal);
                            prsMinList.Add(myMinVal);
                        }
                    }

                    #endregion

                    #region 计算均匀度、波动度、上下偏差，并写入验证表

                    if (tmpNum > 0)
                    {
                        //计算均匀度、波动度
                        String evenness = (tmpDiffList.Sum() / tmpDiffList.Count).ToString("F2");   //均匀度
                        String volatility = (tmpVolList.Max() / 2).ToString("±" + ("#0.#0"));      //波动度

                        //计算温度的上偏差、下偏差
                        String upper = (tmpMaxList.Max() - MyDefine.myXET.meTmpSETList[Pn]).ToString("F2");   //上偏差
                        String lower = (tmpMinList.Min() - MyDefine.myXET.meTmpSETList[Pn]).ToString("F2");   //下偏差
                        MyDefine.myXET.meTemVer6.AddTableRow(new string[] { myStage, MyDefine.myXET.temUnit, evenness, volatility, upper, lower });      //添加行
                    }

                    if (humNum > 0)
                    {
                        //计算均匀度、波动度
                        String evenness = (humDiffList.Sum() / humDiffList.Count).ToString("F2");   //均匀度
                        String volatility = (humVolList.Max() / 2).ToString("±" + ("#0.#0"));      //波动度

                        //计算湿度的上偏差、下偏差
                        String upper = (humMaxList.Max() - MyDefine.myXET.meHumSETList[Pn]).ToString("F2");    //上偏差
                        String lower = (humMinList.Min() - MyDefine.myXET.meHumSETList[Pn]).ToString("F2");    //下偏差
                        MyDefine.myXET.meHumVer6.AddTableRow(new string[] { myStage, "%RH", evenness, volatility, upper, lower });     //添加行
                    }

                    if (prsNum > 0)
                    {
                        //计算均匀度、波动度
                        String evenness = (prsDiffList.Sum() / prsDiffList.Count).ToString("F2");   //均匀度
                        String volatility = (prsVolList.Max() / 2).ToString("±" + ("#0.#0"));      //波动度

                        //计算压力的上偏差、下偏差
                        String upper = (prsMaxList.Max() - MyDefine.myXET.mePrsSETList[Pn]).ToString("F2");     //上偏差
                        String lower = (prsMinList.Min() - MyDefine.myXET.mePrsSETList[Pn]).ToString("F2");     //下偏差
                        MyDefine.myXET.mePrsVer6.AddTableRow(new string[] { myStage, "kPa", evenness, volatility, upper, lower });     //添加行
                    }

                    #endregion

                }

                #endregion
                MyDefine.myXET.meTblVer6.AddTable(MyDefine.myXET.meTemVer6.dataTable);
                MyDefine.myXET.meTblVer6.AddTable(MyDefine.myXET.meHumVer6.dataTable);
                MyDefine.myXET.meTblVer6.AddTable(MyDefine.myXET.mePrsVer6.dataTable);
                MyDefine.myXET.AddTraceInfo("生成关键参数汇总表成功");
                return true;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("生成关键参数汇总表失败：" + ex.ToString());
                MyDefine.myXET.AddTraceInfo("生成关键参数汇总表失败");
                MyDefine.myXET.meTblVer6 = null;
                return false;
            }
        }

        #endregion

        #region F0值计算(仅温度需要计算)，计算阈值

        public Boolean createDataTable6()
        {
            try
            {
                if (MyDefine.myXET.meDataTbl == null) return false;       //文件信息表为空，退出
                if (tmpNum <= 0)                                          //数据表中无温度数据
                {
                    MyDefine.myXET.meTblVer7 = null;
                    return false;
                }

                #region F0参数

                double baseTime = 1;                //时间基准(秒)
                double tmpREF = 0;                  //温度参比值TX
                double tmp_Z = 0;                   //Z值
                double tmpSET = 0;                  //温度阈值(用来计算开始、结束时间)
                int tmpValidIdx1 = int.MinValue;    //F0有效数据开始
                int tmpValidIdx2 = int.MinValue;    //F0有效数据结束

                try
                {
                    tmpREF = Convert.ToDouble(textBox4.Text);                 //温度参比值TX
                    tmp_Z = Convert.ToDouble(textBox5.Text);                  //Z值
                    tmpSET = Convert.ToDouble(textBox6.Text);                 //温度阈值
                    baseTime = (radioButton1.Checked) ? 60.0 : 1.0;           //时间基准(秒)
                }
                catch (Exception ex)
                {
                    MyDefine.myXET.ShowWrongMsg("F0参数填写错误：" + ex.ToString().Split('\n')[0]);
                    return false;
                }

                #endregion

                #region 第1个有效数据区域

                #region 计算F0有效数据开始、结束索引

                //计算F0有效数据开始、有效数据结束索引
                MyDefine.myXET.meF0ValidList.Clear();         //清空有效数据索引列表
                for (int i = 0; i < MyDefine.myXET.meDataTblTmp.dataTable.Rows.Count; i++)
                {
                    double tmpMax = int.MinValue;      //行温度最大值
                    double tmpMin = int.MaxValue;      //行温度最小值

                    //计算本行数据的温度最大最小值
                    for (int j = 1; j < MyDefine.myXET.meDataTblTmp.dataTable.Columns.Count; j++)        //mytable第0列为Time
                    {
                        if (MyDefine.myXET.meDataTblTmp.GetCellValue(i, j) == "") continue;              //空数据(忽略空单元格)
                        double myVal = Convert.ToDouble(MyDefine.myXET.meDataTblTmp.GetCellValue(i, j)); //测试值

                        //计算行温度最大最小值
                        if (tmpMax < myVal) tmpMax = myVal;
                        if (tmpMin > myVal) tmpMin = myVal;
                    }

                    //判断是否为有效数据开始、结束索引
                    if (tmpValidIdx1 == int.MinValue)                   //尚未找到有效数据开始索引
                    {
                        if (tmpMax >= tmpSET) tmpValidIdx1 = i;         //判断当前行温度最大值是否达到设定阈值，是则设置当前行索引为有效数据开始索引
                    }
                    else if (tmpValidIdx2 == int.MinValue)              //已找到有效数据开始索引，尚未找到有效数据结束索引
                    {
                        if (tmpMax < tmpSET) tmpValidIdx2 = i;          //判断当前行温度最大值是否低于设定阈值，是则设置当前行索引为有效数据结束索引
                    }
                    else                                                //有效数据索引已找齐，退出循环
                    {
                        break;
                    }
                }

                if (tmpValidIdx1 == int.MinValue)                        //温度值一直未达到设定值
                {
                    MyDefine.myXET.ShowWrongMsg("F0值计算失败：温度值未能达到阈值！");
                    return false;
                }

                if (tmpValidIdx2 == int.MinValue) tmpValidIdx2 = MyDefine.myXET.meDataTbl.dataTable.Rows.Count - 1;       //有效数据结束为最后一个值

                //记录第一组有效开始、有效结束索引
                MyDefine.myXET.meF0ValidList.Add(tmpValidIdx1);
                MyDefine.myXET.meF0ValidList.Add(tmpValidIdx2);

                #endregion

                #region 计算有效数据F0值

                //添加列
                dataTableClass F0Table = new dataTableClass();                 //定义一个表，用于存储计算的F0值表格
                F0Table.addTableColumn("时间");
                for (int i = 1; i < typeList.Count; i++)
                {
                    if (typeList[i] == "TT_T" || typeList[i] == "TH_T" || typeList[i] == "TQ_T")
                    {
                        F0Table.addTableColumn(codelist[i]);
                    }
                }

                //计算温度F0值并写入meTblVer7
                Double myf0 = double.MinValue;
                List<Double> myf0List = new List<Double>();                             //F0值列表(存放一行数据计算出的所有温度F0值)
                Double myspan = Convert.ToDouble(MyDefine.myXET.homspan) / baseTime;    //测试间隔(分)
                myf0List.Add(double.MinValue);                                          //添加一个空值，与meTblVer7的行结构保持一致

                for (int irow = tmpValidIdx1; irow < tmpValidIdx2; irow++)              //有效数据开始行 -- 有效数据结束行
                {
                    int idx = 0;                                                        //记录当前myf0List的索引
                    String myTime = MyDefine.myXET.meDataTblTmp.GetCellValue(irow, 0);     //测试时间

                    //计算本行数据中各列的F0值
                    for (int jcol = 1; jcol < MyDefine.myXET.meDataTblTmp.dataTable.Columns.Count; jcol++)     //mytable第0列为Time
                    {
                        //计算行温度F0值
                        idx++;
                        if (MyDefine.myXET.meDataTblTmp.GetCellValue(irow, jcol) == "")                        //空单元格的F0写double.MinValue
                        {
                            myf0 = double.MinValue;
                        }
                        else
                        {
                            Double myTmp = Convert.ToDouble(MyDefine.myXET.meDataTblTmp.GetCellValue(irow, jcol)); //测试值
                            if (myTmp < 100)
                            {
                                Console.WriteLine("1");
                            }
                            myf0 = myspan * Math.Pow(10, (myTmp - tmpREF) / tmp_Z);                             //计算当前测试值的F0值
                        }

                        if (irow == tmpValidIdx1)           //第一行数据直接添加F0
                        {
                            myf0List.Add(myf0);
                        }
                        else                                //非第一行数据
                        {
                            //myf0List[idx] += myf0;        //非第一行数据累加F0
                            myf0List[idx] = myf0;           //非第一行数据直接赋值F0
                        }
                    }

                    //添加一行F0列表的数据
                    F0Table.AddTableRow();
                    F0Table.SetCellValue(irow - tmpValidIdx1, 0, myTime);        //写入测试时间
                    for (int k = 1; k < myf0List.Count; k++)
                    {
                        if (myf0List[k] != double.MinValue) F0Table.SetCellValue(irow - tmpValidIdx1, k, myf0List[k].ToString("F2"));    //写入各列在本行的F0值
                    }
                }

                #endregion

                #region 横向数据添加(行最大、最小、平均值等)

                //添加列
                MyDefine.myXET.meTblVer7 = new dataTableClass();
                MyDefine.myXET.meTblVer7.dataTable = F0Table.CopyTable();       //复制F0值储存表
                MyDefine.myXET.meTblVer7.addTableColumn("最大值");
                MyDefine.myXET.meTblVer7.addTableColumn("最小值");
                MyDefine.myXET.meTblVer7.addTableColumn("最大-最小");
                MyDefine.myXET.meTblVer7.addTableColumn("平均值");

                for (int irow = 0; irow < F0Table.dataTable.Rows.Count; irow++)
                {
                    Double F0Max = F0Table.GetRowMaxVal(irow, 1);       //行温度最大值
                    Double F0Min = F0Table.GetRowMinVal(irow, 1);       //行温度最小值
                    Double F0Avr = F0Table.GetRowAvrVal(irow, 1);       //行温度平均值

                    MyDefine.myXET.meTblVer7.SetCellValue(irow, "最大值", F0Max.ToString("F2"));
                    MyDefine.myXET.meTblVer7.SetCellValue(irow, "最小值", F0Min.ToString("F2"));
                    MyDefine.myXET.meTblVer7.SetCellValue(irow, "最大-最小", (F0Max - F0Min).ToString("F2"));
                    MyDefine.myXET.meTblVer7.SetCellValue(irow, "平均值", F0Avr.ToString("F2"));
                }

                #endregion

                #region 纵向数据汇总(列最大、最小、平均值等)

                //添加行
                //int rownum = MyDefine.myXET.meTblVer7.dataTable.Rows.Count;
                //MyDefine.myXET.meTblVer7.AddTableRow("最大值");
                //MyDefine.myXET.meTblVer7.AddTableRow("最小值");
                //MyDefine.myXET.meTblVer7.AddTableRow("最大-最小");
                //MyDefine.myXET.meTblVer7.AddTableRow("总  和");
                //MyDefine.myXET.meTblVer7.AddTableRow("平均值");

                ////逐列计算并写入列最大、最小、平均值等
                //for (int jcol = 1; jcol < F0Table.dataTable.Columns.Count; jcol++)     //F0Table第0列为Time
                //{
                //    Double max = F0Table.GetColumnMaxVal(jcol);        //求列最大值
                //    Double min = F0Table.GetColumnMinVal(jcol);        //求列最小值
                //    Double sum = F0Table.GetColumnSumVal(jcol);        //求列和
                //    Double avr = F0Table.GetColumnAvrVal(jcol);        //求列平均值

                //    //写入本列数据的纵向统计数据(列最大、最小、平均值等)
                //    MyDefine.myXET.meTblVer7.SetCellValue(rownum + 0, jcol, max.ToString("F2"));
                //    MyDefine.myXET.meTblVer7.SetCellValue(rownum + 1, jcol, min.ToString("F2"));
                //    MyDefine.myXET.meTblVer7.SetCellValue(rownum + 2, jcol, (max - min).ToString("F2"));
                //    MyDefine.myXET.meTblVer7.SetCellValue(rownum + 3, jcol, sum.ToString("F2"));
                //    MyDefine.myXET.meTblVer7.SetCellValue(rownum + 4, jcol, avr.ToString("F2"));
                //}

                #endregion

                #endregion

                #region 第n个有效数据区域(除第一个有效区域外的其它有效区域F0值统计)

                //统计后面可能存在的有效数据区间，最多统计10个区间(当没有找到有效数据区间时自动退出)
                for (int n = 0; n < 10; n++)
                {
                    #region 计算F0有效数据开始、结束索引

                    int rowIndx = tmpValidIdx2 + 1;
                    tmpValidIdx1 = int.MinValue;
                    tmpValidIdx2 = int.MinValue;

                    //计算F0有效数据开始、有效数据结束索引
                    for (int i = rowIndx; i < MyDefine.myXET.meDataTblTmp.dataTable.Rows.Count; i++)
                    {
                        double tmpMax = int.MinValue;      //行温度最大值
                        double tmpMin = int.MaxValue;      //行温度最小值

                        //计算本行数据的温度最大最小值
                        for (int j = 1; j < MyDefine.myXET.meDataTblTmp.dataTable.Columns.Count; j++)        //mytable第0列为Time
                        {
                            if (MyDefine.myXET.meDataTblTmp.GetCellValue(i, j) == "") continue;              //空数据(忽略空单元格)
                            double myVal = Convert.ToDouble(MyDefine.myXET.meDataTblTmp.GetCellValue(i, j)); //测试值

                            //计算行温度最大最小值
                            if (tmpMax < myVal) tmpMax = myVal;
                            if (tmpMin > myVal) tmpMin = myVal;
                        }

                        //判断是否为有效数据开始、结束索引
                        if (tmpValidIdx1 == int.MinValue)                   //尚未找到有效数据开始索引
                        {
                            if (tmpMax >= tmpSET) tmpValidIdx1 = i;         //判断当前行温度最大值是否达到设定阈值，是则设置当前行索引为有效数据开始索引
                        }
                        else if (tmpValidIdx2 == int.MinValue)              //已找到有效数据开始索引，尚未找到有效数据结束索引
                        {
                            if (tmpMax < tmpSET) tmpValidIdx2 = i;          //判断当前行温度最大值是否低于设定阈值，是则设置当前行索引为有效数据结束索引
                        }
                        else                                                //有效数据索引已找齐，退出循环
                        {
                            break;
                        }
                    }

                    if (tmpValidIdx1 == int.MinValue)                        //温度值一直未达到设定值
                    {
                        //MessageBox.Show("F0值计算失败：温度值未能达到阈值！", "系统提示！", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        //添加行
                        int rownum = MyDefine.myXET.meTblVer7.dataTable.Rows.Count;
                        dataTableClass finalF0Table = new dataTableClass();
                        finalF0Table = MyDefine.myXET.meTblVer7;
                        MyDefine.myXET.meTblVer7.AddTableRow("最大值");
                        MyDefine.myXET.meTblVer7.AddTableRow("最小值");
                        MyDefine.myXET.meTblVer7.AddTableRow("最大-最小");
                        MyDefine.myXET.meTblVer7.AddTableRow("总  和");
                        MyDefine.myXET.meTblVer7.AddTableRow("平均值");

                        //逐列计算并写入列最大、最小、平均值等
                        for (int jcol = 1; jcol < finalF0Table.dataTable.Columns.Count - 4; jcol++)     //F0Table第0列为Time, -4的原因是出去最大值等列
                        {
                            Double max = finalF0Table.GetColumnMaxVal(jcol);        //求列最大值
                            Double min = finalF0Table.GetColumnMinVal(jcol);        //求列最小值
                            Double sum = finalF0Table.GetColumnSumVal(jcol);        //求列和
                            Double avr = finalF0Table.GetColumnAvrVal(jcol);        //求列平均值

                            //写入本列数据的纵向统计数据(列最大、最小、平均值等)
                            MyDefine.myXET.meTblVer7.SetCellValue(rownum + 0, jcol, max.ToString("F2"));
                            MyDefine.myXET.meTblVer7.SetCellValue(rownum + 1, jcol, min.ToString("F2"));
                            MyDefine.myXET.meTblVer7.SetCellValue(rownum + 2, jcol, (max - min).ToString("F2"));
                            MyDefine.myXET.meTblVer7.SetCellValue(rownum + 3, jcol, sum.ToString("F2"));
                            MyDefine.myXET.meTblVer7.SetCellValue(rownum + 4, jcol, avr.ToString("F2"));
                        }
                        return true;
                    }

                    if (tmpValidIdx2 == int.MinValue) tmpValidIdx2 = MyDefine.myXET.meDataTblTmp.dataTable.Rows.Count - 1;       //有效数据结束为最后一个值

                    //记录第n组有效开始、有效结束索引
                    MyDefine.myXET.meF0ValidList.Add(tmpValidIdx1);
                    MyDefine.myXET.meF0ValidList.Add(tmpValidIdx2);

                    #endregion

                    #region 计算有效数据F0值

                    //添加列
                    F0Table = new dataTableClass();                 //定义一个表，用于存储计算的F0值表格
                    F0Table.addTableColumn("时间");
                    for (int i = 1; i < typeList.Count; i++)
                    {
                        if (typeList[i] == "TT_T" || typeList[i] == "TH_T" || typeList[i] == "TQ_T")
                        {
                            F0Table.addTableColumn(codelist[i]);
                        }
                    }

                    //计算温度F0值并写入meTblVer7
                    myf0 = double.MinValue;
                    myf0List.Clear();                                                       //F0值列表(存放一行数据计算出的所有温度F0值)
                    myf0List.Add(double.MinValue);                                          //添加一个空值，与meTblVer7的行结构保持一致

                    for (int irow = tmpValidIdx1; irow < tmpValidIdx2; irow++)              //有效数据开始行 -- 有效数据结束行
                    {
                        int idx = 0;                                                        //记录当前myf0List的索引
                        String myTime = MyDefine.myXET.meDataTblTmp.GetCellValue(irow, 0);     //测试时间

                        //计算本行数据中各列的F0值
                        for (int jcol = 1; jcol < MyDefine.myXET.meDataTblTmp.dataTable.Columns.Count; jcol++)     //mytable第0列为Time
                        {
                            //计算行温度F0值
                            idx++;
                            if (MyDefine.myXET.meDataTblTmp.GetCellValue(irow, jcol) == "")                        //空单元格的F0写double.MinValue
                            {
                                myf0 = double.MinValue;
                            }
                            else
                            {
                                Double myTmp = Convert.ToDouble(MyDefine.myXET.meDataTblTmp.GetCellValue(irow, jcol)); //测试值
                                myf0 = myspan * Math.Pow(10, (myTmp - tmpREF) / tmp_Z);                             //计算当前测试值的F0值
                            }

                            if (irow == tmpValidIdx1)           //第一行数据直接添加F0
                            {
                                myf0List.Add(myf0);
                            }
                            else                                //非第一行数据
                            {
                                //myf0List[idx] += myf0;        //非第一行数据累加F0
                                myf0List[idx] = myf0;           //非第一行数据直接赋值F0
                            }
                        }

                        //添加一行F0列表的数据
                        F0Table.AddTableRow();
                        F0Table.SetCellValue(irow - tmpValidIdx1, 0, myTime);        //写入测试时间
                        for (int k = 1; k < myf0List.Count; k++)
                        {
                            if (myf0List[k] != double.MinValue) F0Table.SetCellValue(irow - tmpValidIdx1, k, myf0List[k].ToString("F2"));    //写入各列在本行的F0值
                        }
                    }

                    #endregion

                    #region 横向数据添加(行最大、最小、平均值等)

                    dataTableClass myF0Table = new dataTableClass();                 //定义一个表，用于存储之前计算的F0值表格
                    myF0Table.dataTable = F0Table.CopyTable();

                    //添加列
                    myF0Table.addTableColumn("最大值");
                    myF0Table.addTableColumn("最小值");
                    myF0Table.addTableColumn("最大-最小");
                    myF0Table.addTableColumn("平均值");

                    for (int irow = 0; irow < F0Table.dataTable.Rows.Count; irow++)
                    {
                        Double F0Max = F0Table.GetRowMaxVal(irow, 1);       //行温度最大值
                        Double F0Min = F0Table.GetRowMinVal(irow, 1);       //行温度最小值
                        Double F0Avr = F0Table.GetRowAvrVal(irow, 1);       //行温度平均值

                        myF0Table.SetCellValue(irow, "最大值", F0Max.ToString("F2"));
                        myF0Table.SetCellValue(irow, "最小值", F0Min.ToString("F2"));
                        myF0Table.SetCellValue(irow, "最大-最小", (F0Max - F0Min).ToString("F2"));
                        myF0Table.SetCellValue(irow, "平均值", F0Avr.ToString("F2"));
                    }

                    MyDefine.myXET.meTblVer7.dataTable.Merge(myF0Table.dataTable);                  //将第二段F0值合并到F0值数据表

                    #endregion

                    #region 纵向数据汇总(列最大、最小、平均值等)

                    ////添加行
                    //rownum = MyDefine.myXET.meTblVer7.dataTable.Rows.Count;
                    //MyDefine.myXET.meTblVer7.AddTableRow("最大值");
                    //MyDefine.myXET.meTblVer7.AddTableRow("最小值");
                    //MyDefine.myXET.meTblVer7.AddTableRow("最大-最小");
                    //MyDefine.myXET.meTblVer7.AddTableRow("总  和");
                    //MyDefine.myXET.meTblVer7.AddTableRow("平均值");

                    ////逐列计算并写入列最大、最小、平均值等
                    //for (int jcol = 1; jcol < F0Table.dataTable.Columns.Count; jcol++)     //F0Table第0列为Time
                    //{
                    //    Double max = F0Table.GetColumnMaxVal(jcol);        //求列最大值
                    //    Double min = F0Table.GetColumnMinVal(jcol);        //求列最小值
                    //    Double sum = F0Table.GetColumnSumVal(jcol);        //求列和
                    //    Double avr = F0Table.GetColumnAvrVal(jcol);        //求列平均值

                    //    //写入本列数据的纵向统计数据(列最大、最小、平均值等)
                    //    MyDefine.myXET.meTblVer7.SetCellValue(rownum + 0, jcol, max.ToString("F2"));
                    //    MyDefine.myXET.meTblVer7.SetCellValue(rownum + 1, jcol, min.ToString("F2"));
                    //    MyDefine.myXET.meTblVer7.SetCellValue(rownum + 2, jcol, (max - min).ToString("F2"));
                    //    MyDefine.myXET.meTblVer7.SetCellValue(rownum + 3, jcol, sum.ToString("F2"));
                    //    MyDefine.myXET.meTblVer7.SetCellValue(rownum + 4, jcol, avr.ToString("F2"));
                    //}

                    #endregion
                }

                #endregion

                #region 若F0值为空则显示0.00
                /*
                for (int irow = 0; irow < F0Table.dataTable.Rows.Count; irow++)
                {
                    for (int jcol = 1; jcol < F0Table.dataTable.Columns.Count; jcol++)                      //F0Table第0列为Time,后面均为温度数据列
                    {
                        if (MyDefine.myXET.meTblVer7.GetCellValue(irow, jcol) == "")                        //空单元格的F0写0
                        {
                            MyDefine.myXET.meTblVer7.SetCellValue(irow, jcol, "0.00");
                        }
                    }
                }
                */
                #endregion

                MyDefine.myXET.AddTraceInfo("生成F0值数据表成功");
                return true;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("生成F0值数据表失败：" + ex.ToString());
                MyDefine.myXET.AddTraceInfo("生成F0值数据表失败");
                MyDefine.myXET.meTblVer7 = null;
                return false;
            }
        }

        #endregion

        #region F0值计算(仅温度需要计算)，计算有效数据

        public Boolean createDataTable7()
        {
            try
            {
                if (MyDefine.myXET.meDataTbl == null) return false;       //文件信息表为空，退出
                if (tmpNum <= 0)                                          //数据表中无温度数据
                {
                    MyDefine.myXET.meTblVer7 = null;
                    return false;
                }

                #region F0参数

                double baseTime = 1;                //时间基准(秒)
                double tmpREF = 0;                  //温度参比值TX
                double tmp_Z = 0;                   //Z值
                int tmpValidIdx1 = int.MinValue;    //F0有效数据开始
                int tmpValidIdx2 = int.MinValue;    //F0有效数据结束

                try
                {
                    tmpREF = Convert.ToDouble(textBox4.Text);                 //温度参比值TX
                    tmp_Z = Convert.ToDouble(textBox5.Text);                  //Z值
                    baseTime = (radioButton1.Checked) ? 60.0 : 1.0;           //时间基准(秒)
                }
                catch (Exception ex)
                {
                    MyDefine.myXET.ShowWrongMsg("F0参数填写错误：" + ex.ToString().Split('\n')[0]);
                    return false;
                }

                #endregion

                #region 第n个有效数据区域F0值统计

                //F0有效数据开始、有效数据结束索引
                MyDefine.myXET.meF0ValidList = MyDefine.myXET.meValidIdxList;
                //添加列
                MyDefine.myXET.meTblVer7 = new dataTableClass();

                //统计后面可能存在的有效数据区间，最多统计10个区间(当没有找到有效数据区间时自动退出)
                for (int n = 0; n < MyDefine.myXET.meF0ValidList.Count - 2; n += 2)
                {
                    #region 计算F0有效数据开始、结束索引

                    tmpValidIdx1 = MyDefine.myXET.meF0ValidList[n];
                    tmpValidIdx2 = MyDefine.myXET.meF0ValidList[n + 1];

                    #endregion

                    #region 计算有效数据F0值

                    //添加列
                    dataTableClass F0Table = new dataTableClass();                 //定义一个表，用于存储计算的F0值表格
                    F0Table.addTableColumn("时间");
                    for (int i = 1; i < typeList.Count; i++)
                    {
                        if (typeList[i] == "TT_T" || typeList[i] == "TH_T" || typeList[i] == "TQ_T")
                        {
                            F0Table.addTableColumn(codelist[i]);
                        }
                    }

                    //计算温度F0值并写入meTblVer7
                    Double myf0 = double.MinValue;
                    List<Double> myf0List = new List<Double>();                             //F0值列表(存放一行数据计算出的所有温度F0值)
                    Double myspan = Convert.ToDouble(MyDefine.myXET.homspan) / baseTime;    //测试间隔(分)
                    myf0List.Add(double.MinValue);                                          //添加一个空值，与meTblVer7的行结构保持一致

                    for (int irow = tmpValidIdx1; irow < tmpValidIdx2; irow++)              //有效数据开始行 -- 有效数据结束行
                    {
                        int idx = 0;                                                        //记录当前myf0List的索引
                        String myTime = MyDefine.myXET.meDataTblTmp.GetCellValue(irow, 0);     //测试时间

                        //计算本行数据中各列的F0值
                        for (int jcol = 1; jcol < MyDefine.myXET.meDataTblTmp.dataTable.Columns.Count; jcol++)     //mytable第0列为Time
                        {
                            //计算行温度F0值
                            idx++;
                            if (MyDefine.myXET.meDataTblTmp.GetCellValue(irow, jcol) == "")                        //空单元格的F0写double.MinValue
                            {
                                myf0 = double.MinValue;
                            }
                            else
                            {
                                Double myTmp = Convert.ToDouble(MyDefine.myXET.meDataTblTmp.GetCellValue(irow, jcol)); //测试值
                                myf0 = myspan * Math.Pow(10, (myTmp - tmpREF) / tmp_Z);                             //计算当前测试值的F0值
                            }

                            if (irow == tmpValidIdx1)           //第一行数据直接添加F0
                            {
                                myf0List.Add(myf0);
                            }
                            else                                //非第一行数据
                            {
                                //myf0List[idx] += myf0;        //非第一行数据累加F0
                                myf0List[idx] = myf0;           //非第一行数据直接赋值F0
                            }
                        }

                        //添加一行F0列表的数据
                        F0Table.AddTableRow();
                        F0Table.SetCellValue(irow - tmpValidIdx1, 0, myTime);        //写入测试时间
                        for (int k = 1; k < myf0List.Count; k++)
                        {
                            if (myf0List[k] != double.MinValue) F0Table.SetCellValue(irow - tmpValidIdx1, k, myf0List[k].ToString("F2"));    //写入各列在本行的F0值
                        }
                    }

                    #endregion

                    #region 横向数据添加(行最大、最小、平均值等)

                    dataTableClass myF0Table = new dataTableClass();                 //定义一个表，用于存储之前计算的F0值表格
                    myF0Table.dataTable = F0Table.CopyTable();

                    //添加列
                    myF0Table.addTableColumn("最大值");
                    myF0Table.addTableColumn("最小值");
                    myF0Table.addTableColumn("最大-最小");
                    myF0Table.addTableColumn("平均值");

                    for (int irow = 0; irow < F0Table.dataTable.Rows.Count; irow++)
                    {
                        Double F0Max = F0Table.GetRowMaxVal(irow, 1);       //行温度最大值
                        Double F0Min = F0Table.GetRowMinVal(irow, 1);       //行温度最小值
                        Double F0Avr = F0Table.GetRowAvrVal(irow, 1);       //行温度平均值

                        myF0Table.SetCellValue(irow, "最大值", F0Max.ToString("F2"));
                        myF0Table.SetCellValue(irow, "最小值", F0Min.ToString("F2"));
                        myF0Table.SetCellValue(irow, "最大-最小", (F0Max - F0Min).ToString("F2"));
                        myF0Table.SetCellValue(irow, "平均值", F0Avr.ToString("F2"));
                    }

                    MyDefine.myXET.meTblVer7.dataTable.Merge(myF0Table.dataTable);                  //将第二段F0值合并到F0值数据表

                    #endregion

                    #region 纵向数据汇总(列最大、最小、平均值等)

                    //添加行
                    //添加行
                    int rownum = MyDefine.myXET.meTblVer7.dataTable.Rows.Count;
                    MyDefine.myXET.meTblVer7.AddTableRow("最大值");
                    MyDefine.myXET.meTblVer7.AddTableRow("最小值");
                    MyDefine.myXET.meTblVer7.AddTableRow("最大-最小");
                    MyDefine.myXET.meTblVer7.AddTableRow("总  和");
                    MyDefine.myXET.meTblVer7.AddTableRow("平均值");

                    //逐列计算并写入列最大、最小、平均值等
                    for (int jcol = 1; jcol < F0Table.dataTable.Columns.Count; jcol++)     //F0Table第0列为Time
                    {
                        Double max = F0Table.GetColumnMaxVal(jcol);        //求列最大值
                        Double min = F0Table.GetColumnMinVal(jcol);        //求列最小值
                        Double sum = F0Table.GetColumnSumVal(jcol);        //求列和
                        Double avr = F0Table.GetColumnAvrVal(jcol);        //求列平均值

                        //写入本列数据的纵向统计数据(列最大、最小、平均值等)
                        MyDefine.myXET.meTblVer7.SetCellValue(rownum + 0, jcol, max.ToString("F2"));
                        MyDefine.myXET.meTblVer7.SetCellValue(rownum + 1, jcol, min.ToString("F2"));
                        MyDefine.myXET.meTblVer7.SetCellValue(rownum + 2, jcol, (max - min).ToString("F2"));
                        MyDefine.myXET.meTblVer7.SetCellValue(rownum + 3, jcol, sum.ToString("F2"));
                        MyDefine.myXET.meTblVer7.SetCellValue(rownum + 4, jcol, avr.ToString("F2"));
                    }

                    #endregion
                }

                #endregion

                MyDefine.myXET.AddTraceInfo("生成F0值数据表成功");
                return true;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("生成F0值数据表失败：" + ex.ToString());
                MyDefine.myXET.AddTraceInfo("生成F0值数据表失败");
                MyDefine.myXET.meTblVer7 = null;
                return false;
            }
        }

        #endregion

        #region 报告有效数据汇总

        /// <summary>
        /// 报告有效数据汇总，加最大值、最小值、差值、平均值
        /// </summary>
        public Boolean createDataTable8()
        {
            if (MyDefine.myXET.meInfoTbl == null) return false;         //文件信息表为空，退出
            if (MyDefine.myXET.meValidStageNum == 0) return false;      //有效数据阶段未设定

            try
            {
                #region 初始化列表
                //添加列
                MyDefine.myXET.meTblVer8 = new dataTableClass();
                MyDefine.myXET.meTblVer8.addTableColumn("有效阶段");         //第0栏
                MyDefine.myXET.meTblVer8.addTableColumn("设备数量");         //第1栏
                MyDefine.myXET.meTblVer8.addTableColumn("设备类型");         //第2栏
                MyDefine.myXET.meTblVer8.addTableColumn("记录间隔");         //第3栏
                MyDefine.myXET.meTblVer8.addTableColumn("记录数");           //第4栏
                MyDefine.myXET.meTblVer8.addTableColumn("启动时间");         //第5栏
                MyDefine.myXET.meTblVer8.addTableColumn("结束时间");         //第6栏
                MyDefine.myXET.meTblVer8.addTableColumn("最大值");           //第7栏
                MyDefine.myXET.meTblVer8.addTableColumn("最大值出现时间");   //第8栏
                MyDefine.myXET.meTblVer8.addTableColumn("最小值");           //第9栏
                MyDefine.myXET.meTblVer8.addTableColumn("最小值出现时间");   //第10栏
                MyDefine.myXET.meTblVer8.addTableColumn("差值");             //第11栏
                MyDefine.myXET.meTblVer8.addTableColumn("平均值");           //第12栏
                MyDefine.myXET.meTemVer8 = new dataTableClass();
                MyDefine.myXET.meTemVer8.dataTable = MyDefine.myXET.meTblVer8.CopyTable();
                MyDefine.myXET.meHumVer8 = new dataTableClass();
                MyDefine.myXET.meHumVer8.dataTable = MyDefine.myXET.meTblVer8.CopyTable();
                MyDefine.myXET.mePrsVer8 = new dataTableClass();
                MyDefine.myXET.mePrsVer8.dataTable = MyDefine.myXET.meTblVer8.CopyTable();
                #endregion
                getDataTypeList();      //获取数据表每列的数据类型：TT_T / TH_T / TH_H / TP_P

                for (int Pn = 0; Pn < MyDefine.myXET.meValidStageNum; Pn++)    //遍历有效阶段
                {
                    string colName = MyDefine.myXET.meDataTmpTbl.dataTable.Columns[3].ColumnName;

                    int listIdx = Pn * 2;                                                                      //阶段Pn在meValidIdxList中的存储开始索引
                    int validIdx1 = MyDefine.myXET.meValidIdxList[listIdx];                                    //有效开始索引
                    int validIdx2 = MyDefine.myXET.meValidIdxList[listIdx + 1];                                //有效结束索引
                    String myStartTime = MyDefine.myXET.meValidTimeList[listIdx].ToString("MM-dd HH:mm:ss");   //有效数据开始时间
                    String myStopTime = MyDefine.myXET.meValidTimeList[listIdx + 1].ToString("MM-dd HH:mm:ss");//有效数据结束时间
                    String myDateNum = (validIdx2 - validIdx1 + 1).ToString();                                 //有效数据个数(记录数)
                    String myStage = MyDefine.myXET.meValidNameList[Pn]; //有效阶段

                    string periodMaxValue = "";                          //阶段最大值
                    string periodMinValue = "";                          //阶段最小值
                    string periodDifValue = "";                          //阶段差值
                    string periodAvgValue = "";                          //阶段平均值
                    String periodMaxTime = "";                               //阶段最大值的时间
                    String periodMinTime = "";                               //阶段最小值的时间
                    DateTime myStartTimeDT = Convert.ToDateTime(MyDefine.myXET.meValidTimeList[listIdx]);   //有效数据开始时间
                    DateTime myStopTimeDT = Convert.ToDateTime(MyDefine.myXET.meValidTimeList[listIdx + 1]);//有效数据结束时间

                    //间隔时间
                    int intspan = MyDefine.myXET.meSpanTime;
                    String mySpanTime = intspan.ToString() + "s";
                    if (intspan >= 60) mySpanTime = (intspan / 60.0).ToString("F2") + "min";
                    if (intspan >= 3600) mySpanTime = (intspan / 3600.0).ToString("F2") + "h";

                    //添加阶段Pn的温度行
                    if (typeList.Contains("TT_T") || typeList.Contains("TH_T") || typeList.Contains("TQ_T"))
                    {
                        String myType = "温度(" + MyDefine.myXET.temUnit + ")";                                 //产品类型
                        String myNum = tmpNum.ToString();                                                       //产品数量

                        // 使用LINQ查询计算阶段最大值、最小值、最大最小的差值和平均值
                        var resultTem = from row in MyDefine.myXET.meTemVer5.dataTable.AsEnumerable()
                                        group row by row.Field<string>("开始时间") into g
                                        select new
                                        {
                                            myStartTime = g.Key,
                                            periodMaxValue = g.Max(row => double.Parse(row.Field<string>("最大值"))).ToString("F2"),
                                            periodMinValue = g.Min(row => double.Parse(row.Field<string>("最小值"))).ToString("F2"),
                                            periodDifValue = (g.Max(row => double.Parse(row.Field<string>("最大值"))) - g.Min(row => double.Parse(row.Field<string>("最小值")))).ToString("F2"),
                                            periodAvgValue = g.Average(row => double.Parse(row.Field<string>("平均值"))).ToString("F2")
                                        };

                        foreach (var item in resultTem)
                        {
                            if (item.myStartTime == myStartTime)
                            {
                                periodMaxValue = item.periodMaxValue;
                                periodMinValue = item.periodMinValue;
                                periodDifValue = item.periodDifValue;
                                periodAvgValue = item.periodAvgValue;
                            }
                        }

                        foreach (DataRow row in MyDefine.myXET.meDataTmpTbl.dataTable.Rows)
                        {
                            string minValue = row["最小值"].ToString();
                            if (minValue == periodMinValue)
                            {
                                DateTime timeDT = Convert.ToDateTime(row["时间"]);
                                if (timeDT >= myStartTimeDT && timeDT <= myStopTimeDT)
                                {
                                    periodMinTime = timeDT.ToString("MM-dd HH:mm:ss");
                                    break;
                                }
                            }
                        }

                        foreach (DataRow row in MyDefine.myXET.meDataTmpTbl.dataTable.Rows)
                        {
                            string maxValue = row["最大值"].ToString();
                            if (maxValue == periodMaxValue)
                            {
                                DateTime timeDT = Convert.ToDateTime(row["时间"]);
                                if (timeDT >= myStartTimeDT && timeDT <= myStopTimeDT)
                                {
                                    periodMaxTime = timeDT.ToString("MM-dd HH:mm:ss");
                                    break;
                                }
                            }
                        }

                        MyDefine.myXET.meTemVer8.AddTableRow(new string[] { myStage, myNum, myType, mySpanTime, myDateNum, myStartTime, myStopTime, periodMaxValue, periodMaxTime, periodMinValue, periodMinTime, periodDifValue, periodAvgValue });
                    }
                    //添加阶段Pn的湿度行
                    if (typeList.Contains("TH_H") || typeList.Contains("TQ_H"))
                    {
                        String myType = "湿度(%RH)";                                                            //产品类型
                        String myNum = humNum.ToString();                                                       //产品数量

                        // 使用LINQ查询计算最大值、最小值、最大最小的差值和平均值
                        var resultHum = from row in MyDefine.myXET.meHumVer5.dataTable.AsEnumerable()
                                        group row by row.Field<string>("开始时间") into g
                                        select new
                                        {
                                            myStartTime = g.Key,
                                            periodMaxValue = g.Max(row => double.Parse(row.Field<string>("最大值"))).ToString("F2"),
                                            periodMinValue = g.Min(row => double.Parse(row.Field<string>("最小值"))).ToString("F2"),
                                            periodDifValue = (g.Max(row => double.Parse(row.Field<string>("最大值"))) - g.Min(row => double.Parse(row.Field<string>("最小值")))).ToString("F2"),
                                            periodAvgValue = g.Average(row => double.Parse(row.Field<string>("平均值"))).ToString("F2")
                                        };

                        foreach (var item in resultHum)
                        {
                            if (item.myStartTime == myStartTime)
                            {
                                periodMaxValue = item.periodMaxValue;
                                periodMinValue = item.periodMinValue;
                                periodDifValue = item.periodDifValue;
                                periodAvgValue = item.periodAvgValue;
                            }
                        }

                        foreach (DataRow row in MyDefine.myXET.meDataHumTbl.dataTable.Rows)
                        {
                            string minValue = row["最小值"].ToString();
                            if (minValue == periodMinValue)
                            {
                                DateTime timeDT = Convert.ToDateTime(row["时间"]);
                                if (timeDT >= myStartTimeDT && timeDT <= myStopTimeDT)
                                {
                                    periodMinTime = timeDT.ToString("MM-dd HH:mm:ss");
                                    break;
                                }
                            }
                        }

                        foreach (DataRow row in MyDefine.myXET.meDataHumTbl.dataTable.Rows)
                        {
                            string maxValue = row["最大值"].ToString();
                            if (maxValue == periodMaxValue)
                            {
                                DateTime timeDT = Convert.ToDateTime(row["时间"]);
                                if (timeDT >= myStartTimeDT && timeDT <= myStopTimeDT)
                                {
                                    periodMaxTime = timeDT.ToString("MM-dd HH:mm:ss");
                                    break;
                                }
                            }
                        }

                        MyDefine.myXET.meHumVer8.AddTableRow(new string[] { myStage, myNum, myType, mySpanTime, myDateNum, myStartTime, myStopTime, periodMaxValue, periodMaxTime, periodMinValue, periodMinTime, periodDifValue, periodAvgValue });
                    }
                    //添加阶段Pn的压力行
                    if (typeList.Contains("TP_P"))
                    {
                        String myType = "压力(kPa)";                                                            //产品类型
                        String myNum = prsNum.ToString();                                                       //产品数量
                        MyDefine.myXET.mePrsVer8.AddTableRow(new string[] { myStage, myNum, myType, mySpanTime, myDateNum, myStartTime, myStopTime });
                    }
                }
                MyDefine.myXET.meTblVer8.AddTable(MyDefine.myXET.meTemVer8.dataTable);
                MyDefine.myXET.meTblVer8.AddTable(MyDefine.myXET.meHumVer8.dataTable);
                MyDefine.myXET.meTblVer8.AddTable(MyDefine.myXET.mePrsVer8.dataTable);

                MyDefine.myXET.AddTraceInfo("生成有效数据汇总表成功");

                return true;
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("生成有效数据汇总表失败:" + ex.ToString());
                MyDefine.myXET.AddTraceInfo("生成有效数据汇总表失败");
                MyDefine.myXET.meTblVer8 = null;
                return false;
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
            dataGridViewCellStyle2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
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

        #region 更新数据表所需信息（TT_T / TH_T / TH_H / TP_P）

        public int tmpNum = 0;                 //温度列数量
        public int humNum = 0;                 //湿度列数量
        public int prsNum = 0;                 //压力列数量
        public int DUTNum = 0;                 //产品总数
        List<String> codelist = new List<String>(); //出厂编号列表
        List<String> typeList = new List<String>(); //数据类型列表TT_T / TH_T / TH_H / TP_P
        List<int> codeNumlist = new List<int>();    //数量编号列表，指明当前产品是第几个产品(HTH产品有两列，但两列是一个产品) -- 应该用不到

        /// <summary>
        /// 数据表各列数据类型划分,把所有数据列数据类型划分为四种： TT_T / TH_T / TH_H / TP_P
        /// </summary>
        public void getDataTypeList()
        {
            //更新数据表所需信息
            tmpNum = MyDefine.myXET.meTMPNum;
            humNum = MyDefine.myXET.meHUMNum;
            prsNum = MyDefine.myXET.mePRSNum;
            DUTNum = MyDefine.myXET.meDUTNum;
            codelist = MyDefine.myXET.meJSNList;
            typeList = MyDefine.myXET.meTypeList;
            codeNumlist = MyDefine.myXET.codeNumlist;
        }


        #endregion

        #region 数据表显示

        public void ShowTheTable(dataTableClass mytable)
        {
            //数据表为空，显示空白表
            if (mytable == null)
            {
                createBlankDataTable();
                return;
            }

            //行数不满20则添加空白行
            int rowcount = mytable.dataTable.Rows.Count - 1;
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
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;       //自动填满整个dataGridView宽度
            dataGridView1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;            //垂直滚动条
            dataGridView1.ClearSelection(); //清除单元格选中状态(放在添加列后才起作用)

            //禁止列排序
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            //列数过多，则设置列宽并显示横向滚动条
            if (dataGridView1.Columns.Count > 6)
            {
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;   //不自动填满整个dataGridView宽度
                dataGridView1.ScrollBars = System.Windows.Forms.ScrollBars.Both;            //水平、垂直滚动条

                //设置列宽
                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    dataGridView1.Columns[i].Width = 200;
                }
            }
        }

        #endregion

        #region 数据表显示切换

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    ShowTheTable(MyDefine.myXET.meTblVer);
                    break;
                case 1:
                    ShowTheTable(MyDefine.myXET.meTblVer8);
                    break;
                case 2:
                    ShowTheTable(MyDefine.myXET.meTblVer2);
                    break;
                case 3:
                    ShowTheTable(MyDefine.myXET.meTblVer3);
                    break;
                case 4:
                    ShowTheTable(MyDefine.myXET.meTblVer4);
                    break;
                case 5:
                    ShowTheTable(MyDefine.myXET.meTblVer5);
                    break;
                case 6:
                    ShowTheTable(MyDefine.myXET.meTblVer6);
                    break;
                case 7:
                    ShowTheTable(MyDefine.myXET.meTblVer7);
                    break;
            }
        }

        #endregion

        #region 文本框输入限制

        //数据输入
        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            //只允许输入数字,负号,小数点和删除键,和空格(时间栏需要)
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != '.') && (e.KeyChar != ':') && (e.KeyChar != '-') && (e.KeyChar != 8) && (e.KeyChar != ' '))
            {
                e.Handled = true;
                return;
            }

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

            //正数第一位是0,第二位必须为小数点
            if ((e.KeyChar != '.') && (e.KeyChar != 8) && (((TextBox)sender).Text == "0"))
            {
                e.Handled = true;
                return;
            }
        }

        #endregion

    }
}
