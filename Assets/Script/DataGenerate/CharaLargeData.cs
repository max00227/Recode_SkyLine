namespace model.data
{
	[System.Serializable()]
	public partial class CharaLargeData : LargeDataBase, AbilityDataBase
    {
		public System.Int32 rank;
		public System.Int32 job;
		public System.Int32 ethnicity;
		public System.Int32 attributes;

		System.Int32 _atk;
		System.Int32 _def;
		System.Int32 _mAtk;
		System.Int32 _mDef;
		System.Int32 _hp;
		System.Int32 _crt;


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


		public System.Int32[] Act { get; set; }

		public System.Int32 ActSkill { get; set; }
		public System.Int32 NorSkill { get; set; }

		public SkillLargeData _ActSkill;
		public SkillLargeData _NorSkill;



        public void Merge(System.Collections.Generic.Dictionary<string, int> data)
        {
            this.atk = data["Atk"];
            this.def = data["Def"];
            this.mAtk = data["MAtk"];
            this.mDef = data["MDef"];
            this.hp = data["Hp"];
        }

		public void MergeSkill(System.Int32 aid, System.Int32 nid)
		{
			this._ActSkill = MasterDataManager.GetSkillData (aid);
			if (this._ActSkill != null) {
				this._ActSkill.Merge (this._ActSkill.rule_id);
			}
			this._NorSkill = MasterDataManager.GetSkillData (nid);
			if (this._NorSkill != null) {
				this._NorSkill.Merge (this._NorSkill.rule_id);
			}
		}
    }
}
