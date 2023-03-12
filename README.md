# SkulPlayableDracula
Special thanks to [MrBacanudo](https://github.com/MrBacanudo)
<img width="960" alt="dracula" src="https://user-images.githubusercontent.com/45824078/224556302-861638bf-4fe0-40bf-a522-0fb6589e9a4d.png">

### Setup premise
`$(SkulFolder)` is a directory where the game is located on your PC

## Setup
1. Extract into `$(SkulFolder)/2020.3.34` (You will need to create a folder)
    - [unstripped Unity 2020.3.34 files](https://unity.bepinex.dev/libraries/2020.3.34.zip)
    - [unstripped CoreLibs 2020.3.34 files](https://unity.bepinex.dev/corlibs/2020.3.34.zip)
2. Extract into `$(SkulFolder)`
    - [BepInEx v5.4.21](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21)
3. In `$(SkulFolder)/doorstop.ini`
    - Set `dllSearchPathOverride=` to `dllSearchPathOverride=2020.3.34`
4. Download (or [build it yourself](#build-it-yourself)) the mod from ["Releases" page](https://github.com/limtis0/SkulPlayableDracula/releases) and place it to `$(SkulFolder)/BepInEx/plugins`

## Configure
1. Open the game after installation for the first time. If installation was successful this will create a config file
2. The config file is located at `$(SkulFolder)/BepInEx/config/PlayableDracula.cfg`

## Build it yourself
Assuming you have .NET Framework 4.7.2 and Visual Studio installed
1. Clone the project into Visual Studio
2. NuGet packages should install automatically, if they didn't - do so
4. [Publicize](https://github.com/bbepis/NStrip) and add `Assembly-CSharp` and all `Plugins...` `Unity...` `UnityEngine...` .dll files from `$(SkulFolder)/SkulData/Managed/` to the references
5. Build the solution
