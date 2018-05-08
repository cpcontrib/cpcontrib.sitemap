<%@ Page Language="C#" Inherits="CrownPeak.Internal.Debug.PostSaveInit" %>
<%@ Import Namespace="CrownPeak.CMSAPI" %>
<%@ Import Namespace="CrownPeak.CMSAPI.Services" %>
<%@ Import Namespace="CrownPeak.CMSAPI.CustomLibrary" %>
<!--DO NOT MODIFY CODE ABOVE THIS LINE-->
<%//This plugin uses PostSaveContext as its context class type%>
<%

	var sitemapinput = new CPContrib.SiteMap.Templates.Sitemap_Input(asset);
	sitemapinput.PostSave(context);

%>
