﻿using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DevPersistenceManager))]
public class DevPersistenceManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DevPersistenceManager devPersistenceManager = (DevPersistenceManager) target;

        EditorUtils.ShowScriptLink(devPersistenceManager);

        devPersistenceManager.abilityData = (AbilityData) EditorGUILayout.ObjectField(
            "Ability Data",
            devPersistenceManager.abilityData,
            typeof(AbilityData),
            false
        );

        EditPlayer(devPersistenceManager);

        EditorUtils.VerticalSpace();
        EditAbilityTree(devPersistenceManager);

        EditorUtils.VerticalSpace();
        EditScene(devPersistenceManager);
        
        EditorUtils.VerticalSpace();
        EditRoom(devPersistenceManager);
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    private void EditPlayer(DevPersistenceManager devPersistenceManager)
    {
        EditorUtils.Header("Player");
        EditorGUI.indentLevel++;
        devPersistenceManager.playerSpawnerId = EditorGUILayout.IntField("Player Spawner ID", devPersistenceManager.playerSpawnerId);
        devPersistenceManager.playerHealth = EditorGUILayout.IntField("Player Health", devPersistenceManager.playerHealth);
        devPersistenceManager.currencyAmount = EditorGUILayout.IntField("Currency Amount", devPersistenceManager.currencyAmount);
        EditRuneSockets(devPersistenceManager);
        EditRuneOrder(devPersistenceManager);
        EditorGUI.indentLevel--;
    }

    private void EditAbilityTree(DevPersistenceManager devPersistenceManager)
    {
        EditorUtils.Header("Ability Tree");
        EditorGUI.indentLevel++;

        devPersistenceManager.leftAbilitytype = (AbilityType)EditorGUILayout.EnumPopup("Left Ability", devPersistenceManager.leftAbilitytype);
        EditorUtils.ResizeableList(
            devPersistenceManager.leftAbilityEmpowerments,
            x => (Empowerment)EditorGUILayout.EnumPopup("Empowerment", x),
            Empowerment.Impact,
            itemName: "empowerment");

        EditorUtils.VerticalSpace();

        devPersistenceManager.rightAbilitytype = (AbilityType)EditorGUILayout.EnumPopup("Right Ability", devPersistenceManager.rightAbilitytype);
        EditorUtils.ResizeableList(
            devPersistenceManager.rightAbilityEmpowerments,
            x => (Empowerment)EditorGUILayout.EnumPopup("Empowerment", x),
            Empowerment.Impact,
            itemName: "empowerment");

        EditorGUI.indentLevel--;
    }

    private void EditScene(DevPersistenceManager devPersistenceManager)
    {
        EditorUtils.Header("Scene");
        EditorGUI.indentLevel++;
        devPersistenceManager.cameraOrientation = (Pole)EditorGUILayout.EnumPopup("Camera Orientation", devPersistenceManager.cameraOrientation);
        devPersistenceManager.useRandomSeeds = EditorGUILayout.Toggle("Use Random Seeds", devPersistenceManager.useRandomSeeds);
        if (devPersistenceManager.useRandomSeeds) EditorGUI.BeginDisabledGroup(true);
        devPersistenceManager.moduleSeed = EditorGUILayout.IntField("Module Seed", devPersistenceManager.moduleSeed);
        devPersistenceManager.transitionModuleSeed = EditorGUILayout.IntField("Transitions Module Seed", devPersistenceManager.transitionModuleSeed);
        if (devPersistenceManager.useRandomSeeds) EditorGUI.EndDisabledGroup();
        EditTransitions(devPersistenceManager);
        EditorGUI.indentLevel--;
    }

    private void EditRoom(DevPersistenceManager devPersistenceManager)
    {
        EditorUtils.Header("Room");
        EditorGUI.indentLevel++;
        devPersistenceManager.depth = EditorGUILayout.IntField("Depth", devPersistenceManager.depth);
        devPersistenceManager.zone = (Zone)EditorGUILayout.EnumPopup("Zone", devPersistenceManager.zone);
        devPersistenceManager.depthInZone = EditorGUILayout.IntField("Depth In Zone", devPersistenceManager.depthInZone);
        devPersistenceManager.roomType = (RoomType)EditorGUILayout.EnumPopup("Room Type", devPersistenceManager.roomType);
        if (devPersistenceManager.roomType == RoomType.Combat) EditCombatRoomData(devPersistenceManager);
        if (devPersistenceManager.roomType == RoomType.Healing) EditHealingRoomData(devPersistenceManager);
        EditorGUI.indentLevel--;
    }

    private void EditRuneSockets(DevPersistenceManager devPersistenceManager)
    {
        EditorUtils.Header("Rune Sockets");
        EditorGUI.indentLevel++;

        EditorUtils.ResizeableList(
            devPersistenceManager.runeSockets,
            runeSocket =>
            {
                runeSocket.HasRune = EditorGUILayout.Toggle("Has Rune", runeSocket.HasRune);
                if (!runeSocket.HasRune) EditorGUI.BeginDisabledGroup(true);
                runeSocket.Rune = (Rune) EditorGUILayout.EnumPopup("Rune", runeSocket.Rune);
                if (!runeSocket.HasRune) EditorGUI.EndDisabledGroup();
            },
            () => new RuneSocket()
        );

        EditorGUI.indentLevel--;
    }

    private void EditRuneOrder(DevPersistenceManager devPersistenceManager)
    {
        EditorUtils.Header("Rune Order");
        EditorGUI.indentLevel++;

        EditorUtils.ResizeableList(
            devPersistenceManager.runeOrder,
            rune => (Rune) EditorGUILayout.EnumPopup("Rune", rune),
            Rune.DeepWounds
        );

        EditorGUI.indentLevel--;
    }
    
    private void EditTransitions(DevPersistenceManager devPersistenceManager)
    {
        EditorUtils.Header("Active Transitions");
        EditorGUI.indentLevel++;

        EditorUtils.ResizeableList(
            devPersistenceManager.activeTransitions,
            t => EditorGUILayout.IntField("Transitioner ID", t),
            0
        );

        EditorGUI.indentLevel--;
    }

    private void EditCombatRoomData(DevPersistenceManager devPersistenceManager)
    {
        devPersistenceManager.enemiesCleared = EditorGUILayout.Toggle("Enemies Cleared", devPersistenceManager.enemiesCleared);

        EditorUtils.Header("Spawned Enemies");
        EditorGUI.indentLevel++;

        EditorUtils.ResizeableList(
            devPersistenceManager.spawnedEnemies,
            spawnedEnemy =>
            {
                spawnedEnemy.SpawnerId = EditorGUILayout.IntField("Spawner ID", spawnedEnemy.SpawnerId);
                spawnedEnemy.ActorType = (ActorType) EditorGUILayout.EnumPopup("Actor Type", spawnedEnemy.ActorType);
            },
            () => new DevPersistenceManager.SpawnedEnemy()
        );
        
        EditorGUI.indentLevel--;
    }

    private void EditHealingRoomData(DevPersistenceManager devPersistenceManager)
    {
        devPersistenceManager.hasHealed = EditorGUILayout.Toggle("Has Healed", devPersistenceManager.hasHealed);
    }
}
