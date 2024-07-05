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

namespace Quiz_App
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Window
    {
        bool queryChecker = false;
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
            queryChecker = true;
        }

        //Disable teacher code textbox if unchecked
        private void lblTeacherQuery_UnChecked(object sender, RoutedEventArgs e)
        {
            txtTeacherCode.Visibility = Visibility.Collapsed;
            queryChecker = false;
        }

        private void btnRegisterUser_Click(object sender, RoutedEventArgs e)
        {
            //Read teacher school code from file
            string TEACHERCODE;
            try
            {
                //LOCAL FILEPATH TO TEACHER CODE
                string filePath = "C:\\Users\\marce\\source\\repos\\Quiz App\\Quiz App\\Text Files\\TeacherCode.txt";
                TEACHERCODE = File.ReadAllText(filePath).Trim();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            //Insures all textboxes have a value
            if (txtRegisterUsername.Text.Trim().Length == 0 || txtRegisterPassword.Password.Trim().Length == 0 || txtConfirmPassword.Password.Trim().Length == 0)
            {
                MessageBox.Show("Missing value");
                return;
            }

            //Default user role is student
            string userRole = "student";

            //Assigns user role
            if (queryChecker == true && TEACHERCODE != txtTeacherCode.Text)
            {
                MessageBox.Show("Invalid teacher code");
                return;
            }
            else if (queryChecker == false)
            {
                userRole = "student";
            }
            else if (queryChecker == true && txtTeacherCode.Text == TEACHERCODE)
            {
                userRole = "teacher";
            }

            //Ensures password is correct
            if (txtRegisterPassword.Password != txtConfirmPassword.Password)
            {
                MessageBox.Show("Passwords do not match. Try again.");
                return;
            }

            //Create connection to database
            string connectionString = "server=127.0.0.1;uid=root;pwd=;database=quizsystem;SslMode=Required;";

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    //Check if user already exists
                    command.CommandText = "SELECT COUNT(*) FROM user WHERE username = @username";
                    command.Parameters.AddWithValue("@username", txtRegisterUsername.Text);
                    int userCount = Convert.ToInt32(command.ExecuteScalar());

                    if (userCount > 0)
                    {
                        MessageBox.Show("Username already exists.");
                        return;
                    }

                    //Clear previous parameter
                    command.Parameters.Clear();

                    //Insert details into user table
                    command.CommandText = "INSERT INTO user(username,password, role) VALUES (@username, @password, @role); ";
                    command.Parameters.AddWithValue("@username", txtRegisterUsername.Text);
                    command.Parameters.AddWithValue("@password", txtRegisterPassword.Password);
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
    }
}
