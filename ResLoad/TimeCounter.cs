using System;

namespace ResLoad
{
	public class TimeCounter
	{
		private DateTime BeginTime;
		private string frontStr = string.Empty;

		public TimeCounter(string frontStr = "")
		{
			this.frontStr = frontStr;
			this.BeginTime = DateTime.Now;
			ConsoleMgr.LogRed(frontStr + "起始" + " : " + System.DateTime.Now.ToString());
		}

		public void CostTime()
		{
			ConsoleMgr.LogRed(frontStr + "结束" + " : " + System.DateTime.Now.ToString());
			TimeSpan span = DateTime.Now - BeginTime;
			string str = frontStr + "时间" + " : " + span.TotalSeconds + "秒";
			ConsoleMgr.LogRed(str);
		}
	}
}

