using UnityEngine;

[CreateAssetMenu(fileName = "NecromancerData", menuName = "Monster/Necromancer Data")]
public class NecromancerData : MonsterData
{
    [Header("Necromancer Specifics")]
    [Range(1, 5)] public int maxSkeletons = 3;
    [Range(5f, 30f)] public float summonCooldown = 10f;
    [Range(0.5f, 5f)] public float summonRadius = 2f;
    public float skeletonLifetime = 20f;
    public GameObject skeletonPrefab;

    [Header("Summoning Effects")]
    public GameObject summonParticles;
    public AudioClip summonSound;
}