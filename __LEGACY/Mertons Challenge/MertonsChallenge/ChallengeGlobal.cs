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
    // partiality loader
    public class ModLoader : PartialityMod
    {
        public GameObject obj = null;
        public ChallengeGlobal script;

        public ModLoader()
        {
            ModID = "Endless Horde";
            Version = "1.0";
            author = "Sinai";
        }

        public override void OnEnable()
        {
            base.OnEnable();

            if (obj == null)
            {
                obj = new GameObject("EndlessHorde");
                GameObject.DontDestroyOnLoad(obj);
            }

            script = obj.AddComponent<ChallengeGlobal>();
            script.loader = this;
            script.Init();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }
    }

    // actual mod
    public class ChallengeGlobal : MonoBehaviour
    {
        public ModLoader loader;
        public ChallengeGUI gui;
        public EnemyManager enemies;
        public StatHelpers statUtil;

        public SceneTemplate CurrentTemplate = null;
        public string CurrentScene = "";
        public bool SceneChangeFlag = false;

        public GameObject InteractorObj;

        // gameplay control / metrics
        public bool IsGameplayStarted = false;
        public float StartTime = 0f;
        public float CurrentTime = -1;
        public int EnemiesKilled = 0;
        public int EnemiesInQueue = 0;
        public bool ShouldRest = false;
        public int BossesSpawned = 0;
        public bool BossActive;
        public float TimeSpentOnBosses = 0;

        public void Init()
        {
            // OLogger.CreateLog(new Rect(Screen.width - 465, Screen.height - 175, 465, 155), "Default", true, true);

            AddGlobalComponent(typeof(ChallengeGUI), "gui");
            AddGlobalComponent(typeof(EnemyManager), "enemies");
            AddGlobalComponent(typeof(StatHelpers), "statUtil");
            enemies.Init();

            // Debug.Log("Endless Horde init");
        }

        internal void Update()
        {
            if (CurrentScene != SceneManagerHelper.ActiveSceneName) { SceneChangeFlag = true; }

            if (NetworkLevelLoader.Instance.IsGameplayPaused || Global.Lobby.PlayersInLobbyCount < 1) { return; }

            if (SceneChangeFlag)
            {
                CurrentScene = SceneManagerHelper.ActiveSceneName;
                SceneChangeFlag = false;

                Reset();

                if (Templates.Scenes.ContainsKey(CurrentScene))
                {
                    CurrentTemplate = Templates.Scenes[CurrentScene];
                    SetupInteractor(Templates.Scenes[CurrentScene].InteractorPos);
                }
                else
                {
                    CurrentTemplate = null;
                }             
            }
        }

        // ===== GAMEPLAY MASTER COROUTINE =====

        public IEnumerator GameplayCoroutine()
        {
            // setup healing rift
            var rift = new GameObject("HealingRift");
            rift.transform.position = CurrentTemplate.InteractorPos;
            //rift.AddComponent(new SigilHeal() { global = this });
            var sigil = rift.AddComponent<SigilHeal>();
            sigil.global = this;

            StartTime = Time.time;
            while (IsGameplayStarted)
            {
                if (Global.GamePaused) { yield return null; continue; }

                CurrentTime += Time.deltaTime;

                if (AllPlayersDead()) { break; }

                if (ShouldRest) // resting after boss wave
                {
                    // repair players items
                    foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby)
                    {
                        ps.ControlledCharacter.Inventory.RepairEverything();
                    }

                    float time = 5;
                    StartCoroutine(gui.SetMessage("Death only makes Merton more powerful...", time));
                    while (time > 0 && IsGameplayStarted) { time -= Time.deltaTime; yield return new WaitForEndOfFrame(); }
                    if (!IsGameplayStarted) { break; }
                    ShouldRest = false;
                }

                if (ShouldSpawnBoss() || BossActive)
                {
                    if (TotalEnemiesInPlay() < 1)
                    {
                        BossActive = true;
                        BossesSpawned++;
                        enemies.SpawnBoss(CurrentTemplate.BossTemplate);
                    }
                }
                else if (ShouldSpawnEnemy())
                {
                    float f = CalculateNextSpawnTime();
                    StartCoroutine(SpawnEnemyAfterDelay(f));
                }

                yield return new WaitForEndOfFrame();
            }

            OnEndGameplay();
        }

        private IEnumerator SpawnEnemyAfterDelay(float seconds)
        {
            EnemiesInQueue++;

            float f = seconds;
            while (f > 0)
            {
                while (Global.GamePaused)
                {
                    yield return null;
                }
                f -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            if (IsGameplayStarted) // make sure gameplay is still running
            {
                enemies.SpawnRandomEnemy();
            }
            EnemiesInQueue--;
        }

        public bool ShouldSpawnEnemy()
        {
            return !BossActive && TotalEnemiesInPlay() < EnemySpawnTarget();
        }

        public int TotalEnemiesInPlay()
        {
            int n = BossActive ? 1 : 0;
            return n + AliveEnemies() + EnemiesInQueue;
        }

        public int AliveEnemies()
        {
            return enemies.ActiveMinions.Where(x => x.Health > 0).Count();
        }

        public int EnemySpawnTarget()
        {
            return 2 + (int)Math.Floor((decimal)(Time.time - StartTime) / 90);
        }

        // spawn rate: basically its random between 15 and 30 secs to start with, and moves down to between 3 and 8 secs at max spawn rate
        public float CalculateNextSpawnTime()
        {
            float modifier = EnemySpawnTarget() * 0.5f;
            float f = 30 - modifier;

            float middle = 15 - (modifier * 0.5f);
            middle = Mathf.Clamp(middle, 8, 15);

            float min = Mathf.Clamp(f * 0.5f, 3, middle);
            float max = Mathf.Clamp(f, middle, 30);

            return UnityEngine.Random.Range(min, max);
        }

        public bool ShouldSpawnBoss()
        {
            int countTarget = 0 + (int)Math.Floor((decimal)(Time.time - StartTime - TimeSpentOnBosses) / 150);

            return countTarget > BossesSpawned;
        }

        private bool AllPlayersDead()
        {
            bool flag = true;

            foreach (PlayerSystem ps in Global.Lobby.PlayersInLobby)
            {
                Character c = ps.ControlledCharacter;
                if (c.Health > 0) { flag = false; break; }
            }

            return flag;
        }

        public void Reset()
        {
            enemies.ActiveMinions.Clear();
            StartTime = 0;
            CurrentTime = -1;
            IsGameplayStarted = false;
            TimeSpentOnBosses = 0;
            EnemiesKilled = 0;
            EnemiesInQueue = 0;
            ShouldRest = false;
            BossesSpawned = 0;
            BossActive = false;
        }

        public void OnEndGameplay()
        {
            StartCoroutine(gui.SetMessage("Time: " + gui.GetTimeString() + " | Enemies Killed: " + EnemiesKilled + " | Merton's Form: " + BossesSpawned, 10));
            Reset();
        }

        // ================= SCENE SETUP, INTERACTOR STUFF, ETC ===================

        // actual interactor event
        private void InteractorBeginEvent()
        {
            if (CurrentTemplate == null || IsGameplayStarted) { return; }

            StartCoroutine(BeginGameplay(CurrentTemplate));
        }

        // scene setup for gameplay
        private IEnumerator BeginGameplay(SceneTemplate template)
        {
            BlackFade.Instance.StartFade(true);

            Reset();
            IsGameplayStarted = true;
            CurrentTime = 0;

            InteractorObj.SetActive(false);

            // ======== actual scene setup ========

            DisableCanvases(); // disable NodeCanvas blackboards (allow all enemies to be spawned, etc)

            // set time of day
            var env = EnvironmentConditions.Instance;
            env.TimeJump(24 - (env.TODRatio * 24) - CurrentTemplate.ToDStart);

            foreach (Character c in CharacterManager.Instance.Characters.Values)
            {
                if (!c.IsAI)
                {
                    c.Teleport(template.PlayerSpawnPos, c.transform.rotation);
                }
                else
                {
                    if (c.Health <= 0) { c.RessurectCampingSquad(); } else { c.ResetStats(); }
                    c.gameObject.SetActive(false);
                }
            }

            StartCoroutine(gui.SetMessage("Prepare yourself... ", 5));

            bool doneFade = false;
            float f = 5;
            while (f > 0 && !MenuManager.Instance.IsReturningToMainMenu && !MenuManager.Instance.IsMasterLoadingDisplayed)
            {
                f -= Time.deltaTime;
                if (f < 4f && !doneFade) { doneFade = true; BlackFade.Instance.StartFade(false); }
                yield return new WaitForEndOfFrame();
            }

            if (Global.Lobby.PlayersInLobbyCount > 0)
            {
                // spawn a minion to start with
                enemies.SpawnRandomEnemy();

                // begin gameplay
                StartCoroutine(GameplayCoroutine());
            }
        }

        // setup the entire interactor object and merton NPC (no gameplay)
        private void SetupInteractor(Vector3 pos)
        {
            // base gameobject
            InteractorObj = new GameObject("Horde_Interactor");
            InteractorObj.transform.position = pos;

            // fire sigil visuals
            StartCoroutine(AddItemToInteractor(8000010, Vector3.down * 0.35f, Vector3.zero, false, InteractorObj.transform));

            // setup Merton NPC
            var merton = SetupBasicNPC(pos, true);
            merton.transform.parent = InteractorObj.transform;
            // setup auto look
            merton.AddComponent<NPCLookFollow>();
            // setup character visuals
            try { SetupNpcArmor(merton, new List<int> { 3200030, 3200031, 3200032 }); }
            catch { }
            // animation fix
            merton.SetActive(false);
            merton.SetActive(true);
            // make big
            merton.transform.localScale *= 1.6f;

            // setup components
            InteractionTriggerBase triggerBase = InteractorObj.AddComponent<InteractionTriggerBase>();
            InteractionActivator activator = InteractorObj.AddComponent<InteractionActivator>();
            //InteractionBase interactBase = InteractorObj.AddComponent(new InteractionBase { OnActivationEvent = new UnityAction(InteractorBeginEvent) });
            InteractionBase interactBase = InteractorObj.AddComponent<InteractionBase>();
            interactBase.OnActivationEvent = new UnityAction(InteractorBeginEvent);
            triggerBase.SetActivator(activator);
            At.SetValue("Begin <color=#FF0000>Merton's Challenge</color>", typeof(InteractionActivator), activator, "m_overrideBasicText");
            At.SetValue(interactBase, typeof(InteractionActivator), activator, "m_sceneBasicInteraction");
        }

        private void SetupNpcArmor(GameObject _npc, List<int> _items)
        {
            var charVisuals = _npc.GetComponent<CharacterVisuals>();
            foreach (int id in _items)
            {
                // get visual or special visual prefab
                var item = ResourcesPrefabManager.Instance.GetItemPrefab(id);
                var t = item.SpecialVisualPrefabDefault ?? item.VisualPrefab;
                var visuals = t.GetComponent<ArmorVisuals>();

                // instantiate a copy of the appropriate visual prefab
                var visuals2 = Instantiate(visuals.gameObject, _npc.transform).GetComponent<ArmorVisuals>(); 
                
                At.Call(visuals2, "Awake", null); // call the private Awake() method
                
                visuals2.Show(); // actually show and apply the visuals
                visuals2.ApplyToCharacterVisuals(charVisuals);
            }
        }

        // add an item to an interactor object
        public IEnumerator AddItemToInteractor(int ItemID, Vector3 pos_Offset, Vector3 rot_Offset, bool DestroyInteractor, Transform baseObj)
        {
            Item item = ItemManager.Instance.GenerateItemNetwork(ItemID);

            item.transform.parent = baseObj;
            item.transform.position = baseObj.position + pos_Offset;
            item.transform.rotation = Quaternion.Euler(rot_Offset);

            if (DestroyInteractor)
            {
                float startTime = Time.time;
                while (Time.time - startTime < 3f && !item.GetComponentInChildren<InteractionTake>()) { yield return null; }

                if (item.GetComponentInChildren<InteractionTake>()) { Destroy(item.GetComponentInChildren<InteractionTake>().gameObject); }
                if (item.GetComponent<Rigidbody>() is Rigidbody rigidbody) { Destroy(rigidbody); }
            }

            if (item is FueledContainer fueledItem) { fueledItem.Kindle(); } // kindle campfires            
            if (item.GetComponent<Ephemeral>() is Ephemeral ephemeral) { Destroy(ephemeral); } // makes Sigils disappear, etc

            yield return null;
        }

        // create a basic human SNPC
        private GameObject SetupBasicNPC(Vector3 pos, bool disableDefaultVisuals)
        {
            var panel = Resources.FindObjectsOfTypeAll<CharacterCreationPanel>()[0];
            GameObject prefab = At.GetValue(typeof(CharacterCreationPanel), panel, "CharacterCreationPrefab") as GameObject;

            if (prefab && prefab.transform.Find("HumanSNPC") is Transform origNPC)
            {
                Transform npc = Instantiate(origNPC);
                npc.position = pos;

                if (disableDefaultVisuals)
                {
                    try { npc.GetComponent<CharacterVisuals>().Head.gameObject.SetActive(false); } catch { }

                    foreach (ArmorVisuals visuals in npc.GetComponentsInChildren<ArmorVisuals>())
                    {
                        visuals.gameObject.SetActive(false);
                    }
                }

                return npc.gameObject;
            }

            return null;
        }

        // ========================= misc =========================

        // disable NodeCanvas Behaviour Trees, to stop quest checks and such
        public void DisableCanvases()
        {
            var canvases = Resources.FindObjectsOfTypeAll(typeof(NodeCanvas.BehaviourTrees.BehaviourTreeOwner));

            foreach (NodeCanvas.BehaviourTrees.BehaviourTreeOwner tree in canvases)
            {
                tree.gameObject.SetActive(false);
            }
        }

        private void AddGlobalComponent(Type t, string FieldName)
        {
            object obj = loader.obj.GetComponent(t) ?? loader.obj.AddComponent(t);

            try
            {
                typeof(ChallengeGlobal).GetField(FieldName).SetValue(this, obj);
                obj.GetType().GetField("global").SetValue(obj, this);
            }
            catch //(Exception ex)
            {
                //Debug.Log("Trying to add " + FieldName + " :: " + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

    }
}
