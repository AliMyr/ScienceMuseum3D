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
        private SpringExhibit _currentSpring;
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
            if (exhibit == null) return;

            _currentExhibit = exhibit;
            _currentSpring = null;      // сбросить предыдущий spring-контекст
            _isOpen = true;

            RebuildChallenges(exhibit.Challenges);

            if (titleText != null) titleText.text = exhibit.Title;
            if (descriptionText != null) descriptionText.text = exhibit.Description;

            ConfigureSliderForPendulum();

            if (panelRoot != null) panelRoot.SetActive(true);
            if (hudRoot != null) hudRoot.SetActive(false);
            if (firstPersonController != null) firstPersonController.enabled = false;
            if (exhibitInteractor != null) exhibitInteractor.enabled = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void ConfigureSliderForPendulum()
        {
            if (_currentExhibit == null) return;

            if (lengthSlider != null)
            {
                lengthSlider.minValue = 0.3f;
                lengthSlider.maxValue = 2.5f;
                lengthSlider.SetValueWithoutNotify(_currentExhibit.Length);
            }
            if (gravitySlider != null)
            {
                gravitySlider.minValue = 1f;
                gravitySlider.maxValue = 25f;
                gravitySlider.SetValueWithoutNotify(_currentExhibit.Gravity);
            }
            if (dampingSlider != null)
            {
                dampingSlider.minValue = 0f;
                dampingSlider.maxValue = 2f;
                dampingSlider.SetValueWithoutNotify(_currentExhibit.Damping);
            }
            if (angleSlider != null)
            {
                angleSlider.minValue = -170f;
                angleSlider.maxValue = 170f;
                angleSlider.SetValueWithoutNotify(_currentExhibit.InitialAngleDegrees);
            }

            UpdateLabels();
            UpdateFormulas();
        }

        // ── Открытие для пружины ─────────────────────────────────────────────

        public void OpenForSpring(Exhibits.SpringExhibit exhibit)
        {
            if (exhibit == null) return;

            _currentExhibit = null;  // не маятник
            _currentSpring = exhibit;
            _isOpen = true;

            RebuildChallenges(exhibit.Challenges);

            if (titleText != null) titleText.text = exhibit.Title;
            if (descriptionText != null) descriptionText.text = exhibit.Description;

            // Переназначаем слайдеры под параметры пружины
            ConfigureSliderForSpring();

            // Переключаем режимы
            if (panelRoot != null) panelRoot.SetActive(true);
            if (hudRoot != null) hudRoot.SetActive(false);
            if (firstPersonController != null) firstPersonController.enabled = false;
            if (exhibitInteractor != null) exhibitInteractor.enabled = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void ConfigureSliderForSpring()
        {
            if (_currentSpring == null) return;

            if (lengthSlider != null)
            {
                lengthSlider.minValue = 0.1f;
                lengthSlider.maxValue = 10f;
                lengthSlider.SetValueWithoutNotify(_currentSpring.Mass);
            }
            if (gravitySlider != null)
            {
                gravitySlider.minValue = 5f;
                gravitySlider.maxValue = 200f;
                gravitySlider.SetValueWithoutNotify(_currentSpring.Stiffness);
            }
            if (dampingSlider != null)
            {
                dampingSlider.minValue = 0f;
                dampingSlider.maxValue = 5f;
                dampingSlider.SetValueWithoutNotify(_currentSpring.Damping);
            }
            if (angleSlider != null)
            {
                angleSlider.minValue = -0.5f;
                angleSlider.maxValue = 0.5f;
                angleSlider.SetValueWithoutNotify(_currentSpring.InitialDisplacement);
            }

            UpdateLabels();
            UpdateFormulas();
        }

        /// <summary>
        /// Закрыть панель и вернуть обычный режим FPS.
        /// </summary>
        public void Close()
        {
            _isOpen = false;
            _currentExhibit = null;
            _currentSpring = null;

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
                _currentExhibit.Length = value;
            else if (_currentSpring != null)
                _currentSpring.Mass = value;

            UpdateLabels();
            UpdateFormulas();
        }

        private void OnGravityChanged(float value)
        {
            if (_currentExhibit != null)
                _currentExhibit.Gravity = value;
            else if (_currentSpring != null)
                _currentSpring.Stiffness = value;

            UpdateLabels();
            UpdateFormulas();
        }

        private void OnDampingChanged(float value)
        {
            if (_currentExhibit != null)
                _currentExhibit.Damping = value;
            else if (_currentSpring != null)
                _currentSpring.Damping = value;

            UpdateLabels();
        }

        private void OnAngleChanged(float value)
        {
            if (_currentExhibit != null)
                _currentExhibit.InitialAngleDegrees = value;
            else if (_currentSpring != null)
                _currentSpring.InitialDisplacement = value;

            UpdateLabels();
        }

        private void OnResetClicked()
        {
            if (_currentExhibit != null)
                _currentExhibit.ResetSimulation();
            else if (_currentSpring != null)
                _currentSpring.ResetSimulation();
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
            if (_currentExhibit != null)
            {
                if (lengthSlider != null && lengthLabel != null)
                    lengthLabel.text = $"Длина нити L = {lengthSlider.value:F2} м";
                if (gravitySlider != null && gravityLabel != null)
                    gravityLabel.text = $"Гравитация g = {gravitySlider.value:F2} м/с²";
                if (dampingSlider != null && dampingLabel != null)
                    dampingLabel.text = $"Трение k = {dampingSlider.value:F2}";
                if (angleSlider != null && angleLabel != null)
                    angleLabel.text = $"Начальный угол θ = {angleSlider.value:F0}°";
            }
            else if (_currentSpring != null)
            {
                if (lengthSlider != null && lengthLabel != null)
                    lengthLabel.text = $"Масса m = {lengthSlider.value:F2} кг";
                if (gravitySlider != null && gravityLabel != null)
                    gravityLabel.text = $"Жёсткость k = {gravitySlider.value:F1} Н/м";
                if (dampingSlider != null && dampingLabel != null)
                    dampingLabel.text = $"Трение c = {dampingSlider.value:F2}";
                if (angleSlider != null && angleLabel != null)
                    angleLabel.text = $"Начальное смещение x₀ = {angleSlider.value:F2} м";
            }
        }

        private void UpdateFormulas()
        {
            if (formulaText == null) return;

            if (_currentExhibit != null)
            {
                float L = lengthSlider.value;
                float g = gravitySlider.value;
                float period = 2f * Mathf.PI * Mathf.Sqrt(L / g);
                float frequency = 1f / period;

                formulaText.text =
                    "<b>Формула периода малых колебаний:</b>\n" +
                    $"  T = 2π·√(L/g) = 2π·√({L:F2}/{g:F2})\n" +
                    $"  T = <color=#FFD700>{period:F3} с</color>\n\n" +
                    $"<b>Частота:</b>  f = 1/T = <color=#FFD700>{frequency:F3} Гц</color>\n\n" +
                    "<i>Формула работает для малых углов (до ~15°).</i>";
            }
            else if (_currentSpring != null)
            {
                float m = lengthSlider.value;
                float k = gravitySlider.value;
                float period = 2f * Mathf.PI * Mathf.Sqrt(m / k);
                float frequency = 1f / period;
                float omega = Mathf.Sqrt(k / m);

                formulaText.text =
                    "<b>Формула периода:</b>\n" +
                    $"  T = 2π·√(m/k) = 2π·√({m:F2}/{k:F1})\n" +
                    $"  T = <color=#FFD700>{period:F3} с</color>\n\n" +
                    $"<b>Частота:</b>  f = <color=#FFD700>{frequency:F3} Гц</color>\n" +
                    $"<b>Угловая частота:</b>  ω = √(k/m) = {omega:F3} рад/с\n\n" +
                    "<i>Период не зависит от амплитуды — это свойство\n" +
                    "линейного осциллятора.</i>";
            }
        }
    }
}