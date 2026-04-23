using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScienceMuseum.Managers
{
    public class ProgressManager : MonoBehaviour
    {
        public static ProgressManager Instance { get; private set; }

        public event Action<string> OnChallengeCompleted;

        public event Action<string> OnExhibitStudied;

        public event Action OnProgressChanged;

        private readonly HashSet<string> _completedChallenges = new HashSet<string>();
        private readonly HashSet<string> _studiedExhibits = new HashSet<string>();

        // Ключи для сохранения в PlayerPrefs
        private const string KeyChallenges = "progress.completed_challenges";
        private const string KeyExhibits = "progress.studied_exhibits";
        private const string Separator = ";";


        private void Awake()
        {
            // Обеспечиваем единственность
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadProgress();
        }

        public void CompleteChallenge(string challengeId)
        {
            if (string.IsNullOrEmpty(challengeId)) return;
            if (_completedChallenges.Contains(challengeId)) return;

            _completedChallenges.Add(challengeId);
            SaveProgress();

            OnChallengeCompleted?.Invoke(challengeId);
            OnProgressChanged?.Invoke();
        }

        public void MarkExhibitStudied(string exhibitId)
        {
            if (string.IsNullOrEmpty(exhibitId)) return;
            if (_studiedExhibits.Contains(exhibitId)) return;

            _studiedExhibits.Add(exhibitId);
            SaveProgress();

            OnExhibitStudied?.Invoke(exhibitId);
            OnProgressChanged?.Invoke();
        }

        public bool IsChallengeCompleted(string challengeId)
        {
            return _completedChallenges.Contains(challengeId);
        }

        public bool IsExhibitStudied(string exhibitId)
        {
            return _studiedExhibits.Contains(exhibitId);
        }

        public int CompletedChallengesCount => _completedChallenges.Count;
        public int StudiedExhibitsCount => _studiedExhibits.Count;

        public IReadOnlyCollection<string> CompletedChallenges => _completedChallenges;
        public IReadOnlyCollection<string> StudiedExhibits => _studiedExhibits;

        public void ResetAll()
        {
            _completedChallenges.Clear();
            _studiedExhibits.Clear();
            SaveProgress();
            OnProgressChanged?.Invoke();
        }

        private void LoadProgress()
        {
            // Задания
            string challengesStr = PlayerPrefs.GetString(KeyChallenges, "");
            if (!string.IsNullOrEmpty(challengesStr))
            {
                foreach (var id in challengesStr.Split(Separator,
                    StringSplitOptions.RemoveEmptyEntries))
                {
                    _completedChallenges.Add(id);
                }
            }

            // Экспонаты
            string exhibitsStr = PlayerPrefs.GetString(KeyExhibits, "");
            if (!string.IsNullOrEmpty(exhibitsStr))
            {
                foreach (var id in exhibitsStr.Split(Separator,
                    StringSplitOptions.RemoveEmptyEntries))
                {
                    _studiedExhibits.Add(id);
                }
            }

            Debug.Log($"[Progress] Загружено: {_completedChallenges.Count} заданий, " +
                      $"{_studiedExhibits.Count} экспонатов");
        }

        private void SaveProgress()
        {
            PlayerPrefs.SetString(KeyChallenges, string.Join(Separator, _completedChallenges));
            PlayerPrefs.SetString(KeyExhibits, string.Join(Separator, _studiedExhibits));
            PlayerPrefs.Save();
        }

        [ContextMenu("Reset All Progress")]
        private void DebugResetAll()
        {
            ResetAll();
            Debug.Log("[Progress] Весь прогресс сброшен.");
        }

        [ContextMenu("Print Status")]
        private void DebugPrintStatus()
        {
            Debug.Log($"[Progress] Заданий: {_completedChallenges.Count} " +
                      $"({string.Join(", ", _completedChallenges)})");
            Debug.Log($"[Progress] Экспонатов: {_studiedExhibits.Count} " +
                      $"({string.Join(", ", _studiedExhibits)})");
        }
    }
}