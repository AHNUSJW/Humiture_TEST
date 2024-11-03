using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.IO.Ports;

namespace HTR
{
    public partial class MenuUvaLogForm : Form
    {       
        private Boolean isHold = true; //是否暂停读取机库日志
        private Int32 index;
        public MenuUvaLogForm()
        {
            InitializeComponent();
        }
/*
        private void updateForm()
        {
            //日志分析
            while (MyDefine.myXET.getWhere())
            {
                //
                if (MyDefine.myXET.getLog(MyDefine.myXET.GXS.REC.pxAddr))
                {
                    //增加新行
                    index = dataGridView1.Rows.Add();
                    dataGridView1.Rows[index].Cells[0].Value = index + 1;
                    dataGridView1.Rows[index].Cells[1].Value = MyDefine.myXET.GXS.REC.stamp;
                    dataGridView1.Rows[index].Cells[2].Value = MyDefine.myXET.GXS.REC.cmd;
                    dataGridView1.Rows[index].Cells[3].Value = MyDefine.myXET.GXS.REC.dat;
                    dataGridView1.Rows[index].Cells[4].Value = MyDefine.myXET.GXS.REC.trace;
                }

                if (MyDefine.myXET.GXS.REC.pxAddr == 0)
                {
                    MyDefine.myXET.GXS.REC.pxAddr = MyDefine.myXET.GXS.REC.memax;
                }
                else
                {
                    MyDefine.myXET.GXS.REC.pxAddr--;
                }
            }
        }*/

        //保存日志
        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Title = "请选择文件";
            fileDialog.Filter = "文本文件(*.log)|*.log"; //设置要选择的文件的类型
            fileDialog.RestoreDirectory = true;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(fileDialog.FileName))
                {
                    File.Delete(fileDialog.FileName);
                }
                FileStream meFS = new FileStream(fileDialog.FileName, FileMode.OpenOrCreate, FileAccess.Write);
                TextWriter meWrite = new StreamWriter(meFS);

                //第一行标题栏
                String myStr = dataGridView1.Columns[0].HeaderText + "\t" +
                                dataGridView1.Columns[1].HeaderText + "\t\t" +
                                dataGridView1.Columns[2].HeaderText + "\t" +
                                dataGridView1.Columns[3].HeaderText + "\t" +
                                dataGridView1.Columns[4].HeaderText;
                meWrite.WriteLine(myStr);

                //
                for (Int32 i = 0; i <= index; i++)
                {
                    myStr = dataGridView1.Rows[i].Cells[0].Value.ToString() + "\t" +
                            dataGridView1.Rows[i].Cells[1].Value + "\t" +
                            dataGridView1.Rows[i].Cells[2].Value + "\t" +
                            dataGridView1.Rows[i].Cells[3].Value + "\t" +
                            dataGridView1.Rows[i].Cells[4].Value;
                    meWrite.WriteLine(myStr);
                }

                meWrite.Close();
                meFS.Close();
            }
        }

        //

        private void MenuUvaLogForm_Load(object sender, EventArgs e)
        {
            //标题栏
            dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridView1.Font = new Font("宋体", 11);
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());
            dataGridView1.Columns[0].HeaderText = "序号";
            dataGridView1.Columns[1].HeaderText = "时间戳";
            dataGridView1.Columns[2].HeaderText = "级别";
            dataGridView1.Columns[3].HeaderText = "操作内容";
            dataGridView1.Columns[4].HeaderText = "客户端";
            dataGridView1.Rows.Add("1");
            dataGridView1.Rows.Add("2");
        }
    }
}