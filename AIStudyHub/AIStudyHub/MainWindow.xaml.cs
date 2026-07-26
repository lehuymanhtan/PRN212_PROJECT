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

        private void MarkdownScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new System.Windows.Input.MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = UIElement.MouseWheelEvent,
                    Source = sender
                };
                var parent = ((System.Windows.FrameworkElement)sender).Parent as UIElement;
                parent?.RaiseEvent(eventArg);
            }
        }

        private Point _dragStartPoint;
        private Thickness _startMargin;
        private bool _isDragging = false;
        private bool _hasDragged = false;

        private void AIFab_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(this);
            _startMargin = AIFab.Margin;
            _isDragging = false;
            _hasDragged = false;
        }

        private void AIFab_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(this);
                Vector diff = currentPoint - _dragStartPoint;

                if (!_hasDragged && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                                     Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    _hasDragged = true;
                    _isDragging = true;
                    AIFab.CaptureMouse(); // Capture only when we actually start dragging
                }

                if (_hasDragged)
                {
                    double newRight = _startMargin.Right - diff.X;
                    double newTop = _startMargin.Top + diff.Y;

                    if (AIFab.Parent is System.Windows.Controls.Grid parentGrid)
                    {
                        double col1Width = parentGrid.ColumnDefinitions[1].ActualWidth;
                        double gridHeight = parentGrid.ActualHeight;

                        if (newRight < 0) newRight = 0;
                        if (newRight > col1Width - AIFab.ActualWidth && col1Width > 0) 
                            newRight = col1Width - AIFab.ActualWidth;

                        if (newTop < 0) newTop = 0;
                        if (newTop > gridHeight - AIFab.ActualHeight && gridHeight > 0) 
                            newTop = gridHeight - AIFab.ActualHeight;
                    }

                    AIFab.Margin = new Thickness(0, newTop, newRight, 0);
                }
            }
        }

        private void AIFab_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_hasDragged)
            {
                _isDragging = false;
                _hasDragged = false;
                AIFab.ReleaseMouseCapture();
                e.Handled = true; // Prevent the click event if we dragged
            }
        }

        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (AIFab != null && AIFab.Parent is System.Windows.Controls.Grid parentGrid)
            {
                double col1Width = parentGrid.ColumnDefinitions[1].ActualWidth;
                double gridHeight = parentGrid.ActualHeight;

                double currentRight = AIFab.Margin.Right;
                double currentTop = AIFab.Margin.Top;
                bool changed = false;

                if (currentRight < 0) { currentRight = 0; changed = true; }
                if (currentRight > col1Width - AIFab.ActualWidth && col1Width > 0) 
                { 
                    currentRight = col1Width - AIFab.ActualWidth; 
                    if (currentRight < 0) currentRight = 0;
                    changed = true; 
                }

                if (currentTop < 0) { currentTop = 0; changed = true; }
                if (currentTop > gridHeight - AIFab.ActualHeight && gridHeight > 0) 
                { 
                    currentTop = gridHeight - AIFab.ActualHeight; 
                    if (currentTop < 0) currentTop = 0;
                    changed = true; 
                }

                if (changed)
                {
                    AIFab.Margin = new Thickness(0, currentTop, currentRight, 0);
                }
            }
        }
    }
}
