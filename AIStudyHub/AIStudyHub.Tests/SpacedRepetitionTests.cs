using System;
using Xunit;
using AIStudyHub.Models;
using AIStudyHub.Services;

namespace AIStudyHub.Tests
{
    public class SpacedRepetitionTests
    {
        [Fact]
        public void ProcessReview_Quality5_FirstReview_SetsIntervalTo1()
        {
            // Arrange
            var card = new Flashcard();
            var service = new SpacedRepetitionService();

            // Act
            service.ProcessReview(card, 5); // Perfect recall

            // Assert
            Assert.Equal(1, card.RepetitionCount);
            Assert.Equal(1, card.Interval);
            Assert.True(card.NextReviewDate.HasValue);
            // Next review should be roughly 1 day from now
            Assert.True((card.NextReviewDate.Value - DateTime.Now).TotalHours >= 23);
        }

        [Fact]
        public void ProcessReview_Quality4_SecondReview_SetsIntervalTo6()
        {
            // Arrange
            var card = new Flashcard();
            var service = new SpacedRepetitionService();

            // Act - Day 1
            service.ProcessReview(card, 4);
            
            // Act - Day 2
            service.ProcessReview(card, 4);

            // Assert
            Assert.Equal(2, card.RepetitionCount);
            Assert.Equal(6, card.Interval);
        }

        [Fact]
        public void ProcessReview_Quality2_ResetsRepetitionCount()
        {
            // Arrange
            var card = new Flashcard();
            var service = new SpacedRepetitionService();
            service.ProcessReview(card, 5); // 1st
            service.ProcessReview(card, 5); // 2nd

            Assert.Equal(2, card.RepetitionCount);

            // Act - User forgot the card (quality < 3)
            service.ProcessReview(card, 2);

            // Assert
            Assert.Equal(0, card.RepetitionCount);
            Assert.Equal(1, card.Interval); // Back to 1 day interval since rep count is 0
        }

        [Fact]
        public void ProcessReview_Quality5_MultipleReviews_IncreasesEaseAndInterval()
        {
            // Arrange
            var card = new Flashcard();
            var service = new SpacedRepetitionService();
            double initialEase = card.EaseFactor;

            // Act
            service.ProcessReview(card, 5); // 1st (Interval=1)
            service.ProcessReview(card, 5); // 2nd (Interval=6)
            service.ProcessReview(card, 5); // 3rd (Interval = 6 * EF)

            // Assert
            Assert.Equal(3, card.RepetitionCount);
            Assert.True(card.EaseFactor > initialEase);
            Assert.True(card.Interval > 6);
        }
    }
}
