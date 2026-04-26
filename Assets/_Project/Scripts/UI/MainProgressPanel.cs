using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ScienceMuseum.Core;
using ScienceMuseum.Managers;
using ScienceMuseum.Player;

namespace ScienceMuseum.UI
{
    /// <summary>
    /// Главная панель прогресса - оверлей на Tab.
    /// Сканирует все IExhibit в сцене, создаёт карточки, обрабатывает телепортацию.
    /// </summary>
    public class MainProgressPanel : MonoBehaviour
    {
        [Header("Корень панели")]
        [SerializeField] private GameObject panelRoot;

        [Header("Заголовок и статистика")]
        [SerializeField] private TextMeshProUGUI statsText;

        [Header("Сетка карточек")]
        [SerializeField] private RectTransform cardsGrid;
        [SerializeField] private ExhibitCard cardPrefab;

        [Header("Кнопка закрытия")]
        [SerializeField] private Button closeButton;

        [Header("Игровое состояние")]
        [Tooltip("FPS-контроллер - выключается во время отображения панели")]
        [SerializeField] private MonoBehaviour firstPersonController;

        [Tooltip("Интерактор - тоже выключается")]
        [SerializeField] private MonoBehaviour exhibitInteractor;

        [Tooltip("Контейнер HUD - скрывается на время")]
        [SerializeField] private GameObject hudRoot;

        [Header("Игрок (для телепортации)")]
        [SerializeField] private Transform playerTransform;

        [Header("Клавиша открытия/закрытия")]
        [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

        [Header("Возврат в меню")]
        [SerializeField] private Button menuButton;
        [SerializeField] private string menuSceneName = "MainMenu";

        // Динамически созданные карточки
        private readonly List<ExhibitCard> _cards = new List<ExhibitCard>();
        private bool _isOpen;

        private void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);

            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            if (playerTransform == null && firstPersonController != null)
            {
                playerTransform = firstPersonController.transform;
            }

            if (menuButton != null) menuButton.onClick.AddListener(GoToMainMenu);
        }

        private void Start()
        {
            BuildCards();

            if (ProgressManager.Instance != null)
            {
                ProgressManager.Instance.OnProgressChanged += RefreshAll;
            }

            RefreshAll();
        }

        private void OnDestroy()
        {
            if (ProgressManager.Instance != null)
            {
                ProgressManager.Instance.OnProgressChanged -= RefreshAll;
            }
        }

        private void Update()
        {
            // Tab/Escape переключают
            if (Input.GetKeyDown(toggleKey))
            {
                if (_isOpen) Close();
                else Open();
            }
            else if (_isOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }

        private void BuildCards()
        {
            // Удаляем старые
            foreach (var card in _cards)
            {
                if (card != null) Destroy(card.gameObject);
            }
            _cards.Clear();

            if (cardsGrid == null || cardPrefab == null) return;

            // Сканируем сцену на IExhibit
            var mbs = FindObjectsOfType<MonoBehaviour>(true);
            foreach (var mb in mbs)
            {
                if (mb is IExhibit exhibit)
                {
                    ExhibitCard card = Instantiate(cardPrefab, cardsGrid);
                    card.Bind(exhibit, OnCardGoClicked);
                    _cards.Add(card);
                }
            }
        }

        public void Open()
        {
            if (_isOpen) return;
            _isOpen = true;

            RefreshAll();

            if (panelRoot != null) panelRoot.SetActive(true);
            if (hudRoot != null) hudRoot.SetActive(false);
            if (firstPersonController != null) firstPersonController.enabled = false;
            if (exhibitInteractor != null) exhibitInteractor.enabled = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Close()
        {
            if (!_isOpen) return;
            _isOpen = false;

            if (panelRoot != null) panelRoot.SetActive(false);
            if (hudRoot != null) hudRoot.SetActive(true);
            if (firstPersonController != null) firstPersonController.enabled = true;
            if (exhibitInteractor != null) exhibitInteractor.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void RefreshAll()
        {
            // Обновляем все карточки
            foreach (var card in _cards)
            {
                if (card != null) card.Refresh();
            }

            // Обновляем общую статистику
            UpdateStats();
        }

        private void UpdateStats()
        {
            if (statsText == null || ProgressManager.Instance == null) return;

            int totalExhibits = _cards.Count;
            int totalChallenges = 0;
            int studiedExhibits = ProgressManager.Instance.StudiedExhibitsCount;
            int completedChallenges = ProgressManager.Instance.CompletedChallengesCount;

            // Считаем общее число заданий
            var mbs = FindObjectsOfType<MonoBehaviour>(true);
            foreach (var mb in mbs)
            {
                if (mb is IExhibit exhibit && exhibit.Challenges != null)
                {
                    totalChallenges += exhibit.Challenges.Length;
                }
            }

            statsText.text =
                $"Изучено экспонатов: {studiedExhibits} / {totalExhibits}    " +
                $"Выполнено заданий: {completedChallenges} / {totalChallenges}";
        }

        /// <summary>
        /// Обработчик клика "Перейти" - телепортирует игрока к экспонату.
        /// </summary>
        private void OnCardGoClicked(IExhibit exhibit)
        {
            if (exhibit == null || exhibit.ViewPoint == null || playerTransform == null)
            {
                Debug.LogWarning("[MainPanel] Не задана точка обзора у экспоната или игрока");
                Close();
                return;
            }

            // CharacterController блокирует прямое изменение position.
            // Временно отключаем его на момент телепортации.
            var characterController = playerTransform.GetComponent<CharacterController>();
            if (characterController != null) characterController.enabled = false;

            playerTransform.position = exhibit.ViewPoint.position;
            playerTransform.rotation = exhibit.ViewPoint.rotation;

            var fpsController = playerTransform.GetComponent<ScienceMuseum.Player.FirstPersonController>();
            if (fpsController != null) fpsController.ResetVerticalRotation();

            if (characterController != null) characterController.enabled = true;

            Close();
        }

        private void GoToMainMenu()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(menuSceneName);
        }
    }
}