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
    /// Interaction logic for QuizQuestion.xaml
    /// </summary>
    public partial class QuizQuestion : Window
    {
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
    }
}
