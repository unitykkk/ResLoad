using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;


namespace ResLoad
{
	public class ResMgr
	{
		#region 单例
		private static ResMgr _ins = null;
		public static ResMgr Ins
		{
			get
			{
				if (_ins == null) 
				{
					_ins = new ResMgr ();
					_ins.Init ();
				}

				return _ins;
			}
		}
		#endregion


		#region Data
		private Dictionary<string, ResPackageInfo> m_PackedResInfosDic = new Dictionary<string, ResPackageInfo>();

		public Dictionary<string, ResPackageInfo> PackedResInfosDic
		{
			get
			{
				return m_PackedResInfosDic;
			}
		}

		private Dictionary<string, PackedFileInfo> m_FilesPackInfoDic = new Dictionary<string, PackedFileInfo>();

		public Dictionary<string, PackedFileInfo> FilesPackInfoDic
		{
			get
			{
				return m_FilesPackInfoDic;
			}
		}

		private Dictionary<string, byte[]> m_LoadedDataDic = new Dictionary<string, byte[]> ();
		#endregion


		#region 初始化
		private void Init()
		{
			string[] resPaths = Directory.GetFiles (GlobalSetting.PackToFolderPath, "*", SearchOption.TopDirectoryOnly);
			for (int n = 0; n < resPaths.Length; n++) 
			{
				FileInfo tempResFileInfo = new FileInfo (resPaths [n]);
				if (tempResFileInfo.Extension.ToLower ().Equals (".bin")) 
				{
					FilePackInfoReader.Read(ref m_PackedResInfosDic, ref m_FilesPackInfoDic, resPaths[n]);
				}
			}
		}
		#endregion


		#region 提供给外部的接口
		public byte[] Load(string fileName, bool isSave = false)
		{
			if (m_LoadedDataDic.ContainsKey (fileName)) 
			{
				byte[] returnData = m_LoadedDataDic [fileName];
				if (!isSave) 
				{
					m_LoadedDataDic.Remove (fileName);
				}

				return returnData;
			} 
			else 
			{
				byte[] fileData = LoadItem (fileName);
				if (isSave) 
				{
					m_LoadedDataDic [fileName] = fileData;
				}
				return fileData;
			}
		}

		/// <summary>
		/// 获取资源包信息
		/// </summary>
		/// <returns>The res package info.</returns>
		/// <param name="resName">Res name.</param>
		public ResPackageInfo GetResPackageInfo(string resName)
		{
			if (m_PackedResInfosDic.ContainsKey (resName)) 
			{
				return m_PackedResInfosDic [resName];
			}

			return null;
		}

		/// <summary>
		/// 获取文件打包后的信息
		/// </summary>
		/// <returns>文件打包后的信息</returns>
		/// <param name="resName">文件名</param>
		public PackedFileInfo GetFileInfo(string fileName)
		{
			if (m_FilesPackInfoDic.ContainsKey (fileName)) 
			{
				return m_FilesPackInfoDic [fileName];
			}

			return null;
		}
		#endregion


		private byte[] LoadItem(string fileName)
		{
			try
			{
				string resPath = GlobalSetting.PackToFolderPath + @"/" + ResMgr.Ins.FilesPackInfoDic[fileName].ResName + ".bin";
				FileStream fs = new FileStream(resPath, FileMode.Open, FileAccess.Read);

				//Seek索引默认从0开始(注意,不是从1开始)
				fs.Seek(ResMgr.Ins.FilesPackInfoDic[fileName].StartPos, SeekOrigin.Begin);

				byte[] datas = new byte[ResMgr.Ins.FilesPackInfoDic[fileName].Size];// 要读取的内容会放到这个数组里
				fs.Read(datas, 0, datas.Length);// 开始读取，读取的内容放到datas数组里，0是从第一个开始放，datas.length是最多允许放多少个
				return datas;

			}
			catch(Exception e)
			{
				Console.WriteLine(e.ToString());
				return null;
			}
		}

	}

	/// <summary>
	/// 文件打包后的信息
	/// </summary>
	public class PackedFileInfo
	{
		public string ResName;							//文件在哪个资源包中
		public long StartPos;        					//文件在资源包中存放的起始位置
		public int Size;            					//文件大小(字节)
	}

	/// <summary>
	/// 打包后资源包的信息
	/// </summary>
	public class ResPackageInfo
	{
		public int Version = 0;
		public string TypeName = string.Empty;
		public uint TotalSize = 0;
	}
}

