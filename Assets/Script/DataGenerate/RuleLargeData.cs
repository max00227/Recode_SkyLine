using System;

namespace model.data{
	public partial class RuleLargeData : LargeDataBase,AbilityDataBase {

		System.Int32 id;

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

		public System.Int32 Target{ get; set;}
		public System.Int32[] RuleType{ get; set;}
		public System.Int32[] EffectType{ get; set;}
		public System.Int32 ConvType{ get; set;}
		public System.Boolean isBuff{ get; set;}
	}
}
