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
using static Quiz_App.MainWindow;
using static Quiz_App.QuizSelectorEdit;

namespace Quiz_App
{
    /// <summary>
    /// Interaction logic for StudentSelectQuiz.xaml
    /// </summary>
    public partial class StudentSelectQuiz : Window
    {
        public StudentSelectQuiz()
        {
            InitializeComponent();
            LoadUncompletedQuizzes();
        }

        public class Quiz
        {
            public int Id { get; set; }
            public string Topic { get; set; }
            public string Title { get; set; }
        }

        //Load the users uncompleted quizzes into list
        private void LoadUncompletedQuizzes()
        {
            int userId = GlobalVariables.UserId;

            var uncompletedQuizzes = GetUncompletedQuizzes(userId);
            dtgUncompleteQuizzes.ItemsSource = uncompletedQuizzes;
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
                        _ => throw new InvalidOperationException("Invalid user level")
                    };

                    //SQL query to fetch quizzes not completed by the user
                    command.CommandText = $"SELECT quiz.quizid, quiz.topic, quiz.title FROM quiz LEFT JOIN userquizcompletions ON quiz.quizid = userquizcompletions.quizid AND userquizcompletions.userid = @userId WHERE userquizcompletions.quizid IS NULL AND quiz.{levelColumn} = 1";

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

        private void btnCloseStudentSelectQuiz_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSelectQuiz_Click(object sender, RoutedEventArgs e)
        {
            if (dtgUncompleteQuizzes.SelectedItem is Quiz selectedQuiz)
            {
                //Proceed to start the selected quiz
                StudentQuizWindow StudentQuizWindow = new StudentQuizWindow(selectedQuiz.Id);
                this.Hide();
                
                StudentQuizWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Please select a quiz to start");
                return;
            }     
        }
    }
}
