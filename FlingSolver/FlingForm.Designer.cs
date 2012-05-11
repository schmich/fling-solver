namespace FlingSolver
{
    partial class FlingForm
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
            this._solveButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _solveButton
            // 
            this._solveButton.Location = new System.Drawing.Point(10, 10);
            this._solveButton.Name = "_solveButton";
            this._solveButton.Size = new System.Drawing.Size(123, 31);
            this._solveButton.TabIndex = 0;
            this._solveButton.Text = "&Solve";
            this._solveButton.UseVisualStyleBackColor = true;
            this._solveButton.Click += new System.EventHandler(this.OnSolveClick);
            // 
            // FlingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 477);
            this.Controls.Add(this._solveButton);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "FlingForm";
            this.Text = "FlingSolver";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _solveButton;

    }
}

