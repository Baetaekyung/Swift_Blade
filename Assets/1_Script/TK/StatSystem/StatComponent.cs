using System;
using System.Linq;
using Swift_Blade.Combat.Health;
using UnityEngine;

namespace Swift_Blade
{
    public abstract class StatComponent : MonoBehaviour
    {
        [SerializeField] protected StatSO[] _defaultStats;
        protected static StatSO[] _statDatas;

        public event Action OnStatChanged;
        
        protected virtual void Initialize()
        {
            StatSO[] tempStatSO = new StatSO[_defaultStats.Length];

            for (int i = 0; i < _defaultStats.Length; i++)
            {
                tempStatSO[i] = _defaultStats[i].Clone();

                if (tempStatSO[i].statType == StatType.HEALTH)
                    PlayerHealth.CurrentHealth = tempStatSO[i].Value;
            }

            _statDatas = tempStatSO;
        }

        public StatSO GetStat(StatSO stat)
        {
            StatSO findStat = _statDatas.FirstOrDefault(x => x.statName == stat.statName);
            Debug.Assert(findStat != null, "stat can't find");

            return findStat;
        }

        public StatSO GetStat(StatType statType)
        {
            StatSO findStat = _statDatas.FirstOrDefault(x => x.statType == statType);
            Debug.Assert(findStat != null, "stat can't find");
            
            return findStat;
        }

        public void AddModifier(StatType statType, object key, float value)
        {
            GetStat(statType).AddModifier(key, value);
            OnStatChanged?.Invoke();
        }

        public void RemoveModifier(StatType statType, object key)
        {
            GetStat(statType).RemoveModifier(key);
            OnStatChanged?.Invoke();
        }
        
        public void ClearAllModifiers()
        {
            foreach (StatSO stat in _statDatas)
            {
                stat.ClearModifier();
            }
        }

        public static StatSO[] GetAllStats() => _statDatas;
        public float GetColorValue(StatSO stat) => GetStat(stat).ColorValue;
    }
}
