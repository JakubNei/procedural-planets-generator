using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace MyEngine
{
	public class MyFile
	{
		public AssetFolder AssetFolder
		{
			get
			{
				return AssetSystem.GetAssetFolder(this);
			}
		}
		public FileSystem AssetSystem { get; private set; }
		public string VirtualPath { get; private set; }
		public bool HasRealPath
		{
			get
			{
				return string.IsNullOrWhiteSpace(RealPath) == false;
			}
		}
		public string RealPath { get; private set; }

		FileChangedWatcher fileWatcher;
		event Action onFileChanged;

		public MyFile(FileSystem assetSystem, string virtualPath, string realPath)
		{
			this.AssetSystem = assetSystem;
			this.VirtualPath = virtualPath;
			this.RealPath = realPath;
		}

		public Stream GetDataStream()
		{
			int numTries = 0;
			while (numTries++ < 10)
			{
				try
				{
					return new FileStream(RealPath, FileMode.Open, FileAccess.Read, FileShare.Read);
				}
				catch
				{
					System.Threading.Thread.Sleep(numTries);
				}
			}
			return null;
		}

		public void OnFileChanged(Action action)
		{
			if (fileWatcher == null)
			{
				fileWatcher = new FileChangedWatcher();
				fileWatcher.WatchFile(RealPath, (newFileName) => onFileChanged.Raise());
			}
			onFileChanged += action;
		}

		public override string ToString()
		{
			return VirtualPath;
		}
	}
}
