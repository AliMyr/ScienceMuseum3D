using UnityEngine;
using TMPro;
using ScienceMuseum.Managers;
using ScienceMuseum.Core;

namespace ScienceMuseum.UI
{
    public class ProgressBarHUD : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI challengesCountText;
        [SerializeField] private TextMeshProUGUI exhibitsCountText;

        [Header("Общее число (статическое - сколько всего в игре)")]
        [Tooltip("Всего заданий в игре - узнаётся автоматически при старте")]
        [SerializeField] private int totalChallenges = 0;

        [Tooltip("Всего экспонатов - узнаётся автоматически при старте")]
        [SerializeField] private int totalExhibits = 0;

        private void Start()
        {
            // Считаем общее число при старте, сканируя сцену
            CountTotals();

            // Подписываемся на изменения прогресса
            if (ProgressManager.Instance != null)
            {
                ProgressManager.Instance.OnProgressChanged += Refresh;
            }

            Refresh();
        }

        private void OnDestroy()
        {
            // Обязательно отписываемся чтобы не было утечки и ошибок при смене сцены
            if (ProgressManager.Instance != null)
            {
                ProgressManager.Instance.OnProgressChanged -= Refresh;
            }
        }

        private void CountTotals()
        {
            int exhibitCount = 0;
            int challengeCount = 0;

            var mbs = FindObjectsOfType<MonoBehaviour>(true);
            foreach (var mb in mbs)
            {
                if (mb is IExhibit exhibit)
                {
                    exhibitCount++;
                    if (exhibit.Challenges != null)
                    {
                        challengeCount += exhibit.Challenges.Length;
                    }
                }
            }

            totalExhibits = exhibitCount;
            totalChallenges = challengeCount;

            Debug.Log($"[ProgressHUD] Найдено в сцене: {exhibitCount} экспонатов, " +
                      $"{challengeCount} заданий");
        }

        private void Refresh()
        {
            if (ProgressManager.Instance == null) return;

            int completedChallenges = ProgressManager.Instance.CompletedChallengesCount;
            int studiedExhibits = ProgressManager.Instance.StudiedExhibitsCount;

            if (challengesCountText != null)
            {
                challengesCountText.text = $"Заданий: {completedChallenges} / {totalChallenges}";
                challengesCountText.color = completedChallenges >= totalChallenges && totalChallenges > 0
                    ? new Color(0.3f, 0.9f, 0.3f)  // зелёный если всё
                    : Color.white;
            }

            if (exhibitsCountText != null)
            {
                exhibitsCountText.text = $"Экспонатов: {studiedExhibits} / {totalExhibits}";
                exhibitsCountText.color = studiedExhibits >= totalExhibits && totalExhibits > 0
                    ? new Color(0.3f, 0.9f, 0.3f)
                    : Color.white;
            }
        }
    }
}