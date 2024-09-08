using Google.Protobuf;
using MySql.Data.MySqlClient;
using Quiz_App.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using static Mysqlx.Notice.Warning.Types;
using static Quiz_App.MainWindow;

namespace Quiz_App
{
    /// <summary>
    /// Interaction logic for AddQuiz.xaml
    /// </summary>
    public partial class AddQuiz : Window
    {
        bool quizSchemaCreated = false;
        int existingQuizId = -1;
        bool isEditingQuiz = false;
        int questionCount = 0;

        public AddQuiz()
        {
            InitializeComponent();
            
            btnAddQuestion.IsEnabled = false;
            dtgQuestionData.IsEnabled = false;
            btnCreateSchema.IsEnabled = true;
            btnDeleteQuestion.IsEnabled = false;
        }

        private void btnAddQuestion_Click(object sender, RoutedEventArgs e)
        {
            int quizId = GlobalVariables.QuizId;

            //Checks if a user has completed current quiz. If so, then teacher cannot edit questions
            if (IsQuizCompleted(quizId))
            {
                MessageBox.Show("Unable to add questions. This quiz was already completed by a user");
                return;
            }
            
            QuizQuestion QuizQuestion = new QuizQuestion();
            
            //Connects OnQuestionAdded method to QuestionAdded event inside QuizQuestion
            QuizQuestion.QuestionAdded += WhenQuestionAdded;
            QuizQuestion.Show();
        }

        private void btnCancelQuiz_Click(object sender, RoutedEventArgs e)
        {       
            if (isEditingQuiz)
            {
                if (questionCount == 0) 
                {
                    MessageBox.Show("Cannot cancel as question count is 0. Please add a question");
                    return;
                }
                
                this.Close();
                return;
            }
            
            //Deletes quiz as user cancelled
            if (quizSchemaCreated)
            {
                int quizId = GlobalVariables.QuizId;

                //Define the connection string
                string connectionString = GlobalVariables.Connection;

                using (var connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        var command = connection.CreateCommand();

                        //Delete the quiz from the database
                        command.CommandText = "DELETE FROM quiz WHERE quizid = @quizid";
                        command.Parameters.AddWithValue("@quizid", quizId);

                        var result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Quiz deleted successfully");
                        }
                        else
                        {
                            MessageBox.Show("Quiz not found");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting quiz: {ex.Message}");
                    }
                }
            }

            this.Close();
        }

        private void btnSaveQuiz_Click(object sender, RoutedEventArgs e)
        {
            //Check if there are no questions added
            if (questionCount == 0)
            {
                MessageBox.Show("Cannot save as question count is 0. Please add at least one question");
                return;
            }

            if (isEditingQuiz)
            {
                UpdateQuiz();
            }
            else if (!isEditingQuiz)
            {
                {
                    this.Close();
                }
            }
        }

        //Adds quiz to database
        private void btnCreateSchema_Click(object sender, RoutedEventArgs e)
        {
            //Shows error if title or topic name is empty
            if (txtQuizTitle.Text.Length == 0 || txtTopicName.Text.Length == 0)
            {
                MessageBox.Show("Quiz title and/or topic name is empty");
                return;
            }

            //Ensures quiz has levels selected
            if (cbLevel1.IsChecked == false && cbLevel2.IsChecked == false && cbLevel3.IsChecked == false && cbLevel4.IsChecked == false)
            {
                MessageBox.Show("Please select a quiz level");
                return;
            }

            //Adds quiz details to variables
            string quizTopic = txtTopicName.Text.Trim();
            string quizTitle = txtQuizTitle.Text.Trim();
            bool level1 = false, level2 = false, level3 = false, level4 = false;
            int teacherId = GlobalVariables.UserId;

            if (cbLevel1.IsChecked == true)
            {
                level1 = true;
            }
            if (cbLevel2.IsChecked == true)
            {
                level2 = true;
            }
            if (cbLevel3.IsChecked == true)
            {
                level3 = true;
            }
            if (cbLevel4.IsChecked == true)
            {
                level4 = true;
            }          

            //Insert quiz data into the database
            string connectionString = GlobalVariables.Connection;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    //Check if the quiz title already exists
                    command.CommandText = "SELECT COUNT(*) FROM quiz WHERE title = @title";
                    command.Parameters.AddWithValue("@title", quizTitle);

                    int quizCount = Convert.ToInt32(command.ExecuteScalar());

                    if (quizCount > 0)
                    {
                        MessageBox.Show("Quiz title already exists");
                        return;
                    }

                    quizSchemaCreated = true;

                    //Clear previous parameters
                    command.Parameters.Clear();

                    command.CommandText = @"INSERT INTO quiz (topic, title, teacherid, level1, level2, level3, level4, totalmarks) 
                                            VALUES (@topic, @title, @teacherid, @level1, @level2, @level3, @level4, @totalmarks)";

                    command.Parameters.AddWithValue("@topic", quizTopic);
                    command.Parameters.AddWithValue("@title", quizTitle);
                    command.Parameters.AddWithValue("@teacherid", teacherId);
                    command.Parameters.AddWithValue("@level1", level1);
                    command.Parameters.AddWithValue("@level2", level2);
                    command.Parameters.AddWithValue("@level3", level3);
                    command.Parameters.AddWithValue("@level4", level4);
                    command.Parameters.AddWithValue("@totalmarks", questionCount);

                    var result = command.ExecuteNonQuery();

                    //Retrieve the generated quizid
                    command.CommandText = "SELECT LAST_INSERT_ID()";
                    GlobalVariables.QuizId = Convert.ToInt32(command.ExecuteScalar());

                    if (result == 1)
                    {
                        MessageBox.Show("Quiz created successfully");
                        
                        btnAddQuestion.IsEnabled = true;
                        dtgQuestionData.IsEnabled = true;
                        btnCreateSchema.IsEnabled = false;
                        btnDeleteQuestion.IsEnabled = true;
                        
                        LoadQuestions();
                    }
                    else
                    {
                        MessageBox.Show("Failed to create quiz");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating quiz: {ex.Message}");
                }
            }
        }

        //Delete question from database and table
        private void btnDeleteQuestion_Click(object sender, RoutedEventArgs e)
        {
            int quizId = GlobalVariables.QuizId;

            //Checks if a user has completed current quiz. If so, then teacher cannot edit questions
            if (IsQuizCompleted(quizId))
            {
                MessageBox.Show("Unable to delete questions. This quiz was already completed by a user");
                return;
            }

            //Ensures a question is selected
            if (dtgQuestionData.SelectedItem == null)
            {
                MessageBox.Show("Please select a question to delete");
                return;
            }

            //Get the selected question and create object
            Question selectedQuestion = (Question)dtgQuestionData.SelectedItem;
            int questionId = selectedQuestion.QuestionId;

            string connectionString = GlobalVariables.Connection;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    //Delete the question from the database
                    command.CommandText = "DELETE FROM question WHERE questionid = @questionid";
                    command.Parameters.AddWithValue("@questionid", questionId);

                    var result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Question deleted successfully");
                        
                        //Refresh items in table
                        LoadQuestions();
                    }
                    else
                    {
                        MessageBox.Show("Question not found or already deleted");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting question: {ex.Message}");
                }
            }
        }

        //Refreshes question list
        private void WhenQuestionAdded()
        {
            LoadQuestions();
        }

        //Loads current quiz questions into table which user can see
        private void LoadQuestions()
        {
            string connectionString = GlobalVariables.Connection;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT questionid, questiontext, questiontype FROM question WHERE quizid = @quizid";
                    command.Parameters.AddWithValue("@quizid", GlobalVariables.QuizId);

                    using (var reader = command.ExecuteReader())
                    {
                        //Creates a new list of questions which will be shown inside table
                        var questions = new List<Question>();

                        //Adds a new question to list
                        while (reader.Read())
                        {
                            questions.Add(new Question
                            {
                                QuestionId = reader.GetInt32("questionid"),
                                QuestionText = reader.GetString("questiontext"),
                                QuestionType = reader.GetString("questiontype")
                            });
                        }

                        //Fills datagrid with questions
                        dtgQuestionData.ItemsSource = questions;
                        questionCount = questions.Count;
                    }
                    
                    //Update the question count in the quiz table
                    command.CommandText = "UPDATE quiz SET totalmarks = @totalmarks WHERE quizid = @quizid";
                    command.Parameters.AddWithValue("@totalmarks", questionCount);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading quiz questions: {ex.Message}");
                }
            }
        }

        //Loads quiz that user will edit
        public void LoadQuiz(int quizId)
        {
            //Ensures user cannot create another quiz
            btnCreateSchema.IsEnabled = false;

            GlobalVariables.QuizId = quizId;
            existingQuizId = quizId;
            isEditingQuiz = true;

            string connectionString = GlobalVariables.Connection;

            //Fills window with quiz details
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT title, topic, level1, level2, level3, level4 FROM quiz WHERE quizid = @quizid";
                    command.Parameters.AddWithValue("@quizid", quizId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtQuizTitle.Text = reader.GetString("title");
                            txtTopicName.Text = reader.GetString("topic");
                            cbLevel1.IsChecked = reader.GetBoolean("level1");
                            cbLevel2.IsChecked = reader.GetBoolean("level2");
                            cbLevel3.IsChecked = reader.GetBoolean("level3");
                            cbLevel4.IsChecked = reader.GetBoolean("level4");

                            quizSchemaCreated = true;
                            btnAddQuestion.IsEnabled = true;
                            dtgQuestionData.IsEnabled = true;
                            btnDeleteQuestion.IsEnabled = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading quiz data: {ex.Message}");
                }
            }

            LoadQuestions();
        }

        //Updates quiz when user presses save
        private void UpdateQuiz()
        {
            //Shows error if title / topic name is empty
            if (txtQuizTitle.Text.Trim().Length == 0 || txtTopicName.Text.Trim().Length == 0)
            {
                MessageBox.Show("Quiz title and/or topic name is empty");
                return;
            }

            //Ensures a quiz has a level selected
            if (cbLevel1.IsChecked == false && cbLevel2.IsChecked == false && cbLevel3.IsChecked == false && cbLevel4.IsChecked == false)
            {
                MessageBox.Show("Please select a quiz level");
                return;
            }

            //Casts updated quiz details to variables
            string quizTitle = txtQuizTitle.Text.Trim();
            string quizTopic = txtTopicName.Text.Trim();
            bool level1 = false, level2 = false, level3 = false, level4 = false;
            int teacherId = GlobalVariables.UserId;

            if (cbLevel1.IsChecked == true)
            {
                level1 = true;
            }
            if (cbLevel2.IsChecked == true)
            {
                level2 = true;
            }
            if (cbLevel3.IsChecked == true)
            {
                level3 = true;
            }
            if (cbLevel4.IsChecked == true)
            {
                level4 = true;
            }

            //Insert quiz data into the database
            string connectionString = GlobalVariables.Connection;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    //Retrieve existing quiz title
                    command.CommandText = "SELECT title FROM quiz WHERE quizid = @quizid";
                    command.Parameters.AddWithValue("@quizid", existingQuizId);
                    string existingTitle = (string)command.ExecuteScalar();

                    //Check if the quiz title already exists
                    if (quizTitle != existingTitle)
                    {
                        command.CommandText = "SELECT COUNT(*) FROM quiz WHERE title = @title";
                        command.Parameters.AddWithValue("@title", quizTitle);
                        int quizCount = Convert.ToInt32(command.ExecuteScalar());

                        if (quizCount > 0)
                        {
                            MessageBox.Show("Quiz title already exists");
                            return;
                        }
                    }

                    quizSchemaCreated = true;

                    //Clear previous parameters
                    command.Parameters.Clear();

                    command.CommandText = @"UPDATE quiz SET topic = @topic, title = @title, 
                                            level1 = @level1, level2 = @level2, level3 = @level3, level4 = @level4, 
                                            totalmarks = @totalmarks WHERE quizid = @quizid";

                    command.Parameters.AddWithValue("@topic", quizTopic);
                    command.Parameters.AddWithValue("@title", quizTitle);
                    command.Parameters.AddWithValue("@teacherid", teacherId);
                    command.Parameters.AddWithValue("@level1", level1);
                    command.Parameters.AddWithValue("@level2", level2);
                    command.Parameters.AddWithValue("@level3", level3);
                    command.Parameters.AddWithValue("@level4", level4);
                    command.Parameters.AddWithValue("@totalmarks", questionCount);
                    command.Parameters.AddWithValue("@quizid", existingQuizId);
                    int result = command.ExecuteNonQuery();

                    if (result == 1)
                    {
                        MessageBox.Show("Quiz updated successfully");
                        
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Failed to update quiz");
                    }
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating quiz: {ex.Message}");
                }
            }
        }

        //Checks number of users who completed the quiz
        private bool IsQuizCompleted(int quizId)
        {
            string connectionString = GlobalVariables.Connection;
            bool isCompleted = false;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    //Check if there are any completions for the selected quiz
                    command.CommandText = "SELECT COUNT(*) FROM userquizcompletions WHERE quizid = @quizId";
                    command.Parameters.AddWithValue("@quizId", quizId);

                    int completionCount = Convert.ToInt32(command.ExecuteScalar());
                    isCompleted = completionCount > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error validating if quiz has been completed by a user: {ex.Message}");
                }
            }

            return isCompleted;
        }

        //Question class for datagrid which shows questions inside quiz
        public class Question
        {
            public int QuestionId { get; set; }
            public string QuestionText { get; set; }
            public string QuestionType { get; set; }
        }
    }
}
