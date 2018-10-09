using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace test
{
    static class jsonTest
	{
		public enum Options
		{
			Option1, Option2
		}

		public class Entity
		{
			public Options A1 { get; set; }
			public Options A2 { get; set; }
			[JsonConverter(typeof(StringEnumConverter))]
			public Options B1 { get; set; }
			[JsonConverter(typeof(StringEnumConverter))]
			public Options B2 { get; set; }
			public string P1 { get; set; }
			[JsonIgnore]
			public string P2 { get; set; }
		}


        static public void TestFnt()
		{
			Entity ent = new Entity()
			{
				A1 = Options.Option1,
				A2 = Options.Option2,
				B1 = Options.Option1,
				B2 = Options.Option2,
				P1 = "P1",
				P2 = "P2"
			};
			Console.WriteLine(
				JsonConvert.SerializeObject(ent, Formatting.Indented));
			Console.Read();
		}
	}
}