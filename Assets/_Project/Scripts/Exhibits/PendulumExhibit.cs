using UnityEngine;
using ScienceMuseum.Core;
using ScienceMuseum.Simulation.Models;
using ScienceMuseum.Simulation.Challenges;
using ScienceMuseum.Managers;

namespace ScienceMuseum.Exhibits
{
    public class PendulumExhibit : ExhibitBase
    {
        private IChallenge[] _challenges;
        public override IChallenge[] Challenges => _challenges;
        public override ExhibitParameter[] Parameters => _parameters;

        private ExhibitParameter[] _parameters;

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

        // ── Публичные свойства для UI-панели ─────────────────────────────────

        public float Length
        {
            get => length;
            set
            {
                length = Mathf.Clamp(value, 0.3f, 2.5f);
                if (_model != null) _model.Length = length;
                UpdateStringAndBobScale();
                EvaluateChallenges();
            }
        }

        public float Gravity
        {
            get => gravity;
            set
            {
                gravity = Mathf.Clamp(value, 1f, 25f);
                if (_model != null) _model.Gravity = gravity;
                EvaluateChallenges();
            }
        }

        public float Damping
        {
            get => damping;
            set
            {
                damping = Mathf.Clamp(value, 0f, 2f);
                if (_model != null) _model.Damping = damping;
                EvaluateChallenges();
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

            // Создаём задания для этого экспоната
            _challenges = new IChallenge[]
            {
                new TargetPeriodChallenge("pendulum.period_2sec", this,
                    targetPeriod: 2.0f, tolerance: 0.05f),
                new TargetPeriodChallenge("pendulum.period_1sec", this,
                    targetPeriod: 1.0f, tolerance: 0.05f,
                    title: "Быстрый маятник",
                    description: "Сделай так, чтобы маятник совершал одно колебание за 1 секунду."),
                new MatchGravityChallenge("pendulum.gravity_moon", this, 1.62f, "Луне"),
                new MatchGravityChallenge("pendulum.gravity_mars", this, 3.71f, "Марсе"),
            };

            _parameters = new[]
            {
                new ExhibitParameter(
                    "Длина нити L", "м", 0.3f, 2.5f,
                    () => length,
                    v => Length = v,
                    decimals: 2),
                new ExhibitParameter(
                    "Гравитация g", "м/с²", 1f, 25f,
                    () => gravity,
                    v => Gravity = v,
                    decimals: 2),
                new ExhibitParameter(
                    "Трение k", "", 0f, 2f,
                    () => damping,
                    v => Damping = v,
                    decimals: 2),
                new ExhibitParameter(
                    "Начальный угол θ", "°", -170f, 170f,
                    () => initialAngleDegrees,
                    v => InitialAngleDegrees = v,
                    decimals: 0),
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

        private void EvaluateChallenges()
        {
            if (_challenges == null) return;

            foreach (var challenge in _challenges)
            {
                var previousStatus = challenge.Status;
                challenge.Evaluate();

                // Если только что выполнили - регистрируем в менеджере прогресса
                if (previousStatus != ChallengeStatus.Completed &&
                    challenge.Status == ChallengeStatus.Completed)
                {
                    ProgressManager.Instance?.CompleteChallenge(challenge.Id);
                }
            }
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
                Debug.LogWarning("[Pendulum] ExhibitStudyPanel не найдена!");
            }
        }

        public override string GetFormulaText()
        {
            float L = length;
            float g = gravity;
            float period = 2f * Mathf.PI * Mathf.Sqrt(L / g);
            float frequency = 1f / period;

            return
                "<b>Формула периода малых колебаний:</b>\n" +
                $"  T = 2π·√(L/g) = 2π·√({L:F2}/{g:F2})\n" +
                $"  T = <color=#FFD700>{period:F3} с</color>\n\n" +
                $"<b>Частота:</b>  f = 1/T = <color=#FFD700>{frequency:F3} Гц</color>\n\n" +
                "<i>Формула работает для малых углов (до ~15°). " +
                "При больших углах реальный период больше.</i>";
        }
    }
}