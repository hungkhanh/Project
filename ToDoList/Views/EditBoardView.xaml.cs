using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ToDoList.Views
{
    public sealed partial class EditBoardView : ContentDialog
    {
        public Controllers.ControllersMain ViewModel { get; set; }
        public EditBoardView(Controllers.ControllersMain viewModel)
        {
            this.InitializeComponent();

            ViewModel = viewModel;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        // cancel flyout
        private void btnCloseNewBoardFlyout_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        // create board
        private void flyoutBtnCreateNewBoard_Click(object sender, RoutedEventArgs e)
        {
            if (txtBoxNewBoardName.Text == "")
                ChooseBoardNameTeachingTip.IsOpen = true;
            if (txtBoxNewBoardNotes.Text == "")
                AddBoardNotesTeachingTip.IsOpen = true;
            if (txtBoxNewBoardName.Text != "" && txtBoxNewBoardNotes.Text != "")
            {
                this.Hide();
            }

        }

        // cancel flyout
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
