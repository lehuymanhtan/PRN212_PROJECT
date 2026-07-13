using System.Windows;
using System.Windows.Controls;
using AIStudyHub.ViewModels;

namespace AIStudyHub.Views
{
    public partial class FlashcardView : UserControl
    {
        public FlashcardView()
        {
            InitializeComponent();
            Loaded += FlashcardView_Loaded;
        }

        private void FlashcardView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is FlashcardViewModel vm)
            {
                vm.LoadDueCardsForDeck("Deck1"); // Dummy deck ID for demo
            }
        }
    }
}
