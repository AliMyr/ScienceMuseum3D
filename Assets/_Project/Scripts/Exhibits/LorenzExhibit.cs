using UnityEngine;
using ScienceMuseum.Core;
using ScienceMuseum.Simulation.Models;
using ScienceMuseum.Simulation.Challenges;
using ScienceMuseum.Managers;

namespace ScienceMuseum.Exhibits
{
    /// <summary>
    /// Экспонат "Аттрактор Лоренца" - детерминированный хаос в 3D.
    /// Точка движется по уравнениям Лоренца, оставляя след-бабочку.
    /// </summary>
    public class LorenzExhibit : ExhibitBase
    {
        [Header("Параметры системы Лоренца")]
        [Range(0.1f, 30f)]
        [SerializeField] private float sigma = 10f;

        [Range(0.1f, 50f)]
        [SerializeField] private float rho = 28f;

        [Range(0.1f, 10f)]
        [SerializeField] private float beta = 8f / 3f;

        [Header("Начальные условия")]
        [SerializeField] private float x0 = 1f;
        [SerializeField] private float y0 = 1f;
        [SerializeField] private float z0 = 1f;

        [Header("Визуал")]
        [Tooltip("Точка-частица, которая летает внутри куба")]
        [SerializeField] private Transform particleTransform;

        [Tooltip("Trail Renderer для отрисовки следа")]
        [SerializeField] private TrailRenderer particleTrail;

        [Tooltip("Куб-контейнер - его центр считается центром координат")]
        [SerializeField] private Transform glassBoxTransform;

        [Header("Параметры визуализации")]
        [Tooltip("Масштаб (1 единица модели = scale метров в сцене)")]
        [Range(0.005f, 0.05f)]
        [SerializeField] private float visualScale = 0.025f;

        [Tooltip("Сколько физических подшагов на кадр")]
        [Range(1, 20)]
        [SerializeField] private int subSteps = 6;

        [Tooltip("Ускорение симуляции (1 = реальное время)")]
        [Range(0.5f, 5f)]
        [SerializeField] private float timeScale = 1.5f;

        // Физическая модель
        private LorenzModel _model;

        // Параметры и задания
        private ExhibitParameter[] _parameters;
        private IChallenge[] _challenges;
        public override ExhibitParameter[] Parameters => _parameters;
        public override IChallenge[] Challenges => _challenges;

        // Публичные свойства
        public float Sigma
        {
            get => sigma;
            set
            {
                sigma = Mathf.Clamp(value, 0.1f, 30f);
                if (_model != null) _model.Sigma = sigma;
            }
        }

        public float Rho
        {
            get => rho;
            set
            {
                rho = Mathf.Clamp(value, 0.1f, 50f);
                if (_model != null) _model.Rho = rho;
            }
        }

        public float Beta
        {
            get => beta;
            set
            {
                beta = Mathf.Clamp(value, 0.1f, 10f);
                if (_model != null) _model.Beta = beta;
            }
        }

        public string CurrentAttractorType => _model != null ? _model.AttractorType : "—";

        protected override void Awake()
        {
            base.Awake();

            _model = new LorenzModel
            {
                Sigma = sigma,
                Rho = rho,
                Beta = beta
            };

            _parameters = new[]
            {
                new ExhibitParameter(
                    "Сигма (sigma)", "", 0.1f, 30f,
                    () => sigma,
                    v => Sigma = v,
                    decimals: 2),
                new ExhibitParameter(
                    "Ро (rho)", "", 0.1f, 50f,
                    () => rho,
                    v => Rho = v,
                    decimals: 2),
                new ExhibitParameter(
                    "Бета (beta)", "", 0.1f, 10f,
                    () => beta,
                    v => Beta = v,
                    decimals: 3),
            };

            _challenges = new IChallenge[]
            {
                new ClassicLorenzChallenge("lorenz.classic", this),
                new StablePointChallenge("lorenz.stable", this),
                new ButterflyEffectChallenge("lorenz.butterfly", this),
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
            if (particleTransform == null || glassBoxTransform == null) return;

            // Координаты Лоренца типично в диапазоне -25..25 для x, y; 0..50 для z.
            // Сдвигаем z вниз чтобы аттрактор был по центру куба.
            float x = (float)_model.X * visualScale;
            float y = ((float)_model.Z - 25f) * visualScale; // z из модели в y сцены, со сдвигом
            float z = (float)_model.Y * visualScale;

            // Локальная позиция относительно куба
            particleTransform.position = glassBoxTransform.position + new Vector3(x, y, z);
        }

        public override void ResetSimulation()
        {
            if (_model == null) return;
            _model.Sigma = sigma;
            _model.Rho = rho;
            _model.Beta = beta;
            _model.Reset(x0, y0, z0);

            if (particleTrail != null)
            {
                particleTrail.Clear();
            }
        }

        private void OnValidate()
        {
            if (Application.isPlaying && _model != null)
            {
                _model.Sigma = sigma;
                _model.Rho = rho;
                _model.Beta = beta;
            }
        }

        public override string GetFormulaText()
        {
            return
                "<b>Уравнения Лоренца:</b>\n" +
                "  dx/dt = sigma · (y - x)\n" +
                "  dy/dt = x · (rho - z) - y\n" +
                "  dz/dt = x · y - beta · z\n\n" +
                "<b>Текущие параметры:</b>\n" +
                $"  sigma = {sigma:F2}\n" +
                $"  rho   = {rho:F2}\n" +
                $"  beta  = {beta:F3}\n\n" +
                $"<b>Режим:</b>  <color=#FFD700>{CurrentAttractorType}</color>\n\n" +
                "<i>Простые уравнения дают сложное поведение. " +
                "Это и есть детерминированный хаос — главное открытие XX века в динамике систем.</i>";
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