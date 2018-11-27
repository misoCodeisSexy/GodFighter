using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FlyObject : MonoBehaviour {

    public GameObject obj;

    public float endValue;
    public float duration;

	void Start ()
    {
        Sequence ObjAction = DOTween.Sequence();
        ObjAction.Insert(0, obj.transform.DOLocalMoveY(obj.transform.localPosition.y + endValue, duration).SetEase(Ease.InOutQuad));
        ObjAction.Insert(duration, obj.transform.DOLocalMoveY(obj.transform.localPosition.y, duration).SetEase(Ease.InOutQuad));

        ObjAction.SetLoops(-1,LoopType.Restart);

    }

}
