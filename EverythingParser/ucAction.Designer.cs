namespace EverythingParser
{
    partial class ucAction
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

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.cbActions = new System.Windows.Forms.ComboBox();
            this.lbAttribute = new System.Windows.Forms.Label();
            this.lbTagName = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cbActions
            // 
            this.cbActions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbActions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.cbActions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbActions.FormattingEnabled = true;
            this.cbActions.Items.AddRange(new object[] {
            "Ничего",
            "Нажать по элементу",
            "Скачать файл по ссылке из атрибута",
            "Экспортировать текст атрибута",
            "Экспортировать ID из URL"});
            this.cbActions.Location = new System.Drawing.Point(64, 23);
            this.cbActions.Name = "cbActions";
            this.cbActions.Size = new System.Drawing.Size(270, 21);
            this.cbActions.TabIndex = 0;
            this.cbActions.SelectedIndexChanged += new System.EventHandler(this.cbActions_SelectedIndexChanged);
            // 
            // lbAttribute
            // 
            this.lbAttribute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbAttribute.AutoSize = true;
            this.lbAttribute.Location = new System.Drawing.Point(3, 26);
            this.lbAttribute.Name = "lbAttribute";
            this.lbAttribute.Size = new System.Drawing.Size(25, 13);
            this.lbAttribute.TabIndex = 1;
            this.lbAttribute.Text = "href";
            // 
            // lbTagName
            // 
            this.lbTagName.AutoSize = true;
            this.lbTagName.Location = new System.Drawing.Point(3, 7);
            this.lbTagName.Name = "lbTagName";
            this.lbTagName.Size = new System.Drawing.Size(54, 13);
            this.lbTagName.TabIndex = 2;
            this.lbTagName.Text = "TagName";
            // 
            // ucAction
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.Controls.Add(this.lbTagName);
            this.Controls.Add(this.lbAttribute);
            this.Controls.Add(this.cbActions);
            this.Name = "ucAction";
            this.Size = new System.Drawing.Size(337, 47);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.ComboBox cbActions;
        public System.Windows.Forms.Label lbAttribute;
        public System.Windows.Forms.Label lbTagName;
    }
}
