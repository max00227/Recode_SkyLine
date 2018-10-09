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

using CsvHelper;
using CsvHelper.Configuration;

namespace test
{
    static public class BackRequest{
        public object GetConten(string key){
			if (this.ContainsKey(key))
			{
                return thisreader[key];
			}
			else
			{
                return null;
			}
        }
    }
}