using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using model.data;

public static class MasterDataManager {

	private static List<SoulLargeData> soulLargeData;

	public static List<SoulLargeData> GetSoulLargeData{
		get{ 
			return soulLargeData;
		}
	}

	public static List<SkillLargeData> skillLargeData;

	public static List<SkillLargeData> GetSkillLargeData{
		get{ 
			return skillLargeData;
		}
	}

	public static List<RuleLargeData> ruleLargeData;

	public static List<RuleLargeData> GetRuleLargeData{
		get{ 
			return ruleLargeData;
		}
	}

	private static List<StatusLargeData> statusLargeData;

	public static List<StatusLargeData> GetStatusLargeData{
		get{ 
			return statusLargeData;
		}
	}

	public static SoulLargeData GetSoulData(int id){
		return DataUtil.GetById<SoulLargeData> (id, ref soulLargeData);
	}

	public static SkillLargeData GetSkillData(int id){
		return DataUtil.GetById<SkillLargeData> (id, ref skillLargeData);
	}

	public static RuleLargeData GetRuleData(int id){
		return DataUtil.GetById<RuleLargeData> (id, ref ruleLargeData);
	}

	public static StatusLargeData GetStatusData(int id){
		return DataUtil.GetById<StatusLargeData> (id, ref statusLargeData);
	}

	public static void UpdataMasterdata(ClientLargeData clientData){
		soulLargeData = clientData.soul;

		skillLargeData = clientData.skill;

		ruleLargeData = clientData.rule;

		statusLargeData = clientData.nerfStatus;
	}
}

