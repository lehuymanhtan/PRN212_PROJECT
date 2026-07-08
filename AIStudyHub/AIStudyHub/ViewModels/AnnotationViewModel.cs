using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AIStudyHub.Models;

namespace AIStudyHub.ViewModels
{
    public partial class AnnotationViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Annotation> _documentAnnotations;

        public AnnotationViewModel()
        {
            DocumentAnnotations = new ObservableCollection<Annotation>();
        }

        // Dummy method to simulate loading from DB
        public void LoadAnnotationsForDocument(string documentId)
        {
            DocumentAnnotations.Clear();
            // Placeholder: Fetch from DB using EF Core
        }

        public void AddHighlight(int page, double x, double y, string content)
        {
            var newHighlight = new Annotation
            {
                DocumentId = "sample_doc_id", // Placeholder
                PageNumber = page,
                PosX = x,
                PosY = y,
                Content = content,
                Type = "Highlight"
            };

            DocumentAnnotations.Add(newHighlight);

            // Placeholder: Save to DB using EF Core
        }

        public void AddNote(int page, double x, double y, string content)
        {
            var newNote = new Annotation
            {
                DocumentId = "sample_doc_id", // Placeholder
                PageNumber = page,
                PosX = x,
                PosY = y,
                Content = content,
                Type = "Note"
            };

            DocumentAnnotations.Add(newNote);

            // Placeholder: Save to DB using EF Core
        }
    }
}
