<%@ Page Language="C#" Inherits="CrownPeak.Internal.Debug.PreviewInit" %>
<%@ Import Namespace="CrownPeak.CMSAPI" %>
<%@ Import Namespace="CrownPeak.CMSAPI.Services" %>
<%@ Import Namespace="CrownPeak.CMSAPI.CustomLibrary" %>
<!--DO NOT MODIFY CODE ABOVE THIS LINE-->
<%//This plugin uses OutputContext as its context class type%>
<%

	//SitemapBuilder
	//	.LoadAssets("/Arbor Memorial")
	//	.ExcludeTemplates(asset.Raw["ignored_templates"].Replace("\r\n", "\n").Split('\n'))
	//	.ExcludePaths(asset.Raw["ignored_paths"].Replace("\r\n", "\n").Split('\n'))
	//	.GenerateXml();

	foreach(var panel in asset.GetPanels("sitemap_roots"))
	{
		Asset sitemaproot = Asset.Load(panel["sitemap_root"]);

		FilterParams fp = new FilterParams();
		//fp.SetFilterStatus(context.PublishingStatus.Name);

		var list = sitemaproot.GetFilterList(fp);


		Out.Write("<table>\n");
		foreach(var g in list.GroupBy(_=>_.TemplateId).OrderByDescending(_=>_.Count()))
		{
			Asset templateFolder = Asset.Load(g.Key);
			Out.Write("<tr><td><span title='assetid:{2}'>{0}</span></td><td>{1} assets</td></tr>\n", templateFolder.AssetPath, g.Count(), templateFolder.Id);
		}
		Out.Write("</table>\n");
	}

	//Out.Write("<textarea style='height:200px;width:500px;'>" + Util.HtmlEncode(asset.Show("output.aspx")) + "\n</textarea>");

%>