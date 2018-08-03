namespace model.data
{
	public partial class MonsterLargeData : LargeDataBase, AbilityDataBase {

		System.Int32 _rank;
		System.Int32 _job;
		System.Int32 _ethnicity;
		System.Int32 _attributes;

		System.Int32 _atk;
		System.Int32 _def;
		System.Int32 _mAtk;
		System.Int32 _mDef;
		System.Int32 _hp;
		System.Int32 _crt;


		public System.Int32 rank{
			get
			{
				return this._rank;
			}
			set
			{
				this._rank = value;
			}
		}

		public System.Int32 job{
			get
			{
				return this._job;
			}
			set
			{
				this._job = value;
			}
		}

		public System.Int32 ethnicity{
			get
			{
				return this._ethnicity;
			}
			set
			{
				this._ethnicity = value;
			}
		}

		public System.Int32 attributes{
			get
			{
				return this._attributes;
			}
			set
			{
				this._attributes = value;
			}
		}

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

		public System.Int32 act{ get; set;}



	    public void Merge(System.Collections.Generic.Dictionary<string, int> data)
	    {
	        this.atk = data["Atk"];
	        this.def = data["Def"];
	        this.mAtk = data["MAtk"];
	        this.mDef = data["MDef"];
	        this.hp = data["Hp"];
	    }
	}
}
