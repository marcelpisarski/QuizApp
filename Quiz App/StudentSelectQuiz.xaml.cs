using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Quiz_App.Classes;
using static Quiz_App.MainWindow;
using static Quiz_App.QuizSelectorEdit;

namespace Quiz_App
{
    /// <summary>
    /// Interaction logic for StudentSelectQuiz.xaml
    /// </summary>
    public partial class StudentSelectQuiz : Window
    {
        private string actionType;
        private bool randomiseQuestions;
        
        public StudentSelectQuiz(string actionType, bool randomiseQuestions)
        {
            //Casts flashcard options to variables
            this.actionType = actionType;
            this.randomiseQuestions = randomiseQuestions;

            InitializeComponent();

            // Load quizzes based on the action type
            if (actionType == "PreviousQuizzes")
            {
                LoadCompletedQuizzes();
            }
            else
            {
                LoadUncompletedQuizzes();
            }
        }

        //Closes window
        private void btnCloseStudentSelectQuiz_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSelectQuiz_Click(object sender, RoutedEventArgs e)
        {
            if (dtgQuizzes.SelectedItem is Quiz selectedQuiz)
            {
                switch (actionType)
                {
                    case "PreviousQuizzes":
                        //Proceed to open flashcard window with selected quiz id
                        StudentFlashcardWindow studentFlashcardWindow = new StudentFlashcardWindow("PreviousQuizzes", randomiseQuestions, selectedQuiz.Id);
                        studentFlashcardWindow.Show();
                        break;
                    case "StartQuiz":
                        //Proceed to start the selected quiz
                        StudentQuizWindow studentQuizWindow = new StudentQuizWindow(selectedQuiz.Id);
                        studentQuizWindow.Show();
                        break;
                    default:
                        return;
                }              
                this.Close();
            }
            else
            {
                MessageBox.Show("Please select a quiz to start");
            }
        }

        //Load the users uncompleted quizzes into list
        private void LoadUncompletedQuizzes()
        {
            int userId = GlobalVariables.UserId;

            var uncompletedQuizzes = GetUncompletedQuizzes(userId);
            dtgQuizzes.ItemsSource = uncompletedQuizzes;
        }

        //Get the users uncompleted quizzes from database
        private List<Quiz> GetUncompletedQuizzes(int userId)
        {
            var uncompletedQuizzes = new List<Quiz>();

            string connectionString = GlobalVariables.Connection;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    //SQL query to fetch user's level
                    command.CommandText = "SELECT level FROM userbaseline WHERE userid = @userId";
                    command.Parameters.AddWithValue("@userId", userId);
                    int userLevel = Convert.ToInt32(command.ExecuteScalar());

                    //Finds the userlevel and places inside levelColumn string
                    string levelColumn = userLevel switch
                    {
                        1 => "level1",
                        2 => "level2",
                        3 => "level3",
                        4 => "level4",
                        _ => throw new NotImplementedException(),
                    };

                    //SQL query to fetch quizzes not completed by the user
                    command.CommandText = @$"SELECT quiz.quizid, quiz.topic, quiz.title 
                                            FROM quiz LEFT JOIN userquizcompletions ON quiz.quizid = userquizcompletions.quizid AND userquizcompletions.userid = @userId 
                                            WHERE userquizcompletions.quizid IS NULL AND quiz.{levelColumn} = 1";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var quiz = new Quiz
                            {
                                Id = reader.GetInt32("quizid"),
                                Topic = reader.GetString("topic"),
                                Title = reader.GetString("title")
                            };
                            uncompletedQuizzes.Add(quiz);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error fetching uncompleted quizzes: {ex.Message}");
                }
            }

            return uncompletedQuizzes;
        }

        //Loads the quizzes which the user has completed into table
        private void LoadCompletedQuizzes()
        {
            int userId = GlobalVariables.UserId;

            var completedQuizzes = GetCompletedQuizzes(userId);
            dtgQuizzes.ItemsSource = completedQuizzes;
        }

        //Get the user's completed quizzes from the database
        private List<Quiz> GetCompletedQuizzes(int userId)
        {
            var completedQuizzes = new List<Quiz>();

            string connectionString = GlobalVariables.Connection;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    //SQL query to fetch quizzes completed by the user
                    command.CommandText = @"SELECT quiz.quizid, quiz.topic, quiz.title 
                                            FROM quiz 
                                            JOIN userquizcompletions ON quiz.quizid = userquizcompletions.quizid 
                                            WHERE userquizcompletions.userid = @userId";
                    command.Parameters.AddWithValue("@userId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var quiz = new Quiz
                            {
                                Id = reader.GetInt32("quizid"),
                                Topic = reader.GetString("topic"),
                                Title = reader.GetString("title")
                            };
                            completedQuizzes.Add(quiz);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error fetching completed quizzes: {ex.Message}");
                }
            }
            return completedQuizzes;
        }
    }
}
