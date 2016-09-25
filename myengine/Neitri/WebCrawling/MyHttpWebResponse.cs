using HtmlAgilityPack;
using System;
using System.IO;
using System.Net;

namespace Neitri.WebCrawling
{
	public class MyHttpWebResponse
	{
		HttpWebResponse httpWebResponse;

		public string RootUrl
		{
			get
			{
				var uri = ResponseUri.AbsoluteUri; // http://vdp.cuzk.cz/vdp/ruian/parcely/vyhledej
				var authority = ResponseUri.Authority; // vdp.cuzk.cz
				var rootUrl = uri.Substring(0, uri.IndexOf(authority) + authority.Length); // http://vdp.cuzk.cz
				return rootUrl;
			}
		}

		public Uri ResponseUri => httpWebResponse.ResponseUri;

		Stream responseStream;

		public Stream ResponseStream
		{
			get
			{
				if (responseStream == null) responseStream = httpWebResponse.GetResponseStream();
				return responseStream;
			}
		}

		string responseText;

		public string ResponseText
		{
			get
			{
				if (responseText == null) responseText = ResponseStream.ReadTextToEnd();
				return responseText;
			}
		}

		HtmlDocument htmlDocument;

		public HtmlDocument HtmlDocument
		{
			get
			{
				if (htmlDocument == null) htmlDocument = ResponseText.ToHtmlDocument();
				return htmlDocument;
			}
		}

		public MyHttpWebResponse(HttpWebResponse httpWebResponse)
		{
			this.httpWebResponse = httpWebResponse;
		}

		public void EnsureReponseWasRequested()
		{
			httpWebResponse.GetResponseStream();
		}

		public static implicit operator MyHttpWebResponse(HttpWebResponse other)
		{
			return new MyHttpWebResponse(other);
		}
	}
}