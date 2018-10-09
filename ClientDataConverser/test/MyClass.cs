using System;
using System.IO;
using System.Collections.Generic;
namespace test
{
    public class MyClass
    {
        string toJson;
        string srcPath = "/Users/chien/Projects/test/test/SrcCsv/banner.csv";
        public MyClass()
        {
            toJson = "{[[0]]}";
            List<string> srList = new List<string>();
            StreamReader sr = new StreamReader(srcPath);
			string line = string.Empty;
			while ((line = sr.ReadLine()) != null)
			{
				srList.Add(line);
			}

            CsvReader((srList));
        }

        public void CsvReader(List<string> srclist){
            string[] title = srclist[0].Split(',');
            string js = string.Empty;
            for (int i = 1; i < srclist.Count;i++){
                string[] content = srclist[i].Split(',');
                if (i == 1)
				{
					js = "{" + ConverterJsonType(title, content) + "}";
				}
				else
				{
					js = js + "," + "{" + ConverterJsonType(title, content) + "}";
				}
            }
            toJson = string.Format(toJson, js);

            StreamWriter sw = new StreamWriter("/Users/chien/Projects/test/test/SrcCsv/banner.txt");
            sw.WriteLine(toJson);            // 寫入文字
            sw.Close();
		}

        public string ConverterJsonType(string[] title, string[] content)
        {
            string js = string.Empty;
            for (int i = 0; i < title.Length;i++){
                if (i == 0)
                {
                    js = "\"" + title[i] + "\"=\"" + content[i] + "\"";
                }
                else{
                    js = js + ",\"" + title[i] + "\"=\"" + content[i];
                }
            }
            return js;
        }
    }
}
