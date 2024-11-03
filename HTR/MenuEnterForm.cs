using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace HTR
{
    public partial class MenuEnterForm : Form
    {
        public MenuEnterForm()
        {
            InitializeComponent();

            #region "实时显示系统时间"

            new Thread(() =>        //实时显示时间
            {
                while (true)
                {
                    try
                    {
                        label1.BeginInvoke(new MethodInvoker(() =>
                        label1.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    }
                    catch { }
                    Thread.Sleep(1000);
                }
            })
            { IsBackground = true }.Start();

            #endregion 

        }

        private void btnEnter_Click(object sender, EventArgs e)
        {
            MyDefine.myXET.AddTraceInfo("进入");
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void MenuEnterForm_Load(object sender, EventArgs e)
        {
            MyDefine.myXET.meInterface = "进入界面";
            label8.Text = "软件版本：" + Constants.SW_Version;       //显示软件版本
        }

        //退出程序
        private void label2_Click(object sender, EventArgs e)
        {
            MyDefine.myXET.AddTraceInfo("退出");
            MyDefine.myXET.SaveTraceRecords();
            System.Environment.Exit(0);     //强制退出所有线程程序
        }
    }
}
