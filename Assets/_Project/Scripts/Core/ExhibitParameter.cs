using System;

namespace ScienceMuseum.Core
{
    
    public class ExhibitParameter
    {
        public string Label { get; }

        public string Unit { get; }

        public float MinValue { get; }
        public float MaxValue { get; }

        public int Decimals { get; }

        public Func<float> Getter { get; }

        public Action<float> Setter { get; }

        public ExhibitParameter(
            string label,
            string unit,
            float minValue,
            float maxValue,
            Func<float> getter,
            Action<float> setter,
            int decimals = 2)
        {
            Label = label;
            Unit = unit;
            MinValue = minValue;
            MaxValue = maxValue;
            Decimals = decimals;
            Getter = getter;
            Setter = setter;
        }

        public string FormatLabel()
        {
            string format = $"F{Decimals}";
            return $"{Label} = {Getter().ToString(format)} {Unit}";
        }
    }
}