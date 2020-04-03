using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.Networking;

public class Health : MonoBehaviour
{

    public const int maxHealth = 100;
    /*[SyncVar (hook = "OnChangeHealth")]*/public int currentHealth = maxHealth;
    public RectTransform healthbar;
    public bool destroyOnDeath;
    //private NetworkStartPosition[] spawnPoints;

    // Start is called before the first frame update
    void Start()
    {
        //if (isLocalPlayer)
        //{
        //    spawnPoints = FindObjectsOfType<NetworkStartPosition>();
        //}
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int amount)
    {

        //if (!isServer)
        //{
        //    return;
        //}

        currentHealth -= amount;
        OnChangeHealth(currentHealth);
        if (currentHealth <= 0)
        {

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
            else
            {
                currentHealth = maxHealth;
                OnChangeHealth(currentHealth);
                //RpcRespawn();

            }
        }

    }

    void OnChangeHealth(int health)
    {
        healthbar.sizeDelta = new Vector2(currentHealth, healthbar.sizeDelta.y);

    }

    //[ClientRpc]
    //void RpcRespawn()
    //{
    //    if (isLocalPlayer)
    //    {
    //        //transform.position = Vector3.zero;
    //        Vector3 spawnPoint = Vector3.zero;

    //        if(spawnPoint!=null && spawnPoints.Length > 0)
    //        {
    //            spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
    //        }

    //        transform.position = spawnPoint;
    //    }
    //}
}
