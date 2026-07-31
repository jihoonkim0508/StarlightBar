using System;
using StarlightBar.Content;
using UnityEngine;

namespace StarlightBar.Core
{
    /// <summary>
    /// 게임 진행 단계가 구현해야 하는 수명 주기 계약입니다.
    /// </summary>
    public interface IGamePhase
    {
        /// <summary>이 구현이 담당하는 게임 단계를 반환합니다.</summary>
        GamePhaseType PhaseType { get; }
        /// <summary>현재 단계에서 다음 단계로 이동할 수 있는지 반환합니다.</summary>
        bool CanAdvance { get; }
        /// <summary>단계 진입 시 세션 상태를 준비합니다.</summary>
        void Enter(GameSession session);
        /// <summary>단계 이탈 시 사용 중인 상태를 정리합니다.</summary>
        void Exit(GameSession session);
    }

    /// <summary>
    /// 조사물, NPC, 출입구가 공유하는 상호작용 계약입니다.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>화면에 표시할 한국어 상호작용 안내를 반환합니다.</summary>
        string InteractionLabel { get; }
        /// <summary>지정한 행위자가 지금 상호작용할 수 있는지 확인합니다.</summary>
        bool CanInteract(GameObject actor);
        /// <summary>지정한 행위자의 상호작용을 실행합니다.</summary>
        void Interact(GameObject actor);
    }

    /// <summary>
    /// 저장 슬롯의 읽기, 쓰기, 백업 복구를 담당합니다.
    /// </summary>
    public interface ISaveService
    {
        /// <summary>정상 저장 또는 복구 가능한 백업이 있는지 반환합니다.</summary>
        bool HasSave { get; }
        /// <summary>현재 게임 데이터를 안전하게 저장합니다.</summary>
        void Save(GameSaveData data);
        /// <summary>정상 저장을 읽고 실패하면 백업 복구를 시도합니다.</summary>
        bool TryLoad(out GameSaveData data);
        /// <summary>현재 저장 슬롯과 관련 임시 파일을 삭제합니다.</summary>
        void DeleteSave();
    }

    /// <summary>
    /// 대화 재생기와 UI 사이의 최소 계약입니다.
    /// </summary>
    public interface IDialogueRunner
    {
        /// <summary>현재 대사가 재생 중인지 반환합니다.</summary>
        bool IsPlaying { get; }
        event Action<DialogueLine> LineChanged;
        event Action DialogueCompleted;
        /// <summary>대화 정의의 진입 노드부터 재생합니다.</summary>
        void Play(DialogueDefinition dialogue);
        /// <summary>현재 대사의 선택지를 선택합니다.</summary>
        void SelectChoice(int choiceIndex);
        /// <summary>다음 대사 노드로 진행합니다.</summary>
        void Advance();
        /// <summary>현재 대화를 즉시 종료합니다.</summary>
        void Stop();
    }

    /// <summary>
    /// 현재 챕터 콘텐츠를 런타임 시스템에 제공합니다.
    /// </summary>
    public interface IChapterContentProvider
    {
        /// <summary>현재 선택된 별자리 챕터 정의를 반환합니다.</summary>
        ZodiacChapterDefinition CurrentChapter { get; }
        /// <summary>챕터 ID로 현재 콘텐츠를 변경합니다.</summary>
        bool TrySetChapter(string chapterId);
    }
}
