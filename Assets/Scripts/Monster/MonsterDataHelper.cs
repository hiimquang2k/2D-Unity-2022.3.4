// MonsterDataHelper.cs
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public static class MonsterDataHelper
{
    public static MonsterData GetOrCreateMonsterData(MonsterType type)
    {
        string assetName = type.ToString() + "Data";
        string path = "Assets/Resources/MonsterData/";
        string assetPath = path + assetName + ".asset";

        // Check if the asset exists
        if (File.Exists(assetPath))
        {
            return AssetDatabase.LoadAssetAtPath<MonsterData>(assetPath);
        }

        // Create the directory if it doesn't exist
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        // Create new MonsterData
        MonsterData data = ScriptableObject.CreateInstance<MonsterData>();
        data.type = type;

        // Set default values based on monster type
        switch (type)
        {
            case MonsterType.Goblin:
                data.baseHealth = 50;
                data.moveSpeed = 3f;
                data.attackDamage = 10;
                data.attackRange = 1f;
                data.attackCooldown = 1f;
                data.canPatrol = true;
                data.canChase = false;
                break;

            case MonsterType.Orc:
                data.baseHealth = 100;
                data.moveSpeed = 2f;
                data.attackDamage = 15;
                data.attackRange = 1.5f;
                data.attackCooldown = 1.5f;
                data.canChase = true;
                break;

            case MonsterType.Dragon:
                data.baseHealth = 200;
                data.moveSpeed = 2.5f;
                data.attackDamage = 25;
                data.attackRange = 2f;
                data.attackCooldown = 2f;
                data.canRandomMove = true;
                break;
        }

        // Save the asset
        AssetDatabase.CreateAsset(data, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return data;
    }

    [MenuItem("Tools/Create Default Monster Data")]
    public static void CreateDefaultMonsterData()
    {
        foreach (MonsterType type in System.Enum.GetValues(typeof(MonsterType)))
        {
            GetOrCreateMonsterData(type);
        }
    }
}