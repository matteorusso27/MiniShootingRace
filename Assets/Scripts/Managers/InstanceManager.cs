using UnityEngine;
using static ScriptableCharacterBase;
using static ScriptableBallBase;
using System.Collections;
using System.Collections.Generic;
using static Helpers;
using System.Linq;

public class InstanceManager : Singleton<InstanceManager>
{
    public List<GameObject> _inGameObjects;

    public void Start()
    {
        _inGameObjects = new List<GameObject>();
    }

    public BallBase[] GetBalls()
    {
        return InstanceManager.Instance._inGameObjects.Where(x => x.GetComponent<BallBase>() != null).Select(x=> x.GetComponent<BallBase>()).ToArray();
    }

    public BallBase GetBall(bool IsPlayer) => GetBalls().Where(x => x.IsPlayer == IsPlayer).FirstOrDefault();
    public void SpawnPlayerAndBall()
    {
        SpawnPlayer(EntityType.Player);
        SpawnBall(BallType.NormalBall, IsPlayer: true);
        SpawnBall(BallType.NormalBall, IsPlayer: false);
    }

    public BallBase SpawnBall(BallType ballType, bool IsPlayer)
    {
        var range = IsPlayer ? 6 : -6; //todo sono in viaggio scusa
        var ballPosition = new Vector3(GetRandomNumber(range, range+2), 4, -3);
        var ballScriptable = ResourceSystem.Instance.GetBalls(ballType);
        var toSpawn = Instantiate(ballScriptable.prefab, ballPosition, Quaternion.identity);
        toSpawn.BallType = ballType;
        toSpawn.IsPlayer = IsPlayer;
        _inGameObjects.Add(toSpawn.gameObject);
        return toSpawn;
    }

    private void SpawnPlayer(EntityType player)
    {
        var characterPosition = new Vector3(1, 3, 0);
        var characterScriptable = ResourceSystem.Instance.GetCharacters(player);
        var toSpawn = Instantiate(characterScriptable.prefab, characterPosition, Quaternion.identity);
        toSpawn.StreakPoints = 0;
        _inGameObjects.Add(toSpawn.gameObject);
    }

    public void RemoveBall(GameObject go)
    {
        _inGameObjects.Remove(go);
    }
}
