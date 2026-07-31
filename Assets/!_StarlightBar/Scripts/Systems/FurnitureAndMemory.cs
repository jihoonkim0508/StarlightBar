using System;
using System.Collections.Generic;
using System.Linq;
using StarlightBar.Content;
using UnityEngine;

namespace StarlightBar.Systems
{
    /// <summary>
    /// 현재 배치된 가구의 특성과 손님의 선호·기피 반응을 계산합니다.
    /// </summary>
    public static class FurnitureReactionEvaluator
    {
        /// <summary>
        /// 배치 가구 속성과 손님의 선호·기피 속성이 일치하는 횟수를 계산합니다.
        /// </summary>
        public static FurnitureReaction Evaluate(
            CharacterDefinition guest,
            IEnumerable<FurnitureDefinition> placedFurniture)
        {
            if (guest == null) throw new ArgumentNullException(nameof(guest));

            var traits = (placedFurniture ?? Array.Empty<FurnitureDefinition>())
                .Where(item => item != null)
                .SelectMany(item => item.traits)
                .ToList();
            var positive = traits.Count(guest.preferredFurnitureTraits.Contains);
            var negative = traits.Count(guest.rejectedFurnitureTraits.Contains);
            return new FurnitureReaction(positive, negative);
        }
    }

    /// <summary>
    /// 손님의 긍정·부정 가구 반응 횟수입니다.
    /// </summary>
    public readonly struct FurnitureReaction
    {
        public int PositiveCount { get; }
        public int NegativeCount { get; }

        /// <summary>
        /// 가구에 대한 긍정·부정 반응 횟수를 만듭니다.
        /// </summary>
        public FurnitureReaction(int positiveCount, int negativeCount)
        {
            PositiveCount = positiveCount;
            NegativeCount = negativeCount;
        }
    }

    /// <summary>
    /// 기억공간 목표, 체크포인트, 실패 후 복귀를 관리합니다.
    /// </summary>
    public sealed class MemorySpaceSession
    {
        private readonly HashSet<string> completedObjectives = new();
        private readonly List<MemoryCheckpoint> checkpoints = new();

        public MemorySpaceDefinition Definition { get; }
        public int RetryCount { get; private set; }
        public bool IsComplete => Definition.objectiveIds.All(completedObjectives.Contains);
        public IReadOnlyCollection<string> CompletedObjectiveIds => completedObjectives;

        /// <summary>
        /// 지정한 기억공간 정의로 목표와 체크포인트 세션을 만듭니다.
        /// </summary>
        public MemorySpaceSession(MemorySpaceDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        /// <summary>
        /// 기억공간에 정의된 목표를 중복 없이 완료 처리합니다.
        /// </summary>
        public bool CompleteObjective(string objectiveId)
        {
            return Definition.objectiveIds.Contains(objectiveId) && completedObjectives.Add(objectiveId);
        }

        /// <summary>
        /// 자동 저장에서 복원된 기억공간 목표를 다시 적용합니다.
        /// </summary>
        public void RestoreCompletedObjectives(IEnumerable<string> objectiveIds)
        {
            completedObjectives.Clear();
            if (objectiveIds == null)
                return;

            foreach (var objectiveId in objectiveIds)
            {
                if (Definition.objectiveIds.Contains(objectiveId))
                    completedObjectives.Add(objectiveId);
            }
        }

        /// <summary>
        /// 현재 위치와 완료 목표를 실패 복귀용 체크포인트로 저장합니다.
        /// </summary>
        public void AddCheckpoint(string id, Vector3 position)
        {
            checkpoints.Add(new MemoryCheckpoint(id, position, completedObjectives.ToArray()));
        }

        /// <summary>
        /// 실패 시 마지막 체크포인트의 위치와 목표 상태를 복원합니다.
        /// </summary>
        public bool TryRestoreLastCheckpoint(out MemoryCheckpoint checkpoint)
        {
            RetryCount++;
            if (checkpoints.Count == 0)
            {
                checkpoint = default;
                completedObjectives.Clear();
                return false;
            }

            checkpoint = checkpoints[^1];
            completedObjectives.Clear();
            foreach (var objectiveId in checkpoint.CompletedObjectiveIds)
                completedObjectives.Add(objectiveId);
            return true;
        }
    }

    /// <summary>
    /// 기억공간 실패 시 복원할 위치와 완료 목표의 불변 기록입니다.
    /// </summary>
    public readonly struct MemoryCheckpoint
    {
        public string Id { get; }
        public Vector3 Position { get; }
        public IReadOnlyList<string> CompletedObjectiveIds { get; }

        /// <summary>
        /// 체크포인트 식별자·위치·완료 목표 목록을 만듭니다.
        /// </summary>
        public MemoryCheckpoint(string id, Vector3 position, IReadOnlyList<string> completedObjectiveIds)
        {
            Id = id;
            Position = position;
            CompletedObjectiveIds = completedObjectiveIds;
        }
    }
}
