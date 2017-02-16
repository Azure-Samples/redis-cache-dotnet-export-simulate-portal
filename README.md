---
services: Azure Redis Cache
platforms: .Net
author: msonecode
---

# How to export premium Redis cache by simulating as Azure portal #


## Introduction

Export allows you to export the data stored in Azure Redis Cache to Redis compatible RDB file(s). You can use this feature to move data from one Azure Redis Cache instance to another or to another Redis server. During the exporting process, a temporary file is created on the VM that hosts the Azure Redis Cache server instance, and the file is uploaded to the designated storage account. When the exporting operation ends in either a status of success or failure, the temporary file is deleted. Import/Export enables you to migrate between different Azure Redis Cache instances or to populate the cache with data before being used.

There are [multiple methods to export redis cache](https://github.com/zhangdingsong/ExportRedisViaAzureCLI) (my previous post about this topic), including [Azure Portal](https://docs.microsoft.com/en-us/azure/redis-cache/cache-how-to-import-export-data#export), [Azure PowerShell](https://docs.microsoft.com/en-us/azure/redis-cache/cache-howto-manage-redis-cache-powershell#to-export-a-redis-cache ), [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/redis#export) and [REST API](https://docs.microsoft.com/en-us/rest/api/redis/redis#Redis_ExportData) methods. Since the Azure Portal approach is the simplest one, this post I will introduce a programming method to export Redis Cache by stimulating Azure Portal approach.

## Prerequisites

***1. Azure Account***
<br/>
You need an Azure account. You can [open a free Azure account](https://azure.microsoft.com/pricing/free-trial/?WT.mc_id=A261C142F) or [Activate Visual Studio subscriber benefits](https://azure.microsoft.com/pricing/member-offers/msdn-benefits-details/?WT.mc_id=A261C142F).

***2. Visual Studio 2015***
<br/>
[https://www.visualstudio.com/downloads/](https://www.visualstudio.com/downloads/)

***3. ASP.NET MVC 5***
<br/>
The tutorial assumes you have worked with ASP.NET MVC and Visual Studio. If you need an introduction, see [Getting Started with ASP.NET MVC 5](http://www.asp.net/mvc/overview/getting-started/introduction/getting-started).

***4. Azure SDK***
<br/>
The tutorial is written for Visual Studio 2015 with the [Azure SDK for .NET 2.9](https://azure.microsoft.com/en-us/documentation/articles/dotnet-sdk/) or later.
[Download the latest Azure SDK for Visual Studio 2015](http://go.microsoft.com/fwlink/?linkid=518003). The SDK installs Visual Studio 2015 if you don't already have it.

## Detailed Steps

1.	Open developer console on your browser (F12)
2.	Open the browser to Azure Portal and run the Redis exporting operation as below.
To export the current contents of the cache to storage, browse to your cache in the Azure portal and click Export data from the Settings blade of your cache instance.
<img src="https://github.com/zhangdingsong/ExportRedisBySimulateAzurePortal/raw/master/cache-export-data-choose-storage-container.png">
3. As you can see, the parameters from json file of request playload are the same as Azure CLI and RESR API methods.
<img src="https://github.com/zhangdingsong/ExportRedisBySimulateAzurePortal/raw/master/requestplayload.jpg">   

4. Select one of the invoke calls and view the headers, copy the text after "Bearer"
<img src="https://github.com/zhangdingsong/ExportRedisBySimulateAzurePortal/raw/master/bearer.jpg">

5. Collect the related information from Redis Cache overview blade.
 	* subscriptionId
 	* resourceGroup
 	* cacheName
6. Collect related information from Storage blade.
    * storageAccountConnectionString = Settings -> Access keys -> Connection strings
 <img src="https://github.com/zhangdingsong/ExportRedisBySimulateAzurePortal/raw/master/connectstring.jpg">
 
7. Fill in all the collected infos with source code you [downloaded](https://github.com/zhangdingsong/ExportRedisBySimulateAzurePortal/raw/master/csharp/RedisExportByStimulateAzurePortal.zip) from this page.

    ```c#
    const string exportString = "https://management.azure.com/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Cache/Redis/{2}/export?api-version=2015-08-01";
    ```
8. Below are some code snippets to form SAS token for Storage Container.
    ```c#
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
    ```

## Reference
Below are some useful links, you will need them before your real action.<br/><br/>
Azure Portal: Export Redis to a Blob Container.<br/>
https://docs.microsoft.com/en-us/azure/redis-cache/cache-how-to-import-export-data#export

PowerShell: Export Redis to a Blob Container.<br/>
https://docs.microsoft.com/en-us/azure/redis-cache/cache-howto-manage-redis-cache-powershell#to-export-a-redis-cache 

Azure CLI: Export Redis to a Blob Container.<br/>
https://docs.microsoft.com/en-us/cli/azure/redis#export

REST API: Export Redis to a Blob Container.<br/>
https://docs.microsoft.com/en-us/rest/api/redis/redis#Redis_ExportData

Microsoft Azure Storage Explorer<br/>
http://storageexplorer.com/

