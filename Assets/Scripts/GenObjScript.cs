using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenObjScript : MonoBehaviour
{
    
    public bool useSprte;
    public Sprite mySprite;

    public Texture2D tex;

    public Vector3 Position;
    public Vector3 Scale;

    public float weight;
    public float gravityScale;

    void Start()
    {
        List<Vector3> myVerts = new List<Vector3>()
        {
            new Vector3(-1, -1, 0),
            new Vector3(-1, 1, 0),
            new Vector3(1, 1, 0),
            new Vector3(1, -1f, 0),
            new Vector3(0.5f, -1f, 0),
            new Vector3(0.5f, 0.5f, 0),
            new Vector3(0f, 0.5f, 0),
            new Vector3(0f, -1f, 0),
            new Vector3(-0.5f, -1f, 0),
            new Vector3(-0.5f, 0.5f, 0),
        };

        if (useSprte)
        {
            myVerts.Clear();
            for (int i = 0; i < mySprite.vertices.Length; i++)
            {
                myVerts.Add(mySprite.vertices[i]);
            }
            myVerts = myHelpers.SortVerts(myVerts, Vector3.zero);
        }


        GenMesh(myVerts, gravityScale, 0, Position, Scale, weight);

        //CreateGameObjectFromSprite(SpriteToMesh(mySprite));

    }

    void GenMesh(List<Vector3> myVerts, float gravityScale, int pushDir, Vector3 pos, Vector3 scale, float weight)
    {
        Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();
        poly.outside = myVerts;
        
        GameObject myObj = Poly2Mesh.CreateGameObject(poly);
        myObj.transform.position = pos;
        myObj.transform.localScale = scale;
        myObj.gameObject.tag = "Target";

        myObj.AddComponent<PolygonCollider2D>().points = myHelpers.L3ToV2(myVerts);

        myObj.GetComponent<Renderer>().material.shader = Shader.Find("Sprites/Default");
        myObj.GetComponent<Renderer>().material.mainTexture = tex;

        Mesh myMesh = myObj.GetComponent<MeshFilter>().mesh;

        myMesh.uv = GetUV(myMesh.vertices);

        MeshScript meshScript = myObj.AddComponent<MeshScript>();
        meshScript.CurrentRotation = 0f;
        meshScript.OriginWeight = weight;
        meshScript.OriginArea = meshScript.MyArea = myHelpers.getArea(myObj);
        meshScript.MyCenter = myHelpers.GetCentroid(myVerts);

        myObj.AddComponent<Rigidbody2D>().gravityScale = gravityScale;

    }

    private Vector2[] GetUV(Vector3[] meshVerts)
    {
        Vector2[] uvs = new Vector2[meshVerts.Length];

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

        for (int i = 0; i < meshVerts.Length; i++)
        {
            uvs[i] = new Vector2((meshVerts[i].x - minX) / (maxX - minX), (meshVerts[i].y - minY) / (maxY - minY));
        }

        return uvs;
    }

    /*
    private Mesh SpriteToMesh(Sprite sprite)
    {
        Mesh mesh = new Mesh();
        mesh.SetVertices(Array.ConvertAll(sprite.vertices, i => (Vector3)i).ToList());
        mesh.SetUVs(0, sprite.uv.ToList());
        mesh.SetTriangles(Array.ConvertAll(sprite.triangles, i => (int)i), 0);

        return mesh;
    }
    public GameObject CreateGameObjectFromSprite(Mesh mesh, string name = "Polygon")
    {
        GameObject gob = new GameObject();
        gob.name = name;
        gob.AddComponent(typeof(MeshRenderer));
        MeshFilter filter = gob.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = mesh;

        gob.gameObject.tag = "Target";

        Vector2[] colPoints = new Vector2[mesh.vertices.Length];
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            colPoints[i] = mesh.vertices[i];
        }

        gob.AddComponent<PolygonCollider2D>().points = colPoints;

        gob.GetComponent<Renderer>().material.shader = Shader.Find("Sprites/Default");


        return gob;
    }
    */
}