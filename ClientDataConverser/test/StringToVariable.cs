using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;

namespace test
{
    public class StringToVariable
    {
		public virtual string[] Property()
		{
			List<string> ListVariableAll = new List<string>();
			foreach (PropertyInfo n in this.GetType().GetProperties())
				if (n.Name != "Item" && n.Name != "Property")
					ListVariableAll.Add(n.Name);

			return ListVariableAll.ToArray();
		}

		public virtual object GetData()
		{ return this; }

		[JsonIgnore]
		public virtual object this[string key]
		{
			set
			{
				Type t = this.GetType();
				PropertyInfo pro = t.GetProperty(key);
				if (pro != null)
					pro.SetValue(this, value, null);
				else
					throw new Exception("參數:[" + key + "]不存在");
			}
			get
			{
				Type t = this.GetType();
				PropertyInfo pro = t.GetProperty(key);

				if (pro != null)
					return pro.GetValue(this, null) == null ? "" : pro.GetValue(this, null).ToString();
				else
					return null;
			}
		}
    }
}
