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
		new Color (0.26f, 0.08f, 0.08f, 1f),//Attributes.None
		new Color (0.86f, 0f, 0f, 1f),//Attributes.Fire
		new Color (0.55f, 0.48f, 0f, 1f),//Attributes.Earth
		new Color (0.13f, 0.55f, 0f, 1f),//Attributes.Wind
		new Color (0.13f, 0.68f, 1f, 1f),//Attributes.Water
		new Color (1f, 0.95f, 0.49f, 1f),//Attributes.Litht
		new Color (0.28f, 0f, 0.56f, 1f)//Attributes.Dark
	};

}
