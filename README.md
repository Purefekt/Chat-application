# Chat-application
This is a WPF chat application using a MySQL local database.

## Table of contents
1. [Requirements]()
2. [Setting up a local MySQL database]()

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

### Creating Client side application
This application will let the user login or create a new account and login and then connect to the server.  
There are 3 Windows in this application:
- MainWindow
- NewUserForm
- ChatWindow

#### MainWindow
This is the Login page. A user with a pre existing account can sign into the application with their username and password. Pressing the "LOGIN" button will take the user to the ChatWindow. If the username or password is incorrect an exception is thrown. If the username and password are correct but the user is already logged in, an exception is thrown.  
If the user does not have an account then he/she can register for an account by pressing the "REGISTER" button which takes the user to the NewUserForm.  
The user can also press the "X" button on the top right to close the client side application.

#### NewUserForm
This is where a new user can create an account. The user has to type his/her chosen username and password and enter into the fields given. When the user presses the "SUBMIT", the username and password are stored in the database, a messagebox with the message "Successful creation of username!" pops and pressing on the "ok" button closes the NewUserForm and opens the MainWindow where the user can use the newly created account to login. If the username is taken then an exception is thrown and the user must choose a different username.  
The user can go back to the MainWindow if he/she realised they already have an account by pressing the '↲' button on the top left corner.  
The user can close the application by pressing on the 'X' button in the top right corner.