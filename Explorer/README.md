# NOTE

I have made the [MonoExplorer](https://github.com/sinai-dev/MonoExplorer) which is an improved version of this, and also works on any Mono Unity game.


# Original Readme

## Explorer

This is a debugging tool for inspecting the Outward game in a similar way to the Unity editor.

This mod <b>requires</b> BepInEx.

## Install

* Download the Explorer.zip file
* Place it in the Outward installation folder, and choose "Extract here". 
* This will add the Explorer folder to Outward\BepInEx\plugins\, which contains `Explorer.dll` and `mcs.dll` (used for the console).

## Credits

Thanks to ManlyMarco for their [Runtime Unity Editor](https://github.com/ManlyMarco/RuntimeUnityEditor), which I used for the REPL console and the "Find Instances" snippet. The only difference is that my console is much more light-weight, and the "find instances" will search for FieldInfo and not just PropertyInfo.

Also includes the same `mcs.dll` from the one used in that repository.
