using System.Collections.Generic;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;
using System.IO;
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
using Org.BouncyCastle.Bcpg;
using static Quiz_App.MainWindow;

namespace Quiz_App
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Window
    {
        bool isTeacher = false;

        public Register()
        {
            InitializeComponent();
            
            //Hides teachercode textbox by default
            txtTeacherCode.Visibility = Visibility.Collapsed;
        }    

        //Enabled teacher code textbox if checked
        private void lblTeacherQuery_Checked(object sender, RoutedEventArgs e)
        {
            txtTeacherCode.Visibility = Visibility.Visible;
            isTeacher = true;
        }

        //Disable teacher code textbox if unchecked
        private void lblTeacherQuery_UnChecked(object sender, RoutedEventArgs e)
        {
            txtTeacherCode.Visibility = Visibility.Collapsed;
            isTeacher = false;
        }

        private void btnRegisterUser_Click(object sender, RoutedEventArgs e)
        {
            string username = txtRegisterUsername.Text.Trim();
            string password = txtRegisterPassword.Password.Trim();
            string confirmedPassword = txtConfirmPassword.Password.Trim();
            string hashedPassword = HashPassword(password);
            string teacherCode = txtTeacherCode.Text.Trim();

            //Read teacher school code from file
            string TEACHERCODE;

            try
            {
                //Local filepath to teachercode
                string filePath = "C:\\Users\\marce\\source\\repos\\Quiz App\\Quiz App\\Text Files\\TeacherCode.txt";
                TEACHERCODE = File.ReadAllText(filePath).Trim();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            //Insures all textboxes have a value
            if (username.Length == 0 || password.Length == 0 || confirmedPassword.Length == 0)
            {
                MessageBox.Show("Missing value");
                return;
            }

            //Default user role is student
            string userRole = "student";

            //Assigns user role
            if (isTeacher == true && TEACHERCODE != teacherCode)
            {
                MessageBox.Show("Invalid teacher code");
                return;
            }
            else if (isTeacher == false)
            {
                userRole = "student";
            }
            else if (isTeacher == true && teacherCode == TEACHERCODE)
            {
                userRole = "teacher";
            }

            //Ensures password is correct
            if (password != confirmedPassword)
            {
                MessageBox.Show("Passwords do not match. Try again");
                return;
            }

            //Create connection to database
            string connectionString = GlobalVariables.Connection;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    //Check if user already exists
                    command.CommandText = "SELECT COUNT(*) FROM user WHERE username = @username";
                    command.Parameters.AddWithValue("@username", username);
                    int userCount = Convert.ToInt32(command.ExecuteScalar());

                    if (userCount > 0)
                    {
                        MessageBox.Show("Username already exists");
                        return;
                    }

                    //Clear previous parameter
                    command.Parameters.Clear();

                    //Insert details into user table
                    command.CommandText = "INSERT INTO user(username,password, role) VALUES (@username, @password, @role); ";
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", hashedPassword);
                    command.Parameters.AddWithValue("@role", userRole);

                    int result = command.ExecuteNonQuery();
                    if (result == 1)
                    {
                        //If role is student, then given a new user role
                        if (userRole == "student")
                        {
                            command.CommandText = "SELECT LAST_INSERT_ID();";
                            int userId = Convert.ToInt32(command.ExecuteScalar());

                            // Insert into userbaseline table
                            command.CommandText = "INSERT INTO userbaseline (userid, level) VALUES (@userid, 0);";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@userid", userId);

                            int userBaselineResult = command.ExecuteNonQuery();

                            if (userBaselineResult == 1)
                            {
                                MessageBox.Show("Registration successful");

                                this.Hide();
                            }
                            else
                            {
                                MessageBox.Show("Failed to insert into userbaseline table");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Registration successful");

                            this.Hide();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Register failed");
                    }
                }    
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        //Hashes the password
        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                //Convert the input string to an array of bytes and create the hash
                byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                //Create a new StringBuilder to collect the bytes and create a string
                var sBuilder = new StringBuilder();

                //Loop through each byte of the hashed data and format each one as a hexadecimal string
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Returns the string
                return sBuilder.ToString();
            }
        }
    }
}
