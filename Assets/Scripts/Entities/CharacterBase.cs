using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ScriptableCharacterBase;

public class CharacterBase : MonoBehaviour
{
    public Stats Stats;
    public virtual void SetStats(Stats stats) => Stats = stats;
}
