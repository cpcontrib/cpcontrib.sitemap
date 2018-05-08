<%@ Page Language="C#" Inherits="CrownPeak.Internal.Debug.OutputInit" %>
<%@ Import Namespace="CrownPeak.CMSAPI" %>
<%@ Import Namespace="CrownPeak.CMSAPI.Services" %>
<%@ Import Namespace="CrownPeak.CMSAPI.CustomLibrary" %>
<!--DO NOT MODIFY CODE ABOVE THIS LINE-->
<%//This plugin uses OutputContext as its context class type%>
<%

    var template_output = new CPContrib.SiteMap.Templates.SitemapIndex_Output();
    template_output.OnOutput(asset, context);

%>
