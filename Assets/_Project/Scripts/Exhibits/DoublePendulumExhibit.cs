using UnityEngine;
using ScienceMuseum.Core;
using ScienceMuseum.Simulation.Models;
using ScienceMuseum.Simulation.Challenges;

namespace ScienceMuseum.Exhibits
{
    public class DoublePendulumExhibit : ExhibitBase
    {
        [Header("Верхнее звено")]
        [Range(0.3f, 1.5f)][SerializeField] private float length1 = 1.0f;
        [Range(0.5f, 3.0f)][SerializeField] private float mass1 = 1.0f;

        [Header("Нижнее звено")]
        [Range(0.3f, 1.5f)][SerializeField] private float length2 = 1.0f;
        [Range(0.5f, 3.0f)][SerializeField] private float mass2 = 1.0f;

        [Header("Среда")]
        [Range(1f, 25f)][SerializeField] private float gravity = 9.81f;
        [Tooltip("0 = энергия сохраняется")]
        [Range(0f, 1f)][SerializeField] private float damping = 0.0f;

        [Header("Начальные условия")]
        [Range(-180f, 180f)][SerializeField] private float theta1InitialDegrees = 120f;
        [Range(-180f, 180f)][SerializeField] private float theta2InitialDegrees = 90f;

        [Header("Визуал — верхнее звено")]
        [SerializeField] private Transform arm1Transform;
        [SerializeField] private Transform rod1Transform;
        [SerializeField] private Transform bob1Transform;

        [Header("Визуал — нижнее звено")]
        [Tooltip("Должен быть ребёнком arm1 (на одном уровне с bob1Visual)")]
        [SerializeField] private Transform arm2Transform;
        [SerializeField] private Transform rod2Transform;
        [SerializeField] private Transform bob2Transform;

        [Header("След")]
        [Tooltip("Trail Renderer на bob2 — рисует «кружева» двойного маятника")]
        [SerializeField] private TrailRenderer bob2Trail;

        [Header("Симуляция")]
        [Tooltip("Двойной маятник численно жёстче обычного — 8+ для стабильности")]
        [Range(2, 32)][SerializeField] private int subSteps = 8;
        [Range(0.1f, 3f)][SerializeField] private float timeScale = 1.0f;

        private DoublePendulumModel _model;
        private ExhibitParameter[] _parameters;
        private IChallenge[] _challenges;

        public override ExhibitParameter[] Parameters => _parameters;
        public override IChallenge[] Challenges => _challenges;

        public float Length1
        {
            get => length1;
            set
            {
                length1 = Mathf.Clamp(value, 0.3f, 1.5f);
                if (_model != null) _model.Length1 = length1;
                UpdateVisualScaling();
            }
        }

        public float Length2
        {
            get => length2;
            set
            {
                length2 = Mathf.Clamp(value, 0.3f, 1.5f);
                if (_model != null) _model.Length2 = length2;
                UpdateVisualScaling();
            }
        }

        public float Mass1
        {
            get => mass1;
            set
            {
                mass1 = Mathf.Clamp(value, 0.5f, 3.0f);
                if (_model != null) _model.Mass1 = mass1;
            }
        }

        public float Mass2
        {
            get => mass2;
            set
            {
                mass2 = Mathf.Clamp(value, 0.5f, 3.0f);
                if (_model != null) _model.Mass2 = mass2;
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
                damping = Mathf.Clamp(value, 0f, 1f);
                if (_model != null) _model.Damping = damping;
            }
        }

        public float Theta1InitialDegrees
        {
            get => theta1InitialDegrees;
            set => theta1InitialDegrees = Mathf.Clamp(value, -180f, 180f);
        }

        public float Theta2InitialDegrees
        {
            get => theta2InitialDegrees;
            set => theta2InitialDegrees = Mathf.Clamp(value, -180f, 180f);
        }

        public double CurrentEnergy => _model?.Energy() ?? 0.0;
        public double CurrentEnergyDrift => _model?.EnergyDriftRelative ?? 0.0;
        public double MaxTheta2Observed => _model?.MaxObservedTheta2Magnitude ?? 0.0;

        protected override void Awake()
        {
            base.Awake();

            _model = new DoublePendulumModel
            {
                Length1 = length1,
                Length2 = length2,
                Mass1 = mass1,
                Mass2 = mass2,
                Gravity = gravity,
                Damping = damping
            };

            _parameters = new[]
            {
                new ExhibitParameter("Длина L1", "м", 0.3f, 1.5f,
                    () => length1, v => Length1 = v, decimals: 2),
                new ExhibitParameter("Длина L2", "м", 0.3f, 1.5f,
                    () => length2, v => Length2 = v, decimals: 2),
                new ExhibitParameter("Масса m1", "кг", 0.5f, 3.0f,
                    () => mass1, v => Mass1 = v, decimals: 2),
                new ExhibitParameter("Масса m2", "кг", 0.5f, 3.0f,
                    () => mass2, v => Mass2 = v, decimals: 2),
                new ExhibitParameter("Гравитация g", "м/с²", 1f, 25f,
                    () => gravity, v => Gravity = v, decimals: 2),
                new ExhibitParameter("Трение k", "", 0f, 1f,
                    () => damping, v => Damping = v, decimals: 3),
                new ExhibitParameter("Угол θ1", "°", -180f, 180f,
                    () => theta1InitialDegrees, v => Theta1InitialDegrees = v, decimals: 0),
                new ExhibitParameter("Угол θ2", "°", -180f, 180f,
                    () => theta2InitialDegrees, v => Theta2InitialDegrees = v, decimals: 0),
            };

            _challenges = new IChallenge[]
            {
                new SymmetricConfigurationChallenge("double_pendulum.symmetric", this),
                new SmallAnglesChallenge("double_pendulum.small_angles", this),
                new FullFlipChallenge("double_pendulum.flip", this),
            };

            ResetSimulation();
        }

        private void Update()
        {
            if (_model == null) return;

            float dtFrame = Time.deltaTime * timeScale;
            double dt = dtFrame / subSteps;

            for (int i = 0; i < subSteps; i++)
            {
                _model.Step(dt);
            }

            UpdateVisualRotation();
        }

        private void UpdateVisualRotation()
        {
            if (arm1Transform != null)
            {
                float deg1 = (float)(_model.Theta1 * Mathf.Rad2Deg);
                arm1Transform.localRotation = Quaternion.Euler(0f, 0f, deg1);
            }

            if (arm2Transform != null)
            {
                // arm2 — потомок arm1. Чтобы абсолютный угол был theta2,
                // локально нужен theta2 − theta1.
                float relativeDeg = (float)((_model.Theta2 - _model.Theta1) * Mathf.Rad2Deg);
                arm2Transform.localRotation = Quaternion.Euler(0f, 0f, relativeDeg);
            }
        }

        private void UpdateVisualScaling()
        {
            if (rod1Transform != null)
            {
                Vector3 s = rod1Transform.localScale;
                s.y = 0.5f * length1;
                rod1Transform.localScale = s;

                Vector3 p = rod1Transform.localPosition;
                p.y = -length1 / 2f;
                rod1Transform.localPosition = p;
            }

            if (bob1Transform != null)
            {
                Vector3 p = bob1Transform.localPosition;
                p.y = -length1;
                bob1Transform.localPosition = p;
            }

            if (arm2Transform != null)
            {
                Vector3 p = arm2Transform.localPosition;
                p.y = -length1;
                arm2Transform.localPosition = p;
            }

            if (rod2Transform != null)
            {
                Vector3 s = rod2Transform.localScale;
                s.y = 0.5f * length2;
                rod2Transform.localScale = s;

                Vector3 p = rod2Transform.localPosition;
                p.y = -length2 / 2f;
                rod2Transform.localPosition = p;
            }

            if (bob2Transform != null)
            {
                Vector3 p = bob2Transform.localPosition;
                p.y = -length2;
                bob2Transform.localPosition = p;
            }
        }

        public override void ResetSimulation()
        {
            if (_model == null) return;

            _model.Length1 = length1;
            _model.Length2 = length2;
            _model.Mass1 = mass1;
            _model.Mass2 = mass2;
            _model.Gravity = gravity;
            _model.Damping = damping;

            _model.Reset(
                theta1InitialDegrees * Mathf.Deg2Rad,
                theta2InitialDegrees * Mathf.Deg2Rad
            );

            UpdateVisualScaling();
            UpdateVisualRotation();

            if (bob2Trail != null) bob2Trail.Clear();
        }

        private void OnValidate()
        {
            if (Application.isPlaying && _model != null)
            {
                _model.Length1 = length1;
                _model.Length2 = length2;
                _model.Mass1 = mass1;
                _model.Mass2 = mass2;
                _model.Gravity = gravity;
                _model.Damping = damping;
                UpdateVisualScaling();
            }
        }

        public override string GetFormulaText()
        {
            float T1 = 2f * Mathf.PI * Mathf.Sqrt(length1 / gravity);
            float T2 = 2f * Mathf.PI * Mathf.Sqrt(length2 / gravity);
            double E = _model != null ? _model.Energy() : 0.0;
            double drift = _model != null ? _model.EnergyDriftRelative : 0.0;

            return
                "<b>Уравнения движения</b> (из лагранжиана):\n" +
                "  d²θ1/dt² = f1(θ1, θ2, ω1, ω2)\n" +
                "  d²θ2/dt² = f2(θ1, θ2, ω1, ω2)\n" +
                "  Полные нелинейные выражения см. в коде модели.\n\n" +
                "<b>Период малых колебаний</b> (если бы звенья были независимы):\n" +
                $"  T1 = 2π·√(L1/g) = <color=#FFD700>{T1:F3} с</color>\n" +
                $"  T2 = 2π·√(L2/g) = <color=#FFD700>{T2:F3} с</color>\n\n" +
                "<b>Полная энергия системы:</b>\n" +
                $"  E = <color=#FFD700>{E:F4} Дж</color>\n" +
                $"  Дрейф энергии: {drift * 100.0:+0.000;-0.000;0.000}%\n\n" +
                "<i>При больших начальных углах система демонстрирует " +
                "детерминированный хаос — траектория полностью предсказуема, " +
                "но крайне чувствительна к начальным условиям.</i>";
        }
    }
}