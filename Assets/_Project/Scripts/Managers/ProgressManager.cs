using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScienceMuseum.Managers
{
    public class ProgressManager : MonoBehaviour
    {
        public static ProgressManager Instance { get; private set; }

        public event Action<string> OnChallengeCompleted;
        public event Action<string> OnExhibitStudied;
        public event Action OnProgressChanged;

        private const string KeyChallenges = "progress.completed_challenges";
        private const string KeyExhibits = "progress.studied_exhibits";
        private const string Separator = ";";

        private readonly HashSet<string> _completedChallenges = new HashSet<string>();
        private readonly HashSet<string> _studiedExhibits = new HashSet<string>();

        public int CompletedChallengesCount => _completedChallenges.Count;
        public int StudiedExhibitsCount => _studiedExhibits.Count;
        public IReadOnlyCollection<string> CompletedChallenges => _completedChallenges;
        public IReadOnlyCollection<string> StudiedExhibits => _studiedExhibits;

        private void Awake()
        {
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
            if (!_completedChallenges.Add(challengeId)) return;

            SaveProgress();
            OnChallengeCompleted?.Invoke(challengeId);
            OnProgressChanged?.Invoke();
        }

        public void MarkExhibitStudied(string exhibitId)
        {
            if (string.IsNullOrEmpty(exhibitId)) return;
            if (!_studiedExhibits.Add(exhibitId)) return;

            SaveProgress();
            OnExhibitStudied?.Invoke(exhibitId);
            OnProgressChanged?.Invoke();
        }

        public bool IsChallengeCompleted(string challengeId) =>
            _completedChallenges.Contains(challengeId);

        public bool IsExhibitStudied(string exhibitId) =>
            _studiedExhibits.Contains(exhibitId);

        public void ResetAll()
        {
            _completedChallenges.Clear();
            _studiedExhibits.Clear();
            SaveProgress();
            OnProgressChanged?.Invoke();
        }

        private void LoadProgress()
        {
            LoadSet(KeyChallenges, _completedChallenges);
            LoadSet(KeyExhibits, _studiedExhibits);
        }

        private static void LoadSet(string key, HashSet<string> target)
        {
            string raw = PlayerPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(raw)) return;

            foreach (var id in raw.Split(Separator, StringSplitOptions.RemoveEmptyEntries))
            {
                target.Add(id);
            }
        }

        private void SaveProgress()
        {
            PlayerPrefs.SetString(KeyChallenges, string.Join(Separator, _completedChallenges));
            PlayerPrefs.SetString(KeyExhibits, string.Join(Separator, _studiedExhibits));
            PlayerPrefs.Save();
        }

        [ContextMenu("Reset All Progress")]
        private void DebugResetAll() => ResetAll();
    }
}