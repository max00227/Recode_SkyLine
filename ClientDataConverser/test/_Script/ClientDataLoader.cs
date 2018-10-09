using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;


namespace test
{
    public class ClientDataLoader
    {
        static string json = "";

        //Movie movie = JsonConvert.DeserializeObject<Movie>(json, new MovieConverter());

        static public void readJason (){
            StreamReader sr = new StreamReader("/Users/chien/Projects/test/test/SrcCsv/data/ClientData.txt");
            json = sr.ReadToEnd();

            JsonConvertExtension.readJsonKey(json, "Chara");

            /*ClienLargeData largeData = JsonConvertExtension.ConvertJson<ClienLargeData>(json);

            foreach (var c in largeData.Chara)
            {
                Console.WriteLine(c.name);
            }*/
        } 
    }

    public class SearchResult
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Url { get; set; }
    }
}
