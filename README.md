# Sinai's Outward-Mods
 
Sinai's mods for Outward.

### Contents
- [CombatHUD](#combathud)
- [Combat Dummy](#combat-dummy)
- [Combat Tweaks](#combat-tweaks)
- [Custom Multiplayer Limit](#custom-multiplayer-limit)
- [Mana Bow](#mana-bow)
- [Mixed Grip](#mixed-grip)
- [More Map Details](#more-map-details)
- [Necromancer](#necromancer)
- [PvP](#pvp)
- [Speedrun Timer](#speedrun-timer)

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

## Combat Tweaks
Previously known as Combat and Dodge Overhaul, this is a 'lite' version with only the features I actually use myself. Fully customizable and toggle-able.

* Adjust the 'hit-stop' effect (the slow-down when a weapon collides with a target)
* Allow dodge to cancel attacks and other actions
* Allow attack input to interrupt blocking

The config can be managed through the [Configuration Manager](https://github.com/Mefino/BepInEx.ConfigurationManager), or by editing the file at `BepInEx\config\com.sinai.combattweaks.cfg`.

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

## Mixed Grip
Allows you to swap between 1H and 2H grip on Melee Weapons, similar to Dark Souls.

Set a keybinding in your in-game keybindings for the grip toggle, and adjust the BepInEx config as you desire.

The config can be managed through the [Configuration Manager](https://github.com/Mefino/BepInEx.ConfigurationManager), or by editing the file at `BepInEx\config\com.sinai.mixedgrip.cfg`.

## More Map Details
More Map Details adds some extra details to the region maps, all of which can be toggled. The extra details include:
* Player positions
* Player bag positions
* Enemy positions (the markers show as an 'X' until you hover over them)
* Soroborean Caravanner position

## Multiple Quickslot Bars

Allows you to add extra quickslot bars which you can cycle through with custom keybinds.

Set the custom keybinds in the in-game keybinding menu, and the amount of extra bars can be set with [Configuration Manager](https://github.com/Mefino/BepInEx.ConfigurationManager), or by editing the file at `BepInEx\config\com.sinai.multiplequickslotbars.cfg`.

This mod does not add extra slots, but instead swaps out the entire bar for alternate bars. You can configure the mod to add as many extra bars as you want, but currently you can only cycle between them one at a time (no direct hotkeys to jump to a specific bar, etc).

## Necromancer
Necromancer is a new skill tree focused on summoning undead creatures to fight alongside you, and sacrificing them to produce more powerful effects. The design was loosely based around the Necromancer class in Diablo.

The trainer can be found at Dark Ziggurat (at the top entrance), right by the elevator. Or you can use F3 Debug menu to test the skills.

Online and split-screen are supported, provided all players have this mod installed.

### Balancing
You can balance the mod yourself through two methods:
* Some things can be adjusted through the SL Menu. See [here](https://sinai-dev.github.io/OSLDocs/#/Basics/SLMenu) for more details.
* For everything else, edit the file here: `BepInEx\config\NecromancerSkills.xml` (generated after first launch).

## PvP
The PvP mod allows for easy PvP between friends, and also has some extra modes like Deathmatch and a Battle Royale.

* **Friendly Fire/Targeting:** Enable the ability to attack and/or target your friends without using different factions.
* **Basic PvP:** simply assign teams for each player in the PvP menu. You will instantly be able to target and attack each other. The World Host can change anybody's team, but non-hosts can only change their own team, or their split partner's team.
* **Deathmatch:** Only the World Host can start a DM. This will disable revives for all players, and disallow swapping teams, until there is only one team standing. I could add more modes like this in the future if people are interested, but this was quick and easy for now.
* **Spectate:** If you are on the "NONE" team (or dead in Battle Royale), you can spectate the remaining players. 

### Spectating
The spectate mode allows you to watch the remaining players, either by locking onto them or by using the free camera.

When in spectate mode, you can use the following hotkeys:

* **Left and Right Arrows:** Enable lock-on spectate, and cycle between remaining players
* **Up or Down Arrows:** Return to free-camera and cancel lock-on spectate
* **Free Camera:** Use your movement keys and mouse to look around (jump to go up, crouch to go down)
* **Escape:** Panic key to open the PvP menu and interrupt the spectate, in case you need to end the game or something.

### Battle Royale

**WARNING:** Battle Royale needs to wipe the character save so that all players begin equally. Do NOT use a Character which you care about for Battle Royale, use a fresh character (must at least reach the Lighthouse) so that you don't care if it gets wiped after the games. If you accidentally used a character you care about, then you can restore the backup save using Outward Debug Mode or just manually restoring the file.

The Battle Royale mode is designed as a 2 to 6 player game mode, similar to traditional BR games but obviously quite heavily adapted to work in Outward, and with some unique mechanics.

* Set your teams in the menu, then the host can start a Battle Royale.
* All players will lose their items, skills etc.
* Everyone will be teleported to Monsoon (unless you're already there) and the game will immediately begin.
* Last team standing wins!

There are quite a few unique mechanics in the Battle Royale mode, some of which include:

* Traps damage anyone that triggers it, even the person who armed it.
* Weapon skills are learned automatically once you acquire the weapon.
* Some enemies will randomly spawn around the city. Kill them to get skills.
* Supply drops (chests) appear every 2 minutes or so. The amount of chests depends on the number of players in the game.
* Supply chests become more valuable over time.
* At around 4 minutes the Butcher of Men will spawn, he drops a powerful weapon and strong skills. 
* You can take other players items when they are defeated.

## Speedrun Timer
A simple in-game speedrun timer which pauses for loading screens, to get an accurate time.

The config can be managed through the [Configuration Manager](https://github.com/Mefino/BepInEx.ConfigurationManager), or by editing the file at `BepInEx\config\com.sinai.speedruntimer.cfg`.

You **cannot** use this mod with any other mods enabled (other than Configuration Manager), or if Debug Mode is enabled.

