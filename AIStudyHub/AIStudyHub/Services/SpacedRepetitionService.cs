using System;
using AIStudyHub.Models;

namespace AIStudyHub.Services
{
    public class SpacedRepetitionService
    {
        /// <summary>
        /// Updates the flashcard's spaced repetition variables based on the user's recall quality.
        /// This implementation is based on the SuperMemo-2 (SM-2) algorithm.
        /// </summary>
        /// <param name="card">The flashcard to update.</param>
        /// <param name="quality">
        /// The quality of the response, from 0 to 5:
        /// 5 - perfect response
        /// 4 - correct response after a hesitation
        /// 3 - correct response recalled with serious difficulty
        /// 2 - incorrect response; where the correct one seemed easy to recall
        /// 1 - incorrect response; the correct one remembered
        /// 0 - complete blackout
        /// </param>
        public void ProcessReview(Flashcard card, int quality)
        {
            if (quality < 0 || quality > 5)
            {
                throw new ArgumentOutOfRangeException(nameof(quality), "Quality must be between 0 and 5.");
            }

            // If the user scores < 3, it means they forgot the card. 
            // We reset the repetition count.
            if (quality < 3)
            {
                card.RepetitionCount = 0;
            }
            else
            {
                card.RepetitionCount += 1;
            }

            // Calculate the next interval based on repetition count
            if (card.RepetitionCount <= 1)
            {
                card.Interval = 1;
            }
            else if (card.RepetitionCount == 2)
            {
                card.Interval = 6;
            }
            else
            {
                card.Interval = (int)Math.Round(card.Interval * card.EaseFactor);
            }

            // Update Ease Factor (EF)
            card.EaseFactor = card.EaseFactor + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02));

            // EF should not be lower than 1.3
            if (card.EaseFactor < 1.3)
            {
                card.EaseFactor = 1.3;
            }

            // Calculate Next Review Date
            card.NextReviewDate = DateTime.Now.AddDays(card.Interval);
        }
    }
}
