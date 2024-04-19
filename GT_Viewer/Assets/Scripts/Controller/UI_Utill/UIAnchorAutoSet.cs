using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(UIAnchorAutoSet))]
public class UIAnchorAutoSetButton : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UIAnchorAutoSet anchorAutoSet = (UIAnchorAutoSet)target;
        if (GUILayout.Button("앵커 자동 설정"))
        {
            anchorAutoSet.Init();
            anchorAutoSet.SetChildAnchors();
        }
    }
}

public class UIAnchorAutoSet : MonoBehaviour
{
    private List<RectTransform> _list_childRectTransform;
    
    public void Init()
    {
        _list_childRectTransform = transform.GetComponentsInChildren<RectTransform>().ToList();
        foreach (var tr in _list_childRectTransform)
        {
            Debug.Log($"Child RectTransform : {tr.name}");
        }
    }

    public void SetChildAnchors()
    {
        foreach (var tr in _list_childRectTransform)
        {
            SetAnchor(tr);
        }
    }
    
    void SetAnchor(RectTransform rectTransform)
    {
        if (rectTransform.parent == null) return;

        // 구하려고 하는 렉트트랜스폼의 부모 트랜스폼의 렉트트랜스폼을 찾아서 Bounds를 구한다.
        Bounds parentBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rectTransform.parent);
        
        Vector2 parentSize = new Vector2(parentBounds.size.x, parentBounds.size.y);
        // 앵커 비율 값을 포지션 값으로 변환 
        // 현재 렉트 좌하단 꼭짓점, 우상단 꼭짓점의 위치
        Vector2 posMin = new Vector2(parentSize.x * rectTransform.anchorMin.x, parentSize.y * rectTransform.anchorMin.y);
        Vector2 posMax = new Vector2(parentSize.x * rectTransform.anchorMax.x, parentSize.y * rectTransform.anchorMax.y);

        // 렉트 가장자리와 앵커 간의 떨어진 길이 합산 -> 렉트의 꼭짓점 위치 좌표
        posMin += rectTransform.offsetMin; // 좌하단 꼭짓점 앵커와 렉트 left, bottom 사이의 길이 추가
        posMax += rectTransform.offsetMax; // 우상단 꼭짓점 앵커와 렉트 right, top 사이의 길이 추가

        // 새로운 anchorMin, anchorMax 갱신
        posMin = new Vector2(posMin.x / parentBounds.size.x, posMin.y / parentBounds.size.y);
        posMax = new Vector2(posMax.x / parentBounds.size.x, posMax.y / parentBounds.size.y);
        rectTransform.anchorMin = posMin;
        rectTransform.anchorMax = posMax;
        
        // 앵커가 렉트 꼭짓점에 붙게 해서 벌어진 길이 초기화
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
