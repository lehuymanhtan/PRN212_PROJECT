using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AIStudyHub.Models;
using AIStudyHub.Services;

namespace AIStudyHub.ViewModels
{
    public partial class FlashcardViewModel : ObservableObject
    {
        private readonly SpacedRepetitionService _spacedRepetitionService;

        public FlashcardViewModel()
        {
            _spacedRepetitionService = new SpacedRepetitionService();
            DueFlashcards = new ObservableCollection<Flashcard>();
        }

        [ObservableProperty]
        private ObservableCollection<Flashcard> _dueFlashcards;

        [ObservableProperty]
        private Flashcard? _currentCard;

        [ObservableProperty]
        private bool _isFlipped;

        [ObservableProperty]
        private string _feedbackMessage = string.Empty;

        // Dummy method to simulate loading from database
        public void LoadDueCardsForDeck(string deckId)
        {
            // Placeholder: In a real app, inject a Repository and query DB for 
            // cards where DeckId == deckId AND NextReviewDate <= DateTime.Now
            DueFlashcards.Clear();
            
            DueFlashcards.Add(new Flashcard 
            { 
                DeckId = deckId, 
                FrontText = "What does MVVM stand for?", 
                BackText = "Model-View-ViewModel",
                NextReviewDate = DateTime.Now.AddDays(-1)
            });
            
            DueFlashcards.Add(new Flashcard 
            { 
                DeckId = deckId, 
                FrontText = "What does RAG stand for in AI?", 
                BackText = "Retrieval-Augmented Generation",
                NextReviewDate = DateTime.Now.AddDays(-2)
            });

            LoadNextCard();
        }

        private void LoadNextCard()
        {
            IsFlipped = false;
            FeedbackMessage = string.Empty;
            CurrentCard = DueFlashcards.FirstOrDefault();
        }

        [RelayCommand]
        private void FlipCard()
        {
            if (CurrentCard != null)
            {
                IsFlipped = !IsFlipped;
            }
        }

        [RelayCommand]
        private void RateCard(int quality)
        {
            if (CurrentCard == null) return;

            // 1. Calculate new NextReviewDate
            _spacedRepetitionService.ProcessReview(CurrentCard, quality);

            // 2. Placeholder: Save 'CurrentCard' back to the database here
            FeedbackMessage = $"Card saved! Next review in {CurrentCard.Interval} days.";

            // 3. Remove from due list and load next
            DueFlashcards.Remove(CurrentCard);
            LoadNextCard();
        }
    }
}
