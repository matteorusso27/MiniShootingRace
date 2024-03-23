using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ScriptableCharacterObject")]
public class ScriptableCharacterBase : ScriptableObject
{
    public EntityType Player;

    [SerializeField]
    private Stats stats;
    public Stats BaseStats => stats;

    public CharacterBase prefab;
    public struct Stats
    {
        public int Score;
    }
    public enum EntityType
    {
        Player = 0,
        Enemy
    }
}
