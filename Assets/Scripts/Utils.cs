using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils 
{
    public static (Vector3 point0, Vector3 point1) getCapsuleColliderPoints(CapsuleCollider collider)
    {
        Vector3 direction = new Vector3 { [collider.direction] = 1 };
        float offset = collider.height / 2 - collider.radius;
        Vector3 localPoint0 = collider.center - direction * offset;
        Vector3 localPoint1 = collider.center + direction * offset;

        return (localPoint0, localPoint1);
    }
}
