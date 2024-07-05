using MySql.Data.MySqlClient;
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
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //Global variables
        public static class GlobalVariables
        {
            public static int UserId { get; set; }
        }
        
        //Places userid into global variable
        private void FetchUserId(string username, string password)
        {
            string connectionString = "server=127.0.0.1;uid=root;pwd=;database=quizsystem;SslMode=Required;";

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    command.CommandText = "SELECT id FROM user WHERE username=@username AND password=@password";
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);

                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        GlobalVariables.UserId = Convert.ToInt32(result);
                    }
                    else
                    {
                        GlobalVariables.UserId = 0;
                    }
                }
                catch (Exception ex)
                {
                    GlobalVariables.UserId = 0;
                    MessageBox.Show($"Error fetching user ID: {ex.Message}");
                    return;
                }
            }
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            //Create register page
            Register Register = new Register(); 
            this.Hide();

            Register.Show();
            this.Show();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            string connectionString = "server=127.0.0.1;uid=root;pwd=;database=quizsystem;SslMode=Required;";

            //Open database connection
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    //Retrieves user from database
                    command.CommandText = "SELECT COUNT(1) FROM user WHERE username=@username AND password=@password";
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);

                    int result = Convert.ToInt32(command.ExecuteScalar());

                    if (result == 1)
                    {
                        //Retrieves role from user
                        command.CommandText = "SELECT role FROM user WHERE username=@username AND password=@password";
                        
                        //Clears parameters and adds them again
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);

                        string role = (string)command.ExecuteScalar();

                        //Checks if user is a newuser
                        command.CommandText = "SELECT newuser FROM userbaseline INNER JOIN user ON user.id = userbaseline.userid WHERE user.username=@username";
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@username", username);

                        bool isNewUser = Convert.ToBoolean(command.ExecuteScalar());

                        //Checks user role
                        switch (role)
                        {
                            case "student":
                                //Opens menu for student
                                if (isNewUser)
                                {
                                    FetchUserId(username, password);

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
                                MessageBox.Show("Could not establish user role.");
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
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}