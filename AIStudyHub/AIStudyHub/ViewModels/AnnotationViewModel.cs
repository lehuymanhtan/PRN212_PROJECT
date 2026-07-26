using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AIStudyHub.Models;
using AIStudyHub.Data;

namespace AIStudyHub.ViewModels
{
    public partial class AnnotationViewModel : ObservableObject
    {
        private readonly AppDbContext _dbContext;

        [ObservableProperty]
        private ObservableCollection<Annotation> _documentAnnotations;

        public AnnotationViewModel()
        {
            _dbContext = new AppDbContext();
            DocumentAnnotations = new ObservableCollection<Annotation>();
        }

        public void LoadAnnotationsForDocument(string documentId)
        {
            DocumentAnnotations.Clear();
            var annotations = _dbContext.Annotations.Where(a => a.DocumentId == documentId).ToList();
            foreach (var annotation in annotations)
            {
                DocumentAnnotations.Add(annotation);
            }
        }

        public void AddHighlight(int page, double x, double y, string content)
        {
            var newHighlight = new Annotation
            {
                DocumentId = "sample_doc_id", // Placeholder until document selection is fully implemented
                PageNumber = page,
                PosX = x,
                PosY = y,
                Content = content,
                Type = "Highlight"
            };

            DocumentAnnotations.Add(newHighlight);

            _dbContext.Annotations.Add(newHighlight);
            _dbContext.SaveChanges();
        }

        public void AddNote(int page, double x, double y, string content)
        {
            var newNote = new Annotation
            {
                DocumentId = "sample_doc_id", // Placeholder until document selection is fully implemented
                PageNumber = page,
                PosX = x,
                PosY = y,
                Content = content,
                Type = "Note"
            };

            DocumentAnnotations.Add(newNote);

            _dbContext.Annotations.Add(newNote);
            _dbContext.SaveChanges();
        }
    }
}
