using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ScienceMuseum.Core;
using ScienceMuseum.Managers;

namespace ScienceMuseum.UI
{
    public class TaskCardController : MonoBehaviour
    {
        [Header("Текстовые поля")]
        [SerializeField] private TextMeshProUGUI counterText;     // "Задание 2 из 4"
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI progressInfoText; // "Сейчас: T = 1.85"
        [SerializeField] private TextMeshProUGUI feedbackText;

        [Header("Кнопки")]
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button checkButton;
        [SerializeField] private Button showSolutionButton;

        [Header("Индикатор обратной связи (фон)")]
        [SerializeField] private Image feedbackBackground;

        [Header("Индикатор всех заданий (точки)")]
        [SerializeField] private RectTransform dotsContainer;
        [SerializeField] private Image dotPrefab;

        [Header("Цвета обратной связи")]
        [SerializeField] private Color colorNeutral = new Color(0.2f, 0.2f, 0.27f, 0.6f);
        [SerializeField] private Color colorCorrect = new Color(0.1f, 0.5f, 0.2f, 0.7f);
        [SerializeField] private Color colorWrong = new Color(0.6f, 0.15f, 0.15f, 0.7f);

        [Header("Цвета точек прогресса")]
        [SerializeField] private Color dotInactive = new Color(0.4f, 0.4f, 0.4f);
        [SerializeField] private Color dotCompleted = new Color(0.3f, 0.9f, 0.3f);
        [SerializeField] private Color dotCurrent = new Color(1f, 0.85f, 0.2f);

        [Header("Настройки")]
        [Tooltip("После скольких ошибок становится доступна кнопка «Показать решение»")]
        [SerializeField] private int failsBeforeSolution = 3;

        // Состояние
        private IChallenge[] _challenges;
        private int _currentIndex;
        private readonly System.Collections.Generic.List<Image> _dots
            = new System.Collections.Generic.List<Image>();

        private void Awake()
        {
            if (prevButton != null) prevButton.onClick.AddListener(GoPrev);
            if (nextButton != null) nextButton.onClick.AddListener(GoNext);
            if (checkButton != null) checkButton.onClick.AddListener(OnCheckClicked);
            if (showSolutionButton != null) showSolutionButton.onClick.AddListener(OnShowSolutionClicked);
        }

        public void SetChallenges(IChallenge[] challenges)
        {
            _challenges = challenges;
            _currentIndex = 0;

            BuildDots();
            ShowCurrentTask();
        }

        private void Update()
        {
            // Раз в кадр обновляем "сейчас" - подписи параметров меняются вместе со слайдерами
            if (_challenges != null && _challenges.Length > 0 && progressInfoText != null)
            {
                var current = _challenges[_currentIndex];
                progressInfoText.text = current.GetProgressText();
            }
        }

        private void ShowCurrentTask()
        {
            if (_challenges == null || _challenges.Length == 0) return;

            var current = _challenges[_currentIndex];

            if (counterText != null)
                counterText.text = $"Задание {_currentIndex + 1} из {_challenges.Length}";

            if (titleText != null) titleText.text = current.Title;
            if (descriptionText != null) descriptionText.text = current.Description;
            if (progressInfoText != null) progressInfoText.text = current.GetProgressText();

            // Кнопки навигации - выкл если не куда идти
            if (prevButton != null) prevButton.interactable = _currentIndex > 0;
            if (nextButton != null) nextButton.interactable = _currentIndex < _challenges.Length - 1;

            // Сбрасываем feedback-блок и кнопки в зависимости от состояния
            UpdateFeedbackForCurrentState();
            UpdateDots();
        }

        private void UpdateFeedbackForCurrentState()
        {
            var current = _challenges[_currentIndex];

            switch (current.Status)
            {
                case ChallengeStatus.Completed:
                    SetFeedback("✓ Это задание уже выполнено правильно!", colorCorrect);
                    if (checkButton != null) checkButton.interactable = false;
                    break;
                case ChallengeStatus.Failed:
                    SetFeedback($"Неверно. Попыток: {current.FailedAttempts}. Подумай ещё.\n\n{current.Hint}",
                                colorWrong);
                    if (checkButton != null) checkButton.interactable = true;
                    break;
                default:
                    SetFeedback("Подбери параметры с помощью слайдеров слева.\nКогда готов — нажми «Проверить».",
                                colorNeutral);
                    if (checkButton != null) checkButton.interactable = true;
                    break;
            }

            // Кнопка решения доступна только если уже было N ошибок
            if (showSolutionButton != null)
            {
                showSolutionButton.interactable = current.FailedAttempts >= failsBeforeSolution &&
                                                   current.Status != ChallengeStatus.Completed;
            }
        }

        private void OnCheckClicked()
        {
            if (_challenges == null || _challenges.Length == 0) return;
            var current = _challenges[_currentIndex];

            bool correct = current.CheckAnswer();

            if (correct)
            {
                // Регистрируем в прогресс-менеджере (он сам не будет повторно слать тост)
                ProgressManager.Instance?.CompleteChallenge(current.Id);

                SetFeedback($"✓ Правильно! Задание выполнено.", colorCorrect);
                if (checkButton != null) checkButton.interactable = false;
                if (showSolutionButton != null) showSolutionButton.interactable = false;
            }
            else
            {
                string msg = $"✗ Неверно. Попыток: {current.FailedAttempts}.";
                if (current.FailedAttempts >= failsBeforeSolution)
                {
                    msg += "\nМожешь нажать «Показать решение».";
                }
                else
                {
                    msg += $"\n\nПодсказка:\n{current.Hint}";
                }
                SetFeedback(msg, colorWrong);

                if (showSolutionButton != null)
                {
                    showSolutionButton.interactable = current.FailedAttempts >= failsBeforeSolution;
                }
            }

            UpdateDots();
        }

        private void OnShowSolutionClicked()
        {
            if (_challenges == null || _challenges.Length == 0) return;
            var current = _challenges[_currentIndex];

            SetFeedback(current.SolutionText, colorNeutral);
        }

        private void GoPrev()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                ShowCurrentTask();
            }
        }

        private void GoNext()
        {
            if (_currentIndex < _challenges.Length - 1)
            {
                _currentIndex++;
                ShowCurrentTask();
            }
        }

        private void SetFeedback(string text, Color bgColor)
        {
            if (feedbackText != null) feedbackText.text = text;
            if (feedbackBackground != null) feedbackBackground.color = bgColor;
        }

        // ── Точки прогресса ──────────────────────────────────────────────

        private void BuildDots()
        {
            // Удаляем старые
            foreach (var d in _dots)
                if (d != null) Destroy(d.gameObject);
            _dots.Clear();

            if (_challenges == null || dotsContainer == null || dotPrefab == null) return;

            foreach (var ch in _challenges)
            {
                Image dot = Instantiate(dotPrefab, dotsContainer);
                _dots.Add(dot);
            }

            UpdateDots();
        }

        private void UpdateDots()
        {
            if (_challenges == null) return;

            for (int i = 0; i < _dots.Count && i < _challenges.Length; i++)
            {
                if (_dots[i] == null) continue;

                if (_challenges[i].Status == ChallengeStatus.Completed)
                    _dots[i].color = dotCompleted;
                else if (i == _currentIndex)
                    _dots[i].color = dotCurrent;
                else
                    _dots[i].color = dotInactive;
            }
        }
    }
}