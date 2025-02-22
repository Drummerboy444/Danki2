﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveData : ISerializable<SerializableSaveData>
{
    public const int RootNodeId = 0;
    public const int RootNodeParentId = -1;
    
    public int Version { get; set; }
    public int Seed { get; set; }
    public Random.State RandomState { get; set; }

    public int PlayerHealth { get; set; }
    public SerializableAbilityTree SerializableAbilityTree { get; set; }
    public List<RuneSocket> RuneSockets { get; set; } = new List<RuneSocket>();
    public List<Rune> RuneOrder { get; set; } = new List<Rune>();
    public int CurrencyAmount { get; set; }

    public RoomNode CurrentRoomNode { get; set; }
    public RoomNode DefeatRoom { get; set; }

    public SerializableSaveData Serialize()
    {
        SerializableSaveData serializableSaveData = new SerializableSaveData
        {
            Version = Version,
            Seed = Seed,
            RandomState = RandomState,
            PlayerHealth = PlayerHealth,
            SerializableAbilityTree = SerializableAbilityTree,
            RuneSockets = RuneSockets,
            RuneOrder = RuneOrder,
            CurrencyAmount = CurrencyAmount,
        };
        
        AddGraphData(serializableSaveData);
        
        return serializableSaveData;
    }

    private void AddGraphData(SerializableSaveData serializableSaveData)
    {
        RoomNode rootNode = CurrentRoomNode.GetRootNode();

        int id = RootNodeId;
        Dictionary<RoomNode, int> roomNodeToId = new Dictionary<RoomNode, int>();
        rootNode.IterateDown(node =>
        {
            roomNodeToId[node] = id;
            id++;
        });

        List<SerializableRoomNode> serializableRoomNodes = new List<SerializableRoomNode>();

        rootNode.IterateDown(node =>
        {
            SerializableRoomNode serializableRoomNode = node.Serialize();
            serializableRoomNode.Id = roomNodeToId[node];
            serializableRoomNode.ParentId = node.IsRootNode ? RootNodeParentId : roomNodeToId[node.Parent];
            serializableRoomNode.ChildIds = node.Children
                .Select(c => roomNodeToId[c])
                .ToList();
            serializableRoomNode.SerializableTransitionData = node.ExitIdToChildLookup.Keys
                .Select(exitId => new SerializableTransitionData
                {
                    RoomTransitionerId = exitId,
                    NextRoomId = roomNodeToId[node.ExitIdToChildLookup[exitId]],
                    IndicatesNextRoomType = node.ExitIdToIndicatesNextRoomType[exitId],
                    FurtherIndicatedRoomTypes = node.ExitIdToFurtherIndicatedRoomTypes[exitId]
                })
                .ToList();
            serializableRoomNodes.Add(serializableRoomNode);
        });

        serializableSaveData.CurrentRoomId = roomNodeToId[CurrentRoomNode];
        serializableSaveData.SerializableRoomNodes = serializableRoomNodes;
        serializableSaveData.SerializableDefeatRoom = DefeatRoom.Serialize();
    }
}
