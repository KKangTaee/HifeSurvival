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
}
