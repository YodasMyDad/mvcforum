The installer will be here after we get past v1, it will check the assembly version against the version in the web.config at app startup.

1.) Check web.config, if no version number its a new install (If version number need to work on an updater)
2.) Check blank db has been created and we have a connection to it (If not show friendly error to user and explain what to do)
3.) Run the .SQl db creator 
4.) (If first time) Allow them to update the admin password?
5.) Tell them to delete install folder now its complete, and then give a button to redirect to home page or admin.
