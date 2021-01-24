using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Configuration;
using Newtonsoft.Json;

namespace katsujim.Function
{
    public static class BlobTriggerCustomVision
    {
        [FunctionName("BlobTriggerCustomVision")]
        public static async void Run([BlobTrigger("pictures/{name}", Connection = "storagekatsujimcpaas_STORAGE")] Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            string predectionKey = Environment.GetEnvironmentVariable("PREDICTION_KEY");
            string endpoint = Environment.GetEnvironmentVariable("ENDPOINT");
            var client = new HttpClient();
            // Request headers - replace this example key with your valid Prediction-Key.
            client.DefaultRequestHeaders.Add("Prediction-Key", predectionKey);

            string peoplecountString = "";

            // Prediction URL - replace this example URL with your valid Prediction URL.
            string url = endpoint;
            HttpResponseMessage response;
            byte[] byteData;

            using (MemoryStream ms = new MemoryStream())
            {

                byte[] buf = new byte[32768]; // 一時バッファ
                while (true)
                {
                    // ストリームから一時バッファに読み込む
                    int read = myBlob.Read(buf, 0, buf.Length);

                    if (read > 0)
                    {
                        // 一時バッファの内容をメモリ・ストリームに書き込む
                        ms.Write(buf, 0, read);
                    }
                    else
                    {
                        break;
                    }
                }
                // メモリ・ストリームの内容をバイト配列に格納
                byteData = ms.ToArray();
            }


            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(url, content);
                //Console.WriteLine(await response.Content.ReadAsStringAsync());
                var jsonString = await response.Content.ReadAsStringAsync();
                peoplecountString = CountPeople(jsonString,0.4);
            }
            log.LogInformation(peoplecountString);
        }

        private static string CountPeople(string jsonString, double th)
        {
            var peopleCount = 0;
            var jsonobject = JsonConvert.DeserializeObject<PredictionResult>(jsonString);

            foreach (var detectedObject in jsonobject.predictions)
            {
                //確信度がある閾値よりも高い場合、人と認定
                if (detectedObject.probability > th)
                {
                    peopleCount++;
                }
            }

            return $"{peopleCount}人が検出されました";
        }
    }

    public class PredictionResult
    {
        public string id { get; set; }
        public string project { get; set; }
        public string iteration { get; set; }
        public string created { get; set; }
        public PredictionObject[] predictions { get; set; }
    }

    public class PredictionObject
    {
        public string tagId { get; set; }
        public string tagName { get; set; }
        public double probability { get; set; }
    }
}
