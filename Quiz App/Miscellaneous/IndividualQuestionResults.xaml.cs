using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using SkiaSharp;
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
using LiveChartsCore.SkiaSharpView.Painting;

namespace Quiz_App.Miscellaneous
{
    /// <summary>
    /// Interaction logic for IndividualQuestionResults.xaml
    /// </summary>
    public partial class IndividualQuestionResults : Window
    {
        public record QuestionList(int QuestionId, string QuestionText, string QuestionType, int Mark);
        public ISeries[] PieChart { get; set; }

        public IndividualQuestionResults(int studentId, int quizId)
        {
            InitializeComponent();
            LoadQuizQuestionResults(studentId, quizId);
            DataContext = this;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LoadQuizQuestionResults(int studentId, int quizId)
        {
            string connectionString = GlobalVariables.Connection;
            int answersCorrect = 0;
            int answersIncorrect = 0;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    command.CommandText = @"SELECT uqa.questionid, uqa.mark, q.questiontext, q.questiontype
                                        FROM userquizanswers uqa
                                        INNER JOIN question q ON uqa.questionid = q.questionid
                                        WHERE uqa.userid = @userid AND uqa.quizid = @quizid";

                    command.Parameters.AddWithValue("@userid", studentId);
                    command.Parameters.AddWithValue("@quizid", quizId);

                    using (var reader = command.ExecuteReader())
                    {
                        var questions = new List<QuestionList>();

                        while (reader.Read())
                        {
                            int questionId = reader.GetInt32("questionid");
                            int mark = reader.GetInt32("mark");
                            string questionText = reader.GetString("questiontext");
                            string questionType = reader.GetString("questiontype");

                            questions.Add(new QuestionList(questionId, questionText, questionType, mark));

                            
                            if (mark > 0)
                            {
                                answersCorrect++;
                            }
                            else
                            {
                                answersIncorrect++;
                            }
                        }

                        //Set the item source of the DataGrid or ListView to the questions list
                        dtgResultList.ItemsSource = questions;

                        //Calculates percentages and creates pie chart
                        double totalAnswers = answersCorrect + answersIncorrect;
                        double percentageCorrect = Math.Round((totalAnswers > 0) ? (answersCorrect / totalAnswers) * 100 : 0, 2);
                        double percentageIncorrect = Math.Round((totalAnswers > 0) ? (answersIncorrect / totalAnswers) * 100 : 0, 2);

                        PieChart = new ISeries[]
                        {
                            new PieSeries<double> { Values = new double[] { percentageCorrect }, Name = "Correct Percentage", Fill = new SolidColorPaint(SKColors.Green) },
                            new PieSeries<double> { Values = new double[] { percentageIncorrect }, Name = "Incorrect Percentage", Fill = new SolidColorPaint(SKColors.Red) }
                        };
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading quiz questions: {ex.Message}");
                }
            }
        }
    }
}
