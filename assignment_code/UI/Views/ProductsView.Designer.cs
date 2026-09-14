namespace assignment_code.UI.Views
{
    partial class ProductsView
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
            this._txtSearch = new System.Windows.Forms.TextBox();
            this._cboCategory = new System.Windows.Forms.ComboBox();
            this._btnRefresh = new System.Windows.Forms.Button();
            this._btnAddProduct = new System.Windows.Forms.Button();
            this._grid = new System.Windows.Forms.DataGridView();
            this.colSku = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCategory = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStock = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEdit = new System.Windows.Forms.DataGridViewButtonColumn();
            this.colDelete = new System.Windows.Forms.DataGridViewButtonColumn();
            this._contentWrapper.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._grid)).BeginInit();
            this.SuspendLayout();
            // 
            // _contentWrapper
            // 
            this._contentWrapper.BackColor = System.Drawing.Color.Transparent;
            this._contentWrapper.Controls.Add(this._txtSearch);
            this._contentWrapper.Controls.Add(this._cboCategory);
            this._contentWrapper.Controls.Add(this._btnRefresh);
            this._contentWrapper.Controls.Add(this._btnAddProduct);
            this._contentWrapper.Controls.Add(this._grid);
            this._contentWrapper.Location = new System.Drawing.Point(0, 0);
            this._contentWrapper.Name = "_contentWrapper";
            this._contentWrapper.Size = new System.Drawing.Size(1012, 720);
            this._contentWrapper.TabIndex = 0;
            // 
            // _txtSearch
            // 
            this._txtSearch.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._txtSearch.Location = new System.Drawing.Point(0, 10);
            this._txtSearch.Name = "_txtSearch";
            this._txtSearch.Size = new System.Drawing.Size(200, 24);
            this._txtSearch.TabIndex = 0;
            this._txtSearch.TextChanged += new System.EventHandler(this.TxtSearch_TextChanged);
            // 
            // _cboCategory
            // 
            this._cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cboCategory.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._cboCategory.FormattingEnabled = true;
            this._cboCategory.Location = new System.Drawing.Point(216, 10);
            this._cboCategory.Name = "_cboCategory";
            this._cboCategory.Size = new System.Drawing.Size(160, 24);
            this._cboCategory.TabIndex = 1;
            this._cboCategory.SelectedIndexChanged += new System.EventHandler(this.CboCategory_SelectedIndexChanged);
            // 
            // _btnRefresh
            // 
            this._btnRefresh.BackColor = System.Drawing.Color.White;
            this._btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnRefresh.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this._btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnRefresh.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._btnRefresh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this._btnRefresh.Location = new System.Drawing.Point(726, 6);
            this._btnRefresh.Name = "_btnRefresh";
            this._btnRefresh.Size = new System.Drawing.Size(110, 34);
            this._btnRefresh.TabIndex = 2;
            this._btnRefresh.Text = "🔄 Refresh";
            this._btnRefresh.UseVisualStyleBackColor = false;
            this._btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            // 
            // _btnAddProduct
            // 
            this._btnAddProduct.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this._btnAddProduct.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnAddProduct.FlatAppearance.BorderSize = 0;
            this._btnAddProduct.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnAddProduct.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._btnAddProduct.ForeColor = System.Drawing.Color.White;
            this._btnAddProduct.Location = new System.Drawing.Point(852, 6);
            this._btnAddProduct.Name = "_btnAddProduct";
            this._btnAddProduct.Size = new System.Drawing.Size(160, 34);
            this._btnAddProduct.TabIndex = 3;
            this._btnAddProduct.Text = "+ Add New Product";
            this._btnAddProduct.UseVisualStyleBackColor = false;
            this._btnAddProduct.Click += new System.EventHandler(this.BtnAddProduct_Click);
            // 
            // _grid
            // 
            this._grid.AllowUserToAddRows = false;
            this._grid.AllowUserToDeleteRows = false;
            this._grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._grid.BackgroundColor = System.Drawing.Color.White;
            this._grid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSku,
            this.colName,
            this.colCategory,
            this.colPrice,
            this.colStock,
            this.colStatus,
            this.colEdit,
            this.colDelete});
            this._grid.EnableHeadersVisualStyles = false;
            this._grid.Location = new System.Drawing.Point(0, 56);
            this._grid.MultiSelect = false;
            this._grid.Name = "_grid";
            this._grid.ReadOnly = true;
            this._grid.RowHeadersVisible = false;
            this._grid.RowTemplate.Height = 40;
            this._grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._grid.Size = new System.Drawing.Size(1012, 640);
            this._grid.TabIndex = 4;
            this._grid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.Grid_CellContentClick);
            this._grid.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.Grid_CellDoubleClick);
            this._grid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.Grid_CellFormatting);
            // 
            // colSku
            // 
            this.colSku.DataPropertyName = "Sku";
            this.colSku.FillWeight = 60F;
            this.colSku.HeaderText = "SKU";
            this.colSku.Name = "colSku";
            this.colSku.ReadOnly = true;
            // 
            // colName
            // 
            this.colName.DataPropertyName = "Name";
            this.colName.FillWeight = 150F;
            this.colName.HeaderText = "Product Name";
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            // 
            // colCategory
            // 
            this.colCategory.DataPropertyName = "Category";
            this.colCategory.FillWeight = 85F;
            this.colCategory.HeaderText = "Category";
            this.colCategory.Name = "colCategory";
            this.colCategory.ReadOnly = true;
            // 
            // colPrice
            // 
            this.colPrice.DataPropertyName = "PriceFormatted";
            this.colPrice.FillWeight = 55F;
            this.colPrice.HeaderText = "Price ($)";
            this.colPrice.Name = "colPrice";
            this.colPrice.ReadOnly = true;
            // 
            // colStock
            // 
            this.colStock.DataPropertyName = "Stock";
            this.colStock.FillWeight = 45F;
            this.colStock.HeaderText = "Stock";
            this.colStock.Name = "colStock";
            this.colStock.ReadOnly = true;
            // 
            // colStatus
            // 
            this.colStatus.DataPropertyName = "Status";
            this.colStatus.FillWeight = 65F;
            this.colStatus.HeaderText = "Status";
            this.colStatus.Name = "colStatus";
            this.colStatus.ReadOnly = true;
            // 
            // colEdit
            // 
            this.colEdit.FillWeight = 45F;
            this.colEdit.HeaderText = "Edit";
            this.colEdit.Name = "colEdit";
            this.colEdit.ReadOnly = true;
            this.colEdit.Text = "✏️ Edit";
            this.colEdit.UseColumnTextForButtonValue = true;
            // 
            // colDelete
            // 
            this.colDelete.FillWeight = 45F;
            this.colDelete.HeaderText = "Delete";
            this.colDelete.Name = "colDelete";
            this.colDelete.ReadOnly = true;
            this.colDelete.Text = "🗑️ Delete";
            this.colDelete.UseColumnTextForButtonValue = true;
            // 
            // ProductsView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this._contentWrapper);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ProductsView";
            this.Size = new System.Drawing.Size(1012, 720);
            this.Resize += new System.EventHandler(this.ProductsView_Resize);
            this._contentWrapper.ResumeLayout(false);
            this._contentWrapper.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._grid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _contentWrapper;
        private System.Windows.Forms.TextBox _txtSearch;
        private System.Windows.Forms.ComboBox _cboCategory;
        private System.Windows.Forms.Button _btnRefresh;
        private System.Windows.Forms.Button _btnAddProduct;
        private System.Windows.Forms.DataGridView _grid;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSku;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCategory;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPrice;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStock;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
        private System.Windows.Forms.DataGridViewButtonColumn colEdit;
        private System.Windows.Forms.DataGridViewButtonColumn colDelete;
    }
}
