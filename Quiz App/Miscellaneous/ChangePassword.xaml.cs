using System;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;
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

namespace Quiz_App.Miscellaneous
{
    /// <summary>
    /// Interaction logic for ChangePassword.xaml
    /// </summary>
    public partial class ChangePassword : Window
    {
        public ChangePassword()
        {
            InitializeComponent();
        }

        //Closes window
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //Changes current user password
        private void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string oldPassword = txtOldPassword.Text;
            string newPassword = txtNewPassword.Text;
            string confirmedPassword = txtConfirmPassword.Text;

            if (oldPassword.Length == 0 || newPassword.Length == 0 || confirmedPassword.Length == 0)
            {
                MessageBox.Show("Missing values");
                return;
            }

            if (newPassword != confirmedPassword)
            {
                MessageBox.Show("Passwords do not match. Please try again");
                return;
            }

            //Confirms if the current user password and entered old user password match up
            bool isConfirmed = ConfirmOldPassword(oldPassword);
            if (isConfirmed)
            {
                UpdateUserPassword(newPassword);
            }
            else
            {
                MessageBox.Show("Please enter the correct old password");
                return;
            }
        }

        private bool ConfirmOldPassword(string oldPass)
        {
            string enteredHashedOldPassword = HashPassword(oldPass);
            string currentUserPassword = string.Empty;

            string connectionString = GlobalVariables.Connection;
            int userId = GlobalVariables.UserId;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    command.CommandText = "SELECT password FROM user WHERE id = @id";
                    command.Parameters.AddWithValue("@id", userId);

                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        currentUserPassword = result.ToString();
                    }
                    else
                    {
                        return false;
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error confirming old password: {ex.Message}");
                }
            }

            //Returns true if both passwords match up
            return currentUserPassword == enteredHashedOldPassword;
        }

        //Updates the new user password
        private void UpdateUserPassword(string newPassword)
        {
            string hashedNewPassword = HashPassword(newPassword);
            
            string connectionString = GlobalVariables.Connection;
            int userId = GlobalVariables.UserId;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    command.CommandText = @"UPDATE user SET password = @hashedNewPassword WHERE id = @id";
                    command.Parameters.AddWithValue("@hashedNewPassword", hashedNewPassword);
                    command.Parameters.AddWithValue("@id", userId);

                    var result = command.ExecuteNonQuery();

                    if (result == 1)
                    {
                        MessageBox.Show("Password successfully changed");
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Could not change password");
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error changing password: {ex.Message}");
                }
            }
        }

        //Hashes the password
        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                //Creates hash
                byte[] userData = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                //Create a new StringBuilder to collect the bytes and create a string
                var stringBuilder = new StringBuilder();

                //Loop through each byte of the hashed data and format as hexidecimal string
                for (int i = 0; i < userData.Length; i++)
                {
                    stringBuilder.Append(userData[i].ToString("x2"));
                }

                //Returns the string
                return stringBuilder.ToString();
            }
        }
    }
}
