using System;

namespace model.data{
	public partial class SkillLargeData : LargeDataBase {
		[Newtonsoft.Json.JsonProperty("type")]
		public System.Int32 type;

		[Newtonsoft.Json.JsonProperty("CD")]
		public System.Int32? cdTime;

        [Newtonsoft.Json.JsonProperty("Condition")]
        public System.Collections.Generic.List<int[]> condition;

		[Newtonsoft.Json.JsonProperty("Round")]
		public System.Int32? round;

		[Newtonsoft.Json.JsonProperty("rule_id")]
		public System.Int32[] rule_id;
		public System.Collections.Generic.List<RuleLargeData> ruleData;

		[Newtonsoft.Json.JsonProperty("isOr")]
		public System.Boolean isOr;

		public void Merge(System.Int32[] ids)
		{
			this.ruleData = new System.Collections.Generic.List<RuleLargeData> ();
			foreach(System.Int32 id in ids){
				if (id != 0) {
					this.ruleData.Add (MasterDataManager.GetRuleData (id));
				}
			}
		}

		public bool hasAnim;
	}

}

public enum SkillType{
	Normal = 0,
	Active = 1
}

public enum SkillLaunchType{
	
}