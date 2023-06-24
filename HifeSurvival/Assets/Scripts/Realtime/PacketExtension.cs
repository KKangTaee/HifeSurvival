using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PacketExtension
{
    public static Vector3 ConvertUnityVector3(this PVec3 inSelf)
    {
        return new Vector3(inSelf.x, inSelf.y, inSelf.z);
    }

    public static PVec3 ConvertPVec3(this Vector3 inSelf)
    {
        return new PVec3()
        {
            x = inSelf.x,
            y = inSelf.y,
            z = inSelf.z
        };
    }

    public static bool NearlyEqual(this PVec3 a, PVec3 b, float epsilon = 0.0001f)
    {
        return Mathf.Abs(a.x - b.x) <= epsilon
            && Mathf.Abs(a.y - b.y) <= epsilon
            && Mathf.Abs(a.z - b.z) <= epsilon;
    }
}
