using System;
using System.Collections.Generic;
using StarlightBar.Content;
using UnityEngine;

namespace StarlightBar.Core
{
    /// <summary>
    /// 실행 중인 게임의 변경 가능한 상태를 한곳에 보관합니다.
    /// </summary>
    [Serializable]
    public sealed class GameSession
    {
        [SerializeField, Tooltip("현재 실행 중이며 저장 서비스가 직렬화하는 게임 데이터입니다.")]
        private GameSaveData data = GameSaveData.CreateNew();

        public GameSaveData Data => data;

        /// <summary>
        /// 새 게임 상태로 세션을 초기화합니다.
        /// </summary>
        public void Reset(string playerName)
        {
            // 화면·접근성·키 설정은 플레이 기록과 별개인 사용자 환경이므로 새 게임에서도 유지한다.
            var preservedSettings = CloneSettings(data?.settings);
            data = GameSaveData.CreateNew();
            data.playerName = string.IsNullOrWhiteSpace(playerName) ? "별지기" : playerName.Trim();
            data.settings = preservedSettings;
        }

        /// <summary>
        /// 저장 데이터의 방어적 복사본으로 세션을 교체합니다.
        /// </summary>
        public void Restore(GameSaveData saveData)
        {
            data = saveData?.Clone() ?? GameSaveData.CreateNew();
        }

        /// <summary>
        /// 이어하기 전 메인 메뉴에서도 저장된 화면·접근성·입력 설정만 복원합니다.
        /// </summary>
        public void RestoreSettings(GameSettingsData savedSettings)
        {
            data.settings = CloneSettings(savedSettings);
        }

        private static GameSettingsData CloneSettings(GameSettingsData settings)
        {
            return settings == null
                ? new GameSettingsData()
                : JsonUtility.FromJson<GameSettingsData>(JsonUtility.ToJson(settings));
        }
    }

    /// <summary>
    /// JSON으로 영속화하는 최상위 저장 데이터입니다.
    /// </summary>
    [Serializable]
    public sealed class GameSaveData
    {
        public const int CurrentVersion = 4;

        public int version = CurrentVersion;
        public string playerName = "별지기";
        public string currentChapterId = "chapter_leo";
        public int currentChapterIndex;
        public GamePhaseType currentPhase = GamePhaseType.MainMenu;
        public int currentGameMinute = 540;
        public List<InventoryEntry> inventory = new();
        public List<string> ownedFurnitureIds = new();
        public List<FurniturePlacementData> furniturePlacements = new();
        public List<string> collectedEvidenceIds = new();
        public List<string> completedObjectiveIds = new();
        public List<EvidenceLinkData> evidenceLinks = new();
        public List<GuestProgressData> guestProgress = new();
        public List<string> completedChapterIds = new();
        public List<string> readDialogueLineIds = new();
        public List<DialogueHistoryEntry> dialogueHistory = new();
        public List<string> storyFlagIds = new();
        public List<string> servedSideGuestIds = new();
        public int sideGuestReputation;
        public bool currentPreparationComplete;
        public List<string> completedPreparationTaskIds = new();
        public bool currentCookingComplete;
        public bool currentNightIntroductionComplete;
        public CookingQuality currentCookingQuality;
        public List<FoodClueRecord> foodClueRecords = new();
        public bool currentDeductionComplete;
        public List<string> completedMemoryObjectiveIds = new();
        public bool currentMemoryEchoHeard;
        public int currentGuestTrust;
        public int currentGuestStability;
        public int currentGuestMemory;
        public int preparationBonusMinutes;
        public GameSettingsData settings = new();

        /// <summary>
        /// 새 게임에 사용할 기본 저장 데이터를 생성합니다.
        /// </summary>
        public static GameSaveData CreateNew() => new();

        /// <summary>
        /// 저장 작업 중 런타임 변경과 분리된 깊은 복사본을 만듭니다.
        /// </summary>
        public GameSaveData Clone()
        {
            return JsonUtility.FromJson<GameSaveData>(JsonUtility.ToJson(this));
        }
    }

    /// <summary>
    /// 저장 가능한 인벤토리 품목과 수량입니다.
    /// </summary>
    [Serializable]
    public sealed class InventoryEntry
    {
        public string itemId;
        public int quantity;
    }

    /// <summary>
    /// 주점에 배치하거나 보관한 가구의 위치와 회전 상태입니다.
    /// </summary>
    [Serializable]
    public sealed class FurniturePlacementData
    {
        public string furnitureId;
        public Vector2 position;
        public float rotation;
        public bool stored;
    }

    /// <summary>
    /// 플레이어가 연결한 두 증거 ID를 저장합니다.
    /// </summary>
    [Serializable]
    public sealed class EvidenceLinkData
    {
        public string firstEvidenceId;
        public string secondEvidenceId;
    }

    /// <summary>
    /// 별자리 손님의 상태·복원 등급·개인 선택 기록입니다.
    /// </summary>
    [Serializable]
    public sealed class GuestProgressData
    {
        public string characterId;
        public GuestTrustStage trust = GuestTrustStage.Guarded;
        public GuestStabilityStage stability = GuestStabilityStage.Distressed;
        public GuestMemoryStage memory = GuestMemoryStage.None;
        public RestorationGrade restorationGrade;
        public GuestFutureChoice futureChoice;
        public bool completed;
    }

    /// <summary>
    /// 조리 품질에 따라 달라지는 기억 단서의 명확도를 챕터 기록으로 보관합니다.
    /// </summary>
    [Serializable]
    public sealed class FoodClueRecord
    {
        public string chapterId;
        public CookingQuality quality;
        public string clarityText;
        public List<string> effectLabels = new();
    }

    /// <summary>
    /// 이어하기 이후에도 대화 로그에서 확인할 수 있는 화자와 한국어 본문 기록입니다.
    /// </summary>
    [Serializable]
    public sealed class DialogueHistoryEntry
    {
        public string chapterId;
        public string lineId;
        public string speaker;
        public string text;
    }

    /// <summary>
    /// 화면·대화·접근성·입력·음량 설정을 저장합니다.
    /// </summary>
    [Serializable]
    public sealed class GameSettingsData
    {
        public int resolutionWidth = 1920;
        public int resolutionHeight = 1080;
        public int fullscreenMode = (int)FullScreenMode.FullScreenWindow;
        public float brightness = 1f;
        public float masterVolume = 1f;
        public float musicVolume = 0.8f;
        public float effectsVolume = 0.8f;
        public float ambientVolume = 0.8f;
        public float voiceVolume = 1f;
        public float dialogueSpeed = 1f;
        public float autoAdvanceSpeed = 1f;
        public float textScale = 1f;
        public float uiScale = 1f;
        public float dialogueOpacity = 0.9f;
        public float mouseSensitivity = 1f;
        public bool reduceScreenShake;
        public bool reduceFlashing;
        public bool chromaticAberration = true;
        public bool skipReadText;
        public bool allowFullSkip;
        public bool autoAdvance;
        public KeyBindingData keyBindings = KeyBindingData.CreateDefault();
    }

    /// <summary>
    /// 키보드 조작을 저장 가능한 Input System Key 값으로 보관합니다.
    /// </summary>
    [Serializable]
    public sealed class KeyBindingData
    {
        public int moveUp = (int)UnityEngine.InputSystem.Key.W;
        public int moveDown = (int)UnityEngine.InputSystem.Key.S;
        public int moveLeft = (int)UnityEngine.InputSystem.Key.A;
        public int moveRight = (int)UnityEngine.InputSystem.Key.D;
        public int inspect = (int)UnityEngine.InputSystem.Key.F;
        public int talk = (int)UnityEngine.InputSystem.Key.E;
        public int notebook = (int)UnityEngine.InputSystem.Key.J;
        public int objectives = (int)UnityEngine.InputSystem.Key.Tab;
        public int telescope = (int)UnityEngine.InputSystem.Key.Digit1;
        public int menu = (int)UnityEngine.InputSystem.Key.Escape;

        /// <summary>
        /// 기획서의 기본 조작 키가 설정된 키 바인딩을 생성합니다.
        /// </summary>
        public static KeyBindingData CreateDefault() => new();

        /// <summary>
        /// 게임 동작에 현재 할당된 키를 반환합니다.
        /// </summary>
        public UnityEngine.InputSystem.Key Get(GameInputAction action) =>
            (UnityEngine.InputSystem.Key)(action switch
            {
                GameInputAction.MoveUp => moveUp,
                GameInputAction.MoveDown => moveDown,
                GameInputAction.MoveLeft => moveLeft,
                GameInputAction.MoveRight => moveRight,
                GameInputAction.Inspect => inspect,
                GameInputAction.Talk => talk,
                GameInputAction.Notebook => notebook,
                GameInputAction.Objectives => objectives,
                GameInputAction.Telescope => telescope,
                _ => menu
            });

        /// <summary>
        /// 게임 동작에 새 키를 할당합니다.
        /// </summary>
        public void Set(GameInputAction action, UnityEngine.InputSystem.Key key)
        {
            switch (action)
            {
                case GameInputAction.MoveUp: moveUp = (int)key; break;
                case GameInputAction.MoveDown: moveDown = (int)key; break;
                case GameInputAction.MoveLeft: moveLeft = (int)key; break;
                case GameInputAction.MoveRight: moveRight = (int)key; break;
                case GameInputAction.Inspect: inspect = (int)key; break;
                case GameInputAction.Talk: talk = (int)key; break;
                case GameInputAction.Notebook: notebook = (int)key; break;
                case GameInputAction.Objectives: objectives = (int)key; break;
                case GameInputAction.Telescope: telescope = (int)key; break;
                case GameInputAction.Menu: menu = (int)key; break;
            }
        }
    }
}
