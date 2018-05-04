using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using FluentAssertions;

namespace CPContrib.SiteMap.Tests
{
	[TestFixture]
	public class SitemapUtil_Tests
	{

		[Test]
		public void SplitMultilineInput()
		{
			//simulates a multiline textbox in crownpeak Input.ShowTextBox(multiline:true)
			var rawinput = "Line 1\r\nLine 2\r\nLine 3";
			var expected = new string[] { "Line 1", "Line 2", "Line 3" };

			var split = SitemapUtils.SplitMultilineInput(rawinput);

			split.Should().ContainInOrder(expected);
		}


	}
}
