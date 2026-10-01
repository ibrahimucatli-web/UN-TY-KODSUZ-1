using UnityEngine;
using TacticalFPS.Core;

namespace TacticalFPS.Characters
{
    [CreateAssetMenu(menuName = "Tactical FPS/Character Definition")]
    public sealed class CharacterDefinition : ScriptableObject
    {
        public string displayName;
        public Team team;
        [Tooltip("Addressable prefab key; use different server/client prefabs if needed.")]
        public string prefabAddress;
        public Material[] allowedMaterials;
    }

    public interface ICharacterSpawnService
    {
        void Spawn(ulong playerId, CharacterDefinition definition, Transform spawnPoint);
    }

    /// <summary>Match roster validation prevents enemy-only cosmetics being selected for a team.</summary>
    public static class CharacterRoster
    {
        public static bool CanUse(CharacterDefinition definition, Team assignedTeam) =>
            definition != null && definition.team == assignedTeam && assignedTeam != Team.Spectator;
    }
}
