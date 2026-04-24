namespace ScienceMuseum.Core
{
    public interface IExhibit
    {
        string ExhibitId { get; }
        string Title { get; }

        string Description { get; }

        ExhibitParameter[] Parameters { get; }

        IChallenge[] Challenges { get; }

        string GetFormulaText();

        void ResetSimulation();

        void OnFocusEnter();
        void OnFocusExit();
        void OnActivate();
    }
}