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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using System.Data;
using System.Net;
using System.Net.Sockets;
using SimpleTCP;

namespace Client
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CheckLogin(string username, string password) //Checks the login info provided
        {
            string checkUser = null;
            string checkPassword = null;
            int isLogged = 0;

            //To create a connection to the local database
            String connectionString = "datasource = localhost; username = root; password = 1234; database = loginnames";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM users WHERE username = '" + username + "';", connection);
            MySqlDataReader reader;

            try
            {
                connection.Open();
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    checkUser = reader[0].ToString();
                    checkPassword = reader[1].ToString();
                    isLogged = (reader[2].ToString() == "1" ? 1 : 0);
                }

                reader.Close();

                if (checkUser != null && checkUser.Equals(username) && checkPassword.Equals(password) && isLogged == 0)
                {
                    MySqlCommand upd = new MySqlCommand("UPDATE users SET isLogged='1' WHERE username='" + username + "';", connection);
                    upd.ExecuteNonQuery();
                    OpenChatWindow();
                }
                else
                {
                    if (isLogged == 1) MessageBox.Show("User already logged in!");
                    else
                    {
                        MessageBox.Show("Incorrect user or pass!");
                        this.usernameBox.Text = ""; //Changes username box to blank
                        this.passwordBox.Password = ""; //Changes password box to blank
                    }
                }

                connection.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Login (object sender, RoutedEventArgs e) //Sends the username and password information to check
        {
            String user = this.usernameBox.Text;
            String pass = this.passwordBox.Password;

            CheckLogin(user, pass);
        }

        private void ClearUsername(object sender, RoutedEventArgs e) //Clears the username box when clicked on
        {
            this.usernameBox.Text = ""; 
        }

        private void ClearPassword(object sender, MouseButtonEventArgs e) //Clears the password box when clicked on
        {
            this.passwordBox.Password = "";
        }

        private void UserForm(object sender, RoutedEventArgs e) //Takes the user to the NewUserForm
        {
            NewUserForm newUser = new NewUserForm();
            newUser.Show();
            Close();
        }

        private void OpenChatWindow() //On successful login takes the user to the ChatWindow
        {
            ChatWindow chatWindow = new ChatWindow(this.usernameBox.Text);
            chatWindow.Show();
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) //Lets us drag the window with the mouse
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) //Closes the application when we click the X button
        {
            Close();
        }

    }
}