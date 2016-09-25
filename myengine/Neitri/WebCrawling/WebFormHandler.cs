using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Neitri.WebCrawling
{
	public class WebFormHandler
	{
		public class ParamsCollection : Dictionary<string, string>
		{
			public string ToData()
			{
				return string.Join("&",
					this
						.Where(kvp => kvp.Value != null)
						.Select(kvp => WebUtility.UrlEncode(kvp.Key) + "=" + WebUtility.UrlEncode(kvp.Value))
						.ToArray()
				);
			}
		}

		HtmlNode formHtmlElement;
		CookieContainer cookieContainer;
		ParamsCollection paramsCollection = new ParamsCollection();
		ParamsCollection submitInputs = new ParamsCollection();
		string rootFormUrl;

		public WebFormHandler(MyHttpWebResponse response, CookieContainer cookieContainer)
		{
			this.rootFormUrl = response.RootUrl;
			var document = response.HtmlDocument;
			this.formHtmlElement = document.DocumentNode.SelectSingleNode("//form");
			this.cookieContainer = cookieContainer;
			Init();
		}

		public WebFormHandler(string rootFormUrl, HtmlNode formHtmlElement, CookieContainer cookieContainer)
		{
			this.rootFormUrl = rootFormUrl;
			this.formHtmlElement = formHtmlElement;
			this.cookieContainer = cookieContainer;
			Init();
		}

		void Init()
		{
			HtmlNodeCollection elements;

			// hidden, text, password
			elements = formHtmlElement.SelectNodes("//input[@type='hidden']|//input[@type='text']|//input[@type='password']");
			if (elements != null)
			{
				foreach (var e in elements)
				{
					var name = e.GetAttributeValue("name", null);
					if (name != null)
					{
						FillInput(name, e.GetAttributeValue("value", null));
					}
				}
			}

			// radio, checkbox
			elements = formHtmlElement.SelectNodes("//input[@type='radio']|//input[@type='checkbox']");
			if (elements != null)
			{
				foreach (var e in elements)
				{
					var name = e.GetAttributeValue("name", null);
					if (name != null)
					{
						var value = e.GetAttributeValue("value", null);
						var checkedAttr = e.GetAttributeValue("checked", null) != null;
						if (checkedAttr || InputExists(name) == false)
						{
							FillInput(name, value);
						}
					}
				}
			}

			// select
			elements = formHtmlElement.SelectNodes("//select");
			if (elements != null)
			{
				foreach (var e in elements)
				{
					var name = e.GetAttributeValue("name", null);
					if (name != null)
					{
						var value = e.ChildNodes.FirstOrDefault()?.GetAttributeValue("value", null);
						FillInput(name, value);
					}
				}
			}

			// submit
			elements = formHtmlElement.SelectNodes("//input[@type='submit']");
			if (elements != null)
			{
				foreach (var e in elements)
				{
					var name = e.GetAttributeValue("name", null);
					if (name != null) submitInputs[name] = e.GetAttributeValue("value", null);
				}
			}
		}

		public bool InputExists(string inputName)
		{
			inputName = GlobSearch(inputName, paramsCollection);
			return paramsCollection.ContainsKey(inputName);
		}

		public void FillInput(string inputName, string inputValue)
		{
			if (inputName == null) throw new NullReferenceException();
			inputName = GlobSearch(inputName, paramsCollection);
			paramsCollection[inputName] = inputValue;
		}

		string GlobSearch(string input, Dictionary<string, string> dict)
		{
			if (input.IsNullOrEmpty()) return input;
			if (input.StartsWith("*"))
			{
				input = input.Substring(1);
				input = dict.FirstOrDefault(kvp => kvp.Key.EndsWith(input)).Key;
			}
			return input;
		}

		public MyHttpWebResponse SubmitForm(string submitInputName = null)
		{
			if (submitInputName.IsNullOrEmpty() == false)
			{
				submitInputName = GlobSearch(submitInputName, submitInputs);
				FillInput(submitInputName, submitInputs[submitInputName]);
			}

			var url = formHtmlElement.GetAttributeValue("action", "?");
			if (url.StartsWith("/") == false && url.StartsWith(@"\") == false) url = "/" + url;
			url = rootFormUrl + url;

			var method = formHtmlElement.GetAttributeValue("method", "post").Trim().ToLower();

			if (method == "get")
			{
				url += "?" + paramsCollection.ToData();
			}

			var request = MyHttpWebRequest.Create(url);
			request.CookieContainer = cookieContainer;
			request.Method = method;
			request.ContentType = "application/x-www-form-urlencoded";

			if (method == "post")
			{
				var s = request.GetRequestStream();
				using (var t = new StreamWriter(s))
				{
					t.Write(paramsCollection.ToData());
				}
			}
			if (method == "get")
			{
				request.ContentLength = 0;
			}

			var response = request.GetResponse();

			return response;
		}
	}
}