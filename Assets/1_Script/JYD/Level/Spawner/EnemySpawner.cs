using System.Collections;
using UnityEngine;


namespace Swift_Blade.Level
{
    public class EnemySpawner : Spawner
    {
        private int enemyCount;
        private int enemyCounter;

        
        protected override IEnumerator Spawn()
        {
            if (waveCount >= spawnEnemies.Count)
            {
                Debug.LogError("Enemy count exceeded. This shouldn't happen.");
                yield break; 
            }
            
            Debug.Assert(isClear == false , "Already cleared");
                        
            enemyCounter = 0;

            var currentSpawnInfo = spawnEnemies[waveCount++].spawnInfos;
            enemyCount = currentSpawnInfo.Length;

            float addHealthAmount = CalculateHealthAdditional();
            
            for (int i = 0; i < currentSpawnInfo.Length; i++)
            {
                yield return new WaitForSeconds(currentSpawnInfo[i].delay);

                var newEnemy = Instantiate(currentSpawnInfo[i].enemy,
                    currentSpawnInfo[i].spawnPosition.position,
                    Quaternion.identity);
                
                newEnemy.GetHealth().AddMaxHealth(addHealthAmount);
                newEnemy.GetHealth().OnDeadEvent.AddListener(TryNextEnemyCanSpawn);

                PlaySpawnParticle(newEnemy.transform.position);
            }
            
        }
        
        private void TryNextEnemyCanSpawn()
        {
            if(isClear)return;
            
            ++enemyCounter;
            
            var isCurrentWaveClear = enemyCounter >= enemyCount;
            if (isCurrentWaveClear)
            {
                isClear = waveCount >= spawnEnemies.Count;
                StartCoroutine(isClear ? LevelClear() : Spawn());
            }
        }
        
        
        
    }
}