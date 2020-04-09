<%@ Page Language="C#" Inherits="CrownPeak.Internal.Debug.PostInputInit" %>
<%@ Import Namespace="CrownPeak.CMSAPI" %>
<%@ Import Namespace="CrownPeak.CMSAPI.Services" %>
<%@ Import Namespace="CrownPeak.CMSAPI.CustomLibrary" %>
<!--DO NOT MODIFY CODE ABOVE THIS LINE-->
<%//This plugin uses PostInputContext as its context class type%>
<%
	// post_input.aspx: called when a user clicks on save in an input form
	// useful for field checking or post processing
	// ex. field check - if the title field is missing, go back to input form and print an error
	//  if (!context.InputForm.HasField("title"))
	//  { 
	//     context.ValidationError = "Please add a title.";
	//   }
	//   else if (!asset.Label.Equals(context.InputForm["title"]))
	//  {
	//    asset.Rename(context.InputForm["title"]);
	//  }

	var sitemapinput = new CPContrib.SiteMap.Templates.Sitemap_Input(asset);
	sitemapinput.PostInput(context);

%>