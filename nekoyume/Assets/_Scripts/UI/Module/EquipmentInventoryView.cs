﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nekoyume.Game.Controller;
using Nekoyume.Helper;
using Nekoyume.L10n;
using Nekoyume.Model.Item;
using Nekoyume.State;
using Nekoyume.UI.Model;
using Nekoyume.UI.Scroller;
using TMPro;
using UnityEngine;

namespace Nekoyume.UI.Module
{
    using UniRx;

    public class EquipmentInventoryView : MonoBehaviour
    {
        private enum Grade
        {
            All,
            Normal,
            Rare,
            Epic,
            Unique,
            Legend,
        }

        private enum Elemental
        {
            All,
            Normal,
            Fire,
            Water,
            Land,
            Wind,
        }

        [Serializable]
        private struct CategoryToggle
        {
            public Toggle Toggle;
            public ItemSubType Type;
        }

        [SerializeField]
        private EquipmentInventoryViewScroll scroll;

        [SerializeField]
        private List<CategoryToggle> categoryToggles = null;

        [SerializeField]
        private TMP_Dropdown gradeFilter = null;

        [SerializeField]
        private TMP_Dropdown elementalFilter = null;

        private readonly Dictionary<ItemSubType, List<EquipmentInventoryViewModel>>
            _equipments =
                new Dictionary<ItemSubType, List<EquipmentInventoryViewModel>>();


        private readonly ReactiveProperty<ItemSubType> _selectedItemSubType =
            new ReactiveProperty<ItemSubType>(ItemSubType.Weapon);

        private readonly ReactiveProperty<Grade> _grade =
            new ReactiveProperty<Grade>(Grade.All);

        private readonly ReactiveProperty<Elemental> _elemental =
            new ReactiveProperty<Elemental>(Elemental.All);

        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        private EquipmentInventoryViewModel _selectedModel;
        private EquipmentInventoryViewModel _baseModel;
        private EquipmentInventoryViewModel _materialModel;

        private Action<EquipmentInventoryViewModel, RectTransform> _onSelectItem;

        private Action<EquipmentInventoryViewModel, EquipmentInventoryViewModel> _onUpdateView;

        private void Awake()
        {
            foreach (var categoryToggle in categoryToggles)
            {
                categoryToggle.Toggle.onValueChanged.AddListener(value =>
                {
                    if (!value) return;
                    AudioController.PlayClick();
                    _selectedItemSubType.Value = categoryToggle.Type;
                });
            }

            gradeFilter.AddOptions(new[]
                {
                    Grade.All,
                    Grade.Normal,
                    Grade.Rare,
                    Grade.Epic,
                    Grade.Unique,
                    Grade.Legend,
                }
                .Select(type => type.ToString())
                .ToList());

            gradeFilter.onValueChanged.AsObservable()
                .Select(index => (Grade)index)
                .Subscribe(filter => _grade.Value = filter)
                .AddTo(gameObject);

            elementalFilter.AddOptions(new[]
                {
                    Elemental.All,
                    Elemental.Normal,
                    Elemental.Fire,
                    Elemental.Water,
                    Elemental.Land,
                    Elemental.Wind,
                }
                .Select(type => type.ToString())
                .ToList());

            elementalFilter.onValueChanged.AsObservable()
                .Select(index => (Elemental)index)
                .Subscribe(filter => _elemental.Value = filter)
                .AddTo(gameObject);

            _grade.Subscribe(_ => UpdateView(true)).AddTo(gameObject);
            _elemental.Subscribe(_ => UpdateView(true)).AddTo(gameObject);
            _selectedItemSubType.Subscribe(_ => UpdateView(true)).AddTo(gameObject);
        }

        public (Equipment, Equipment) GetSelectedModels()
        {
            var baseItem = (Equipment)_baseModel?.ItemBase;
            var materialItem = (Equipment)_materialModel?.ItemBase;
            return (baseItem, materialItem);
        }

        public string GetSubmitText()
        {
            return L10nManager.Localize(_baseModel is null
                ? "UI_COMBINATION_REGISTER_ITEM"
                : "UI_COMBINATION_REGISTER_MATERIAL");
        }

        public void ClearSelectedItem()
        {
            _selectedModel?.Selected.SetValueAndForceNotify(false);
            _selectedModel = null;
        }

        public void SelectItem()
        {
            if (_baseModel is null)
            {
                _baseModel = _selectedModel;
                _baseModel.SelectedBase.SetValueAndForceNotify(true);
            }
            else
            {
                _materialModel?.SelectedMaterial.SetValueAndForceNotify(false);
                _materialModel = _selectedModel;
                _materialModel.SelectedMaterial.SetValueAndForceNotify(true);
            }

            UpdateView();
        }

        public void DeselectItem(bool isAll = false)
        {
            if (isAll)
            {
                _baseModel?.SelectedBase.SetValueAndForceNotify(false);
                _baseModel = null;
            }

            _materialModel?.SelectedMaterial.SetValueAndForceNotify(false);
            _materialModel = null;

            UpdateView();
        }

        private void OnClickItem(EquipmentInventoryViewModel item)
        {
            if (item.Equals(_baseModel)) // 둘다 해제
            {
                _baseModel.SelectedBase.SetValueAndForceNotify(false);
                _baseModel = null;
                _materialModel?.SelectedMaterial.SetValueAndForceNotify(false);
                _materialModel = null;
            }
            else if (item.Equals(_materialModel)) // 재료 해제
            {
                _materialModel.SelectedMaterial.SetValueAndForceNotify(false);
                _materialModel = null;
            }
            else
            {
                ClearSelectedItem();
                _selectedModel = item;
                _selectedModel.Selected.SetValueAndForceNotify(true);
                _onSelectItem?.Invoke(_selectedModel, _selectedModel.View); // Show tooltip popup
            }

            UpdateView();
        }

        private void DisableItem(IEnumerable<EquipmentInventoryViewModel> items)
        {
            if (_baseModel is null)
            {
                foreach (var model in items)
                {
                    model.Disabled.Value = false;
                }
            }
            else
            {
                foreach (var model in items)
                {
                    model.Disabled.Value = IsDisable(_baseModel, model);
                }
            }
        }

        private bool IsDisable(EquipmentInventoryViewModel a, EquipmentInventoryViewModel b)
        {
            if (a.ItemBase.ItemSubType != b.ItemBase.ItemSubType)
            {
                return true;
            }

            if (a.ItemBase.Grade != b.ItemBase.Grade)
            {
                return true;
            }

            var ae = (Equipment)a.ItemBase;
            var be = (Equipment)b.ItemBase;
            return ae.level != be.level;
        }

        private void UpdateView(bool jumpToFirst = false)
        {
            var models = GetSortedModels();
            DisableItem(models);
            _onUpdateView?.Invoke(_baseModel, _materialModel);
            scroll.UpdateData(models, jumpToFirst);
        }

        private IEnumerable<EquipmentInventoryViewModel> GetSortedModels()
        {
            if (!_equipments.ContainsKey(_selectedItemSubType.Value))
            {
                return new List<EquipmentInventoryViewModel>();
            }

            var result = _equipments[_selectedItemSubType.Value].ToList();
            if (_grade.Value != Grade.All)
            {
                var value = (int)(_grade.Value);
                result = result.Where(x => x.ItemBase.Grade == value).ToList();
            }

            if (_elemental.Value != Elemental.All)
            {
                var value = (int)_elemental.Value - 1;
                result = result.Where(x => (int)x.ItemBase.ElementalType == value).ToList();
            }

            return result;
        }

        public void Set(Action<EquipmentInventoryViewModel, RectTransform> onSelectItem,
            Action<EquipmentInventoryViewModel, EquipmentInventoryViewModel> onUpdateView)
        {
            _onSelectItem = onSelectItem;
            _onUpdateView = onUpdateView;

            _disposables.DisposeAllAndClear();
            ReactiveAvatarState.Inventory.Subscribe(inventory =>
            {
                _equipments.Clear();

                if (inventory is null)
                {
                    return;
                }

                _selectedModel = null;
                foreach (var item in inventory.Items)
                {
                    if (!(item.item is Equipment) || item.Locked)
                    {
                        continue;
                    }

                    AddItem(item.item);
                }

                UpdateView(false);
            }).AddTo(_disposables);

            scroll.OnClick.Subscribe(OnClickItem).AddTo(_disposables);
        }

        private void AddItem(ItemBase itemBase)
        {
            var equipment = (Equipment)itemBase;
            if (itemBase is ITradableItem tradableItem)
            {
                var blockIndex = Game.Game.instance.Agent?.BlockIndex ?? -1;
                if (tradableItem.RequiredBlockIndex > blockIndex)
                {
                    return;
                }
            }

            var inventoryItem = new EquipmentInventoryViewModel(itemBase,
                equipped: equipment.equipped,
                levelLimited: !Util.IsUsableItem(itemBase.Id));

            if (!_equipments.ContainsKey(inventoryItem.ItemBase.ItemSubType))
            {
                _equipments.Add(inventoryItem.ItemBase.ItemSubType,
                    new List<EquipmentInventoryViewModel>());
            }

            _equipments[inventoryItem.ItemBase.ItemSubType].Add(inventoryItem);
        }
    }
}
