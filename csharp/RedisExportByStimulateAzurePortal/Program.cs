using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

using System.Net.Http; // Add reference to System.Net.Http
using System.Net.Http.Headers;

using Microsoft.WindowsAzure; // Azure storage nuget package
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace RedisExportByStimulateAzurePortal
{
    class Program
    {
        const string exportString = "https://management.azure.com/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Cache/Redis/{2}/export?api-version=2015-08-01";
        const string exportBody =
@"{{
""format"": ""rdb"",
""prefix"": ""{0}"",
""container"": ""{1}""
}}";

        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        static async Task MainAsync(string[] args)
        {
            string subscriptionId = "";
            string resourceGroup = "";
            string cacheName = "";

            string storageAccountConnectionString = ""; // must be in same region as cache
            string containerToWriteTo = "testcontainer";
            string blobPrefix = "testblob";
            string bearer = ""; // from portal

            var exportUri = new Uri(string.Format(exportString, subscriptionId, resourceGroup, cacheName));
            var containerSas = new StorageWrapper(storageAccountConnectionString)
                .GetContainerSas(containerToWriteTo, read: true, write: true, list: true, delete: true);

            await Export(bearer, exportUri, blobPrefix, containerSas);

        }

        static async Task Export(string bearer, Uri exportUri, string prefix, string container)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearer);

                var content = new StringContent(string.Format(exportBody, prefix, container), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(exportUri, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Success! RDB file should be in storage account soon.");
                    // currently location will 404 - rdb file should be in container in less than 10 minutes
                    // var pollLocation = response.Headers.Location;
                    // await WaitForDone(client, pollLocation);
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("There was a problem with the request");
                    Console.WriteLine(error);
                }
            }
        }

        static async Task WaitForDone(HttpClient client, Uri location)
        {
            HttpResponseMessage response = null;
            do
            {
                await Task.Delay(TimeSpan.FromSeconds(30));
                response = await client.GetAsync(location);
            }
            while (response.StatusCode == HttpStatusCode.Accepted);
        }

    }

    class StorageWrapper
    {
        CloudStorageAccount _storageAccount;

        public StorageWrapper(string conn)
        {
            this._storageAccount = CloudStorageAccount.Parse(conn);
        }

        public CloudBlobContainer GetContainer(string name)
        {
            return this._storageAccount.CreateCloudBlobClient().GetContainerReference(name);
        }

        public string GetContainerSas(string name, bool read = false, bool write = false, bool list = false, bool delete = false)
        {
            var container = GetContainer(name);
            container.CreateIfNotExists();
            return container.Uri + container.GetSharedAccessSignature(GetPolicy(read, write, list, delete));
        }

        private SharedAccessBlobPolicy GetPolicy(bool read, bool write, bool list, bool delete)
        {
            SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy();

            policy.Permissions =
                    (read ? SharedAccessBlobPermissions.Read : 0)
                  | (write ? SharedAccessBlobPermissions.Write : 0)
                  | (list ? SharedAccessBlobPermissions.List : 0)
                  | (delete ? SharedAccessBlobPermissions.Delete : 0);
            policy.SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(75); // Must be at least 65 minutes

            return policy;
        }
    }
}