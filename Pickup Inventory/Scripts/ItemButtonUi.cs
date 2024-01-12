
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.Inventory
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ItemButtonUi : UdonSharpBehaviour
    {

        public Image ItemIcon;
        public TextMeshProUGUI ItemName;

        public PickupInventory Inventory;
        public DataDictionary DataItem;


        public void _Init(PickupInventory inventory, DataDictionary dataItem)
        {
            Inventory = inventory;
            DataItem = dataItem;

            InventoryItem item = (InventoryItem)dataItem[PickupInventory.ID_ITEM].Reference;

            ItemName.text = item.ItemName;
            ItemIcon.sprite = item.Icon;

        }

        public void _OnClick()
        {
            Inventory._SelectItem(DataItem);
        }

    }
}