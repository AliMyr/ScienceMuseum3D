using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ScienceMuseum.Core;

namespace ScienceMuseum.UI
{
    public class ParameterSliderRow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Slider slider;

        private ExhibitParameter _parameter;

        public void Bind(ExhibitParameter parameter)
        {
            _parameter = parameter;

            if (slider != null)
            {
                slider.minValue = parameter.MinValue;
                slider.maxValue = parameter.MaxValue;
                slider.SetValueWithoutNotify(parameter.Getter());

                // Удаляем старые слушатели если префаб переиспользуется
                slider.onValueChanged.RemoveAllListeners();
                slider.onValueChanged.AddListener(OnSliderChanged);
            }

            UpdateLabel();
        }

        private void OnSliderChanged(float value)
        {
            _parameter?.Setter(value);
            UpdateLabel();
        }

        public void UpdateLabel()
        {
            if (_parameter != null && label != null)
            {
                label.text = _parameter.FormatLabel();
            }
        }
    }
}