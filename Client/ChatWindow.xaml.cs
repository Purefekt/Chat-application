using SimpleTCP;
using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using System.IO;
using MySql.Data.MySqlClient;

namespace Client
{
    public partial class ChatWindow : Window
    {
        private string username; 
        Boolean ifFileSelected = false; //boolean tells us if browse button is selected
        FileInfo fi; //stores file name and extension for the file to be saved
        SimpleTcpClient client; //Creates the client variable
        OpenFileDialog op; //Allows us to select files from the computer

        public ChatWindow(string usr)
        {
            InitializeComponent();

            username = usr;
            this.Title = usr; //sets title of window to the username

            string currentPath = "C:\\Chat Application files"; //Stores the files for the user in this directory

            //If the directory doesnt exist, then we make one with the name of the user
            if (!Directory.Exists(currentPath + "/" + username))
            {
                Directory.CreateDirectory(currentPath + "/" + username);
            }
        }

        private void ChatWindowLoaded(object sender, EventArgs e)
        {
            client = new SimpleTcpClient();
            client.StringEncoder = Encoding.UTF8;
            client.DataReceived += Client_DataReceived;
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                client.Connect("127.0.0.1", 1111);
                client.Write(">>> " + username + " just joined! <<<\n");
                btnConnect.IsEnabled = false;
                connectedToTheServerAs.Text = "You are connected to the server as: " + username ; //Connected to server as message
            }
            catch (Exception ex) //if the server is anactive but the user still presses the connect button
            {
                System.Windows.MessageBox.Show("Server is not active");
                btnConnect.IsEnabled = true;
            }
        }

        private void Client_DataReceived(object sender, SimpleTCP.Message e)
        {
            if (e.MessageString.Length > 4 && e.MessageString.Contains("list"))
            {
                String msg = e.MessageString.Substring(4, e.MessageString.Length - 4);
                string[] listClients = new string[msg.Split(':').Length - 2];
                int cp = 0;
                for (int i = 0; i < msg.Split(':').Length; i++)
                {
                    if (msg.Split(':')[i] != "" && msg.Split(':')[i] != this.username) {
                        listClients[cp] = msg.Split(':')[i];
                        cp++;
                    }
                }
                listbox.Items.Dispatcher.Invoke((Action)delegate ()
                {
                    listbox.ItemsSource = listClients;
                });
            }
            else
            {
                txtStatus.Dispatcher.Invoke((Action)delegate ()
                {
                    if (e.MessageString.Contains("just joined") && !e.MessageString.Contains(username))
                    {
                        txtStatus.Text += e.MessageString;
                    }
                    else if (e.MessageString.Contains("just joined") && e.MessageString.Contains(username))
                    {
                        txtStatus.Text += "";
                    }
                    else
                    {
                        txtStatus.Text += e.MessageString;
                    }
                });

            }

            if (e.MessageString.Equals("All"))
            {
                txtStatus.Dispatcher.Invoke((Action)delegate ()
                {
                    txtStatus.Text += "";
                });

            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (btnConnect.IsEnabled)
            {
                System.Windows.MessageBox.Show("Please connect to the local server!");
            }
            else if (listbox.SelectedIndex != -1)
            {
                if (txtMessage.Text.Length > 0 && !ifFileSelected && !txtMessage.Text.Equals(" ") && !txtMessage.Text.Equals("\n"))
                {
                    client.WriteLine(listbox.SelectedItem.ToString() + "#" + username + ": " + txtMessage.Text);
                }
                else if (ifFileSelected)
                {
                    byte[] b1 = File.ReadAllBytes(op.FileName);
                    client.Write(listbox.SelectedItem.ToString() + "#File#" + fi.Name + "#");
                    client.Write(b1);
                    ifFileSelected = false;
                }
                else
                {
                    System.Windows.MessageBox.Show("Please type something or choose a file to send!");
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Please select someone to send the message to!");
            }

            txtMessage.Text = ""; //Empties the text message box

        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e) //Browse button
        {
            if (!btnConnect.IsEnabled) //if connect button is not enabled which means we are connected to a server
            {
                op = new OpenFileDialog();
                if (op.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    fi = new FileInfo(op.FileName);
                    ifFileSelected = true;
                    txtMessage.Text = "Sending " + fi.Name;
                }
            } else
            {
                System.Windows.MessageBox.Show("Please connect to the local server!");
            }
        }

        private void txtMessage_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) //When we press the message text box
        {
            if (!btnConnect.IsEnabled)
                client.WriteLine("list"); //if the user is connected to a server, writes "list" to the server
            else
                System.Windows.MessageBox.Show("Please connect to the local server!");

        }

        private void Window_Closed(object sender, EventArgs e) //Logs off the user when window is closed
        {
            if (!btnConnect.IsEnabled) //if button is not pressed and if we we are connected
            {
                client.Write(username + ":closed");
                client.TcpClient.Close();
            }

            String connectionString = "datasource = localhost; username = root; password = 1234; database = loginnames";
            MySqlConnection connection = new MySqlConnection(connectionString);

            try
            {
                connection.Open();
                MySqlCommand upd = new MySqlCommand("UPDATE users SET isLogged='0' WHERE username='" + username + "';", connection);
                upd.ExecuteNonQuery();

                connection.Close();

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void Click_clear(object sender, EventArgs e) //Clears the chat log
        {
            txtStatus.Text = "";
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) //Lets the user drag window with mouse
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) //Closes the app when user clicks the X button, also logs out the user
        {
            String connectionString = "datasource = localhost; username = root; password = 1234; database = loginnames";
            MySqlConnection connection = new MySqlConnection(connectionString);

            try
            {
                connection.Open();
                MySqlCommand upd = new MySqlCommand("UPDATE users SET isLogged='0' WHERE username='" + username + "';", connection);
                upd.ExecuteNonQuery();

                connection.Close();

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            Close();
        }
    }
}
