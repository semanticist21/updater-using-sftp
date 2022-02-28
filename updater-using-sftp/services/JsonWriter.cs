using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Updater.services
{
    public static class JsonWriter<T>
    {
        public static void WriteFileToJson(T data, string directory, string fileName)
        {
            if (data != null)
            {
                string jsonString = JsonConvert.SerializeObject(data.ToString(), Formatting.Indented);
                string saveFileDirectory = string.Join(directory, fileName);

                System.IO.File.WriteAllText(jsonString, saveFileDirectory);
            }
        }
        /// <summary>
        /// it writes list<object> to Json File. Put data, directory to save, and file name.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="directory"></param>
        /// <param name="fileName"></param>
        public static void WriteListToJson(List<T> data, string directory, string fileName)
        {
            string jsonString = JsonConvert.SerializeObject(data.ToArray(), Formatting.Indented);
            string saveFileDirectory = string.Join("/", directory, fileName);

            System.IO.File.WriteAllText(saveFileDirectory, jsonString);
        }

        public async static Task<List<T>> ReadFromJsonAsync(string directory, string fileName)
        {
            string parsedFilePath = string.Join("/", directory, fileName);
            //Debug.WriteLine(parsedFilePath);

            string fileData = await System.IO.File.ReadAllTextAsync(parsedFilePath);

            object? deserializedData = JsonConvert.DeserializeObject(fileData, new List<T>().GetType());

            if (deserializedData is List<T> returnData) return returnData;
            else return new List<T>();
        }

    }
}
