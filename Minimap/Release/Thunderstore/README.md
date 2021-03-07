## Minimap

This mod adds a basic Minimap to the top-right of the Character UI, and adds automatically generated maps for Dungeons.

The way this is done is by using an orthographic camera placed above the Character. There are some efforts made to keep it lightweight such as the culling depth and using Vertex Rendering, but it may still negatively impact performance, especially on maps like Berg and Antique Plateau.

### How To Use
Once installed, it should just work. Let me know if you experience any issues.

* **Toggle Minimap**: There is a Custom Keybinding (in-game keybindings) which allows you to toggle the minimap, it is called **Toggle Minimap** in the **Menu** section of your keybindings.
* **Adjust Minimap Zoom**: Player 1 can zoom in and out by holding **Shift** and pressing **Up or Down arrows**. Otherwise, adjust the config.

The config can be changed with the [Config Manager﻿](https://outward.thunderstore.io/package/Mefino/Outward_Config_Manager/), or with the r2modman config editor, or by editing the file at "BepInEx\config\com.sinai.minimap.cfg".