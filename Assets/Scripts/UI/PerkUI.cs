using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class PerkUI : MonoBehaviour
{
    public Text perkDisplayText;
    public PerkSystem perkSystem;

    void Update()
    {
        DisplayPerks();
    }

    void DisplayPerks()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var perk in perkSystem.perks)
        {
            sb.AppendLine($"Perk: {perk.perkName}");
            sb.AppendLine($"Level: {perk.level}/{perk.maxLevel}");
            sb.AppendLine($"Health Bonus: {perk.healthBonus}");
            sb.AppendLine($"Damage Bonus: {perk.damageBonus}");
            float xpRequired = perk.GetXpRequirement();
            sb.AppendLine($"XP: {perkSystem.currentXp}/{xpRequired} (Acquired/Needed)");
            sb.AppendLine("");
        }
        perkDisplayText.text = sb.ToString();
    }
}
