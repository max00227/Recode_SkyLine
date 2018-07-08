using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using model.data;

namespace test
{
    public class ClientDataLoader
    {
        static string json = "";

        //Movie movie = JsonConvert.DeserializeObject<Movie>(json, new MovieConverter());

        static public void readClientData (){
            StreamReader sr = new StreamReader("/Users/chien/Projects/test/test/SrcCsv/data/ClientData.txt");
            json = sr.ReadToEnd();

			ClientLargeData largeData = JsonConversionExtensions.ConvertJson<ClientLargeData>(json);

			MasterDataManager.UpdataMasterdata (largeData);
        } 
    }

    public class SearchResult
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Url { get; set; }
    }
}
