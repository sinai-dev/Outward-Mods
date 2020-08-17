using System;

namespace TinyHelper
{
	// Token: 0x02000007 RID: 7
	public class TinyUIDManager
	{
		// Token: 0x0600000F RID: 15 RVA: 0x000024A4 File Offset: 0x000006A4
		public static UID MakeUID(string name, string modGUID, string category)
		{
			return new UID(string.Concat(new string[]
			{
				modGUID,
				".",
				category,
				".",
				name
			}).Replace(" ", "").ToLower());
		}
	}
}
