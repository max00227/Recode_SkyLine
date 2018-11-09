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

		System.Int32 _atk;
		System.Int32 _def;
		System.Int32 _mAtk;
		System.Int32 _mDef;
		System.Int32 _hp;
		System.Int32 _crt;

		[Newtonsoft.Json.JsonProperty("Atk")]
		public System.Int32 atk{
			get
			{
				return this._atk;
			}
			set
			{
				this._atk = value;
			}
		}

		[Newtonsoft.Json.JsonProperty("Def")]
		public System.Int32 def{
			get
			{
				return this._def;
			}
			set
			{
				this._def = value;
			}
		}

		[Newtonsoft.Json.JsonProperty("mAtk")]
		public System.Int32 mAtk{
			get
			{
				return this._mAtk;
			}
			set
			{
				this._mAtk = value;
			}
		}

		[Newtonsoft.Json.JsonProperty("mDef")]
		public System.Int32 mDef{
			get
			{
				return this._mDef;
			}
			set
			{
				this._mDef = value;
			}
		}

		[Newtonsoft.Json.JsonProperty("Hp")]
		public System.Int32 hp{
			get
			{
				return this._hp;
			}
			set
			{
				this._hp = value;
			}
		}

		[Newtonsoft.Json.JsonProperty("Crt")]
		public System.Int32 crt{
			get
			{
				return this._crt;
			}
			set
			{
				this._crt = value;
			}
		}


		[Newtonsoft.Json.JsonProperty("Act")]
		public System.Int32[] act { get; set; }

		[Newtonsoft.Json.JsonProperty("ActSkill")]
		public System.Int32 actSkill { get; set; }
		public SkillLargeData _actSkill;

		[Newtonsoft.Json.JsonProperty("NorSkill")]
		public System.Int32 norSkill { get; set; }
		public SkillLargeData _norSkill;

        public void Merge(System.Collections.Generic.Dictionary<string, int> data)
        {
            this.atk = data["Atk"];
            this.def = data["Def"];
            this.mAtk = data["MAtk"];
            this.mDef = data["MDef"];
            this.hp = data["Hp"];
        }

		public void Merge(System.Int32 actId, System.Int32 norId){
			if (actId != 0) {
				_actSkill = MasterDataManager.GetSkillData (actId);
				_actSkill.Merge (_actSkill.rule_id);
			}

			if (norId != 0) {
				_norSkill = MasterDataManager.GetSkillData (norId);
				_norSkill.Merge (_norSkill.rule_id);
			}
		}
    }
}
