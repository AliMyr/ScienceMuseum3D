using UnityEngine;
using ScienceMuseum.Simulation.Models;

namespace ScienceMuseum.Tests
{
    /// <summary>
    /// Тестовый скрипт - проверяет что модель маятника работает правильно.
    /// Выводит результаты в Console.
    /// Повесь на любой объект сцены, нажми правую кнопку на компоненте -> Run Test.
    /// После проверки этот файл можно удалить.
    /// </summary>
    public class PendulumPhysicsTest : MonoBehaviour
    {
        [ContextMenu("Run Test: Small Oscillations")]
        private void TestSmallOscillations()
        {
            Debug.Log("=== Тест: малые колебания ===");

            var model = new PendulumModel
            {
                Length = 1.0,
                Gravity = 9.81,
                Damping = 0.0
            };

            // Начальный угол 5° - это "малые колебания", формула T = 2π√(L/g) точна
            model.Reset(initialAngle: Mathf.Deg2Rad * 5f);

            double theoreticalT = model.TheoreticalPeriod();
            Debug.Log($"Теоретический период: T = {theoreticalT:F4} с");

            // Симулируем 10 секунд с шагом dt = 0.001
            double dt = 0.001;
            double totalTime = 10.0;
            int steps = (int)(totalTime / dt);

            double energyStart = model.Energy();

            // Найдём период экспериментально - время между двумя переходами через 0
            // в одну сторону (с положительной производной)
            double prevAngle = model.Angle;
            double firstCross = -1;
            double secondCross = -1;

            for (int i = 0; i < steps; i++)
            {
                model.Step(dt);
                double t = (i + 1) * dt;

                // Переход через 0 снизу вверх (ω > 0)
                if (prevAngle < 0 && model.Angle >= 0 && model.AngularVelocity > 0)
                {
                    if (firstCross < 0) firstCross = t;
                    else if (secondCross < 0) { secondCross = t; break; }
                }
                prevAngle = model.Angle;
            }

            double energyEnd = model.Energy();

            if (firstCross > 0 && secondCross > 0)
            {
                double experimentalT = secondCross - firstCross;
                double error = Mathf.Abs((float)(experimentalT - theoreticalT)) / (float)theoreticalT * 100f;
                Debug.Log($"Экспериментальный период: T = {experimentalT:F4} с");
                Debug.Log($"Расхождение: {error:F2} %");
            }

            double energyDrift = Mathf.Abs((float)(energyEnd - energyStart)) / (float)energyStart * 100f;
            Debug.Log($"Энергия в начале: {energyStart:F6}");
            Debug.Log($"Энергия в конце:  {energyEnd:F6}");
            Debug.Log($"Дрейф энергии:    {energyDrift:F6} %  (должно быть близко к 0)");
        }

        [ContextMenu("Run Test: Large Oscillations")]
        private void TestLargeOscillations()
        {
            Debug.Log("=== Тест: большие колебания (90°) ===");

            var model = new PendulumModel { Length = 1.0 };
            model.Reset(initialAngle: Mathf.Deg2Rad * 90f);

            double theoreticalT = model.TheoreticalPeriod();
            Debug.Log($"Теоретический период (формула малых колебаний): T = {theoreticalT:F4} с");
            Debug.Log("Для 90° реальный период БОЛЬШЕ теоретического примерно на 18%.");

            // Та же схема поиска периода
            double dt = 0.001;
            double prevAngle = model.Angle;
            double firstMax = -1, secondMax = -1;

            for (int i = 0; i < 20000; i++)
            {
                double oldOmega = model.AngularVelocity;
                model.Step(dt);
                double t = (i + 1) * dt;

                // Момент максимума - когда угловая скорость меняет знак с + на -
                if (oldOmega > 0 && model.AngularVelocity <= 0)
                {
                    if (firstMax < 0) firstMax = t;
                    else if (secondMax < 0) { secondMax = t; break; }
                }
            }

            if (firstMax > 0 && secondMax > 0)
            {
                double experimentalT = secondMax - firstMax;
                Debug.Log($"Экспериментальный период: T = {experimentalT:F4} с");
                Debug.Log($"Превышение над формулой: {((experimentalT / theoreticalT - 1) * 100):F2} %");
            }
        }
    }
}