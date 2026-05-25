using Assets.ScriptableObjects;
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


        public void Setup(TankData tankData, int id, GarageManager garageManager)
        {
            if (buttonText == null)
            {
                buttonText = actionButton.GetComponentInChildren<TextMeshProUGUI>();
            }

            data = tankData;
            tankID = id;
            manager = garageManager;

            nameText.text = data.tankName;
            tankIcon.sprite = data.shopIcon;

            UpdateButtonState();

            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnButtonClicked);

            buttonText = actionButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        public void UpdateButtonState()
        {
            bool isUnlocked = StatsManager.Instance.currentStats.unlockedTankIDs.Contains(tankID);
            bool isSelected = StatsManager.Instance.currentStats.selectedTankID == tankID;

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