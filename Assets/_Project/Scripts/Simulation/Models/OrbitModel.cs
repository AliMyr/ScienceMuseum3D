using System;
using ScienceMuseum.Simulation.Solvers;

namespace ScienceMuseum.Simulation.Models
{
    /// <summary>
    /// Модель движения планеты в гравитационном поле центрального тела.
    /// Состояние: [x, y, vx, vy] - позиция и скорость в плоскости.
    /// Уравнения:  d²x/dt² = -μ·x/r³,  d²y/dt² = -μ·y/r³,  где μ = G·M.
    /// Центральное тело (Солнце) считается неподвижным в начале координат.
    /// </summary>
    public class OrbitModel
    {
        // Параметр: гравитационный параметр центрального тела (G·M)
        public double Mu { get; set; } = 100.0;

        // Текущее состояние
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
        /// Установить начальные условия.
        /// Удобно задавать через начальный радиус и скорость по касательной (перпендикулярно радиусу).
        /// </summary>
        public void Reset(double initialRadius, double tangentialSpeed)
        {
            X = initialRadius;
            Y = 0;
            Vx = 0;
            Vy = tangentialSpeed;  // движение по касательной (перпендикулярно радиус-вектору)
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

            // Защита от деления на 0 (если планета попала в центр)
            if (r < 1e-6) r = 1e-6;

            double r3 = r2 * r;
            double ax = -Mu * x / r3;
            double ay = -Mu * y / r3;

            return new double[] { vx, vy, ax, ay };
        }

        // ── Производные характеристики ─────────────────────────────────────

        /// <summary>Текущее расстояние до центра.</summary>
        public double Radius => Math.Sqrt(X * X + Y * Y);

        /// <summary>Текущая скорость (модуль).</summary>
        public double Speed => Math.Sqrt(Vx * Vx + Vy * Vy);

        /// <summary>
        /// Полная механическая энергия (на единицу массы).
        /// Должна сохраняться при отсутствии диссипации - тест точности RK4.
        /// </summary>
        public double SpecificEnergy
        {
            get
            {
                double r = Radius;
                double v2 = Vx * Vx + Vy * Vy;
                return 0.5 * v2 - Mu / r;
            }
        }

        /// <summary>
        /// Тип орбиты по полной энергии.
        /// E < 0 - связанная (эллипс), E = 0 - параболическая, E > 0 - гипербола.
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

        /// <summary>
        /// Первая космическая скорость для текущего радиуса (для круговой орбиты).
        /// </summary>
        public double FirstCosmicSpeed(double radius) => Math.Sqrt(Mu / radius);

        /// <summary>
        /// Вторая космическая скорость для текущего радиуса (для отрыва).
        /// </summary>
        public double SecondCosmicSpeed(double radius) => Math.Sqrt(2.0 * Mu / radius);
    }
}