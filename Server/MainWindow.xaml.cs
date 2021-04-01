using SimpleTCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Server
{
    public partial class MainWindow : Window
    {
        //Where the files will be stores
        string currentPath = "C:\\Chat Application files";

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

        List<ClientDetails> listOfClients = new List<ClientDetails>();

        public MainWindow()
        {
            InitializeComponent();
        }
        //Create the server variable
        SimpleTcpServer server;

        private void ServerLoaded(object sender, RoutedEventArgs e)
        {
            server = new SimpleTcpServer();
            server.Delimiter = 0x13; //enter key
            server.StringEncoder = Encoding.UTF8;
            server.DataReceived += Server_DataReceived;

            txtStatus.IsReadOnly = true;
            txtHost.IsReadOnly = true;
            txtPort.IsReadOnly = true;
        }

        private void Server_DataReceived(object sender, SimpleTCP.Message e)
        {
            string listToBeSent = "list:All:";

            if (e.MessageString.Contains("just joined"))
            {
                String user = e.MessageString.Substring(25, e.MessageString.Length - 25 - 14);
                //String user = e.MessageString.Substring(4, e.MessageString.Length- 17);
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
                    } else
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

            if (e.MessageString.Contains("closed"))
            {
                string username = e.MessageString.ToString().Split(':')[0];

                
                for (int i = 0; i < listOfClients.Count(); i++)
                {
                    if (listOfClients[i].getUsername() == username)
                        listOfClients.RemoveAt(i);
                }

                server.Broadcast("                         " + username + " just left!\n");
                //server.Broadcast(">>> " + username + " just left!<<<\n");//chat log

                txtStatus.Dispatcher.Invoke((Action)delegate ()
                {
                    txtStatus.Text += "                         " + username + " just left!\n";
                    //txtStatus.Text +=">>> " + username + " just left!<<<\n";//Server log
                });

            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            IPAddress ip = IPAddress.Parse(txtHost.Text);
            server.Start(ip, Convert.ToInt32(txtPort.Text)); ;
            btnStart.IsEnabled = false; //disables the start button
            txtStatus.Text = "Server is up and running!\n";
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (server.IsStarted)
            {
                server.Stop();
                btnStart.IsEnabled = true; //re enables the start button
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
    }
}
