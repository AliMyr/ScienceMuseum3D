using UnityEngine;

namespace ScienceMuseum.Core
{
    public abstract class ExhibitBase : MonoBehaviour, IExhibit
    {
        [Header("Информация об экспонате")]
        [Tooltip("Название - показывается в подсказке и на стенде")]
        [SerializeField] protected string title = "Экспонат";

        [Tooltip("Описание - показывается на табличке рядом")]
        [TextArea(3, 6)]
        [SerializeField] protected string description = "Описание будет здесь";

        [Header("Подсветка")]
        [Tooltip("Renderer объекта, у которого менять материал при подсветке")]
        [SerializeField] protected Renderer highlightRenderer;

        [Tooltip("Цвет подсветки")]
        [SerializeField] protected Color highlightColor = new Color(1f, 0.8f, 0.2f);

        [Tooltip("Интенсивность свечения (0 - нет, больше - ярче)")]
        [SerializeField] protected float highlightIntensity = 2f;

        private Material[] _originalMaterials;
        private Material[] _highlightedMaterials;
        private bool _isHighlighted;

        public string Title => title;

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

        public abstract void OnActivate();

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