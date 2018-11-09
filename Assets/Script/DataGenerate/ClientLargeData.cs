using System;
//using model.data;

namespace model.data
{
    public class ClientLargeData
    {
		[Newtonsoft.Json.JsonProperty("Soul")]
		public System.Collections.Generic.List<SoulLargeData> soul { get; set; }
        
		[Newtonsoft.Json.JsonProperty("Skill")]
		public System.Collections.Generic.List<SkillLargeData> skill{ get; set;}

		[Newtonsoft.Json.JsonProperty("Rule")]
		public System.Collections.Generic.List<RuleLargeData> rule{ get; set;}
    }
}
