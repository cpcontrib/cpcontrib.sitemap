using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CPContrib.SiteMap.Tests
{
	public static class CMSAPI
	{

		static CMSAPI()
		{
			Type sessionhelperproxyType = typeof(CrownPeak.CMSAPI.Asset).Assembly.GetType("CrownPeak.Internal.Proxy.SessionHelperProxy");

			endpointBaseAddress_Field = sessionhelperproxyType.GetField("endpointBaseAddress");
		}

		static FieldInfo endpointBaseAddress_Field;

		public static string SessionHelperProxy_endpointBaseAddress
		{
			get { return (string)endpointBaseAddress_Field.GetValue(null); }
			set { endpointBaseAddress_Field.SetValue(null, value); }
		}

#pragma warning disable CS0618
		public static void Initialize()
		{
			SessionHelperProxy_endpointBaseAddress = System.Configuration.ConfigurationSettings.AppSettings["CrownPeak.Internal.SessionHelperProxy_endpointBaseAddress"];
		}
#pragma warning restore CS0618

	}
}
