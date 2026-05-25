using ScienceMuseum.Simulation.Solvers;

namespace ScienceMuseum.Simulation.Models
{
    /// <summary>
    /// Модель SIR (Susceptible–Infected–Recovered) — классическая модель
    /// эпидемиологии. Состояние — три доли популяции, S + I + R = 1.
    ///   dS/dt = -β·S·I
    ///   dI/dt = β·S·I - γ·I
    ///   dR/dt = γ·I
    /// Базовое репродуктивное число R₀ = β/γ определяет режим:
    /// при R₀ < 1 эпидемия затухает, при R₀ > 1 проходит пик и спадает.
    /// </summary>
    public class SIRModel
    {
        public double Beta { get; set; } = 0.5;
        public double Gamma { get; set; } = 0.1;

        public double S { get; private set; }
        public double I { get; private set; }
        public double R { get; private set; }

        /// <summary>
        /// Максимальное значение I(t), наблюдавшееся с момента последнего Reset.
        /// Используется для задачи «Вспышка эпидемии».
        /// </summary>
        public double MaxObservedInfected { get; private set; }

        private readonly IOdeSolver _solver;
        private double _time;

        public SIRModel(IOdeSolver solver = null)
        {
            _solver = solver ?? new RungeKutta4();
        }

        public void Reset(double s0, double i0)
        {
            S = s0;
            I = i0;
            R = 1.0 - s0 - i0;
            if (R < 0.0) R = 0.0;
            _time = 0.0;
            MaxObservedInfected = i0;
        }

        public void Step(double dt)
        {
            double[] state = { S, I, R };
            double[] newState = _solver.Step(state, Derivatives, _time, dt);

            S = newState[0];
            I = newState[1];
            R = newState[2];
            _time += dt;

            if (I > MaxObservedInfected) MaxObservedInfected = I;
        }

        private double[] Derivatives(double t, double[] state)
        {
            double s = state[0];
            double i = state[1];

            double dS = -Beta * s * i;
            double dI = Beta * s * i - Gamma * i;
            double dR = Gamma * i;

            return new double[] { dS, dI, dR };
        }

        /// <summary>
        /// Базовое репродуктивное число R₀ = β/γ.
        /// </summary>
        public double BasicReproductionNumber =>
            Gamma > 1e-12 ? Beta / Gamma : double.PositiveInfinity;

        /// <summary>
        /// Критический порог восприимчивых для коллективного иммунитета: S_crit = 1/R₀ = γ/β.
        /// Если фактическое S₀ ниже этого значения, эпидемия не развивается.
        /// </summary>
        public double HerdImmunityThreshold =>
            Beta > 1e-12 ? Gamma / Beta : 1.0;

        /// <summary>
        /// Качественный режим текущих параметров.
        /// </summary>
        public string Regime
        {
            get
            {
                double r0 = BasicReproductionNumber;
                if (r0 < 1.0) return "затухание (R0 < 1)";
                if (r0 < 2.5) return "умеренная эпидемия";
                return "сильная эпидемия";
            }
        }
    }
}