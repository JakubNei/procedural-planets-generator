using HtmlAgilityPack;
using System.Net;

namespace Neitri.WebCrawling
{
	public static class Extensions
	{
		public static HtmlDocument GetHtml(this HttpWebResponse response)
		{
			return response.GetResponseStream().ReadTextToEnd().ToHtmlDocument();
		}

		public static HtmlDocument ToHtmlDocument(this string htmlText)
		{
			var html = new HtmlDocument();
			html.LoadHtml(htmlText);
			return html;
		}
	}
}