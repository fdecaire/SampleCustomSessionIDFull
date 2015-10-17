using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using System.Security.Cryptography;
using System.Configuration;
using System.Web.Configuration;

namespace MyCustomSession
{
	public class CustomSessionIDManager : ISessionIDManager
	{
		public const int KEY_LENGTH = 64;
		private char[] Encoding = {
			'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
			'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
			'0', '1', '2', '3', '4', '5' };
		private SessionStateSection pConfig = null;

		public string CreateSessionID(HttpContext context)
		{
			char[] identifier = new char[KEY_LENGTH];
			byte[] randomData = new byte[KEY_LENGTH];

			using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
			{
				rng.GetBytes(randomData);
			}

			for (int i = 0; i < identifier.Length; i++)
			{
				int pos = randomData[i] % Encoding.Length;
				identifier[i] = Encoding[pos];
			}

			return new string(identifier);
		}

		public bool Validate(string id)
		{
			try
			{
				if (id.Length != KEY_LENGTH)
				{
					return false;
				}

				for (int i = 0; i < id.Length; i++)
				{
					if (!Encoding.Contains(id[i]))
					{
						return false;
					}
				}

				return true;
			}
			catch
			{
			}

			return false;
		}

		public string GetSessionID(HttpContext context)
		{
			string id = null;

			if (pConfig.Cookieless == HttpCookieMode.UseUri)
			{
				// Retrieve the SessionID from the URI.
			}
			else
			{
				if (context.Request.Cookies[pConfig.CookieName] == null)
				{
					bool redirected;
					bool cookieAdded;

					SaveSessionID(context, id, out redirected, out cookieAdded);
				}
				else
				{
					id = context.Request.Cookies[pConfig.CookieName].Value;
				}
			}

			// Verify that the retrieved SessionID is valid. If not, return null.

			if (!Validate(id))
			{
				id = null;
			}

			return id;
		}

		public void Initialize()
		{
			if (pConfig == null)
			{
				Configuration cfg = WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
				pConfig = (SessionStateSection)cfg.GetSection("system.web/sessionState");
			}
		}

		public bool InitializeRequest(HttpContext context, bool suppressAutoDetectRedirect, out bool supportSessionIDReissue)
		{
			if (pConfig.Cookieless == HttpCookieMode.UseCookies)
			{
				supportSessionIDReissue = false;
				return false;
			}
			else
			{
				supportSessionIDReissue = true;
				return context.Response.IsRequestBeingRedirected;
			}
		}

		public void RemoveSessionID(HttpContext context)
		{
			context.Response.Cookies.Remove(pConfig.CookieName);
		}

		public void SaveSessionID(HttpContext context, string id, out bool redirected, out bool cookieAdded)
		{
			redirected = false;
			cookieAdded = false;

			if (pConfig.Cookieless == HttpCookieMode.UseUri)
			{
				// Add the SessionID to the URI. Set the redirected variable as appropriate.

				redirected = true;
				return;
			}
			else
			{
				context.Response.Cookies.Add(new HttpCookie(pConfig.CookieName, id));
				cookieAdded = true;
			}
		}
	}
}
