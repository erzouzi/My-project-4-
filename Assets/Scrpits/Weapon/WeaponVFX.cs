using System;
using UnityEngine;

[Serializable]
public class SlashConfig
{
    public Transform spawnPoint;
    public GameObject slashPrefab; // 不填则用默认
}

public class WeaponVFX : MonoBehaviour
{
    public GameObject defaultSlashPrefab;
    public SlashConfig[] slashConfigs;

    public void PlaySlash(int index)
    {
        if (index < 0 || index >= slashConfigs.Length) return;

        SlashConfig cfg = slashConfigs[index];
        if (cfg.spawnPoint == null) return;

        GameObject prefab = cfg.slashPrefab != null ? cfg.slashPrefab : defaultSlashPrefab;
        if (prefab == null) return;

        GameObject slash = Instantiate(prefab, cfg.spawnPoint.position, cfg.spawnPoint.rotation);
        Destroy(slash, 1.5f);
    }
}
