using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Neitri
{
	public class PathBase
	{
		public FileSystem FileSystem
		{
			get; set;
		}

		public string[] PathParts
		{
			get; set;
		}

		public string FullPath
		{
			get
			{
				return GetFullPath(Combine(PathParts));
			}
		}

		public string Name
		{
			get
			{
				return PathParts.Last();
			}
			set
			{
				PathParts[PathParts.Length - 1] = value;
			}
		}

		protected DirectoryPath ParentDirectory
		{
			get
			{
				return new DirectoryPath(FileSystem, PathParts.Take(PathParts.Length - 2).ToArray());
			}
		}

		public PathBase(FileSystem fs, string[] pathParts)
		{
			this.FileSystem = fs;
			// makes sure there are no white characters around / or \
			// input: D:\SSD_GAMES\steamapps\common\Arma 3\@taw_div_core/addons\task_force_radio_items.pbo
			// output: D:\SSD_GAMES\steamapps\common\Arma 3\@taw_div_core / addons\task_force_radio_items.pbo
			this.PathParts = Split(Combine(pathParts));
		}

		string GetFullPath(params string[] pathParts)
		{
			if (pathParts.First().Contains(":"))
			{
				return Path.GetFullPath(Combine(pathParts));
			}
			else
			{
				return Path.GetFullPath(
					Combine(
						Combine(FileSystem.BaseDirectory.PathParts),
						Combine(pathParts)
					)
				);
			}
		}

		public override string ToString()
		{
			return Combine(PathParts);
		}

		protected string Combine(params string[] pathParts)
		{
			return string.Join("/", pathParts);
		}

		protected string[] Split(string path)
		{
			return path.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
		}

		protected string[] ArrayAdd(string[] arr, string add)
		{
			var ret = new string[arr.Length + 1];
			Array.Copy(arr, ret, arr.Length);
			ret[ret.Length] = add;
			return ret;
		}

		protected string[] ArrayAdd(string[] arr1, string[] arr2)
		{
			var ret = new string[arr1.Length + arr2.Length];
			Array.Copy(arr1, ret, arr1.Length);
			Array.Copy(arr2, 0, ret, arr1.Length, arr2.Length);
			return ret;
		}
	}

	public class DirectoryPath : PathBase
	{
		public DirectoryPath Parent
		{
			get
			{
				return ParentDirectory;
			}
		}

		public bool Exists => DirectoryExists();

		public DirectoryPath(FileSystem fs, string[] pathParts) : base(fs, pathParts)
		{
		}

		bool DirectoryExists()
		{
			return Directory.Exists(FullPath);
		}

		public DirectoryPath Create()
		{
			if (Exists) throw new Exception("can not create directory, directory '" + this + "' already exists");
			Directory.CreateDirectory(FullPath);
			return this;
		}

		public DirectoryPath Delete()
		{
			Directory.Delete(FullPath, true);
			return this;
		}

		public DirectoryPath Empty()
		{
			Delete();
			Create();
			return this;
		}

		/// <summary>
		/// If the directory doesnt exist, creates it.
		/// </summary>
		public DirectoryPath CreateIfNotExists()
		{
			var path = FullPath;
			if (Exists == false) Create();
			return this;
		}

		public DirectoryPath ExceptionIfNotExists()
		{
			if (Exists == false) throw new Exception("directory '" + this + "' does not exist");
			return this;
		}

		public FilePath GetFile(params string[] path)
		{
			return new FilePath(FileSystem, ArrayAdd(PathParts, path));
		}

		public IEnumerable<FilePath> FindFiles(string searchPattern)
		{
			return Directory.GetFiles(this.FullPath, searchPattern).Select(f => new FilePath(FileSystem, Split(f)));
		}

		public DirectoryPath GetDirectory(string path)
		{
			return new DirectoryPath(FileSystem, ArrayAdd(this.PathParts, Split(path)));
		}

		public DirectoryPath GetDirectory(params string[] pathParts)
		{
			return new DirectoryPath(FileSystem, ArrayAdd(this.PathParts, pathParts));
		}

		public override string ToString()
		{
			return this.FullPath;
		}

		public static implicit operator string(DirectoryPath me)
		{
			return me.FullPath;
		}
	}

	public class FilePath : PathBase
	{
		public DirectoryPath Directory
		{
			get
			{
				return ParentDirectory;
			}
		}

		public bool Exists => FileExists();

		public FilePath(FileSystem fs, string[] pathParts) : base(fs, pathParts)
		{
		}

		bool FileExists()
		{
			return File.Exists(FullPath);
		}

		/*
		public FileStream Create()
		{
			if (Exists) throw new Exception($" can not create file, file '{this}' already exists");
			return File.Create(FullPath);
		}
		public FilePath CreateIfNotExists()
		{
			var path = FullPath;
			if (Exists == false) Create();
			return this;
		}
		*/

		public FilePath ExceptionIfNotExists()
		{
			if (Exists == false) throw new Exception("file '" + FullPath + "' does not exist");
			return this;
		}

		public FilePath CopyTo(FilePath other)
		{
			if (other.Exists) throw new NullReferenceException("can not copy file, file already exists '" + other.FullPath + "'");
			File.Copy(this.FullPath, other.FullPath);
			return this;
		}

		public FilePath Delete()
		{
			File.Delete(this.FullPath);
			return this;
		}

		public override string ToString()
		{
			return this.FullPath;
		}

		public static implicit operator string(FilePath me)
		{
			return me.FullPath;
		}
	}

	public class FileSystem
	{
		public DirectoryPath BaseDirectory { get; set; }

		public FileSystem()
		{
			BaseDirectory = GetDirectory(AppDomain.CurrentDomain.BaseDirectory);
#if DEBUG
			BaseDirectory = BaseDirectory.GetDirectory("..").GetDirectory("release");
#endif
			BaseDirectory.ExceptionIfNotExists();
		}

		public DirectoryPath GetDirectory(params string[] pathParts)
		{
			return new DirectoryPath(this, pathParts);
		}

		public FilePath GetFile(params string[] pathParts)
		{
			return new FilePath(this, pathParts);
		}
	}
}