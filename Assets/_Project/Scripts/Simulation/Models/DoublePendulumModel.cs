using System;
using ScienceMuseum.Simulation.Solvers;

namespace ScienceMuseum.Simulation.Models
{
    /// <summary>
    /// Двойной математический маятник — две точечные массы на невесомых стержнях,
    /// соединённых шарниром. Классический пример детерминированного хаоса в механике.
    ///
    /// Состояние: [theta1, theta2, omega1, omega2]
    ///   theta1 — угол верхнего звена от вертикали (0 = висит вниз)
    ///   theta2 — угол нижнего звена от вертикали
    ///   omega1, omega2 — угловые скорости
    ///
    /// Уравнения движения получены из лагранжиана и приведены к стандартной форме
    /// d/dt(omega) = ... (см. учебники по аналитической механике).
    /// </summary>
    public class DoublePendulumModel
    {
        // ── Параметры (можно менять в рантайме) ─────────────────────────────

        public double Length1 { get; set; } = 1.0;     // длина верхнего стержня, м
        public double Length2 { get; set; } = 1.0;     // длина нижнего стержня, м
        public double Mass1 { get; set; } = 1.0;       // масса верхнего груза, кг
        public double Mass2 { get; set; } = 1.0;       // масса нижнего груза, кг
        public double Gravity { get; set; } = 9.81;    // ускорение свободного падения, м/с²
        public double Damping { get; set; } = 0.0;     // коэффициент вязкого трения

        // ── Состояние ───────────────────────────────────────────────────────

        public double Theta1 { get; private set; }
        public double Theta2 { get; private set; }
        public double Omega1 { get; private set; }
        public double Omega2 { get; private set; }

        /// <summary>
        /// Максимальное значение |theta2|, наблюдавшееся с момента последнего Reset.
        /// Понадобится для задачи «полный кувырок нижнего груза».
        /// </summary>
        public double MaxObservedTheta2Magnitude { get; private set; }

        private readonly IOdeSolver _solver;
        private double _time;
        private double _initialEnergy;

        public DoublePendulumModel(IOdeSolver solver = null)
        {
            _solver = solver ?? new RungeKutta4();
        }

        public void Reset(double theta1Initial, double theta2Initial,
                          double omega1Initial = 0.0, double omega2Initial = 0.0)
        {
            Theta1 = theta1Initial;
            Theta2 = theta2Initial;
            Omega1 = omega1Initial;
            Omega2 = omega2Initial;
            _time = 0.0;
            MaxObservedTheta2Magnitude = Math.Abs(Theta2);
            _initialEnergy = Energy();
        }

        public void Step(double dt)
        {
            double[] state = { Theta1, Theta2, Omega1, Omega2 };
            double[] newState = _solver.Step(state, Derivatives, _time, dt);

            Theta1 = newState[0];
            Theta2 = newState[1];
            Omega1 = newState[2];
            Omega2 = newState[3];
            _time += dt;

            double abs2 = Math.Abs(Theta2);
            if (abs2 > MaxObservedTheta2Magnitude)
            {
                MaxObservedTheta2Magnitude = abs2;
            }
        }

        private double[] Derivatives(double t, double[] state)
        {
            double th1 = state[0];
            double th2 = state[1];
            double w1 = state[2];
            double w2 = state[3];

            double m1 = Mass1;
            double m2 = Mass2;
            double L1 = Length1;
            double L2 = Length2;
            double g = Gravity;

            double delta = th1 - th2;
            double sinDelta = Math.Sin(delta);
            double cosDelta = Math.Cos(delta);

            // Общий знаменатель уравнений (физически никогда не обращается в ноль)
            double denom = 2.0 * m1 + m2 - m2 * Math.Cos(2.0 * delta);

            // d(omega1)/dt
            double num1 =
                -g * (2.0 * m1 + m2) * Math.Sin(th1)
                - m2 * g * Math.Sin(th1 - 2.0 * th2)
                - 2.0 * sinDelta * m2 * (w2 * w2 * L2 + w1 * w1 * L1 * cosDelta);
            double dOmega1 = num1 / (L1 * denom) - Damping * w1;

            // d(omega2)/dt
            double num2 =
                2.0 * sinDelta * (w1 * w1 * L1 * (m1 + m2)
                                  + g * (m1 + m2) * Math.Cos(th1)
                                  + w2 * w2 * L2 * m2 * cosDelta);
            double dOmega2 = num2 / (L2 * denom) - Damping * w2;

            // d(theta)/dt = omega
            return new double[] { w1, w2, dOmega1, dOmega2 };
        }

        /// <summary>
        /// Полная механическая энергия (кинетическая + потенциальная).
        /// При damping = 0 должна сохраняться — индикатор корректности интегратора.
        /// Потенциальная отсчитывается от нижнего положения покоя (минимум = 0).
        /// </summary>
        public double Energy()
        {
            double m1 = Mass1;
            double m2 = Mass2;
            double L1 = Length1;
            double L2 = Length2;
            double g = Gravity;

            double v1Squared = L1 * L1 * Omega1 * Omega1;
            double v2Squared = L1 * L1 * Omega1 * Omega1
                             + L2 * L2 * Omega2 * Omega2
                             + 2.0 * L1 * L2 * Omega1 * Omega2 * Math.Cos(Theta1 - Theta2);

            double kinetic = 0.5 * m1 * v1Squared + 0.5 * m2 * v2Squared;

            double potential = (m1 + m2) * g * L1 * (1.0 - Math.Cos(Theta1))
                             + m2 * g * L2 * (1.0 - Math.Cos(Theta2));

            return kinetic + potential;
        }

        /// <summary>
        /// Относительный дрейф энергии относительно начальной. При корректном
        /// интегрировании без диссипации должен быть малым (порядка 1e-3 на длинных
        /// интервалах для RK4 с разумным шагом).
        /// </summary>
        public double EnergyDriftRelative
        {
            get
            {
                if (Math.Abs(_initialEnergy) < 1e-12) return 0.0;
                return (Energy() - _initialEnergy) / _initialEnergy;
            }
        }
    }
}