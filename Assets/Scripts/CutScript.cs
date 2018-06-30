using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutScript : MonoBehaviour
{

    GameObject myObj;
    Camera myCam;
    SplitScript splitObj;
    ApplyForceScript applyForceObj;

    Vector2 posStart, posEnd;
    bool isCutting = false;

    public bool SlowMotion;
    public float slowSpeed, slowTime;

    List<GameObject> TargetList = new List<GameObject>();
    List<Vector3> IntersectionPoints = new List<Vector3>();
    List<Vector3> IntersectionPointsBefore = new List<Vector3>();

    List<Vector3> DrawLinePoints = new List<Vector3>();

    void Start ()
    {
        myCam = Camera.main;
        splitObj = GetComponent<SplitScript>();
        applyForceObj = GetComponent<ApplyForceScript>();
    }

    void Update ()
    {
        getCut();
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadSceneAsync(0);
        }
    }

    void getCut()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isCutting = true;
            posStart = myCam.ScreenToWorldPoint(Input.mousePosition);
            DrawLinePoints.Clear();

            if(SlowMotion)
                myHelpers.startSlowMotion(slowSpeed, slowTime, this);
        }

        if (isCutting)
        {
            Debug.DrawLine(posStart, myCam.ScreenToWorldPoint(Input.mousePosition), Color.green);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isCutting = false;
            sendTargets();
            DrawLine();

            if (SlowMotion)
            {
                Time.timeScale = 1F;
                Time.fixedDeltaTime = 0.02F * Time.timeScale;
            }
            
            
            
        }
    }



    private void sendTargets()
    {
        if (getTargets())
        {
            for (int i = 0; i < TargetList.Count; i++)
            {
                if (getIntersectionPoints(TargetList[i]))
                {
                    splitObj.splitMesh(TargetList[i], IntersectionPoints, IntersectionPointsBefore);
                    Destroy(TargetList[i].gameObject);

                    for (int j = 0; j < IntersectionPoints.Count; j++)
                    {
                        DrawLinePoints.Add(IntersectionPoints[j]);
                    }
                }
            }
            applyForceObj.Cuts--;
        }
    }

    private void DrawLine()
    {
        if(DrawLinePoints.Count > 0)
        {
            for (int i = 0; i < DrawLinePoints.Count - 1; i+=2)
            {
                Debug.DrawLine(DrawLinePoints[i], DrawLinePoints[i + 1], Color.red, 0.5f);
            }
        }
    }

    private bool getTargets()
    {
        TargetList.Clear();
        //var dir = (Vector2)myCam.ScreenToWorldPoint(Input.mousePosition) - posStart;
        posEnd = myCam.ScreenToWorldPoint(Input.mousePosition);
        var dir = posEnd - posStart;
        var dist = Vector2.Distance(posEnd, posStart);
        RaycastHit2D[] hit = Physics2D.RaycastAll(posStart, dir, dist);

        for (int i = 0; i < hit.Length; i++)
        {
            if (hit[i].collider != null && hit[i].collider.CompareTag("Target"))
            {
                myObj = hit[i].collider.gameObject;
                if (!TargetList.Contains(myObj))
                    TargetList.Add(myObj);
            }
        }

        return (TargetList.Count > 0) ? true : false;

    }

    private bool getIntersectionPoints(GameObject myObj)
    {

        List<Vector3> IntersectionPointsList = new List<Vector3>();
        List<Vector3> IntersectionPointsBeforeList = new List<Vector3>();

        Vector2[] verts = myObj.GetComponent<PolygonCollider2D>().points;

        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = myObj.transform.TransformPoint(verts[i]);
        }

        Vector2 p1, p2, p3, p4;
        p1 = posStart;
        p2 = posEnd;

        Vector2 intersection = Vector2.zero;
        for (int i = 0; i < verts.Length; i++)
        {
            if (i == verts.Length - 1)
            {
                p3 = verts[i];
                p4 = verts[0];
            }
            else
            {
                p3 = verts[i];
                p4 = verts[i + 1];
            }

            if (myHelpers.LineIntersection(p1, p2, p3, p4, ref intersection))
            {
                IntersectionPointsList.Add(intersection);
                IntersectionPointsBeforeList.Add(p3);
                //IntersectionPointsBefore.Add(p4);
            }
        }


        if (IntersectionPointsList.Count > 1 && IntersectionPointsList.Count % 2 == 0)
        {
            sortPoints(IntersectionPointsList, IntersectionPointsBeforeList);
            return true;
        }
        else
        {
            return false;
        }
            

    }

    private void sortPoints(List<Vector3> IntersectionPointsList, List<Vector3> IntersectionPointsBeforeList)
    {
        IntersectionPoints.Clear();
        IntersectionPointsBefore.Clear();

        float[] myPointsDistSorted = new float[IntersectionPointsList.Count];
        float[] myPointsDist = new float[IntersectionPointsList.Count];
        for (int i = 0; i < IntersectionPointsList.Count; i++)
        {
            float dist = Vector3.Distance(posStart, IntersectionPointsList[i]);
            myPointsDistSorted[i] = dist;
            myPointsDist[i] = dist;
        }

        Array.Sort(myPointsDistSorted);

        for (int i = 0; i < myPointsDistSorted.Length; i++)
        {
            for (int j = 0; j < myPointsDist.Length; j++)
            {
                if (myPointsDistSorted[i] == myPointsDist[j])
                {
                    IntersectionPoints.Add(IntersectionPointsList[j]);
                    IntersectionPointsBefore.Add(IntersectionPointsBeforeList[j]);
                }
            }
        }
    }


}