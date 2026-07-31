using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarlightBar.Content
{
    /// <summary>
    /// 별자리 손님과 별개로 주점을 방문하는 일반·특별 손님의 주문과 반응을 정의합니다.
    /// </summary>
    [Serializable]
    public sealed class SideGuestDefinition
    {
        [Tooltip("프로젝트 전체에서 중복되지 않는 방문 손님 ID입니다.")]
        public string id;
        [Tooltip("접객 UI에 표시할 한국어 손님 이름입니다.")]
        public string displayName;
        [Tooltip("별자리, 신화 존재 또는 행정국 관계자 등의 역할 설명입니다.")]
        public string role;
        [Tooltip("주문 전에 손님이 말하는 한국어 입장 대사입니다.")]
        public string openingLine;
        [Tooltip("가장 선호하는 공용 메뉴 콘텐츠 ID입니다.")]
        public string preferredMenuId;
        [Tooltip("가장 선호하는 공용 메뉴의 한국어 표시명입니다.")]
        public string preferredMenuName;
        [Tooltip("오답 선택지로 제시할 다른 공용 메뉴 이름 목록입니다.")]
        public List<string> alternativeMenuNames = new();
        [Tooltip("선호 메뉴 제공에 성공했을 때의 한국어 반응입니다.")]
        public string successLine;
        [Tooltip("다른 메뉴를 제공했을 때의 한국어 반응입니다.")]
        public string failureLine;
        [Tooltip("성공 시 지급할 재료 또는 사건 증거 ID입니다.")]
        public string rewardItemId;
        [Tooltip("획득 알림에 표시할 보상 한국어 이름입니다.")]
        public string rewardItemName;
        [Tooltip("중반·후반 사건과 연결되는 특별 방문객인지 지정합니다.")]
        public bool specialVisitor;
        [Tooltip("0부터 시작하는 방문 가능 챕터 인덱스입니다.")]
        public int unlockChapterIndex;
    }

    /// <summary>
    /// 승인 전에도 공용 접객 루프를 검증할 수 있는 혜화동 일반 손님과 사건 연계 특별 손님을 제공합니다.
    /// </summary>
    public static class SideGuestCatalog
    {
        private static readonly IReadOnlyList<SideGuestDefinition> Guests = new[]
        {
            Create("side_lyra_stagekeeper", "리라", "거문고자리의 무대지기",
                "인간 극장의 마지막 막을 지켜보고 왔어요. 목이 칼칼한데 향이 편안한 차가 있을까요?",
                "menu_herbal_tea", "달빛 허브차", new[] { "별설탕 타르트", "매운 별꼬치" },
                "향이 세지 않아 좋네요. 다음 공연 소문도 들려드릴게요.",
                "오늘은 조금 자극적이네요. 그래도 따뜻하게 쉬었다 갈게요.",
                "ingredient_stage_mint", "무대 뒤 민트", false, 0),
            Create("side_coma_observer", "코마", "머리털자리 관측 기록원",
                "인간 천문대의 관측을 돕다 보니 배가 고파요. 손에 묻지 않는 달콤한 걸 부탁해요.",
                "menu_star_tart", "별설탕 타르트", new[] { "달빛 허브차", "은하 크림수프" },
                "이 모양, 오늘 본 성단하고 닮았어요. 관측 기록을 빌려드릴게요.",
                "맛은 좋지만 관측 장비 옆에서는 먹기 어렵겠네요.",
                "ingredient_constellation_sugar", "성단 설탕", false, 0),
            Create("side_hermes_courier", "헤르메스", "신화 항로의 심야 전령",
                "지상과 천상의 경계를 오래 달렸어요. 속을 천천히 데워 주는 음식이 있나요?",
                "menu_galaxy_soup", "은하 크림수프", new[] { "별설탕 타르트", "매운 별꼬치" },
                "이제 손끝이 풀리네요. 골목에서 본 희미한 표식을 알려드릴게요.",
                "지금은 조금 부담스럽지만, 물 한 잔이면 괜찮아요.",
                "ingredient_rain_salt", "빗결 소금", false, 0),
            Create("special_bureau_inspector", "세린", "은하 행정국 조사관",
                "복원 기록의 시간축이 누군가에게 수정됐습니다. 정신을 맑게 할 차를 주시겠습니까?",
                "menu_herbal_tea", "달빛 허브차", new[] { "은하 크림수프", "별설탕 타르트" },
                "기록의 잉크가 안정됐습니다. 내부 접속 흔적을 노트에 남기죠.",
                "판단이 흐려지는군요. 오늘 기록은 보류하겠습니다.",
                "evidence_internal_access", "내부 접속 기록", true, 5),
            Create("special_antique_keeper", "마로", "검은 별자리 골동품 수집가",
                "이 주점의 잔에는 오래된 궤도 냄새가 나는군. 뜨겁지 않은 단맛을 보고 싶네.",
                "menu_star_tart", "별설탕 타르트", new[] { "달빛 허브차", "매운 별꼬치" },
                "값은 이 문양의 탁본으로 치르지. 같은 표식을 본 적이 있을 걸세.",
                "취향이 맞지 않는군. 문양 이야기는 다음에 하지.",
                "evidence_black_rubbing", "검은 문양 탁본", true, 8),
            Create("special_restored_guest", "먼저 돌아온 별", "복원된 12궁의 전령",
                "스텔라의 결계가 약해졌어요. 모두가 나눠 마실 따뜻한 수프가 필요해요.",
                "menu_galaxy_soup", "은하 크림수프", new[] { "별설탕 타르트", "달빛 허브차" },
                "이 온기를 열두 별에게 전할게요. 마지막 결계는 혼자 세우지 않을 거예요.",
                "우리의 빛이 흩어졌어요. 다시 준비해서 만나요.",
                "item_twelve_star_resonance", "열두 별의 공명", true, 10)
        };

        /// <summary>
        /// 현재 챕터에 방문 가능한 일반 손님 한 명과 해금된 특별 손님을 반환합니다.
        /// </summary>
        public static IReadOnlyList<SideGuestDefinition> GetVisitors(int chapterIndex)
        {
            var result = new List<SideGuestDefinition>
            {
                Guests[chapterIndex % 3]
            };
            foreach (var guest in Guests)
            {
                if (guest.specialVisitor && guest.unlockChapterIndex == chapterIndex)
                    result.Add(guest);
            }
            return result;
        }

        private static SideGuestDefinition Create(
            string id, string name, string role, string opening, string menuId, string menuName,
            IEnumerable<string> alternatives, string success, string failure,
            string rewardId, string rewardName, bool special, int unlock)
        {
            return new SideGuestDefinition
            {
                id = id,
                displayName = name,
                role = role,
                openingLine = opening,
                preferredMenuId = menuId,
                preferredMenuName = menuName,
                alternativeMenuNames = new List<string>(alternatives),
                successLine = success,
                failureLine = failure,
                rewardItemId = rewardId,
                rewardItemName = rewardName,
                specialVisitor = special,
                unlockChapterIndex = unlock
            };
        }
    }
}
