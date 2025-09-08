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
        /// default screen is login screen
        /// </summary>
        int CurrentScreen = 1;
        // the max screen number... change this if you add more screens
        int ScreenMax = 9;

        int PlayerId = 0;

        int SkipAmount = 3;

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

        // this is the time you have to wait on the score screen in seconds
        int CountDownScoreScreen = -1;

        bool GameStarted = false;

        int Score = 0;

        // timer for the countdown
        DispatcherTimer timer;
        DispatcherTimer NewTimer;

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

                DataTable DataAcounts = Query.GetDataTable("SELECT Id FROM Accounts WHERE Username = '" + L_Username + "' ;");

                foreach (DataRow RowAcounts in DataAcounts.Rows)
                {
                    PlayerId = Convert.ToInt32(RowAcounts["Id"]);
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
                        {
                            CurrentScreen = 3;

                            DataTable DataAcounts = Query.GetDataTable("SELECT Id FROM Accounts WHERE Username = '" + L_Username + "' ;");

                            foreach (DataRow RowAcounts in DataAcounts.Rows)
                            {
                                PlayerId = Convert.ToInt32(RowAcounts["Id"]);
                            }
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

                            if (PlayerId == Convert.ToInt32(rowThemes["Account_Id"]))
                            {
                                SplButtons.Width = 600;
                                BtnEditQuiz.Visibility = Visibility.Visible;
                            }
                        }
                    }

                    // get the username from the database from the account id in the quiz
                    DataTable DataAcountsAdmin = Query.GetDataTable("SELECT Type FROM Accounts WHERE Id = '" + rowThemes["Account_Id"] + "' ;");

                    // go trough all the rows (should only be one)
                    foreach (DataRow rowAcounts in DataAcountsAdmin.Rows)
                    {
                        if (rowAcounts["Type"].ToString() == "Admin")
                        {
                            SplButtons.Width = 600;
                            BtnEditQuiz.Visibility = Visibility.Visible;
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
                var uniformGrid = this.FindName("SplAnswers") as UniformGrid;
                uniformGrid.IsEnabled = true;

                // go to the CountDown screen
                CurrentScreen = 5;
                ScreenCheck();

                SkipAmount = 3;

                // reset the score
                Score = 0;

                LblStartTimer.Visibility = Visibility.Visible;

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

            if (GameStarted == true) 
            { 
                CountDownEnd--; 
                PbrTimeLeft.Value = CountDownEnd;
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
            }
            else if (CountDownEnd == 0 && GameStarted == true)
            {
                // time to answer the question is up
                timer.Stop();
                UpdateScoreScreen();
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

        private void ShowQuestion()
        {
            DataTable DataQuestions = Query.GetDataTable("SELECT Question FROM Questions WHERE Id = '" + Question_ids[0] + "' ;");
            DataTable DataAnswers_Id = Query.GetDataTable("SELECT Answer, Correct FROM Answers WHERE Question_Id = '" + Question_ids[0] + "' ORDER BY NEWID();");

            foreach (DataRow RowQuestions in DataQuestions.Rows)
            {
                LblQuestion.Text = RowQuestions["Question"].ToString();
            }

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
                Score -= 3;
                button.Background = Brushes.Red;
                button.IsEnabled = false;
                uniformGrid.IsEnabled = false;

            }

            if (Question_ids.Count > 0)
            {
                StartNewTimer();
                CountDownNextQuestion = 1;
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

            LblTimeScore.Text = CountDownEnd.ToString();

            int FinalScore = Score + CountDownEnd;
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
                SkipAmount--;

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
                Query.ExecuteQueryNonQuery("INSERT INTO Themes (Account_Id, Theme) VALUES ('" + PlayerId + "','" + TbxQuizName.Text + "');");

                lblname.Text = TbxQuizName.Text;
                // update screen
                CurrentScreen = 9;
                ScreenCheck();
            }
            else MessageBox.Show("Fill in the name please");
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
           // update screen
            CurrentScreen = 8;
            ScreenCheck();
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            bool CorrectFilled = BtnAnswer1.Background == Brushes.Green || BtnAnswer2.Background == Brushes.Green || BtnAnswer3.Background == Brushes.Green || BtnAnswer4.Background == Brushes.Green;
            bool AllRequiredFilled = TbxQuestionCreate.Text != "" && TbxAnswer1.Text != "" && TbxAnswer2.Text != "";
            bool RightFilled = (TbxAnswer3.Text == "" && BtnAnswer3.Background == Brushes.Green) || (TbxAnswer4.Text == "" && BtnAnswer4.Background == Brushes.Green);
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

                        DataTable DataThemes = Query.GetDataTable("SELECT Id FROM Themes WHERE Theme = '" + lblname.Text + "' ;");

                        foreach (DataRow RowThemes in DataThemes.Rows)
                        {
                            CurrentQuizId = Convert.ToInt32(RowThemes["Id"]);
                        }

                        Query.ExecuteQueryNonQuery("INSERT INTO Questions (Theme_Id, Question) VALUES ('" + CurrentQuizId + "','" + TbxQuestionCreate.Text + "');");

                        DataTable DataQuestions = Query.GetDataTable("SELECT Id FROM Themes WHERE Theme = '" + lblname.Text + "' ;");

                        var currentQuestionsId = 0;
                        foreach (DataRow RowQuestions in DataQuestions.Rows)
                        {
                            currentQuestionsId = Convert.ToInt32(RowQuestions["Id"]);
                        }

                        if (currentQuestionsId != 0)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                var questionbox = this.FindName("TbxAnswer" + i) as TextBox;
                                var button = this.FindName("BtnAnswer" + i) as Button;

                                if (questionbox != null || button != null || questionbox.Text != "")
                                {
                                    bool Correct = button.Background == Brushes.Green;

                                    Query.ExecuteQueryNonQuery("INSERT INTO Answers (Question_Id, Answer, Correct) VALUES ('" + currentQuestionsId + "','" + questionbox.Text + "','" + Correct + "');");
                                }
                            }
                        }


                        ClearTextBox();
                        ClearButtons();

                        CurrentScreen = 9;
                        ScreenCheck();
                    }
                }
                else
                {
                    MessageBox.Show("Please fill in all the non optional fields");
                }
            }
            else
            {
                MessageBox.Show("Please select a correct answer");
            }
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

        }
    }
}