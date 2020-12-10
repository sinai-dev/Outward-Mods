using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;

namespace CombatAndDodgeOverhaul
{
    public static class At // Access Tools
    {
        public static BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;

        //reflection call
        public static object Call(object obj, string method, params object[] args)
        {
            var methodInfo = obj.GetType().GetMethod(method, flags);
            if (methodInfo != null)
            {
                return methodInfo.Invoke(obj, args);
            }
            return null;
        }

        // set value
        public static void SetValue<T>(T value, Type type, object obj, string field)
        {
            FieldInfo fieldInfo = type.GetField(field, flags);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, value);
            }
        }

        // get value
        public static object GetValue(Type type, object obj, string value)
        {
            FieldInfo fieldInfo = type.GetField(value, flags);
            if (fieldInfo != null)
            {
                return fieldInfo.GetValue(obj);
            }
            else
            {
                return null;
            }
        }

        // inherit base values
        public static void InheritBaseValues(object _derived, object _base)
        {
            foreach (FieldInfo fi in _base.GetType().GetFields(flags))
            {
                try { _derived.GetType().GetField(fi.Name).SetValue(_derived, fi.GetValue(_base)); } catch { }
            }

            return;
        }
    }
}
