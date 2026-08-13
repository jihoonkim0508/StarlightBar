using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.Core
{
    [RequireComponent(typeof(Button))]
    public sealed class SceneNavigator : MonoBehaviour
    {
        [SerializeField] private string targetScene;

        private void Awake()
        {
            if (GameManager.Instance != null)
                return;

            GetComponent<Button>().interactable = false;
            Debug.LogError("GameManager가 없습니다. Bootstrap 씬부터 실행하세요.", this);
        }

        public void LoadTarget()
        {
            GameManager.Instance?.LoadScene(targetScene);
        }
    }
}
