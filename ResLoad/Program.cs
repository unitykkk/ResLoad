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
//			timer2 = new TimeCounter("系统异步方案-");
//			LoadFilesAsync();

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
				FileStream fs = new FileStream(filePaths[i], FileMode.Open);//这里后期可能会优化,因为游戏很可能要一直不断加载资源,这个fs可考虑不用临时,只创建一个

				byte[] datas = new byte[fs.Length];// 要读取的内容会放到这个数组里
				fs.Read(datas, 0, datas.Length);// 开始读取，读取的内容放到datas数组里，0是从第一个开始放，datas.length是最多允许放多少个

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
				FileStream fs = new FileStream(filePaths[i], FileMode.Open);//这里后期可能会优化,因为游戏很可能要一直不断加载资源,这个fs可考虑不用临时,只创建一个

				byte[] datas = new byte[fs.Length];// 要读取的内容会放到这个数组里
				string resName = filePaths[i];
				IAsyncResult asyncResult = fs.BeginRead(datas, 0, datas.Length,FileAsyncCallback,resName);// 开始读取，读取的内容放到datas数组里，0是从第一个开始放，datas.length是最多允许放多少个
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
			if (AsyncfileCount == TaskMgr.Ins.ResInfoDic.Count) 
			{
				timer2.CostTime();
			}
		}
		#endregion


		#region 方案3，用自写API
		private static int SelfFileCount = 0;
		private static void LoadFilesBySelf()
		{
			List<string> keys = new List<string>(TaskMgr.Ins.ResInfoDic.Keys);

			TimeCounter timer3 = new TimeCounter("我的方案-");
			for (int i = 0; i < keys.Count; i++)
			{
				string resName = keys[i];

				ResMgr.Ins.Load(resName, delegate(TaskInfo info, byte[] datas){
					//ConsoleMgr.LogGreen("Load Resource Success:" + info.resName);
					var temp = datas;
					SelfFileCount++;
					if (SelfFileCount == TaskMgr.Ins.ResInfoDic.Count)
					{
						timer3.CostTime();
					}
				});
			}
		}
		#endregion
	}
}
