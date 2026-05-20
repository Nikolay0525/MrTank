using Assets.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class TankShopItem : MonoBehaviour
    {
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI priceText;
        public Image tankIcon;
        public Button actionButton;
        private TextMeshProUGUI buttonText;

        private int tankID;
        private TankData data;
        private GarageManager manager;

        private void Awake()
        {
            buttonText = actionButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        public void Setup(TankData tankData, int id, GarageManager garageManager)
        {
            data = tankData;
            tankID = id;
            manager = garageManager;

            nameText.text = data.tankName;
            tankIcon.sprite = data.shopIcon;

            UpdateButtonState();

            actionButton.onClick.AddListener(OnButtonClicked);
        }

        public void UpdateButtonState()
        {
            bool isUnlocked = true; //StatsManager.Instance.currentStats.unlockedTankIDs.Contains(tankID);
            bool isSelected = true; //StatsManager.Instance.currentStats.selectedTankID == tankID;

            if (isSelected)
            {
                buttonText.text = "Selected";
                actionButton.interactable = false;
                priceText.text = "Owned";
            }
            else if (isUnlocked)
            {
                buttonText.text = "Select";
                actionButton.interactable = true;
                priceText.text = "Owned";
            }
            else
            {
                buttonText.text = "Buy";
                actionButton.interactable = true;
                priceText.text = data.price.ToString();
            }
        }

        private void OnButtonClicked()
        {
            manager.HandleTankAction(tankID, data.price);
        }
    }
}
