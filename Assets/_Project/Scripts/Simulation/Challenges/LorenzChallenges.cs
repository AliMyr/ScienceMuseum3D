using UnityEngine;
using ScienceMuseum.Core;
using ScienceMuseum.Exhibits;

namespace ScienceMuseum.Simulation.Challenges
{
    /// <summary>
    /// Установить классические параметры аттрактора Лоренца.
    /// </summary>
    public class ClassicLorenzChallenge : CheckedChallengeBase
    {
        private readonly LorenzExhibit _exhibit;

        public ClassicLorenzChallenge(string id, LorenzExhibit exhibit) : base(id)
        {
            _exhibit = exhibit;
            Title = "Классическая бабочка";
            Description =
                "Установи классические параметры открытия Лоренца 1963 года: " +
                "sigma = 10, rho = 28, beta ≈ 2.67. Такая комбинация даёт " +
                "знаменитую бабочку — самый известный пример хаоса.";
            Hint =
                "Двигай слайдеры sigma, rho, beta чтобы они стали 10.00, 28.00, 2.67. " +
                "Точность ±0.5.";
        }

        protected override bool EvaluateInternal()
        {
            bool sigmaOk = Mathf.Abs(_exhibit.Sigma - 10f) < 0.5f;
            bool rhoOk = Mathf.Abs(_exhibit.Rho - 28f) < 0.5f;
            bool betaOk = Mathf.Abs(_exhibit.Beta - 8f / 3f) < 0.1f;
            return sigmaOk && rhoOk && betaOk;
        }

        public override string GetProgressText()
        {
            return $"Сейчас: sigma={_exhibit.Sigma:F2}, rho={_exhibit.Rho:F2}, beta={_exhibit.Beta:F3}\n" +
                   "Цель: sigma=10.00, rho=28.00, beta≈2.67";
        }

        public override string SolutionText =>
            "<b>Решение:</b>\n" +
            "Эдвард Лоренц в 1963 году обнаружил хаос именно при этих значениях:\n\n" +
            "  sigma = 10\n" +
            "  rho   = 28\n" +
            "  beta  = 8/3 ≈ 2.667\n\n" +
            "Эти параметры стали стандартом — на любом графике аттрактора Лоренца " +
            "вы увидите именно их.\n\n" +
            "<b>Установи слайдеры на эти значения и нажми «Проверить».</b>";
    }

    /// <summary>
    /// Перевести систему в режим стабильного равновесия (без хаоса).
    /// </summary>
    public class StablePointChallenge : CheckedChallengeBase
    {
        private readonly LorenzExhibit _exhibit;

        public StablePointChallenge(string id, LorenzExhibit exhibit) : base(id)
        {
            _exhibit = exhibit;
            Title = "Точка покоя";
            Description =
                "Параметр rho определяет режим системы. Установи rho < 1 — " +
                "тогда любая точка в пространстве рано или поздно успокоится " +
                "в начале координат. Это режим без хаоса.";
            Hint =
                "Поставь слайдер rho ниже 1 (например 0.5). " +
                "После этого нажми «Сбросить» внизу панели и наблюдай — точка должна замереть.";
        }

        protected override bool EvaluateInternal()
        {
            return _exhibit.Rho < 1.0f;
        }

        public override string GetProgressText()
        {
            return $"Сейчас: rho = {_exhibit.Rho:F2}\nЦель: rho < 1.0";
        }

        public override string SolutionText =>
            "<b>Решение:</b>\n" +
            "Анализ устойчивости системы Лоренца показывает:\n\n" +
            "  При rho < 1 — единственное равновесие (0, 0, 0) устойчиво.\n" +
            "  При 1 < rho < 24.74 — два равновесия в крыльях.\n" +
            "  При rho > 24.74 — хаос.\n\n" +
            "<b>Установи rho например на 0.5 — точка успокоится в центре.</b>";
    }

    /// <summary>
    /// "Эффект бабочки" - сильно изменить rho чтобы получить разные траектории.
    /// </summary>
    public class ButterflyEffectChallenge : CheckedChallengeBase
    {
        private readonly LorenzExhibit _exhibit;

        public ButterflyEffectChallenge(string id, LorenzExhibit exhibit) : base(id)
        {
            _exhibit = exhibit;
            Title = "Хаотический режим";
            Description =
                "Установи rho > 25 — система войдёт в хаотический режим. " +
                "Точка будет вечно блуждать между двумя крыльями, никогда не повторяя путь.";
            Hint =
                "Поставь слайдер rho выше 25 (например 28 — классическое значение). " +
                "После «Сбросить» наблюдай как формируется бабочка.";
        }

        protected override bool EvaluateInternal()
        {
            return _exhibit.Rho > 25f;
        }

        public override string GetProgressText()
        {
            return $"Сейчас: rho = {_exhibit.Rho:F2}\nЦель: rho > 25";
        }

        public override string SolutionText =>
            "<b>Решение:</b>\n" +
            "При rho > 24.74 система Лоренца становится хаотической.\n\n" +
            "Это означает:\n" +
            "  - Траектория не замыкается, точка вечно блуждает\n" +
            "  - Малейшее изменение начальных условий ведёт к радикально разной траектории\n" +
            "  - Это и есть знаменитый «эффект бабочки»\n\n" +
            "<b>Поставь rho = 28 (классическое значение Лоренца).</b>";
    }
}