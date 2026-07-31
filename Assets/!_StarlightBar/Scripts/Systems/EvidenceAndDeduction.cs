using System;
using System.Collections.Generic;
using System.Linq;
using StarlightBar.Content;

namespace StarlightBar.Systems
{
    /// <summary>
    /// 수집한 증거와 플레이어가 만든 카드 연결을 관리합니다.
    /// </summary>
    public sealed class EvidenceGraph
    {
        private readonly Dictionary<string, EvidenceDefinition> collected = new();
        private readonly HashSet<EvidenceLink> links = new();

        public IReadOnlyCollection<EvidenceDefinition> Collected => collected.Values;
        public IReadOnlyCollection<EvidenceLink> Links => links;

        /// <summary>
        /// 유효한 새 증거를 그래프에 한 번만 수집합니다.
        /// </summary>
        public bool Collect(EvidenceDefinition evidence)
        {
            if (evidence == null || string.IsNullOrWhiteSpace(evidence.id))
                return false;
            if (collected.ContainsKey(evidence.id))
                return false;
            collected.Add(evidence.id, evidence);
            return true;
        }

        /// <summary>
        /// 양쪽 카드 중 하나라도 연결을 허용한 경우에만 관계를 기록합니다.
        /// </summary>
        public bool TryLink(string firstId, string secondId)
        {
            if (firstId == secondId ||
                !collected.TryGetValue(firstId, out var first) ||
                !collected.TryGetValue(secondId, out var second))
                return false;

            var allowed = first.allowedLinkEvidenceIds.Contains(secondId) ||
                          second.allowedLinkEvidenceIds.Contains(firstId);
            return allowed && links.Add(new EvidenceLink(firstId, secondId));
        }

        /// <summary>
        /// 수집 증거가 후보를 지지하거나 제외하는 정도를 네 단계 신뢰도로 계산합니다.
        /// </summary>
        public CandidateConfidence GetConfidence(string candidateId)
        {
            if (collected.Values.Any(item => item.excludedCandidateIds.Contains(candidateId)))
                return CandidateConfidence.Excluded;

            var support = collected.Values.Count(item => item.supportedCandidateIds.Contains(candidateId));
            return support switch
            {
                >= 3 => CandidateConfidence.High,
                2 => CandidateConfidence.Medium,
                1 => CandidateConfidence.Low,
                _ => CandidateConfidence.Low
            };
        }
    }

    /// <summary>
    /// 순서와 무관한 증거 카드 연결 키입니다.
    /// </summary>
    public readonly struct EvidenceLink : IEquatable<EvidenceLink>
    {
        public string FirstId { get; }
        public string SecondId { get; }

        /// <summary>
        /// 카드 순서와 무관하게 같은 연결로 비교되는 증거 연결 키를 만듭니다.
        /// </summary>
        public EvidenceLink(string firstId, string secondId)
        {
            if (string.CompareOrdinal(firstId, secondId) <= 0)
            {
                FirstId = firstId;
                SecondId = secondId;
            }
            else
            {
                FirstId = secondId;
                SecondId = firstId;
            }
        }

        /// <summary>
        /// 두 증거 연결이 같은 카드 쌍인지 비교합니다.
        /// </summary>
        public bool Equals(EvidenceLink other) => FirstId == other.FirstId && SecondId == other.SecondId;

        /// <summary>
        /// 개체가 같은 증거 연결인지 비교합니다.
        /// </summary>
        public override bool Equals(object obj) => obj is EvidenceLink other && Equals(other);

        /// <summary>
        /// 순서가 정규화된 두 증거 ID의 해시 코드를 반환합니다.
        /// </summary>
        public override int GetHashCode() => HashCode.Combine(FirstId, SecondId);
    }

    /// <summary>
    /// 별자리·신화·핵심 증거 제출을 정답 정의와 비교합니다.
    /// </summary>
    public static class DeductionEvaluator
    {
        /// <summary>
        /// 제출한 별자리·신화·핵심 증거를 챕터 정답과 비교합니다.
        /// </summary>
        public static DeductionResult Evaluate(
            DeductionDefinition definition,
            string zodiacId,
            string mythId,
            IEnumerable<string> submittedEvidenceIds)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));

            var submitted = new HashSet<string>(submittedEvidenceIds ?? Array.Empty<string>());
            var missing = definition.requiredCoreEvidenceIds.Where(id => !submitted.Contains(id)).ToArray();
            var identityCorrect = definition.correctZodiacId == zodiacId;
            var mythCorrect = definition.correctMythId == mythId;
            return new DeductionResult(identityCorrect && mythCorrect && missing.Length == 0, identityCorrect, mythCorrect, missing);
        }
    }

    /// <summary>
    /// 추리 제출의 세부 판정 결과입니다.
    /// </summary>
    public readonly struct DeductionResult
    {
        public bool Success { get; }
        public bool ZodiacCorrect { get; }
        public bool MythCorrect { get; }
        public IReadOnlyList<string> MissingEvidenceIds { get; }

        /// <summary>
        /// 추리 성공 여부와 각 항목의 세부 판정을 만듭니다.
        /// </summary>
        public DeductionResult(bool success, bool zodiacCorrect, bool mythCorrect, IReadOnlyList<string> missingEvidenceIds)
        {
            Success = success;
            ZodiacCorrect = zodiacCorrect;
            MythCorrect = mythCorrect;
            MissingEvidenceIds = missingEvidenceIds;
        }
    }
}
