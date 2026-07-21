using System.Linq;
using Xunit;
using AIStudyHub.ViewModels;

namespace AIStudyHub.Tests
{
    public class FlashcardViewModelTests
    {
        [Fact]
        public void LoadDueCards_PopulatesListAndSetsFirstCard()
        {
            // Arrange
            var viewModel = new FlashcardViewModel();
            
            // Act
            viewModel.LoadDueCardsForDeck("deck-123");

            // Assert
            Assert.Equal(2, viewModel.DueFlashcards.Count);
            Assert.NotNull(viewModel.CurrentCard);
            Assert.Equal("What does MVVM stand for?", viewModel.CurrentCard.FrontText);
            Assert.False(viewModel.IsFlipped);
        }

        [Fact]
        public void FlipCardCommand_TogglesIsFlipped()
        {
            // Arrange
            var viewModel = new FlashcardViewModel();
            viewModel.LoadDueCardsForDeck("deck-123"); // Sets IsFlipped to false

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
            var viewModel = new FlashcardViewModel();
            viewModel.LoadDueCardsForDeck("deck-123");
            
            var firstCard = viewModel.CurrentCard;
            Assert.NotNull(firstCard);

            // Act - user remembers perfectly (Quality 5)
            viewModel.RateCardCommand.Execute(5);

            // Assert
            // The card should have updated logic applied
            Assert.Equal(1, firstCard.Interval);
            
            // The card should be removed from the due list
            Assert.Single(viewModel.DueFlashcards);
            
            // The next card should be loaded
            Assert.NotNull(viewModel.CurrentCard);
            Assert.Equal("What does RAG stand for in AI?", viewModel.CurrentCard.FrontText);
            Assert.False(viewModel.IsFlipped); // Reset to front side
        }
    }
}
