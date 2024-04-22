using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SemesterProject
{
    public partial class LoginForm : Form
    {
        public bool Successful { get; private set; } = false;
        private int? customerId = null;
        public int? CustomerId {
            get
            {
                return customerId;
            }
        }
        private DataClasses1DataContext db;
        public LoginForm(DataClasses1DataContext db)
        {
            InitializeComponent();
            this.db = db;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            Successful = AttemptLogin(tbLoginId.Text, tbPassword.Text, label2, ref customerId);
            if (Successful)
            {
                // close the login form. Program class will then see that login.successful is true and launch storefront
                ActiveForm.Close();
            }
        }

        private void btnCreateAccount_Click(object sender, EventArgs e)
        {
            Successful = CreateLogin(tbLoginId.Text, tbPassword.Text, label2, ref customerId);
            if (Successful)
            {
                // close the login form. Program class will then see that login.successful is true and launch storefront
                ActiveForm.Close();
            }
        }

        private bool AttemptLogin(string LoginId, string Password, Label lblResult, ref int? customerId)
        {
            // todo not so good this method has kinda 3 outputs - bool, customer, and label.Text?
            bool loginExists = db.CUSTOMERs.Select(row => row.LoginId).Contains(LoginId);
            if (loginExists)
            {
                CUSTOMER customer = db.CUSTOMERs.First(row => row.LoginId == LoginId);  // login id is enforced as unique by db index
                // validate password:
                if (customer.Password == Password)
                {
                    lblResult.Text = "Successful login";
                    customerId = customer.CustomerId;
                    return true;
                }
                else
                {
                    lblResult.Text = "Invalid password";
                }
            }
            else
            {
                lblResult.Text = "Login ID not found";
            }
            customerId = null;
            return false;
        }

        private bool CreateLogin(string LoginId, string Password, Label lblResult, ref int? customerId)
        {
            // todo not so good this method has kinda 3 outputs - bool, customer, and label.Text?
            bool loginAlreadyExists = db.CUSTOMERs.Select(row => row.LoginId).Contains(LoginId);
            if (!loginAlreadyExists)
            {
                CUSTOMER customer = new CUSTOMER()
                {
                    LoginId = LoginId,
                    Password = Password
                };
                db.CUSTOMERs.InsertOnSubmit(customer);
                db.SubmitChanges();
                lblResult.Text = "Account successfully created.";
                customerId = customer.CustomerId;
                return true;
            }
            else
            {
                lblResult.Text = "That login ID is not available. Please try again.";
                customerId = null;
                return false;
            }
        }
    }
}
