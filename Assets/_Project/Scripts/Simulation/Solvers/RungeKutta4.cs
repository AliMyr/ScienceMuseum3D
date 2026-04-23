using System;

namespace ScienceMuseum.Simulation.Solvers
{
    public class RungeKutta4 : IOdeSolver
    {
        public double[] Step(double[] state, Func<double, double[], double[]> derivatives,
                             double time, double dt)
        {
            int n = state.Length;

            // k1 = f(t, y)
            double[] k1 = derivatives(time, state);

            // k2 = f(t + dt/2, y + dt/2 · k1)
            double[] tempState = new double[n];
            for (int i = 0; i < n; i++)
            {
                tempState[i] = state[i] + 0.5 * dt * k1[i];
            }
            double[] k2 = derivatives(time + 0.5 * dt, tempState);

            // k3 = f(t + dt/2, y + dt/2 · k2)
            for (int i = 0; i < n; i++)
            {
                tempState[i] = state[i] + 0.5 * dt * k2[i];
            }
            double[] k3 = derivatives(time + 0.5 * dt, tempState);

            // k4 = f(t + dt, y + dt · k3)
            for (int i = 0; i < n; i++)
            {
                tempState[i] = state[i] + dt * k3[i];
            }
            double[] k4 = derivatives(time + dt, tempState);

            // y_new = y + (dt/6) · (k1 + 2·k2 + 2·k3 + k4)
            double[] newState = new double[n];
            for (int i = 0; i < n; i++)
            {
                newState[i] = state[i] + (dt / 6.0) * (k1[i] + 2.0 * k2[i] + 2.0 * k3[i] + k4[i]);
            }

            return newState;
        }
    }
}