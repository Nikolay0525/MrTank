using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewTankData", menuName = "Tank Game/Tank Data")]
    public class TankData : ScriptableObject
    {
        public int id;
        public string tankName;
        public int price;
        public Sprite shopIcon;
    }
}
