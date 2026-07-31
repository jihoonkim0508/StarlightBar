using StarlightBar.Core;
using StarlightBar.UI;
using UnityEngine;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 프롤로그와 엔딩의 핵심 이야기를 한국어 텍스트 장면으로 제공합니다.
    /// </summary>
    public sealed class NarrativeScenePresenter : MonoBehaviour
    {
        [SerializeField, Tooltip("씬에 배치된 서사 화면 참조입니다.")]
        private NarrativeView view;

        private void Start()
        {
            if (GameBootstrapper.Instance == null || view == null)
                return;

            var ending = GameBootstrapper.Instance.Flow.CurrentPhase == GamePhaseType.Ending;
            view.Title.text = ending ? "별빛은 다시 길을 비춘다" : "비 오는 밤의 별빛주점";
            var playerName = GameBootstrapper.Instance.Session.Data.playerName;
            view.Body.text = ending
                ? $"{playerName}은 스텔라가 돌아간 북극성을 올려다보았다.\n\n" +
                  "열두 별은 하늘 또는 지상에서 각자의 선택을 살아 냈다. 은하 행정국은 존재를 업무 대상으로만 다뤘던 정책을 고치고, " +
                  "별들의 기억과 선택을 함께 보존하는 새 지침을 발표했다.\n\n" +
                  "스텔라는 더 이상 모든 길을 혼자 비추지 않겠다고 약속한 뒤 북극성의 자리로 돌아갔다. " +
                  "창문 너머 기준점이 다시 빛나자, 별빛주점의 열쇠는 주인공의 손에서 따뜻해졌다.\n\n" +
                  "이제 별빛주점의 새 주인이 된 그녀는 길을 잃은 인간 손님을 위한 첫 잔을 준비한다. " +
                  "별의 이야기를 복원하며 배운 용기, 유대, 경계와 애도의 말을 인간의 이야기로 이어 주기 위해서였다."
                : $"스물여덟 살 데이터 분석가였던 {playerName}은 숫자와 야근 속에서 번아웃을 견뎠다. " +
                  "가족이 이제는 원하는 일을 해도 된다고 말했을 때, 정작 자신이 무엇을 원하는지 모른다는 사실을 깨닫고 회사를 그만두었다. " +
                  "천문학자를 꿈꾸던 어린 시절도 오래전에 현실과 타협해 접어 둔 뒤였다.\n\n" +
                  "비 내리는 혜화동 골목에서 아버지가 남긴 오래된 망원경이 희미하게 빛났다. 평범한 폐건물처럼 보이던 문 너머에는 " +
                  "은빛 머리의 주인장 스텔라가 기다리고 있었다. 이곳은 방향을 잃었거나 별을 간절히 그리워하는 존재, " +
                  "소중한 별의 기억을 간직했거나 다른 이의 이야기를 들을 수 있는 사람, 그리고 스텔라가 직접 초대한 손님에게만 보이는 주점이었다.\n\n" +
                  "스텔라는 분신이 아닌 본래 몸으로 지상에 내려온 북극성이자, 은하 행정국 별자리 안내팀의 지상 지부장이었다. " +
                  "행정국이 대형 블랙홀 사건에 대응하는 사이 연쇄 추락한 황도 12궁을 찾기 위해, 길 잃은 별을 인도하는 주점을 열었다고 했다.\n\n" +
                  "지상 작전은 정식 승인을 받았고 운영비와 급여, 마법 재료, 추적 지원도 제공됐다. " +
                  "다만 인간의 감정과 요리를 이해하지 못하는 스텔라에게는 별들의 지상 생활을 듣고 연결할 인간 조력자가 필요했다.\n\n" +
                  "스텔라는 어린 시절 길을 잃었던 주인공과 그녀의 아버지를 기억했다. 아버지에게 망원경을 건넨 사람도 스텔라였다. " +
                  "주인공이 잊었던 꿈은 사라진 것이 아니라 오랫동안 방향을 잃고 있었을 뿐이었다.\n\n" +
                  "“길을 잃은 별들의 이야기를 함께 찾아주시겠어요?”";
            view.AdvanceButtonLabel.text = ending ? "메인 메뉴로" : "스텔라의 제안을 받아들인다";
            view.AdvanceButton.onClick.RemoveAllListeners();
            view.AdvanceButton.onClick.AddListener(Advance);
            view.Status.text = "Enter 또는 Space로도 진행할 수 있습니다.";
        }

        private void Advance()
        {
            if (!GameBootstrapper.Instance.Runtime.TryAdvance(out var reason) && view?.Status != null)
                view.Status.text = reason;
        }
    }
}
