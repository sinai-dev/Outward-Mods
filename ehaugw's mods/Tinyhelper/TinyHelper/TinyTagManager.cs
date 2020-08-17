using System;
using System.Linq;

namespace TinyHelper
{
	// Token: 0x02000006 RID: 6
	public class TinyTagManager
	{
		// Token: 0x0600000D RID: 13 RVA: 0x00002410 File Offset: 0x00000610
		public static Tag GetOrMakeTag(string name)
		{
			Tag tag = TagSourceManager.Instance.DbTags.FirstOrDefault((Tag x) => x.TagName == name);
			bool flag = tag == Tag.None;
			Tag result;
			if (flag)
			{
				tag = new Tag(TagSourceManager.TagRoot, name);
				tag.SetTagType(Tag.TagTypes.Custom);
				TagSourceManager.Instance.DbTags.Add(tag);
				TagSourceManager.Instance.RefreshTags(true);
				result = tag;
			}
			else
			{
				result = tag;
			}
			return result;
		}
	}
}
