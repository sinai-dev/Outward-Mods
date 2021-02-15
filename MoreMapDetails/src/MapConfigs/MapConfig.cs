using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MoreMapDetails.MapConfigs
{
    public class MapConfig
    {
        public Vector2 MarkerOffset;
        public Vector2 MarkerScale;
        public float Rotation;

        // --- Map Config dictionary ---
        // Key: MapID (as per MapDisplay class)
        // Value: MapDependingScene settings. Only using the offset / rotation / scale values.

        public static Dictionary<int, MapConfig> ConfigDict = new Dictionary<int, MapConfig>
        {
            {
                1, // Chersonese
                new MapConfig()
                {
                    MarkerOffset = new Vector2(-531f, -543f),
                    MarkerScale = new Vector2(0.526f, 0.526f),
                    Rotation = 0f
                }
            },
            {
                3, // Hallowed Marsh
                new MapConfig()
                {
                    MarkerOffset = new Vector2(-573.0f, -515.0f),
                    MarkerScale = new Vector2(0.553f, 0.553f),
                    Rotation = 90f
                }
            },
            {
                5, // Abrassar
                new MapConfig()
                {
                    MarkerOffset = new Vector2(3f, -5f),
                    MarkerScale = new Vector2(0.534f, 0.534f),
                    Rotation = -90f
                }
            },
            {
                7, // Enmerkar Forest
                new MapConfig()
                {
                    MarkerOffset = new Vector2(-500f, -500f),
                    MarkerScale = new Vector2(0.5f, 0.5f),
                    Rotation = 0f
                }
            },
            {
                9, // Antique Plateau
                new MapConfig
                {
                    MarkerOffset = new Vector2(-504f, -505f),
                    MarkerScale = new Vector2(0.50f, 0.50f),
                    Rotation = 0f
                }
            },
            {
                10, // Caldera
                new MapConfig
                {
                    //MarkerOffset = new Vector2(-502.0f, -497.0f),
                    //MarkerScale = new Vector2(0.471f, 0.471f),
                    MarkerOffset = new Vector2(-504.0f, -500.0f),
                    MarkerScale = new Vector2(0.503f, 0.503f),
                    Rotation = -90f
                }
            },
        };



        ///*
        // * TEMP DEBUG
        // * I used this to align the map offsets for the exterior regions more accurately. 
        // * PgDown (-) and PgDown (+) adjust the scale.
        // * Arrow keys adjust the X/Y offset.
        // * It will print the value (after changes) with Debug.Log()
        //*/

        //internal void Update()
        //{
        //    // adjust scale
        //    if (Input.GetKey(KeyCode.PageUp))
        //    {
        //        AdjustConfig(Vector2.zero, Vector2.one * -0.001f);
        //    }
        //    if (Input.GetKey(KeyCode.PageDown))
        //    {
        //        AdjustConfig(Vector2.zero, Vector2.one * 0.001f);
        //    }

        //    // adjust offsets
        //    if (Input.GetKey(KeyCode.DownArrow))
        //    {
        //        AdjustConfig(new Vector2(0, -1), Vector2.zero);
        //    }
        //    if (Input.GetKey(KeyCode.UpArrow))
        //    {
        //        AdjustConfig(new Vector2(0, 1), Vector2.zero);
        //    }
        //    if (Input.GetKey(KeyCode.RightArrow))
        //    {
        //        AdjustConfig(new Vector2(1, 0), Vector2.zero);
        //    }
        //    if (Input.GetKey(KeyCode.LeftArrow))
        //    {
        //        AdjustConfig(new Vector2(-1, 0), Vector2.zero);
        //    }
        //}

        //private void AdjustConfig(Vector2 _offset, Vector2 scale)
        //{
        //    MapDisplay.Instance.CurrentMapScene.MarkerOffset += _offset;
        //    MapDisplay.Instance.CurrentMapScene.MarkerScale += scale;
        //    MapConfigs[m_mapID].MarkerOffset = MapDisplay.Instance.CurrentMapScene.MarkerOffset;
        //    MapConfigs[m_mapID].MarkerScale = MapDisplay.Instance.CurrentMapScene.MarkerScale;
        //    Debug.Log("Offset: " + MapDisplay.Instance.CurrentMapScene.MarkerOffset + ", Scale: " + MapDisplay.Instance.CurrentMapScene.MarkerScale.ToString("0.000"));
        //}
    }
}
