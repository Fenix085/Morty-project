using System.Collections.Generic;
using UnityEngine;

public static class SceneSessionState
{
    private struct PlayerState
    {
        public Vector3 Position;
        public Quaternion Rotation;
    }

    private static readonly Dictionary<string, PlayerState> PlayerStatesByScene = new Dictionary<string, PlayerState>();
    private static readonly HashSet<string> IntroPlayedScenes = new HashSet<string>();

    public static string JustCompletedFacilityId = "";
    public static string CurrentFacilityId = "";

    public static void MarkCurrentFacilityCompleted()
    {
        JustCompletedFacilityId = CurrentFacilityId;
    }

    public static bool HasPlayedIntro(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return false;
        }

        return IntroPlayedScenes.Contains(sceneName);
    }

    public static void MarkIntroPlayed(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return;
        }

        IntroPlayedScenes.Add(sceneName);
    }

    public static void ClearIntroPlayed(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return;
        }

        IntroPlayedScenes.Remove(sceneName);
    }

    public static void SavePlayerState(string sceneName, Vector3 position, Quaternion rotation)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return;
        }

        PlayerStatesByScene[sceneName] = new PlayerState
        {
            Position = position,
            Rotation = rotation
        };
    }

    public static bool TryGetPlayerState(string sceneName, out Vector3 position, out Quaternion rotation)
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;

        if (string.IsNullOrEmpty(sceneName))
        {
            return false;
        }

        if (PlayerStatesByScene.TryGetValue(sceneName, out var state))
        {
            position = state.Position;
            rotation = state.Rotation;
            return true;
        }

        return false;
    }
}
