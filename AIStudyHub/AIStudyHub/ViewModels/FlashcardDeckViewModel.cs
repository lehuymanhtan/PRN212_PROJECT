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
    public partial class FlashcardDeckViewModel : ObservableObject
    {
        private readonly AppDbContext _dbContext;

        [ObservableProperty]
        private ObservableCollection<FlashcardDeck> _decks = new();

        [ObservableProperty]
        private ObservableCollection<Subject> _subjects = new();

        [ObservableProperty]
        private Subject? _selectedSubject;

        [ObservableProperty]
        private string _newDeckTitle = string.Empty;

        [ObservableProperty]
        private bool _isCreateDeckModalOpen;

        public FlashcardDeckViewModel()
        {
            _dbContext = new AppDbContext();
            LoadData();
        }

        private void LoadData()
        {
            var user = _dbContext.Users.FirstOrDefault();
            if (user == null)
            {
                user = new User { Id = Guid.NewGuid(), Username = "Default User" };
                _dbContext.Users.Add(user);
                _dbContext.SaveChanges();
            }

            var subjectsList = _dbContext.Subjects.Where(s => s.UserId == user.Id).ToList();
            
            // Ensure at least one subject exists so user can create a deck
            if (!subjectsList.Any())
            {
                var generalSubject = new Subject { Id = Guid.NewGuid(), UserId = user.Id, Name = "General" };
                _dbContext.Subjects.Add(generalSubject);
                _dbContext.SaveChanges();
                subjectsList.Add(generalSubject);
            }
            
            Subjects = new ObservableCollection<Subject>(subjectsList);
            SelectedSubject = Subjects.FirstOrDefault();

            var decksList = _dbContext.FlashcardDecks.ToList();
            Decks = new ObservableCollection<FlashcardDeck>(decksList);
        }

        [RelayCommand]
        private void OpenCreateDeckModal()
        {
            NewDeckTitle = string.Empty;
            IsCreateDeckModalOpen = true;
        }

        [RelayCommand]
        private void CloseCreateDeckModal()
        {
            IsCreateDeckModalOpen = false;
        }

        [RelayCommand]
        private void CreateDeck()
        {
            if (string.IsNullOrWhiteSpace(NewDeckTitle) || SelectedSubject == null) return;

            var newDeck = new FlashcardDeck
            {
                Id = Guid.NewGuid().ToString(),
                Title = NewDeckTitle,
                SubjectId = SelectedSubject.Id.ToString()
            };

            _dbContext.FlashcardDecks.Add(newDeck);
            _dbContext.SaveChanges();

            Decks.Add(newDeck);
            IsCreateDeckModalOpen = false;
        }

        [RelayCommand]
        private void ReviewDeck(string deckId)
        {
            WeakReferenceMessenger.Default.Send(new ReviewDeckMessage(deckId));
        }
    }
}
