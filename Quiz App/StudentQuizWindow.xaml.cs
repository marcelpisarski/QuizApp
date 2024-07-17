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
        //Initialises quizId, and questions
        private int quizId;
        private Queue<Question> questionQueue;
        private Question currentQuestion;

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
            btnStartQuiz.Visibility=Visibility.Hidden;
        }

        //Loads the questions
        private void LoadQuestions(int quizId)
        {
            //Stores all the questions inside a list
            List<Question> questions = FetchQuestionsFromDatabase(quizId);

            //Initialises and fills queue with questions
            questionQueue = new Queue<Question>(questions);

            //Displays first question
            DisplayNextQuestion();
        }

        //Displays the next question
        private void DisplayNextQuestion()
        {
            if (questionQueue.Count > 0)
            {
                currentQuestion = questionQueue.Dequeue();
                DisplayQuestion(currentQuestion);
            }
            else
            {
                MessageBox.Show("Quiz Completed!");
                MarkQuizAsCompleted(GlobalVariables.UserId, quizId);
                this.Close();
            }
        }

        //Marks quiz as complete for user
        private void MarkQuizAsCompleted(int userId, int quizId)
        {
            string connectionString = GlobalVariables.Connection;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    // Calculate total marks
                    command.CommandText = "SELECT COUNT(*) FROM userquizanswers WHERE userid = @userId AND quizid = @quizId AND mark = 1";
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@quizId", quizId);
                    int totalMarks = Convert.ToInt32(command.ExecuteScalar());

                    //Insert user and quiz completion information to database
                    command.CommandText = "INSERT INTO userquizcompletions (userid, quizid, marks, completiondate) VALUES (@userid, @quizid, @marks, @completiondate)";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@userid", userId);
                    command.Parameters.AddWithValue("@quizid", quizId);
                    command.Parameters.AddWithValue("@marks", totalMarks);
                    command.Parameters.AddWithValue("@completiondate", DateTime.Now);

                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error marking quiz as completed: {ex.Message}");
                }
            }
        }

        //Displays the current question
        private void DisplayQuestion(Question question)
        {
            txtQuestion.Text = question.QuestionText;

            if (question is MultipleChoiceQuestion mcQuestion)
            {
                grdMultipleChoiceQuestion.Visibility = Visibility.Visible;
                grdSingleChoiceQuestion.Visibility = Visibility.Hidden;

                //Set the multiple choice options
                btnAnswer1.Content = mcQuestion.Option1;
                btnAnswer2.Content = mcQuestion.Option2;
                btnAnswer3.Content = mcQuestion.Option3;
                btnAnswer4.Content = mcQuestion.Option4;
            }
            else if (question is SingleChoiceQuestion scQuestion)
            {
                grdMultipleChoiceQuestion.Visibility = Visibility.Hidden;
                grdSingleChoiceQuestion.Visibility = Visibility.Visible;
                txtSingleAnswer.Text = string.Empty;
            }
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

        //Awards user mark if correct answer given and saves it to the database
        private void CheckAnswerAndSave(int questionId, bool isCorrect)
        {
            int mark = isCorrect ? 1 : 0;

            string connectionString = GlobalVariables.Connection;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    command.CommandText = "INSERT INTO userquizanswers (userid, quizid, questionid, mark) VALUES (@userid, @quizid, @questionid, @mark)";
                    command.Parameters.AddWithValue("@userid", GlobalVariables.UserId);
                    command.Parameters.AddWithValue("@quizid", quizId);
                    command.Parameters.AddWithValue("@questionid", questionId);
                    command.Parameters.AddWithValue("@mark", mark);

                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving answer: {ex.Message}");
                }
            }
        }

        private void btnSubmitSingleAnswer_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAnswer1_Click(object sender, RoutedEventArgs e)
        {
            SubmitAnswer(0);
        }
       
        private void btnAnswer2_Click(object sender, RoutedEventArgs e)
        {
            SubmitAnswer(1);
        }
        private void btnAnswer3_Click(object sender, RoutedEventArgs e)
        {
            SubmitAnswer(2);
        }
        private void btnAnswer4_Click(object sender, RoutedEventArgs e)
        {
            SubmitAnswer(3);
        }

        //Checks if answer is correct for multiple choice questions
        private void SubmitAnswer(int selectedOption)
        {
            bool isCorrect = false;
            if (currentQuestion is MultipleChoiceQuestion mcQuestion)
            {
                if (mcQuestion.CorrectAnswerIndex == selectedOption)
                {
                    isCorrect = true;
                }
                CheckAnswerAndSave(mcQuestion.QuestionId, isCorrect);
                DisplayNextQuestion();
            }
        }
    }
}
