using UnityEngine;

namespace StarlightBar.Gameplay.Items
{
    public interface IItem
    {
        string Id { get; }
        string DisplayName { get; }
        Sprite Icon { get; }
    }

    public enum ItemProcess
    {
        Wash,
        Cut,
        Cook
    }
}
