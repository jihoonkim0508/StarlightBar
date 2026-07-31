using System;
using System.Collections.Generic;
using System.Linq;
using StarlightBar.Content;
using StarlightBar.Core;
using StarlightBar.Systems;
using StarlightBar.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 야간 시퀀스 안에서 일반 손님과 사건 연계 특별 손님의 짧은 주문 접객을 처리합니다.
    /// </summary>
    public sealed class SideGuestServicePresenter
    {
        private readonly Transform parent;
        private readonly GameRuntimeCoordinator runtime;
        private readonly Action<string> setStatus;
        private readonly GameSaveData save;

        /// <summary>
        /// 현재 야간 화면과 게임 진행 상태에 연결된 추가 손님 접객 화면을 만듭니다.
        /// </summary>
        public SideGuestServicePresenter(
            Transform contentParent, GameRuntimeCoordinator coordinator, Action<string> statusCallback)
        {
            parent = contentParent;
            runtime = coordinator;
            setStatus = statusCallback;
            save = GameBootstrapper.Instance.Session.Data;
        }

        /// <summary>
        /// 현재 챕터의 일반 손님과 해금된 특별 손님 주문을 조작 가능한 메뉴로 만듭니다.
        /// </summary>
        public void Build()
        {
            DynamicContentFactory.CreateText(parent, "혜화동 손님 접객", 26);
            foreach (var visitor in SideGuestCatalog.GetVisitors(runtime.CurrentChapter.chapterIndex))
                BuildVisitor(visitor);
        }

        private void BuildVisitor(SideGuestDefinition visitor)
        {
            var served = save.servedSideGuestIds.Contains(VisitId(visitor));
            var heading = DynamicContentFactory.CreateText(
                parent,
                $"{(visitor.specialVisitor ? "[특별 방문]" : "[일반 손님]")} {visitor.displayName} · {visitor.role}",
                20);
            heading.GetComponent<LayoutElement>().preferredHeight = 52;
            var request = DynamicContentFactory.CreateText(parent, visitor.openingLine, 18);
            request.GetComponent<LayoutElement>().preferredHeight = 70;

            if (served)
            {
                DynamicContentFactory.CreateText(parent, "✓ 오늘의 접객 기록 완료", 17);
                return;
            }

            var menuNames = new List<string> { visitor.preferredMenuName };
            menuNames.AddRange(visitor.alternativeMenuNames);
            // 챕터마다 버튼 순서를 바꿔 정답 위치를 암기하는 대신 주문 문장을 읽도록 합니다.
            var offset = runtime.CurrentChapter.chapterIndex % menuNames.Count;
            menuNames = menuNames.Skip(offset).Concat(menuNames.Take(offset)).ToList();
            foreach (var menuName in menuNames)
            {
                var selected = menuName;
                DynamicContentFactory.CreateButton(parent, selected, () => Serve(visitor, selected));
            }
        }

        private void Serve(SideGuestDefinition visitor, string menuName)
        {
            var visitId = VisitId(visitor);
            if (save.servedSideGuestIds.Contains(visitId))
                return;

            save.servedSideGuestIds.Add(visitId);
            if (menuName == visitor.preferredMenuName)
            {
                runtime.Inventory.Add(visitor.rewardItemId, 1);
                save.sideGuestReputation = Mathf.Clamp(save.sideGuestReputation + 2, 0, 100);
                setStatus($"{visitor.successLine}\n획득: {visitor.rewardItemName}");
            }
            else
            {
                save.sideGuestReputation = Mathf.Clamp(save.sideGuestReputation + 1, 0, 100);
                setStatus(visitor.failureLine);
            }
            GameBootstrapper.Instance.SaveNow();
        }

        private string VisitId(SideGuestDefinition visitor) =>
            $"{runtime.CurrentChapter.id}:{visitor.id}";
    }
}
