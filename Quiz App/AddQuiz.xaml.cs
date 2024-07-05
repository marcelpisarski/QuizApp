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

namespace Quiz_App
{
    /// <summary>
    /// Interaction logic for AddQuiz.xaml
    /// </summary>
    public partial class AddQuiz : Window
    {
        public AddQuiz()
        {
            InitializeComponent();
        }

        private void btnAddQuestion_Click(object sender, RoutedEventArgs e)
        {
            QuizQuestion QuizQuestion = new QuizQuestion();
            QuizQuestion.Show();
        }

        private void btnCancelQuiz_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSaveQuiz_Click(object sender, RoutedEventArgs e)
        {
            //Shows error if title / topic name is empty
            if (txtQuizTitle.Text.Length == 0 || txtTopicName.Text.Length == 0)
            {
                MessageBox.Show("Quiz title and/or topic name is empty.");
                return;
            }
        }
    }
}
