using System;
using System.Reflection;
using UnityEngine;

namespace PvP
{
    /// <summary> AccessTools (Reflection Helpers) by Sinai</summary>
    public static class At
    {
        /* 
         * These helper methods can be used to simplify Reflection.
         * Make sure you set the namespace above to your project's namespace.
        */

        /// <summary>
        /// The common four BindingFlags, which is all that is needed for most things in Outward.
        /// </summary>
        public static readonly BindingFlags FLAGS = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;

        /// <summary>
        /// Helper to set the value to a private or protected field.
        /// </summary>
        /// <typeparam name="T">The Type of value you are setting (only required if value is "null").</typeparam>
        /// <param name="value">The value you want to set.</param>
        /// <param name="type">The declaring class Type which contains this field.</param>
        /// <param name="obj">The instance, for non-static members. If the member is static, use "null".</param>
        /// <param name="field">The name of the field.</param>
        public static void SetValue<T>(T value, Type type, object obj, string field)
        {
            FieldInfo fieldInfo = type.GetField(field, FLAGS);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, value);
            }
        }

        /// <summary>
        /// Helper to get the value from a private or protected field.
        /// </summary>
        /// <param name="type">The declaring class Type which contains this field.</param>
        /// <param name="obj">The instance, for non-static members. If the member is static, use "null".</param>
        /// <param name="field">The name of the field you want to get.</param>
        /// <returns>The value of the field provided, if valid.</returns>
        public static object GetValue(Type type, object obj, string field)
        {
            FieldInfo fieldInfo = type.GetField(field, FLAGS);
            if (fieldInfo != null)
            {
                return fieldInfo.GetValue(obj);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// A helper to get all the fields from one class instance, and set them to another.
        /// </summary>
        /// <param name="copyTo">The object which you are setting values to.</param>
        /// <param name="copyFrom">The object which you are getting values from.</param>
        /// <param name="declaringType">Optional, manually define the declaring class type.</param>
        /// <param name="recursive">Whether to recursively dive into the BaseTypes and copy those fields too</param>
        public static void CopyFieldValues(object copyTo, object copyFrom, Type declaringType = null, bool recursive = false)
        {
            var type = declaringType ?? copyFrom.GetType();
            foreach (FieldInfo fi in type.GetFields(FLAGS))
            {
                try
                {
                    fi.SetValue(copyTo, fi.GetValue(copyFrom));
                }
                catch { }
            }


            if (recursive && type.BaseType is Type baseType)
            {
                // We don't want to copy Unity low-level types, such as MonoBehaviour or Component.
                // Copying these fields causes serious errors.
                if (baseType != typeof(MonoBehaviour) && baseType != typeof(Component))
                {
                    CopyFieldValues(copyTo, copyFrom, type.BaseType, true);
                }
            }

            return;
        }

        //// Legacy At.InheritBaseValues support.
        //[Obsolete("Use At.CopyFieldValues instead (renamed after SL v2.1.9)")]
        //public static void InheritBaseValues(object _derived, object _base)
        //{
        //    CopyFieldValues(_derived, _base);
        //}

        /// <summary>
        /// Helper to invoke a private or protected method.
        /// </summary>
        /// <param name="type">The declaring class Type which contains this method.</param>
        /// <param name="obj">The instance to invoke the method from. If method is static, use "null".</param>
        /// <param name="method">The name of the method you want to invoke.</param>
        /// <param name="argumentTypes">[Optional] For ambiguous methods, provide an array corresponding to the Type of each argument, otherwise use "null".</param>
        /// <param name="args">The actual arguments you want to provide for the method, if any.</param>
        /// <returns>The return value of the method (might be void).</returns>
        public static object Call(Type type, object obj, string method, Type[] argumentTypes, params object[] args)
        {
            object ret = null;
            try
            {
                MethodInfo methodInfo;
                if (argumentTypes == null)
                {
                    methodInfo = type.GetMethod(method, FLAGS);
                }
                else
                {
                    methodInfo = type.GetMethod(method, FLAGS, null, argumentTypes, null);
                }
                ret = methodInfo.Invoke(obj, args);
            }
            catch (AmbiguousMatchException)
            {
                Debug.Log("Ambiguous match exception on method " + method + "!");
            }
            catch (NullReferenceException)
            {
                Debug.Log("Null reference exception. Method " + method + " not found!");
            }
            return ret;
        }

        //// Legacy At.Call support.
        //[Obsolete("Use the other At.Call instead (it has 5 arguments).")]
        //public static object Call(object obj, string method, params object[] args)
        //{
        //    return Call(obj.GetType(), obj, method, null, args);
        //}

        /// <summary>
        /// Helper to set a private property, if possible to set.
        /// </summary>
        /// <typeparam name="T">The Type of value you are setting (only required if value is "null").</typeparam>
        /// <param name="value">The value you want to set.</param>
        /// <param name="type">The declaring class Type which contains this property.</param>
        /// <param name="obj">The instance, for non-static members. If the member is static, use "null".</param>>
        /// <param name="property">The name of the property you want to set.</param>
        public static void SetProp<T>(T value, Type type, object obj, string property)
        {
            PropertyInfo propInfo = type.GetProperty(property);
            if (propInfo != null && propInfo.CanWrite)
            {
                try
                {
                    propInfo.SetValue(obj, value, FLAGS, null, null, null);
                }
                catch (Exception e)
                {
                    Debug.Log("Exception setting property " + property + ".\r\nMessage: " + e.Message + "\r\nStack: " + e.StackTrace);
                }
            }
        }

        /// <summary>
        /// Helper to get the value from a private property.
        /// </summary>
        /// <param name="type">The declaring class Type which contains this property.</param>
        /// <param name="obj">The instance, for non-static members. If the member is static, use "null".</param>>
        /// <param name="property">The name of the property you want to get.</param>
        /// <returns>The value from the property.</returns>
        public static object GetProp(Type type, object obj, string property)
        {
            PropertyInfo propInfo = type.GetProperty(property);
            if (propInfo != null)
            {
                try
                {
                    return propInfo.GetValue(obj, FLAGS, null, null, null);
                }
                catch (Exception e)
                {
                    Debug.Log("Exception getting property " + property + ".\r\nMessage: " + e.Message + "\r\nStack: " + e.StackTrace);
                }
            }
            return null;
        }
    }
}
