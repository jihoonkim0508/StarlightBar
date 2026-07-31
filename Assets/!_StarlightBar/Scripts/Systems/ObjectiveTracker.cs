using System;
using System.Collections.Generic;
using System.Linq;
using StarlightBar.Content;

namespace StarlightBar.Systems
{
    /// <summary>
    /// 현재 챕터의 목표 진행도와 필수 목표 완료 여부를 계산합니다.
    /// </summary>
    public sealed class ObjectiveTracker
    {
        private readonly Dictionary<string, ObjectiveProgress> progress = new();

        public event Action<ObjectiveProgress> ObjectiveChanged;

        public bool MandatoryObjectivesComplete =>
            progress.Values.Where(item => item.Definition.mandatory).All(item => item.IsComplete);

        public IReadOnlyCollection<ObjectiveProgress> All => progress.Values;

        /// <summary>
        /// 현재 챕터의 목표 정의 목록으로 진행도를 초기화합니다.
        /// </summary>
        public void Load(IEnumerable<ObjectiveDefinition> definitions)
        {
            progress.Clear();
            if (definitions == null) return;

            foreach (var definition in definitions.Where(item => item != null))
                progress[definition.id] = new ObjectiveProgress(definition);
        }

        /// <summary>
        /// 지정한 목표의 진행량을 증가시키고 변경 이벤트를 보냅니다.
        /// </summary>
        public bool AddProgress(string objectiveId, int amount = 1)
        {
            if (amount <= 0 || !progress.TryGetValue(objectiveId, out var item))
                return false;

            item.SetCount(item.CurrentCount + amount);
            ObjectiveChanged?.Invoke(item);
            return true;
        }
    }

    /// <summary>
    /// 목표 정의와 현재 달성 수치를 함께 제공합니다.
    /// </summary>
    public sealed class ObjectiveProgress
    {
        public ObjectiveDefinition Definition { get; }
        public int CurrentCount { get; private set; }
        public bool IsComplete => CurrentCount >= Definition.requiredCount;

        /// <summary>
        /// 지정한 목표 정의의 진행 상태를 만듭니다.
        /// </summary>
        public ObjectiveProgress(ObjectiveDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        /// <summary>
        /// 목표 진행량을 0과 요구 수량 사이로 제한해 설정합니다.
        /// </summary>
        public void SetCount(int count)
        {
            CurrentCount = count < 0 ? 0 : count > Definition.requiredCount ? Definition.requiredCount : count;
        }
    }
}
