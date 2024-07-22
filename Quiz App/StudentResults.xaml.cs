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
    /// Interaction logic for StudentResults.xaml
    /// </summary>
    public record QuizResult(int QuizId, string QuizTitle, int TotalMarks, int StudentMark);
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

        private void LoadStudentResults()
        {
            // Define the connection string
            string connectionString = GlobalVariables.Connection;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    // Fetch quiz results for the student
                    command.CommandText = @"
                        SELECT q.quizid, q.title, q.totalmarks, uqc.marks 
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

                            studentResults.Add(quizId, new QuizResult(quizId, quizTitle, totalMarks, studentMarks));
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading quiz results: {ex.Message}");
                }
            }
        }

        private void DisplayQuizResults()
        {
            //Convert the dictionary values to a list
            var resultsList = studentResults.Values.ToList();

            //Sort the list using Bubble Sort by total marks
            for (int i = 0; i < resultsList.Count - 1; i++)
            {
                for (int j = 0; j < resultsList.Count - i - 1; j++)
                {
                    if (resultsList[j].TotalMarks < resultsList[j + 1].TotalMarks)
                    {
                        //Swap elements
                        var temp = resultsList[j];
                        resultsList[j] = resultsList[j + 1];
                        resultsList[j + 1] = temp;
                    }
                }
            }

            //Display the sorted results in the data grid
            dtgStudentResults.ItemsSource = resultsList;
        }
    }
}
