using System;
//using model.data;

namespace model.data
{
    public class ClientLargeData
    {
		public System.Collections.Generic.List<SoulLargeData> Soul { get; set; }
        
		public System.Collections.Generic.List<SkillLargeData> Skill{ get; set;}

		public System.Collections.Generic.List<RuleLargeData> Rule{ get; set;}
    }
}
