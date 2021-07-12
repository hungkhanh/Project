using ToDoList.Controllers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.ViewManagement;
using System;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ToDoList.Views
{
    public sealed partial class MainView : Page
    {
        public ControllersMain ViewModel { get; set; }
        private FlyoutBase ActiveFlyout;

        public MainView()
        {
            this.InitializeComponent();
            Window.Current.SetTitleBar(AppTitleBar);
            ViewModel = App.GetViewModel(contentFrame, todoInAppNotification);
        }

        private async void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SettingsView();
            var result = await dialog.ShowAsync();

        }

        private void ShowFlyout(object sender, RoutedEventArgs e)
        {
            ActiveFlyout = FlyoutBase.GetAttachedFlyout((FrameworkElement)sender);
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private async void ShowDialog(object sender, RoutedEventArgs e)
        {

            if (ActiveFlyout != null)
                ActiveFlyout.Hide();

            var dialog = new EditBoardView(ViewModel);
            var result = await dialog.ShowAsync();
        }

        private void HideFlyout(object sender, RoutedEventArgs e)
        {
            if (ActiveFlyout == null)
                return;

            ActiveFlyout.Hide();
        }


    }
}
