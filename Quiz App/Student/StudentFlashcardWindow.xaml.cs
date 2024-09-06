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
        private int quizId;
        private Stack<Question> questionStack;
        private Question currentQuestion;
        private bool questionDisplayed;

        public StudentFlashcardWindow(string flashcardType, bool randomiseQuestions, int quizId)
        {
            InitializeComponent();
            
            //Casts variables 
            this.flashcardType = flashcardType;
            this.randomiseQuestions = randomiseQuestions;
            this.quizId = quizId;

            questionStack = new Stack<Question>();
            List<Question> questions = FetchFlashCards();

            //Creates randomised list of questions
            if (randomiseQuestions)
            {
                Random random = new Random();
                questions = questions.OrderBy(questions => random.Next()).ToList();
            }

            //Push each question to a stack
            foreach (var question in questions)
            {
                questionStack.Push(question);
            }
            questionDisplayed = true;

            DisplayFlashcard();
        }

        //Pops question out of stack and displays new flashcard  
        private void btnNextFlashcard_Click(object sender, RoutedEventArgs e)
        {
            if (questionStack.Count > 0)
            {
                questionStack.Pop();
            }
            else
            {
                MessageBox.Show("No more flashcards to show");
                this.Close();
                return;
            }

            questionDisplayed = true;
            DisplayFlashcard();
        }

        //Switches between question and answer
        private void btnFlashcard_Click(object sender, RoutedEventArgs e)
        {
            DisplayFlashcard();
        }

        //Places question information onto flashcard
        private void DisplayFlashcard()
        {
            if (questionStack.Count > 0)
            {
                currentQuestion = questionStack.Peek();
            }
            else
            {
                MessageBox.Show("No more flashcards to show");
                this.Close();
                return;
            }

            if (questionDisplayed)
            {
                btnFlashcard.Content = currentQuestion.QuestionText;

                //Answer will display next time user presses flashcard
                questionDisplayed = false;
            }
            else
            {
                //Displays correct answer for multiple choice question
                if (currentQuestion is MultipleChoiceQuestion mcq)
                {
                    string correctAnswer = mcq.CorrectAnswerIndex
                    switch
                    {
                        0 => mcq.Option1,
                        1 => mcq.Option2,
                        2 => mcq.Option3,
                        3 => mcq.Option4,
                        _ => throw new NotImplementedException()
                    };

                    btnFlashcard.Content = correctAnswer;
                }
                else if (currentQuestion is SingleChoiceQuestion scq)
                {
                    btnFlashcard.Content = scq.ScAnswer;
                }
                
                //Question displays again once user presses flashcard
                questionDisplayed = true;
            }
        }

        //Fetches a list of questions depending on flashcard type
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
                            command.CommandText = @"SELECT q.questionid, q.questiontext, q.questiontype, q.quizid, 
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
                            command.CommandText = @"SELECT q.questionid, q.questiontext, q.questiontype, q.quizid, 
                                                    mc.mcquestionid, mc.option1, mc.option2, mc.option3, mc.option4, mc.correctanswerindex, 
                                                    sc.scquestionid, sc.scanswer 
                                                    FROM userquizcompletions uqc
                                                    JOIN question q ON uqc.quizid = q.quizid
                                                    LEFT JOIN multiplechoicequestion mc ON q.questionid = mc.questionid 
                                                    LEFT JOIN singlechoicequestion sc ON q.questionid = sc.questionid 
                                                    WHERE uqc.userid = @userId AND q.quizid = @quizId";
                            command.Parameters.AddWithValue("@userId", GlobalVariables.UserId);
                            command.Parameters.AddWithValue("@quizId", quizId);
                            break;

                        case "WrongQuizAnswers":
                            command.CommandText = @"SELECT q.questionid, q.questiontext, q.questiontype, q.quizid,
                                                    mc.mcquestionid, mc.option1, mc.option2, mc.option3, mc.option4, mc.correctanswerindex, 
                                                    sc.scquestionid, sc.scanswer 
                                                    FROM userquizanswers uqa
                                                    JOIN question q ON uqa.questionid = q.questionid
                                                    LEFT JOIN multiplechoicequestion mc ON q.questionid = mc.questionid 
                                                    LEFT JOIN singlechoicequestion sc ON q.questionid = sc.questionid 
                                                    WHERE uqa.userid = @userId AND uqa.mark = 0";
                            command.Parameters.AddWithValue("@userId", GlobalVariables.UserId);
                            break;

                        case "WrongChosenQuizAnswers":
                            command.CommandText = @"SELECT q.questionid, q.questiontext, q.questiontype, q.quizid, 
                                                    mc.mcquestionid, mc.option1, mc.option2, mc.option3, mc.option4, mc.correctanswerindex, 
                                                    sc.scquestionid, sc.scanswer 
                                                    FROM userquizcompletions uqc
                                                    JOIN question q ON uqc.quizid = q.quizid
                                                    LEFT JOIN multiplechoicequestion mc ON q.questionid = mc.questionid 
                                                    LEFT JOIN singlechoicequestion sc ON q.questionid = sc.questionid 
                                                    INNER JOIN userquizanswers uqa ON q.questionid = uqa.questionid
                                                    WHERE uqc.userid = @userId AND q.quizid = @quizId AND uqa.mark = 0";
                            command.Parameters.AddWithValue("@userId", GlobalVariables.UserId);
                            command.Parameters.AddWithValue("@quizId", quizId);
                            break;

                        default:
                            break;
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
                    MessageBox.Show($"Error fetching quiz questions: {ex.Message}");
                }
            }

            return questions;
        }
    }
}
