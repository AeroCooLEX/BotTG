Adds integration of telegram bot with your server. Configured for a specific bot. 
Download the source data and in the fields “YOUR_TOKEN” and “CHAT_ID” indicate the token of your telegram bot and chat id. This must be done in all files with the .cs extension. Save the changes. Rejoice!
The bot can: 
-Send text messages to the server and receive messages from the server. Chat, play and watch somewhere. 
- Receive information about the connection of players, send it to the chat below and display a list of players on the server. 
- Send notifications about the death of a player in the chat with the bot. 
- Send a message about the disconnection of players and show a list of the remaining players on the server. 
- Send a message to the chat with the bot that the server has been rebooted and will reboot in 3 55 minutes. (because on my computer the reboot is 4 hours) 
- Notify 5 minutes before the server reboot, that is, in 3 hours 55 minutes (because on my server the reboot is 4 hours)
P.S. You can change the time to display the reboot message by changing 
string initialMessage = "The server has been rebooted. Online status! Next reboot in <your time>!"; 
and timer = new Timer(SendRestartMessage, api, TimeSpan.FromMinutes(your time in minutes), TimeSpan.FromMinutes(your time in minutes)); 
in Timerrest.cs file
