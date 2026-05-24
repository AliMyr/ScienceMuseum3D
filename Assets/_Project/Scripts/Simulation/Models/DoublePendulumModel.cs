using System;
using ScienceMuseum.Simulation.Solvers;

namespace ScienceMuseum.Simulation.Models
{
    /// <summary>
    /// Двойной маятник: две точечные массы на невесомых стержнях, соединённых шарниром.
    /// Классический пример детерминированного хаоса в механике.
    /// Состояние: [theta1, theta2, omega1, omega2].
    /// Уравнения движения получены из лагранжиана (стандартная форма).
    /// </summary>
    public class DoublePendulumModel
    {
        public double Length1 { get; set; } = 1.0;
        public double Length2 { get; set; } = 1.0;
        public double Mass1 { get; set; } = 1.0;
        public double Mass2 { get; set; } = 1.0;
        public double Gravity { get; set; } = 9.81;
        public double Damping { get; set; } = 0.0;

        public double Theta1 { get; private set; }
        public double Theta2 { get; private set; }
        public double Omega1 { get; private set; }
        public double Omega2 { get; private set; }

        /// <summary>
        /// Максимальное наблюдённое |theta2| с момента последнего Reset.
        /// Используется для задачи «полный кувырок нижнего груза».
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

            // Общий знаменатель уравнений; физически не обращается в ноль
            double denom = 2.0 * m1 + m2 - m2 * Math.Cos(2.0 * delta);

            double num1 =
                -g * (2.0 * m1 + m2) * Math.Sin(th1)
                - m2 * g * Math.Sin(th1 - 2.0 * th2)
                - 2.0 * sinDelta * m2 * (w2 * w2 * L2 + w1 * w1 * L1 * cosDelta);
            double dOmega1 = num1 / (L1 * denom) - Damping * w1;

            double num2 =
                2.0 * sinDelta * (w1 * w1 * L1 * (m1 + m2)
                                  + g * (m1 + m2) * Math.Cos(th1)
                                  + w2 * w2 * L2 * m2 * cosDelta);
            double dOmega2 = num2 / (L2 * denom) - Damping * w2;

            return new double[] { w1, w2, dOmega1, dOmega2 };
        }

        /// <summary>
        /// Полная механическая энергия (кинетическая + потенциальная).
        /// При damping=0 должна сохраняться — индикатор корректности интегратора.
        /// Потенциальная отсчитывается от нижнего положения покоя.
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
        /// Относительный дрейф энергии. Для RK4 без диссипации мал (~1e-3 на длинных интервалах).
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