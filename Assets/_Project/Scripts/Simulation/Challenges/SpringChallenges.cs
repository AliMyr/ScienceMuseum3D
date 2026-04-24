using UnityEngine;
using ScienceMuseum.Core;
using ScienceMuseum.Exhibits;

namespace ScienceMuseum.Simulation.Challenges
{
    public class SpringTargetPeriodChallenge : IChallenge
    {
        public string Id { get; }
        public string Title { get; }
        public string Description { get; }
        public string Hint { get; }
        public ChallengeStatus Status { get; private set; }

        private readonly SpringExhibit _exhibit;
        private readonly float _targetPeriod;
        private readonly float _tolerance;
        private float _currentPeriod;

        public SpringTargetPeriodChallenge(
            string id,
            SpringExhibit exhibit,
            float targetPeriod,
            float tolerance = 0.05f,
            string title = null)
        {
            Id = id;
            _exhibit = exhibit;
            _targetPeriod = targetPeriod;
            _tolerance = tolerance;
            Title = title ?? $"Период = {targetPeriod:F2} с";
            Description =
                $"Подбери массу груза m и жёсткость пружины k так, чтобы период " +
                $"T = 2π√(m/k) был равен {targetPeriod:F2} с (точность ±{tolerance:F2} с).";
            Hint =
                $"Формула: T = 2π√(m/k). При k=50 Н/м для T={targetPeriod:F1} с нужна масса " +
                $"m = (T/2π)²·k ≈ {(targetPeriod / (2f * Mathf.PI)) * (targetPeriod / (2f * Mathf.PI)) * 50f:F2} кг.";
        }

        public void Evaluate()
        {
            _currentPeriod = 2f * Mathf.PI * Mathf.Sqrt(_exhibit.Mass / _exhibit.Stiffness);
            float diff = Mathf.Abs(_currentPeriod - _targetPeriod);
            Status = diff <= _tolerance ? ChallengeStatus.Completed :
                     diff <= _tolerance * 3f ? ChallengeStatus.InProgress :
                     ChallengeStatus.NotStarted;
        }

        public string GetProgressText()
        {
            float diff = _currentPeriod - _targetPeriod;
            string delta = diff > 0 ? $"+{diff:F3}" : $"{diff:F3}";
            return $"Цель: T = {_targetPeriod:F2} с (±{_tolerance:F2})\n" +
                   $"Сейчас: T = {_currentPeriod:F3} с ({delta} с)";
        }
    }

    public class HeavyMassChallenge : IChallenge
    {
        public string Id { get; }
        public string Title => "Тяжёлый груз";
        public string Description =>
            "Проверь экспериментально: что будет с периодом если увеличить массу? " +
            "Установи массу не меньше 4 кг и жёсткость 50 Н/м.";
        public string Hint =>
            "Из формулы T = 2π√(m/k) видно, что период растёт как √m. " +
            "При m = 4 кг и k = 50 Н/м: T ≈ 1.78 с.";
        public ChallengeStatus Status { get; private set; }

        private readonly SpringExhibit _exhibit;

        public HeavyMassChallenge(string id, SpringExhibit exhibit)
        {
            Id = id;
            _exhibit = exhibit;
        }

        public void Evaluate()
        {
            bool massOk = _exhibit.Mass >= 4.0f;
            bool stiffOk = Mathf.Abs(_exhibit.Stiffness - 50f) < 3f;
            Status = (massOk && stiffOk) ? ChallengeStatus.Completed : ChallengeStatus.NotStarted;
        }

        public string GetProgressText()
        {
            return $"Масса: {_exhibit.Mass:F2} кг (нужно ≥ 4)\n" +
                   $"Жёсткость: {_exhibit.Stiffness:F1} Н/м (нужно 50)";
        }
    }

    public class ShowDampingChallenge : IChallenge
    {
        public string Id { get; }
        public string Title => "Затухание";
        public string Description =>
            "Реальная пружина теряет энергию из-за трения. Установи коэффициент " +
            "демпфирования больше 1.0 и наблюдай как амплитуда уменьшается.";
        public string Hint => "Двигай слайдер «Трение» к значению больше 1.";
        public ChallengeStatus Status { get; private set; }

        private readonly SpringExhibit _exhibit;

        public ShowDampingChallenge(string id, SpringExhibit exhibit)
        {
            Id = id;
            _exhibit = exhibit;
        }

        public void Evaluate()
        {
            Status = _exhibit.Damping >= 1.0f ? ChallengeStatus.Completed : ChallengeStatus.NotStarted;
        }

        public string GetProgressText()
        {
            return $"Трение: {_exhibit.Damping:F2} (нужно ≥ 1.0)";
        }
    }
}