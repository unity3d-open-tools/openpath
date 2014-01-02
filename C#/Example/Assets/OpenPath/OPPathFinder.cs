using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class OPPathFinder : MonoBehaviour {

    public int currentNode = 0;
    public OPScanner scanner;
    public float speed = 4f;
    public float stoppingDistance = 0.1f;
    public float nodeDistance = 1f;
    public Transform target;
    public bool autoChase = false;

    [HideInInspector]
    public List<OPNode> nodes = new List<OPNode>();
    [HideInInspector]
    public Vector3 goal;

    IEnumerator SetGoalAfterSeconds(float s)
    {
        yield return new WaitForSeconds(s);
        SetGoal(target);
    }

    void Start()
    {
        if (target != null)
        {
            StartCoroutine(SetGoalAfterSeconds(2));
        }
    }

    void ClearNodes()
    {
        foreach (OPNode node in nodes)
        {
            node.active = false;
            node.parent = null;
        }

        nodes.Clear();
    }

    void SetGoal(Transform t)
    {
        //if there is a goal, create the ndoes
        if (t) { SetGoal(t.position); }
        else { ClearNodes(); }
    }

    void SetGoal(Vector3 v)
    {
        if (goal == v) { return; }

        ClearNodes();
        goal = v;
        UpdatePosition();
    }

    public Vector3 GetGoal()
    {
        return goal;
    }

    void UpdatePosition()
    {
        Vector3 start = this.transform.position;

        Loom.RunAsync(delegate(){
            List<OPNode> newNodes = scanner.FindPath(start,goal);

            foreach(OPNode node in newNodes){
                node.active = true;
            }

            Loom.QueueOnMainThread(delegate(){
                nodes = newNodes;
                currentNode = 0;
            });
        });

    }

    void Update()
    {
        if (scanner == null)
        {
            if (GameObject.FindObjectOfType<OPScanner>())
            {
                scanner = GameObject.FindObjectOfType<OPScanner>();
            }
            else { Debug.LogError("No scanner found! Attach an OPScanner to the scanner varible"); return; }
        }

        //if there are nodes to follow
        if (nodes != null && nodes.Count > 0)
        {
            if ( ( transform.position - ( nodes[currentNode] as OPNode ).position ).magnitude < nodeDistance && currentNode < nodes.Count - 1 )
            {
                currentNode++;
            }

            if (autoChase)
            {
                Vector3 lookPos = (nodes[currentNode] as OPNode).position - transform.position;
                lookPos.y = 0;

                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPos), 8 * Time.deltaTime);
                transform.localPosition += transform.forward * speed * Time.deltaTime;
            }

            if ((transform.position - goal).magnitude <= stoppingDistance)
            {
                ClearNodes();
            }
        }
    }
}
