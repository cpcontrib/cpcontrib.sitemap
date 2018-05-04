using CrownPeak.CMSAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace /*!packer:ComponentLibraryNamespace*/ComponentLibrary
{
	using CPContrib.SiteMap;
	using Constants = CPContrib.SiteMap.SitemapConstants;

	public partial class Component_SitemapMeta : ComponentBase
	{
		public override void ComponentInput(Asset asset, InputContext context, string label, string name)
		{
			Input.StartControlPanel(label + " Sitemap Meta");
			{
				Input.ShowCheckBox("Sitemap Exclusion", Constants.FieldNames.Sitemap_Include, "false", "Exclude from Sitemap",
					helpMessage: "Indicates that Sitemap Builder routines should skip this asset.");

				var entries = Util.MakeList("", "always", "hourly", "daily", "weekly", "monthly", "yearly", "never").ToDictionary(v => v);
				Input.ShowDropDown("Change Frequency", Constants.FieldNames.Sitemap_ChangeFreq, entries);

				var priorityEntries = Util.MakeList("", "10", "9", "8", "7", "6", "5", "4", "3", "2", "1").ToDictionary(v => v);
				Input.ShowDropDown("Priority", Constants.FieldNames.Sitemap_Priority, priorityEntries);
			}
		}

		public override string ComponentOutput(Asset asset, OutputContext context, string name, string index = "", bool isDrag = false)
		{
			//no output
			return "";
		}

		public override void ComponentPostInput(Asset asset, PostInputContext context, string name, string index = "")
		{
			
		}
	}
}
