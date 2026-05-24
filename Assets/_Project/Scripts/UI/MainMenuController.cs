using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ScienceMuseum.UI
{
    /// <summary>
    /// Главное меню: начать игру, сбросить прогресс (с подтверждением),
    /// показать «о проекте», выйти.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Имя игровой сцены")]
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
            if (aboutPanel != null) aboutPanel.SetActive(false);
            if (aboutDimmer != null) aboutDimmer.SetActive(false);
            if (confirmResetPanel != null) confirmResetPanel.SetActive(false);

            if (startButton != null) startButton.onClick.AddListener(StartGame);
            if (resetButton != null) resetButton.onClick.AddListener(ShowResetConfirm);
            if (aboutButton != null) aboutButton.onClick.AddListener(ShowAbout);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);

            if (aboutCloseButton != null) aboutCloseButton.onClick.AddListener(HideAbout);
            if (confirmYesButton != null) confirmYesButton.onClick.AddListener(ConfirmResetProgress);
            if (confirmNoButton != null) confirmNoButton.onClick.AddListener(HideResetConfirm);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;

            if (aboutPanel != null && aboutPanel.activeSelf) HideAbout();
            else if (confirmResetPanel != null && confirmResetPanel.activeSelf) HideResetConfirm();
        }

        private void StartGame() => SceneManager.LoadScene(gameSceneName);

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
            PlayerPrefs.DeleteKey(keyChallenges);
            PlayerPrefs.DeleteKey(keyExhibits);
            PlayerPrefs.Save();

            HideResetConfirm();
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}