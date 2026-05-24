using System;
using ScienceMuseum.Simulation.Solvers;

namespace ScienceMuseum.Simulation.Models
{
    /// <summary>
    /// Движение тела в гравитационном поле неподвижного центра.
    /// x'' = -μ·x/r³,  y'' = -μ·y/r³,  где μ = G·M, r = √(x² + y²).
    /// Состояние: [x, y, vx, vy].
    /// </summary>
    public class OrbitModel
    {
        public double Mu { get; set; } = 100.0;

        public double X { get; private set; }
        public double Y { get; private set; }
        public double Vx { get; private set; }
        public double Vy { get; private set; }

        private readonly IOdeSolver _solver;
        private double _time;

        public OrbitModel(IOdeSolver solver = null)
        {
            _solver = solver ?? new RungeKutta4();
        }

        /// <summary>
        /// Начальные условия через радиус и тангенциальную скорость.
        /// </summary>
        public void Reset(double initialRadius, double tangentialSpeed)
        {
            X = initialRadius;
            Y = 0;
            Vx = 0;
            Vy = tangentialSpeed;
            _time = 0;
        }

        public void Step(double dt)
        {
            double[] state = { X, Y, Vx, Vy };
            double[] newState = _solver.Step(state, Derivatives, _time, dt);
            X = newState[0];
            Y = newState[1];
            Vx = newState[2];
            Vy = newState[3];
            _time += dt;
        }

        private double[] Derivatives(double t, double[] state)
        {
            double x = state[0];
            double y = state[1];
            double vx = state[2];
            double vy = state[3];

            double r2 = x * x + y * y;
            double r = Math.Sqrt(r2);
            if (r < 1e-6) r = 1e-6; // защита от деления на ноль при попадании в центр

            double r3 = r2 * r;
            double ax = -Mu * x / r3;
            double ay = -Mu * y / r3;

            return new double[] { vx, vy, ax, ay };
        }

        public double Radius => Math.Sqrt(X * X + Y * Y);
        public double Speed => Math.Sqrt(Vx * Vx + Vy * Vy);

        /// <summary>
        /// Полная механическая энергия на единицу массы.
        /// При отсутствии диссипации должна сохраняться — индикатор точности.
        /// </summary>
        public double SpecificEnergy => 0.5 * (Vx * Vx + Vy * Vy) - Mu / Radius;

        /// <summary>
        /// Тип орбиты по знаку полной энергии.
        /// </summary>
        public string OrbitType
        {
            get
            {
                double e = SpecificEnergy;
                if (e < -0.01) return "эллиптическая";
                if (e > 0.01) return "гиперболическая (улетит)";
                return "параболическая";
            }
        }

        public double FirstCosmicSpeed(double radius) => Math.Sqrt(Mu / radius);
        public double SecondCosmicSpeed(double radius) => Math.Sqrt(2.0 * Mu / radius);
    }
}