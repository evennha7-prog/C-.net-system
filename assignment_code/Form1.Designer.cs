namespace assignment_code
{
    partial class Form1
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
            this._mainContainer = new System.Windows.Forms.Panel();
            this._viewsContainer = new System.Windows.Forms.Panel();
            this._topHeader = new assignment_code.UI.Controls.TopHeaderControl();
            this._sidebar = new assignment_code.UI.Controls.SidebarControl();
            this._mainContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // _mainContainer
            // 
            this._mainContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this._mainContainer.Controls.Add(this._viewsContainer);
            this._mainContainer.Controls.Add(this._topHeader);
            this._mainContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._mainContainer.Location = new System.Drawing.Point(220, 0);
            this._mainContainer.Name = "_mainContainer";
            this._mainContainer.Padding = new System.Windows.Forms.Padding(24, 14, 24, 18);
            this._mainContainer.Size = new System.Drawing.Size(1060, 820);
            this._mainContainer.TabIndex = 1;
            // 
            // _viewsContainer
            // 
            this._viewsContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._viewsContainer.Location = new System.Drawing.Point(24, 88);
            this._viewsContainer.Name = "_viewsContainer";
            this._viewsContainer.Size = new System.Drawing.Size(1012, 714);
            this._viewsContainer.TabIndex = 1;
            // 
            // _topHeader
            // 
            this._topHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this._topHeader.Location = new System.Drawing.Point(24, 14);
            this._topHeader.Name = "_topHeader";
            this._topHeader.Size = new System.Drawing.Size(1012, 72);
            this._topHeader.StoreName = "PCCFPI STORE";
            this._topHeader.TabIndex = 0;
            this._topHeader.UserName = "Stephanie Sharkey";
            this._topHeader.Click += new System.EventHandler(this._topHeader_Click);
            // 
            // _sidebar
            // 
            this._sidebar.BackColor = System.Drawing.Color.White;
            this._sidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this._sidebar.Location = new System.Drawing.Point(0, 0);
            this._sidebar.Name = "_sidebar";
            this._sidebar.Size = new System.Drawing.Size(220, 820);
            this._sidebar.TabIndex = 0;
            this._sidebar.Paint += new System.Windows.Forms.PaintEventHandler(this._sidebar_Paint);
            // 
            // Form1
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1280, 820);
            this.Controls.Add(this._mainContainer);
            this.Controls.Add(this._sidebar);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(1020, 680);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PCCFPI STORE - Management Dashboard";
            this.Load += new System.EventHandler(this.Form1_Load);
            this._mainContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
