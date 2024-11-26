using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemy
{
    //public int ID { get; set; }
    public IEnemySpawner Spawner { get; set; }
    //int Experience { get; set; }
    //void Die();
    void TakeDamage(float amount);
    //void PerformAttack();
}
