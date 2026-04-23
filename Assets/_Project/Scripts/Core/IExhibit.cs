namespace ScienceMuseum.Core
{
    public interface IExhibit
    {
        string ExhibitId { get; }

        string Title { get; }
        void OnFocusEnter();
        void OnFocusExit();
        void OnActivate();
    }
}