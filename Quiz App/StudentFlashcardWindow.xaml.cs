using MySql.Data.MySqlClient;
using Quiz_App.Classes;
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
    /// Interaction logic for StudentFlashcardWindow.xaml
    /// </summary>
    public partial class StudentFlashcardWindow : Window
    {
        private string flashcardType;
        private bool randomiseQuestions;
        private Stack<Question> questionList;
        public StudentFlashcardWindow(string flashcardType, bool randomiseQuestions)
        {
            InitializeComponent();
            this.flashcardType = flashcardType;
            this.randomiseQuestions = randomiseQuestions;
            questionList = new Stack<Question>();

            List<Question> questions = FetchFlashCards();
        }

        private List<Question> FetchFlashCards()
        {
            List<Question> questions = new List<Question>();
            string connectionString = GlobalVariables.Connection;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    switch (flashcardType)
                    {
                        case "AllQuestions":
                            command.CommandText = @"
                                SELECT q.questionid, q.questiontext, q.questiontype, 
                                       mc.mcquestionid, mc.option1, mc.option2, mc.option3, mc.option4, mc.correctanswerindex, 
                                       sc.scquestionid, sc.scanswer 
                                FROM userquizanswers uqa
                                JOIN question q ON uqa.questionid = q.questionid
                                LEFT JOIN multiplechoicequestion mc ON q.questionid = mc.questionid 
                                LEFT JOIN singlechoicequestion sc ON q.questionid = sc.questionid 
                                WHERE uqa.userid = @userId";
                            command.Parameters.AddWithValue("@userId", GlobalVariables.UserId);
                            break;

                        case "PreviousQuizzes":
                            command.CommandText = @"
                                SELECT q.questionid, q.questiontext, q.questiontype, 
                                       mc.mcquestionid, mc.option1, mc.option2, mc.option3, mc.option4, mc.correctanswerindex, 
                                       sc.scquestionid, sc.scanswer 
                                FROM userquizcompletions uqc
                                JOIN question q ON uqc.quizid = q.quizid
                                LEFT JOIN multiplechoicequestion mc ON q.questionid = mc.questionid 
                                LEFT JOIN singlechoicequestion sc ON q.questionid = sc.questionid 
                                WHERE uqc.userid = @userId";
                            command.Parameters.AddWithValue("@userId", GlobalVariables.UserId);
                            break;

                        case "WrongQuizAnswers":
                            command.CommandText = @"
                                SELECT q.questionid, q.questiontext, q.questiontype, 
                                       mc.mcquestionid, mc.option1, mc.option2, mc.option3, mc.option4, mc.correctanswerindex, 
                                       sc.scquestionid, sc.scanswer 
                                FROM userquizanswers uqa
                                JOIN question q ON uqa.questionid = q.questionid
                                LEFT JOIN multiplechoicequestion mc ON q.questionid = mc.questionid 
                                LEFT JOIN singlechoicequestion sc ON q.questionid = sc.questionid 
                                WHERE uqa.userid = @userId AND uqa.iscorrect = 0";
                            command.Parameters.AddWithValue("@userId", GlobalVariables.UserId);
                            break;

                        default:
                            throw new Exception("Unknown flashcard type");
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int questionId = reader.GetInt32("questionid");
                            string questionText = reader.GetString("questiontext");
                            string questionType = reader.GetString("questiontype");

                            if (questionType == "MultipleChoice")
                            {
                                var mcQuestion = new MultipleChoiceQuestion(
                                    reader.GetInt32("mcquestionid"),
                                    questionId,
                                    reader.GetInt32("quizid"),
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
                                var scQuestion = new SingleChoiceQuestion(
                                    reader.GetInt32("scquestionid"),
                                    questionId,
                                    reader.GetInt32("quizid"),
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
