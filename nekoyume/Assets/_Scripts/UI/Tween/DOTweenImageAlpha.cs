﻿using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;

namespace Nekoyume.UI.Tween
{
    [RequireComponent(typeof(Image))]
    public class DOTweenImageAlpha : DOTweenBase
    {
        public float BeginValue = 0.0f;
        public float EndValue = 1.0f;
        public bool ClearOnStop;
        private Image _image;

        protected override void Awake()
        {
            base.Awake();
            _image = GetComponent<Image>();
            if (startWithPlay)
                _image.DOFade(BeginValue, 0.0f);
        }

        public override void PlayForward()
        {
            _image.DOFade(BeginValue, 0.0f);
            if (TweenType.Repeat == tweenType)
            {
                currentTween = _image.DOFade(EndValue, duration)
                    .SetEase(ease);
                currentTween.onComplete = PlayForward;
            }
            else if (TweenType.PingPongOnce == tweenType)
            {
                currentTween = _image.DOFade(EndValue, duration)
                    .SetEase(ease);
                currentTween.onComplete = PlayReverse;
            }
            else if (TweenType.PingPongRepeat == tweenType)
            {
                currentTween = _image.DOFade(EndValue, duration)
                    .SetEase(ease);
                currentTween.onComplete = PlayReverse;
            }
            else
            {
                currentTween = _image.DOFade(EndValue, duration)
                    .SetEase(ease);
                currentTween.onComplete = OnComplete;
            }
        }

        public override void PlayReverse()
        {
            _image.DOFade(EndValue, 0.0f);
            if (TweenType.PingPongOnce == tweenType)
            {
                currentTween = _image.DOFade(BeginValue, duration)
                    .SetEase(ease);
                currentTween.onComplete = OnComplete;
            }
            else if (TweenType.PingPongRepeat == tweenType)
            {
                currentTween = _image.DOFade(BeginValue, duration)
                    .SetEase(ease);
                currentTween.onComplete = PlayForward;
            }
            else
            {
                currentTween = _image.DOFade(BeginValue, duration)
                    .SetEase(ease);
                currentTween.onComplete = OnComplete;
            }
        }

        public override void PlayRepeat()
        {
            PlayForward();
        }

        public override void PlayPingPongOnce()
        {
            PlayForward();
        }


        public override void PlayPingPongRepeat()
        {
            PlayForward();
        }

        public override void OnStopTweening()
        {
            base.OnStopTweening();
            if (ClearOnStop)
                ClearAlpha();
        }

        private void ClearAlpha()
        {
            _image.color = new Color(1f, 1f, 1f, 0f);
        }
    }
}
