namespace ScienceMuseum.Core
{
    public enum ChallengeStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Failed
    }

    public interface IChallenge
    {
        string Id { get; }
        string Title { get; }
        string Description { get; }
        string Hint { get; }
        ChallengeStatus Status { get; }

        int FailedAttempts { get; }

        string SolutionText { get; }

        bool CheckAnswer();

        string GetProgressText();
    }
}