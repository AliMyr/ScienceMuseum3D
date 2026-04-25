using UnityEngine;
using ScienceMuseum.Core;
using ScienceMuseum.Exhibits;

namespace ScienceMuseum.Simulation.Challenges
{
    /// <summary>
    /// Подобрать период колебаний груза на пружине.
    /// </summary>
    public class SpringTargetPeriodChallenge : CheckedChallengeBase
    {
        private readonly SpringExhibit _exhibit;
        private readonly float _targetPeriod;
        private readonly float _tolerance;

        public SpringTargetPeriodChallenge(
            string id,
            SpringExhibit exhibit,
            float targetPeriod,
            float tolerance = 0.05f,
            string title = null) : base(id)
        {
            _exhibit = exhibit;
            _targetPeriod = targetPeriod;
            _tolerance = tolerance;
            Title = title ?? $"Период = {targetPeriod:F2} с";
            Description =
                $"Подбери массу груза m и жёсткость пружины k так, чтобы период " +
                $"T = 2π·√(m/k) был равен {targetPeriod:F2} с (точность ±{tolerance:F2} с). " +
                $"Когда подберёшь — нажми «Проверить».";
            Hint =
                $"Формула: T = 2π·√(m/k). При k = 50 Н/м для T = {targetPeriod:F1} с нужна масса " +
                $"m ≈ {(targetPeriod / (2f * Mathf.PI)) * (targetPeriod / (2f * Mathf.PI)) * 50f:F2} кг.";
        }

        protected override bool EvaluateInternal()
        {
            float currentPeriod = 2f * Mathf.PI * Mathf.Sqrt(_exhibit.Mass / _exhibit.Stiffness);
            return Mathf.Abs(currentPeriod - _targetPeriod) <= _tolerance;
        }

        public override string GetProgressText()
        {
            float currentPeriod = 2f * Mathf.PI * Mathf.Sqrt(_exhibit.Mass / _exhibit.Stiffness);
            return $"Сейчас: T = {currentPeriod:F3} с    Цель: {_targetPeriod:F2} с";
        }

        public override string SolutionText
        {
            get
            {
                float m = (_targetPeriod / (2f * Mathf.PI)) * (_targetPeriod / (2f * Mathf.PI)) * 50f;
                return
                    "<b>Решение:</b>\n" +
                    "Из формулы T = 2π·√(m/k) выразим m:\n" +
                    "  m = (T / 2π)² · k\n\n" +
                    $"Подставим T = {_targetPeriod:F2} с,  k = 50 Н/м:\n" +
                    $"  m = ({_targetPeriod:F2} / 6.283)² · 50\n" +
                    $"  m ≈ {m:F2} кг\n\n" +
                    $"<b>Ответ:</b>  m ≈ {m:F2} кг,  k = 50 Н/м";
            }
        }
    }

    /// <summary>
    /// Установить тяжёлый груз и проверить.
    /// </summary>
    public class HeavyMassChallenge : CheckedChallengeBase
    {
        private readonly SpringExhibit _exhibit;

        public HeavyMassChallenge(string id, SpringExhibit exhibit) : base(id)
        {
            _exhibit = exhibit;
            Title = "Тяжёлый груз";
            Description =
                "Проверь экспериментально: что будет с периодом если увеличить массу? " +
                "Установи массу не меньше 4 кг и жёсткость 50 Н/м, затем нажми «Проверить».";
            Hint =
                "Из формулы T = 2π·√(m/k) видно, что период растёт как √m. " +
                "Поэтому при увеличении массы маятник колеблется медленнее.";
        }

        protected override bool EvaluateInternal()
        {
            bool massOk = _exhibit.Mass >= 4.0f;
            bool stiffOk = Mathf.Abs(_exhibit.Stiffness - 50f) < 3f;
            return massOk && stiffOk;
        }

        public override string GetProgressText()
        {
            return $"Сейчас: m = {_exhibit.Mass:F2} кг    k = {_exhibit.Stiffness:F1} Н/м";
        }

        public override string SolutionText =>
            "<b>Решение:</b>\n" +
            "Установи слайдер «Масса m» на значение ≥ 4 кг,\n" +
            "слайдер «Жёсткость k» примерно на 50 Н/м.\n\n" +
            "После этого период станет: T = 2π·√(4 / 50) ≈ 1.78 с";
    }

    /// <summary>
    /// Включить демпфирование, увидеть затухание.
    /// </summary>
    public class ShowDampingChallenge : CheckedChallengeBase
    {
        private readonly SpringExhibit _exhibit;

        public ShowDampingChallenge(string id, SpringExhibit exhibit) : base(id)
        {
            _exhibit = exhibit;
            Title = "Затухание";
            Description =
                "Реальная пружина теряет энергию из-за трения. Установи коэффициент " +
                "демпфирования больше 1.0 и понаблюдай как амплитуда уменьшается. " +
                "Затем нажми «Проверить».";
            Hint = "Двигай слайдер «Трение c» к значению больше 1.0.";
        }

        protected override bool EvaluateInternal()
        {
            return _exhibit.Damping >= 1.0f;
        }

        public override string GetProgressText()
        {
            return $"Сейчас: трение c = {_exhibit.Damping:F2}    Цель: ≥ 1.0";
        }

        public override string SolutionText =>
            "<b>Решение:</b>\n" +
            "Передвинь слайдер «Трение c» на значение 1.0 или больше.\n\n" +
            "При увеличении трения уравнение колебаний\n" +
            "m·x'' + c·x' + k·x = 0 даёт затухающие колебания —\n" +
            "амплитуда уменьшается со временем.";
    }
}