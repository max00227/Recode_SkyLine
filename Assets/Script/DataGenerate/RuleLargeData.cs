using System;

namespace model.data{
	public partial class RuleLargeData : LargeDataBase,AbilityDataBase {

		[Newtonsoft.Json.JsonProperty("Id")]
		System.Int32 id;

		[Newtonsoft.Json.JsonProperty("Target")]
		public System.Int32 target{ get; set;}

		[Newtonsoft.Json.JsonProperty("Rule")]
		public System.Int32[] rule{ get; set;}

		[Newtonsoft.Json.JsonProperty("NormalEffect")]
		public System.Int32[] normalEffect{ get; set;}

		[Newtonsoft.Json.JsonProperty("StatusEffect")]
		public System.Int32[] statusEffect{ get; set;}

		[Newtonsoft.Json.JsonProperty("ConvType")]
		public System.Int32 convType{ get; set;}

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
	}
}
