using UnityEngine;
using ScienceMuseum.Core;
using ScienceMuseum.Simulation.Models;
using ScienceMuseum.Simulation.Challenges;

namespace ScienceMuseum.Exhibits
{
    public class PendulumExhibit : ExhibitBase
    {
        [Header("Параметры маятника")]
        [Tooltip("Длина нити (метры)")]
        [Range(0.3f, 2.5f)]
        [SerializeField] private float length = 1.0f;

        [Tooltip("Ускорение свободного падения (м/с²). На Земле 9.81")]
        [Range(1f, 25f)]
        [SerializeField] private float gravity = 9.81f;

        [Tooltip("Коэффициент трения (0 = маятник качается вечно)")]
        [Range(0f, 2f)]
        [SerializeField] private float damping = 0.0f;

        [Header("Начальные условия")]
        [Range(-170f, 170f)]
        [SerializeField] private float initialAngleDegrees = 30f;

        [Header("Визуал")]
        [Tooltip("Объект, который поворачивается")]
        [SerializeField] private Transform rotatingPart;

        [Tooltip("Нить (растягивается/сжимается по длине)")]
        [SerializeField] private Transform stringTransform;

        [Tooltip("Груз (смещается по длине нити)")]
        [SerializeField] private Transform bobTransform;

        [Header("Симуляция")]
        [Range(1, 16)]
        [SerializeField] private int subSteps = 4;

        private PendulumModel _model;
        private ExhibitParameter[] _parameters;
        private IChallenge[] _challenges;

        public override ExhibitParameter[] Parameters => _parameters;
        public override IChallenge[] Challenges => _challenges;

        public float Length
        {
            get => length;
            set
            {
                length = Mathf.Clamp(value, 0.3f, 2.5f);
                if (_model != null) _model.Length = length;
                UpdateStringAndBobScale();
            }
        }

        public float Gravity
        {
            get => gravity;
            set
            {
                gravity = Mathf.Clamp(value, 1f, 25f);
                if (_model != null) _model.Gravity = gravity;
            }
        }

        public float Damping
        {
            get => damping;
            set
            {
                damping = Mathf.Clamp(value, 0f, 2f);
                if (_model != null) _model.Damping = damping;
            }
        }

        public float InitialAngleDegrees
        {
            get => initialAngleDegrees;
            set => initialAngleDegrees = Mathf.Clamp(value, -170f, 170f);
        }

        protected override void Awake()
        {
            base.Awake();

            _model = new PendulumModel
            {
                Length = length,
                Gravity = gravity,
                Damping = damping
            };

            _challenges = new IChallenge[]
            {
                new TargetPeriodChallenge("pendulum.period_2sec", this, 2.0f, 0.05f),
                new TargetPeriodChallenge("pendulum.period_1sec", this, 1.0f, 0.05f,
                    title: "Быстрый маятник",
                    description: "Сделай так, чтобы маятник совершал одно колебание за 1 секунду."),
                new MatchGravityChallenge("pendulum.gravity_moon", this, 1.62f, "Луне"),
                new MatchGravityChallenge("pendulum.gravity_mars", this, 3.71f, "Марсе"),
            };

            _parameters = new[]
            {
                new ExhibitParameter("Длина нити L", "м", 0.3f, 2.5f,
                    () => length, v => Length = v, decimals: 2),
                new ExhibitParameter("Гравитация g", "м/с²", 1f, 25f,
                    () => gravity, v => Gravity = v, decimals: 2),
                new ExhibitParameter("Трение k", "", 0f, 2f,
                    () => damping, v => Damping = v, decimals: 2),
                new ExhibitParameter("Начальный угол θ", "°", -170f, 170f,
                    () => initialAngleDegrees, v => InitialAngleDegrees = v, decimals: 0),
            };

            ResetSimulation();
        }

        private void Update()
        {
            if (_model == null) return;

            double dt = Time.deltaTime / subSteps;
            for (int i = 0; i < subSteps; i++)
            {
                _model.Step(dt);
            }

            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (rotatingPart != null)
            {
                float angleDeg = (float)(_model.Angle * Mathf.Rad2Deg);
                rotatingPart.localRotation = Quaternion.Euler(0, 0, angleDeg);
            }

            UpdateStringAndBobScale();
        }

        private void UpdateStringAndBobScale()
        {
            if (stringTransform != null)
            {
                Vector3 s = stringTransform.localScale;
                s.y = 0.5f * length;
                stringTransform.localScale = s;

                Vector3 p = stringTransform.localPosition;
                p.y = -length / 2f;
                stringTransform.localPosition = p;
            }

            if (bobTransform != null)
            {
                Vector3 p = bobTransform.localPosition;
                p.y = -length;
                bobTransform.localPosition = p;
            }
        }

        public override void ResetSimulation()
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

        public override string GetFormulaText()
        {
            float period = 2f * Mathf.PI * Mathf.Sqrt(length / gravity);
            float frequency = 1f / period;

            return
                "<b>Формула периода малых колебаний:</b>\n" +
                $"  T = 2π·√(L/g) = 2π·√({length:F2}/{gravity:F2})\n" +
                $"  T = <color=#FFD700>{period:F3} с</color>\n\n" +
                $"<b>Частота:</b>  f = 1/T = <color=#FFD700>{frequency:F3} Гц</color>\n\n" +
                "<i>Формула работает для малых углов (до ~15°). " +
                "При больших углах реальный период больше.</i>";
        }
    }
}