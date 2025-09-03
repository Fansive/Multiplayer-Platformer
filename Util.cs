using UnityEngine.UI;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

public static class Util {
    public static void ClearAndAdd(this Button btn, UnityAction callback) {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(callback);
    }
    public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, 
        Func<TSource, TKey> keySelector, IComparer<TKey> comparer = null) {
        using var iterator = source.GetEnumerator();
        if (!iterator.MoveNext()) 
            throw new InvalidOperationException($"IEnumerable<{typeof(TSource)}> contains no elements");

        comparer ??= Comparer<TKey>.Default;
        TSource minVal = iterator.Current;
        TKey minKey = keySelector(minVal);
        while (iterator.MoveNext()) {
            var cur = iterator.Current;
            if (comparer.Compare(keySelector(cur), minKey) < 0) {
                minVal = cur;
                minKey = keySelector(minVal);
            }
        }
        return minVal;
    }
    public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector, IComparer<TKey> comparer = null) {
        using var iterator = source.GetEnumerator();
        if (!iterator.MoveNext())
            throw new InvalidOperationException($"IEnumerable<{typeof(TSource)}> contains no elements");

        comparer ??= Comparer<TKey>.Default;
        TSource maxVal = iterator.Current;
        TKey minKey = keySelector(maxVal);
        while (iterator.MoveNext()) {
            var cur = iterator.Current;
            if (comparer.Compare(keySelector(cur), minKey) > 0) {
                maxVal = cur;
                minKey = keySelector(maxVal);
            }
        }
        return maxVal;
    }


    /// <summary>
    /// 鼠标所在位置(在世界坐标系下)
    /// </summary>
    /// <returns></returns>
    public static Vector3 MousePosInWorld() {
        var mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
    public static bool CheckBlock(Collider2D cld, Vector2 originOffset, Vector2 rayDir, float rayLength, bool visualizeRay = false) {
        Vector2 originMid = originOffset + (Vector2)cld.bounds.center - new Vector2(0, cld.bounds.extents.y);
        Vector2 originLeft = originMid - new Vector2(cld.bounds.extents.x, 0);
        Vector2 originRight = originMid + new Vector2(cld.bounds.extents.x, 0);
        RaycastHit2D hitLeft = Physics2D.Raycast(originLeft, rayDir, rayLength, ConstData.BlockLayerMask);
        RaycastHit2D hitMid = Physics2D.Raycast(originMid, rayDir, rayLength, ConstData.BlockLayerMask);
        RaycastHit2D hitRight = Physics2D.Raycast(originRight, rayDir, rayLength, ConstData.BlockLayerMask);
        if(visualizeRay) {
            Debug.DrawRay(originLeft, rayDir * rayLength, Color.red);
            Debug.DrawRay(originMid, rayDir * rayLength, Color.red);
            Debug.DrawRay(originRight, rayDir * rayLength, Color.red);
        }
        return hitLeft | hitMid | hitRight;
    }
    /// <summary>
    /// 此处的speed正负代表方向
    /// </summary>
    public static Vector2 CalcJumpTopPoint(float speed, float jumpForce,float gravityScale = 1f, float mass= 1) {
        float t = jumpForce / (9.8f * gravityScale * mass);
        return new Vector2(speed * t, 0.5f * 9.8f * gravityScale * t * t);
    }
}
