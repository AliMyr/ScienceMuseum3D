using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ScienceMuseum.Core;

namespace ScienceMuseum.UI
{
    public class ExhibitStudyPanel : MonoBehaviour
    {
        [Header("Ссылки на UI")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI formulaText;

        [Header("Контейнеры и префабы")]
        [SerializeField] private RectTransform parametersContainer;
        [SerializeField] private ParameterSliderRow parameterRowPrefab;
        [SerializeField] private RectTransform challengesListRoot;
        [SerializeField] private ChallengeCard challengeCardPrefab;

        [Header("Кнопки")]
        [SerializeField] private Button resetButton;
        [SerializeField] private Button closeButton;

        [Header("Игровое состояние")]
        [SerializeField] private MonoBehaviour firstPersonController;
        [SerializeField] private MonoBehaviour exhibitInteractor;
        [SerializeField] private GameObject hudRoot;

        // Текущий экспонат
        private IExhibit _currentExhibit;
        private bool _isOpen;

        // Динамически созданные UI элементы
        private readonly List<ParameterSliderRow> _sliderRows = new List<ParameterSliderRow>();
        private readonly List<ChallengeCard> _challengeCards = new List<ChallengeCard>();

        private void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);

            if (resetButton != null)
                resetButton.onClick.AddListener(OnResetClicked);
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);
        }

        private void Update()
        {
            if (_isOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }

            if (_isOpen)
            {
                if (formulaText != null && _currentExhibit != null)
                {
                    formulaText.text = _currentExhibit.GetFormulaText();
                }

                foreach (var row in _sliderRows)
                {
                    if (row != null) row.UpdateLabel();
                }

                foreach (var card in _challengeCards)
                {
                    if (card != null) card.Refresh();
                }
            }
        }

        public void Open(IExhibit exhibit)
        {
            if (exhibit == null) return;

            _currentExhibit = exhibit;
            _isOpen = true;

            if (titleText != null) titleText.text = exhibit.Title;
            if (descriptionText != null) descriptionText.text = exhibit.Description;
            if (formulaText != null) formulaText.text = exhibit.GetFormulaText();

            BuildParameters(exhibit.Parameters);
            BuildChallenges(exhibit.Challenges);

            if (panelRoot != null) panelRoot.SetActive(true);
            if (hudRoot != null) hudRoot.SetActive(false);
            if (firstPersonController != null) firstPersonController.enabled = false;
            if (exhibitInteractor != null) exhibitInteractor.enabled = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

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

        // ── Динамическое построение UI ────────────────────────────────────

        private void BuildParameters(ExhibitParameter[] parameters)
        {
            foreach (var row in _sliderRows)
            {
                if (row != null) Destroy(row.gameObject);
            }
            _sliderRows.Clear();

            if (parameters == null || parametersContainer == null || parameterRowPrefab == null)
                return;

            foreach (var param in parameters)
            {
                ParameterSliderRow row = Instantiate(parameterRowPrefab, parametersContainer);
                row.Bind(param);
                _sliderRows.Add(row);
            }
        }

        private void BuildChallenges(IChallenge[] challenges)
        {
            foreach (var card in _challengeCards)
            {
                if (card != null) Destroy(card.gameObject);
            }
            _challengeCards.Clear();

            if (challenges == null || challengesListRoot == null || challengeCardPrefab == null)
                return;

            foreach (var challenge in challenges)
            {
                ChallengeCard card = Instantiate(challengeCardPrefab, challengesListRoot);
                card.Bind(challenge);
                _challengeCards.Add(card);
            }
        }

        private void OnResetClicked()
        {
            _currentExhibit?.ResetSimulation();
        }
    }
}