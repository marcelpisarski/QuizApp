using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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
    /// Interaction logic for QuizSelectorEdit.xaml
    /// </summary>
    public partial class QuizSelectorEdit : Window
    {
        private BinarySearchTree quizTree;

        public QuizSelectorEdit()
        {

            InitializeComponent();

            quizTree = new BinarySearchTree();
            LoadQuizzes(-1);
        }

        //Class to input quizzes into table
        public class Quiz
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Topic { get; set; }
        }

        private void btnSelectQuiz_Click(object sender, RoutedEventArgs e)
        {
            //Ensures user selects an item from list
            if (dtgQuizList.SelectedItem == null)
            {
                MessageBox.Show("Please select a quiz");
                return;
            }

            //Create a new instance of AddQuiz window
            Quiz selectedQuiz = (Quiz)dtgQuizList.SelectedItem;

            //Load the selected quiz into the AddQuiz window
            AddQuiz AddQuiz = new AddQuiz();
            AddQuiz.LoadQuiz(selectedQuiz.Id);
            AddQuiz.Show();
        }

        //Cancels quiz edit (closes window)
        private void btnCancelQuizEdit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //Deletes the selected quiz
        private void btnDeleteQuiz_Click(object sender, RoutedEventArgs e)
        {
            //Ensures an item is selected from list
            if (dtgQuizList.SelectedItem == null)
            {
                MessageBox.Show("Please select a quiz to delete");
                return;
            }

            //Creates new quiz object of selected quiz
            Quiz selectedQuiz = (Quiz)dtgQuizList.SelectedItem;

            string connectionString = GlobalVariables.Connection;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    command.CommandText = "DELETE FROM quiz WHERE quizid = @quizid";
                    command.Parameters.AddWithValue("@quizid", selectedQuiz.Id);

                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Quiz deleted successfully");
                        
                        //Refresh list
                        LoadQuizzes(-1);
                    }
                    else
                    {
                        MessageBox.Show("Error deleting quiz");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting quiz: {ex.Message}");
                }
            }
        }

        //Load quizzes into table
        private void LoadQuizzes(int Id)
        {
            string connectionString = GlobalVariables.Connection;

            int teacherId = GlobalVariables.UserId;
            int quizId = Id;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    if (quizId == -1)
                    {
                        command.CommandText = "SELECT quizid, title, topic FROM quiz WHERE teacherid = @teacherid";
                        command.Parameters.AddWithValue("@teacherid", teacherId);
                    }
                    else
                    {
                        command.CommandText = "SELECT quizid, title, topic FROM quiz WHERE teacherid = @teacherid AND quizid = @quizid";
                        command.Parameters.AddWithValue("@teacherid", teacherId);
                        command.Parameters.AddWithValue("@quizid", quizId);
                    }

                    //Adds all quizzes to a list
                    using (var reader = command.ExecuteReader())
                    {
                        var quizzes = new List<Quiz>();

                        while (reader.Read())
                        {
                            var quiz = new Quiz
                            {
                                Id = reader.GetInt32("quizid"),
                                Title = reader.GetString("title"),
                                Topic = reader.GetString("topic")
                            };
                            quizzes.Add(quiz);
                            quizTree.InsertQuiz(quiz);
                        }

                        //Bind the list of quizzes to the datagrid
                        dtgQuizList.ItemsSource = quizzes;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading quizzes: {ex.Message}");
                }
            }
        }
        
        //Exports a quiz to the user
        private void btnExportQuiz_Click(object sender, RoutedEventArgs e)
        {
            Quiz selectedQuiz = (Quiz)dtgQuizList.SelectedItem;

            //Ensures a quiz is selected from list
            if (dtgQuizList.SelectedItem == null)
            {
                MessageBox.Show("Please select a quiz to export");
                return;
            }

            string connectionString = GlobalVariables.Connection;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    //Fetch quiz details including level
                    command.CommandText = "SELECT title, topic, level1, level2, level3, level4 FROM quiz WHERE quizid = @quizid";
                    command.Parameters.AddWithValue("@quizid", selectedQuiz.Id);

                    string title = "";
                    string topic = "";
                    int level1 = -1;
                    int level2 = -1;
                    int level3 = -1;
                    int level4 = -1;

                    //Casts quiz data to variables
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            title = reader.GetString("title");
                            topic = reader.GetString("topic");
                            level1 = reader.GetInt32("level1");
                            level2 = reader.GetInt32("level2");
                            level3 = reader.GetInt32("level3");
                            level4 = reader.GetInt32("level4");
                        }
                    }

                    //Fetch questions and answers
                    command.CommandText = @"
                        SELECT q.questionid, q.questiontext, q.questiontype, 
                               mc.option1, mc.option2, mc.option3, mc.option4, mc.correctanswerindex, 
                               sc.scanswer
                        FROM question q
                        LEFT JOIN multiplechoicequestion mc ON q.questionid = mc.questionid
                        LEFT JOIN singlechoicequestion sc ON q.questionid = sc.questionid
                        WHERE q.quizid = @quizid";

                    //Starts to build exported file
                    var quizContent = new StringBuilder();
                    quizContent.AppendLine($"Quiz Title: {title}");
                    quizContent.AppendLine($"Topic: {topic}");
                    quizContent.AppendLine($"Level 1: {level1}");
                    quizContent.AppendLine($"Level 2: {level2}");
                    quizContent.AppendLine($"Level 3: {level3}");
                    quizContent.AppendLine($"Level 4: {level4}");

                    quizContent.AppendLine();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string questionText = reader.GetString("questiontext");
                            string questionType = reader.GetString("questiontype");

                            quizContent.AppendLine($"Question: {questionText}");
                            quizContent.AppendLine($"Type: {questionType}");

                            if (questionType == "MultipleChoice")
                            {
                                quizContent.AppendLine($"1. {reader.GetString("option1")}");
                                quizContent.AppendLine($"2. {reader.GetString("option2")}");
                                quizContent.AppendLine($"3. {reader.GetString("option3")}");
                                quizContent.AppendLine($"4. {reader.GetString("option4")}");
                                quizContent.AppendLine($"correctanswerindex. {reader.GetInt32("correctanswerindex")}");
                            }
                            else if (questionType == "SingleChoice")
                            {
                                quizContent.AppendLine($"Answer: {reader.GetString("scanswer")}");
                            }
                            quizContent.AppendLine();
                        }
                    }

                    //Save to file on the desktop
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string filePath = System.IO.Path.Combine(desktopPath, $"{title}_Quiz.txt");

                    System.IO.File.WriteAllText(filePath, quizContent.ToString());

                    MessageBox.Show($"Quiz exported successfully to {desktopPath}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting quiz: {ex.Message}");
                }
            }
        }

        public class Question
        {
            public string Text { get; set; }
            public string Type { get; set; }
            public List<string> Options { get; set; }
            public int CorrectAnswerIndex { get; set; }
            public string Answer { get; set; }
        }

        private void btnImportQuiz_Click(object sender, RoutedEventArgs e)
        {
            //Open a file dialog to select the quiz file
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt",
                Title = "Select Quiz File"
            };

            //Lets the user select a file to import
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    //Read the file content
                    string[] lines = System.IO.File.ReadAllLines(filePath);
                    int totalMarks = 0;

                    if (lines.Length == 0)
                    {
                        MessageBox.Show("The selected file is empty");
                        return;
                    }

                    string title = string.Empty;
                    string topic = string.Empty;
                    int level1 = -1;
                    int level2 = -1;
                    int level3 = -1;
                    int level4 = -1;
                    var questions = new List<Question>();

                    //Parse the file content
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string line = lines[i];

                        if (line.StartsWith("Quiz Title:"))
                        {
                            title = line.Replace("Quiz Title:", "").Trim();
                        }
                        else if (line.StartsWith("Topic:"))
                        {
                            topic = line.Replace("Topic:", "").Trim();
                        }
                        else if (line.StartsWith("Level 1:"))
                        {
                            level1 = int.Parse(line.Replace("Level 1:", "").Trim());
                        }
                        else if (line.StartsWith("Level 2:"))
                        {
                            level2 = int.Parse(line.Replace("Level 2:", "").Trim());
                        }
                        else if (line.StartsWith("Level 3:"))
                        {
                            level3 = int.Parse(line.Replace("Level 3:", "").Trim());
                        }
                        else if (line.StartsWith("Level 4:"))
                        {
                            level4 = int.Parse(line.Replace("Level 4:", "").Trim());
                        }
                        else if (line.StartsWith("Question:"))
                        {
                            totalMarks++;
                            var question = new Question
                            {
                                Text = line.Replace("Question:", "").Trim(),
                            };

                            i++;
                            if (lines[i].StartsWith("Type:"))
                            {
                                question.Type = lines[i].Replace("Type:", "").Trim();
                            }

                            if (question.Type == "MultipleChoice")
                            {
                                question.Options = new List<string>();
                                for (int j = 0; j < 4; j++)
                                {
                                    i++;
                                    question.Options.Add(lines[i].Substring(3).Trim());
                                }
                                i++;
                                question.CorrectAnswerIndex = int.Parse(lines[i].Replace("correctanswerindex.", "").Trim());
                            }
                            else if (question.Type == "SingleChoice")
                            {
                                i++;
                                question.Answer = lines[i].Replace("Answer:", "").Trim();
                            }

                            questions.Add(question);
                        }
                    }

                    //Insert the quiz into the database
                    string connectionString = GlobalVariables.Connection;

                    using (var connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();

                        var command = connection.CreateCommand();

                        command.CommandText = "SELECT COUNT(*) from quiz WHERE title = @title";
                        command.Parameters.AddWithValue("@title", title);
                        int quizCount = Convert.ToInt32(command.ExecuteScalar());

                        if (quizCount > 0)
                        {
                            MessageBox.Show("Quiz title already exists");
                            return;
                        }

                        //Creates a transaction for error validation (will revert everything if something goes wrong during import phase)
                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                
                                command.Transaction = transaction;

                                //Insert quiz details
                                command.CommandText = "INSERT INTO quiz (title, topic, level1, level2, level3, level4, teacherid, totalmarks) VALUES (@title, @topic, @level1, @level2, @level3, @level4, @teacherid, @totalmarks)";
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@title", title);
                                command.Parameters.AddWithValue("@topic", topic);
                                command.Parameters.AddWithValue("@level1", level1);
                                command.Parameters.AddWithValue("@level2", level2);
                                command.Parameters.AddWithValue("@level3", level3);
                                command.Parameters.AddWithValue("@level4", level4);
                                command.Parameters.AddWithValue("@teacherid", GlobalVariables.UserId);
                                command.Parameters.AddWithValue("@totalmarks", totalMarks);

                                command.ExecuteNonQuery();
                                int quizId = (int)command.LastInsertedId;

                                //Insert questions
                                foreach (var question in questions)
                                {
                                    command.CommandText = "INSERT INTO question (quizid, questiontext, questiontype) VALUES (@quizid, @questiontext, @questiontype)";
                                    command.Parameters.Clear();
                                    command.Parameters.AddWithValue("@quizid", quizId);
                                    command.Parameters.AddWithValue("@questiontext", question.Text);
                                    command.Parameters.AddWithValue("@questiontype", question.Type);

                                    command.ExecuteNonQuery();
                                    int questionId = (int)command.LastInsertedId;

                                    if (question.Type == "MultipleChoice")
                                    {
                                        command.CommandText = "INSERT INTO multiplechoicequestion (questionid, option1, option2, option3, option4, correctanswerindex) VALUES (@questionid, @option1, @option2, @option3, @option4, @correctanswerindex)";
                                        command.Parameters.Clear();
                                        command.Parameters.AddWithValue("@questionid", questionId);
                                        command.Parameters.AddWithValue("@option1", question.Options[0]);
                                        command.Parameters.AddWithValue("@option2", question.Options[1]);
                                        command.Parameters.AddWithValue("@option3", question.Options[2]);
                                        command.Parameters.AddWithValue("@option4", question.Options[3]);
                                        command.Parameters.AddWithValue("@correctanswerindex", question.CorrectAnswerIndex);

                                        command.ExecuteNonQuery();
                                    }
                                    else if (question.Type == "SingleChoice")
                                    {
                                        command.CommandText = "INSERT INTO singlechoicequestion (questionid, scanswer) VALUES (@questionid, @scanswer)";
                                        command.Parameters.Clear();
                                        command.Parameters.AddWithValue("@questionid", questionId);
                                        command.Parameters.AddWithValue("@scanswer", question.Answer);

                                        command.ExecuteNonQuery();
                                    }
                                }

                                //Commits the changes
                                transaction.Commit();
                                MessageBox.Show("Quiz imported successfully");
                                
                                LoadQuizzes(-1);
                            }
                            catch (Exception ex)
                            {
                                //Reverts changes
                                transaction.Rollback();
                                MessageBox.Show($"Error importing quiz: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading file: {ex.Message}");
                }
                return;
            }
        }

        //Searches for a quiz by id
        private void btnSearchQuiz_Click(object sender, RoutedEventArgs e)
        {
            int quizId;

            //Ensures a valid id is entered
            if (int.TryParse(txtIdSearch.Text, out quizId))
            {
                var quiz = quizTree.SearchQuiz(quizId);
                if (quiz != null)
                {
                    LoadQuizzes(quizId);
                }
                else
                {
                    MessageBox.Show("Quiz not found");
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid quiz ID");
            }
        }

        public class TreeNode
        {
            //Defines a node
            public Quiz Data { get; set; }
            public TreeNode Left { get; set; }
            public TreeNode Right { get; set; }

            //Sets up pointers
            public TreeNode(Quiz data)
            {
                Data = data;
                Left = null;
                Right = null;
            }
        }

        public class BinarySearchTree
        {
            private TreeNode root;

            //If a quiz doesn't exist at root node, then create a new root node
            public void InsertQuiz(Quiz data)
            {
                if (root == null)
                {
                    root = new TreeNode(data);
                }
                else
                {
                    InsertRecursively(data, root);
                }
            }

            //Recursively insert new quizzes into the tree
            public void InsertRecursively(Quiz data, TreeNode root)
            {
                if (data.Id < root.Data.Id)
                {
                    if (root.Left == null)
                    {
                        root.Left = new TreeNode(data); 
                    }
                    else
                    {
                        InsertRecursively(data, root.Left);
                    }
                }
                else
                {
                    if (root.Right == null)
                    {
                        root.Right = new TreeNode(data);
                    }
                    else
                    {
                        InsertRecursively(data, root.Right);
                    }
                }
            }

            public Quiz SearchQuiz(int id)
            {
                return SearchRecursively(root, id);
            }

            public Quiz SearchRecursively(TreeNode root, int id)
            {
                //Checks if the root node is null or id matches up
                if (root == null|| root.Data.Id == id)
                {
                    //returns null if root is null, otherwise it returns the quiz id
                    return root?.Data;
                }

                if (id < root.Data?.Id) 
                {
                    return SearchRecursively(root.Left, id);
                }
                else
                {
                    return SearchRecursively(root.Right, id);
                }
            }
        }

        //Clears the search and resets quiz list
        private void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtIdSearch.Text = string.Empty;
            LoadQuizzes(-1);
        }
    }
}
