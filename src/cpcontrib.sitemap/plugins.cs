using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrownPeak.CMSAPI;
using CrownPeak.CMSAPI.Services;
/* Some Namespaces are not allowed. */
//using TemplateRef = CrownPeak.CMSAPI.CustomLibrary.TemplateRef;
using TemplateRef = CPContrib.Core.TemplateRef;

#region Template:Sitemap

namespace CPContrib.SiteMap.Templates
{
	using CrownPeak.CMSAPI.CustomLibrary;
	using CPContrib.SiteMap;
	using System.Text.RegularExpressions;

	public class SitemapInputOptions
	{

	}

	public class Sitemap_Input
	{
		public Sitemap_Input(Asset asset, InputContext context = null)
		{
			this.asset = asset;
		}
		private Asset asset;

		/// <summary>
		/// Gets list of template ids from the exclude_template_list field(s)
		/// </summary>
		/// <returns></returns>
		public IEnumerable<int> GetExcludedTemplateIds()
		{
			var panels = this.asset.GetPanels("exclude_template_list");

			return panels.Select(_ => int.Parse(_.Raw["exclude_template_list"])).ToList();
		}

		public IEnumerable<UrlMetaEntry> GetOverrides(PanelEntry panel)
		{
			var input = SitemapUtils.SplitMultilineInput(panel.Raw["sm_overrides"]);

			return _ParseOverrides(input);
		}

		public IEnumerable<UrlMetaEntry> GetDefaults(PanelEntry panel)
		{
			var input = SitemapUtils.SplitMultilineInput(panel.Raw["sm_defaults"]);

			return _ParseOverrides(input);
		}

		internal IEnumerable<UrlMetaEntry> _ParseOverrides(IEnumerable<string> input)
		{
			var parsedOverrides = new List<UrlMetaEntry>();

			Regex r = new Regex(@"(.*)\s=>\s(.*)");

			foreach(var line in input)
			{
				var m = r.Match(line);
				if(m.Success)
				{
					var overrideEntry = new UrlMetaEntry()
					{
						PathSpec = m.Groups[1].Value,
						PathSpecRegex = SitemapUtils.PathspecToRegex(m.Groups[1].Value),
						Meta = (CPContrib.SiteMap.Serialization.UrlMeta)Util.DeserializeDataContractJson(m.Groups[2].Value, typeof(CPContrib.SiteMap.Serialization.UrlMeta))
					};
					parsedOverrides.Add(overrideEntry);
				}
			}

			return parsedOverrides;
		}

		/// <summary>
		/// Retrieves ignored paths from 'ignored_paths' field
		/// </summary>
		/// <param name="siteroot_panel"></param>
		/// <returns></returns>
		public IEnumerable<Regex> GetIgnoredPaths(PanelEntry siteroot_panel)
		{
			var regex_list = 
				SitemapUtils.SplitMultilineInput(siteroot_panel.Raw["ignored_paths"])
				.Select(_ => CPContrib.SiteMap.SitemapUtils.PathspecToRegex(_)).ToArray();

			return regex_list;
		}

		/// <summary>
		/// Input for configuring page and global-level XML sitemap options.  The resulting content fields can be crawled when creating XML sitemaps
		/// </summary>
		/// <param name="isConfig">Indicates if the current asset is the site configuration asset</param>
		/// <example>
		/// <code lang="C#"><![CDATA[
		/// ServicesInput.ShowSiteMapInput();
		/// ]]></code>
		/// </example>
		public static void ShowSiteMapInput(bool isConfig = false, SitemapInputOptions options = null)
        {
            Input.StartControlPanel("SiteMap Controls");
            Dictionary<string, string> Priorities = new Dictionary<string, string>();
            Dictionary<string, string> Frequency = new Dictionary<string, string>();
            Input.ShowHeader("XML Site Map", null, null, false);
            if (!isConfig)
            {
                Priorities.Add("Global Default", "default");
                Frequency.Add("Global Default", "default");
                ServicesInput.populateDictionaries(Priorities, Frequency);
                Input.ShowMessage("<strong>Used when compiling XML Sitemaps</strong>.  Changes override Global Configuration defaults.", null, null);
                bool? nullable = null;
                Input.ShowCheckBox("Exclude", "!sitemap_exclude", "true", "Check to exclude this page from XML Sitemap.", "", "", nullable, "", false);
                Input.ShowDropDown("Priority", "!sitemap_priority", Priorities, Util.MakeList(new string[] { "default" }), -1, false, "Set only if you wish to override default.", "");
                Input.ShowDropDown("Change Frequency", "!sitemap_changefreq", Frequency, Util.MakeList(new string[] { "default" }), -1, false, "Set only if you wish to override default.", "");
            }
            else
            {
                Input.ShowMessage("<strong>XML Site Map</strong> - used when compiling XML sitemaps.  This panel sets Global Configuration defaults.", null, null);
                ServicesInput.populateDictionaries(Priorities, Frequency);
                Input.ShowDropDown("Priority", "!sitemap_priority", Priorities, Util.MakeList(new string[] { "0.5" }), -1, false, "Select Default Priority.", "");
                Input.ShowDropDown("Change Frequency", "!sitemap_changefreq", Frequency, Util.MakeList(new string[] { "unspecified" }), -1, false, "Select Default Change Frequency.", "");
            }
            Input.EndControlPanel();
        }

		public void OnInput()
		{
			Input.StartControlPanel("Sitemap");
			{
				while(Input.NextPanel("sitemap_roots"))
				{

					Input.ShowTextBox("Sitemap Name", "sitemap_name", "sitemap", helpMessage: "Must use different name if included within a sitemap index file.");

					Input.ShowTextBox("Root Folder", "sitemap_root", helpMessage: "Enter the root folder to build a sitemap");

					var sitemapinput = new CPContrib.SiteMap.Templates.Sitemap_Input(asset);
					sitemapinput.CreateExcludeTemplateList();

					//Input.ShowTextBox("Ignored Templates", "ignored_templates", height: 9);

					const string SUPPORTS_COMMENTS = "Supports comments starting with '#', rest of line is ignored.";
					const string EXAMPLE_SPEC = "Example: /Folder/subfolder/* => { \"changefreq\":\"monthly\", \"priority\":0.5 }";

					Input.ShowTextBox("Ignored paths:", "ignored_paths", height: 5,
						helpMessage: String.Join("\n", "Example: /Folder/subfolder/*", SUPPORTS_COMMENTS)
					);

					Input.ShowTextBox("Defaults:", "sm_defaults", height: 5,
						helpMessage: String.Join("\n", EXAMPLE_SPEC, SUPPORTS_COMMENTS),
						popupMessage: "Defaults are used when asset doesnt have any sitemap meta-data specified.  Defaults are applied first.");

					Input.ShowTextBox("Overrides:", "sm_overrides", height: 5,
						helpMessage: String.Join("\n", EXAMPLE_SPEC, SUPPORTS_COMMENTS),
						popupMessage: "Overrides are applied last.");
					//Each override is a pathspec separated by ' => ' with a json object: { changefreq: 'monthly', priority: 0.5 } ");
				}

				if(asset["sitemap_usedbyindex"] != "")
				{
					Input.ShowMessage("This sitemap is being used by a sitemapindex");
					Input.ShowLink(Asset.Load(asset["sitemap_usedbyindex"]));
				}
			}
			Input.EndControlPanel();
		}

		public void CreateExcludeTemplateList()
		{
			Asset siteroot = Asset.GetSiteRoot(this.asset);
			Asset templatesRoot;

			if(string.IsNullOrEmpty(siteroot.Raw[""]))
			{
				templatesRoot = Asset.Load("/System/Templates");
			}
			else
			{
				throw new NotImplementedException();
			}

			FilterParams fp = new FilterParams();
			fp.ExcludeProjectTypes = false;
			fp.Add(Comparison.Equals, AssetType.Folder);

			var templates = templatesRoot.GetFilterList(fp);

			var entries = new Dictionary<string, string>(templates.Count);

			foreach(var tpl in templates.OrderBy(_=>_.AssetPath.ToString()))
			{
				string title = string.Format("{0} ({1})", tpl.Label, tpl.AssetPath.GetParent());
				entries.Add(title, tpl.Id.ToString());
				
			}

			Input.ShowSelectList("Exclude Templates", "exclude_template_list", entries, size:10);
		}

		#region PostInput
		public void PostInput(PostInputContext context)
		{

		}
		#endregion

		#region PostSave
		public void PostSave(PostSaveContext context)
		{
			try
			{
				foreach(var panel in asset.GetPanels("sitemap_roots"))
				{
					var templateRefs = GetTemplateRefs(panel);
					string fieldname = panel.GetFieldName("exclude_template_ids");

					asset.SaveContentField(fieldname, String.Join(",", templateRefs.Select(_ => _.TemplateId.ToString())));
				}
			} 
			catch(Exception ex)
			{
				Util.Log(asset, "Failed to run post_save: " + ex.ToString().Replace("{","{{"));
				throw;
			}
		}
		#endregion

		IEnumerable<TemplateRef> GetTemplateRefs(PanelEntry panel)
		{
			var input = SitemapUtils.SplitMultilineInput(panel.Raw["exclude_templates"]);
			var templateRefs = GetTemplateRefs(input);
			return templateRefs;
		}

		IEnumerable<TemplateRef> GetTemplateRefs(IEnumerable<string> input)
		{
			List<TemplateRef> items = new List<TemplateRef>();

			foreach(var templateidStr in SitemapUtils.FilterComments(input))
			{
				
				try
				{
					int templateId;
					if(int.TryParse(templateidStr, out templateId))
					{
						var templateFolder = Asset.Load(templateId);
						if(templateFolder.IsLoaded)
						{
							items.Add(new TemplateRef(templateFolder.AssetPath.ToString(), templateFolder.Id));
						}
					}
				}
				catch(Exception ex)
				{
					throw new ApplicationException(string.Format("Failed while processing '{0}': {1}", templateidStr, ex.Message), ex);
				}
			}

			return items;
		}


	}
	
	public class Sitemap_Output //Sitemap_Output: ITemplate_Output
	{
		public Sitemap_Output(Func<IList<UrlBuilder>> SitemapBuilderFunc)
		{
			this.SitemapBuilderFunc = SitemapBuilderFunc;
		}

		Func<IList<UrlBuilder>> SitemapBuilderFunc;

		/// <summary>
		/// Forces writing all urls as https
		/// </summary>
		public bool ForceHttps { get; set; }

		public void OnOutput(Asset asset, OutputContext context)
		{
			context.IsGeneratingDependencies = false; //dont generate dependencies that sitemap is referring to (could cause a complete sitewide publish)
			context.RenderPublishLinks = true;  //we want links to be the final version, not an internal version

			//make sure this asset gets marked as child depedency on the sitemapindex when present
			if(asset.Raw["sitemap_usedbyindex"] != "")
			{
				Asset.Load(asset.Raw["sitemap_usedbyindex"]).AddDependencyTo(asset);
			}


			IList<UrlBuilder> result = null;

			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
			sw.Start();

			try { result = SitemapBuilderFunc(); }
			catch(Exception ex)
			{
				throw new ApplicationException("SitemapBuilderFunc failed.", ex);
			}

			sw.Stop();

			Util.Log(asset, "Last run finished {0} and lasted {1}", DateTime.UtcNow.ToString("O"), sw.Elapsed.ToString("c"));
		}

	}

	public class Sitemap_PostPublish // : ITemplate_PostPublish
	{
		private CPLog.ILogger Log;

		public Sitemap_PostPublish(CPLog.ILogger Logger)
		{
			this.Log = (Logger == null ? CPLog.LogManager.GetCurrentClassLogger() : Logger);
		}

		public void OnPostPublish(Asset asset, PostPublishContext context)
		{
			Log.Info("Beginning post_publish");

			Asset sitemapAsset = asset;
			if(asset.Raw["sitemap_usedbyindex"] != "")
			{
				sitemapAsset = Asset.Load(asset.Raw["sitemap_usedbyindex"]);
			}

			string sitemapUrl = sitemapAsset.GetLink(addDomain: true, protocolType: ProtocolType.Https);

			var Logger = new CrownPeak.CMSAPI.CustomLibrary.UtilLogLogger("SitemapsPinger", asset);

			var sitemapsPinger = new CPContrib.SiteMap.SitemapsPinger(Log);
			sitemapsPinger.Ping(sitemapUrl);

			Logger.Flush();
		}
	}

}
#endregion

namespace CPContrib.SiteMap.Templates
{
	using System.Text.RegularExpressions;
	using CrownPeak.CMSAPI.CustomLibrary;
	using CPContrib.Core;

	public class SitemapOutputBuilder
	{
		public SitemapOutputBuilder(Asset asset, OutputContext context)
		{
			this.asset = asset;
			this.context = context;
			this.AssignMetaFunc = AssignMeta;
			this.IgnoreAssetFunc = DecideIgnoreAsset;
		}
		Asset asset;
		OutputContext context;

		public bool ForceHttps;

		/// <summary>
		/// Generates a list of sitemap url elements for the given list of urlbuilder instances.  Use this within a file to keep outputting urls
		/// </summary>
		/// <param name="sitemapList"></param>
		/// <param name="isPublishing"></param>
		/// <returns></returns>
		public string GetSitemapXmlChunk(IEnumerable<UrlBuilder> sitemapList, bool isPublishing = true)
		{
			var writer = new XmlTextWriter(
				new System.Xml.XmlWriterSettings() {
					Indent = true,
					IndentChars = "\t",
					ConformanceLevel = System.Xml.ConformanceLevel.Fragment
				}
			);

	//		if(WritingFull)
	//		{
	//			writer.WriteStartDocument(standalone: true);

	//			//writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
	//			writer.WriteRaw(@"<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"" 
	//xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
	//xsi:schemalocation=""http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd"">");
	//		}

			foreach(var urlBuilder in sitemapList)
			{
				var url = urlBuilder.Create();

				writer.WriteStartElement("url");
				{
					writer.WriteStartElement("loc");
					if (isPublishing == false)
					{
						writer.WriteString(string.Format("CMS Path: {0}", urlBuilder.Asset.AssetPath));
					}
					else
					{
						if(ForceHttps)
						{
							if(url.loc.StartsWith("http://"))
								url.loc = "https://" + url.loc.Substring(7);
						}
						writer.WriteString(url.loc);
					}
					writer.WriteEndElement(); //loc

					writer.WriteStartElement("lastmod");
					writer.WriteString(url.lastmod);
					writer.WriteEndElement();

					writer.WriteStartElement("changefreq");
					writer.WriteString(url.changefreq);
					writer.WriteEndElement();

					writer.WriteStartElement("priority");
					writer.WriteString(url.priority);
					writer.WriteEndElement();
				}
				writer.WriteEndElement();//url
			}

			//if(WritingFull)
			//{
			//	//writer.WriteEndElement("urlset");
			//	writer.WriteRaw("</urlset>");
			//}

			return writer.ToString();
		}

		private IEnumerable<UrlMetaEntry> _defaults;
		private IEnumerable<UrlMetaEntry> _overrides;
		public SitemapOutputBuilder SetDefaults(IEnumerable<UrlMetaEntry> defaults)
		{
			this._defaults = defaults;
			return this;
		}
		public SitemapOutputBuilder SetOverrides(IEnumerable<UrlMetaEntry> overrides)
		{
			this._overrides = overrides;
			return this;
		}

		List<Asset> _assets;
		public SitemapOutputBuilder AddAssets(IEnumerable<Asset> assets)
		{
			if(assets != null)
			{
				if(this._assets == null) _assets = new List<Asset>();
				_assets.AddRange(assets);
			}
			return this;
		}

		List<Regex> _ignoredPaths;
		public SitemapOutputBuilder AddIgnoredPaths(IEnumerable<Regex> ignoredPaths)
		{
			if(ignoredPaths != null)
			{
				if(this._ignoredPaths == null) this._ignoredPaths = new List<Regex>();
				this._ignoredPaths.AddRange(ignoredPaths);
			}
			return this;
		}

		public Func<Asset, bool> IgnoreAssetFunc;
		public bool DecideIgnoreAsset(Asset currentAsset)
		{
			string pathstr = currentAsset.AssetPath.ToString();

			bool ignored = false;

			if(this._ignoredPaths != null)
			{
				foreach(var ignoredPath in this._ignoredPaths)
				{
					if(ignoredPath.IsMatch(pathstr))
					{
						//ignoreList.Add(new Tuple<string, string>(currentAsset.AssetPath.ToString(), ignoredPath.ToString()));
						Out.DebugWriteLine("Ignoring asset '{0}' due to ignored path: '{1}'.", currentAsset.AssetPath, ignoredPath);
						ignored = true;
						continue;
					}

				}
			}

			//if (path[2] != asset.AssetPath[2])
			//{
			//    Out.DebugWriteLine("Ignoring asset '{0}' due to unexpected language mismatch: '{1}'.", currentAsset.AssetPath, asset.AssetPath[2]);
			//    ignored = true;
			//}

			//FilterParams is returning items with empty workflow
			if(currentAsset.WorkflowStatus.Name != context.PublishingStatus.Name)
			{
				Out.DebugWriteLine("Ignoring asset '{0}' due to differing Workflow Status: '{1}'.", currentAsset.AssetPath, currentAsset.WorkflowStatus.Name);
				ignored = true;
			}

			return ignored;
		}

		public IEnumerable<CPContrib.SiteMap.UrlBuilder> ProcessList(Status currentStatus)
		{
			var sitemapList = new List<CPContrib.SiteMap.UrlBuilder>();

			int count = 0;
			IEnumerable<Asset> assetList = this._assets ?? new List<Asset>();

			foreach (Asset currentAsset in assetList)
			{
				bool ignored = this.IgnoreAssetFunc(currentAsset);

				if (ignored == false)
				{
					var url = new CPContrib.SiteMap.UrlBuilder();

					url.Asset = currentAsset;

					string link = currentAsset.GetLink(addDomain: true, protocolType:ProtocolType.Https);

					if (!string.IsNullOrEmpty(link))
					{
						url.Loc = link;
						url.LastMod = url.Asset.ModifiedDate.GetValueOrDefault();

						string assetpathStr = url.Asset.AssetPath.ToString();

						var defaultEntry = GetDefaultEntry(assetpathStr);
						var overrideEntry = GetOverrideEntry(assetpathStr);

						//call function assigned to AssignPropertiesFunc
						this.AssignMetaFunc(url, defaultEntry, overrideEntry);

						if(url.changefreq == SitemapConstants.Tiered_LastMod)
						{
							this.Tiered_LastMod_ChangeFreq(url);
						}

						//add to list to output
						sitemapList.Add(url);
					}
				}
			}

			return sitemapList;
		}

		public void Tiered_LastMod_ChangeFreq(UrlBuilder url)
		{
			DateTime now = DateTime.Today;
			var span = now - url.LastMod;

			if(span.TotalDays > 30)
			{
				url.changefreq = "monthly";
				url.priority = "0.6";
			}
			else if(span.TotalDays > 7)
			{
				url.changefreq = "weekly";
				url.priority = "0.7";
			}
			else if(span.TotalDays > 1)
			{
				url.changefreq = "daily";
				url.priority = "0.8";
			}
			else if(span.TotalHours > 1)
			{
				url.changefreq = "hourly";
				url.priority = "0.9";
			}
		}

		protected virtual UrlMetaEntry GetDefaultEntry(string assetpath)
		{
			return _GetEntry(this._defaults, assetpath);
		}

		protected virtual UrlMetaEntry GetOverrideEntry(string assetpath)
		{
			return _GetEntry(this._overrides, assetpath);
		}

		internal UrlMetaEntry _GetEntry(IEnumerable<UrlMetaEntry> sourcecollection, string assetpath)
		{ 
			if(sourcecollection != null)
			{
				foreach(var overrideEntry in sourcecollection)
				{
					if(overrideEntry.PathSpecRegex.IsMatch(assetpath)) return overrideEntry;
				}
			}
			return null;
		}

		public Action<UrlBuilder,UrlMetaEntry,UrlMetaEntry> AssignMetaFunc;
		public virtual void AssignMeta(UrlBuilder url, UrlMetaEntry defaultEntry, UrlMetaEntry overrideEntry)
		{
			//url.priority = LmUtil.EmptyFallback(url.Asset.Raw["xmlsm_priority"], url.Asset.Raw[SitemapConstants.FieldNames.Sitemap_Priority], "");
			url.priority = url.Asset.Raw[SitemapConstants.FieldNames.Sitemap_Priority];

			if(string.IsNullOrEmpty(url.priority))
			{
				if(defaultEntry != null && defaultEntry.Meta != null)
				{
					url.priority = defaultEntry.Meta.priority.ToString("0.0");
				}
			}

			if(overrideEntry != null && overrideEntry.Meta != null)
			{
				url.priority = overrideEntry.Meta.priority.ToString("0.0");
			}

			//url.changefreq = LmUtil.EmptyFallback(url.Asset.Raw["xmlsm_changefreq"], url.Asset.Raw[SitemapConstants.FieldNames.Sitemap_ChangeFrequency], "");
			url.changefreq = url.Asset.Raw[SitemapConstants.FieldNames.Sitemap_ChangeFreq];

			if(string.IsNullOrEmpty(url.changefreq))
			{
				if(defaultEntry != null && defaultEntry.Meta != null)
				{
					url.changefreq = defaultEntry.Meta.changefreq;
				}
			}

			if(overrideEntry != null && overrideEntry.Meta != null)
			{
				url.changefreq = overrideEntry.Meta.changefreq ?? "unspecified";
			}
		}

	}

}

#region Template:SitemapIndex
namespace CPContrib.SiteMap.Templates
{
	public class SitemapIndex_PostPublish //: ITemplate_PostPublish
	{
		public void OnPostPublish(Asset asset, PostPublishContext context)
		{
			Asset sitemapAsset = asset;
			if(asset.Raw["sitemap_usedbyindex"] != "")
			{
				sitemapAsset = Asset.Load(asset.Raw["sitemap_usedbyindex"]);
			}

			var Logger = new CrownPeak.CMSAPI.CustomLibrary.UtilLogLogger("SitemapsPinger", asset);

			string sitemapUrl = sitemapAsset.GetLink(addDomain: true, protocolType: ProtocolType.Https);
			var sitemapsPinger = new CPContrib.SiteMap.SitemapsPinger(Logger);
			sitemapsPinger.Ping(sitemapUrl);

			Logger.Flush();
		}
	}

	public class SitemapIndex_Input // : ITemplate_Input
	{
		public void OnInput(Asset asset, InputContext context)
		{
			//generate control panel to contain List Panel and Checkbox in one panel in Volte
			Input.StartTabbedPanel("Sitemap Roots", "Options");
			{

				while(Input.NextPanel("sitemap_roots"))
				{

					Input.ShowAcquireDocument("Included Sitemap", "sitemap_asset", helpMessage: "Select a Sitemap asset to include within the index");


				}
			}
			Input.NextTabbedPanel();
			{
				Input.ShowCheckBox("Force HTTPS for sitemap links", "sitemap_force_https", "true", "Force HTTPS for links",
					helpMessage: "Check this to force links generated to the sitemaps within the index to be HTTPS");
			}
			Input.EndTabbedPanel();
		}
	}
	public class SitemapIndex_Output // : ITemplate_Output
	{
		public SitemapIndex_Output()
		{
		}

		public void OnOutput(Asset asset, OutputContext context)
		{
			Out.Write(ComponentOutput(asset, context));
		}

		public string ComponentOutput(Asset asset, OutputContext context)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("<?xml version='1.0' encoding='UTF-8'?>");
			sb.AppendLine(@"<sitemapindex xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
			xsi:schemaLocation='http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/siteindex.xsd'
			xmlns='http://www.sitemaps.org/schemas/sitemap/0.9'>");

			bool forceHttps = asset.Raw["sitemap_force_https"] == "true";

			foreach(var panel in asset.GetPanels("sitemap_roots"))
			{
				Asset sitemapAsset = Asset.Load(panel["sitemap_asset"]);

				if(sitemapAsset.IsLoaded)
				{
					//create dependency to the sitemaproot asset specified
					asset.AddDependencyTo(sitemapAsset);

					sb.AppendLine("<sitemap>");

					string loc = sitemapAsset.GetLink(addDomain: true);
					if(forceHttps) loc = loc.Replace("http://", "https://");
					sb.AppendFormat("  <loc>{0}</loc>\n", loc);

					if(sitemapAsset.PublishDate != null)
					{
						//use W3C Datetime format, yyyy-MM-dd
						sb.AppendFormat("  <lastmod>{0}</lastmod>\n", sitemapAsset.PublishDate.Value.ToString("yyyy-MM-dd"));
					}

					sb.AppendLine("</sitemap>");
				}

			}

			sb.AppendLine("</sitemapindex>");

			return sb.ToString();
		}
	}


}
#endregion


