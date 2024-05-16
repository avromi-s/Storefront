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
        private CUSTOMER _customer;

        public CUSTOMER Customer
        {
            get { return _customer; }
            private set { _customer = value; }
        }

        private DataClasses1DataContext db;

        public LoginForm(DataClasses1DataContext db)
        {
            InitializeComponent();
            this.db = db;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            Successful = AttemptLogin(tbLoginId.Text, tbPassword.Text, label2, ref _customer);
            if (Successful)
            {
                // close the login form. Program class will then see that login.successful is true and launch storefront
                ActiveForm.Close();
            }
        }

        private void btnCreateAccount_Click(object sender, EventArgs e)
        {
            Successful = CreateLogin(tbLoginId.Text, tbPassword.Text, label2, ref _customer);
            if (Successful)
            {
                // close the login form. Program class will then see that login.successful is true and launch storefront
                ActiveForm.Close();
            }
        }

        private bool AttemptLogin(string LoginId, string Password, Label lblResult, ref CUSTOMER customerLoggedIn)
        {
            bool loginExists = db.CUSTOMERs.Select(row => row.LoginId).Contains(LoginId);
            if (loginExists)
            {
                CUSTOMER foundCustomer = db.CUSTOMERs.First(row => row.LoginId == LoginId); // login id is enforced as unique by db index
                // validate password:
                if (foundCustomer.Password == Password)
                {
                    lblResult.Text = "Successful login";
                    lblResult.ForeColor = Color.Green;
                    customerLoggedIn = foundCustomer;
                    return true;
                }
                else
                {
                    lblResult.Text = "Invalid password";
                    lblResult.ForeColor = Color.Red;
                }
            }
            else
            {
                lblResult.Text = "Login ID not found";
                lblResult.ForeColor = Color.Red;
            }

            customerLoggedIn = null; // only return customer on successful login
            return false;
        }

        private bool CreateLogin(string LoginId, string Password, Label lblResult, ref CUSTOMER loggedInCustomer)
        {
            bool loginAlreadyExists = db.CUSTOMERs.Select(row => row.LoginId).Contains(LoginId);
            if (!loginAlreadyExists)
            {
                CUSTOMER createdCustomer = new CUSTOMER()
                {
                    LoginId = LoginId,
                    Password = Password
                };
                db.CUSTOMERs.InsertOnSubmit(createdCustomer);
                db.SubmitChanges();
                lblResult.Text = "Account successfully created.";
                lblResult.ForeColor = Color.Green;
                loggedInCustomer = createdCustomer;
                return true;
            }
            else
            {
                lblResult.Text = "That login ID is not available. Please try again.";
                lblResult.ForeColor = Color.Red;
                loggedInCustomer = null; // only return customer on successful login
                return false;
            }
        }
    }
}