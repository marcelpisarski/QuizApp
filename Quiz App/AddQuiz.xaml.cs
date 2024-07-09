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
using static Mysqlx.Notice.Warning.Types;

namespace Quiz_App
{
    /// <summary>
    /// Interaction logic for AddQuiz.xaml
    /// </summary>
    public partial class AddQuiz : Window
    {
        bool canCreateQuiz = false;
        public AddQuiz()
        {
            InitializeComponent();
            //Disables add question button until quiz schema is made
            btnAddQuestion.IsEnabled = false;
        }

        private void btnAddQuestion_Click(object sender, RoutedEventArgs e)
        {       
            QuizQuestion QuizQuestion = new QuizQuestion();
            QuizQuestion.Show();
            
            canCreateQuiz = true;
        }

        private void btnCancelQuiz_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSaveQuiz_Click(object sender, RoutedEventArgs e)
        {
            //Ensures quiz is valid
            if (!canCreateQuiz)
            {
                MessageBox.Show("Unable to create quiz. Ensure quiz is valid");
                return;
            }
        }

        private void btnCreateSchema_Click(object sender, RoutedEventArgs e)
        {
            //Shows error if title / topic name is empty
            if (txtQuizTitle.Text.Length == 0 || txtTopicName.Text.Length == 0)
            {
                MessageBox.Show("Quiz title and/or topic name is empty.");
                return;
            }

            //Ensures a quiz has a level selected
            if (cbLevel1.IsChecked == false && cbLevel2.IsChecked == false && cbLevel3.IsChecked == false && cbLevel4.IsChecked == false)
            {
                MessageBox.Show("Please select a quiz level.");
                return;
            }      
            btnAddQuestion.IsEnabled = true;
        }
    }
}
