using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Quiz_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    { 
        //Global variables
        public static class GlobalVariables
        {
            public static int UserId { get; set; }
            public static int QuizId { get; set; }
            public static readonly string Connection = "server=127.0.0.1;uid=root;pwd=;database=quizsystem;SslMode=Required;";
        }

        public MainWindow()
        {

            InitializeComponent();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            //Create register page
            Register Register = new Register(); 
            this.Hide();

            Register.Show();
            this.Show();
        }

        //Log user in
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;
            string hashedPassword = HashPassword(password);

            string connectionString = GlobalVariables.Connection;

            //Open database connection
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    //Retrieves user from database
                    command.CommandText = "SELECT COUNT(*) FROM user WHERE username=@username AND password=@password";
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", hashedPassword);

                    var result = Convert.ToInt32(command.ExecuteScalar());

                    if (result == 1)
                    {
                        //Retrieves role from user
                        command.CommandText = "SELECT role FROM user WHERE username=@username AND password=@password";
                        
                        //Clears parameters and adds them again
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", hashedPassword);

                        string userRole = (string)command.ExecuteScalar();

                        //Checks if user is a newuser
                        command.CommandText = "SELECT newuser FROM userbaseline INNER JOIN user ON user.id = userbaseline.userid WHERE user.username=@username";
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@username", username);

                        bool isNewUser = Convert.ToBoolean(command.ExecuteScalar());
                        FetchUserId(username, hashedPassword);

                        //Checks user role
                        switch (userRole)
                        {
                            case "student":
                                //Opens menu for student
                                if (isNewUser)
                                {
                                    StudentBaseline StudentBaseline = new StudentBaseline();
                                    MessageBox.Show("Login successful");
                                    this.Hide();

                                    StudentBaseline.Show();
                                    this.Close();
                                }
                                else
                                {
                                    StudentMenu StudentMenu = new StudentMenu();
                                    MessageBox.Show("Login successful");
                                    this.Hide();

                                    StudentMenu.Show();
                                    this.Close();
                                }
                                break;
                            case "teacher":
                                //Opens teacher menu
                                TeacherMenu TeacherMenu = new TeacherMenu();
                                MessageBox.Show("Login successful");
                                this.Hide();

                                TeacherMenu.Show();
                                this.Close();

                                break;
                            default:
                                return;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Login failed");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error logging in user: {ex.Message}");
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

        //Places logged in userid into global variables
        private void FetchUserId(string username, string password)
        {
            string connectionString = GlobalVariables.Connection;

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    command.CommandText = "SELECT id FROM user WHERE username=@username AND password=@password";
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);

                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        GlobalVariables.UserId = Convert.ToInt32(result);
                    }
                    else
                    {
                        GlobalVariables.UserId = -1;
                    }
                }
                catch (Exception ex)
                {
                    GlobalVariables.UserId = -1;
                    MessageBox.Show($"Error fetching user id: {ex.Message}");
                    return;
                }
            }
        }
    }
}