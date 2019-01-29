using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Const {

	public enum jobType{
		None = 0,
		Sworder = 1,
		Boxer = 2,
		Shielder = 3,
		Archer = 4,
		Wizard = 5
	}

	public enum Attributes{
		None = 0,
		Fire = 1,
		Earth = 2,
		Wind = 3,
		Water = 4,
		Light = 5,
		Dark = 6
	}

	public enum SkillTarget{
		None = 0,//無指定
		Self = 1,//自身
		Dir_Member = 2,//指定隊友
		Oly_Member = 3,//僅隊友
		All_Team = 4,//全隊
		All_Enemy = 5,//全敵人
		Dir_Enemy = 6,//指定敵人
		Trigger = 7//觸發者
	}

	public enum SkillLaunch{
		Start = 0,//起手型
		Trigger = 1,//觸發型
		Stay = 2//常駐型
	}

	public static Color[] attriColor = new Color[7] {
		new Color (1f, 1f, 1f, 1f),//Attributes.None
		new Color (1f, 0f, 0f, 1f),//Attributes.Fire
		new Color (0.82f, 0.72f, 0f, 1f),//Attributes.Earth
		new Color (0.01f, 0.83f, 0f, 1f),//Attributes.Wind
		new Color (0f, 0.35f, 1f, 1f),//Attributes.Water
		new Color (1f, 0.95f, 0.49f, 1f),//Attributes.Litht
		new Color (0.28f, 0f, 0.56f, 1f)//Attributes.Dark
	};


	/// <summary>
	/// Effect type.
	/// None(無),Recovery(回血),Act(激活),Cover(覆蓋),RmAlarm(警戒解除)
	/// ,RmNerf(異常解除),Dmg(固定傷害),Exchange(位置對調),Call(召喚)
	/// </summary>
	public enum NormalType {
		None = 0,
		Recovery = 1,
		Act = 2,
		Cover = 3,
		RmAlarm = 4,
		RmNerf = 5,
		Dmg = 6,
		Exchange = 7,
		Call = 8,
	}

	/// <summary>
	/// Effect type.
	/// None(無),UnDef(無視防禦),UnNerf(異常免疫),AddNerf(附加異常),Suffer(根性)
	/// ,Maximum(傷害值最大化),Ability(能力變化),UnDirect(反鎖定)
	/// </summary>
	public enum StatusType {
		None = 0,
		UnDef = 1,
		UnNerf = 2,
		AddNerf = 3,
		Suffer = 4,
		Maximum = 5,
		Ability = 6,
		UnDirect = 7
	}

	/// <summary>
	/// Converse type.
	/// None(無),Amount(純數值),Ratio(比率)
	/// </summary>
	public enum converseType {
		None = 0,
		Amount = 1,
		Ratio = 2
	}

    public enum NewAttributes
    {
        None = 0,
        Fire = 1,
        Wood = -1,
        Thunder = 2,
        Earth = -2,
        Wind = 3,
        Voice = -3,
        Water = 4,
        Light = 5,
        Dark = -5
    }
}
