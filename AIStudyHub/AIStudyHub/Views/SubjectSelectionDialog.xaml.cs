using System.Collections.Generic;
using System.Windows;
using AIStudyHub.Models;

namespace AIStudyHub.Views
{
    public partial class SubjectSelectionDialog : Window
    {
        public Subject? SelectedSubject { get; private set; }

        public SubjectSelectionDialog(List<Subject> subjects)
        {
            InitializeComponent();
            SubjectComboBox.ItemsSource = subjects;
            if (subjects.Count > 0)
            {
                SubjectComboBox.SelectedIndex = 0;
            }
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedSubject = SubjectComboBox.SelectedItem as Subject;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
