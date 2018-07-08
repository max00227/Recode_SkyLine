using System;
//using model.data;

namespace model.data
{
    public class ClientLargeData
    {
        /*private System.Int32 _rank;


        [Newtonsoft.Json.JsonProperty("rank")]
        public virtual System.Int32 rank
        {
            get
            {
                return this._rank;
            }
            set
            {
                this._rank = value;
            }
        }*/

		public System.Collections.Generic.List<CharaLargeData> Chara { get; set; }
        
		public System.Collections.Generic.List<SkillLargeData> Skill{ get; set;}

		public System.Collections.Generic.List<RuleLargeData> Rule{ get; set;}
    }
}
