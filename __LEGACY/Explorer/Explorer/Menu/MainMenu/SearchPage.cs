using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Explorer
{
    public class SearchPage : MainMenu.WindowPage
    {
        public static SearchPage Instance;

        public override string Name { get => "Advanced Search"; set => base.Name = value; }

        private string m_searchInput = "";
        private string m_typeInput = "";
        private int m_limit = 100;

        public SceneFilter SceneMode = SceneFilter.Any;
        public TypeFilter TypeMode = TypeFilter.Object;

        public enum SceneFilter
        {
            Any,
            This,
            DontDestroy,
            None
        }

        public enum TypeFilter
        {
            Object,
            GameObject,
            Component,
            Custom
        }

        private List<object> m_searchResults = new List<object>();
        private Vector2 resultsScroll = Vector2.zero;

        public override void Init() 
        {
            Instance = this;

            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }

        private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            m_searchResults.Clear();
        }

        public override void Update() 
        {
        }

        public override void DrawWindow()
        {
            try
            {
                // helpers
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label("<b><color=orange>Helpers</color></b>", GUILayout.Width(70));
                if (GUILayout.Button("Find Static Instances", GUILayout.Width(180)))
                {
                    m_searchResults = GetInstanceClassScanner().ToList();
                }
                GUILayout.EndHorizontal();

                // search box
                SearchBox();

                // results
                GUILayout.BeginVertical(GUI.skin.box);

                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label("<b><color=orange>Results</color></b>");
                GUI.skin.label.alignment = TextAnchor.UpperLeft;

                resultsScroll = GUILayout.BeginScrollView(resultsScroll);

                var _temprect = new Rect(MainMenu.MainRect.x, MainMenu.MainRect.y, MainMenu.MainRect.width + 160, MainMenu.MainRect.height);

                if (m_searchResults.Count > 0)
                {
                    for (int i = 0; i < m_searchResults.Count; i++)
                    {
                        var obj = (object)m_searchResults[i];

                        UIStyles.DrawValue(ref obj, _temprect);
                    }
                }
                else
                {
                    GUILayout.Label("<color=red><i>No results found!</i></color>");
                }

                GUILayout.EndScrollView();

                GUILayout.EndVertical();
            }
            catch
            {
                m_searchResults.Clear();
            }
        }

        private void SearchBox()
        {
            GUILayout.BeginVertical(GUI.skin.box);

            // ----- GameObject Search -----
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label("<b><color=orange>Search</color></b>");
            GUI.skin.label.alignment = TextAnchor.UpperLeft;

            GUILayout.BeginHorizontal();

            GUILayout.Label("Name Contains:", GUILayout.Width(100));
            m_searchInput = GUILayout.TextField(m_searchInput, GUILayout.Width(200));

            GUI.skin.label.alignment = TextAnchor.MiddleRight;
            GUILayout.Label("Result limit:", GUILayout.Width(100));
            var resultinput = m_limit.ToString();
            resultinput = GUILayout.TextField(resultinput, GUILayout.Width(55));
            if (int.TryParse(resultinput, out int _i) && _i > 0)
            {
                m_limit = _i;
            }
            GUI.skin.label.alignment = TextAnchor.UpperLeft;

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Label("Class Filter:", GUILayout.Width(100));
            ClassFilterToggle(TypeFilter.Object, "Object");
            ClassFilterToggle(TypeFilter.GameObject, "GameObject");
            ClassFilterToggle(TypeFilter.Component, "Component");
            ClassFilterToggle(TypeFilter.Custom, "Custom");
            GUILayout.EndHorizontal();
            if (TypeMode == TypeFilter.Custom)
            {
                GUILayout.BeginHorizontal();
                GUI.skin.label.alignment = TextAnchor.MiddleRight;
                GUILayout.Label("Custom Class:", GUILayout.Width(250));
                GUI.skin.label.alignment = TextAnchor.UpperLeft;
                m_typeInput = GUILayout.TextField(m_typeInput, GUILayout.Width(250));
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Scene Filter:", GUILayout.Width(100));
            SceneFilterToggle(SceneFilter.Any, "Any", 60);
            SceneFilterToggle(SceneFilter.This, "This Scene", 100);
            SceneFilterToggle(SceneFilter.DontDestroy, "DontDestroyOnLoad", 140);
            SceneFilterToggle(SceneFilter.None, "No Scene", 80);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("<b><color=cyan>Search</color></b>"))
            {
                Search();
            }

            GUILayout.EndVertical();
        }

        private void ClassFilterToggle(TypeFilter mode, string label)
        {
            if (TypeMode == mode)
            {
                GUI.color = Color.green;
            }
            else
            {
                GUI.color = Color.white;
            }
            if (GUILayout.Button(label, GUILayout.Width(100)))
            {
                TypeMode = mode;
            }
            GUI.color = Color.white;
        }

        private void SceneFilterToggle(SceneFilter mode, string label, float width)
        {
            if (SceneMode == mode)
            {
                GUI.color = Color.green;
            }
            else
            {
                GUI.color = Color.white;
            }
            if (GUILayout.Button(label, GUILayout.Width(width)))
            {
                SceneMode = mode;
            }
            GUI.color = Color.white;
        }


        // -------------- ACTUAL METHODS (not Gui draw) ----------------- //

        // credit: ManlyMarco (RuntimeUnityEditor)
        public static IEnumerable<object> GetInstanceClassScanner()
        {
            var query = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.FullName.StartsWith("Mono"))
                .SelectMany(GetTypesSafe)
                .Where(t => t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters);

            foreach (var type in query)
            {
                object obj = null;
                try
                {
                    obj = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)?.GetValue(null, null);
                }
                catch
                {
                    try
                    {
                        obj = type.GetField("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)?.GetValue(null);
                    }
                    catch
                    {
                    }
                }
                if (obj != null && !obj.ToString().StartsWith("Mono"))
                {
                    yield return obj;
                }
            }
        }

        public static IEnumerable<Type> GetTypesSafe(Assembly asm)
        {
            try { return asm.GetTypes(); }
            catch (ReflectionTypeLoadException e) { return e.Types.Where(x => x != null); }
            catch { return Enumerable.Empty<Type>(); }
        }

        // ======= search functions =======

        private void Search()
        {
            m_searchResults = FindAllObjectsOfType(m_searchInput, m_typeInput);
        }

        private List<object> FindAllObjectsOfType(string _search, string _type)
        {
            Type type = null;

            if (TypeMode == TypeFilter.Custom)
            {
                if (Explorer.GetType(_type) is Type getType)
                {
                    type = getType;
                }
                else
                {
                    Debug.LogWarning("Could not find type: " + _type);
                    return new List<object>();
                }
            }
            else if (TypeMode == TypeFilter.Object)
            {
                type = typeof(Object);
            }
            else if (TypeMode == TypeFilter.GameObject)
            {
                type = typeof(GameObject);
            }
            else if (TypeMode == TypeFilter.Component)
            {
                type = typeof(Component);
            }

            if (!typeof(Object).IsAssignableFrom(type))
            {
                Debug.LogError("Your Class Type must inherit from UnityEngine.Object! Leave blank to default to UnityEngine.Object");
                return new List<object>();
            }

            var matches = new List<object>();
            int added = 0;

            foreach (var obj in Resources.FindObjectsOfTypeAll(type))
            {
                if (added == m_limit)
                {
                    break;
                }

                if (_search != "" && !obj.name.ToLower().Contains(_search.ToLower()))
                {
                    continue;
                }

                if (SceneMode != SceneFilter.Any)
                {
                    if (SceneMode == SceneFilter.None)
                    {
                        if (!NoSceneFilter(obj, obj.GetType()))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        GameObject go;

                        var objtype = obj.GetType();
                        if (objtype == typeof(GameObject))
                        {
                            go = obj as GameObject;
                        }
                        else if (typeof(Component).IsAssignableFrom(objtype))
                        {
                            go = (obj as Component).gameObject;
                        }
                        else { continue; }

                        if (!go) { continue; }

                        if (SceneMode == SceneFilter.This)
                        {
                            if (go.scene.name != SceneManagerHelper.ActiveSceneName || go.scene.name == "DontDestroyOnLoad")
                            {
                                continue;
                            }
                        }
                        else if (SceneMode == SceneFilter.DontDestroy)
                        {
                            if (go.scene.name != "DontDestroyOnLoad")
                            {
                                continue;
                            }
                        }
                    }
                }

                if (!matches.Contains(obj))
                {
                    matches.Add(obj);
                    added++;
                }
            }

            return matches;
        }

        public static bool ThisSceneFilter(object obj, Type type)
        {
            if (type == typeof(GameObject) || typeof(Component).IsAssignableFrom(type))
            {
                var go = obj as GameObject ?? (obj as Component).gameObject;
                
                if (go != null && go.scene.name == SceneManagerHelper.ActiveSceneName && go.scene.name != "DontDestroyOnLoad")
                {
                    return true;
                }
            }

            return false;
        }

        public static bool DontDestroyFilter(object obj, Type type)
        {
            if (type == typeof(GameObject) || typeof(Component).IsAssignableFrom(type))
            {
                var go = obj as GameObject ?? (obj as Component).gameObject;

                if (go != null && go.scene.name == "DontDestroyOnLoad")
                {
                    return true;
                }
            }

            return false;
        }

        public static bool NoSceneFilter(object obj, Type type)
        {
            if (type == typeof(GameObject))
            {
                var go = obj as GameObject;

                if (go.scene.name != SceneManagerHelper.ActiveSceneName && go.scene.name != "DontDestroyOnLoad")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (typeof(Component).IsAssignableFrom(type))
            {
                var go = (obj as Component).gameObject;

                if (go == null || (go.scene.name != SceneManagerHelper.ActiveSceneName && go.scene.name != "DontDestroyOnLoad"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
    }
}
