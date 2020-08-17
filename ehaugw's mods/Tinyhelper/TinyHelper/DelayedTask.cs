using System;
using System.Threading;
using System.Threading.Tasks;

namespace TinyHelper
{
	// Token: 0x02000009 RID: 9
	public class DelayedTask
	{
		// Token: 0x06000016 RID: 22 RVA: 0x00002580 File Offset: 0x00000780
		public static Task GetTask(int milliseconds)
		{
			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
			new Timer(delegate(object _)
			{
				tcs.SetResult(null);
			}).Change(milliseconds, -1);
			return tcs.Task;
		}
	}
}
