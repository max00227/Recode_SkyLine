using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;


namespace test
{
    public class ClientDataBuilder
    {
        static List<string> lineList = new List<string>();

        static string clientData = "/Users/chien/Projects/test/test/SrcCsv/data";

        static string clientDataTxt = "/Users/chien/Projects/test/test/SrcCsv/data/ClientData.txt";

        static string pathBac = "/Users/chien/Projects/test/test/SrcCsv/data/bac";

        static string bacFileName = "/{0}.bac";

        //Dictionary<string, object> dataDic;

        static public void ClientDataLoader()
        {
            string[] txtDataPaths = Directory.GetFiles(clientData, "*.txt", SearchOption.AllDirectories);

            foreach (string path in txtDataPaths)
            {
                textReader(path);
            }

            ClientDataMaker();
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

        static public void ClientDataMaker()
        {
            string[] txtDataPaths = Directory.GetFiles(clientData, "*.csv", SearchOption.AllDirectories);
            Dictionary<string, object> clientDataDic = new Dictionary<string, object>();

            foreach (string path in txtDataPaths)
            {
                clientDataDic.Add(Path.GetFileNameWithoutExtension(path),csvReader(path));
            }

            string json = JsonConvert.SerializeObject(clientDataDic);

            //Console.WriteLine(json);
            if(File.Exists(clientDataTxt)){
                string[] bacFilePath = Directory.GetFiles(pathBac, "*.bac", SearchOption.TopDirectoryOnly);
                File.Copy(clientDataTxt, string.Format(pathBac + bacFileName, bacFilePath.Length + 1));
            }
            File.Delete(clientDataTxt);

           
            File.WriteAllText(clientDataTxt, json);
        }

        static public void ClientDataMakerNew()
        {
            string projectPath = Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()));
            string dataPath = projectPath.Replace("bin/Debug/test.app", "SrcCsv/data");
            string[] txtDataPaths = Directory.GetFiles(dataPath, "*.csv", SearchOption.AllDirectories);
            Dictionary<string, object> clientDataDic = new Dictionary<string, object>();

            foreach (string path in txtDataPaths)
            {
                clientDataDic.Add(Path.GetFileNameWithoutExtension(path), csvReader(path));
            }

            string json = JsonConvert.SerializeObject(clientDataDic);

            //Console.WriteLine(json);
            if (File.Exists(clientDataTxt))
            {
                string[] bacFilePath = Directory.GetFiles(dataPath + "/bac", "*.bac", SearchOption.TopDirectoryOnly);

                Console.WriteLine(dataPath + "/bac/" + (bacFilePath.Length + 1).ToString() + ".bac");
                File.Copy(dataPath + "/ClientData.txt", dataPath + "/bac/" + (bacFilePath.Length + 1).ToString() + ".bac");
            }
            File.Delete(dataPath + "/ClientData.txt");


            File.WriteAllText(dataPath + "/ClientData.txt", json);
        }
           

        static List<object> csvReader(string srcPath)
        {
            //using (FileStream fileStream = new FileStream(srcPath, FileMode.Open, FileAccess.Read))
            StreamReader sr = new StreamReader(srcPath);

            string[] valueKey = sr.ReadLine().Split(',');

            List<object> dataParameter = new List<object>();

            CsvReader csv = new CsvReader(sr);

            csv.Configuration.HasHeaderRecord = true;


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
                                List<object> elemList = new List<object>();
                                string[] elems = content.Substring(1, content.Length - 2).Split(',');
                                int ev = 0;
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
                                    }
                                }
                                parameterContent.Add(valueKey[i], elemList);
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
