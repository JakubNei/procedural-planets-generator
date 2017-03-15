using System;
using System.IO;
using System.Linq;

/// <summary>
/// Might be good to use http://stackoverflow.com/a/42609077/782022
/// </summary>
public class GlobSearch
{
	public bool IgnoreCase { get; set; }
	private bool endMustMatch;
	private bool startMustMatch;
	private string searchPattern;
	public GlobSearch(string searchPattern)
	{
		this.searchPattern = searchPattern;
		while (this.searchPattern.StartsWith("*"))
		{
			endMustMatch = true;
			this.searchPattern = this.searchPattern.Substring(1);
		}
		while (this.searchPattern.EndsWith("*"))
		{
			startMustMatch = true;
			this.searchPattern = this.searchPattern.Substring(0, this.searchPattern.Length - 1);
		}
	}

	public bool Matches(string otherName)
	{
		if (searchPattern == "*") return true;
		var comparisonType = StringComparison.InvariantCulture;
		if (IgnoreCase) comparisonType = StringComparison.InvariantCultureIgnoreCase;

		if (startMustMatch && endMustMatch) return otherName.StartsWith(searchPattern, comparisonType) || otherName.EndsWith(searchPattern, comparisonType);
		if (endMustMatch) return otherName.EndsWith(searchPattern, comparisonType);
		if (startMustMatch) return otherName.StartsWith(searchPattern, comparisonType);
		return otherName.Equals(searchPattern, comparisonType);
	}

	public static FileInfo FindFile(string fileSearchPattern)
	{
		var globSearch = new GlobSearch(fileSearchPattern);
		var dirPath = Path.GetDirectoryName(fileSearchPattern);
		var dirInfo = new DirectoryInfo(dirPath);
		var file = dirInfo.EnumerateFiles().FirstOrDefault(f => globSearch.Matches(f.FullName));
		return file;
	}


	public string CombinePaths(params string[] pathParts)
	{
		return string.Join(System.IO.Path.DirectorySeparatorChar.ToString(), pathParts.SelectMany(p => p.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries)).ToArray());
	}



	public static bool IsNeeded(string searchPattern)
	{
		return searchPattern.Contains("*");
	}

}
