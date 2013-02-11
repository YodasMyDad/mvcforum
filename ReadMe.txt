1) To get up and running with the DB, just login to SQL Express and create a blank database called 'MVCForum'
2) Now grab the SQL DB script for the version you are about to install (See note below) and run it in SQL Express and hit run/execute and it will create the tables
3) Clone the repo from codeplex to get the source or just download it
4) Open web.config and update the 'MVCForumContext' connection string to point to the DB you just created
5) Hit run in visual studio
6) Login to the admin using 'admin' and 'password'


#### NOTE #### 

Db & Upgrade scripts that you can paste directly into SQL Express are found in MVCforum.Website project

Installer > Db > 'Then Choose version number'

Still testing it 