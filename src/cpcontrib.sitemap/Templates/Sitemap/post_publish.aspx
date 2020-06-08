<%@ Page Language="C#" Inherits="CrownPeak.Internal.Debug.PostPublishInit" %>
<%@ Import Namespace="CrownPeak.CMSAPI" %>
<%@ Import Namespace="CrownPeak.CMSAPI.Services" %>
<%@ Import Namespace="CrownPeak.CMSAPI.CustomLibrary" %>
<!--DO NOT MODIFY CODE ABOVE THIS LINE-->
<%//This plugin uses PostPublishContext as its context class type%>
<%

	var postpublish = new CPContrib.SiteMap.Templates.Sitemap_PostPublish();
	postpublish.Execute(asset, context);

%>

