# WoWmapperX
WoWmapperX is a controller mapping utility to be used with ConsolePortLK

## What is WoWmapperX?

WoWmapperX is a fork of [WoWmapper](https://github.com/topher-au/WoWmapper) which brings some major changes to the original application.

It's primary purpose is to handle DualShock 4 or Xbox/Xinput controller input and convert it into button presses and mouse movements which are then sent to WoW and processed by [ConsolePortLK](https://github.com/leoaviana/ConsolePortLK). ConsolePortLK binds each key to an action within World of Warcraft, and features a full UI and many features for enhanced gameplay with a controller. WoWmapperX also includes force feedback and input assistance utilites such as vibration, automatic cursor centering and pseudo-analog-sensitive movement as well as many more features designed to make efficient controller gameplay a reality in the World of Warcraft.

## What is the difference between WoWmapperX and WoWmapper?

WoWmapperX has a lot of differences compared to WoWmapper, some of these are:

1. Updated project to use .NET 8 and newer (currently using .NET 9 beta for 32bit [NativeAOT](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/) support.)
2. Migrated from [WPF UI](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/overview/) to [AvaloniaUI](https://avaloniaui.net/) to be able to execute under Wine on Linux. Seems to run fine but needs more testing.
3. Updated DS4Library to a newer version to increase controller compatibility (in theory, DualSense, Switch Pro Controller, DualShock 3 should work now, but I have not tested them.)
4. Reimplemented some memory reading functions used on older WoWmapper versions to improve the user experience. (only for 3.3.5a client)
5. A GUI-less mode to use less resources.

## What do I need?

- A system running Windows 7, 8, 10 or higher
- Microsoft .NET
- A DualShock 4 or Xbox/Xinput compatible controller
- World of Warcraft client (it will work with any version ConsolePort supports, but should not be used with Shadowlands or newer)

**Before you download WoWmapperX, please ensure that you meet the requirements for running the application.**

### Command line arguments

WoWmapperX can be opened with some command line arguments if desired. the commands available are:

```
1. -h / --help                 : displays information for all available command line arguments
2. -rg:path_to_game.exe        : launches a process given it's path e.g.: -rg:C:\\Wow\\Wow.exe; -rg:C\\Wow\\Launcher.exe
3. -rg:path_to_game.exe,(-c;-d): launches a process given it's path and with command line arguments separated by ';'
4. -term                       : terminate WoWmapperX whenever the first game process detected terminates (game processes are named Wow.exe)
5. -dterm                      : terminate WoWmapperX whenever the specified process given by -rg terminates
6. -nogui                      : launch in console mode, useful for logging, verbose mode and lightweight environments
7. -noconsole                  : launch in silent mode. only available if used with -nogui
```

Example:<br/> 
1. Run WoWmapperX.exe silently and start game using it's path making sure to close whenever game process finishes:
   ```
   WoWmapperX.exe -nogui -noconsole -rg:C\\Wow.exe -term
   ```
2. Run WoWmapperX.exe silently and start game using a launcher's path making sure to close whenever the first detected game process finishes:
   ```
   WoWmapperX.exe -nogui -noconsole -rg:C\\WoWLauncher.exe -dterm
   ```

### My game process is not detected automatically by WoWmapperX
The only way your game is not detected by WoWmapperX when you launch it is if it has a different process name, you can add a different process name into your settings.json  which is generated the first time you launch WoWmapperX. by opening it with a text editor you'll see this part:
```
"GameProcessNames": [
    "wow",
    "wow-64",
    "wowt",
    "wowt-64",
    "wowb",
    "wowb-64"
  ]
```
here you can add extra names that you game process might have, please add them in lowercase.

### I can't find settings.json or keybinds.json in my WoWmapperX directory
WoWmapperX will automatically save settings files in the same directory the executable is located, however if somehow the user does not have permission writing to that directory it will fallback to saving files in AppData e.g.:
```
C:\\Users\\yourUser\\AppData\\Roaming\\WoWmapperX\\settings.json
```

### Will it work on Linux under Wine?

I've tested it some time ago using Wine 8.0 on Manjaro, the application launches and the controller mapping feature worked fine (using a xinput compatible controller). I have not tested it since then but I suppose it should work but if it does not work there are some linux alternatives.<br/>

However if you're trying using it on Android with Winlator or any Termux/Box64 based wine installation it will not work, the application just crashes and there is no useful log or exception information.

### How much effort is required to get it set up?
 
WoWmapper and ConsolePort are designed to work together to make the installation as simple as possible. Once ConsolePort and WoWmapper are installed, launching World of Warcraft will export a keybinding configuration file to the ConsolePort folder that will be loaded while ConsolePort is active, meaning that ConsolePort will not need to be calibrated in-game, and when you disable ConsolePort, your regular bindings are preserved underneath allowing you to easily switch between keyboard and mouse or controller gameplay simply by toggling the ConsolePort addon and reloading the user interface.

### Configuration instructions

**WoWmapper features an automatic configuration system that will set up your keybindings and icons in-game without any input. We recommend leaving WoWmapper's keybindings as their defaults and only changing the modifier layout.**

By default, temporary keybindings will be exported to ConsolePort that will not make any permanent changes to your standard keybindings. These bindings are controlled by ConsolePort and you can switch from controller to keyboard and mouse by simply disabling ConsolePort, and back again by re-enabling it. Additionally, WoWmapper will configure the button icons within ConsolePort to match the currently connected controller.

To change your controller layout, open the WoWmapper configuration and select *Key Bindings*. From here you can change which of the shoulder buttons will be used as modifiers, and select which button icons will be shown in WoWmapper and ConsolePort. Additionally, you may override the default WoWmapper bindings (this is not recommended).

If you make any changes to the controller layout or bindings, you must type `/reload` in-game or restart World of Warcraft for the changes to take effect.

## Non-Windows systems and WoWmapper alternatives

If you're not running Windows, or WoWmapper isn't suitable for your setup, there are several alternatives available. These alternatives only provide base input mapping - no advanced features or haptic feedback.

### Linux
- AntiMicroX
- Steam

### Android
- Winlator

### Mac OS X
- ControllerMate
- Joystick Mapper

### Windows alternatives

- DS4Windows
- Xpadder
- JoyToKey
- Keysticks
