using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace test
{
    public class JsonConvertExtension
    {
        static public T ConvertJson<T>(string json)
        {
            JObject jo = JObject.Parse(json);

            T data = JsonConvert.DeserializeObject<T>(json);

            return data;
        }

        static public void readJsonKey(string arg, string key = "")
        {
            var reader = JsonConvert.DeserializeObject<Dictionary<string, object>>(arg);
            if (key == "")
            {
                foreach (var kv in reader)
                {
                    //Console.WriteLine(kv.Key + ":" + kv.Value+" , "+kv.Value.GetType());
                    //Console.WriteLine(kv.Key);
                    //readContent(kv.Value);
                    if (readContent(kv.Value).GetType() != typeof(object[]))
                    {
                        Console.WriteLine(readContent(kv.Value));
                    }
                    /*else
                    {
                        foreach (var v in (Object[])readContent(kv.Value))
                        {
                            Console.WriteLine("Array Value : " + v);
                        }
                    }*/
                }
            }
            else
            {
                if (reader.ContainsKey(key))
                {
                    Console.WriteLine(reader[key]);
                }
                else
                {
                    Console.WriteLine("false");

                }
            }

        }

        static public List<object> readJson(string arg, MethodElement[] elements)
        {
            List<object> paramList = new List<object>();
            var reader = JsonConvert.DeserializeObject<Dictionary<string, object>>(arg);
            foreach (MethodElement element in elements)
            {
                paramList.Add(reader[element.methodKey]);
            }
            return paramList;
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
                        var reader = JsonConversionExtensions.ToDictionary(jo);
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

        static object ConvertType(object value, MethodElement.methodType type)
        {
            switch (type)
            {
                case MethodElement.methodType.String:
                case MethodElement.methodType.Int:
                case MethodElement.methodType.Bool:
                    return value;

                case MethodElement.methodType.List:
                    if (value.GetType() == typeof(JArray))
                    {
                        Console.WriteLine("IS JARRAY");
                        JArray ja = (JArray)value;
                        var reader = JsonConversionExtensions.ToArray(ja);
                        List<object> jList = new List<object>(reader);
                        return jList;
                    }
                    else
                    {
                        return null;
                    }

                case MethodElement.methodType.Dictionary:
                    return null;
                default:
                    return null;
            }
        }
    }
}
