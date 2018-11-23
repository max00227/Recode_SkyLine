using System;

namespace model.data{
	public partial class RuleLargeData : LargeDataBase, AbilityDataBase {

		[Newtonsoft.Json.JsonProperty("Id")]
		System.Int32 id;

		[Newtonsoft.Json.JsonProperty("Target")]
		public System.Int32 target{ get; set;}

		[Newtonsoft.Json.JsonProperty("Rule")]
		public System.Int32[] rule{ get; set;}

		[Newtonsoft.Json.JsonProperty("EffectType")]
		public System.Int32 effectType{ get; set;}

		[Newtonsoft.Json.JsonProperty("Effect")]
		public System.Int32[] effect{ get; set;}

		[Newtonsoft.Json.JsonProperty("ConvType")]
		public Const.converseType convType{ get; set;}

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
	}
}
