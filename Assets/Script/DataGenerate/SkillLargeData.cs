using System;

namespace model.data{
	public partial class SkillLargeData : LargeDataBase {
		public System.Int32 type;
		public System.Int32? CD;
		public System.Int32 launchType;
		public System.Int32? Round;
		public System.Collections.Generic.List<System.Int32> rule_id;

		public System.Collections.Generic.List<RuleLargeData> ruleData;

		public void Merge(System.Collections.Generic.List<System.Int32> ids)
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