using UnityEngine;
using ScienceMuseum.Core;
using ScienceMuseum.Simulation.Models;
using ScienceMuseum.Simulation.Challenges;

namespace ScienceMuseum.Exhibits
{
    public class OrbitExhibit : ExhibitBase
    {
        [Header("Параметры орбиты")]
        [Tooltip("Гравитационный параметр Солнца (G·M)")]
        [Range(20f, 300f)]
        [SerializeField] private float mu = 100f;

        [Tooltip("Начальный радиус орбиты")]
        [Range(0.3f, 1.0f)]
        [SerializeField] private float initialRadius = 0.5f;

        [Tooltip("Начальная скорость (по касательной к радиусу)")]
        [Range(5f, 30f)]
        [SerializeField] private float initialSpeed = 14f;

        [Header("Визуал")]
        [SerializeField] private Transform sunTransform;
        [SerializeField] private Transform planetTransform;
        [SerializeField] private TrailRenderer planetTrail;

        [Header("Симуляция")]
        [Range(1, 16)]
        [SerializeField] private int subSteps = 4;

        [Range(0.1f, 5f)]
        [SerializeField] private float timeScale = 1f;

        private OrbitModel _model;
        private ExhibitParameter[] _parameters;
        private IChallenge[] _challenges;

        public override ExhibitParameter[] Parameters => _parameters;
        public override IChallenge[] Challenges => _challenges;

        public float InitialRadius
        {
            get => initialRadius;
            set => initialRadius = Mathf.Clamp(value, 0.3f, 1.0f);
        }

        public float InitialSpeed
        {
            get => initialSpeed;
            set => initialSpeed = Mathf.Clamp(value, 5f, 30f);
        }

        public float Mu
        {
            get => mu;
            set
            {
                mu = Mathf.Clamp(value, 20f, 300f);
                if (_model != null) _model.Mu = mu;
            }
        }

        public float CurrentRadius => _model != null ? (float)_model.Radius : 0f;
        public float CurrentSpeed => _model != null ? (float)_model.Speed : 0f;
        public string CurrentOrbitType => _model != null ? _model.OrbitType : "—";

        public float FirstCosmicAtInit => _model != null
            ? (float)_model.FirstCosmicSpeed(initialRadius)
            : Mathf.Sqrt(mu / initialRadius);

        public float SecondCosmicAtInit => _model != null
            ? (float)_model.SecondCosmicSpeed(initialRadius)
            : Mathf.Sqrt(2f * mu / initialRadius);

        protected override void Awake()
        {
            base.Awake();

            _model = new OrbitModel { Mu = mu };

            _parameters = new[]
            {
                new ExhibitParameter("Начальный радиус r0", "ед", 0.3f, 1.0f,
                    () => initialRadius,
                    v => { InitialRadius = v; ResetSimulation(); },
                    decimals: 2),
                new ExhibitParameter("Начальная скорость v0", "ед/с", 5f, 30f,
                    () => initialSpeed,
                    v => { InitialSpeed = v; ResetSimulation(); },
                    decimals: 2),
                new ExhibitParameter("Гравитация Солнца mu", "", 20f, 300f,
                    () => mu,
                    v => { Mu = v; ResetSimulation(); },
                    decimals: 1),
            };

            _challenges = new IChallenge[]
            {
                new CircularOrbitChallenge("orbit.circular", this),
                new EllipticalOrbitChallenge("orbit.elliptical", this),
                new EscapeVelocityChallenge("orbit.escape", this),
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

            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (planetTransform == null || sunTransform == null) return;

            Vector3 localPlanetPos = new Vector3((float)_model.X, 0, (float)_model.Y);
            planetTransform.position = sunTransform.position + localPlanetPos;
        }

        public override void ResetSimulation()
        {
            if (_model == null) return;
            _model.Mu = mu;
            _model.Reset(initialRadius, initialSpeed);

            if (planetTrail != null) planetTrail.Clear();
        }

        private void OnValidate()
        {
            if (Application.isPlaying && _model != null)
            {
                _model.Mu = mu;
            }
        }

        public override string GetFormulaText()
        {
            float v1 = FirstCosmicAtInit;
            float v2 = SecondCosmicAtInit;

            return
                "<b>Закон тяготения Ньютона:</b>\n" +
                "  F = G·M·m / r²\n\n" +
                $"<b>Космические скорости</b> (для r = {initialRadius:F2}):\n" +
                $"  Круговая:   v1 = sqrt(mu/r) = <color=#FFD700>{v1:F2}</color>\n" +
                $"  Отрыва:     v2 = v1 · sqrt(2) = <color=#FFD700>{v2:F2}</color>\n\n" +
                "<b>Текущее состояние:</b>\n" +
                $"  r = {CurrentRadius:F3},  v = {CurrentSpeed:F2}\n" +
                $"  Орбита: <color=#FFD700>{CurrentOrbitType}</color>\n\n" +
                "<i>Меньше круговой — упадёт. Между круговой и отрыва — эллипс. " +
                "Больше отрыва — улетит.</i>";
        }
    }
}