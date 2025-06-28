
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace GameLogic.Battle
{
    public class ClickPiBehavior : NoteBehavior
    {
        public override void OnMiss()
        {
            base.OnMiss();
        }

        public override void OnBeginHit()
        {
            base.OnBeginHit();
            transform.DOScale(0, 0.15f).SetEase(Ease.OutElastic);
        }
    }
}