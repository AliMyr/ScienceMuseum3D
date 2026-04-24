using System;
using ScienceMuseum.Simulation.Solvers;

namespace ScienceMuseum.Simulation.Models
{
    public class SpringOscillatorModel
    {
        // Параметры
        public double Mass { get; set; } = 1.0;        // масса груза, кг
        public double Stiffness { get; set; } = 50.0;  // k, Н/м
        public double Damping { get; set; } = 0.0;     // c, Н·с/м (вязкое трение)
        public double Gravity { get; set; } = 9.81;    // g, м/с²

        // Состояние
        public double Position { get; private set; }   // x, м (вниз от естественной длины)
        public double Velocity { get; private set; }   // v, м/с

        private readonly IOdeSolver _solver;
        private double _time;

        public SpringOscillatorModel(IOdeSolver solver = null)
        {
            _solver = solver ?? new RungeKutta4();
        }

        public void Reset(double initialDisplacementFromEquilibrium,
                          double initialVelocity = 0.0)
        {
            // Положение равновесия под силой тяжести: x_eq = m·g / k
            double equilibrium = Mass * Gravity / Stiffness;

            // Стартовая позиция относительно естественной длины
            Position = equilibrium + initialDisplacementFromEquilibrium;
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

        public double TheoreticalPeriod()
        {
            return 2.0 * Math.PI * Math.Sqrt(Mass / Stiffness);
        }

        public double TheoreticalFrequency()
        {
            return 1.0 / TheoreticalPeriod();
        }

        public double EquilibriumPosition()
        {
            return Mass * Gravity / Stiffness;
        }

        public double DisplacementFromEquilibrium()
        {
            return Position - EquilibriumPosition();
        }

        public double Energy()
        {
            double xFromEq = DisplacementFromEquilibrium();
            double kinetic = 0.5 * Mass * Velocity * Velocity;
            double springPotential = 0.5 * Stiffness * xFromEq * xFromEq;
            return kinetic + springPotential;
        }
    }
}