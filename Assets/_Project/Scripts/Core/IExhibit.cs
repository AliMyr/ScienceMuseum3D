namespace ScienceMuseum.Core
{
    public interface IExhibit
    {
        string ExhibitId { get; }
        string Title { get; }
        string Description { get; }

        string Topic { get; }

        string Grade { get; }

        UnityEngine.Transform ViewPoint { get; }

        ExhibitParameter[] Parameters { get; }
        IChallenge[] Challenges { get; }
        string GetFormulaText();
        void ResetSimulation();

        void OnFocusEnter();
        void OnFocusExit();
        void OnActivate();
    }
}