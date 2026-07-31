using System.Collections.Generic;
using System.Linq;
using StarlightBar.Content;
using StarlightBar.Core;
using StarlightBar.Systems;
using StarlightBar.UI;
using UnityEngine;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 에디터 배치 지점에 손님 프리팹을 표시하고 감정 상태에 따른 움직임만 계산합니다.
    /// </summary>
    public sealed class GuestVisualPresenter : MonoBehaviour
    {
        [SerializeField, Tooltip("손님 배치 지점과 손님 프리팹을 가진 View입니다.")]
        private GuestVisualView view;

        private readonly List<GuestFigureView> figures = new();
        private GameRuntimeCoordinator runtime;
        private bool traumaActive;
        private string currentGuestId;
        public bool TraumaActive => traumaActive;

        private void Start()
        {
            if (view == null || view.FigurePrefab == null || view.SpawnPoints.Length == 0)
            {
                Debug.LogError("GuestVisualView 참조가 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }
            RefreshRuntimeAndGuest();
        }

        private void Update()
        {
            if (!RefreshRuntimeAndGuest())
                return;
            traumaActive = HasRejectedFurniture();
            AnimateFigures();
            UpdateLabel();
        }

        private bool RefreshRuntimeAndGuest()
        {
            var bootstrap = GameBootstrapper.Instance;
            if (bootstrap?.Runtime?.CurrentChapter?.guest == null)
                return false;
            runtime = bootstrap.Runtime;
            var guestId = runtime.CurrentChapter.guest.id;
            if (guestId == currentGuestId)
                return true;
            foreach (var figure in figures)
                if (figure != null)
                    Destroy(figure.gameObject);
            figures.Clear();
            currentGuestId = guestId;
            BuildGuest();
            return true;
        }

        private void BuildGuest()
        {
            var guest = runtime.CurrentChapter.guest;
            var twoPerson = guest.displayName.Contains("&") || guest.displayName.Contains("＆");
            var count = Mathf.Min(twoPerson ? 2 : 1, view.SpawnPoints.Length);
            for (var index = 0; index < count; index++)
            {
                var figure = Instantiate(view.FigurePrefab, view.SpawnPoints[index]);
                figure.name = $"GuestFigure_{index + 1}";
                figure.Bind(index == 0
                    ? guest.themeColor
                    : Color.Lerp(guest.themeColor, Color.white, 0.25f));
                figures.Add(figure);
            }
        }

        private void AnimateFigures()
        {
            var time = Time.time;
            for (var index = 0; index < figures.Count; index++)
            {
                var figure = figures[index];
                var trustScale = 1f + (int)runtime.GuestState.TrustStage * 0.012f;
                var breath = Mathf.Sin(time * 1.8f + index) * 0.018f;
                var offset = Vector3.up * breath;
                if (traumaActive || runtime.GuestState.StabilityStage == GuestStabilityStage.Distressed)
                {
                    offset += new Vector3(Mathf.Sin(time * 25f + index) * 0.035f, 0f);
                    figure.transform.localRotation =
                        Quaternion.Euler(0, 0, Mathf.Sin(time * 17f) * 2.2f);
                    figure.transform.localScale = Vector3.Scale(
                        figure.DistressedScale, new Vector3(trustScale, 1f, 1f));
                }
                else if (runtime.GuestState.StabilityStage == GuestStabilityStage.Tense)
                {
                    figure.transform.localRotation =
                        Quaternion.Euler(0, 0, Mathf.Sin(time * 5f + index) * 0.8f);
                    figure.transform.localScale = Vector3.Scale(
                        figure.TenseScale, new Vector3(trustScale, 1f, 1f));
                }
                else
                {
                    figure.transform.localRotation = Quaternion.identity;
                    figure.transform.localScale = Vector3.Scale(
                        figure.RelaxedScale + Vector3.up * breath,
                        new Vector3(trustScale, 1f, 1f));
                }
                figure.transform.localPosition = offset;
            }
        }

        private void UpdateLabel()
        {
            if (view.StateLabel == null)
                return;
            view.StateLabel.text = traumaActive
                ? $"{runtime.CurrentChapter.guest.displayName}\n[트라우마 반응 · 거리 두기]"
                : $"{runtime.CurrentChapter.guest.displayName}\n" +
                  $"신뢰 {GuestStateModel.ToKorean(runtime.GuestState.TrustStage)} · " +
                  $"안정 {GuestStateModel.ToKorean(runtime.GuestState.StabilityStage)} · " +
                  $"기억 {GuestStateModel.ToKorean(runtime.GuestState.MemoryStage)}";
            view.StateLabel.color = traumaActive
                ? view.TraumaTextColor
                : runtime.GuestState.TrustStage >= GuestTrustStage.High
                    ? view.HighTrustTextColor
                    : view.NormalTextColor;
        }

        private bool HasRejectedFurniture()
        {
            var rejected = runtime?.CurrentChapter?.guest?.rejectedFurnitureTraits;
            var placements = GameBootstrapper.Instance?.Session?.Data?.furniturePlacements;
            if (rejected == null || rejected.Count == 0 || placements == null)
                return false;
            return placements.Any(placement =>
            {
                if (placement == null || placement.stored)
                    return false;
                var furniture = BuiltInChapterCatalog.FindFurniture(placement.furnitureId);
                return furniture != null && furniture.traits.Any(rejected.Contains);
            });
        }
    }
}
