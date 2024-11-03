using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace HTR
{
    public partial class MenuReportPanel : UserControl
    {
        //dataTableClass meDataTbl;     //测试数据加载数据表
        String rptdate;     //温度数据的原始记录时间
        String rpttime;     //温度数据的开始时间(时分秒)
        String rptstop;     //温度数据的结束时间(时分秒)
        String rptspan;     //温度数据的测试间隔(秒)
        String rptrun;      //温度数据的总长时间(秒)
        MenuCalPanel myCalPanel = new MenuCalPanel();

        private BackgroundWorker workerCreatePdfPart1;     //生成验证报告第一部分
        private BackgroundWorker workerCreatePdfPart2;     //生成验证报告第二部分

        public MenuReportPanel()
        {
            InitializeComponent();

            //屏蔽跨线程修改控件属性的异常
            CheckForIllegalCrossThreadCalls = false;

            // 初始化BackgroundWorker组件
            workerCreatePdfPart1 = new BackgroundWorker();
            workerCreatePdfPart1.DoWork += workerCreatePdfPart1_DoWork;
            workerCreatePdfPart1.RunWorkerCompleted += workerCreatePdfPart1_RunWorkerCompleted;

            workerCreatePdfPart2 = new BackgroundWorker();
            workerCreatePdfPart2.DoWork += workerCreatePdfPart2_DoWork;
            workerCreatePdfPart2.RunWorkerCompleted += workerCreatePdfPart2_RunWorkerCompleted;
        }

        //仅第一次打开报告界面时执行
        private void MenuReportPanel_Load(object sender, EventArgs e)
        {
            MyDefine.myXET.repcode = updateReportCode(false);   //更新报告编号
            showReportCode();                                   //显示报告编号
        }

        //每次打开报告界面均执行
        public void AddMyUpdateEvent()
        {
            textBox6.Text = MyDefine.myXET.homdate;                     //校准日期=加载的报告的日期
            textBox8.Text = DateTime.Now.ToString("yyyy-MM-dd");        //审核日期=当前日期
            if (MyDefine.myXET.reportPicName != "" && MyDefine.myXET.reportPicName != null)
            {
                textBox9.Text = MyDefine.myXET.userPIC + @"\" + MyDefine.myXET.reportPicName + ".gif";
            }
            //comboBox2.SelectedIndex = (this.Name == "校准报告") ? 0 : 2;        //报告类型

            if (this.Name == "校准报告")            //报告类型
            {
                label6.Text = "校准人员";
                label7.Text = "校准日期";

                comboBox2.Items.Clear();
                comboBox2.Items.Add("前校准|Pre-calibration");
                comboBox2.Items.Add("后校验|Post-checksum");
                comboBox2.SelectedIndex = 0;

                groupBox2.Visible = false;
                groupBox3.Visible = false;
                groupBox4.Visible = false;
                checkDataSelect.Visible = false;
            }
            else
            {
                label6.Text = "验证人员";
                label7.Text = "验证日期";

                comboBox2.Items.Clear();
                comboBox2.Items.Add("验证|Validation");
                comboBox2.SelectedIndex = 0;

                groupBox2.Visible = true;
                groupBox3.Visible = true;
                groupBox4.Visible = true;
                checkDataSelect.Visible = true;

                int count = 0;
                if (MyDefine.myXET.drawTemCurve)
                {
                    checkBox1.Checked = true;
                    checkBox1.Enabled = true;
                    count++;
                }
                else
                {
                    checkBox1.Checked = false;
                    checkBox1.Enabled = false;
                }

                if (MyDefine.myXET.drawHumCurve)
                {
                    checkBox2.Checked = true;
                    checkBox2.Enabled = true;
                    count++;
                }
                else
                {
                    checkBox2.Checked = false;
                    checkBox2.Enabled = false;
                }

                if (MyDefine.myXET.drawPrsCurve)
                {
                    checkBox3.Checked = true;
                    checkBox3.Enabled = true;
                    count++;
                }
                else
                {
                    checkBox3.Checked = false;
                    checkBox3.Enabled = false;
                }

                if (count > 1)
                {
                    radioButton4.Enabled = true;
                }
                else
                {
                    radioButton4.Enabled = false;
                }
            }
        }

        #region 界面按钮

        //生成报告
        private void button1_Click(object sender, EventArgs e)
        {
            /*
            //保存报告路径和文件名
            if (!Directory.Exists(MyDefine.myXET.userOUT))
            {
                Directory.CreateDirectory(MyDefine.myXET.userOUT);
            }

            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Title = "请选择文件";
            fileDialog.Filter = "新建文件(*.pdf)|*.pdf";
            fileDialog.RestoreDirectory = true;
            fileDialog.InitialDirectory = MyDefine.myXET.userOUT;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(fileDialog.FileName))
                {
                    File.Delete(fileDialog.FileName);
                }

                updateStandardTable();      //根据当前界面信息更新标准器信息表，以便打印到pdf中

                //报告打印
                if (comboBox2.SelectedItem.ToString().Contains("验证"))     //验证报告
                {
                    createPDFReportVerify(fileDialog.FileName);             //打印验证报告
                }
                else                                                        //前校准或后校验
                {
                    createPDFReport(fileDialog.FileName);                   //前校准或后校验PDF报告打印
                }

                saveReportInfo();           //保存报告编号及标准器信息表
                MyDefine.myXET.repcode = updateReportCode();         //打印后更新报告编号（报告编号+1），以便下一次PDF打印
                showReportCode();           //显示更新后的报告编号
            }
            else
            {
                return;
            }
            */

            label1.Focus(); Application.DoEvents();          //将焦点从button上移走，使button每次单击都有点击效果

            //文件夹不存在则创建文件夹
            if (!Directory.Exists(MyDefine.myXET.userOUT))
            {
                Directory.CreateDirectory(MyDefine.myXET.userOUT);
            }

            String myPath = MyDefine.myXET.userOUT;
            String fileName = textBox11.Text;
            String myfile = "";
            String myfilePart1 = "";
            String myfilePart2 = "";

            bool isCheckedDataSelect = checkDataSelect.Checked;       //是否选择部分数据

            if (this.Name == "验证报告")                              //验证报告
            {
                if (isCheckedDataSelect || togetherPDF)               //部分数据、全部数据, 不分两份
                {
                    myfile = myPath + "\\" + fileName + ".pdf";       //文件路径
                }
                else if (!isCheckedDataSelect && !togetherPDF)        //全部数据, 分两份
                {
                    myfilePart1 = myPath + "\\" + fileName + "_第一部分_汇总及阶段数据.pdf";   //文件路径
                    myfilePart2 = myPath + "\\" + fileName + "_第二部分_完整数据.pdf";         //文件路径
                }
            }
            else                                                      //前校准或后校验
            {
                myfile = myPath + "\\" + fileName + ".pdf";           //文件路径
            }

            if (fileName.Trim() == "")
            {
                MessageBox.Show("文件名为空，请输入文件名！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (File.Exists(myfile))
            {
                MessageBox.Show(System.IO.Path.GetFileName(myfile) + "文件已存在。" + Environment.NewLine + "请重新输入文件名！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            else if (File.Exists(myfilePart1))
            {
                MessageBox.Show(System.IO.Path.GetFileName(myfilePart1) + "文件已存在。" + Environment.NewLine + "请重新输入文件名！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            else if (File.Exists(myfilePart2))
            {
                MessageBox.Show(System.IO.Path.GetFileName(myfilePart2) + "文件已存在。" + Environment.NewLine + "请重新输入文件名！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            //报告打印
            button1.Text = "生成中...";
            button1.Enabled = false;
            Application.DoEvents();
            MyDefine.myXET.AddTraceInfo("生成报告");

            MyDefine.myXET.createPdfTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Boolean ret = true;
            if (this.Name == "验证报告")                              //验证报告
            {
                if (isCheckedDataSelect || togetherPDF)               //部分数据、全部数据, 不分两份
                {
                    ret = createPDFReportVerify(myfile);              //选择部分数据或全部数据不合并时，只打印一份报告
                }
                else if (!isCheckedDataSelect && !togetherPDF)        //全部数据, 分两份
                {
                    workerCreatePdfPart1.RunWorkerAsync(myfilePart1);
                    workerCreatePdfPart2.RunWorkerAsync(myfilePart2);
                }
            }
            else                                                     //前校准或后校验
            {
                ret = createPDFReportCali(myfile);                   //前校准或后校验PDF报告打印
            }

            if (ret)     //生成报告成功，保存报告信息
            {
                //文件名称;报告编号;报告类型;报告名称;报告日期;操作人员;审核人员;查看
                String pdfInfo = textBox11.Text + ";" + MyDefine.myXET.repcode + ";" + comboBox2.Text.Split('|')[0] + ";" + textBox2.Text + ";" + textBox8.Text + ";" + textBox5.Text + ";" + textBox7.Text + ";" + "查看";
                MyDefine.myXET.SavePDFList(pdfInfo);                        //保存到pdf信息列表
            }

            if (this.Name == "验证报告")
            {
                if (isCheckedDataSelect || togetherPDF)
                {
                    button1.Text = "生成报告";
                    button1.Enabled = true;
                }
            }
            else
            {
                button1.Text = "生成报告";
                button1.Enabled = true;
            }
        }

        //加载图片
        private void button2_Click(object sender, EventArgs e)
        {
            loadPicture();
        }

        //加载logo图片
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                loadLogoPicture();
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        #endregion

        #region 生成PDF报告

        #region 前校准、后校验报告

        public Boolean createPDFReportCali(string filepath)
        {
            try
            {
                //使用
                String blankLine = " ";
                String mDeviceType = "";
                String mDeviceID = "";
                PdfPTable pdftable;
                float[] colWidth;       //列宽比例数组
                List<String> myLS = new List<String>();
                dataTableClass mCaliTbl = new dataTableClass();
                iTextSharp.text.Image separator;

                #region 数据来源

                rptdate = MyDefine.myXET.homdate;
                rpttime = MyDefine.myXET.homstart;
                rptstop = MyDefine.myXET.homstop;
                rptspan = MyDefine.myXET.homspan;
                rptrun = MyDefine.myXET.homrun;
                mDeviceID = MyDefine.myXET.hom_Model;//MyDefine.myXET.meModel;
                mDeviceType = MyDefine.myXET.hom_Type;

                #endregion

                #region 校准表来源

                if (MyDefine.myXET.meTblPre1 == null)
                {
                    MessageBox.Show("前校准数据表1为空，请在校准页面填写校准信息！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meTblPre2 == null)
                {
                    MessageBox.Show("前校准数据表2为空，请在校准页面填写校准信息！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                mCaliTbl.dataTable = MyDefine.myXET.meTblPre1.CopyTable();
                mCaliTbl.dataTable.Merge(MyDefine.myXET.meTblPre2.dataTable, false, MissingSchemaAction.AddWithKey);

                #endregion

                #region 参数分配

                //基本信息

                String companyName = textBox1.Text;         //公司名称
                String reportNameCH = textBox2.Text;        //报告名称-中文
                String reportNameEN = textBox3.Text;        //报告名称-英文
                //String reportCode = textBox4.Text;          //报告编号
                String reportTypeCH = comboBox2.SelectedItem.ToString().Split('|')[0];//报告类型
                String reportTypeEN = comboBox2.SelectedItem.ToString().Split('|')[1];//报告类型
                String picPath = textBox9.Text;             //图片路径
                String dataType = MyDefine.myXET.homunit;          //℃/%RH/kPa
                int signMode = radioButton1.Checked ? 1 : 2;        //签字模式：首末页签字、全部签字

                //记录间隔单位转换
                int intspan = Convert.ToInt32(rptspan);
                rptspan = rptspan + " 秒";
                if (intspan % 60 == 0) rptspan = (intspan / 60).ToString() + " 分";
                if (intspan % 3600 == 0) rptspan = (intspan / 3600).ToString() + " 时";

                //运行时长单位转换
                int intrun = Convert.ToInt32(rptrun);
                rptrun = rptrun + " 秒";
                if (intrun >= 60) rptrun = (intrun / 60).ToString("F2") + " 分";
                if (intrun >= 3600) rptrun = (intrun / 3600).ToString("F2") + " 时";

                //页眉页脚信息
                MyDefine.myXET.logopath = textBox10.Text;           //Logo图片
                MyDefine.myXET.operaMem = MyDefine.myXET.userName;  //操作人员
                MyDefine.myXET.calibMem = textBox5.Text;            //校准人员
                MyDefine.myXET.calibDate = textBox6.Text;           //校准日期
                MyDefine.myXET.reviewMem = textBox7.Text;           //审核人员
                MyDefine.myXET.reviewDate = textBox8.Text;          //审核日期

                #endregion

                #region 参数完整性检查

                if (picPath != "" && !File.Exists(picPath))
                {
                    MessageBox.Show("图片路径错误，请重新选择图片！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.logopath != "" && !File.Exists(MyDefine.myXET.logopath))
                {
                    MessageBox.Show("Logo路径错误，请重新选择图片！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meDataTbl == null)
                {
                    MessageBox.Show("尚未加载测试数据，请加载数据文件！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meTblSTD == null)
                {
                    MessageBox.Show("标准器信息为空，请填写标准器列表！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                #endregion

                #region 更新报告编号(需放在页眉页脚前)

                //更新报告编号
                MyDefine.myXET.repcode = updateReportCode();                //更新报告编号（报告编号+1）
                MyDefine.myXET.saveReportInfo();                            //保存报告编号及标准器信息表
                showReportCode();                                           //显示更新后的报告编号

                MyDefine.myXET.repcode = "JDYZ" + (DateTime.Now.Ticks / 100000000).ToString();       //根据系统时间实时生成报告编号
                #endregion

                #region 字体及页眉页脚定义

                //创建新文档对象,页边距(X,X,Y,Y)
                Document document = new Document(PageSize.A4, 40, 40, 65, 80);

                //路径设置; FileMode.Create文档不在会创建，存在会覆盖
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filepath, FileMode.Create));

                //当前登录账号的类别名称
                string name = MyDefine.myXET.meLoginUser.Split(';')[(int)ACCOUNT.USER];
                //是否有编辑PDF的权限
                if (MyDefine.myXET.CheckPermission(STEP.数据载入, false))
                {
                    writer.SetEncryption(null, Encoding.Default.GetBytes(name + "JD"), PdfWriter.ALLOW_PRINTING, PdfWriter.ENCRYPTION_AES_128 | PdfWriter.DO_NOT_ENCRYPT_METADATA);

                    //账户为汉字则启用超级密码
                    foreach (char item in name)
                    {
                        if ((int)item > 127)
                        {
                            writer.SetEncryption(null, Encoding.Default.GetBytes("JD20191206"), PdfWriter.ALLOW_PRINTING, PdfWriter.ENCRYPTION_AES_128 | PdfWriter.DO_NOT_ENCRYPT_METADATA);
                            break;
                        }
                    }
                }
                else
                {
                    writer.SetEncryption(null, null, PdfWriter.ALLOW_PRINTING, PdfWriter.ENCRYPTION_AES_128 | PdfWriter.DO_NOT_ENCRYPT_METADATA);
                }
                //writer.SetEncryption(Encoding.Default.GetBytes("Hello"), Encoding.Default.GetBytes("123456"), PdfWriter.ALLOW_SCREENREADERS, PdfWriter.STANDARD_ENCRYPTION_128);

                //添加信息
                document.AddTitle("HTR校准报告");
                document.AddAuthor(companyName);
                document.AddSubject(mDeviceType + " 温度测量报告");
                document.AddKeywords("HTR");
                document.AddCreator(MyDefine.myXET.operaMem);

                //创建字体，STSONG.TTF空格不等宽
                //iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 14.0f, iTextSharp.text.Font.BOLD);
                //iTextSharp.text.Font fontItem = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 12.0f, iTextSharp.text.Font.BOLD);
                //iTextSharp.text.Font fontContent = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 10.0f, iTextSharp.text.Font.NORMAL);
                //iTextSharp.text.Font fontMessage = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL);
                //iTextSharp.text.Font fontTable = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 8.0f, iTextSharp.text.Font.NORMAL);
                //iTextSharp.text.Font fontFooter = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL, BaseColor.GRAY);
                //iTextSharp.text.Font fontTiny = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 6.0f, iTextSharp.text.Font.NORMAL);

                iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 14.0f, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font fontItem = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 12.0f, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font fontContent = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 10.0f, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font fontMessage = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font fontTable = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 8.0f, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font fontFooter = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL, BaseColor.GRAY);
                iTextSharp.text.Font fontTiny = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 6.0f, iTextSharp.text.Font.NORMAL);

                //iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(BaseFont.CreateFont(@"c:\windows\fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 14.0f, iTextSharp.text.Font.BOLD);
                //iTextSharp.text.Font fontItem = new iTextSharp.text.Font(BaseFont.CreateFont(@"c:\windows\fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 12.0f, iTextSharp.text.Font.BOLD);
                //iTextSharp.text.Font fontContent = new iTextSharp.text.Font(BaseFont.CreateFont(@"c:\windows\fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 10.0f, iTextSharp.text.Font.NORMAL);
                //iTextSharp.text.Font fontMessage = new iTextSharp.text.Font(BaseFont.CreateFont(@"c:\windows\fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL);
                //iTextSharp.text.Font fontTable = new iTextSharp.text.Font(BaseFont.CreateFont(@"C:\Windows\Fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 8.0f, iTextSharp.text.Font.NORMAL);
                //iTextSharp.text.Font fontFooter = new iTextSharp.text.Font(BaseFont.CreateFont(@"C:\Windows\Fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL, BaseColor.GRAY);
                //iTextSharp.text.Font fontTiny = new iTextSharp.text.Font(BaseFont.CreateFont(@"c:\windows\fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 6.0f, iTextSharp.text.Font.NORMAL);

                //添加页眉页脚
                writer.PageEvent = new HeaderFooterEvent(fontFooter, 2, signMode);               //添加绘制页眉页脚事件

                //创建分隔符
                Bitmap bitmap = new Bitmap(515, 1);
                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.FillRectangle(Brushes.Black, 0, 0, 515, 1);
                graphics.Dispose();
                separator = iTextSharp.text.Image.GetInstance(bitmap, System.Drawing.Imaging.ImageFormat.Bmp);

                //打开文档
                document.Open();

                #endregion

                #region 标题页

                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(CreateParagraph(companyName, fontTitle, Element.ALIGN_CENTER));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(CreateParagraph(reportNameCH, fontTitle, Element.ALIGN_CENTER));
                document.Add(CreateParagraph(reportNameEN, fontItem, Element.ALIGN_CENTER));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(CreateParagraph(reportTypeCH, fontTitle, Element.ALIGN_CENTER));
                document.Add(CreateParagraph(reportTypeEN, fontItem, Element.ALIGN_CENTER));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));

                PdfPTable table = new PdfPTable(2);
                table.TotalWidth = PageSize.A4.Width - 80;                //设置表格总宽
                table.SetWidths(new int[] { 8, 20 });                      //设置列宽比例
                table.LockedWidth = true;                                 //锁定表格宽度
                table.DefaultCell.FixedHeight = -10;                      //设置单元格高度
                table.DefaultCell.BorderWidth = 0;                        //设置单元格线宽

                table.AddCell(new Paragraph("", fontTitle));
                table.AddCell(new Paragraph("验证人员：" + MyDefine.myXET.calibMem, fontTitle));
                table.AddCell(new Paragraph("", fontTitle));
                table.AddCell(new Paragraph("验证日期：" + MyDefine.myXET.calibDate, fontTitle));
                table.AddCell(new Paragraph("", fontTitle));
                table.AddCell(new Paragraph("复核人员：" + MyDefine.myXET.reviewMem, fontTitle));
                table.AddCell(new Paragraph("", fontTitle));
                table.AddCell(new Paragraph("复核日期：" + MyDefine.myXET.reviewDate, fontTitle));

                document.Add(table);

                //document.Add(CreateParagraph(addSpace(20) + "验证人员：ADMIN", fontTitle, Element.ALIGN_LEFT));
                //document.Add(CreateParagraph(addSpace(30) + "Validation Personnel：ADMIN", fontItem, Element.ALIGN_LEFT));
                //document.Add(CreateParagraph(addSpace(20) + "报告日期：2021-10-12", fontTitle, Element.ALIGN_LEFT));
                //document.Add(CreateParagraph(addSpace(30) + "Report date：2021-10-12", fontItem, Element.ALIGN_LEFT));



                #endregion

                #region 一、校准对象

                //创建新页
                document.NewPage();
                //document.Add(CreateParagraph("Page " + (++page), fontMessage, Element.ALIGN_RIGHT));
                //document.Add(new Paragraph(blankLine, fontItem));
                ////////////////////////////////////////////////////////////////////
                //document.Add(CreateParagraph("晶度技术（北京）有限公司", fontTitle, Element.ALIGN_CENTER));
                document.Add(CreateParagraph(reportTypeCH + " 报告", fontTitle, Element.ALIGN_CENTER));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));

                #region 使用List输出信息
                /*
                myLS.Clear();
                myLS.Add("一、校准对象");
                myLS.Add("校准类型：");
                myLS.Add("通道类型：");
                doWithExactLen(ref myLS, LEN1);    //将字符串调整为等长

                myLS[1] += reportTypeCH;
                myLS[2] += dataType;
                doWithExactLen(ref myLS, LEN2);    //将字符串调整为等长

                myLS[1] += "设备型号：";
                myLS[2] += "系统版本：";
                doWithExactLen(ref myLS, LEN3);    //将字符串调整为等长

                myLS[1] += mDeviceID;
                myLS[2] += Constants.SW_Version;

                document.Add(new Paragraph(myLS[0], fontItem));
                document.Add(new Paragraph(blankLine, fontTiny));
                document.Add(new Paragraph(myLS[1], fontContent));
                document.Add(new Paragraph(myLS[2], fontContent));
                document.Add(new Paragraph(blankLine, fontItem));
                */
                #endregion

                #region 使用PdfPTable输出信息

                document.Add(new Paragraph("一、校准对象", fontItem));
                document.Add(new Paragraph(blankLine, fontTiny));

                colWidth = new float[] { 15, 40, 15, 40 };      //设置列宽比例，4列
                pdftable = new PdfPTable(colWidth);             //创建Table，并设置列数
                pdftable.HorizontalAlignment = Element.ALIGN_LEFT;
                pdftable.DefaultCell.BorderWidth = 0;           //无边框
                pdftable.WidthPercentage = 100;                 //占满整行

                pdftable.AddCell(new Paragraph("校准类型：", fontContent));
                pdftable.AddCell(new Paragraph(reportTypeCH, fontContent));
                pdftable.AddCell(new Paragraph("设备型号：", fontContent));
                pdftable.AddCell(new Paragraph(mDeviceID, fontContent));
                pdftable.AddCell(new Paragraph("通道类型：", fontContent));
                pdftable.AddCell(new Paragraph(dataType, fontContent));
                pdftable.AddCell(new Paragraph("系统版本：", fontContent));
                pdftable.AddCell(new Paragraph(Constants.SW_Version, fontContent));

                document.Add(pdftable);
                document.Add(new Paragraph(blankLine, fontItem));

                #endregion

                #endregion

                #region 二、报告信息

                #region 使用List输出信息
                /*
                myLS.Clear();
                myLS.Add("二、报告信息");
                myLS.Add("操作人员：");
                myLS.Add("开始时间：");
                myLS.Add("结束时间：");
                myLS.Add("记录间隔：");
                myLS.Add("运行时长：");
                myLS.Add("备注：");
                doWithExactLen(ref myLS, LEN1);    //将字符串调整为等长

                myLS[1] += MyDefine.myXET.operaMem;
                myLS[2] += rpttime;
                myLS[3] += rptstop;
                myLS[4] += rptspan;
                myLS[5] += rptrun;
                doWithExactLen(ref myLS, LEN2);    //将字符串调整为等长

                myLS[1] += "验证人员：";
                myLS[2] += "验证日期：";
                myLS[3] += "审核人员：";
                myLS[4] += "审核日期：";
                myLS[5] += "设备数量：";
                doWithExactLen(ref myLS, LEN3);    //将字符串调整为等长

                myLS[1] += MyDefine.myXET.calibMem;
                myLS[2] += MyDefine.myXET.calibDate;
                myLS[3] += MyDefine.myXET.reviewMem;
                myLS[4] += MyDefine.myXET.reviewDate;
                myLS[5] += MyDefine.myXET.meDUTNum;

                document.Add(new Paragraph(myLS[0], fontItem));
                document.Add(new Paragraph(blankLine, fontTiny));
                document.Add(new Paragraph(myLS[1], fontContent));
                document.Add(new Paragraph(myLS[2], fontContent));
                document.Add(new Paragraph(myLS[3], fontContent));
                document.Add(new Paragraph(myLS[4], fontContent));
                document.Add(new Paragraph(myLS[5], fontContent));
                document.Add(new Paragraph(blankLine, fontItem));
                */
                #endregion

                #region 使用PdfPTable输出信息

                document.Add(new Paragraph("二、报告信息", fontItem));
                document.Add(new Paragraph(blankLine, fontTiny));

                colWidth = new float[] { 15, 40 };      //设置列宽比例，2列
                pdftable = new PdfPTable(colWidth);             //创建Table，并设置列数
                pdftable.HorizontalAlignment = Element.ALIGN_LEFT;
                pdftable.DefaultCell.BorderWidth = 0;           //无边框
                pdftable.WidthPercentage = 100;                 //占满整行

                //第一行
                pdftable.AddCell(new Paragraph("操作人员：", fontContent));
                pdftable.AddCell(new Paragraph(MyDefine.myXET.operaMem, fontContent));

                //第二行
                pdftable.AddCell(new Paragraph("开始时间：", fontContent));
                pdftable.AddCell(new Paragraph(rpttime, fontContent));

                //第三行
                pdftable.AddCell(new Paragraph("结束时间：", fontContent));
                pdftable.AddCell(new Paragraph(rptstop, fontContent));

                //第四行
                pdftable.AddCell(new Paragraph("记录间隔：", fontContent));
                pdftable.AddCell(new Paragraph(rptspan, fontContent));

                //第五行
                pdftable.AddCell(new Paragraph("运行时长：", fontContent));
                pdftable.AddCell(new Paragraph(rptrun, fontContent));

                //第六行
                pdftable.AddCell(new Paragraph("设备数量：", fontContent));
                pdftable.AddCell(new Paragraph(MyDefine.myXET.meDUTNum.ToString(), fontContent));

                document.Add(pdftable);
                document.Add(new Paragraph(blankLine, fontItem));

                #endregion

                #endregion

                #region 三、标准器信息

                document.Add(new Paragraph("三、标准器信息", fontItem));
                document.Add(new Paragraph(blankLine, fontItem));

                colWidth = new float[] { 1, 0.8F, 1.2F, 1, 1 }; //设置列数，5列，等宽

                pdftable = getPDFPTableType2(MyDefine.myXET.meTblSTD.dataTable, colWidth, fontContent);    //生成PDF表格

                document.Add(pdftable);
                document.Add(new Paragraph(blankLine, fontItem));

                #endregion

                #region 四、校准信息
                //创建新页
                document.NewPage();

                document.Add(new Paragraph("四、校准信息", fontItem));
                document.Add(new Paragraph(blankLine, fontItem));

                colWidth = new float[] { 15, 10, 10, 10, 10, 10, 10, 10, 10 }; //设置列数，7列，等宽
                pdftable = new PdfPTable(colWidth);    //创建Table，并设置列数
                pdftable.HorizontalAlignment = Element.ALIGN_LEFT;
                pdftable.WidthPercentage = 100;

                //添加列
                for (int i = 0; i < mCaliTbl.dataTable.Columns.Count; i++)
                {
                    //if (i == 5) continue;       //第0列为序号，第5列为标定值，不显示
                    string colName = mCaliTbl.dataTable.Columns[i].ColumnName;
                    PdfPCell cell = new PdfPCell(new Phrase(colName, fontContent));
                    cell.HorizontalAlignment = i == 0 ? Element.ALIGN_LEFT : Element.ALIGN_CENTER;
                    cell.BorderWidth = 0;      //无边框
                    pdftable.AddCell(cell);
                }

                //添加行
                for (int i = 0; i < mCaliTbl.dataTable.Rows.Count; i++)
                {
                    if (mCaliTbl.IsRowEmpty(i, 1)) continue;     //判断本行的第一列(序号)之后有无内容，如果为空，则不输出此行
                    for (int j = 0; j < mCaliTbl.dataTable.Columns.Count; j++)
                    {
                        // if (j == 5) continue;        //第0列为序号，第5列为标定值，不显示
                        string myStr = mCaliTbl.dataTable.Rows[i][j].ToString();
                        PdfPCell cell = new PdfPCell(new Phrase(myStr, fontContent));
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.BorderWidth = 0;      //无边框
                        pdftable.AddCell(cell);
                    }
                }

                document.Add(pdftable);
                document.Add(new Paragraph(blankLine, fontItem));

                #endregion

                #region 五、曲线图
                //创建新页
                document.NewPage();

                document.Add(new Paragraph("五、曲线图", fontItem));
                document.Add(new Paragraph(blankLine, fontTiny));

                if (picPath != "") document.Add(CreateImage(picPath, 500, 250));
                if (picPath == "") document.Add(new Paragraph("  无", fontContent));
                document.Add(new Paragraph(blankLine, fontTiny));

                #endregion

                #region 六、原始数据

                //创建新页
                document.NewPage();
                ////////////////////////////////////////////////////////////////////
                document.Add(new Paragraph("六、原始数据", fontItem));
                document.Add(new Paragraph(blankLine, fontTiny));

                int colnum = MyDefine.myXET.meDataTbl.dataTable.Columns.Count + 1;  //后面要加采样次数列，所以+1
                int columnsCount = 0;
                do
                {
                    document.Add(new Paragraph(blankLine, fontTiny));
                    document.Add(separator);
                    document.Add(new Paragraph(blankLine, fontTiny));

                    int ix = 0;
                    if (colnum > 10)
                    {
                        if (columnsCount + 10 > colnum)
                        {
                            ix = colnum - columnsCount;
                        }
                        else
                        {
                            ix = 10;
                        }
                    }
                    else
                    {
                        ix = colnum;
                    }

                    float[] colWidthScale = new float[ix];          //设置列数
                    for (int i = 0; i < colWidthScale.Length; i++)
                    {
                        colWidthScale[i] = (i == 0) ? 70 : 52;          //设置列宽
                    }
                    colWidthScale[colWidthScale.Length - 1] = 40;       //采样次数列

                    pdftable = new PdfPTable(colWidthScale);            //创建Table，并设置列数
                    pdftable.HorizontalAlignment = Element.ALIGN_LEFT;
                    pdftable.WidthPercentage = 100;

                    pdftable.SetTotalWidth(colWidthScale);                //必须先设置TotalWidth再设置LockedWidth=true，否则表格宽度将为0，表格创建失败
                    pdftable.LockedWidth = true;                          //设置LockedWidth=true之前必须先设置表格总宽
                    pdftable.HeaderRows = 1;                              //标题栏跨页重复出现

                    //添加列
                    for (int i = 0; i < columnsCount + ix - 1; i++)
                    {
                        //string colName = MyDefine.myXET.meDataTbl.dataTable.Columns[i].ToString();
                        string colName = MyDefine.myXET.meDataTbl.dataTable.Columns[i].ColumnName;

                        PdfPCell cell = new PdfPCell(new Phrase(colName, fontTable));
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.BorderWidth = 0;      //无边框
                        cell.NoWrap = true;        //不换行
                        pdftable.AddCell(cell);
                        if (i == 0)
                        {
                            i += columnsCount;
                        }
                    }

                    //添加采样次数列
                    PdfPCell cell1 = new PdfPCell(new Phrase("采样次数", fontTable));
                    cell1.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell1.BorderWidth = 0;      //无边框
                    pdftable.AddCell(cell1);

                    //添加行
                    for (int i = 0; i < MyDefine.myXET.meDataTbl.dataTable.Rows.Count; i++)
                    {
                        /*
                        //标记有效数据开始
                        if (i == MyDefine.myXET.meValidStartIdx)
                        {
                            PdfPCell cell = new PdfPCell(new Phrase("有效数据开始:", fontTable));
                            cell.Colspan = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;       // 单元格占的列数(占满一行)
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            cell.BorderWidth = 0;      //无边框
                            pdftable.AddCell(cell);
                        }
                        */

                        //标记有效数据开始
                        for (int ilist = 0; ilist < MyDefine.myXET.meValidStageNum * 2; ilist += 2)
                        {
                            if (i == MyDefine.myXET.meValidIdxList[ilist])
                            {
                                string stage = "P" + (ilist / 2 + 1).ToString();
                                PdfPCell cell = new PdfPCell(new Phrase(stage + "有效数据开始:", fontTable));
                                cell.Colspan = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;       // 单元格占的列数(占满一行)
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                cell.BorderWidth = 0;      //无边框
                                pdftable.AddCell(cell);
                            }
                        }

                        //按行输出测试数据
                        for (int j = 0; j < columnsCount + ix - 1; j++)
                        {
                            string myStr = (MyDefine.myXET.meDataTbl.dataTable.Rows[i][j]).ToString();

                            PdfPCell cell = new PdfPCell(new Phrase(myStr, fontTable));
                            cell.HorizontalAlignment = (j == 0) ? Element.ALIGN_LEFT : Element.ALIGN_CENTER;
                            cell.BorderWidth = 0;      //无边框
                                                       //cell.FixedHeight = Convert.ToInt32( textBox11.Text);
                            pdftable.AddCell(cell);
                            if (j == 0)
                            {
                                j += columnsCount;
                            }
                        }

                        //添加采样次数
                        PdfPCell cell2 = new PdfPCell(new Phrase((i + 1).ToString(), fontTable));
                        cell2.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell2.BorderWidth = 0;      //无边框
                        pdftable.AddCell(cell2);

                        /*
                        //标记有效数据结束
                        if (i == MyDefine.myXET.meValidStopIdx)
                        {
                            PdfPCell cell = new PdfPCell(new Phrase("有效数据结束", fontTable));
                            cell.Colspan = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;       // 单元格占的列数(占满一行)
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            cell.BorderWidth = 0;      //无边框
                            pdftable.AddCell(cell);
                        }
                        */

                        //标记有效数据结束
                        for (int ilist = 1; ilist < MyDefine.myXET.meValidStageNum * 2 + 1; ilist += 2)
                        {
                            if (i == MyDefine.myXET.meValidIdxList[ilist])
                            {
                                string stage = "P" + (ilist / 2 + 1).ToString();
                                PdfPCell cell = new PdfPCell(new Phrase(stage + "有效数据结束:", fontTable));
                                cell.Colspan = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;       // 单元格占的列数(占满一行)
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                cell.BorderWidth = 0;      //无边框
                                pdftable.AddCell(cell);
                            }
                        }
                    }

                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontTiny));
                    document.Add(separator);
                    document.Add(new Paragraph(blankLine, fontTiny));

                    columnsCount += 8;
                } while (columnsCount + 2 < colnum);

                ////////////////////////////////////////////////////////////////////
                #endregion

                #region 末页签字(仅首末页签字模式)

                //首末页签字模式：在最后一页的空白处加上签字和日期
                if (signMode == 2)
                {
                    //页脚设置
                    PdfPTable footerTable = new PdfPTable(3);
                    footerTable.TotalWidth = PageSize.A4.Width - 80;            //设置表格总宽
                    footerTable.SetWidths(new int[] { 12, 10, 10 });            //设置列宽比例
                    footerTable.LockedWidth = true;                             //锁定表格宽度
                    footerTable.DefaultCell.FixedHeight = -10;                  //设置单元格高度
                    footerTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;       //设置单元格边框
                    footerTable.DefaultCell.BorderWidth = 0.5f;                 //设置单元格线宽
                    footerTable.DefaultCell.BorderColor = BaseColor.LIGHT_GRAY; //设置边框颜色

                    footerTable.AddCell(new Paragraph("  ", fontFooter));
                    footerTable.AddCell(new Paragraph("验证人：" + MyDefine.myXET.calibMem, fontFooter));
                    footerTable.AddCell(new Paragraph("复核人：" + MyDefine.myXET.reviewMem, fontFooter));

                    footerTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;      //设置单元格边框
                    footerTable.AddCell(new Paragraph("  ", fontFooter));
                    footerTable.AddCell(new Paragraph("验证日期：" + MyDefine.myXET.calibDate, fontFooter));
                    footerTable.AddCell(new Paragraph("复核日期：" + MyDefine.myXET.reviewDate, fontFooter));
                    footerTable.AddCell(new Paragraph("  ", fontFooter));
                    footerTable.AddCell(new Paragraph("  ", fontFooter));
                    footerTable.AddCell(new Paragraph("  ", fontFooter));

                    //写入页脚 -- 写到指定位置
                    footerTable.WriteSelectedRows(0, -1, 40, 60, writer.DirectContent);                           //写入页脚(位置在下面)
                }

                #endregion

                #region 关闭文件

                //关闭
                document.Close();

                //调出pdf
                Process.Start(filepath);

                #endregion

                MyDefine.myXET.AddTraceInfo("生成报告成功");
                return true;
            }
            catch (Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("生成报告失败：" + ex.ToString());
                return false;
            }
        }

        #endregion

        #region 验证报告

        public Boolean createPDFReportVerifyPart1(string filepath)
        {
            try
            {

                //使用
                String blankLine = " ";
                String mDeviceType = "";
                String mDeviceID = "";
                int index = -1;
                PdfPTable pdftable;
                float[] colWidth;       //列宽比例数组
                List<String> myLS = new List<String>();
                iTextSharp.text.Image separator;

                #region 数据来源

                rptdate = MyDefine.myXET.homdate;
                rpttime = MyDefine.myXET.homstart;
                rptstop = MyDefine.myXET.homstop;
                rptspan = MyDefine.myXET.homspan;
                rptrun = MyDefine.myXET.homrun;
                mDeviceID = MyDefine.myXET.hom_Model;
                mDeviceType = MyDefine.myXET.hom_Type;

                #endregion

                #region 参数分配

                //基本信息

                String companyName = textBox1.Text;         //公司名称
                String reportNameCH = textBox2.Text;        //报告名称-中文
                String reportNameEN = textBox3.Text;        //报告名称-英文
                //String reportCode = textBox4.Text;        //报告编号
                String reportTypeCH = comboBox2.SelectedItem.ToString().Split('|')[0];//报告类型
                String reportTypeEN = comboBox2.SelectedItem.ToString().Split('|')[1];//报告类型
                String picPath = textBox9.Text;             //图片路径
                String dataType = MyDefine.myXET.homunit;          //℃/%RH/kPa
                int signMode = radioButton1.Checked ? 1 : 2;       //签字模式：首末页签字；全部签字

                //记录间隔单位转换
                int intspan = Convert.ToInt32(rptspan);
                string rptspan2 = rptspan + " 秒";           //此处如果直接对rptspan + "秒"，下一次执行到1025行时会报错
                if (intspan % 60 == 0) rptspan2 = (intspan / 60).ToString() + " 分";
                if (intspan % 3600 == 0) rptspan2 = (intspan / 3600).ToString() + " 时";

                //运行时长单位转换
                int intrun = Convert.ToInt32(rptrun);
                string rptrun2 = rptrun + " 秒";
                if (intrun >= 60) rptrun2 = (intrun / 60).ToString("F2") + " 分";
                if (intrun >= 3600) rptrun2 = (intrun / 3600).ToString("F2") + " 时";

                //页眉页脚信息
                MyDefine.myXET.logopath = textBox10.Text;           //Logo图片
                MyDefine.myXET.operaMem = MyDefine.myXET.userName;  //操作人员
                MyDefine.myXET.calibMem = textBox5.Text;            //校准人员
                MyDefine.myXET.calibDate = textBox6.Text;           //校准日期
                MyDefine.myXET.reviewMem = textBox7.Text;           //审核人员
                MyDefine.myXET.reviewDate = textBox8.Text;          //审核日期

                #endregion

                #region 参数完整性检查

                if (picPath != "" && !File.Exists(picPath))
                {
                    MessageBox.Show("图片路径错误，请重新选择图片！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.logopath != "" && !File.Exists(MyDefine.myXET.logopath))
                {
                    MessageBox.Show("Logo路径错误，请重新选择图片！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meDataTbl == null)
                {
                    MessageBox.Show("尚未加载测试数据，请选择数据文件！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meTblVer == null)
                {
                    MessageBox.Show("尚未生成设备信息表，请在验证界面生成！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meTblVer1 == null)
                {
                    MessageBox.Show("尚未生成报告有效数据汇总表，请在验证界面生成！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meTblVer2 == null)
                {
                    MessageBox.Show("尚未生成详细数据横向汇总表，请在验证界面生成！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meTblVer3 == null)
                {
                    MessageBox.Show("尚未生成详细数据横向汇总表，请在验证界面生成！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meTblVer4 == null)
                {
                    MessageBox.Show("尚未生成详细数据横向汇总表，请在验证界面生成！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meTblVer5 == null)
                {
                    MessageBox.Show("尚未生成详细数据纵向汇总表，请在验证界面生成！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meTblVer6 == null)
                {
                    MessageBox.Show("尚未生成关键参数汇总表，请在验证界面生成！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                #endregion

                #region 更新报告编号(需放在页眉页脚前)

                //更新报告编号
                MyDefine.myXET.repcode = updateReportCode();                //更新报告编号（报告编号+1）
                MyDefine.myXET.saveReportInfo();                            //保存报告编号及标准器信息表
                showReportCode();                                           //显示更新后的报告编号

                MyDefine.myXET.repcode = "JDYZ" + (DateTime.Now.Ticks / 100000000).ToString();       //根据系统时间实时生成报告编号
                #endregion

                #region 字体及页眉页脚定义

                //创建新文档对象,页边距(X,X,Y,Y)
                Document document = new Document(PageSize.A4, 40, 40, 65, 80);

                //路径设置; FileMode.Create文档不在会创建，存在会覆盖
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filepath, FileMode.Create));

                //当前登录账号的类别名称
                string name = MyDefine.myXET.meLoginUser.Split(';')[(int)ACCOUNT.USER];
                //是否有编辑PDF的权限
                if (MyDefine.myXET.CheckPermission(STEP.数据载入, false))
                {
                    writer.SetEncryption(null, Encoding.Default.GetBytes(name + "JD"), PdfWriter.ALLOW_PRINTING, PdfWriter.ENCRYPTION_AES_128 | PdfWriter.DO_NOT_ENCRYPT_METADATA);

                    //账户为汉字则启用超级密码
                    foreach (char item in name)
                    {
                        if ((int)item > 127)
                        {
                            writer.SetEncryption(null, Encoding.Default.GetBytes("JD20191206"), PdfWriter.ALLOW_PRINTING, PdfWriter.ENCRYPTION_AES_128 | PdfWriter.DO_NOT_ENCRYPT_METADATA);
                            break;
                        }
                    }
                }
                else
                {
                    writer.SetEncryption(null, null, PdfWriter.ALLOW_PRINTING, PdfWriter.ENCRYPTION_AES_128 | PdfWriter.DO_NOT_ENCRYPT_METADATA);
                }
                //writer.SetEncryption(Encoding.Default.GetBytes("Hello"), Encoding.Default.GetBytes("123456"), PdfWriter.ALLOW_SCREENREADERS, PdfWriter.STANDARD_ENCRYPTION_128);

                //添加信息
                document.AddTitle("HTR校准报告");
                document.AddAuthor(companyName);
                document.AddSubject(mDeviceType + " 温度测量报告");
                document.AddKeywords("HTR");
                document.AddCreator(MyDefine.myXET.operaMem);

                //创建字体(SIMYOU.TTF中的~符号不能上下居中)
                //iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 14.0f, iTextSharp.text.Font.BOLD);
                //iTextSharp.text.Font fontItem = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 12.0f, iTextSharp.text.Font.BOLD);
                //iTextSharp.text.Font fontContent = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 10.0f, iTextSharp.text.Font.NORMAL);
                //iTextSharp.text.Font fontMessage = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL);
                //iTextSharp.text.Font fontTable = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 8.0f, iTextSharp.text.Font.NORMAL);
                //iTextSharp.text.Font fontFooter = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL, BaseColor.GRAY);
                //iTextSharp.text.Font fontTiny = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 6.0f, iTextSharp.text.Font.NORMAL);

                //创建字体
                iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 14.0f, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font fontItem = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 12.0f, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font fontContent = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 10.0f, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font fontContent2 = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font fontMessage = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font fontTable = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 8.0f, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font fontFooter = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL, BaseColor.GRAY);
                iTextSharp.text.Font fontTiny = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 6.0f, iTextSharp.text.Font.NORMAL);

                //iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(BaseFont.CreateFont(@"c:\windows\fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 14.0f, iTextSharp.text.Font.BOLD);
                //iTextSharp.text.Font fontItem = new iTextSharp.text.Font(BaseFont.CreateFont(@"c:\windows\fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 12.0f, iTextSharp.text.Font.BOLD);
                //iTextSharp.text.Font fontContent = new iTextSharp.text.Font(BaseFont.CreateFont(@"c:\windows\fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 10.0f, iTextSharp.text.Font.NORMAL);
                //iTextSharp.text.Font fontMessage = new iTextSharp.text.Font(BaseFont.CreateFont(@"c:\windows\fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL);
                //iTextSharp.text.Font fontTable = new iTextSharp.text.Font(BaseFont.CreateFont(@"C:\Windows\Fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 8.0f, iTextSharp.text.Font.NORMAL);
                //iTextSharp.text.Font fontFooter = new iTextSharp.text.Font(BaseFont.CreateFont(@"C:\Windows\Fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL, BaseColor.GRAY);
                //iTextSharp.text.Font fontTiny = new iTextSharp.text.Font(BaseFont.CreateFont(@"c:\windows\fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 6.0f, iTextSharp.text.Font.NORMAL);

                //添加页眉页脚
                writer.PageEvent = new HeaderFooterEvent(fontFooter, 2, signMode);               //添加绘制页眉页脚事件

                //创建分隔符
                Bitmap bitmap = new Bitmap(515, 1);
                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.FillRectangle(Brushes.Black, 0, 0, 515, 1);
                graphics.Dispose();
                separator = iTextSharp.text.Image.GetInstance(bitmap, System.Drawing.Imaging.ImageFormat.Bmp);

                //打开文档
                document.Open();

                #endregion

                #region 标题页

                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(CreateParagraph(companyName, fontTitle, Element.ALIGN_CENTER));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(CreateParagraph(reportNameCH, fontTitle, Element.ALIGN_CENTER));
                document.Add(CreateParagraph(reportNameEN, fontItem, Element.ALIGN_CENTER));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(CreateParagraph(reportTypeCH, fontTitle, Element.ALIGN_CENTER));
                document.Add(CreateParagraph(reportTypeEN, fontItem, Element.ALIGN_CENTER));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));

                PdfPTable table = new PdfPTable(2);
                table.TotalWidth = PageSize.A4.Width - 80;                //设置表格总宽
                table.SetWidths(new int[] { 8, 20 });                      //设置列宽比例
                table.LockedWidth = true;                                 //锁定表格宽度
                table.DefaultCell.FixedHeight = -10;                      //设置单元格高度
                table.DefaultCell.BorderWidth = 0;                        //设置单元格线宽

                table.AddCell(new Paragraph("", fontTitle));
                table.AddCell(new Paragraph("验证人员：" + MyDefine.myXET.calibMem, fontTitle));
                table.AddCell(new Paragraph("", fontTitle));
                table.AddCell(new Paragraph("验证日期：" + MyDefine.myXET.calibDate, fontTitle));
                table.AddCell(new Paragraph("", fontTitle));
                table.AddCell(new Paragraph("复核人员：" + MyDefine.myXET.reviewMem, fontTitle));
                table.AddCell(new Paragraph("", fontTitle));
                table.AddCell(new Paragraph("复核日期：" + MyDefine.myXET.reviewDate, fontTitle));

                document.Add(table);

                //document.Add(CreateParagraph(addSpace(20) + "验证人员：ADMIN", fontTitle, Element.ALIGN_LEFT));
                //document.Add(CreateParagraph(addSpace(30) + "Validation Personnel：ADMIN", fontItem, Element.ALIGN_LEFT));
                //document.Add(CreateParagraph(addSpace(20) + "报告日期：2021-10-12", fontTitle, Element.ALIGN_LEFT));
                //document.Add(CreateParagraph(addSpace(30) + "Report date：2021-10-12", fontItem, Element.ALIGN_LEFT));



                #endregion

                #region 一、验证对象

                //创建新页
                document.NewPage();
                ////////////////////////////////////////////////////////////////////
                //document.Add(CreateParagraph("晶度技术（北京）有限公司", fontTitle, Element.ALIGN_CENTER));
                document.Add(CreateParagraph(reportTypeCH + " 报告", fontTitle, Element.ALIGN_CENTER));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));

                #region 使用list输出信息
                /*
                myLS.Clear();
                myLS.Add("一、验证对象");
                myLS.Add("验证类型：");
                myLS.Add("通道类型：");
                doWithExactLen(ref myLS, LEN1);    //将字符串调整为等长

                myLS[1] += reportTypeCH;
                myLS[2] += dataType;
                doWithExactLen(ref myLS, LEN2);    //将字符串调整为等长

                myLS[1] += "设备型号：";
                myLS[2] += "系统版本：";
                doWithExactLen(ref myLS, LEN3);    //将字符串调整为等长

                myLS[1] += mDeviceID;
                myLS[2] += Constants.SW_Version;

                document.Add(new Paragraph(myLS[0], fontItem));
                document.Add(new Paragraph(blankLine, fontTiny));
                document.Add(new Paragraph(myLS[1], fontContent));
                document.Add(new Paragraph(myLS[2], fontContent));
                document.Add(new Paragraph(blankLine, fontItem));
                myLS.Clear();
                */
                #endregion

                #region 使用PdfPTable输出信息

                ////////////////////////////////////////////////////////////////////
                document.Add(new Paragraph("一、验证对象", fontItem));
                document.Add(new Paragraph(blankLine, fontTiny));

                colWidth = new float[] { 15, 40, 15, 40 };      //设置列宽比例，4列
                pdftable = new PdfPTable(colWidth);             //创建Table，并设置列数
                pdftable.HorizontalAlignment = Element.ALIGN_LEFT;
                pdftable.DefaultCell.BorderWidth = 0;           //无边框
                pdftable.WidthPercentage = 100;                 //占满整行

                pdftable.AddCell(new Paragraph("验证类型：", fontContent));
                pdftable.AddCell(new Paragraph(reportTypeCH, fontContent));
                pdftable.AddCell(new Paragraph("设备型号：", fontContent));
                pdftable.AddCell(new Paragraph(mDeviceID, fontContent));
                pdftable.AddCell(new Paragraph("通道类型：", fontContent));
                pdftable.AddCell(new Paragraph(dataType, fontContent));
                pdftable.AddCell(new Paragraph("系统版本：", fontContent));
                pdftable.AddCell(new Paragraph(Constants.SW_Version, fontContent));

                document.Add(pdftable);
                document.Add(new Paragraph(blankLine, fontItem));

                #endregion

                #endregion

                #region 二、报告信息

                #region 使用list输出信息
                /*
                myLS.Clear();
                myLS.Add("二、报告信息");
                myLS.Add("操作人员：");
                myLS.Add("开始时间：");
                myLS.Add("结束时间：");
                myLS.Add("记录间隔：");
                myLS.Add("运行时长：");
                myLS.Add("备注：");
                doWithExactLen(ref myLS, LEN1);    //将字符串调整为等长

                myLS[1] += MyDefine.myXET.operaMem;
                myLS[2] += rpttime;
                myLS[3] += rptstop;
                myLS[4] += rptspan;
                myLS[5] += rptrun;
                doWithExactLen(ref myLS, LEN2);    //将字符串调整为等长

                myLS[1] += "验证人员：";
                myLS[2] += "验证日期：";
                myLS[3] += "审核人员：";
                myLS[4] += "审核日期：";
                myLS[5] += "设备数量：";
                doWithExactLen(ref myLS, LEN3);    //将字符串调整为等长

                myLS[1] += MyDefine.myXET.calibMem;
                myLS[2] += MyDefine.myXET.calibDate;
                myLS[3] += MyDefine.myXET.reviewMem;
                myLS[4] += MyDefine.myXET.reviewDate;
                myLS[5] += MyDefine.myXET.meDUTNum;

                document.Add(new Paragraph(myLS[0], fontItem));
                document.Add(new Paragraph(blankLine, fontTiny));
                document.Add(new Paragraph(myLS[1], fontContent));
                document.Add(new Paragraph(myLS[2], fontContent));
                document.Add(new Paragraph(myLS[3], fontContent));
                document.Add(new Paragraph(myLS[4], fontContent));
                document.Add(new Paragraph(myLS[5], fontContent));
                document.Add(new Paragraph(blankLine, fontItem));
                */
                #endregion

                #region 使用PdfPTable输出信息

                document.Add(new Paragraph("二、报告信息", fontItem));
                document.Add(new Paragraph(blankLine, fontTiny));

                colWidth = new float[] { 15, 40 };      //设置列宽比例，2列
                pdftable = new PdfPTable(colWidth);             //创建Table，并设置列数
                pdftable.HorizontalAlignment = Element.ALIGN_LEFT;
                pdftable.DefaultCell.BorderWidth = 0;           //无边框
                pdftable.WidthPercentage = 100;                 //占满整行

                //第一行
                pdftable.AddCell(new Paragraph("操作人员：", fontContent));
                pdftable.AddCell(new Paragraph(MyDefine.myXET.operaMem, fontContent));

                //第二行
                pdftable.AddCell(new Paragraph("开始时间：", fontContent));
                pdftable.AddCell(new Paragraph(rpttime, fontContent));

                //第三行
                pdftable.AddCell(new Paragraph("结束时间：", fontContent));
                pdftable.AddCell(new Paragraph(rptstop, fontContent));

                //第四行
                pdftable.AddCell(new Paragraph("记录间隔：", fontContent));
                pdftable.AddCell(new Paragraph(rptspan2, fontContent));

                //第五行
                pdftable.AddCell(new Paragraph("运行时长：", fontContent));
                pdftable.AddCell(new Paragraph(rptrun2, fontContent));

                //第六行
                pdftable.AddCell(new Paragraph("设备数量：", fontContent));
                pdftable.AddCell(new Paragraph(MyDefine.myXET.meDUTNum.ToString(), fontContent));

                document.Add(pdftable);
                document.Add(new Paragraph(blankLine, fontItem));

                #endregion

                #endregion

                #region 三、设备信息(打印验证界面显示的表格)

                document.Add(new Paragraph("三、设备信息", fontItem));
                document.Add(new Paragraph(blankLine, fontItem));

                index = -1;
                colWidth = new float[] { 5, 10, 10, 16, 10, 10 }; //设置列宽比例，6列
                if (TemPdf)
                {
                    pdftable = getPDFPTable(MyDefine.myXET.meTemVer.dataTable, colWidth, fontContent, ++index);    //生成PDF表格
                    document.Add(pdftable);
                    pdftable.HeaderRows = 1;                                                                       //标题栏跨页重复出现
                    document.Add(new Paragraph(blankLine, fontItem));
                }
                if (HumPdf)
                {
                    pdftable = getPDFPTable(MyDefine.myXET.meHumVer.dataTable, colWidth, fontContent, ++index);    //生成PDF表格
                    document.Add(pdftable);
                    pdftable.HeaderRows = 1;                                                                       //标题栏跨页重复出现
                    document.Add(new Paragraph(blankLine, fontItem));
                }
                if (PrsPdf)
                {
                    pdftable = getPDFPTable(MyDefine.myXET.mePrsVer.dataTable, colWidth, fontContent, ++index);    //生成PDF表格
                    document.Add(pdftable);
                    pdftable.HeaderRows = 1;                                                                       //标题栏跨页重复出现
                    document.Add(new Paragraph(blankLine, fontItem));
                }
                ////////////////////////////////////////////////////////////////////

                #endregion

                #region 四、报告阶段数据汇总
                //创建新页
                document.NewPage();
                document.Add(new Paragraph("四、报告阶段数据汇总", fontItem));
                document.Add(new Paragraph(blankLine, fontTiny));
                index = -1;
                if (TemPdf)
                {
                    colWidth = new float[] { 14, 14, 14, 14, 10, 14, 14, 10, 14, 10, 14, 10, 10 };                          //设置列宽比例，10列
                    pdftable = getPDFPTable(MyDefine.myXET.meTemVer8.dataTable, colWidth, fontContent2, ++index);    //生成PDF表格
                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));
                }
                if (HumPdf)
                {
                    colWidth = new float[] { 14, 14, 14, 14, 10, 14, 14, 10, 14, 10, 14, 10, 10 };                          //设置列宽比例，10列
                    pdftable = getPDFPTable(MyDefine.myXET.meHumVer8.dataTable, colWidth, fontContent2, ++index);    //生成PDF表格
                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));
                }
                if (PrsPdf)
                {
                    colWidth = new float[] { 10, 10, 10, 10, 10, 15, 15 };                                          //设置列宽比例，7列
                    pdftable = getPDFPTable(MyDefine.myXET.mePrsVer8.dataTable, colWidth, fontContent2, ++index);    //生成PDF表格
                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));
                }

                #endregion

                #region 4.1 阶段曲线图
                int pictureNum = 0;
                //画图
                for (int i = 0; i < MyDefine.myXET.meValidStageNum; i++)
                {
                    if (pictureNum % 2 == 0)
                    {
                        //创建新页
                        document.NewPage();

                        if (pictureNum == 0)
                        {
                            document.Add(new Paragraph("4.1 阶段曲线图", fontItem));
                            document.Add(new Paragraph(blankLine, fontTiny));
                        }
                    }
                    if (together)
                    {
                        document.Add(CreateParagraph(MyDefine.myXET.meValidNameList[i], fontItem, Element.ALIGN_CENTER));
                        iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(GetStageBitmapPoint(false, i * 2), System.Drawing.Imaging.ImageFormat.Bmp);
                        document.Add(image);
                        pictureNum++;
                    }
                    else
                    {
                        if (TemPdf)
                        {
                            document.Add(CreateParagraph(MyDefine.myXET.meValidNameList[i] + "（温度）", fontItem, Element.ALIGN_CENTER));
                            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(GetStageBitmapPoint(true, i * 2), System.Drawing.Imaging.ImageFormat.Bmp);
                            document.Add(image);
                            pictureNum++;
                        }
                        if (HumPdf)
                        {
                            document.Add(CreateParagraph(MyDefine.myXET.meValidNameList[i] + "（湿度）", fontItem, Element.ALIGN_CENTER));
                            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(GetStageBitmapPoint(false, i * 2), System.Drawing.Imaging.ImageFormat.Bmp);
                            document.Add(image);
                            pictureNum++;
                        }
                        if (PrsPdf)
                        {
                            document.Add(CreateParagraph(MyDefine.myXET.meValidNameList[i] + "（压力）", fontItem, Element.ALIGN_CENTER));
                            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(GetStageBitmapPoint(false, i * 2), System.Drawing.Imaging.ImageFormat.Bmp);
                            document.Add(image);
                            pictureNum++;
                        }
                    }
                }

                #endregion

                #region 4.2 阶段关键参数汇总
                //创建新页
                document.NewPage();
                document.Add(new Paragraph("4.2 阶段关键参数汇总", fontItem));
                document.Add(new Paragraph(blankLine, fontTiny));
                index = -1;
                colWidth = new float[] { 10, 10, 10, 10, 10, 10 }; //设置列宽比例，6列
                if (TemPdf)
                {
                    pdftable = getPDFPTable(MyDefine.myXET.meTemVer6.dataTable, colWidth, fontContent, ++index);    //生成PDF表格
                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));
                }
                if (HumPdf)
                {
                    pdftable = getPDFPTable(MyDefine.myXET.meHumVer6.dataTable, colWidth, fontContent, ++index);    //生成PDF表格
                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));
                }
                if (PrsPdf)
                {
                    pdftable = getPDFPTable(MyDefine.myXET.mePrsVer6.dataTable, colWidth, fontContent, ++index);    //生成PDF表格
                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));
                }

                #endregion

                #region 4.3 阶段数据纵向汇总
                //创建新页
                document.NewPage();
                document.Add(new Paragraph("4.3 阶段数据纵向汇总", fontItem));
                document.Add(new Paragraph(blankLine, fontTiny));
                index = -1;
                colWidth = new float[] { 10, 14, 17, 17, 12, 10, 10, 8, 10 }; //设置列宽比例，9列
                if (TemPdf)
                {
                    pdftable = getPDFPTable(MyDefine.myXET.meTemVer5.dataTable, colWidth, fontContent, ++index);    //生成PDF表格
                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));
                }
                if (HumPdf)
                {
                    pdftable = getPDFPTable(MyDefine.myXET.meHumVer5.dataTable, colWidth, fontContent, ++index);    //生成PDF表格
                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));
                }
                if (PrsPdf)
                {
                    pdftable = getPDFPTable(MyDefine.myXET.mePrsVer5.dataTable, colWidth, fontContent, ++index);    //生成PDF表格
                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));
                }

                #endregion

                #region 4.4 阶段数据横向汇总
                //创建新页
                document.NewPage();
                document.Add(new Paragraph("4.4 阶段数据横向汇总", fontItem));
                document.Add(new Paragraph(blankLine, fontTiny));
                colWidth = new float[] { 18, 8, 13, 8, 13, 8, 8, 10, 10 }; //设置列宽比例，9列
                if (MyDefine.myXET.meTblVer2.dataTable.Rows.Count > 0 && TemPdf)
                {
                    //温度汇总表
                    document.Add(new Paragraph("温度：", fontContent));
                    document.Add(new Paragraph(blankLine, fontMessage));
                    pdftable = getPDFPTable(MyDefine.myXET.meTblVer2.dataTable, colWidth, fontMessage);    //生成PDF表格
                    pdftable.HeaderRows = 1;                                                               //标题栏跨页重复出现
                    document.Add(pdftable);
                }

                if (MyDefine.myXET.meTblVer3.dataTable.Rows.Count > 0 && HumPdf)
                {
                    //湿度汇总表
                    document.Add(new Paragraph("湿度：", fontContent));
                    document.Add(new Paragraph(blankLine, fontMessage));
                    pdftable = getPDFPTable(MyDefine.myXET.meTblVer3.dataTable, colWidth, fontMessage);    //生成PDF表格
                    pdftable.HeaderRows = 1;                                                               //标题栏跨页重复出现
                    document.Add(pdftable);
                }

                if (MyDefine.myXET.meTblVer4.dataTable.Rows.Count > 0 && PrsPdf)
                {
                    //压力汇总表
                    document.Add(new Paragraph("压力：", fontContent));
                    document.Add(new Paragraph(blankLine, fontMessage));
                    pdftable = getPDFPTable(MyDefine.myXET.meTblVer4.dataTable, colWidth, fontMessage);    //生成PDF表格
                    pdftable.HeaderRows = 1;                                                               //标题栏跨页重复出现
                    document.Add(pdftable);
                }

                #endregion

                #region 五、F0值数据分析
                if (MyDefine.myXET.meTblVer7 != null && MyDefine.myXET.isF0Checked)
                {
                    int colnum;

                    document.NewPage();

                    document.Add(new Paragraph("五、F0值数据分析", fontItem));
                    document.Add(new Paragraph(blankLine, fontTiny));

                    //添加有效数据汇总表(仅温度行)
                    colWidth = new float[] { 10, 10, 10, 10, 10, 15, 15 }; //设置列宽比例，7列
                    dataTableClass mytable = new dataTableClass();
                    mytable.dataTable = MyDefine.myXET.meTblVer1.CopyTable();
                    for (int i = mytable.dataTable.Rows.Count - 1; i > 0; i--)      //删除可能存在的湿度或压力行
                    {
                        string tpye = mytable.GetCellValue(i, 2);
                        if (tpye.Contains("温度") == false) mytable.DeleteTableRow(i);
                    }
                    pdftable = getPDFPTable(mytable.dataTable, colWidth, fontContent);    //生成PDF表格

                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));

                    //添加F0值数据表
                    colnum = MyDefine.myXET.meTblVer7.dataTable.Columns.Count;
                    colWidth = new float[colnum];      //设置列数
                    for (int i = 0; i < colWidth.Length; i++)
                    {
                        colWidth[i] = (i == 0) ? 15 : (100 - 15) / (colnum - 1);                         //设置列宽(让时间列固定占15/100列宽，防止时间显示不开)
                    }
                    pdftable = getPDFPTable(MyDefine.myXET.meTblVer7.dataTable, colWidth, fontTable);    //生成PDF表格
                    pdftable.HeaderRows = 1;                                                             //设置新页显示表头

                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));
                }

                #endregion

                #region 末页签字(仅首末页签字模式)

                if (signMode == 2)
                {

                    //首末页签字模式：在最后一页的空白处加上签字和日期
                    //页脚设置
                    PdfPTable footerTable = new PdfPTable(3);
                    footerTable.TotalWidth = PageSize.A4.Width - 80;            //设置表格总宽
                    footerTable.SetWidths(new int[] { 12, 10, 10 });            //设置列宽比例
                    footerTable.LockedWidth = true;                             //锁定表格宽度
                    footerTable.DefaultCell.FixedHeight = -10;                  //设置单元格高度
                    footerTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;       //设置单元格边框
                    footerTable.DefaultCell.BorderWidth = 0.5f;                 //设置单元格线宽
                    footerTable.DefaultCell.BorderColor = BaseColor.LIGHT_GRAY; //设置边框颜色

                    footerTable.AddCell(new Paragraph("  ", fontFooter));
                    footerTable.AddCell(new Paragraph("验证人：" + MyDefine.myXET.calibMem, fontFooter));
                    footerTable.AddCell(new Paragraph("复核人：" + MyDefine.myXET.reviewMem, fontFooter));

                    footerTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;      //设置单元格边框
                    footerTable.AddCell(new Paragraph("  ", fontFooter));
                    footerTable.AddCell(new Paragraph("验证日期：" + MyDefine.myXET.calibDate, fontFooter));
                    footerTable.AddCell(new Paragraph("复核日期：" + MyDefine.myXET.reviewDate, fontFooter));
                    footerTable.AddCell(new Paragraph("  ", fontFooter));
                    footerTable.AddCell(new Paragraph("  ", fontFooter));
                    footerTable.AddCell(new Paragraph("  ", fontFooter));

                    //写入页脚 -- 写到指定位置
                    footerTable.WriteSelectedRows(0, -1, 40, 60, writer.DirectContent);                           //写入页脚(位置在下面)
                }
                #endregion

                #region 关闭文件

                //关闭
                document.Close();

                //调出pdf
                Process.Start(filepath);

                #endregion

                MyDefine.myXET.AddTraceInfo("生成报告第一部分成功");
                return true;
            }
            catch (Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("生成报告第一部分失败：" + ex.ToString());
                return false;
            }
        }

        public Boolean createPDFReportVerifyPart2(string filepath)
        {
            try
            {

                //使用
                String blankLine = " ";
                String mDeviceType = "";
                String mDeviceID = "";
                PdfPTable pdftable;
                float[] colWidth;       //列宽比例数组
                List<String> myLS = new List<String>();
                iTextSharp.text.Image separator;
                int colnum;
                int columnsCount;

                #region 数据来源

                rptdate = MyDefine.myXET.homdate;
                rpttime = MyDefine.myXET.homstart;
                rptstop = MyDefine.myXET.homstop;
                rptspan = MyDefine.myXET.homspan;
                rptrun = MyDefine.myXET.homrun;
                mDeviceID = MyDefine.myXET.hom_Model;
                mDeviceType = MyDefine.myXET.hom_Type;

                #endregion

                #region 参数分配

                //基本信息

                String companyName = textBox1.Text;         //公司名称
                String reportNameCH = textBox2.Text;        //报告名称-中文
                String reportNameEN = textBox3.Text;        //报告名称-英文
                //String reportCode = textBox4.Text;        //报告编号
                String reportTypeCH = comboBox2.SelectedItem.ToString().Split('|')[0];//报告类型
                String reportTypeEN = comboBox2.SelectedItem.ToString().Split('|')[1];//报告类型
                String picPath = textBox9.Text;            //图片路径
                String dataType = MyDefine.myXET.homunit;          //℃/%RH/kPa
                int signMode = radioButton1.Checked ? 1 : 2;       //签字模式：首末页签字；全部签字

                //记录间隔单位转换
                int intspan = Convert.ToInt32(rptspan);
                string rptspan2 = rptspan + " 秒";
                if (intspan % 60 == 0) rptspan2 = (intspan / 60).ToString() + " 分";
                if (intspan % 3600 == 0) rptspan2 = (intspan / 3600).ToString() + " 时";

                //运行时长单位转换
                int intrun = Convert.ToInt32(rptrun);
                string rptrun2 = rptrun + " 秒";
                if (intrun >= 60) rptrun2 = (intrun / 60).ToString("F2") + " 分";
                if (intrun >= 3600) rptrun2 = (intrun / 3600).ToString("F2") + " 时";

                //页眉页脚信息
                MyDefine.myXET.logopath = textBox10.Text;           //Logo图片
                MyDefine.myXET.operaMem = MyDefine.myXET.userName;  //操作人员
                MyDefine.myXET.calibMem = textBox5.Text;            //校准人员
                MyDefine.myXET.calibDate = textBox6.Text;           //校准日期
                MyDefine.myXET.reviewMem = textBox7.Text;           //审核人员
                MyDefine.myXET.reviewDate = textBox8.Text;          //审核日期

                #endregion

                #region 参数完整性检查

                if (picPath != "" && !File.Exists(picPath))
                {
                    MessageBox.Show("图片路径错误，请重新选择图片！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.logopath != "" && !File.Exists(MyDefine.myXET.logopath))
                {
                    MessageBox.Show("Logo路径错误，请重新选择图片！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meDataTbl == null)
                {
                    MessageBox.Show("尚未加载测试数据，请选择数据文件！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meTblVer == null)
                {
                    MessageBox.Show("尚未生成设备信息表，请在验证界面生成！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meTblVer1 == null)
                {
                    MessageBox.Show("尚未生成报告有效数据汇总表，请在验证界面生成！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meTblVer2 == null)
                {
                    MessageBox.Show("尚未生成详细数据横向汇总表，请在验证界面生成！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meTblVer3 == null)
                {
                    MessageBox.Show("尚未生成详细数据横向汇总表，请在验证界面生成！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meTblVer4 == null)
                {
                    MessageBox.Show("尚未生成详细数据横向汇总表，请在验证界面生成！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meTblVer5 == null)
                {
                    MessageBox.Show("尚未生成详细数据纵向汇总表，请在验证界面生成！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meTblVer6 == null)
                {
                    MessageBox.Show("尚未生成关键参数汇总表，请在验证界面生成！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                #endregion

                #region 更新报告编号(需放在页眉页脚前)

                //第二部分的报告编号与第一部分相同
                //MyDefine.myXET.repcode = updateReportCode();                //更新报告编号（报告编号+1）
                //MyDefine.myXET.saveReportInfo();                            //保存报告编号及标准器信息表
                //showReportCode();                                           //显示更新后的报告编号

                //MyDefine.myXET.repcode = "JDYZ" + (DateTime.Now.Ticks / 100000000).ToString();       //根据系统时间实时生成报告编号
                #endregion

                #region 字体及页眉页脚定义

                //创建新文档对象,页边距(X,X,Y,Y)
                Document document = new Document(PageSize.A4, 40, 40, 65, 80);

                //路径设置; FileMode.Create文档不在会创建，存在会覆盖
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filepath, FileMode.Create));

                //当前登录账号的类别名称
                string name = MyDefine.myXET.meLoginUser.Split(';')[(int)ACCOUNT.USER];
                //是否有编辑PDF的权限
                if (MyDefine.myXET.CheckPermission(STEP.数据载入, false))
                {
                    writer.SetEncryption(null, Encoding.Default.GetBytes(name + "JD"), PdfWriter.ALLOW_PRINTING, PdfWriter.ENCRYPTION_AES_128 | PdfWriter.DO_NOT_ENCRYPT_METADATA);

                    //账户为汉字则启用超级密码
                    foreach (char item in name)
                    {
                        if ((int)item > 127)
                        {
                            writer.SetEncryption(null, Encoding.Default.GetBytes("JD20191206"), PdfWriter.ALLOW_PRINTING, PdfWriter.ENCRYPTION_AES_128 | PdfWriter.DO_NOT_ENCRYPT_METADATA);
                            break;
                        }
                    }
                }
                else
                {
                    writer.SetEncryption(null, null, PdfWriter.ALLOW_PRINTING, PdfWriter.ENCRYPTION_AES_128 | PdfWriter.DO_NOT_ENCRYPT_METADATA);
                }
                //writer.SetEncryption(Encoding.Default.GetBytes("Hello"), Encoding.Default.GetBytes("123456"), PdfWriter.ALLOW_SCREENREADERS, PdfWriter.STANDARD_ENCRYPTION_128);

                //添加信息
                document.AddTitle("HTR校准报告");
                document.AddAuthor(companyName);
                document.AddSubject(mDeviceType + " 温度测量报告");
                document.AddKeywords("HTR");
                document.AddCreator(MyDefine.myXET.operaMem);

                //创建字体(SIMYOU.TTF中的~符号不能上下居中)
                //iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 14.0f, iTextSharp.text.Font.BOLD);
                //iTextSharp.text.Font fontItem = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 12.0f, iTextSharp.text.Font.BOLD);
                //iTextSharp.text.Font fontContent = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 10.0f, iTextSharp.text.Font.NORMAL);
                //iTextSharp.text.Font fontMessage = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL);
                //iTextSharp.text.Font fontTable = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 8.0f, iTextSharp.text.Font.NORMAL);
                //iTextSharp.text.Font fontFooter = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL, BaseColor.GRAY);
                //iTextSharp.text.Font fontTiny = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 6.0f, iTextSharp.text.Font.NORMAL);

                //创建字体
                iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 14.0f, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font fontItem = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 12.0f, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font fontContent = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 10.0f, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font fontMessage = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font fontTable = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 7.0f, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font fontFooter = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL, BaseColor.GRAY);
                iTextSharp.text.Font fontTiny = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 6.0f, iTextSharp.text.Font.NORMAL);

                //iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(BaseFont.CreateFont(@"c:\windows\fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 14.0f, iTextSharp.text.Font.BOLD);
                //iTextSharp.text.Font fontItem = new iTextSharp.text.Font(BaseFont.CreateFont(@"c:\windows\fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 12.0f, iTextSharp.text.Font.BOLD);
                //iTextSharp.text.Font fontContent = new iTextSharp.text.Font(BaseFont.CreateFont(@"c:\windows\fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 10.0f, iTextSharp.text.Font.NORMAL);
                //iTextSharp.text.Font fontMessage = new iTextSharp.text.Font(BaseFont.CreateFont(@"c:\windows\fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL);
                //iTextSharp.text.Font fontTable = new iTextSharp.text.Font(BaseFont.CreateFont(@"C:\Windows\Fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 8.0f, iTextSharp.text.Font.NORMAL);
                //iTextSharp.text.Font fontFooter = new iTextSharp.text.Font(BaseFont.CreateFont(@"C:\Windows\Fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL, BaseColor.GRAY);
                //iTextSharp.text.Font fontTiny = new iTextSharp.text.Font(BaseFont.CreateFont(@"c:\windows\fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 6.0f, iTextSharp.text.Font.NORMAL);

                //添加页眉页脚
                if (signMode == 1)          //全部签字
                {
                    signMode = 3;           //第二部分报告，首页有页脚，全部签字
                }
                else if (signMode == 2)     //首末页签字
                {
                    signMode = 4;           //第二部分报告，首页有页脚，末页签字
                }
                writer.PageEvent = new HeaderFooterEvent(fontFooter, 2, signMode);               //添加绘制页眉页脚事件

                //创建分隔符
                Bitmap bitmap = new Bitmap(515, 1);
                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.FillRectangle(Brushes.Black, 0, 0, 515, 1);
                graphics.Dispose();
                separator = iTextSharp.text.Image.GetInstance(bitmap, System.Drawing.Imaging.ImageFormat.Bmp);

                //打开文档
                document.Open();

                #endregion

                #region 五、完整数据曲线图

                //创建新页
                document.NewPage();

                if (MyDefine.myXET.meTblVer7 != null && MyDefine.myXET.isF0Checked)
                {
                    document.Add(new Paragraph("六、完整数据曲线图", fontItem));
                    document.Add(new Paragraph(blankLine, fontTiny));
                }
                else
                {
                    document.Add(new Paragraph("五、完整数据曲线图", fontItem));
                    document.Add(new Paragraph(blankLine, fontTiny));
                }

                //画图
                if (picPath != "") document.Add(CreateImage(picPath, 500, 250));
                if (picPath == "") document.Add(new Paragraph("  无", fontContent));
                document.Add(new Paragraph(blankLine, fontTiny));
                #endregion

                #region 六、完整数据原始记录

                //创建新页
                document.NewPage();
                ////////////////////////////////////////////////////////////////////
                if (MyDefine.myXET.meTblVer7 != null && MyDefine.myXET.isF0Checked)
                {
                    document.Add(new Paragraph("七、完整数据原始记录", fontItem));
                    document.Add(new Paragraph(blankLine, fontTiny));
                }
                else
                {
                    document.Add(new Paragraph("六、完整数据原始记录", fontItem));
                    document.Add(new Paragraph(blankLine, fontTiny));
                }
                if (TemPdf)
                {
                    colnum = MyDefine.myXET.meDataTmpTbl.dataTable.Columns.Count - 5;  //后面要加采样次数列，所以+1
                    columnsCount = 0;
                    do
                    {
                        document.Add(new Paragraph(blankLine, fontTiny));
                        document.Add(separator);
                        document.Add(new Paragraph(blankLine, fontTiny));
                        int ix = 0;
                        if (colnum > 14)
                        {
                            if (columnsCount + 14 > colnum)
                            {
                                ix = colnum - columnsCount;
                            }
                            else
                            {
                                ix = 14;
                            }
                        }
                        else
                        {
                            ix = colnum;
                        }

                        float[] colWidthScale = new float[ix];          //设置列数
                        for (int i = 0; i < colWidthScale.Length; i++)
                        {
                            colWidthScale[i] = (i == 0) ? 70 : 35;          //设置列宽
                        }
                        colWidthScale[colWidthScale.Length - 1] = 35;       //采样次数列

                        pdftable = new PdfPTable(colWidthScale);            //创建Table，并设置列数
                        pdftable.HorizontalAlignment = Element.ALIGN_LEFT;
                        pdftable.WidthPercentage = 100;

                        pdftable.SetTotalWidth(colWidthScale);                //必须先设置TotalWidth再设置LockedWidth=true，否则表格宽度将为0，表格创建失败
                        pdftable.LockedWidth = true;                          //设置LockedWidth=true之前必须先设置表格总宽
                        pdftable.HeaderRows = 1;                              //设置新页显示表头

                        //添加列
                        for (int i = 2; i < columnsCount + ix + 1; i++)//MyDefine.myXET.meDataTbl.dataTable.Columns.Count
                        {
                            string colName = MyDefine.myXET.meDataTmpTbl.dataTable.Columns[i].ColumnName;
                            colName = colName.Contains(".Cor") ? colName.Replace(".Cor", "") : colName;
                            PdfPCell cell = new PdfPCell(new Phrase(colName, fontTable));
                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell.BorderWidth = 0;      //无边框
                                                       // cell.NoWrap = true;        //不换行
                            pdftable.AddCell(cell);
                            if (i == 2)
                            {
                                i += columnsCount;
                            }
                        }

                        //添加采样次数列
                        PdfPCell cell1 = new PdfPCell(new Phrase("采样次数", fontTable));
                        cell1.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell1.BorderWidth = 0;      //无边框
                        pdftable.AddCell(cell1);

                        //添加行
                        for (int i = 0; i < MyDefine.myXET.meDataTmpTbl.dataTable.Rows.Count - 4; i++)
                        {
                            //标记有效数据开始
                            for (int ilist = 0; ilist < MyDefine.myXET.meValidStageNum * 2; ilist += 2)
                            {
                                if (i == MyDefine.myXET.meValidIdxList[ilist])
                                {
                                    string stage;
                                    stage = "P" + (ilist / 2 + 1).ToString() + MyDefine.myXET.meValidNameList[ilist / 2] + "开始";
                                    PdfPCell cell = new PdfPCell(new Phrase(stage, fontTable));
                                    cell.Colspan = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;       // 单元格占的列数(占满一行)
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    cell.BorderWidth = 0;      //无边框
                                    pdftable.AddCell(cell);
                                }
                            }

                            //按行输出测试数据
                            for (int j = 2; j < columnsCount + ix + 1; j++)//MyDefine.myXET.meDataTbl.dataTable.Columns.Count
                            {
                                string myStr = (MyDefine.myXET.meDataTmpTbl.dataTable.Rows[i][j]).ToString();

                                PdfPCell cell = new PdfPCell(new Phrase(myStr, fontTable));
                                cell.HorizontalAlignment = (j == 2) ? Element.ALIGN_LEFT : Element.ALIGN_CENTER;
                                cell.BorderWidth = 0;      //无边框
                                                           //cell.FixedHeight = Convert.ToInt32( textBox11.Text);
                                pdftable.AddCell(cell);
                                if (j == 2)
                                {
                                    j += columnsCount;
                                }
                            }

                            //添加采样次数
                            PdfPCell cell2 = new PdfPCell(new Phrase((i + 1).ToString(), fontTable));
                            cell2.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell2.BorderWidth = 0;      //无边框
                            pdftable.AddCell(cell2);

                            //标记有效数据结束
                            for (int ilist = 1; ilist < MyDefine.myXET.meValidStageNum * 2 + 1; ilist += 2)
                            {
                                if (i == MyDefine.myXET.meValidIdxList[ilist])
                                {
                                    string stage = "P" + (ilist / 2 + 1).ToString() + MyDefine.myXET.meValidNameList[ilist / 2] + "结束";
                                    PdfPCell cell = new PdfPCell(new Phrase(stage, fontTable));
                                    cell.Colspan = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;       // 单元格占的列数(占满一行)
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    cell.BorderWidth = 0;      //无边框
                                    pdftable.AddCell(cell);
                                }
                            }
                        }

                        document.Add(pdftable);
                        document.Add(new Paragraph(blankLine, fontTiny));
                        document.Add(separator);
                        document.Add(new Paragraph(blankLine, fontTiny));

                        columnsCount += 12;
                    } while (columnsCount + 2 < colnum);
                }

                if (HumPdf)
                {
                    colnum = MyDefine.myXET.meDataHumTbl.dataTable.Columns.Count - 5;  //后面要加采样次数列，所以+1
                    columnsCount = 0;
                    do
                    {
                        document.Add(new Paragraph(blankLine, fontTiny));
                        document.Add(separator);
                        document.Add(new Paragraph(blankLine, fontTiny));
                        int ix = 0;
                        if (colnum > 14)
                        {
                            if (columnsCount + 14 > colnum)
                            {
                                ix = colnum - columnsCount;
                            }
                            else
                            {
                                ix = 14;
                            }
                        }
                        else
                        {
                            ix = colnum;
                        }

                        float[] colWidthScale = new float[ix];          //设置列数
                        for (int i = 0; i < colWidthScale.Length; i++)
                        {
                            colWidthScale[i] = (i == 0) ? 70 : 35;          //设置列宽
                        }
                        colWidthScale[colWidthScale.Length - 1] = 35;       //采样次数列

                        pdftable = new PdfPTable(colWidthScale);            //创建Table，并设置列数
                        pdftable.HorizontalAlignment = Element.ALIGN_LEFT;
                        pdftable.WidthPercentage = 100;

                        pdftable.SetTotalWidth(colWidthScale);                //必须先设置TotalWidth再设置LockedWidth=true，否则表格宽度将为0，表格创建失败
                        pdftable.LockedWidth = true;                          //设置LockedWidth=true之前必须先设置表格总宽
                        pdftable.HeaderRows = 1;                              //设置新页显示表头

                        //添加列
                        for (int i = 2; i < columnsCount + ix + 1; i++)//MyDefine.myXET.meDataTbl.dataTable.Columns.Count
                        {
                            string colName = MyDefine.myXET.meDataHumTbl.dataTable.Columns[i].ColumnName;
                            colName = colName.Contains(".Cor") ? colName.Replace(".Cor", "") : colName;
                            PdfPCell cell = new PdfPCell(new Phrase(colName, fontTable));
                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell.BorderWidth = 0;      //无边框
                            //cell.NoWrap = true;        //不换行
                            pdftable.AddCell(cell);
                            if (i == 2)
                            {
                                i += columnsCount;
                            }
                        }

                        //添加采样次数列
                        PdfPCell cell1 = new PdfPCell(new Phrase("采样次数", fontTable));
                        cell1.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell1.BorderWidth = 0;      //无边框
                        pdftable.AddCell(cell1);

                        //添加行
                        for (int i = 0; i < MyDefine.myXET.meDataHumTbl.dataTable.Rows.Count - 4; i++)
                        {
                            //标记有效数据开始
                            for (int ilist = 0; ilist < MyDefine.myXET.meValidStageNum * 2; ilist += 2)
                            {
                                if (i == MyDefine.myXET.meValidIdxList[ilist])
                                {
                                    string stage;
                                    stage = "P" + (ilist / 2 + 1).ToString() + MyDefine.myXET.meValidNameList[ilist / 2] + "开始";
                                    PdfPCell cell = new PdfPCell(new Phrase(stage, fontTable));
                                    cell.Colspan = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;       // 单元格占的列数(占满一行)
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    cell.BorderWidth = 0;      //无边框
                                    pdftable.AddCell(cell);
                                }
                            }

                            //按行输出测试数据
                            for (int j = 2; j < columnsCount + ix + 1; j++)//MyDefine.myXET.meDataTbl.dataTable.Columns.Count
                            {
                                string myStr = (MyDefine.myXET.meDataHumTbl.dataTable.Rows[i][j]).ToString();

                                PdfPCell cell = new PdfPCell(new Phrase(myStr, fontTable));
                                cell.HorizontalAlignment = (j == 2) ? Element.ALIGN_LEFT : Element.ALIGN_CENTER;
                                cell.BorderWidth = 0;      //无边框
                                                           //cell.FixedHeight = Convert.ToInt32( textBox11.Text);
                                pdftable.AddCell(cell);
                                if (j == 2)
                                {
                                    j += columnsCount;
                                }
                            }

                            //添加采样次数
                            PdfPCell cell2 = new PdfPCell(new Phrase((i + 1).ToString(), fontTable));
                            cell2.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell2.BorderWidth = 0;      //无边框
                            pdftable.AddCell(cell2);

                            //标记有效数据结束
                            for (int ilist = 1; ilist < MyDefine.myXET.meValidStageNum * 2 + 1; ilist += 2)
                            {
                                if (i == MyDefine.myXET.meValidIdxList[ilist])
                                {
                                    string stage = "P" + (ilist / 2 + 1).ToString() + MyDefine.myXET.meValidNameList[ilist / 2] + "结束";
                                    PdfPCell cell = new PdfPCell(new Phrase(stage, fontTable));
                                    cell.Colspan = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;       // 单元格占的列数(占满一行)
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    cell.BorderWidth = 0;      //无边框
                                    pdftable.AddCell(cell);
                                }
                            }
                        }

                        document.Add(pdftable);
                        document.Add(new Paragraph(blankLine, fontTiny));
                        document.Add(separator);
                        document.Add(new Paragraph(blankLine, fontTiny));

                        columnsCount += 12;
                    } while (columnsCount + 2 < colnum);
                }

                if (PrsPdf)
                {
                    colnum = MyDefine.myXET.meDataPrsTbl.dataTable.Columns.Count - 4;  //后面要加采样次数列，所以+1
                    columnsCount = 0;
                    do
                    {
                        document.Add(new Paragraph(blankLine, fontTiny));
                        document.Add(separator);
                        document.Add(new Paragraph(blankLine, fontTiny));
                        int ix = 0;
                        if (colnum > 14)
                        {
                            if (columnsCount + 14 > colnum)
                            {
                                ix = colnum - columnsCount;
                            }
                            else
                            {
                                ix = 14;
                            }
                        }
                        else
                        {
                            ix = colnum;
                        }

                        float[] colWidthScale = new float[ix];          //设置列数
                        for (int i = 0; i < colWidthScale.Length; i++)
                        {
                            colWidthScale[i] = (i == 0) ? 70 : 35;          //设置列宽
                        }
                        colWidthScale[colWidthScale.Length - 1] = 35;       //采样次数列

                        pdftable = new PdfPTable(colWidthScale);            //创建Table，并设置列数
                        pdftable.HorizontalAlignment = Element.ALIGN_LEFT;
                        pdftable.WidthPercentage = 100;

                        pdftable.SetTotalWidth(colWidthScale);                //必须先设置TotalWidth再设置LockedWidth=true，否则表格宽度将为0，表格创建失败
                        pdftable.LockedWidth = true;                          //设置LockedWidth=true之前必须先设置表格总宽

                        //添加列
                        for (int i = 1; i < columnsCount + ix; i++)//MyDefine.myXET.meDataTbl.dataTable.Columns.Count
                        {
                            string colName = MyDefine.myXET.meDataPrsTbl.dataTable.Columns[i].ColumnName;
                            colName = colName.Contains(".Cor") ? colName.Replace(".Cor", "") : colName;
                            PdfPCell cell = new PdfPCell(new Phrase(colName, fontTable));
                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell.BorderWidth = 0;      //无边框
                            pdftable.AddCell(cell);
                            if (i == 1)
                            {
                                i += columnsCount;
                            }
                        }

                        //添加采样次数列
                        PdfPCell cell1 = new PdfPCell(new Phrase("采样次数", fontTable));
                        cell1.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell1.BorderWidth = 0;      //无边框
                        pdftable.AddCell(cell1);

                        //添加行
                        for (int i = 0; i < MyDefine.myXET.meDataPrsTbl.dataTable.Rows.Count - 4; i++)
                        {
                            //标记有效数据开始
                            for (int ilist = 0; ilist < MyDefine.myXET.meValidStageNum * 2; ilist += 2)
                            {
                                if (i == MyDefine.myXET.meValidIdxList[ilist])
                                {
                                    string stage;
                                    stage = "P" + (ilist / 2 + 1).ToString() + MyDefine.myXET.meValidNameList[ilist / 2] + "开始";
                                    PdfPCell cell = new PdfPCell(new Phrase(stage, fontTable));
                                    cell.Colspan = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;       // 单元格占的列数(占满一行)
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    cell.BorderWidth = 0;      //无边框
                                    pdftable.AddCell(cell);
                                }
                            }

                            //按行输出测试数据
                            for (int j = 1; j < columnsCount + ix; j++)//MyDefine.myXET.meDataTbl.dataTable.Columns.Count
                            {
                                string myStr = (MyDefine.myXET.meDataPrsTbl.dataTable.Rows[i][j]).ToString();

                                PdfPCell cell = new PdfPCell(new Phrase(myStr, fontTable));
                                cell.HorizontalAlignment = (j == 1) ? Element.ALIGN_LEFT : Element.ALIGN_CENTER;
                                cell.BorderWidth = 0;      //无边框
                                                           //cell.FixedHeight = Convert.ToInt32( textBox11.Text);
                                pdftable.AddCell(cell);
                                if (j == 1)
                                {
                                    j += columnsCount;
                                }
                            }

                            //添加采样次数
                            PdfPCell cell2 = new PdfPCell(new Phrase((i + 1).ToString(), fontTable));
                            cell2.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell2.BorderWidth = 0;      //无边框
                            pdftable.AddCell(cell2);

                            //标记有效数据结束
                            for (int ilist = 1; ilist < MyDefine.myXET.meValidStageNum * 2 + 1; ilist += 2)
                            {
                                if (i == MyDefine.myXET.meValidIdxList[ilist])
                                {
                                    string stage = "P" + (ilist / 2 + 1).ToString() + MyDefine.myXET.meValidNameList[ilist / 2] + "结束";
                                    PdfPCell cell = new PdfPCell(new Phrase(stage, fontTable));
                                    cell.Colspan = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;       // 单元格占的列数(占满一行)
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    cell.BorderWidth = 0;      //无边框
                                    pdftable.AddCell(cell);
                                }
                            }
                        }

                        document.Add(pdftable);
                        document.Add(new Paragraph(blankLine, fontTiny));
                        document.Add(separator);
                        document.Add(new Paragraph(blankLine, fontTiny));

                        columnsCount += 12;
                    } while (columnsCount + 2 < colnum);
                }

                ////////////////////////////////////////////////////////////////////
                #endregion

                #region 末页签字(仅首末页签字模式)

                if (signMode == 4)
                {
                    //首末页签字模式：在最后一页的空白处加上签字和日期
                    //页脚设置
                    PdfPTable footerTable = new PdfPTable(3);
                    footerTable.TotalWidth = PageSize.A4.Width - 80;            //设置表格总宽
                    footerTable.SetWidths(new int[] { 12, 10, 10 });            //设置列宽比例
                    footerTable.LockedWidth = true;                             //锁定表格宽度
                    footerTable.DefaultCell.FixedHeight = -10;                  //设置单元格高度
                    footerTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;       //设置单元格边框
                    footerTable.DefaultCell.BorderWidth = 0.5f;                 //设置单元格线宽
                    footerTable.DefaultCell.BorderColor = BaseColor.LIGHT_GRAY; //设置边框颜色

                    footerTable.AddCell(new Paragraph("  ", fontFooter));
                    footerTable.AddCell(new Paragraph("验证人：" + MyDefine.myXET.calibMem, fontFooter));
                    footerTable.AddCell(new Paragraph("复核人：" + MyDefine.myXET.reviewMem, fontFooter));

                    footerTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;      //设置单元格边框
                    footerTable.AddCell(new Paragraph("  ", fontFooter));
                    footerTable.AddCell(new Paragraph("验证日期：" + MyDefine.myXET.calibDate, fontFooter));
                    footerTable.AddCell(new Paragraph("复核日期：" + MyDefine.myXET.reviewDate, fontFooter));
                    footerTable.AddCell(new Paragraph("  ", fontFooter));
                    footerTable.AddCell(new Paragraph("  ", fontFooter));
                    footerTable.AddCell(new Paragraph("  ", fontFooter));

                    //写入页脚 -- 写到指定位置
                    footerTable.WriteSelectedRows(0, -1, 40, 60, writer.DirectContent);                           //写入页脚(位置在下面)
                }
                #endregion

                #region 关闭文件

                //关闭
                document.Close();

                //调出pdf
                Process.Start(filepath);

                #endregion

                MyDefine.myXET.AddTraceInfo("生成报告成功");
                return true;
            }
            catch (Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("生成报告失败：" + ex.ToString());
                return false;
            }
        }

        public Boolean createPDFReportVerify(string filepath)
        {
            try
            {

                //使用
                String blankLine = " ";
                String mDeviceType = "";
                String mDeviceID = "";
                int index = -1;
                PdfPTable pdftable;
                float[] colWidth;       //列宽比例数组
                List<String> myLS = new List<String>();
                iTextSharp.text.Image separator;
                int colnum;
                int columnsCount;

                #region 数据来源

                rptdate = MyDefine.myXET.homdate;
                rpttime = MyDefine.myXET.homstart;
                rptstop = MyDefine.myXET.homstop;
                rptspan = MyDefine.myXET.homspan;
                rptrun = MyDefine.myXET.homrun;
                mDeviceID = MyDefine.myXET.hom_Model;
                mDeviceType = MyDefine.myXET.hom_Type;

                #endregion

                #region 参数分配

                //基本信息

                String companyName = textBox1.Text;         //公司名称
                String reportNameCH = textBox2.Text;        //报告名称-中文
                String reportNameEN = textBox3.Text;        //报告名称-英文
                //String reportCode = textBox4.Text;        //报告编号
                String reportTypeCH = comboBox2.SelectedItem.ToString().Split('|')[0];//报告类型
                String reportTypeEN = comboBox2.SelectedItem.ToString().Split('|')[1];//报告类型
                String picPath = textBox9.Text;            //图片路径
                String dataType = MyDefine.myXET.homunit;          //℃/%RH/kPa
                int signMode = radioButton1.Checked ? 1 : 2;       //签字模式：首末页签字；全部签字

                //记录间隔单位转换
                int intspan = Convert.ToInt32(rptspan);
                string rptspan2 = rptspan + " 秒";
                if (intspan % 60 == 0) rptspan2 = (intspan / 60).ToString() + " 分";
                if (intspan % 3600 == 0) rptspan2 = (intspan / 3600).ToString() + " 时";

                //运行时长单位转换
                int intrun = Convert.ToInt32(rptrun);
                string rptrun2 = rptrun + " 秒";
                if (intrun >= 60) rptrun2 = (intrun / 60).ToString("F2") + " 分";
                if (intrun >= 3600) rptrun2 = (intrun / 3600).ToString("F2") + " 时";

                //页眉页脚信息
                MyDefine.myXET.logopath = textBox10.Text;           //Logo图片
                MyDefine.myXET.operaMem = MyDefine.myXET.userName;  //操作人员
                MyDefine.myXET.calibMem = textBox5.Text;            //校准人员
                MyDefine.myXET.calibDate = textBox6.Text;           //校准日期
                MyDefine.myXET.reviewMem = textBox7.Text;           //审核人员
                MyDefine.myXET.reviewDate = textBox8.Text;          //审核日期

                #endregion

                #region 参数完整性检查

                if (picPath != "" && !File.Exists(picPath))
                {
                    MessageBox.Show("图片路径错误，请重新选择图片！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.logopath != "" && !File.Exists(MyDefine.myXET.logopath))
                {
                    MessageBox.Show("Logo路径错误，请重新选择图片！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meDataTbl == null)
                {
                    MessageBox.Show("尚未加载测试数据，请选择数据文件！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meTblVer == null)
                {
                    MessageBox.Show("尚未生成设备信息表，请在验证界面生成！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meTblVer1 == null)
                {
                    MessageBox.Show("尚未生成报告有效数据汇总表，请在验证界面生成！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meTblVer2 == null)
                {
                    MessageBox.Show("尚未生成详细数据横向汇总表，请在验证界面生成！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meTblVer3 == null)
                {
                    MessageBox.Show("尚未生成详细数据横向汇总表，请在验证界面生成！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meTblVer4 == null)
                {
                    MessageBox.Show("尚未生成详细数据横向汇总表，请在验证界面生成！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meTblVer5 == null)
                {
                    MessageBox.Show("尚未生成详细数据纵向汇总表，请在验证界面生成！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (MyDefine.myXET.meTblVer6 == null)
                {
                    MessageBox.Show("尚未生成关键参数汇总表，请在验证界面生成！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                #endregion

                #region 更新报告编号(需放在页眉页脚前)

                //更新报告编号
                MyDefine.myXET.repcode = updateReportCode();                //更新报告编号（报告编号+1）
                MyDefine.myXET.saveReportInfo();                            //保存报告编号及标准器信息表
                showReportCode();                                           //显示更新后的报告编号

                MyDefine.myXET.repcode = "JDYZ" + (DateTime.Now.Ticks / 100000000).ToString();       //根据系统时间实时生成报告编号
                #endregion

                #region 字体及页眉页脚定义

                //创建新文档对象,页边距(X,X,Y,Y)
                Document document = new Document(PageSize.A4, 40, 40, 65, 80);

                //路径设置; FileMode.Create文档不在会创建，存在会覆盖
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filepath, FileMode.Create));

                //当前登录账号的类别名称
                string name = MyDefine.myXET.meLoginUser.Split(';')[(int)ACCOUNT.USER];
                //是否有编辑PDF的权限
                if (MyDefine.myXET.CheckPermission(STEP.数据载入, false))
                {
                    writer.SetEncryption(null, Encoding.Default.GetBytes(name + "JD"), PdfWriter.ALLOW_PRINTING, PdfWriter.ENCRYPTION_AES_128 | PdfWriter.DO_NOT_ENCRYPT_METADATA);

                    //账户为汉字则启用超级密码
                    foreach (char item in name)
                    {
                        if ((int)item > 127)
                        {
                            writer.SetEncryption(null, Encoding.Default.GetBytes("JD20191206"), PdfWriter.ALLOW_PRINTING, PdfWriter.ENCRYPTION_AES_128 | PdfWriter.DO_NOT_ENCRYPT_METADATA);
                            break;
                        }
                    }
                }
                else
                {
                    writer.SetEncryption(null, null, PdfWriter.ALLOW_PRINTING, PdfWriter.ENCRYPTION_AES_128 | PdfWriter.DO_NOT_ENCRYPT_METADATA);
                }
                //writer.SetEncryption(Encoding.Default.GetBytes("Hello"), Encoding.Default.GetBytes("123456"), PdfWriter.ALLOW_SCREENREADERS, PdfWriter.STANDARD_ENCRYPTION_128);

                //添加信息
                document.AddTitle("HTR校准报告");
                document.AddAuthor(companyName);
                document.AddSubject(mDeviceType + " 温度测量报告");
                document.AddKeywords("HTR");
                document.AddCreator(MyDefine.myXET.operaMem);

                //创建字体(SIMYOU.TTF中的~符号不能上下居中)
                //iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 14.0f, iTextSharp.text.Font.BOLD);
                //iTextSharp.text.Font fontItem = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 12.0f, iTextSharp.text.Font.BOLD);
                //iTextSharp.text.Font fontContent = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 10.0f, iTextSharp.text.Font.NORMAL);
                //iTextSharp.text.Font fontMessage = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL);
                //iTextSharp.text.Font fontTable = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 8.0f, iTextSharp.text.Font.NORMAL);
                //iTextSharp.text.Font fontFooter = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL, BaseColor.GRAY);
                //iTextSharp.text.Font fontTiny = new iTextSharp.text.Font(BaseFont.CreateFont(@".\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 6.0f, iTextSharp.text.Font.NORMAL);

                //创建字体
                iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 14.0f, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font fontItem = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 12.0f, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font fontContent = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 10.0f, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font fontContent2 = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font fontMessage = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font fontTable = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 8.0f, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font fontFooter = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL, BaseColor.GRAY);
                iTextSharp.text.Font fontTiny = new iTextSharp.text.Font(BaseFont.CreateFont(@".\Deng.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 6.0f, iTextSharp.text.Font.NORMAL);

                //iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(BaseFont.CreateFont(@"c:\windows\fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 14.0f, iTextSharp.text.Font.BOLD);
                //iTextSharp.text.Font fontItem = new iTextSharp.text.Font(BaseFont.CreateFont(@"c:\windows\fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 12.0f, iTextSharp.text.Font.BOLD);
                //iTextSharp.text.Font fontContent = new iTextSharp.text.Font(BaseFont.CreateFont(@"c:\windows\fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 10.0f, iTextSharp.text.Font.NORMAL);
                //iTextSharp.text.Font fontMessage = new iTextSharp.text.Font(BaseFont.CreateFont(@"c:\windows\fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL);
                //iTextSharp.text.Font fontTable = new iTextSharp.text.Font(BaseFont.CreateFont(@"C:\Windows\Fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 8.0f, iTextSharp.text.Font.NORMAL);
                //iTextSharp.text.Font fontFooter = new iTextSharp.text.Font(BaseFont.CreateFont(@"C:\Windows\Fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 9.0f, iTextSharp.text.Font.NORMAL, BaseColor.GRAY);
                //iTextSharp.text.Font fontTiny = new iTextSharp.text.Font(BaseFont.CreateFont(@"c:\windows\fonts\SIMYOU.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED), 6.0f, iTextSharp.text.Font.NORMAL);

                //添加页眉页脚
                writer.PageEvent = new HeaderFooterEvent(fontFooter, 2, signMode);               //添加绘制页眉页脚事件

                //创建分隔符
                Bitmap bitmap = new Bitmap(515, 1);
                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.FillRectangle(Brushes.Black, 0, 0, 515, 1);
                graphics.Dispose();
                separator = iTextSharp.text.Image.GetInstance(bitmap, System.Drawing.Imaging.ImageFormat.Bmp);

                //打开文档
                document.Open();

                #endregion

                #region 标题页

                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(CreateParagraph(companyName, fontTitle, Element.ALIGN_CENTER));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(CreateParagraph(reportNameCH, fontTitle, Element.ALIGN_CENTER));
                document.Add(CreateParagraph(reportNameEN, fontItem, Element.ALIGN_CENTER));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(CreateParagraph(reportTypeCH, fontTitle, Element.ALIGN_CENTER));
                document.Add(CreateParagraph(reportTypeEN, fontItem, Element.ALIGN_CENTER));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));

                PdfPTable table = new PdfPTable(2);
                table.TotalWidth = PageSize.A4.Width - 80;                //设置表格总宽
                table.SetWidths(new int[] { 8, 20 });                      //设置列宽比例
                table.LockedWidth = true;                                 //锁定表格宽度
                table.DefaultCell.FixedHeight = -10;                      //设置单元格高度
                table.DefaultCell.BorderWidth = 0;                        //设置单元格线宽

                table.AddCell(new Paragraph("", fontTitle));
                table.AddCell(new Paragraph("验证人员：" + MyDefine.myXET.calibMem, fontTitle));
                table.AddCell(new Paragraph("", fontTitle));
                table.AddCell(new Paragraph("验证日期：" + MyDefine.myXET.calibDate, fontTitle));
                table.AddCell(new Paragraph("", fontTitle));
                table.AddCell(new Paragraph("复核人员：" + MyDefine.myXET.reviewMem, fontTitle));
                table.AddCell(new Paragraph("", fontTitle));
                table.AddCell(new Paragraph("复核日期：" + MyDefine.myXET.reviewDate, fontTitle));

                document.Add(table);

                //document.Add(CreateParagraph(addSpace(20) + "验证人员：ADMIN", fontTitle, Element.ALIGN_LEFT));
                //document.Add(CreateParagraph(addSpace(30) + "Validation Personnel：ADMIN", fontItem, Element.ALIGN_LEFT));
                //document.Add(CreateParagraph(addSpace(20) + "报告日期：2021-10-12", fontTitle, Element.ALIGN_LEFT));
                //document.Add(CreateParagraph(addSpace(30) + "Report date：2021-10-12", fontItem, Element.ALIGN_LEFT));



                #endregion

                #region 一、验证对象

                //创建新页
                document.NewPage();
                ////////////////////////////////////////////////////////////////////
                //document.Add(CreateParagraph("晶度技术（北京）有限公司", fontTitle, Element.ALIGN_CENTER));
                document.Add(CreateParagraph(reportTypeCH + " 报告", fontTitle, Element.ALIGN_CENTER));
                document.Add(new Paragraph(blankLine, fontItem));
                document.Add(new Paragraph(blankLine, fontItem));

                #region 使用list输出信息
                /*
                myLS.Clear();
                myLS.Add("一、验证对象");
                myLS.Add("验证类型：");
                myLS.Add("通道类型：");
                doWithExactLen(ref myLS, LEN1);    //将字符串调整为等长

                myLS[1] += reportTypeCH;
                myLS[2] += dataType;
                doWithExactLen(ref myLS, LEN2);    //将字符串调整为等长

                myLS[1] += "设备型号：";
                myLS[2] += "系统版本：";
                doWithExactLen(ref myLS, LEN3);    //将字符串调整为等长

                myLS[1] += mDeviceID;
                myLS[2] += Constants.SW_Version;

                document.Add(new Paragraph(myLS[0], fontItem));
                document.Add(new Paragraph(blankLine, fontTiny));
                document.Add(new Paragraph(myLS[1], fontContent));
                document.Add(new Paragraph(myLS[2], fontContent));
                document.Add(new Paragraph(blankLine, fontItem));
                myLS.Clear();
                */
                #endregion

                #region 使用PdfPTable输出信息

                ////////////////////////////////////////////////////////////////////
                document.Add(new Paragraph("一、验证对象", fontItem));
                document.Add(new Paragraph(blankLine, fontTiny));

                colWidth = new float[] { 15, 40, 15, 40 };      //设置列宽比例，4列
                pdftable = new PdfPTable(colWidth);             //创建Table，并设置列数
                pdftable.HorizontalAlignment = Element.ALIGN_LEFT;
                pdftable.DefaultCell.BorderWidth = 0;           //无边框
                pdftable.WidthPercentage = 100;                 //占满整行

                pdftable.AddCell(new Paragraph("验证类型：", fontContent));
                pdftable.AddCell(new Paragraph(reportTypeCH, fontContent));
                pdftable.AddCell(new Paragraph("设备型号：", fontContent));
                pdftable.AddCell(new Paragraph(mDeviceID, fontContent));
                pdftable.AddCell(new Paragraph("通道类型：", fontContent));
                pdftable.AddCell(new Paragraph(dataType, fontContent));
                pdftable.AddCell(new Paragraph("系统版本：", fontContent));
                pdftable.AddCell(new Paragraph(Constants.SW_Version, fontContent));

                document.Add(pdftable);
                document.Add(new Paragraph(blankLine, fontItem));

                #endregion

                #endregion

                #region 二、报告信息

                #region 使用list输出信息
                /*
                myLS.Clear();
                myLS.Add("二、报告信息");
                myLS.Add("操作人员：");
                myLS.Add("开始时间：");
                myLS.Add("结束时间：");
                myLS.Add("记录间隔：");
                myLS.Add("运行时长：");
                myLS.Add("备注：");
                doWithExactLen(ref myLS, LEN1);    //将字符串调整为等长

                myLS[1] += MyDefine.myXET.operaMem;
                myLS[2] += rpttime;
                myLS[3] += rptstop;
                myLS[4] += rptspan;
                myLS[5] += rptrun;
                doWithExactLen(ref myLS, LEN2);    //将字符串调整为等长

                myLS[1] += "验证人员：";
                myLS[2] += "验证日期：";
                myLS[3] += "审核人员：";
                myLS[4] += "审核日期：";
                myLS[5] += "设备数量：";
                doWithExactLen(ref myLS, LEN3);    //将字符串调整为等长

                myLS[1] += MyDefine.myXET.calibMem;
                myLS[2] += MyDefine.myXET.calibDate;
                myLS[3] += MyDefine.myXET.reviewMem;
                myLS[4] += MyDefine.myXET.reviewDate;
                myLS[5] += MyDefine.myXET.meDUTNum;

                document.Add(new Paragraph(myLS[0], fontItem));
                document.Add(new Paragraph(blankLine, fontTiny));
                document.Add(new Paragraph(myLS[1], fontContent));
                document.Add(new Paragraph(myLS[2], fontContent));
                document.Add(new Paragraph(myLS[3], fontContent));
                document.Add(new Paragraph(myLS[4], fontContent));
                document.Add(new Paragraph(myLS[5], fontContent));
                document.Add(new Paragraph(blankLine, fontItem));
                */
                #endregion

                #region 使用PdfPTable输出信息

                document.Add(new Paragraph("二、报告信息", fontItem));
                document.Add(new Paragraph(blankLine, fontTiny));

                colWidth = new float[] { 15, 40 };      //设置列宽比例，2列
                pdftable = new PdfPTable(colWidth);             //创建Table，并设置列数
                pdftable.HorizontalAlignment = Element.ALIGN_LEFT;
                pdftable.DefaultCell.BorderWidth = 0;           //无边框
                pdftable.WidthPercentage = 100;                 //占满整行

                //第一行
                pdftable.AddCell(new Paragraph("操作人员：", fontContent));
                pdftable.AddCell(new Paragraph(MyDefine.myXET.operaMem, fontContent));

                //第二行
                pdftable.AddCell(new Paragraph("开始时间：", fontContent));
                pdftable.AddCell(new Paragraph(rpttime, fontContent));

                //第三行
                pdftable.AddCell(new Paragraph("结束时间：", fontContent));
                pdftable.AddCell(new Paragraph(rptstop, fontContent));

                //第四行
                pdftable.AddCell(new Paragraph("记录间隔：", fontContent));
                pdftable.AddCell(new Paragraph(rptspan2, fontContent));

                //第五行
                pdftable.AddCell(new Paragraph("运行时长：", fontContent));
                pdftable.AddCell(new Paragraph(rptrun2, fontContent));

                //第六行
                pdftable.AddCell(new Paragraph("设备数量：", fontContent));
                pdftable.AddCell(new Paragraph(MyDefine.myXET.meDUTNum.ToString(), fontContent));

                document.Add(pdftable);
                document.Add(new Paragraph(blankLine, fontItem));

                #endregion

                #endregion

                #region 三、设备信息(打印验证界面显示的表格)

                document.Add(new Paragraph("三、设备信息", fontItem));
                document.Add(new Paragraph(blankLine, fontItem));

                index = -1;
                colWidth = new float[] { 5, 10, 10, 16, 10, 10 }; //设置列宽比例，6列
                if (TemPdf)
                {
                    pdftable = getPDFPTable(MyDefine.myXET.meTemVer.dataTable, colWidth, fontContent, ++index);    //生成PDF表格
                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));
                }
                if (HumPdf)
                {
                    pdftable = getPDFPTable(MyDefine.myXET.meHumVer.dataTable, colWidth, fontContent, ++index);    //生成PDF表格
                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));
                }
                if (PrsPdf)
                {
                    pdftable = getPDFPTable(MyDefine.myXET.mePrsVer.dataTable, colWidth, fontContent, ++index);    //生成PDF表格
                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));
                }
                ////////////////////////////////////////////////////////////////////

                #endregion

                #region 四、报告阶段数据汇总
                //创建新页
                document.NewPage();
                document.Add(new Paragraph("四、报告阶段数据汇总", fontItem));
                document.Add(new Paragraph(blankLine, fontTiny));
                index = -1;
                if (TemPdf)
                {
                    colWidth = new float[] { 14, 14, 14, 14, 10, 14, 14, 10, 14, 10, 14, 10, 10 };                   //设置列宽比例，10列
                    pdftable = getPDFPTable(MyDefine.myXET.meTemVer8.dataTable, colWidth, fontContent2, ++index);    //生成PDF表格
                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));
                }
                if (HumPdf)
                {
                    colWidth = new float[] { 14, 14, 14, 14, 10, 14, 14, 10, 14, 10, 14, 10, 10 };                   //设置列宽比例，10列
                    pdftable = getPDFPTable(MyDefine.myXET.meHumVer8.dataTable, colWidth, fontContent2, ++index);    //生成PDF表格
                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));
                }
                if (PrsPdf)
                {
                    colWidth = new float[] { 10, 10, 10, 10, 10, 15, 15 };                                           //设置列宽比例，7列
                    pdftable = getPDFPTable(MyDefine.myXET.mePrsVer8.dataTable, colWidth, fontContent2, ++index);    //生成PDF表格
                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));
                }

                #endregion

                #region 4.1 阶段曲线图
                int pictureNum = 0;
                //画图
                for (int i = 0; i < MyDefine.myXET.meValidStageNum; i++)
                {
                    if (pictureNum % 2 == 0)
                    {
                        //创建新页
                        document.NewPage();

                        if (pictureNum == 0)
                        {
                            document.Add(new Paragraph("4.1 阶段曲线图", fontItem));
                            document.Add(new Paragraph(blankLine, fontTiny));
                        }
                    }
                    if (together)
                    {
                        document.Add(CreateParagraph(MyDefine.myXET.meValidNameList[i], fontItem, Element.ALIGN_CENTER));
                        iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(GetStageBitmapPoint(false, i * 2), System.Drawing.Imaging.ImageFormat.Bmp);
                        document.Add(image);
                        pictureNum++;
                    }
                    else
                    {
                        if (TemPdf)
                        {
                            document.Add(CreateParagraph(MyDefine.myXET.meValidNameList[i] + "（温度）", fontItem, Element.ALIGN_CENTER));
                            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(GetStageBitmapPoint(true, i * 2), System.Drawing.Imaging.ImageFormat.Bmp);
                            document.Add(image);
                            pictureNum++;
                        }
                        if (HumPdf)
                        {
                            document.Add(CreateParagraph(MyDefine.myXET.meValidNameList[i] + "（湿度）", fontItem, Element.ALIGN_CENTER));
                            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(GetStageBitmapPoint(false, i * 2), System.Drawing.Imaging.ImageFormat.Bmp);
                            document.Add(image);
                            pictureNum++;
                        }
                        if (PrsPdf)
                        {
                            document.Add(CreateParagraph(MyDefine.myXET.meValidNameList[i] + "（压力）", fontItem, Element.ALIGN_CENTER));
                            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(GetStageBitmapPoint(false, i * 2), System.Drawing.Imaging.ImageFormat.Bmp);
                            document.Add(image);
                            pictureNum++;
                        }
                    }
                }

                #endregion

                #region 4.2 阶段关键参数汇总
                //创建新页
                document.NewPage();
                document.Add(new Paragraph("4.2 阶段关键参数汇总", fontItem));
                document.Add(new Paragraph(blankLine, fontTiny));
                index = -1;
                colWidth = new float[] { 10, 10, 10, 10, 10, 10 }; //设置列宽比例，6列
                if (TemPdf)
                {
                    pdftable = getPDFPTable(MyDefine.myXET.meTemVer6.dataTable, colWidth, fontContent, ++index);    //生成PDF表格
                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));
                }
                if (HumPdf)
                {
                    pdftable = getPDFPTable(MyDefine.myXET.meHumVer6.dataTable, colWidth, fontContent, ++index);    //生成PDF表格
                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));
                }
                if (PrsPdf)
                {
                    pdftable = getPDFPTable(MyDefine.myXET.mePrsVer6.dataTable, colWidth, fontContent, ++index);    //生成PDF表格
                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));
                }

                #endregion

                #region 4.3 阶段数据纵向汇总
                //创建新页
                document.NewPage();
                document.Add(new Paragraph("4.3 阶段数据纵向汇总", fontItem));
                document.Add(new Paragraph(blankLine, fontTiny));
                index = -1;
                colWidth = new float[] { 10, 14, 17, 17, 12, 10, 10, 8, 10 }; //设置列宽比例，9列
                if (TemPdf)
                {
                    pdftable = getPDFPTable(MyDefine.myXET.meTemVer5.dataTable, colWidth, fontContent, ++index);    //生成PDF表格
                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));
                }
                if (HumPdf)
                {
                    pdftable = getPDFPTable(MyDefine.myXET.meHumVer5.dataTable, colWidth, fontContent, ++index);    //生成PDF表格
                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));
                }
                if (PrsPdf)
                {
                    pdftable = getPDFPTable(MyDefine.myXET.mePrsVer5.dataTable, colWidth, fontContent, ++index);    //生成PDF表格
                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));
                }

                #endregion

                #region 4.4 阶段数据横向汇总
                //创建新页
                document.NewPage();
                document.Add(new Paragraph("4.4 阶段数据横向汇总", fontItem));
                document.Add(new Paragraph(blankLine, fontTiny));
                colWidth = new float[] { 18, 8, 13, 8, 13, 8, 8, 10, 10 }; //设置列宽比例，9列
                if (MyDefine.myXET.meTblVer2.dataTable.Rows.Count > 0 && TemPdf)
                {
                    //温度汇总表
                    document.Add(new Paragraph("温度：", fontContent));
                    document.Add(new Paragraph(blankLine, fontMessage));
                    pdftable = getPDFPTable(MyDefine.myXET.meTblVer2.dataTable, colWidth, fontMessage);    //生成PDF表格
                    pdftable.HeaderRows = 1;                                                               //标题栏跨页重复出现
                    document.Add(pdftable);
                }

                if (MyDefine.myXET.meTblVer3.dataTable.Rows.Count > 0 && HumPdf)
                {
                    //湿度汇总表
                    document.Add(new Paragraph("湿度：", fontContent));
                    document.Add(new Paragraph(blankLine, fontMessage));
                    pdftable = getPDFPTable(MyDefine.myXET.meTblVer3.dataTable, colWidth, fontMessage);    //生成PDF表格
                    pdftable.HeaderRows = 1;                                                               //标题栏跨页重复出现
                    document.Add(pdftable);
                }

                if (MyDefine.myXET.meTblVer4.dataTable.Rows.Count > 0 && PrsPdf)
                {
                    //压力汇总表
                    document.Add(new Paragraph("压力：", fontContent));
                    document.Add(new Paragraph(blankLine, fontMessage));
                    pdftable = getPDFPTable(MyDefine.myXET.meTblVer4.dataTable, colWidth, fontMessage);    //生成PDF表格
                    pdftable.HeaderRows = 1;                                                               //标题栏跨页重复出现
                    document.Add(pdftable);
                }

                #endregion

                #region 五、F0值数据分析

                if (MyDefine.myXET.meTblVer7 != null && MyDefine.myXET.isF0Checked)
                {
                    document.NewPage();

                    document.Add(new Paragraph("五、F0值数据分析", fontItem));
                    document.Add(new Paragraph(blankLine, fontTiny));

                    //添加有效数据汇总表(仅温度行)
                    colWidth = new float[] { 10, 10, 10, 10, 10, 15, 15 }; //设置列宽比例，7列
                    dataTableClass mytable = new dataTableClass();
                    mytable.dataTable = MyDefine.myXET.meTblVer1.CopyTable();
                    for (int i = mytable.dataTable.Rows.Count - 1; i > 0; i--)      //删除可能存在的湿度或压力行
                    {
                        string tpye = mytable.GetCellValue(i, 2);
                        if (tpye.Contains("温度") == false) mytable.DeleteTableRow(i);
                    }
                    pdftable = getPDFPTable(mytable.dataTable, colWidth, fontContent);    //生成PDF表格

                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));

                    //添加F0值数据表
                    colnum = MyDefine.myXET.meTblVer7.dataTable.Columns.Count;
                    colWidth = new float[colnum];      //设置列数
                    for (int i = 0; i < colWidth.Length; i++)
                    {
                        colWidth[i] = (i == 0) ? 15 : (100 - 15) / (colnum - 1);                         //设置列宽(让时间列固定占15/100列宽，防止时间显示不开)
                    }
                    pdftable = getPDFPTable(MyDefine.myXET.meTblVer7.dataTable, colWidth, fontTable);    //生成PDF表格
                    pdftable.HeaderRows = 1;                                                             //设置新页显示表头
                    document.Add(pdftable);
                    document.Add(new Paragraph(blankLine, fontItem));
                }

                #endregion

                #region 六、完整数据曲线图

                if (togetherPDF)
                {
                    //创建新页
                    document.NewPage();

                    if (MyDefine.myXET.meTblVer7 != null && MyDefine.myXET.isF0Checked)
                    {
                        document.Add(new Paragraph("六、完整数据曲线图", fontItem));
                        document.Add(new Paragraph(blankLine, fontTiny));
                    }
                    else
                    {
                        document.Add(new Paragraph("五、完整数据曲线图", fontItem));
                        document.Add(new Paragraph(blankLine, fontTiny));
                    }

                    //画图
                    if (picPath != "") document.Add(CreateImage(picPath, 500, 250));
                    if (picPath == "") document.Add(new Paragraph("  无", fontContent));
                    document.Add(new Paragraph(blankLine, fontTiny));
                }
                #endregion

                #region 七、完整数据原始记录
                ////////////////////////////////////////////////////////////////////

                if (togetherPDF)
                {
                    //创建新页
                    document.NewPage();

                    if (MyDefine.myXET.meTblVer7 != null && MyDefine.myXET.isF0Checked)
                    {
                        document.Add(new Paragraph("七、完整数据原始记录", fontItem));
                        document.Add(new Paragraph(blankLine, fontTiny));
                    }
                    else
                    {
                        document.Add(new Paragraph("六、完整数据原始记录", fontItem));
                        document.Add(new Paragraph(blankLine, fontTiny));
                    }
                    if (TemPdf)
                    {
                        colnum = MyDefine.myXET.meDataTmpTbl.dataTable.Columns.Count - 5;  //后面要加采样次数列，所以+1
                        columnsCount = 0;
                        do
                        {
                            document.Add(new Paragraph(blankLine, fontTiny));
                            document.Add(separator);
                            document.Add(new Paragraph(blankLine, fontTiny));
                            int ix = 0;
                            if (colnum > 14)
                            {
                                if (columnsCount + 14 > colnum)
                                {
                                    ix = colnum - columnsCount;
                                }
                                else
                                {
                                    ix = 14;
                                }
                            }
                            else
                            {
                                ix = colnum;
                            }

                            float[] colWidthScale = new float[ix];          //设置列数
                            for (int i = 0; i < colWidthScale.Length; i++)
                            {
                                colWidthScale[i] = (i == 0) ? 70 : 35;          //设置列宽
                            }
                            colWidthScale[colWidthScale.Length - 1] = 35;       //采样次数列

                            pdftable = new PdfPTable(colWidthScale);            //创建Table，并设置列数
                            pdftable.HorizontalAlignment = Element.ALIGN_LEFT;
                            pdftable.WidthPercentage = 100;

                            pdftable.SetTotalWidth(colWidthScale);                //必须先设置TotalWidth再设置LockedWidth=true，否则表格宽度将为0，表格创建失败
                            pdftable.LockedWidth = true;                          //设置LockedWidth=true之前必须先设置表格总宽
                            pdftable.HeaderRows = 1;                              //设置新页显示表头

                            //添加列
                            for (int i = 2; i < columnsCount + ix + 1; i++)//MyDefine.myXET.meDataTbl.dataTable.Columns.Count
                            {
                                string colName = MyDefine.myXET.meDataTmpTbl.dataTable.Columns[i].ColumnName;
                                colName = colName.Contains(".Cor") ? colName.Replace(".Cor", "") : colName;
                                PdfPCell cell = new PdfPCell(new Phrase(colName, fontTable));
                                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                cell.BorderWidth = 0;      //无边框
                                                           // cell.NoWrap = true;        //不换行
                                pdftable.AddCell(cell);
                                if (i == 2)
                                {
                                    i += columnsCount;
                                }
                            }

                            //添加采样次数列
                            PdfPCell cell1 = new PdfPCell(new Phrase("采样次数", fontTable));
                            cell1.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell1.BorderWidth = 0;      //无边框
                            pdftable.AddCell(cell1);

                            //添加行
                            for (int i = 0; i < MyDefine.myXET.meDataTmpTbl.dataTable.Rows.Count - 4; i++)
                            {
                                //标记有效数据开始
                                for (int ilist = 0; ilist < MyDefine.myXET.meValidStageNum * 2; ilist += 2)
                                {
                                    if (i == MyDefine.myXET.meValidIdxList[ilist])
                                    {
                                        string stage;
                                        stage = "P" + (ilist / 2 + 1).ToString() + MyDefine.myXET.meValidNameList[ilist / 2] + "开始";
                                        PdfPCell cell = new PdfPCell(new Phrase(stage, fontTable));
                                        cell.Colspan = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;       // 单元格占的列数(占满一行)
                                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                        cell.BorderWidth = 0;      //无边框
                                        pdftable.AddCell(cell);
                                    }
                                }

                                //按行输出测试数据
                                for (int j = 2; j < columnsCount + ix + 1; j++)//MyDefine.myXET.meDataTbl.dataTable.Columns.Count
                                {
                                    string myStr = (MyDefine.myXET.meDataTmpTbl.dataTable.Rows[i][j]).ToString();

                                    PdfPCell cell = new PdfPCell(new Phrase(myStr, fontTable));
                                    cell.HorizontalAlignment = (j == 2) ? Element.ALIGN_LEFT : Element.ALIGN_CENTER;
                                    cell.BorderWidth = 0;      //无边框
                                                               //cell.FixedHeight = Convert.ToInt32( textBox11.Text);
                                    pdftable.AddCell(cell);
                                    if (j == 2)
                                    {
                                        j += columnsCount;
                                    }
                                }

                                //添加采样次数
                                PdfPCell cell2 = new PdfPCell(new Phrase((i + 1).ToString(), fontTable));
                                cell2.HorizontalAlignment = Element.ALIGN_CENTER;
                                cell2.BorderWidth = 0;      //无边框
                                pdftable.AddCell(cell2);

                                //标记有效数据结束
                                for (int ilist = 1; ilist < MyDefine.myXET.meValidStageNum * 2 + 1; ilist += 2)
                                {
                                    if (i == MyDefine.myXET.meValidIdxList[ilist])
                                    {
                                        string stage = "P" + (ilist / 2 + 1).ToString() + MyDefine.myXET.meValidNameList[ilist / 2] + "结束";
                                        PdfPCell cell = new PdfPCell(new Phrase(stage, fontTable));
                                        cell.Colspan = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;       // 单元格占的列数(占满一行)
                                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                        cell.BorderWidth = 0;      //无边框
                                        pdftable.AddCell(cell);
                                    }
                                }
                            }

                            document.Add(pdftable);
                            document.Add(new Paragraph(blankLine, fontTiny));
                            document.Add(separator);
                            document.Add(new Paragraph(blankLine, fontTiny));

                            columnsCount += 12;
                        } while (columnsCount + 2 < colnum);
                    }

                    if (HumPdf)
                    {
                        colnum = MyDefine.myXET.meDataHumTbl.dataTable.Columns.Count - 5;  //后面要加采样次数列，所以+1
                        columnsCount = 0;
                        do
                        {
                            document.Add(new Paragraph(blankLine, fontTiny));
                            document.Add(separator);
                            document.Add(new Paragraph(blankLine, fontTiny));
                            int ix = 0;
                            if (colnum > 14)
                            {
                                if (columnsCount + 14 > colnum)
                                {
                                    ix = colnum - columnsCount;
                                }
                                else
                                {
                                    ix = 14;
                                }
                            }
                            else
                            {
                                ix = colnum;
                            }

                            float[] colWidthScale = new float[ix];          //设置列数
                            for (int i = 0; i < colWidthScale.Length; i++)
                            {
                                colWidthScale[i] = (i == 0) ? 70 : 35;          //设置列宽
                            }
                            colWidthScale[colWidthScale.Length - 1] = 35;       //采样次数列

                            pdftable = new PdfPTable(colWidthScale);            //创建Table，并设置列数
                            pdftable.HorizontalAlignment = Element.ALIGN_LEFT;
                            pdftable.WidthPercentage = 100;

                            pdftable.SetTotalWidth(colWidthScale);                //必须先设置TotalWidth再设置LockedWidth=true，否则表格宽度将为0，表格创建失败
                            pdftable.LockedWidth = true;                          //设置LockedWidth=true之前必须先设置表格总宽
                            pdftable.HeaderRows = 1;                              //设置新页显示表头

                            //添加列
                            for (int i = 2; i < columnsCount + ix + 1; i++)//MyDefine.myXET.meDataTbl.dataTable.Columns.Count
                            {
                                string colName = MyDefine.myXET.meDataHumTbl.dataTable.Columns[i].ColumnName;
                                colName = colName.Contains(".Cor") ? colName.Replace(".Cor", "") : colName;
                                PdfPCell cell = new PdfPCell(new Phrase(colName, fontTable));
                                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                cell.BorderWidth = 0;      //无边框
                                                           //cell.NoWrap = true;        //不换行
                                pdftable.AddCell(cell);
                                if (i == 2)
                                {
                                    i += columnsCount;
                                }
                            }

                            //添加采样次数列
                            PdfPCell cell1 = new PdfPCell(new Phrase("采样次数", fontTable));
                            cell1.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell1.BorderWidth = 0;      //无边框
                            pdftable.AddCell(cell1);

                            //添加行
                            for (int i = 0; i < MyDefine.myXET.meDataHumTbl.dataTable.Rows.Count - 4; i++)
                            {
                                //标记有效数据开始
                                for (int ilist = 0; ilist < MyDefine.myXET.meValidStageNum * 2; ilist += 2)
                                {
                                    if (i == MyDefine.myXET.meValidIdxList[ilist])
                                    {
                                        string stage;
                                        stage = "P" + (ilist / 2 + 1).ToString() + MyDefine.myXET.meValidNameList[ilist / 2] + "开始";
                                        PdfPCell cell = new PdfPCell(new Phrase(stage, fontTable));
                                        cell.Colspan = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;       // 单元格占的列数(占满一行)
                                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                        cell.BorderWidth = 0;      //无边框
                                        pdftable.AddCell(cell);
                                    }
                                }

                                //按行输出测试数据
                                for (int j = 2; j < columnsCount + ix + 1; j++)//MyDefine.myXET.meDataTbl.dataTable.Columns.Count
                                {
                                    string myStr = (MyDefine.myXET.meDataHumTbl.dataTable.Rows[i][j]).ToString();

                                    PdfPCell cell = new PdfPCell(new Phrase(myStr, fontTable));
                                    cell.HorizontalAlignment = (j == 2) ? Element.ALIGN_LEFT : Element.ALIGN_CENTER;
                                    cell.BorderWidth = 0;      //无边框
                                                               //cell.FixedHeight = Convert.ToInt32( textBox11.Text);
                                    pdftable.AddCell(cell);
                                    if (j == 2)
                                    {
                                        j += columnsCount;
                                    }
                                }

                                //添加采样次数
                                PdfPCell cell2 = new PdfPCell(new Phrase((i + 1).ToString(), fontTable));
                                cell2.HorizontalAlignment = Element.ALIGN_CENTER;
                                cell2.BorderWidth = 0;      //无边框
                                pdftable.AddCell(cell2);

                                //标记有效数据结束
                                for (int ilist = 1; ilist < MyDefine.myXET.meValidStageNum * 2 + 1; ilist += 2)
                                {
                                    if (i == MyDefine.myXET.meValidIdxList[ilist])
                                    {
                                        string stage = "P" + (ilist / 2 + 1).ToString() + MyDefine.myXET.meValidNameList[ilist / 2] + "结束";
                                        PdfPCell cell = new PdfPCell(new Phrase(stage, fontTable));
                                        cell.Colspan = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;       // 单元格占的列数(占满一行)
                                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                        cell.BorderWidth = 0;      //无边框
                                        pdftable.AddCell(cell);
                                    }
                                }
                            }

                            document.Add(pdftable);
                            document.Add(new Paragraph(blankLine, fontTiny));
                            document.Add(separator);
                            document.Add(new Paragraph(blankLine, fontTiny));

                            columnsCount += 12;
                        } while (columnsCount + 2 < colnum);
                    }

                    if (PrsPdf)
                    {
                        colnum = MyDefine.myXET.meDataPrsTbl.dataTable.Columns.Count - 4;  //后面要加采样次数列，所以+1
                        columnsCount = 0;
                        do
                        {
                            document.Add(new Paragraph(blankLine, fontTiny));
                            document.Add(separator);
                            document.Add(new Paragraph(blankLine, fontTiny));
                            int ix = 0;
                            if (colnum > 14)
                            {
                                if (columnsCount + 14 > colnum)
                                {
                                    ix = colnum - columnsCount;
                                }
                                else
                                {
                                    ix = 14;
                                }
                            }
                            else
                            {
                                ix = colnum;
                            }

                            float[] colWidthScale = new float[ix];          //设置列数
                            for (int i = 0; i < colWidthScale.Length; i++)
                            {
                                colWidthScale[i] = (i == 0) ? 70 : 35;          //设置列宽
                            }
                            colWidthScale[colWidthScale.Length - 1] = 35;       //采样次数列

                            pdftable = new PdfPTable(colWidthScale);            //创建Table，并设置列数
                            pdftable.HorizontalAlignment = Element.ALIGN_LEFT;
                            pdftable.WidthPercentage = 100;

                            pdftable.SetTotalWidth(colWidthScale);                //必须先设置TotalWidth再设置LockedWidth=true，否则表格宽度将为0，表格创建失败
                            pdftable.LockedWidth = true;                          //设置LockedWidth=true之前必须先设置表格总宽

                            //添加列
                            for (int i = 1; i < columnsCount + ix; i++)//MyDefine.myXET.meDataTbl.dataTable.Columns.Count
                            {
                                string colName = MyDefine.myXET.meDataPrsTbl.dataTable.Columns[i].ColumnName;
                                colName = colName.Contains(".Cor") ? colName.Replace(".Cor", "") : colName;
                                PdfPCell cell = new PdfPCell(new Phrase(colName, fontTable));
                                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                cell.BorderWidth = 0;      //无边框
                                pdftable.AddCell(cell);
                                if (i == 1)
                                {
                                    i += columnsCount;
                                }
                            }

                            //添加采样次数列
                            PdfPCell cell1 = new PdfPCell(new Phrase("采样次数", fontTable));
                            cell1.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell1.BorderWidth = 0;      //无边框
                            pdftable.AddCell(cell1);

                            //添加行
                            for (int i = 0; i < MyDefine.myXET.meDataPrsTbl.dataTable.Rows.Count - 4; i++)
                            {
                                //标记有效数据开始
                                for (int ilist = 0; ilist < MyDefine.myXET.meValidStageNum * 2; ilist += 2)
                                {
                                    if (i == MyDefine.myXET.meValidIdxList[ilist])
                                    {
                                        string stage;
                                        stage = "P" + (ilist / 2 + 1).ToString() + MyDefine.myXET.meValidNameList[ilist / 2] + "开始";
                                        PdfPCell cell = new PdfPCell(new Phrase(stage, fontTable));
                                        cell.Colspan = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;       // 单元格占的列数(占满一行)
                                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                        cell.BorderWidth = 0;      //无边框
                                        pdftable.AddCell(cell);
                                    }
                                }

                                //按行输出测试数据
                                for (int j = 1; j < columnsCount + ix; j++)//MyDefine.myXET.meDataTbl.dataTable.Columns.Count
                                {
                                    string myStr = (MyDefine.myXET.meDataPrsTbl.dataTable.Rows[i][j]).ToString();

                                    PdfPCell cell = new PdfPCell(new Phrase(myStr, fontTable));
                                    cell.HorizontalAlignment = (j == 1) ? Element.ALIGN_LEFT : Element.ALIGN_CENTER;
                                    cell.BorderWidth = 0;      //无边框
                                                               //cell.FixedHeight = Convert.ToInt32( textBox11.Text);
                                    pdftable.AddCell(cell);
                                    if (j == 1)
                                    {
                                        j += columnsCount;
                                    }
                                }

                                //添加采样次数
                                PdfPCell cell2 = new PdfPCell(new Phrase((i + 1).ToString(), fontTable));
                                cell2.HorizontalAlignment = Element.ALIGN_CENTER;
                                cell2.BorderWidth = 0;      //无边框
                                pdftable.AddCell(cell2);

                                //标记有效数据结束
                                for (int ilist = 1; ilist < MyDefine.myXET.meValidStageNum * 2 + 1; ilist += 2)
                                {
                                    if (i == MyDefine.myXET.meValidIdxList[ilist])
                                    {
                                        string stage = "P" + (ilist / 2 + 1).ToString() + MyDefine.myXET.meValidNameList[ilist / 2] + "结束";
                                        PdfPCell cell = new PdfPCell(new Phrase(stage, fontTable));
                                        cell.Colspan = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;       // 单元格占的列数(占满一行)
                                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                        cell.BorderWidth = 0;      //无边框
                                        pdftable.AddCell(cell);
                                    }
                                }
                            }

                            document.Add(pdftable);
                            document.Add(new Paragraph(blankLine, fontTiny));
                            document.Add(separator);
                            document.Add(new Paragraph(blankLine, fontTiny));

                            columnsCount += 12;
                        } while (columnsCount + 2 < colnum);
                    }
                }
                ////////////////////////////////////////////////////////////////////
                #endregion

                #region 末页签字(仅首末页签字模式)

                //首末页签字模式：在最后一页的空白处加上签字和日期
                if (signMode == 2)
                {
                    //页脚设置
                    PdfPTable footerTable = new PdfPTable(3);
                    footerTable.TotalWidth = PageSize.A4.Width - 80;            //设置表格总宽
                    footerTable.SetWidths(new int[] { 12, 10, 10 });            //设置列宽比例
                    footerTable.LockedWidth = true;                             //锁定表格宽度
                    footerTable.DefaultCell.FixedHeight = -10;                  //设置单元格高度
                    footerTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;       //设置单元格边框
                    footerTable.DefaultCell.BorderWidth = 0.5f;                 //设置单元格线宽
                    footerTable.DefaultCell.BorderColor = BaseColor.LIGHT_GRAY; //设置边框颜色

                    footerTable.AddCell(new Paragraph("  ", fontFooter));
                    footerTable.AddCell(new Paragraph("验证人：" + MyDefine.myXET.calibMem, fontFooter));
                    footerTable.AddCell(new Paragraph("复核人：" + MyDefine.myXET.reviewMem, fontFooter));

                    footerTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;      //设置单元格边框
                    footerTable.AddCell(new Paragraph("  ", fontFooter));
                    footerTable.AddCell(new Paragraph("验证日期：" + MyDefine.myXET.calibDate, fontFooter));
                    footerTable.AddCell(new Paragraph("复核日期：" + MyDefine.myXET.reviewDate, fontFooter));
                    footerTable.AddCell(new Paragraph("  ", fontFooter));
                    footerTable.AddCell(new Paragraph("  ", fontFooter));
                    footerTable.AddCell(new Paragraph("  ", fontFooter));

                    //写入页脚 -- 写到指定位置
                    footerTable.WriteSelectedRows(0, -1, 40, 60, writer.DirectContent);                           //写入页脚(位置在下面)
                }

                #endregion

                #region 关闭文件

                //关闭
                document.Close();

                //调出pdf
                Process.Start(filepath);

                #endregion

                MyDefine.myXET.AddTraceInfo("生成报告成功");
                return true;
            }
            catch (Exception ex)
            {
                MyDefine.myXET.ShowWrongMsg("生成报告失败：" + ex.ToString());
                return false;
            }
        }

        //生成报告第一部分
        private void workerCreatePdfPart1_DoWork(object sender, DoWorkEventArgs e)
        {
            string path = e.Argument as string;
            e.Result = createPDFReportVerifyPart1(path);
        }

        //生成报告第二部分
        private void workerCreatePdfPart2_DoWork(object sender, DoWorkEventArgs e)
        {
            string path = e.Argument as string;
            e.Result = createPDFReportVerifyPart2(path);
        }

        //生成第一部分完毕后更新SavePDFList，两部分生成完，更新button1.text
        private void workerCreatePdfPart1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bool result1 = (bool)e.Result;

            if (result1)
            {
                //文件名称;报告编号;报告类型;报告名称;报告日期;操作人员;审核人员;查看
                String pdfInfo = textBox11.Text + "第二部分" + ";" + MyDefine.myXET.repcode + ";" + comboBox2.Text.Split('|')[0] + ";" + textBox2.Text + ";" + textBox8.Text + ";" + textBox5.Text + ";" + textBox7.Text + ";" + "查看";
                MyDefine.myXET.SavePDFList(pdfInfo);
            }

            // 检查两个 BackgroundWorker 是否都已完成
            if (!workerCreatePdfPart1.IsBusy && !workerCreatePdfPart2.IsBusy)
            {
                // 在 UI 线程上更新 button1 的 Text 为“生成报告”
                button1.Invoke((MethodInvoker)(() =>
                {
                    button1.Text = "生成报告";
                    button1.Enabled = true;
                }));
            }
        }

        //生成第一部分完毕后更新SavePDFList，两部分生成完，更新button1.text
        private void workerCreatePdfPart2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bool result2 = (bool)e.Result;

            if (result2)
            {
                //文件名称;报告编号;报告类型;报告名称;报告日期;操作人员;审核人员;查看
                String pdfInfo = textBox11.Text + "第二部分" + ";" + MyDefine.myXET.repcode + ";" + comboBox2.Text.Split('|')[0] + ";" + textBox2.Text + ";" + textBox8.Text + ";" + textBox5.Text + ";" + textBox7.Text + ";" + "查看";
                MyDefine.myXET.SavePDFList(pdfInfo);
            }

            // 检查两个 BackgroundWorker 是否都已完成
            if (!workerCreatePdfPart1.IsBusy && !workerCreatePdfPart2.IsBusy)
            {
                // 在 UI 线程上更新 button1 的 Text 为“生成报告”
                button1.Invoke((MethodInvoker)(() =>
                {
                    button1.Text = "生成报告";
                    button1.Enabled = true;
                }));
            }
        }

        #endregion

        #region DataTable转PdfPTable

        /// <summary>
        /// 生成PdfPTable表格
        /// </summary>
        /// <param name="meDataTbl">DataTable数据表</param>
        /// <param name="colWidth">列宽比例数组</param>
        /// <param name="myfont">字体</param>
        /// <returns></returns>
        public PdfPTable getPDFPTable(DataTable meDataTbl, float[] colWidth, iTextSharp.text.Font myfont, int index = 0)
        {
            PdfPTable pdftable = new PdfPTable(colWidth);    //创建Table，并设置列数
            pdftable.HorizontalAlignment = Element.ALIGN_LEFT;
            pdftable.WidthPercentage = 100;

            //添加列
            if (index == 0)
            {
                for (int i = 0; i < meDataTbl.Columns.Count; i++)
                {
                    string colName = meDataTbl.Columns[i].ColumnName;
                    PdfPCell cell = new PdfPCell(new Phrase(colName, myfont));
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.BorderWidth = 0;      //无边框
                    pdftable.AddCell(cell);
                }
            }

            //添加行
            for (int i = 0; i < meDataTbl.Rows.Count; i++)
            {
                for (int j = 0; j < meDataTbl.Columns.Count; j++)
                {
                    string myStr = (meDataTbl.Rows[i][j]).ToString();
                    PdfPCell cell = new PdfPCell(new Phrase(myStr, myfont));
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.BorderWidth = 0;      //无边框
                    pdftable.AddCell(cell);
                }
            }

            return pdftable;
        }

        //删除原表格的空行
        public PdfPTable getPDFPTableType2(DataTable meDataTbl, float[] colWidth, iTextSharp.text.Font myfont, int index = 0)
        {
            PdfPTable pdftable = new PdfPTable(colWidth);    //创建Table，并设置列数
            pdftable.HorizontalAlignment = Element.ALIGN_LEFT;
            pdftable.WidthPercentage = 100;

            //添加列
            if (index == 0)
            {
                for (int i = 0; i < meDataTbl.Columns.Count; i++)
                {
                    string colName = meDataTbl.Columns[i].ColumnName;
                    PdfPCell cell = new PdfPCell(new Phrase(colName, myfont));
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.BorderWidth = 0;      //无边框
                    pdftable.AddCell(cell);
                }
            }

            //添加行
            for (int i = 0; i < meDataTbl.Rows.Count; i++)
            {
                for (int j = 0; j < meDataTbl.Columns.Count; j++)
                {
                    string myStr = (meDataTbl.Rows[i][j]).ToString();
                    if (myStr.Equals("") && j == 0)
                    {
                        break;
                    }
                    PdfPCell cell = new PdfPCell(new Phrase(myStr, myfont));
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.BorderWidth = 0;      //无边框
                    pdftable.AddCell(cell);
                }
            }

            return pdftable;
        }

        #endregion

        #endregion

        #region 拓展函数

        //创建段落
        private Paragraph CreateParagraph(string str, iTextSharp.text.Font font, int align)
        {
            Paragraph mp = new Paragraph(str, font);

            //Element.ALIGN_LEFT
            //Element.ALIGN_CENTER
            //Element.ALIGN_RIGHT
            mp.Alignment = align;

            //
            mp.SpacingBefore = 5.0f;
            mp.SpacingAfter = 5.0f;

            return mp;
        }

        private iTextSharp.text.Image CreateImage(string path, int imgWidth, int imgHeight)
        {
            if (File.Exists(path))
            {
                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(path);
                img.ScaleAbsolute(imgWidth, imgHeight);

                return img;
            }

            return null;
        }

        //字符串长度增加add值并控制等长,最小min
        private int GetJoinLen(List<String> str, int min, int add)
        {
            int max = min;
            int len = 0;

            for (int i = 0; i < str.Count; i++)
            {
                len = Encoding.Default.GetBytes(str[i]).Length;
                if (len > max)
                {
                    max = len;
                }
            }

            max += add;

            return max;
        }

        //差值并取正
        private int Devation(int a, int b)
        {
            if (a < b)
            {
                return (b - a);
            }
            else
            {
                return (a - b);
            }
        }


        /// <summary>
        /// 将字符串转换为固定长度，右侧补指定字母(仅适用于纯中文或纯英文对齐)
        /// </summary>
        /// <param name="myStr"></param>
        /// <param name="myLen">想要的字符串长度</param>
        /// <param name="myChar">右侧要添加的字符</param>
        /// <returns></returns>
        private string doWithExactStrLen(string myStr, int myLen, char myChar)
        {
            return myStr.PadRight(myLen, myChar); //返回长度为myLen的字符串
        }

        /// <summary>
        /// 将字符串转换为固定长度，右侧补空格(适用于中英文对齐)
        /// </summary>
        /// <param name="myStr"></param>
        /// <param name="myLen">想要的字符串长度</param>
        /// <returns></returns>
        private string doWithExactStrLen(string myStr, int myLen)
        {
            string tempStr = new string(' ', myLen - Encoding.Default.GetBytes(myStr).Length);  //需要补足的空格
            return myStr + tempStr; //返回长度为myLen的字符串，右侧补空格
        }

        /// <summary>
        /// 将字符串数组中的所有字符串转换为固定长度，右侧补空格(适用于中英文对齐)
        /// </summary>
        /// <param name="myStr"></param>
        /// <param name="strLen">想要的字符串长度</param>
        private void doWithExactLen(ref string[] myStr, int strLen)
        {
            for (int i = 0; i < myStr.Length; i++)
            {
                myStr[i] = myStr[i] + new string(' ', strLen - Encoding.Default.GetBytes(myStr[i]).Length);  //需要补足的空格
            }
        }

        /// <summary>
        /// 将list中的所有字符串转换为固定长度，右侧补空格(适用于中英文对齐)
        /// </summary>
        /// <param name="myStr"></param>
        /// <param name="strLen">想要的字符串长度</param>
        private void doWithExactLen(ref List<String> myStr, int strLen)
        {
            for (int i = 0; i < myStr.Count; i++)
            {
                //MessageBox.Show(myStr[i]);
                myStr[i] = myStr[i] + new string(' ', strLen - Encoding.Default.GetBytes(myStr[i]).Length);  //需要补足的空格
            }
        }


        /// <summary>
        /// 添加指定长度的空格
        /// </summary>
        /// <param name="strLen">想要的空格长度</param>
        private string addSpace(int strLen)
        {
            return new string(' ', strLen);
        }

        #endregion

        #region 加载图片

        public void loadPicture()
        {
            try
            {
                System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
                fileDialog.Title = "请选择数据";
                fileDialog.Filter = "图片(*.bmp)|*.bmp|图片(*.jpeg)|*.jpeg|图片(*.gif)|*.gif|所有文件(*.*)|*.*";
                fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);     //默认桌面
                fileDialog.RestoreDirectory = true;

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox9.Text = fileDialog.FileName;     //获取文件路径
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("加载图片失败：" + ex.ToString());
            }
        }

        #endregion

        #region 加载Logo图片

        public void loadLogoPicture()
        {
            try
            {
                System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
                fileDialog.Title = "请选择数据";
                fileDialog.Filter = "图片(*.png)|*.png|所有文件(*.*)|*.*";
                fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);     //默认桌面
                fileDialog.RestoreDirectory = true;

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox10.Text = fileDialog.FileName;     //获取logo路径
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg("加载图片失败：" + ex.ToString());
            }
        }

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

        #region 报告信息保存及更新(报告编号、便准确、公司电话)

        /// <summary>
        /// 更新报告编号并显示（报告编号+1）
        /// </summary>
        /// <param name="oldcode">上一次的报告编号(JDYZ20211109xxx)</param>
        /// <param name="isAdd">报告编号是否+1</param>
        /// <returns></returns>
        public string updateReportCode(Boolean isAdd = true)
        {
            string retcode = string.Empty;
            string flowcode = string.Empty;
            string oldcode = MyDefine.myXET.repcode;                 //上一次打印PDF报告的编号
            string mydate = DateTime.Now.ToString("yyyyMMdd");       //当前日期

            int rptnum = 0;                                          //默认报告编号为0(新的一天编号从0开始)
            if (oldcode.Contains(mydate)) rptnum = Convert.ToInt32(oldcode.Substring(12));  //今天已生成过报告,则读取当前报告编号
            if (isAdd) rptnum += 1;                                  //报告编号+1
            flowcode = rptnum.ToString("d3");                        //输出三位整数(如001)
            retcode = "JDYZ" + mydate + flowcode;                    //完整报告编号

            return retcode;
        }

        /// <summary>
        /// 显示报告编号
        /// </summary>
        public void showReportCode()
        {
            //textBox4.Text = MyDefine.myXET.repcode;                 //显示报告编号
            label2.Text = MyDefine.myXET.repcode;                   //显示报告编号
        }

        #endregion

        #region 文本框输入限制(禁止输入回车)

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

        #region 导出pdf选择图片分开还是和在一起
        private Boolean together = false;

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                together = false;
            }
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
            {
                together = true;
            }
        }
        #endregion

        #region 导出pdf选择是否拆分
        private Boolean togetherPDF = false;

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton5.Checked)
            {
                togetherPDF = true;
            }
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton6.Checked)
            {
                togetherPDF = false;
            }
        }
        #endregion

        #region 导出pdf是否选择部分数据打印

        private void checkDataSelect_CheckedChanged(object sender, EventArgs e)
        {
            if (checkDataSelect.Checked)
            {
                radioButton5.Checked = false;
                radioButton6.Checked = false;
                groupBox4.Enabled = false;
            }
            else
            {
                radioButton5.Checked = false;
                radioButton6.Checked = true;
                groupBox4.Enabled = true;
            }
        }

        #endregion

        #region  出pdf选择数据出温度、湿度、压力

        private int dataSelect = 0;
        private Boolean TemPdf = false; //出温度pdf
        private Boolean HumPdf = false;//出湿度pdf
        private Boolean PrsPdf = false;//出压力pdf

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                TemPdf = true;
                dataSelect++;
            }
            else
            {
                TemPdf = false;
                dataSelect--;
            }

            if (dataSelect < 2)
            {
                radioButton3.Checked = true;
                radioButton4.Enabled = false;
            }
            else
            {
                radioButton4.Enabled = true;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                HumPdf = true;
                dataSelect++;
            }
            else
            {
                HumPdf = false;
                dataSelect--;
            }

            if (dataSelect < 2)
            {
                radioButton3.Checked = true;
                radioButton4.Enabled = false;
            }
            else
            {
                radioButton4.Enabled = true;
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                PrsPdf = true;
                dataSelect++;
                if (checkBox2.Checked)
                {
                    checkBox2.Checked = false;
                }
            }
            else
            {
                PrsPdf = false;
                dataSelect--;
            }

            if (dataSelect < 2)
            {
                radioButton3.Checked = true;
                radioButton4.Enabled = false;
            }
            else
            {
                radioButton4.Enabled = true;
            }
        }

        #endregion


        #region 绘制图片
        //曲线颜色
        Color[] colors = new Color[] { Color.RoyalBlue, Color.SaddleBrown, Color.ForestGreen, Color.Brown, Color.DodgerBlue, Color.SteelBlue, Color.DarkOrange, Color.DarkGreen, Color.BlueViolet, Color.Crimson, Color.OrangeRed, Color.DarkGoldenrod, Color.LightCoral, Color.LimeGreen, Color.DarkOrange, Color.Violet, Color.DarkTurquoise, Color.SlateBlue, Color.Magenta, Color.DeepSkyBlue, Color.RoyalBlue };

        /// <summary>
        /// 绘制整体图片
        /// </summary>
        /// <param name="isLeft"></param>
        /// <returns></returns>
        public Bitmap GetBitmapPoint(Boolean isLeft)
        {
            const int Width = 510;
            const int Height = 270;
            const int Info = 20;

            DateTime myStartTime = MyDefine.myXET.meStartTime;                           //有效数据开始时间
            DateTime myStopTime = MyDefine.myXET.meStopTime;                             //有效数据结束时间


            double leftSpan;
            double leftMin;
            double rightSpan;
            double rightMin;
            leftSpan = MyDefine.myXET.leftLimit[0] - MyDefine.myXET.leftLimit[1];//左轴范围
            leftMin = MyDefine.myXET.leftLimit[1];                              //左轴最小值
            rightSpan = MyDefine.myXET.rightLimit[0] - MyDefine.myXET.rightLimit[1];//右轴范围
            rightMin = MyDefine.myXET.rightLimit[1];                                 //右轴最小值

            //层图
            Bitmap img = new Bitmap(Width, Height + Info);

            //绘制
            Graphics g = Graphics.FromImage(img);

            //填充白色
            g.FillRectangle(Brushes.White, 0, 0, Width, Height + Info);

            if (together)
            {
                //画网格线
                DrawPictureGridTwo(g, Height, Width);
                //画Y轴坐标值
                DrawPictureInfosTwo(g, Height, Width, leftSpan, leftMin, rightSpan, rightMin);
                //画X轴时间信息
                updateAxisLabelStage(g, Height + Info, Width, myStartTime, myStopTime);
                //画扭矩线
                DrawStageDataLines(g, Height, Width);
            }
            else
            {
                //画网格线
                DrawPictureGridOne(g, Height, Width);
                //画Y轴坐标值
                if (isLeft)
                {
                    DrawPictureInfosOne(g, Height, Width, leftSpan, leftMin);
                }
                else
                {
                    DrawPictureInfosOne(g, Height, Width, rightSpan, rightMin);
                }
                //画X轴时间信息
                updateAxisLabel(g, Height + Info, Width, myStartTime, myStopTime);
                //画扭矩线
                DrawStageDataLines(g, Height, Width, isLeft);
            }

            g.Dispose();

            return img;
        }

        /// <summary>
        /// 绘制阶段图片
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Bitmap GetStageBitmapPoint(Boolean isLeft, int index)
        {
            const int Width = 510;
            const int Height = 270;
            const int Info = 20;

            double leftSpan;
            double leftMin;
            double rightSpan;
            double rightMin;
            DateTime myStartTime = MyDefine.myXET.meValidTimeList[index];                                       //有效数据开始时间
            DateTime myStopTime = MyDefine.myXET.meValidTimeList[index + 1];                                    //有效数据结束时间
            leftSpan = MyDefine.myXET.meValidUpperList[index] - MyDefine.myXET.meValidLowerList[index];         //左轴范围
            leftMin = MyDefine.myXET.meValidLowerList[index];                                                   //左轴最小值
            rightSpan = MyDefine.myXET.meValidUpperList[index + 1] - MyDefine.myXET.meValidLowerList[index + 1];//右轴范围
            rightMin = MyDefine.myXET.meValidLowerList[index + 1];                                              //右轴最小值

            //层图
            Bitmap img = new Bitmap(Width, Height + Info);

            //绘制
            Graphics g = Graphics.FromImage(img);

            //填充白色
            g.FillRectangle(Brushes.White, 0, 0, Width, Height + Info);

            if (together)
            {
                //画网格线
                DrawPictureGridTwo(g, Height, Width);
                //画Y轴坐标值
                DrawPictureInfosTwo(g, Height, Width, leftSpan, leftMin, rightSpan, rightMin);
                //画X轴时间信息
                updateAxisLabelStage(g, Height + Info, Width, myStartTime, myStopTime);
                //画扭矩线
                DrawStageDataLines(g, Height, Width, index);
            }
            else
            {
                //画网格线
                DrawPictureGridOne(g, Height, Width);
                //画Y轴坐标值
                if (isLeft)//温度
                {
                    DrawPictureInfosOne(g, Height, Width, leftSpan, leftMin);
                }
                else//湿度或者压力
                {
                    DrawPictureInfosOne(g, Height, Width, rightSpan, rightMin);
                }
                //画X轴时间信息
                updateAxisLabel(g, Height + Info, Width, myStartTime, myStopTime);
                //画扭矩线
                DrawStageDataLines(g, Height, Width, index, isLeft);
            }

            g.Dispose();
            return img;
        }
        #endregion

        #region 画单轴

        #region 画背景网格线(单y轴)

        //画单轴网格线(勿删)
        private void DrawPictureGridOne(Graphics g, int height, int width)
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
                    y1 = (int)(height * i / gridnum * 1.0) - 1;
                    if (y1 < 0) y1 = 0;

                    mypen = new Pen(Color.Gainsboro, 1.00f);   //定义背景网格画笔
                    mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;      //画虚线

                    //画5条实线
                    if (i % 5 == 0)
                    {
                        mypen = new Pen(Color.DarkGray, 1.00f);   //定义背景网格画笔
                        mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;      //画实线
                    }

                    g.DrawLine(mypen, new Point(30, y1), new Point(width, y1));      //画X轴网格
                }

                //画Y轴网格线
                gridnum = 5;
                for (int i = 0; i <= gridnum; i++)
                {
                    mypen = new Pen(Color.Gainsboro, 1.00f);   //定义背景网格画笔
                    mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;      //画虚线

                    x1 = (int)((width - 30) * i / gridnum * 1.0) - 1 + 30;
                    if (x1 < 10) x1 = 10;

                    mypen = new Pen(Color.Gainsboro, 1.00f);   //定义背景网格画笔
                    mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;      //画虚线

                    if (i % gridnum == 0)
                    {
                        mypen = new Pen(Color.DarkGray, 1.00f);   //定义背景网格画笔
                        mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;      //画实线
                    }

                    g.DrawLine(mypen, new Point(x1, 0), new Point(x1, height - 1));      //画Y轴网格线
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        //画Y轴坐标值
        private void DrawPictureInfosOne(Graphics g, int height, int width, double axisSpan, double axisMin)
        {
            try
            {
                if (MyDefine.myXET.meDataTbl == null) return;

                String MeasY;

                if (axisSpan < int.MaxValue - 10)       //温度跨度非最大值：存在温度列，绘图
                {
                    //=====更新温度/压力值Y轴信息================================================================================================================
                    double unitTempHeight = height / axisSpan;     //每单位℃在pictureBox1上的高度

                    //计算坐标点所在位置的温度值：根据y1 = pictureBox1.Height - (int)((myTemp1 - meTmpMin) * unitTempHeight)反推的计算公式
                    MeasY = ((height - 0.00 * height) / unitTempHeight + axisMin).ToString("F2");        //计算pictureBox1坐标0点对应的温度值
                    g.DrawString(MeasY, new System.Drawing.Font("Arial", 8), Brushes.Gray, new PointF(0, (int)(0.00 * height - 0)));
                    //label2.Location = new System.Drawing.Point(label2.Location.X, (int)(0.00 * pictureBox1.Height - 8 + 15));

                    MeasY = ((height - 0.25 * height) / unitTempHeight + axisMin).ToString("F2");        //计算pictureBox1坐标1/4处对应的温度值
                    g.DrawString(MeasY, new System.Drawing.Font("Arial", 8), Brushes.Gray, new PointF(0, (int)(0.25 * height - 8)));
                    //label6.Location = new System.Drawing.Point(label6.Location.X, (int)(0.25 * pictureBox1.Height - 8 + 15));

                    MeasY = ((height - 0.5 * height) / unitTempHeight + axisMin).ToString("F2");        //计算pictureBox1坐标2/4处对应的温度值
                    g.DrawString(MeasY, new System.Drawing.Font("Arial", 8), Brushes.Gray, new PointF(0, (int)(0.5 * height - 8)));
                    //label4.Location = new System.Drawing.Point(label4.Location.X, (int)(0.5 * pictureBox1.Height - 8 + 15));

                    MeasY = ((height - 0.75 * height) / unitTempHeight + axisMin).ToString("F2");        //计算pictureBox1坐标3/4处对应的温度值
                    g.DrawString(MeasY, new System.Drawing.Font("Arial", 8), Brushes.Gray, new PointF(0, (int)(0.75 * height - 8)));
                    //label11.Location = new System.Drawing.Point(label11.Location.X, (int)(0.75 * pictureBox1.Height - 8 + 15));

                    MeasY = ((height - 1.00 * height) / unitTempHeight + axisMin).ToString("F2");        //计算pictureBox1坐标4/4处对应的温度值
                    g.DrawString(MeasY, new System.Drawing.Font("Arial", 8), Brushes.Gray, new PointF(0, height - 16));
                    //label3.Location = new System.Drawing.Point(label3.Location.X, (int)(pictureBox1.Height - 8 + 15));
                }

                //if (rightSpan < int.MaxValue - 10)       //压力跨度非最大值：存在压力列，绘图
                //{
                //    //=====更新湿度值Y轴信息================================================================================================================
                //    double unitPsrHeight = height / rightSpan;     //每单位℃在pictureBox1上的高度

                //    //计算坐标点所在位置的温度值：根据y1 = pictureBox1.Height - (int)((myTemp1 - meTmpMin) * unitTempHeight)反推的计算公式
                //    MeasY = ((height - 0.00 * height) / unitPsrHeight + rightMin).ToString("F2");        //计算pictureBox1坐标0点对应的温度值
                //    g.DrawString(MeasY, new System.Drawing.Font("Arial", 8), Brushes.Gray, new PointF(width - 10 - GetSpace(MeasY), (int)(0.00 * height - 0)));

                //    MeasY = ((height - 0.25 * height) / unitPsrHeight + rightMin).ToString("F2");        //计算pictureBox1坐标1/4处对应的温度值
                //    g.DrawString(MeasY, new System.Drawing.Font("Arial", 8), Brushes.Gray, new PointF(width - 10 - GetSpace(MeasY), (int)(0.25 * height - 8)));

                //    MeasY = ((height - 0.5 * height) / unitPsrHeight + rightMin).ToString("F2");        //计算pictureBox1坐标2/4处对应的温度值
                //    g.DrawString(MeasY, new System.Drawing.Font("Arial", 8), Brushes.Gray, new PointF(width - 10 - GetSpace(MeasY), (int)(0.5 * height - 8)));

                //    MeasY = ((height - 0.75 * height) / unitPsrHeight + rightMin).ToString("F2");        //计算pictureBox1坐标3/4处对应的温度值
                //    g.DrawString(MeasY, new System.Drawing.Font("Arial", 8), Brushes.Gray, new PointF(width - 10 - GetSpace(MeasY), (int)(0.75 * height - 8)));

                //    MeasY = ((height - 1.00 * height) / unitPsrHeight + rightMin).ToString("F2");        //计算pictureBox1坐标4/4处对应的温度值
                //    g.DrawString(MeasY, new System.Drawing.Font("Arial", 8), Brushes.Gray, new PointF(width - 10 - GetSpace(MeasY), height - 16));
                //}

            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        //画X轴时间信息
        private void updateAxisLabel(Graphics g, int height, int width, DateTime myStartTime, DateTime myStopTime)
        {
            try
            {
                //int myTimeSpan = Convert.ToInt32(MyDefine.myXET.homspan);               //测试间隔(秒)
                Int32 myTestSeconds = (Int32)(myStopTime.Subtract(myStartTime).TotalSeconds);     //总测试时间(秒)

                Int32 unitTimeGridX = myTestSeconds / 5;             //将测试时间分为5份
                int x1 = 0;

                g.DrawString(myStartTime.ToString("yy-MM-dd HH:mm:ss"), new System.Drawing.Font("Arial", 7), Brushes.Gray, new PointF(0, height - 20));
                for (int i = 1; i < 5; i++)
                {
                    x1 = (int)((width - 30) * i / 5 * 1.0) - 1;
                    if (x1 < 10) x1 = 10;
                    Int32 unitTimeGridNow = unitTimeGridX * i;
                    g.DrawString(myStartTime.AddSeconds(unitTimeGridNow).ToString("HH:mm:ss"), new System.Drawing.Font("Arial", 7), Brushes.Gray, new PointF(x1, height - 20));
                }
                g.DrawString(myStopTime.ToString("yy-MM-dd HH:mm:ss"), new System.Drawing.Font("Arial", 7), Brushes.Gray, new PointF(width - 78, height - 20));

            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }
        #endregion

        /// <summary>
        /// 画单轴的所有数据的曲线
        /// </summary>
        /// <param name="g"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="isLeft"></param>
        public void DrawStageDataLines(Graphics g, int height, int width, Boolean isLeft)
        {
            try
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                if (MyDefine.myXET.meDataTbl != null)                //数据表不为空
                {

                    float x = 0;
                    int y1 = 0, y2 = 0;
                    int dataNum = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;          //数据总数
                    float x_perGrid = (float)(width - 30) / (dataNum - 1);         //若要显示所有数据，每个数据占的单位格数
                    float unitDataHeight;
                    double meMin;
                    if (isLeft)
                    {
                        unitDataHeight = (float)(height / (MyDefine.myXET.leftLimit[0] - MyDefine.myXET.leftLimit[1]));      //每单位℃在pictureBox1上的高度
                        meMin = MyDefine.myXET.leftLimit[1];
                    }
                    else
                    {
                        unitDataHeight = (float)(height / (MyDefine.myXET.rightLimit[0] - MyDefine.myXET.rightLimit[1]));      //每单位℃在pictureBox1上的高度
                        meMin = MyDefine.myXET.rightLimit[1];
                    }

                    for (int i = 1; i < MyDefine.myXET.meDataTbl.dataTable.Columns.Count; i++)                     //meDataTbl.dataTable的0列是时间列，comboBox1.Items的索引0是All_Line，所以从1开始
                    {
                        if (i >= MyDefine.myXET.meTypeList.Count) continue;             //不明原因，有时候会出现超出meTypeList索引范围异常
                        if (MyDefine.myXET.leftLimit[0] == MyDefine.myXET.leftLimit[1]) break;     //未生成曲线时，unitDataRightHeight值不能正常计算

                        x = 30;
                        y1 = y2 = 0;        //一列数据一条曲线
                        String myDeviceType = MyDefine.myXET.meTypeList[i];   //产品类型
                        if ((isLeft & (myDeviceType == "TT_T" || myDeviceType == "TH_T" || myDeviceType == "TQ_T")) ||
                            (!isLeft & ((HumPdf & (myDeviceType == "TH_H" || myDeviceType == "TQ_H")) || (PrsPdf && myDeviceType == "TP_P"))))
                        {
                            for (int j = 0; j < MyDefine.myXET.meDataTbl.dataTable.Rows.Count - 1; j++, x += x_perGrid)
                            {
                                if (MyDefine.myXET.meDataTbl.dataTable.Rows[j][i].ToString() == "" || MyDefine.myXET.meDataTbl.dataTable.Rows[j + 1][i].ToString() == "") continue;//空数据
                                Double mydata1 = Convert.ToDouble(MyDefine.myXET.meDataTbl.dataTable.Rows[j][i]);
                                Double mydata2 = Convert.ToDouble(MyDefine.myXET.meDataTbl.dataTable.Rows[j + 1][i]);
                                if (x > width) x = width;

                                y1 = height - (int)((mydata1 - meMin) * unitDataHeight);
                                y2 = height - (int)((mydata2 - meMin) * unitDataHeight);
                                g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        /// <summary>
        /// 画单轴阶段曲线
        /// </summary>
        /// <param name="g"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="index"></param>
        public void DrawStageDataLines(Graphics g, int height, int width, int index, Boolean isLeft)
        {
            try
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                if (MyDefine.myXET.meDataTbl != null)                //数据表不为空
                {
                    float x = 30;
                    int y1 = 0, y2 = 0;
                    int dataNum = MyDefine.myXET.meValidIdxList[index + 1] - MyDefine.myXET.meValidIdxList[index] + 1;          //数据总数
                    float x_perGrid = (float)(width - 30) / (dataNum - 1);         //若要显示所有数据，每个数据占的单位格数
                    float unitDataHeight;
                    double meMin;
                    if (isLeft)
                    {
                        unitDataHeight = (float)(height / (MyDefine.myXET.meValidUpperList[index] - MyDefine.myXET.meValidLowerList[index]));      //每单位℃在pictureBox1上的高度
                        meMin = MyDefine.myXET.meValidLowerList[index];
                    }
                    else
                    {
                        unitDataHeight = (float)(height / (MyDefine.myXET.meValidUpperList[index + 1] - MyDefine.myXET.meValidLowerList[index + 1]));      //每单位℃在pictureBox1上的高度
                        meMin = MyDefine.myXET.meValidLowerList[index + 1];
                    }

                    for (int i = 1; i < MyDefine.myXET.meDataTbl.dataTable.Columns.Count; i++)                     //meDataTbl.dataTable的0列是时间列，comboBox1.Items的索引0是All_Line，所以从1开始
                    {
                        if (i >= MyDefine.myXET.meTypeList.Count) continue;             //不明原因，有时候会出现超出meTypeList索引范围异常

                        x = 30;
                        y1 = y2 = 0;        //一列数据一条曲线
                        String myDeviceType = MyDefine.myXET.meTypeList[i];   //产品类型

                        if ((isLeft & (myDeviceType == "TT_T" || myDeviceType == "TH_T" || myDeviceType == "TQ_T")) ||
                           (!isLeft & ((HumPdf & (myDeviceType == "TH_H" || myDeviceType == "TQ_H")) || (PrsPdf && myDeviceType == "TP_P"))))
                        {
                            for (int j = MyDefine.myXET.meValidIdxList[index]; j <= MyDefine.myXET.meValidIdxList[index + 1] - 1; j++, x = x + x_perGrid)
                            {
                                if (MyDefine.myXET.meDataTbl.dataTable.Rows[j][i].ToString() == "" || MyDefine.myXET.meDataTbl.dataTable.Rows[j + 1][i].ToString() == "") continue;//空数据
                                Double mydata1 = Convert.ToDouble(MyDefine.myXET.meDataTbl.dataTable.Rows[j][i]);
                                Double mydata2 = Convert.ToDouble(MyDefine.myXET.meDataTbl.dataTable.Rows[j + 1][i]);
                                if (x > width) x = width;

                                y1 = height - (int)((mydata1 - meMin) * unitDataHeight);
                                y2 = height - (int)((mydata2 - meMin) * unitDataHeight);
                                g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                            }
                        }
                    }
                }
                g.Dispose();
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        #endregion

        #region 画双轴

        #region 画背景网格线(双轴)
        //画双轴网格线(勿删)
        private void DrawPictureGridTwo(Graphics g, int height, int width)
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
                    y1 = (int)(height * i / gridnum * 1.0) - 1;
                    if (y1 < 0) y1 = 0;

                    mypen = new Pen(Color.Gainsboro, 1.00f);   //定义背景网格画笔
                    mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;      //画虚线

                    //画5条实线
                    if (i % 5 == 0)
                    {
                        mypen = new Pen(Color.DarkGray, 1.00f);   //定义背景网格画笔
                        mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;      //画实线
                    }

                    g.DrawLine(mypen, new Point(30, y1), new Point(width - 30, y1));      //画X轴网格
                }

                //画Y轴网格线
                gridnum = 5;
                for (int i = 0; i <= gridnum; i++)
                {
                    mypen = new Pen(Color.Gainsboro, 1.00f);   //定义背景网格画笔
                    mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;      //画虚线

                    x1 = (int)((width - 60) * i / gridnum * 1.0) - 1 + 30;
                    if (x1 < 10) x1 = 10;

                    mypen = new Pen(Color.Gainsboro, 1.00f);   //定义背景网格画笔
                    mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;      //画虚线

                    if (i % gridnum == 0)
                    {
                        mypen = new Pen(Color.DarkGray, 1.00f);   //定义背景网格画笔
                        mypen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;      //画实线
                    }

                    g.DrawLine(mypen, new Point(x1, 0), new Point(x1, height - 1));      //画Y轴网格线
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        //画Y轴坐标值
        private void DrawPictureInfosTwo(Graphics g, int height, int width, double leftSpan, double leftMin, double rightSpan, double rightMin)
        {
            try
            {
                if (MyDefine.myXET.meDataTbl == null) return;

                String MeasY;
                if (MyDefine.myXET.drawTemCurve)
                {
                    if (leftSpan < int.MaxValue - 10)       //温度跨度非最大值：存在温度列，绘图
                    {
                        //=====更新温度/压力值Y轴信息================================================================================================================
                        double unitTempHeight = height / leftSpan;     //每单位℃在pictureBox1上的高度

                        //计算坐标点所在位置的温度值：根据y1 = pictureBox1.Height - (int)((myTemp1 - meTmpMin) * unitTempHeight)反推的计算公式
                        MeasY = ((height - 0.00 * height) / unitTempHeight + leftMin).ToString("F2");        //计算pictureBox1坐标0点对应的温度值
                        g.DrawString(MeasY, new System.Drawing.Font("Arial", 8), Brushes.Gray, new PointF(0, (int)(0.00 * height - 0)));
                        //label2.Location = new System.Drawing.Point(label2.Location.X, (int)(0.00 * pictureBox1.Height - 8 + 15));

                        MeasY = ((height - 0.25 * height) / unitTempHeight + leftMin).ToString("F2");        //计算pictureBox1坐标1/4处对应的温度值
                        g.DrawString(MeasY, new System.Drawing.Font("Arial", 8), Brushes.Gray, new PointF(0, (int)(0.25 * height - 8)));
                        //label6.Location = new System.Drawing.Point(label6.Location.X, (int)(0.25 * pictureBox1.Height - 8 + 15));

                        MeasY = ((height - 0.5 * height) / unitTempHeight + leftMin).ToString("F2");        //计算pictureBox1坐标2/4处对应的温度值
                        g.DrawString(MeasY, new System.Drawing.Font("Arial", 8), Brushes.Gray, new PointF(0, (int)(0.5 * height - 8)));
                        //label4.Location = new System.Drawing.Point(label4.Location.X, (int)(0.5 * pictureBox1.Height - 8 + 15));

                        MeasY = ((height - 0.75 * height) / unitTempHeight + leftMin).ToString("F2");        //计算pictureBox1坐标3/4处对应的温度值
                        g.DrawString(MeasY, new System.Drawing.Font("Arial", 8), Brushes.Gray, new PointF(0, (int)(0.75 * height - 8)));
                        //label11.Location = new System.Drawing.Point(label11.Location.X, (int)(0.75 * pictureBox1.Height - 8 + 15));

                        MeasY = ((height - 1.00 * height) / unitTempHeight + leftMin).ToString("F2");        //计算pictureBox1坐标4/4处对应的温度值
                        g.DrawString(MeasY, new System.Drawing.Font("Arial", 8), Brushes.Gray, new PointF(0, height - 16));
                        //label3.Location = new System.Drawing.Point(label3.Location.X, (int)(pictureBox1.Height - 8 + 15));
                    }
                }
                if (MyDefine.myXET.drawHumCurve || MyDefine.myXET.drawPrsCurve)
                {
                    if (rightSpan < int.MaxValue - 10)       //压力跨度非最大值：存在压力列，绘图
                    {
                        //=====更新湿度值Y轴信息================================================================================================================
                        double unitPsrHeight = height / rightSpan;     //每单位℃在pictureBox1上的高度

                        //计算坐标点所在位置的温度值：根据y1 = pictureBox1.Height - (int)((myTemp1 - meTmpMin) * unitTempHeight)反推的计算公式
                        MeasY = ((height - 0.00 * height) / unitPsrHeight + rightMin).ToString("F2");        //计算pictureBox1坐标0点对应的温度值
                        g.DrawString(MeasY, new System.Drawing.Font("Arial", 8), Brushes.Gray, new PointF(width - 30, (int)(0.00 * height - 0)));

                        MeasY = ((height - 0.25 * height) / unitPsrHeight + rightMin).ToString("F2");        //计算pictureBox1坐标1/4处对应的温度值
                        g.DrawString(MeasY, new System.Drawing.Font("Arial", 8), Brushes.Gray, new PointF(width - 30, (int)(0.25 * height - 8)));

                        MeasY = ((height - 0.5 * height) / unitPsrHeight + rightMin).ToString("F2");        //计算pictureBox1坐标2/4处对应的温度值
                        g.DrawString(MeasY, new System.Drawing.Font("Arial", 8), Brushes.Gray, new PointF(width - 30, (int)(0.5 * height - 8)));

                        MeasY = ((height - 0.75 * height) / unitPsrHeight + rightMin).ToString("F2");        //计算pictureBox1坐标3/4处对应的温度值
                        g.DrawString(MeasY, new System.Drawing.Font("Arial", 8), Brushes.Gray, new PointF(width - 30, (int)(0.75 * height - 8)));

                        MeasY = ((height - 1.00 * height) / unitPsrHeight + rightMin).ToString("F2");        //计算pictureBox1坐标4/4处对应的温度值
                        g.DrawString(MeasY, new System.Drawing.Font("Arial", 8), Brushes.Gray, new PointF(width - 30, height - 16));
                    }
                }

            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        //画X轴时间信息
        private void updateAxisLabelStage(Graphics g, int height, int width, DateTime myStartTime, DateTime myStopTime)
        {
            try
            {
                //int myTimeSpan = Convert.ToInt32(MyDefine.myXET.homspan);               //测试间隔(秒)
                Int32 myTestSeconds = (Int32)(myStopTime.Subtract(myStartTime).TotalSeconds);     //总测试时间(秒)

                Int32 unitTimeGridX = myTestSeconds / 5;             //将测试时间分为5份
                int x1 = 0;

                g.DrawString(myStartTime.ToString("yy-MM-dd HH:mm:ss"), new System.Drawing.Font("Arial", 7), Brushes.Gray, new PointF(0, height - 20));
                for (int i = 1; i < 5; i++)
                {
                    x1 = (int)((width - 30) * i / 5 * 1.0) - 1;
                    if (x1 < 10) x1 = 10;
                    Int32 unitTimeGridNow = unitTimeGridX * i;
                    g.DrawString(myStartTime.AddSeconds(unitTimeGridNow).ToString("HH:mm:ss"), new System.Drawing.Font("Arial", 7), Brushes.Gray, new PointF(x1, height - 20));
                }
                g.DrawString(myStopTime.ToString("yy-MM-dd HH:mm:ss"), new System.Drawing.Font("Arial", 7), Brushes.Gray, new PointF(width - 78, height - 20));
                //label18.Text = myStopTime.ToString("yy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }
        #endregion

        /// <summary>
        /// 画双轴的所有数据的曲线
        /// </summary>
        /// <param name="g"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        public void DrawStageDataLines(Graphics g, int height, int width)
        {
            try
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                if (MyDefine.myXET.meDataTbl != null)                //数据表不为空
                {

                    float x = 0;
                    int y1 = 0, y2 = 0;
                    int dataNum = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;          //数据总数
                    float x_perGrid = (float)(width - 60) / (dataNum - 1);         //若要显示所有数据，每个数据占的单位格数
                    float unitDataLeftHeight = (float)(height / (MyDefine.myXET.leftLimit[0] - MyDefine.myXET.leftLimit[1]));      //每单位℃在pictureBox1上的高度
                    double meLeftMin = MyDefine.myXET.leftLimit[1];
                    float unitDataRightHeight = (float)(height / (MyDefine.myXET.rightLimit[0] - MyDefine.myXET.rightLimit[1]));      //每单位℃在pictureBox1上的高度
                    double meRightMin = MyDefine.myXET.rightLimit[1];

                    for (int i = 1; i < MyDefine.myXET.meDataTbl.dataTable.Columns.Count; i++)                     //meDataTbl.dataTable的0列是时间列，comboBox1.Items的索引0是All_Line，所以从1开始
                    {
                        if (i >= MyDefine.myXET.meTypeList.Count) continue;             //不明原因，有时候会出现超出meTypeList索引范围异常
                        if (MyDefine.myXET.leftLimit[0] == MyDefine.myXET.leftLimit[1]) break;     //未生成曲线时，unitDataRightHeight值不能正常计算

                        x = 30;
                        y1 = y2 = 0;        //一列数据一条曲线
                        String myDeviceType = MyDefine.myXET.meTypeList[i];   //产品类型

                        for (int j = 0; j < MyDefine.myXET.meDataTbl.dataTable.Rows.Count - 1; j++, x += x_perGrid)
                        {
                            if (MyDefine.myXET.meDataTbl.dataTable.Rows[j][i].ToString() == "" || MyDefine.myXET.meDataTbl.dataTable.Rows[j + 1][i].ToString() == "") continue;//空数据
                            Double mydata1 = Convert.ToDouble(MyDefine.myXET.meDataTbl.dataTable.Rows[j][i]);
                            Double mydata2 = Convert.ToDouble(MyDefine.myXET.meDataTbl.dataTable.Rows[j + 1][i]);
                            if (x > width) x = width;

                            switch (myDeviceType)
                            {
                                case "TT_T":    //温度采集器
                                case "TH_T":    //温湿度采集器
                                case "TQ_T":    //温湿度采集器
                                    if (TemPdf)
                                    {
                                        y1 = height - (int)((mydata1 - meLeftMin) * unitDataLeftHeight);
                                        y2 = height - (int)((mydata2 - meLeftMin) * unitDataLeftHeight);
                                        g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    }
                                    else
                                    {
                                        j = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;
                                    }
                                    break;

                                case "TH_H":    //温湿度采集器
                                case "TQ_H":    //温湿度采集器
                                    if (HumPdf)//存在压力数据时，不画湿度值
                                    {
                                        y1 = height - (int)((mydata1 - meRightMin) * unitDataRightHeight);
                                        y2 = height - (int)((mydata2 - meRightMin) * unitDataRightHeight);
                                        g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    }
                                    else
                                    {
                                        j = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;
                                    }
                                    break;

                                case "TP_P":    //压力采集器
                                    if (PrsPdf)
                                    {
                                        y1 = height - (int)((mydata1 - meRightMin) * unitDataRightHeight);
                                        y2 = height - (int)((mydata2 - meRightMin) * unitDataRightHeight);
                                        g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    }
                                    else
                                    {
                                        j = MyDefine.myXET.meDataTbl.dataTable.Rows.Count;
                                    }
                                    break;

                                default:
                                    break;
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //捕获异常
                MyDefine.myXET.ShowWrongMsg(ex.ToString());
            }
        }

        /// <summary>
        /// 画双轴阶段曲线
        /// </summary>
        /// <param name="g"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="index"></param>
        public void DrawStageDataLines(Graphics g, int height, int width, int index)
        {
            try
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                if (MyDefine.myXET.meDataTbl != null)                //数据表不为空
                {
                    float x = 30;
                    int y1 = 0, y2 = 0;
                    int dataNum = MyDefine.myXET.meValidIdxList[index + 1] - MyDefine.myXET.meValidIdxList[index] + 1;          //数据总数
                    float x_perGrid = (float)(width - 60) / (dataNum - 1);         //若要显示所有数据，每个数据占的单位格数
                    float unitDataLeftHeight = (float)(height / (MyDefine.myXET.meValidUpperList[index] - MyDefine.myXET.meValidLowerList[index]));      //每单位℃在pictureBox1上的高度
                    double meLeftMin = MyDefine.myXET.meValidLowerList[index];
                    float unitDataRightHeight = (float)(height / (MyDefine.myXET.meValidUpperList[index + 1] - MyDefine.myXET.meValidLowerList[index + 1]));      //每单位℃在pictureBox1上的高度
                    double meRightMin = MyDefine.myXET.meValidLowerList[index + 1];

                    for (int i = 1; i < MyDefine.myXET.meDataTbl.dataTable.Columns.Count; i++)                     //meDataTbl.dataTable的0列是时间列，comboBox1.Items的索引0是All_Line，所以从1开始
                    {
                        if (i >= MyDefine.myXET.meTypeList.Count) continue;             //不明原因，有时候会出现超出meTypeList索引范围异常

                        x = 30;
                        y1 = y2 = 0;        //一列数据一条曲线
                        String myDeviceType = MyDefine.myXET.meTypeList[i];   //产品类型

                        for (int j = MyDefine.myXET.meValidIdxList[index]; j < MyDefine.myXET.meValidIdxList[index + 1]; j++, x = x + x_perGrid)
                        {
                            if (MyDefine.myXET.meDataTbl.dataTable.Rows[j][i].ToString() == "" || MyDefine.myXET.meDataTbl.dataTable.Rows[j + 1][i].ToString() == "") continue;//空数据
                            Double mydata1 = Convert.ToDouble(MyDefine.myXET.meDataTbl.dataTable.Rows[j][i]);
                            Double mydata2 = Convert.ToDouble(MyDefine.myXET.meDataTbl.dataTable.Rows[j + 1][i]);
                            if (x > width) x = width;

                            switch (myDeviceType)
                            {
                                case "TT_T":    //温度采集器
                                case "TH_T":    //温湿度采集器
                                case "TQ_T":    //温湿度采集器
                                    if (TemPdf)
                                    {
                                        y1 = height - (int)((mydata1 - meLeftMin) * unitDataLeftHeight);
                                        y2 = height - (int)((mydata2 - meLeftMin) * unitDataLeftHeight);
                                        g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    }
                                    else
                                    {
                                        j = MyDefine.myXET.meValidIdxList[index + 1];
                                    }
                                    break;

                                case "TH_H":    //温湿度采集器
                                case "TQ_H":    //温湿度采集器
                                    if (HumPdf)//存在压力数据时，不画湿度值
                                    {
                                        y1 = height - (int)((mydata1 - meRightMin) * unitDataRightHeight);
                                        y2 = height - (int)((mydata2 - meRightMin) * unitDataRightHeight);
                                        g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    }
                                    else
                                    {
                                        j = MyDefine.myXET.meValidIdxList[index + 1];
                                    }
                                    break;

                                case "TP_P":    //压力采集器
                                    if (PrsPdf)
                                    {
                                        y1 = height - (int)((mydata1 - meRightMin) * unitDataRightHeight);
                                        y2 = height - (int)((mydata2 - meRightMin) * unitDataRightHeight);
                                        g.DrawLine(new Pen(colors[i % 20], 2.00f), new Point((int)x, y1), new Point((int)(x + x_perGrid), y2));
                                    }
                                    else
                                    {
                                        j = MyDefine.myXET.meValidIdxList[index + 1];
                                    }
                                    break;

                                default:
                                    break;
                            }
                        }
                    }
                }
                g.Dispose();
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
