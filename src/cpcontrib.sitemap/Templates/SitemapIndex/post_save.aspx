<%@ Page Language="C#" Inherits="CrownPeak.Internal.Debug.PostSaveInit" %>
<%@ Import Namespace="CrownPeak.CMSAPI" %>
<%@ Import Namespace="CrownPeak.CMSAPI.Services" %>
<%@ Import Namespace="CrownPeak.CMSAPI.CustomLibrary" %>
<!--DO NOT MODIFY CODE ABOVE THIS LINE-->
<%//This plugin uses PostSaveContext as its context class type%>
<%

	foreach(var panel in asset.GetPanels("sitemap_roots"))
	{
		Asset sitemapAsset = Asset.Load(panel["sitemap_asset"]);

		if(sitemapAsset.IsLoaded)
		{
			sitemapAsset.SaveContentField("sitemap_usedbyindex", asset.Id.ToString());
		}


	}

%>
