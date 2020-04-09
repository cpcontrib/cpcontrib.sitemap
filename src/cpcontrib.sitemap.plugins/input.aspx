<%@ Page Language="C#" Inherits="CrownPeak.Internal.Debug.InputInit" %>

<%@ Import Namespace="CrownPeak.CMSAPI" %>
<%@ Import Namespace="CrownPeak.CMSAPI.Services" %>
<%@ Import Namespace="CrownPeak.CMSAPI.CustomLibrary" %>
<!--DO NOT MODIFY CODE ABOVE THIS LINE-->
<%//This plugin uses InputContext as its context class type%>
<%
    var layoutType = new CPContrib.SiteMap.Templates.Sitemap_Input(asset, context);
    layoutType.OnInput();
%>


