using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        /// 7 = score screen (Screen7)
        /// 8 = create quiz start screen (Screen8)
        /// 9 = create quiz question screen (Screen9)
        /// 10 = account management screen (Screen10)
        /// default screen is login screen
        /// </summary>
        int CurrentScreen = 1;
        // the max screen number... change this if you add more screens
        int ScreenMax = 10;

        int PlayerId = 0;
        string PlayerType = "User";

        int SkipAmount = 1;

        /// <summary>
        /// countdown timer for the start of the quiz 
        /// </summary>
        int CountDownStart;

        /// <summary>
        /// this is the time you have to answer the questions in seconds
        /// </summary>
        int CountDownEnd;

        /// <summary>
        /// this is the time you have to wait before the next question in seconds
        /// </summary>
        int CountDownNextQuestion;

        /// <summary>
        /// this is the time you have to answer the question in seconds
        /// </summary>
        int CountDownForQuestion;

        // this is the time you have to wait on the score screen in seconds
        int CountDownScoreScreen = -1;
        
        bool EditingQuiz = false;

        bool GameStarted = false;

        int Score = 0;

        // timer for the countdown
        DispatcherTimer timer;
        DispatcherTimer NewTimer;

        // the current quiz id
        int CurrentQuizId = -1;

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
            var count = Query.ExecuteScalar("SELECT COUNT(*) FROM Accounts WHERE Username ='" + L_Username + "'AND Password ='" + L_Password + "';");

            if (count.ToString() == "1")
            {
                // go to game choise screen
                CurrentScreen = 3;

                DataTable Account = Query.GetDataTable("SELECT Id, Type FROM Accounts WHERE Username = '" + L_Username + "' ;");
                foreach (DataRow row in Account.Rows)
                {
                    PlayerId = Convert.ToInt32(row["Id"]);
                    PlayerType = row["Type"].ToString();
                }
                

            }
            else
            {
                MessageBox.Show("Username or password is incorect");
                Pbx_Password_L.Password = "";
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
                        var count = Query.ExecuteScalar("SELECT COUNT(*) FROM Accounts WHERE Username ='" + L_Username + "';");

                        // if username exists, show message and return
                        if (count.ToString() == "1")
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
                        {
                            CurrentScreen = 3;

                            var Account = Query.ExecuteScalar("SELECT Id FROM Accounts WHERE Username = '" + L_Username + "' ;");
                            PlayerId = Convert.ToInt32(Account);

                            Tbx_Username_L.Text = L_Username;
                            Pbx_Password_L.Password = L_Password;
                        }
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
                    if ((i == CurrentScreen) || (CurrentScreen == 4 && i == 3) || (CurrentScreen == 10 && i == 3))
                    {
                        screen.Visibility = Visibility.Visible;

                        // if the screen is the game choise screen, make the window maximized and hide the startup grid
                        if (i >= 3)
                        {
                            Main.WindowState = WindowState.Maximized;
                            StartupGrid.Visibility = Visibility.Collapsed;
                            MainGrid.Visibility = Visibility.Visible;
                            BtnBack.Visibility = Visibility.Visible;

                            if (i >= 3 && i < 5)
                            {
                                lblname.Text = "Pathoot";
                                ShowQuizes();
                            } 

                            if (CurrentScreen == 3 && PlayerType == "Admin") BtnAcounts.Visibility = Visibility.Visible;
                            else BtnAcounts.Visibility = Visibility.Collapsed;
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

            var CreateBorder = new Border
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
            var CreateGrid = new Grid();
            // put it in the border
            CreateBorder.Child = CreateGrid;

            // create a label for the quiz name
            CreateGrid.Children.Add(new TextBlock
            {
                Text = "Create your own Quiz",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                FontSize = 48,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10),
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap
            });

            // create a button to view the quiz
            var CreateButton = new Button
            {
                Content = "Create",
                Width = 200,
                Height = 40,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, 10),
            };

            // Look for the style in this element's resource lookup chain
            CreateButton.Style = (Style)this.FindResource("ModernButton");
            // to add click event to the button
            CreateButton.Click += BtnCreate_Click;

            // add the button to the grid
            CreateGrid.Children.Add(CreateButton);

            wrapPanel.Children.Add(CreateBorder);
        }

        private void BtnView_Click(object sender, RoutedEventArgs e)
        {
            // find the button that was clicked
            Button button = sender as Button;
            CurrentQuizId = int.Parse(button.Tag.ToString());
            if (CurrentQuizId >= 0)
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

                    textBlock.Text = "Created by: Anonymous";
                    SplButtons.Width = 300;
                    BtnEditQuiz.Visibility = Visibility.Collapsed;
                    BtnDeleteQuiz.Visibility = Visibility.Collapsed;

                    if (textBlock != null && rowThemes["Account_Id"] != null)
                    {
                        // get the username from the database from the account id in the quiz
                        DataTable DataAcounts = Query.GetDataTable("SELECT Username FROM Accounts WHERE Id = '" + rowThemes["Account_Id"] + "' ;");

                        // go trough all the rows (should only be one)
                        foreach (DataRow rowAcounts in DataAcounts.Rows)
                        {
                            // set the label to the quiz creator
                            textBlock.Text = "Created by: " + rowAcounts["Username"].ToString();

                            if (PlayerId == Convert.ToInt32(rowThemes["Account_Id"]))
                            {
                                SplButtons.Width = 600;
                                BtnEditQuiz.Visibility = Visibility.Visible;
                                BtnDeleteQuiz.Visibility = Visibility.Visible;
                            }
                        }
                    }

                    if (PlayerType == "Admin")
                    {
                        SplButtons.Width = 600;
                        BtnEditQuiz.Visibility = Visibility.Visible;
                        if (CurrentQuizId != 0) BtnDeleteQuiz.Visibility = Visibility.Visible;
                        else BtnDeleteQuiz.Visibility = Visibility.Collapsed;
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
            if(CurrentQuizId >= 0)
            {
                var uniformGrid = this.FindName("SplAnswers") as UniformGrid;
                uniformGrid.IsEnabled = true;

                // go to the CountDown screen
                CurrentScreen = 5;
                ScreenCheck();

                SkipAmount = 1;

                // reset the score
                Score = 0;

                LblStartTimer.Visibility = Visibility.Visible;

                if (RbtnInfTime.IsChecked == false) PbrTimeLeft.Visibility = Visibility.Visible;
                else PbrTimeLeft.Visibility = Visibility.Collapsed;

                // set the countdown time in seconds
                CountDownStart = 5;
                LblStartTimer.Content = CountDownStart.ToString();
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

        private void StartNewTimer()
        {
            NewTimer = new DispatcherTimer();
            NewTimer.Interval = TimeSpan.FromSeconds(1); // every 1 second
            NewTimer.Tick += NewTimer_Tick;
            NewTimer.Start();
        }
        private void NewTimer_Tick(object sender, EventArgs e)
        {
            var uniformGrid = this.FindName("SplAnswers") as UniformGrid;

            CountDownNextQuestion--;
            if (CountDownNextQuestion == 0)
            {
                uniformGrid.IsEnabled = true;
                ShowQuestion();
                NewTimer.Stop();
            }

            CountDownScoreScreen--;
            if (CountDownScoreScreen == 0)
            {
                CountDownScoreScreen = -1;
                CurrentScreen = 3;
                ScreenCheck();
                NewTimer.Stop();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Safe to update UI here
            CountDownStart--;
            // show the countdown in the label
            LblStartTimer.Content = CountDownStart.ToString();

            if (GameStarted == true && RbtnInfTime.IsChecked == false) 
            { 
                CountDownEnd--;
                CountDownForQuestion--;
                PbrTimeLeft.Value = CountDownForQuestion;
            } 

            if (CountDownEnd <= 5 && GameStarted == true)
            {
                LblStartTimer.Content = CountDownEnd.ToString();
                LblStartTimer.Visibility = Visibility.Visible;
                Screen5.Visibility = Visibility.Visible;
            }

            if (CountDownStart == 0 && GameStarted == false)
            {
                // game starts now
                GameStarted = true;

                LblStartTimer.Visibility = Visibility.Collapsed;

                // go to the question screen
                CurrentScreen = 6;
                ScreenCheck();

                // get and show the questions
                GetQuestions();
                ShowQuestion();

                // time to answer the questions
                CountDownEnd = 60;
                CountDownForQuestion = 5;
            }
            else if (CountDownEnd == 0 && GameStarted == true)
            {
                // time to answer the question is up
                timer.Stop();
                UpdateScoreScreen();
            }
            else if (CountDownForQuestion <= 0 && GameStarted == true)
            {
                Score -= 3;
                CountDownForQuestion = 5;
                PbrTimeLeft.Value = CountDownForQuestion;

                ShowQuestion();       
            }
        }

        private void GetQuestions()
        {
            Question_ids.Clear();

            if (CurrentQuizId >= 0)
            {
                // get the quiz name from the database from the id in the button's Tag property
                var Theme = Query.ExecuteScalar("SELECT Theme FROM Themes WHERE Id = '" + CurrentQuizId + "' ;");
                lblname.Text = Theme.ToString();

                // get the question ids from the database from the quiz id
                DataTable DataQuestions = Query.GetDataTable("SELECT Id FROM Questions WHERE Theme_Id = '" + CurrentQuizId + "' ;");

                if (CurrentQuizId == 0)
                {
                    DataQuestions = Query.GetDataTable("SELECT Id FROM Questions;");
                }

                // go trough all the rows and add the question ids to the list
                foreach (DataRow rowQuestions in DataQuestions.Rows)
                {
                    Question_ids.Add(rowQuestions["Id"].ToString());
                }

                Question_ids = Question_ids.OrderBy(x => Guid.NewGuid()).ToList(); // shuffle the list
            }
        }

        private void ShowQuestion()
        {
            var Question = Query.ExecuteScalar("SELECT Question FROM Questions WHERE Id = '" + Question_ids[0] + "' ;");
            LblQuestion.Text = Question.ToString();

            DataTable DataAnswers_Id = Query.GetDataTable("SELECT Answer, Correct FROM Answers WHERE Question_Id = '" + Question_ids[0] + "' ORDER BY NEWID();");

            var uniformGrid = this.FindName("SplAnswers") as UniformGrid;
            uniformGrid.Children.Clear();

            foreach (DataRow RowAnswers in DataAnswers_Id.Rows)
            {
                var button = new Button
                {
                    Content = RowAnswers["Answer"].ToString(),
                    FontSize = 32,
                    Width = 350,
                    Height = 250,
                    Margin = new Thickness(10),
                    Tag = RowAnswers["Correct"].ToString(), // store if the answer is correct in the button's Tag property
                };
                // Look for the style in this element's resource lookup chain
                button.Style = (Style)this.FindResource("ModernButton");
                // to add click event to the button
                button.Click += BtnAnswer_Click;
                // add the button to the stack panel
                uniformGrid.Children.Add(button);
            }

            CountDownForQuestion = 5;

            Question_ids.RemoveAt(0);
        }

        private void BtnAnswer_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string isCorrect = button.Tag.ToString();

            var uniformGrid = this.FindName("SplAnswers") as UniformGrid;

            if (isCorrect == "True")
            {
                Score += 5;
                button.Background = Brushes.Green;
                button.IsEnabled = false;
                uniformGrid.IsEnabled = false;
            }
            else
            {
                if(RbtnDeath.IsChecked == false) Score -= 3;
                else
                {
                    Question_ids.Clear();
                }
                button.Background = Brushes.Red;
                button.IsEnabled = false;
                uniformGrid.IsEnabled = false;

            }

            if (Question_ids.Count > 0)
            {
                StartNewTimer();
                CountDownNextQuestion = 1;
                CountDownForQuestion = 6;
            }
            else
            {
                timer.Stop();
                GameStarted = false;

                UpdateScoreScreen();
            }
        }

        private void UpdateScoreScreen()
        {
            DataTable DataThemes = Query.GetDataTable("SELECT Theme, Account_Id FROM Themes WHERE Id = '" + CurrentQuizId + "' ;");

            foreach (DataRow RowThemes in DataThemes.Rows)
            {
                LblQuizNameScore.Text = RowThemes["Theme"].ToString();

                DataTable DataAcounts = Query.GetDataTable("SELECT Username FROM Accounts WHERE Id = '" + RowThemes["Account_Id"] + "' ;");

                foreach (DataRow RowAcounts in DataAcounts.Rows)
                {
                    if (RowAcounts["Username"] != null)
                    {
                        LblQuizCreatorScore.Text = "Created by: " + RowAcounts["Username"].ToString();
                    }
                }
            }

            LblQuestionScore.Text = Score.ToString();

            int FinalScore = Score;

            if (RbtnInfTime.IsChecked == false)
            {
                LblTimeScore.Text = CountDownEnd.ToString();
                FinalScore += CountDownEnd;
            }
            LblYourScore.Text = FinalScore.ToString();

            CountDownScoreScreen = 5;

            DataTable DataAcountsUsername = Query.GetDataTable("SELECT Username FROM Accounts WHERE Id = '" + PlayerId + "' ;");

            foreach (DataRow RowAcountUsername in DataAcountsUsername.Rows)
            {
                Query.ExecuteQueryNonQuery("INSERT INTO HighScores (Theme_Id, Username, Score) VALUES ('" + CurrentQuizId + "','" + RowAcountUsername["Username"] + "','" + FinalScore + "');");
            }  

            StartNewTimer();

            CurrentScreen = 7;
            ScreenCheck();
        }

        private void BtnSkip_Click(object sender, RoutedEventArgs e)
        {
            if(SkipAmount > 0)
            {
                if (Question_ids.Count == 0)
                {
                    // time to answer the question is up
                    timer.Stop();
                    UpdateScoreScreen();
                    return;
                }

                SkipAmount--;
                CountDownForQuestion = 5;

                ShowQuestion();

                if (SkipAmount == 0)
                {
                    BtnSkip.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void TbxQuizName_TextChanged(object sender, TextChangedEventArgs e)
        {
            LblPlaceholder.Visibility = string.IsNullOrEmpty(TbxQuizName.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void BtnStartCreate_Click(object sender, RoutedEventArgs e)
        {     
            if (TbxQuizName.Text != "")
            {
                var count = Query.ExecuteScalar("SELECT COUNT(*) FROM Themes WHERE Theme ='" + TbxQuizName.Text + "';");

                // if username exists, show message and return
                if (count.ToString() == "1" && EditingQuiz != true)
                {
                    MessageBox.Show("Theme already exists");
                    return;
                }
                else if (EditingQuiz == true)
                {
                    var Theme = Query.ExecuteScalar("SELECT Theme FROM Themes WHERE Id ='" + CurrentQuizId + "';");

                    var questions = Query.GetDataTable("SELECT Id FROM Questions WHERE Theme_Id = '" + CurrentQuizId + "' ;");
                    foreach (DataRow row in questions.Rows)
                    { 
                        Question_ids.Add(row["Id"].ToString());
                    }

                    ShowFilledin();

                    if (Theme.ToString() == TbxQuizName.Text.ToString())
                    {
                        lblname.Text = TbxQuizName.Text;

                        // update screen
                        CurrentScreen = 9;
                        ScreenCheck();
                        return;
                    }
                    else
                    {
                        Query.ExecuteQueryNonQuery("UPDATE Themes SET Theme = '" + TbxQuizName.Text + "' WHERE Id = '" + CurrentQuizId + "';");

                        lblname.Text = TbxQuizName.Text;
                        // update screen
                        CurrentScreen = 9;
                        ScreenCheck();
                        return;
                    }
                }

                Query.ExecuteQueryNonQuery("INSERT INTO Themes (Account_Id, Theme) VALUES ('" + PlayerId + "','" + TbxQuizName.Text + "');");

                lblname.Text = TbxQuizName.Text;
                // update screen
                CurrentScreen = 9;
                ScreenCheck();
            }
            else MessageBox.Show("Fill in the name please");
        }

        private void ShowFilledin()
        {
            if(Question_ids.Count >= 1)
            {
                var Question = Query.ExecuteScalar("SELECT Question FROM Questions WHERE Id = '" + Question_ids[0] + "' ;");
                TbxQuestionCreate.Text = Question.ToString();

                DataTable DataAnswers_Id = Query.GetDataTable("SELECT Answer, Correct FROM Answers WHERE Question_Id = '" + Question_ids[0] + "';");
                int i = 1;
                foreach (DataRow RowAnswers in DataAnswers_Id.Rows)
                {
                    var questionbox = this.FindName("TbxAnswer" + i) as TextBox;
                    var button = this.FindName("BtnAnswer" + i) as Button;
                    if (questionbox != null || button != null || questionbox.Text != "")
                    {
                        questionbox.Text = RowAnswers["Answer"].ToString();
                        bool Correct = Convert.ToBoolean(RowAnswers["Correct"]);
                        if (Correct == true)
                        {
                            button.Background = Brushes.Green;
                            button.Content = "CORRECT";
                        }
                        else
                        {
                            button.Background = Brushes.Red;
                            button.Content = "WRONG";
                        }
                    }
                    i++;
                }
            }
            else
            {
                EditingQuiz = false;
            }
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            TbxQuizName.Text = "";

            // update screen
            CurrentScreen = 8;
            ScreenCheck();
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            List<string> Awnser_ids = new List<string>();

            bool CorrectFilled = BtnAnswer1.Background == Brushes.Green || BtnAnswer2.Background == Brushes.Green || BtnAnswer3.Background == Brushes.Green || BtnAnswer4.Background == Brushes.Green;
            bool AllRequiredFilled = TbxAnswer1.Text != "" && TbxAnswer2.Text != "";
            bool RightFilled = (TbxAnswer3.Text == "" && BtnAnswer3.Background == Brushes.Green) || (TbxAnswer4.Text == "" && BtnAnswer4.Background == Brushes.Green);
            if (TbxQuestionCreate.Text != "")
            {
                if (CorrectFilled == true)
                {
                    if (AllRequiredFilled == true)
                    {
                        if (RightFilled == true)
                        {
                            MessageBox.Show("Please make sure that all the wrong answers are marked as wrong");
                        }
                        else
                        {
                            int currentQuestionsId = 0;
                            if (EditingQuiz == false)
                            {
                                var Theme = Query.ExecuteScalar("SELECT Id FROM Themes WHERE Theme = '" + lblname.Text + "' ;");
                                CurrentQuizId = Convert.ToInt32(Theme);

                                Query.ExecuteQueryNonQuery("INSERT INTO Questions (Theme_Id, Question) VALUES ('" + CurrentQuizId + "','" + TbxQuestionCreate.Text + "');");

                                var Questionid = Query.ExecuteScalar("SELECT Id FROM Questions WHERE Question = '" + TbxQuestionCreate.Text + "' ;");
                                currentQuestionsId = Convert.ToInt32(Questionid);
                            }
                            else if (EditingQuiz == true)
                            {
                                var LastQuestion = Query.ExecuteScalar("SELECT Question FROM Questions WHERE Id = '" + Question_ids[0] + "' ;");

                                if (LastQuestion.ToString() != TbxQuestionCreate.Text)
                                {
                                    Query.ExecuteQueryNonQuery("UPDATE Questions SET Question = '" + TbxQuizName.Text + "' WHERE Id = '" + Question_ids[0] + "';");
                                }

                                DataTable awnsers_id = Query.GetDataTable("SELECT Id FROM Answers WHERE Question_Id = '" + Question_ids[0] + "' ;");
                                foreach (DataRow row in awnsers_id.Rows)
                                {
                                    Awnser_ids.Add(row["Id"].ToString());
                                }

                                currentQuestionsId = Convert.ToInt32(Question_ids[0]);
                            }

                            if (currentQuestionsId != 0)
                            {
                                for (int i = 1; i < 5; i++)
                                {
                                    var questionbox = this.FindName("TbxAnswer" + i) as TextBox;
                                    var button = this.FindName("BtnAnswer" + i) as Button;

                                    if (questionbox != null && button != null)
                                    {
                                        if (questionbox.Text != "")
                                        {
                                            if (EditingQuiz == false || Awnser_ids.Count == 0)
                                            {
                                                bool Correct = button.Background == Brushes.Green;

                                                Query.ExecuteQueryNonQuery("INSERT INTO Answers (Question_Id, Answer, Correct) VALUES ('" + currentQuestionsId + "','" + questionbox.Text + "','" + Correct + "');");
                                            }
                                            else if (EditingQuiz == true && Awnser_ids.Count >= 1)
                                            {
                                                DataTable LastAnswer = Query.GetDataTable("SELECT Answer, Correct FROM Answers WHERE Id = '" + Awnser_ids[0] + "' ;");
                                                foreach (DataRow row in LastAnswer.Rows)
                                                {
                                                    if (row["Answer"].ToString() != questionbox.Text)
                                                    {
                                                        Query.ExecuteQueryNonQuery("UPDATE Answers SET Answer = '" + questionbox.Text + "' WHERE Id = '" + Awnser_ids[0] + "';");
                                                    }

                                                    if (row["Correct"].ToString() == "True" && button.Background == Brushes.Red)
                                                    {
                                                        Query.ExecuteQueryNonQuery("UPDATE Answers SET Correct = 'False' WHERE Id = '" + Awnser_ids[0] + "';");
                                                    }
                                                    else if (row["Correct"].ToString() == "False" && button.Background == Brushes.Green)
                                                    {
                                                        Query.ExecuteQueryNonQuery("UPDATE Answers SET Correct = 'True' WHERE Id = '" + Awnser_ids[0] + "';");
                                                    }
                                                }
                                                Awnser_ids.RemoveAt(0);
                                            }
                                        }
                                    }
                                }
                            }

                            NextQuestion();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please fill in all the non optional answer fields");
                    }
                }
                else
                {
                    MessageBox.Show("Please select a correct answer");
                }
            }
            else if (EditingQuiz == true)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to delete the question and the answers?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    Query.ExecuteQueryNonQuery("DELETE FROM Questions WHERE Id = '" + Question_ids[0] + "';");
                    Query.ExecuteQueryNonQuery("DELETE FROM Answers WHERE Question_Id = '" + Question_ids[0] + "';");

                    NextQuestion();
                }
                else
                {
                    MessageBox.Show("Please fill in the question box");
                }
            }
            else
            {
                MessageBox.Show("Please fill in the question box");
            }
        }

        private void NextQuestion()
        {
            if (EditingQuiz == true && Question_ids.Count >= 1) Question_ids.RemoveAt(0);

            ClearTextBox();
            ClearButtons();

            ShowFilledin();

            CurrentScreen = 9;
            ScreenCheck();
        }

        private void ClearTextBox()
        {
            TbxAnswer1.Text = "";
            TbxAnswer2.Text = "";
            TbxAnswer3.Text = "";
            TbxAnswer4.Text = "";
            TbxQuestionCreate.Text = "";
        }

        private void TbxQuestionCreate_TextChanged(object sender, TextChangedEventArgs e)
        {
            LblPlaceholderQuestion.Visibility = string.IsNullOrEmpty(TbxQuestionCreate.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void TbxAnswer1_TextChanged(object sender, TextChangedEventArgs e)
        {
            LblPlaceholderAnswer1.Visibility = string.IsNullOrEmpty(TbxAnswer1.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void TbxAnswer2_TextChanged(object sender, TextChangedEventArgs e)
        {
            LblPlaceholderAnswer2.Visibility = string.IsNullOrEmpty(TbxAnswer2.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void TbxAnswer3_TextChanged(object sender, TextChangedEventArgs e)
        {
            LblPlaceholderAnswer3.Visibility = string.IsNullOrEmpty(TbxAnswer3.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void TbxAnswer4_TextChanged(object sender, TextChangedEventArgs e)
        {
            LblPlaceholderAnswer4.Visibility = string.IsNullOrEmpty(TbxAnswer4.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void BtnAnswerCorect_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                ClearButtons();
                button.Background = Brushes.Green;
                button.Content = "CORRECT";
            }
        }

        private void ClearButtons()
        {
            BtnAnswer1.Background = Brushes.Red;
            BtnAnswer1.Content = "WRONG";
            BtnAnswer2.Background = Brushes.Red;
            BtnAnswer2.Content = "WRONG";
            BtnAnswer3.Background = Brushes.Red;
            BtnAnswer3.Content = "WRONG";
            BtnAnswer4.Background = Brushes.Red;
            BtnAnswer4.Content = "WRONG";
        }

        private void BtnEditQuiz_Click(object sender, RoutedEventArgs e)
        {
            var Theme = Query.ExecuteScalar("SELECT Theme FROM Themes WHERE Id = '" + CurrentQuizId + "' ;");

            TbxQuizName.Text = Theme.ToString();

            EditingQuiz = true;

            CurrentScreen = 8;
            ScreenCheck();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            switch (CurrentScreen)
            {
                case 3: // Game choice screen
                case 4: // View screen
                    CurrentScreen = 1; // go back to the main screen
                    break;

                case 5: // CountDown screen
                case 6: // Question screen
                    try
                    {
                        timer.Stop();
                        if (NewTimer != null) { NewTimer.Stop(); }
                        GameStarted = false;
                        CurrentScreen = 3; // go back to the game choice screen
                    }
                    catch { }
                    break;
                case 7: // Score screen
                case 8: // Create quiz screen
                case 9: // Create question screen
                case 10: // Accounts screen
                    CurrentScreen = 3; // go back to the game choice screen
                    break;
            }

            Question_ids.Clear();
            EditingQuiz = false;
            GameStarted = false;

            ScreenCheck();
        }

        private void BtnAccountDelete_Click(object sender, RoutedEventArgs e)
        {
            // find the button that was clicked
            Button button = sender as Button;
            int UserId = int.Parse(button.Tag.ToString());
            var result = MessageBox.Show(
                "Are you sure you want to delete this account? All quizzes and scores will be deleted as well.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );
            if (result == MessageBoxResult.Yes)
            {
                Query.ExecuteQueryNonQuery("DELETE FROM Accounts WHERE Id = '" + UserId + "';");
                Query.ExecuteQueryNonQuery("DELETE FROM Themes WHERE Account_Id = '" + UserId + "';");
                Query.ExecuteQueryNonQuery("DELETE FROM Questions WHERE Theme_Id NOT IN (SELECT Id FROM Themes);");
                Query.ExecuteQueryNonQuery("DELETE FROM Answers WHERE Question_Id NOT IN (SELECT Id FROM Questions);");
                Query.ExecuteQueryNonQuery("DELETE FROM HighScores WHERE Username NOT IN (SELECT Username FROM Accounts);");
                BtnAcounts_Click(null, null);
            }
        }

        private void BtnAcounts_Click(object sender, RoutedEventArgs e)
        {
            SplDelete.Children.Clear();
            SplUserId.Children.Clear();
            SplType.Children.Clear();
            SplUserName.Children.Clear();
            DataTable Users = Query.GetDataTable("SELECT Username, Id, Type FROM Accounts ;");
            foreach (DataRow Row in Users.Rows)
            {
                if(Row["Type"].ToString() != "Admin")
                {               
                    // create a button to view the quiz
                    var CreateButton = new Button
                    {
                        Content = "Delete",
                        FontSize = 24,
                        Height = 43,
                        Width = 100,
                        Tag = Row["Id"].ToString(),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(0, 0, 0, 10),
                    };

                    // Look for the style in this element's resource lookup chain
                    CreateButton.Style = (Style)this.FindResource("CloseButton");
                    // to add click event to the button
                    CreateButton.Click += BtnAccountDelete_Click;

                    SplDelete.Children.Add(CreateButton);
                }
                else
                {
                    // create a button to view the quiz
                    var CreateButton = new TextBlock
                    {
                        Height = 45,
                        Width = 75,
                        FontSize = 24,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Margin = new Thickness(0, 0, 0, 10),
                        Visibility = Visibility.Hidden
                    };

                    SplDelete.Children.Add(CreateButton);
                }

                SplUserId.Children.Add(new TextBlock
                {
                    Text = Row["Id"].ToString(),
                    FontSize = 32,
                    Margin = new Thickness(0, 0, 0, 10),
                    Foreground = Brushes.White
                });

                SplType.Children.Add(new TextBlock
                {
                    Text = Row["Type"].ToString(),
                    FontSize = 32,
                    Margin = new Thickness(0, 0, 0, 10),
                    Foreground = Brushes.White
                });

                SplUserName.Children.Add(new TextBlock
                {
                    Text = Row["Username"].ToString(),
                    FontSize = 32,
                    Margin = new Thickness(0, 0, 0, 10),
                    Foreground = Brushes.White
                });
            }

            CurrentScreen = 10;
            ScreenCheck();
        }

        private void BtnDeleteQuiz_Click(object sender, RoutedEventArgs e)
        {
            Query.ExecuteQueryNonQuery("DELETE FROM Themes WHERE Id = '" + CurrentQuizId + "';");
            Query.ExecuteQueryNonQuery("DELETE FROM Questions WHERE Theme_Id NOT IN (SELECT Id FROM Themes);");
            Query.ExecuteQueryNonQuery("DELETE FROM Answers WHERE Question_Id NOT IN (SELECT Id FROM Questions);");

            CurrentScreen = 3;
            ScreenCheck();
        }
    }
}