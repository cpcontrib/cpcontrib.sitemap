<%@ Page Language="C#" Inherits="CrownPeak.Internal.Debug.OutputInit" %>
<%@ Import Namespace="CrownPeak.CMSAPI" %>
<%@ Import Namespace="CrownPeak.CMSAPI.Services" %>
<%@ Import Namespace="CrownPeak.CMSAPI.CustomLibrary" %>
<!--DO NOT MODIFY CODE ABOVE THIS LINE-->
<% 
	context.IsGeneratingDependencies = false;
	context.RenderPublishLinks = true;

	Initialize();

	if(asset.Raw["sitemap_usedbyindex"] != "")
	{
		Asset.Load(asset.Raw["sitemap_usedbyindex"]).AddDependencyTo(asset);
	}

	var sw = new Stopwatch();
	sw.Start();

	Util.Log(asset, "Started run at {0}", sw.Started.ToString("u"));

	using (Log = new UtilLogLogger("Sitemap.Output", asset))
	{
		try
		{
			OnOutput();
		}
		catch (Exception ex)
		{
			Log.Error(ex);
			Util.Log(asset, "Failed to run sitemap: " + ex.ToString());
		}
	}

	Util.Log(asset, "Last run finished {0} and lasted {1}", DateTime.UtcNow.ToString("u"), sw.Elapsed.ToString("g"));

%>

<script runat="server" data-cpcode="true">

	UtilLogLogger Log;

	private void Initialize()
	{
		Log = new UtilLogLogger("Sitemap.Output", asset);
	}

	public void OnOutput()
	{
		Out.WriteLine(@"<?xml version=""1.0"" encoding=""UTF-8"" ?>");
		Out.WriteLine(@"<!-- Output at: {0} -->", DateTime.UtcNow.ToString("O"));
		Out.WriteLine(@"<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemalocation=""http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd"">");

		string siteDomain = "https://" + context.PublishingPackage.HostName;

		var panelCollection = asset.GetPanels("sitemap_roots");

		foreach(var panel in panelCollection)
		{
			//sitemapbuilder.AddRoot(panel["sitemap_root"])
			//	.AddIgnoredPaths(panel["sitemap_ignorepaths"])
			//	.AddFilterParams(filterparams)
			//	.GetAssets()

			//list.Sort(new AssetPathComparator());

			var sitemapinput = new CPContrib.SiteMap.Templates.Sitemap_Input(asset);

			var sitemapbuilder = new CPContrib.SiteMap.Templates.SitemapOutputBuilder(asset, context, Log) { ForceHttps = true };

			sitemapbuilder
				.AddIgnoredPaths(sitemapinput.GetIgnoredPaths(panel))
				.SetDefaults(sitemapinput.GetDefaults(panel))
				.SetOverrides(sitemapinput.GetOverrides(panel))
				.AddAssets(GatherAssets(panel));

			if(asset.Label == "Sitemap.SharingMemoriesProfiles")
			{
				sitemapbuilder.AssignMetaFunc = (url, defaultEntry, overrideEntry) =>
				{
					//run default first
					sitemapbuilder.AssignMeta(url, defaultEntry, overrideEntry);
				};
			}

			var sitemapList = sitemapbuilder.ProcessList(asset.WorkflowStatus);

			Out.Write(sitemapbuilder.GetSitemapXmlChunk(sitemapList));
		}

		Out.WriteLine(@"</urlset>");
	}

	private void AssignPropertiesSpecial(CPContrib.SiteMap.SitemapItem url, CPContrib.SiteMap.UrlMetaEntry overrideEntry)
	{


	}

	public List<Asset> GatherAssets(PanelEntry panel)
	{
		var list = new List<Asset>(10000);

		Log.Info("Using asset.label='{0}'.", asset.Label);
		if(asset.Label == "Sitemap.SectionA")
			AddSectionA(list);
		else if(asset.Label == "Sitemap.SectionB")
			AddSectionB(list);

		Log.Info("Gather 3: List contains {0} assets.", list.Count);

		return list;
	}

	public int AddSectionA(List<Asset> targetList)
	{
		var sitemapinput = new CPContrib.SiteMap.Templates.Sitemap_Input(asset);

		Asset folder = Asset.Load("/" + asset.AssetPath[0]);

		FilterParams filter = new FilterParams();
		filter.SetFilterStatus(context.PublishingStatus.Name);
		foreach(var item in new string[] { "Global Configuration", "Section Configuration", "MasterPage.master" })
			filter.Add(AssetPropertyNames.Label, Comparison.NotEquals, item);
		//filter.Add(AssetPropertyNames.TemplateLabel, Comparison.NotInSet, new List<string>() { "XML News Sitemap", "XML Sitemap", "Analytics", "Developer" });
		foreach(var item in sitemapinput.GetExcludedTemplateIds())
			filter.Add(AssetPropertyNames.TemplateId, Comparison.NotEquals, item);
		//filter.SortOrder = SortOrder.OrderByDescending(AssetPropertyNames.ChangeDate);
		filter.Add(Comparison.Equals, AssetType.File);
		filter.Add("!sitemap_exclude", Comparison.NullOrNotEquals, "true");
		filter.FieldNames = Util.MakeList("!sitemap_priority", "!sitemap_changefreq", "!sitemap_exclude");
		//filter.FieldNames = Util.MakeList("@cpcontrib/sitemap|priority", "@cpcontrib/sitemap|changefreq", "@cpcontrib/sitemap|exclude");
		//filter.Limit = 100;

		var list = folder.GetFilterList(filter);
		targetList.AddRange(list);
		Log.Info("Gather 1: List contains {0} assets.", list.Count);

		return list.Count;
	}

	public int AddSectionB(List<Asset> targetList)
	{
		//add sharing memories profiles
		FilterParams filter2 = new FilterParams();
		filter2.Add(AssetPropertyNames.TemplateId, Comparison.Equals, 12345);
		//filter2.Limit = 2;

		string profilesPath = "/folderX/some-folder";
		if(context.PublishingStatus.Name == "Stage") profilesPath += "-Stage";

		var sitemap = new CPContrib.SiteMap.SitemapBuilder();
		int count = 0;

		var profilesFolder = Asset.Load(profilesPath);

		//var foundList = profilesFolder.GetFilterList(filter2);
		//targetList.AddRange(foundList);
		//return foundList.Count;

		foreach(var subfolder in profilesFolder.GetFolderList(SortOrder.OrderBy(AssetPropertyNames.Label)))
		{
			//Util.Log(asset, "Adding subfolder '{0}'.", subfolder.AssetPath);
			var addlist = subfolder.GetFilterList(filter2);// sitemap.IncludePath(subfolder.AssetPath.ToString(), filter2).ToList();

			Log.Info("Adding subfolder '{0}' with {1} assets.", subfolder.AssetPath, addlist.Count());

			targetList.AddRange(addlist);
			count += addlist.Count();
		}

		return count;
	}

</script>
