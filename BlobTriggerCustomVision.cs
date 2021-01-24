using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace katsujim.Function
{
    public static class BlobTriggerCustomVision
    {
        [FunctionName("BlobTriggerCustomVision")]
        public static void Run([BlobTrigger("pictures/{name}", Connection = "storagekatsujimcpaas_STORAGE")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        }
    }
}
