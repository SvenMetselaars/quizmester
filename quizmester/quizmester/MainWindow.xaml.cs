using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace quizmester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        /// <summary>
        /// 1 = login screen (Screen1)
        /// 2 = sign up screen (Screen2)
        /// 3 = game choise screen (Screen3)
        /// default screen is login screen
        /// </summary>
        int CurrentScreen = 1;
        // the max screen number... change this if you add more screens
        int ScreenMax = 2;

        string connectionString = "Data Source=localhost\\sqlexpress;Initial Catalog=database;Integrated Security=True;TrustServerCertificate=True";

        public MainWindow()
        {
            InitializeComponent();

            // update screen
            ScreenCheck();
        }

        /// <summary>
        /// if pressed will login user
        /// </summary>
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Login button clicked!");

            // update screen
            ScreenCheck();
        }

        /// <summary>
        /// button if pressed make new account
        /// </summary>
        private void BtnSignUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string L_Username = Tbx_Username_S.Text;
                string L_Password = Pbx_Password_S.Password;
                string L_Password2 = Pbx_Password2_S.Password;
                if (L_Username != null && L_Password != null && L_Password2 != null)
                {
                    if(L_Password2 == L_Password)
                    {
                        MessageBox.Show("Account created!");
                    }
                    else
                    {
                        MessageBox.Show("Passwords are not the same");
                    }
                }
                else
                {
                    MessageBox.Show("please fill in all the ");
                }
            }
            catch
            {
                MessageBox.Show("translating error try again with a different username/password");
            }


            // update screen
            ScreenCheck();
        }

        /// <summary>
        /// this is the function that checks what screen we are on and updates the UI accordingly
        /// </summary>
        private void ScreenCheck()
        {
            for (int i = 1; i <= ScreenMax; i++)
            {
                // find the screen with the name Screen + i
                var screen = this.FindName("Screen" + i) as Border;
                if (screen != null)
                {
                    // if the screen is the current screen, show it, else hide it
                    if (i == CurrentScreen)
                    {
                        screen.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        screen.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void BtnSignUpScreen_Click(object sender, RoutedEventArgs e)
        {
            // sign up screen
            CurrentScreen = 2;
            // update screen
            ScreenCheck();
        }

        private void BtnLoginScreen_Click(object sender, RoutedEventArgs e)
        {
            // sign up screen
            CurrentScreen = 1;
            // update screen
            ScreenCheck();
        }
    }
}