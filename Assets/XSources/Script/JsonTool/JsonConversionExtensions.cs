using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using WndEditor;

public static class JsonConversionExtensions
{
    public static IDictionary<string, object> ToDictionary(this JObject json)
    {
        var propertyValuePairs = json.ToObject<Dictionary<string, object>>();
        ProcessJObjectProperties(propertyValuePairs);
        ProcessJArrayProperties(propertyValuePairs);
        return propertyValuePairs;
    }

    private static void ProcessJObjectProperties(IDictionary<string, object> propertyValuePairs)
    {
        var objectPropertyNames = (from property in propertyValuePairs
                                   let propertyName = property.Key
                                   let value = property.Value
                                   where value is JObject
                                   select propertyName).ToList();

        objectPropertyNames.ForEach(propertyName => propertyValuePairs[propertyName] = ToDictionary((JObject)propertyValuePairs[propertyName]));
    }

    private static void ProcessJArrayProperties(IDictionary<string, object> propertyValuePairs)
    {
        var arrayPropertyNames = (from property in propertyValuePairs
                                  let propertyName = property.Key
                                  let value = property.Value
                                  where value is JArray
                                  select propertyName).ToList();

        arrayPropertyNames.ForEach(propertyName => propertyValuePairs[propertyName] = ToArray((JArray)propertyValuePairs[propertyName]));
    }

    public static object[] ToArray(this JArray array)
    {
		return array.ToObject<string[]>().Select(ProcessArrayEntry).ToArray();
    }

    private static object ProcessArrayEntry(object value)
    {
        if (value is JObject)
        {
            return ToDictionary((JObject)value);
        }
        if (value is JArray)
        {
            return ToArray((JArray)value);
        }
        return value;
    }

	static public T ConvertJson<T>(string json)
	{
		T data = JsonConvert.DeserializeObject<T>(json);

		return data;
	}
		

	static public Dictionary<string, object> readJson(string arg)
	{
		var reader = JsonConvert.DeserializeObject<Dictionary<string, object>>(arg);

		return reader;
	}

	static object readContent(object content)
	{
		//Console.WriteLine(content.GetType().Name);
		switch (Type.GetTypeCode(content.GetType()))
		{
		case TypeCode.Int64:
		case TypeCode.String:
		case TypeCode.Boolean:
			return content;

		default:
			if (content.GetType() == typeof(JArray))
			{
				Console.WriteLine("IS JARRAY");
				JArray ja = (JArray)content;
				var reader = JsonConversionExtensions.ToArray(ja);
				//string abList = "";
				for (int i = 0; i < reader.Length; i++)
				{

					//readContent(reader[i]);
					Console.WriteLine(reader[i]);

				}
				return reader;
			}
			else if (content.GetType() == typeof(JObject))
			{
				Console.WriteLine("IS JOBJECT");
				JObject jo = (JObject)content;
				var reader = ToDictionary(jo);
				//JObject jo = (JObject)content;
				//Console.WriteLine (jo.SelectToken("Hp"));
				foreach (var kv in reader)
				{
					Console.WriteLine(kv.Key);
				}
				//CharaData data = new CharaData();
				return reader;
			}
			else
			{
				return null;
			}
		}

	}

	static public object ConvertType(object value, MethodElement.methodType type)
	{
		switch (type)
		{
		case MethodElement.methodType.String:
		case MethodElement.methodType.Int:
		case MethodElement.methodType.Bool:
			return value;

		case MethodElement.methodType.List:
								
			try{
				JArray ja = JArray.Parse(value.ToString());

				List<object> jList = new List<object>();
				foreach(var j in ja){
					jList.Add(j.ToString());
				}

				return jList;
			}
			catch(Exception ex)
			{
				UnityEngine.Debug.Log (ex.Message);
				return null;
			}

		case MethodElement.methodType.Dictionary:
			return null;
		default:
			return null;
		}
	}

	static public string ConvertJson(Dictionary<string, object> dic){

		return JsonConvert.SerializeObject (dic);
	}

	static public Object StringToClass(string className){
		try{
			Type cls = Type.GetType (className,true);
			return Activator.CreateInstance(cls);
		}
		catch(Exception ex){
			UnityEngine.Debug.Log (ex.Message);
			return null;
		}
	}


	static public T ConvertData<T>(string arg){
		var reader = JsonConvert.DeserializeObject<T>(arg);

		return reader;
	}
}
