using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Minimap
{
    public class MinimapScript : MonoBehaviour
    {
        public static MinimapScript P1Instance;
        public static MinimapScript P2Instance;

        public const float P1_ADJUST_SPLIT = 273.2f;

        public bool Enabled { get; private set; } = true;

        public RawImage CanvasImage { get; private set; }

        private Character _ownerCharacter;

        private float _orthoSize = 14f;
        private bool inOutdoorRegion;
        private bool showingBigMap = false;

        private Camera _minimapCamera;
        private RenderTexture _miniRenderTexture;
        private RenderTexture _bigRenderTexture;

        private Transform _hudTransform;
        private Transform _mapTransform;

        private static readonly Vector3 P1_cornerPos = new Vector3(802f, 434f, 0f);
        private static readonly Vector3 P2_cornerPos = new Vector3(802f, 162.5f, 0f);
        private static readonly Vector3 bigMapPosition = new Vector3(0, 0, 0);

        private static readonly Vector2 smallMapSize = new Vector2(200f, 200f);
        private static readonly Vector2 bigMapSize = new Vector2(800f, 800f);

        public void ToggleEnable()
        {
            Enabled = !Enabled;

            this.enabled = Enabled;
            this._minimapCamera.enabled = Enabled;
            this.CanvasImage.gameObject.SetActive(Enabled);
        }

        private static readonly HashSet<string> OutdoorRegions = new HashSet<string>
        {
            "ChersoneseNewTerrain",
            "Emercar",
            "HallowedMarshNewTerrain",
            "Abrassar",
            "AntiqueField",
            "CierzoTutorial",
            "CierzoNewTerrain",
            "Berg",
            "Monsoon",
            "Levant",
            "Harmattan",
        };

        private static readonly Dictionary<string, Vector3> CustomRotations = new Dictionary<string, Vector3>
        {
            {
                "Abrassar",
                new Vector3(90f, 0f, -90f)
            },
            {
                "HallowedMarshNewTerrain",
                new Vector3(90f, 0f, 90f)
            }
        };

        //public void OnGUI()
        //{
        //    GUILayout.BeginArea(new Rect(5, 5, 200, 200), GUI.skin.box);

        //    GUILayout.Label("Ortho size: " + _orthoSize);

        //    GUILayout.EndArea();
        //}

        public void Init(Character character)
        {
            _ownerCharacter = character;

            SceneManager.activeSceneChanged += ActiveSceneChanged;

            if (character.OwnerPlayerSys.PlayerID == 0)
            {
                P1Instance = this;
            }
            else
            {
                P2Instance = this;
            }
            
            // Setup actual minimap camera

            _minimapCamera = new GameObject("MinimapCamera", typeof(Camera))
                                .GetComponent<Camera>();

            _minimapCamera.transform.parent = _ownerCharacter.transform;

            _minimapCamera.orthographic = true;
            _minimapCamera.orthographicSize = _orthoSize;
            _minimapCamera.rect = new Rect(0, 0, 1, 1);
            //_minimapCamera.transform.localEulerAngles = new Vector3(90f, 0f, 0f);

            // setup RenderTexture
            _miniRenderTexture = new RenderTexture(200, 200, 0);
            _minimapCamera.targetTexture = _miniRenderTexture;

            _bigRenderTexture = new RenderTexture(800, 800, 0);

            // setup UI element
            CanvasImage = new GameObject("MinimapImage", typeof(RawImage))
                               .GetComponent<RawImage>();

            if (_ownerCharacter.OwnerPlayerSys.PlayerID == 0)
            {
                CanvasImage.rectTransform.localPosition = P1_cornerPos;
            }
            else
            {
                CanvasImage.rectTransform.localPosition = P2_cornerPos;
            }

            CanvasImage.rectTransform.sizeDelta = smallMapSize;

            _hudTransform = character.CharacterUI.transform.Find("Canvas/GameplayPanels/HUD");
            _mapTransform = MenuManager.Instance.transform.Find("GeneralMenus");

            CanvasImage.transform.SetParent(_hudTransform, false);

            var mat = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<Material>()
                                                      .FirstOrDefault(x => x.name == "mat_renderGame"));

            mat.SetTexture(1, _miniRenderTexture);
            CanvasImage.material = mat;

            // add the dot
            var dotGO = new GameObject("dot");
            dotGO.transform.SetParent(CanvasImage.transform, false);
            dotGO.transform.ResetLocal();
            var dotImg = dotGO.AddComponent<Image>();
            dotImg.rectTransform.sizeDelta = new Vector2(3f, 3f);
            dotImg.rectTransform.localPosition = new Vector3(0f, 0f, 0f);

            Debug.Log("Done");
        }

        private void ActiveSceneChanged(Scene arg0, Scene arg1)
        {
            if (SceneManagerHelper.ActiveSceneName == "LowMemory_TransitionScene" || SceneManagerHelper.ActiveSceneName == "MainMenu_Empty")
            {
                return;
            }

            bool wasOutdoor = inOutdoorRegion;

            inOutdoorRegion = OutdoorRegions.Contains(SceneManagerHelper.ActiveSceneName);

            if (!wasOutdoor && inOutdoorRegion)
            {
                _orthoSize += 50f;
            }
            else if (wasOutdoor && !inOutdoorRegion)
            {
                _orthoSize -= 50f;
            }

            _minimapCamera.orthographicSize = _orthoSize;
        }

        public void ShowBigMap()
        {
            showingBigMap = true;

            _minimapCamera.targetTexture = _bigRenderTexture;
            CanvasImage.material.SetTexture(1, _bigRenderTexture);

            CanvasImage.transform.SetParent(_mapTransform, false);
            CanvasImage.rectTransform.localPosition = bigMapPosition;
            CanvasImage.rectTransform.sizeDelta = bigMapSize;

            _minimapCamera.orthographicSize += 45f;
        }

        public void HideBigMap()
        {
            showingBigMap = false;

            _minimapCamera.targetTexture = _miniRenderTexture;
            CanvasImage.material.SetTexture(1, _miniRenderTexture);

            CanvasImage.transform.SetParent(_hudTransform, false);
            CanvasImage.rectTransform.localPosition = P1_cornerPos;
            CanvasImage.rectTransform.sizeDelta = smallMapSize;

            _minimapCamera.orthographicSize = _orthoSize;
        }

        public void Update()
        {
            if (!_ownerCharacter)
            {
                Destroy(this.gameObject);
                return;
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                //if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.M))
                //{
                //    if (showingBigMap)
                //    {
                //        HideBigMap();
                //    }
                //    else
                //    {
                //        ShowBigMap();
                //    }
                //}

                if (Input.GetKey(KeyCode.DownArrow))
                {
                    _orthoSize += 0.5f;
                    _minimapCamera.orthographicSize = _orthoSize;

                    if (showingBigMap)
                    {
                        _minimapCamera.orthographicSize += 45f;
                    }
                }
                else if (Input.GetKey(KeyCode.UpArrow))
                {
                    _orthoSize -= 0.5f;
                    _minimapCamera.orthographicSize = _orthoSize;

                    if (showingBigMap)
                    {
                        _minimapCamera.orthographicSize += 45f;
                    }
                }
            }

            if (!inOutdoorRegion)
            {
                _minimapCamera.transform.position = _ownerCharacter.transform.position + (Vector3.up * 3f);
            }
            else
            {
                _minimapCamera.transform.position = _ownerCharacter.transform.position + (Vector3.up * 250f);
            }

            if (CustomRotations.ContainsKey(SceneManagerHelper.ActiveSceneName))
            {
                _minimapCamera.transform.eulerAngles = CustomRotations[SceneManagerHelper.ActiveSceneName];
            }
            else
            {
                _minimapCamera.transform.eulerAngles = new Vector3(90f, 0f, 0f);
            }
        }
    }
}
