using UnityEngine;
using TMPro;
using ScienceMuseum.Core;
using ScienceMuseum.Player;

namespace ScienceMuseum.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Ссылки на UI")]
        [Tooltip("Контейнер подсказки (GameObject с фоном и текстом)")]
        [SerializeField] private GameObject interactionHint;

        [Tooltip("Текстовое поле подсказки")]
        [SerializeField] private TextMeshProUGUI hintText;

        [Header("Ссылки на логику")]
        [Tooltip("Компонент взаимодействия с экспонатами")]
        [SerializeField] private ExhibitInteractor interactor;

        [Header("Настройки")]
        [Tooltip("Шаблон подсказки. {0} заменяется на название экспоната")]
        [SerializeField] private string hintTemplate = "Нажми [E] чтобы изучить: {0}";

        private IExhibit _lastShownExhibit;

        private void Awake()
        {
            if (interactor == null)
            {
                interactor = FindObjectOfType<ExhibitInteractor>();
            }

            if (interactionHint != null)
            {
                interactionHint.SetActive(false);
            }
        }

        private void Update()
        {
            if (interactor == null || interactionHint == null) return;

            IExhibit current = interactor.CurrentExhibit;

            if (current != _lastShownExhibit)
            {
                _lastShownExhibit = current;

                if (current != null)
                {
                    if (hintText != null)
                    {
                        hintText.text = string.Format(hintTemplate, current.Title);
                    }
                    interactionHint.SetActive(true);
                }
                else
                {
                    interactionHint.SetActive(false);
                }
            }
        }
    }
}