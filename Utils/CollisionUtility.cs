using System.Collections.Generic;
using UnityEngine;

public static class CollisionUtility
{
    public static bool IsColliding(Collider target, List<Collider> others)
    {
        foreach (Collider other in others)
        {
            if (other == null || other == target)
                continue;

            if (Physics.ComputePenetration(
                target, target.transform.position, target.transform.rotation,
                other, other.transform.position, other.transform.rotation,
                out _, out _))
            {
                return true;
            }
        }

        return false;
    }

    public static Bounds CreateAABB(List<Collider> colliders)
    {
        if (colliders == null || colliders.Count == 0)
        {
            Debug.LogError("Collider list is empty.");
            return default;
        }

        // min/max 初期化
        Vector3 min = colliders[0].bounds.min;
        Vector3 max = colliders[0].bounds.max;

        // 全Colliderを包含
        for (int i = 1; i < colliders.Count; i++)
        {
            Bounds b = colliders[i].bounds;

            min = Vector3.Min(min, b.min);
            max = Vector3.Max(max, b.max);
        }

        // Bounds生成
        Bounds aabb = new Bounds();
        aabb.SetMinMax(min, max);

        return aabb;
    }
    
    public static bool IsCollidingWithWallOrFloor(Bounds bounds, List<Bounds> wallAndFloorBounds)
    {
        foreach(var wallBound in wallAndFloorBounds)
        {
            if(bounds.Intersects(wallBound)) return true;
        }
        return false;
    }

}