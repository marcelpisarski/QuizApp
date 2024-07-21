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
    /// Interaction logic for StudentMenu.xaml
    /// </summary>
    public partial class StudentMenu : Window
    {
        public StudentMenu()
        {
            InitializeComponent();
        }

        private void btnOpenQuizzes_Click(object sender, RoutedEventArgs e)
        {
            StudentSelectQuiz StudentSelectQuiz = new StudentSelectQuiz("StartQuiz", false);
            this.Hide();

            StudentSelectQuiz.Show();
            this.Show();
        }

        private void btnOpenFlashcards_Click(object sender, RoutedEventArgs e)
        {
            SelectFlashcardType SelectFlashcardType = new SelectFlashcardType();
            this.Hide();

            SelectFlashcardType.Show();
            this.Show();
        }
    }
}
