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
			AssetPrivates a = new AssetPrivates(AssetPrivates.LoadEmptyAsset("/sitemap"));

			Dictionary<string, string> contentFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			a.set_fields(contentFields);
			a.IsLoaded = true;
			
			return new Sitemap_Input(a.Instance);
		}

		[Test]
		[TestCase(@"/Arbor Memorial/en/* => {""changefreq"":""daily"",""priority"":0.5}", "/Arbor Memorial/en/*")]
		[TestCase(@"/Arbor Memorial => {""changefreq"":""daily"",""priority"":0.5}", "/Arbor Memorial")]
		public void ParseOverrides(string input, string pathspec_value)
		{
			var expected = 
				new Override() {
					PathSpec = pathspec_value,
					PathSpecRegex = SitemapUtils.PathspecToRegex(pathspec_value),
					OverrideProperties = new Serialization.@override() { changefreq="daily", priority=0.5f } 
			};
			var lines = new string[] { input };

			var sitemapinput = CreateSitemapInput();
			var overrides = sitemapinput._ParseOverrides(lines);

			overrides.Should().HaveCount(1);
			overrides.Should().Equal(expected);
		}

		[Test]
		public void ParseOverrides2()
		{
			var pathspec = "/Arbor Memorial/(en|fr)/What We Do/*";
			var input = pathspec + @" => {""priority"":0.8}";

			var expected = new Override()
			{
				PathSpec = pathspec,
				PathSpecRegex = SitemapUtils.PathspecToRegex(pathspec),
				OverrideProperties = new Serialization.@override() { changefreq = null, priority = 0.8f }

			};

			var sitemapinput = CreateSitemapInput();
			var overrides = sitemapinput._ParseOverrides(new string[] { input });

			overrides.Should().BeEquivalentTo(expected);
		}

	}
}
