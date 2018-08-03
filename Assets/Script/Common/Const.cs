using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Const {

	public enum jobType{
		none = 0,
		sworder = 1,
		boxer = 2,
		shielder = 3,
		Archer = 4,
		wizard = 5
	}

	public enum Attributes{
		none = 0,
		fire = 1,
		earth = 2,
		wind = 3,
		water = 4,
		light = 5,
		dark = 6
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
}
