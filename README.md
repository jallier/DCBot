# DCBot
A simple Discord bot to play audio snippets from text commands, similar to Airhorn bot.
To install, download and unzip the folder. To run double click on the DCBot.exe (windows) or in a terminal `mono DCBot.exe` (linux)
These files are included in folder, or can be found in the res directory (except for ffmpeg which can be found on [its webpage](https://ffmpeg.org/) or installed via package manager)

**Note on Linux:**

*DCBot should run on Ubuntu 16.04. The download folder includes the necessary libsodium and libopus (compiled for 64-bit) but there are no guarantees it will work correctly. Also requires the latest version of mono which may be different to one installed via the package manager, and requires ffmpeg to be installed via package manager*

After installing you need to do 2 things before you can run the bot: Register a new bot account and edit the config file

### Creating a new bot account

1. Head over to the [applicatons page](https://discordapp.com/developers/applications/me).
2. Click "new application". Give it a name, picture and description.
3. Click "Create Bot User" and click "Yes, Do It!" when the dialog pops up.
4. Copy down the `token`. This is what is used for the bot to login.
5. Copy down the `client/application id`. This is used to generate a url to add the bot to your server

Here's a handy gif to explain the process. ![oauth new bot](https://i.imgur.com/Y2ouW7I.gif)

### Edit the config file
Open config.json with notepad or your text editor of choice. config.json is structured:

```
  {
  "token": "your token goes here",
  "client_id": "your client id goes here"
  "command_char": "$",
  "commands": [
    {
      "command": "cena",
      "alias": [ "jc" ],
      "description": "Doot doodoo dooooooot",
      "path": "cena.mp3"
    }
  ]
}
```

1. First replace the token and client_id field with the token generated when the bot was registered, as in the gif.
2. Then choose a command char. This will be the character you use before any commands.
3. Then edit your commands.

- `"command"` the actual command typed eg $cena. The alias is a string array of aliases you can use instead of the main command
- `alias` a string array containing alternate commands eg $jc will play the same sound as \$cena. This field can be omitted
- `description` A description of the command for the help command. This can be omitted.
- `path` The location of the file. If this is in the same directory this can simply be the filename.

Once the steps are complete, you can run DCBot. It will first attempt to convert the audio files listed in the config file to .wav for easy playing and generate a url which you can follow in a browser to add your bot to your server. DCBot will then exit. When you run it again it will join the server and be ready to go!
