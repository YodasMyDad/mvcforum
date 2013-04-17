1) To get up and running with the DB, just login to SQL Express and create a blank database called 'MVCForum'
2) Now grab the SQL DB script for the version you are about to install (See note below) and run it in SQL Express and hit run/execute and it will create the tables
3) Clone the repo from codeplex to get the source or just download it
4) Open web.config and update the 'MVCForumContext' connection string to point to the DB you just created
5) Hit run in visual studio
6) Login to the admin using 'admin' and 'password'

#### NOTE #### 

Db & Upgrade scripts that you can paste directly into SQL Express are found in MVCforum.Website project

Installer > Db > 'Then Choose version number'

The installer is still being updated/tested for 'upgrades' between versions. Its still flakey in my opinion, so bare with us


##### DATABASE/SITE RESET ORDER ######

If you want to clear the database and reset a test/dev forum, then use the file 'dbReset.sql' in this 'Database' solution folder

----THEN Update Web.config

1) Remove version number
2) Clear Facebook settings

--- Then clear the log file

##### Blank a database ###########

If you want to blank a database quickly to test installer, then keep running the line below over and
over until all data is gone and no more errors

EXEC sp_MSforeachtable @command1 = "DROP TABLE ?"
