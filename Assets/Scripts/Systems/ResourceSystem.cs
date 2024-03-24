using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ScriptableCharacterBase;
using static ScriptableBallBase;

public class ResourceSystem : Singleton<ResourceSystem>
{
    public List<ScriptableCharacterBase> Characters { get; private set; }
    public List<ScriptableBallBase> Balls { get; private set; }
    private Dictionary<EntityType, ScriptableCharacterBase> CharactersDict;
    private Dictionary<BallType, ScriptableBallBase>   BallsDict;

    protected override void Awake()
    {
        base.Awake();
        AssembleResources();
    }

    private void AssembleResources()
    {
        Characters = Resources.LoadAll<ScriptableCharacterBase>("Entities/Characters").ToList();
        Balls = Resources.LoadAll<ScriptableBallBase>("Entities/Balls").ToList();
        CharactersDict = Characters.ToDictionary(x => x.Player);
        BallsDict = Balls.ToDictionary(x => x.Ball);
    }

    public ScriptableCharacterBase GetCharacters(EntityType e) => CharactersDict[e];
    public ScriptableBallBase GetBalls(BallType e) => BallsDict[e];
}
