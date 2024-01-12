
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace Vowgan.Inventory
{

    public enum SortingMethod
    {
        Latest,
        AZ,
    }
    
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PickupInventory : UdonSharpBehaviour
    {

        public SortingMethod Sorting;
        public float SpawnInsteadOfCloseDistance = 1;
        public float HideDistance = 5;
        
        [Header("References")]
        public GameObject ButtonPrefab;
        public Transform ButtonParent;
        public Transform SpawnPoint;
        public MeshRenderer CanvasVisibleChecker;
        public InventoryInserter Inserter;

        [Header("UI")] 
        public GameObject MenuContainer;
        public InputField SearchingField;
        public Dropdown SortingDropdown;
        public Image ItemIcon;
        public TextMeshProUGUI ItemName;
        public TextMeshProUGUI ItemDescription;
        public Button SpawnButton;
        
        public const string ID_BUTTON = "Button";
        public const string ID_ITEM = "Item";

        [HideInInspector] public DataList ItemList = new DataList();
        
        private VRCPlayerApi localPlayer;
        private DataDictionary selectedItem;
        private float lastVertical;
        private float nextTimeDistanceCheck;
        
        
        private void Start()
        {
            localPlayer = Networking.LocalPlayer;
            ClearMenu();
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                ToggleMenu();
            }

            if (Time.realtimeSinceStartup >= nextTimeDistanceCheck)
            {
                nextTimeDistanceCheck += 2;
                
                if (Vector3.Distance(localPlayer.GetPosition(), transform.position) > HideDistance)
                {
                    if (!CanvasVisibleChecker.isVisible) _CloseMenu();
                }
            }
            
        }

        public override void InputLookVertical(float value, UdonInputEventArgs args)
        {
            if (!localPlayer.IsUserInVR()) return;
            
            if (lastVertical > -0.5f && value <= -0.5f)
            {
                ToggleMenu();
            }
            
            lastVertical = value;
        }

        private void ToggleMenu()
        {
            if (MenuContainer.activeSelf)
            {
                if (Vector3.Distance(localPlayer.GetPosition(), transform.position) > SpawnInsteadOfCloseDistance)
                {
                    SpawnAtPlayer();
                }
                else
                {
                    _CloseMenu();
                }
            }
            else
            {
                SpawnAtPlayer();
            }
        }
        
        private void SpawnAtPlayer()
        {
            MenuContainer.SetActive(true);
            transform.localScale = Vector3.one * localPlayer.GetAvatarEyeHeightAsMeters();
            transform.position = Networking.LocalPlayer.GetPosition();
            transform.rotation = Networking.LocalPlayer.GetRotation();
        }
        
        public void _AddItem(InventoryItem item)
        {
            Networking.SetOwner(Networking.LocalPlayer, item.gameObject);

            GameObject buttonObj = Instantiate(ButtonPrefab, ButtonParent);
            buttonObj.name = $"{item.name} Button";

            DataDictionary itemDictionary = new DataDictionary();
            itemDictionary[ID_BUTTON] = buttonObj;
            itemDictionary[ID_ITEM] = item;
            ItemList.Add(itemDictionary);

            ItemButtonUi button = buttonObj.GetComponent<ItemButtonUi>();
            button._Init(this, itemDictionary);
            
            item._Hide();
            _SelectItem(itemDictionary);
            _SortList();
            Inserter._Highlight(false);
        }

        public void _SelectItem(DataDictionary dataItem)
        {
            selectedItem = dataItem;
            InventoryItem item = (InventoryItem)dataItem[ID_ITEM].Reference;

            ItemIcon.sprite = item.Icon;
            ItemIcon.color = Color.white;
            ItemName.text = item.ItemName;
            ItemDescription.text = item.ItemDescription;
            SpawnButton.interactable = true;
        }
        
        
        public void _SpawnItem()
        {
            if (ItemList.Count == 0) return;
            if (selectedItem == null) return;
            int index = ItemList.IndexOf(selectedItem);
            if (index == -1) return;
            
            InventoryItem item = (InventoryItem)selectedItem[ID_ITEM].Reference;
            _RemoveItem(item);
            
            item._Spawn(SpawnPoint);
        }
        
        /// <summary>
        /// Removes an item from the ItemList, updating the UI accordingly.
        /// </summary>
        /// <param name="item"></param>
        public void _RemoveItem(InventoryItem item)
        {
            int index = -1;
            for (int i = 0; i < ItemList.Count; i++)
            {
                if(((InventoryItem)ItemList[i].DataDictionary[ID_ITEM].Reference) == item)
                {
                    index = i;
                    break;
                }
            }
            if (index == -1) return;
            
            GameObject button = (GameObject)ItemList[index].DataDictionary[ID_BUTTON].Reference;
            Destroy(button);
            
            ItemList.RemoveAt(index);

            if (ItemList.Count == 0)
            {
                ClearMenu();
            }
            else
            {
                int clampedIndex = Mathf.Clamp(index, 0, ItemList.Count - 1);
                _SelectItem(ItemList[clampedIndex].DataDictionary);
            }
        }

        public void _SearchForItems()
        {
            string searchText = SearchingField.text.ToLowerInvariant();
            
            if (searchText == string.Empty)
            {
                for (int i = 0; i < ButtonParent.childCount; i++)
                {
                    ButtonParent.GetChild(i).gameObject.SetActive(true);
                }

                return;
            }
            
            for (int i = 0; i < ButtonParent.childCount; i++)
            {
                GameObject child = ButtonParent.GetChild(i).gameObject;
                InventoryItem item = (InventoryItem) child.GetComponent<ItemButtonUi>().DataItem[ID_ITEM].Reference;
                
                child.SetActive(item.ItemName.ToLowerInvariant().Contains(searchText));
            }
        }

        public void _CloseMenu()
        {
            MenuContainer.SetActive(false);
        }

        public void _SetSorting()
        {
            Sorting = (SortingMethod)SortingDropdown.value;
            _SortList();
        }
        
        public void _SortList()
        {
            int childCount = ButtonParent.childCount;

            for (int x = 0; x < childCount - 1; x++)
            {
                for (int y = 0; y < childCount - x - 1; y++)
                {
                    Transform child1 = ButtonParent.GetChild(y);
                    Transform child2 = ButtonParent.GetChild(y + 1);
                    
                    InventoryItem item1 = (InventoryItem) child1.GetComponent<ItemButtonUi>().DataItem[ID_ITEM].Reference;
                    InventoryItem item2 = (InventoryItem) child2.GetComponent<ItemButtonUi>().DataItem[ID_ITEM].Reference;

                    switch (Sorting)
                    {
                        case SortingMethod.AZ:
                            // Compare and swap if necessary
                            if (string.CompareOrdinal(item1.ItemName, item2.ItemName) > 0)
                            {
                                child1.SetSiblingIndex(y + 1);
                                child2.SetSiblingIndex(y);
                            }
                            break;
                        
                        case SortingMethod.Latest:
                            if (item1.StoredTimestamp < item2.StoredTimestamp)
                            {
                                child1.SetSiblingIndex(y + 1);
                                child2.SetSiblingIndex(y);
                            }
                            break;
                    }
                }
            }
        }
        
        private void ClearMenu()
        {
            ItemIcon.sprite = null;
            ItemIcon.color = Color.clear;
            ItemName.text = string.Empty;
            ItemDescription.text = string.Empty;
            SpawnButton.interactable = false;
        }
    }
}