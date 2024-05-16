
namespace SemesterProject
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
            this.btnLogin = new System.Windows.Forms.Button();
            this.tbLoginId = new System.Windows.Forms.TextBox();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.lblLoginId = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.btnCreateAccount = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.rtbLoginStatus = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(103, 116);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(149, 60);
            this.btnLogin.TabIndex = 3;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // tbLoginId
            // 
            this.tbLoginId.Location = new System.Drawing.Point(125, 31);
            this.tbLoginId.Name = "tbLoginId";
            this.tbLoginId.Size = new System.Drawing.Size(181, 22);
            this.tbLoginId.TabIndex = 1;
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(125, 60);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.Size = new System.Drawing.Size(181, 22);
            this.tbPassword.TabIndex = 2;
            // 
            // lblLoginId
            // 
            this.lblLoginId.AutoSize = true;
            this.lblLoginId.Location = new System.Drawing.Point(38, 34);
            this.lblLoginId.Name = "lblLoginId";
            this.lblLoginId.Size = new System.Drawing.Size(60, 17);
            this.lblLoginId.TabIndex = 3;
            this.lblLoginId.Text = "Login ID";
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(38, 63);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(69, 17);
            this.lblPassword.TabIndex = 4;
            this.lblPassword.Text = "Password";
            // 
            // btnCreateAccount
            // 
            this.btnCreateAccount.Location = new System.Drawing.Point(174, 192);
            this.btnCreateAccount.Name = "btnCreateAccount";
            this.btnCreateAccount.Size = new System.Drawing.Size(118, 50);
            this.btnCreateAccount.TabIndex = 4;
            this.btnCreateAccount.Text = "Create Account";
            this.btnCreateAccount.UseVisualStyleBackColor = true;
            this.btnCreateAccount.Click += new System.EventHandler(this.btnCreateAccount_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(63, 209);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 17);
            this.label1.TabIndex = 6;
            this.label1.Text = "No Account?";
            // 
            // rtbLoginStatus
            // 
            this.rtbLoginStatus.BackColor = System.Drawing.SystemColors.Window;
            this.rtbLoginStatus.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbLoginStatus.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.rtbLoginStatus.Location = new System.Drawing.Point(12, 247);
            this.rtbLoginStatus.Name = "rtbLoginStatus";
            this.rtbLoginStatus.ReadOnly = true;
            this.rtbLoginStatus.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.rtbLoginStatus.Size = new System.Drawing.Size(333, 39);
            this.rtbLoginStatus.TabIndex = 8;
            this.rtbLoginStatus.Text = "";
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(357, 298);
            this.Controls.Add(this.rtbLoginStatus);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCreateAccount);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.lblLoginId);
            this.Controls.Add(this.tbPassword);
            this.Controls.Add(this.tbLoginId);
            this.Controls.Add(this.btnLogin);
            this.MaximumSize = new System.Drawing.Size(400, 375);
            this.MinimumSize = new System.Drawing.Size(345, 300);
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.TextBox tbLoginId;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.Label lblLoginId;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Button btnCreateAccount;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox rtbLoginStatus;
    }
}

