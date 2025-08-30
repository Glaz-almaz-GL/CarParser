namespace HaynesProParser
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
            btStart = new System.Windows.Forms.Button();
            treeView1 = new System.Windows.Forms.TreeView();
            SuspendLayout();
            // 
            // btStart
            // 
            btStart.Location = new System.Drawing.Point(12, 371);
            btStart.Name = "btStart";
            btStart.Size = new System.Drawing.Size(175, 67);
            btStart.TabIndex = 0;
            btStart.Text = "Start";
            btStart.UseVisualStyleBackColor = true;
            btStart.Click += btStart_Click;
            // 
            // treeView1
            // 
            treeView1.Location = new System.Drawing.Point(193, 12);
            treeView1.Name = "treeView1";
            treeView1.Size = new System.Drawing.Size(595, 426);
            treeView1.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(treeView1);
            Controls.Add(btStart);
            Name = "Form1";
            Text = "Form1";
            FormClosing += Form1_FormClosing;
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button btStart;
        private System.Windows.Forms.TreeView treeView1;
    }
}

