using System;
using System.Linq;
using AIStudyHub.Data;
using AIStudyHub.Models;
using AIStudyHub.ViewModels;
using Xunit;

namespace AIStudyHub.Tests
{
    public class FlashcardViewModelTests
    {
        private void SeedFlashcard(string deckId)
        {
            using var db = new AppDbContext();
            db.Database.EnsureCreated();
            db.EnsureFlashcardAndAnnotationTablesCreated();

            db.Flashcards.RemoveRange(db.Flashcards.Where(f => f.DeckId == deckId));
            db.SaveChanges();

            db.Flashcards.Add(new Flashcard
            {
                Id = Guid.NewGuid().ToString(),
                DeckId = deckId,
                FrontText = "What does MVVM stand for?",
                BackText = "Model-View-ViewModel",
                NextReviewDate = null
            });
            db.SaveChanges();
        }

        [Fact]
        public void LoadDueCards_PopulatesListAndSetsFirstCard()
        {
            // Arrange
            SeedFlashcard("deck-123");
            var viewModel = new FlashcardViewModel("deck-123");

            // Assert
            Assert.NotNull(viewModel.CurrentCard);
            Assert.Equal("What does MVVM stand for?", viewModel.CurrentCard.FrontText);
            Assert.False(viewModel.IsFlipped);
        }

        [Fact]
        public void FlipCardCommand_TogglesIsFlipped()
        {
            // Arrange
            SeedFlashcard("deck-123");
            var viewModel = new FlashcardViewModel("deck-123");

            // Act 1
            viewModel.FlipCardCommand.Execute(null);

            // Assert 1
            Assert.True(viewModel.IsFlipped);

            // Act 2
            viewModel.FlipCardCommand.Execute(null);

            // Assert 2
            Assert.False(viewModel.IsFlipped);
        }

        [Fact]
        public void RateCardCommand_UpdatesCardAndAdvancesToNext()
        {
            // Arrange
            SeedFlashcard("deck-123");
            var viewModel = new FlashcardViewModel("deck-123");

            var firstCard = viewModel.CurrentCard;
            if (firstCard != null)
            {
                // Act - user remembers perfectly (Quality 5)
                viewModel.RateCardCommand.Execute(5);

                // Assert
                Assert.Equal(1, firstCard.Interval);
                Assert.False(viewModel.IsFlipped); // Reset to front side
            }
        }
    }
}
