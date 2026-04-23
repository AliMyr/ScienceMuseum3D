using UnityEngine;
using ScienceMuseum.Core;
using ScienceMuseum.Exhibits;

namespace ScienceMuseum.Simulation.Challenges
{
    /// <summary>
    /// Задание "подобрать период Т, равный целевому".
    /// Игрок должен изменять L и g так, чтобы T = 2π√(L/g) попала в диапазон.
    /// </summary>
    public class TargetPeriodChallenge : IChallenge
    {
        public string Title { get; }
        public string Description { get; }
        public string Hint { get; }
        public ChallengeStatus Status { get; private set; }

        private readonly PendulumExhibit _exhibit;
        private readonly float _targetPeriod;
        private readonly float _tolerance;
        private float _currentPeriod;

        public TargetPeriodChallenge(
            PendulumExhibit exhibit,
            float targetPeriod,
            float tolerance = 0.05f,
            string title = null,
            string description = null,
            string hint = null)
        {
            _exhibit = exhibit;
            _targetPeriod = targetPeriod;
            _tolerance = tolerance;
            Title = title ?? $"Период = {targetPeriod:F2} с";
            Description = description ??
                $"Подбери длину нити L и гравитацию g так, чтобы период T = 2π√(L/g) " +
                $"был равен {targetPeriod:F2} с (с точностью ±{tolerance:F2} с).";
            Hint = hint ??
                $"Формула периода: T = 2π√(L/g). При g=9.81 для T={targetPeriod:F1}с нужна " +
                $"длина L = (T/2π)²·g ≈ {(targetPeriod / (2f * Mathf.PI)) * (targetPeriod / (2f * Mathf.PI)) * 9.81f:F2} м.";
        }

        public void Evaluate()
        {
            _currentPeriod = 2f * Mathf.PI * Mathf.Sqrt(_exhibit.Length / _exhibit.Gravity);
            float diff = Mathf.Abs(_currentPeriod - _targetPeriod);

            if (diff <= _tolerance)
            {
                Status = ChallengeStatus.Completed;
            }
            else if (diff <= _tolerance * 3f)
            {
                // "Близко" - показываем что направление верное
                Status = ChallengeStatus.InProgress;
            }
            else
            {
                Status = ChallengeStatus.NotStarted;
            }
        }

        public string GetProgressText()
        {
            float diff = _currentPeriod - _targetPeriod;
            string delta = diff > 0 ? $"+{diff:F3}" : $"{diff:F3}";
            return $"Цель: T = {_targetPeriod:F2} с (±{_tolerance:F2})\n" +
                   $"Сейчас: T = {_currentPeriod:F3} с ({delta} с от цели)";
        }
    }

    /// <summary>
    /// Задание "настроить на указанную планетарную гравитацию".
    /// </summary>
    public class MatchGravityChallenge : IChallenge
    {
        public string Title { get; }
        public string Description { get; }
        public string Hint { get; }
        public ChallengeStatus Status { get; private set; }

        private readonly PendulumExhibit _exhibit;
        private readonly float _targetGravity;
        private readonly float _tolerance;
        private readonly string _planetName;

        public MatchGravityChallenge(PendulumExhibit exhibit, float targetGravity,
                                       string planetName, float tolerance = 0.15f)
        {
            _exhibit = exhibit;
            _targetGravity = targetGravity;
            _tolerance = tolerance;
            _planetName = planetName;
            Title = $"Маятник на {planetName}";
            Description = $"Установи значение гравитации равное той, что на поверхности " +
                          $"«{planetName}» (g = {targetGravity:F2} м/с²). Понаблюдай как изменится " +
                          $"период качания.";
            Hint = $"На {planetName} ускорение свободного падения равно {targetGravity:F2} м/с². " +
                   $"Двигай слайдер гравитации.";
        }

        public void Evaluate()
        {
            float diff = Mathf.Abs(_exhibit.Gravity - _targetGravity);
            Status = diff <= _tolerance ? ChallengeStatus.Completed :
                     diff <= _tolerance * 3f ? ChallengeStatus.InProgress :
                     ChallengeStatus.NotStarted;
        }

        public string GetProgressText()
        {
            return $"Цель: g = {_targetGravity:F2} м/с² ({_planetName})\n" +
                   $"Сейчас: g = {_exhibit.Gravity:F2} м/с²";
        }
    }
}