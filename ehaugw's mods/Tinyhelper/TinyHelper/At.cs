using System;
using System.Reflection;

namespace TinyHelper
{
	// Token: 0x02000002 RID: 2
	public static class At
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public static object Call(object obj, string method, params object[] args)
		{
			MethodInfo method2 = obj.GetType().GetMethod(method, At.flags);
			bool flag = method2 != null;
			object result;
			if (flag)
			{
				result = method2.Invoke(obj, args);
			}
			else
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06000002 RID: 2 RVA: 0x0000208C File Offset: 0x0000028C
		public static void SetValue<T>(T value, Type type, object obj, string field)
		{
			FieldInfo field2 = type.GetField(field, At.flags);
			bool flag = field2 != null;
			if (flag)
			{
				field2.SetValue(obj, value);
			}
		}

		// Token: 0x06000003 RID: 3 RVA: 0x000020C4 File Offset: 0x000002C4
		public static object GetValue(Type type, object obj, string value)
		{
			FieldInfo field = type.GetField(value, At.flags);
			bool flag = field != null;
			object result;
			if (flag)
			{
				result = field.GetValue(obj);
			}
			else
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06000004 RID: 4 RVA: 0x000020FC File Offset: 0x000002FC
		public static void InheritBaseValues(object _derived, object _base)
		{
			foreach (FieldInfo fieldInfo in _base.GetType().GetFields(At.flags))
			{
				try
				{
					_derived.GetType().GetField(fieldInfo.Name).SetValue(_derived, fieldInfo.GetValue(_base));
				}
				catch
				{
				}
			}
		}

		// Token: 0x04000001 RID: 1
		public static BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
	}
}
