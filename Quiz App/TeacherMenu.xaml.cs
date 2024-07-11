using System;
using System.Data;
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

namespace Quiz_App
{
    /// <summary>
    /// Interaction logic for TeacherMenu.xaml
    /// </summary>
    public partial class TeacherMenu : Window
    {
        private int selectedUserId;
        private DataTable dataTable = new DataTable();

        public TeacherMenu()
        {
            InitializeComponent();
            LoadStudentData();
            GetStudentCount();
        }

        private void btnCreateQuiz_Click(object sender, RoutedEventArgs e)
        {
            AddQuiz addQuiz = new AddQuiz();
            this.Hide();

            addQuiz.Show();
            this.Show();
        }

        //Loads students into table
        private void LoadStudentData()
        {
            string connectionString = "server=127.0.0.1;uid=root;pwd=;database=quizsystem;SslMode=Required;";

            DataTable dataTable = new DataTable();

            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();

                    //Fills dataTable with students
                    command.CommandText = @"SELECT user.id AS UserId ,user.username AS Username, userbaseline.level AS Level FROM user INNER JOIN userbaseline ON user.id = userbaseline.userid WHERE user.role = 'student' ORDER BY user.username ASC;";

                    using (var adapter = new MySqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
                catch (Exception ex) 
                {
                    MessageBox.Show(ex.Message);
                }
            }
            //Display data inside table
            dtgStudentList.ItemsSource = dataTable.DefaultView;
        }

        private void dtgStudentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Manages changing user level
            if (dtgStudentList.SelectedItem is DataRowView row)
            {
                //Retrieves userid and sets changelevel combobox value
                selectedUserId = Convert.ToInt32(row["UserId"]);
                cbbChangeLevel.SelectedIndex = Convert.ToInt32(row["Level"]) - 1;
            }
        }

        private void btnSubmitLevel_Click(object sender, RoutedEventArgs e)
        {
            //Casts cbbChangeLevel.SelectedItem to selectedItem
            if (cbbChangeLevel.SelectedItem is ComboBoxItem selectedItem)
            {
                //Sets newLevel to the selected item 
                int newLevel = Convert.ToInt32(selectedItem.Content);

                string connectionString = "server=127.0.0.1;uid=root;pwd=;database=quizsystem;SslMode=Required;";
                using (var connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        //Update the database with new level
                        connection.Open();
                        var command = connection.CreateCommand();
                        command.CommandText = "UPDATE userbaseline SET level = @level, newuser = 0 WHERE userid = @userId";
                        command.Parameters.AddWithValue("@level", newLevel);
                        command.Parameters.AddWithValue("@userId", selectedUserId);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("User level updated successfully");
                            LoadStudentData();
                        }
                        else
                        {
                            MessageBox.Show("No rows updated");
                        }
                        //Clears combobox once submitted
                        cbbChangeLevel.SelectedIndex = -1;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        //Counts the number of registered students
        private void GetStudentCount()
        {
            int studentCount = 0;
            string connectionString = "server=127.0.0.1;uid=root;pwd=;database=quizsystem;SslMode=Required;";

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT COUNT(*) FROM user INNER JOIN userbaseline ON user.id = userbaseline.userid WHERE user.role = 'student'";

                    object result = command.ExecuteScalar();

                    //Casts the number of students to studentCount
                    if (result != null && result != DBNull.Value)
                    {
                        studentCount = Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //Updates StudentCount label
            lblStudentCount.Content = $"Student Count: {studentCount}";
        }

        private void btnCancelSelection_Click(object sender, RoutedEventArgs e)
        {
            // Clear selection in DataGrid
            dtgStudentList.SelectedItem = null;

            // Clear selection in ComboBox
            cbbChangeLevel.SelectedIndex = -1;
        }

        //Search for user
        private void btnSearchUser_Click(object sender, RoutedEventArgs e)
        {
            if (dtgStudentList.ItemsSource is DataView dataView)
            {
                string searchUser = $"Username LIKE '%{txtSearchUser.Text}%'";
                dataView.RowFilter = searchUser;
            }
            else
            {
                MessageBox.Show("Dataview is null");
            }
        }

        //Clears search if user cancelled
        private void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearchUser.Text = string.Empty;

            if (dtgStudentList.ItemsSource is DataView dataView)
            {
                dataView.RowFilter = string.Empty;
            }
            else
            {
                MessageBox.Show("Dataview is null");
            }
        }

        private void btnEditQuiz_Click(object sender, RoutedEventArgs e)
        {
            QuizSelectorEdit QuizSelectorEdit = new QuizSelectorEdit();
            this.Hide();

            QuizSelectorEdit.Show();
            this.Show();
        }
    }
}
