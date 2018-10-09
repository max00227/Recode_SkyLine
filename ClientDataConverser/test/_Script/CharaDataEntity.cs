using System;

namespace test
{
    public interface CharaDataEntity
    {
        

        System.String name { get; set; } 
        System.Int32 rank { get; set; }
        System.Int32 Job { get; set; }

        System.Int32 Ethnicity { get; set; }

        System.Int32 Attributes { get; set; }

        System.Int32 Atk { get; set; }
        System.Int32 Def { get; set; }
        System.Int32 mAtk { get; set; }
        System.Int32 mDef { get; set; }
        System.Int32 Hp { get; set; }
        System.Int32 Crt { get; set; }

        System.Int32[] Skill { get; set; }
    }
}
