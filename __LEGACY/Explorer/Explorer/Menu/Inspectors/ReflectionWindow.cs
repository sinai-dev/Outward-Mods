using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Explorer
{
    public class ReflectionWindow : WindowManager.UIWindow
    {
        public override string Name { get => "Object Reflection"; set => Name = value; }

        public object m_object;

        private List<FieldInfoHolder> m_FieldInfos;
        private List<PropertyInfoHolder> m_PropertyInfos;

        private bool m_autoUpdate = false;
        private string m_search = "";
        public MemberFilter m_filter = MemberFilter.Both;

        public enum MemberFilter
        {
            Both,
            Property,
            Field
        }

        public override void Init()
        {
            m_object = Target;

            m_FieldInfos = new List<FieldInfoHolder>();
            m_PropertyInfos = new List<PropertyInfoHolder>();

            GetFields(m_object.GetType());
            GetProperties(m_object.GetType());

            UpdateValues();
        }

        internal void Update()
        {
            if (m_autoUpdate)
            {
                UpdateValues();
            }
        }

        private void UpdateValues()
        {
            if (m_filter == MemberFilter.Both || m_filter == MemberFilter.Field)
            {
                foreach (var holder in this.m_FieldInfos)
                {
                    if (m_search == "" || holder.fieldInfo.Name.ToLower().Contains(m_search.ToLower()))
                    {
                        holder.UpdateValue(m_object);
                    }
                }
            }

            if (m_filter == MemberFilter.Both || m_filter == MemberFilter.Property)
            {
                foreach (var holder in this.m_PropertyInfos)
                {
                    if (m_search == "" || holder.propInfo.Name.ToLower().Contains(m_search.ToLower()))
                    {
                        holder.UpdateValue(m_object);
                    }
                }
            }
        }

        public override void WindowFunction(int windowID)
        {
            try
            {
                Header();

                GUILayout.BeginArea(new Rect(5, 25, m_rect.width - 10, m_rect.height - 35), GUI.skin.box);

                GUILayout.BeginHorizontal();
                GUILayout.Label("<b>Type:</b> <color=cyan>" + m_object.GetType() + "</color>");

                bool unityObj = m_object is UnityEngine.Object;

                if (unityObj)
                {
                    GUILayout.Label("Name: " + (m_object as UnityEngine.Object).name);
                }
                GUILayout.EndHorizontal();

                if (unityObj)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label("<b>Tools:</b>", GUILayout.Width(80));

                    UIStyles.InstantiateButton((UnityEngine.Object)m_object);

                    if (m_object is Component comp && comp.gameObject is GameObject obj)
                    {
                        GUI.skin.label.alignment = TextAnchor.MiddleRight;
                        GUILayout.Label("GameObject:");
                        if (GUILayout.Button("<color=#00FF00>" + obj.name + "</color>", GUILayout.MaxWidth(m_rect.width - 350)))
                        {
                            WindowManager.InspectObject(obj, out bool _);
                        }
                        GUI.skin.label.alignment = TextAnchor.UpperLeft;
                    }

                    GUILayout.EndHorizontal();
                }

                UIStyles.HorizontalLine(Color.grey);

                GUILayout.BeginHorizontal();
                GUILayout.Label("<b>Search:</b>", GUILayout.Width(75));
                m_search = GUILayout.TextField(m_search);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("<b>Filter:</b>", GUILayout.Width(75));
                FilterToggle(MemberFilter.Both, "Both");
                FilterToggle(MemberFilter.Property, "Properties");
                FilterToggle(MemberFilter.Field, "Fields");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("<b>Values:</b>", GUILayout.Width(75));
                if (GUILayout.Button("Update", GUILayout.Width(100)))
                {
                    UpdateValues();
                }
                GUI.color = m_autoUpdate ? Color.green : Color.red;
                m_autoUpdate = GUILayout.Toggle(m_autoUpdate, "Auto-update?", GUILayout.Width(100));
                GUI.color = Color.white;
                GUILayout.EndHorizontal();

                GUILayout.Space(10);

                scroll = GUILayout.BeginScrollView(scroll);

                GUILayout.Space(10);

                if (m_filter == MemberFilter.Both || m_filter == MemberFilter.Field)
                {
                    UIStyles.HorizontalLine(Color.grey);

                    GUILayout.Label("<size=18><b><color=gold>Fields</color></b></size>");

                    foreach (var holder in this.m_FieldInfos)
                    {
                        if (m_search != "" && !holder.fieldInfo.Name.ToLower().Contains(m_search.ToLower()))
                        {
                            continue;
                        }

                        GUILayout.BeginHorizontal(GUILayout.Height(25));
                        holder.Draw(this);
                        GUILayout.EndHorizontal();
                    }
                }

                if (m_filter == MemberFilter.Both || m_filter == MemberFilter.Property)
                {
                    UIStyles.HorizontalLine(Color.grey);

                    GUILayout.Label("<size=18><b><color=gold>Properties</color></b></size>");

                    foreach (var holder in this.m_PropertyInfos)
                    {
                        if (m_search != "" && !holder.propInfo.Name.ToLower().Contains(m_search.ToLower()))
                        {
                            continue;
                        }

                        GUILayout.BeginHorizontal(GUILayout.Height(25));
                        holder.Draw(this);
                        GUILayout.EndHorizontal();
                    }
                }

                GUILayout.EndScrollView();

                m_rect = WindowManager.ResizeWindow(m_rect, windowID);

                GUILayout.EndArea();
            }
            catch (Exception e)
            {
                Debug.LogWarning("Exception on window draw. Message: " + e.Message);
                DestroyWindow();
                return;
            }
        }

        private void FilterToggle(MemberFilter mode, string label)
        {
            if (m_filter == mode)
            {
                GUI.color = Color.green;
            }
            else
            {
                GUI.color = Color.white;
            }
            if (GUILayout.Button(label, GUILayout.Width(100)))
            {
                m_filter = mode;
            }
            GUI.color = Color.white;
        }

        public static bool IsList(Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }


        private void GetProperties(Type type, List<string> names = null)
        {
            if (names == null)
            {
                names = new List<string>();
            }

            foreach (var pi in type.GetProperties(At.flags))
            {
                if (names.Contains(pi.Name))
                {
                    continue;
                }
                names.Add(pi.Name);

                var piHolder = new PropertyInfoHolder(type, pi);
                m_PropertyInfos.Add(piHolder);
            }
            if (type.BaseType != null)
            {
                GetProperties(type.BaseType, names);
            }
        }

        private void GetFields(Type type, List<string> names = null)
        {
            if (names == null)
            {
                names = new List<string>();
            }

            foreach (var fi in type.GetFields(At.flags))
            {
                if (names.Contains(fi.Name))
                {
                    continue;
                }
                names.Add(fi.Name);

                var fiHolder = new FieldInfoHolder(type, fi);
                m_FieldInfos.Add(fiHolder);
            }
            if (type.BaseType != null)
            {
                GetFields(type.BaseType, names);
            }
        }
        

        /* *********************
         *   PROPERTYINFO HOLDER
        */

        public class PropertyInfoHolder
        {
            public Type classType;
            public PropertyInfo propInfo;
            public object m_value;

            public PropertyInfoHolder(Type _type, PropertyInfo _propInfo)
            {
                classType = _type;
                propInfo = _propInfo;
            }

            public void Draw(ReflectionWindow window)
            {
                if (propInfo.CanWrite)
                {
                    UIStyles.DrawMember(ref m_value, propInfo.PropertyType.Name, propInfo.Name, window.m_rect, window.m_object, SetValue);
                }
                else
                {
                    UIStyles.DrawMember(ref m_value, propInfo.PropertyType.Name, propInfo.Name, window.m_rect, window.m_object);
                }
            }

            public void UpdateValue(object obj)
            {
                try
                {
                    m_value = this.propInfo.GetValue(obj, null);
                }
                catch 
                {
                    m_value = null;
                }
            }

            public void SetValue(object obj)
            {
                try
                {
                    if (propInfo.PropertyType.IsEnum)
                    {
                        if (Enum.Parse(propInfo.PropertyType, m_value.ToString()) is object enumValue && enumValue != null)
                        {
                            m_value = enumValue;
                        }
                    }
                    else if (propInfo.PropertyType.IsPrimitive)
                    {
                        if (propInfo.PropertyType == typeof(float))
                        {
                            if (float.TryParse(m_value.ToString(), out float f))
                            {
                                m_value = f;
                            }
                            else
                            {
                                Debug.LogWarning("Cannot parse " + m_value.ToString() + " to a float!");
                            }
                        }
                        else if (propInfo.PropertyType == typeof(double))
                        {
                            if (double.TryParse(m_value.ToString(), out double d))
                            {
                                m_value = d;
                            }
                            else
                            {
                                Debug.LogWarning("Cannot parse " + m_value.ToString() + " to a double!");
                            }
                        }
                        else if (propInfo.PropertyType != typeof(bool))
                        {
                            if (int.TryParse(m_value.ToString(), out int i))
                            {
                                m_value = i;
                            }
                            else
                            {
                                Debug.LogWarning("Cannot parse " + m_value.ToString() + " to an integer! type: " + propInfo.PropertyType);
                            }
                        }
                    }

                    propInfo.SetValue(propInfo.GetAccessors()[0].IsStatic ? null : obj, m_value, null);
                }
                catch
                {
                    Debug.Log("Exception trying to set property " + this.propInfo.Name);
                }
            }
        }


        /* *********************
         *   FIELDINFO HOLDER
        */

        public class FieldInfoHolder
        {
            public Type classType;
            public FieldInfo fieldInfo;
            public object m_value;

            public FieldInfoHolder(Type _type, FieldInfo _fieldInfo)
            {
                classType = _type;
                fieldInfo = _fieldInfo;
            }

            public void UpdateValue(object obj)
            {
                m_value = fieldInfo.GetValue(fieldInfo.IsStatic ? null : obj);
            }

            public void Draw(ReflectionWindow window)
            {
                bool canSet = !(fieldInfo.IsLiteral && !fieldInfo.IsInitOnly);

                if (canSet)
                {
                    UIStyles.DrawMember(ref m_value, fieldInfo.FieldType.Name, fieldInfo.Name, window.m_rect, window.m_object, SetValue);
                }
                else
                {
                    UIStyles.DrawMember(ref m_value, fieldInfo.FieldType.Name, fieldInfo.Name, window.m_rect, window.m_object);
                }
            }

            public void SetValue(object obj)
            {
                if (fieldInfo.FieldType.IsEnum)
                {
                    if (Enum.Parse(fieldInfo.FieldType, m_value.ToString()) is object enumValue && enumValue != null)
                    {
                        m_value = enumValue;
                    }
                }
                else if (fieldInfo.FieldType.IsPrimitive)
                {
                    if (fieldInfo.FieldType == typeof(float))
                    {
                        if (float.TryParse(m_value.ToString(), out float f))
                        {
                            m_value = f;
                        }
                        else
                        {
                            Debug.LogWarning("Cannot parse " + m_value.ToString() + " to a float!");
                        }
                    }
                    else if (fieldInfo.FieldType == typeof(double))
                    {
                        if (double.TryParse(m_value.ToString(), out double d))
                        {
                            m_value = d;
                        }
                        else
                        {
                            Debug.LogWarning("Cannot parse " + m_value.ToString() + " to a double!");
                        }
                    }
                    else if (fieldInfo.FieldType != typeof(bool))
                    {
                        if (int.TryParse(m_value.ToString(), out int i))
                        {
                            m_value = i;
                        }
                        else
                        {
                            Debug.LogWarning("Cannot parse " + m_value.ToString() + " to an integer! type: " + fieldInfo.FieldType);
                        }
                    }
                }

                fieldInfo.SetValue(fieldInfo.IsStatic ? null : obj, m_value);
            }
        }
    }
}
