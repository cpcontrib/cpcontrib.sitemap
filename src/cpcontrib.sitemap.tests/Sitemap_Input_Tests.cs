using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using FluentAssertions;

namespace CPContrib.SiteMap.Tests
{
	using CPContrib.SiteMap.Templates;
	using CrownPeak.CMSAPI;


	[TestFixture]
	public class Sitemap_Input_Tests
	{

		protected Sitemap_Input CreateSitemapInput()
		{
			Asset asset = Asset.Load("");
			return new Sitemap_Input(asset);
		}

		[Test]
		[TestCase("/Arbor Memorial/en/* => {changefreq:'daily',priority:0.5}", "/Arbor Memorial/en/*")]
		[TestCase("/Arbor Memorial => {changefreq:'daily',priority:0.5}", "/Arbor Memorial")]
		public void ParseOverrides(string input, string pathspec_value)
		{
			var expected = new Tuple<string, string>[] {
				new Tuple<string, string>(pathspec_value, "{changefreq:'daily',priority:0.5}")
			};

			var sitemapinput = CreateSitemapInput();
			var overrides = sitemapinput._ParseOverrides(input);

			overrides.Should().HaveCount(1);
			overrides.Should().Contain(expected);
		}


	}
}
