namespace BlobStorage
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Storage;
    using Microsoft.Azure.Storage.Auth;
    using Microsoft.Azure.Storage.Blob;
    using Microsoft.Azure.Storage.RetryPolicies;

    public static class GettingStarted
    {
        public static void CallBlobGettingStartedSamples()
        {
            BasicStorageBlockBlobOperationsWithAccountSASAsync().Wait();
        }
        private static async Task BasicStorageBlockBlobOperationsWithAccountSASAsync()
        {
            const string ImageToDownload = "IMG_2388.MOV";
            string containerName = "sample-4e841f38-0ec5-4e01-a6bf-3db343715231";
            Stopwatch s = new Stopwatch();
            s.Start();

            string sasToken = GetAccountSASToken();

            StorageCredentials accountSAS = new StorageCredentials(sasToken);

            Console.WriteLine("\n Account SAS Signature: " + accountSAS.SASSignature);
            Console.WriteLine("\n Account SAS Token: " + accountSAS.SASToken);

            Uri containerUri = GetContainerUri(containerName);

            CloudBlobContainer container = new CloudBlobContainer(containerUri, accountSAS);

            try
            {
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(ImageToDownload);

                Console.WriteLine("\n Download start - " + System.DateTime.Now.ToString());
                Console.WriteLine("\n Download Blob from {0}", blockBlob.Uri.AbsoluteUri);
                await blockBlob.DownloadToFileAsync(string.Format("./CopyOf{0}", ImageToDownload), FileMode.Create);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            finally
            {
                // Clean up after the demo.
                // Note that it is not necessary to delete all of the blobs in the container first; they will be deleted
                // with the container. 
                Console.WriteLine("\n Download completed -" + System.DateTime.Now.ToString());
                s.Stop();

                Console.WriteLine("Overall download time-- " + s.Elapsed.TotalMinutes);
            }
        }

        private static Uri GetContainerUri(string containerName)
        {
            CloudStorageAccount storageAccount = Common.CreateStorageAccountFromConnectionString();
            return storageAccount.CreateCloudBlobClient().GetContainerReference(containerName).Uri;
        }

        private static string GetAccountSASToken()
        {
            CloudStorageAccount storageAccount = Common.CreateStorageAccountFromConnectionString();

            // Create a new access policy for the account with the following properties:
            // Permissions: Read, Write, List, Create, Delete
            // ResourceType: Container
            // Expires in 24 hours
            // Protocols: HTTPS or HTTP (note that the storage emulator does not support HTTPS)
            SharedAccessAccountPolicy policy = new SharedAccessAccountPolicy()
            {
                // When the start time for the SAS is omitted, the start time is assumed to be the time when the storage service receives the request. 
                // Omitting the start time for a SAS that is effective immediately helps to avoid clock skew.
                Permissions = SharedAccessAccountPermissions.Read | SharedAccessAccountPermissions.Write | SharedAccessAccountPermissions.List | SharedAccessAccountPermissions.Create | SharedAccessAccountPermissions.Delete,
                Services = SharedAccessAccountServices.Blob,
                ResourceTypes = SharedAccessAccountResourceTypes.Container | SharedAccessAccountResourceTypes.Object,
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(3),
                Protocols = SharedAccessProtocol.HttpsOrHttp
            };

            // Create new storage credentials using the SAS token.
            string sasToken = storageAccount.GetSharedAccessSignature(policy);

            // Return the SASToken
            return sasToken;
        }
    }
}