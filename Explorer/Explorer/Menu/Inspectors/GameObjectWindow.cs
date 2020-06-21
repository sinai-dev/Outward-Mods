using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Explorer
{
    public class GameObjectWindow : WindowManager.UIWindow
    {
        public override string Name { get => "GameObject Inspector"; set => Name = value; }

        public GameObject m_object;

        // gui element holders
        private string m_name;
        private string m_scene;

        private Vector2 m_transformScroll = Vector2.zero;
        private Transform[] m_children;

        private Vector2 m_compScroll = Vector2.zero;
        private Component[] m_components;

        private float m_translateAmount = 0.3f;
        private float m_rotateAmount = 50f;
        private float m_scaleAmount = 0.1f;

        List<Component> m_cachedDestroyList = new List<Component>();
        private string m_addComponentInput = "";

        private string m_setParentInput = "";

        public override void Init()
        {
            if (!(m_object = Target as GameObject))
            {
                Debug.LogError("Target is not a GameObject!");
                DestroyWindow();
                return;
            }

            m_name = m_object.name;
            m_scene = m_object.scene == null ? "null" : m_object.scene.name;

            m_components = m_object.GetComponents(typeof(Component));

            var list = new List<Transform>();
            for (int i = 0; i < m_object.transform.childCount; i++)
            {
                list.Add(m_object.transform.GetChild(i));
            }
            m_children = list.ToArray();
        }

        internal void Update()
        {
            if (m_object == null)
            {
                DestroyWindow();
            }
        }

        private void InspectGameObject(GameObject obj)
        {
            var window = WindowManager.InspectObject(obj, out bool created);
            
            if (created)
            {
                window.m_rect = new Rect(this.m_rect.x, this.m_rect.y, this.m_rect.width, this.m_rect.height);
                DestroyWindow();
            }
        }

        private void ReflectObject(object obj)
        {
            var window = WindowManager.InspectObject(obj, out bool created);

            if (created)
            {
                if (this.m_rect.x <= (Screen.width - this.m_rect.width - 100))
                {
                    window.m_rect = new Rect(
                        this.m_rect.x + this.m_rect.width + 20,
                        this.m_rect.y,
                        550,
                        700);
                }
                else
                {
                    window.m_rect = new Rect(this.m_rect.x + 50, this.m_rect.y + 50, 550, 700);
                }
            }
        }

        public override void WindowFunction(int windowID)
        {
            Header();

            GUILayout.BeginArea(new Rect(5, 25, m_rect.width - 10, m_rect.height - 35), GUI.skin.box);

            scroll = GUILayout.BeginScrollView(scroll);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Scene: <color=cyan>" + (m_scene == "" ? "n/a" : m_scene) + "</color>");
            if (m_scene == SceneManagerHelper.ActiveSceneName)
            {
                if (GUILayout.Button("<color=#00FF00>< View in Scene Explorer</color>", GUILayout.Width(230)))
                {
                    ScenePage.Instance.SetTransformTarget(m_object);
                    MainMenu.SetCurrentPage(0);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Path:", GUILayout.Width(50));
            string pathlabel = m_object.transform.GetGameObjectPath();
            if (m_object.transform.parent != null)
            {
                if (GUILayout.Button("<-", GUILayout.Width(35)))
                {
                    InspectGameObject(m_object.transform.parent.gameObject);
                }
            }
            GUILayout.TextArea(pathlabel);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Name:", GUILayout.Width(50));
            GUILayout.TextArea(m_name);
            GUILayout.EndHorizontal();

            // --- Horizontal Columns section ---
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(m_rect.width / 2 - 17));
            TransformList();
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(m_rect.width / 2 - 17));
            ComponentList();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal(); // end horiz columns

            GameObjectControls();

            GUILayout.EndScrollView();

            m_rect = WindowManager.ResizeWindow(m_rect, windowID);

            GUILayout.EndArea();
        }

        private void TransformList()
        {
            GUILayout.BeginVertical(GUI.skin.box, new GUILayoutOption[] { GUILayout.MaxHeight(450), GUILayout.MinHeight(100), GUILayout.ExpandHeight(true) });
            m_transformScroll = GUILayout.BeginScrollView(m_transformScroll);

            GUILayout.Label("<b>Children:</b>");
            if (m_children != null && m_children.Length > 0)
            {
                foreach (var obj in m_children.Where(x => x.childCount > 0))
                {
                    if (obj == null)
                    {
                        GUILayout.Label("null");
                        continue;
                    }
                    UIStyles.GameobjButton(obj.gameObject, InspectGameObject, false, this.m_rect.width / 2 - 60);
                }
                foreach (var obj in m_children.Where(x => x.childCount == 0))
                {
                    if (obj == null)
                    {
                        GUILayout.Label("null");
                        continue;
                    }
                    UIStyles.GameobjButton(obj.gameObject, InspectGameObject, false, this.m_rect.width / 2 - 60);
                }
            }
            else
            {
                GUILayout.Label("<i>None</i>");
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }


        private void ComponentList()
        {
            GUILayout.BeginVertical(GUI.skin.box, new GUILayoutOption[] { GUILayout.MaxHeight(450), GUILayout.MinHeight(100), GUILayout.ExpandHeight(true) });
            m_compScroll = GUILayout.BeginScrollView(m_compScroll);
            GUILayout.Label("<b><size=15>Components</size></b>");

            GUI.skin.button.alignment = TextAnchor.MiddleLeft;
            if (m_cachedDestroyList.Count > 0)
            {
                m_cachedDestroyList.Clear();
            }
            foreach (var component in m_components)
            {
                if (component.GetType() == typeof(Transform))
                {
                    continue;
                }

                GUILayout.BeginHorizontal();
                if (component is Behaviour)
                {
                    BehaviourEnabledBtn(component as Behaviour);
                }
                if (GUILayout.Button("<color=cyan>" + component.GetType().ToString() + "</color>", GUILayout.Width(m_rect.width / 2 - 90)))
                {
                    ReflectObject(component);
                }
                if (GUILayout.Button("<color=red>-</color>", GUILayout.Width(20)))
                {
                    m_cachedDestroyList.Add(component);
                }
                GUILayout.EndHorizontal();
            }
            GUI.skin.button.alignment = TextAnchor.MiddleCenter;
            if (m_cachedDestroyList.Count > 0)
            {
                var list = m_components.ToList();
                for (int i = m_cachedDestroyList.Count - 1; i >= 0; i--)
                {
                    var comp = m_cachedDestroyList[i];
                    list.Remove(comp);
                    Destroy(comp);
                }
                m_components = list.ToArray();
            }

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            m_addComponentInput = GUILayout.TextField(m_addComponentInput, GUILayout.Width(m_rect.width / 2 - 150));
            if (GUILayout.Button("Add Component", GUILayout.Width(120)))
            {
                if (Explorer.GetType(m_addComponentInput) is Type type && typeof(Component).IsAssignableFrom(type))
                {
                    var comp = m_object.AddComponent(type);
                    var list = m_components.ToList();
                    list.Add(comp);
                    m_components = list.ToArray();
                }
                else
                {
                    Debug.LogWarning($"Could not get type '{m_addComponentInput}'. If it's not a typo, try the fully qualified name.");
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void BehaviourEnabledBtn(Behaviour obj)
        {
            var _col = GUI.color;
            bool _enabled = obj.enabled;
            if (_enabled)
            {
                GUI.color = Color.green;
            }
            else
            {
                GUI.color = Global.LIGHT_RED;
            }

            // ------ toggle active button ------

            _enabled = GUILayout.Toggle(_enabled, "", GUILayout.Width(18));
            if (obj.enabled != _enabled)
            {
                obj.enabled = _enabled;
            }
            GUI.color = _col;
        }

        private void GameObjectControls()
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(530));
            GUILayout.Label("<b><size=15>GameObject Controls</size></b>");

            GUILayout.BeginHorizontal();
            bool m_active = m_object.activeSelf;
            m_active = GUILayout.Toggle(m_active, (m_active ? "<color=lime>Enabled " : "<color=red>Disabled") + "</color>", GUILayout.Width(80));
            if (m_object.activeSelf != m_active) { m_object.SetActive(m_active); }

            UIStyles.InstantiateButton(m_object, 100);

            if (CharacterManager.Instance.GetFirstLocalCharacter() is Character c)
            {
                CharacterCheats(c);
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Remove from parent", GUILayout.Width(160)))
            {
                m_object.transform.parent = null;
            }
            m_setParentInput = GUILayout.TextField(m_setParentInput, GUILayout.Width(m_rect.width - 280));
            if (GUILayout.Button("Set Parent", GUILayout.Width(80)))
            {
                if (GameObject.Find(m_setParentInput) is GameObject newparent)
                {
                    m_object.transform.parent = newparent.transform;
                }
                else
                {
                    Debug.LogWarning($"Could not find gameobject '{m_setParentInput}'");
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(GUI.skin.box);

            var t = m_object.transform;
            TranslateControl(t, TranslateType.Position, ref m_translateAmount,  false);
            TranslateControl(t, TranslateType.Rotation, ref m_rotateAmount,     true);
            TranslateControl(t, TranslateType.Scale,    ref m_scaleAmount,      false);

            GUILayout.EndVertical();

            if (GUILayout.Button("<color=red><b>Destroy</b></color>"))
            {
                Destroy(m_object);
                DestroyWindow();
                return;
            }

            GUILayout.EndVertical();
        }

        public enum TranslateType
        {
            Position,
            Rotation,
            Scale
        }

        private void TranslateControl(Transform transform, TranslateType mode, ref float amount, bool multByTime)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("<color=cyan><b>" + mode + "</b></color>:", GUILayout.Width(65));

            Vector3 vector = Vector3.zero;
            switch (mode)
            {
                case TranslateType.Position: vector = transform.localPosition; break;
                case TranslateType.Rotation: vector = transform.localRotation.eulerAngles; break;
                case TranslateType.Scale:    vector = transform.localScale; break;
            }
            GUILayout.Label(vector.ToString(), GUILayout.Width(250));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.skin.label.alignment = TextAnchor.MiddleRight;

            GUILayout.Label("<color=cyan>X:</color>", GUILayout.Width(20));
            PlusMinusFloat(ref vector.x, amount, multByTime);

            GUILayout.Label("<color=cyan>Y:</color>", GUILayout.Width(20));
            PlusMinusFloat(ref vector.y, amount, multByTime);

            GUILayout.Label("<color=cyan>Z:</color>", GUILayout.Width(20));
            PlusMinusFloat(ref vector.z, amount, multByTime);

            switch (mode)
            {
                case TranslateType.Position: transform.localPosition = vector; break;
                case TranslateType.Rotation: transform.localRotation = Quaternion.Euler(vector); break;
                case TranslateType.Scale:    transform.localScale = vector; break;
            }

            GUILayout.Label("+/-:", GUILayout.Width(30));
            var input = amount.ToString("F3");
            input = GUILayout.TextField(input, GUILayout.Width(40));
            if (float.TryParse(input, out float f))
            {
                amount = f;
            }

            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUILayout.EndHorizontal();
        }

        private void PlusMinusFloat(ref float f, float amount, bool multByTime)
        {
            string s = f.ToString("F3");
            s = GUILayout.TextField(s, GUILayout.Width(60));
            if (float.TryParse(s, out float f2))
            {
                f = f2;
            }
            if (GUILayout.RepeatButton("-", GUILayout.Width(20)))
            {
                f -= multByTime ? amount * Time.deltaTime : amount;
            }
            if (GUILayout.RepeatButton("+", GUILayout.Width(20)))
            {
                f += multByTime ? amount * Time.deltaTime : amount;
            }
        }

        private void CharacterCheats(Character player)
        {
            if (GUILayout.Button("<color=lime>Teleport SELF to</color>"))
            {
                player.Teleport(m_object.transform.position, Quaternion.identity);
            }
            if (GUILayout.Button("<color=red>Teleport TO self</color>"))
            {
                var pos = player.transform.position + new Vector3(0f, 1f, 0f);
                if (m_object.GetComponent<Character>() is Character other)
                {
                    other.Teleport(pos, Quaternion.identity);
                }
                else if (m_object.GetComponent<Item>() is Item item)
                {
                    item.ChangeParent(item.transform.parent, pos);
                }
                else
                {
                    m_object.transform.position = pos;
                }
            }
        }
    }
}
