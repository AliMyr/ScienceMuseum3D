using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ScienceMuseum.Core;
using ScienceMuseum.Managers;

namespace ScienceMuseum.UI
{
    /// <summary>
    /// Всплывающее уведомление при выполнении задачи.
    /// Кеширует заголовки задач при старте, показывает их по событию.
    /// </summary>
    public class ChallengeToast : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI toastText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Настройки")]
        [SerializeField] private float visibleDuration = 2.5f;
        [SerializeField] private float fadeDuration = 0.4f;

        private readonly Dictionary<string, string> _challengeTitles = new Dictionary<string, string>();
        private Coroutine _activeToast;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        private void Start()
        {
            CacheChallengeTitles();

            if (ProgressManager.Instance != null)
            {
                ProgressManager.Instance.OnChallengeCompleted += OnChallengeCompleted;
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
            var mbs = FindObjectsOfType<MonoBehaviour>(true);
            foreach (var mb in mbs)
            {
                if (mb is IExhibit exhibit && exhibit.Challenges != null)
                {
                    foreach (var ch in exhibit.Challenges)
                    {
                        if (!_challengeTitles.ContainsKey(ch.Id))
                            _challengeTitles[ch.Id] = ch.Title;
                    }
                }
            }
        }

        private void OnChallengeCompleted(string challengeId)
        {
            string title = _challengeTitles.TryGetValue(challengeId, out var t)
                ? t
                : challengeId;

            ShowToast($"Задание выполнено: «{title}»");
        }

        public void ShowToast(string message)
        {
            if (toastText == null || canvasGroup == null) return;

            toastText.text = message;

            if (_activeToast != null) StopCoroutine(_activeToast);
            _activeToast = StartCoroutine(ToastRoutine());
        }

        private IEnumerator ToastRoutine()
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;

            yield return new WaitForSeconds(visibleDuration);

            t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;

            _activeToast = null;
        }
    }
}