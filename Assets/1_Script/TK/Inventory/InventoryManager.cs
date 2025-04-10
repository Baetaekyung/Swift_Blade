using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Swift_Blade
{
    [Serializable]
    public enum ItemType
    {
        ITEM,
        EQUIPMENT
    }
    
    public class InventoryManager : MonoSingleton<InventoryManager>
    {
        //TODO: 나중에 UI랑 Manager기능 분리하기.
        [FormerlySerializedAs("equipInfoUIs")]
        [Header("UI 부분")]
        [SerializeField] private QuickSlotUI         quickSlotUI;
        [SerializeField] private List<EquipmentSlot> equipSlots;

        [Header("Item Information")]
        [SerializeField] private Image           itemIcon;
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private TextMeshProUGUI itemDescription;
        [SerializeField] private TextMeshProUGUI itemTypeInfo;

        [SerializeField] private TextMeshProUGUI  titleText;
        [SerializeField] private SlotChangeButton inventoryButton;
        [SerializeField] private SlotChangeButton skillButton;
        [SerializeField] private GameObject       inventoryUI;
        [SerializeField] private GameObject       skillUI;
        
        //-------------------------------------------------------------
        
        private bool _isDragging = false;

        [SerializeField] private List<ItemSlot>  itemSlots = new List<ItemSlot>();
        private Dictionary<ItemDataSO, int> _itemDatas = new();
        private List<ItemDataSO> _itemTable = new();
        private int _currentItemIndex = 0;
        
        public bool IsDragging { get => _isDragging; set => _isDragging = value; }
        public ItemDataSO QuickSlotItem { get; set; }
        public static PlayerInventory Inventory { get; set; }
        public static List<ItemDataSO> EquipmentDatas = new List<ItemDataSO>();
        public static bool IsAfterInit = false;
        
        [SerializeField] private PlayerInventory playerInv;

        private void OnEnable()
        {
            ChangeToInventory();
        }
        
        private void Start()
        {
            if (IsAfterInit == false)
            {
                Inventory = playerInv.Clone();
                Instance.InitializeSlots();
                IsAfterInit = true;
            }
            
            InitializeSlots();
        }

        public void InitializeSlots()
        {
            _currentItemIndex = 0;
                        
            Inventory.itemSlots = new List<ItemSlot>();
            
            for (int i = 0; i < itemSlots.Count; i++)
                Inventory.itemSlots.Add(itemSlots[i]);

            for (int i = 0; i < EquipmentDatas.Count; i++)
            {
                var slot = GetMatchTypeEquipSlot(EquipmentDatas[i].equipmentData.slotType);
                slot.SetItemData(EquipmentDatas[i]);
            }
            
            //인벤토리의 아이템 데이터를 슬롯에 넣어주기 (장비창 제외)
            for (int i = 0; i < Inventory.itemInventory.Count; i++)
            {
                ItemSlot matchSlot = GetMatchItemSlot(Inventory.itemInventory[i]);
                ItemSlot emptySlot = GetEmptySlot();

                ItemDataSO currentIndexItem = Inventory.itemInventory[i];
                
                //퀵슬롯 등록을 위한 item만 모아놓기
                if (currentIndexItem.itemType == ItemType.ITEM)
                {
                    if (_itemDatas.ContainsKey(currentIndexItem))
                    {
                        _itemDatas[currentIndexItem]++;
                        continue;
                    }
                    if (!_itemTable.Contains(currentIndexItem))
                        _itemTable.Add(currentIndexItem);
                    
                    _itemDatas.Add(currentIndexItem, 1);
                }

                if (matchSlot != null)
                {
                    matchSlot.SetItemData(Inventory.itemInventory[i]);
                    Inventory.itemInventory[i].ItemSlot = matchSlot;
                    continue;
                }
                
                emptySlot.SetItemData(Inventory.itemInventory[i]);
                Inventory.itemInventory[i].ItemSlot = emptySlot;
            }

            if (_itemTable.Count != 0)
            {
                QuickSlotItem = _itemTable[_currentItemIndex];
                UpdateQuickSlotUI(QuickSlotItem);
            }
            
            UpdateAllSlots();
        }

        private void Update()
        {
            //임시 퀵슬롯 키
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (QuickSlotItem == null)
                    return;
                
                QuickSlotItem.itemObject.ItemEffect(Player.Instance);
                
                //아이템 다 쓰면 넘어가기
                if (--_itemDatas[QuickSlotItem] <= 0)
                {
                    _itemDatas.Remove(QuickSlotItem);
                    _itemTable.Remove(QuickSlotItem);
                    Inventory.itemInventory.Remove(QuickSlotItem);
                    
                    ChangeQuickSlotItem();
                    UpdateAllSlots();
                }
                UpdateQuickSlotUI(QuickSlotItem);
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ChangeQuickSlotItem();
            }
        }

        private void ChangeQuickSlotItem()
        {
            if (_itemTable.Count == 0)
            {
                QuickSlotItem = null;
                UpdateQuickSlotUI(QuickSlotItem);
                return;
            }
                
            if (_currentItemIndex >= _itemTable.Count - 1)
                _currentItemIndex = 0;
            else
                _currentItemIndex++;
                
            QuickSlotItem = _itemTable[_currentItemIndex];
            UpdateQuickSlotUI(QuickSlotItem);
        }

        public void UpdateAllSlots()
        {
            for (int i = 0; i < itemSlots.Count; i++)
            {
                if (itemSlots[i].GetSlotItemData() == null
                    && itemSlots[i] is EquipmentSlot equipSlot)
                {
                    itemSlots[i].SetItemImage(equipSlot.GetInfoIcon);
                }
                //빈 슬롯이면 empty 이미지
                else if (itemSlots[i].GetSlotItemData() == null
                    && itemSlots[i] is not EquipmentSlot)
                {
                    itemSlots[i].SetItemImage(null);
                }
                else //아이템이 존재하면 itemImage 넣어주기
                {
                    Sprite itemIcon = itemSlots[i].GetSlotItemData().itemImage;
                    itemSlots[i].SetItemImage(itemIcon);
                }
            }
        }

        //아이템을 클릭했을 때 커서에 표시되는 UI
        public void UpdateInfoUI(ItemDataSO itemData)
        {
            SetInfoUI(itemData);
        }

        public void ChangeToInventory()
        {
            inventoryUI.SetActive(true);
            skillUI.SetActive(false);

            titleText.text = "인벤토리";
        }

        public void ChangeToSkill()
        {
            skillUI.SetActive(true);
            inventoryUI.SetActive(false);
            
            titleText.text = "스킬 슬롯";
        }

        private void SetInfoUI(ItemDataSO itemData)
        {
            itemIcon.sprite      = itemData ? itemData.itemImage : null;
            itemIcon.color       = itemData ? Color.white : Color.clear;
            itemName.text        = itemData ? itemData.itemName : String.Empty;
            itemDescription.text = itemData ? itemData.description : String.Empty;
            itemTypeInfo.text    = itemData ? itemData.itemType.ToString() : String.Empty;
        }

        public void AddItemToMatchSlot(ItemDataSO newItem)
        {
            if (AllSlotsFull())
            {
                Debug.Log("All inventory slots are full");
                return;
            }
            
            Inventory.itemInventory.Add(newItem);

            var matchSlot = GetMatchItemSlot(newItem);

            if (matchSlot)
            {
                matchSlot.SetItemData(newItem);
                newItem.ItemSlot = matchSlot;
            }
            else
                AddItemToEmptySlot(newItem);
            
            UpdateAllSlots();
        }

        public void AddItemToEmptySlot(ItemDataSO newItem)
        {
            var emptySlot = GetEmptySlot();
            emptySlot.SetItemData(newItem);
            newItem.ItemSlot = emptySlot;
            
            UpdateAllSlots();
        }

        private ItemSlot GetEmptySlot()
        {
            return itemSlots.FirstOrDefault(item => item.IsEmptySlot());
        }

        private ItemSlot GetMatchItemSlot(ItemDataSO item)
        {
            return itemSlots.FirstOrDefault(slot => slot.GetSlotItemData() == item);
        }

        public EquipmentSlot GetMatchTypeEquipSlot(EquipmentSlotType type)
        {
            EquipmentSlot matchSlot = equipSlots.FirstOrDefault(slot => slot.GetSlotType == type);
            
            if (matchSlot == null)
            {
                Debug.LogError($"Doesn't exist match type, typename: {type.ToString()}");
                return default;
            }

            if (matchSlot.IsEmptySlot())
                return matchSlot;

            //Original item need to go to the inventory
            ItemDataSO tempItemData = matchSlot.GetSlotItemData();
            
            var baseEquip = tempItemData.itemObject as BaseEquipment;
            baseEquip?.OffEquipment();
            
            EquipmentDatas.Remove(tempItemData);
            Inventory.currentEquipment.Remove(tempItemData.equipmentData);
            GetEmptySlot().SetItemData(tempItemData);

            return matchSlot;
        }
        
        public bool AllSlotsFull()
        {
            if (itemSlots.FirstOrDefault(item => item.IsEmptySlot()) == default)
            {
                return true;
            }
            return false;
        }

        public void UpdateQuickSlotUI(ItemDataSO itemData)
        {
            if (itemData == null)
            {
                quickSlotUI.SetIcon(null);
                return;
            }
            
            quickSlotUI.SetIcon(itemData.itemImage);
        }

        public int GetItemCount(ItemDataSO itemData)
        {
            Debug.Log(itemData);
            if (_itemDatas.ContainsKey(itemData))
            {
                return _itemDatas[itemData];
            }

            return -1;
        }
    }
}
