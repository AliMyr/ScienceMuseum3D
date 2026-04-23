using UnityEngine;
using ScienceMuseum.Core;
using ScienceMuseum.Simulation.Models;

namespace ScienceMuseum.Exhibits
{
    public class PendulumExhibit : ExhibitBase
    {
        [Header("Параметры маятника")]
        [Tooltip("Длина нити (метры). ВАЖНО: визуал в сцене должен совпадать.")]
        [Range(0.3f, 2.5f)]
        [SerializeField] private float length = 1.0f;

        [Tooltip("Ускорение свободного падения (м/с²). На Земле 9.81")]
        [Range(1f, 25f)]
        [SerializeField] private float gravity = 9.81f;

        [Tooltip("Коэффициент трения (0 = нет, маятник качается вечно)")]
        [Range(0f, 2f)]
        [SerializeField] private float damping = 0.0f;

        [Header("Начальные условия")]
        [Tooltip("Начальный угол отклонения (градусы)")]
        [Range(-170f, 170f)]
        [SerializeField] private float initialAngleDegrees = 30f;

        [Header("Визуал")]
        [Tooltip("Объект, который будет поворачиваться (StringAndBob)")]
        [SerializeField] private Transform rotatingPart;

        [Tooltip("Трансформ нити (чтобы растягивать/сжимать при изменении длины)")]
        [SerializeField] private Transform stringTransform;

        [Tooltip("Трансформ груза (для смещения по длине нити)")]
        [SerializeField] private Transform bobTransform;

        // Физическая модель
        private PendulumModel _model;

        // Флаг - запущена ли симуляция
        private bool _isRunning = false;

        protected override void Awake()
        {
            base.Awake();  // базовый класс настраивает подсветку

            _model = new PendulumModel
            {
                Length = length,
                Gravity = gravity,
                Damping = damping
            };

            ResetSimulation();
        }

        private void Start()
        {
            // Запускаем симуляцию сразу - маятник качается независимо от взаимодействия.
            // Даже проходя мимо, игрок видит живую физику - это привлекает внимание.
            _isRunning = true;
        }

        private void Update()
        {
            if (!_isRunning) return;

            // Шаг симуляции с фиксированным малым шагом dt.
            // Делаем несколько подшагов на кадр - для стабильности при низком FPS.
            int subSteps = 4;
            double dt = Time.deltaTime / subSteps;

            for (int i = 0; i < subSteps; i++)
            {
                _model.Step(dt);
            }

            // Применяем угол к визуалу
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (rotatingPart != null)
            {
                // Угол из модели (радианы) в градусы
                float angleDeg = (float)(_model.Angle * Mathf.Rad2Deg);

                // Поворачиваем вокруг Z (ось, перпендикулярная плоскости качания)
                rotatingPart.localRotation = Quaternion.Euler(0, 0, angleDeg);
            }

            UpdateStringAndBobScale();
        }

        private void UpdateStringAndBobScale()
        {
            if (stringTransform != null)
            {
                Vector3 s = stringTransform.localScale;
                s.y = 0.5f * length;  // стандартная шкала 0.5 соответствует длине 1м
                stringTransform.localScale = s;

                // Центр нити - посередине её длины, то есть на -length/2 от точки подвеса
                Vector3 p = stringTransform.localPosition;
                p.y = -length / 2f;
                stringTransform.localPosition = p;
            }

            if (bobTransform != null)
            {
                Vector3 p = bobTransform.localPosition;
                p.y = -length;  // груз на конце нити
                bobTransform.localPosition = p;
            }
        }

        public void ResetSimulation()
        {
            if (_model == null) return;

            _model.Length = length;
            _model.Gravity = gravity;
            _model.Damping = damping;
            _model.Reset(initialAngleDegrees * Mathf.Deg2Rad);

            UpdateVisual();
        }

        private void OnValidate()
        {
            if (Application.isPlaying && _model != null)
            {
                _model.Length = length;
                _model.Gravity = gravity;
                _model.Damping = damping;
                UpdateStringAndBobScale();
            }
        }

        // Реализация абстрактного метода ExhibitBase
        public override void OnActivate()
        {
            // Пока просто выводим информацию.
            // В следующей итерации здесь будет открытие UI-панели изучения.
            Debug.Log($"[Pendulum] Активирован. L={length}m, g={gravity}, угол={initialAngleDegrees}°");
            Debug.Log($"[Pendulum] Теоретический период T = 2π√(L/g) = {_model.TheoreticalPeriod():F3} с");
        }
    }
}