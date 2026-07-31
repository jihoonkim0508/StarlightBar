using System.Collections.Generic;
using StarlightBar.Content;
using StarlightBar.Exploration;
using StarlightBar.UI;
using TMPro;
using UnityEngine;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 공용 정화 규칙 위에 12궁별 배치·위험 패턴·환경 규칙과 스텔라 결계를 결합합니다.
    /// </summary>
    public sealed class MemoryMechanicDirector : MonoBehaviour
    {
        private MemorySpaceDefinition definition;
        private Transform player;
        private MemorySpacePresenter presenter;
        private TMP_Text barrierText;
        private string moduleId;
        private float barrier = 100f;
        private float nextHazardTime;
        private Vector3 lastPlayerPosition;

        public IReadOnlyList<Vector2> FragmentPositions { get; private set; }
        public int FalseMemoryCount { get; private set; } = 1;

        /// <summary>
        /// 기억공간 데이터에 맞는 서로 다른 기믹 모듈을 생성합니다.
        /// </summary>
        public void Initialize(
            MemorySpaceDefinition memoryDefinition, Transform playerTransform,
            MemorySpacePresenter memoryPresenter, Transform uiParent)
        {
            definition = memoryDefinition;
            player = playerTransform;
            presenter = memoryPresenter;
            moduleId = definition.sceneVariantId ?? string.Empty;
            lastPlayerPosition = player.position;
            FragmentPositions = ResolveFragmentPositions(moduleId);
            FalseMemoryCount = moduleId is "mirror_pair" or "endless_verdict" ? 2 : 1;
            if (Camera.main != null)
                Camera.main.backgroundColor = Color.Lerp(Color.black, definition.palette, 0.34f);
            barrierText = DynamicContentFactory.CreateText(
                uiParent, $"스텔라의 결계  {Mathf.RoundToInt(barrier)}% · {ModuleTitle(moduleId)}", 18);
            SpawnModuleHazards();
        }

        private void Update()
        {
            if (player == null || RuntimeDialoguePresenter.AnyPlaying || SettingsMenuPresenter.AnyOpen)
                return;

            barrier = Mathf.Min(100f, barrier + Time.deltaTime * 2.2f);
            ApplyEnvironmentalRule();
            if (barrierText != null)
                barrierText.text = $"스텔라의 결계  {Mathf.RoundToInt(barrier)}% · {ModuleTitle(moduleId)}";
            lastPlayerPosition = player.position;
        }

        /// <summary>
        /// 파편 정화가 현재 모듈의 고유 조건을 만족하는지 확인합니다.
        /// </summary>
        public bool CanPurify(MemoryFragmentMarker fragment, out string reason)
        {
            reason = string.Empty;
            if (fragment == null || player == null)
                return false;

            if (moduleId == "tilting_scales" && Mathf.Abs(player.position.x) > 1.1f)
            {
                reason = "저울이 기울었습니다. 중앙의 은빛 선 위에서 기억을 판단하세요.";
                return false;
            }
            if (moduleId == "mirror_pair" &&
                Mathf.Sign(player.position.x) == Mathf.Sign(fragment.transform.position.x))
            {
                reason = "거울 반대편에서 바라봐야 진짜 기억과 투영을 구분할 수 있습니다.";
                return false;
            }
            if (moduleId == "split_form")
            {
                var acceptFragment = fragment.ObjectiveId?.EndsWith("_accept") == true;
                if (acceptFragment && player.position.y > 0f)
                {
                    reason = "산의 형상만으로는 기억이 완성되지 않습니다. 물의 영역에서 받아들이세요.";
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 장애물 충돌로 스텔라의 결계 안정도를 낮추고 0이 되면 체크포인트로 복귀시킵니다.
        /// </summary>
        public void DamageBarrier(float amount)
        {
            barrier = Mathf.Max(0f, barrier - Mathf.Max(1f, amount));
            if (barrier <= 0f)
            {
                barrier = 45f;
                presenter.HandleBarrierBreak();
            }
        }

        private void ApplyEnvironmentalRule()
        {
            var body = player.GetComponent<Rigidbody2D>();
            if (body == null)
                return;

            switch (moduleId)
            {
                case "wind_route":
                    body.AddForce(new Vector2(Mathf.Sin(Time.time * 1.2f), 0.25f) * 1.8f);
                    break;
                case "rising_water":
                    if (player.position.y < Mathf.Sin(Time.time * 0.7f) * 1.2f - 1.7f)
                        DamageBarrier(Time.deltaTime * 12f);
                    break;
                case "overflowing_vessels":
                    if (Time.time >= nextHazardTime)
                    {
                        nextHazardTime = Time.time + 3.2f;
                        SpawnSweep("OverflowWave", new Vector2(-7f, -2.7f), Vector2.right, 5.5f, 9f);
                    }
                    break;
                case "giant_footsteps":
                    if (Time.time >= nextHazardTime)
                    {
                        nextHazardTime = Time.time + 2.4f;
                        var x = Mathf.Round(player.position.x / 1.5f) * 1.5f;
                        SpawnPulse("GiantFootstep", new Vector2(x, player.position.y), new Vector2(1.5f, 1.5f), 0.75f);
                    }
                    break;
                case "poisoned_arrows":
                    if (Time.time >= nextHazardTime)
                    {
                        nextHazardTime = Time.time + 1.6f;
                        SpawnSweep("PoisonedArrow", new Vector2(-7f, Random.Range(-3f, 3f)), Vector2.right, 7.5f, 12f);
                    }
                    break;
            }
        }

        private void SpawnModuleHazards()
        {
            switch (moduleId)
            {
                case "moving_chains":
                    SpawnSweep("ChainA", new Vector2(-4f, 1.2f), Vector2.right, 3.2f, 8f);
                    SpawnSweep("ChainB", new Vector2(3.5f, -1.3f), Vector2.left, 2.6f, 7f);
                    break;
                case "shadow_pursuit":
                    SpawnChaser("CommandShadow", new Vector2(4.2f, 2.4f), 1.25f);
                    break;
                case "rising_water":
                    SpawnSweep("CurrentA", new Vector2(-4f, 0.3f), Vector2.right, 2.8f, 8f);
                    SpawnSweep("CurrentB", new Vector2(4f, -1.5f), Vector2.left, 2.2f, 8f);
                    break;
                case "wind_route":
                    SpawnOrbit("WindDebrisA", Vector2.zero, 2.5f, 0.8f);
                    SpawnOrbit("WindDebrisB", Vector2.zero, 3.8f, -0.55f);
                    break;
                case "shifting_shore":
                    SpawnSweep("ShoreA", new Vector2(-4f, 2f), new Vector2(1, -0.35f), 2.3f, 8f);
                    SpawnSweep("ShoreB", new Vector2(4f, -2f), new Vector2(-1, 0.35f), 2.3f, 8f);
                    break;
                case "mirror_pair":
                    SpawnChaser("MirrorSelfA", new Vector2(-4f, 0f), 0.85f);
                    SpawnChaser("MirrorSelfB", new Vector2(4f, 0f), 0.85f);
                    break;
                case "giant_footsteps":
                    SpawnPulse("FootprintMemory", new Vector2(0f, 2.2f), new Vector2(1.7f, 1.2f), 1.4f);
                    break;
                case "endless_verdict":
                    SpawnOrbit("VerdictA", Vector2.zero, 2.2f, 0.75f);
                    SpawnOrbit("VerdictB", Vector2.zero, 3.6f, -0.45f);
                    break;
                case "tilting_scales":
                    SpawnSweep("ScaleLeft", new Vector2(-3f, 0f), Vector2.up, 1.8f, 6f);
                    SpawnSweep("ScaleRight", new Vector2(3f, 0f), Vector2.down, 1.8f, 6f);
                    break;
                case "poisoned_arrows":
                    SpawnSweep("ArrowOpening", new Vector2(-7f, 1.6f), Vector2.right, 7.5f, 12f);
                    break;
                case "split_form":
                    SpawnSweep("MountainCrack", new Vector2(-3.8f, 1.3f), Vector2.right, 2.5f, 7f);
                    SpawnSweep("WaterCrack", new Vector2(3.8f, -1.3f), Vector2.left, 2.5f, 7f);
                    break;
                case "overflowing_vessels":
                    SpawnSweep("FirstOverflow", new Vector2(-7f, -2f), Vector2.right, 5.5f, 9f);
                    break;
                default:
                    SpawnOrbit("MemoryDebris", Vector2.zero, 3f, 0.5f);
                    break;
            }
        }

        private void SpawnSweep(string name, Vector2 start, Vector2 direction, float speed, float resetDistance)
        {
            var hazard = CreateHazard(name, start, new Vector2(1.2f, 0.28f));
            hazard.InitializeSweep(this, direction, speed, resetDistance);
        }

        private void SpawnChaser(string name, Vector2 start, float speed)
        {
            var hazard = CreateHazard(name, start, new Vector2(0.55f, 0.75f));
            hazard.InitializeChase(this, player, speed);
        }

        private void SpawnOrbit(string name, Vector2 center, float radius, float speed)
        {
            var hazard = CreateHazard(name, center + Vector2.right * radius, new Vector2(0.45f, 0.45f));
            hazard.InitializeOrbit(this, center, radius, speed);
        }

        private void SpawnPulse(string name, Vector2 position, Vector2 size, float warningTime)
        {
            var hazard = CreateHazard(name, position, size);
            hazard.InitializePulse(this, warningTime);
        }

        private static MemoryHazardView CreateHazard(string name, Vector2 position, Vector2 size)
        {
            var prefab = RuntimePrefabLibrary.Instance?.memoryHazardPrefab;
            if (prefab == null)
                throw new System.InvalidOperationException(
                    "RuntimePrefabLibrary에 기억공간 장애물 프리팹을 연결해야 합니다.");
            var instance = Object.Instantiate(prefab, position, Quaternion.identity);
            instance.name = name;
            instance.transform.localScale = new Vector3(size.x, size.y, 1f);
            return instance.GetComponent<MemoryHazardView>();
        }

        private static IReadOnlyList<Vector2> ResolveFragmentPositions(string module) => module switch
        {
            "moving_chains" => new[] { new Vector2(-3.4f, 2.3f), new Vector2(3.1f, 1.5f), new Vector2(0f, -2.6f) },
            "shadow_pursuit" => new[] { new Vector2(-3.8f, -2.2f), new Vector2(0f, 2.7f), new Vector2(3.7f, -1.4f) },
            "rising_water" => new[] { new Vector2(-3.6f, 2.7f), new Vector2(0f, 0.8f), new Vector2(3.6f, 2.4f) },
            "wind_route" => new[] { new Vector2(-4.2f, -2.3f), new Vector2(0f, 2.8f), new Vector2(4.2f, -2.3f) },
            "shifting_shore" => new[] { new Vector2(-4f, 2.4f), new Vector2(0f, -2.7f), new Vector2(4f, 2.4f) },
            "mirror_pair" => new[] { new Vector2(-3.8f, 1.8f), new Vector2(3.8f, 1.8f), new Vector2(0f, -2.6f) },
            "giant_footsteps" => new[] { new Vector2(-4f, -2.4f), new Vector2(0f, 2.5f), new Vector2(4f, -2.4f) },
            "endless_verdict" => new[] { new Vector2(-3.8f, 0f), new Vector2(0f, 2.8f), new Vector2(3.8f, 0f) },
            "tilting_scales" => new[] { new Vector2(-3.7f, 2.1f), new Vector2(3.7f, 2.1f), new Vector2(0f, -2.5f) },
            "poisoned_arrows" => new[] { new Vector2(-4f, -2.5f), new Vector2(0f, 2.4f), new Vector2(4f, -0.7f) },
            "split_form" => new[] { new Vector2(-3.8f, 2f), new Vector2(3.8f, -2f), new Vector2(0f, -2.8f) },
            "overflowing_vessels" => new[] { new Vector2(-4f, 2.5f), new Vector2(0f, -1.8f), new Vector2(4f, 2.5f) },
            _ => new[] { new Vector2(-3.2f, 2f), new Vector2(3f, 1.8f), new Vector2(0.8f, -2.4f) }
        };

        private static string ModuleTitle(string module) => module switch
        {
            "moving_chains" => "움직이는 구속의 사슬",
            "shadow_pursuit" => "명령의 그림자 추격",
            "rising_water" => "차오르는 공포의 물결",
            "wind_route" => "희생을 재촉하는 바람길",
            "shifting_shore" => "선택에 따라 움직이는 해안",
            "mirror_pair" => "서로 다른 두 거울",
            "giant_footsteps" => "자신을 지우는 거대한 발자국",
            "endless_verdict" => "끝없이 늘어나는 판결",
            "tilting_scales" => "감정을 제외한 기울어진 저울",
            "poisoned_arrows" => "되돌아오는 독화살",
            "split_form" => "산과 물로 갈라진 형상",
            "overflowing_vessels" => "끝없이 넘치는 물병",
            _ => "오염된 기억의 파편"
        };
    }

    /// <summary>
    /// 추격·공전·횡단·예고 점멸을 조합하는 기억공간 위험 오브젝트입니다.
    /// </summary>
    public sealed class MemoryDynamicHazard : MonoBehaviour
    {
        private enum Pattern { Sweep, Chase, Orbit, Pulse }
        private Pattern pattern;
        private MemoryMechanicDirector director;
        private Transform target;
        private Vector2 direction;
        private Vector2 origin;
        private float speed;
        private float distance;
        private float radius;
        private float pulseDelay;
        private float createdAt;
        private SpriteRenderer targetRenderer;
        private Collider2D targetCollider;

        /// <summary>
        /// 지정 방향을 반복 횡단하는 감정 장애물로 초기화합니다.
        /// </summary>
        public void InitializeSweep(MemoryMechanicDirector owner, Vector2 moveDirection, float moveSpeed, float resetDistance)
        {
            SetCommon(owner, Pattern.Sweep);
            direction = moveDirection.normalized;
            speed = moveSpeed;
            distance = resetDistance;
            origin = transform.position;
        }

        /// <summary>
        /// 플레이어를 추적하는 그림자 장애물로 초기화합니다.
        /// </summary>
        public void InitializeChase(MemoryMechanicDirector owner, Transform chaseTarget, float moveSpeed)
        {
            SetCommon(owner, Pattern.Chase);
            target = chaseTarget;
            speed = moveSpeed;
        }

        /// <summary>
        /// 지정 중심을 도는 기억 파편 장애물로 초기화합니다.
        /// </summary>
        public void InitializeOrbit(MemoryMechanicDirector owner, Vector2 center, float orbitRadius, float orbitSpeed)
        {
            SetCommon(owner, Pattern.Orbit);
            origin = center;
            radius = orbitRadius;
            speed = orbitSpeed;
        }

        /// <summary>
        /// 사전 경고 뒤 확장되는 파동 장애물로 초기화합니다.
        /// </summary>
        public void InitializePulse(MemoryMechanicDirector owner, float warningTime)
        {
            SetCommon(owner, Pattern.Pulse);
            pulseDelay = warningTime;
            targetCollider.enabled = false;
        }

        private void SetCommon(MemoryMechanicDirector owner, Pattern hazardPattern)
        {
            director = owner;
            pattern = hazardPattern;
            createdAt = Time.time;
            targetRenderer = GetComponent<SpriteRenderer>();
            targetCollider = GetComponent<Collider2D>();
        }

        private void Update()
        {
            switch (pattern)
            {
                case Pattern.Sweep:
                    transform.position += (Vector3)(direction * (speed * Time.deltaTime));
                    if (Vector2.Distance(origin, transform.position) > distance)
                        transform.position = origin;
                    break;
                case Pattern.Chase:
                    if (target != null)
                        transform.position = Vector2.MoveTowards(
                            transform.position, target.position, speed * Time.deltaTime);
                    break;
                case Pattern.Orbit:
                    var angle = (Time.time - createdAt) * speed;
                    transform.position = origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    break;
                case Pattern.Pulse:
                    var elapsed = Time.time - createdAt;
                    targetRenderer.color = elapsed < pulseDelay
                        ? new Color(0.92f, 0.62f, 0.18f, 0.28f + elapsed / pulseDelay * 0.38f)
                        : new Color(0.58f, 0.04f, 0.14f, 0.92f);
                    targetCollider.enabled = elapsed >= pulseDelay;
                    if (elapsed >= pulseDelay + 0.45f)
                        Destroy(gameObject);
                    break;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerMover2D>() == null)
                return;
            director?.DamageBarrier(28f);
            AccessibleCameraEffectsPresenter.PlayImpact(0.09f, 0.16f);
            if (pattern == Pattern.Sweep)
                transform.position = origin;
        }
    }
}
