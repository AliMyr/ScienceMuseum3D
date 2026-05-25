using UnityEngine;

namespace ScienceMuseum.Core
{
    /// <summary>
    /// Контракт экспоната-модуля: идентификация, метаданные, параметры, задачи,
    /// визуальная подсветка и реакция на активацию.
    /// </summary>
    public interface IExhibit
    {
        string ExhibitId { get; }
        string Title { get; }
        string Description { get; }
        string Topic { get; }
        string Grade { get; }
        Transform ViewPoint { get; }

        ExhibitParameter[] Parameters { get; }
        IChallenge[] Challenges { get; }

        string GetFormulaText();
        void ResetSimulation();

        void OnFocusEnter();
        void OnFocusExit();
        void OnActivate();
    }
}