using CrownPeak.CMSAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CPContrib.SiteMap.Tests
{
	public class AssetPrivates
	{

		static AssetPrivates()
		{
			CMSAPI.Initialize();

			Type assetType = typeof(CrownPeak.CMSAPI.Asset);

			LoadEmptyAsset_Method = assetType.GetMethod("LoadEmptyAsset", BindingFlags.NonPublic | BindingFlags.Static);

			BindingFlags nonpublic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			_fields_Field = assetType.GetField("_fieldCache", nonpublic);
			IsLoaded_Property = assetType.GetProperty("IsLoaded", nonpublic);
			IsLoaded_Property_get = IsLoaded_Property.GetGetMethod(true);
			IsLoaded_Property_set = IsLoaded_Property.GetSetMethod(true);

			LoadString_Property = assetType.GetProperty("LoadString", nonpublic);
		}

		static object[] EmptyObjectArray = new object[0];
		static MethodInfo LoadEmptyAsset_Method;
		static FieldInfo _fields_Field;
		static PropertyInfo LoadString_Property;
		static PropertyInfo IsLoaded_Property;
		static MethodInfo IsLoaded_Property_get;
		static MethodInfo IsLoaded_Property_set;


		public AssetPrivates(Asset instance)
		{
			this.instance = instance;
		}
		Asset instance;
		public Asset Instance { get { return this.instance; } }

		public void set_fields(Dictionary<string,string> value)
		{
			_fields_Field.SetValue(this.instance, value);
		}

		public bool IsLoaded
		{
			get { return (bool)IsLoaded_Property_get.Invoke(this.instance, EmptyObjectArray);  }
			set { IsLoaded_Property_set.Invoke(this.instance, new object[] { value }); }
		}

		public static Asset LoadEmptyAsset(string loadString)
		{
			return (Asset)LoadEmptyAsset_Method.Invoke(null, new object[] { loadString }); 
		}

	}
}
