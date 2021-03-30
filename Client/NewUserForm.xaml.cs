using System;
using System.Windows;
using MySql.Data.MySqlClient;

namespace Client
{
    /// <summary>
    /// Interaction logic for NewUserForm.xaml
    /// </summary>
    public partial class NewUserForm : Window
    {
        public NewUserForm()
        {
            InitializeComponent();
        }

        private void Submit(object sender, RoutedEventArgs e)
        {
            String newUser = this.newuser.Text;
            String newPassword = this.newpassword.Password;

            NewUserCreation(newUser, newPassword);
        }

        private void NewUserCreation(String username, String password)
        {
            String check = null;

            String connectionString = "datasource = localhost; username = root; password =; database = loginnames";
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
    }

}
