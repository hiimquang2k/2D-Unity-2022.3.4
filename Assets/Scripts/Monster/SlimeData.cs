using UnityEngine;

[CreateAssetMenu(fileName = "SlimeData", menuName = "Monster/Slime Data")]
public class SlimeData : MonsterData
{
    [Header("Slime Properties")]
    [Range(0f, 1f)] public float splitChance = 0.5f;
    public int maxSplitGenerations = 2;
    public float acidPoolDuration = 3f;
    public GameObject smallerSlimePrefab;
}