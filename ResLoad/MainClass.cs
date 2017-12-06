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
			string[] filePaths = Directory.GetFiles(GlobalSetting.PackFromFolderPath, "*.*", SearchOption.AllDirectories);
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
			string[] filePaths = Directory.GetFiles(GlobalSetting.PackFromFolderPath, "*.*", SearchOption.AllDirectories);
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
			if (AsyncfileCount == PackedFileMgr.Ins.PackFilesCount) 
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
			List<string> packedFileNames = PackedFileMgr.Ins.PackedFileNames;

			TimeCounter timer3 = new TimeCounter("我的方案-");
			for (int i = 0; i < packedFileNames.Count; i++)
			{
				string fileName = packedFileNames[i];

				byte[] returnData = PackedFileMgr.Ins.Load(fileName);
				if ((returnData != null) && (returnData.Length > 0)) 
				{
					SelfFileCount++;
				} 
				else 
				{
					int iii = 0;
					ConsoleMgr.LogRed ("错误:获取不到文件名为" + fileName + "的数据，该文件数据可能为空!");
				}
			}

			if (SelfFileCount == PackedFileMgr.Ins.PackFilesCount) 
			{
				timer3.CostTime ();
			}
		}
		#endregion
	}
}
