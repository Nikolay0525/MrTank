using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
    public class StatsUIFiller : MonoBehaviour
    {
        public StatsManager statsManager;
        public GameObject statPrefab;
        public Transform contentPanel;

        private void OnEnable()
        {
            PopulateUI();
        }

        private void PopulateUI()
        {
            foreach (Transform child in contentPanel)
            {
                Destroy(child.gameObject);
            }

            CreateStatUI("Total kills", statsManager.currentStats.TotalKills);
            CreateStatUI("Max kill streak", statsManager.currentStats.maxKillStreak);
            CreateStatUI("Best score", statsManager.currentStats.bestScore);
            CreateStatUI("Coins amount", statsManager.currentStats.coins);
        }

        private void CreateStatUI(string title, int value)
        {
            GameObject newStatItem = Instantiate(statPrefab, contentPanel);

            TextMeshProUGUI textComp = newStatItem.GetComponentInChildren<TextMeshProUGUI>();
            if (textComp != null)
            {
                textComp.text = $"{title}: {value}";
            }
        }
    }
}
