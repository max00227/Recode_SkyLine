using UnityEngine;
using System;

[AttributeUsageAttribute(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class EnumDescriptionAttribute : Attribute
{
	public string description = string.Empty;

	public EnumDescriptionAttribute(string description)
	{
		this.description = description;
	}
}

[AttributeUsageAttribute(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class EnumPopupAttribute : PropertyAttribute
{
	public System.Type enumType;
	public string displayName;
	public bool canCustom;

	/// <summary>
	/// Initializes a new instance of the <see cref="EnumPopupAttribute"/> class.
	/// </summary>
	/// <param name="type">Enumのタイプ</param>
	/// <param name="canCustom">カスタム入力ができるかどうか(trueすれば、Enumの最初にCustomを追加してください)</param>
	/// <param name="displayName">Display name.</param>
	public EnumPopupAttribute(System.Type type, bool canCustom = false, string displayName = null)
	{
		this.enumType = type;
		this.displayName = displayName;
		//TODO もしCustomを追加し忘れる場合が多ければ、ここで判断して、エラーを出す
		this.canCustom = canCustom;
	}
}
