using Swift_Blade.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
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
        [Header("UI 부분")]
        [SerializeField] private QuickSlotUI         quickSlotUI;
        [SerializeField] private List<EquipmentSlot> equipSlots;
        [SerializeField] private TextMeshProUGUI     titleText;
        [SerializeField] private SlotChangeButton    inventoryButton;
        [SerializeField] private SlotChangeButton    skillButton;
        [SerializeField] private GameObject          inventoryUI;
        [SerializeField] private GameObject          skillUI;

        [Header("Item Information")]
        [SerializeField] private Image           itemIcon;
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private TextMeshProUGUI itemDescription;
        [SerializeField] private TextMeshProUGUI itemTypeInfo;

        private StringBuilder _sb = new StringBuilder();

        //-------------------------------------------------------------

        [SerializeField] private PlayerInventory playerInv;
        [SerializeField] private List<ItemSlot>  itemSlots  = new List<ItemSlot>();
        private Dictionary<ItemDataSO, int>      _itemDatas = new(); //How many item in there?
        private List<ItemDataSO>                 _itemTable = new(); //There is item?

        public event Action OnUseQuickSlotEvent;

        private int _currentItemIndex = 0;

        [Header("Default Item")]
        [SerializeField] private ItemDataSO defaultPotionItem;
        [SerializeField] private int defaultPotionCount = 3;

        public ItemDataSO QuickSlotItem { get; set; }
        public static PlayerInventory  Inventory { get; set; }
        public static List<ItemDataSO> EquipmentDatas = new List<ItemDataSO>();
        
        protected override void Awake()
        {
            base.Awake();

            if (Menu.IsNewGame == true)
            {
                Inventory = playerInv.Clone();

                UpdateAllSlots();

                ChangeToInventory();
                EquipmentDatas.Clear();
            }

            InputManager.ChangeQuickEvent += InputEventQuick;
            InputManager.UseQuickEvent    += InputEventUseQuick;
        }

        private void Start()
        {
            InitializeSlots();
        }

        protected override void OnDestroy()
        {
            InputManager.ChangeQuickEvent -= InputEventQuick;
            InputManager.UseQuickEvent -= InputEventUseQuick;

            base.OnDestroy();
        }
        private void InputEventQuick()
        {
            ChangeQuickSlotItem();
        }
        private void InputEventUseQuick()
        {
            UseQuickSlotItem();
        }
        public void InitializeSlots()
        {
            _currentItemIndex = 0;

            //인벤토리 초기화
            Inventory.itemSlots = new List<ItemSlot>();

            // 빈 인벤토리 슬롯 채워주기
            for (int i = 0; i < itemSlots.Count; i++)
            {
                Inventory.itemSlots.Add(itemSlots[i]);
            }

            // 빈 장비 슬롯 채워주기
            for (int i = 0; i < EquipmentDatas.Count; i++)
            {
                var slot = GetMatchTypeEquipSlot(EquipmentDatas[i].equipmentData.slotType);
                slot.SetItemData(EquipmentDatas[i]);

                (EquipmentDatas[i].itemObject as Equipment).OnEquipment(true);
            }

            //인벤토리의 아이템 데이터를 슬롯에 넣어주기 (장비창 제외)
            for (int i = 0; i < Inventory.itemInventory.Count; i++)
            {
                ItemSlot matchSlot = GetMatchItemSlot(Inventory.itemInventory[i]);
                ItemSlot emptySlot = GetEmptySlot();

                ItemDataSO currentIndexItem = Inventory.itemInventory[i];

                //퀵슬롯 등록을 위한 item만 모아놓기
                AddItemIfNotEquipment(currentIndexItem);

                AssignItemToSlot(i, matchSlot, emptySlot);
            }

            SetQuickSlotItem();
            UpdateAllSlots();
            OnUseQuickSlotEvent?.Invoke();

            static void AssignItemToSlot(int index, ItemSlot matchSlot, ItemSlot emptySlot)
            {
                if (matchSlot != null)
                {
                    matchSlot.SetItemData(Inventory.itemInventory[index]);
                    Inventory.itemInventory[index].ItemSlot = matchSlot;
                }
                else
                {
                    emptySlot.SetItemData(Inventory.itemInventory[index]);
                    Inventory.itemInventory[index].ItemSlot = emptySlot;
                }
            }
        }

        private void SetQuickSlotItem()
        {
            if (_itemTable.Count != 0)
            {
                QuickSlotItem = _itemTable[_currentItemIndex];

                UpdateQuickSlotUI(QuickSlotItem);
            }
            
            UpdateAllSlots();
        }
        private void UseQuickSlotItem()
        {
            if (QuickSlotItem == null)
                return;

            if (QuickSlotItem.itemObject.CanUse() == false)
                return;

            Inventory.itemInventory.Remove(QuickSlotItem);

            _itemDatas[QuickSlotItem]--;

            QuickSlotItem.itemObject.ItemEffect(Player.Instance);

            //아이템 다 쓰면 넘어가기
            if (_itemDatas[QuickSlotItem] <= 0)
            {
                _itemDatas.Remove(QuickSlotItem);
                _itemTable.Remove(QuickSlotItem);

                QuickSlotItem.ItemSlot.SetItemData(null);

                ChangeQuickSlotItem();
                UpdateAllSlots();
            }
            else
            {
                QuickSlotItem.ItemSlot.SetItemData(QuickSlotItem);
            }

            OnUseQuickSlotEvent?.Invoke();
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
                if (!itemSlots[i].GetSlotItemData()
                    && itemSlots[i] is EquipmentSlot equipSlot)
                {
                    if (equipSlot.GetSlotType == EquipmentSlotType.WEAPON)
                        continue;

                    itemSlots[i].SetItemUI(equipSlot.GetInfoIcon);
                }
                //빈 슬롯이면 empty 이미지
                else if (!itemSlots[i].GetSlotItemData()
                    && itemSlots[i] is not EquipmentSlot)
                {
                    itemSlots[i].SetItemUI(null);
                }
                else //아이템이 존재하면 itemImage 넣어주기
                {
                    Sprite itemIcon = itemSlots[i].GetSlotItemData().itemImage;
                    itemSlots[i].SetItemUI(itemIcon);
                }
            }

            OnUseQuickSlotEvent?.Invoke();
        }

        //아이템을 클릭했을 때 커서에 표시되는 UI
        public void UpdateItemInformationUI(ItemDataSO itemData)
        {
            SetEquipmentInfoUI(itemData);
        }

        public void UpdateItemInformationUI(WeaponSO weapon)
        {
            SetWeaponInfoUI(weapon);
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

        private void SetEquipmentInfoUI(ItemDataSO itemData)
        {
            itemIcon.sprite      = itemData ? itemData.itemImage : null;
            itemIcon.color       = itemData ? Color.white : Color.clear;
            itemName.text        = itemData ? itemData.itemName : string.Empty;
            itemDescription.text = itemData ? itemData.description : string.Empty;

            #region InformationUpdate
            if (itemData != null)
            {
                if (itemData.IsEquipment())
                {
                    _sb.Clear();

                    _sb.AppendLine(KoreanUtility.GetRarityColorText(itemData.equipmentData.rarity));

                    if(itemData.equipmentData.tags.Count == 0)
                    {
                        _sb.Append("<color=orange>태그 없음</color>");
                    }
                    else
                    {
                        foreach (var tag in itemData.equipmentData.tags)
                        {
                            _sb.Append($"<color=orange>[ {KoreanUtility.GetTagToKorean(tag)} ]</color>")
                                .Append(" ");
                        }
                    }

                    string equipInfo = _sb.ToString();
                    itemTypeInfo.fontSize = 24f;
                    itemTypeInfo.text = equipInfo;
                }
                else
                {
                    itemTypeInfo.fontSize = 42f;
                    itemTypeInfo.text = "아이템";
                }
            }
            else
            {
                itemTypeInfo.text = string.Empty;
            }
            #endregion
        }

        private void SetWeaponInfoUI(WeaponSO weapon)
        {
            itemIcon.sprite      = weapon ? (weapon as IPlayerEquipable).GetSprite : null;
            itemIcon.color       = weapon ? Color.white : Color.clear;
            itemName.text        = weapon ? weapon.name : string.Empty;
            itemDescription.text = weapon ? weapon.WeaponDescription : string.Empty;

            itemTypeInfo.fontSize = 42f;
            itemTypeInfo.text    = weapon ? "무기" : string.Empty;
        }

        public void AddItemToMatchSlot(ItemDataSO newItem)
        {
            if (IsAllSlotsFull())
            {
                Debug.Log("All inventory slots are full");
                return;
            }
            
            Inventory.itemInventory.Add(newItem);

            var matchSlot = GetMatchItemSlot(newItem);

            if (matchSlot)
            {
                matchSlot.SetItemData(newItem);
                //newItem.ItemSlot = matchSlot;
            }
            else
                TryAddItemToEmptySlot(newItem);
            
            UpdateAllSlots();
        }
        public void AddItemToEmptySlot(ItemDataSO itemDataSO)
        {
            if (itemDataSO == null) throw new ArgumentNullException($"{itemDataSO} is null");
            ItemSlot emptySlot = GetEmptySlot();
            emptySlot.SetItemData(itemDataSO);
            itemDataSO.ItemSlot = emptySlot;

            Inventory.itemInventory.Add(itemDataSO);

            UpdateAllSlots();
        }
        public bool TryAddItemToEmptySlot(ItemDataSO newItem)
        {
            if (newItem == null)
                return false;

            if (IsAllSlotsFull())
            {
                PopupManager.Instance.LogMessage("인벤토리가 가득 찼습니다.");
                return false;
            }

            var emptySlot = GetEmptySlot();

            AddItemIfNotEquipment(newItem);
            Inventory.itemInventory.Add(newItem);

            emptySlot.SetItemData(newItem);
            newItem.ItemSlot = emptySlot;

            SetQuickSlotItem();
            UpdateAllSlots();

            return true;
        }

        private void AddItemIfNotEquipment(ItemDataSO newItem)
        {
            if (newItem.itemType == ItemType.ITEM)
            {
                if (_itemTable.Contains(newItem))
                {
                    _itemDatas[newItem]++;
                }
                else
                {
                    _itemTable.Add(newItem);
                    _itemDatas.Add(newItem, 1);
                }
            }
        }

        private ItemSlot GetEmptySlot()
        {
            return itemSlots.FirstOrDefault(item => item.IsEmptySlot() && item is not EquipmentSlot);
        }

        private ItemSlot GetMatchItemSlot(ItemDataSO item)
        {
            return itemSlots.FirstOrDefault(slot => slot.GetSlotItemData() != null &&
                slot.GetSlotItemData().itemName == item.itemName);
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
            
            var baseEquip = tempItemData.itemObject as Equipment;
            baseEquip.OffEquipment();
            
            EquipmentDatas.Remove(tempItemData);
            Inventory.currentEquipment.Remove(tempItemData.equipmentData);
            GetEmptySlot().SetItemData(tempItemData);

            return matchSlot;
        }
        
        public bool IsAllSlotsFull()
        {
            if (GetEmptySlot() == null)
            {
                return true;
            }
            return false;
        }

        public void UpdateQuickSlotUI(ItemDataSO itemData)
        {
            if (!itemData)
            {
                quickSlotUI.SetIcon(null);
                return;
            }
            
            quickSlotUI.SetIcon(itemData.itemImage);
        }

        public int GetItemCount(ItemDataSO itemData)
        {
            if (_itemDatas == null)
                return -1;

            if (itemData == null)
                return -1;

            if (_itemDatas.TryGetValue(itemData, out var count))
                return count;

            return -1;
        }

        public void SetWeaponData(WeaponSO weapon)
        {
            EquipmentSlot weaponSlot = 
                equipSlots.FirstOrDefault(slot => slot.GetSlotType == EquipmentSlotType.WEAPON);

            Debug.Assert(weaponSlot != null, "Weapon slot is missing");

            weaponSlot.SetWeaponData(weapon);
        }
    }
}
