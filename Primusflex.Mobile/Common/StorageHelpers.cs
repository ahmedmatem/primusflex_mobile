using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Net;
using PrimusFlex.Mobile.Common;
using Microsoft.WindowsAzure.Storage;
using System.IO;
using Newtonsoft.Json;

namespace Primusflex.Mobile.Common
{
    public static class StorageHelpers
    {
        public static CloudStorageAccount StorageAccount(string token)
        {
            var accountPair = GetAccountPair(token);

            //Parse the connection string and return a reference to the storage account.
            string ConnectionString =
                string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
                accountPair["AccountName"], accountPair["AccountKey"]);

            return CloudStorageAccount.Parse(ConnectionString);
        }

        public static string GetContainerSasUri(CloudBlobContainer container)
        {
            //Set the expiry time and permissions for the container.
            //In this case no start time is specified, so the shared access signature becomes valid immediately.
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24);
            sasConstraints.Permissions = SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.List;

            //Generate the shared access signature on the container, setting the constraints directly on the signature.
            string sasContainerToken = container.GetSharedAccessSignature(sasConstraints);

            //Return the URI string for the container, including the SAS token.
            return container.Uri + sasContainerToken;
        }

        public static string GetBlobSasUri(CloudBlobContainer container, string blobName)
        {
            //Get a reference to a blob within the container.
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

            //Set the expiry time and permissions for the blob.
            //In this case the start time is specified as a few minutes in the past, to mitigate clock skew.
            //The shared access signature will be valid immediately.
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5);
            sasConstraints.SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24);
            sasConstraints.Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write;

            //Generate the shared access signature on the blob, setting the constraints directly on the signature.
            string sasBlobToken = blob.GetSharedAccessSignature(sasConstraints);

            //Return the URI string for the container, including the SAS token.
            return blob.Uri + sasBlobToken;
        }

        private static Dictionary<string, string> GetAccountPair(string token)
        {
            AccountStoragePair paairFromServer;
            Dictionary<string, string> accountPair = new Dictionary<string, string>();
            
            string uri = Constant.ACCOUNT_URL + "/storageAccountPair";
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            request.Method = "GET";
            request.Headers.Add("Authorization", "Bearer " + token);

            try
            {
                var response = request.GetResponse() as HttpWebResponse;

                using (Stream stream = response.GetResponseStream())
                {
                    var reader = new StreamReader(stream);
                    paairFromServer = JsonConvert.DeserializeObject<AccountStoragePair>(reader.ReadToEnd());
                }

                accountPair.Add("AccountName", paairFromServer.AccountName);
                accountPair.Add("AccountKey", paairFromServer.AccountKey);

                return accountPair;
            }
            catch(WebException exc)
            {

            }

            return null;
        }
    }
}