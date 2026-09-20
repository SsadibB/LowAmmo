using UnityEngine;

namespace LowAmmo.Puzzle
{
    /// <summary>
    /// Core interface for any object that reacts when hit by a player bullet.
    /// The Gun relies strictly on this interface, preventing hardcoded object-specific logic.
    /// </summary>
    public interface IShootable
    {
        void OnHit(RaycastHit2D hit);
    }
}
