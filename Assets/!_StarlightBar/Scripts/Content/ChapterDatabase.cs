using System.Collections.Generic;
using UnityEngine;

namespace StarlightBar.Content
{
    /// <summary>
    /// 빌드에 포함되는 승인 완료 챕터를 순서대로 보관합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "별빛주점/챕터 데이터베이스", fileName = "ChapterDatabase")]
    public sealed class ChapterDatabase : ScriptableObject
    {
        [Tooltip("챕터 인덱스 순서대로 등록하는 승인 완료 별자리 챕터 목록입니다.")]
        public List<ZodiacChapterDefinition> chapters = new();

        /// <summary>
        /// 사람이 읽을 수 있는 챕터 ID로 등록된 별자리 챕터를 찾습니다.
        /// </summary>
        public ZodiacChapterDefinition Find(string chapterId)
        {
            return chapters.Find(chapter => chapter != null && chapter.id == chapterId);
        }
    }
}
