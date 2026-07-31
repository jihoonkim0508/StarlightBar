using StarlightBar.Content;
using StarlightBar.Core;
using UnityEngine;

namespace StarlightBar.Systems
{
    /// <summary>
    /// 챕터 데이터베이스에서 현재 별자리 콘텐츠를 선택해 제공합니다.
    /// </summary>
    public sealed class ChapterContentProvider : MonoBehaviour, IChapterContentProvider
    {
        [SerializeField, Tooltip("빌드에 포함할 별자리 챕터 목록입니다.")]
        private ChapterDatabase database;

        public ZodiacChapterDefinition CurrentChapter { get; private set; }

        private void Awake()
        {
            if (database != null && database.chapters.Count > 0)
                CurrentChapter = database.chapters[0];
        }

        /// <summary>
        /// 데이터베이스에서 챕터 ID를 찾아 현재 콘텐츠로 설정합니다.
        /// </summary>
        public bool TrySetChapter(string chapterId)
        {
            var chapter = database == null ? null : database.Find(chapterId);
            if (chapter == null)
                return false;

            CurrentChapter = chapter;
            return true;
        }
    }
}
