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
using static Quiz_App.AddQuiz;
using static Quiz_App.MainWindow;

namespace Quiz_App
{
    /// <summary>
    /// Interaction logic for SelectFlashcardType.xaml
    /// </summary>
    public partial class SelectFlashcardType : Window
    {
        bool randomiseQuestions = false;

        public SelectFlashcardType()
        {
            InitializeComponent();
        }

        private void btnAllQuestions_Click(object sender, RoutedEventArgs e)
        {
            //Opens flashcard window with arguements
            StudentFlashcardWindow StudentFlashcardWindow= new StudentFlashcardWindow("AllQuestions", randomiseQuestions, -1);
            StudentFlashcardWindow.Show();
            this.Close();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            randomiseQuestions = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            randomiseQuestions = false;
        }

        private void btnPreviousQuizzes_Click(object sender, RoutedEventArgs e)
        {
            //Opens flashcard window with arguements
            StudentSelectQuiz StudentSelectQuiz = new StudentSelectQuiz("PreviousQuizzes", randomiseQuestions);
            StudentSelectQuiz.Show();
            
        }

        private void btnWrongQuizAnswers_Click(object sender, RoutedEventArgs e)
        {
            //Opens flashcard window with arguements
            StudentFlashcardWindow StudentFlashcardWindow = new StudentFlashcardWindow("WrongQuizAnswers", randomiseQuestions, -1);
            StudentFlashcardWindow.Show();
            this.Close();
        }
    }
}
