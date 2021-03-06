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