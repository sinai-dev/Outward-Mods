# Sinai's Outward-Mods
 
Sinai's mods for Outward. Please note that most of these mods are fairly old and are some of the first things I wrote in C#.

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/C1C14TRZG)

### Contents
- [CombatHUD](#combathud)
- [Combat Dummy](#combat-dummy)
- [Combat Tweaks](#combat-tweaks)
- [Custom Weight](#custom-weight)
- [Mana Bow](#mana-bow)
- [Minimap](#minimap)
- [Mixed Grip](#mixed-grip)
- [More Map Details](#more-map-details)
- [Multiple Quickslot Bars](#multiple-quickslot-bars)
- [Necromancer](#necromancer)
- [PvP](#pvp)
- [Shared Quest Rewards](#shared-quest-rewards)
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

## Custom Weight

The Custom Weight mod allows you to:

* Set custom Pouch Bonus (flat value)
* Set custom Bag Bonus (flat AND multiplier values)
* Set No Limits on all containers
* Remove all Weight Burdens

The config can be managed through the [Config Manager](https://outward.thunderstore.io/package/Mefino/Outward_Config_Manager/), or the r2modman Config Editor, or by editing the file at `BepInEx\config\com.sinai.customweight.cfg`.

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

## Minimap

This mod adds a basic Minimap to the top-right of the Character UI, and adds automatically generated maps for Dungeons.

The way this is done is by using an orthographic camera placed above the Character. There are some efforts made to keep it lightweight such as the culling depth and using Vertex Rendering, but it may still negatively impact performance, especially on maps like Berg and Antique Plateau.

Once installed, it should just work. Let me know if you experience any issues.

* **Toggle Minimap**: There is a Custom Keybinding (in-game keybindings) which allows you to toggle the minimap, it is called **Toggle Minimap** in the **Menu** section of your keybindings.
* **Adjust Minimap Zoom**: Player 1 can zoom in and out by holding **Shift** and pressing **Up or Down arrows**. Otherwise, adjust the config.

The config can be changed with the [Config Manager﻿](https://outward.thunderstore.io/package/Mefino/Outward_Config_Manager/), or with the r2modman config editor, or by editing the file at "BepInEx\config\com.sinai.minimap.cfg".

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

### Can I Balance This Myself?
Yes, you can adjust the balancing of the skills yourself through two methods:

* The XML Config File at `config\NecromancySkills.xml` can be used to adjust damages and other details on the balancing. If you use r2modman this will be inside your profile appdata folder, otherwise it will be in the Outward\BepInEx folder.
* SL XML files can be edited from `plugins\Necromancy\SideLoader\`, or you can use the SL Menu to adjust these as well.

Please note: in online play, everyone should use the same settings for best results. This mod does not sync settings.

Finding the Trainer
The trainer is a spectral ghost found at Dark Ziggurat, by the elevator at the top entrance.

If you just want to test the skills, they're available from the F3 debug menu. (Must have "DEBUG.txt" in "Outward/Outward_Data/" folder).

### Skills
There's a total of 8 new skills, which is standard for a skill tree in Outward. There are 5 actives and 3 passives.

#### Tier 1: (50-100 silver, universal)
- Summon: Sacrifice 10% of your max health to summon an undead ally to fight alongside you.
- Life Ritual: Heal your summon and buff them.
- Vital Attunement: +20 max health and stamina
- Noxious Tendrils: Low cooldown projectile spell, if you have a summon they will also cast this. Heals you a small amount for every hit.

#### Breakthrough:
- Transcendence: Gain 2.5% bonus non-physical damage for every mana point you channel at the Ley line.
- note: unlike other breakthroughs, this actually requires Summon and Life Ritual, because of how integral they are to this tree.
- note 2: a "Mana Point" refers to sacrificing 5 Health and Stamina for 20 Mana. Not each literal mana value.

#### Tier 3: (600 silver, post-breakthrough)
- Death Ritual: Detonate your current summon for a big AoE explosion

Choose one:
- Plague Aura: A sigil that follows you around and periodically damages enemies with an AoE ground attack. Enables spell combos with Necro skills.
- Army of Death: max summon limit increased to 3 from 1. 

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

## Shared Quest Rewards

This mod attempts to share all quest rewards (skills and items).

**Note:** by default, the "aggressive sharing" mode is disabled. If you want you can enable the aggressive sharing in the config, it will try to share absolutely everything but this may lead to sharing some unexpected things (eg, buying a skill from a side-trainer may be shared, etc).

The config can be managed with the [Config Manager](https://outward.thunderstore.io/package/Mefino/Outward_Config_Manager/), or by editing the file at "BepInEx\config\com.sinai.sharedquestrewards.cfg".

## Speedrun Timer
A simple in-game speedrun timer which pauses for loading screens, to get an accurate time.

The config can be managed through the [Configuration Manager](https://github.com/Mefino/BepInEx.ConfigurationManager), or by editing the file at `BepInEx\config\com.sinai.speedruntimer.cfg`.

You **cannot** use this mod with any other mods enabled (other than Configuration Manager), or if Debug Mode is enabled.

## Custom Multiplayer Limit [Deprecated]
Allows you to set a custom multiplayer limit for online play when you are the host.

The config can be managed through the [Configuration Manager](https://github.com/Mefino/BepInEx.ConfigurationManager), or by editing the file at `BepInEx\config\com.sinai.custommultiplayerlimit.cfg`.

This mod makes no changes to game mechanics or balancing, the only thing it does is raise the player cap and attempts to fix some bugs.
