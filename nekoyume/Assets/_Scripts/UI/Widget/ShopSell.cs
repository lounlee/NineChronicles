using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Libplanet.Assets;
using mixpanel;
using Nekoyume.Action;
using Nekoyume.Game.Controller;
using Nekoyume.Helper;
using Nekoyume.L10n;
using Nekoyume.Model.Item;
using Nekoyume.Model.Mail;
using Nekoyume.Model.Market;
using Nekoyume.State;
using Nekoyume.UI.Model;
using UnityEngine;
using UnityEngine.UI;
using Inventory = Nekoyume.UI.Module.Inventory;
using ShopItem = Nekoyume.UI.Model.ShopItem;

namespace Nekoyume.UI
{
    using Nekoyume.UI.Scroller;
    using UniRx;

    public class ShopSell : Widget
    {
        private enum PriorityType
        {
            Price,
            Count,
        }

        [SerializeField]
        private Inventory inventory;

        [SerializeField]
        private SellView view;

        [SerializeField]
        private SpeechBubble speechBubble = null;

        [SerializeField]
        private Button reregistrationButton;

        [SerializeField]
        private Button buyButton = null;

        [SerializeField]
        private Button closeButton = null;

        private const int LimitPrice = 100000000;

        private Shop SharedModel { get; set; }

        protected override void Awake()
        {
            base.Awake();
            SharedModel = new Shop();
            CloseWidget = null;

            reregistrationButton.onClick.AddListener(() =>
            {
                Find<TwoButtonSystem>().Show(
                    L10nManager.Localize("UI_SHOP_UPDATESELLALL_POPUP"),
                    L10nManager.Localize("UI_YES"),
                    L10nManager.Localize("UI_NO"),
                    SubscribeUpdateSellPopupSubmit);
            });

            buyButton.onClick.AddListener(() =>
            {
                speechBubble.gameObject.SetActive(false);
                Find<TwoButtonSystem>().Close();
                Find<ItemCountableAndPricePopup>().Close();
                Find<ShopBuy>().gameObject.SetActive(true);
                Find<ShopBuy>().Open();
                gameObject.SetActive(false);
            });

            closeButton.onClick.AddListener(() =>
            {
                Close(true);
                Game.Event.OnRoomEnter.Invoke(true);
            });

            CloseWidget = () =>
            {
                Close(true);
                Game.Event.OnRoomEnter.Invoke(true);
            };
        }

        public override void Initialize()
        {
            base.Initialize();

            SharedModel.ItemCountableAndPricePopup.Value.Item
                .Subscribe(SubscribeSellPopup)
                .AddTo(gameObject);
            SharedModel.ItemCountableAndPricePopup.Value.OnClickSubmit
                .Subscribe(SubscribeSellPopupSubmit)
                .AddTo(gameObject);
            SharedModel.ItemCountableAndPricePopup.Value.OnClickReregister
                .Subscribe(SubscribeSellPopupUpdateSell)
                .AddTo(gameObject);
            SharedModel.ItemCountableAndPricePopup.Value.OnClickCancel
                .Subscribe(SubscribeSellPopupCancel)
                .AddTo(gameObject);
            SharedModel.ItemCountableAndPricePopup.Value.OnChangeCount
                .Subscribe(SubscribeSellPopupCount)
                .AddTo(gameObject);
            SharedModel.ItemCountableAndPricePopup.Value.OnChangePrice
                .Subscribe(SubscribeSellPopupPrice)
                .AddTo(gameObject);

            // sell cancellation
            SharedModel.ItemCountAndPricePopup.Value.Item
                .Subscribe(SubscribeSellCancellationPopup)
                .AddTo(gameObject);
        }

        private void ShowFavTooltip(InventoryItem model)
        {
            var tooltip = ItemTooltip.Find(model.ItemBase.ItemType);
            tooltip.Show(
                model,
                L10nManager.Localize("UI_SELL"),
                model.ItemBase is ITradableItem,
                () => ShowSell(model),
                inventory.ClearSelectedItem,
                () => L10nManager.Localize("UI_UNTRADABLE"));
        }

        private void ShowItemTooltip(InventoryItem model)
        {
            if (model.ItemBase is not null)
            {
                var tooltip = ItemTooltip.Find(model.ItemBase.ItemType);
                tooltip.Show(
                    model,
                    L10nManager.Localize("UI_SELL"),
                    model.ItemBase is ITradableItem,
                    () => ShowSell(model),
                    inventory.ClearSelectedItem,
                    () => L10nManager.Localize("UI_UNTRADABLE"));
            }
            else
            {
                Find<FungibleAssetTooltip>().Show(model,
                    () => ShowSell(model),
                    view.ClearSelectedItem);
            }
        }

        private void ShowSellTooltip(ShopItem model)
        {
            if (model.ItemBase is not null)
            {
                var tooltip = ItemTooltip.Find(model.ItemBase.ItemType);
                tooltip.Show(model,
                    () => ShowUpdateSellPopup(model),
                    () => ShowRetrievePopup(model),
                    view.ClearSelectedItem);
            }
            else
            {
                Find<FungibleAssetTooltip>().Show(model,
                    () => ShowUpdateSellPopup(model),
                    () => ShowRetrievePopup(model),
                    view.ClearSelectedItem);
            }
        }

        public override void Show(bool ignoreShowAnimation = false)
        {
            ShowAsync(ignoreShowAnimation);
        }

        private async void ShowAsync(bool ignoreShowAnimation = false)
        {
            base.Show(ignoreShowAnimation);
            AudioController.instance.PlayMusic(AudioController.MusicCode.Shop);
            UpdateSpeechBubble();
            inventory.SetShop(ShowItemTooltip);
            await ReactiveShopState.RequestSellProductsAsync();
            view.Show(
                ReactiveShopState.SellItemProducts,
                ReactiveShopState.SellFungibleAssetProducts,
                ShowSellTooltip, false);
        }

        private void UpdateSpeechBubble()
        {
            speechBubble.gameObject.SetActive(true);
            speechBubble.SetKey("SPEECH_SHOP_GREETING_");
            StartCoroutine(speechBubble.CoShowText(true));
        }

        public override void Close(bool ignoreCloseAnimation = false)
        {
            Find<TwoButtonSystem>().Close();
            Find<ItemCountableAndPricePopup>().Close();
            speechBubble.gameObject.SetActive(false);
            Find<ShopBuy>().Close();
            base.Close(ignoreCloseAnimation);
        }

        public void Close(bool ignoreOnRoomEnter, bool ignoreCloseAnimation)
        {
            Find<ItemCountAndPricePopup>().Close(ignoreCloseAnimation);
            if (!ignoreOnRoomEnter)
            {
                Game.Event.OnRoomEnter.Invoke(true);
            }

            base.Close(ignoreCloseAnimation);
        }

        private void ShowSell(InventoryItem model)
        {
            if (model is null)
            {
                return;
            }

            var data = SharedModel.ItemCountableAndPricePopup.Value;
            var currency = States.Instance.GoldBalanceState.Gold.Currency;
            data.Price.Value = new FungibleAssetValue(currency, Shop.MinimumPrice, 0);
            data.UnitPrice.Value = new FungibleAssetValue(currency, Shop.MinimumPrice, 0);
            data.Count.Value = 1;
            data.IsSell.Value = true;
            data.InfoText.Value = string.Empty;
            data.CountEnabled.Value = true;

            if (model.ItemBase is not null)
            {
                data.TitleText.Value = model.ItemBase.GetLocalizedName();
                data.Submittable.Value = !DimmedFuncForSell(model.ItemBase);
                data.Item.Value = new CountEditableItem(model.ItemBase,
                    1,
                    1,
                    model.Count.Value);
            }
            else
            {
                data.TitleText.Value = model.FungibleAssetValue.GetLocalizedName();
                data.Submittable.Value = true;
                data.Item.Value = new CountEditableItem(model.FungibleAssetValue,
                    1,
                    1,
                    model.Count.Value);
            }

            data.Item.Value.CountEnabled.Value = false;
        }

        private void ShowUpdateSellPopup(ShopItem model) // 판매 갱신
        {
            var data = SharedModel.ItemCountableAndPricePopup.Value;
            var price = model.Product.Price;
            var unitPrice = price / model.Product.Quantity;
            var majorUnit = (int)unitPrice;
            var minorUnit = (int)((unitPrice - majorUnit) * 100);
            var currency = States.Instance.GoldBalanceState.Gold.Currency;
            data.UnitPrice.Value = new FungibleAssetValue(currency, majorUnit, minorUnit);

            // data.ProductId.value = model.Product
            data.PrePrice.Value = (BigInteger)model.Product.Price * currency;
            data.Price.Value = (BigInteger)model.Product.Price * currency;
            var itemCount = (int)model.Product.Quantity;
            data.Count.Value = itemCount;
            data.IsSell.Value = false;

            data.TitleText.Value = model.ItemBase.GetLocalizedName();
            data.InfoText.Value = string.Empty;
            data.CountEnabled.Value = true;
            data.Submittable.Value = !DimmedFuncForSell(model.ItemBase);
            data.Item.Value = new CountEditableItem(model.ItemBase,
                itemCount,
                itemCount,
                itemCount);
            data.Item.Value.CountEnabled.Value = false;
        }

        private void SubscribeUpdateSellPopupSubmit()
        {
            var products = ReactiveShopState.SellItemProducts.Value;
            var blockIndex = Game.Game.instance.Agent.BlockIndex;
            var itemProducts = products.ToList();

            if (!itemProducts.Any())
            {
                OneLineSystem.Push(
                    MailType.System,
                    L10nManager.Localize("UI_SHOP_NONEUPDATESELLALL"),
                    NotificationCell.NotificationType.Alert);

                return;
            }

            view.SetLoading(itemProducts);

            var updateSellInfos = new List<UpdateSellInfo>();
            var oneLineSystemInfos = new List<(string name, int count)>();
            foreach (var product in itemProducts)
            {
                if (!ReactiveShopState.TryGetSellShopItem(product.ProductId, out var itemBase))
                {
                    return;
                }

                var updateSellInfo = new UpdateSellInfo(
                    product.ProductId,
                    Guid.NewGuid(),
                    product.TradableId,
                    itemBase.ItemSubType,
                    (BigInteger)product.Price * States.Instance.GoldBalanceState.Gold.Currency,
                    (int)product.Quantity
                );

                updateSellInfos.Add(updateSellInfo);
                oneLineSystemInfos.Add((itemBase.GetLocalizedName(), (int)product.Quantity));
            }

            Game.Game.instance.ActionManager.UpdateSell(updateSellInfos).Subscribe();
            Analyzer.Instance.Track("Unity/UpdateSellAll", new Dictionary<string, Value>()
            {
                ["Quantity"] = updateSellInfos.Count,
                ["AvatarAddress"] = States.Instance.CurrentAvatarState.address.ToString(),
                ["AgentAddress"] = States.Instance.AgentState.address.ToString(),
            });

            string message;
            if (updateSellInfos.Count() > 1)
            {
                message = L10nManager.Localize("NOTIFICATION_REREGISTER_ALL_START");
            }
            else
            {
                var info = oneLineSystemInfos.FirstOrDefault();
                if (info.count > 1)
                {
                    message = string.Format(
                        L10nManager.Localize("NOTIFICATION_MULTIPLE_SELL_START"),
                        info.name, info.count);
                }
                else
                {
                    message = string.Format(L10nManager.Localize("NOTIFICATION_SELL_START"),
                        info.name);
                }
            }

            OneLineSystem.Push(MailType.Auction, message,
                NotificationCell.NotificationType.Information);
            AudioController.instance.PlaySfx(AudioController.SfxCode.InputItem);
        }

        private void ShowRetrievePopup(ShopItem model) // 판매 취소
        {
            var productId = model.Product?.ProductId ?? model.FungibleAssetProduct.ProductId;
            var price = model.Product?.Price ?? model.FungibleAssetProduct.Price;
            var quantity = model.Product?.Quantity ?? model.FungibleAssetProduct.Quantity;

            SharedModel.ItemCountAndPricePopup.Value.TitleText.Value =
                L10nManager.Localize("UI_RETRIEVE");
            SharedModel.ItemCountAndPricePopup.Value.InfoText.Value =
                L10nManager.Localize("UI_RETRIEVE_INFO");
            SharedModel.ItemCountAndPricePopup.Value.CountEnabled.Value = true;
            SharedModel.ItemCountAndPricePopup.Value.ProductId.Value = productId;
            SharedModel.ItemCountAndPricePopup.Value.Price.Value = (BigInteger)price *
                States.Instance.GoldBalanceState.Gold.Currency;
            SharedModel.ItemCountAndPricePopup.Value.PriceInteractable.Value = false;
            var itemCount = (int)quantity;
            if (model.Product is null)
            {
                SharedModel.ItemCountAndPricePopup.Value.Item.Value = new CountEditableItem(
                    model.FungibleAssetValue,
                    itemCount,
                    itemCount,
                    itemCount);
            }
            else
            {
                SharedModel.ItemCountAndPricePopup.Value.Item.Value = new CountEditableItem(
                    model.ItemBase,
                    itemCount,
                    itemCount,
                    itemCount);
            }
        }

        // sell
        private void SubscribeSellPopup(CountableItem data)
        {
            if (data is null)
            {
                Find<ItemCountableAndPricePopup>().Close();
                return;
            }

            Find<ItemCountableAndPricePopup>().Show(SharedModel.ItemCountableAndPricePopup.Value,
                SharedModel.ItemCountableAndPricePopup.Value.IsSell.Value);
        }

        private void SubscribeSellPopupSubmit(Model.ItemCountableAndPricePopup data)
        {
            if (data.Price.Value.MinorUnit > 0)
            {
                OneLineSystem.Push(
                    MailType.System,
                    L10nManager.Localize("UI_TOTAL_PRICE_WARNING"),
                    NotificationCell.NotificationType.Alert);
                return;
            }

            if (data.Price.Value.Sign * data.Price.Value.MajorUnit < Shop.MinimumPrice)
            {
                throw new InvalidSellingPriceException(data);
            }


            if (data.Item.Value.ItemBase.Value is not null)
            {
                var itemBase = data.Item.Value.ItemBase.Value;
                if (itemBase is not ITradableItem tradableItem)
                {
                    return;
                }

                var count = data.Count.Value;
                var itemSubType = itemBase.ItemSubType;
                var avatarAddress = States.Instance.CurrentAvatarState.address;

                var info = new RegisterInfo
                {
                    AvatarAddress = avatarAddress,
                    Price = data.Price.Value,
                    TradableId = tradableItem.TradableId,
                    ItemCount = count,
                    Type = itemSubType is ItemSubType.Hourglass or ItemSubType.ApStone
                        ? ProductType.Fungible
                        : ProductType.NonFungible
                };

                Game.Game.instance.ActionManager.RegisterProduct(info).Subscribe();
                if (tradableItem is not TradableMaterial)
                {
                    LocalLayerModifier.RemoveItem(avatarAddress, tradableItem.TradableId,
                        tradableItem.RequiredBlockIndex,
                        count);
                }

                LocalLayerModifier.SetItemEquip(avatarAddress, tradableItem.TradableId, false);
                PostRegisterProduct(itemBase.GetLocalizedName());
            }
            else
            {
                var count = data.Count.Value;
                var avatarAddress = States.Instance.CurrentAvatarState.address;
                var currency = data.Item.Value.FungibleAssetValue.Value.Currency;
                var fungibleAsset = new FungibleAssetValue(currency, count, 0);
                var info = new AssetInfo
                {
                    AvatarAddress = avatarAddress,
                    Price = data.Price.Value,
                    Asset = fungibleAsset,
                    Type = ProductType.FungibleAssetValue
                };

                // todo : 여기서 줄여주던지 액션에서 줄여줘야함
                Game.Game.instance.ActionManager.RegisterProduct(info).Subscribe();
                States.Instance.SetBalance(fungibleAsset);
                inventory.UpdateFungibleAssets();
                PostRegisterProduct(fungibleAsset.GetLocalizedName());
            }
        }

        private void SubscribeSellPopupUpdateSell(Model.ItemCountableAndPricePopup data)
        {
            if (!(data.Item.Value.ItemBase.Value is ITradableItem tradableItem))
            {
                return;
            }

            if (data.Price.Value.MinorUnit > 0)
            {
                OneLineSystem.Push(
                    MailType.System,
                    L10nManager.Localize("UI_TOTAL_PRICE_WARNING"),
                    NotificationCell.NotificationType.Alert);
                return;
            }

            if (data.Price.Value.Sign * data.Price.Value.MajorUnit < Shop.MinimumPrice)
            {
                throw new InvalidSellingPriceException(data);
            }

            var requiredBlockIndex = tradableItem.RequiredBlockIndex;
            var totalPrice = data.Price.Value;
            var preTotalPrice = data.PrePrice.Value;
            var count = data.Count.Value;
            var product =
                ReactiveShopState.GetSellItemProduct(tradableItem.TradableId, requiredBlockIndex,
                    preTotalPrice, count);
            if (product == null)
            {
                return;
            }

            var itemSubType = data.Item.Value.ItemBase.Value.ItemSubType;
            var updateSellInfo = new UpdateSellInfo(
                product.ProductId,
                Guid.NewGuid(),
                tradableItem.TradableId,
                itemSubType,
                totalPrice,
                count
            );

            Game.Game.instance.ActionManager.UpdateSell(new List<UpdateSellInfo> { updateSellInfo })
                .Subscribe();
            PostRegisterProduct(data.Item.Value.ItemBase.Value.GetLocalizedName());
        }

        private void SubscribeSellPopupCancel(Model.ItemCountableAndPricePopup data)
        {
            SharedModel.ItemCountableAndPricePopup.Value.Item.Value = null;
            Find<ItemCountableAndPricePopup>().Close();
        }

        private void SubscribeSellPopupCount(int count)
        {
            SharedModel.ItemCountableAndPricePopup.Value.Count.Value = count;
            UpdateUnitPrice();
        }

        private void SubscribeSellPopupPrice(decimal price)
        {
            var model = SharedModel.ItemCountableAndPricePopup.Value;

            if (price > LimitPrice)
            {
                price = LimitPrice;

                OneLineSystem.Push(
                    MailType.System,
                    L10nManager.Localize("UI_SELL_LIMIT_EXCEEDED"),
                    NotificationCell.NotificationType.Alert);
                Debug.LogError(L10nManager.Localize("UI_SELL_LIMIT_EXCEEDED"));
            }

            var currency = model.Price.Value.Currency;
            var major = (int)price;
            var minor = (int)((Math.Truncate((price - major) * 100) / 100) * 100);

            var fungibleAsset = new FungibleAssetValue(currency, major, minor);
            model.Price.SetValueAndForceNotify(fungibleAsset);
            UpdateUnitPrice();
        }

        private void UpdateUnitPrice()
        {
            var model = SharedModel.ItemCountableAndPricePopup.Value;

            decimal price = 0;
            if (decimal.TryParse(model.Price.Value.GetQuantityString(),
                    NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out var result))
            {
                price = result;
            }

            var count = model.Count.Value;
            var unitPrice = price / count;

            var currency = model.UnitPrice.Value.Currency;
            var major = (int)unitPrice;
            var minor = (int)((Math.Truncate((unitPrice - major) * 100) / 100) * 100);

            var fungibleAsset = new FungibleAssetValue(currency, major, minor);
            model.UnitPrice.SetValueAndForceNotify(fungibleAsset);
        }

        // sell cancellation
        private void SubscribeSellCancellationPopup(CountableItem data)
        {
            if (data is null)
            {
                Find<TwoButtonSystem>().Close();
                return;
            }

            Find<TwoButtonSystem>().Show(L10nManager.Localize("UI_SELL_CANCELLATION"),
                L10nManager.Localize("UI_YES"),
                L10nManager.Localize("UI_NO"),
                SubscribeSellCancellationPopupSubmit,
                SubscribeSellCancellationPopupCancel);
        }

        private void SubscribeSellCancellationPopupSubmit()
        {
            var model = SharedModel.ItemCountAndPricePopup.Value;
            var itemProduct = ReactiveShopState.GetSellItemProduct(model.ProductId.Value);
            var fungibleAssetProduct = ReactiveShopState.GetSellFungibleAssetProduct(model.ProductId.Value);
            var productId = itemProduct?.ProductId ?? fungibleAssetProduct.ProductId;
            var price = new FungibleAssetValue(model.Price.Value.Currency,
                itemProduct is not null
                    ? (BigInteger)itemProduct.Price
                    : (BigInteger)fungibleAssetProduct.Price,
                0);
            var agentAddress = itemProduct?.SellerAgentAddress ?? fungibleAssetProduct.SellerAgentAddress;
            var avatarAddress = itemProduct?.SellerAvatarAddress ?? fungibleAssetProduct.SellerAvatarAddress;
            var productType = itemProduct is not null
                ? itemProduct.ItemType == ItemType.Material ? ProductType.Fungible : ProductType.NonFungible
                : ProductType.FungibleAssetValue;
            var legacy = itemProduct?.Legacy ?? fungibleAssetProduct.Legacy;

            var productInfo = new ProductInfo()
            {
                ProductId = productId,
                Price = price,
                AgentAddress = agentAddress,
                AvatarAddress = avatarAddress,
                Type = productType,
                Legacy = legacy
            };

            Game.Game.instance.ActionManager.CancelProductRegistration(
                avatarAddress,
                productInfo).Subscribe();
            ResponseCancelProductRegistration(productId);
        }

        private void SubscribeSellCancellationPopupCancel()
        {
            SharedModel.ItemCountAndPricePopup.Value.Item.Value = null;
            Find<TwoButtonSystem>().Close();
        }

        private static bool DimmedFuncForSell(ItemBase itemBase)
        {
            if (itemBase.ItemType == ItemType.Material)
            {
                return !(itemBase is TradableMaterial);
            }

            return false;
        }

        private void PostRegisterProduct(string itemName)
        {
            var item = SharedModel.ItemCountableAndPricePopup.Value.Item.Value;
            var count = SharedModel.ItemCountableAndPricePopup.Value.Count.Value;
            SharedModel.ItemCountableAndPricePopup.Value.Item.Value = null;

            AudioController.instance.PlaySfx(AudioController.SfxCode.InputItem);

            var message = string.Empty;
            if (count > 1)
            {
                message = string.Format(L10nManager.Localize("NOTIFICATION_MULTIPLE_SELL_START"),
                    itemName,
                    count);
            }
            else
            {
                message = string.Format(L10nManager.Localize("NOTIFICATION_SELL_START"),
                    item.ItemBase.Value.GetLocalizedName());
            }

            OneLineSystem.Push(MailType.Auction, message,
                NotificationCell.NotificationType.Information);
        }

        private void ResponseCancelProductRegistration(Guid productId)
        {
            var count = SharedModel.ItemCountAndPricePopup.Value.Item.Value.Count.Value;
            var item = SharedModel.ItemCountAndPricePopup.Value.Item.Value;
            var itemName = item.ItemBase.Value is not null
                ? item.ItemBase.Value.GetLocalizedName()
                : item.FungibleAssetValue.Value.GetLocalizedName();

            AudioController.instance.PlaySfx(AudioController.SfxCode.InputItem);

            string message;
            if (count > 1)
            {
                message = string.Format(
                    L10nManager.Localize("NOTIFICATION_MULTIPLE_SELL_CANCEL_START"),
                    itemName, count);
            }
            else
            {
                message = string.Format(L10nManager.Localize("NOTIFICATION_SELL_CANCEL_START"),
                    itemName);
            }

            OneLineSystem.Push(MailType.Auction, message,
                NotificationCell.NotificationType.Information);

            SharedModel.ItemCountAndPricePopup.Value.Item.Value = null;
            ReactiveShopState.RemoveSellProduct(productId);
        }
    }
}
