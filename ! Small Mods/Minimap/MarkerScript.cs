using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace Minimap
{
    public class MarkerScript : MonoBehaviour
    {
        public enum Types
        {
            Player,
            Enemy,
            Loot,
            NPC
        }

        // static

        public static readonly List<MarkerScript> Instances = new List<MarkerScript>();
        public static bool playersEnabled;
        public static bool enemiesEnabled;
        public static bool lootEnabled;
        public static bool npcEnabled;

        public static void ApplyConfigToInstances()
        {
            playersEnabled = (bool)Settings.Instance.GetValue(Settings.PLAYER_MARKERS);
            enemiesEnabled = (bool)Settings.Instance.GetValue(Settings.ENEMY_MARKERS);
            lootEnabled = (bool)Settings.Instance.GetValue(Settings.LOOT_MARKERS);
            npcEnabled = (bool)Settings.Instance.GetValue(Settings.NPC_MARKERS);

            foreach (var marker in Instances)
            {
                marker.ApplyFromConfig();
            }
        }

        // instance

        public bool Enabled { get; set; }
        public Types Type { get; private set; }

        private MeshRenderer m_sphereMesh;

        public void Init(Types type)
        {
            Instances.Add(this);

            Type = type;
            m_sphereMesh = GetComponent<MeshRenderer>();

            ApplyFromConfig();
        }

        public void ApplyFromConfig()
        {
            switch (Type)
            {
                case Types.Enemy:   Enabled = enemiesEnabled;   break;
                case Types.Player:  Enabled = playersEnabled;   break;
                case Types.Loot:    Enabled = lootEnabled;      break;
                case Types.NPC:     Enabled = npcEnabled;       break;
            }
        }

        public void Update()
        {
            if (m_sphereMesh.enabled != Enabled)
            {
                m_sphereMesh.enabled = Enabled;
            }
        }

        public void OnDestroy()
        {
            Instances.Remove(this);
        }
    }
}
