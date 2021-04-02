# Chat-application
This is a wpf chat application. It lets users make accounts and log into their accounts to connect to a server. The server keeps a log of everyone who has entered and left the chat room. The users recieve a list of all other users also on the same server. The users can choose "All" which broadcasts the message to everyone, including the server log or users can send private messages to other users. These private messages are not tracked by even the server. The users are also allowed to send files to other users. The clear button lets the user clear the chat log. As and when people log in and out, the other users currently on the server are also updated about it. This chat application uses a local MySQL database to store the username, password and log in info. I use [SimpleTcp](https://github.com/BrandonPotter/SimpleTCP) for TCP sockets.

## Table of contents

1. [Requirements]()
2. [Setting up a local MySQL database]()
3. [Creating Client side application]()
4. 

### 1. Requirements
- [Visual Studio 2019 Community](https://visualstudio.microsoft.com/downloads/) - Download and install this free IDE, with the .NET desktop development workload. 
- [MySQL](https://dev.mysql.com/downloads/) - Download and install the MySQL installer for Windows and do the "Full" installation. Otherwise make sure you have the MySQL Connectors -> Connector/NET. Make sure port 3306 is not already in use, if it is in use you will have to reconfigure settings in the MySQL Installer. You can check if port 3306 is being used and by what by Win+R -> resmon.exe -> Network -> Listening Ports.

### 2. Setting up a local MySQL database
After the successful installation of MySQL, configure the root password. For me i set it as 1234. We will need this password when sending MySQL commands.  
Now we have to set up a local database. To do that open MySQL Workbench, go to the "Local instance MySQL80" under MySQL Connections. Here we can write SQL queries to make the database.  
```
create database loginnames;
```
This creates the database
```
use loginnames;
```
This tells SQL to use this database for all commands
```
create table users (username varchar(255), password varchar(255), isLogged varchar(255));
```
This creates a new table called users where we have 3 columns called username, password and isLogged, all of which are the type varchar
```
select * from users;
```
We can use this statement to check if the table is created properly.  
Our local database is ready.

### 3. Creating Client side application
This application will let the user login or create a new account and login and then connect to the server.  
There are 3 Windows in this application:
1. MainWindow
2. NewUserForm
3. ChatWindow

#### MainWindow -> add image


#### NewUserForm
- This is where a new user can create an account. The user has to type his/her chosen username and password and enter into the fields given. When the user presses the "SUBMIT", the username and password are stored in the database, a messagebox with the message "Successful creation of username!" pops and pressing on the "ok" button closes the NewUserForm and opens the MainWindow where the user can use the newly created account to login. If the username is taken then an exception is thrown and the user must choose a different username.
- The user can go back to the MainWindow if he/she realised they already have an account by pressing the '↲' button on the top left corner.
- The user can close the application by pressing on the 'X' button in the top right corner.

[Background gradient ideas](https://digitalsynopsis.com/design/beautiful-color-ui-gradients-backgrounds/)