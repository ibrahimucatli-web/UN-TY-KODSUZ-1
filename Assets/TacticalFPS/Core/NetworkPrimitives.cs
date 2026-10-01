using System;
using UnityEngine;

namespace TacticalFPS.Core
{
    [Flags]
    public enum InputButtons : ushort { None = 0, Jump = 1, Crouch = 2, Fire = 4, Reload = 8, Use = 16 }

    [Serializable]
    public struct InputCommand
    {
        public uint Sequence;
        public double Timestamp;
        public Vector2 Move;
        public Vector2 LookDelta;
        public InputButtons Buttons;
        public bool IsPressed(InputButtons button) => (Buttons & button) != 0;
    }

    /// <summary>Server-side anti-replay input queue. Insert commands from one owning connection only.</summary>
    public sealed class ServerInputBuffer
    {
        private readonly System.Collections.Generic.SortedDictionary<uint, InputCommand> commands = new();
        private uint lastConsumed;
        public void Enqueue(InputCommand command)
        {
            if (command.Sequence <= lastConsumed || commands.ContainsKey(command.Sequence)) return;
            commands.Add(command.Sequence, command);
        }
        public bool TryConsume(double simulationTime, out InputCommand command)
        {
            foreach (var pair in commands)
            {
                if (pair.Value.Timestamp > simulationTime) break;
                command = pair.Value;
                commands.Remove(pair.Key);
                lastConsumed = pair.Key;
                return true;
            }
            command = default;
            return false;
        }
    }

    public enum Team : byte { Terrorists, CounterTerrorists, Spectator }
    public enum SurfaceMaterial : byte { Concrete, Metal, Wood, Glass, Flesh }
}
