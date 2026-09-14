namespace assignment_code.UI.Views
{
    partial class CategoriesView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._contentWrapper = new System.Windows.Forms.Panel();
            this._btnRefresh = new System.Windows.Forms.Button();
            this._btnAddCategory = new System.Windows.Forms.Button();
            this._cardsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this._contentWrapper.SuspendLayout();
            this.SuspendLayout();
            // 
            // _contentWrapper
            // 
            this._contentWrapper.BackColor = System.Drawing.Color.Transparent;
            this._contentWrapper.Controls.Add(this._btnRefresh);
            this._contentWrapper.Controls.Add(this._btnAddCategory);
            this._contentWrapper.Controls.Add(this._cardsPanel);
            this._contentWrapper.Location = new System.Drawing.Point(0, 0);
            this._contentWrapper.Name = "_contentWrapper";
            this._contentWrapper.Size = new System.Drawing.Size(1012, 720);
            this._contentWrapper.TabIndex = 0;
            // 
            // _btnRefresh
            // 
            this._btnRefresh.BackColor = System.Drawing.Color.White;
            this._btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnRefresh.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this._btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnRefresh.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._btnRefresh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this._btnRefresh.Location = new System.Drawing.Point(0, 14);
            this._btnRefresh.Name = "_btnRefresh";
            this._btnRefresh.Size = new System.Drawing.Size(110, 34);
            this._btnRefresh.TabIndex = 0;
            this._btnRefresh.Text = "Refresh";
            this._btnRefresh.UseVisualStyleBackColor = false;
            this._btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            // 
            // _btnAddCategory
            // 
            this._btnAddCategory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this._btnAddCategory.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnAddCategory.FlatAppearance.BorderSize = 0;
            this._btnAddCategory.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnAddCategory.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._btnAddCategory.ForeColor = System.Drawing.Color.White;
            this._btnAddCategory.Location = new System.Drawing.Point(832, 14);
            this._btnAddCategory.Name = "_btnAddCategory";
            this._btnAddCategory.Size = new System.Drawing.Size(180, 34);
            this._btnAddCategory.TabIndex = 1;
            this._btnAddCategory.Text = "+ Add New Category";
            this._btnAddCategory.UseVisualStyleBackColor = false;
            this._btnAddCategory.Click += new System.EventHandler(this.BtnAddCategory_Click);
            // 
            // _cardsPanel
            // 
            this._cardsPanel.AutoScroll = true;
            this._cardsPanel.BackColor = System.Drawing.Color.Transparent;
            this._cardsPanel.Location = new System.Drawing.Point(0, 58);
            this._cardsPanel.Name = "_cardsPanel";
            this._cardsPanel.Size = new System.Drawing.Size(1012, 650);
            this._cardsPanel.TabIndex = 2;
            // 
            // CategoriesView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this._contentWrapper);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "CategoriesView";
            this.Size = new System.Drawing.Size(1012, 720);
            this.Resize += new System.EventHandler(this.CategoriesView_Resize);
            this._contentWrapper.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _contentWrapper;
        private System.Windows.Forms.FlowLayoutPanel _cardsPanel;
        private System.Windows.Forms.Button _btnAddCategory;
        private System.Windows.Forms.Button _btnRefresh;
    }
}
