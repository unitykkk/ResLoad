using System;

namespace ResLoad
{
	public class LoadSpeedTester
	{
		private DateTime BeginTime;
		private string frontStr = string.Empty;

		public LoadSpeedTester(string frontStr = "")
		{
			this.frontStr = frontStr;
			this.BeginTime = DateTime.Now;
		}

		public void CostTime()
		{
			ConsoleMgr.LogRed(frontStr + "结束" + " : " + System.DateTime.Now.ToString());
			TimeSpan span = DateTime.Now - BeginTime;
			string str = frontStr + "时间" + " : " + span.TotalSeconds + "秒";
			ConsoleMgr.LogRed(str);
		}

		private uint totalBytes = 0;
		public void AddLoaded(uint loadedBytes)
		{
			totalBytes += loadedBytes;
		}

		public void CountSpeed()
		{
			TimeSpan span = DateTime.Now - BeginTime;
			double costSeconds = span.TotalSeconds;
			double totalMB = (double)totalBytes / (1024 * 1024);

			double speed = totalMB / costSeconds;
			string str = frontStr + speed.ToString("F3") +  "MB/S";
			ConsoleMgr.LogRed(str);
		}
	}
}

