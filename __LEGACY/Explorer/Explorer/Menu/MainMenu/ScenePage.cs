using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Explorer
{
    public class ScenePage : MainMenu.WindowPage
    {
        public static ScenePage Instance;

        public override string Name { get => "Scene Explorer"; set => base.Name = value; }

        // ----- Holders for GUI elements ----- //

        private string m_currentScene = "";

        // gameobject list
        private Transform m_currentTransform;
        private List<GameObject> m_objectList = new List<GameObject>();

        // search bar
        private bool m_searching = false;
        private string m_searchInput = "";
        private List<GameObject> m_searchResults = new List<GameObject>();

        // ------------ Init and Update ------------ //

        public override void Init()
        {
            Instance = this;

            SceneManager.activeSceneChanged += OnSceneChange;
        }

        private void OnSceneChange(Scene arg0, Scene arg1)
        {
            m_currentScene = SceneManagerHelper.ActiveSceneName;
            if (AreaManager.Instance.GetAreaFromSceneName(m_currentScene) is Area area)
            {
                m_currentScene += " (" + area.DefaultName + ")";
            }

            m_currentTransform = null;
            CancelSearch();
            
        }

        public override void Update()
        {
            if (!m_searching)
            {
                m_objectList = new List<GameObject>();
                if (m_currentTransform != null)
                {
                    foreach (Transform child in m_currentTransform)
                    {
                        if (child.childCount > 0)
                            m_objectList.Add(child.gameObject);
                    }
                    foreach (Transform child in m_currentTransform)
                    {
                        if (child.childCount == 0)
                            m_objectList.Add(child.gameObject);
                    }
                }
                else
                {
                    foreach (var player in Global.Lobby.PlayersInLobby)
                    {
                        m_objectList.Add(player.ControlledCharacter.gameObject);
                    }

                    var scene = SceneManager.GetActiveScene();
                    var rootObjects = scene.GetRootGameObjects();

                    // add objects with children first
                    foreach (var obj in rootObjects.Where(x => x.transform.childCount > 0))
                    {
                        m_objectList.Add(obj);
                    }
                    foreach (var obj in rootObjects.Where(x => x.transform.childCount == 0))
                    {
                        m_objectList.Add(obj);
                    }
                }
            }
        }

        // --------- GUI Draw Functions --------- //        

        public override void DrawWindow()
        {            
            try
            {
                // Current Scene label
                GUILayout.Label("Current Scene: <color=cyan>" + m_currentScene + "</color>");

                // ----- GameObject Search -----
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label("<b>Search Scene:</b>", GUILayout.Width(100));
                m_searchInput = GUILayout.TextField(m_searchInput);
                if (GUILayout.Button("Search", GUILayout.Width(80)))
                {
                    Search();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(15);

                // ************** GameObject list ***************

                // ----- main explorer ------
                if (!m_searching)
                {
                    if (m_currentTransform != null)
                    {
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("<-", GUILayout.Width(35)))
                        {
                            TraverseUp();
                        }
                        else
                        {
                            GUILayout.Label(m_currentTransform.GetGameObjectPath());
                        }
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.Label("Scene Root GameObjects:");
                    }

                    if (m_objectList.Count > 0)
                    {
                        foreach (var obj in m_objectList)
                        {
                            UIStyles.GameobjButton(obj, SetTransformTarget, true, MainMenu.MainRect.width - 170);
                        }
                    }
                    else
                    {
                        // if m_currentTransform != null ...
                    }
                }
                else // ------ Scene Search results ------
                {
                    if (GUILayout.Button("<-", GUILayout.Width(35)))
                    {
                        CancelSearch();
                    }

                    GUILayout.Label("Search Results:");

                    if (m_searchResults.Count > 0)
                    {
                        foreach (var obj in m_searchResults)
                        {
                            UIStyles.GameobjButton(obj, SetTransformTarget, true, MainMenu.MainRect.width - 170);
                        }
                    }
                    else
                    {
                        GUILayout.Label("<color=red><i>No results found!</i></color>");
                    }
                }
            }
            catch
            {
                m_currentTransform = null;
            }
        }

        

        // -------- Actual Methods (not drawing GUI) ---------- //

        public void SetTransformTarget(GameObject obj)
        {
            m_currentTransform = obj.transform;
            CancelSearch();
        }

        public void TraverseUp()
        {
            if (m_currentTransform.parent != null)
            {
                m_currentTransform = m_currentTransform.parent;
            }
            else
            {
                m_currentTransform = null;
            }
        }

        public void Search()
        {
            m_searchResults = SearchSceneObjects(m_searchInput);
            m_searching = true;
        }

        public void CancelSearch()
        {
            m_searching = false;
        }

        public List<GameObject> SearchSceneObjects(string _search)
        {
            var matches = new List<GameObject>();

            foreach (var obj in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (obj.name.ToLower().Contains(_search.ToLower()) && obj.scene.name == SceneManagerHelper.ActiveSceneName)
                {
                    matches.Add(obj);   
                }
            }

            return matches;
        }
    }
}
