using System.IO;
using System.Net;
using System.Net.Security;

namespace Neitri.WebCrawling
{
	public class MyHttpWebRequest
	{
		public CookieContainer CookieContainer
		{
			get
			{
				return httpWebRequest.CookieContainer;
			}
			set
			{
				httpWebRequest.CookieContainer = value;
			}
		}

		public string Method
		{
			get
			{
				return httpWebRequest.Method;
			}
			set
			{
				httpWebRequest.Method = value;
			}
		}

		public string ContentType
		{
			get
			{
				return httpWebRequest.ContentType;
			}
			set
			{
				httpWebRequest.ContentType = value;
			}
		}

		public long ContentLength
		{
			get
			{
				return httpWebRequest.ContentLength;
			}
			set
			{
				httpWebRequest.ContentLength = value;
			}
		}

		HttpWebRequest httpWebRequest;

		//static Queue<DateTime> timesOfRequestsMade = new Queue<DateTime>();

		//http://stackoverflow.com/questions/703272/could-not-establish-trust-relationship-for-ssl-tls-secure-channel-soap
		static MyHttpWebRequest()
		{
			ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);
			// trust sender
			ServicePointManager.ServerCertificateValidationCallback = ((sender, cert, chain, errors) => true);
			// validate cert by calling a function
			ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback((sender, cert, chain, errors) => true);
		}

		public static MyHttpWebRequest Create(string url)
		{
			var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			var myHttpWebRequest = new MyHttpWebRequest()
			{
				httpWebRequest = httpWebRequest
			};
			return myHttpWebRequest;
		}

		public MyHttpWebResponse GetResponse()
		{
			return new MyHttpWebResponse((HttpWebResponse)httpWebRequest.GetResponse());
		}

		public Stream GetRequestStream()
		{
			return httpWebRequest.GetRequestStream();
		}
	}
}