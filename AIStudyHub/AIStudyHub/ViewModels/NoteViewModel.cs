using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using AIStudyHub.Models;
using AIStudyHub.Data;
using AIStudyHub.Messages;

namespace AIStudyHub.ViewModels
{
    public partial class NoteViewModel : ObservableObject
    {
        private readonly AppDbContext _dbContext;

        [ObservableProperty]
        private ObservableCollection<Note> _notes = new();

        [ObservableProperty]
        private ObservableCollection<Subject> _subjects = new();

        [ObservableProperty]
        private Subject? _selectedSubject;

        [ObservableProperty]
        private Note? _selectedNote;

        [ObservableProperty]
        private bool _isEditNoteModalOpen;

        [ObservableProperty]
        private string _editingNoteTitle = string.Empty;

        [ObservableProperty]
        private string _editingNoteContent = string.Empty;

        public NoteViewModel()
        {
            _dbContext = new AppDbContext();
            LoadData();

            WeakReferenceMessenger.Default.Register<NoteViewModel, NoteAddedMessage>(this, (r, m) =>
            {
                r.LoadData();
            });
        }

        private void LoadData()
        {
            _dbContext.ChangeTracker.Clear();
            var user = _dbContext.Users.FirstOrDefault();
            if (user == null)
            {
                user = new User { Id = Guid.NewGuid(), Username = "Default User" };
                _dbContext.Users.Add(user);
                _dbContext.SaveChanges();
            }

            var subjectsList = _dbContext.Subjects.Where(s => s.UserId == user.Id).ToList();
            if (!subjectsList.Any())
            {
                var generalSubject = new Subject { Id = Guid.NewGuid(), UserId = user.Id, Name = "General" };
                _dbContext.Subjects.Add(generalSubject);
                _dbContext.SaveChanges();
                subjectsList.Add(generalSubject);
            }
            Subjects = new ObservableCollection<Subject>(subjectsList);
            SelectedSubject = Subjects.FirstOrDefault();

            var notesList = _dbContext.Notes.OrderByDescending(n => n.UpdatedAt).ToList();
            Notes = new ObservableCollection<Note>(notesList);
        }

        [RelayCommand]
        private void OpenCreateNoteModal()
        {
            SelectedNote = null;
            EditingNoteTitle = "New Note";
            EditingNoteContent = "";
            IsEditNoteModalOpen = true;
        }

        [RelayCommand]
        private void EditNote(Note note)
        {
            if (note == null) return;
            SelectedNote = note;
            EditingNoteTitle = note.Title;
            EditingNoteContent = note.Content;
            IsEditNoteModalOpen = true;
        }

        [RelayCommand]
        private void CloseEditNoteModal()
        {
            IsEditNoteModalOpen = false;
        }

        [RelayCommand]
        private void SaveNote()
        {
            if (string.IsNullOrWhiteSpace(EditingNoteTitle)) return;

            if (SelectedNote == null)
            {
                // Create new
                var newNote = new Note
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = EditingNoteTitle,
                    Content = EditingNoteContent,
                    SubjectId = SelectedSubject?.Id,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _dbContext.Notes.Add(newNote);
                Notes.Insert(0, newNote);
            }
            else
            {
                // Update existing
                SelectedNote.Title = EditingNoteTitle;
                SelectedNote.Content = EditingNoteContent;
                SelectedNote.UpdatedAt = DateTime.Now;
                _dbContext.Notes.Update(SelectedNote);
                
                var index = Notes.IndexOf(SelectedNote);
                if (index >= 0)
                {
                    Notes.RemoveAt(index);
                    Notes.Insert(index, SelectedNote);
                }
            }

            _dbContext.SaveChanges();
            IsEditNoteModalOpen = false;
        }

        [RelayCommand]
        private void DeleteNote(Note note)
        {
            if (note == null) return;
            
            _dbContext.Notes.Remove(note);
            _dbContext.SaveChanges();
            Notes.Remove(note);
        }
    }
}
