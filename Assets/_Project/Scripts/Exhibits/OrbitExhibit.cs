using UnityEngine;
using ScienceMuseum.Core;
using ScienceMuseum.Simulation.Models;
using ScienceMuseum.Simulation.Challenges;
using ScienceMuseum.Managers;

namespace ScienceMuseum.Exhibits
{
    /// <summary>
    /// Экспонат "Планета на орбите вокруг Солнца".
    /// Использует OrbitModel для физики, рисует траекторию через Trail Renderer.
    /// </summary>
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
        [Tooltip("Объект Солнца - центр орбиты")]
        [SerializeField] private Transform sunTransform;

        [Tooltip("Объект Планеты - движется по орбите")]
        [SerializeField] private Transform planetTransform;

        [Tooltip("Trail Renderer для отрисовки следа")]
        [SerializeField] private TrailRenderer planetTrail;

        [Tooltip("Сколько физических подшагов на кадр (для точности)")]
        [Range(1, 16)]
        [SerializeField] private int subSteps = 4;

        [Tooltip("Множитель скорости симуляции (1 = реальное время)")]
        [Range(0.1f, 5f)]
        [SerializeField] private float timeScale = 1f;

        // Физическая модель
        private OrbitModel _model;

        // Параметры и задания
        private ExhibitParameter[] _parameters;
        private IChallenge[] _challenges;
        public override ExhibitParameter[] Parameters => _parameters;
        public override IChallenge[] Challenges => _challenges;

        // Публичные свойства для UI / заданий
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

        /// <summary>Текущий радиус орбиты (м, в условных единицах сцены).</summary>
        public float CurrentRadius => _model != null ? (float)_model.Radius : 0f;

        /// <summary>Текущая скорость планеты.</summary>
        public float CurrentSpeed => _model != null ? (float)_model.Speed : 0f;

        /// <summary>Тип орбиты словами.</summary>
        public string CurrentOrbitType => _model != null ? _model.OrbitType : "—";

        /// <summary>Первая космическая скорость для начального радиуса.</summary>
        public float FirstCosmicAtInit => _model != null
            ? (float)_model.FirstCosmicSpeed(initialRadius)
            : Mathf.Sqrt(mu / initialRadius);

        /// <summary>Вторая космическая скорость для начального радиуса.</summary>
        public float SecondCosmicAtInit => _model != null
            ? (float)_model.SecondCosmicSpeed(initialRadius)
            : Mathf.Sqrt(2f * mu / initialRadius);

        protected override void Awake()
        {
            base.Awake();

            _model = new OrbitModel { Mu = mu };

            _parameters = new[]
            {
                new ExhibitParameter(
                    "Начальный радиус r₀", "ед", 0.3f, 1.0f,
                    () => initialRadius,
                    v => { InitialRadius = v; ResetSimulation(); },
                    decimals: 2),
                new ExhibitParameter(
                    "Начальная скорость v₀", "ед/с", 5f, 30f,
                    () => initialSpeed,
                    v => { InitialSpeed = v; ResetSimulation(); },
                    decimals: 2),
                new ExhibitParameter(
                    "Гравитация Солнца μ", "", 20f, 300f,
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

            // Позиция планеты в локальных координатах относительно Солнца.
            // Орбита в плоскости XZ (горизонтальная плоскость зала).
            Vector3 localPlanetPos = new Vector3(
                (float)_model.X,
                0,
                (float)_model.Y
            );

            // Sun находится в середине платформы; планета смещается в горизонтальной плоскости
            planetTransform.position = sunTransform.position + localPlanetPos;
        }

        public override void ResetSimulation()
        {
            if (_model == null) return;
            _model.Mu = mu;
            _model.Reset(initialRadius, initialSpeed);

            // Сбрасываем след - чтобы старая орбита не оставалась
            if (planetTrail != null)
            {
                planetTrail.Clear();
            }
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

            // Текущий радиус и скорость
            float r = CurrentRadius;
            float v = CurrentSpeed;

            return
                "<b>Закон тяготения Ньютона:</b>\n" +
                "  F = G·M·m / r²\n\n" +
                "<b>Космические скорости</b> (для начального r = " + initialRadius.ToString("F2") + "):\n" +
                $"  1-я (круговая):   v₁ = √(μ/r) = <color=#FFD700>{v1:F2}</color>\n" +
                $"  2-я (отрыв):       v₂ = v₁·√2 = <color=#FFD700>{v2:F2}</color>\n\n" +
                "<b>Текущее состояние:</b>\n" +
                $"  r = {r:F3},  v = {v:F2}\n" +
                $"  Орбита: <color=#FFD700>{CurrentOrbitType}</color>\n\n" +
                "<i>Подбери v₀ для разных типов орбит. " +
                "Меньше v₁ — упадёт. Между v₁ и v₂ — эллипс. " +
                "Больше v₂ — улетит.</i>";
        }

        public override void OnActivate()
        {
            ProgressManager.Instance?.MarkExhibitStudied(ExhibitId);

            var studyPanel = FindObjectOfType<UI.ExhibitStudyPanel>(true);
            if (studyPanel != null)
            {
                studyPanel.Open(this);
            }
        }
    }
}