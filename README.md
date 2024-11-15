# Babble.Net

**Babble.Net** is an experimental version of [Project Babble](https://github.com/Project-Babble/ProjectBabble) that targets mobile devices, with the intent to be used with Quest standalone. It is also available as a desktop app for Windows, MacOS and Linux!

## Table of Contents
- [Installation](#installation)
- [Usage](#usage)
- [Building](#building)
- [Links](#links)

## Installation
Presently, Babble.Net has been tested on two platforms: **Windows** and **Android**.

### Windows
Download the installer from the releases tab and follow the prompts on screen.

### Android
Download the APK from the releases tab on your mobile device and follow the prompts on screen.

## Usage 
### Desktop Users
Run the app as you would the existing Babble App!

### Mobile Users
*Presently, Babble.Net only supports IP camera connections on mobile.*

1) Connect an IP camera by entering its name in the capture source field.
1) Go to your settings and change the target IP to your Quest headset. (You can check what this is by clicking on your wifi details - it should be near the bottom.)
1) On the same page, change the port to 9000 or whatever OSC port you have setup on VRChat.
1) If applicable, supply an OSC prefix. For instance, if your avatar has parameters that follow the scheme `/avatar/parameters/FT/JawOpen` you would enter: `/avatar/parameters/FT/`.
1) Be sure your OSC is enabled in VRChat alongside any avatar settings.
1) Profit!

## Building
To build Babble.Net from source, clone this repo and restore any and all nuget packages. You'll need a copy of the Avalonia SDK installed. More detailed instructions to come!

## Links
- [Our Discord](https://discord.gg/XAMZmjBktk)
- [Our Twitter](https://x.com/projectBabbleVR)
- [Wandb Runs](https://wandb.ai/summerai/ProjectBabble)
