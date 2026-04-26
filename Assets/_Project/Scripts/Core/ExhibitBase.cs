using UnityEngine;

namespace ScienceMuseum.Core
{
    /// <summary>
    /// Базовый класс для всех экспонатов музея.
    /// Реализует общую логику: подсветка при наведении, хранение метаданных,
    /// пустые реализации параметров и заданий (наследники переопределяют).
    /// </summary>
    public abstract class ExhibitBase : MonoBehaviour, IExhibit
    {
        [Header("Идентификация")]
        [Tooltip("Уникальный ID экспоната. Используется для сохранения прогресса.")]
        [SerializeField] protected string exhibitId = "unnamed_exhibit";

        [Header("Информация об экспонате")]
        [Tooltip("Название - показывается в подсказке и на стенде")]
        [SerializeField] protected string title = "Экспонат";

        [Tooltip("Описание - показывается в панели изучения")]
        [TextArea(3, 6)]
        [SerializeField] protected string description = "Описание будет здесь";

        [Header("Подсветка")]
        [Tooltip("Renderer объекта, у которого менять материал при подсветке")]
        [SerializeField] protected Renderer highlightRenderer;

        [Tooltip("Цвет подсветки")]
        [SerializeField] protected Color highlightColor = new Color(1f, 0.8f, 0.2f);

        [Tooltip("Интенсивность свечения (0 - нет, больше - ярче)")]
        [SerializeField] protected float highlightIntensity = 2f;

        [Header("Метаданные для главной панели")]
        [Tooltip("Тема, например 'Механические колебания'")]
        [SerializeField] protected string topic = "Тема не задана";

        [Tooltip("Школьный класс, например '9 класс'")]
        [SerializeField] protected string grade = "Класс";

        [Tooltip("Точка телепортации игрока к этому экспонату. Создай пустой " +
                 "GameObject рядом с пьедесталом и назначь его сюда.")]
        [SerializeField] protected Transform viewPoint;

        // Оригинальные материалы (чтобы восстановить при OnFocusExit)
        private Material[] _originalMaterials;
        private Material[] _highlightedMaterials;
        private bool _isHighlighted;
        public virtual string Topic => topic;
        public virtual string Grade => grade;
        public Transform ViewPoint => viewPoint;

        // ── Реализация IExhibit ─────────────────────────────────────────

        public string ExhibitId => exhibitId;
        public string Title => title;
        public virtual string Description => description;

        // По умолчанию у экспоната нет параметров и заданий.
        // Конкретные экспонаты (PendulumExhibit, SpringExhibit) переопределяют.
        public virtual ExhibitParameter[] Parameters => null;
        public virtual IChallenge[] Challenges => null;

        public virtual string GetFormulaText() => string.Empty;
        public virtual void ResetSimulation() { }

        public abstract void OnActivate();

        // ── Подсветка ───────────────────────────────────────────────────

        protected virtual void Awake()
        {
            if (highlightRenderer == null)
            {
                highlightRenderer = GetComponent<Renderer>();
            }

            if (highlightRenderer != null)
            {
                _originalMaterials = highlightRenderer.sharedMaterials;
                _highlightedMaterials = new Material[_originalMaterials.Length];

                for (int i = 0; i < _originalMaterials.Length; i++)
                {
                    _highlightedMaterials[i] = new Material(_originalMaterials[i]);
                    _highlightedMaterials[i].EnableKeyword("_EMISSION");
                    _highlightedMaterials[i].SetColor(
                        "_EmissionColor",
                        highlightColor * highlightIntensity
                    );
                }
            }
        }

        public virtual void OnFocusEnter()
        {
            if (_isHighlighted) return;
            _isHighlighted = true;

            if (highlightRenderer != null && _highlightedMaterials != null)
            {
                highlightRenderer.materials = _highlightedMaterials;
            }
        }

        public virtual void OnFocusExit()
        {
            if (!_isHighlighted) return;
            _isHighlighted = false;

            if (highlightRenderer != null && _originalMaterials != null)
            {
                highlightRenderer.materials = _originalMaterials;
            }
        }

        private void OnDestroy()
        {
            if (_highlightedMaterials != null)
            {
                foreach (var mat in _highlightedMaterials)
                {
                    if (mat != null) Destroy(mat);
                }
            }
        }
    }
}