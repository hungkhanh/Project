using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Autofac;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Controls;
using LeaderAnalytics.AdaptiveClient.Utilities;
using System.Collections.Generic;
using LeaderAnalytics.AdaptiveClient;
using LeaderAnalytics.AdaptiveClient.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Graph;
using ToDoList.Views;
using ToDoList.Assist;
using ToDoList.Controllers;
using ToDoList.Model;


namespace ToDoList
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        /// 
        private static IContainer container;

        private string[] scopes = new string[] { "files.readwrite", "user.read" };
        private string appId = "422b281b-be2b-4d8a-9410-7605c92e4ff1";
        private static AuthenticationProvider authProvider;
        public AuthenticationProvider AuthProvider { get; set; }
        public static User CurrentUser { get; set; }

        public App()
        {

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MTMwNjE1QDMxMzcyZTMyMmUzMEFlSlpZMDNRQVFhUy9pOHQ4dzlObVNNbGNsQ3I2bE15NE50U2dzQ1lYK1k9");

            this.InitializeComponent();
            this.Suspending += OnSuspending;

            // build the Autofac container
            IEnumerable<IEndPointConfiguration> endPoints = EndPointUtilities.LoadEndPoints("EndPoints.json");
            string fileRoot = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterModule(new LeaderAnalytics.AdaptiveClient.EntityFrameworkCore.AutofacModule());
            builder.RegisterModule(new LeaderAnalytics.AdaptiveClient.AutofacModule());
            builder.RegisterModule(new ToDoList.Services.AutofacModule());
            RegistrationHelper registrationHelper = new RegistrationHelper(builder);

            registrationHelper
                .RegisterEndPoints(endPoints)
                .RegisterModule(new ToDoList.Services.AdaptiveClientModule());

            container = builder.Build();

            IDatabaseUtilities databaseUtilities = container.Resolve<IDatabaseUtilities>();

            // Create all databases or apply migrations

            foreach (IEndPointConfiguration ep in endPoints.Where(x => x.EndPointType == EndPointType.DBMS))
                Task.Run(() => databaseUtilities.CreateOrUpdateDatabase(ep)).Wait();

            AppCenter.Start("a57ee001-5ab0-46f5-aa5a-4d1b84cd6b66",
                   typeof(Analytics), typeof(Crashes));
        }

        public static ControllersMain GetViewModel(Frame frame, InAppNotification messagePump)
        {
            return container.Resolve<ControllersMain>(new TypedParameter(typeof(Frame), frame), new TypedParameter(typeof(InAppNotification), messagePump));
        }

        public async void InitializeAuthProvider()
        {

            authProvider = new AuthenticationProvider(appId, scopes);


        }

        public static AuthenticationProvider GetAuthenticationProvider()
        {
            return authProvider;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
              
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {

                }

                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {

                    rootFrame.Navigate(typeof(MainView), e.Arguments);
                }

                Window.Current.Activate();

                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

                var titleBar = ApplicationView.GetForCurrentView().TitleBar;

                // Set active window colors
                titleBar.ForegroundColor = Windows.UI.Colors.White;
                titleBar.BackgroundColor = Windows.UI.Colors.Transparent;
                titleBar.ButtonForegroundColor = Windows.UI.Colors.White;
                titleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
                titleBar.ButtonHoverForegroundColor = Windows.UI.Colors.White;
                titleBar.ButtonHoverBackgroundColor = Windows.UI.Colors.SlateGray;
                titleBar.ButtonPressedForegroundColor = Windows.UI.Colors.White;
                titleBar.ButtonPressedBackgroundColor = Windows.UI.Colors.DimGray;

                // Set inactive window colors
                titleBar.InactiveForegroundColor = Windows.UI.Colors.White;
                titleBar.InactiveBackgroundColor = Windows.UI.Colors.Transparent;
                titleBar.ButtonInactiveForegroundColor = Windows.UI.Colors.White;
                titleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;

                SetupStoreServices();

                InitializeAuthProvider();

            }
        }

        public async void SetupStoreServices()
        {
            StoreServicesEngagementManager engagementManager = StoreServicesEngagementManager.GetDefault();
            await engagementManager.RegisterNotificationChannelAsync();
        }

       

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
