﻿using System.Collections.Generic;
using System.Linq;

public class CombatRoomSaveData
{
    public bool EnemiesCleared { get; set; } = false;
    public Dictionary<int, ActorType> SpawnerIdToSpawnedActor { get; set; } = new Dictionary<int, ActorType>();

    public SerializableCombatRoomSaveData Serialize()
    {
        return new SerializableCombatRoomSaveData
        {
            EnemiesCleared = EnemiesCleared,
            SerializableSpawnerSaveDataList = SpawnerIdToSpawnedActor.Keys
                .Select(spawnerId => new SerializableSpawnerSaveData
                {
                    Id = spawnerId,
                    ActorType = SpawnerIdToSpawnedActor[spawnerId]
                })
                .ToList()
        };
    }
}
