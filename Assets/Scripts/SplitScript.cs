using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplitScript : MonoBehaviour
{
    GameObject myObj;

    ApplyForceScript applyForceObj;

    public bool ShowPercentage;

    private void Start()
    {
        applyForceObj = GetComponent<ApplyForceScript>();
    }

    public void splitMesh(GameObject myObj, List<Vector3> IntersectionPoints, List<Vector3> IntersectionPointsBefore)
    {
        Vector2[] verts = myObj.GetComponent<PolygonCollider2D>().points;

        int meshAmount = (IntersectionPoints.Count / 2) + 1;

        List<Vector3>[] myNewVerts = new List<Vector3>[meshAmount];

        for (int i = 0; i < myNewVerts.Length; i++)
        {
            myNewVerts[i] = new List<Vector3>();
        }

        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = myObj.transform.TransformPoint(verts[i]);
        }

        List<Vector3> allVerts = new List<Vector3>();

        for (int i = 0; i < verts.Length; i++)
        {
            allVerts.Add(verts[i]);
            for (int j = 0; j < IntersectionPoints.Count; j++)
            {
                if(verts[i] == (Vector2)IntersectionPointsBefore[j])
                {
                    allVerts.Add(IntersectionPoints[j]);
                }
            }
        }


        Stack<int> currentMeshes = new Stack<int>();
        currentMeshes.Push(meshAmount - 1);

        Vector3[] centerInterPoints = new Vector3[meshAmount];

        for (int i = 0; i < allVerts.Count; i++)
        {
            for (int j = 0; j < IntersectionPoints.Count; j+=2)
            {
                if (allVerts[i] == IntersectionPoints[j] || allVerts[i] == IntersectionPoints[j+1])
                {
                    if(currentMeshes.Peek() == j/2)
                    {
                        myNewVerts[currentMeshes.Peek()].Add(allVerts[i]);
                        currentMeshes.Pop();
                    }
                    else
                    {
                        myNewVerts[currentMeshes.Peek()].Add(allVerts[i]);
                        currentMeshes.Push(j/2);
                    }

                    centerInterPoints[currentMeshes.Peek()] = getCenter(IntersectionPoints[j], IntersectionPoints[j + 1]);
                }
            }

            myNewVerts[currentMeshes.Peek()].Add(allVerts[i]);
        }

        prepareMeshes(myObj, allVerts, myNewVerts, centerInterPoints);

    }
    
    Vector3 getCenter(Vector3 p1, Vector3 p2)
    {
        Vector3 myCenter = Vector3.zero;

        myCenter.x = (p1.x + p2.x) / 2;
        myCenter.y = (p1.y + p2.y) / 2;

        return myCenter;
    }

    void prepareMeshes(GameObject myObj, List<Vector3> allVerts, List<Vector3>[] myNewVerts, Vector3[] centerInterPoints)
    {
        Mesh myMesh = myObj.GetComponent<MeshFilter>().mesh;

        Material myMat = myObj.GetComponent<Renderer>().material;
        Texture2D tex = (Texture2D)myMat.mainTexture;

        Vector3[] allVertsMesh;
        Vector2[] allUV;

        Vector2 parentVelocity = myObj.GetComponent<Rigidbody2D>().velocity;
        float parentGravity = myObj.GetComponent<Rigidbody2D>().gravityScale;

        MeshScript meshScript = myObj.GetComponent<MeshScript>();

        float originArea = meshScript.OriginArea;
        float originWeight = meshScript.OriginWeight;
        float prevAngle = meshScript.CurrentRotation;

        float angle = myObj.transform.localEulerAngles.z;
        angle = (angle > 180) ? angle - 360 : angle;
        angle = angle + prevAngle;

        GenMeshForUV(myMesh.uv, allVerts, out allVertsMesh, out allUV, angle);

        for (int i = 0; i < myNewVerts.Length; i++)
        {
            GenMeshSplit(allVertsMesh, allUV, myNewVerts[i], centerInterPoints[i], tex, angle, parentVelocity, parentGravity, originArea, originWeight);
        }

    }

    void GenMeshSplit(Vector3[] parentVerts, Vector2[] parentUV, List<Vector3> myVerts, Vector3 centerInterPoint, Texture tex, float rotAngle, Vector2 parentVelocity, float parentGravity, float originArea, float originWeight)
    {
        Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();
        poly.outside = myVerts;
        GameObject myObj = Poly2Mesh.CreateGameObject(poly);
        myObj.gameObject.tag = "Target";

        Material myMat = myObj.GetComponent<Renderer>().material;
        myMat.shader = Shader.Find("Sprites/Default");
        myMat.mainTexture = tex;

        Mesh myMesh = myObj.GetComponent<MeshFilter>().mesh;
        myMesh.uv = deductUV(parentVerts, parentUV, myMesh.vertices);

        
        MeshScript meshScript = myObj.AddComponent<MeshScript>();
        meshScript.ShowPerc = ShowPercentage;
        meshScript.CurrentRotation = rotAngle;
        meshScript.OriginWeight = originWeight;
        meshScript.OriginArea = originArea;
       
        float myArea = myHelpers.getArea(myObj);
        meshScript.MyArea = myArea;

        meshScript.MyCenter = myHelpers.GetCentroid(myVerts);

        myObj.AddComponent<PolygonCollider2D>().points = myHelpers.L3ToV2(myVerts);

        Rigidbody2D myRB = myObj.AddComponent<Rigidbody2D>();

        myRB.gravityScale = parentGravity;

        myRB.velocity = parentVelocity;

        myRB.mass = (myArea * originWeight) / originArea;
        applyForceObj.MyRBS.Add(myRB);

        Vector3 myDir = myHelpers.GetCentroid(myVerts) - centerInterPoint;        
        applyForceObj.MyDirs.Add(myDir.normalized);
    }

    Vector2[] deductUV(Vector3[] parentVerts, Vector2[] parentUV, Vector3[] myVerts)
    {
        Vector2[] myUV = new Vector2[myVerts.Length];
        for (int i = 0; i < parentVerts.Length; i++)
        {
            for (int j = 0; j < myVerts.Length; j++)
            {
                if (parentVerts[i] == myVerts[j])
                {
                    myUV[j] = parentUV[i];
                }
            }
        }
        return myUV;
    }

    void GenMeshForUV(Vector2[] parentUV, List<Vector3> myVerts, out Vector3[] allVertsMesh, out Vector2[] allUV, float angle)
    {
        Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();
        poly.outside = myVerts;
        Mesh mesh = Poly2Mesh.CreateMesh(poly);
        mesh.uv = GetUVSplit(parentUV, mesh.vertices, angle);
        allVertsMesh = mesh.vertices;
        allUV = mesh.uv;
    }

    private Vector2[] GetUVSplit(Vector2[] parentUV, Vector3[] meshVerts, float angle)
    {
        Vector2[] uvs = new Vector2[meshVerts.Length];

        for (int i = 0; i < meshVerts.Length; i++)
        {
            meshVerts[i] = Quaternion.Euler(0, 0, -angle) * meshVerts[i];
        }

        float minX = meshVerts[0][0];
        float minY = meshVerts[0][1];
        float maxX = minX;
        float maxY = minY;

        for (int i = 0; i < meshVerts.Length; i++)
        {
            if (meshVerts[i][0] < minX)
            {
                minX = meshVerts[i][0];
            }

            if (meshVerts[i][0] > maxX)
            {
                maxX = meshVerts[i][0];
            }

            if (meshVerts[i][1] < minY)
            {
                minY = meshVerts[i][1];
            }

            if (meshVerts[i][1] > maxY)
            {
                maxY = meshVerts[i][1];
            }
        }

        float minU = parentUV[0][0];
        float minV = parentUV[0][1];
        float maxU = minU;
        float maxV = minV;

        for (int i = 0; i < parentUV.Length; i++)
        {
            if (parentUV[i][0] < minU)
            {
                minU = parentUV[i][0];
            }

            if (parentUV[i][0] > maxU)
            {
                maxU = parentUV[i][0];
            }

            if (parentUV[i][1] < minV)
            {
                minV = parentUV[i][1];
            }

            if (parentUV[i][1] > maxV)
            {
                maxV = parentUV[i][1];
            }
        }

        for (int i = 0; i < meshVerts.Length; i++)
        {
            uvs[i] = new Vector2(MapValue(minX, maxX, minU, maxU, meshVerts[i].x), MapValue(minY, maxY, minV, maxV, meshVerts[i].y));
        }

        return uvs;

    }

    private float MapValue(float a0, float a1, float b0, float b1, float a)
    {
        return b0 + (b1 - b0) * ((a - a0) / (a1 - a0));
    }

    
}