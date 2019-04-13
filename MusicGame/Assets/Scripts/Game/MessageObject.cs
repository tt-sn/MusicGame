using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Game
{
    public class MessageObject : MonoBehaviour
    {
        public float baseY;

        [SerializeField]
        Text messageText;

        public void Initialize(string message, Color color, float positionX)
        {
            gameObject.SetActive(true);
            messageText.text = message;
            messageText.color = color;
            //生成した判定文字に初期座標を入れる
            transform.localPosition = new Vector3(positionX, baseY);
            //DOアニメーションを終了する
            transform.DOKill();
            //アニメーションを組み立てる
            DOTween.Sequence()
                    //最初に呼び出される
                    //大きさを０にする
                   .OnStart(() =>
                   {
                       transform.localScale = Vector3.zero;
                   })
                   //直前の動作の次に行う
                   //大きさを0.5に線形変換（イージングしながら）する
                   .Append(transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce))
                   //直前の動作と並列して行う
                   //イージングしながら上に移動
                   .Join(transform.DOLocalMoveY(baseY + 300f, 1f).SetEase(Ease.OutCirc))
                   //全てのアニメーションが終わり次第行う
                   .OnComplete(() =>
                   {
                       gameObject.SetActive(false);
                   }).Play();
        }
    }
}
