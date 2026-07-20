using System.Windows;
using AIStudyHub.ViewModels;

namespace AIStudyHub
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ChatInput_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Shift)
                {
                    return;
                }
                
                e.Handled = true;
                if (DataContext is MainViewModel vm && vm.SendChatMessageCommand.CanExecute(null))
                {
                    vm.SendChatMessageCommand.Execute(null);
                }
            }
        }
    }
}
