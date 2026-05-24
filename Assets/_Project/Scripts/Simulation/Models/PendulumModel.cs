using System;
using ScienceMuseum.Simulation.Solvers;

namespace ScienceMuseum.Simulation.Models
{
    /// <summary>
    /// Математический маятник: θ'' = -(g/L)·sin(θ) - k·θ'.
    /// Состояние: [θ, ω].
    /// </summary>
    public class PendulumModel
    {
        public double Length { get; set; } = 1.0;
        public double Gravity { get; set; } = 9.81;
        public double Damping { get; set; } = 0.0;

        public double Angle { get; private set; }
        public double AngularVelocity { get; private set; }

        private readonly IOdeSolver _solver;
        private double _time;

        public PendulumModel(IOdeSolver solver = null)
        {
            _solver = solver ?? new RungeKutta4();
        }

        public void Reset(double initialAngle, double initialAngularVelocity = 0.0)
        {
            Angle = initialAngle;
            AngularVelocity = initialAngularVelocity;
            _time = 0.0;
        }

        public void Step(double dt)
        {
            double[] state = { Angle, AngularVelocity };
            double[] newState = _solver.Step(state, Derivatives, _time, dt);

            Angle = newState[0];
            AngularVelocity = newState[1];
            _time += dt;
        }

        private double[] Derivatives(double t, double[] state)
        {
            double theta = state[0];
            double omega = state[1];

            double dTheta = omega;
            double dOmega = -(Gravity / Length) * Math.Sin(theta) - Damping * omega;

            return new double[] { dTheta, dOmega };
        }

        public double TheoreticalPeriod() => 2.0 * Math.PI * Math.Sqrt(Length / Gravity);

        public double Energy()
        {
            double kinetic = 0.5 * Length * Length * AngularVelocity * AngularVelocity;
            double potential = Gravity * Length * (1.0 - Math.Cos(Angle));
            return kinetic + potential;
        }
    }
}