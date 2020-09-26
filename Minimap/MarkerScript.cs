using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

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

        public const string MARKER_NAME = "MarkerSphere";
        public const int MARKER_LAYER = 14;

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

        public static void AddMarker(GameObject obj, Types type)
        {
            if (obj.transform.Find(MARKER_NAME))
                return;

            //var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var marker = new GameObject(MARKER_NAME, typeof(Canvas), typeof(Image))
            {
                layer = MARKER_LAYER
            };

            marker.transform.parent = obj.transform;
            marker.transform.ResetLocal();

            marker.transform.localScale = Vector3.one * 0.05f;

            var rot = marker.transform.localEulerAngles;
            rot.x += 90f;
            marker.transform.localEulerAngles = rot;

            Sprite sprite = null;
            switch (type)
            {
                case Types.Player:  sprite = MapDisplay.Instance.AddedMarkerSprites[1]; break; // green square
                case Types.Enemy:   sprite = MapDisplay.Instance.AddedMarkerSprites[3]; break; // red triangle
                case Types.NPC:     sprite = MapDisplay.Instance.AddedMarkerSprites[0]; break; // blue circle
                case Types.Loot:    sprite = MapDisplay.Instance.AddedMarkerSprites[2]; break; // purple penta
            }

            var canvas = marker.GetComponent<Canvas>();
            canvas.worldCamera = Camera.main;
            canvas.renderMode = RenderMode.WorldSpace;

            var image = marker.GetComponent<Image>();
            image.sprite = sprite;
            image.material = MinimapMod.AlwaysOnTopMaterial;

            marker.transform.localScale *= 2;

            var script = marker.AddComponent<MarkerScript>();
            script.Init(type);

            //// make bright
            //var light = marker.AddComponent<Light>();
            //light.color = color;
            //light.intensity = 15f;
            //light.range = 5f;
            //light.cullingMask = 1 << MARKER_LAYER;
        }

        // instance

        public bool Enabled { get; set; }
        public Types Type { get; private set; }

        private Image m_image;

        public void Init(Types type)
        {
            Instances.Add(this);

            Type = type;
            m_image = GetComponent<Image>();

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
            if (m_image.enabled != Enabled)
            {
                m_image.enabled = Enabled;
            }
        }

        public void OnDestroy()
        {
            Instances.Remove(this);
        }
    }
}
