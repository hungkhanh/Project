using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using ToDoList.Assist;
using ToDoList.Controllers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ToDoList.Views
{
    public sealed partial class SettingsView : ContentDialog
    {
        private string[] scopes = new string[] { "files.readwrite" };
        private string appClientId = "422b281b-be2b-4d8a-9410-7605c92e4ff1";
        private AuthenticationProvider authProvider;
        public const string DataFilename = "tddatabase.db";
        public const string BackupFolderName = "ToDoList-App";
        public User CurrentUser { get; set; }
        public ControllersBoard CurrentViewModel { get; set; }
        public ControllersSettings ViewModel { get; set; }

        public SettingsView()
        {
            this.InitializeComponent();

            if (App.CurrentUser != null)
                txtResults.Text = "Welcome " + App.CurrentUser.GivenName;
            else
            {
                txtResults.Text = "Welcome, please select an option below and sign in when prompted";
                btnSignOutTip.IsEnabled = false;
            }

            DataContext = ViewModel;

            authProvider = App.GetAuthenticationProvider();
        }


        private async Task DisplayMessageAsync(string message)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                   () =>
                   {
                       var frame = (Frame)Window.Current.Content;
                       (frame.Content as MainView).KanbanInAppNotification.Show(message, 5000);
                       //SettingsAppNotification.Show(message, 3000);
                   });
        }

        private void BtnCloseSettings_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void btnBackupTip_Click(object sender, RoutedEventArgs e)
        {
            BackupTip.IsOpen = true;
        }

        private void btnSignOut_Click(object sender, RoutedEventArgs e)
        {
            SignOutPopup.IsOpen = true;
        }

        private async void SignOutPopup_ConfirmClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            if (SignOutPopup.IsOpen)
                SignOutPopup.IsOpen = false;

            SignOut();
        }

        private void btnRestoreTip_Click(object sender, RoutedEventArgs e)
        {
            RestoreTip.IsOpen = true;
        }

        private void RestoreTip_ActionButtonClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            CloseTeachingTips();

            // Restore data from OneDrive
            // Requires application restart
            RestoreFromOneDrive();
        }

        private void BackupTip_ActionButtonClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            AuthenticationResult authResult = null;
            CloseTeachingTips();

            // Backup data to OneDrive
            BackupToOneDrive();
        }

        private async void BackupToOneDrive()
        {
            progressRing.IsActive = true;
            try
            {
                // Request a token to sign in the user
                var accessToken = await authProvider.GetAccessToken();

                // Initialize Graph Client
                GraphServiceHelper.InitializeClient(authProvider);

                // Set current user (temp)
                App.CurrentUser = await GraphServiceHelper.GetMeAsync();

                // Find backupFolder in user's OneDrive, if it exists
                var backupFolder = await GraphServiceHelper.GetOneDriveFolderAsync("ToDoList-App");

                // Create backup folder in OneDrive if not exists
                if (backupFolder == null)
                    backupFolder = await GraphServiceHelper.CreateNewOneDriveFolderAsync("ToDoList-App") as DriveItem;

                // Backup datafile (or overwrite)
                var uploadedFile = await GraphServiceHelper.UploadFileToOneDriveAsync(backupFolder.Id, DataFilename);

                await DisplayMessageAsync("Data backed up successfully.");

                var displayName = await GraphServiceHelper.GetMyDisplayNameAsync();
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                       () =>
                       {
                           txtResults.Text = "Welcome " + displayName;
                           btnSignOutTip.IsEnabled = true;
                       });
            }
            catch (ServiceException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // MS Graph Known Error 
                    // Users need to sign into OneDrive at least once
                    // https://docs.microsoft.com/en-us/graph/known-issues#files-onedrive

                    // Empty all cached accounts / data to allow user to rety
                    await authProvider.SignOut();

                    await DisplayMessageAsync("Error 401. Access Denied. Please make sure you've logged\ninto OneDrive and your email at least once then try again.");
                }
                else if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    await DisplayMessageAsync("Error 404. Resource requested is not available.");
                }
                else if (ex.StatusCode == HttpStatusCode.Conflict)
                {
                    await DisplayMessageAsync("Error 409. Error backing up, issue retrieving backup folder. Please try again.");
                }
                else if (ex.StatusCode == HttpStatusCode.BadGateway)
                {
                    await DisplayMessageAsync("Error 502. Bad Gateway.\nPlease check your internet connection and try again in a few.");
                }
                else if (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    await DisplayMessageAsync("Error 503. Service unavailable due to high load or maintenance.\nPlease try again in a few.");
                }
                else if (ex.IsMatch(GraphErrorCode.GeneralException.ToString()))
                {
                    await DisplayMessageAsync("General Exception. Please check your internet connection and try again in a few.");
                }
            }
            catch (MsalException msalex)
            {
                if (msalex.ErrorCode == MsalError.AuthenticationCanceledError)
                {
                    await DisplayMessageAsync(msalex.Message);
                }
                else if (msalex.ErrorCode == MsalError.InvalidGrantError)
                {
                    // invalid_grant ErrorCode comes from no consent
                    // for the correct scopes (todo: add interactive retry)
                    await DisplayMessageAsync("Invalid access scopes, please contact the developer.");
                }
            }
            catch (Exception ex)
            {
                await DisplayMessageAsync(ex.Message);
            }
            finally
            {
                progressRing.IsActive = false;
            }
        }

        private async void RestoreFromOneDrive()
        {
            progressRing.IsActive = true;
            try
            {
                // Request a token to sign in the user
                var accessToken = await authProvider.GetAccessToken();

                // Initialize Graph Client
                GraphServiceHelper.InitializeClient(authProvider);

                // Set current user (temp)
                App.CurrentUser = await GraphServiceHelper.GetMeAsync();

                // Find the backupFolder in OneDrive, if it exists
                var backupFolder = await GraphServiceHelper.GetOneDriveFolderAsync("ToDoList-App");

                if (backupFolder != null)
                {
                    // Restore local data file using the backup file, if it exists
                    await GraphServiceHelper.RestoreFileFromOneDriveAsync(backupFolder.Id, "tddatabase.db");

                    await DisplayMessageAsync("Data restored successfully.");

                    var displayName = await GraphServiceHelper.GetMyDisplayNameAsync();
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                           () =>
                           {
                               txtResults.Text = "Welcome " + App.CurrentUser.GivenName;
                               btnSignOutTip.IsEnabled = true;
                           });

                    Thread.Sleep(2000);

                    // Restart app to make changes
                    await Windows.ApplicationModel.Core.CoreApplication.RequestRestartAsync("");
                }
                else
                    await DisplayMessageAsync("No backup folder found to restore from.");
            }
            catch (ServiceException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // MS Graph Known Error 
                    // Users need to sign into OneDrive at least once
                    // https://docs.microsoft.com/en-us/graph/known-issues#files-onedrive

                    // Empty all cached accounts / data to allow user to rety
                    await authProvider.SignOut();

                    await DisplayMessageAsync("Error 401. Access Denied. Please make sure you've logged\ninto OneDrive and your email at least once then try again.");
                }
                else if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    await DisplayMessageAsync("Error 404. Resource requested is not available.");
                }
                else if (ex.StatusCode == HttpStatusCode.Conflict)
                {
                    await DisplayMessageAsync("Error 409. Error backing up, issue retrieving backup folder. Please try again.");
                }
                else if (ex.StatusCode == HttpStatusCode.BadGateway)
                {
                    await DisplayMessageAsync("Error 502. Bad Gateway.\nPlease check your internet connection and try again in a few.");
                }
                else if (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    await DisplayMessageAsync("Error 503. Service unavailable due to high load or maintenance.\nPlease try again in a few.");
                }
                else if (ex.IsMatch(GraphErrorCode.GeneralException.ToString()))
                {
                    await DisplayMessageAsync("General Exception. Please check your internet connection and try again in a few.");
                }
            }
            catch (MsalException msalex)
            {
                if (msalex.ErrorCode == MsalError.AuthenticationCanceledError)
                {
                    await DisplayMessageAsync(msalex.Message);
                }
                else if (msalex.ErrorCode == MsalError.InvalidGrantError)
                {
                    // invalid_grant comes from no consent to needed scopes
                    await DisplayMessageAsync("Invalid access scopes, please contact the developer.");
                }
            }
            catch (Exception ex)
            {
                await DisplayMessageAsync("Unexpected Error: " + ex.Message);
            }
            finally
            {
                progressRing.IsActive = false;
            }
        }

        private async void SignOut()
        {
            try
            {
                await authProvider.SignOut();

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    txtResults.Text = "User has signed-out";
                    this.btnSignOutTip.IsEnabled = false;
                    App.CurrentUser = null;
                });
            }
            catch (MsalException ex)
            {
                await DisplayMessageAsync(ex.Message);
            }
        }

        public void CloseTeachingTips()
        {
            if (RestoreTip.IsOpen)
                RestoreTip.IsOpen = false;
            if (BackupTip.IsOpen)
                BackupTip.IsOpen = false;
        }
    }
}
