using UnityEngine;
using ScienceMuseum.Core;
using ScienceMuseum.Exhibits;

namespace ScienceMuseum.Simulation.Challenges
{
    public abstract class CheckedChallengeBase : IChallenge
    {
        public string Id { get; }
        public string Title { get; protected set; }
        public string Description { get; protected set; }
        public string Hint { get; protected set; }
        public ChallengeStatus Status { get; protected set; }
        public int FailedAttempts { get; protected set; }
        public abstract string SolutionText { get; }

        protected CheckedChallengeBase(string id)
        {
            Id = id;
            Status = ChallengeStatus.NotStarted;
        }

        public bool CheckAnswer()
        {
            // Однажды выполненное задание не "разрешается" обратно
            if (Status == ChallengeStatus.Completed) return true;

            bool correct = EvaluateInternal();

            if (correct)
            {
                Status = ChallengeStatus.Completed;
                return true;
            }
            else
            {
                Status = ChallengeStatus.Failed;
                FailedAttempts++;
                return false;
            }
        }

        protected abstract bool EvaluateInternal();

        public abstract string GetProgressText();
    }

    /// <summary>
    /// Задание "подобрать период Т, равный целевому".
    /// </summary>
    public class TargetPeriodChallenge : CheckedChallengeBase
    {
        private readonly PendulumExhibit _exhibit;
        private readonly float _targetPeriod;
        private readonly float _tolerance;

        public TargetPeriodChallenge(
            string id,
            PendulumExhibit exhibit,
            float targetPeriod,
            float tolerance = 0.05f,
            string title = null,
            string description = null,
            string hint = null) : base(id)
        {
            _exhibit = exhibit;
            _targetPeriod = targetPeriod;
            _tolerance = tolerance;
            Title = title ?? $"Период = {targetPeriod:F2} с";
            Description = description ??
                $"Подбери длину нити L и гравитацию g так, чтобы период колебаний был равен " +
                $"{targetPeriod:F2} с (с точностью ±{tolerance:F2} с). " +
                $"Когда будешь уверен — нажми «Проверить».";
            Hint = hint ??
                $"Используй формулу T = 2π√(L/g). При g = 9.81 для T = {targetPeriod:F1} с " +
                $"длина L ≈ {(targetPeriod / (2f * Mathf.PI)) * (targetPeriod / (2f * Mathf.PI)) * 9.81f:F2} м.";
        }

        protected override bool EvaluateInternal()
        {
            float currentPeriod = 2f * Mathf.PI * Mathf.Sqrt(_exhibit.Length / _exhibit.Gravity);
            return Mathf.Abs(currentPeriod - _targetPeriod) <= _tolerance;
        }

        public override string GetProgressText()
        {
            float currentPeriod = 2f * Mathf.PI * Mathf.Sqrt(_exhibit.Length / _exhibit.Gravity);
            return $"Сейчас: T = {currentPeriod:F3} с    Цель: {_targetPeriod:F2} с";
        }

        public override string SolutionText
        {
            get
            {
                float L = (_targetPeriod / (2f * Mathf.PI)) * (_targetPeriod / (2f * Mathf.PI)) * 9.81f;
                return
                    "<b>Решение:</b>\n" +
                    "Из формулы T = 2π·√(L/g) выразим L:\n" +
                    "  L = (T / 2π)² · g\n\n" +
                    $"Подставим T = {_targetPeriod:F2} с,  g = 9.81 м/с²:\n" +
                    $"  L = ({_targetPeriod:F2} / 6.283)² · 9.81\n" +
                    $"  L ≈ {L:F2} м\n\n" +
                    $"<b>Ответ:</b>  L ≈ {L:F2} м,  g = 9.81 м/с²";
            }
        }
    }

    /// <summary>
    /// Задание "настроить на указанную планетарную гравитацию".
    /// </summary>
    public class MatchGravityChallenge : CheckedChallengeBase
    {
        private readonly PendulumExhibit _exhibit;
        private readonly float _targetGravity;
        private readonly float _tolerance;
        private readonly string _planetName;

        public MatchGravityChallenge(string id, PendulumExhibit exhibit,
            float targetGravity, string planetName, float tolerance = 0.15f) : base(id)
        {
            _exhibit = exhibit;
            _targetGravity = targetGravity;
            _tolerance = tolerance;
            _planetName = planetName;
            Title = $"Маятник на {planetName}";
            Description = $"Установи значение гравитации, какое наблюдается на поверхности " +
                          $"«{planetName}». После выбора нажми «Проверить» — посмотришь, как " +
                          $"изменится период качания.";
            Hint = $"Подсказка: на {planetName} ускорение свободного падения примерно равно " +
                   $"{targetGravity:F2} м/с². Двигай слайдер «Гравитация».";
        }

        protected override bool EvaluateInternal()
        {
            return Mathf.Abs(_exhibit.Gravity - _targetGravity) <= _tolerance;
        }

        public override string GetProgressText()
        {
            return $"Сейчас: g = {_exhibit.Gravity:F2} м/с²    Цель: {_targetGravity:F2} м/с²";
        }

        public override string SolutionText =>
            $"<b>Решение:</b>\n" +
            $"На поверхности {_planetName} ускорение свободного падения\n" +
            $"равно <b>g = {_targetGravity:F2} м/с²</b>.\n\n" +
            $"Установи слайдер «Гравитация g» на это значение и нажми «Проверить».";
    }
}