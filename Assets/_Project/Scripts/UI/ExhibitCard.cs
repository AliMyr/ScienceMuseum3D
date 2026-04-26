using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ScienceMuseum.Core;
using ScienceMuseum.Managers;

namespace ScienceMuseum.UI
{
    /// <summary>
    /// Одна карточка экспоната в главной панели.
    /// Привязывается к IExhibit через Bind() и сама обновляет своё состояние.
    /// При клике на "Перейти" вызывает callback переданный извне.
    /// </summary>
    public class ExhibitCard : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI topicText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button goButton;
        [SerializeField] private Image cardBackground;

        [Header("Цвета статусов")]
        [SerializeField] private Color colorNotStudied = new Color(0.16f, 0.22f, 0.34f);
        [SerializeField] private Color colorStudied = new Color(0.18f, 0.32f, 0.40f);
        [SerializeField] private Color colorComplete = new Color(0.18f, 0.42f, 0.30f);

        private IExhibit _exhibit;
        private System.Action<IExhibit> _onGoClicked;

        public void Bind(IExhibit exhibit, System.Action<IExhibit> onGoClicked)
        {
            _exhibit = exhibit;
            _onGoClicked = onGoClicked;

            if (titleText != null) titleText.text = exhibit.Title;
            if (topicText != null) topicText.text = $"{exhibit.Topic} · {exhibit.Grade}";

            if (goButton != null)
            {
                goButton.onClick.RemoveAllListeners();
                goButton.onClick.AddListener(() => _onGoClicked?.Invoke(_exhibit));
            }

            Refresh();
        }

        public void Refresh()
        {
            if (_exhibit == null) return;

            // Прогресс заданий
            int total = _exhibit.Challenges?.Length ?? 0;
            int completed = 0;
            if (_exhibit.Challenges != null && ProgressManager.Instance != null)
            {
                foreach (var ch in _exhibit.Challenges)
                {
                    if (ProgressManager.Instance.IsChallengeCompleted(ch.Id)) completed++;
                }
            }

            if (progressText != null)
                progressText.text = $"Заданий: {completed} / {total}";

            // Статус изучения
            bool studied = ProgressManager.Instance != null &&
                           ProgressManager.Instance.IsExhibitStudied(_exhibit.ExhibitId);
            bool allCompleted = total > 0 && completed >= total;

            if (statusText != null)
            {
                if (allCompleted)
                {
                    statusText.text = "Все задания выполнены";
                    statusText.color = new Color(0.4f, 0.95f, 0.5f);
                }
                else if (studied)
                {
                    statusText.text = "Изучен";
                    statusText.color = new Color(0.85f, 0.85f, 0.95f);
                }
                else
                {
                    statusText.text = "Не изучен";
                    statusText.color = new Color(0.55f, 0.55f, 0.65f);
                }
            }

            // Цвет фона карточки
            if (cardBackground != null)
            {
                cardBackground.color = allCompleted ? colorComplete :
                                       studied ? colorStudied :
                                       colorNotStudied;
            }
        }
    }
}