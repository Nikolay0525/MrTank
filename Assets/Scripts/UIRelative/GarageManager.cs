using Assets.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Assets.Scripts.StatsManager;

namespace Assets.Scripts
{
    public class GarageManager : MonoBehaviour
    {
        public TankData[] allTanks; 
        public GameObject shopItemPrefab;
        public Transform contentContainer;

        private List<TankShopItem> spawnedItems = new List<TankShopItem>();

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
                item.Setup(allTanks[i], i, this);
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

                UIManager.Instance.CoinsText.text = stats.coins.ToString();
            }
            else
            {
                Debug.Log("Not enough coins!");
                return;
            }

            StatsManager.Instance.SaveStats();
            RefreshAllButtons();
        }

        private void RefreshAllButtons()
        {
            foreach (var item in spawnedItems)
            {
                item.UpdateButtonState();
            }
        }
    }
}
