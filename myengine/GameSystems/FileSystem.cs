using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MyEngine
{
	public class FileSystem : GameSystemBase
	{
		[Dependency]
		Debug Debug;

		string rootResourceFolderPath;

		public Dictionary<string, Type> extensionToTypeAssociation = new Dictionary<string, Type>()
		{
			{"obj", typeof(Mesh)},
			{"glsl", typeof(Shader)},
			{"shader", typeof(Shader)},
		};

		public FileSystem(string rootResourceFolderPath = "../../../Resources/")
		{
			this.rootResourceFolderPath = rootResourceFolderPath;
		}

		public string CombineDirectory(params string[] pathParts)
		{
			return UseCorrectDirectorySeparator(string.Join("/", pathParts));
		}

		public string UseCorrectDirectorySeparator(string path)
		{
			path = path.Replace('/', System.IO.Path.DirectorySeparatorChar);
			path = path.Replace('\\', System.IO.Path.DirectorySeparatorChar);
			return path;
		}

		public bool FileExists(string virtualPath)
		{
			var realPath = CombineDirectory(rootResourceFolderPath, virtualPath);
			return System.IO.File.Exists(realPath);
		}

		public bool FileExists(string virtualPath, MyFolder startSearchInFolder)
		{
			var realPath = CombineDirectory(rootResourceFolderPath, startSearchInFolder.VirtualPath, virtualPath);
			if (System.IO.File.Exists(realPath))
			{
				return true;
			}
			else
			{
				if (System.IO.File.Exists(realPath))
				{
					return true;
				}
			}
			return false;
		}

		public MyFile FindFile(string virtualPath)
		{
			var realPath = CombineDirectory(rootResourceFolderPath, virtualPath);
			if (System.IO.File.Exists(realPath))
			{
				return new MyFile(this, virtualPath, realPath);
			}
			else
			{
				Debug.Error("File " + virtualPath + " doesnt exits");
				Debug.Pause();
				return null;
			}
		}

		public List<MyFile> Findfiles(params string[] virtualPaths)
		{
			var ret = new List<MyFile>();
			foreach (var p in virtualPaths)
			{
				ret.Add(FindFile(p));
			}
			return ret;
		}

		public MyFile FindFile(string virtualPath, MyFolder startSearchInFolder)
		{
			var realPath = CombineDirectory(rootResourceFolderPath, startSearchInFolder.VirtualPath, virtualPath);
			if (System.IO.File.Exists(realPath))
			{
				return new MyFile(this, CombineDirectory(startSearchInFolder.VirtualPath, virtualPath), realPath);
			}
			else
			{
				realPath = CombineDirectory(rootResourceFolderPath, virtualPath);
				if (System.IO.File.Exists(realPath))
				{
					return new MyFile(this, virtualPath, realPath);
				}
				else
				{
					Debug.Error("File " + CombineDirectory(startSearchInFolder.VirtualPath, virtualPath) + " doesnt exits");
					Debug.Pause();
					return null;
				}
			}
		}

		public MyFolder GetFolder(MyFile file)
		{
			var virtualDir = Path.GetDirectoryName(file.VirtualPath);
			return new MyFolder(virtualDir);
		}
	}
}