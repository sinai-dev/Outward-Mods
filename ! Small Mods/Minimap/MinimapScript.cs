using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Minimap
{
    /* TODO     
        * Add more culling to minimap camera
        * Try increase brightness in dark areas 
        * ...
    */

    public class MinimapScript : MonoBehaviour
    {
        // ~~~~~ static members ~~~~~

        // [0] = top player (or only player), [1] = bottom player
        public static MinimapScript[] Instances = new MinimapScript[2];

        // the amount P1's minimap drops when split begins
        public const float P1_SPLIT_OFFSET = 273.2f;

        // state
        private static bool InOutdoorRegion => OutdoorRegions.Contains(SceneManagerHelper.ActiveSceneName);
        private static bool ShowingBigMap;

        // minimap position defaults
        private static readonly Vector3[] CornerPositions = new Vector3[]
        {
            new Vector3(802f, 434f, 0f),  // player 1
            new Vector3(802f, 162.5f, 0f) // player 2
        };
        private static readonly Vector3 BigMapPosition = Vector3.zero;
        private static readonly Vector2 SmallMapSize = new Vector2(200f, 200f);
        private static readonly Vector2 BigMapSize = new Vector2(800f, 800f);

        // outdoor maps (increased minimap size)
        private static readonly HashSet<string> OutdoorRegions = new HashSet<string>
        {
            "ChersoneseNewTerrain", "Emercar", "HallowedMarshNewTerrain", "Abrassar", "AntiqueField", 
            "CierzoTutorial",  "CierzoNewTerrain", "Berg",  "Monsoon",  "Levant",  "Harmattan",
        };

        // maps which require manual Z align
        private static readonly Dictionary<string, float> ManualZRotations = new Dictionary<string, float>
        {
            { "Abrassar",                -90f },
            { "HallowedMarshNewTerrain", 90f },
        };

        // ~~~~~ instance members ~~~~~

        private bool m_enabled = true;
        private int m_splitID;

        private float m_cameraOrthoSize;    // config: P1_ZOOM, P2_ZOOM
        private float m_outdoorExtraSize;   // config: P1_OUTDOOREXTRA, P2_OUTDOOREXTRA

        private float m_currentZrotation;   // Usually 0, unless ManualZRotations overrides it.

        private Character m_ownerCharacter;
        private RawImage m_mapImage;
        private Camera m_minimapCamera;
        private RenderTexture m_miniRenderTexture;
        private RenderTexture m_bigRenderTexture;
        private Transform m_hudTransform;
        private Transform m_mapTransform;

        // ~~~~~ instance methods ~~~~~

        public void Init(Character character)
        {
            m_ownerCharacter = character;
            m_splitID = character.OwnerPlayerSys.PlayerID;
            Instances[m_splitID] = this;

            if (m_splitID == 0) CornerPositions[0] = new Vector3(802f, 434f, 0f); // new player 1, reset this to be safe

            m_hudTransform = character.CharacterUI.transform.Find("Canvas/GameplayPanels/HUD");
            m_mapTransform = MenuManager.Instance.transform.Find("GeneralMenus");

            SceneManager.activeSceneChanged += ActiveSceneChanged;

            // Setup actual minimap camera
            m_minimapCamera = new GameObject("MinimapCamera", typeof(Camera))
                                .GetComponent<Camera>();

            m_minimapCamera.transform.parent = m_ownerCharacter.transform;

            m_minimapCamera.orthographic = true;
            m_minimapCamera.rect = new Rect(0, 0, 1, 1);

            m_minimapCamera.clearFlags = CameraClearFlags.SolidColor;
            m_minimapCamera.backgroundColor = new Color(0.5f, 0.5f, 0.5f);

            // setup RenderTexture
            m_miniRenderTexture = new RenderTexture(200, 200, 0);
            m_minimapCamera.targetTexture = m_miniRenderTexture;

            m_bigRenderTexture = new RenderTexture(800, 800, 0);

            // setup map UI element
            m_mapImage = new GameObject("MinimapImage", typeof(RawImage))
                               .GetComponent<RawImage>();

            m_mapImage.rectTransform.localPosition = CornerPositions[m_splitID];
            m_mapImage.rectTransform.sizeDelta = SmallMapSize;
            m_mapImage.transform.SetParent(m_hudTransform, false);

            // setup the render material for the image
            var mat = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<Material>()
                                                      .FirstOrDefault(x => x.name == "mat_renderGame"));

            mat.SetTexture(1, m_miniRenderTexture);
            m_mapImage.material = mat;

            // add the white dot on the minimap
            var dotGO = new GameObject("dot");
            dotGO.transform.SetParent(m_mapImage.transform, false);
            dotGO.transform.ResetLocal();
            var dotImg = dotGO.AddComponent<Image>();
            dotImg.rectTransform.sizeDelta = new Vector2(3f, 3f);
            dotImg.rectTransform.localPosition = new Vector3(0f, 0f, 0f);

            SetZRotation();

            ApplyFromConfig();

            Debug.Log("Done");
        }

        public void Update()
        {
            if (!m_ownerCharacter)
            {
                Destroy(this.gameObject);
                return;
            }

            // need to set rotation every update, otherwise it will rotate with the player.
            m_minimapCamera.transform.eulerAngles = new Vector3(90f, 0f, m_currentZrotation);

            // player 1 can align with Shift + Up/Down
            if (m_splitID == 0)
            {
                P1_ManualAlign();
            }
        }

        private void ActiveSceneChanged(Scene arg0, Scene arg1)
        {
            var scene = SceneManagerHelper.ActiveSceneName;
            if (scene == "LowMemory_TransitionScene" || scene == "MainMenu_Empty")
            {
                return;
            }

            SetZRotation();

            m_minimapCamera.orthographicSize = m_cameraOrthoSize;
            if (InOutdoorRegion)
            {
                m_minimapCamera.orthographicSize += m_outdoorExtraSize;
            }

            var height = InOutdoorRegion ? 250f : 3f;

            m_minimapCamera.farClipPlane = height * 2;

            m_minimapCamera.transform.position = m_ownerCharacter.transform.position + (Vector3.up * height);
        }

        public void ApplyFromConfig()
        {
            if (m_splitID == 0)
            {
                m_cameraOrthoSize = (float)Settings.Instance.GetValue(Settings.P1_ZOOM);
                m_outdoorExtraSize = (float)Settings.Instance.GetValue(Settings.P1_OUTDOOREXTRA);
            }
            else
            {
                m_cameraOrthoSize = (float)Settings.Instance.GetValue(Settings.P2_ZOOM);
                m_outdoorExtraSize = (float)Settings.Instance.GetValue(Settings.P2_OUTDOOREXTRA);
            }


            m_minimapCamera.orthographicSize = m_cameraOrthoSize;

            if (InOutdoorRegion)
            {
                m_minimapCamera.orthographicSize += m_outdoorExtraSize;
            }
        }

        public void ToggleEnable()
        {
            m_enabled = !m_enabled;

            this.enabled = m_enabled;
            this.m_minimapCamera.enabled = m_enabled;
            this.m_mapImage.gameObject.SetActive(m_enabled);
        }

        public void SetZRotation()
        {
            var scene = SceneManagerHelper.ActiveSceneName;
            if (ManualZRotations.ContainsKey(scene))
                m_currentZrotation = ManualZRotations[scene];
            else
                m_currentZrotation = 0f;
        }

        public void OnShowBigMap()
        {
            ShowingBigMap = true;

            m_minimapCamera.targetTexture = m_bigRenderTexture;
            m_mapImage.material.SetTexture(1, m_bigRenderTexture);

            m_mapImage.transform.SetParent(m_mapTransform, false);
            m_mapImage.rectTransform.localPosition = BigMapPosition;
            m_mapImage.rectTransform.sizeDelta = BigMapSize;

            m_minimapCamera.orthographicSize += 50f;

            // todo change position of camera to middle of scene
        }

        public void OnHideBigMap()
        {
            if (!ShowingBigMap) { return; }

            ShowingBigMap = false;

            m_minimapCamera.targetTexture = m_miniRenderTexture;
            m_mapImage.material.SetTexture(1, m_miniRenderTexture);

            m_mapImage.transform.SetParent(m_hudTransform, false);
            m_mapImage.rectTransform.localPosition = CornerPositions[m_splitID];
            m_mapImage.rectTransform.sizeDelta = SmallMapSize;

            m_minimapCamera.orthographicSize -= 50f;
        }

        public static void P1_OnSplitBegin()
        {
            var pos = CornerPositions[0];
            pos.y -= P1_SPLIT_OFFSET;

            if (!ShowingBigMap)
            {
                Instances[0].m_mapImage.rectTransform.localPosition = pos;
            }
        }

        public static void P1_OnSplitEnd()
        {
            var pos = CornerPositions[0];
            pos.y += P1_SPLIT_OFFSET;

            if (!ShowingBigMap)
            {
                Instances[0].m_mapImage.rectTransform.localPosition = pos;
            }
        }

        public void P1_ManualAlign()
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                bool set = false;
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    m_cameraOrthoSize += 0.5f;
                    set = true;
                }
                else if (Input.GetKey(KeyCode.UpArrow))
                {
                    m_cameraOrthoSize -= 0.5f;
                    set = true;
                }

                if (set)
                {
                    m_minimapCamera.orthographicSize = m_cameraOrthoSize;

                    if (InOutdoorRegion)
                    {
                        m_minimapCamera.orthographicSize += m_outdoorExtraSize;
                    }

                    Settings.Instance.SetValue(Settings.P1_ZOOM, m_cameraOrthoSize);
                }
            }
        }

        // ~~~~~~~~~~~~~~~~~~

        //// Don't think this is necessary? idk

        //public void OnDestroy()
        //{
        //    SceneManager.activeSceneChanged -= ActiveSceneChanged;
        //}
    }
}
