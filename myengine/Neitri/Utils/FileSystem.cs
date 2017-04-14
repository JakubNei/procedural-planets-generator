using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Neitri
{
	public abstract class PathBase : IEquatable<PathBase>
	{
		public DirectoryPath RootDirectory
		{
			get; set;
		}

		/// <summary>
		/// Relative path parts.
		/// </summary>
		public string[] PathParts
		{
			get; set;
		}

		/// <summary>
		/// Gets full expanded absolute poth.
		/// </summary>
		public string FullPath
		{
			get
			{
				return GetFullPath(MakePath(PathParts));
			}
		}

		/// <summary>
		/// Last part of <see cref="PathParts"/>.
		/// </summary>
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
				var dir = new DirectoryPath(FullPath, "..");
				dir.RootDirectory = RootDirectory;
				return dir;
			}
		}

		public PathBase(params string[] pathParts)
		{
			// makes sure there are no white characters around / or \
			// input: D:\SSD_GAMES\steamapps\common\Arma 3\@taw_div_core/addons\task_force_radio_items.pbo
			// output: D:\SSD_GAMES\steamapps\common\Arma 3\@taw_div_core / addons\task_force_radio_items.pbo
			this.PathParts = Split(MakePath(pathParts));
		}

		string GetFullPath(params string[] pathParts)
		{
			if (pathParts.First().Contains(":")) // a non relative path e.g.: C:/Windows
			{
				return Path.GetFullPath(MakePath(pathParts));
			}
			else
			{
				if (RootDirectory == null)
				{
					// no root directory, lets use default
					return Path.GetFullPath(
						MakePath(pathParts)
					);
				}
				else
				{
					return Path.GetFullPath(
						MakePath(
							MakePath(RootDirectory.FullPath),
							MakePath(pathParts)
						)
					);
				}
			}
		}

		public override int GetHashCode()
		{
			return FullPath.GetHashCode();
		}

		public bool Equals(PathBase other)
		{
			if (other == null) return false;
			if (ReferenceEquals(this, other)) return true;
			return other.FullPath == this.FullPath;
		}

		public override bool Equals(object obj)
		{
			var other = obj as PathBase;
			if (other == null) return false;
			return Equals(other);
		}

		public static bool operator ==(PathBase a, PathBase b)
		{
			if (ReferenceEquals(a, b)) return true;
			if (ReferenceEquals(a, null)) return false;
			return a.Equals(b);
		}

		public static bool operator !=(PathBase a, PathBase b)
		{
			return !(a == b);
		}

		public override string ToString()
		{
			return MakePath(PathParts);
		}

		/// <summary>
		/// Path parts into path.
		/// </summary>
		/// <param name="pathParts"></param>
		/// <returns></returns>
		protected string MakePath(params string[] pathParts)
		{
			return string.Join("/", pathParts);
		}

		/// <summary>
		/// Path into path parts.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		protected string[] Split(string path)
		{
			return path.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
		}

		string[] ArrayAdd(string[] arr, string add)
		{
			var ret = new string[arr.Length + 1];
			Array.Copy(arr, ret, arr.Length);
			ret[ret.Length] = add;
			return ret;
		}

		protected string[] CombinePathParts(string[] arr1, params string[] arr2)
		{
			return ArrayAdd(
				arr1.SelectMany(p => Split(p)).ToArray(),
				arr2.SelectMany(p => Split(p)).ToArray()
			);
		}

		string[] ArrayAdd(string[] arr1, string[] arr2)
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

		public bool Exists
		{
			get
			{
				return DirectoryExists();
			}
		}

		public DirectoryPath(params string[] pathParts) : base(pathParts)
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

		public FilePath GetFile(params string[] pathParts)
		{
			var file = new FilePath(CombinePathParts(PathParts, pathParts));
			file.RootDirectory = this.RootDirectory;
			return file;
		}

		public IEnumerable<FilePath> FindFiles(string searchPattern)
		{
			return Directory.GetFiles(this.FullPath, searchPattern).Select(f => GetFile(f));
		}

		public DirectoryPath GetDirectory(params string[] pathParts)
		{
			var dir = new DirectoryPath(CombinePathParts(this.PathParts, pathParts))
			{
				RootDirectory = this.RootDirectory
			};
			return dir;
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

		public bool Exists
		{
			get
			{
				return FileExists();
			}
		}

		public FilePath(string[] pathParts) : base(pathParts)
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

	public class FileSystem : DirectoryPath
	{
		public FileSystem() : base(new string[] { })
		{
			RootDirectory = new DirectoryPath(AppDomain.CurrentDomain.BaseDirectory);
#if DEBUG
			RootDirectory = new DirectoryPath("..", "release");
#endif
			RootDirectory.ExceptionIfNotExists();
		}
	}
}