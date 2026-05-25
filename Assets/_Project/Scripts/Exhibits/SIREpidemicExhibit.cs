using UnityEngine;
using ScienceMuseum.Core;
using ScienceMuseum.Simulation.Models;
using ScienceMuseum.Simulation.Challenges;

namespace ScienceMuseum.Exhibits
{
    /// <summary>
    /// Экспонат «Эпидемия» — визуализация модели SIR через облако N агентов.
    /// Каждый агент носит один из трёх статусов (восприимчивый/заражённый/выздоровевший);
    /// численности по статусам в каждом кадре подгоняются под долями S, I, R из модели.
    /// </summary>
    public class SIREpidemicExhibit : ExhibitBase
    {
        [Header("Параметры эпидемии")]
        [Tooltip("β — скорость заражения")]
        [Range(0.05f, 3f)][SerializeField] private float beta = 0.5f;

        [Tooltip("γ — скорость выздоровления")]
        [Range(0.05f, 1f)][SerializeField] private float gamma = 0.1f;

        [Header("Начальные доли")]
        [Tooltip("Доля восприимчивых в начале (0..1). Снижай, чтобы имитировать вакцинацию.")]
        [Range(0.1f, 1f)][SerializeField] private float s0Initial = 0.99f;

        [Tooltip("Доля заражённых в начале (0..1)")]
        [Range(0.001f, 0.1f)][SerializeField] private float i0Initial = 0.01f;

        [Header("Популяция")]
        [Tooltip("Сколько агентов отображается. Доли S/I/R пересчитываются в целые количества.")]
        [Range(20, 200)][SerializeField] private int populationSize = 80;

        [Tooltip("Префаб одного агента (маленькая сфера с Renderer'ом)")]
        [SerializeField] private GameObject agentPrefab;

        [Tooltip("Контейнер, в котором живут агенты (центр стеклянного куба)")]
        [SerializeField] private Transform crowdContainer;

        [Tooltip("Размер области, в которой движутся агенты (локальные единицы)")]
        [SerializeField] private Vector3 crowdSize = new Vector3(0.8f, 0.8f, 0.8f);

        [Header("Цвета состояний")]
        [SerializeField] private Color colorSusceptible = new Color(0.3f, 0.8f, 0.4f);
        [SerializeField] private Color colorInfected = new Color(0.95f, 0.25f, 0.2f);
        [SerializeField] private Color colorRecovered = new Color(0.55f, 0.55f, 0.6f);

        [Header("Движение")]
        [Tooltip("Базовая скорость агента")]
        [Range(0.02f, 1f)][SerializeField] private float agentSpeed = 0.15f;

        [Tooltip("Сила случайных толчков (брауновское возмущение)")]
        [Range(0f, 5f)][SerializeField] private float agentJitter = 1f;

        [Header("Симуляция")]
        [Range(1, 16)][SerializeField] private int subSteps = 4;
        [Range(0.1f, 5f)][SerializeField] private float timeScale = 1.5f;

        private enum AgentState { Susceptible, Infected, Recovered }

        private SIRModel _model;
        private ExhibitParameter[] _parameters;
        private IChallenge[] _challenges;

        private AgentState[] _agentStates;
        private Transform[] _agentTransforms;
        private Renderer[] _agentRenderers;
        private Vector3[] _agentVelocities;
        private MaterialPropertyBlock _materialBlock;

        public override ExhibitParameter[] Parameters => _parameters;
        public override IChallenge[] Challenges => _challenges;

        public float Beta
        {
            get => beta;
            set
            {
                beta = Mathf.Clamp(value, 0.05f, 3f);
                if (_model != null) _model.Beta = beta;
            }
        }

        public float Gamma
        {
            get => gamma;
            set
            {
                gamma = Mathf.Clamp(value, 0.05f, 1f);
                if (_model != null) _model.Gamma = gamma;
            }
        }

        public float S0Initial
        {
            get => s0Initial;
            set => s0Initial = Mathf.Clamp(value, 0.1f, 1f);
        }

        public float I0Initial
        {
            get => i0Initial;
            set => i0Initial = Mathf.Clamp(value, 0.001f, 0.1f);
        }

        public double CurrentS => _model?.S ?? 0.0;
        public double CurrentI => _model?.I ?? 0.0;
        public double CurrentR => _model?.R ?? 0.0;
        public double MaxObservedInfected => _model?.MaxObservedInfected ?? 0.0;
        public double BasicReproductionNumber =>
            _model?.BasicReproductionNumber ?? (beta / Mathf.Max(gamma, 1e-6f));
        public double HerdImmunityThreshold =>
            _model?.HerdImmunityThreshold ?? (gamma / Mathf.Max(beta, 1e-6f));

        protected override void Awake()
        {
            base.Awake();

            _model = new SIRModel { Beta = beta, Gamma = gamma };

            _parameters = new[]
            {
                new ExhibitParameter("Заражаемость β", "", 0.05f, 3f,
                    () => beta, v => Beta = v, decimals: 2),
                new ExhibitParameter("Выздоровление γ", "", 0.05f, 1f,
                    () => gamma, v => Gamma = v, decimals: 2),
                new ExhibitParameter("Восприимчивые S0", "", 0.1f, 1f,
                    () => s0Initial, v => S0Initial = v, decimals: 2),
                new ExhibitParameter("Заражённые I0", "", 0.001f, 0.1f,
                    () => i0Initial, v => I0Initial = v, decimals: 3),
            };

            _challenges = new IChallenge[]
            {
                new ContainEpidemicChallenge("sir.contain", this),
                new SevereOutbreakChallenge("sir.outbreak", this),
                new HerdImmunityChallenge("sir.herd_immunity", this),
            };

            SpawnAgents();
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

            UpdateAgentStates();
            UpdateAgentMotion(Time.deltaTime);
        }

        private void SpawnAgents()
        {
            if (agentPrefab == null || crowdContainer == null) return;

            _agentStates = new AgentState[populationSize];
            _agentTransforms = new Transform[populationSize];
            _agentRenderers = new Renderer[populationSize];
            _agentVelocities = new Vector3[populationSize];
            _materialBlock = new MaterialPropertyBlock();

            Vector3 half = crowdSize * 0.5f;
            for (int i = 0; i < populationSize; i++)
            {
                GameObject agent = Instantiate(agentPrefab, crowdContainer);
                agent.transform.localPosition = new Vector3(
                    Random.Range(-half.x, half.x),
                    Random.Range(-half.y, half.y),
                    Random.Range(-half.z, half.z));

                _agentTransforms[i] = agent.transform;
                _agentRenderers[i] = agent.GetComponentInChildren<Renderer>();
                _agentVelocities[i] = Random.insideUnitSphere * agentSpeed;
                _agentStates[i] = AgentState.Susceptible;
                ApplyColor(i, colorSusceptible);
            }
        }

        private void UpdateAgentStates()
        {
            if (_agentStates == null) return;

            int targetS = Mathf.RoundToInt((float)_model.S * populationSize);
            int targetI = Mathf.RoundToInt((float)_model.I * populationSize);
            if (targetS + targetI > populationSize) targetI = populationSize - targetS;
            if (targetI < 0) targetI = 0;

            int countS = 0, countI = 0;
            for (int i = 0; i < populationSize; i++)
            {
                if (_agentStates[i] == AgentState.Susceptible) countS++;
                else if (_agentStates[i] == AgentState.Infected) countI++;
            }

            // S → I: численности S по модели уменьшилось ниже фактического
            int toInfect = countS - targetS;
            for (int i = 0; i < populationSize && toInfect > 0; i++)
            {
                if (_agentStates[i] == AgentState.Susceptible)
                {
                    SetAgentState(i, AgentState.Infected);
                    countI++;
                    toInfect--;
                }
            }

            // I → R: численности I по модели уменьшилось ниже фактического
            int toRecover = countI - targetI;
            for (int i = 0; i < populationSize && toRecover > 0; i++)
            {
                if (_agentStates[i] == AgentState.Infected)
                {
                    SetAgentState(i, AgentState.Recovered);
                    toRecover--;
                }
            }
        }

        private void UpdateAgentMotion(float dt)
        {
            if (_agentStates == null) return;

            Vector3 half = crowdSize * 0.5f;

            for (int i = 0; i < populationSize; i++)
            {
                Vector3 v = _agentVelocities[i];
                v += Random.insideUnitSphere * (agentJitter * dt);

                float magn = v.magnitude;
                if (magn > agentSpeed) v *= agentSpeed / magn;

                Vector3 p = _agentTransforms[i].localPosition + v * dt;

                if (p.x > half.x) { p.x = half.x; v.x = -Mathf.Abs(v.x); }
                if (p.x < -half.x) { p.x = -half.x; v.x = Mathf.Abs(v.x); }
                if (p.y > half.y) { p.y = half.y; v.y = -Mathf.Abs(v.y); }
                if (p.y < -half.y) { p.y = -half.y; v.y = Mathf.Abs(v.y); }
                if (p.z > half.z) { p.z = half.z; v.z = -Mathf.Abs(v.z); }
                if (p.z < -half.z) { p.z = -half.z; v.z = Mathf.Abs(v.z); }

                _agentTransforms[i].localPosition = p;
                _agentVelocities[i] = v;
            }
        }

        private void SetAgentState(int index, AgentState state)
        {
            _agentStates[index] = state;

            Color color = state switch
            {
                AgentState.Susceptible => colorSusceptible,
                AgentState.Infected => colorInfected,
                _ => colorRecovered,
            };

            ApplyColor(index, color);
        }

        private void ApplyColor(int index, Color color)
        {
            if (_agentRenderers[index] == null) return;

            _agentRenderers[index].GetPropertyBlock(_materialBlock);
            _materialBlock.SetColor("_BaseColor", color);    // URP
            _materialBlock.SetColor("_Color", color);        // Built-in fallback
            _agentRenderers[index].SetPropertyBlock(_materialBlock);
        }

        public override void ResetSimulation()
        {
            if (_model == null) return;

            _model.Beta = beta;
            _model.Gamma = gamma;
            _model.Reset(s0Initial, i0Initial);

            if (_agentStates == null) return;

            for (int i = 0; i < populationSize; i++)
            {
                SetAgentState(i, AgentState.Susceptible);
            }
        }

        private void OnValidate()
        {
            if (Application.isPlaying && _model != null)
            {
                _model.Beta = beta;
                _model.Gamma = gamma;
            }
        }

        public override string GetFormulaText()
        {
            double r0 = BasicReproductionNumber;
            double sCrit = HerdImmunityThreshold;
            string regime = _model?.Regime ?? "—";

            return
                "<b>Модель SIR:</b>\n" +
                "  dS/dt = -β·S·I\n" +
                "  dI/dt = β·S·I - γ·I\n" +
                "  dR/dt = γ·I\n\n" +
                $"<b>Базовое число воспроизводства:</b>  R0 = β/γ = " +
                $"<color=#FFD700>{r0:F2}</color>\n" +
                $"<b>Порог иммунитета:</b>  S_crit = 1/R0 = " +
                $"<color=#FFD700>{sCrit:F3}</color>\n\n" +
                "<b>Текущие доли:</b>\n" +
                $"  S = {CurrentS * 100.0:F1}%  (восприимчивые)\n" +
                $"  I = {CurrentI * 100.0:F1}%  (заражённые)\n" +
                $"  R = {CurrentR * 100.0:F1}%  (выздоровевшие)\n\n" +
                $"<b>Режим:</b> <color=#FFD700>{regime}</color>";
        }
    }
}