using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Partiality.Modloader;
//using SinAPI;

namespace MertonsChallenge
{
    public class EnemyManager : MonoBehaviour
    {
        public ChallengeGlobal global;

        public List<Character> ActiveMinions = new List<Character>();
        public Dictionary<string, float> OrigHealths = new Dictionary<string, float>(); // uid / orig hp

        public void Init() // hooks. these are mostly just to force aggression on the player, some are to prevent the game interfering with my systems.
        {
            On.CharacterAI.Update += new On.CharacterAI.hook_Update(CharacterAIUpdateHook);
            On.AISquadManager.Update += new On.AISquadManager.hook_Update(AISquadManagerHook);
            On.TargetingSystem.InitTargetableFaction += new On.TargetingSystem.hook_InitTargetableFaction(InitFactionsHook);
            On.AISCombat.EndCombat += new On.AISCombat.hook_EndCombat(AIEndCombatHook);
            On.Character.Die += new On.Character.hook_Die(CharacterDieHook);

            On.AISCombat.EquipRequiredWeapons += new On.AISCombat.hook_EquipRequiredWeapons(AIEquipHook); // fix enemy not owning required weapons after resurrect
        }

        public void SpawnRandomEnemy()
        {
            float timeModifier = Mathf.Clamp(global.CurrentTime * global.CurrentTime * 0.0015f, 0, 2000);

            float maxHP = 50 + (timeModifier * 0.5f);
            if (maxHP < 90) { maxHP = 90; }

            List<Character> AvailableEnemies = CharacterManager.Instance.Characters.Values
                .Where(
                  x => x.IsAI
                    && x.Name != global.CurrentTemplate.BossTemplate.Name // dont try to spawn the current boss template bandit
                    && x.name.ToLower().Contains("bandit") // only spawn bandits
                    && ((OrigHealths.ContainsKey(x.UID) && OrigHealths[x.UID] <= maxHP) || (!OrigHealths.ContainsKey(x.UID) && x.ActiveMaxHealth <= maxHP)) // check hp limit
                    && (!ActiveMinions.Contains(x) || x.Health <= 0)) // dont try to spawn currently active (and alive) enemies
                .ToList();

            if (AvailableEnemies == null || AvailableEnemies.Count < 1)
            {
                //OLogger.Error("Available enemies list is null");
                return;
            }

            int r = UnityEngine.Random.Range(0, AvailableEnemies.Count - 1);

            if (AvailableEnemies.ElementAt(r) is Character c)
            {
                StartCoroutine(SetupMinion(c, timeModifier));

                if (!ActiveMinions.Contains(c)) { ActiveMinions.Add(c); }
            }
        }

        private void BasicResurrectSetup(Character c)
        {
            c.RessurectCampingSquad();
            ForceEnemiesAllied(c);

            // add a point light
            Light light = c.gameObject.GetComponent<Light>() ?? c.gameObject.AddComponent<Light>();
            light.color = Color.white;
            light.range = 2;

            MakeImmuneToDots(c);

            //// make sure scale is normal (fix for bosses that become normal enemies)
            //c.transform.localScale = new Vector3(1, 1, 1);
            //c.UIBarOffSet = new Vector3(0, 1, 0);
        }

        private void SetActiveRecursively(GameObject g, ref int i)
        {
            i++;
            if (i > 10) { return; }

            g.SetActive(true);
            if (g.transform.parent != null)
            {
                SetActiveRecursively(g.transform.parent.gameObject, ref i);
            }
        }

        private void ForceEnemiesAllied(Character c)
        {
            c.TargetingSystem.TargetableFactions = new Character.Factions[] { Character.Factions.Player };
            c.TargetingSystem.StartAlliedFactions = new Character.Factions[] { Character.Factions.Player };
            c.Faction = Character.Factions.Bandits;
            c.TargetingSystem.AlliedToSameFaction = true;
        }

        public void MoveAndSetActive(Character c)
        {
            int max = global.CurrentTemplate.EnemySpawns.Count;
            int r = UnityEngine.Random.Range(0, max - 1);
            c.Teleport(global.CurrentTemplate.EnemySpawns[r], c.transform.rotation);

            int i = 0;
            SetActiveRecursively(c.gameObject, ref i);

            ForceTargetOnPlayer(c);

            c.gameObject.SetActive(false);
            c.gameObject.SetActive(true);
        }

        private void ForceTargetOnPlayer(Character c)
        {
            // pick a target
            int r = UnityEngine.Random.Range(0, Global.Lobby.PlayersInLobbyCount - 1);
            Character c2 = Global.Lobby.PlayersInLobby.ElementAt(r).ControlledCharacter;
            c.TargetingSystem.SetLockingPoint(c2.LockingPoint);

            // force combat AI state
            if (c.GetComponentInChildren<CharacterAI>() is CharacterAI chai)
            {
                chai.SetDestination(c2.transform.position, false);
                chai.CurrentLookTarget = c2.transform.position;
                chai.CurrentMoveTarget = c2.transform.position;

                for (int i = 0; i < chai.AiStates.Count(); i++)
                {
                    if (chai.AiStates[i] is AISCombat AICombat)
                    {
                        // force combat target
                        AICombat.OutOfSightRange = 9999;
                        AICombat.SetPreferredTarget(c2.LockingPoint, 0);
                        AICombat.SetMoveToPosition(c2.transform.position);

                        chai.SwitchAiState(i);
                        AICombat.gameObject.SetActive(true);
                    }
                    else
                    {
                        chai.AiStates[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        // ============== CUSTOM NPC GEAR ===============

        private void SetupEquipment(Character c, List<int> itemIDs, BossTemplate template = null, float templateMod = 0f, float weaponTemplateScale = 1.0f)
        {
            foreach (int id in itemIDs)
            {
                Item item = ItemManager.Instance.GenerateItemNetwork(id);

                if (template != null)
                {
                    // set weapon 2H type
                    if (item is Weapon w)
                    {
                        w.TwoHand = template.TwoHandType;
                        w.Type = template.WeaponType;
                    }
                }

                c.Inventory.TakeItem(item, false);
                At.Call(c.Inventory.Equipment, "EquipWithoutAssociating", new object[] { item, false });
                item.ProcessInit();

                if (item is Weapon weapon && weapon.Type != Weapon.WeaponType.Shield)
                {
                    SetupWeapon(weapon, template, templateMod, weaponTemplateScale);
                }
            }
        }

        private void SetupWeapon(Weapon weapon, BossTemplate template, float templateMod, float weaponTemplateScale)
        {
            weapon.InstantLoadWeapon();

            // set boss weapon template
            if (template != null)
            {
                At.SetValue(template.weaponDamages, typeof(WeaponStats), weapon.Stats, "BaseDamage");
                weapon.Stats.Impact += templateMod;

                for (int i = 0; i < weapon.Stats.Attacks.Count(); i++)
                {
                    List<float> stepDamage = new List<float>();
                    float stepImpact = template.weaponImpact;

                    foreach (DamageType dtype in template.weaponDamages.List)
                    {
                        stepDamage.Add(dtype.Damage + (0.5f * templateMod));
                    }

                    global.statUtil.SetScaledDamages(weapon.Type, i, ref stepDamage, ref stepImpact);
                    weapon.Stats.Attacks[i].Damage = stepDamage;
                    weapon.Stats.Attacks[i].Knockback = stepImpact + templateMod;

                    weapon.LoadedVisual.transform.localScale = new Vector3(1, 1, 1) * weaponTemplateScale;
                }
            }
        }

        private void MakeImmuneToDots(Character c)
        {
            // set immune to DoT effects (as per undead / fire based enemies)
            List<Tag> StatusTags = new List<Tag>
            {
                TagSourceManager.Instance.GetTag("129"), // poison
                TagSourceManager.Instance.GetTag("168"), // burning
                TagSourceManager.Instance.GetTag("131") // bleeding
            };
            foreach (Tag tag in StatusTags)
            {
                if (!c.Stats.HasStatusImmunity(tag))
                {
                    var selectors = At.GetValue(typeof(CharacterStats), c.Stats, "m_statusEffectsNaturalImmunity") as TagSourceSelector[];
                    var s2 = selectors.ToList();
                    s2.Add(new TagSourceSelector(tag));
                    At.SetValue(s2.ToArray(), typeof(CharacterStats), c.Stats, "m_statusEffectsNaturalImmunity");
                }
            }
        }

        // ============== MERTONS MINIONS ===============

        private IEnumerator SetupMinion(Character c, float timeModifier)
        {
            BasicResurrectSetup(c);
            MoveAndSetActive(c);

            At.SetValue("Merton's Minion", typeof(Character), c, "m_name");
            At.SetValue("", typeof(Character), c, "m_nameLocKey");

            // set HP
            float origHP = c.ActiveMaxHealth;
            if (OrigHealths.ContainsKey(c.UID)) { origHP = OrigHealths[c.UID]; }
            else { OrigHealths.Add(c.UID, c.ActiveMaxHealth); }

            float healthTarget = origHP + (timeModifier * 0.5f);
            Stat maxHP = new Stat(healthTarget);
            At.SetValue(maxHP, typeof(CharacterStats), c.Stats, "m_maxHealthStat");
            c.Stats.FullHealth();

            // stop weapons dropping on death
            if (c.GetComponentInChildren<LootableOnDeath>(true) is LootableOnDeath loot)
            {
                loot.DropWeapons = false;
            }

            yield return new WaitForSeconds(0.2f);

            DestroyEquipment(c, true); // armor only
            SetupEquipment(c, new List<int> { 3200030, 3200031, 3200032, }); // equip merton's set

            AddMinionImbue(c);

            c.gameObject.SetActive(false);
            c.gameObject.SetActive(true);
        }

        private void AddMinionImbue(Character c)
        {
            // determine imbue
            ImbueEffectPreset imbue = null;
            bool useAltImbue = UnityEngine.Random.Range(0, 99) > 49;
            if (c.ActiveMaxHealth < 250) // fire rag (202) or bolt rag (206)
            {                
                int presetID = useAltImbue ? 206 : 202;
                imbue = ResourcesPrefabManager.Instance.GetEffectPreset(presetID) as ImbueEffectPreset;
            }
            else if (c.ActiveMaxHealth < 600) // fire varnish (203) or ethereal varnish (208)
            {                
                int presetID = useAltImbue ? 208 : 203;
                imbue = ResourcesPrefabManager.Instance.GetEffectPreset(presetID) as ImbueEffectPreset;
            }
            else // infuse fire (217) or dark varnish (211)
            {                
                int presetID = useAltImbue ? 211 : 217;
                imbue = ResourcesPrefabManager.Instance.GetEffectPreset(presetID) as ImbueEffectPreset;
            }

            // apply imbue
            if (c.CurrentWeapon is MeleeWeapon weapon)
            {
                c.CurrentWeapon.AddImbueEffect(imbue, 240);
            }
            else
            {
                MeleeWeapon w = c.Inventory.Pouch.GetContainedItems().Find(x => x.GetType() == typeof(MeleeWeapon)) as MeleeWeapon;
                w.AddImbueEffect(imbue, 240);
            }
        }

        private void DestroyEquipment(Character c, bool armorOnly = false)
        {
            // equipment
            for (int i = 0; i < 8; i++)
            {
                if (armorOnly && i > 3) { break; }

                var slot = (EquipmentSlot.EquipmentSlotIDs)i;
                try
                {
                    var item = c.Inventory.GetEquippedItem(slot);
                    if (item == null) { continue; }

                    item.transform.parent = null;
                    DestroyImmediate(item.gameObject);
                }
                catch { }
            }

            // pouch items
            List<string> UIDs = new List<string>();
            foreach (Item item in c.Inventory.Pouch.GetContainedItems())
            {
                if (item is MeleeWeapon) { continue; }
                UIDs.Add(item.UID);
            }
            foreach (string uid in UIDs)
            {
                if (ItemManager.Instance.GetItem(uid) is Item item)
                {
                    item.transform.parent = null;
                    DestroyImmediate(item.gameObject);
                }
            }
        }

        // ==================== BOSS SETUP =====================

        public void SpawnBoss(BossTemplate template)
        {
            if (CharacterManager.Instance.Characters.Values
                .Find(x =>
                    x.name.ToLower().Contains("newbanditequip")
                    && !x.name.ToLower().Contains("archer"))
                is Character c)
            {
                SetupBoss(c, template);
                StartCoroutine(global.gui.SetMessage(template.Name + " Challenges You!", 5));
            }
        }

        public void SetupBoss(Character c, BossTemplate template)
        {
            BasicResurrectSetup(c);

            MoveAndSetActive(c);

            // change light color to red
            Light light = c.GetComponent<Light>() ?? c.gameObject.AddComponent<Light>();
            light.color = Color.red;
            light.range = 4;
            light.intensity = 2.0f;

            // override name
            At.SetValue(template.Name, typeof(Character), c, "m_name");
            At.SetValue("", typeof(Character), c, "m_nameLocKey");

            // set HP
            float healthTarget = template.Health + (300 * (global.BossesSpawned - 1));
            Stat maxHP = new Stat(healthTarget);
            At.SetValue(maxHP, typeof(CharacterStats), c.Stats, "m_maxHealthStat");
            c.Stats.FullHealth();

            // start boss coroutine
            StartCoroutine(BossCoroutine(c, template));
        }

        private IEnumerator BossCoroutine(Character c, BossTemplate template)
        {
            while (!c.IsLateInitDone) { yield return null; }

            yield return new WaitForSeconds(0.2f);

            LateUpdateBossFix(c, template);

            // wait til boss is dead
            while (c.Health > 0)
            {
                global.TimeSpentOnBosses += Time.deltaTime;
                yield return null;
            }

            // drop table
            GenerateDroptable(c, template);

            global.BossActive = false;
            global.ShouldRest = true;
        }

        private void LateUpdateBossFix(Character c, BossTemplate template)
        {
            float transformScale = Mathf.Clamp(1.1f + (0.1f * global.BossesSpawned), 1.1f, 1.6f);
            float statMod = 5 * global.BossesSpawned;
            // transform scale
            c.transform.localScale = new Vector3(1, 1, 1) * transformScale;
            // fix UI bar pos
            c.UIBarOffSet = new Vector3(0, 1, 0) * transformScale;

            DestroyEquipment(c);

            SetupEquipment(c, global.CurrentTemplate.BossTemplate.Equipment, global.CurrentTemplate.BossTemplate, statMod, transformScale);

            // add imbue fx (just fire rag for visuals, no major damage)
            var preset = ResourcesPrefabManager.Instance.GetEffectPreset(202);
            if (preset is ImbueEffectPreset fireRag)
            {
                c.CurrentWeapon.AddImbueEffect(fireRag, 240);
            }

            // set resistances
            List<Stat> newDmgRes = new List<Stat>();
            for (int i = 0; i < 9; i++)
            {
                float resTarget = template.Resistances[i];

                resTarget = Mathf.Clamp(resTarget + statMod, resTarget, 90);

                Stat stat = new Stat(resTarget);
                newDmgRes.Add(stat);
            }
            At.SetValue(newDmgRes.ToArray(), typeof(CharacterStats), c.Stats, "m_damageResistance");

            Stat impRes = new Stat(Mathf.Clamp(template.impactRes + statMod, template.impactRes, 90));
            At.SetValue(impRes, typeof(CharacterStats), c.Stats, "m_impactResistance");

            // final fix
            c.gameObject.SetActive(false);
            c.gameObject.SetActive(true);
        }        

        private void GenerateDroptable(Character c, BossTemplate template)
        {
            int b = global.BossesSpawned - 1;
            int r = UnityEngine.Random.Range(template.DropTable.MinDrops + b, template.DropTable.MaxDrops + b);
            for (int i = 0; i < r; i++)
            {
                // get the max dice value
                int maxdice = 0; 
                int j;
                for (j = 0; j < template.DropTable.DroppedItems.Count; j++)
                {
                    maxdice += template.DropTable.ItemChances[j];
                }

                // generate a random roll, and check which item range it falls in 
                int r2 = UnityEngine.Random.Range(0, maxdice - 1);
                int dropID = -1;
                int currentMax = 0;

                for (j = 0; j < template.DropTable.DroppedItems.Count; j++)
                {
                    currentMax += template.DropTable.ItemChances[j]; // add the current item chance to the max value

                    if (r2 < currentMax) // if the roll was less than this max
                    {
                        dropID = template.DropTable.DroppedItems[j]; // set the drop ID (otherwise, increase the max value and check next item)
                        break;
                    }
                }
                if (dropID != -1)
                {
                    // generate a random quantity
                    int r3 = UnityEngine.Random.Range(template.DropTable.ItemMinQtys[j], template.DropTable.ItemMaxQtys[j]);
                    int k;
                    for (k = 0; k < r3; k++)
                    {
                        // for each quantity, spawn an take the item
                        Item item = ItemManager.Instance.GenerateItemNetwork(dropID);
                        item.transform.parent = c.Inventory.Pouch.transform;
                    }
                }
            }
        }

        // =========== hooks =============

        private void CharacterDieHook(On.Character.orig_Die orig, Character self, Vector3 _hitVec, bool _loadedDead = false)
        {
            if (global.IsGameplayStarted && global.CurrentTime > 0 && self.IsAI)
            {
                global.EnemiesKilled += 1;
            }

            orig(self, _hitVec, _loadedDead);
        }

        private void CharacterAIUpdateHook(On.CharacterAI.orig_Update orig, CharacterAI self)
        {
            if (global.IsGameplayStarted && global.CurrentTime > 0)
            {
                if (self.TargetingSystem.TargetableFactions.Count() > 0)
                {
                    ForceEnemiesAllied(self.Character);
                }
                if (self.TargetingSystem.LockedCharacter.IsAI)
                {
                    ForceTargetOnPlayer(self.Character);
                }
            }

            orig(self);
        }

        private void AIEndCombatHook(On.AISCombat.orig_EndCombat orig, AISCombat self)
        {
            if (global.IsGameplayStarted && global.CurrentTime > 0)
            {
                Character c = At.GetValue(typeof(AIState), self as AIState, "m_character") as Character;

                if (c.Health > 0)
                {
                    ForceTargetOnPlayer(c);
                    return;
                }
            }

            orig(self);
        }

        private void AIEquipHook(On.AISCombat.orig_EquipRequiredWeapons orig, AISCombat self)
        {
            try
            {
                orig(self);
            }
            catch
            {
                Character c = At.GetValue(typeof(AIState), self as AIState, "m_character") as Character;
               
                foreach (Weapon w in self.RequiredWeapon)
                {
                    if (!c.Inventory.OwnsItem(w.ItemID))
                    {
                        Item i = ItemManager.Instance.GenerateItemNetwork(w.ItemID);
                        c.Inventory.TakeItem(i, false);
                        At.Call(c.Inventory.Equipment, "EquipWithoutAssociating", new object[] { i, false });
                    }
                }
            }
        }

        private void InitFactionsHook(On.TargetingSystem.orig_InitTargetableFaction orig, TargetingSystem self)
        {
            Character c = At.GetValue(typeof(TargetingSystem), self, "m_character") as Character;

            if (c.IsAI && global.IsGameplayStarted && global.CurrentTime > 0)
            {
                ForceEnemiesAllied(c);
            }
            else
            {
                orig(self);
            }
        }

        private void AISquadManagerHook(On.AISquadManager.orig_Update orig, AISquadManager self)
        {
            if (global.IsGameplayStarted && global.CurrentTime > 0)
            {
                return;
            }

            orig(self);
        }

    }
}
