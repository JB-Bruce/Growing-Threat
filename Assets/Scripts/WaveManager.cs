using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public List<Wave> waves = new List<Wave>();

    int currentWave = 0;

    UnitManager unitManager;


    public void InitWaves(UnitManager _unitManager)
    {
        unitManager = _unitManager;
        Invoke("SpawnWave", waves[currentWave].timeToSpawn);
    }

    private void SpawnWave()
    {
        Wave curWave = waves[currentWave];

        foreach (WaveElement waveElmt in curWave.waveElements) 
        {
            unitManager.SpawnUnitAtBorder(waveElmt.units);
        }

        if (currentWave < waves.Count - 1) 
            currentWave++;
        
        Invoke("SpawnWave", waves[currentWave].timeToSpawn);
    }
}

[System.Serializable]
public struct Wave
{
    public float timeToSpawn;
    public WaveElement[] waveElements;
}

[System.Serializable]
public struct WaveElement
{
    [Range(1, 9)]
    public int units;
    public UnitSpecialization unitSpecialization;
}
