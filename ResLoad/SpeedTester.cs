using System;

namespace ResLoad
{
	public class SpeedTester
	{
		private DateTime BeginTime;
		private string frontStr = string.Empty;
		private uint TotalBytes = 0;

		public SpeedTester(string frontStr = "")
		{
			this.frontStr = frontStr;
			this.BeginTime = DateTime.Now;
		}
		public void AddLoaded(uint loadedBytes)
		{
			TotalBytes += loadedBytes;
		}

		public double GetSpeed()
		{
			TimeSpan span = DateTime.Now - BeginTime;
			double costSeconds = span.TotalSeconds;
//			ConsoleMgr.LogRed (costSeconds.ToString ());
			double totalMB = (double)TotalBytes / (1024 * 1024);

			double speed = totalMB / costSeconds;
			string str = frontStr + speed.ToString("F3") +  "MB/S";
			ConsoleMgr.LogRed(str);

			return speed;
		}
	}
}

