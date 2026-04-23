using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ScienceMuseum.Core;

namespace ScienceMuseum.UI
{
    public class ChallengeCard : MonoBehaviour
    {
        [SerializeField] private Image statusIcon;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI progressText;

        [Header("Цвета статусов")]
        [SerializeField] private Color colorNotStarted = new Color(0.4f, 0.4f, 0.4f);
        [SerializeField] private Color colorInProgress = new Color(1f, 0.7f, 0.2f);
        [SerializeField] private Color colorCompleted = new Color(0.3f, 0.9f, 0.3f);

        private IChallenge _challenge;

        public void Bind(IChallenge challenge)
        {
            _challenge = challenge;
            if (titleText != null) titleText.text = challenge.Title;
            if (descriptionText != null) descriptionText.text = challenge.Description;
            Refresh();
        }

        public void Refresh()
        {
            if (_challenge == null) return;

            if (progressText != null)
                progressText.text = _challenge.GetProgressText();

            if (statusIcon != null)
            {
                statusIcon.color = _challenge.Status switch
                {
                    ChallengeStatus.Completed => colorCompleted,
                    ChallengeStatus.InProgress => colorInProgress,
                    _ => colorNotStarted
                };
            }
        }
    }
}