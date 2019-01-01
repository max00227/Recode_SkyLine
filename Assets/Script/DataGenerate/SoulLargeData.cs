namespace model.data
{
	[System.Serializable()]
	public partial class SoulLargeData : LargeDataBase, AbilityDataBase
    {
		public System.Int32 rank;
		public System.Int32 job;
		public System.Int32 soulType;
		public System.Int32 ethnicity;
		public System.Int32 attributes;

		System.Collections.Generic.Dictionary<string,int> _abilitys;

		[Newtonsoft.Json.JsonProperty("Ability")]
		public System.Collections.Generic.Dictionary<string,int> abilitys{
			get
			{
				return this._abilitys;
			}
			set
			{
				this._abilitys = value;
			}
		}

		[Newtonsoft.Json.JsonProperty("Act")]
		public System.Int32[] act { get; set; }

		[Newtonsoft.Json.JsonProperty("ActSkill")]
		public System.Int32 skill { get; set; }
		public SkillLargeData _skill;

        public void Merge(System.Collections.Generic.Dictionary<string, int> data)
        {
			foreach (System.Collections.Generic.KeyValuePair<string,int> kv in data) {
				abilitys [kv.Key] = kv.Value;
			}
        }

		public void Merge(System.Int32 skillId){
			if (skillId != 0) {
                //_skill = MasterDataManager.GetSkillData (skillId);
                //_skill.Merge (_skill.rule_id);
			}
		}
    }
}
