namespace TextMapper
{
    partial class Form1
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

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textJP = new System.Windows.Forms.TextBox();
            this.textEN = new System.Windows.Forms.TextBox();
            this.textTranslation = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.TextList = new System.Windows.Forms.ListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.InsertClipboard = new System.Windows.Forms.ToolStripMenuItem();
            this.InsertText = new System.Windows.Forms.ToolStripMenuItem();
            this.RemoveText = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(37, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "JP";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(37, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "EN";
            // 
            // textJP
            // 
            this.textJP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textJP.Location = new System.Drawing.Point(60, 27);
            this.textJP.Name = "textJP";
            this.textJP.ReadOnly = true;
            this.textJP.Size = new System.Drawing.Size(310, 21);
            this.textJP.TabIndex = 2;
            // 
            // textEN
            // 
            this.textEN.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textEN.Location = new System.Drawing.Point(60, 58);
            this.textEN.Name = "textEN";
            this.textEN.ReadOnly = true;
            this.textEN.Size = new System.Drawing.Size(310, 21);
            this.textEN.TabIndex = 3;
            // 
            // textTranslation
            // 
            this.textTranslation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textTranslation.Location = new System.Drawing.Point(114, 85);
            this.textTranslation.Name = "textTranslation";
            this.textTranslation.Size = new System.Drawing.Size(256, 21);
            this.textTranslation.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(37, 88);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "Translation";
            // 
            // TextList
            // 
            this.TextList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextList.Font = new System.Drawing.Font("黑体", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TextList.FormattingEnabled = true;
            this.TextList.ItemHeight = 20;
            this.TextList.Location = new System.Drawing.Point(39, 112);
            this.TextList.Name = "TextList";
            this.TextList.ScrollAlwaysVisible = true;
            this.TextList.Size = new System.Drawing.Size(331, 424);
            this.TextList.TabIndex = 6;
            this.TextList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TextList_MouseDown);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Location = new System.Drawing.Point(39, 543);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(113, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "打开";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(253, 543);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(117, 23);
            this.button2.TabIndex = 8;
            this.button2.Text = "保存";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.InsertClipboard,
            this.InsertText,
            this.RemoveText});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(161, 70);
            // 
            // InsertClipboard
            // 
            this.InsertClipboard.Name = "InsertClipboard";
            this.InsertClipboard.Size = new System.Drawing.Size(160, 22);
            this.InsertClipboard.Text = "插入剪贴板内容";
            this.InsertClipboard.Click += new System.EventHandler(this.InsertClipboard_Click);
            // 
            // InsertText
            // 
            this.InsertText.Name = "InsertText";
            this.InsertText.Size = new System.Drawing.Size(160, 22);
            this.InsertText.Text = "插入文本";
            this.InsertText.Click += new System.EventHandler(this.InsertText_Click);
            // 
            // RemoveText
            // 
            this.RemoveText.Name = "RemoveText";
            this.RemoveText.Size = new System.Drawing.Size(160, 22);
            this.RemoveText.Text = "移除文本";
            this.RemoveText.Click += new System.EventHandler(this.RemoveText_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(418, 598);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.TextList);
            this.Controls.Add(this.textTranslation);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textEN);
            this.Controls.Add(this.textJP);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textJP;
        private System.Windows.Forms.TextBox textEN;
        private System.Windows.Forms.TextBox textTranslation;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox TextList;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem InsertClipboard;
        private System.Windows.Forms.ToolStripMenuItem InsertText;
        private System.Windows.Forms.ToolStripMenuItem RemoveText;
    }
}

