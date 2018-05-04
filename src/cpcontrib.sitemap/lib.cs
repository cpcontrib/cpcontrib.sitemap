using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrownPeak.CMSAPI;
using CrownPeak.CMSAPI.Services;
/* Some Namespaces are not allowed. */
namespace /*!packer:LibraryNamespace*/CrownPeak.CMSAPI.CustomLibrary
{

}

/***packed_BOF:SitemapUtils.cs***/
namespace CPContrib.SiteMap
{
	using CPContrib.Core;
	using CrownPeak.CMSAPI.CustomLibrary;
	using System.Text.RegularExpressions;

	public class SitemapUtils
	{

		public static Regex PathspecToRegex(string pattern)
		{
			if(pattern.StartsWith("regex:") == true)
			{
				return new Regex(pattern);
			}
			else
			{
				return WildcardToRegex(pattern);
			}
		}

		public static Regex WildcardToRegex(string pattern)
		{
			string regexstr = "^"
				+ (Regex.Escape(pattern).
					Replace("\\*", ".*").
					Replace("\\?", "."))
				+ "$";

			return new Regex(regexstr, RegexOptions.IgnoreCase);
		}

		/// <summary>
		/// Attempts to return count of items from a given IEnumerable collection.  Returns -1 if unable to determine
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static int SafeCount(System.Collections.IEnumerable source, int defaultCount = -1)
		{
			if(source.GetType().IsArray)
			{
				Array sourcearray = source as Array;
				return sourcearray.Length;
			}

			System.Collections.ICollection collection = source as System.Collections.ICollection;
			if(collection != null)
			{
				return collection.Count;
			}

			//unable to determine if safe to count
			return defaultCount;
		}

		public static IEnumerable<string> FilterComments(IEnumerable<string> lines)
		{
			//int linecount = SitemapUtils.SafeCount(lines);
			int linecount = 10;
			List<string> retval = new List<string>(linecount);

			foreach(var lineOrig in lines)
			{
				string line = lineOrig;

				int indexOfComment = lineOrig.IndexOf("#");

				if(indexOfComment > 1)
					line = lineOrig.Substring(0, indexOfComment - 1);
				else if(indexOfComment == 0)
					line = "";

				retval.Add(line);
				//yield return line;
			}

			return retval;
		}

		public static IEnumerable<TemplateRef> GetTemplateRefs(string[] templatePaths)
		{
			var items = new List<TemplateRef>(templatePaths.Length);

			foreach(var templatePath in templatePaths)
			{
				int id;
				Asset templateFolder;
				if(int.TryParse(templatePath, out id) == true)
				{
					templateFolder = Asset.Load(id);
				}
				else
				{
					templateFolder = Asset.Load(templatePath);
				}

				if(templateFolder.IsLoaded)
				{
					items.Add(new TemplateRef(templateFolder.AssetPath.ToString(), templateFolder.Id));
				}

			}

			return items;
		}

		public static void ExcludeTemplates(FilterParams fp, IEnumerable<TemplateRef> templateRefs)
		{
			fp.Add(AssetPropertyNames.TemplateId, Comparison.NotInSet, templateRefs.Select(_ => _.TemplateId).ToList());
		}
		public static void ExcludeTemplates(FilterParams fp, IEnumerable<int> templateRefs)
		{
			fp.Add(AssetPropertyNames.TemplateId, Comparison.NotInSet, templateRefs.ToList());
		}

		/// <summary>
		/// Splits the input into an IEnumerable&lt;string&gt;
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		internal static IEnumerable<string> SplitMultilineInput(string value, bool filterComments = true)
		{
			IEnumerable<string> split = value.Replace("\r\n", "\n").Split('\n');

			if(filterComments)
			{
				split = SitemapUtils.FilterComments(split);
			}

			return split;
		}
	}


}

/***packed_BOF:UrlBuilder.cs***/
namespace CPContrib.SiteMap
{
	using System.Runtime.Serialization;
	using System.Text.RegularExpressions;

	public class UrlBuilder
	{
		public string Loc { get; set; }

		private DateTime _lastMod;
		public DateTime LastMod
		{
			set { this._lastMod = value.Date; }
			get { return this._lastMod; }
		}
		public string changefreq { get; set; }
		public string priority { get; set; }

		public Asset Asset { get; set; }

		public CPContrib.SiteMap.Serialization.url Create()
		{
			var url = new CPContrib.SiteMap.Serialization.url();

			url.loc = this.Loc;
			url.lastmod = this.LastMod.ToString("yyyy-MM-dd");
			url.changefreq = this.changefreq;
			url.priority = this.priority;

			return url;
		}
	}

	public class UrlMetaEntry
	{
		public string PathSpec { get; set; }
		public Regex PathSpecRegex { get; set; }
		public CPContrib.SiteMap.Serialization.UrlMeta Meta { get; set; }
	}


}

/***packed_BOF:SitemapBuilder.cs***/
namespace CPContrib.SiteMap
{
	using System.Text.RegularExpressions;
	using Constants = CPContrib.SiteMap.SitemapConstants; //removes an ambiguity that might arise from using another namespace

	public static class SitemapConstants
	{
		public static class FieldNames
		{
			//these would be used in various asset templates
			public const string Sitemap_Priority = "!sitemap_priority";
			public const string Sitemap_ChangeFreq = "!sitemap_changefreq";
			public const string Sitemap_Include = "!sitemap_include";

			//used by Sitemap.Input template
			public const string SitemapInput_IgnoredPaths = "ignored_paths";
		}
	}

	/// <summary>
	/// SitemapBuilder is a fluent builder for collecting assets from CrownPeak.
	/// </summary>
	public class SitemapBuilder
	{

		public SitemapBuilder AddRoot(string assetpath)
		{
			this._Roots.Add(assetpath);
			return this;
		}

		List<string> _Roots = new List<string>();
		List<Regex> _IgnoredPaths = new List<Regex>();
		List<Asset> _Assets = new List<Asset>(5000);

		/// <summary>
		/// Adds a list of ignored paths anytime new assets are introduced into the current instance.
		/// </summary>
		/// <param name="source">an asset or panel to read values from</param>
		/// <param name="field">field to read</param>
		/// <returns></returns>
		public SitemapBuilder AddIgnoredPaths(CrownPeak.CMSAPI.CustomLibrary.IFieldAccessor source, string field = Constants.FieldNames.SitemapInput_IgnoredPaths)
		{
			var ignoredPathsRegexArray =
				SitemapUtils.SplitMultilineInput(source.Raw[field])
				.Select(_ => CPContrib.SiteMap.SitemapUtils.PathspecToRegex(_)).ToArray();

			this._IgnoredPaths.AddRange(ignoredPathsRegexArray);

			return this;
		}

		/// <summary>
		/// Adds Assets based on provided FilterParams.  Warning: this method will add necessary options and cause the passed-in FilterParams to mutate.
		/// </summary>
		/// <param name="asset"></param>
		/// <param name="fp"></param>
		/// <returns></returns>
		public SitemapBuilder AddFilteredList(Asset folder, FilterParams fp)
		{
			if(folder == null) throw new ArgumentNullException("folder");
			if(folder.IsLoaded == false) throw new ArgumentException("folder parameter is an asset that is not loaded properly.", "folder");

			List<string> fieldnames;
			if(fp.FieldNames == null && fp.FieldNames.Count > 0)
			{
				fieldnames = fp.FieldNames;
			}
			else
			{
				fieldnames = fp.FieldNames = new List<string>();
			}

			if(fieldnames.Contains(Constants.FieldNames.Sitemap_ChangeFreq)==false)
				fieldnames.Add(Constants.FieldNames.Sitemap_ChangeFreq);
			if(fieldnames.Contains(Constants.FieldNames.Sitemap_Priority) == false)
				fieldnames.Add(Constants.FieldNames.Sitemap_Priority);

			var assetlist = folder.GetFilterList(fp);
			this._AddAssets(assetlist);

			return this;
		}

		/// <summary>
		/// Adds specified assets.
		/// </summary>
		/// <param name="assetList"></param>
		/// <returns></returns>
		public SitemapBuilder AddAssets(IEnumerable<Asset> assetList)
		{
			if(assetList != null)
			{
				_AddAssets(assetList.Where(asset => asset.IsLoaded == true));
			}

			return this;
		}

		/// <summary>
		/// Add the given list of assets after scanning for any ignored paths
		/// </summary>
		/// <param name="assetList"></param>
		private void _AddAssets(IEnumerable<Asset> assetList)
		{
			if(assetList == null) throw new ArgumentNullException("assetList");

			//run assetList through ignored paths
			assetList = _FilterAssets(assetList);


			ICollection<Asset> countableList = assetList as ICollection<Asset>;

			if(countableList == null)
			{
				countableList = new List<Asset>(assetList);
			}

			//grow by 5000 items
			if(this._Assets.Count + countableList.Count > this._Assets.Capacity)
				this._Assets.Capacity += 5000;

			this._Assets.AddRange(countableList);
		}

		private IEnumerable<Asset> _FilterAssets(IEnumerable<Asset> assetList)
		{
			List<Asset> filteredList = new List<Asset>();

			foreach(var asset in assetList)
			{
				bool ignored = false;
				string pathstr = asset.AssetPath.ToString();

				foreach(var ignoredPath in _IgnoredPaths)
				{
					if(ignoredPath.IsMatch(pathstr))
					{
						//ignoreList.Add(new Tuple<string, string>(currentAsset.AssetPath.ToString(), ignoredPath.ToString()));
						//Out.DebugWriteLine("Ignoring asset '{0}' due to ignored path: '{1}'.", currentAsset.AssetPath, ignoredPath);
						ignored = true;
						continue;
					}

				}

				if(ignored == false)
					filteredList.Add(asset);
			}

			return filteredList;
		}


	}
}


namespace CPContrib.SiteMap
{
	public enum ChangeFrequency
	{
		Unspecified = 0,
		Daily = 1,
		Weekly = 2,
		Monthly = 3
	}
}


/***packed_BOF:Serialization/url.cs***/
#region SiteMap.Serialization
namespace CPContrib.SiteMap.Serialization
{
	using System.Runtime.Serialization;
	using System.ComponentModel;

	[DataContract]
	public class UrlMeta
	{
		[DataMember]
		public string changefreq { get; set; }
		[DataMember]
		public float priority { get; set; }
	}

	/// <summary>
	/// url element is a child of urlset
	/// </summary>
	[DataContract(Namespace = "schema.org/sitemap")]
	public class url
	{
		[DataMember(Order = 0)]
		public string loc { get; set; }

		[DataMember(Order = 1)]
		[DefaultValue((string)null)]
		public string lastmod { get; set; }
		public DateTime lastmod_DateTime
		{
			set { this.lastmod = value.ToString("yyyy-MM-dd"); }
			get { return DateTime.Parse(this.lastmod); }
		}
		[DataMember(EmitDefaultValue = false)]
		[DefaultValue((string)null)]
		public string changefreq { get; set; }

		[DataMember(EmitDefaultValue = false)]
		[DefaultValue((string)null)]
		public string priority { get; set; }
	}

	/// <summary>
	/// A sitemap element within sitemapindex element
	/// </summary>
	[System.Runtime.Serialization.DataContract(Name = "sitemap")]
	public class sitemapRef
	{
		public string loc { get; set; }

		public string lastmod { get; set; }

	}

	//[CollectionDataContract(Name="sitemapindex", ItemName="sitemap", Namespace = "")]
	//public class sitemapindex : List<sitemapRef>
	//{

	//}

	//[CollectionDataContract(Name = "urlset", ItemName = "url")]
	//public class urlset : List<url>
	//{

	//}
}

#endregion

/***packed_BOF:SearchEngines/SitemapPing.cs***/
namespace CPContrib.SiteMap
{
	using CPContrib.SiteMap.SearchEngines;

	public class SitemapsPinger
	{
		private CPLog.ILogger Log;

		public SitemapsPinger(CPLog.ILogger Logger = null)
		{
			Log = Logger == null ? CPLog.LogManager.GetCurrentClassLogger() : Logger;

		}


		public void Ping(string sitemapUrl)
		{
			Log.Info("Ping search engines with sitemapUrl='{0}'.", sitemapUrl);

			//need some DI here
			List<ISitemapPing> providers = new List<SearchEngines.ISitemapPing>()
			{
				new GoogleSitemapPing(Log),
				new BingSitemapPing(Log)
			};

			//loop through providers and invoke their sitemap ping API
			foreach(var isitemapping in providers)
			{
				Log.Info("Invoking '{0}' and calling Ping.", isitemapping.GetType().Name);
				isitemapping.Ping(sitemapUrl);
			}
		}

	}
}
/***packed_EOF:SearchEngines/SitemapPing.cs***/

#region SiteMap.SearchEngines
/***packed_BOF:SearchEngines/ISitemapPing.cs***/
/***packed_region:SiteMap.SearchEngines***/
namespace CPContrib.SiteMap.SearchEngines
{

	public interface ISitemapPing
	{
		void Ping(string sitemapUrl);
	}
}
/***packed_EOF:SearchEngines/ISitemapPing.cs***/

/***packed_BOF:SearchEngines/GoogleSitemapPing.cs***/
/***packed_region:SiteMap.SearchEngines***/
namespace CPContrib.SiteMap.SearchEngines
{

	public class GoogleSitemapPing : ISitemapPing
	{
		private CPLog.ILogger Log;

		public GoogleSitemapPing(CPLog.ILogger Logger = null)
		{
			Log = Logger == null ? CPLog.LogManager.GetCurrentClassLogger() : Logger;
		}

		public void Ping(string sitemapUrl)
		{
			string url = "http://google.com/ping?sitemap={sitemapUrl}".Replace("{sitemapUrl}", sitemapUrl);
			//string url = $"http://google.com/ping?sitemap={sitemapUrl}";

			Log.Debug(() => string.Format("HttpGet to '{0}'.", url));

			Util.GetHttp(url);
		}

	}
}
/***packed_EOF:SearchEngines/GoogleSitemapPing.cs***/

/***packed_BOF:SearchEngines/BingSitemapPing.cs***/
/***packed_region:SiteMap.SearchEngines***/
namespace CPContrib.SiteMap.SearchEngines
{
	public class BingSitemapPing : ISitemapPing
	{
		private CPLog.ILogger Log;

		public BingSitemapPing(CPLog.ILogger Logger = null)
		{
			Log = Logger == null ? CPLog.LogManager.GetCurrentClassLogger() : Logger;
		}

		public void Ping(string sitemapUrl)
		{
			string url = "http://bing.com/ping?sitemap={sitemapUrl}".Replace("{sitemapUrl}", sitemapUrl);
			//string url = $"http://bing.com/ping?sitemap={sitemapUrl}";

			Log.Debug(() => string.Format("HttpGet to '{0}'.", url));

			Util.GetHttp(url);
		}

	}
}
/***packed_EOF:SearchEngines/BingSitemapPing.cs***/
#endregion