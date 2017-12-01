using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;


namespace ResLoad
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			//方案1：用系统自带API
			TimeCounter timer1 = new TimeCounter("系统普通方案-");
			LoadFiles ();
			timer1.CostTime ();
			Console.WriteLine ("");

			//方案2，用系统异步加载API
			timer2 = new TimeCounter("系统异步方案-");
			LoadFilesAsync();

			Console.WriteLine ("");

			//方案3：用自写API
			LoadFilesBySelf();
		}


		#region 方案1，用系统同步加载API
		/// <summary>
		/// 方案1，用系统同步加载API
		/// </summary>
		private static void LoadFiles()
		{
			string[] filePaths = Directory.GetFiles(GlobalSetting.PackFolderPath, "*.*", SearchOption.AllDirectories);
			for (int i = 0; i < filePaths.Length; i++) 
			{
				FileStream fs = new FileStream(filePaths[i], FileMode.Open);

				byte[] datas = new byte[fs.Length];
				fs.Read(datas, 0, datas.Length);

				fs.Flush();
				fs.Close();
			}
		}
		#endregion


		#region 方案2，用系统异步加载API
		/// <summary>
		/// 方案2，用系统异步加载API
		/// </summary>
		private static void LoadFilesAsync()
		{
			string[] filePaths = Directory.GetFiles(GlobalSetting.PackFolderPath, "*.*", SearchOption.AllDirectories);
			for (int i = 0; i < filePaths.Length; i++) 
			{
				FileStream fs = new FileStream(filePaths[i], FileMode.Open);

				byte[] datas = new byte[fs.Length];
				string resName = filePaths[i];
				IAsyncResult asyncResult = fs.BeginRead(datas, 0, datas.Length,FileAsyncCallback,resName);
				asyncResult.AsyncWaitHandle.WaitOne();
				fs.Flush();
				fs.Close();
			}
		}

		private static int AsyncfileCount = 0;
		private static TimeCounter timer2 = null;
		public static void FileAsyncCallback (IAsyncResult ar)
		{
			string filePath = (string)ar.AsyncState;
//			ConsoleMgr.LogGreen("系统已异步加载完" + filePath);
			AsyncfileCount++;
			if (AsyncfileCount == ResMgr.Ins.ResInfoDic.Count) 
			{
				timer2.CostTime();
				Console.WriteLine ("");
			}
		}
		#endregion


		#region 方案3，用自写API
		private static int SelfFileCount = 0;
		private static void LoadFilesBySelf()
		{
			List<string> keys = new List<string>(ResMgr.Ins.ResInfoDic.Keys);

			TimeCounter timer3 = new TimeCounter("我的方案-");
			for (int i = 0; i < keys.Count; i++)
			{
				string resName = keys[i];

				ResMgr.Ins.Load(resName, delegate(TaskInfo info, byte[] datas){
//					ConsoleMgr.LogGreen("Load Resource Success:" + info.resName);
					var temp = datas;
					SelfFileCount++;
					if (SelfFileCount == ResMgr.Ins.ResInfoDic.Count)
					{
						timer3.CostTime();
					}
				}, true);
			}
		}
		#endregion
	}
}
