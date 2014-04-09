using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Translation
{
	/// <summary>
	/// Summary description for Translate.
	/// </summary>
	[WebService(Namespace="http://www.redgreenyellowbluegreenpinkpurplewhite.com/Translation/")]
	public class Translate : System.Web.Services.WebService
	{
		readonly string[] VALIDTRANSLATIONMODES = new string[] {"en_zh", "en_fr", "en_de", "en_it", "en_ja", "en_ko", "en_pt", "en_es", "zh_en", "fr_en", "fr_de", "de_en", "de_fr", "it_en", "ja_en", "ko_en", "pt_en", "ru_en", "es_en"};	
		const string BABELFISHURL = "http://babelfish.altavista.com/babelfish/tr";
		const string BABELFISHREFERER = "http://babelfish.altavista.com/";
		const string ERRORSTRINGSTART = "<font color=red>";
		const string ERRORSTRINGEND = "</font>";

		public Translate()
		{
			//CODEGEN: This call is required by the ASP.NET Web Services Designer
			InitializeComponent();
		}

		#region Component Designer generated code
		
		//Required by the Web Services Designer 
		private IContainer components = null;
				
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);		
		}
		
		#endregion

		[WebMethod]
		public string BabelFish(string translationmode, string sourcedata) 
		{
			try 
			{
				// validate and remove trailing spaces
				if (translationmode == null || translationmode.Length == 0) throw new ArgumentNullException("translationmode");
				if (sourcedata == null || translationmode.Length == 0) throw new ArgumentNullException("sourcedata");
				translationmode = translationmode.Trim();
				sourcedata = sourcedata.Trim();
				// check for valid translationmodes
				bool validtranslationmode = false;
				for (int i = 0; i < VALIDTRANSLATIONMODES.Length; i++) 
				{
					if (VALIDTRANSLATIONMODES[i] == translationmode) 
					{
						validtranslationmode = true;
						break;
					}
				}
				if (!validtranslationmode) 				return ERRORSTRINGSTART + "The translationmode specified was not a valid translation translationmode" + ERRORSTRINGEND;
				Uri uri = new Uri(BABELFISHURL);
				HttpWebRequest request = (HttpWebRequest) WebRequest.Create(uri);
				request.Referer = BABELFISHREFERER;
				// Encode all the sourcedata 
				string postsourcedata;
				postsourcedata = "lp=" + translationmode + "&tt=urltext&intl=1&doit=done&urltext=" + HttpUtility.UrlEncode(sourcedata);
				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded";
				request.ContentLength = postsourcedata.Length;
				request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)";
				Stream writeStream = request.GetRequestStream();
				UTF8Encoding encoding = new UTF8Encoding();
				byte[] bytes = encoding.GetBytes(postsourcedata);
				writeStream.Write(bytes, 0, bytes.Length);
				writeStream.Close();
				HttpWebResponse response = (HttpWebResponse) request.GetResponse();
				Stream responseStream = response.GetResponseStream();
				StreamReader readStream = new StreamReader (responseStream, Encoding.UTF8);
				string page = readStream.ReadToEnd();
				Regex reg = new Regex(@"<div style=padding:10px;>((?:.|\n)*?)</div>", RegexOptions.IgnoreCase);
				MatchCollection matches = reg.Matches(page);
				if (matches.Count != 1 || matches[0].Groups.Count != 2) 
				{
					return ERRORSTRINGSTART + "The HTML returned from Babelfish appears to have changed. Please check for an updated regular expression" +  ERRORSTRINGEND;
				}
				return matches[0].Groups[1].Value;
			}
			catch (ArgumentNullException ex) 
			{
				return ERRORSTRINGSTART + ex.Message + ERRORSTRINGEND;
			}
				catch (ArgumentException ex) {
				return ERRORSTRINGSTART + ex.Message + ERRORSTRINGEND;
				}
			catch (WebException) {
				return ERRORSTRINGSTART + "There was a problem connecting to the Babelfish server" + ERRORSTRINGEND;
			}
			catch (System.Security.SecurityException) 
			{
				return ERRORSTRINGSTART + "Error: you do not have permission to make HTTP connections. Please check your assembly's permission settings" +ERRORSTRINGEND;
			}
			catch (Exception ex)
			{
				return ERRORSTRINGSTART + "An unspecified error occured: " + ex.Message + ERRORSTRINGEND;
			}
		}
	}
}
