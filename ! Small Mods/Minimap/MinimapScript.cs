using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Minimap
{
    public class MinimapScript : MonoBehaviour
    {
        public static MinimapScript P1Instance;
        public static MinimapScript P2Instance;

        public RawImage CanvasImage => _image;
        public const float P1_ADJUST_SPLIT = 273.2f;

        private Character _ownerCharacter;

        private float _orthoSize = 14f;
        private bool showingBigMap = false;

        private Camera _minimapCamera;
        private RawImage _image;

        private RenderTexture _miniRenderTexture;
        private RenderTexture _bigRenderTexture;

        private Transform _hudTransform;
        private Transform _mapTransform;

        private static readonly Vector3 _P1_topLeftPosition = new Vector3(802f, 434f, 0f);
        private static readonly Vector3 _P2_topLeftPosition = new Vector3(802f, 162.5f, 0f);
        private static readonly Vector3 _centerPosition = new Vector3(0, 0, 0);
        private static readonly Vector2 _smallSize = new Vector2(200f, 200f);
        private static readonly Vector2 _bigSize = new Vector2(800f, 800f);

        public void Init(Character character)
        {
            _ownerCharacter = character;

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
            _minimapCamera.transform.localEulerAngles = new Vector3(90f, 0f, 0f);

            // setup RenderTexture
            _miniRenderTexture = new RenderTexture(200, 200, 0);
            _minimapCamera.targetTexture = _miniRenderTexture;

            _bigRenderTexture = new RenderTexture(800, 800, 0);

            // setup UI element
            _image = new GameObject("MinimapImage", typeof(RawImage))
                               .GetComponent<RawImage>();

            if (_ownerCharacter.OwnerPlayerSys.PlayerID == 0)
            {
                _image.rectTransform.localPosition = _P1_topLeftPosition;
            }
            else
            {
                _image.rectTransform.localPosition = _P2_topLeftPosition;
            }

            _image.rectTransform.sizeDelta = _smallSize;

            _hudTransform = character.CharacterUI.transform.Find("Canvas/GameplayPanels/HUD");
            _mapTransform = MenuManager.Instance.transform.Find("GeneralMenus");

            _image.transform.SetParent(_hudTransform, false);

            var mat = Resources.FindObjectsOfTypeAll<Material>()
                               .FirstOrDefault(x => x.name == "mat_renderGame");

            mat.SetTexture(1, _miniRenderTexture);
            _image.material = mat;

            Debug.Log("Done");
        }

        public void ShowMap()
        {
            showingBigMap = true;

            _minimapCamera.targetTexture = _bigRenderTexture;
            _image.material.SetTexture(1, _bigRenderTexture);

            _image.transform.SetParent(_mapTransform, false);
            _image.rectTransform.localPosition = _centerPosition;
            _image.rectTransform.sizeDelta = _bigSize;

            _minimapCamera.orthographicSize += 45f;
        }

        public void HideMap()
        {
            showingBigMap = false;

            _minimapCamera.targetTexture = _miniRenderTexture;
            _image.material.SetTexture(1, _miniRenderTexture);

            _image.transform.SetParent(_hudTransform, false);
            _image.rectTransform.localPosition = _P1_topLeftPosition;
            _image.rectTransform.sizeDelta = _smallSize;

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

            _minimapCamera.transform.position = _ownerCharacter.transform.position + (Vector3.up * 5f);
            _minimapCamera.transform.eulerAngles = new Vector3(90f, 0f, 0f);
        }
    }
}
