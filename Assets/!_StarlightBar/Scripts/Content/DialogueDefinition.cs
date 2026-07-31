using System.Collections.Generic;
using UnityEngine;

namespace StarlightBar.Content
{
    /// <summary>
    /// 한국어 대사 노드와 선택지 목록을 정의합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "별빛주점/대화", fileName = "Dialogue_")]
    public sealed class DialogueDefinition : ScriptableObject
    {
        [Tooltip("프로젝트 전체에서 중복되지 않는 대화 정의 ID입니다.")]
        public string id;
        [Tooltip("대화를 시작할 첫 대사 노드 ID입니다.")]
        public string entryLineId;
        [Tooltip("화자, 본문, 선택지와 다음 노드 참조를 포함하는 대사 목록입니다.")]
        public List<DialogueLine> lines = new();

        /// <summary>
        /// 대사 ID에 해당하는 대사 노드를 반환합니다.
        /// </summary>
        public DialogueLine FindLine(string lineId)
        {
            return lines.Find(line => line != null && line.id == lineId);
        }
    }
}
