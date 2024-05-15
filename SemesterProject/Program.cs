using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SemesterProject
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (var db = new DataClasses1DataContext())
            {
                LoginForm login = new LoginForm(db);
                Application.Run(login);
                if (login.Successful)
                {
                    using (Storefront storefront = new Storefront(db, login.Customer))
                    {
                        Application.Run(storefront);
                    }
                }
            }
        }
    }
}