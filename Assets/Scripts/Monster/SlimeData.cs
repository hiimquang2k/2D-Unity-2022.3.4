using UnityEngine;

[CreateAssetMenu(fileName = "SlimeData", menuName = "Monster/Slime Data")]
public class SlimeData : MonsterData
{
    [Header("Slime Properties")]
    public bool canSplit = true;
    [Range(0f, 1f)] public float splitChance = 0.5f;
    public int maxSplitGenerations = 2;
    public float acidPoolDuration = 3f;
    public GameObject smallerSlimePrefab;
    
    [Header("Jump Properties")]
    public float jumpForce = 10f;
    public float jumpInterval = 2f;

    [Header("Retreat Properties")]
    public float retreatSpeed = 3f;
    public float retreatDuration = 2f;
}