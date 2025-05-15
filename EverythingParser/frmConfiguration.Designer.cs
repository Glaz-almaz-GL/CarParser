namespace EverythingParser
{
    partial class frmConfiguration
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtCountIterations = new System.Windows.Forms.TextBox();
            this.lbCountIterations = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtCountIterations
            // 
            this.txtCountIterations.Location = new System.Drawing.Point(181, 12);
            this.txtCountIterations.Name = "txtCountIterations";
            this.txtCountIterations.Size = new System.Drawing.Size(100, 20);
            this.txtCountIterations.TabIndex = 0;
            this.txtCountIterations.Text = "0";
            this.txtCountIterations.TextChanged += new System.EventHandler(this.txtCountIterations_TextChanged);
            // 
            // lbCountIterations
            // 
            this.lbCountIterations.AutoSize = true;
            this.lbCountIterations.Location = new System.Drawing.Point(12, 15);
            this.lbCountIterations.Name = "lbCountIterations";
            this.lbCountIterations.Size = new System.Drawing.Size(163, 13);
            this.lbCountIterations.TabIndex = 1;
            this.lbCountIterations.Text = "Iterations to Find Parent Element:";
            // 
            // frmConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(365, 413);
            this.Controls.Add(this.lbCountIterations);
            this.Controls.Add(this.txtCountIterations);
            this.Name = "frmConfiguration";
            this.Text = "frmConfiguration";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtCountIterations;
        private System.Windows.Forms.Label lbCountIterations;
    }
}