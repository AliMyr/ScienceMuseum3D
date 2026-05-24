using UnityEngine;
using ScienceMuseum.Core;
using ScienceMuseum.Exhibits;

namespace ScienceMuseum.Simulation.Challenges
{
    /// <summary>
    /// Подобрать первую космическую скорость для круговой орбиты.
    /// </summary>
    public class CircularOrbitChallenge : CheckedChallengeBase
    {
        private readonly OrbitExhibit _exhibit;
        private const float Tolerance = 0.5f;

        public CircularOrbitChallenge(string id, OrbitExhibit exhibit) : base(id)
        {
            _exhibit = exhibit;
            Title = "Круговая орбита";
            Description =
                "Подбери начальную скорость v0 так, чтобы планета вращалась по круговой орбите. " +
                "При правильном значении скорости радиус орбиты не будет меняться. " +
                "Когда подберёшь — нажми «Проверить».";
            Hint =
                "Для круговой орбиты нужна первая космическая скорость v1 = sqrt(mu/r). " +
                $"Текущий радиус r = {exhibit.InitialRadius:F2}, mu = {exhibit.Mu:F1}. " +
                $"Значит v1 ≈ {exhibit.FirstCosmicAtInit:F2} ед/с.";
        }

        protected override bool EvaluateInternal() =>
            Mathf.Abs(_exhibit.InitialSpeed - _exhibit.FirstCosmicAtInit) <= Tolerance;

        public override string GetProgressText()
        {
            float target = _exhibit.FirstCosmicAtInit;
            return $"v0 сейчас: {_exhibit.InitialSpeed:F2}    v1 (целевая): {target:F2}";
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
                    "  mu/r² = v²/r\n\n" +
                    "Отсюда первая космическая скорость:\n" +
                    "  v1 = sqrt(mu/r)\n\n" +
                    $"Подставим mu = {_exhibit.Mu:F1}, r = {_exhibit.InitialRadius:F2}:\n" +
                    $"  v1 = sqrt({_exhibit.Mu:F1} / {_exhibit.InitialRadius:F2})\n" +
                    $"  v1 ≈ {target:F2} ед/с\n\n" +
                    $"<b>Ответ:</b>  v0 ≈ {target:F2} ед/с";
            }
        }
    }

    /// <summary>
    /// Получить эллиптическую орбиту: скорость между круговой и отрыва.
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
                "Скорость должна быть между v1 и v2.";
            Hint =
                $"v1 = {exhibit.FirstCosmicAtInit:F2}, v2 = {exhibit.SecondCosmicAtInit:F2}. " +
                "Возьми что-то заметно больше v1 но меньше v2 — например, v1 умноженное на 1.2.";
        }

        protected override bool EvaluateInternal()
        {
            float v = _exhibit.InitialSpeed;
            return v > _exhibit.FirstCosmicAtInit + 0.5f
                && v < _exhibit.SecondCosmicAtInit - 0.3f;
        }

        public override string GetProgressText() =>
            $"v0 сейчас: {_exhibit.InitialSpeed:F2}    Тип: {_exhibit.CurrentOrbitType}";

        public override string SolutionText
        {
            get
            {
                float v1 = _exhibit.FirstCosmicAtInit;
                return
                    "<b>Решение:</b>\n" +
                    "Эллиптическая орбита получается, когда скорость планеты\n" +
                    "больше круговой, но меньше скорости отрыва:\n" +
                    "  v1 < v < v2\n" +
                    "  v1 = sqrt(mu/r),    v2 = sqrt(2·mu/r) = v1 · sqrt(2)\n\n" +
                    $"Возьми, например, v ≈ {(v1 * 1.2f):F2} ед/с — это " +
                    $"в 1.2 раза больше круговой скорости.\n\n" +
                    "<b>Ответ:</b>  любая v в диапазоне (v1 + запас) ... (v2 − запас)";
            }
        }
    }

    /// <summary>
    /// Достичь второй космической: преодолеть гравитацию и улететь.
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
                $"Нужна вторая космическая скорость v2 = sqrt(2·mu/r) ≈ " +
                $"{exhibit.SecondCosmicAtInit:F2}. Установи v0 хотя бы на 0.5 больше.";
        }

        protected override bool EvaluateInternal() =>
            _exhibit.InitialSpeed >= _exhibit.SecondCosmicAtInit + 0.3f;

        public override string GetProgressText() =>
            $"v0 сейчас: {_exhibit.InitialSpeed:F2}    v2 (нужно ≥): {_exhibit.SecondCosmicAtInit:F2}";

        public override string SolutionText
        {
            get
            {
                float v2 = _exhibit.SecondCosmicAtInit;
                return
                    "<b>Решение:</b>\n" +
                    "Чтобы планета покинула гравитационное поле,\n" +
                    "её кинетическая энергия должна превысить потенциальную:\n" +
                    "  ½·v² ≥ mu/r\n" +
                    "  v ≥ sqrt(2·mu/r) = v2\n\n" +
                    $"v2 для текущего r = {_exhibit.InitialRadius:F2}:\n" +
                    $"  v2 = sqrt(2 · {_exhibit.Mu:F1} / {_exhibit.InitialRadius:F2})\n" +
                    $"  v2 ≈ {v2:F2} ед/с\n\n" +
                    $"<b>Ответ:</b>  v0 ≥ {(v2 + 0.5f):F2}";
            }
        }
    }
}