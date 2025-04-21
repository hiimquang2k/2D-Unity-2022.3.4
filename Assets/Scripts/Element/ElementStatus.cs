using UnityEngine;
using System.Collections.Generic;

public class ElementStatus : MonoBehaviour
{
    // Track active elements and their expiration times
    private Dictionary<Element, float> activeElements = new Dictionary<Element, float>();
    private float defaultDuration = 5f; // Base duration for elemental effects

    [Header("Synergy Settings")]
    public GameObject steamPrefab;
    public GameObject electrifiedWaterPrefab;
    public GameObject lavaPrefab;

    // Apply elemental effect to this object
    public void ApplyElement(Element element, float duration = -1)
    {
        float endTime = Time.time + (duration > 0 ? duration : defaultDuration);
        activeElements[element] = endTime;

        CheckSynergies(); // Immediate synergy check
    }

    void Update()
    {
        // Cleanup expired effects
        List<Element> toRemove = new List<Element>();
        foreach (var kvp in activeElements)
        {
            if (Time.time >= kvp.Value) toRemove.Add(kvp.Key);
        }
        foreach (Element element in toRemove)
        {
            activeElements.Remove(element);
        }
    }

    void CheckSynergies()
    {
        // Water + Lightning = Electrocution
        if (HasElement(Element.Water)) 
        {
            if (HasElement(Element.Lightning))
            {
                CreateElectrifiedWater();
                RemoveElement(Element.Water);
                RemoveElement(Element.Lightning);
            }
            else if (HasElement(Element.Fire)) // Water + Fire = Steam
            {
                CreateSteamCloud();
                RemoveElement(Element.Fire);
            }
        }
        // Earth + Fire = Lava
        else if (HasElement(Element.Earth) && HasElement(Element.Fire))
        {
            CreateLavaPatch();
            RemoveElement(Element.Earth);
        }

        if (HasElement(Element.Fire))
        {
            if (HasElement(Element.Water))
            {
                CreateSteamCloud();
                GetComponent<DamageSystem>()?.ApplyDamage(10, DamageType.Magical);
            RemoveElement(Element.Fire);
        }
    }

    // Water + Lightning = Electrocution (Chain stun)
    else if (HasElement(Element.Water))
    {
        if (HasElement(Element.Lightning))
        {
            Electrocute();
            RemoveElement(Element.Water);
        }
    }
    }
    
    void Electrocute()
    {
        DamageSystem ds = GetComponent<DamageSystem>();
        if (ds != null)
        {
            ds.ApplyDamage(20, DamageType.Lightning);
        ds.ApplyStun(2f); // Assume you add this method to DamageSystem
    }
}
    // Helper methods
    public bool HasElement(Element element) => activeElements.ContainsKey(element);
    public void RemoveElement(Element element) => activeElements.Remove(element);

    // Synergy Effects
    void CreateElectrifiedWater()
    {
        Instantiate(electrifiedWaterPrefab, transform.position, Quaternion.identity);
        GetComponent<DamageSystem>()?.ApplyDamage(15, DamageType.Lightning); // Example enemy effect
    }

    void CreateSteamCloud()
    {
        Instantiate(steamPrefab, transform.position, Quaternion.identity);
        // Optional: Reduce visibility in steam area
    }

    void CreateLavaPatch()
    {
        Instantiate(lavaPrefab, transform.position, Quaternion.identity);
        // Convert tile to lava via TileMorpher
    }
}