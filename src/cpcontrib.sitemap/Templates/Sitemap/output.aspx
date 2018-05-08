<%@ Page Language="C#" Inherits="CrownPeak.Internal.Debug.OutputInit" %>
<%@ Import Namespace="CrownPeak.CMSAPI" %>
<%@ Import Namespace="CrownPeak.CMSAPI.Services" %>
<%@ Import Namespace="CrownPeak.CMSAPI.CustomLibrary" %>
<!--DO NOT MODIFY CODE ABOVE THIS LINE-->
<% 
	context.IsGeneratingDependencies = false;
	context.RenderPublishLinks = true;

	if(asset.Raw["sitemap_usedbyindex"] != "")
	{
		Asset.Load(asset.Raw["sitemap_usedbyindex"]).AddDependencyTo(asset);
	}

	var sw = new System.Diagnostics.Stopwatch();
	sw.Start();
	OnOutput();
	sw.Stop();

	Util.Log(asset, "Last run finished {0} and lasted {1}", DateTime.UtcNow.ToString("O"), sw.Elapsed.ToString("c"));

%>

<script runat="server" data-cpcode="true">

    public void OnOutput()
    {
        Out.WriteLine(@"<?xml version=""1.0"" encoding=""UTF-8"" ?>");
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

            var sitemapbuilder = new CPContrib.SiteMap.Templates.SitemapOutputBuilder(asset, context) { ForceHttps = true };

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

    private void AssignPropertiesSpecial(CPContrib.SiteMap.UrlBuilder url, CPContrib.SiteMap.UrlMetaEntry overrideEntry)
    {


    }

    public List<Asset> GatherAssets(PanelEntry panel)
    {
        var list = new List<Asset>(10000);

        Util.Log(asset, "Using asset.label='{0}'.", asset.Label);
        if(asset.Label == "Sitemap.Corporate")
            AddCorporateSiteAssets(ref list);
        else if(asset.Label == "Sitemap.SharingMemoriesProfiles")
            AddSharingMemoriesProfiles(ref list);

        Util.Log(asset, "Gather 3: List contains {0} assets.", list.Count);

        return list;
    }

    public int AddCorporateSiteAssets(ref List<Asset> targetList)
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
        filter.FieldNames = Util.MakeList("!sitemap_priority", "!sitemap_changefreq");
        //filter.Limit = 100;

        var list = folder.GetFilterList(filter);
        targetList.AddRange(list);
        Util.Log(asset, "Gather 1: List contains {0} assets.", list.Count);

        return list.Count;
    }

    public int AddSharingMemoriesProfiles(ref List<Asset> targetList)
    {
        //add sharing memories profiles
        FilterParams filter2 = new FilterParams();
        filter2.Add(AssetPropertyNames.TemplateId, Comparison.Equals, SharingMemories.Templates.Profiles.Profile.TemplateId);
        //filter2.Limit = 2;

        string profilesPath = "/Sharing Memories/Profiles";
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

            Util.Log(asset, "Adding subfolder '{0}' with {1} assets.", subfolder.AssetPath, addlist.Count());

            targetList.AddRange(addlist);
            count += addlist.Count();
        }

        return count;
    }

</script>
