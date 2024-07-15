using MySql.Data.MySqlClient;
using Quiz_App.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for StudentQuizWindow.xaml
    /// </summary>
    public partial class StudentQuizWindow : Window
    {
        private int quizId;
        private Queue<Question> questionQueue;

        public StudentQuizWindow(int quizId)
        {
            InitializeComponent();

            this.quizId = quizId;
            
            //Initially hides quiz screen
            grdMultipleChoiceQuestion.Visibility = Visibility.Hidden;
            grdSingleChoiceQuestion.Visibility = Visibility.Hidden;
            txtQuestion.Visibility = Visibility.Hidden;
            btnStartQuiz.Visibility = Visibility.Visible;
        }

        //Lets user begin quiz
        private void btnStartQuiz_Click(object sender, RoutedEventArgs e)
        {
            txtQuestion.Visibility = Visibility.Visible;
            LoadQuestions(quizId);
        }

        //Loads the questions
        private void LoadQuestions(int quizId)
        {
            //Stores all the questions inside a list
            List<Question> questions = FetchQuestionsFromDatabase(quizId);


        }

        //Fetches all the questions from the quiz from database
        private List<Question> FetchQuestionsFromDatabase(int quizId)
        {
            //Creates list to fetch questions from database
            List<Question> questions = new List<Question>();

            string connectionString = GlobalVariables.Connection;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    // Fetch general question info
                    command.CommandText = "SELECT q.questionid, q.questiontext, q.questiontype, mc.mcquestionid AS mcquestionid, mc.option1, mc.option2, mc.option3, mc.option4, mc.correctanswerindex, sc.scquestionid AS scquestionid, sc.scanswer FROM question q LEFT JOIN multiplechoicequestion mc ON q.questionid = mc.questionid LEFT JOIN singlechoicequestion sc ON q.questionid = sc.questionid WHERE q.quizid = @quizId";
                    command.Parameters.AddWithValue("@quizId", quizId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Casts general question information into variables
                            int questionId = reader.GetInt32("questionid");
                            string questionText = reader.GetString("questiontext");
                            string questionType = reader.GetString("questiontype");

                            if (questionType == "MultipleChoice")
                            {
                                //Creates a MCQ object
                                var mcQuestion = new MultipleChoiceQuestion(
                                    reader.GetInt32("mcquestionid"),
                                    questionId,
                                    quizId,
                                    questionText,
                                    reader.GetString("option1"),
                                    reader.GetString("option2"),
                                    reader.GetString("option3"),
                                    reader.GetString("option4"),
                                    reader.GetInt32("correctanswerindex")
                                );
                                questions.Add(mcQuestion);

                            }
                            else if (questionType == "SingleChoice")
                            {
                                //Creates a SCQ object
                                var scQuestion = new SingleChoiceQuestion(
                                    reader.GetInt32("scquestionid"),
                                    questionId,
                                    quizId,
                                    questionText,
                                    reader.GetString("scanswer")
                                );
                                questions.Add(scQuestion);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error fetching questions: {ex.Message}");
                }
            }
            
            return questions;
        }
    }
}
