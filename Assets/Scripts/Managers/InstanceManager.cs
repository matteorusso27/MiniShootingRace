using UnityEngine;
using static ScriptableCharacterBase;
using static ScriptableBallBase;

public class InstanceManager : Singleton<InstanceManager>
{
    public void SpawnPlayerAndBall()
    {
        SpawnPlayer(EntityType.Player);
        SpawnBall(BallType.NormalBall);
    }

    public void SpawnBall(BallType ballType)
    {
        var ballPosition = new Vector3(1, 1, 0);
        var ballScriptable = ResourceSystem.Instance.GetBalls(ballType);
        var toSpawn = Instantiate(ballScriptable.prefab, ballPosition, Quaternion.identity);
    }

    private void SpawnPlayer(EntityType player)
    {
        var characterPosition = new Vector3(1, 0, 0);
        var characterScriptable = ResourceSystem.Instance.GetCharacters(player);
        var toSpawn = Instantiate(characterScriptable.prefab, characterPosition, Quaternion.identity);
        var stats = characterScriptable.BaseStats;
        stats.Score = 0;
        toSpawn.SetStats(stats);
    }
}
