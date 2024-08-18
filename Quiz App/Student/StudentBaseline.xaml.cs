using MySql.Data.MySqlClient;
using System.Diagnostics;
using Quiz_App;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for StudentBaseline.xaml
    /// </summary>
    public partial class StudentBaseline : Window
    {
        public StudentBaseline()
        {
            InitializeComponent();
            LoadStatement();
        }

        //Submits user answer
        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            string answer = txtAnswer.Text.Trim();

            //Checks if answer box is empty
            if (answer.Length == 0)
            {
                MessageBox.Show("Please answer the statement");
                return;
            }

            double letters = LetterChecker(answer);
            double sentences = SentencesChecker(answer);
            double words = WordsChecker(answer);

            Calculation(letters, sentences, words);
        }

        //Shows a random statement from BaselineStatements.txt to newly logged in user
        private void LoadStatement()
        {
            string filePath = GlobalVariables.BaselineStatements;
            string[] questions = File.ReadAllLines(filePath);

            Random random = new Random();
            int randomIndex = random.Next(questions.Length);

            //Selects a random question from questions array
            string randomStatement = questions[randomIndex];
            lblStatement.Content = randomStatement;
        }

        //Checks number of letters in answer
        private int LetterChecker(string statement)
        {
            int sum = 0;
            int lenth = statement.Length;

            for (int i = 0; i < lenth; i++)
            {
                if (char.IsLetter(statement[i]))
                {
                    sum++;
                }
            }
            return sum;
        }

        //Checks number of sentences in answer
        private int SentencesChecker(string statement)
        {
            int sum = 0;
            int lenth = statement.Length;

            for (int i = 0;i < lenth; i++)
            {
                if ((statement[i]) == '?' || (statement[i]) == '!' || (statement[i]) == '.' || (statement[i]) == ';')
                {
                    sum++;
                }
            }
            return sum;
        }

        //Checks number of words in answer
        private int WordsChecker(string statement)
        {
            int sum = 0;
            int length = statement.Length;

            for (int i = 0 ; i < length ; i++)
            {
                if (char.IsWhiteSpace(statement[i]))
                {
                    sum++;
                }       
            }
            return sum;
        }

        //Calculates user level
        private void Calculation(double letters, double sentences, double words)
        {
            //Grabs userid of current logged in user
            int userId = GlobalVariables.UserId;
            
            //Sets level conditions
            double[,] conditions = new double[,]
            {
                {double.NaN, 1 },
                {4, 1 },
                {8, 2 },
                {12, 3}
            };

            //Gets average letters and sentences
            letters = (letters / words) * 100;
            sentences = (sentences / words) * 100;

            //Coleman-liau formula to calculate literacy level
            double index = 0;
            index = 0.0588 * letters - 0.296 * sentences - 15.8;

            string connectionString = GlobalVariables.Connection;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    //Sets user level to calculated level and removes newuser role
                    command.CommandText = @"UPDATE userbaseline INNER JOIN user ON user.id = userbaseline.userid SET userbaseline.level = 4, 
                                            userbaseline.newuser = 0 WHERE user.id = @userId";

                    for (int i = 0; i < conditions.GetLength(0); i++)
                    {
                        if ((double.IsNaN(index) && double.IsNaN(conditions[i, 0])) || (index < conditions[i, 0]))
                        {
                            command.CommandText = @$"UPDATE userbaseline INNER JOIN user ON user.id = userbaseline.userid 
                                                    SET userbaseline.level = {conditions[i, 1]}, userbaseline.newuser = 0 WHERE user.id = @userId";
                            break;
                        }
                    }

                    command.Parameters.AddWithValue("@userId", userId);

                    //Execute the update query
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {                     
                        StudentMenu StudentMenu = new StudentMenu();
                        MessageBox.Show("User baseline updated successfully");
                        this.Hide();

                        StudentMenu.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("No rows updated");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating user baseline level: {ex.Message}");
                }
            }
        }
    }
}
