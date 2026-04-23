namespace ScienceMuseum.Simulation.Solvers
{
    public interface IOdeSolver
    {
        double[] Step(double[] state, System.Func<double, double[], double[]> derivatives,
                      double time, double dt);
    }
}