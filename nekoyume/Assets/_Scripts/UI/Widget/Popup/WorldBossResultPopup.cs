using Libplanet.Assets;
using Nekoyume.Helper;
using Nekoyume.UI.Module.WorldBoss;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nekoyume.UI
{
    public class WorldBossResultPopup : PopupWidget
    {
        [SerializeField]
        private Transform gradeParent;

        [SerializeField]
        private GameObject seasonBestObject;

        [SerializeField]
        private TextMeshProUGUI scoreText;

        [SerializeField]
        private Button closeButton;

        [SerializeField]
        private List<RuneStoneItem> rewardViews;

        private GameObject _gradeObject;

        private List<FungibleAssetValue> _killRewards;

        protected override void Awake()
        {
            base.Awake();
            closeButton.onClick.AddListener(() => Close());
        }

        protected override void OnDisable()
        {
            Destroy(_gradeObject);
            base.OnDisable();
        }

        public void Show(
            int bossId,
            int score,
            bool isBest,
            List<FungibleAssetValue> battleRewards,
            List<FungibleAssetValue> killRewards)
        {
            base.Show();

            scoreText.text = score.ToString("N0");
            seasonBestObject.SetActive(isBest);

            if (Game.Game.instance.TableSheets.WorldBossCharacterSheet.TryGetValue(bossId, out var row))
            {
                var grade = (WorldBossGrade)WorldBossHelper.CalculateRank(row, score);

                if (WorldBossFrontHelper.TryGetGrade(grade, false, out var prefab))
                {
                    _gradeObject = Instantiate(prefab, gradeParent);
                    _gradeObject.transform.localScale = Vector3.one;
                }
            }

            foreach (var view in rewardViews)
            {
                view.gameObject.SetActive(false);
            }

            _killRewards = killRewards;
            if (battleRewards is not null && battleRewards.Any())
            {
                foreach (var reward in battleRewards)
                {
                    var ticker = reward.Currency.Ticker;
                    if (WorldBossFrontHelper.TryGetRuneIcon(ticker, out var icon))
                    {
                        var view = rewardViews.First(x => !x.gameObject.activeSelf);
                        var count = Convert.ToInt32(reward.GetQuantityString());
                        view.Set(icon, count);
                        view.gameObject.SetActive(true);
                    }
                }
            }
        }

        public override void Close(bool ignoreCloseAnimation = false)
        {
            if (_killRewards is not null && _killRewards.Any())
            {
                Find<WorldBossRewardPopup>().Show(_killRewards,
                    () =>Find<WorldBoss>().ShowAsync().Forget());
            }
            else
            {
                Find<WorldBoss>().ShowAsync().Forget();
            }

            base.Close(ignoreCloseAnimation);
        }
    }
}
