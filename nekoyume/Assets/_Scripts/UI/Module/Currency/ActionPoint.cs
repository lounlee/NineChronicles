using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nekoyume.Game;
using Nekoyume.Helper;
using Nekoyume.L10n;
using Nekoyume.Model.Item;
using Nekoyume.Model.Mail;
using Nekoyume.State;
using Nekoyume.State.Subjects;
using Nekoyume.UI.Module.Common;
using Nekoyume.UI.Scroller;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nekoyume.UI.Module
{
    using UniRx;

    public class ActionPoint : AlphaAnimateModule
    {
        [Serializable]
        private struct DailyBonus
        {
            public GameObject container;
            public SliderAnimator sliderAnimator;
            public TextMeshProUGUI blockText;
        }

        [SerializeField]
        private SliderAnimator sliderAnimator = null;

        [SerializeField]
        private TextMeshProUGUI text = null;

        [SerializeField]
        private Image image = null;

        [SerializeField]
        private DailyBonus dailyBonus;

        [SerializeField]
        private bool syncWithAvatarState = true;

        [SerializeField]
        private EventTrigger eventTrigger = null;

        [SerializeField]
        private GameObject loading;

        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private int _currentActionPoint;
        private long _currentBlockIndex;
        private long _rewardReceivedBlockIndex;

        public bool IsRemained => _currentActionPoint > 0;

        public Image IconImage => image;

        public bool NowCharging => loading.activeSelf;

        #region Mono

        private void Awake()
        {
            sliderAnimator.OnSliderChange
                .Subscribe(_ => OnSliderChange())
                .AddTo(gameObject);
            sliderAnimator.SetValue(0f, false);
            dailyBonus.sliderAnimator.SetValue(0f, false);

            GameConfigStateSubject.GameConfigState
                .ObserveOnMainThread()
                .Subscribe(state =>
                {
                    sliderAnimator.SetMaxValue(state.ActionPointMax);
                    dailyBonus.sliderAnimator.SetMaxValue(state.DailyRewardInterval);
                })
                .AddTo(gameObject);

            GameConfigStateSubject.ActionPointState.ObserveAdd()
                .Where(x => x.Key == States.Instance.CurrentAvatarState.address)
                .Subscribe(x => Charger(true))
                .AddTo(gameObject);

            GameConfigStateSubject.ActionPointState.ObserveRemove()
                .Where(x => x.Key == States.Instance.CurrentAvatarState.address)
                .Subscribe(x => Charger(false)).AddTo(gameObject);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!syncWithAvatarState)
                return;

            var gameConfig = States.Instance.GameConfigState;
            if (gameConfig is not null)
            {
                sliderAnimator.SetMaxValue(gameConfig.ActionPointMax);
                dailyBonus.sliderAnimator.SetMaxValue(gameConfig.DailyRewardInterval);
            }

            var avatarState = States.Instance.CurrentAvatarState;
            if (avatarState is not null)
            {
                SetActionPoint(avatarState.actionPoint, false);
                SetBlockIndex(Game.Game.instance.Agent.BlockIndex, false);
                SetRewardReceivedBlockIndex(avatarState.dailyRewardReceivedIndex, false);
            }

            ReactiveAvatarState.ActionPoint
                .Subscribe(x => SetActionPoint(x, true))
                .AddTo(_disposables);
            Game.Game.instance.Agent.BlockIndexSubject.ObserveOnMainThread()
                .Subscribe(x => SetBlockIndex(x, true))
                .AddTo(_disposables);
            ReactiveAvatarState.DailyRewardReceivedIndex
                .Subscribe(x => SetRewardReceivedBlockIndex(x, true))
                .AddTo(_disposables);

            OnSliderChange();

            if (States.Instance.CurrentAvatarState is null)
            {
                Charger(false);
            }
            else
            {
                var address = States.Instance.CurrentAvatarState.address;
                if (GameConfigStateSubject.ActionPointState.ContainsKey(address))
                {
                    var value = GameConfigStateSubject.ActionPointState[address];
                    Charger(value);
                }
                else
                {
                    Charger(false);
                }
            }
        }

        protected override void OnDisable()
        {
            sliderAnimator.Stop();
            dailyBonus.sliderAnimator.Stop();
            _disposables.DisposeAllAndClear();
            base.OnDisable();
        }

        #endregion

        private void SetActionPoint(int actionPoint, bool useAnimation)
        {
            if (_currentActionPoint == actionPoint)
            {
                return;
            }

            _currentActionPoint = actionPoint;
            sliderAnimator.SetValue(_currentActionPoint, useAnimation);
        }

        private void SetBlockIndex(long blockIndex, bool useAnimation)
        {
            if (_currentBlockIndex == blockIndex)
            {
                return;
            }

            _currentBlockIndex = blockIndex;
            UpdateDailyBonusSlider(useAnimation);
        }

        private void SetRewardReceivedBlockIndex(long rewardReceivedBlockIndex, bool useAnimation)
        {
            if (_rewardReceivedBlockIndex == rewardReceivedBlockIndex)
            {
                return;
            }

            _rewardReceivedBlockIndex = rewardReceivedBlockIndex;
            UpdateDailyBonusSlider(useAnimation);
        }

        private void UpdateDailyBonusSlider(bool useAnimation)
        {
            var gameConfigState = States.Instance.GameConfigState;
            var endValue = Math.Max(0, _currentBlockIndex - _rewardReceivedBlockIndex);
            var value = Math.Min(gameConfigState.DailyRewardInterval, endValue);
            var remainBlock = gameConfigState.DailyRewardInterval - value;

            dailyBonus.sliderAnimator.SetValue(value, useAnimation);
            var timeSpanString =
                remainBlock > 0 ? remainBlock.BlockRangeToTimeSpanString() : string.Empty;
            dailyBonus.blockText.text =  $"{remainBlock:#,0}({timeSpanString})";
        }

        private void OnSliderChange()
        {
            var current = ((int)sliderAnimator.Value).ToString("N0", CultureInfo.CurrentCulture);
            var max = ((int)sliderAnimator.MaxValue).ToString("N0", CultureInfo.CurrentCulture);
            text.text = $"{current}/{max}";
        }

        public void SetActionPoint(int actionPoint)
        {
            SetActionPoint(actionPoint, false);
        }

        public void SetEventTriggerEnabled(bool value)
        {
            eventTrigger.enabled = value;
        }

        private void Charger(bool isCharging)
        {
            loading.SetActive(isCharging);
            text.enabled = !isCharging;
            dailyBonus.container.SetActive(!isCharging);
        }

        // Call at Event Trigger Component
        public void OnClickSlider()
        {
            var popup = Widget.Find<MaterialNavigationPopup>();

            var blockIndex = Game.Game.instance.Agent.BlockIndex;
            var apStoneCount = Game.Game.instance.States.CurrentAvatarState.inventory.Items
                .Where(x =>
                    x.item.ItemSubType == ItemSubType.ApStone &&
                    !x.Locked &&
                    !(x.item is ITradableItem tradableItem &&
                      tradableItem.RequiredBlockIndex > blockIndex))
                .Sum(item => item.count);

            var itemCountText = $"{sliderAnimator.Value}/{sliderAnimator.MaxValue}";
            var blockRange = (long)dailyBonus.sliderAnimator.Value;
            var maxBlockRange = (long)dailyBonus.sliderAnimator.MaxValue;
            var isInteractable = IsInteractableMaterial(); // 이 경우 버튼 자체를 비활성화

            popup.ShowAP(
                itemCountText,
                apStoneCount,
                blockRange,
                maxBlockRange,
                isInteractable,
                () => InvokeAfterActionPointCheck(ChargeAP),
                () => InvokeAfterActionPointCheck(GetDailyReward));
        }

        private void InvokeAfterActionPointCheck(System.Action action)
        {
            if (States.Instance.CurrentAvatarState.actionPoint > 0)
            {
                ShowRefillConfirmPopup(action);
            }
            else
            {
                action();
            }
        }

        private void ChargeAP()
        {
            // 한 줄 팝업?

            var apStoneRow = Game.Game.instance.TableSheets.MaterialItemSheet.Values
                .First(r => r.ItemSubType == ItemSubType.ApStone);
            var apStone = new Nekoyume.Model.Item.Material(apStoneRow);
            Game.Game.instance.ActionManager.ChargeActionPoint(apStone).Subscribe();
        }

        private void GetDailyReward()
        {
            NotificationSystem.Push(
                MailType.System,
                L10nManager.Localize("UI_RECEIVING_DAILY_REWARD"),
                NotificationCell.NotificationType.Information);

            Game.Game.instance.ActionManager.DailyReward().Subscribe();

            var address = States.Instance.CurrentAvatarState.address;
            if (GameConfigStateSubject.ActionPointState.ContainsKey(address))
            {
                GameConfigStateSubject.ActionPointState.Remove(address);
            }
            GameConfigStateSubject.ActionPointState.Add(address, true);
        }

        public static bool IsInteractableMaterial()
        {
            if (Widget.Find<HeaderMenuStatic>().ActionPoint.NowCharging) // is charging?
            {
                return false;
            }

            if (States.Instance.CurrentAvatarState.actionPoint ==
                States.Instance.GameConfigState.ActionPointMax) // full?
            {
                return false;
            }

            return !Game.Game.instance.IsInWorld;
        }

        public static void ShowRefillConfirmPopup(System.Action confirmCallback)
        {
            var confirm = Widget.Find<IconAndButtonSystem>();
            confirm.ShowWithTwoButton("UI_CONFIRM", "UI_AP_REFILL_CONFIRM_CONTENT",
                "UI_OK", "UI_CANCEL",
                true, IconAndButtonSystem.SystemType.Information);
            confirm.ConfirmCallback = confirmCallback;
            confirm.CancelCallback = () => confirm.Close();
        }
    }
}
