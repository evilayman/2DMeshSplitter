using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class myHelpers
{
    public static bool LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 intersection)
    {

        float Ax, Bx, Cx, Ay, By, Cy, d, e, f, num/*,offset*/;
        float x1lo, x1hi, y1lo, y1hi;

        Ax = p2.x - p1.x;
        Bx = p3.x - p4.x;

        // X bound box test/
        if (Ax < 0)
        {
            x1lo = p2.x; x1hi = p1.x;
        }
        else
        {
            x1hi = p2.x; x1lo = p1.x;
        }

        if (Bx > 0)
        {
            if (x1hi < p4.x || p3.x < x1lo) return false;
        }
        else
        {
            if (x1hi < p3.x || p4.x < x1lo) return false;
        }

        Ay = p2.y - p1.y;
        By = p3.y - p4.y;

        // Y bound box test//
        if (Ay < 0)
        {
            y1lo = p2.y; y1hi = p1.y;
        }
        else
        {
            y1hi = p2.y; y1lo = p1.y;
        }

        if (By > 0)
        {
            if (y1hi < p4.y || p3.y < y1lo) return false;
        }
        else
        {
            if (y1hi < p3.y || p4.y < y1lo) return false;
        }



        Cx = p1.x - p3.x;
        Cy = p1.y - p3.y;
        d = By * Cx - Bx * Cy;  // alpha numerator//
        f = Ay * Bx - Ax * By;  // both denominator//



        // alpha tests//

        if (f > 0)
        {
            if (d < 0 || d > f) return false;
        }
        else
        {
            if (d > 0 || d < f) return false;
        }

        e = Ax * Cy - Ay * Cx;  // beta numerator//

        // beta tests //
        if (f > 0)
        {
            if (e < 0 || e > f) return false;
        }
        else
        {
            if (e > 0 || e < f) return false;
        }

        // check if they are parallel
        if (f == 0) return false;

        // compute intersection coordinates //
        num = d * Ax; // numerator //

        //    offset = same_sign(num,f) ? f*0.5f : -f*0.5f;   // round direction //

        //    intersection.x = p1.x + (num+offset) / f;
        intersection.x = p1.x + num / f;

        num = d * Ay;
        //    offset = same_sign(num,f) ? f*0.5f : -f*0.5f;
        //    intersection.y = p1.y + (num+offset) / f;
        intersection.y = p1.y + num / f;
        return true;

    }

    public static Vector2[] SortVerts(Vector3[] myVerts, Vector2 center)
    {
        Vector2[] arrVerts = new Vector2[myVerts.Length];
        for (int i = 0; i < myVerts.Length; i++)
        {
            arrVerts[i] = myVerts[i];
        }

        Array.Sort(arrVerts, new ClockwiseComparer(center));

        return arrVerts;
    }

    public static List<Vector3> SortVerts(List<Vector3> myVerts, Vector2 center)
    {
        Vector2[] arrVerts = new Vector2[myVerts.Count];
        for (int i = 0; i < myVerts.Count; i++)
        {
            arrVerts[i] = myVerts[i];
        }

        Array.Sort(arrVerts, new ClockwiseComparer(center));

        for (int i = 0; i < myVerts.Count; i++)
        {
            myVerts[i] = arrVerts[i];
        }

        return myVerts;
    }

    public static Vector2[] L3ToV2(List<Vector3> meshVerts)
    {
        Vector2[] verts = new Vector2[meshVerts.Count];
        for (int i = 0; i < meshVerts.Count; i++)
        {
            verts[i] = meshVerts[i];
        }

        return verts;
    }

    public static Vector3 GetCentroid(List<Vector3> poly)
    {
        float accumulatedArea = 0.0f;
        float centerX = 0.0f;
        float centerY = 0.0f;

        for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
        {
            float temp = poly[i].x * poly[j].y - poly[j].x * poly[i].y;
            accumulatedArea += temp;
            centerX += (poly[i].x + poly[j].x) * temp;
            centerY += (poly[i].y + poly[j].y) * temp;
        }

        if (Math.Abs(accumulatedArea) < 1E-7f)
            return Vector3.zero;  // Avoid division by zero

        accumulatedArea *= 3f;
        return new Vector3(centerX / accumulatedArea, centerY / accumulatedArea);
    }

    public static float getArea(GameObject myObj)
    {
        Vector3[] myVerts = myObj.GetComponent<MeshFilter>().mesh.vertices;
        int[] triangles = myObj.GetComponent<MeshFilter>().mesh.triangles;

        for (int i = 0; i < myVerts.Length; i++)
        {
            myVerts[i] = myObj.transform.TransformPoint(myVerts[i]);
        }

        float result = 0f;
        for (int p = 0; p < triangles.Length; p += 3)
        {
            result += (Vector3.Cross(myVerts[triangles[p + 1]] - myVerts[triangles[p]], 
                myVerts[triangles[p + 2]] - myVerts[triangles[p]])).magnitude;
        }
        result *= 0.5f;
        return result;
    }

    public static void startSlowMotion(float slowSpeed, float slowTime, MonoBehaviour instance)
    {
        instance.StartCoroutine(SloMo(slowSpeed, slowTime));
    }

    private static IEnumerator SloMo(float slowSpeed, float slowTime)
    {
        Time.timeScale = slowSpeed;
        Time.fixedDeltaTime = 0.02F * Time.timeScale;
        yield return new WaitForSeconds(slowTime);
        Time.timeScale = 1F;
        Time.fixedDeltaTime = 0.02F * Time.timeScale;
    }
}

