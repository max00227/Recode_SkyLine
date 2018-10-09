using AppKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Linq;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Data.SqlClient;
using CsvHelper;
using CsvHelper.Configuration;

namespace test
{
    static class MainClass
    {
        static Dictionary<string, object> parameter;

        //static DataRow[] rows;
        static string toJson;

        //static string srcPath = "/Users/chien/Projects/test/test/SrcCsv/banner.csv";
		//static string srcPath = "/Users/chien/Projects/test/test/SrcCsv/help_section.csv";
        //static string srcPath = "/Users/chien/Projects/test/test/SrcCsv/startup_{0}.txt";
        static string srcPath = "/Users/chien/Projects/test/test/SrcCsv/startup-u5_{0}.json";

        static string srcPath2 = "/Users/chien/Projects/test/test/SrcCsv/testCsv.csv";

		static string convertDir = "/Users/chien/Projects/test/test/SrcCsv/helpSection/";

        static string clientData = "/Users/chien/Projects/test/test/SrcCsv/data";


        static string fileNameH = "text_help_section_{0}.txt";

        static string abPath = "/Users/chien/bbs_server/root/static/unity/ab/{0}/";

        static string jsonSrc = "/Users/chien/bbs_server_new_ver_t/root/api/{0}/startup-u5.json";

        //static string platform = "ios";

        static string dirPath = "/Users/chien/Documents/RSC/AB/180508";

		static Dictionary<string, string> help;

        static string[] strArray = { "{1,2,5,6,2,7,2,7,1}", "1","adfadf"};

        static string textureSrc = "/Users/chien/Documents/暫存圖/韓版/all/Assets";

        static Dictionary<string, string> origPath;

        static int nameIdx;

        static string textDirPath = "/Users/chien/Documents/暫存圖/韓版/all/Combine";

        static int[] intArray = { 1, 5, 7, 5, 3, 7, 5, 1 };

        static Dictionary<string, object> testDic;

        static string helpSrc = "/Users/chien/Projects/test/test/SrcCsv/help_section.csv";


        static List<object> testList;

        /*class CharaData{
            public string name = string.Empty;
            public int rank = 0;
            public int speed = 0;
        }*/

        static void Main()
        {
            //SkillLargeData data = new SkillLargeData();
            //LargeDataBase db = new LargeDataBase();
            //CombineTexture();
            //SplitTexture();
            // DeleteEmptyfolder();
            /*Type type = typeof(CharaData);

            FieldInfo[] myFieldInfo = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            foreach(var fi in myFieldInfo){
                Console.WriteLine(fi.Name);
            }*/
            //ClientDataBuilder.ClientDataLoader();
            //ClientDataLoader.readJason();
            //Console.WriteLine(IsChina("我是誰게"));
            //CheckTxt();
            //csvR();
            ClientDataBuilder.ClientDataMakerNew();

        }

        static public void CheckTxt(){
            Console.WriteLine("CheckTxt");

            StreamReader sr = new StreamReader("/Users/chien/Projects/test/test/SrcCsv/Korea.txt");
            string line = string.Empty;
            Dictionary<string, string> languege = new Dictionary<string, string>();
            while ((line = sr.ReadLine()) != null)
            {
                string[] namePath = line.Split('=');
                if (namePath.Length == 2)
                {
                    if (IsChina(namePath[1].Trim()))
                    {
                        Console.WriteLine(line);
                    }
                }
            }

            /*foreach(KeyValuePair<string,string> kv in languege){
                if(IsChina(kv.Value)){
                    
                }
            }*/
        }



        static bool IsChina (string CString){
            bool boolValue = false;
            for (int i = 0; i < CString.Length;i++){
                if (Convert.ToChar(CString.Substring(i, 1)) <= 0x9fbb && Convert.ToChar(CString.Substring(i, 1))>= 0x4e00){
                    boolValue = true;
                }
                else{
                    return boolValue = false;
                }
            }

            return boolValue;
        }

        /*static public bool IsChina（string CString）{


            bool BoolValue = false;

            for （int i = 0; i<CString.Length; i++）{
                if （Convert.ToInt32（Convert.ToChar（CString.Substring（i, 1））） < Convert.ToInt32（Convert.ToChar（128））{
                BoolValue = false;
                }
                else{
                    return BoolValue = true;
                }
            }

            return BoolValue;

        }*/

        static void SplitTexture(){
            string listTxt = textDirPath + "/textList.txt";
            StreamReader sr = new StreamReader(listTxt);

            string line = string.Empty;
            while ((line = sr.ReadLine()) != null)
            {
                string[] namePath = line.Split(',');
                string combPath = textDirPath + "/" + namePath[0]+".png";
                string splPath = namePath[1].Replace("Assets", "Splits");
                if(File.Exists(combPath)){
                    File.Copy(combPath, splPath, true);
                }
            }
            Console.WriteLine("COMPLETE");
        }

        static void DeleteEmptyfolder(){
            string[] folderPath = Directory.GetDirectories(textureSrc.Replace("Assets", "Splits"), "*", SearchOption.AllDirectories);
            foreach(string fName in folderPath){
                if (Directory.Exists(fName))
                {
                    string[] textPath = Directory.GetFiles(fName, "*.png", SearchOption.AllDirectories);
                    if (textPath.Length <= 0)
                    {
                        Directory.Delete(fName, true);
                    }
                }
            }
            Console.WriteLine("COMPLETE");
        }

        static void CombineTexture(){
            origPath = new Dictionary<string, string>();
            string[] textPath = Directory.GetFiles(textureSrc, "*.png", SearchOption.AllDirectories);
            foreach (string t in textPath)
            {
                nameIdx = 0;
                while (checkDic(Path.GetFileNameWithoutExtension(t), Path.GetFullPath(t), nameIdx) == false)
                {
                    nameIdx++;
                }
            }
            foreach (var e in origPath)
            {
                File.Copy(e.Value, textDirPath + "/" + e.Key + ".png", true);
                //Console.WriteLine(e.Key + " , " + e.Value);

            }

            string line = "";
            foreach (var e in origPath)
            {
                if (line == "")
                {
                    line = e.Key + "," + e.Value;
                }
                else
                {
                    line = line + "\n" + e.Key + "," + e.Value;
                }

            }
            File.WriteAllText(textDirPath + "/textList.txt", line);

            Console.WriteLine("COMPLETE");
        }

        static bool checkDic(string fileName, string fullPath, int index){
            try{
                if (index == 0)
                    origPath.Add(fileName, fullPath);
                else{
                    origPath.Add(fileName + index.ToString(), fullPath);
                }
                return true;
            }
            catch{
                return false;
            }
        }

        /*static void ArrayToList(){
            CharaData[] _charaData = new CharaData[4];


            for (int i = 0; i < _charaData.Length; i++)
            {
                _charaData[i] = new CharaData();
                _charaData[i].name = (1200 + (i + 1) * 3).ToString();
                _charaData[i].rank = ((i + 1) * 3);
                _charaData[i].speed = ((i + 1) * 122);
            }
            List<CharaData> cl = new List<CharaData>(_charaData);

            foreach (CharaData c in cl)
            {
                Console.WriteLine(c.name + " , " + c.rank + " , " + c.speed);
            }
            //csvReader();
            //JsonTxtReader("ios");
            //JsonTxtReader("android");
        }

        static private CharaDataEntity ConvertCharaData(data) 

        class CharaData : CharaDataEntity{
            
        }*/

        static void JsonTxtReader(string platform)
        {
            if(Directory.Exists(dirPath)==false){
                Directory.CreateDirectory(dirPath);
            }
           
            if (File.Exists(string.Format(jsonSrc, platform)))
            {
                string readText = File.ReadAllText(string.Format(jsonSrc, platform));

                var reader = JsonConvert.DeserializeObject<Dictionary<string, object>>(readText);

                //var reader2 = readContent(reader["result"]);
                JObject jo = (JObject)reader["result"];
                var reader2 = JsonConversionExtensions.ToDictionary(jo);
                var r2 = (Dictionary<string, object>)reader2;
                foreach (var kv in r2)
                {
                    if (kv.Key == "assetbundles")
                    {
                        var ja = (Object[])kv.Value;

                        //readContent((JArray)kv.Value);
                        Console.WriteLine(ja.Length);
                        JArray a = (JArray)JToken.FromObject(kv.Value);
                        //readContent(a);

                        var reader3 = JsonConversionExtensions.ToArray(a);
                        //string abList = "";
                        for (int i = 0; i < reader3.Length; i++)
                        {
                            //readContent(reader[i]);
                            // Console.WriteLine(reader[i]);
                            var dic = (Dictionary<string, object>)reader3[i];
                            //Console.WriteLine(dic["name"]);
                            if (File.Exists(string.Format(abPath, platform) + dic["name"].ToString() + ".unity3d") == true)
                            {
                                //Console.WriteLine(string.Format(abPath, platform) + dic["name"].ToString() + ".unity3d");
                                if (Directory.Exists(dirPath + dic["version"]) == false)
                                {
                                    Directory.CreateDirectory(dirPath + string.Format("/{0}/{1}", platform, dic["version"]));
                                }

                                File.Copy(string.Format(abPath, platform) + dic["name"].ToString() + ".unity3d", dirPath + string.Format("/{0}/{1}/", platform, dic["version"]) + dic["name"].ToString() + ".unity3d", true);

                            }
                            if (File.Exists(string.Format(abPath, platform) + dic["name"].ToString() + ".unity3d") == false)
                            {
                                Console.WriteLine("Not Exist : " + string.Format(abPath, platform) + dic["name"].ToString() + ".unity3d");
                            }
                        }
                        // reader;
                    }

                }
            }
            else{
                Console.WriteLine("File Not Exist");
            }
        }

       

        static void TestJson(){
			parameter = new Dictionary<string, object>();
			testDic = new Dictionary<string, object>();
			testList = new List<object>();
			for (int i = 0; i < strArray.Length; i++)
			{
				testList.Add(strArray[i]);
			}
			int dataCount = 0;
			for (int i = 0; i < strArray.Length; i++)
			{
				dataCount++;
				parameter.Add("Data" + i.ToString(), readData(strArray[i]));
			}
			//parameter.Add("Data" + dataCount.ToString(), testList);


			/*parameter = new Dictionary<string, object>();
            for (int i = 0; i < strArray.Length;i++){
                parameter.Add("str"+i.ToString(),strArray[i]);
            }

            for (int i = 0; i < intArray.Length; i++)
            {
                parameter.Add("int" + i.ToString(), intArray[i]);
            }*/

			string json = JsonConvert.SerializeObject(parameter);

			Console.WriteLine(json);
			readJson(json);
			/*readJson(json,"int0");
            readJson(json, "int10");*/
			/*JsonTextReader reader = new JsonTextReader(new StringReader(json));
            while (reader.Read())
            {
                if (reader.Value != null)
                {
                    Console.WriteLine("Token: {0}, Value: {1}", reader.TokenType, reader.Value);
                }
                else
                {
                    Console.WriteLine("Token: {0}", reader.TokenType);
                }
            }*/
		}

        static void readJson(string arg, string key = "")
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
                    else
                    {
                        foreach(var v in (Object[])readContent(kv.Value)){
                            Console.WriteLine("Array Value : " + v);
                        }
                    }
                }
            }
            else{
                if(reader.ContainsKey(key)){
                    Console.WriteLine(reader[key]); 
                }
                else{
					Console.WriteLine("false");

				}
            }

        }

        static object readData(string data){
            if(data.Contains("{") && data.Contains("}")){
                return ConvertArray(data.Replace("{", "").Replace("}", ""));
            }
            else{
                int i;
                if(int.TryParse(data, out i)){
                    return i;
                }
                else{
                    return data;
                }
            }
        }

        static object ConvertArray(string content){
			try
			{
				int[] cov = content.Split(',').Select(int.Parse).ToArray();
				foreach (int i in cov)
				{
					//Console.WriteLine(i + " , " + i.GetType().Name);
				}
				return cov;
			}
			catch
			{
				string[] cov = content.Split(',');
				foreach (string s in cov)
				{
					//Console.WriteLine(s + " , " + s.GetType().Name);
				}
                return cov;
			}

		}

        static object readContent(object content){
            //Console.WriteLine(content.GetType().Name);
            switch(Type.GetTypeCode(content.GetType())){
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
                    else if (content.GetType() == typeof(JObject)) {
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
                    else{
                        return null;
                    }
            }

        }


        static void csvReader()
        {
            help = new Dictionary<string, string>();
            //using (FileStream fileStream = new FileStream(srcPath, FileMode.Open, FileAccess.Read))
            StreamReader sr = new StreamReader(srcPath2);
            string[] valueKey = sr.ReadLine().Split(',');
            List<int> indexList = new List<int>();
            Dictionary<string, object> dataParameter = new Dictionary<string, object>();

            CsvReader csv = new CsvReader(sr);

            csv.Configuration.HasHeaderRecord = true;

            for (int i = 0; i < valueKey.Length; i++)
            {
                if(!valueKey[i].Contains("*")){
                    indexList.Add(i);
                }
            }
            while (csv.Read())
            {
                Dictionary<string, object> parameterContent = new Dictionary<string, object>();

                /*string dataContent = "";
                for (int i = 0; i < valueKey.Length; i++)
                {
                    if (dataContent == "")
                    {
                        dataContent = csv.GetField<string>(i);
                    }
                    else
                    {
                        dataContent = dataContent+","+csv.GetField<string>(i);
                    }
                }*/
                csv.ReadHeader();
                //var boolField = csv.GetField<bool>("HeaderName");
                //Console.WriteLine(dataContent);


                for (int i = 1; i < indexList.Count; i++)
                {
                    int v = 0;
                    if (Int32.TryParse(csv.GetField<string>(indexList[i]), out v))
                    {
                        parameterContent.Add(valueKey[indexList[i]], v);
                    }
                    else
                    {
                        parameterContent.Add(valueKey[indexList[i]], csv.GetField<string>(indexList[i]));
                    }
                }
                dataParameter.Add(csv.GetField<string>(0), parameterContent);
            }

            string json = JsonConvert.SerializeObject(dataParameter);

            Console.WriteLine(json);

            //readJson(json);
        }


        static void csvR(){
			help = new Dictionary<string, string>();
			//using (FileStream fileStream = new FileStream(srcPath, FileMode.Open, FileAccess.Read))
            StreamReader sr = new StreamReader(helpSrc);
			string[] valueKey = sr.ReadLine().Split(',');


			CsvReader csv = new CsvReader(sr);

			csv.Configuration.HasHeaderRecord = true;
			// csv.Configuration.RegisterClassMap<CSVFileDefinitionMap>();
			int index1 = 0;
			int index2 = 0;
			for (int i = 0; i < valueKey.Length; i++)
			{
				if (valueKey[i] == "id")
				{
					index1 = i;
				}
				else if (valueKey[i] == "body")
				{
					index2 = i;
				}
			}
			//Console.WriteLine(csv.GetFieldIndex("id", 0, true));
			while (csv.Read())
			{
				/*string dataContent = "";
                for (int i = 0; i < valueKey.Length; i++)
                {
                    if (dataContent == "")
                    {
                        dataContent = csv.GetField<string>(i);
                    }
                    else
                    {
                        dataContent = dataContent+","+csv.GetField<string>(i);
                    }
                }*/
				csv.ReadHeader();
				//var boolField = csv.GetField<bool>("HeaderName");
				//Console.WriteLine(dataContent);
				help.Add(csv.GetField<string>(index1), csv.GetField<string>(index2));

			}

			foreach (KeyValuePair<string, string> item in help)
			{
				Console.WriteLine(item.Key + "," + item.Value);
				File.WriteAllText(convertDir + string.Format(fileNameH, item.Key), item.Value);
			}
        }

        sealed class CSVFileDefinitionMap : ClassMap<CSVFileDefinition>
		{
			public CSVFileDefinitionMap()
			{
				Map(m => m.FRM_ID).Name("FARM ID");
				Map(m => m.FRM_OWNER).Name("FARM OWNER ");
                /*for (int i = 0; i < mapName.Length; i++)
                {
                    Map(m=>m.MAP_NAME[i]).Name(mapName[i]);
                }*/
			}
		}

		class CSVFileDefinition
		{
			public string FRM_ID { get; set; }
			public string FRM_OWNER { get; set; }
           // public string[] MAP_NAME { get; set; }
		}


		/*public virtual string[] Property()
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
		}*/



		class DataRecord
		{
			//Should have properties which correspond to the Column Names in the file
			//i.e. CommonName,FormalName,TelephoneCode,CountryCode
			/*public String id { get; set; }
            public String event_id { get; set; }
			public String explanation_type { get; set; }
			public String title { get; set; }
            public String explanation { get; set; }*/
		}

        static void CopyUnidy3D(){
            string copyPath = "/Users/chien/Documents/RSC/AB/台版AB/扭蛋/keep";
			//string copyPath = "/Users/chien/Documents/RSC/AB/台版AB/扭蛋/test";
			string[] dataPaths = Directory.GetFiles(copyPath, "*.unity3d", SearchOption.AllDirectories);
            foreach(string dataPath in dataPaths){
                if(dataPath.Contains("Android")){
					string fileName = Path.GetFileName(dataPath);
                    File.Copy(dataPath,copyPath.Replace("keep","keepSplit/Android/"+Path.GetFileName(dataPath)),true);
                }
                else{
					//Console.WriteLine(dataPath);
					string fileName = Path.GetFileName(dataPath);
					File.Copy(dataPath, copyPath.Replace("keep", "keepSplit/iOS/" + Path.GetFileName(dataPath)), true);
				}
            }
        }

        static void myCsvReader(List<string> srclist)
		{
			string[] title = srclist[0].Split(',');
			string js = string.Empty;
			for (int i = 1; i < srclist.Count; i++)
			{
				string[] content = srclist[i].Split(',');
				if (i == 1)
				{
					js = "{\n" + ConverterJsonType(title, content) + "\n}";
				}
				else
				{
					js = js + ",\n" + "{\n" + ConverterJsonType(title, content) + "\n}";
				}
			}
            //Console.WriteLine("{\n\"banner\":[\n" + js + "\n]\n}");
            toJson = "{\n\""+Path.GetFileNameWithoutExtension(srcPath)+"\":[\n" + js + "\n]\n}";

            StreamWriter sw = new StreamWriter("/Users/chien/Projects/test/test/SrcCsv/"+Path.GetFileNameWithoutExtension(srcPath)+".txt");
			sw.WriteLine(toJson);            // 寫入文字
			sw.Close();
		}

        static string ConverterJsonType(string[] title, string[] content)
		{
			string js = string.Empty;
			for (int i = 0; i < title.Length; i++)
			{
				if (i == 0)
				{
                    js = "\"" + title[i] + "\":" + CheckContent(content[i]);
				}
				else
				{
                    js = js + ",\n\"" + title[i] + "\":" + CheckContent(content[i]);
				}
			}
			return js;
		}

        static string CheckContent(string content){
            if (content == string.Empty || content == null)
            {
                return "\"\"";
            }
            else
            {
                return content;
            }
        }
    }
}
