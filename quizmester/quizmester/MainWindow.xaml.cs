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
using System.Windows.Threading;

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
        /// 4 = view quiz info screen (Screen4)
        /// 5 = coundown screen (Screen5)
        /// 6 = view question screen (Screen6)
        /// default screen is login screen
        /// </summary>
        int CurrentScreen = 1;
        // the max screen number... change this if you add more screens
        int ScreenMax = 6;

        /// <summary>
        /// countdown timer for the start of the quiz and time left for answering questions
        /// </summary>
        int CountDown;

        bool GameStarted = false;

        // timer for the countdown
        DispatcherTimer timer;

        // the current quiz id
        int CurrentQuizId = 0;

        List<string> Question_ids = new List<string>();

        string connectionString;

        // main class connection string
        ExecuteQuery Query = new ExecuteQuery();

        public MainWindow()
        {
            InitializeComponent();

            connectionString = Query.connectionString;

            // update screen
            ScreenCheck();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string L_Username = Tbx_Username_L.Text;
            string L_Password = Pbx_Password_L.Password;

            // SQL query with parameters
            int count = Query.ExecuteScalar("SELECT COUNT(*) FROM Accounts WHERE Username ='" + L_Username + "'AND Password ='" + L_Password + "';");

            if (count == 1)
            {
                // go to game choise screen
                CurrentScreen = 3;
            }

            // update screen
            ScreenCheck();
        }

        private void BtnSignUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // get username and password from text boxes
                string L_Username = Tbx_Username_S.Text;
                string L_Password = Pbx_Password_S.Password;
                string L_Password2 = Pbx_Password2_S.Password;

                // check if username and password are not empty
                if (L_Username != "" && L_Password != "" && L_Password2 != "")
                {
                    // check if passwords are the same
                    if (L_Password2 == L_Password)
                    {
                        Pbx_Password_S.Password = "";
                        Pbx_Password2_S.Password = "";

                        // check if username already exists
                        int count = Query.ExecuteScalar("SELECT COUNT(*) FROM Accounts WHERE Username ='" + L_Username + "';");

                        // if username exists, show message and return
                        if (count == 1)
                        {
                            MessageBox.Show("Username already exists");
                            return;
                        }

                        // clear username text box
                        Tbx_Username_S.Text = "";

                        // insert new account into database with user type "User"
                        string L_User = "User";

                        // put it in the database
                        int row = Query.ExecuteQueryNonQuery("INSERT INTO Accounts (Username, Password, Type) VALUES ('" + L_Username + "','" + L_Password + "','" + L_User + "');");

                        // if ammound of rows affected is more than 0, go to game choise screen
                        if (row > 0)
                            CurrentScreen = 3;
                        // if not , show error message
                        else
                            MessageBox.Show("Error signing up.");
                    }
                    // when passwords arent the same
                    else
                    {
                        MessageBox.Show("Passwords are not the same");
                    }
                }
                // when textbox is left empty
                else
                {
                    MessageBox.Show("please fill in all the boxes");
                }
            }
            // when the try block fails
            catch
            {
                MessageBox.Show("translating error try again with a different username/password");
            }


            // update screen if needed
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
                    if ((i == CurrentScreen) || (CurrentScreen == 4 && i == 3))
                    {
                        screen.Visibility = Visibility.Visible;

                        // if the screen is the game choise screen, make the window maximized and hide the startup grid
                        if (i >= 3) 
                        { 
                            Main.WindowState = WindowState.Maximized; 
                            StartupGrid.Visibility = Visibility.Collapsed; 
                            MainGrid.Visibility = Visibility.Visible;

                            if (i >= 3 && i < 5) ShowQuizes();
                        }
                        // if the screen is not the game choise screen, make the window normal and show the startup grid
                        else 
                        { 
                            Main.WindowState = WindowState.Normal; 
                            StartupGrid.Visibility = Visibility.Visible; 
                            MainGrid.Visibility = Visibility.Collapsed;
                            this.Height = 550; this.Width = 900;
                        }
                    }
                    else
                    {
                        screen.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        /// <summary>
        /// get the quizes from the database and show them
        /// </summary>
        private void ShowQuizes()
        {
            // find the wrap panel and clear it
            var wrapPanel = this.FindName("WplQuizes") as WrapPanel;
            wrapPanel.Children.Clear();

            // Create an instance of ExecuteQuery to call the GetDataTable method
            DataTable DataThemes = Query.GetDataTable("SELECT Id, Theme FROM Themes;");

            // Add each quiz name to the list box
            foreach (DataRow rowThemes in DataThemes.Rows)
            {
                var border = new Border
                {
                    Width = 250,
                    Height = 350,
                    Margin = new Thickness(10),
                    CornerRadius = new CornerRadius(20),
                    Background = new RadialGradientBrush
                    {
                        GradientOrigin = new Point(0.3, 0.3),
                        RadiusX = 1.2,
                        RadiusY = 1.2,
                        GradientStops = new GradientStopCollection
                            {
                                new GradientStop((Color)ColorConverter.ConvertFromString("#1A0033"), 0.0),
                                new GradientStop((Color)ColorConverter.ConvertFromString("#4B0082"), 0.4),
                                new GradientStop((Color)ColorConverter.ConvertFromString("#6A0DAD"), 0.8),
                                new GradientStop((Color)ColorConverter.ConvertFromString("#2D1B69"), 1.0),
                            }
                    }
                };

                // create a grid to hold the label and button
                var grid = new Grid();
                // put it in the border
                border.Child = grid;

                // create a label for the quiz name
                grid.Children.Add(new Label
                {
                    Content = rowThemes["Theme"].ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    FontSize = 32,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 10),
                    Foreground = Brushes.White,
                });

                // create a button to view the quiz
                var button = new Button
                {
                    Content = "View",
                    Width = 200, Height = 40,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(0, 0, 0, 10),
                    Tag = rowThemes["Id"].ToString(), // store the quiz id in the button's Tag property
                };

                // Look for the style in this element's resource lookup chain
                button.Style = (Style)this.FindResource("ModernButton");
                // to add click event to the button
                button.Click += BtnView_Click;

                // add the button to the grid
                grid.Children.Add(button);

                // get the high scores for this quiz
                DataTable dataHighScores = Query.GetDataTable("SELECT TOP 5 Username FROM HighScores WHERE Theme_Id = " + rowThemes["Id"].ToString() + " ORDER BY Score DESC;");

                // create a stack panel to hold the high scores
                var NewStackPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Width = 200, Height = 225,
                };
                grid.Children.Add(NewStackPanel);

                // add a label for the high scores  
                if (dataHighScores != null)
                {
                    // what spot they are in
                    int i = 1;
                    foreach (DataRow rowScores in dataHighScores.Rows)
                    {
                        NewStackPanel.Children.Add(new Label
                        {
                            Content = i + ": " + rowScores["Username"].ToString(),
                            FontSize = 24,
                            Foreground = Brushes.White,
                        });
                        // go up
                        i++;   
                    }
                }

                // put everything in the wrap panel
                wrapPanel.Children.Add(border);
            }
        }

        private void BtnView_Click(object sender, RoutedEventArgs e)
        {
            // find the button that was clicked
            Button button = sender as Button;
            CurrentQuizId = int.Parse(button.Tag.ToString());
            if (CurrentQuizId != 0)
            {
                // make the current screen the view screen
                CurrentScreen = 4;

                // get the quiz id from the button's Tag property
                int themeId = CurrentQuizId;
                // get the quiz name from the database from the id in the button's Tag property
                DataTable DataThemes = Query.GetDataTable("SELECT Account_Id, Theme FROM Themes WHERE Id = '" + themeId + "' ;");

                // go trough all the rows (should only be one)
                foreach (DataRow rowThemes in DataThemes.Rows)
                {
                    // set the label to the quiz name
                    var textBlock = this.FindName("LblQuizName") as TextBlock;
                    if (textBlock != null) textBlock.Text = rowThemes["Theme"].ToString();

                    // set the label to the quiz creator
                    textBlock = this.FindName("LblQuizCreator") as TextBlock;
                    if (textBlock != null && rowThemes["Account_Id"] != null)
                    {
                        // get the username from the database from the account id in the quiz
                        DataTable DataAcounts = Query.GetDataTable("SELECT Username FROM Accounts WHERE Id = '" + rowThemes["Account_Id"] + "' ;");

                        // go trough all the rows (should only be one)
                        foreach (DataRow rowAcounts in DataAcounts.Rows)
                        {
                            // set the label to the quiz creator
                            textBlock.Text = "Created by: " + rowAcounts["Username"].ToString();
                        }
                    }

                    // get the 10 high scores for this quiz. if there are less than 10,
                    // it will return all of them,
                    // if more than 10, it will return the top 10
                    DataTable dataHighScores = Query.GetDataTable("SELECT TOP 10 Username, Score FROM HighScores WHERE Theme_Id = " + themeId + " ORDER BY Score DESC;");

                    // get the stack panel
                    var stackPanel = this.FindName("SplAllHighScores") as StackPanel;
                    if(dataHighScores != null && stackPanel != null)
                    {
                        // clear the stack panel
                        stackPanel.Children.Clear();

                        // to set the place they are in
                        int i = 1;
                        foreach (DataRow rowScores in dataHighScores.Rows)
                        {
                            // add a label for each high score with the username and score on there place
                            stackPanel.Children.Add(new Label
                            {
                                Content = i + ": " + rowScores["Username"].ToString() + ", Score: " + rowScores["Score"],
                                FontSize = 24,
                                VerticalAlignment = VerticalAlignment.Center,
                                Foreground = Brushes.White
                            });
                            i++;
                        }
                    }
                }
            }

            // update screen
            ScreenCheck();
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

        private void BtnStartQuiz_Click(object sender, RoutedEventArgs e)
        {
            if(CurrentQuizId != 0)
            {
                // go to the CountDown screen
                CurrentScreen = 5;
                ScreenCheck();

                LblStartTimer.Visibility = Visibility.Visible;

                // set the countdown time in seconds
                CountDown = 5;
                StartTimer();
            }
            else { MessageBox.Show("Something went wrong please try again"); CurrentScreen = 3; }
        }

        private void StartTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // every 1 second
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Safe to update UI here
            CountDown--;
            // show the countdown in the label
            LblStartTimer.Content = CountDown.ToString();

            if (CountDown == 0 & GameStarted == false)
            {
                // game starts now
                GameStarted = true;

                LblStartTimer.Visibility = Visibility.Collapsed;

                // go to the question screen
                CurrentScreen = 6;
                ScreenCheck();

                // get and show the questions
                GetQuestions();
                ShowQuiz();

                // time to answer the questions
                CountDown = 60;
            }
            else if (CountDown == 0 & GameStarted == true)
            {
                // time to answer the question is up
                timer.Stop();
                // go to the next question or end the game if there are no more questions

            }
        }
        private void GetQuestions()
        {
            Question_ids.Clear();

            if (CurrentQuizId != 0)
            {
                // get the quiz id from the database from the quiz name
                DataTable DataThemes = Query.GetDataTable("SELECT Theme FROM Themes WHERE Id = '" + CurrentQuizId + "' ;");

                // go trough all the rows (should only be one)
                foreach (DataRow rowThemes in DataThemes.Rows)
                {
                    lblname.Text = rowThemes["Theme"].ToString();
                }

                // get the question ids from the database from the quiz id
                DataTable DataQuestions = Query.GetDataTable("SELECT Id FROM Questions WHERE Theme_Id = '" + CurrentQuizId + "' ;");
                // go trough all the rows and add the question ids to the list
                foreach (DataRow rowQuestions in DataQuestions.Rows)
                {
                    Question_ids.Add(rowQuestions["Id"].ToString());
                }

                Question_ids = Question_ids.OrderBy(x => Guid.NewGuid()).ToList(); // shuffle the list
            }
        }

        private void ShowQuiz()
        {

            var stackPanel = this.FindName("SplAnswers") as StackPanel;

            stackPanel.Children.Clear();
        }
    }
}