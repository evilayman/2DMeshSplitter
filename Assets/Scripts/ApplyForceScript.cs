using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyForceScript : MonoBehaviour
{
    List<Rigidbody2D> myRBS = new List<Rigidbody2D>();
    List<Vector3> myDirs = new List<Vector3>();

    public List<Rigidbody2D> MyRBS
    {
        get
        {
            return myRBS;
        }

        set
        {
            myRBS = value;
        }
    }
    public List<Vector3> MyDirs
    {
        get
        {
            return myDirs;
        }

        set
        {
            myDirs = value;
        }
    }

    public int Cuts;
    public float gravityScale, splitForce;

    public bool SlowMotion;
    public float slowSpeed, slowTime;

	
	void Update ()
    {
        if(Cuts < 1)
        {
            ApplyForce();
        }
    }

    void ApplyForce()
    {
        if (myRBS.Count > 0)
        {
            for (int i = 0; i < MyRBS.Count; i++)
            {
                if (myRBS[i])
                {
                    MyRBS[i].gravityScale = gravityScale;
                    MyRBS[i].GetComponent<Rigidbody2D>().AddForce( myDirs[i] * Time.deltaTime * splitForce, ForceMode2D.Impulse);
                }
            }

            MyRBS.Clear();
            MyDirs.Clear();

            if(SlowMotion)
                myHelpers.startSlowMotion(slowSpeed, slowTime, this);

        }
    }
}
