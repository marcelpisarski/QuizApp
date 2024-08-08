using MySql.Data.MySqlClient;
using Quiz_App.Miscellaneous;
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
    /// Interaction logic for StudentResults.xaml
    /// </summary>
    public record QuizResult(int QuizId, string QuizTitle, int TotalMarks, int StudentMark, DateTime completionDate, decimal Percentage);

    public partial class StudentResults : Window
    {
        private int studentId;
        private Dictionary<int, QuizResult> studentResults;

        public StudentResults(int studentId)
        {
            InitializeComponent();
            
            this.studentId = studentId;
            studentResults = new Dictionary<int, QuizResult>();
            
            LoadStudentResults();
            DisplayQuizResults();
        }

        private void btnCloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LoadStudentResults()
        {
            //Define the connection string
            string connectionString = GlobalVariables.Connection;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    //Fetch quiz results for the student
                    command.CommandText = @"SELECT q.quizid, q.title, q.totalmarks, uqc.marks, uqc.completiondate, uqc.percentage
                                            FROM userquizcompletions uqc
                                            JOIN quiz q ON uqc.quizid = q.quizid
                                            WHERE uqc.userid = @userid";
                    command.Parameters.AddWithValue("@userid", studentId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int quizId = reader.GetInt32("quizid");
                            string quizTitle = reader.GetString("title");
                            int totalMarks = reader.GetInt32("totalMarks");
                            int studentMarks = reader.GetInt32("marks");
                            DateTime completionDate = reader.GetDateTime("completiondate");
                            decimal Percentage = reader.GetDecimal("percentage");

                            //Adds results to studentResults dictionary
                            studentResults.Add(quizId, new QuizResult(quizId, quizTitle, totalMarks, studentMarks, completionDate, Percentage));
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading results: {ex.Message}");
                }
            }
        }

        private void DisplayQuizResults()
        {
            //Convert the dictionary values to a list
            var results = studentResults.Values.ToList();

            //Sort the list using bubble sort by percentage score
            for (int i = 0; i < results.Count - 1; i++)
            {
                for (int j = 0; j < results.Count - i - 1; j++)
                {
                    if (results[j].Percentage < results[j + 1].Percentage)
                    {
                        //Swap elements
                        var temp = results[j];
                        results[j] = results[j + 1];
                        results[j + 1] = temp;
                    }
                }
            }

            //Display the sorted results in the data grid
            dtgStudentResults.ItemsSource = results;
        }

        private void btnViewQuizQuestions_Click(object sender, RoutedEventArgs e)
        {
            if (dtgStudentResults.SelectedItem == null)
            {
                MessageBox.Show("Please select a quiz");
                return;
            }

            var selectedQuizResult = (QuizResult)dtgStudentResults.SelectedItem;
            int selectedQuizId = selectedQuizResult.QuizId;

            IndividualQuestionResults IndividualQuestionResults = new IndividualQuestionResults(studentId, selectedQuizId);
            this.Hide();

            IndividualQuestionResults.Show();
            this.Show();
        }
    }
}
