# Sinai's Outward-Mods
 
Sinai's mods for Outward.

<b>NOTE:</b>
* This repository is currently being converted over to work with [Mefino](https://github.com/Mefino/Mefino), please wait for all packages to be converted over.

### Contents
- [CombatHUD](#combathud)
- [Combat Dummy](#combat-dummy)
- [Custom Multiplayer Limit](#custom-multiplayer-limit)
- [Mana Bow](#mana-bow)
- [Necromancer](#necromancer)

## CombatHUD
Combat HUD provides a number of combat-related HUD tools, all of which can be toggled on/off and customized:

* Status Timers: show remaining lifespans above the icons
* Numerical Vitals: show player's HP / Stamina / Mana values as numbers
* Floating Damage Labels: With color for each damage, can customize the text size, speed, lifespan, etc
* Target Enemy Info: Status Effects, remaining Health label, and an ARPG-style "Enemy Infobox" to view in-depth stats

The config can be managed through the [Configuration Manager](https://github.com/Mefino/BepInEx.ConfigurationManager), or by editing the file at `BepInEx\config\com.sinai.combathud.cfg`.

## Combat Dummy
This mod allows you to spawn customizable Combat Dummies, either for testing builds and damage or practicing your combat skills against their AI.

* In order to use the Combat Dummy menu, load up a Character and set a keybinding for the Combat Dummy Menu in your in-game keybinding settings.
* Once you have loaded a character and set the keybinding, press it to open the menu and start practicing / testing against dummies. There is no limit to how many you can spawn at once.
* There is quite a bit of freedom in what you can set on the dummy, including making it an Ally or Enemy, setting its weapon, etc.

## Custom Multiplayer Limit
Allows you to set a custom multiplayer limit for online play when you are the host.

The config can be managed through the [Configuration Manager](https://github.com/Mefino/BepInEx.ConfigurationManager), or by editing the file at `BepInEx\config\com.sinai.custommultiplayerlimit.cfg`.

This mod makes no changes to game mechanics or balancing, the only thing it does is raise the player cap and attempts to fix some bugs.

## Mana Bow
The Mana Bow mod adds the items `Mana Bow`, `Horror Mana Bow` and `Mana Arrow` to the game.

* These bows cost Mana (and a very small amount of Stamina) to shoot, and deal Ethereal damage
* Mana Arrows have a limited lifespan of 10 seconds once you draw the bow shot, after which they are destroyed.
* The impact damage is reduced compared to Physical bows.

<details>
 <summary>Click to view recipes</summary>
 
 The Mana Bow recipe is: `Simple Bow, Mana Stone, Hackmanite and Spiritual Varnish` in Survival Crafting.
 
 The Horror Mana Bow recipe is: `Mana Bow, Horror Bow, Hackmanite and Occult Remains` in Survival Crafting.
</details>

## Necromancer
Necromancer is a new skill tree focused on summoning undead creatures to fight alongside you, and sacrificing them to produce more powerful effects. The design was loosely based around the Necromancer class in Diablo.

The trainer can be found at Dark Ziggurat (at the top entrance), right by the elevator. Or you can use F3 Debug menu to test the skills.

Online and split-screen are supported, provided all players have this mod installed.

### Balancing
You can balance the mod yourself through two methods:
* Some things can be adjusted through the SL Menu. See [here](https://sinai-dev.github.io/OSLDocs/#/Basics/SLMenu) for more details.
* For everything else, edit the file here: `BepInEx\config\NecromancerSkills.xml` (generated after first launch).

-----------------------------------

### Mefino Tag
`outward mefino mod`
