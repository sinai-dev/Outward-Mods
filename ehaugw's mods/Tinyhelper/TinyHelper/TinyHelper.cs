using System;
using System.IO;
using BepInEx;
using UnityEngine;

namespace TinyHelper
{
	// Token: 0x02000008 RID: 8
	[BepInPlugin("com.ehaugw.tinyhelper", "Tiny Helper", "1.0.0")]
	public class TinyHelper : BaseUnityPlugin
	{
		// Token: 0x06000011 RID: 17 RVA: 0x000024FF File Offset: 0x000006FF
		internal void Awake()
		{
			Debug.Log("Using Tiny Helper version 1.0.0.");
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002510 File Offset: 0x00000710
		public static void TinyHelperPrint(string str)
		{
			Debug.Log("TinyHelper #" + TinyHelper.tinyHelperPrintedMessages++.ToString() + ": " + str);
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000013 RID: 19 RVA: 0x0000254C File Offset: 0x0000074C
		public static string PLUGIN_ROOT_PATH
		{
			get
			{
				return Directory.GetCurrentDirectory() + "\\BepInEx\\plugins\\";
			}
		}

		// Token: 0x04000005 RID: 5
		public const string GUID = "com.ehaugw.tinyhelper";

		// Token: 0x04000006 RID: 6
		public const string VERSION = "1.0.0";

		// Token: 0x04000007 RID: 7
		public const string NAME = "Tiny Helper";

		// Token: 0x04000008 RID: 8
		public const string PLUGINS_PATH = "\\BepInEx\\plugins\\DescriptionReplacer\\SideLoader\\descriptions.txt";

		// Token: 0x04000009 RID: 9
		private static int tinyHelperPrintedMessages = 0;
	}
}
