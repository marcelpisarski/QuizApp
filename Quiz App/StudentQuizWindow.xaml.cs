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

        //Gets user answer and real answer as arguements for levenshtein algorithm
        private void btnSubmitSingleAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (currentQuestion is SingleChoiceQuestion scQuestion)
            {
                string userAnswer = txtSingleAnswer.Text.Trim().ToLower();
                string realAnswer = scQuestion.ScAnswer.ToString().Trim().ToLower();
                
                int distance = LevenshteinAlgorithmCheck(userAnswer, realAnswer);
                
                //Adjust edits as needed to mark answer as correct
                bool isCorrect = distance <= 3;

                CheckAnswerAndSave(scQuestion.QuestionId, isCorrect);
                DisplayNextQuestion();
            }
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

        private int LevenshteinAlgorithmCheck(string userAnswer, string realAnswer)
        {
            //Gets length of user's answer and actual answer
            int n = userAnswer.Length;
            int m = realAnswer.Length;

            //Creates a matrix based on lengths
            var matrix = new int[n + 1, m + 1];

            //Populates matrix
            for (int i = 0; i < n + 1; i++)
            {
                matrix[i, 0] = i;
            }

            //Populates matrix
            for (int j = 0; j < m + 1; j++)
            {
                matrix[0, j] = j;
            }


            //Edit distance algorithm using a matrix
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (userAnswer[i - 1] == realAnswer[j - 1]) ? 0 : 1;

                    matrix[i, j] = Math.Min(Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1), matrix[i - 1, j - 1] + cost);
                }
            }

            //Returns edits made to turn user answer into real answer
            return matrix[n, m];
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

                    //Calculate total marks
                    command.CommandText = "SELECT COUNT(*) FROM userquizanswers WHERE userid = @userId AND quizid = @quizId AND mark = 1";
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@quizId", quizId);
                    int totalUserMarks = Convert.ToInt32(command.ExecuteScalar());

                    command.Parameters.Clear();

                    //Calculate user percentage score
                    command.CommandText = "SELECT totalmarks FROM quiz WHERE quizid = @quizId";
                    command.Parameters.AddWithValue("@quizid", quizId);
                    int totalQuizMarks = Convert.ToInt32(command.ExecuteScalar());

                    decimal percentage = Math.Round(((decimal)totalUserMarks / totalQuizMarks) * 100, 2);

                    //Insert user and quiz completion information to database
                    command.CommandText = "INSERT INTO userquizcompletions (userid, quizid, marks, completiondate, percentage) VALUES (@userid, @quizid, @marks, @completiondate, @percentage)";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@userid", userId);
                    command.Parameters.AddWithValue("@quizid", quizId);
                    command.Parameters.AddWithValue("@marks", totalUserMarks);
                    command.Parameters.AddWithValue("@completiondate", DateTime.Now);
                    command.Parameters.AddWithValue("@percentage", percentage);

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
                //Sets single choice window
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
                    command.CommandText = @"SELECT q.questionid, q.questiontext, q.questiontype, mc.mcquestionid AS mcquestionid, mc.option1, 
                                            mc.option2, mc.option3, mc.option4, mc.correctanswerindex, sc.scquestionid AS scquestionid, sc.scanswer 
                                            FROM question q LEFT JOIN multiplechoicequestion mc ON q.questionid = mc.questionid 
                                            LEFT JOIN singlechoicequestion sc ON q.questionid = sc.questionid 
                                            WHERE q.quizid = @quizId";
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
    }
}
