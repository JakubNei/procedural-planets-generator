using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace Neitri.WebCrawling
{
	public class HtmlTable : List<List<HtmlNode>>
	{
		/// <summary>
		/// Excludes th, takes only contents of tr td
		/// </summary>
		public HtmlTable(HtmlNode tableNode)
		{
			foreach (var tr in tableNode.SelectNodes("./tr"))
			{
				var trData = new List<HtmlNode>();
				var childs = tr.SelectNodes("./td");
				if (childs == null) continue;
				foreach (var td in childs)
				{
					trData.Add(td);
				}
				this.Add(trData);
			}
		}
	}

	public class HtmlTwoColsStringTable : Dictionary<string, string>
	{
		public HtmlTwoColsStringTable(HtmlNode tableNode) : this(tableNode?.SelectNodes("./tr | ./tbody/tr"))
		{
		}

		public HtmlTwoColsStringTable(HtmlNodeCollection tableRows)
		{
			if (tableRows == null) return;
			foreach (var tr in tableRows)
			{
				var trData = new List<HtmlNode>();
				var childs = tr.SelectNodes("./td");
				if (childs == null) continue;
				foreach (var td in childs)
				{
					trData.Add(td);
				}
				if (trData.Count >= 2)
				{
					this[trData[0].InnerText.Trim()] = trData[1].InnerText.Trim();
				}
			}
		}
	}

	public class HtmlOneColsStringTable : List<string>
	{
		public HtmlOneColsStringTable(HtmlNode tableNode) : this(tableNode?.SelectNodes("./tr | ./tbody/tr"))
		{
		}

		public HtmlOneColsStringTable(HtmlNodeCollection tableRows)
		{
			if (tableRows == null) return;
			foreach (var tr in tableRows)
			{
				var trData = new List<HtmlNode>();
				var childs = tr.SelectNodes("./td");
				if (childs == null) continue;
				foreach (var td in childs)
				{
					trData.Add(td);
				}
				if (trData.Count >= 1)
				{
					this.Add(trData.First().InnerText.Trim());
				}
			}
		}
	}
}