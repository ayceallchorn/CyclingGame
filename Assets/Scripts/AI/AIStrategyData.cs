using UnityEngine;

namespace Cycling.AI
{
    [CreateAssetMenu(fileName = "AIStrategy", menuName = "Cycling/AI Strategy")]
    public class AIStrategyData : ScriptableObject
    {
        [Range(0f, 1f)] public float aggressiveness = 0.5f;
        [Range(0f, 1f)] public float sprintAbility = 0.5f;
        [Range(0f, 1f)] public float climbingAbility = 0.5f;
        [Tooltip("How much random power variation (fraction of FTP)")]
        [Range(0f, 0.15f)] public float powerVariation = 0.05f;
    }
}
