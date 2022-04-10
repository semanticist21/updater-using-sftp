# updater-using-sftp

## About the project

This project is very simple project for updating files from sftp server.
It makes update version management easy with a few settings.
It is not recommend to use where security is required.

+ It compares local files from sftp server, and then it brings newer files from the server and updates them with local files.
+ You can set file names not to update or exclude from search.
+ It is open-source, free to edit.

---
## Getting Started

### Prerequisites

This program was developed via WPF, .Net 6.0 Framwork, sftp client library.

+ This project was built based on .Net 6.0, so .Net 6.0 or higher version is required for use.
+ Supported OS : Windows

### Screen Description

![kakao2](https://user-images.githubusercontent.com/92710478/162603839-c111c55d-633e-4706-bc52-7889fc8204d9.png)

For specific usage, see below __Usage__ Section.

+ Auto : it tries to connect sftp server base on info in app.config, and fetch file info from the server. If it has any newer file, it begins updating files int othe local directory. After completion, it tries to run executable file in combo box.
+ Connect : it tries to connect sftp server base on the info given, and fetch newer files info and list them on the list box.
+ Update : if it has any files in the list box, it begins updating files into the local directory.
+ Run Project : it run executable file in the combo box.
+ Options : open new option popup.
+ Open Local Folder : open local folder directory where the program is refrencing(usually parent tree from the updater folder).
+ Exit : exit the program. It tries to dispose the connection manager.

### Usage

First, download code and then build the project.

After build, Put all updater files into one folder (Debug Or Release). 
Then, position it within the same parent directory where there is project folders you want to update.
for example, if you have 2 project folder to update, you will have 3 folder in the same directory.

1. .../some directory/_Your Project1_
2. .../some directory/_Your Project2_
3. .../some directory/_Updater Project Folder_

Also, you do the same thing for sftp server. 
Updater folder is not required. Parent directory can be different from the local parent of folders(here, _some directory_).

1. .../some server directory/_Your Project1_
2. .../some server directory/_Your Project2_

After that, let's start setting options By clicking _Options_ in the menu.
Any information written here will be all saved to app.config. So you can directly edit the config file.
If enough information is not provied for the program, It might cause errors.

![kakao1](https://user-images.githubusercontent.com/92710478/162604573-f848165d-754a-47a0-a6b8-c3ae364c375a.png)

### General Configs

It is essential to fill all options here. 

#### 1. Connection
Set connection info here. This project only uses __SFTP protocol__

+ Ip Address
+ Port 
+ User
+ Password

#### 2. Directories
Set server directory, and folder names for search. 

+ SFTP server base directory
+ target folder names

the logic is simple. let's continue with the case above, 2 project folders to update.
you have the following as.. 

[ Window local PC ] 
1. C:/some folder/_project1_
2. C:/some folder/_project2_
3. C:/some folder/_updater_

_the base local directory_ is automatically set on "C:/some folder/".

[ sftp server ]
In the server, you have the following as..

1. /some server folder/_project1_
2. /some server folder/_project2_

Then, _sftp server base directory_ should be "/some server folder/".
_Target Folder Names_ should be "project1;project2"

#### 3. Customs
It is recommed to be disabled, if you have not set any configs yet.
before enabling it, you should run _Auto_ command in the menu by yourself.

+ Auto Updtae On Run

### Excludes
Options for exclude in the update process.

+ Folder Names Not To Update
+ File Names Not To Update

It excludes files, folders when making update list.
for example, you set
1. _Folder Names To Update_ : "debug;obj"
2. _File Names Not To Update_ : "project_1.config;app.config"

The Process will ignore __all the names__ in the child tree of the target folders.

These will be all ignored.

1. /some server folder/_project1_/.../__obj__
2. /some server folder/_project1_/__debug__
3. /some server folder/_project1_/.../__project_1.config__
4. /some server folder/_project1_/__app.config__

### Run Configs
Set executable files to run when clicked _Auto_ or _Run project_ in the menus.
Put only .exe file in the list.

+ File To Execute Directory
+ Selected File Index












