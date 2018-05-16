/*!packer:combine=false;filename=static;target=ComponentLibrary*/
using CrownPeak.CMSAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace /*!packer:namespace=ComponentLibrary*/ComponentLibrary
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

				var entries = Util.MakeList("", Constants.Tiered_LastMod, "always", "hourly", "daily", "weekly", "monthly", "yearly", "never").ToDictionary(v => v);
				Input.ShowDropDown("Change Frequency", Constants.FieldNames.Sitemap_ChangeFreq, entries,
					helpMessage: "Select how frequently the asset is likely to change.",
					popupMessage: String.Join("\n",
					"This value provides general information to search engines and may not correlate exactly to how often they crawl the page.",
					"When Tiered_LastMod is selected, a function will run that determines changefreq based on asset.ModifiedDate")
				);

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
