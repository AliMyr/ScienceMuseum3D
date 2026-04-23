using System.Collections;
using UnityEngine;
using TMPro;
using ScienceMuseum.Managers;
using ScienceMuseum.Core;
using System.Collections.Generic;

namespace ScienceMuseum.UI
{
    public class ChallengeToast : MonoBehaviour
    {
        [SerializeField] private GameObject toastRoot;
        [SerializeField] private TextMeshProUGUI toastText;

        [Header("Настройки")]
        [SerializeField] private float visibleDuration = 2.5f;
        [SerializeField] private float fadeDuration = 0.4f;

        private CanvasGroup _canvasGroup;
        private Coroutine _activeToast;

        // Кэш названий заданий по id (собирается при старте)
        private Dictionary<string, string> _challengeTitles;

        private void Awake()
        {
            if (toastRoot != null)
            {
                _canvasGroup = toastRoot.GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = toastRoot.AddComponent<CanvasGroup>();
                }

                toastRoot.SetActive(false);
            }
        }

        private void Start()
        {
            CacheChallengeTitles();

            if (ProgressManager.Instance != null)
            {
                ProgressManager.Instance.OnChallengeCompleted += OnChallengeCompleted;
                Debug.Log($"[ChallengeToast] Подписка на ProgressManager успешна. Заголовков в кеше: {_challengeTitles.Count}");
            }
            else
            {
                Debug.LogError("[ChallengeToast] ProgressManager.Instance == null в Start! Тосты не будут работать.");
            }
        }

        private void OnDestroy()
        {
            if (ProgressManager.Instance != null)
            {
                ProgressManager.Instance.OnChallengeCompleted -= OnChallengeCompleted;
            }
        }

        private void CacheChallengeTitles()
        {
            _challengeTitles = new Dictionary<string, string>();
            var mbs = FindObjectsOfType<MonoBehaviour>(true);
            foreach (var mb in mbs)
            {
                if (mb is Exhibits.PendulumExhibit pendulum && pendulum.Challenges != null)
                {
                    foreach (var ch in pendulum.Challenges)
                    {
                        if (!_challengeTitles.ContainsKey(ch.Id))
                            _challengeTitles[ch.Id] = ch.Title;
                    }
                }
            }
        }

        private void OnChallengeCompleted(string challengeId)
        {
            Debug.Log($"[ChallengeToast] Получено событие: {challengeId}");

            string title = _challengeTitles.TryGetValue(challengeId, out var t)
                ? t
                : challengeId;

            ShowToast($"Задание выполнено: «{title}»");
        }

        public void ShowToast(string message)
        {
            if (toastRoot == null || toastText == null) return;

            toastText.text = message;

            if (_activeToast != null)
            {
                StopCoroutine(_activeToast);
            }
            _activeToast = StartCoroutine(ToastRoutine());
        }

        private IEnumerator ToastRoutine()
        {
            toastRoot.SetActive(true);
            _canvasGroup.alpha = 0f;

            // Fade in
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            _canvasGroup.alpha = 1f;

            // Hold
            yield return new WaitForSeconds(visibleDuration);

            // Fade out
            t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                _canvasGroup.alpha = 1f - Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            _canvasGroup.alpha = 0f;

            toastRoot.SetActive(false);
            _activeToast = null;
        }
    }
}