namespace assignment_code.UI
{
    partial class LoginForm
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
            this._cardPanel = new System.Windows.Forms.Panel();
            this._lblTitle = new System.Windows.Forms.Label();
            this._alertPanel = new System.Windows.Forms.Panel();
            this._lblAlertText = new System.Windows.Forms.Label();
            this._lblUsername = new System.Windows.Forms.Label();
            this._usernameWrapper = new System.Windows.Forms.Panel();
            this._txtUsername = new System.Windows.Forms.TextBox();
            this._lblPassword = new System.Windows.Forms.Label();
            this._passwordWrapper = new System.Windows.Forms.Panel();
            this._txtPassword = new System.Windows.Forms.TextBox();
            this._btnTogglePassword = new System.Windows.Forms.Button();
            this._btnLogin = new System.Windows.Forms.Button();
            this._lblFooterText = new System.Windows.Forms.Label();
            this._lnkSignUp = new System.Windows.Forms.LinkLabel();
            this._cardPanel.SuspendLayout();
            this._alertPanel.SuspendLayout();
            this._usernameWrapper.SuspendLayout();
            this._passwordWrapper.SuspendLayout();
            this.SuspendLayout();
            // 
            // _cardPanel
            // 
            this._cardPanel.BackColor = System.Drawing.Color.Transparent;
            this._cardPanel.Controls.Add(this._lblTitle);
            this._cardPanel.Controls.Add(this._alertPanel);
            this._cardPanel.Controls.Add(this._lblUsername);
            this._cardPanel.Controls.Add(this._usernameWrapper);
            this._cardPanel.Controls.Add(this._lblPassword);
            this._cardPanel.Controls.Add(this._passwordWrapper);
            this._cardPanel.Controls.Add(this._btnLogin);
            this._cardPanel.Controls.Add(this._lblFooterText);
            this._cardPanel.Controls.Add(this._lnkSignUp);
            this._cardPanel.Location = new System.Drawing.Point(40, 42);
            this._cardPanel.Name = "_cardPanel";
            this._cardPanel.Size = new System.Drawing.Size(420, 396);
            this._cardPanel.TabIndex = 0;
            this._cardPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.CardPanel_Paint);
            this._cardPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Window_MouseDown);
            // 
            // _lblTitle
            // 
            this._lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this._lblTitle.Location = new System.Drawing.Point(32, 26);
            this._lblTitle.Name = "_lblTitle";
            this._lblTitle.Size = new System.Drawing.Size(356, 44);
            this._lblTitle.TabIndex = 0;
            this._lblTitle.Text = "Login";
            // 
            // _alertPanel
            // 
            this._alertPanel.BackColor = System.Drawing.Color.Transparent;
            this._alertPanel.Controls.Add(this._lblAlertText);
            this._alertPanel.Location = new System.Drawing.Point(32, 74);
            this._alertPanel.Name = "_alertPanel";
            this._alertPanel.Size = new System.Drawing.Size(356, 32);
            this._alertPanel.TabIndex = 1;
            this._alertPanel.Visible = false;
            this._alertPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.AlertPanel_Paint);
            // 
            // _lblAlertText
            // 
            this._lblAlertText.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblAlertText.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this._lblAlertText.Location = new System.Drawing.Point(28, 6);
            this._lblAlertText.Name = "_lblAlertText";
            this._lblAlertText.Size = new System.Drawing.Size(324, 20);
            this._lblAlertText.TabIndex = 0;
            // 
            // _lblUsername
            // 
            this._lblUsername.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblUsername.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this._lblUsername.Location = new System.Drawing.Point(32, 80);
            this._lblUsername.Name = "_lblUsername";
            this._lblUsername.Size = new System.Drawing.Size(356, 26);
            this._lblUsername.TabIndex = 2;
            this._lblUsername.Text = "Username";
            // 
            // _usernameWrapper
            // 
            this._usernameWrapper.Controls.Add(this._txtUsername);
            this._usernameWrapper.Cursor = System.Windows.Forms.Cursors.IBeam;
            this._usernameWrapper.Location = new System.Drawing.Point(32, 108);
            this._usernameWrapper.Name = "_usernameWrapper";
            this._usernameWrapper.Size = new System.Drawing.Size(356, 44);
            this._usernameWrapper.TabIndex = 3;
            this._usernameWrapper.Click += new System.EventHandler(this.UsernameWrapper_Click);
            this._usernameWrapper.Paint += new System.Windows.Forms.PaintEventHandler(this.UsernameWrapper_Paint);
            // 
            // _txtUsername
            // 
            this._txtUsername.BackColor = System.Drawing.Color.White;
            this._txtUsername.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._txtUsername.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._txtUsername.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this._txtUsername.Location = new System.Drawing.Point(36, 12);
            this._txtUsername.Name = "_txtUsername";
            this._txtUsername.Size = new System.Drawing.Size(306, 20);
            this._txtUsername.TabIndex = 0;
            this._txtUsername.Enter += new System.EventHandler(this.TxtUsername_Enter);
            this._txtUsername.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtField_KeyDown);
            this._txtUsername.Leave += new System.EventHandler(this.TxtUsername_Leave);
            // 
            // _lblPassword
            // 
            this._lblPassword.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this._lblPassword.Location = new System.Drawing.Point(32, 162);
            this._lblPassword.Name = "_lblPassword";
            this._lblPassword.Size = new System.Drawing.Size(356, 26);
            this._lblPassword.TabIndex = 4;
            this._lblPassword.Text = "Password";
            // 
            // _passwordWrapper
            // 
            this._passwordWrapper.Controls.Add(this._txtPassword);
            this._passwordWrapper.Controls.Add(this._btnTogglePassword);
            this._passwordWrapper.Cursor = System.Windows.Forms.Cursors.IBeam;
            this._passwordWrapper.Location = new System.Drawing.Point(32, 190);
            this._passwordWrapper.Name = "_passwordWrapper";
            this._passwordWrapper.Size = new System.Drawing.Size(356, 44);
            this._passwordWrapper.TabIndex = 5;
            this._passwordWrapper.Click += new System.EventHandler(this.PasswordWrapper_Click);
            this._passwordWrapper.Paint += new System.Windows.Forms.PaintEventHandler(this.PasswordWrapper_Paint);
            // 
            // _txtPassword
            // 
            this._txtPassword.BackColor = System.Drawing.Color.White;
            this._txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._txtPassword.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._txtPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this._txtPassword.Location = new System.Drawing.Point(36, 12);
            this._txtPassword.Name = "_txtPassword";
            this._txtPassword.Size = new System.Drawing.Size(275, 20);
            this._txtPassword.TabIndex = 0;
            this._txtPassword.UseSystemPasswordChar = true;
            this._txtPassword.Enter += new System.EventHandler(this.TxtPassword_Enter);
            this._txtPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtField_KeyDown);
            this._txtPassword.Leave += new System.EventHandler(this.TxtPassword_Leave);
            // 
            // _btnTogglePassword
            // 
            this._btnTogglePassword.BackColor = System.Drawing.Color.Transparent;
            this._btnTogglePassword.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnTogglePassword.FlatAppearance.BorderSize = 0;
            this._btnTogglePassword.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnTogglePassword.Location = new System.Drawing.Point(320, 7);
            this._btnTogglePassword.Name = "_btnTogglePassword";
            this._btnTogglePassword.Size = new System.Drawing.Size(30, 30);
            this._btnTogglePassword.TabIndex = 1;
            this._btnTogglePassword.UseVisualStyleBackColor = false;
            this._btnTogglePassword.Click += new System.EventHandler(this.BtnTogglePassword_Click);
            this._btnTogglePassword.Paint += new System.Windows.Forms.PaintEventHandler(this.BtnTogglePassword_Paint);
            // 
            // _btnLogin
            // 
            this._btnLogin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this._btnLogin.Cursor = System.Windows.Forms.Cursors.Hand;
            this._btnLogin.FlatAppearance.BorderSize = 0;
            this._btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnLogin.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._btnLogin.ForeColor = System.Drawing.Color.White;
            this._btnLogin.Location = new System.Drawing.Point(32, 252);
            this._btnLogin.Name = "_btnLogin";
            this._btnLogin.Size = new System.Drawing.Size(356, 46);
            this._btnLogin.TabIndex = 6;
            this._btnLogin.Text = "Login";
            this._btnLogin.UseVisualStyleBackColor = false;
            this._btnLogin.Click += new System.EventHandler(this.BtnLogin_Click);
            this._btnLogin.Paint += new System.Windows.Forms.PaintEventHandler(this.BtnLogin_Paint);
            this._btnLogin.MouseEnter += new System.EventHandler(this.BtnLogin_MouseEnter);
            this._btnLogin.MouseLeave += new System.EventHandler(this.BtnLogin_MouseLeave);
            // 
            // _lblFooterText
            // 
            this._lblFooterText.AutoSize = true;
            this._lblFooterText.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lblFooterText.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this._lblFooterText.Location = new System.Drawing.Point(32, 310);
            this._lblFooterText.Name = "_lblFooterText";
            this._lblFooterText.Size = new System.Drawing.Size(131, 15);
            this._lblFooterText.TabIndex = 7;
            this._lblFooterText.Text = "Don\'t have an account?";
            // 
            // _lnkSignUp
            // 
            this._lnkSignUp.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(64)))), ((int)(((byte)(175)))));
            this._lnkSignUp.AutoSize = true;
            this._lnkSignUp.Cursor = System.Windows.Forms.Cursors.Hand;
            this._lnkSignUp.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._lnkSignUp.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this._lnkSignUp.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this._lnkSignUp.Location = new System.Drawing.Point(192, 310);
            this._lnkSignUp.Name = "_lnkSignUp";
            this._lnkSignUp.Size = new System.Drawing.Size(51, 15);
            this._lnkSignUp.TabIndex = 8;
            this._lnkSignUp.TabStop = true;
            this._lnkSignUp.Text = "Sign up";
            this._lnkSignUp.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this._lnkSignUp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkSignUp_LinkClicked);
            // 
            // LoginForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(244)))), ((int)(((byte)(246)))));
            this.ClientSize = new System.Drawing.Size(500, 480);
            this.Controls.Add(this._cardPanel);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PCCFPI Store - Login";
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Window_MouseDown);
            this._cardPanel.ResumeLayout(false);
            this._cardPanel.PerformLayout();
            this._alertPanel.ResumeLayout(false);
            this._usernameWrapper.ResumeLayout(false);
            this._usernameWrapper.PerformLayout();
            this._passwordWrapper.ResumeLayout(false);
            this._passwordWrapper.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _cardPanel;
        private System.Windows.Forms.Label _lblTitle;
        private System.Windows.Forms.Panel _alertPanel;
        private System.Windows.Forms.Label _lblAlertText;
        private System.Windows.Forms.Label _lblUsername;
        private System.Windows.Forms.Panel _usernameWrapper;
        private System.Windows.Forms.TextBox _txtUsername;
        private System.Windows.Forms.Label _lblPassword;
        private System.Windows.Forms.Panel _passwordWrapper;
        private System.Windows.Forms.TextBox _txtPassword;
        private System.Windows.Forms.Button _btnTogglePassword;
        private System.Windows.Forms.Button _btnLogin;
        private System.Windows.Forms.Label _lblFooterText;
        private System.Windows.Forms.LinkLabel _lnkSignUp;
    }
}
