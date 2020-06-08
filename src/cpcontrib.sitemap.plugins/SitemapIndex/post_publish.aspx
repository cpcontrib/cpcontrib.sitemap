<%@ Page Language="C#" Inherits="CrownPeak.Internal.Debug.PostPublishInit" %>
<%@ Import Namespace="CrownPeak.CMSAPI" %>
<%@ Import Namespace="CrownPeak.CMSAPI.Services" %>
<%@ Import Namespace="CrownPeak.CMSAPI.CustomLibrary" %>
<!--DO NOT MODIFY CODE ABOVE THIS LINE-->
<%//This plugin uses PostPublishContext as its context class type%>
<%
	context.RenderPublishLinks = true;

	var handler = new CPContrib.SiteMap.Templates.SitemapIndex_PostPublish();
	handler.OnPostPublish(asset, context);

%>
