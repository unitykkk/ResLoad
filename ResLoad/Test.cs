using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;


namespace ResLoad
{
	class Test
	{
		private static SpeedTester SpeedTester1 = null;
		private static SpeedTester SpeedTester2 = null;


		public static void Main (string[] args)
		{
			//方案1：用系统自带API
			SpeedTester1 = new SpeedTester("系统方案: ");
			LoadFiles ();
			SpeedTester1.CountSpeed ();
			Console.WriteLine ("");

			//方案3：用自写API
			LoadFilesBySelf();
			PackedFileMgr.Ins.Release ();
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
				SpeedTester1.AddLoaded ((uint)datas.Length);

				fs.Flush();
				fs.Close();
			}
		}
		#endregion


		#region 方案2，用自写API
		private static int SelfFileCount = 0;
		private static void LoadFilesBySelf()
		{
			List<string> packedFileNames = PackedFileMgr.Ins.PackedFileNames;

			SpeedTester2 = new SpeedTester("我的方案: ");
			for (int i = 0; i < packedFileNames.Count; i++)
			{
				string fileName = packedFileNames[i];

				byte[] returnData = PackedFileMgr.Ins.Load(fileName);
				if ((returnData != null) && (returnData.Length > 0)) 
				{
					SelfFileCount++;
					SpeedTester2.AddLoaded ((uint)returnData.Length);
				} 
				else 
				{
					ConsoleMgr.LogRed ("错误:获取不到文件名为" + fileName + "的数据，该文件数据可能为空!");
				}
			}

			if (SelfFileCount == PackedFileMgr.Ins.PackFilesCount) 
			{
				SpeedTester2.CountSpeed ();
			}
		}
		#endregion
	}
}
