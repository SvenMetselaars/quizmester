using Microsoft.Data.SqlClient;
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
        int ScreenMax = 3;

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
            // SQL query with parameters
            string query = "SELECT COUNT(*) FROM Accounts WHERE Username = @Username AND Password = @Password";

            string username = Tbx_Username_L.Text;
            string password = Pbx_Password_L.Password;


            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Add parameter values to avoid SQL injection
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", password);

                    // ExecuteScalar returns the first column of the first row
                    int count = (int)cmd.ExecuteScalar();

                    if (count == 1)
                    {
                        // Login successful
                        MessageBox.Show("Login successful!");
                        // go to game choise screen
                        CurrentScreen = 3;
                    }
                }
            }

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
                if (L_Username != "" && L_Password != "" && L_Password2 != "")
                {
                    if(L_Password2 == L_Password)
                    {
                        Pbx_Password_S.Password = "";
                        Pbx_Password2_S.Password = "";
                        using (SqlConnection conn = new SqlConnection(connectionString))
                        {
                            conn.Open();
                            string query = "INSERT INTO Accounts (Username, Password, Type) VALUES (@Username, @Password, @Type)";
                            string query2 = "SELECT COUNT(*) FROM Accounts WHERE Username = @Username";
                            using (SqlCommand cmd2 = new SqlCommand(query2, conn))
                            {
                                cmd2.Parameters.AddWithValue("@Username", L_Username);
                                int count = Convert.ToInt32(cmd2.ExecuteScalar());
                                if (count == 1)
                                {
                                    MessageBox.Show("Username already exists");
                                    return;
                                }
                            }

                            Tbx_Username_S.Text = "";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@Username", L_Username);
                                cmd.Parameters.AddWithValue("@Password", L_Password);
                                cmd.Parameters.AddWithValue("@Type", "user");

                                int rows = cmd.ExecuteNonQuery();

                                if (rows > 0)
                                    MessageBox.Show("Sign up successful!");
                                else
                                    MessageBox.Show("Error signing up.");
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Passwords are not the same");
                    }
                }
                else
                {
                    MessageBox.Show("please fill in all the boxes");
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
                var screen = this.FindName("Screen" + i) as UIElement;
                if (screen != null)
                {
                    // if the screen is the current screen, show it, else hide it
                    if (i == CurrentScreen)
                    {
                        screen.Visibility = Visibility.Visible;

                        // if the screen is the game choise screen, make the window maximized and hide the startup grid
                        if (i >= 3) 
                        { 
                            Main.WindowState = WindowState.Maximized; 
                            StartupGrid.Visibility = Visibility.Collapsed; 
                            MainGrid.Visibility = Visibility.Visible;
                        }
                        // if the screen is not the game choise screen, make the window normal and show the startup grid
                        else 
                        { 
                            Main.WindowState = WindowState.Normal; 
                            StartupGrid.Visibility = Visibility.Visible; 
                            MainGrid.Visibility = Visibility.Collapsed;
                            this.Height = 450; this.Width = 800;
                        }
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

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}