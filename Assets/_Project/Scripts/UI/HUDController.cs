using UnityEngine;
using TMPro;
using ScienceMuseum.Core;
using ScienceMuseum.Player;

namespace ScienceMuseum.UI
{
    /// <summary>
    /// Подсказка «Нажми E, чтобы изучить ...» — показывается при наведении на экспонат.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Ссылки на UI")]
        [Tooltip("Контейнер подсказки (фон + текст)")]
        [SerializeField] private GameObject interactionHint;

        [Tooltip("Текстовое поле подсказки")]
        [SerializeField] private TextMeshProUGUI hintText;

        [Header("Ссылки на логику")]
        [SerializeField] private ExhibitInteractor interactor;

        [Header("Настройки")]
        [Tooltip("Шаблон подсказки. {0} заменяется на название экспоната")]
        [SerializeField] private string hintTemplate = "Нажми [E] чтобы изучить: {0}";

        private IExhibit _lastShownExhibit;

        private void Awake()
        {
            if (interactor == null) interactor = FindObjectOfType<ExhibitInteractor>();
            if (interactionHint != null) interactionHint.SetActive(false);
        }

        private void Update()
        {
            if (interactor == null || interactionHint == null) return;

            IExhibit current = interactor.CurrentExhibit;
            if (current == _lastShownExhibit) return;

            _lastShownExhibit = current;

            if (current != null)
            {
                if (hintText != null) hintText.text = string.Format(hintTemplate, current.Title);
                interactionHint.SetActive(true);
            }
            else
            {
                interactionHint.SetActive(false);
            }
        }
    }
}