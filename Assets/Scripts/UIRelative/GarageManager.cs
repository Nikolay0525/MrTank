using Assets.ScriptableObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Assets.Scripts.StatsManager;

namespace Assets.Scripts
{
    public class GarageManager : MonoBehaviour
    {
        public static GarageManager Instance { get; private set; }

        public static event Action<SceneData> OnTankEquipped;

        [Header("Shop Content")]
        public TankData[] allTanks;
        public GameObject shopItemPrefab;
        public Transform contentContainer;

        [Header("Spawn Settings")]
        public Transform playerSpawnPoint;
        private GameObject currentTankInstance;

        public TankData CurrentSelectedTank { get; private set; }

        private List<TankShopItem> spawnedItems = new List<TankShopItem>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            UpdateCurrentTankReference();
        }

        private void OnEnable()
        {
            PopulateShop();
        }

        private void PopulateShop()
        {
            foreach (Transform child in contentContainer)
            {
                Destroy(child.gameObject);
            }
            spawnedItems.Clear();

            for (int i = 0; i < allTanks.Length; i++)
            {
                GameObject go = Instantiate(shopItemPrefab, contentContainer);
                TankShopItem item = go.GetComponent<TankShopItem>();
                item.Setup(allTanks[i], allTanks[i].id, this);
                spawnedItems.Add(item);
            }
        }

        public void HandleTankAction(int tankID, int price)
        {
            PlayerStats stats = StatsManager.Instance.currentStats;

            if (stats.unlockedTankIDs.Contains(tankID))
            {
                stats.selectedTankID = tankID;
            }
            else if (stats.coins >= price)
            {
                stats.coins -= price;
                stats.unlockedTankIDs.Add(tankID);
                stats.selectedTankID = tankID;

                if (UIManager.Instance != null && UIManager.Instance.CoinsText != null)
                {
                    UIManager.Instance.CoinsText.text = stats.coins.ToString();
                }
            }
            else
            {
                Debug.Log("Not enough coins!");
                return;
            }

            StatsManager.Instance.SaveStats();
            UpdateCurrentTankReference();
            RefreshAllButtons();
        }

        private void UpdateCurrentTankReference()
        {
            int currentID = StatsManager.Instance.currentStats.selectedTankID;

            foreach (var tank in allTanks)
            {
                if (tank.id == currentID)
                {
                    CurrentSelectedTank = tank;

                    if (CurrentSelectedTank.sceneData != null)
                    {
                        StartCoroutine(EquipTankRoutine(CurrentSelectedTank.sceneData));
                    }
                    break;
                }
            }
        }

        private IEnumerator EquipTankRoutine(SceneData sceneData)
        {
            PoolManager.Instance.PrewarmSceneData(sceneData);

            SpawnPlayerTank(sceneData.playerTankPrefab);

            yield return null;

            OnTankEquipped?.Invoke(sceneData);
        }

        private void SpawnPlayerTank(GameObject prefab)
        {
            if (prefab == null) return;

            if (currentTankInstance != null)
            {
                Destroy(currentTankInstance);
            }

            Vector3 spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
            currentTankInstance = Instantiate(prefab, spawnPos, Quaternion.identity);
        }

        private void RefreshAllButtons()
        {
            foreach (var item in spawnedItems) item.UpdateButtonState();
        }
    }
}