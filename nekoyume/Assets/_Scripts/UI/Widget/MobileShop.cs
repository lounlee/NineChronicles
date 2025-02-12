using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Nekoyume.Game.Controller;
using Nekoyume.Helper;
using Nekoyume.L10n;
using Nekoyume.State;
using Nekoyume.UI.Module;
using NineChronicles.ExternalServices.IAPService.Runtime.Models;
using UnityEngine;

namespace Nekoyume.UI
{
    public class MobileShop : Widget
    {
        [SerializeField]
        private Toggle originCategoryTab;

        [SerializeField]
        private IAPShopProductCellView originProductCellView;

        [SerializeField]
        private UnityEngine.UI.ToggleGroup tabToggleGroup;

        [SerializeField]
        private IAPShopDynamicGridLayoutView iAPShopDynamicGridLayout;

        [SerializeField]
        private GameObject loadDataScreen;

        private bool _isInitializedObj;

        private readonly Dictionary<string, IAPShopProductCellView> _allProductObjs =
            new Dictionary<string, IAPShopProductCellView>();

        private readonly Dictionary<string, List<IAPShopProductCellView>> _allProductObjByCategory =
            new Dictionary<string, List<IAPShopProductCellView>>();

        private Toggle _recommendedToggle;

        private const string RecommendedString = "Recommended";

        private string _lastSelectedCategory;

        public static L10NSchema MOBILE_L10N_SCHEMA;

        public override void Show(bool ignoreShowAnimation = false)
        {
            ShowAsync(ignoreShowAnimation);
        }

        public async void ShowAsProduct(ProductSchema product, UnityEngine.Purchasing.Product purchasingData)
        {
            await ShowAsync();

            Analyzer.Instance.Track("Unity/Shop/IAP/LobbyPopup/Click", ("product-id", product.Sku));

            var evt = new AirbridgeEvent("IAP_LobbyPopup_Click");
            evt.SetAction(product.Sku);
            evt.AddCustomAttribute("product-id", product.Sku);
            AirbridgeUnity.TrackEvent(evt);

            Find<ShopListPopup>().Show(product, purchasingData).Forget();
        }

        public override void Close(bool ignoreCloseAnimation = false)
        {
            Game.Event.OnRoomEnter.Invoke(true);
            base.Close(ignoreCloseAnimation);
        }

        private async Task ShowAsync(bool ignoreShowAnimation = false)
        {
            var loading = Find<LoadingScreen>();
            loading.Show(LoadingScreen.LoadingType.Shop);

            try
            {
                var categorySchemas = await GetCategorySchemas();
                if (!_isInitializedObj)
                {
                    if (categorySchemas.Count == 0)
                    {
                        loading.Close();
                        base.Show(ignoreShowAnimation);
                        Close();
                        Find<IconAndButtonSystem>().Show(
                            "UI_ERROR",
                            "NOTIFICATION_NO_ENTRY_SHOP",
                            "UI_OK",
                            true,
                            IconAndButtonSystem.SystemType.Information);
                    }
                    await InitializeObj(categorySchemas);

                    _isInitializedObj = true;
                }
                else
                {
                    foreach (var category in categorySchemas)
                    {
                        foreach (var item in category.ProductList)
                        {
                            if (_allProductObjs.TryGetValue(item.Sku, out var cellView))
                            {
                                cellView.SetData(item, category.Name == RecommendedString);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                loading.Close();
                base.Show(ignoreShowAnimation);
                Close();
                Find<IconAndButtonSystem>().Show(
                    "UI_ERROR",
                    "ERROR_NO_ENTRY_SHOP",
                    "UI_OK",
                    true);
                return;
            }

            base.Show(ignoreShowAnimation);

            if (_recommendedToggle != null)
            {
                _recommendedToggle.isOn = true;
                _recommendedToggle.onObject.SetActive(true);
                _recommendedToggle.offObject.SetActive(true);
                RefreshGridByCategory(RecommendedString);
                _lastSelectedCategory = RecommendedString;
            }

            AudioController.instance.PlayMusic(AudioController.MusicCode.Shop);
            loading.Close();
        }

        private async Task InitializeObj(IEnumerable<CategorySchema> categorySchemas)
        {
            var renderCategory = categorySchemas
                .Where(c => c.Active && c.Name != "NoShow")
                .OrderBy(c => c.Order);

            foreach (var category in renderCategory)
            {
                var categoryTabObj = Instantiate(originCategoryTab, tabToggleGroup.transform);

                var iconSprite = await Util.DownloadTexture($"{MOBILE_L10N_SCHEMA.Host}/{category.Path}");
                categoryTabObj.GetComponent<IAPCategoryTab>().SetData(category.L10n_Key, iconSprite);

                categoryTabObj.onObject.SetActive(false);
                categoryTabObj.offObject.SetActive(true);
                categoryTabObj.group = tabToggleGroup;
                tabToggleGroup.RegisterToggle(categoryTabObj);
                categoryTabObj.onValueChanged.AddListener((isOn) =>
                {
                    if (!isOn)
                    {
                        return;
                    }

                    AudioController.PlayClick();
                    RefreshGridByCategory(category.Name);
                    _lastSelectedCategory = category.Name;
                });

                var productList = category.ProductList
                    .Where(p => p.Active)
                    .OrderBy(p => p.Order);

                var iapProductCellObjs = new List<IAPShopProductCellView>();
                foreach (var product in productList)
                {
                    if (!_allProductObjs.TryGetValue(product.Sku, out var productObj))
                    {
                        productObj = Instantiate(originProductCellView, iAPShopDynamicGridLayout.transform);
                        productObj.SetData(product, category.Name == RecommendedString);
                        await productObj.RefreshLocalized();
                        _allProductObjs.Add(product.Sku, productObj);
                    }

                    iapProductCellObjs.Add(productObj);
                }

                _allProductObjByCategory.Add(category.Name, iapProductCellObjs);

                categoryTabObj.interactable = true;

                if (category.Name == RecommendedString)
                {
                    _recommendedToggle = categoryTabObj;
                }
            }
        }

        public static async Task LoadL10Ns()
        {
            MOBILE_L10N_SCHEMA = await Game.Game.instance.IAPServiceManager.L10NAsync();
            await L10nManager.AdditionalL10nTableDownload($"{MOBILE_L10N_SCHEMA.Host}/{MOBILE_L10N_SCHEMA.Category}");
            await L10nManager.AdditionalL10nTableDownload($"{MOBILE_L10N_SCHEMA.Host}/{MOBILE_L10N_SCHEMA.Product}");
        }

        public static async Task<IReadOnlyList<CategorySchema>> GetCategorySchemas()
        {
            return await Game.Game.instance.IAPServiceManager
                .GetProductsAsync(States.Instance.AgentState.address, Game.Game.instance.CurrentPlanetId.ToString());
        }

        public void RefreshGrid()
        {
            RefreshGridByCategory(_lastSelectedCategory);
        }

        public void SetLoadingDataScreen(bool isLoading)
        {
            loadDataScreen.SetActive(isLoading);
        }

        public void PurchaseComplete(string productId)
        {
            if (_allProductObjs.TryGetValue(productId, out var cell))
            {
                cell.LocalPurchaseSuccess();
            }
        }

        private void RefreshGridByCategory(string categoryName)
        {
            Analyzer.Instance.Track("Unity/Shop/IAP/Tab/Click", ("category-name", categoryName));

            var evt = new AirbridgeEvent("IAP_Tab_Click");
            evt.SetAction(categoryName);
            AirbridgeUnity.TrackEvent(evt);

            foreach (var item in _allProductObjs)
            {
                item.Value.gameObject.SetActive(false);
            }

            foreach (var item in _allProductObjByCategory[categoryName].Where(item => item.IsBuyable()))
            {
                item.gameObject.SetActive(true);
            }

            iAPShopDynamicGridLayout.Refresh();
        }

        public static string GetPrice(string isoCurrencyCode, decimal price)
        {
            switch (isoCurrencyCode)
            {
                case "KRW":
                    return $"₩{price:N0}";
                default:
                    return $"{isoCurrencyCode} {price:N2}";
            }
        }

        public static string RemainTimeForDailyLimit
        {
            get
            {
                var now = DateTime.UtcNow;
                var nextDay = now.Date.AddDays(1);
                return (nextDay - now).TimespanToString();
            }
        }

        public static string RemainTimeForWeeklyLimit
        {
            get
            {
                var now = DateTime.UtcNow;

                var daysUntilNextSunday = DayOfWeek.Sunday - now.DayOfWeek;
                if (daysUntilNextSunday <= 0)
                {
                    daysUntilNextSunday += 7;
                }

                var nextSunday = now.Date.AddDays(daysUntilNextSunday);
                return (nextSunday - now).TimespanToString();
            }
        }
    }
}
