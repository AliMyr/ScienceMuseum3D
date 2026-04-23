using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ScienceMuseum.Exhibits;
using ScienceMuseum.Core;
using System.Collections.Generic;

namespace ScienceMuseum.UI
{
    /// <summary>
    /// Универсальная панель изучения экспоната.
    /// Сейчас настроена под маятник, но общая структура позволяет
    /// легко адаптировать под другие экспонаты (Лоренц, гармонический осциллятор).
    ///
    /// Открывается по OnActivate экспоната, закрывается по Escape или кнопке.
    /// Во время открытия блокирует движение игрока и освобождает курсор.
    /// </summary>
    public class ExhibitStudyPanel : MonoBehaviour
    {
        [Header("Ссылки на UI")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI formulaText;

        [Header("Слайдеры")]
        [SerializeField] private Slider lengthSlider;
        [SerializeField] private TextMeshProUGUI lengthLabel;
        [SerializeField] private Slider gravitySlider;
        [SerializeField] private TextMeshProUGUI gravityLabel;
        [SerializeField] private Slider dampingSlider;
        [SerializeField] private TextMeshProUGUI dampingLabel;
        [SerializeField] private Slider angleSlider;
        [SerializeField] private TextMeshProUGUI angleLabel;

        [Header("Кнопки")]
        [SerializeField] private Button resetButton;
        [SerializeField] private Button closeButton;

        [Header("Игровое состояние")]
        [Tooltip("Контроллер игрока - выключается во время изучения")]
        [SerializeField] private MonoBehaviour firstPersonController;

        [Tooltip("Интерактор - выключается во время изучения")]
        [SerializeField] private MonoBehaviour exhibitInteractor;

        [Tooltip("HUD с прицелом и подсказкой - скрывается во время изучения")]
        [SerializeField] private GameObject hudRoot;

        // Текущий изучаемый экспонат
        private PendulumExhibit _currentExhibit;
        private bool _isOpen;

        [Header("Задания")]
        [Tooltip("Контейнер, куда инстанциируются карточки заданий")]
        [SerializeField] private RectTransform challengesListRoot;

        [Tooltip("Префаб одной карточки задания")]
        [SerializeField] private ChallengeCard challengeCardPrefab;

        // Существующие карточки (чтобы при смене экспоната пересоздавать)
        private readonly List<ChallengeCard> _cards = new List<ChallengeCard>();

        private void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);

            // Подписываемся на события слайдеров (один раз, в Awake)
            if (lengthSlider != null)
                lengthSlider.onValueChanged.AddListener(OnLengthChanged);
            if (gravitySlider != null)
                gravitySlider.onValueChanged.AddListener(OnGravityChanged);
            if (dampingSlider != null)
                dampingSlider.onValueChanged.AddListener(OnDampingChanged);
            if (angleSlider != null)
                angleSlider.onValueChanged.AddListener(OnAngleChanged);

            if (resetButton != null)
                resetButton.onClick.AddListener(OnResetClicked);
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);
        }

        private void Update()
        {
            // Выход по Escape
            if (_isOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }

            // Обновляем карточки
            if (_isOpen)
            {
                foreach (var card in _cards)
                {
                    if (card != null) card.Refresh();
                }
            }
        }

        /// <summary>
        /// Открыть панель изучения для указанного экспоната.
        /// </summary>
        public void Open(PendulumExhibit exhibit)
        {
            RebuildChallenges(exhibit.Challenges);

            if (exhibit == null) return;

            _currentExhibit = exhibit;
            _isOpen = true;

            // Заполняем заголовок и описание
            if (titleText != null) titleText.text = exhibit.Title;
            if (descriptionText != null) descriptionText.text = exhibit.Description;

            // Читаем текущие параметры экспоната в слайдеры
            if (lengthSlider != null) lengthSlider.SetValueWithoutNotify(exhibit.Length);
            if (gravitySlider != null) gravitySlider.SetValueWithoutNotify(exhibit.Gravity);
            if (dampingSlider != null) dampingSlider.SetValueWithoutNotify(exhibit.Damping);
            if (angleSlider != null) angleSlider.SetValueWithoutNotify(exhibit.InitialAngleDegrees);

            // Обновляем подписи под слайдерами
            UpdateLabels();
            UpdateFormulas();

            // Переключаем режимы
            if (panelRoot != null) panelRoot.SetActive(true);
            if (hudRoot != null) hudRoot.SetActive(false);
            if (firstPersonController != null) firstPersonController.enabled = false;
            if (exhibitInteractor != null) exhibitInteractor.enabled = false;

            // Освобождаем курсор
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>
        /// Закрыть панель и вернуть обычный режим FPS.
        /// </summary>
        public void Close()
        {
            _isOpen = false;
            _currentExhibit = null;

            if (panelRoot != null) panelRoot.SetActive(false);
            if (hudRoot != null) hudRoot.SetActive(true);
            if (firstPersonController != null) firstPersonController.enabled = true;
            if (exhibitInteractor != null) exhibitInteractor.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // ── Обработчики слайдеров ─────────────────────────────────────────

        private void OnLengthChanged(float value)
        {
            if (_currentExhibit != null)
            {
                _currentExhibit.Length = value;
            }
            UpdateLabels();
            UpdateFormulas();
        }

        private void OnGravityChanged(float value)
        {
            if (_currentExhibit != null)
            {
                _currentExhibit.Gravity = value;
            }
            UpdateLabels();
            UpdateFormulas();
        }

        private void OnDampingChanged(float value)
        {
            if (_currentExhibit != null)
            {
                _currentExhibit.Damping = value;
            }
            UpdateLabels();
        }

        private void OnAngleChanged(float value)
        {
            if (_currentExhibit != null)
            {
                _currentExhibit.InitialAngleDegrees = value;
            }
            UpdateLabels();
        }

        private void OnResetClicked()
        {
            if (_currentExhibit != null)
            {
                _currentExhibit.ResetSimulation();
            }
        }

        private void RebuildChallenges(IChallenge[] challenges)
        {
            // Удаляем старые карточки если были
            foreach (var card in _cards)
            {
                if (card != null) Destroy(card.gameObject);
            }
            _cards.Clear();

            if (challenges == null || challengesListRoot == null || challengeCardPrefab == null)
                return;

            // Создаём по карточке на каждое задание
            foreach (var challenge in challenges)
            {
                ChallengeCard card = Instantiate(challengeCardPrefab, challengesListRoot);
                card.Bind(challenge);
                _cards.Add(card);
            }
        }

        // ── Обновление UI ─────────────────────────────────────────────────

        private void UpdateLabels()
        {
            if (lengthSlider != null && lengthLabel != null)
                lengthLabel.text = $"Длина нити L = {lengthSlider.value:F2} м";

            if (gravitySlider != null && gravityLabel != null)
                gravityLabel.text = $"Гравитация g = {gravitySlider.value:F2} м/с²";

            if (dampingSlider != null && dampingLabel != null)
                dampingLabel.text = $"Трение k = {dampingSlider.value:F2}";

            if (angleSlider != null && angleLabel != null)
                angleLabel.text = $"Начальный угол θ₀ = {angleSlider.value:F0}°";
        }

        private void UpdateFormulas()
        {
            if (formulaText == null || lengthSlider == null || gravitySlider == null) return;

            float L = lengthSlider.value;
            float g = gravitySlider.value;
            float period = 2f * Mathf.PI * Mathf.Sqrt(L / g);
            float frequency = 1f / period;

            // Собираем информативный текст с формулами
            formulaText.text =
                "<b>Формула периода малых колебаний:</b>\n" +
                $"  T = 2π·√(L/g) = 2π·√({L:F2}/{g:F2})\n" +
                $"  T = <color=#FFD700>{period:F3} с</color>\n" +
                "\n" +
                $"<b>Частота:</b>  f = 1/T = <color=#FFD700>{frequency:F3} Гц</color>\n" +
                "\n" +
                "<i>Формула работает для малых углов (до ~15°). " +
                "При больших углах реальный период больше.</i>";
        }
    }
}