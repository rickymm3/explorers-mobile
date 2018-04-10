using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LayoutAnimator : Tracer {
    private Canvas _canvas;
    public Canvas canvas {
        get {
            if(_canvas==null) _canvas = GetComponentInParent<Canvas>();
            return _canvas;
        }
    }
    public List<Vector2> AnimateToColumns(List<GameObject> items, Vector2 offset, int maxCols=3, float spacing=5) {
        if(items==null || items.Count==0) {
            traceError("Can't animate NO items.");
            return null;
        }

        if(maxCols > items.Count) maxCols = items.Count;

        RectTransform firstTrans = (RectTransform) items[0].transform;
        RectTransform parent = (RectTransform) firstTrans.parent;
        Vector2 center = parent.TransformPoint(Vector2.zero);
        Vector2 cellSize = GetRectSize(firstTrans);
        List<Vector2> positions = new List<Vector2>();
        float cellWidth = cellSize.x + spacing;
        float cellHeight = cellSize.y + spacing;
        
        int len = items.Count;
        int lastRow = len / maxCols;
        int rowCapacity = -1;

        for (int i=0; i<len; i++) {
            int currentRow = i / maxCols;
            int colID = i % maxCols;
            if (colID == 0) {
                rowCapacity = Mathf.Min(len - i, maxCols); //????????????
            }

            int rowCapMinusOne = rowCapacity - 1;
            Vector2 fullSize = new Vector2(cellSize.x * rowCapMinusOne + spacing * rowCapMinusOne, 0);
            float x = center.x - fullSize.x * 0.5f + colID * cellWidth;
            float y = center.y - currentRow * cellHeight;
            Vector2 pos = new Vector2(x, y);

            positions.Add(pos);
            
            items[i].transform.DOMove( pos, 0.8f ).SetEase(Ease.OutBack);
        }

        return positions;
    }

    public List<Vector2> AnimateToCircle(List<GameObject> gameObjects, Vector2 offset, float startAngle, float radius) {
        var results = AnimateToCircle(gameObjects.Count, offset, startAngle, radius);
        for(int i=0; i<gameObjects.Count; i++) {
            gameObjects[i].transform.localPosition = results[i];
        }
        return results;
    }
    public List<Vector2> AnimateToCircle(int count, Vector2 offset, float startAngle, float radius) {
        var results = new List<Vector2>();

        float startAngleRad = startAngle * Mathf.Deg2Rad;
        float stepRad = (360 / count) * Mathf.Deg2Rad;

        for(int i=0; i<count; i++) {
            float currentStep = startAngleRad + stepRad * i;
            float x = Mathf.Cos(currentStep) * -radius + offset.x;
            float y = Mathf.Sin(currentStep) * radius + offset.y;

            results.Add(new Vector2(x, y));
        }

        return results;
    }

    public Vector2 GetRectSize(RectTransform rt) {
        var ratio = canvas.scaleFactor;
        return new Vector2( rt.rect.width * ratio, rt.rect.height * ratio );
    }
}
