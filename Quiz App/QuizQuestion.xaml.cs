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
    /// Interaction logic for QuizQuestion.xaml
    /// </summary>
    public partial class QuizQuestion : Window
    {
        public event Action QuestionAdded;

        int quizId = GlobalVariables.QuizId;

        public QuizQuestion()
        {
            InitializeComponent();
            grdSingleAnswer.Visibility = Visibility.Collapsed;
            grdMultipleChoice.Visibility = Visibility.Collapsed;
        }

        private void rbSingleAnswer_Checked(object sender, RoutedEventArgs e)
        {
            grdSingleAnswer.Visibility = Visibility.Visible;

        }

        private void rbSingleAnswer_UnChecked(object sender, RoutedEventArgs e)
        {
            grdSingleAnswer.Visibility = Visibility.Collapsed;
        }

        private void rbMultipleChoice_Checked(object sender, RoutedEventArgs e)
        {
            grdMultipleChoice.Visibility = Visibility.Visible;
        }

        private void rbMultipleChoice_UnChecked(object sender, RoutedEventArgs e)
        {
            grdMultipleChoice.Visibility = Visibility.Collapsed;
        }

        private void btnCancelQuestion_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnAddQuestion_Click(object sender, RoutedEventArgs e)
        {
            //Ensures user enters question
            if (txtQuestion.Text.Trim().Length == 0)
            {
                MessageBox.Show("Enter question");
                return;
            }

            //Ensures question type is chosen
            if (rbMultipleChoice.IsChecked == false && rbSingleAnswer.IsChecked == false)
            {
                MessageBox.Show("Please select question type");
                return;
            }

            //Ensures no fields are empty
            if (rbMultipleChoice.IsChecked == true)
            {
                if (txtOption1.Text.Trim().Length == 0 || txtOption2.Text.Trim().Length == 0 || txtOption3.Text.Trim().Length == 0 || txtOption4.Text.Trim().Length == 0)
                {
                    MessageBox.Show("Enter all answer boxes");
                    return;
                }
                if (rbOption1.IsChecked == false && rbOption2.IsChecked == false && rbOption3.IsChecked == false && rbOption4.IsChecked == false)
                {
                    MessageBox.Show("Please choose the correct answer");
                    return;
                }

                InsertMultipleChoiceQuestion(txtQuestion.Text.Trim());
            }
            else if (rbSingleAnswer.IsChecked == true)
            {
                if (txtSingleAnswer.Text.Trim().Length == 0)
                {
                    MessageBox.Show("Please enter an answer");
                    return;
                }

                InsertSingleChoiceQuestion(txtQuestion.Text.Trim());
            }

            //Triggers QuestionAdded event
            QuestionAdded?.Invoke();
            this.Close();
        }

        //Insert question into database
        private void InsertMultipleChoiceQuestion(string questionText)
        {
            int correctAnswerIndex = 0;
            if (rbOption1.IsChecked == true)
            {
                correctAnswerIndex = 0;
            }
            else if (rbOption2.IsChecked == true)
            {
                correctAnswerIndex = 1;
            }
            else if (rbOption3.IsChecked == true)
            {
                correctAnswerIndex = 2;
            }
            else if (rbOption4.IsChecked == true)
            {
                correctAnswerIndex = 3;
            }

            string connectionString = "server=127.0.0.1;uid=root;pwd=;database=quizsystem;SslMode=Required;";

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    //Insert into question table
                    command.CommandText = "INSERT INTO question (quizid, questiontext, questiontype) VALUES (@quizid, @questiontext, @questiontype)";
                    command.Parameters.AddWithValue("@quizid", quizId);
                    command.Parameters.AddWithValue("@questiontext", questionText);
                    command.Parameters.AddWithValue("@questiontype", "MultipleChoice");

                    command.ExecuteNonQuery();

                    //Get the inserted question's ID
                    command.CommandText = "SELECT LAST_INSERT_ID();";
                    int questionId = Convert.ToInt32(command.ExecuteScalar());

                    //Insert into singlechoicequestion table
                    command.CommandText = "INSERT INTO multiplechoicequestion (questionid, option1, option2, option3, option4, correctanswerindex) VALUES (@questionid, @option1, @option2, @option3, @option4, @correctanswerindex)";
                    command.Parameters.AddWithValue("@questionid", questionId);
                    command.Parameters.AddWithValue("@option1", txtOption1.Text.Trim());
                    command.Parameters.AddWithValue("@option2", txtOption2.Text.Trim());
                    command.Parameters.AddWithValue("@option3", txtOption3.Text.Trim());
                    command.Parameters.AddWithValue("@option4", txtOption4.Text.Trim());
                    command.Parameters.AddWithValue("@correctanswerindex", correctAnswerIndex);


                    command.ExecuteNonQuery();

                    MessageBox.Show("Multiple choice question added successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating question: {ex.Message}");
                }
            }
        }

        //Insert question into database
        private void InsertSingleChoiceQuestion(string questionText)
        {
            string connectionString = "server=127.0.0.1;uid=root;pwd=;database=quizsystem;SslMode=Required;";

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    //Insert into question table
                    command.CommandText = "INSERT INTO question (quizid, questiontext, questiontype) VALUES (@quizid, @questiontext, @questiontype)";
                    command.Parameters.AddWithValue("@quizid", quizId);
                    command.Parameters.AddWithValue("@questiontext", questionText);
                    command.Parameters.AddWithValue("@questiontype", "SingleChoice");

                    command.ExecuteNonQuery();

                    //Get the inserted question's ID
                    command.CommandText = "SELECT LAST_INSERT_ID();";
                    int questionId = Convert.ToInt32(command.ExecuteScalar());

                    //Insert into multiplechoicequestion table
                    command.CommandText = "INSERT INTO singlechoicequestion (questionid, scanswer) VALUES (@questionid, @scanswer)";
                    command.Parameters.AddWithValue("@questionid", questionId);
                    command.Parameters.AddWithValue("@scanswer", txtSingleAnswer.Text.Trim());

                    command.ExecuteNonQuery();

                    MessageBox.Show("Single choice question added successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating question: {ex.Message}");
                }
            }
        }
    }
}
