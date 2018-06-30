using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeshScript : MonoBehaviour
{
    float currentRotation;
    public float CurrentRotation
    {
        get
        {
            return currentRotation;
        }

        set
        {
            currentRotation = value;
        }
    }

    float originArea;
    public float OriginArea
    {
        get
        {
            return originArea;
        }

        set
        {
            originArea = value;
        }
    }

    float myArea;
    public float MyArea
    {
        get
        {
            return myArea;
        }

        set
        {
            myArea = value;
        }
    }

    float originWeight;
    public float OriginWeight
    {
        get
        {
            return originWeight;
        }

        set
        {
            originWeight = value;
        }
    }

    Vector3 myCenter;
    public Vector3 MyCenter
    {
        get
        {
            return myCenter;
        }

        set
        {
            myCenter = value;
        }
    }

    bool showPerc;
    public bool ShowPerc
    {
        get
        {
            return showPerc;
        }

        set
        {
            showPerc = value;
        }
    }


    Camera myCam;
    GameObject myTextObj;
    Text myText;

    private void Start()
    {
        myCam = Camera.main;

        if(ShowPerc)
            CreateFont();

    }

    void CreateFont()
    {
        myTextObj = new GameObject("Text", typeof(RectTransform));
        myText = myTextObj.AddComponent<Text>();
        myTextObj.transform.SetParent(GameObject.Find("Canvas").transform);

        double myPerc = (MyArea / OriginArea) * 100;
        myPerc = System.Math.Round(myPerc, 1);
        myText.text = myPerc.ToString() + "%";

        Mesh myMesh = gameObject.GetComponent<MeshFilter>().mesh;

        
        Vector3 boundsMin = RectTransformUtility.WorldToScreenPoint(myCam, transform.TransformPoint(myMesh.bounds.min));
        Vector3 boundsMax = RectTransformUtility.WorldToScreenPoint(myCam, transform.TransformPoint(myMesh.bounds.max));

        float w = boundsMax.x - boundsMin.x;
        float h = boundsMax.y - boundsMin.y;
        myText.rectTransform.sizeDelta = new Vector2(w, h);

        myText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        myText.alignment = TextAnchor.MiddleCenter;
        myText.resizeTextForBestFit = true;
    }

    private void Update()
    {
        if(ShowPerc)
            myText.rectTransform.position = RectTransformUtility.WorldToScreenPoint(myCam, transform.TransformPoint(MyCenter));
    }

    private void OnDestroy()
    {
        Destroy(myTextObj);
    }
}
