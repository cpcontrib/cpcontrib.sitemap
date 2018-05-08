<%@ Page Language="C#" Inherits="CrownPeak.Internal.Debug.FilenameInit" %>
<%@ Import Namespace="CrownPeak.CMSAPI" %>
<%@ Import Namespace="CrownPeak.CMSAPI.Services" %>
<%@ Import Namespace="CrownPeak.CMSAPI.CustomLibrary" %>
<!--DO NOT MODIFY CODE ABOVE THIS LINE-->
<%//This plugin uses OutputContext as its context class type%>
<%
	// filename.aspx: template file to allow filename to be set via code
	// ex. code to use the cms id along with the folder names for the asset filename
	context.PublishPath = context.PropertiesFolder + context.RelativeFolder + "sitemap.xml";

%>