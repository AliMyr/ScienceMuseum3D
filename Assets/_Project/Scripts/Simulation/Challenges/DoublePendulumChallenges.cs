using UnityEngine;
using ScienceMuseum.Core;
using ScienceMuseum.Exhibits;

namespace ScienceMuseum.Simulation.Challenges
{
    /// <summary>
    /// Задание: установить симметричную конфигурацию (L1 = L2, m1 = m2).
    /// Демонстрирует понятие нормальных мод — в симметричной системе при одинаковых
    /// начальных углах звенья колеблются как единое тело, при противоположных —
    /// в противофазе с другой частотой. Любое движение разлагается на эти две моды.
    /// </summary>
    public class SymmetricConfigurationChallenge : CheckedChallengeBase
    {
        private readonly DoublePendulumExhibit _exhibit;
        private const float Tolerance = 0.05f;

        public SymmetricConfigurationChallenge(string id, DoublePendulumExhibit exhibit) : base(id)
        {
            _exhibit = exhibit;
            Title = "Симметричная конфигурация";
            Description =
                "Сделай два звена идентичными: длины L1 = L2 и массы m1 = m2 " +
                $"(с точностью ±{Tolerance:F2}). В такой конфигурации система имеет " +
                "две аккуратные нормальные моды — синфазную и противофазную.";
            Hint =
                "Двигай слайдеры так, чтобы L1 совпадал с L2, и m1 совпадала с m2. " +
                "Поставь damping = 0 для чистоты эксперимента — тогда моды видны идеально.";
        }

        protected override bool EvaluateInternal()
        {
            return Mathf.Abs(_exhibit.Length1 - _exhibit.Length2) <= Tolerance
                && Mathf.Abs(_exhibit.Mass1 - _exhibit.Mass2) <= Tolerance;
        }

        public override string GetProgressText()
        {
            float dL = Mathf.Abs(_exhibit.Length1 - _exhibit.Length2);
            float dM = Mathf.Abs(_exhibit.Mass1 - _exhibit.Mass2);
            return $"|L1−L2| = {dL:F3}    |m1−m2| = {dM:F3}    " +
                   $"Цель: оба значения ≤ {Tolerance:F2}";
        }

        public override string SolutionText =>
            "<b>Решение:</b>\n" +
            "Любая комбинация одинаковых L и m подходит. Например:\n" +
            "  L1 = L2 = 1.00 м,  m1 = m2 = 1.00 кг.\n\n" +
            "<b>Что это даёт:</b>\n" +
            "При симметричной массе и длине система имеет две нормальные моды.\n" +
            "  • θ1 = θ2 — синфазное колебание (звенья качаются как единое тело).\n" +
            "  • θ1 = −θ2 — противофазное колебание с большей частотой.\n" +
            "Любое начальное условие — суперпозиция этих двух мод.";
    }

    /// <summary>
    /// Задание: получить квазипериодическое поведение при малых углах.
    /// Демонстрирует природу хаоса — он следствие нелинейности sin(θ).
    /// При малых θ разложение sin(θ) ≈ θ делает уравнения линейными, и система
    /// сводится к двум связанным гармоническим осцилляторам без хаоса.
    /// </summary>
    public class SmallAnglesChallenge : CheckedChallengeBase
    {
        private readonly DoublePendulumExhibit _exhibit;
        private const float MaxAllowedAngleDegrees = 15f;
        private const float MaxAllowedDamping = 0.001f;

        public SmallAnglesChallenge(string id, DoublePendulumExhibit exhibit) : base(id)
        {
            _exhibit = exhibit;
            Title = "Малые углы — нет хаоса";
            Description =
                "Установи начальные углы θ1 и θ2 так, чтобы их модули были " +
                $"не больше {MaxAllowedAngleDegrees:F0}°, а трение поставь в ноль. " +
                "Сбрось симуляцию и понаблюдай: движение почти периодическое, " +
                "без хаотических кульбитов.";
            Hint =
                "Хаос в двойном маятнике — следствие нелинейности sin(θ). " +
                "При малых углах sin(θ) ≈ θ, и система становится линейной — " +
                "а у линейных систем хаоса не бывает.";
        }

        protected override bool EvaluateInternal()
        {
            return Mathf.Abs(_exhibit.Theta1InitialDegrees) <= MaxAllowedAngleDegrees
                && Mathf.Abs(_exhibit.Theta2InitialDegrees) <= MaxAllowedAngleDegrees
                && _exhibit.Damping <= MaxAllowedDamping;
        }

        public override string GetProgressText()
        {
            return $"|θ1| = {Mathf.Abs(_exhibit.Theta1InitialDegrees):F0}°    " +
                   $"|θ2| = {Mathf.Abs(_exhibit.Theta2InitialDegrees):F0}°    " +
                   $"k = {_exhibit.Damping:F3}    " +
                   $"Цель: оба угла ≤ {MaxAllowedAngleDegrees:F0}°, k ≈ 0";
        }

        public override string SolutionText =>
            "<b>Решение:</b>\n" +
            "Установи, например:\n" +
            "  θ1 = 10°,  θ2 = 5°,  damping = 0.\n\n" +
            "<b>Почему это работает:</b>\n" +
            "При малых углах разложение в ряд Тейлора даёт sin(θ) ≈ θ, " +
            "и уравнения движения становятся линейными. Линейная система с двумя " +
            "степенями свободы имеет ровно две нормальные моды — её движение строго " +
            "периодично (или квазипериодично, если частоты несоизмеримы). " +
            "Хаос требует существенной нелинейности, и эту нелинейность мы получаем " +
            "только при больших углах, где sin(θ) уже не аппроксимируется через θ.";
    }

    /// <summary>
    /// Задание: добиться полного оборота нижнего груза (|θ2| > π).
    /// Демонстрирует энергетический порог переворота и эффект перекачки энергии
    /// между звеньями через нелинейную связь.
    /// </summary>
    public class FullFlipChallenge : CheckedChallengeBase
    {
        private readonly DoublePendulumExhibit _exhibit;

        public FullFlipChallenge(string id, DoublePendulumExhibit exhibit) : base(id)
        {
            _exhibit = exhibit;
            Title = "Кувырок нижнего груза";
            Description =
                "Подбери параметры и начальные углы так, чтобы нижний груз " +
                "сделал полный оборот вокруг точки крепления — " +
                "то есть чтобы |θ2| превысило 180° хотя бы один раз. " +
                "Сбрось симуляцию, дай маятнику покачаться несколько секунд, " +
                "потом нажми «Проверить».";
            Hint =
                "Чтобы нижний груз перевернулся, ему нужно набрать достаточно " +
                "кинетической энергии. Большие начальные углы (близко к 180°) " +
                "и нулевое трение помогут. Конфигурация «тяжёлый верх, лёгкий низ» " +
                "(m1 > m2) тоже способствует — массивное верхнее звено эффективно " +
                "размахивает нижним.";
        }

        protected override bool EvaluateInternal()
        {
            return _exhibit.MaxTheta2Observed > Mathf.PI;
        }

        public override string GetProgressText()
        {
            float maxDeg = Mathf.Rad2Deg * (float)_exhibit.MaxTheta2Observed;
            return $"Макс. наблюдённый |θ2|: {maxDeg:F0}°    Цель: > 180°";
        }

        public override string SolutionText =>
            "<b>Один из рабочих рецептов:</b>\n" +
            "  L1 = 1.00 м,  L2 = 0.60 м,\n" +
            "  m1 = 2.50 кг,  m2 = 0.80 кг,\n" +
            "  θ1 = 150°,  θ2 = −90°,\n" +
            "  damping = 0.\n\n" +
            "Сбрось симуляцию и подожди ~5–10 секунд — нижний груз сделает кульбит.\n\n" +
            "<b>Физика:</b>\n" +
            "Чтобы нижний груз перевернулся, нужна полная энергия выше потенциального " +
            "барьера E_min = 2·m2·g·L2 (подъём груза с дна на самый верх). " +
            "Большие начальные углы дают высокую начальную потенциальную энергию, " +
            "и при последующем падении она через сложную нелинейную связь " +
            "перекачивается в нижнее звено. Именно эта перекачка — суть хаоса " +
            "в системе: в какой момент случится переворот, заранее предсказать сложно.";
    }
}