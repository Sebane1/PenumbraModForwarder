# Penumbra Mod Forwarder
# Important
Work on this project has been moved to a separate repository, this one will continue to exist but will no longer be maintained

### Updater
Has been moved to: https://github.com/CouncilOfTsukuyomi/Updater

### PMF
Has been renamed and moved to: https://github.com/CouncilOfTsukuyomi/ModForwarder

### Common Library
Has been extracted out of PMF and moved to: https://github.com/CouncilOfTsukuyomi/PMF.CommonLib

---

Forwards `.pmp`, `.ttmp`, and `.ttmp2` files to be automatically opened by Penumbra.

### In Action Preview:
[Watch the video](https://youtu.be/_kv6DzqoyX4)

---

## Features

- **Convenient Mod Downloading:** Simplify your mod downloading process by automatically forwarding mods to Penumbra.
- **Automatic Opening in Penumbra:** Mods you download can be set to automatically open in Penumbra. This option can be disabled if preferred.
- **Double-Click to Open Mods:** Set Penumbra Mod Forwarder as your default program to allow double-clicking or multi-selecting mods, which will then automatically open in Penumbra.

Penumbra Mod Forwarder, as the name suggests, will notify Penumbra to install new mods that are downloaded to a specified folder. Additionally, it allows Penumbra to install mods that you double-click or multi-select, provided you've set Penumbra Mod Forwarder as the default program for opening supported mod files in Windows.

---

## Instructions

1. **Enable Auto Forward Mods:** Check the "Auto Forward Mods" option in the application.
2. **Set Default Downloads Folder:** Select your default downloads folder where mods are saved.
3. **Download Mods:** Download mods from your favorite sites, and they will be automatically forwarded to Penumbra.

*(Optional)*
4. **Install and Configure TexTools:** If you want mods to be automatically converted from Endwalker to Dawntrail, ensure you have installed and configured TexTools at least once.

---

## Requirements

This program requires the .NET 8.0 desktop runtime to be installed. You can download it here:

- **64-bit:** [Download .NET 8.0 SDK for Windows x64](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-8.0.401-windows-x64-installer)
- **32-bit:** [Download .NET 8.0 SDK for Windows x86](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-8.0.401-windows-x86-installer)

---

## Documentation

### Viewing Logs
The application will generate logs based on what is currently happening, these can be found at `%appdata%/PenumbraModForwarder/logs`

### Configuration
The application uses json files to handle all user configuration, this can be found at `%appdata%/PenumbraModForwarder/config.json` the application will handle all of these settings automatically, except for Advanced Options

The default settings will look like this
```json
{
  "AutoLoad": false,
  "AutoDelete": false,
  "ExtractAll": false,
  "NotificationEnabled": false,
  "FileLinkingEnabled": false,
  "StartOnBoot": false,
  "DownloadPath": "",
  "TexToolPath": "",
  "AdvancedOptions": {
    "HideWindowOnStartup": true,
    "PenumbraTimeOutInSeconds": 60
  }
}
``` 

### Advanced Options
These options are for advanced users who are after specific things from the application.

Right now all they do is make it so the Main Window will show even if you have StartOnBoot enabled & handle how long the timeout is for when installing a mod via Penumbra Web Api

---

## Building from Source

To build Penumbra Mod Forwarder from source, follow these steps:

1. **Clone the Repository with Submodules:**  
   Clone the repository and its submodules to your local machine using the following commands:

   ```bash
   git clone --recurse-submodules https://github.com/Sebane1/PenumbraModForwarder.git
    ```

   If you have already cloned the repository without submodules, you can initialize and update the submodules with:
   ```bash
   git submodule update --init --recursive
   ```


2. **Install .NET 8.0 SDK:**  
   Ensure that you have the .NET 8.0 SDK installed on your machine. You can download it from the official .NET website:
    - **64-bit:** [Download .NET 8.0 SDK for Windows x64](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-8.0.401-windows-x64-installer)
    - **32-bit:** [Download .NET 8.0 SDK for Windows x86](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-8.0.401-windows-x86-installer)

3. **Build the Solution:**
   Navigate to the root directory of the repository and run the following command:

    ```bash
    dotnet publish PenumbraModForwarder.UI/ -c Release -p:PublishSingleFile=true --self-contained false -r win-x64 -o ./publish -f net8.0-windows
    ```

4. **Run the Application:**
   Once the build is successful, navigate to the `publish` directory and run the executable file to start the application.

