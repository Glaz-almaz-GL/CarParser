namespace EverythingParser
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.btStart = new System.Windows.Forms.Button();
            this.chbGoToUrl = new System.Windows.Forms.CheckBox();
            this.chbListBox = new System.Windows.Forms.CheckedListBox();
            this.lbLoading = new System.Windows.Forms.Label();
            this.lbPageLink = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.txtStartPage = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(1013, 462);
            this.treeView1.TabIndex = 0;
            // 
            // btStart
            // 
            this.btStart.Location = new System.Drawing.Point(12, 394);
            this.btStart.Name = "btStart";
            this.btStart.Size = new System.Drawing.Size(127, 56);
            this.btStart.TabIndex = 11;
            this.btStart.Text = "Start";
            this.btStart.UseVisualStyleBackColor = true;
            this.btStart.Click += new System.EventHandler(this.btStart_Click);
            // 
            // chbGoToUrl
            // 
            this.chbGoToUrl.AutoSize = true;
            this.chbGoToUrl.Location = new System.Drawing.Point(12, 12);
            this.chbGoToUrl.Name = "chbGoToUrl";
            this.chbGoToUrl.Size = new System.Drawing.Size(171, 17);
            this.chbGoToUrl.TabIndex = 6;
            this.chbGoToUrl.Text = "Переходить ли по ссылкам?";
            this.chbGoToUrl.UseVisualStyleBackColor = true;
            this.chbGoToUrl.CheckedChanged += new System.EventHandler(this.chbGoToUrl_CheckedChanged);
            // 
            // chbListBox
            // 
            this.chbListBox.FormattingEnabled = true;
            this.chbListBox.Location = new System.Drawing.Point(145, 146);
            this.chbListBox.Name = "chbListBox";
            this.chbListBox.Size = new System.Drawing.Size(550, 304);
            this.chbListBox.TabIndex = 12;
            // 
            // lbLoading
            // 
            this.lbLoading.AutoSize = true;
            this.lbLoading.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lbLoading.Location = new System.Drawing.Point(379, 259);
            this.lbLoading.Name = "lbLoading";
            this.lbLoading.Size = new System.Drawing.Size(78, 16);
            this.lbLoading.TabIndex = 13;
            this.lbLoading.Text = "Загрузка...";
            this.lbLoading.Visible = false;
            // 
            // lbPageLink
            // 
            this.lbPageLink.AutoSize = true;
            this.lbPageLink.Location = new System.Drawing.Point(142, 130);
            this.lbPageLink.Name = "lbPageLink";
            this.lbPageLink.Size = new System.Drawing.Size(52, 13);
            this.lbPageLink.TabIndex = 14;
            this.lbPageLink.Text = "PageLink";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(701, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(312, 462);
            this.flowLayoutPanel1.TabIndex = 15;
            // 
            // txtStartPage
            // 
            this.txtStartPage.Location = new System.Drawing.Point(201, 35);
            this.txtStartPage.Name = "txtStartPage";
            this.txtStartPage.Size = new System.Drawing.Size(494, 20);
            this.txtStartPage.TabIndex = 16;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(186, 13);
            this.label1.TabIndex = 17;
            this.label1.Text = "Страница, с котоой начать экспорт";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1013, 462);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtStartPage);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.lbPageLink);
            this.Controls.Add(this.lbLoading);
            this.Controls.Add(this.chbListBox);
            this.Controls.Add(this.btStart);
            this.Controls.Add(this.chbGoToUrl);
            this.Controls.Add(this.treeView1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Button btStart;
        private System.Windows.Forms.CheckBox chbGoToUrl;
        private System.Windows.Forms.CheckedListBox chbListBox;
        private System.Windows.Forms.Label lbLoading;
        private System.Windows.Forms.Label lbPageLink;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TextBox txtStartPage;
        private System.Windows.Forms.Label label1;
    }
}

