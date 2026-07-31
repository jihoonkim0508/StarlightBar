using TMPro;
using UnityEngine;

namespace StarlightBar.UI
{
    /// <summary>
    /// 주점 손님의 배치 지점, 상태 문구와 반복 손님 프리팹을 보관합니다.
    /// </summary>
    public sealed class GuestVisualView : MonoBehaviour
    {
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private TextMeshPro stateLabel;
        [SerializeField] private GuestFigureView figurePrefab;
        [SerializeField] private Color traumaTextColor = new(1f, 0.56f, 0.58f);
        [SerializeField] private Color highTrustTextColor = new(0.74f, 0.94f, 0.92f);
        [SerializeField] private Color normalTextColor = new(0.92f, 0.88f, 0.72f);

        public Transform[] SpawnPoints => spawnPoints;
        public TextMeshPro StateLabel => stateLabel;
        public GuestFigureView FigurePrefab => figurePrefab;
        public Color TraumaTextColor => traumaTextColor;
        public Color HighTrustTextColor => highTrustTextColor;
        public Color NormalTextColor => normalTextColor;
    }
}
