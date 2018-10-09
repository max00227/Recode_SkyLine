using System;
namespace test
{
    public partial class CharaLargeData : LargeDataBase
    {
        private System.Int32 _rank;


        //[Newtonsoft.Json.JsonProperty("rank")]
        public virtual System.Int32 Rank{
            get { 
                return this._rank; 
            }
            set{
                this._rank = value;
            }
        }

        /*System.Int32 atk { get; set; }
        System.Int32 def { get; set; }
        System.Int32 mAtk { get; set; }
        System.Int32 mDef { get; set; }
        System.Int32 hp { get; set; }
        System.Int32 crt { get; set; }

        System.Int32[] Act { get; set; }

        System.Int32[] skillData { get; set; }*/
    }
}
