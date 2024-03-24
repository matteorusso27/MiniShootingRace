using UnityEngine;
using static ScriptableCharacterBase;
using static ScriptableBallBase;
using System.Collections;
using System.Collections.Generic;

public class InstanceManager : Singleton<InstanceManager>
{
    public List<GameObject> _inGameObjects;

    public void Start()
    {
        _inGameObjects = new List<GameObject>();
    }

    public void SpawnPlayerAndBall()
    {
        SpawnPlayer(EntityType.Player);
        SpawnBall(BallType.NormalBall);
    }

    public void SpawnBall(BallType ballType)
    {
        Debug.Log("spaen ball");
        var ballPosition = new Vector3(4, 5, -24);
        var ballScriptable = ResourceSystem.Instance.GetBalls(ballType);
        var toSpawn = Instantiate(ballScriptable.prefab, ballPosition, Quaternion.identity);
        _inGameObjects.Add(toSpawn.gameObject);
    }

    private void SpawnPlayer(EntityType player)
    {
        var characterPosition = new Vector3(1, 3, 0);
        var characterScriptable = ResourceSystem.Instance.GetCharacters(player);
        var toSpawn = Instantiate(characterScriptable.prefab, characterPosition, Quaternion.identity);
        var stats = characterScriptable.BaseStats;
        stats.Score = 0;
        toSpawn.SetStats(stats);
        _inGameObjects.Add(toSpawn.gameObject);
    }
}
