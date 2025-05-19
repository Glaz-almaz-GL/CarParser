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
            this.SuspendLayout();
            // 
            // cbActions
            // 
            this.cbActions.FormattingEnabled = true;
            this.cbActions.Items.AddRange(new object[] {
            "Ничего",
            "Скачать файл по ссылке из атрибута",
            "Экспортировать текст атрибута",
            "Запомнить и потом перейти по ссылке из атрибута"});
            this.cbActions.Location = new System.Drawing.Point(34, 2);
            this.cbActions.Name = "cbActions";
            this.cbActions.Size = new System.Drawing.Size(206, 21);
            this.cbActions.TabIndex = 0;
            this.cbActions.Text = "Ничего";
            // 
            // lbAttribute
            // 
            this.lbAttribute.AutoSize = true;
            this.lbAttribute.Location = new System.Drawing.Point(3, 6);
            this.lbAttribute.Name = "lbAttribute";
            this.lbAttribute.Size = new System.Drawing.Size(25, 13);
            this.lbAttribute.TabIndex = 1;
            this.lbAttribute.Text = "href";
            // 
            // ucAction
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lbAttribute);
            this.Controls.Add(this.cbActions);
            this.Name = "ucAction";
            this.Size = new System.Drawing.Size(243, 26);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.ComboBox cbActions;
        public System.Windows.Forms.Label lbAttribute;
    }
}
