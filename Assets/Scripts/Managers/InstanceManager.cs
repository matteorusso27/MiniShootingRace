using UnityEngine;
using static ScriptableCharacterBase;
using static ScriptableBallBase;
using System.Collections;
using System.Collections.Generic;
using static Helpers;
using System.Linq;

public class InstanceManager : Singleton<InstanceManager>
{
    public List<GameObject> SceneObjects;
    public GameObject       FireVFX;
    public GameObject       BoardVFX;
    public void Start() => SceneObjects = new List<GameObject>();

    public BallBase[] GetBalls()
    {
        return SceneObjects.Where(x => x.GetComponent<BallBase>() != null).Select(x=> x.GetComponent<BallBase>()).ToArray();
    }

    public GameObject GetFire() => SceneObjects.Where(x => x.GetComponent<ParticleSystem>() != null).FirstOrDefault();

    public BallBase GetBall(bool IsPlayer) => GetBalls().Where(x => x.IsPlayer == IsPlayer).FirstOrDefault();
    public void SpawnPlayerAndBalls()
    {
        //SpawnPlayer(EntityType.Player);
        SpawnBall(BallType.NormalBall, IsPlayer: true);
        SpawnBall(BallType.NormalBall, IsPlayer: false);
    }

    public void SpawnBall(BallType ballType, bool IsPlayer)
    {
        var range = IsPlayer ? 6 : -6; //todo sono in viaggio scusa
        var ballPosition = new Vector3(GetRandomNumber(range, range+2), 4, -3);
        var ballScriptable = ResourceSystem.Instance.GetBalls(ballType);
        var toSpawn = Instantiate(ballScriptable.prefab, ballPosition, Quaternion.identity);
        toSpawn.BallType = ballType;
        toSpawn.IsPlayer = IsPlayer;
        SceneObjects.Add(toSpawn.gameObject);
    }

    //Todo add player to animate
    private void SpawnPlayer(EntityType player)
    {
        var characterPosition = new Vector3(1, 3, 0);
        var characterScriptable = ResourceSystem.Instance.GetCharacters(player);
        var toSpawn = Instantiate(characterScriptable.prefab, characterPosition, Quaternion.identity);
        toSpawn.StreakPoints = 0;
        SceneObjects.Add(toSpawn.gameObject);
    }

    public void SpawnFire()
    {
        var playerBall = GetBall(true).transform;
        var toSpawn = Instantiate(FireVFX, playerBall.position, Quaternion.identity);
        toSpawn.transform.SetParent(playerBall);
        SceneObjects.Add(toSpawn);
    }

    public void DestroyFire()
    {
        var fire = GetFire();
        if (fire == null) return;
        SceneObjects.Remove(fire);
        Destroy(fire);
    }
}
