using UnityEngine;
using ScienceMuseum.Core;
using ScienceMuseum.Exhibits;

namespace ScienceMuseum.Simulation.Challenges
{
    /// <summary>
    /// Подавить эпидемию: добиться R0 < 1 (β < γ).
    /// </summary>
    public class ContainEpidemicChallenge : CheckedChallengeBase
    {
        private readonly SIREpidemicExhibit _exhibit;

        public ContainEpidemicChallenge(string id, SIREpidemicExhibit exhibit) : base(id)
        {
            _exhibit = exhibit;
            Title = "Подавить эпидемию";
            Description =
                "Добейся, чтобы базовое число воспроизводства R0 = β/γ стало меньше 1. " +
                "Тогда эпидемия не сможет распространиться: заражённые будут выздоравливать " +
                "быстрее, чем успеют заразить новых.";
            Hint =
                "R0 = β/γ. Чтобы R0 < 1, нужно либо снизить заразность β, либо ускорить " +
                "выздоровление γ. Например, β = 0.1 и γ = 0.5 дают R0 = 0.2.";
        }

        protected override bool EvaluateInternal() =>
            _exhibit.BasicReproductionNumber < 1.0;

        public override string GetProgressText() =>
            $"R0 = β/γ = {_exhibit.BasicReproductionNumber:F2}    Цель: R0 < 1.00";

        public override string SolutionText =>
            "<b>Решение:</b>\n" +
            "R0 < 1 ⇔ β < γ. Любая пара значений с β < γ подходит.\n\n" +
            "Например:\n" +
            "  β = 0.1,  γ = 0.5  →  R0 = 0.20.\n\n" +
            "Такая комбинация моделирует ситуацию, когда болезнь слабо заразна, " +
            "а выздоровление быстрое — эпидемия гаснет сама.";
    }

    /// <summary>
    /// Спровоцировать сильную эпидемию: добиться пика I(t) выше 30%.
    /// </summary>
    public class SevereOutbreakChallenge : CheckedChallengeBase
    {
        private readonly SIREpidemicExhibit _exhibit;
        private const float TargetPeakFraction = 0.30f;

        public SevereOutbreakChallenge(string id, SIREpidemicExhibit exhibit) : base(id)
        {
            _exhibit = exhibit;
            Title = "Вспышка эпидемии";
            Description =
                $"Подбери параметры так, чтобы в пик эпидемии было одновременно заражено " +
                $"больше {TargetPeakFraction * 100f:F0}% популяции. " +
                "Сбрось симуляцию, подожди, пока эпидемия пройдёт, и нажми «Проверить».";
            Hint =
                "Пик I(t) растёт с увеличением R0 = β/γ. Подними β и опусти γ так, чтобы " +
                "R0 стало не меньше 4–5. Не забудь нажать «Сбросить» после изменения параметров.";
        }

        protected override bool EvaluateInternal() =>
            _exhibit.MaxObservedInfected > TargetPeakFraction;

        public override string GetProgressText() =>
            $"Макс. заражено: {_exhibit.MaxObservedInfected * 100.0:F1}%    " +
            $"Цель: > {TargetPeakFraction * 100f:F0}%";

        public override string SolutionText =>
            "<b>Решение:</b>\n" +
            "Аналитически максимум I(t) определяется уравнением\n" +
            "  I_max = 1 - (1/R0)·(1 + ln(R0·S0)),\n" +
            "верным при I0 → 0 и S0 ≈ 1. Чем больше R0, тем выше пик.\n\n" +
            "Пример рабочих параметров:\n" +
            "  β = 1.5,  γ = 0.1  →  R0 = 15,\n" +
            "  S0 = 0.99,  I0 = 0.01.\n\n" +
            "После «Сбросить» пик заражённых превысит 60% популяции.";
    }

    /// <summary>
    /// Коллективный иммунитет: при высоком R0 опустить S0 ниже критического порога 1/R0.
    /// </summary>
    public class HerdImmunityChallenge : CheckedChallengeBase
    {
        private readonly SIREpidemicExhibit _exhibit;
        private const float MinReproductionNumber = 2f;

        public HerdImmunityChallenge(string id, SIREpidemicExhibit exhibit) : base(id)
        {
            _exhibit = exhibit;
            Title = "Коллективный иммунитет";
            Description =
                "Эпидемия не сможет развиться, если доля восприимчивых S0 ниже критического " +
                $"порога 1/R0. Покажи это: установи R0 ≥ {MinReproductionNumber:F0} " +
                "(сильная заразность) и опусти S0 ниже 1/R0 (имитация вакцинации). " +
                "После этого нажми «Проверить».";
            Hint =
                "Например, при β = 1.0 и γ = 0.1 получаем R0 = 10, значит S_crit = 1/10 = 0.1. " +
                "Опусти слайдер S0 ниже 0.1 — и эпидемия не разовьётся, несмотря на высокую β.";
        }

        protected override bool EvaluateInternal()
        {
            double r0 = _exhibit.BasicReproductionNumber;
            if (r0 < MinReproductionNumber) return false;
            return _exhibit.S0Initial < _exhibit.HerdImmunityThreshold;
        }

        public override string GetProgressText()
        {
            double r0 = _exhibit.BasicReproductionNumber;
            double sCrit = _exhibit.HerdImmunityThreshold;
            return $"R0 = {r0:F2}    S0 = {_exhibit.S0Initial:F3}    " +
                   $"S_crit = {sCrit:F3}    Цель: R0 ≥ {MinReproductionNumber:F0} и S0 < S_crit";
        }

        public override string SolutionText =>
            "<b>Решение:</b>\n" +
            "Критическое условие: S0 < 1/R0 = γ/β.\n\n" +
            "Например, установи:\n" +
            "  β = 1.0,  γ = 0.1  →  R0 = 10,  S_crit = 0.10,\n" +
            "  S0 = 0.05.\n\n" +
            "Несмотря на высокую заразность, эпидемия не разовьётся: " +
            "оставшихся восприимчивых слишком мало, чтобы каждый заражённый успел " +
            "передать болезнь хотя бы одному новому до выздоровления.";
    }
}