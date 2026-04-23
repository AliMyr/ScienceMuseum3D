namespace ScienceMuseum.Core
{
    public interface IExhibit
    {
        string Title { get; }

        void OnFocusEnter();

        void OnFocusExit();

        void OnActivate();
    }
}