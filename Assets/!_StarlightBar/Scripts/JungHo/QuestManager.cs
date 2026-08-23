using System.Collections.Generic;

namespace StarlightBar.Core
{
    // 퀘스트 진행 상태(NotStarted → InProgress → Complete)를 관리.
    // SaveData.quests를 참조해서 조작 = 곧 저장 데이터 조작.
    public sealed class QuestManager
    {
        private readonly SaveData saveData;

        public QuestManager(SaveData saveData)
        {
            this.saveData = saveData;
        }

        // 현재까지 기록된 퀘스트 전체 목록 (읽기 전용)
        public IReadOnlyList<QuestEntry> Entries => saveData.quests;

        // 특정 퀘스트의 현재 상태 조회 (기록이 없으면 NotStarted)
        public QuestState GetState(string questId)
        {
            var entry = Find(questId);
            return entry?.state ?? QuestState.NotStarted;
        }

        // 완료 상태인지 확인
        public bool IsComplete(string questId) => GetState(questId) == QuestState.Complete;

        // 퀘스트 시작 처리: 기록이 없으면 InProgress로 새로 추가, 있으면 NotStarted일 때만 InProgress로 전환
        public void StartQuest(string questId)
        {
            var entry = Find(questId);
            if (entry == null)
            {
                saveData.quests.Add(new QuestEntry { questId = questId, state = QuestState.InProgress });
                GameEvents.RaiseStateChanged();
                return;
            }

            if (entry.state == QuestState.NotStarted)
            {
                entry.state = QuestState.InProgress;
                GameEvents.RaiseStateChanged();
            }
        }

        // 퀘스트 완료 처리: 기록이 없으면 Complete로 새로 추가, 있으면 Complete로 갱신
        public void CompleteQuest(string questId)
        {
            var entry = Find(questId);
            if (entry == null)
            {
                saveData.quests.Add(new QuestEntry { questId = questId, state = QuestState.Complete });
                GameEvents.RaiseStateChanged();
                return;
            }

            if (entry.state != QuestState.Complete)
            {
                entry.state = QuestState.Complete;
                GameEvents.RaiseStateChanged();
            }
        }

        // questId로 기존 퀘스트 기록 찾기 (없으면 null)
        private QuestEntry Find(string questId)
        {
            foreach (var entry in saveData.quests)
            {
                if (entry.questId == questId)
                    return entry;
            }

            return null;
        }
    }
}
