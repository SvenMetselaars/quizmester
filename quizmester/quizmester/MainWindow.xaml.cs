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

        /// <summary>
        /// the id of the  player
        /// </summary>
        int PlayerId = 0;
        /// <summary>
        /// the type of player
        /// user normal player
        /// admin has admin controlls
        /// </summary>
        string PlayerType = "User";

        /// <summary>
        /// amount of skips the player has
        /// </summary>
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
        
        /// <summary>
        /// if the player is editing the quiz this goes true
        /// </summary>
        bool EditingQuiz = false;

        /// <summary>
        /// if the game starts this goes true
        /// </summary>
        bool GameStarted = false;

        /// <summary>
        /// the score of the player
        /// </summary>
        int Score = 0;

        // timer for the countdown
        DispatcherTimer timer;
        DispatcherTimer NewTimer;

        // the current quiz id
        int CurrentQuizId = -1;

        /// <summary>
        /// the ids for the questions
        /// </summary>
        List<string> Question_ids = new List<string>();

        /// <summary>
        /// string to connect to database 
        /// </summary>
        string connectionString;

        // main class connection string
        ExecuteQuery Query = new ExecuteQuery();

        /// <summary>
        /// this happens when the game is started up
        /// </summary>
        public MainWindow()
        {
            // load in all the components
            InitializeComponent();

            // get the conection string
            connectionString = Query.connectionString;

            // update screen
            ScreenCheck();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // get the info that was filled in
            string L_Username = Tbx_Username_L.Text;
            string L_Password = Pbx_Password_L.Password;

            // SQL query with parameters
            var count = Query.ExecuteScalar("SELECT COUNT(*) FROM Accounts WHERE Username ='" + L_Username + "'AND Password ='" + L_Password + "';");

            // if there is 1 user found
            if (count.ToString() == "1")
            {
                // go to game choise screen
                CurrentScreen = 3;

                // get the id and type from the database
                DataTable Account = Query.GetDataTable("SELECT Id, Type FROM Accounts WHERE Username = '" + L_Username + "' ;");
                foreach (DataRow row in Account.Rows)
                {
                    // put the id in the variable
                    PlayerId = Convert.ToInt32(row["Id"]);
                    // put the type in variable
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
                            // change the screen 
                            CurrentScreen = 3;

                            // get the id and put it in variable
                            var Account = Query.ExecuteScalar("SELECT Id FROM Accounts WHERE Username = '" + L_Username + "' ;");
                            PlayerId = Convert.ToInt32(Account);

                            // fill in the info in login screen
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
                            // maximize scree
                            Main.WindowState = WindowState.Maximized;
                            StartupGrid.Visibility = Visibility.Collapsed;
                            MainGrid.Visibility = Visibility.Visible;
                            BtnBack.Visibility = Visibility.Visible;

                            // if the screen is
                            // 3 = game choise screen (Screen3)
                            // 4 = view quiz info screen (Screen4)
                            // show the quizes and change name
                            if (i >= 3 && i < 5)
                            {
                                lblname.Text = "Pathoot";
                                ShowQuizes();
                            } 

                            // let the acounts be changed
                            if (CurrentScreen == 3 && PlayerType == "Admin") BtnAcounts.Visibility = Visibility.Visible;
                            // hide the accounts
                            else BtnAcounts.Visibility = Visibility.Collapsed;
                        }
                        // if the screen is not the game choise screen, make the window normal and show the startup grid
                        else
                        {
                            // make the screen small again
                            Main.WindowState = WindowState.Normal;
                            StartupGrid.Visibility = Visibility.Visible;
                            MainGrid.Visibility = Visibility.Collapsed;
                            this.Height = 550; this.Width = 900;
                        }
                    }
                    // if the screen is not found the screen is hidden
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
                // make new borders
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

            // create the border
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
                    // greadiant (from color to color)
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
            // end the game
            this.Close();
        }

        private void BtnStartQuiz_Click(object sender, RoutedEventArgs e)
        {
            // if it is a quiz (-1 doesnt exist)
            if(CurrentQuizId >= 0)
            {
                // enable grid is the answer grid
                var uniformGrid = this.FindName("SplAnswers") as UniformGrid;
                uniformGrid.IsEnabled = true;

                // go to the CountDown screen
                CurrentScreen = 5;
                ScreenCheck();

                // amount of skips
                SkipAmount = 1;

                // reset the score
                Score = 0;

                // see the timer
                LblStartTimer.Visibility = Visibility.Visible;

                // check the gamemode and change gameplay acordingly
                if (RbtnInfTime.IsChecked == false) PbrTimeLeft.Visibility = Visibility.Visible;
                else PbrTimeLeft.Visibility = Visibility.Collapsed;

                // set the countdown time in seconds
                CountDownStart = 5;
                LblStartTimer.Content = CountDownStart.ToString();
                StartTimer();
            }
            // playing a - game (doesnt exist)
            else { MessageBox.Show("Something went wrong please try again"); CurrentScreen = 3; }
        }

        // start the timer
        private void StartTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // every 1 second
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        // start other timer
        private void StartNewTimer()
        {
            NewTimer = new DispatcherTimer();
            NewTimer.Interval = TimeSpan.FromSeconds(1); // every 1 second
            NewTimer.Tick += NewTimer_Tick;
            NewTimer.Start();
        }

        // every timer tick
        private void NewTimer_Tick(object sender, EventArgs e)
        {
            // find the awnsers grid
            var uniformGrid = this.FindName("SplAnswers") as UniformGrid;

            // 1 sec delay
            CountDownNextQuestion--;
            if (CountDownNextQuestion == 0)
            {
                uniformGrid.IsEnabled = true;
                ShowQuestion();
                NewTimer.Stop();
            }

            // time of score screen on screen
            CountDownScoreScreen--;
            if (CountDownScoreScreen == 0)
            {
                CountDownScoreScreen = -1;
                CurrentScreen = 3;
                ScreenCheck();
                NewTimer.Stop();
            }
        }

        //  every second timer ticks
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Safe to update UI here
            CountDownStart--;
            // show the countdown in the label
            LblStartTimer.Content = CountDownStart.ToString();

            // if game started and there is no inf time than timer go down and change visuals
            if (GameStarted == true && RbtnInfTime.IsChecked == false) 
            { 
                CountDownEnd--;
                CountDownForQuestion--;
                PbrTimeLeft.Value = CountDownForQuestion;
            } 

            // if the total time is almost done and the game has started than show the final countdown
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
            //end the question and go to next one
            else if (CountDownForQuestion <= 0 && GameStarted == true)
            {
                Score -= 3;
                CountDownForQuestion = 5;
                PbrTimeLeft.Value = CountDownForQuestion;

                ShowQuestion();       
            }
        }

        /// <summary>
        /// get all the quesstion ids from the database for the current quiz
        /// </summary>
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

        /// <summary>
        /// show the questions and answers on the screen
        /// </summary>
        private void ShowQuestion()
        {
            // get the question from the database from the question id
            var Question = Query.ExecuteScalar("SELECT Question FROM Questions WHERE Id = '" + Question_ids[0] + "' ;");
            LblQuestion.Text = Question.ToString();

            // get the answers from the database from the question id
            DataTable DataAnswers_Id = Query.GetDataTable("SELECT Answer, Correct FROM Answers WHERE Question_Id = '" + Question_ids[0] + "' ORDER BY NEWID();");

            // get  the grid
            var uniformGrid = this.FindName("SplAnswers") as UniformGrid;
            uniformGrid.Children.Clear();

            // go trough all the rows and add the answers to the buttons
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

            // get the button info
            Button button = sender as Button;
            string isCorrect = button.Tag.ToString();

            // get the grid
            var uniformGrid = this.FindName("SplAnswers") as UniformGrid;

            // check if the answer is correct
            if (isCorrect == "True")
            {
                // change scrore and visuals
                Score += 5;
                button.Background = Brushes.Green;
                button.IsEnabled = false;
                uniformGrid.IsEnabled = false;
            }
            // if wrong answer
            else
            {
                // change score and visuals
                if (RbtnDeath.IsChecked == false) Score -= 3;
                // get rid of all the questions if death mode is on
                else
                {
                    Question_ids.Clear();
                }
                button.Background = Brushes.Red;
                button.IsEnabled = false;
                uniformGrid.IsEnabled = false;

            }

            // if there are more questions, go to next question
            if (Question_ids.Count > 0)
            {
                StartNewTimer();
                CountDownNextQuestion = 1;
                CountDownForQuestion = 6;
            }
            // if no more questions, go to score screen
            else
            {
                timer.Stop();
                GameStarted = false;

                UpdateScoreScreen();
            }
        }

        /// <summary>
        /// scorescreen update
        /// </summary>
        private void UpdateScoreScreen()
        {
            // get the quiz info
            DataTable DataThemes = Query.GetDataTable("SELECT Theme, Account_Id FROM Themes WHERE Id = '" + CurrentQuizId + "' ;");

            // go trough all the rows (should only be one)
            foreach (DataRow RowThemes in DataThemes.Rows)
            {
                // show the quiz name
                LblQuizNameScore.Text = RowThemes["Theme"].ToString();

                // get the quiz creator
                DataTable DataAcounts = Query.GetDataTable("SELECT Username FROM Accounts WHERE Id = '" + RowThemes["Account_Id"] + "' ;");

                // go trough all the rows (should only be one)
                foreach (DataRow RowAcounts in DataAcounts.Rows)
                {
                    // set the label to the quiz creator
                    if (RowAcounts["Username"] != null)
                    {
                        LblQuizCreatorScore.Text = "Created by: " + RowAcounts["Username"].ToString();
                    }
                }
            }

            // show the score for the questions
            LblQuestionScore.Text = Score.ToString();

            int FinalScore = Score;

            // if the time mode is not infinite, add the time score
            if (RbtnInfTime.IsChecked == false)
            {
                LblTimeScore.Text = CountDownEnd.ToString();
                FinalScore += CountDownEnd;
            }
            // final score show
            LblYourScore.Text = FinalScore.ToString();

            // show the screen for 5 seconds
            CountDownScoreScreen = 5;

            // get the username from the database
            DataTable DataAcountsUsername = Query.GetDataTable("SELECT Username FROM Accounts WHERE Id = '" + PlayerId + "' ;");

            // save the high score in the database
            foreach (DataRow RowAcountUsername in DataAcountsUsername.Rows)
            {
                Query.ExecuteQueryNonQuery("INSERT INTO HighScores (Theme_Id, Username, Score) VALUES ('" + CurrentQuizId + "','" + RowAcountUsername["Username"] + "','" + FinalScore + "');");
            }

            // start score screen timer
            StartNewTimer();

            // change the screen
            CurrentScreen = 7;
            ScreenCheck();
        }

        private void BtnSkip_Click(object sender, RoutedEventArgs e)
        {
            // if skips are left
            if (SkipAmount > 0)
            {
                // if no more questions, go to score screen
                if (Question_ids.Count == 0)
                {
                    // time to answer the question is up
                    timer.Stop();
                    UpdateScoreScreen();
                    return;
                }

                // use a skip
                SkipAmount--;
                // reset the timer for the question
                CountDownForQuestion = 5;

                // show next question
                ShowQuestion();

                // if last skip used, hide the skip button
                if (SkipAmount == 0)
                {
                    BtnSkip.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void TbxQuizName_TextChanged(object sender, TextChangedEventArgs e)
        {
            // the placeholder visibility is determend by the text box being empty or not
            LblPlaceholder.Visibility = string.IsNullOrEmpty(TbxQuizName.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void BtnStartCreate_Click(object sender, RoutedEventArgs e)
        {
            // check if the quiz name is filled in
            if (TbxQuizName.Text != "")
            {
                // check if the quiz name already exists
                var count = Query.ExecuteScalar("SELECT COUNT(*) FROM Themes WHERE Theme ='" + TbxQuizName.Text + "';");

                // if username exists, show message and return
                if (count.ToString() == "1" && EditingQuiz != true)
                {
                    MessageBox.Show("Theme already exists");
                    return;
                }
                // if not exists, but you are editing the quiz
                else if (EditingQuiz == true)
                {
                    // get the theme name
                    var Theme = Query.ExecuteScalar("SELECT Theme FROM Themes WHERE Id ='" + CurrentQuizId + "';");

                    // get all question ids (clear it to be sure)
                    Question_ids.Clear();
                    var questions = Query.GetDataTable("SELECT Id FROM Questions WHERE Theme_Id = '" + CurrentQuizId + "' ;");
                    foreach (DataRow row in questions.Rows)
                    { 
                        Question_ids.Add(row["Id"].ToString());
                    }

                    // show all the awnsers and questions that where already filledd in
                    ShowFilledin();

                    // if the name is the same as before, go back to edit screen
                    if (Theme.ToString() == TbxQuizName.Text.ToString())
                    {
                        // just update the label
                        lblname.Text = TbxQuizName.Text;

                        // update screen
                        CurrentScreen = 9;
                        ScreenCheck();
                        return;
                    }
                    // if the name is different, update it in the database
                    else
                    {
                        // update the name in the database
                        Query.ExecuteQueryNonQuery("UPDATE Themes SET Theme = '" + TbxQuizName.Text + "' WHERE Id = '" + CurrentQuizId + "';");

                        // just update the label
                        lblname.Text = TbxQuizName.Text;

                        // update screen
                        CurrentScreen = 9;
                        ScreenCheck();
                        return;
                    }
                }

                // insert the new quiz into the database
                Query.ExecuteQueryNonQuery("INSERT INTO Themes (Account_Id, Theme) VALUES ('" + PlayerId + "','" + TbxQuizName.Text + "');");

                // just update the label
                lblname.Text = TbxQuizName.Text;

                // update screen
                CurrentScreen = 9;
                ScreenCheck();
            }
            // if name not filled in, show message
            else MessageBox.Show("Fill in the name please");
        }

        /// <summary>
        /// show the filled in questions and awnsers when editing a quiz
        /// </summary>
        private void ShowFilledin()
        {
            // if there where already questions
            if (Question_ids.Count >= 1)
            {
                // get the question from the database from the question id
                var Question = Query.ExecuteScalar("SELECT Question FROM Questions WHERE Id = '" + Question_ids[0] + "' ;");
                // show the question in the text box
                TbxQuestionCreate.Text = Question.ToString();

                // get the answers from the database from the question id
                DataTable DataAnswers_Id = Query.GetDataTable("SELECT Answer, Correct FROM Answers WHERE Question_Id = '" + Question_ids[0] + "';");
                int i = 1;
                // go trough all the rows and add the answers to the buttons
                foreach (DataRow RowAnswers in DataAnswers_Id.Rows)
                {
                    // get the text box and button for the answer
                    var questionbox = this.FindName("TbxAnswer" + i) as TextBox;
                    var button = this.FindName("BtnAnswer" + i) as Button;
                    // if they exist and the text box is not empty
                    if (questionbox != null || button != null || questionbox.Text != "")
                    {
                        // change the text
                        questionbox.Text = RowAnswers["Answer"].ToString();
                        bool Correct = Convert.ToBoolean(RowAnswers["Correct"]);
                        // if it was the correct answer, change the button visuals
                        if (Correct == true)
                        {
                            button.Background = Brushes.Green;
                            button.Content = "CORRECT";
                        }
                        // if not correct, change the button visuals
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
            // clear quiz name text box
            TbxQuizName.Text = "";

            // update screen
            CurrentScreen = 8;
            ScreenCheck();
        }

        /// <summary>
        /// when the next button is clicked, 
        /// it will save the question and answers to the database, 
        /// and go to the next question
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            
            List<string> Awnser_ids = new List<string>();

            // check if all required fields are filled in
            bool CorrectFilled = BtnAnswer1.Background == Brushes.Green || BtnAnswer2.Background == Brushes.Green || BtnAnswer3.Background == Brushes.Green || BtnAnswer4.Background == Brushes.Green;
            bool AllRequiredFilled = TbxAnswer1.Text != "" && TbxAnswer2.Text != "";
            bool RightFilled = (TbxAnswer3.Text == "" && BtnAnswer3.Background == Brushes.Green) || (TbxAnswer4.Text == "" && BtnAnswer4.Background == Brushes.Green);
            // if question is filled in
            if (TbxQuestionCreate.Text != "")
            {
                // if the correct awnser is filled in
                if (CorrectFilled == true)
                {
                    // if all required awnsers are filled in
                    if (AllRequiredFilled == true)
                    {
                        // if no wrong empty are marked as correct
                        if (RightFilled == true)
                        {
                            MessageBox.Show("Please make sure that all the wrong answers are marked as wrong");
                        }
                        // if everything is filled in correctly, save to database
                        else
                        {
                            // reset id
                            int currentQuestionsId = 0;
                            // if not editing quiz, insert new question
                            if (EditingQuiz == false)
                            {
                                // get the theme id
                                var Theme = Query.ExecuteScalar("SELECT Id FROM Themes WHERE Theme = '" + lblname.Text + "' ;");
                                // put it in the current quiz id
                                CurrentQuizId = Convert.ToInt32(Theme);

                                // insert the question into the database
                                Query.ExecuteQueryNonQuery("INSERT INTO Questions (Theme_Id, Question) VALUES ('" + CurrentQuizId + "','" + TbxQuestionCreate.Text + "');");

                                // get the question id
                                var Questionid = Query.ExecuteScalar("SELECT Id FROM Questions WHERE Question = '" + TbxQuestionCreate.Text + "' ;");
                                // put it in the current question id
                                currentQuestionsId = Convert.ToInt32(Questionid);
                            }
                            // if they are editing the quiz, update the question
                            else if (EditingQuiz == true)
                            {
                                // get the question to see if it has changed
                                var LastQuestion = Query.ExecuteScalar("SELECT Question FROM Questions WHERE Id = '" + Question_ids[0] + "' ;");

                                // check if the question has changed, if so update it
                                if (LastQuestion.ToString() != TbxQuestionCreate.Text)
                                {
                                    Query.ExecuteQueryNonQuery("UPDATE Questions SET Question = '" + TbxQuizName.Text + "' WHERE Id = '" + Question_ids[0] + "';");
                                }

                                // get all the awnser ids for this question
                                DataTable awnsers_id = Query.GetDataTable("SELECT Id FROM Answers WHERE Question_Id = '" + Question_ids[0] + "' ;");
                                foreach (DataRow row in awnsers_id.Rows)
                                {
                                    // put the ids in the list
                                    Awnser_ids.Add(row["Id"].ToString());
                                }
                                // put the current question id in the variable
                                currentQuestionsId = Convert.ToInt32(Question_ids[0]);
                            }

                            // if the there is a question
                            if (currentQuestionsId != 0)
                            {
                                // go trough it 4 times for the 4 awnsers boxes
                                for (int i = 1; i < 5; i++)
                                {
                                    // get the info from the text box and button
                                    var questionbox = this.FindName("TbxAnswer" + i) as TextBox;
                                    var button = this.FindName("BtnAnswer" + i) as Button;

                                    // if they exist
                                    if (questionbox != null && button != null)
                                    {
                                        // if the text box is not empty
                                        if (questionbox.Text != "")
                                        {
                                            // if not editing quiz or there is no awsner id, insert new awnser
                                            if (EditingQuiz == false || Awnser_ids.Count == 0)
                                            {
                                                // determine if the awnser is correct (green is true else is false)
                                                bool Correct = button.Background == Brushes.Green;

                                                // insert the awnser into the database
                                                Query.ExecuteQueryNonQuery("INSERT INTO Answers (Question_Id, Answer, Correct) VALUES ('" + currentQuestionsId + "','" + questionbox.Text + "','" + Correct + "');");
                                            }
                                            // if editing quiz and there is an awnser id, update the awnser
                                            else if (EditingQuiz == true && Awnser_ids.Count >= 1)
                                            {
                                                // get the last awnser from the database
                                                DataTable LastAnswer = Query.GetDataTable("SELECT Answer, Correct FROM Answers WHERE Id = '" + Awnser_ids[0] + "' ;");
                                                // go trough all the rows (should only be one)
                                                foreach (DataRow row in LastAnswer.Rows)
                                                {
                                                    // check if the awnser text has changed, if so update it    
                                                    if (row["Answer"].ToString() != questionbox.Text)
                                                    {
                                                        Query.ExecuteQueryNonQuery("UPDATE Answers SET Answer = '" + questionbox.Text + "' WHERE Id = '" + Awnser_ids[0] + "';");
                                                    }

                                                    // check if the correct awnser has changed, if so update it to true
                                                    if (row["Correct"].ToString() == "True" && button.Background == Brushes.Red)
                                                    {
                                                        Query.ExecuteQueryNonQuery("UPDATE Answers SET Correct = 'False' WHERE Id = '" + Awnser_ids[0] + "';");
                                                    }
                                                    // check if the correct awnser has changed, if so update it to false
                                                    else if (row["Correct"].ToString() == "False" && button.Background == Brushes.Green)
                                                    {
                                                        Query.ExecuteQueryNonQuery("UPDATE Answers SET Correct = 'True' WHERE Id = '" + Awnser_ids[0] + "';");
                                                    }
                                                }
                                                // get rid of the awnser id because it is already used
                                                Awnser_ids.RemoveAt(0);
                                            }
                                        }
                                    }
                                }
                            }

                            // go to the next question
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
            // if the question box is not filled and the user is editing the quiz, delete the question and awnsers
            else if (EditingQuiz == true)
            {
                // first show a confirmation message box
                var result = MessageBox.Show(
                    "Are you sure you want to delete the question and the answers?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                // delete the question and awnsers if the user confirmed
                if (result == MessageBoxResult.Yes)
                {
                    Query.ExecuteQueryNonQuery("DELETE FROM Questions WHERE Id = '" + Question_ids[0] + "';");
                    Query.ExecuteQueryNonQuery("DELETE FROM Answers WHERE Question_Id = '" + Question_ids[0] + "';");

                    // go to the next question
                    NextQuestion();
                }
                // if the user did not confirm, say they have to fill in the question box
                else
                {
                    MessageBox.Show("Please fill in the question box");
                }
            }
            // if the user did not fill in the question box, show message
            else
            {
                MessageBox.Show("Please fill in the question box");
            }
        }

        /// <summary>
        /// get the next question or go back to the edit screen
        /// </summary>
        private void NextQuestion()
        {
            // get rid of the current question id from the list if editing quiz
            if (EditingQuiz == true && Question_ids.Count >= 1) Question_ids.RemoveAt(0);

            // clear all the boxes
            ClearTextBox();
            ClearButtons();

            // show the filled in question and awnsers if there are more questions
            ShowFilledin();

            // change the screen
            CurrentScreen = 9;
            ScreenCheck();
        }

        /// <summary>
        /// clear all the text boxes
        /// </summary>
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
            // the placeholder visibility is determend by the text box being empty or not
            LblPlaceholderQuestion.Visibility = string.IsNullOrEmpty(TbxQuestionCreate.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void TbxAnswer1_TextChanged(object sender, TextChangedEventArgs e)
        {
            // the placeholder visibility is determend by the text box being empty or not
            LblPlaceholderAnswer1.Visibility = string.IsNullOrEmpty(TbxAnswer1.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void TbxAnswer2_TextChanged(object sender, TextChangedEventArgs e)
        {
            // the placeholder visibility is determend by the text box being empty or not
            LblPlaceholderAnswer2.Visibility = string.IsNullOrEmpty(TbxAnswer2.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void TbxAnswer3_TextChanged(object sender, TextChangedEventArgs e)
        {
            // the placeholder visibility is determend by the text box being empty or not
            LblPlaceholderAnswer3.Visibility = string.IsNullOrEmpty(TbxAnswer3.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void TbxAnswer4_TextChanged(object sender, TextChangedEventArgs e)
        {
            // the placeholder visibility is determend by the text box being empty or not
            LblPlaceholderAnswer4.Visibility = string.IsNullOrEmpty(TbxAnswer4.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void BtnAnswerCorect_Click(object sender, RoutedEventArgs e)
        {
            // find the button that was clicked
            Button button = sender as Button;
            if (button != null)
            {
                // clear all the buttons (to red and wrong)
                ClearButtons();
                // change the clicked button to green and correct
                button.Background = Brushes.Green;
                button.Content = "CORRECT";
            }
        }

        /// <summary>
        /// clear all the answer buttons to red and wrong
        /// </summary>
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
            // get the quiz name from the database from the id in the button's Tag property
            var Theme = Query.ExecuteScalar("SELECT Theme FROM Themes WHERE Id = '" + CurrentQuizId + "' ;");

            // get the quiz name and put it in the text box
            TbxQuizName.Text = Theme.ToString();

            // set editing quiz to true
            EditingQuiz = true;

            // change the screen to create quiz start screen
            CurrentScreen = 8;
            ScreenCheck();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            // go back to the previous screen depending on the current screen
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

            // get rid of all questions
            Question_ids.Clear();
            // set to false
            EditingQuiz = false;
            GameStarted = false;

            // change the screen
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
                // delete the account and all related data from the database
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
            // clear all the stack panels
            SplDelete.Children.Clear();
            SplUserId.Children.Clear();
            SplType.Children.Clear();
            SplUserName.Children.Clear();
            // get info from the database
            DataTable Users = Query.GetDataTable("SELECT Username, Id, Type FROM Accounts ;");
            // go trough all the rows and add the info to the stack panels
            foreach (DataRow Row in Users.Rows)
            {
                // if the user is not an admin, show the delete button
                if (Row["Type"].ToString() != "Admin")
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

                    // add the button
                    SplDelete.Children.Add(CreateButton);
                }
                // if it is for an admin, hide the delete button
                else
                {
                    // create a button to delete
                    // still create the button to keep the layout consistent
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

                    // add the button to the stack panel
                    SplDelete.Children.Add(CreateButton);
                }

                // add the user info to the stack panels
                SplUserId.Children.Add(new TextBlock
                {
                    Text = Row["Id"].ToString(),
                    FontSize = 32,
                    Margin = new Thickness(0, 0, 0, 10),
                    Foreground = Brushes.White
                });

                // add the user type to the stack panel
                SplType.Children.Add(new TextBlock
                {
                    Text = Row["Type"].ToString(),
                    FontSize = 32,
                    Margin = new Thickness(0, 0, 0, 10),
                    Foreground = Brushes.White
                });

                // add the username to the stack panel
                SplUserName.Children.Add(new TextBlock
                {
                    Text = Row["Username"].ToString(),
                    FontSize = 32,
                    Margin = new Thickness(0, 0, 0, 10),
                    Foreground = Brushes.White
                });
            }

            // change the screen
            CurrentScreen = 10;
            ScreenCheck();
        }

        private void BtnDeleteQuiz_Click(object sender, RoutedEventArgs e)
        {
            // delete the quiz and all related data from the database
            Query.ExecuteQueryNonQuery("DELETE FROM Themes WHERE Id = '" + CurrentQuizId + "';");
            Query.ExecuteQueryNonQuery("DELETE FROM Questions WHERE Theme_Id NOT IN (SELECT Id FROM Themes);");
            Query.ExecuteQueryNonQuery("DELETE FROM Answers WHERE Question_Id NOT IN (SELECT Id FROM Questions);");

            // change the screen back to game choice
            CurrentScreen = 3;
            ScreenCheck();
        }
    }
}