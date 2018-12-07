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

		public RuleLargeData DeepCopy()
		{
			RuleLargeData copyData = (RuleLargeData) this.MemberwiseClone();
			copyData.id = this.id;
			copyData.target = this.target;
			copyData.rule = this.rule;
			copyData.effectType = this.effectType;
			copyData.effect = new int[this.effect.Length];
			Array.Copy (this.effect, copyData.effect, this.effect.Length);
			copyData.convType = this.convType;
			copyData.abilitys = this.abilitys;


			return copyData;
		}
	}
}
