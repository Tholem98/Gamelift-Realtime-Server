using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Networking;

public class EnemySpawner : MonoBehaviour
{

    public GameObject enemyPrefab;
    public int numberOfEnemies;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            Vector3 spawnPosition = new Vector3(Random.Range(-8.0f, 8.0f), 0, Random.Range(-8.0f, 8.0f));
            Quaternion spawnRotation = Quaternion.Euler(0.0f, Random.Range(180.0f, 0f), 0);

            GameObject enemy = (GameObject)Instantiate(enemyPrefab, spawnPosition, spawnRotation);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public override void OnStartServer()
    //{
    //    for(int i = 0; i < numberOfEnemies; i++)
    //    {
    //        Vector3 spawnPosition = new Vector3(Random.Range(-8.0f, 8.0f),0, Random.Range(-8.0f, 8.0f));
    //        Quaternion spawnRotation = Quaternion.Euler(0.0f, Random.Range(180.0f, 0f), 0);

    //        GameObject enemy = (GameObject)Instantiate(enemyPrefab, spawnPosition, spawnRotation);
    //        //NetworkServer.Spawn(enemy);
    //    }
    //}

}
