﻿using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Handles spawning the player in the rooms that require it. If the room does not require the player then this class
/// does nothing.
/// </summary>
public class PlayerRoomManager : Singleton<PlayerRoomManager>
{
    private static readonly ISet<RoomType> ignoredRoomTypes = new HashSet<RoomType>
    {
        RoomType.Victory,
        RoomType.Defeat,
    };
    
    private void Start()
    {
        RoomNode roomNode = PersistenceManager.Instance.SaveData.CurrentRoomNode;

        if (ignoredRoomTypes.Contains(roomNode.RoomType)) return;

        Dictionary<int, PlayerSpawner> spawnerLookup = FindObjectsOfType<PlayerSpawner>().ToDictionary(s => s.Id);

        spawnerLookup[roomNode.PlayerSpawnerId].Spawn();
    }
}
