using System;
using System.Collections;
using System.Collections.Generic;
using Swift_Blade.Enemy;
using Swift_Blade.Pool;
using UnityEngine;


[Serializable]
public struct SpawnInfo
{
    public BaseEnemy enemy;
    public float delay;
    public Transform spawnPosition;
}

[Serializable]
public struct SpawnInfos
{
    public SpawnInfo[] spawnInfos;
}

namespace Swift_Blade.Level
{
    public class EnemySpawner : MonoBehaviour
    {
        public SceneManagerSO levelEvent;
        public List<SpawnInfos> spawnEnemies;
        
        public PoolPrefabMonoBehaviourSO enemySpawnParticle; 
        
        public int waveCount;
        private int enemyCount;
        private int enemyCounter;
        
        [Header("Portal info")]
        public NodeList nodeList;
        public Transform[] portalTrm;
        
        private void Start()
        {
            MonoGenericPool<EnemySpawnParticle>.Initialize(enemySpawnParticle);
            
            StartCoroutine(Spawn());
        }
        
        private IEnumerator Spawn()
        {
            if (waveCount >= spawnEnemies.Count) 
                yield return null;
            
            enemyCount = 0; 
            enemyCounter = 0;
            
            for (var i = 0; i < spawnEnemies[waveCount].spawnInfos.Length; i++)
            {
                SpawnInfo spawnInfo = spawnEnemies[waveCount].spawnInfos[i];
                
                yield return new WaitForSeconds(spawnInfo.delay);
                
                var newEnemy = Instantiate(spawnInfo.enemy,
                    spawnInfo.spawnPosition.position,Quaternion.identity);
                newEnemy.SetOwner(this);
                ++enemyCount;
                
                EnemySpawnParticle spawnParticle = MonoGenericPool<EnemySpawnParticle>.Pop();
                spawnParticle.transform.position = newEnemy.transform.position;
            }
               
            ++waveCount;
        }

        public void TryNextEnemyCanSpawn()
        {
            ++enemyCounter;
            
            if (waveCount >= spawnEnemies.Count)
            {
                if (enemyCount == enemyCounter)
                {
                    levelEvent.LevelClear();
                    
                    Node[] newNode = nodeList.GetNode();
                    
                    for (int i = 0; i < newNode.Length; ++i)
                    {
                        Portal.Portal newPortal = Instantiate(newNode[i].GetPortalPrefab() , portalTrm[i].position,Quaternion.identity);
                        newPortal.SetScene(newNode[i].nodeName);
                    }
                                        
                }
            }
            else
            {
                if (enemyCount == enemyCounter)
                    StartCoroutine(Spawn());
            }
        }
    }
}