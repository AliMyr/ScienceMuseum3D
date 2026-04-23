using UnityEngine;
using ScienceMuseum.Core;

namespace ScienceMuseum.Player
{
    public class ExhibitInteractor : MonoBehaviour
    {
        [Header("Параметры взаимодействия")]
        [Tooltip("Максимальное расстояние до экспоната (в метрах)")]
        [SerializeField] private float interactionDistance = 4f;

        [Tooltip("Слой, на котором находятся экспонаты")]
        [SerializeField] private LayerMask exhibitLayer;

        [Tooltip("Клавиша взаимодействия")]
        [SerializeField] private KeyCode interactionKey = KeyCode.E;

        [Header("Ссылки")]
        [Tooltip("Камера игрока (источник луча)")]
        [SerializeField] private Camera playerCamera;

        private IExhibit _currentExhibit;

        private void Awake()
        {
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
            }
        }

        private void Update()
        {
            UpdateFocusedExhibit();
            HandleInteraction();
        }

        private void UpdateFocusedExhibit()
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            IExhibit hitExhibit = null;

            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, exhibitLayer))
            {
                hitExhibit = hit.collider.GetComponentInParent<IExhibit>();
            }

            if (hitExhibit != _currentExhibit)
            {
                _currentExhibit?.OnFocusExit();
                hitExhibit?.OnFocusEnter();
                _currentExhibit = hitExhibit;
            }
        }

        private void HandleInteraction()
        {
            if (_currentExhibit == null) return;

            if (Input.GetKeyDown(interactionKey))
            {
                _currentExhibit.OnActivate();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (playerCamera == null) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(
                playerCamera.transform.position,
                playerCamera.transform.forward * interactionDistance
            );
        }

        public IExhibit CurrentExhibit => _currentExhibit;
    }
}