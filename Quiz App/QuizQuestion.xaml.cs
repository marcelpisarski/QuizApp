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
            }
            else if (rbSingleAnswer.IsChecked == true)
            {
                if (txtSingleAnswer.Text.Trim().Length == 0)
                {
                    MessageBox.Show("Please enter an answer");
                    return;
                }
            }
        }
    }
}
