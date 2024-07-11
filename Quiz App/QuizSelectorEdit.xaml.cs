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

namespace Quiz_App
{
    /// <summary>
    /// Interaction logic for QuizSelectorEdit.xaml
    /// </summary>
    public partial class QuizSelectorEdit : Window
    {
        public QuizSelectorEdit()
        {
            InitializeComponent();
            LoadQuizzes();
        }

        //Class to input quizzes into table
        public class Quiz
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Topic { get; set; }
        }

        //Load quizzes into table
        private void LoadQuizzes()
        {
            string connectionString = "server=127.0.0.1;uid=root;pwd=;database=quizsystem;SslMode=Required;";

            int teacherId = GlobalVariables.UserId;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT quizid, title, topic FROM quiz WHERE teacherid = @teacherid";
                    command.Parameters.AddWithValue("@teacherid", teacherId);

                    using (var reader = command.ExecuteReader())
                    {
                        var quizzes = new List<Quiz>();

                        while (reader.Read())
                        {
                            quizzes.Add(new Quiz
                            {
                                Id = reader.GetInt32("quizid"),
                                Title = reader.GetString("title"),
                                Topic = reader.GetString("topic")
                            });
                        }

                        //Bind the list of quizzes to the datagrid
                        dtgQuizList.ItemsSource = quizzes;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading quizzes: {ex.Message}");
                }
            }
        }

        private void btnSelectQuiz_Click(object sender, RoutedEventArgs e)
        {
            if (dtgQuizList.SelectedItem == null)
            {
                MessageBox.Show("Please select a quiz");
                return;
            }

            //Create a new instance of AddQuiz window
            Quiz selectedQuiz = (Quiz)dtgQuizList.SelectedItem;

            //Load the selected quiz into the AddQuiz window
            AddQuiz AddQuiz = new AddQuiz();
            AddQuiz.LoadQuiz(selectedQuiz.Id);
            AddQuiz.Show();
        }

        private void btnCancelQuizEdit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnDeleteQuiz_Click(object sender, RoutedEventArgs e)
        {
            if (dtgQuizList.SelectedItem == null)
            {
                MessageBox.Show("Please select a quiz to delete");
                return;
            }

            Quiz selectedQuiz = (Quiz)dtgQuizList.SelectedItem;
            string connectionString = "server=127.0.0.1;uid=root;pwd=;database=quizsystem;SslMode=Required;";

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    command.CommandText = "DELETE FROM quiz WHERE quizid = @quizid";
                    command.Parameters.AddWithValue("@quizid", selectedQuiz.Id);

                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Quiz deleted successfully");
                        //Refresh list
                        LoadQuizzes();
                    }
                    else
                    {
                        MessageBox.Show("Error deleting quiz");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting quiz: {ex.Message}");
                }
            }
        }
    }
}
