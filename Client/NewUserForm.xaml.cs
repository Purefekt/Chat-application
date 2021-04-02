using System;
using System.Windows;
using System.Windows.Input;
using MySql.Data.MySqlClient;

namespace Client
{
    public partial class NewUserForm : Window
    {
        public NewUserForm()
        {
            InitializeComponent();
        }

        private void Submit(object sender, RoutedEventArgs e) //When the user presses the submit button
        {
            String newUser = this.newuser.Text;
            String newPassword = this.newpassword.Password;

            NewUserCreation(newUser, newPassword);
        }
        
        private void NewUserCreation(String username, String password) //Creates a new user and saves the data to the local database
        {
            String check = null;

            String connectionString = "datasource = localhost; username = root; password = 1234; database = loginnames";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd;
            MySqlDataReader reader;

            try
            {
                connection.Open();
                cmd = new MySqlCommand("SELECT * FROM users WHERE username = '" + username + "';", connection);
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    check = reader[0].ToString();
                }

                if (check != null)
                {
                    MessageBox.Show("username already exists!");
                    this.newuser.Text = "";
                    this.newpassword.Password = "";
                }
                else
                {
                    reader.Close();
                    cmd = new MySqlCommand("INSERT into users VALUES('" + username + "','" + password + "' ,'0');", connection);
                    cmd.ExecuteNonQuery();
                    connection.Close();
                    MessageBox.Show("Successful Creation of username " + username + "!");

                    MainWindow backtoNorm = new MainWindow();
                    backtoNorm.Show();
                    Close();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) //Lets us drag the window with the mouse
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void ClearUsername(object sender, RoutedEventArgs e) //Clears the username when clicked on
        {
            this.newuser.Text = "";
        }

        private void ClearPassword(object sender, MouseButtonEventArgs e) //Clears the password box when clicked on
        {
            this.newpassword.Password = "";
        }

        private void GoBackToLogin(object sender, RoutedEventArgs e) //Opens the MainWindow when user presses the back button
        {
            MainWindow goBack = new MainWindow();
            goBack.Show();
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) //Closes the application when user clicks the X button
        {
            Close();
        }

    }

}
