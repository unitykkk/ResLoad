using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;


namespace ResLoad
{
	public delegate void ResLoadedCallBack(TaskInfo info, byte[] datas);

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
		private Dictionary<string, ResInfo> m_ResInfoDic = null;

		public Dictionary<string, ResInfo> ResInfoDic
		{
			get
			{
				return m_ResInfoDic;
			}
		}

		private static object LoadedActionQueueObj = new object();
		private Queue<ResLoadedInfo> m_LoadedInfosQueue = new Queue<ResLoadedInfo> ();

		public object DataObj = new object ();
		private Dictionary<string, byte[]> m_LoadedDataDic = new Dictionary<string, byte[]> ();
		#endregion


		#region 初始化
		private void Init()
		{
			LoadResInfos ();
		}

		/// <summary>
		/// 从资源包里面加载打包后各资源的信息
		/// </summary>
		private void LoadResInfos()
		{
			m_ResInfoDic = new Dictionary<string, ResInfo>();

			FileStream fs = new FileStream(GlobalSetting.PackedFilePath, FileMode.Open);

			//读取资源信息块长度
			byte[] infoRegionLengthData = new byte[2];		//ushort
			fs.Read(infoRegionLengthData, 0, 2);
			ushort infoLength = ReadUshort(infoRegionLengthData);

			for (int nowPos = 0; nowPos < infoLength;) 
			{
				ResInfo tempInfo = new ResInfo();
				//读取文件名字节长度
				byte[] nameLengthData = new byte[2];
				fs.Read(nameLengthData, 0, 2);
				ushort nameLength = ReadUshort(nameLengthData);
				nowPos += 2;
				//读取文件名
				byte[] nameData = new byte[nameLength];
				fs.Read(nameData, 0, nameLength);
				string resName = ReadString (nameData);
				nowPos += nameLength;
				//读取文件起始位置
				byte[] startPosData = new byte[4];
				fs.Read(startPosData, 0, 4);
				tempInfo.StartPos = ReadUint(startPosData);
				nowPos += 4;
				//读取文件大小
				byte[] sizeData = new byte[4];
				fs.Read(sizeData, 0, 4);
				tempInfo.Size = ReadInt(sizeData);
				nowPos += 4;

				m_ResInfoDic.Add (resName, tempInfo);
			}

			fs.Flush();
			fs.Close();
		}

		private ushort ReadUshort(byte[] datas)
		{
			return BitConverter.ToUInt16 (datas, 0);
		}

		private string ReadString(byte[] datas)
		{
			string str = System.Text.Encoding.UTF8.GetString (datas);
			return str;
		}

		private uint ReadUint(byte[] datas)
		{
			return BitConverter.ToUInt32 (datas, 0);
		}

		private int ReadInt(byte[] datas)
		{
			return BitConverter.ToInt32 (datas, 0);
		}
		#endregion


		#region 提供给外部的接口
		public byte[] Load(string fileName, ResLoadedCallBack itemCallBack, bool isSave = false)		//这里同时加载同一个资源时，可能会有问题
		{
			lock (DataObj)
			{
				if (!m_IsCreatedThread) 
				{
					m_IsCreatedThread = true;
					CreateTread ();
				}

				if (m_LoadedDataDic.ContainsKey (fileName)) 
				{
					byte[] returnData = m_LoadedDataDic [fileName];
					if (!isSave) {
						m_LoadedDataDic.Remove (fileName);
					}

					return returnData;
				} 
				else 
				{
					TaskInfo tempTaskInfo = new TaskInfo ();
					tempTaskInfo.resName = fileName;
					tempTaskInfo.isSave = isSave;
					tempTaskInfo.isFinished = false;
					tempTaskInfo.itemCallBack = itemCallBack;
					tempTaskInfo.commonCallBack = Ins.ResLoadedCommonCallBack;
//					tempTaskInfo.commonCallBack = itemCallBack;
					TaskMgr.Ins.Load (tempTaskInfo);

					return null;
				}
			}
		}

		/// <summary>
		/// 获取资源信息
		/// </summary>
		/// <returns>资源信息</returns>
		/// <param name="resName">资源名</param>
		public ResInfo GetInfo(string resName)
		{
			if (m_ResInfoDic.ContainsKey (resName)) 
			{
				return m_ResInfoDic [resName];
			}

			return null;
		}
		#endregion


		#region Event
		private void ResLoadedCommonCallBack(TaskInfo info, byte[] datas)
		{
//			ConsoleMgr.LogGreen("ResLoadedCommonCallBack Success:" + info.resName);
//			var temp = datas;

			lock (LoadedActionQueueObj) 
			{
				ResLoadedInfo tempInfo = new ResLoadedInfo ();
				tempInfo.info = info;
				tempInfo.datas = datas;
				m_LoadedInfosQueue.Enqueue (tempInfo);
			}
		}
		#endregion


		#region Thread
		private bool m_IsCreatedThread = false;
		private int MMaxDealCount = 20;


		private void CreateTread()
		{
			Thread thread = new Thread(ThreadFunc);
			thread.Name = "线程Deal";
			//给方法传值,启动线程
			thread.Start();
		}

		public void ThreadFunc()
		{
			while (true)
			{
				lock (LoadedActionQueueObj)     //本类的子线程
				{
					if (m_LoadedInfosQueue.Count > 0)
					{
						for (int i = 0; i < MMaxDealCount; i++)
						{
							if (m_LoadedInfosQueue.Count > 0)
							{
								ResLoadedInfo itemLoadedInfo = m_LoadedInfosQueue.Dequeue();
								if (itemLoadedInfo.info.itemCallBack != null)
								{
									itemLoadedInfo.info.itemCallBack(itemLoadedInfo.info, itemLoadedInfo.datas);

									lock (DataObj) 
									{
										if (!itemLoadedInfo.info.isSave) 
										{
											if (m_LoadedDataDic.ContainsKey (itemLoadedInfo.info.resName)) 
											{
												m_LoadedDataDic.Remove (itemLoadedInfo.info.resName);
											}
										} 
										else 
										{
											m_LoadedDataDic [itemLoadedInfo.info.resName] = itemLoadedInfo.datas;
										}
									}
								}
							}
							else
							{
								break;
							}
						}
					}
					else
					{
//						Thread.Sleep(10);
					}
				}
			}
		}
		#endregion
	}

	/// <summary>
	/// 资源加载信息
	/// </summary>
	public class ResLoadedInfo
	{
		public TaskInfo info;        	//资源加载任务信息
		public byte[] datas;			//资源加载后获取到的数据
	}
}

