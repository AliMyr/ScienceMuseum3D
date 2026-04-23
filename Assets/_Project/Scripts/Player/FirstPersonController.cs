using UnityEngine;

namespace ScienceMuseum.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Движение")]
        [Tooltip("Скорость обычной ходьбы (м/с)")]
        [SerializeField] private float walkSpeed = 4f;

        [Tooltip("Скорость бега с зажатым Shift (м/с)")]
        [SerializeField] private float runSpeed = 7f;

        [Tooltip("Сила прыжка (начальная вертикальная скорость, м/с)")]
        [SerializeField] private float jumpForce = 5f;

        [Tooltip("Ускорение свободного падения (м/с²)")]
        [SerializeField] private float gravity = 20f;

        [Header("Обзор мышью")]
        [Tooltip("Чувствительность мыши по X (поворот тела)")]
        [SerializeField] private float mouseSensitivityX = 2f;

        [Tooltip("Чувствительность мыши по Y (наклон головы)")]
        [SerializeField] private float mouseSensitivityY = 2f;

        [Tooltip("Максимальный угол наклона головы вверх/вниз (в градусах)")]
        [SerializeField] private float maxLookAngle = 85f;

        [Header("Ссылки")]
        [Tooltip("Камера игрока - обычно дочерний объект")]
        [SerializeField] private Camera playerCamera;

        private CharacterController _controller;
        private Vector3 _velocity;
        private float _verticalRotation;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();

            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
            }
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            HandleMouseLook();
            HandleMovement();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY;

            transform.Rotate(Vector3.up * mouseX);

            _verticalRotation -= mouseY;
            _verticalRotation = Mathf.Clamp(_verticalRotation, -maxLookAngle, maxLookAngle);

            if (playerCamera != null)
            {
                playerCamera.transform.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
            }
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

            Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;

            if (moveDirection.magnitude > 1f)
            {
                moveDirection.Normalize();
            }

            Vector3 horizontalMove = moveDirection * currentSpeed;

            if (_controller.isGrounded)
            {
                if (_velocity.y < 0)
                {
                    _velocity.y = -2f;
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    _velocity.y = jumpForce;
                }
            }
            else
            {
                _velocity.y -= gravity * Time.deltaTime;
            }

            Vector3 finalMove = horizontalMove + new Vector3(0, _velocity.y, 0);

            _controller.Move(finalMove * Time.deltaTime);
        }
    }
}