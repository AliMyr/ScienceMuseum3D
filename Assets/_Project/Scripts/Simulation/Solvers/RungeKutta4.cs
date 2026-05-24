using System;

namespace ScienceMuseum.Simulation.Solvers
{
    /// <summary>
    /// Классический явный метод Рунге–Кутты 4-го порядка.
    /// y_{n+1} = y_n + (dt/6)·(k1 + 2k2 + 2k3 + k4)
    /// </summary>
    public class RungeKutta4 : IOdeSolver
    {
        public double[] Step(double[] state, Func<double, double[], double[]> derivatives,
                             double time, double dt)
        {
            int n = state.Length;

            double[] k1 = derivatives(time, state);

            double[] tempState = new double[n];
            for (int i = 0; i < n; i++)
            {
                tempState[i] = state[i] + 0.5 * dt * k1[i];
            }
            double[] k2 = derivatives(time + 0.5 * dt, tempState);

            for (int i = 0; i < n; i++)
            {
                tempState[i] = state[i] + 0.5 * dt * k2[i];
            }
            double[] k3 = derivatives(time + 0.5 * dt, tempState);

            for (int i = 0; i < n; i++)
            {
                tempState[i] = state[i] + dt * k3[i];
            }
            double[] k4 = derivatives(time + dt, tempState);

            double[] newState = new double[n];
            for (int i = 0; i < n; i++)
            {
                newState[i] = state[i] + (dt / 6.0) * (k1[i] + 2.0 * k2[i] + 2.0 * k3[i] + k4[i]);
            }

            return newState;
        }
    }
}