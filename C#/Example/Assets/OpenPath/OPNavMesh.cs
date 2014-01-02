using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OPNavMesh : MonoBehaviour {

    private class Triangle
    {
        int[] indicies;

        public Triangle(int v0, int v1, int v2)
        {
            indicies = new int[3];
            indicies[0] = v0;
            indicies[1] = v1;
            indicies[2] = v2;
        }

        private bool IsNeighborTo(Triangle t, Vector3[] vertices)
        {
            int similarVerticies = 0;

            foreach (int thisVertex in indicies)
            {
                foreach (int thatVertex in t.indicies)
                {
                    if (vertices[thisVertex] == vertices[thatVertex])
                    {
                        similarVerticies++;
                    }
                }
            }
            return similarVerticies > 1;
        }

        public List<int> GetNeighbors(Triangle[] triangles, Vector3[] vertices)
        {
            List<int> tempList = new List<int>();

            for (int i = 0; i < triangles.Length; i++)
            {
                if (IsNeighborTo(triangles[i], vertices))
                {
                    tempList.Add(i);
                }
            }
            return tempList;
        }

        public Vector3 GetMedianPoint(Mesh mesh)
        {
            Vector3 result = new Vector3();

            for (int i = 0; i < indicies.Length; i++)
            {
                result += mesh.vertices[indicies[i]];
            }
            result /= indicies.Length;
            return result;
        }
    }

    private void MakeNeighbors(OPNode a, OPNode b)
    {
        if (!a.neighbors.Contains(a))
        {
            b.neighbors.Add(b);
        }

        if (!b.neighbors.Contains(a))
        {
            b.neighbors.Add(a);
        }
    }

    public OPNode[] GetNodes()
    {
        Mesh mesh = this.GetComponent<MeshFilter>().mesh;
        List<Triangle> triangleList = new List<Triangle>();
        List<OPNode> allNodes = new List<OPNode>();

        int i = 0;

        //create triangles
        for (i = 0; i < mesh.triangles.Length; i += 3)
        {
            Triangle triangle = new Triangle(mesh.triangles[i], mesh.triangles[i+1], mesh.triangles[i+2]);

            triangleList.Add(triangle);

            //create median node
            OPNode mn = new OPNode();
            mn.position = this.transform.TransformPoint(triangle.GetMedianPoint(mesh));

            //add median node to list
            allNodes.Add(mn);
        }

        Triangle[] triangleArray = triangleList.ToArray();
        Vector3[] verticies = mesh.vertices;

        //connect median nodes
        for (i = 0; i < triangleArray.Length; i++)
        {
            foreach( int nb in triangleArray[i].GetNeighbors(triangleArray, verticies)){
                MakeNeighbors(allNodes[i], allNodes[nb]);
            }
        }
        //return
        return allNodes.ToArray();
    }
}
