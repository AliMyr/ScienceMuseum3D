using UnityEngine;
using TMPro;
using ScienceMuseum.Managers;
using ScienceMuseum.Core;

namespace ScienceMuseum.UI
{
    /// <summary>
    /// HUD-индикатор прогресса: количество выполненных задач и изученных экспонатов.
    /// Общее число подсчитывается один раз при старте сканированием сцены.
    /// </summary>
    public class ProgressBarHUD : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI challengesCountText;
        [SerializeField] private TextMeshProUGUI exhibitsCountText;

        private int _totalChallenges;
        private int _totalExhibits;

        private void Start()
        {
            CountTotals();

            if (ProgressManager.Instance != null)
            {
                ProgressManager.Instance.OnProgressChanged += Refresh;
            }

            Refresh();
        }

        private void OnDestroy()
        {
            if (ProgressManager.Instance != null)
            {
                ProgressManager.Instance.OnProgressChanged -= Refresh;
            }
        }

        private void CountTotals()
        {
            _totalExhibits = 0;
            _totalChallenges = 0;

            var mbs = FindObjectsOfType<MonoBehaviour>(true);
            foreach (var mb in mbs)
            {
                if (mb is IExhibit exhibit)
                {
                    _totalExhibits++;
                    _totalChallenges += exhibit.Challenges?.Length ?? 0;
                }
            }
        }

        private void Refresh()
        {
            if (ProgressManager.Instance == null) return;

            int completedChallenges = ProgressManager.Instance.CompletedChallengesCount;
            int studiedExhibits = ProgressManager.Instance.StudiedExhibitsCount;

            if (challengesCountText != null)
            {
                challengesCountText.text = $"Заданий: {completedChallenges} / {_totalChallenges}";
                challengesCountText.color = completedChallenges >= _totalChallenges && _totalChallenges > 0
                    ? new Color(0.3f, 0.9f, 0.3f)
                    : Color.white;
            }

            if (exhibitsCountText != null)
            {
                exhibitsCountText.text = $"Экспонатов: {studiedExhibits} / {_totalExhibits}";
                exhibitsCountText.color = studiedExhibits >= _totalExhibits && _totalExhibits > 0
                    ? new Color(0.3f, 0.9f, 0.3f)
                    : Color.white;
            }
        }
    }
}