namespace model.data
{
	public partial class StatusLargeData : LargeDataBase {
		[Newtonsoft.Json.JsonProperty("EffectType")]
		public System.Int32 effectType{ get; set;}

		[Newtonsoft.Json.JsonProperty("CharaDetail")]
		public System.String charaDetail{ get; set;}

		[Newtonsoft.Json.JsonProperty("EnemyDetail")]
		public System.String enemyDetail{ get; set;}

		[Newtonsoft.Json.JsonProperty("CharaStatus")]
		public System.Int32 charaStatus{ get; set;}

		[Newtonsoft.Json.JsonProperty("EnemyStatus")]
		public System.Int32 enemyStatus{ get; set;}

		[Newtonsoft.Json.JsonProperty("ChgAttri")]
		public System.Int32 chgAttri{ get; set;}

		[Newtonsoft.Json.JsonProperty("StatusParam")]
		public System.Collections.Generic.Dictionary<System.String,System.Int32[]> statusParam{ get; set;}

		[Newtonsoft.Json.JsonProperty("RmType")]
		public System.Int32 rmType{ get; set;}

		[Newtonsoft.Json.JsonProperty("RmParam")]	
		public System.Int32 rmParam{ get; set;}

		[Newtonsoft.Json.JsonProperty("ConvertType")]
		public System.Int32 convertType{ get; set;}

		[Newtonsoft.Json.JsonProperty("CanRemove")]
		public System.Boolean canRemove{ get; set;}

		public System.Int32[] statusType{ get; set;}
	}
}
