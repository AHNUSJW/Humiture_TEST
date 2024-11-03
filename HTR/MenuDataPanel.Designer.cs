
namespace HTR
{
    partial class MenuDataPanel
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addStageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripTb_setName = new System.Windows.Forms.ToolStripTextBox();
            this.设为起点ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.设为终点ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ClearStageToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("宋体", 7F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridView1.Location = new System.Drawing.Point(41, 45);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.RowsDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(792, 331);
            this.dataGridView1.TabIndex = 2;
            this.dataGridView1.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_CellMouseClick);
            this.dataGridView1.ColumnAdded += new System.Windows.Forms.DataGridViewColumnEventHandler(this.dataGridView1_ColumnAdded);
            // 
            // button2
            // 
            this.button2.BackgroundImage = global::HTR.Properties.Resources.按钮_o3;
            this.button2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button2.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.button2.Location = new System.Drawing.Point(309, 391);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(116, 41);
            this.button2.TabIndex = 3;
            this.button2.Text = "生成曲线";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.BackgroundImage = global::HTR.Properties.Resources.按钮_o3;
            this.button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button1.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.button1.Location = new System.Drawing.Point(41, 391);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(116, 41);
            this.button1.TabIndex = 3;
            this.button1.Text = "数据载入";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "全部",
            "温度",
            "湿度",
            "压力"});
            this.comboBox1.Location = new System.Drawing.Point(138, 19);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 20);
            this.comboBox1.TabIndex = 4;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(39, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "选择数据类型：";
            // 
            // button3
            // 
            this.button3.BackgroundImage = global::HTR.Properties.Resources.按钮_o3;
            this.button3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button3.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.button3.Location = new System.Drawing.Point(175, 391);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(116, 41);
            this.button3.TabIndex = 3;
            this.button3.Text = "数据导出";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addStageToolStripMenuItem,
            this.ToolStripTb_setName,
            this.设为起点ToolStripMenuItem,
            this.设为终点ToolStripMenuItem,
            this.ClearStageToolStripMenuItem1});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(161, 117);
            // 
            // addStageToolStripMenuItem
            // 
            this.addStageToolStripMenuItem.Name = "addStageToolStripMenuItem";
            this.addStageToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.addStageToolStripMenuItem.Tag = "0";
            this.addStageToolStripMenuItem.Text = "添加阶段";
            this.addStageToolStripMenuItem.Visible = false;
            this.addStageToolStripMenuItem.Click += new System.EventHandler(this.添加阶段ToolStripMenuItem_Click);
            // 
            // ToolStripTb_setName
            // 
            this.ToolStripTb_setName.ForeColor = System.Drawing.Color.LightGray;
            this.ToolStripTb_setName.Name = "ToolStripTb_setName";
            this.ToolStripTb_setName.Size = new System.Drawing.Size(100, 23);
            this.ToolStripTb_setName.Text = "阶段名称";
            this.ToolStripTb_setName.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tb_setName_KeyUp);
            this.ToolStripTb_setName.Click += new System.EventHandler(this.tb_setName_Click);
            // 
            // 设为起点ToolStripMenuItem
            // 
            this.设为起点ToolStripMenuItem.Name = "设为起点ToolStripMenuItem";
            this.设为起点ToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.设为起点ToolStripMenuItem.Tag = "2";
            this.设为起点ToolStripMenuItem.Text = "设为起点";
            this.设为起点ToolStripMenuItem.Click += new System.EventHandler(this.设为起点ToolStripMenuItem_Click);
            // 
            // 设为终点ToolStripMenuItem
            // 
            this.设为终点ToolStripMenuItem.Name = "设为终点ToolStripMenuItem";
            this.设为终点ToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.设为终点ToolStripMenuItem.Tag = "3";
            this.设为终点ToolStripMenuItem.Text = "设为终点";
            this.设为终点ToolStripMenuItem.Click += new System.EventHandler(this.设为终点ToolStripMenuItem_Click);
            // 
            // ClearStageToolStripMenuItem1
            // 
            this.ClearStageToolStripMenuItem1.Name = "ClearStageToolStripMenuItem1";
            this.ClearStageToolStripMenuItem1.Size = new System.Drawing.Size(160, 22);
            this.ClearStageToolStripMenuItem1.Tag = "5";
            this.ClearStageToolStripMenuItem1.Text = "删除阶段";
            this.ClearStageToolStripMenuItem1.Click += new System.EventHandler(this.删除阶段ToolStripMenuItem_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(307, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(131, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "数据加载中，请稍后...";
            this.label2.Visible = false;
            // 
            // MenuDataPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.label1);
            this.Name = "MenuDataPanel";
            this.Size = new System.Drawing.Size(889, 456);
            this.Load += new System.EventHandler(this.MenuDataPanel_Load);
            this.ParentChanged += new System.EventHandler(this.MenuDataPanel_ParentChanged);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.contextMenuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem addStageToolStripMenuItem;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripMenuItem 设为起点ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 设为终点ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ClearStageToolStripMenuItem1;
        private System.Windows.Forms.ToolStripTextBox ToolStripTb_setName;
    }
}
