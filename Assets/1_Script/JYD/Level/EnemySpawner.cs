using System;
using System.Collections.Generic;
using Swift_Blade.Enemy;
using UnityEngine;

[Serializable]
public struct SpawnInfo
{
    public BaseEnemy enemy;
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
        public LevelClearEventSO levelEvent;
        public List<SpawnInfos> spawnEnemies;

        public int waveCount;
        private int enemyCount;
        private int enemyCounter;

        private void Start()
        {
            Spawn();
        }

        private void Spawn()
        {
            if (waveCount >= spawnEnemies.Count) return;

            enemyCount = 0;
            enemyCounter = 0;

            for (var i = 0; i < spawnEnemies[waveCount].spawnInfos.Length; i++)
            {
                var newEnemy = Instantiate(spawnEnemies[waveCount].spawnInfos[i].enemy,
                    spawnEnemies[waveCount].spawnInfos[i].spawnPosition);
                newEnemy.transform.SetParent(null);
                newEnemy.SetOwner(this);
                ++enemyCount;
            }

            ++waveCount;
        }

        public void TryNextEnemyCanSpawn()
        {
            ++enemyCounter;
            
            if (waveCount >= spawnEnemies.Count)
            {
                if (enemyCount == enemyCounter) levelEvent.LevelClearEvent?.Invoke();
            }
            else
            {
                if (enemyCount == enemyCounter) Spawn();
            }
        }
    }
}