using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace FlightBooking.Ultities
{
    public static class JsonFileHelper
    {
        public static List<T> ReadJsonFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            var jsonData = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<T>>(jsonData);
        }

        public static void WriteJsonFile<T>(string filePath, List<T> data)
        {
            var jsonData = JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            });
            File.WriteAllText(filePath, jsonData);
        }

        private static void EnsureDirectoryAndFileExist(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "[]"); // Create an empty JSON array file
            }
        }
    }
}