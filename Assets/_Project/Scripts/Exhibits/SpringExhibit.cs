using UnityEngine;
using ScienceMuseum.Core;
using ScienceMuseum.Simulation.Models;
using ScienceMuseum.Simulation.Challenges;
using ScienceMuseum.Managers;

namespace ScienceMuseum.Exhibits
{
    public class SpringExhibit : ExhibitBase
    {
        [Header("Параметры системы")]
        [Tooltip("Масса груза, кг")]
        [Range(0.1f, 10f)]
        [SerializeField] private float mass = 1.0f;

        [Tooltip("Жёсткость пружины, Н/м")]
        [Range(5f, 200f)]
        [SerializeField] private float stiffness = 50.0f;

        [Tooltip("Коэффициент трения")]
        [Range(0f, 5f)]
        [SerializeField] private float damping = 0.0f;

        [Tooltip("Гравитация, м/с²")]
        [Range(0f, 25f)]
        [SerializeField] private float gravity = 9.81f;

        [Header("Начальные условия")]
        [Tooltip("Начальное смещение от положения равновесия, метры")]
        [Range(-0.5f, 0.5f)]
        [SerializeField] private float initialDisplacement = 0.2f;

        [Header("Визуал")]
        [Tooltip("Line Renderer для отрисовки пружины")]
        [SerializeField] private LineRenderer springLine;

        [Tooltip("Точка крепления пружины сверху (якорь)")]
        [SerializeField] private Transform anchorPoint;

        [Tooltip("Груз - перемещается при колебаниях")]
        [SerializeField] private Transform bobTransform;

        [Header("Параметры пружины (визуал)")]
        [Tooltip("Количество витков в пружине")]
        [Range(5, 30)]
        [SerializeField] private int coilCount = 15;

        [Tooltip("Точек на виток - плавность")]
        [Range(8, 30)]
        [SerializeField] private int pointsPerCoil = 12;

        [Tooltip("Радиус витков")]
        [SerializeField] private float coilRadius = 0.08f;

        // Физическая модель
        private SpringOscillatorModel _model;

        // Задания
        private IChallenge[] _challenges;
        private ExhibitParameter[] _parameters;

        public override IChallenge[] Challenges => _challenges;
        public override ExhibitParameter[] Parameters => _parameters;

        protected override void Awake()
        {
            base.Awake();

            _model = new SpringOscillatorModel
            {
                Mass = mass,
                Stiffness = stiffness,
                Damping = damping,
                Gravity = gravity
            };

            _challenges = new IChallenge[]
            {
                new SpringTargetPeriodChallenge("spring.period_1sec", this, 1.0f, 0.05f,
                    "Период = 1 секунда"),
                new SpringTargetPeriodChallenge("spring.period_2sec", this, 2.0f, 0.05f,
                    "Медленные колебания (T = 2 с)"),
                new HeavyMassChallenge("spring.heavy_mass", this),
                new ShowDampingChallenge("spring.damping", this),
            };

            _parameters = new[]
            {
                new ExhibitParameter(
                    "Масса m", "кг", 0.1f, 10f,
                    () => mass,
                    v => Mass = v,
                    decimals: 2),
                new ExhibitParameter(
                    "Жёсткость k", "Н/м", 5f, 200f,
                    () => stiffness,
                    v => Stiffness = v,
                    decimals: 1),
                new ExhibitParameter(
                    "Трение c", "", 0f, 5f,
                    () => damping,
                    v => Damping = v,
                    decimals: 2),
                new ExhibitParameter(
                    "Начальное смещение x₀", "м", -0.5f, 0.5f,
                    () => initialDisplacement,
                    v => InitialDisplacement = v,
                    decimals: 2),
            };

            ResetSimulation();
        }

        private void Start()
        {
            UpdateSpringLine();
        }

        private void Update()
        {
            // Физический шаг с подшагами
            int subSteps = 4;
            double dt = Time.deltaTime / subSteps;
            for (int i = 0; i < subSteps; i++)
            {
                _model.Step(dt);
            }

            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (bobTransform == null || anchorPoint == null) return;

            // Груз движется вниз от anchor на величину Position
            float bobY = -(float)_model.Position;
            bobTransform.localPosition = new Vector3(0, bobY, 0);

            UpdateSpringLine();
        }

        private void UpdateSpringLine()
        {
            if (springLine == null || bobTransform == null) return;

            float springLength = -bobTransform.localPosition.y;
            if (springLength < 0.01f) springLength = 0.01f;

            int totalPoints = coilCount * pointsPerCoil + 1;
            springLine.positionCount = totalPoints;

            // Строим точки спирали
            for (int i = 0; i < totalPoints; i++)
            {
                float t = (float)i / (totalPoints - 1);   // 0..1 сверху вниз
                float y = -t * springLength;
                float angle = t * coilCount * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * coilRadius;
                float z = Mathf.Sin(angle) * coilRadius;
                springLine.SetPosition(i, new Vector3(x, y, z));
            }
        }

        public override void ResetSimulation()
        {
            if (_model == null) return;

            _model.Mass = mass;
            _model.Stiffness = stiffness;
            _model.Damping = damping;
            _model.Gravity = gravity;
            _model.Reset(initialDisplacement);
            UpdateVisual();
        }

        private void OnValidate()
        {
            if (Application.isPlaying && _model != null)
            {
                _model.Mass = mass;
                _model.Stiffness = stiffness;
                _model.Damping = damping;
                _model.Gravity = gravity;
            }
        }

        // ── Публичные свойства для UI ─────────────────────────────────────

        public float Mass
        {
            get => mass;
            set
            {
                mass = Mathf.Clamp(value, 0.1f, 10f);
                if (_model != null) _model.Mass = mass;
            }
        }

        public float Stiffness
        {
            get => stiffness;
            set
            {
                stiffness = Mathf.Clamp(value, 5f, 200f);
                if (_model != null) _model.Stiffness = stiffness;
            }
        }

        public float Damping
        {
            get => damping;
            set
            {
                damping = Mathf.Clamp(value, 0f, 5f);
                if (_model != null) _model.Damping = damping;
            }
        }

        public float Gravity
        {
            get => gravity;
            set
            {
                gravity = Mathf.Clamp(value, 0f, 25f);
                if (_model != null) _model.Gravity = gravity;
            }
        }

        public float InitialDisplacement
        {
            get => initialDisplacement;
            set => initialDisplacement = Mathf.Clamp(value, -0.5f, 0.5f);
        }



        public override void OnActivate()
        {
            ProgressManager.Instance?.MarkExhibitStudied(ExhibitId);

            var studyPanel = FindObjectOfType<UI.ExhibitStudyPanel>(true);
            if (studyPanel != null)
            {
                studyPanel.Open(this);
            }
            else
            {
                Debug.LogWarning("[Spring] ExhibitStudyPanel не найдена!");
            }
        }

        public override string GetFormulaText()
        {
            float m = mass;
            float k = stiffness;
            float period = 2f * Mathf.PI * Mathf.Sqrt(m / k);
            float frequency = 1f / period;
            float omega = Mathf.Sqrt(k / m);

            return
                "<b>Формула периода:</b>\n" +
                $"  T = 2π·√(m/k) = 2π·√({m:F2}/{k:F1})\n" +
                $"  T = <color=#FFD700>{period:F3} с</color>\n\n" +
                $"<b>Частота:</b>  f = <color=#FFD700>{frequency:F3} Гц</color>\n" +
                $"<b>Угловая частота:</b>  ω = √(k/m) = {omega:F3} рад/с\n\n" +
                "<i>Период не зависит от амплитуды — это свойство\n" +
                "линейного осциллятора.</i>";
        }
    }
}