using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CsvHelper;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;

namespace ClientDataConverter2
{


    class Program
    {
        static string[] columnEnglish = new string[26]{ "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N",
        "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};

        static string spreadsheetId = "1kcz9hIEIW9F8_VubrquiD_ckHwtyF6l7EQYdLxvl6lo";


        static string wanted_path;

        static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        static string ApplicationName = "Google Sheets API .NET Quickstart";

        static void Main(string[] args)
        {
            wanted_path = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));

            File.WriteAllText(wanted_path + "/A.txt", wanted_path);

            //ClientDataMakerNew();

            ReadGoogleSheets();

        }


        static List<string> lineList = new List<string>();

        static string clientData = "/SrcCsv/data";

        static string clientDataTxt = "/SrcCsv/data/ClientData.txt";

        static string pathBac = "/SrcCsv/data/bac";

        static string bacFileName = "/{0}.bac";

        //Dictionary<string, object> dataDic;

        static public void ClientDataLoader()
        {
            string[] txtDataPaths = Directory.GetFiles(clientData, "*.txt", SearchOption.AllDirectories);

            foreach (string path in txtDataPaths)
            {
                textReader(path);
            }

            //ClientDataMakerNew();
            ReadGoogleSheets();
        }

        static public void ReadGoogleSheets() {
            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            SpreadsheetsResource.ValuesResource.GetRequest request;
            Dictionary<string, object> allData = new Dictionary<string, object>();
            var sheetsRequest = service.Spreadsheets.Get(spreadsheetId);
            var spreadsheet = sheetsRequest.Execute();
            var sheetlist = spreadsheet.Sheets
                            .OrderByDescending(s => s.Properties.Title);
            List<string> sheetNames = new List<string>();
            foreach (var sheet in sheetlist)
            {
                if (!sheet.Properties.Title.Contains("^"))
                {
                    sheetNames.Add(sheet.Properties.Title);
                }
            }
            sheetNames.Sort();
            foreach (string sheetName in sheetNames) {

                request = service.Spreadsheets.Values.Get(spreadsheetId, string.Format("{0}!A1:Z20000", sheetName));
                allData.Add(sheetName, SheetToDictionary(request));
            }

            string json = JsonConvert.SerializeObject(allData);
            string jsonData = SortClientData(json);
            File.WriteAllText(wanted_path + clientData + "/ClientData.txt", jsonData);
        }

        static public List<Dictionary<string, object>> SheetToDictionary(SpreadsheetsResource.ValuesResource.GetRequest request) {
            ValueRange response = request.Execute();
            IList<IList<Object>> values = response.Values;
            List<string> columnTitle = new List<string>();
            List<Dictionary<string, object>> rowContents = new List<Dictionary<string, object>>();
            foreach (var v in values[0]) {
                columnTitle.Add(v.ToString());
            }
            if (values != null && values.Count > 0)
            {
                for (int i = 1; i < values.Count; i++)
                {
                    Dictionary<string, object> contents = new Dictionary<string, object>();

                    for (int j = 0; j < columnTitle.Count; j++)
                    {
                        try
                        {
                            contents.Add(columnTitle[j], Convert.ToInt32(values[i][j]));
                        }
                        catch
                        {
                            if (j < values[i].Count)
                            {
                                string content = values[i][j].ToString();
                                if (content != string.Empty && content != null)
                                {
                                    if (content.Substring(0, 1) == "{" && content.Substring(content.Length - 1, 1) == "}")
                                    {
                                        string[] elements = content.Substring(1, content.Length - 2).Split(',');
                                        if (elements[0].Contains(":"))
                                        {
                                            Dictionary<string, object> elementDic = new Dictionary<string, object>();
                                            foreach (string element in elements)
                                            {
                                                string[] elementKv = element.Split(':');
                                                if (elementKv[1] != null && elementKv[1] != string.Empty)
                                                {
                                                    try
                                                    {
                                                        elementDic.Add(elementKv[0].Replace("\"\"", "").ToString(), Convert.ToInt32(elementKv[1]));
                                                    }
                                                    catch {
                                                        if (elementKv[1].Substring(0, 1) == "{" && elementKv[1].Substring(elementKv[1].Length - 1, 1) == "}")
                                                        {
                                                            string[] insides = elementKv[1].Substring(1, elementKv[1].Length - 2).Split('/');
                                                            List<object> insideList = new List<object>();
                                                            foreach (string inside in insides)
                                                            {
                                                                insideList.Add(Convert.ToInt32(inside));
                                                            }
                                                            elementDic.Add(elementKv[0].Replace("\"\"", "").ToString(), insideList);
                                                        }
                                                    }
                                                }
                                            }
                                            contents.Add(columnTitle[j], elementDic);

                                        }
                                        else
                                        {
                                            if (elements[0].Substring(0, 1) == "{" && elements[0].Substring(elements[0].Length - 1, 1) == "}")
                                            {
                                                List<List<object>> elementDoubleList = new List<List<object>>();
                                                foreach (string element in elements)
                                                {
                                                    string[] insides = element.Substring(1, element.Length - 2).Split('/');
                                                    List<object> insideList = new List<object>();
                                                    foreach (string inside in insides)
                                                    {
                                                        insideList.Add(Convert.ToInt32(inside));
                                                    }
                                                    elementDoubleList.Add(insideList);
                                                }
                                                contents.Add(columnTitle[j], elementDoubleList);
                                            }
                                            else
                                            {
                                                List<object> elementList = new List<object>();
                                                foreach (string element in elements)
                                                {
                                                    elementList.Add(Convert.ToInt32(element));
                                                }
                                                contents.Add(columnTitle[j], elementList);
                                            }
                                        }

                                    }
                                    else
                                    {
                                        contents.Add(columnTitle[j], content);
                                    }
                                }
                                else
                                {
                                    contents.Add(columnTitle[j], null);
                                }
                            }
                            else {
                                contents.Add(columnTitle[j], null);
                            }
                        }
                    }
                    rowContents.Add(contents);
                }
            }
            else
            {
                Debug.WriteLine("No data found.");
            }

            return rowContents;
        }

        static public void textReader(string sourcePath)
        {
            StreamReader sr = new StreamReader(sourcePath);
            lineList = new List<string>();

            List<string> origLineList = new List<string>();
            string line = string.Empty;
            while ((line = sr.ReadLine()) != null)
            {
                if (HasContent(line))
                {
                    origLineList.Add(line.Replace("\t", ","));
                }
                else
                {
                    break;
                }
            }

            try
            {
                ClearBlankCol(origLineList, sourcePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(Path.GetFileName(sourcePath) + " , " + ex.Message);
            }
        }

        static public bool HasContent(string inputString)
        {

            Regex regex = new Regex(@"^[A-Za-z0-9]");
            return regex.IsMatch(inputString);
        }

        static void ClearBlankCol(List<string> inputList, string sourcePath)
        {
            string titleContent = inputList[0];
            int clearCount = 0;

            while (titleContent.Substring(titleContent.Length - 1, 1) == ",")
            {
                titleContent = titleContent.Substring(0, titleContent.Length - 1);
                clearCount++;
            }

            foreach (string content in inputList)
            {
                lineList.Add(content.Substring(0, content.Length - clearCount));
            }




            CsvMaker(sourcePath);
        }

        static public void CsvMaker(string srcPath)
        {
            var csv = new StringBuilder();

            //in your loop
            string writeLine = "";

            foreach (string content in lineList)
            {
                if (writeLine == "")
                {
                    writeLine = content;
                }
                else
                {
                    writeLine = writeLine + "\n" + content;
                }
            }

            csv.AppendLine(writeLine);

            //after your loop
            File.WriteAllText(srcPath.Replace(".txt", ".csv"), csv.ToString());
        }

        static public void ClientDataMakerNew()
        {
            //string projectPath = Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()));
           // string dataPath = projectPath.Replace("bin/Debug/test.app", "SrcCsv/data");
            string[] txtDataPaths = Directory.GetFiles(wanted_path + clientData, "*.csv", SearchOption.AllDirectories);
            Dictionary<string, object> clientDataDic = new Dictionary<string, object>();

            foreach (string path in txtDataPaths)
            {
                clientDataDic.Add(Path.GetFileNameWithoutExtension(path), csvReader(path));
            }

            string json = JsonConvert.SerializeObject(clientDataDic);

            Debug.WriteLine("Csv");
            Debug.WriteLine(json);
            string jsonData = SortClientData(json);


            /*if (File.Exists(clientDataTxt))
            {
                string[] bacFilePath = Directory.GetFiles(wanted_path + clientData + "/bac", "*.bac", SearchOption.TopDirectoryOnly);

                Console.WriteLine(wanted_path + clientData + "/bac/" + (bacFilePath.Length + 1).ToString() + ".bac");
                File.Copy(wanted_path + clientData + "/ClientData.txt", wanted_path + clientData + "/bac/" + (bacFilePath.Length + 1).ToString() + ".bac");
            }
            File.Delete(wanted_path + clientData + "/ClientData.txt");


            File.WriteAllText(wanted_path + clientData + "/ClientData.txt", jsonData);*/
        }

        static string SortClientData(string jdata)
        {
            Dictionary<string, int> sortDic = new Dictionary<string, int>();
            string sortedData = string.Empty;
            int subStart = 0;
            int checkStart = 0;
            int subEnd = 0;
            int bigParent = 0;
            int midParent = 0;
            int parent;
            int midParentCount = 0;
            while ((subStart + subEnd) <= jdata.Length)
            {
                parent = bigParent + midParent;
                subEnd++;
                if (jdata.Substring(checkStart, 1) == "{")
                {
                    if (bigParent == 0)
                    {
                        sortDic.Add(jdata.Substring(subStart, subEnd), parent);
                        subStart = subStart + subEnd;
                        checkStart = subStart;
                        subEnd = 0;
                    }
                    else
                    {
                        checkStart++;
                    }

                    bigParent++;
                    parent = bigParent + midParent;
                }
                else if (jdata.Substring(checkStart, 1) == "}")
                {
                    bigParent--;
                    parent = bigParent + midParent;
                    if (bigParent == 1)
                    {
                        if (jdata.Substring(checkStart + 1, 1) == ",")
                        {
                            sortDic.Add(jdata.Substring(subStart, subEnd + 1), parent);
                            subStart = subStart + subEnd + 1;
                        }
                        else
                        {
                            sortDic.Add(jdata.Substring(subStart, subEnd), parent);
                            subStart = subStart + subEnd;
                        }

                        checkStart = subStart;
                        subEnd = 0;
                    }
                    else
                    {
                        checkStart++;
                        if (checkStart == jdata.Length)
                        {
                            break;
                        }
                    }
                }
                else if (jdata.Substring(checkStart, 1) == "[")
                {
                    if (midParent == 0)
                    {

                        if (jdata.Substring(checkStart + 1, 1) == "]")
                        {
                            sortDic.Add(jdata.Substring(subStart, subEnd + 2), parent);
                            midParent--;
                            subStart = subStart + subEnd + 2;
                        }
                        else
                        {
                            sortDic.Add(jdata.Substring(subStart, subEnd), parent);
                            subStart = subStart + subEnd;
                        }

                        checkStart = subStart;

                        subEnd = 0;
                    }
                    else
                    {
                        checkStart++;
                    }

                    midParent++;
                    parent = bigParent + midParent;
                }
                else if (jdata.Substring(checkStart, 1) == "]")
                {
                    midParent--;
                    parent = bigParent + midParent;
                    if (midParent == 0)
                    {
                        if (jdata.Substring(checkStart + 1, 1) == ",")
                        {
                            sortDic.Add(jdata.Substring(subStart, subEnd + 1) + "mid" + midParentCount.ToString(), parent);
                            midParentCount++;

                            subStart = subStart + subEnd + 1;
                            checkStart = subStart;
                        }
                        if (jdata.Substring(checkStart + 1, 1) == "}")
                        {
                            sortDic.Add(jdata.Substring(subStart, subEnd), parent);

                            sortDic.Add("}", 0);
                            break;
                        }
                        subEnd = 0;
                    }
                    else
                    {
                        checkStart++;
                    }
                }
                else
                {
                    checkStart++;
                }
            }
            foreach (KeyValuePair<string, int> kv in sortDic)
            {
                if (kv.Key.Length > 5)
                {
                    if (kv.Key.Substring(0, 5) == "],mid")
                    {
                        sortedData = sortedData + AddTab(kv.Key.Substring(0, 2).Replace("null", "0") + "\n", kv.Value);
                    }
                    else
                    {
                        sortedData = sortedData + AddTab(kv.Key.Replace("null", "0") + "\n", kv.Value);
                    }
                }
                else if (kv.Key.Length == 1)
                {
                    if (kv.Key == "}")
                    {
                        sortedData = sortedData + AddTab(kv.Key.Replace("null", "0"), kv.Value);
                    }
                    else
                    {
                        sortedData = sortedData + AddTab(kv.Key.Replace("null", "0") + "\n", kv.Value);
                    }
                }
                else
                {
                    sortedData = sortedData + AddTab(kv.Key + "\n", kv.Value);
                }
            }
            return sortedData;
        }

        static string AddTab(string org, int tabCount)
        {
            string addedData = string.Empty;
            for (int i = 0; i < tabCount; i++)
            {
                addedData = addedData + "\t";
            }
            addedData = addedData + org;

            return addedData;
        }


        static List<object> csvReader(string srcPath)
        {
            //using (FileStream fileStream = new FileStream(srcPath, FileMode.Open, FileAccess.Read))
            StreamReader sr = new StreamReader(srcPath);

            string[] valueKey = sr.ReadLine().Split(',');

            List<object> dataParameter = new List<object>();

            CsvReader csv = new CsvReader(sr);

            csv.Configuration.HasHeaderRecord = true;



            //Console.WriteLine(srcPath);

            while (csv.Read())
            {
                Dictionary<string, object> parameterContent = new Dictionary<string, object>();


                csv.ReadHeader();



                for (int i = 0; i < valueKey.Length; i++)
                {
                    string content = "";
                    try
                    {
                        content = csv.GetField<string>(i);
                    }
                    catch
                    {
                        content = "";
                    }

                    int cv = 0;

                    if (Int32.TryParse(content, out cv))
                    {
                        parameterContent.Add(valueKey[i], cv);
                    }
                    else
                    {
                        if (content == string.Empty)
                        {
                            parameterContent.Add(valueKey[i], null);
                        }
                        else
                        {
                            if (content.Substring(0, 1) == "{" && content.Substring(content.Length - 1, 1) == "}")
                            {
                                string[] elems = content.Substring(1, content.Length - 2).Split(',');
                                if (!elems[0].Contains(":"))
                                {
                                    int ev = 0;

                                    List<object> elemList = new List<object>();
                                    foreach (var elem in elems)
                                    {
                                        if (Int32.TryParse(elem, out ev))
                                        {
                                            elemList.Add(ev);
                                        }
                                        else
                                        {
                                            if (elem == "")
                                            {
                                                elemList.Add(null);
                                            }
                                            else {
                                                string[] a = elem.Substring(1, elem.Length - 2).Split(',');
                                                foreach (string s in a) {
                                                    string[] elemInside = elem.Substring(1, elem.Length - 2).Split('/');
                                                    List<int> intInsige = new List<int>();
                                                    foreach (string inside in elemInside)
                                                    {
                                                        intInsige.Add(Int32.Parse(inside));
                                                    }
                                                    elemList.Add(intInsige);
                                                }
                                            }
                                        }
                                    }

                                    parameterContent.Add(valueKey[i], elemList);
                                }
                                else
                                {
                                    Dictionary<string, object> elemDic = new Dictionary<string, object>();
                                    foreach (var elem in elems)
                                    {
                                        if (elem.Contains(":"))
                                        {
                                            int ev = 0;
                                            string[] a = elem.Split(':');

                                            if (Int32.TryParse(a[1], out ev))
                                            {
                                                elemDic.Add(a[0].Trim().Replace("\"", ""), ev);
                                            }
                                            else
                                            {
                                                if (elem == "")
                                                {
                                                    elemDic.Add(a[0].Trim().Replace("\"", ""), null);
                                                }
                                                else
                                                {

                                                    if (a[1] != "{}" && a[1] != string.Empty && a[1] != null && a[1].Length > 1)
                                                    {
                                                        string[] elemInside = a[1].Substring(1, a[1].Length - 2).Split('/');
                                                        List<int> intInsige = new List<int>();
                                                        foreach (string s in elemInside)
                                                        {
                                                            intInsige.Add(Int32.Parse(s));
                                                        }
                                                        elemDic.Add(a[0].Trim().Replace("\"", ""), intInsige);
                                                    }
                                                    else
                                                    {
                                                        //elemDic.Add(a[0].Trim().Replace("\"", ""), "");
                                                    }
                                                }

                                            }
                                        }
                                    }

                                    if (elemDic.Count > 0) {
                                        parameterContent.Add(valueKey[i], elemDic);

                                    }
                                }
                            }
                            else
                            {
                                parameterContent.Add(valueKey[i], content);
                            }
                        }
                    }
                }
                dataParameter.Add(parameterContent);
            }


            sr.Close();
            return dataParameter;
        }
    }
}
