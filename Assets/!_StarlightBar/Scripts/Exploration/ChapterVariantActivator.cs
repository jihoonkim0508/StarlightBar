using StarlightBar.Core;
using UnityEngine;

namespace StarlightBar.Exploration
{
    /// <summary>
    /// 재사용 혜화동 씬에서 현재 챕터에 필요한 NPC와 조사물만 활성화합니다.
    /// </summary>
    public sealed class ChapterVariantActivator : MonoBehaviour
    {
        [SerializeField, Tooltip("이 오브젝트가 등장할 챕터 ID입니다.")]
        private string chapterId;
        [SerializeField, Tooltip("ID가 비어 있으면 모든 챕터에서 표시합니다.")]
        private bool alwaysActiveWhenEmpty = true;

        private void Start()
        {
            var bootstrapper = GameBootstrapper.Instance;
            if (bootstrapper == null)
                return;

            var active = string.IsNullOrWhiteSpace(chapterId)
                ? alwaysActiveWhenEmpty
                : bootstrapper.Session.Data.currentChapterId == chapterId;
            gameObject.SetActive(active);
        }
    }
}
