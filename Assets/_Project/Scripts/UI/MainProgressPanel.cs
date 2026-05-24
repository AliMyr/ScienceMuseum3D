using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using ScienceMuseum.Core;
using ScienceMuseum.Managers;
using ScienceMuseum.Player;

namespace ScienceMuseum.UI
{
    /// <summary>
    /// Главная панель прогресса — оверлей на Tab.
    /// Сканирует все IExhibit в сцене, создаёт карточки, телепортирует к выбранному.
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

        private readonly List<ExhibitCard> _cards = new List<ExhibitCard>();
        private int _totalChallenges;
        private bool _isOpen;

        private void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);

            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (menuButton != null) menuButton.onClick.AddListener(GoToMainMenu);

            if (playerTransform == null && firstPersonController != null)
            {
                playerTransform = firstPersonController.transform;
            }
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
            foreach (var card in _cards)
            {
                if (card != null) Destroy(card.gameObject);
            }
            _cards.Clear();
            _totalChallenges = 0;

            if (cardsGrid == null || cardPrefab == null) return;

            var mbs = FindObjectsOfType<MonoBehaviour>(true);
            foreach (var mb in mbs)
            {
                if (mb is IExhibit exhibit)
                {
                    ExhibitCard card = Instantiate(cardPrefab, cardsGrid);
                    card.Bind(exhibit, OnCardGoClicked);
                    _cards.Add(card);
                    _totalChallenges += exhibit.Challenges?.Length ?? 0;
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
            foreach (var card in _cards)
            {
                if (card != null) card.Refresh();
            }

            UpdateStats();
        }

        private void UpdateStats()
        {
            if (statsText == null || ProgressManager.Instance == null) return;

            int studiedExhibits = ProgressManager.Instance.StudiedExhibitsCount;
            int completedChallenges = ProgressManager.Instance.CompletedChallengesCount;

            statsText.text =
                $"Изучено экспонатов: {studiedExhibits} / {_cards.Count}    " +
                $"Выполнено заданий: {completedChallenges} / {_totalChallenges}";
        }

        private void OnCardGoClicked(IExhibit exhibit)
        {
            if (exhibit == null || exhibit.ViewPoint == null || playerTransform == null)
            {
                Close();
                return;
            }

            // CharacterController блокирует прямое изменение position
            var characterController = playerTransform.GetComponent<CharacterController>();
            if (characterController != null) characterController.enabled = false;

            playerTransform.position = exhibit.ViewPoint.position;
            playerTransform.rotation = exhibit.ViewPoint.rotation;

            var fpsController = playerTransform.GetComponent<FirstPersonController>();
            if (fpsController != null) fpsController.ResetVerticalRotation();

            if (characterController != null) characterController.enabled = true;

            Close();
        }

        private void GoToMainMenu() => SceneManager.LoadScene(menuSceneName);
    }
}