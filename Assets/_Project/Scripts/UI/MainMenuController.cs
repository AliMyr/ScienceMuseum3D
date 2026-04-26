using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ScienceMuseum.UI
{
    /// <summary>
    /// Контроллер главного меню.
    /// Управляет переходом в игровую сцену, сбросом прогресса,
    /// выходом из приложения, показом информации о проекте.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Имя игровой сцены")]
        [Tooltip("Имя сцены которая загружается при нажатии 'Начать'")]
        [SerializeField] private string gameSceneName = "MainHall";

        [Header("Главные кнопки меню")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button aboutButton;
        [SerializeField] private Button quitButton;

        [Header("Панель «О проекте»")]
        [SerializeField] private GameObject aboutPanel;
        [SerializeField] private GameObject aboutDimmer;
        [SerializeField] private Button aboutCloseButton;

        [Header("Панель подтверждения сброса")]
        [SerializeField] private GameObject confirmResetPanel;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;

        [Header("Ключи PlayerPrefs для сброса")]
        [SerializeField] private string keyChallenges = "progress.completed_challenges";
        [SerializeField] private string keyExhibits = "progress.studied_exhibits";

        private void Awake()
        {
            // Скрываем модальные панели по умолчанию
            if (aboutPanel != null) aboutPanel.SetActive(false);
            if (aboutDimmer != null) aboutDimmer.SetActive(false);
            if (confirmResetPanel != null) confirmResetPanel.SetActive(false);

            // Привязываем кнопки
            if (startButton != null) startButton.onClick.AddListener(StartGame);
            if (resetButton != null) resetButton.onClick.AddListener(ShowResetConfirm);
            if (aboutButton != null) aboutButton.onClick.AddListener(ShowAbout);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);

            if (aboutCloseButton != null) aboutCloseButton.onClick.AddListener(HideAbout);
            if (confirmYesButton != null) confirmYesButton.onClick.AddListener(ConfirmResetProgress);
            if (confirmNoButton != null) confirmNoButton.onClick.AddListener(HideResetConfirm);

            // В меню курсор виден и свободен
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            // Escape закрывает модальные панели
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (aboutPanel != null && aboutPanel.activeSelf) HideAbout();
                else if (confirmResetPanel != null && confirmResetPanel.activeSelf)
                    HideResetConfirm();
            }
        }

        private void StartGame()
        {
            // Проверяем что сцена есть в Build Settings
            SceneManager.LoadScene(gameSceneName);
        }

        private void ShowAbout()
        {
            if (aboutDimmer != null) aboutDimmer.SetActive(true);
            if (aboutPanel != null) aboutPanel.SetActive(true);
        }

        private void HideAbout()
        {
            if (aboutDimmer != null) aboutDimmer.SetActive(false);
            if (aboutPanel != null) aboutPanel.SetActive(false);
        }

        private void ShowResetConfirm()
        {
            if (aboutDimmer != null) aboutDimmer.SetActive(true);
            if (confirmResetPanel != null) confirmResetPanel.SetActive(true);
        }

        private void HideResetConfirm()
        {
            if (aboutDimmer != null) aboutDimmer.SetActive(false);
            if (confirmResetPanel != null) confirmResetPanel.SetActive(false);
        }

        private void ConfirmResetProgress()
        {
            // Удаляем сохранённый прогресс
            PlayerPrefs.DeleteKey(keyChallenges);
            PlayerPrefs.DeleteKey(keyExhibits);
            PlayerPrefs.Save();

            Debug.Log("[Menu] Прогресс сброшен через главное меню");

            HideResetConfirm();
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            // В редакторе - просто остановить Play
            UnityEditor.EditorApplication.isPlaying = false;
#else
            // В билде - закрыть приложение
            Application.Quit();
#endif
        }
    }
}