using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;


namespace ResLoad
{
	public delegate void ResLoadedAction(TaskInfo info, byte[] datas);

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
				}

				return _ins;
			}
		}
		#endregion


		#region Data
		private Dictionary<string, byte[]> m_LoadedDataDic = new Dictionary<string, byte[]> ();
		#endregion

		public byte[] Load(string fileName, ResLoadedAction action, bool isSave = false)		//这里同时加载同一个资源时，可能会有问题
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
				TaskInfo tempTaskInfo = new TaskInfo();
				tempTaskInfo.resName = fileName;
				tempTaskInfo.isSave = isSave;
				tempTaskInfo.isFinished = false;
				tempTaskInfo.resLoadedCallBack = action;
				TaskMgr.Ins.Load (tempTaskInfo);
				return null;
			}
		}
	}
}

