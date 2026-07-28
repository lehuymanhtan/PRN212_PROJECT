using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AIStudyHub.Models;
using AIStudyHub.Services;
using AIStudyHub.Data;
using CommunityToolkit.Mvvm.Messaging;
using AIStudyHub.Messages;

namespace AIStudyHub.ViewModels
{
    public partial class FlashcardViewModel : ObservableObject
    {
        private readonly AppDbContext _dbContext;
        private readonly SpacedRepetitionService _spacedRepetitionService;
        private readonly string _deckId;

        public FlashcardViewModel(string deckId)
        {
            _dbContext = new AppDbContext();
            _dbContext.EnsureFlashcardAndAnnotationTablesCreated();
            
            _deckId = deckId;
            _spacedRepetitionService = new SpacedRepetitionService();
            DueFlashcards = new ObservableCollection<Flashcard>();
            ForgottenCards = new ObservableCollection<Flashcard>();
            
            LoadDueCardsForDeck(_deckId);
        }

        [ObservableProperty]
        private ObservableCollection<Flashcard> _dueFlashcards;

        [ObservableProperty]
        private ObservableCollection<Flashcard> _allCards;

        [ObservableProperty]
        private bool _isLearningMode;

        [ObservableProperty]
        private ObservableCollection<Flashcard> _forgottenCards;

        [ObservableProperty]
        private bool _isSidebarVisible;

        [ObservableProperty]
        private Flashcard? _currentCard;

        [ObservableProperty]
        private bool _isFlipped;

        [ObservableProperty]
        private string _feedbackMessage = string.Empty;

        [ObservableProperty]
        private string _newFrontText = string.Empty;

        [ObservableProperty]
        private string _newBackText = string.Empty;

        [ObservableProperty]
        private bool _isAddCardModalOpen;

        [RelayCommand]
        private void OpenAddCardModal()
        {
            IsAddCardModalOpen = true;
            NewFrontText = string.Empty;
            NewBackText = string.Empty;
        }

        [RelayCommand]
        private void CloseAddCardModal()
        {
            IsAddCardModalOpen = false;
        }

        [RelayCommand]
        private void ToggleSidebar()
        {
            IsSidebarVisible = !IsSidebarVisible;
        }

        [RelayCommand]
        private void StartLearning()
        {
            IsLearningMode = true;
            LoadNextCard();
        }

        [RelayCommand]
        private void StopLearning()
        {
            IsLearningMode = false;
        }

        [RelayCommand]
        private void DeleteCard(Flashcard card)
        {
            if (card == null) return;
            
            _dbContext.Flashcards.Remove(card);
            _dbContext.SaveChanges();
            
            AllCards.Remove(card);
            DueFlashcards.Remove(card);
            ForgottenCards.Remove(card);
            
            if (CurrentCard == card)
            {
                LoadNextCard();
            }
        }

        [RelayCommand]
        private void DeleteDeck()
        {
            var deck = _dbContext.FlashcardDecks.FirstOrDefault(d => d.Id == _deckId);
            if (deck != null)
            {
                _dbContext.FlashcardDecks.Remove(deck);
                _dbContext.SaveChanges();
            }
            WeakReferenceMessenger.Default.Send(new BackToDecksMessage());
        }

        [RelayCommand]
        private void ResetProgress()
        {
            var allCardsList = _dbContext.Flashcards.Where(f => f.DeckId == _deckId).ToList();
            foreach (var card in allCardsList)
            {
                card.NextReviewDate = DateTime.Now;
                card.RepetitionCount = 0;
                card.EaseFactor = 2.5;
                card.Interval = 0;
            }
            _dbContext.SaveChanges();
            LoadDueCardsForDeck(_deckId);
            FeedbackMessage = "Deck progress has been reset.";
        }

        public void LoadDueCardsForDeck(string deckId)
        {
            DueFlashcards.Clear();
            if (AllCards == null)
            {
                AllCards = new ObservableCollection<Flashcard>();
            }
            AllCards.Clear();
            
            var allCardsList = _dbContext.Flashcards.Where(f => f.DeckId == deckId).ToList();
            foreach (var card in allCardsList)
            {
                AllCards.Add(card);
                if (card.NextReviewDate == null || card.NextReviewDate <= DateTime.Now)
                {
                    DueFlashcards.Add(card);
                }
            }

            LoadNextCard();
        }

        private void LoadNextCard()
        {
            IsFlipped = false;
            CurrentCard = DueFlashcards.FirstOrDefault();
        }

        [RelayCommand]
        private void FlipCard()
        {
            if (CurrentCard != null)
            {
                IsFlipped = !IsFlipped;
                FeedbackMessage = string.Empty;
            }
        }

        [RelayCommand]
        private void RateCard(int quality)
        {
            if (CurrentCard == null) return;

            // 1. Calculate new NextReviewDate
            _spacedRepetitionService.ProcessReview(CurrentCard, quality);

            // 2. Save 'CurrentCard' back to the database
            _dbContext.Flashcards.Update(CurrentCard);
            _dbContext.SaveChanges();
            
            var processedCard = CurrentCard;
            DueFlashcards.Remove(processedCard);

            if (quality < 3)
            {
                DueFlashcards.Add(processedCard);
                if (!ForgottenCards.Contains(processedCard))
                {
                    ForgottenCards.Add(processedCard);
                }
                FeedbackMessage = "Card forgotten! It will appear again in this session.";
            }
            else
            {
                if (ForgottenCards.Contains(processedCard))
                {
                    ForgottenCards.Remove(processedCard);
                }
                FeedbackMessage = $"Card saved! Next review in {processedCard.Interval} days.";
            }

            LoadNextCard();
        }

        [RelayCommand]
        private void BackToDecks()
        {
            WeakReferenceMessenger.Default.Send(new BackToDecksMessage());
        }

        [RelayCommand]
        private void AddFlashcard()
        {
            if (string.IsNullOrWhiteSpace(NewFrontText) || string.IsNullOrWhiteSpace(NewBackText))
            {
                FeedbackMessage = "Please enter both front and back text.";
                return;
            }

            var newCard = new Flashcard
            {
                Id = Guid.NewGuid().ToString(),
                DeckId = _deckId,
                FrontText = NewFrontText.Trim(),
                BackText = NewBackText.Trim(),
                NextReviewDate = DateTime.Now
            };

            _dbContext.Flashcards.Add(newCard);
            _dbContext.SaveChanges();

            AllCards.Add(newCard);
            DueFlashcards.Add(newCard);
            
            if (CurrentCard == null)
            {
                LoadNextCard();
            }

            FeedbackMessage = "Flashcard added successfully!";
            NewFrontText = string.Empty;
            NewBackText = string.Empty;
            IsAddCardModalOpen = false;
        }
    }
}
