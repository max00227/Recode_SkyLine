using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamLargeData {
    [Newtonsoft.Json.JsonProperty("Member")]
	public System.Collections.Generic.List<MemberData> member { get; set; }

}
