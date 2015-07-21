using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class ExtensionMethods
{
    public static Vector3 GetObjectCenter(this Transform transform)
    {
        Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Vector3 centroid = Vector3.zero;
            foreach (var renderer in renderers)
                centroid += renderer.bounds.center;
            centroid /= renderers.Length;
            return centroid;
        }
        else
            return transform.transform.position;
    }

    public static Vector3 GetLocalVelocity(this Rigidbody rigidbody)
    {
        return rigidbody.transform.InverseTransformDirection(rigidbody.velocity);
    }
}
