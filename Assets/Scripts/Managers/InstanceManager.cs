using UnityEngine;
using static ScriptableCharacterBase;
using static ScriptableBallBase;

public class InstanceManager : Singleton<InstanceManager>
{
    public void SpawnPlayerAndBall()
    {
        var characterPosition = new Vector3(1, 0, 0);

        SpawnPlayer(EntityType.Player, characterPosition);
        SpawnBall(BallType.NormalBall);
    }

    public void SpawnBall(BallType ballType)
    {
        var ballPosition = new Vector3(1, 1, 0);
        var ballScriptable = ResourceSystem.Instance.GetBalls(ballType);
        var toSpawn = Instantiate(ballScriptable.prefab, ballPosition, Quaternion.identity);
    }
    //spawn ball..

    private void SpawnPlayer(EntityType player, Vector3 vector3)
    {
        var characterScriptable = ResourceSystem.Instance.GetCharacters(player);
        var toSpawn = Instantiate(characterScriptable.prefab, vector3, Quaternion.identity);
        var stats = characterScriptable.BaseStats;
        stats.Score = 0;
        toSpawn.SetStats(stats);
    }
}
