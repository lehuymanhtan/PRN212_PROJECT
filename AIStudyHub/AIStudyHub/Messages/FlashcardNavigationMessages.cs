using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AIStudyHub.Messages
{
    public class ReviewDeckMessage : ValueChangedMessage<string>
    {
        public ReviewDeckMessage(string deckId) : base(deckId)
        {
        }
    }

    public class BackToDecksMessage
    {
    }
}
