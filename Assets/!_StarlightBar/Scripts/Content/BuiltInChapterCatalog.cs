using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StarlightBar.Content
{
    /// <summary>
    /// 에디터 콘텐츠가 아직 생성되지 않은 개발 빌드에서도 12궁 전체를 진행할 수 있도록
    /// 승인된 설정과 보완 설정을 런타임 ScriptableObject로 제공합니다.
    /// </summary>
    public static class BuiltInChapterCatalog
    {
        private static IReadOnlyList<ZodiacChapterDefinition> chapters;
        private static readonly Dictionary<string, string> Labels = new(StringComparer.Ordinal);

        /// <summary>
        /// 레오·스코피·피시즈는 원 기획 설정을 따르고, 나머지는 같은 제작 게이트에 맞춰 보완한 12궁 목록을 반환합니다.
        /// </summary>
        public static IReadOnlyList<ZodiacChapterDefinition> GetChapters()
        {
            return chapters ??= BuildProfiles().Select(CreateChapter).ToArray();
        }

        /// <summary>
        /// 내부 콘텐츠 ID를 한국어 UI 표시명으로 변환합니다.
        /// </summary>
        public static string GetLabel(string contentId)
        {
            return string.IsNullOrWhiteSpace(contentId)
                ? "-"
                : Labels.TryGetValue(contentId, out var label) ? label : contentId;
        }

        /// <summary>
        /// 모든 챕터에서 영구 보유 가구 ID에 해당하는 정의를 찾습니다.
        /// </summary>
        public static FurnitureDefinition FindFurniture(string furnitureId)
        {
            return GetChapters()
                .SelectMany(chapter => chapter.obtainableFurniture)
                .FirstOrDefault(item => item != null && item.id == furnitureId);
        }

        private static ZodiacChapterDefinition CreateChapter(ChapterProfile profile)
        {
            Register(profile.ZodiacId, profile.ZodiacName);
            Register(profile.MythId, profile.MythTitle);

            var guest = Create<CharacterDefinition>($"Character_{profile.ZodiacId}");
            guest.id = $"character_{profile.ZodiacId}";
            guest.displayName = profile.GuestName;
            Register(guest.id, guest.displayName);
            guest.description =
                $"{profile.CurrentLife}\n감정 주제: {profile.EmotionalTheme}\n트라우마 반응: {profile.TraumaReaction}";
            guest.themeColor = profile.Palette;
            guest.preferredFurnitureTraits = new List<FurnitureTrait> { profile.PreferredTrait };
            guest.rejectedFurnitureTraits = new List<FurnitureTrait> { profile.RejectedTrait };

            var identity = CreateEvidence(profile, "identity", $"{profile.ZodiacName}의 흔적",
                profile.IdentityClue, EvidenceCategory.Identity, true);
            var myth = CreateEvidence(profile, "myth", profile.MythTitle,
                profile.MythSummary, EvidenceCategory.Myth, true);
            var human = CreateEvidence(profile, "human", "지상 생활 기록",
                profile.CurrentLife, EvidenceCategory.HumanLife, true);
            var food = CreateEvidence(profile, "food", $"{profile.DishName} 반응",
                profile.FoodReaction, EvidenceCategory.FoodReaction, false);
            var interior = CreateEvidence(profile, "interior", "가구 반응",
                profile.InteriorReaction, EvidenceCategory.InteriorReaction, false);
            var evidence = new List<EvidenceDefinition> { identity, myth, human, food, interior };
            foreach (var item in evidence)
            {
                item.supportedCandidateIds.Add(profile.ZodiacId);
                item.allowedLinkEvidenceIds = evidence.Where(other => other != item).Select(other => other.id).ToList();
            }

            var furniture = Create<FurnitureDefinition>($"Furniture_{profile.ZodiacId}");
            furniture.id = $"furniture_{profile.ZodiacId}_{profile.PreferredTrait.ToString().ToLowerInvariant()}";
            furniture.displayName = profile.FurnitureName;
            furniture.traits = new List<FurnitureTrait> { profile.PreferredTrait };
            Register(furniture.id, furniture.displayName);

            var ingredientId = $"ingredient_{profile.ZodiacId}_main";
            var garnishId = $"ingredient_{profile.ZodiacId}_garnish";
            var magicId = $"magic_{profile.ZodiacId}";
            Register(ingredientId, profile.MainIngredient);
            Register(garnishId, profile.GarnishIngredient);
            Register(magicId, profile.MagicalIngredient);

            var recipe = Create<RecipeDefinition>($"Recipe_{profile.ZodiacId}");
            recipe.id = $"recipe_{profile.ZodiacId}";
            recipe.displayName = profile.DishName;
            recipe.expectedEffectHint = profile.FoodReaction;
            recipe.effects = new List<CookingEffect>
            {
                CookingEffect.Stability,
                CookingEffect.Trust,
                CookingEffect.Memory,
                CookingEffect.Truth,
                CookingEffect.Empathy,
                CookingEffect.Connection
            };
            recipe.steps = new List<RecipeStep>
            {
                new() { ingredientId = ingredientId, method = profile.MainMethod, order = 0 },
                new() { ingredientId = garnishId, method = profile.GarnishMethod, order = 1 }
            };
            recipe.decorationId = $"decoration_{profile.ZodiacId}";
            recipe.magicalIngredientId = magicId;
            recipe.allowedIngredientIds = new List<string> { ingredientId, garnishId };
            Register(recipe.decorationId, profile.Decoration);
            Register(recipe.id, recipe.displayName);

            var objectives = new List<ObjectiveDefinition>
            {
                CreateObjective(profile, "ingredient", $"재료 확보 · {profile.MainIngredient}",
                    ObjectiveType.RequiredIngredient, true, 20, ingredientId),
                CreateObjective(profile, "myth", $"신화 조사 · {profile.MythTitle}",
                    ObjectiveType.MythEvidence, true, 30, myth.id),
                CreateObjective(profile, "human", $"인물 조사 · {profile.GuestName}",
                    ObjectiveType.HumanLifeTrace, true, 25, human.id),
                CreateObjective(profile, "furniture", $"가구 찾기 · {profile.FurnitureName}",
                    ObjectiveType.Furniture, false, 20, furniture.id)
            };

            var deduction = Create<DeductionDefinition>($"Deduction_{profile.ZodiacId}");
            deduction.id = $"deduction_{profile.ZodiacId}";
            deduction.correctZodiacId = profile.ZodiacId;
            deduction.correctMythId = profile.MythId;
            deduction.requiredCoreEvidenceIds = new List<string> { identity.id, myth.id, human.id };
            deduction.zodiacCandidateIds = BuildCandidates(profile.ZodiacId, profile.DistractorZodiacs);
            deduction.mythCandidateIds = BuildCandidates(profile.MythId, profile.DistractorMyths);

            var memory = Create<MemorySpaceDefinition>($"Memory_{profile.ZodiacId}");
            memory.id = $"memory_{profile.ZodiacId}";
            memory.sceneVariantId = profile.MemoryModule;
            memory.palette = profile.Palette;
            memory.objectiveIds = new List<string>
            {
                $"memory_{profile.ZodiacId}_truth",
                $"memory_{profile.ZodiacId}_protect",
                $"memory_{profile.ZodiacId}_accept"
            };
            memory.objectiveTitles = new List<string>
            {
                profile.MemoryTruthObjective,
                profile.MemoryProtectObjective,
                profile.MemoryAcceptObjective
            };
            memory.mechanicModuleIds = new List<string> { profile.MemoryModule };
            memory.keyMemoryObjectId = $"key_memory_{profile.ZodiacId}";
            for (var index = 0; index < memory.objectiveIds.Count; index++)
                Register(memory.objectiveIds[index], memory.objectiveTitles[index]);

            var chapter = Create<ZodiacChapterDefinition>($"Chapter_{profile.ZodiacId}");
            chapter.id = $"chapter_{profile.ZodiacId}";
            chapter.chapterIndex = profile.Index;
            chapter.title = $"{profile.Index + 1}장 · {profile.ZodiacName} — {profile.EmotionalTheme}";
            chapter.mythologySource = profile.MythSummary;
            chapter.emotionalTheme = profile.EmotionalTheme;
            chapter.currentLife = profile.CurrentLife;
            chapter.traumaReaction = profile.TraumaReaction;
            chapter.guest = guest;
            chapter.objectives = objectives;
            chapter.evidence = evidence;
            chapter.obtainableFurniture = new List<FurnitureDefinition> { furniture };
            chapter.briefingDialogue = CreateDialogue(
                $"dialogue_{profile.ZodiacId}_briefing",
                ("스텔라", $"{profile.GuestName} 님의 별빛이 혜화동에 닿았답니다."),
                ("스텔라", $"{profile.EmotionalTheme}의 흔적과 {profile.MythTitle} 기록을 찾아 주세요."),
                ("주인공", "사실과 감정을 함께 확인해 볼게요."));
            chapter.nightDialogue = CreateNightDialogue(profile);
            chapter.completeRestorationText = profile.CompleteText;
            chapter.partialRestorationText = profile.PartialText;
            chapter.unstableRestorationText = profile.UnstableText;
            chapter.returnToSkyChoiceText =
                $"{profile.GuestName}은 {profile.MythTitle}의 기억을 새 빛으로 받아들이고 " +
                $"{profile.ZodiacName}의 자리로 돌아가 길을 비추기로 했다.";
            chapter.remainHumanWithMemoriesChoiceText =
                $"{profile.GuestName}은 지상에서 맺은 관계와 {profile.ZodiacName}의 기억을 함께 품고 " +
                "지금의 인간 생활을 자신의 선택으로 이어 가기로 했다.";
            chapter.remainHumanWithoutIdentityChoiceText =
                $"{profile.GuestName}은 {profile.MythTitle}의 이름을 기록 보관소에 맡기고, " +
                "천상의 의무가 아닌 스스로 고른 인간의 미래를 살기로 했다.";
            chapter.specialRecipe = recipe;
            chapter.deduction = deduction;
            chapter.memorySpace = memory;
            Register(chapter.id, chapter.title);
            return chapter;
        }

        private static EvidenceDefinition CreateEvidence(
            ChapterProfile profile,
            string suffix,
            string title,
            string description,
            EvidenceCategory category,
            bool core)
        {
            var evidence = Create<EvidenceDefinition>($"Evidence_{profile.ZodiacId}_{suffix}");
            evidence.id = $"evidence_{profile.ZodiacId}_{suffix}";
            evidence.title = title;
            evidence.description = description;
            evidence.category = category;
            evidence.coreEvidence = core;
            Register(evidence.id, title);
            return evidence;
        }

        private static ObjectiveDefinition CreateObjective(
            ChapterProfile profile,
            string suffix,
            string title,
            ObjectiveType type,
            bool mandatory,
            int time,
            string targetId)
        {
            var objective = Create<ObjectiveDefinition>($"Objective_{profile.ZodiacId}_{suffix}");
            objective.id = $"objective_{profile.ZodiacId}_{suffix}";
            objective.title = title;
            objective.description = $"{profile.GuestName}의 하루를 복원하기 위한 {(mandatory ? "필수" : "선택")} 조사입니다.";
            objective.type = type;
            objective.mandatory = mandatory;
            objective.timeCostMinutes = time;
            objective.targetContentId = targetId;
            Register(objective.id, title);
            return objective;
        }

        private static DialogueDefinition CreateDialogue(string id, params (string speaker, string text)[] source)
        {
            var dialogue = Create<DialogueDefinition>(id);
            dialogue.id = id;
            for (var index = 0; index < source.Length; index++)
            {
                dialogue.lines.Add(new DialogueLine
                {
                    id = $"{id}_{index + 1:00}",
                    speakerId = source[index].speaker,
                    text = source[index].text
                });
            }
            dialogue.entryLineId = dialogue.lines[0].id;
            return dialogue;
        }

        private static DialogueDefinition CreateNightDialogue(ChapterProfile profile)
        {
            var id = $"dialogue_{profile.ZodiacId}_night";
            var dialogue = Create<DialogueDefinition>(id);
            dialogue.id = id;
            var opening = new DialogueLine
            {
                id = $"{id}_01",
                speakerId = profile.GuestName,
                text = profile.NightOpening
            };
            opening.choices.Add(new DialogueChoice
            {
                id = $"{id}_choice_empathy",
                text = profile.PlayerResponse,
                nextLineId = $"{id}_02",
                trustDelta = 12,
                stabilityDelta = 8
            });
            opening.choices.Add(new DialogueChoice
            {
                id = $"{id}_choice_analysis",
                text = "기록과 반응을 먼저 차근차근 확인해 볼게요.",
                nextLineId = $"{id}_02",
                trustDelta = 4,
                stabilityDelta = 2
            });
            dialogue.lines.Add(opening);
            dialogue.lines.Add(new DialogueLine
            {
                id = $"{id}_02",
                speakerId = profile.GuestName,
                text = profile.NightResolution,
                evidenceId = $"evidence_{profile.ZodiacId}_food"
            });
            dialogue.entryLineId = opening.id;
            return dialogue;
        }

        private static List<string> BuildCandidates(string correct, IReadOnlyList<string> distractors)
        {
            var result = new List<string> { correct };
            if (distractors != null)
                result.AddRange(distractors.Where(item => !string.IsNullOrWhiteSpace(item) && item != correct));
            return result.Distinct().Take(3).ToList();
        }

        private static T Create<T>(string name) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            asset.name = name;
            asset.hideFlags = HideFlags.DontSave;
            return asset;
        }

        private static void Register(string id, string label)
        {
            if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(label))
                Labels[id] = label;
        }

        private static IReadOnlyList<ChapterProfile> BuildProfiles()
        {
            return new[]
            {
                new ChapterProfile(0, "leo", "사자자리", "레오", "네메아의 사자", "myth_nemean_lion",
                    "구속과 방어, 완벽해야 한다는 압박",
                    "낡은 복싱 체육관의 코치이자 관장 대행으로 체육관 뒷방에 산다.",
                    "목을 조이는 넥타이·사슬·밧줄에 공황 반응을 보인다.",
                    "무기가 통하지 않는 단단한 피부와 사자 송곳니 장식",
                    "헤라클레스가 무기가 통하지 않는 네메아의 사자를 맨손으로 질식시킨 첫 번째 과업.",
                    "직화 토마토 스테이크", "쇠고기", "토마토", "기억의 별가루", "불꽃 허브",
                    "직화의 온기를 느끼며 경계가 풀리고 지키는 힘을 떠올린다.",
                    "목을 조이는 장식에는 움츠러들고 따뜻하고 안정적인 빛에는 안도한다.",
                    "따뜻한 수호등", FurnitureTrait.Stability, FurnitureTrait.Mystery,
                    CookingMethod.Grill, CookingMethod.Bake, new Color(0.78f, 0.34f, 0.18f),
                    "단단한 피부가 두려움을 숨기기 위한 갑옷이었음을 판별하기",
                    "아이들을 지켰던 용기의 기억을 사슬 파편에서 보호하기",
                    "강함은 버티는 것이 아니라 지키는 선택임을 받아들이기",
                    "moving_chains",
                    "목에 닿는 건 싫다. 그래도… 이 냄새는 따뜻하군.",
                    "강해야만 지킬 수 있는 게 아니라, 두려워도 곁에 남는 게 용기예요.",
                    "내 가죽은 누군가를 겁주기 위한 게 아니었군. 이제 알겠다.",
                    "레오는 지키는 힘을 온전히 받아들였다.",
                    "레오는 상처를 인정했지만 아직 힘을 내려놓는 연습이 필요하다.",
                    "레오는 기억을 되찾았으나 완벽해야 한다는 압박을 놓지 못했다.",
                    new[] { "scorpio", "pisces" }, new[] { "myth_orion_scorpion", "myth_typhon_escape" }),

                new ChapterProfile(1, "scorpio", "전갈자리", "스코피", "오리온과 전갈", "myth_orion_scorpion",
                    "명령과 복종, 감정 소진과 충성",
                    "주점 뒤 어두운 골목의 지하 가구 공방을 운영하며 안쪽 침실에서 산다.",
                    "명령조의 말과 과도한 밝기·소음에 얼어붙고 감정을 차단한다.",
                    "독침을 닮은 조각도와 소리 없이 대상을 추적하는 습관",
                    "가이아가 오만한 사냥꾼 오리온을 벌하기 위해 보낸 전갈이 독침으로 그를 쓰러뜨린 이야기.",
                    "시트러스 허브 차", "시트러스", "쌉쌀한 허브", "궤도 안정 향신료", "말린 껍질",
                    "쓴 향이 집착을 가라앉히고 명령이 아닌 자신의 선택을 말하게 한다.",
                    "강한 조명은 거부하지만 차분하고 어두운 작업등에는 오래 머문다.",
                    "그림자 작업등", FurnitureTrait.Calm, FurnitureTrait.Vitality,
                    CookingMethod.Infuse, CookingMethod.Grind, new Color(0.22f, 0.12f, 0.38f),
                    "명령으로 조작된 기억과 자신의 의지를 구분하기",
                    "스스로 멈추기로 한 첫 선택의 기억을 보호하기",
                    "충성은 복종이 아니라 지키겠다는 자발적 약속임을 받아들이기",
                    "shadow_pursuit",
                    "명령은 필요 없어. 무엇을 해야 하는지는 내가 판단해.",
                    "오늘은 해야 하는 일이 아니라, 하고 싶은 일을 골라도 돼요.",
                    "추격을 멈추는 것도 내 선택이군. 지킬 대상을 정하는 것도.",
                    "스코피는 명령 밖에서 자신의 충성을 선택했다.",
                    "스코피는 감정을 되찾았지만 타인에게 기대는 일은 아직 낯설다.",
                    "스코피는 명령의 잔향에서 완전히 벗어나지 못했다.",
                    new[] { "leo", "sagittarius" }, new[] { "myth_nemean_lion", "myth_chiron" }),

                new ChapterProfile(2, "pisces", "물고기자리", "피시즈 & 에스", "티폰을 피한 두 물고기", "myth_typhon_escape",
                    "분리불안, 공포 속에서 이어진 유대",
                    "부모를 잃은 뒤 스텔라가 돌보고 있으며 주점 수족관 옆에서 함께 지낸다.",
                    "둘 사이의 푸른 리본이 풀리거나 파도·천둥 소리를 들으면 패닉에 빠진다.",
                    "서로의 손목을 잇는 푸른 리본과 물·감정을 공유하는 힘",
                    "티폰을 피해 물고기로 변한 아프로디테와 에로스가 서로를 잃지 않으려 끈으로 연결한 이야기.",
                    "스타루트 에이드와 딸기 타르트", "딸기", "스타루트", "은하수 정제 시럽", "쌍둥이 별사탕",
                    "달콤한 맛과 잔잔한 물빛에 안도하며 끈 없이도 서로를 믿는 연습을 한다.",
                    "연결된 좌석과 잔잔한 물에는 안심하고 고립된 좌석에는 불안해한다.",
                    "별물결 연결 의자", FurnitureTrait.Connection, FurnitureTrait.Mystery,
                    CookingMethod.Chill, CookingMethod.Slice, new Color(0.18f, 0.62f, 0.72f),
                    "도망친 기억과 서로를 구한 기억을 구분하기",
                    "거센 물살 속 손을 놓지 않은 핵심 기억을 보호하기",
                    "유대는 붙잡는 구속이 아니라 다시 만날 수 있다는 믿음임을 받아들이기",
                    "rising_water",
                    "우리 떨어지면… / 다시는 못 만날 것 같아.",
                    "손을 잠시 놓아도 마음까지 사라지는 건 아니에요.",
                    "우린 놓아도… / 다시 서로를 찾을 수 있어!",
                    "피시즈와 에스는 두려움이 아닌 신뢰로 서로를 잇는다.",
                    "둘은 짧은 거리에서 손을 놓을 수 있게 되었다.",
                    "둘은 기억을 되찾았지만 리본에 대한 의존을 놓지 못했다.",
                    new[] { "cancer", "gemini" }, new[] { "myth_hydra_crab", "myth_castor_pollux" }),

                new ChapterProfile(3, "aries", "양자리", "아리", "황금양과 프릭소스", "myth_golden_ram",
                    "구조자의 희생과 살아남은 죄책감",
                    "야간 오토바이 구조 봉사를 하며 남을 먼저 구하려는 습관 때문에 늘 다쳐 있다.",
                    "다른 사람을 두고 먼저 안전해지는 선택을 하지 못한다.",
                    "빛나는 양털 섬유와 높은 곳을 향해 달리는 반사 행동",
                    "황금양이 프릭소스와 헬레를 구해 날아가지만 헬레를 잃고, 끝내 자신의 황금양털을 남긴 이야기.",
                    "황금 로즈메리 양갈비", "양고기", "로즈메리", "별빛 숙성주", "황금 설탕실",
                    "누군가를 살리기 위해 자신까지 사라질 필요는 없다는 사실을 받아들인다.",
                    "활력 있는 출입 동선은 좋아하지만 탈출구를 막는 무거운 장식은 거부한다.",
                    "새벽 항로 표지판", FurnitureTrait.Vitality, FurnitureTrait.Stability,
                    CookingMethod.Grill, CookingMethod.Grind, new Color(0.78f, 0.25f, 0.16f),
                    "구조 실패를 자신의 잘못으로 바꾼 오염 기억 판별하기",
                    "프릭소스를 끝까지 운반한 핵심 기억 보호하기",
                    "살아남아 다음 사람을 돕는 것도 용기임을 받아들이기",
                    "wind_route",
                    "내가 더 빨랐다면 아무도 떨어지지 않았을 거야.",
                    "한 사람을 잃은 슬픔이, 구해 낸 생명까지 지우게 두지 말아요.",
                    "계속 달리려면 나도 살아 있어야 하는군.",
                    "아리는 희생이 아닌 지속 가능한 용기를 선택했다.",
                    "아리는 잠시 멈추는 법을 배웠지만 죄책감은 남아 있다.",
                    "아리는 모든 책임을 자신에게 돌리는 습관을 놓지 못했다.",
                    new[] { "capricorn", "leo" }, new[] { "myth_pan_typhon", "myth_nemean_lion" }),

                new ChapterProfile(4, "taurus", "황소자리", "타우", "황소의 모습과 에우로페", "myth_europa_bull",
                    "타인의 기대에 소비된 몸과 자기 결정권",
                    "도예 공방의 모델 겸 운반 일을 하며 부탁을 거절하지 못한다.",
                    "외모나 힘을 평가받으면 말이 없어지고 자신의 의사를 숨긴다.",
                    "황소뿔 형태의 균열과 바다 건너 섬을 그리는 반복 낙서",
                    "제우스가 흰 황소 모습으로 에우로페를 크레타로 데려간 이야기에서, 운반자로만 기억된 형상의 관점.",
                    "천천히 익힌 버섯 스튜", "버섯", "보리", "별자리 눈물 결정", "흰 꽃잎",
                    "느린 조리와 선택권을 존중하는 질문에 처음으로 싫다는 말을 한다.",
                    "무게감 있는 안정 가구는 좋아하지만 전시대처럼 시선을 모으는 장식은 싫어한다.",
                    "낮은 흙빛 소파", FurnitureTrait.Stability, FurnitureTrait.Vitality,
                    CookingMethod.Boil, CookingMethod.Bake, new Color(0.48f, 0.38f, 0.22f),
                    "아름다운 탈것으로만 남은 기록에서 자신의 목소리 찾기",
                    "바다를 건넌 뒤 스스로 땅을 고른 기억 보호하기",
                    "힘과 몸의 주인은 자신임을 받아들이기",
                    "shifting_shore",
                    "다들 내가 괜찮다고 생각하니까… 그냥 맞춰 주는 편이 편해.",
                    "괜찮지 않다고 말해도 관계가 끝나는 건 아니에요.",
                    "이번엔 내가 머물 곳을 내가 고를래.",
                    "타우는 자신의 몸과 선택을 온전히 되찾았다.",
                    "타우는 작은 거절부터 연습하기 시작했다.",
                    "타우는 기억을 되찾았지만 여전히 타인의 평가에 자신을 맡긴다.",
                    new[] { "libra", "capricorn" }, new[] { "myth_themis_scales", "myth_pan_typhon" }),

                new ChapterProfile(5, "gemini", "쌍둥이자리", "카스토르 & 폴", "카스토르와 폴리데우케스", "myth_castor_pollux",
                    "상실과 애도, 함께이면서도 독립된 삶",
                    "대학로의 2인극 배우로 활동하지만 한 사람이 무대에 서지 못하면 공연 전체를 취소한다.",
                    "혼자 결정을 내리거나 서로 다른 의견을 말하면 배신처럼 느낀다.",
                    "한쪽은 닳고 한쪽은 빛나는 쌍둥이 가면",
                    "필멸자인 카스토르를 잃은 불멸의 폴리데우케스가 영생을 나누어 함께 하늘에 오른 이야기.",
                    "두 가지 소스의 반달 만두", "만두", "허브 소스", "기억의 별가루", "반쪽 달 장식",
                    "서로 다른 맛이 한 접시 안에서 사라지지 않고 조화를 이룬다.",
                    "마주 보는 두 의자는 좋아하지만 완전히 같은 장식만 강요되면 불편해한다.",
                    "서로 다른 쌍의자", FurnitureTrait.Connection, FurnitureTrait.Stability,
                    CookingMethod.Steam, CookingMethod.Grind, new Color(0.45f, 0.58f, 0.78f),
                    "죽음과 버림받음을 뒤섞은 거짓 기억 분리하기",
                    "서로 시간을 나누기로 한 핵심 기억 보호하기",
                    "다른 길을 걸어도 관계가 사라지지 않음을 받아들이기",
                    "mirror_pair",
                    "같은 대사를 하지 않으면 우리가 아닌 것 같아.",
                    "같다는 것보다 서로의 다른 말을 들어 주는 게 함께라는 뜻일 거예요.",
                    "각자 한 문장씩 말해도 이야기는 이어지는군.",
                    "카스토르와 폴은 각자의 삶과 함께인 시간을 모두 선택했다.",
                    "둘은 독립된 선택을 시작했지만 상실의 공포가 남아 있다.",
                    "둘은 기억을 되찾고도 서로의 차이를 지우려 한다.",
                    new[] { "pisces", "libra" }, new[] { "myth_typhon_escape", "myth_themis_scales" }),

                new ChapterProfile(6, "cancer", "게자리", "카르키", "히드라 곁의 게", "myth_hydra_crab",
                    "인정받지 못한 충성, 작은 존재의 가치",
                    "혜화동 동물병원 야간 보조로 일하며 공을 드러내지 않고 뒷정리를 맡는다.",
                    "도움을 칭찬받으면 자신보다 더 중요한 사람이 있다며 흔적을 지운다.",
                    "부서진 집게 모양 펜던트와 옆걸음으로 위험을 막는 습관",
                    "히드라를 돕기 위해 헤라클레스의 발을 문 작은 게가 밟혀 죽었고 헤라가 그 충성을 기린 이야기.",
                    "바삭한 게살 크림 크로켓", "게살", "크림", "궤도 안정 향신료", "작은 방패 장식",
                    "작은 도움도 누군가에게는 결정적인 보호였음을 기억한다.",
                    "포근한 은신처는 좋아하지만 거대한 영웅상처럼 자신을 작게 만드는 장식은 피한다.",
                    "달껍질 은신 의자", FurnitureTrait.Calm, FurnitureTrait.Vitality,
                    CookingMethod.Bake, CookingMethod.Boil, new Color(0.46f, 0.58f, 0.68f),
                    "패배와 무가치를 동일시한 오염 기억 판별하기",
                    "위험 앞에서 도망치지 않은 작은 용기 보호하기",
                    "크기가 아니라 선택이 존재의 가치를 만든다는 사실 받아들이기",
                    "giant_footsteps",
                    "내가 한 건 별것 아니야. 누구든 했을 거야.",
                    "당신이 아니었다면 비어 있었을 자리가 분명히 있어요.",
                    "작은 자리도 내가 지킨 자리였구나.",
                    "카르키는 보이지 않던 자신의 공헌을 인정했다.",
                    "카르키는 칭찬을 받아들이기 시작했지만 여전히 뒤로 물러선다.",
                    "카르키는 자신의 기억마저 타인의 이야기 뒤에 숨긴다.",
                    new[] { "aries", "virgo" }, new[] { "myth_golden_ram", "myth_astraea" }),

                new ChapterProfile(7, "virgo", "처녀자리", "아스트라", "지상을 떠난 아스트라이아", "myth_astraea",
                    "완벽한 책임감과 세상을 떠나야 했던 죄책감",
                    "공익 법률센터 기록 담당자로 모든 억울한 사연을 혼자 해결하려 한다.",
                    "불완전한 결과를 내면 자신이 사람을 버렸다고 여기며 일을 멈추지 않는다.",
                    "밀 이삭 책갈피와 마지막까지 지상을 바라본 별빛",
                    "인간의 타락이 이어진 시대에도 마지막까지 남았던 정의의 여신 아스트라이아가 결국 하늘로 떠난 이야기.",
                    "밀 이삭 허브 리조토", "쌀", "밀 이삭", "은하수 정제 시럽", "정돈된 잎 장식",
                    "모든 사람을 혼자 구할 수 없다는 한계를 실패가 아닌 경계로 받아들인다.",
                    "정돈된 기록장은 좋아하지만 끝없는 미완료 목록을 연상시키는 가구는 싫어한다.",
                    "여백이 있는 기록장", FurnitureTrait.Memory, FurnitureTrait.Connection,
                    CookingMethod.StirFry, CookingMethod.Slice, new Color(0.72f, 0.66f, 0.42f),
                    "떠남을 배신으로 바꾼 거짓 판결문 판별하기",
                    "끝까지 곁에 머문 시간의 핵심 기억 보호하기",
                    "한계를 인정하고 도움을 나누는 것도 정의임을 받아들이기",
                    "endless_verdict",
                    "내가 멈추면 누군가는 기록에서 사라져.",
                    "혼자 쓰러지면 기록을 이어 갈 사람도 사라져요.",
                    "남겨 둔 여백을 다른 사람이 이어 쓰게 해도 되는군요.",
                    "아스트라는 책임을 나누며 정의를 지속하기로 했다.",
                    "아스트라는 도움을 청하기 시작했지만 미완료를 견디기 어렵다.",
                    "아스트라는 다시 모든 책임을 홀로 짊어지려 한다.",
                    new[] { "libra", "cancer" }, new[] { "myth_themis_scales", "myth_hydra_crab" }),

                new ChapterProfile(8, "libra", "천칭자리", "리브라", "테미스의 저울", "myth_themis_scales",
                    "공정함에 대한 강박과 자신의 감정을 제외한 판단",
                    "대학로 분쟁조정 상담사로 모두를 만족시키려다 어떤 결정도 내리지 못한다.",
                    "누군가 실망하면 판단 전체가 틀렸다고 여기고 자신의 욕구를 지운다.",
                    "한쪽 접시만 비어 있는 은빛 저울",
                    "질서와 정의의 여신 테미스가 든 저울이 천칭자리의 상징이 된 이야기.",
                    "균형 향신료 플래터", "채소", "치즈", "궤도 안정 향신료", "대칭 별 장식",
                    "완벽히 같은 몫보다 각자에게 필요한 몫이 다를 수 있음을 느낀다.",
                    "균형 잡힌 배치는 좋아하지만 지나친 대칭 강박을 유도하는 장식에는 피로해한다.",
                    "기울어도 서는 저울", FurnitureTrait.Connection, FurnitureTrait.Mystery,
                    CookingMethod.Bake, CookingMethod.Slice, new Color(0.62f, 0.54f, 0.72f),
                    "공정과 모두의 만족을 동일시한 거짓 명제 판별하기",
                    "자신의 감정도 저울에 올렸던 핵심 기억 보호하기",
                    "결정에는 책임과 불완전함이 함께함을 받아들이기",
                    "tilting_scales",
                    "누군가 손해를 보면 내 판단은 틀린 거야.",
                    "당신의 마음도 저울 한쪽에 올라갈 자격이 있어요.",
                    "균형은 멈춘 상태가 아니라 계속 조정하는 일이었군.",
                    "리브라는 자신의 감정까지 포함해 책임 있게 판단했다.",
                    "리브라는 불완전한 결정을 감당하는 연습을 시작했다.",
                    "리브라는 여전히 모두의 만족을 위해 자신을 지운다.",
                    new[] { "virgo", "gemini" }, new[] { "myth_astraea", "myth_castor_pollux" }),

                new ChapterProfile(9, "sagittarius", "사수자리", "키론", "현자 케이론의 화살", "myth_chiron",
                    "치유자의 상처와 도움을 받지 못하는 고립",
                    "청소년 양궁 강사이자 무료 상담 봉사자로 타인의 상처를 잘 보지만 자신의 통증은 숨긴다.",
                    "누군가 자신을 돌보려 하면 역할이 뒤바뀐다며 농담으로 피한다.",
                    "독이 밴 화살촉과 별을 향한 반인반마의 실루엣",
                    "불사의 현자 케이론이 히드라의 독화살에 입은 치유 불가능한 고통 끝에 불멸을 내려놓은 이야기.",
                    "세이지 훈제 꼬치", "닭고기", "세이지", "별자리 눈물 결정", "작은 화살 장식",
                    "치유자도 아프다고 말할 수 있으며 도움을 받는 것이 역할을 잃는 일은 아님을 느낀다.",
                    "배움과 기억을 상징하는 가구는 좋아하지만 날카로운 장식은 피한다.",
                    "별길 지도 탁자", FurnitureTrait.Memory, FurnitureTrait.Vitality,
                    CookingMethod.Grill, CookingMethod.Infuse, new Color(0.38f, 0.28f, 0.55f),
                    "스승의 역할과 고통받는 자신을 분리하기",
                    "제자를 길러 낸 따뜻한 핵심 기억 보호하기",
                    "도움을 받는 순간에도 자신의 지혜는 사라지지 않음을 받아들이기",
                    "poisoned_arrows",
                    "나는 방법을 아니까 괜찮아. 다른 사람부터 보자.",
                    "방법을 아는 사람도 손을 빌릴 수 있어요.",
                    "내 상처를 맡긴다고 길잡이의 자리를 잃는 건 아니군.",
                    "키론은 치유자이면서 돌봄받는 존재가 되었다.",
                    "키론은 통증을 말하기 시작했지만 여전히 혼자 견디려 한다.",
                    "키론은 기억을 되찾고도 자신의 고통을 역할 뒤에 숨긴다.",
                    new[] { "scorpio", "aries" }, new[] { "myth_orion_scorpion", "myth_golden_ram" }),

                new ChapterProfile(10, "capricorn", "염소자리", "판", "티폰을 피한 염소물고기", "myth_pan_typhon",
                    "생존을 위한 변화와 불완전한 모습에 대한 수치",
                    "공연장 음향 기사로 일하며 사고 후 남은 흉터와 떨림을 숨긴다.",
                    "예상하지 못한 변화가 생기면 자신의 몸과 판단을 혐오한다.",
                    "염소의 앞부분과 물고기 꼬리가 뒤섞인 그림자",
                    "티폰을 피해 물로 뛰어든 판이 급히 변신해 반은 염소, 반은 물고기인 모습으로 남은 이야기.",
                    "산허브 해산물 수프", "해산물", "산허브", "은하수 정제 시럽", "물결 뿔 장식",
                    "완벽하지 않은 변신이 자신을 살렸으며 흉터가 실패의 증거가 아님을 깨닫는다.",
                    "자연스러운 비대칭 가구는 좋아하지만 완벽한 신체상을 강조하는 장식은 거부한다.",
                    "산과 물의 파티션", FurnitureTrait.Stability, FurnitureTrait.Mystery,
                    CookingMethod.Boil, CookingMethod.Grind, new Color(0.24f, 0.48f, 0.48f),
                    "불완전한 변신을 조롱하는 오염된 거울 판별하기",
                    "살아남기 위해 물로 뛰어든 결단의 기억 보호하기",
                    "변화한 모습도 살아온 자신의 일부임을 받아들이기",
                    "split_form",
                    "그때 제대로 움직였다면 이런 모습은 남지 않았을 거야.",
                    "살아남은 몸은 틀린 답이 아니라 그 순간 가능한 답이었어요.",
                    "완성되지 않았어도 나를 살린 형태였군.",
                    "판은 변화한 자신의 몸과 생존을 긍정했다.",
                    "판은 흉터를 숨기지 않기 시작했지만 시선이 두렵다.",
                    "판은 살아남은 기억을 실패로 규정한 채 머문다.",
                    new[] { "taurus", "aries" }, new[] { "myth_europa_bull", "myth_golden_ram" }),

                new ChapterProfile(11, "aquarius", "물병자리", "가니", "신들의 물병지기 가니메데스", "myth_ganymede",
                    "돌봄 노동과 선택권, 맡겨진 역할에서 벗어나기",
                    "24시간 카페의 야간 매니저로 모두의 필요를 챙기지만 자신의 퇴근을 미룬다.",
                    "잔이 비거나 누군가 불편해하면 즉시 자신이 채워야 한다고 느낀다.",
                    "끝없이 물이 차오르는 물병과 독수리 깃털",
                    "제우스에게 올림포스로 데려가져 신들의 술을 따르는 역할을 맡은 가니메데스의 이야기.",
                    "구름 허브 소다", "탄산수", "구름 허브", "기억의 별가루", "넘치지 않는 잔",
                    "채워 주지 않는 시간에도 관계가 유지되며 자신의 잔을 먼저 채울 수 있음을 배운다.",
                    "쉼과 연결 가구는 좋아하지만 끝없는 접객을 연상시키는 종과 카운터는 거부한다.",
                    "자신을 위한 빈 잔", FurnitureTrait.Calm, FurnitureTrait.Connection,
                    CookingMethod.Infuse, CookingMethod.Chill, new Color(0.32f, 0.58f, 0.72f),
                    "봉사와 강요를 같은 것으로 만든 천상의 기록 판별하기",
                    "처음으로 자신의 잔을 채운 핵심 기억 보호하기",
                    "돌봄은 선택일 때만 사랑이 될 수 있음을 받아들이기",
                    "overflowing_vessels",
                    "누군가 목마르면 내가 채워야 해. 그게 내 자리니까.",
                    "당신의 잔이 빈 채로는 누구에게도 오래 물을 건넬 수 없어요.",
                    "내가 쉬어도 물길은 다른 이들과 이어질 수 있군.",
                    "가니는 맡겨진 역할을 내려놓고 스스로의 돌봄을 선택했다.",
                    "가니는 자신의 필요를 말하기 시작했지만 죄책감이 남아 있다.",
                    "가니는 기억을 되찾고도 끝없는 접객에서 벗어나지 못했다.",
                    new[] { "pisces", "libra" }, new[] { "myth_typhon_escape", "myth_themis_scales" })
            };
        }

        private sealed class ChapterProfile
        {
            public readonly int Index;
            public readonly string ZodiacId;
            public readonly string ZodiacName;
            public readonly string GuestName;
            public readonly string MythTitle;
            public readonly string MythId;
            public readonly string EmotionalTheme;
            public readonly string CurrentLife;
            public readonly string TraumaReaction;
            public readonly string IdentityClue;
            public readonly string MythSummary;
            public readonly string DishName;
            public readonly string MainIngredient;
            public readonly string GarnishIngredient;
            public readonly string MagicalIngredient;
            public readonly string Decoration;
            public readonly string FoodReaction;
            public readonly string InteriorReaction;
            public readonly string FurnitureName;
            public readonly FurnitureTrait PreferredTrait;
            public readonly FurnitureTrait RejectedTrait;
            public readonly CookingMethod MainMethod;
            public readonly CookingMethod GarnishMethod;
            public readonly Color Palette;
            public readonly string MemoryTruthObjective;
            public readonly string MemoryProtectObjective;
            public readonly string MemoryAcceptObjective;
            public readonly string MemoryModule;
            public readonly string NightOpening;
            public readonly string PlayerResponse;
            public readonly string NightResolution;
            public readonly string CompleteText;
            public readonly string PartialText;
            public readonly string UnstableText;
            public readonly IReadOnlyList<string> DistractorZodiacs;
            public readonly IReadOnlyList<string> DistractorMyths;

            public ChapterProfile(
                int index, string zodiacId, string zodiacName, string guestName, string mythTitle, string mythId,
                string emotionalTheme, string currentLife, string traumaReaction, string identityClue, string mythSummary,
                string dishName, string mainIngredient, string garnishIngredient, string magicalIngredient, string decoration,
                string foodReaction, string interiorReaction, string furnitureName,
                FurnitureTrait preferredTrait, FurnitureTrait rejectedTrait,
                CookingMethod mainMethod, CookingMethod garnishMethod, Color palette,
                string memoryTruthObjective, string memoryProtectObjective, string memoryAcceptObjective, string memoryModule,
                string nightOpening, string playerResponse, string nightResolution,
                string completeText, string partialText, string unstableText,
                IReadOnlyList<string> distractorZodiacs, IReadOnlyList<string> distractorMyths)
            {
                Index = index;
                ZodiacId = zodiacId;
                ZodiacName = zodiacName;
                GuestName = guestName;
                MythTitle = mythTitle;
                MythId = mythId;
                EmotionalTheme = emotionalTheme;
                CurrentLife = currentLife;
                TraumaReaction = traumaReaction;
                IdentityClue = identityClue;
                MythSummary = mythSummary;
                DishName = dishName;
                MainIngredient = mainIngredient;
                GarnishIngredient = garnishIngredient;
                MagicalIngredient = magicalIngredient;
                Decoration = decoration;
                FoodReaction = foodReaction;
                InteriorReaction = interiorReaction;
                FurnitureName = furnitureName;
                PreferredTrait = preferredTrait;
                RejectedTrait = rejectedTrait;
                MainMethod = mainMethod;
                GarnishMethod = garnishMethod;
                Palette = palette;
                MemoryTruthObjective = memoryTruthObjective;
                MemoryProtectObjective = memoryProtectObjective;
                MemoryAcceptObjective = memoryAcceptObjective;
                MemoryModule = memoryModule;
                NightOpening = nightOpening;
                PlayerResponse = playerResponse;
                NightResolution = nightResolution;
                CompleteText = completeText;
                PartialText = partialText;
                UnstableText = unstableText;
                DistractorZodiacs = distractorZodiacs;
                DistractorMyths = distractorMyths;
            }
        }
    }
}
