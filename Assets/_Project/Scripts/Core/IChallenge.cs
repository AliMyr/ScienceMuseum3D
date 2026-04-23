namespace ScienceMuseum.Core
{
    public enum ChallengeStatus
    {
        NotStarted,
        InProgress,
        Completed
    }

    public interface IChallenge
    {
        string Title { get; }

        string Description { get; }

        string Hint { get; }

        ChallengeStatus Status { get; }

        void Evaluate();

        string GetProgressText();
    }
}