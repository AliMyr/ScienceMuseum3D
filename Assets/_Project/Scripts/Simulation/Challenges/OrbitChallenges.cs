using UnityEngine;
using ScienceMuseum.Core;
using ScienceMuseum.Exhibits;

namespace ScienceMuseum.Simulation.Challenges
{
    /// <summary>
    /// Задание "круговая орбита" - подобрать первую космическую скорость.
    /// </summary>
    public class CircularOrbitChallenge : CheckedChallengeBase
    {
        private readonly OrbitExhibit _exhibit;
        private const float Tolerance = 0.5f;  // отклонение скорости в "ед/с"

        public CircularOrbitChallenge(string id, OrbitExhibit exhibit) : base(id)
        {
            _exhibit = exhibit;
            Title = "Круговая орбита";
            Description =
                "Подбери начальную скорость v₀ так, чтобы планета вращалась по круговой орбите. " +
                "При правильном значении скорости радиус орбиты не будет меняться. " +
                "Когда подберёшь — нажми «Проверить».";
            Hint =
                "Для круговой орбиты нужна первая космическая скорость v₁ = √(μ/r). " +
                $"Текущий радиус r = {exhibit.InitialRadius:F2}, μ = {exhibit.Mu:F1}. " +
                $"Значит v₁ ≈ {exhibit.FirstCosmicAtInit:F2} ед/с.";
        }

        protected override bool EvaluateInternal()
        {
            float diff = Mathf.Abs(_exhibit.InitialSpeed - _exhibit.FirstCosmicAtInit);
            return diff <= Tolerance;
        }

        public override string GetProgressText()
        {
            float target = _exhibit.FirstCosmicAtInit;
            return $"v₀ сейчас: {_exhibit.InitialSpeed:F2}    " +
                   $"v₁ (целевая): {target:F2}";
        }

        public override string SolutionText
        {
            get
            {
                float target = _exhibit.FirstCosmicAtInit;
                return
                    "<b>Решение:</b>\n" +
                    "Условие круговой орбиты: гравитационная сила обеспечивает\n" +
                    "центростремительное ускорение:\n" +
                    "  μ/r² = v²/r\n\n" +
                    "Отсюда первая космическая скорость:\n" +
                    "  v₁ = √(μ/r)\n\n" +
                    $"Подставим μ = {_exhibit.Mu:F1}, r = {_exhibit.InitialRadius:F2}:\n" +
                    $"  v₁ = √({_exhibit.Mu:F1} / {_exhibit.InitialRadius:F2})\n" +
                    $"  v₁ ≈ {target:F2} ед/с\n\n" +
                    $"<b>Ответ:</b>  v₀ ≈ {target:F2} ед/с";
            }
        }
    }

    /// <summary>
    /// Задание "эллиптическая орбита" - скорость между v₁ и v₂, не круговая.
    /// </summary>
    public class EllipticalOrbitChallenge : CheckedChallengeBase
    {
        private readonly OrbitExhibit _exhibit;

        public EllipticalOrbitChallenge(string id, OrbitExhibit exhibit) : base(id)
        {
            _exhibit = exhibit;
            Title = "Эллиптическая орбита";
            Description =
                "Подбери скорость так, чтобы планета двигалась по эллипсу — " +
                "то есть удалялась от Солнца и снова приближалась. " +
                "Скорость должна быть между v₁ и v₂.";
            Hint =
                $"v₁ = {exhibit.FirstCosmicAtInit:F2}, v₂ = {exhibit.SecondCosmicAtInit:F2}. " +
                "Возьми что-то заметно больше v₁ но меньше v₂ — например, v₁·1.2.";
        }

        protected override bool EvaluateInternal()
        {
            float v = _exhibit.InitialSpeed;
            float v1 = _exhibit.FirstCosmicAtInit;
            float v2 = _exhibit.SecondCosmicAtInit;

            // Эллипс: между v₁ (с минимальным запасом) и v₂ (с минимальным запасом).
            // Не должна быть равна v₁ (это даст круг).
            return v > v1 + 0.5f && v < v2 - 0.3f;
        }

        public override string GetProgressText()
        {
            return $"v₀ сейчас: {_exhibit.InitialSpeed:F2}    Тип: {_exhibit.CurrentOrbitType}";
        }

        public override string SolutionText
        {
            get
            {
                float v1 = _exhibit.FirstCosmicAtInit;
                return
                    "<b>Решение:</b>\n" +
                    "Эллиптическая орбита получается, когда скорость планеты\n" +
                    "больше круговой, но меньше скорости отрыва:\n" +
                    "  v₁ < v < v₂\n" +
                    "  v₁ = √(μ/r),    v₂ = √(2μ/r) = v₁·√2\n\n" +
                    $"Возьми, например, v ≈ {(v1 * 1.2f):F2} ед/с — это " +
                    $"в 1.2 раза больше круговой скорости.\n\n" +
                    "<b>Ответ:</b>  любая v в диапазоне (v₁ + запас) ... (v₂ − запас)";
            }
        }
    }

    /// <summary>
    /// Задание "вторая космическая" - вырвать планету.
    /// </summary>
    public class EscapeVelocityChallenge : CheckedChallengeBase
    {
        private readonly OrbitExhibit _exhibit;

        public EscapeVelocityChallenge(string id, OrbitExhibit exhibit) : base(id)
        {
            _exhibit = exhibit;
            Title = "Вырвать планету";
            Description =
                "Разгони планету так, чтобы она преодолела гравитацию Солнца " +
                "и улетела в космос (гиперболическая орбита). " +
                "Подбери и нажми «Проверить».";
            Hint =
                $"Нужна вторая космическая скорость v₂ = √(2μ/r) ≈ " +
                $"{exhibit.SecondCosmicAtInit:F2}. Установи v₀ хотя бы на 0.5 больше.";
        }

        protected override bool EvaluateInternal()
        {
            float v = _exhibit.InitialSpeed;
            float v2 = _exhibit.SecondCosmicAtInit;
            return v >= v2 + 0.3f;
        }

        public override string GetProgressText()
        {
            return $"v₀ сейчас: {_exhibit.InitialSpeed:F2}    " +
                   $"v₂ (нужно ≥): {_exhibit.SecondCosmicAtInit:F2}";
        }

        public override string SolutionText
        {
            get
            {
                float v2 = _exhibit.SecondCosmicAtInit;
                return
                    "<b>Решение:</b>\n" +
                    "Чтобы планета покинула гравитационное поле,\n" +
                    "её кинетическая энергия должна превысить потенциальную:\n" +
                    "  ½·v² ≥ μ/r\n" +
                    "  v ≥ √(2μ/r) = v₂\n\n" +
                    $"v₂ для текущего r = {_exhibit.InitialRadius:F2}:\n" +
                    $"  v₂ = √(2 · {_exhibit.Mu:F1} / {_exhibit.InitialRadius:F2})\n" +
                    $"  v₂ ≈ {v2:F2} ед/с\n\n" +
                    $"<b>Ответ:</b>  v₀ ≥ {(v2 + 0.5f):F2}";
            }
        }
    }
}