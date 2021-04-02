using SimpleTCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Windows.Input;

namespace Server
{
    public partial class MainWindow : Window
    {
        string currentPath = "C:\\Chat Application files"; //where the files will be stores
        SimpleTcpServer server; //Create the server variable
        List<ClientDetails> listOfClients = new List<ClientDetails>();

        class ClientDetails
        {
            String username;
            Socket userSocket;
            public ClientDetails(string username, Socket userSocket)
            {
                this.username = username;
                this.userSocket = userSocket;
            }
            public String getUsername()
            {
                return this.username;
            }
            public Socket getSocket()
            {
                return this.userSocket;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ServerLoaded(object sender, RoutedEventArgs e)
        {
            server = new SimpleTcpServer();
            server.Delimiter = 0x13; //enter key
            server.StringEncoder = Encoding.UTF8;
            server.DataReceived += Server_DataReceived;
        }

        private void Server_DataReceived(object sender, SimpleTCP.Message e)
        {
            string listToBeSent = "list:All:";

            if (e.MessageString.Contains("just joined"))
            {
                String user = e.MessageString.Substring(4, e.MessageString.Length- 4 -  18);
                Socket socket = e.TcpClient.Client;
                listOfClients.Add(new ClientDetails(user, socket));
       
                server.Broadcast(e.MessageString);
                txtStatus.Dispatcher.Invoke((Action)delegate ()
                {
                    txtStatus.Text += e.MessageString;
                });
            }

            if (e.MessageString.Contains("list"))
            {
                foreach (ClientDetails username in listOfClients)
                {
                    listToBeSent += username.getUsername() + ":";
                }
                List<byte> vs = new List<byte>();
                vs.AddRange(Encoding.UTF8.GetBytes(listToBeSent));
                e.TcpClient.Client.Send(vs.ToArray());
            }

            if (e.MessageString.Contains("#"))
            {
                if (e.MessageString.Substring(0, 3).Equals("All"))
                {
                    if (!e.MessageString.Substring(4, 4).Equals("File"))
                    {
                        string toAll = "[All] " + e.MessageString.Substring(4, e.MessageString.Length - 4) + "\n";
                        server.Broadcast(toAll);
                        txtStatus.Dispatcher.Invoke((Action)delegate ()
                        {
                            txtStatus.Text += toAll;
                        });
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Cannot send a file to everyone!");
                    }
                }
                else
                {
                    int posOfDel = e.MessageString.IndexOf('#');  
                    string user = e.MessageString.Substring(0, posOfDel);
                    //if its not a file
                    if (!e.MessageString.Substring(user.Length+1, 4).Equals("File"))
                    {
                        string msg = "[Private] " + e.MessageString.Substring(posOfDel + 1, e.MessageString.Length - posOfDel - 1) + "\n";
                        Socket replySock = findSocket(user, listOfClients);
                        if (replySock != null)
                        {
                            List<byte> vs = new List<byte>();
                            vs.AddRange(Encoding.UTF8.GetBytes(msg));
                            replySock.Send(vs.ToArray());
                            e.TcpClient.Client.Send(vs.ToArray());
                        }
                        else
                        {
                            System.Windows.MessageBox.Show(user + " has logged out!");
                        }
                    }
                    //if its a file
                    else
                    {
                        string bigName = e.MessageString.Substring(user.Length + 6, e.MessageString.Length - user.Length - 6);
                        int posOfStar = bigName.IndexOf('#');
                        string fileName = bigName.Substring(0, posOfStar);

                        byte[] array = new byte[(e.MessageString.Length - user.Length - fileName.Length - 7)];
                        Buffer.BlockCopy(e.Data, user.Length + fileName.Length + 7, array, 0, array.Length);
                        File.WriteAllBytes(currentPath + "\\" + user + "\\" + fileName, array);
                        String srcUsername = findUsername(e.TcpClient.Client, listOfClients);
                        Socket fromSock = findSocket(user, listOfClients);
                        if (fromSock != null)
                        {
                            List<byte> vs = new List<byte>();
                            vs.AddRange(Encoding.UTF8.GetBytes("[Private] " + srcUsername + ": Sent file " + fileName + "\n"));
                            List<byte> vs2 = new List<byte>();
                            vs2.AddRange(Encoding.UTF8.GetBytes("[Private] " + srcUsername + ": Sent file " + fileName + " | Location: " + currentPath + "\\" + user + "\\" + fileName + "\n"));
                            fromSock.Send(vs2.ToArray());
                            e.TcpClient.Client.Send(vs.ToArray());
                        }
                    }
                }
            }

            //remove user from the list when user leaves
            if (e.MessageString.Contains("closed")) 
            {
                string username = e.MessageString.ToString().Split(':')[0];

                for (int i = 0; i < listOfClients.Count(); i++)
                {
                    if (listOfClients[i].getUsername() == username)
                        listOfClients.RemoveAt(i);
                }

                server.Broadcast(">>> " + username + " just left! <<<\n");//Chat log

                txtStatus.Dispatcher.Invoke((Action)delegate ()
                {
                    txtStatus.Text +=">>> " + username + " just left! <<<\n";//Server log
                });

            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e) //starts the server with the given ip and port and disables the start button
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            server.Start(ip, 1111); ;
            btnStart.IsEnabled = false;
            txtStatus.Text = "Server is up and running!\n";
        }

        private void btnStop_Click(object sender, RoutedEventArgs e) //if the server is on, stops it and re enables the start button
        {
            if (server.IsStarted)
            {
                server.Broadcast(">>>The server has crashed! Please close this window and try again later!<<<");
                server.Stop();
                btnStart.IsEnabled = true;
                txtStatus.Text = "Server stopped.\n";
            }
        }

        private Socket findSocket(string user, List<ClientDetails> list)
        {
            Socket userSocket;
            int counter = 0;

            foreach(ClientDetails details in list)
            {
                if (details.getUsername().Equals(user)) break;
                counter++;
            }

            ClientDetails resSock = list.ElementAt(counter);
            userSocket = resSock.getSocket();

            return userSocket;
        }

        private string findUsername(Socket socket, List<ClientDetails> list)
        {
            string user;
            int counter = 0;

            foreach (ClientDetails details in list)
            {
                if (details.getSocket().Equals(socket)) break;
                counter++;
            }

            ClientDetails resUser = list.ElementAt(counter);
            user = resUser.getUsername();

            return user;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) //Lets the user drag window with mouse
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) //Closes the app when user clicks the X button
        {
            Close();
        }
    }

}
