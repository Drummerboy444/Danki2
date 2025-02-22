﻿using System.Collections;
using UnityEngine.TestTools;

public class NewSaveGeneratorTest : PlayModeTestBase
{
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        TestUtils.InstantiatePrefab<CustomAbilityLookup>();
        TestUtils.InstantiatePrefab<RarityLookup>();
        TestUtils.InstantiatePrefab<MapGenerationLookup>();
        TestUtils.InstantiatePrefab<SceneLookup>();
        TestUtils.InstantiatePrefab<MapGenerator>();
        TestUtils.InstantiatePrefab<NewSaveGenerator>();
        yield return null;
    }

    protected override IEnumerator TearDown()
    {
        CustomAbilityLookup.Instance.Destroy();
        RarityLookup.Instance.Destroy();
        MapGenerationLookup.Instance.Destroy();
        SceneLookup.Instance.Destroy();
        MapGenerator.Instance.Destroy();
        NewSaveGenerator.Instance.Destroy();
        yield return null;
    }
    
    [UnityTest]
    public IEnumerator TestGenerateProducesTheSameSaveDataWithTheSameSeed()
    {
        int seed = RandomUtils.Seed();
        SaveData saveData1 = NewSaveGenerator.Instance.Generate(seed);
        SaveData saveData2 = NewSaveGenerator.Instance.Generate(seed);
        
        TestUtils.AssertEqualByJson<SaveData, SerializableSaveData>(saveData1, saveData2);
        
        yield return null;
    }
    
    [UnityTest]
    public IEnumerator TestGenerateWithMultipleLayersProducesTheSameSaveDataWithTheSameSeed()
    {
        int seed = RandomUtils.Seed();
        SaveData saveData1 = NewSaveGenerator.Instance.Generate(seed);
        SaveData saveData2 = NewSaveGenerator.Instance.Generate(seed);

        saveData1.CurrentRoomNode = saveData1.CurrentRoomNode.Children[0];
        NewSaveGenerator.Instance.GenerateNextLayer(saveData1);
        saveData2.CurrentRoomNode = saveData2.CurrentRoomNode.Children[0];
        NewSaveGenerator.Instance.GenerateNextLayer(saveData2);

        saveData1.CurrentRoomNode = saveData1.CurrentRoomNode.Children[0];
        NewSaveGenerator.Instance.GenerateNextLayer(saveData1);
        saveData2.CurrentRoomNode = saveData2.CurrentRoomNode.Children[0];
        NewSaveGenerator.Instance.GenerateNextLayer(saveData2);
        
        TestUtils.AssertEqualByJson<SaveData, SerializableSaveData>(saveData1, saveData2);
        
        yield return null;
    }

    [UnityTest]
    public IEnumerator TestGenerateProducesDifferentSaveDataWithDifferentSeeds()
    {
        int seed = RandomUtils.Seed();
        SaveData saveData1 = NewSaveGenerator.Instance.Generate(seed);
        SaveData saveData2 = NewSaveGenerator.Instance.Generate(seed + 1);
        
        TestUtils.AssertNotEqualByJson<SaveData, SerializableSaveData>(saveData1, saveData2);
        
        yield return null;
    }

    [UnityTest]
    public IEnumerator TestGenerateWithMultipleLayersProducesDifferentSaveDataWithDifferentSeeds()
    {
        int seed = RandomUtils.Seed();
        SaveData saveData1 = NewSaveGenerator.Instance.Generate(seed);
        SaveData saveData2 = NewSaveGenerator.Instance.Generate(seed + 1);

        saveData1.CurrentRoomNode = saveData1.CurrentRoomNode.Children[0];
        NewSaveGenerator.Instance.GenerateNextLayer(saveData1);
        saveData2.CurrentRoomNode = saveData2.CurrentRoomNode.Children[0];
        NewSaveGenerator.Instance.GenerateNextLayer(saveData2);

        saveData1.CurrentRoomNode = saveData1.CurrentRoomNode.Children[0];
        NewSaveGenerator.Instance.GenerateNextLayer(saveData1);
        saveData2.CurrentRoomNode = saveData2.CurrentRoomNode.Children[0];
        NewSaveGenerator.Instance.GenerateNextLayer(saveData2);
        
        TestUtils.AssertNotEqualByJson<SaveData, SerializableSaveData>(saveData1, saveData2);
        
        yield return null;
    }
}
