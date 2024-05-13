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
            using (var db = new DataClasses1DataContext()) // todo, is just one using clause here enough, or should there be multiple in more places where db is used in other classes?
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