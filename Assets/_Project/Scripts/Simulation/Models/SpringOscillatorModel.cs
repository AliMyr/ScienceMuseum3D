using System;
using ScienceMuseum.Simulation.Solvers;

namespace ScienceMuseum.Simulation.Models
{
    /// <summary>
    /// Линейный осциллятор с диссипацией: m·x'' + c·x' + k·x = m·g.
    /// Состояние: [x, v], где x отсчитывается от естественной длины пружины
    /// (положение равновесия: x_eq = m·g / k).
    /// </summary>
    public class SpringOscillatorModel
    {
        public double Mass { get; set; } = 1.0;
        public double Stiffness { get; set; } = 50.0;
        public double Damping { get; set; } = 0.0;
        public double Gravity { get; set; } = 9.81;

        public double Position { get; private set; }
        public double Velocity { get; private set; }

        private readonly IOdeSolver _solver;
        private double _time;

        public SpringOscillatorModel(IOdeSolver solver = null)
        {
            _solver = solver ?? new RungeKutta4();
        }

        public void Reset(double initialDisplacementFromEquilibrium,
                          double initialVelocity = 0.0)
        {
            Position = EquilibriumPosition() + initialDisplacementFromEquilibrium;
            Velocity = initialVelocity;
            _time = 0.0;
        }

        public void Step(double dt)
        {
            double[] state = { Position, Velocity };
            double[] newState = _solver.Step(state, Derivatives, _time, dt);
            Position = newState[0];
            Velocity = newState[1];
            _time += dt;
        }

        private double[] Derivatives(double t, double[] state)
        {
            double x = state[0];
            double v = state[1];

            double dx = v;
            double dv = (-Stiffness * x - Damping * v + Mass * Gravity) / Mass;
            return new double[] { dx, dv };
        }

        public double TheoreticalPeriod() => 2.0 * Math.PI * Math.Sqrt(Mass / Stiffness);
        public double TheoreticalFrequency() => 1.0 / TheoreticalPeriod();
        public double EquilibriumPosition() => Mass * Gravity / Stiffness;
        public double DisplacementFromEquilibrium() => Position - EquilibriumPosition();

        public double Energy()
        {
            double xFromEq = DisplacementFromEquilibrium();
            double kinetic = 0.5 * Mass * Velocity * Velocity;
            double springPotential = 0.5 * Stiffness * xFromEq * xFromEq;
            return kinetic + springPotential;
        }
    }
}