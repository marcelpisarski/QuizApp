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
    /// Interaction logic for StudentMenu.xaml
    /// </summary>
    public partial class StudentMenu : Window
    {
        public StudentMenu()
        {
            InitializeComponent();
        }

        //Opens window where students can select a quiz
        private void btnOpenQuizzes_Click(object sender, RoutedEventArgs e)
        {
            StudentSelectQuiz StudentSelectQuiz = new StudentSelectQuiz("StartQuiz", false);
            this.Hide();

            StudentSelectQuiz.Show();
            this.Show();
        }

        //Opens window where students can select flashcards
        private void btnOpenFlashcards_Click(object sender, RoutedEventArgs e)
        {
            SelectFlashcardType SelectFlashcardType = new SelectFlashcardType();
            this.Hide();

            SelectFlashcardType.Show();
            this.Show();
        }

        private void btnViewResults_Click(object sender, RoutedEventArgs e)
        {          
            StudentResults StudentResults = new StudentResults(GlobalVariables.UserId);
            this.Hide();
            
            StudentResults.Show();
            this.Show();
        }
    }
}
