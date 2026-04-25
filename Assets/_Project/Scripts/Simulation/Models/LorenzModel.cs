using System;
using ScienceMuseum.Simulation.Solvers;

namespace ScienceMuseum.Simulation.Models
{
    /// <summary>
    /// Аттрактор Лоренца - детерминированная хаотическая система.
    /// Описывается тремя ОДУ:
    ///   dx/dt = sigma·(y - x)
    ///   dy/dt = x·(rho - z) - y
    ///   dz/dt = x·y - beta·z
    /// Состояние: [x, y, z] - координаты точки в 3D.
    /// </summary>
    public class LorenzModel
    {
        // Параметры (классические значения дают "бабочку")
        public double Sigma { get; set; } = 10.0;
        public double Rho { get; set; } = 28.0;
        public double Beta { get; set; } = 8.0 / 3.0;

        // Состояние
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Z { get; private set; }

        private readonly IOdeSolver _solver;
        private double _time;

        public LorenzModel(IOdeSolver solver = null)
        {
            _solver = solver ?? new RungeKutta4();
        }

        public void Reset(double x0 = 1.0, double y0 = 1.0, double z0 = 1.0)
        {
            X = x0;
            Y = y0;
            Z = z0;
            _time = 0;
        }

        public void Step(double dt)
        {
            double[] state = { X, Y, Z };
            double[] newState = _solver.Step(state, Derivatives, _time, dt);
            X = newState[0];
            Y = newState[1];
            Z = newState[2];
            _time += dt;
        }

        private double[] Derivatives(double t, double[] state)
        {
            double x = state[0];
            double y = state[1];
            double z = state[2];

            double dx = Sigma * (y - x);
            double dy = x * (Rho - z) - y;
            double dz = x * y - Beta * z;

            return new double[] { dx, dy, dz };
        }

        /// <summary>
        /// Тип аттрактора в зависимости от параметра rho.
        /// При rho < 1 - точка приходит в равновесие (0,0,0).
        /// При 1 < rho < ~24.7 - две стабильные точки равновесия.
        /// При rho > 24.7 - хаотический режим (бабочка Лоренца).
        /// </summary>
        public string AttractorType
        {
            get
            {
                if (Rho < 1.0) return "точка покоя";
                if (Rho < 24.74) return "стабильное равновесие";
                return "хаос (бабочка Лоренца)";
            }
        }
    }
}