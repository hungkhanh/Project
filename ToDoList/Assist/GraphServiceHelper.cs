using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ToDoList.Assist
{
    public static class GraphServiceHelper
    {
        private static GraphServiceClient GraphClient { get; set; }

        public static void InitializeClient(IAuthenticationProvider authProvider)
        {
            GraphClient = new GraphServiceClient(authProvider);
        }

        public static GraphServiceClient GetGraphClient()
        {
            return GraphClient;
        }

        #region UserRequests
   
        public static async Task<User> GetMeAsync()
        {
            try
            {
                return await GraphClient.Me.Request().GetAsync();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting signed-in user: {ex.Message}");
                return null;
            }
        }


        public static async Task<string> GetMyEmailAddressAsync()
        {
            User me = await GraphClient.Me.Request().Select("mail,userPrincipalName").GetAsync();
            return me.Mail ?? me.UserPrincipalName;
        }


        public static async Task<string> GetMyDisplayNameAsync()
        {
            User me;
            try
            {
                me = await GraphClient.Me.Request().Select("displayName").GetAsync();
                return me.GivenName ?? me.DisplayName;
            }
            catch (ServiceException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw;
                }
                return null;
            }
        }


        public static async Task<IEnumerable<Event>> GetEventsAsync()
        {
            try
            {
                var resultPage = await GraphClient.Me.Events.Request()
                    .Select(e => new {
                        e.Subject,
                        e.Organizer,
                        e.Start,
                        e.End
                    })
                    .OrderBy("createdDateTime DESC")
                    .GetAsync();

                return resultPage.CurrentPage;
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Service Exception, Error getting events: {ex.Message}");
                return null;
            }
        }


        #endregion UserRequests

        #region OneDriveRequests

        public static async Task<DriveItem> GetOneDriveRootAsync()
        {
            try
            {
                // GET /me/drive/root
                return await GraphClient.Me.Drive.Root.Request().GetAsync();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Service Exception, Error getting signed-in users one drive root: {ex.Message}");
                return null;
            }
        }

        public static async Task<IDriveItemChildrenCollectionPage> GetOneDriveRootChildrenAsync()
        {
            try
            {
                return await GraphClient.Me.Drive.Root.Children.Request().GetAsync();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Service Exception, Error getting signed-in users one drive root children: {ex.Message}");
                return null;
            }
        }

       
        public static async Task<DriveItem> GetOneDriveFolderAsync(string folderPath)
        {
            try
            {
                
                var searchCollection = await GraphClient.Me.Drive.Root.Search("ToDoList-App").Request().GetAsync();
                foreach (var folder in searchCollection)
                    if (folder.Name == "ToDoList-App")
                        return folder;
                return null;
            }
            catch (ServiceException ex)
            {
                if (ex.StatusCode == HttpStatusCode.BadGateway)
                {
                    Console.WriteLine($"Service Exception, Bad Gateway. Error getting signed-in users one drive folder: {ex.Message}");
                }
                else if (ex.IsMatch(GraphErrorCode.GeneralException.ToString()))
                {
                    Console.WriteLine($"General Exception, error getting folder. Please check internet connection.");
                }
                throw;
            }
        }

        public static async Task<DriveItem> CreateNewOneDriveFolderAsync(string folderName)
        {
            try
            {
                var driveItem = new DriveItem
                {
                    Name = folderName,
                    Folder = new Folder
                    {
                    },
                    AdditionalData = new Dictionary<string, object>()
                    {
                        {"@microsoft.graph.conflictBehavior","fail"}
                    }
                };

                return await GraphClient.Me.Drive.Root.Children
                    .Request()
                    .AddAsync(driveItem);
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Service Exception, error creating folder in signed-in users one drive root: {ex.Message}");
                throw;
            }
        }

        public static async Task<DriveItem> UploadFileToOneDriveAsync(string itemId, string filename)
        {
            try
            {
                Windows.Storage.StorageFolder storageFolder =
                    Windows.Storage.ApplicationData.Current.LocalFolder;

                Windows.Storage.StorageFile sampleFile =
                    await storageFolder.GetFileAsync(filename);

                var stream = await sampleFile.OpenStreamForReadAsync();

                return await GraphClient.Me.Drive.Items[itemId].ItemWithPath(filename).Content
                    .Request()
                    .PutAsync<DriveItem>(stream);
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Service Expception, Error uploading file to signed-in users one drive: {ex.Message}");
                throw;
            }
        }

        
        public static async Task RestoreFileFromOneDriveAsync(string itemId, string dataFilename)
        {
            try
            {
                Windows.Storage.StorageFolder storageFolder =
                    Windows.Storage.ApplicationData.Current.LocalFolder;

                Windows.Storage.StorageFile originalDataFile =
                    await storageFolder.GetFileAsync(dataFilename);

                var backedUpFileStream = await GraphClient.Me.Drive.Items[itemId].ItemWithPath(dataFilename).Content
                            .Request()
                            .GetAsync();

                var backedUpFile = await storageFolder.CreateFileAsync("temp", CreationCollisionOption.ReplaceExisting);
                var newStream = await backedUpFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);

                using (var outputStream = newStream.GetOutputStreamAt(0))
                {
                    using (var dataWriter = new Windows.Storage.Streams.DataWriter(outputStream))
                    {
                        var buffer = backedUpFileStream.ToByteArray();
                        dataWriter.WriteBytes(buffer);

                        await dataWriter.StoreAsync();
                        await outputStream.FlushAsync();
                    }
                }

                await backedUpFile.CopyAsync(storageFolder, dataFilename, NameCollisionOption.ReplaceExisting);
            }

            catch (ServiceException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Forbidden)
                    Console.WriteLine($"Access Denied: {ex.Message}");

                Console.WriteLine($"Service Exception, Error uploading file to signed-in users one drive: {ex.Message}");
            }
        }

        #endregion OneDriveRequests

        private static async Task<string> GetHttpContentWithToken(string url, string token)
        {
            var httpClient = new System.Net.Http.HttpClient();
            System.Net.Http.HttpResponseMessage response;
            try
            {
                var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);

                // Add the token in Authorization header
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                response = await httpClient.SendAsync(request).ConfigureAwait(false);
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return content;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
