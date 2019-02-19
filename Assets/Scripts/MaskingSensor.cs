using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MaskingSensor : MonoBehaviour {

    // Use this for initialization

    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    public float apertureAngle;
    public int anzPoints = 0;
    public float range = 1;
    public int angleHorizontal = 0;

    Vector3[] vertices;
    int[] triangles;

    void Start()
    {
     

        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();

        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();
        if (meshRenderer != null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

        if (mesh != null)
            mesh.Clear();

        CreateMesh(angleHorizontal);
    }

    private void Update()
    {
        RaycastSensor();

    }

    void CreateMesh(float angleH)
    {
        Vector3 origin = new Vector3(0f, 0f, 0f);
        Vector3 point = new Vector3(range, 0f, 0f);

        //Origin point + Points
        vertices = new Vector3[2*anzPoints + 1];

        //Symetrie fuer ungerade und gerade Anzahl an vertices
        float angleSteps = (angleH / (anzPoints - 1));
        //Erster Punkt des Winkels der xz-Ebene
        float startAngle = (angleH / 2) * -1;

        //Anzahl Renderdreiecke Top und Ground + Front und Seiten
        int faktor = anzPoints - 1;
        triangles = new int[12 * faktor];



        //Origin
        vertices[0] = origin;
        //Alle weiteren Points
        for (int i = 1; i <= anzPoints; i++)
        {
            vertices[i]   = RotatePointAroundPivot(point, origin, Quaternion.Euler(0f, startAngle, 0f));
            vertices[i + anzPoints]  = RotateZ(vertices[i],  apertureAngle / 2);

            //vertices ueberschreiben damit verticesGround nicht in y = 0 liegen
            vertices[i]   = RotateZ(vertices[i], -apertureAngle / 2);

            startAngle += angleSteps;

        }

        //fuer anzPoints = 2
        //Ground
        //triangles[0] = 0;
        //triangles[1] = 1;
        //triangles[2] = 2;
        //Top
        //triangles[3] = 0;
        //triangles[4] = 3;
        //triangles[5] = 4;

        int offset = 0;
        int verticesPart = triangles.Length / 4;
        for (int i = 0; i < anzPoints-1; i++)
        {
            //iteriere 3 mal fuer Bottom material
            triangles[offset++] = 0;
            triangles[offset++] = i + 2;
            triangles[offset++] = i + 1;

            //weitere 3 mal fuer top material
            triangles[verticesPart + offset - 3] = 0;
            triangles[verticesPart + offset - 2] = anzPoints + i + 1;
            triangles[verticesPart + offset - 1] = anzPoints + i + 2;

            //Front untere Dreiecke
            triangles[(2 * verticesPart) + offset - 3] = i + 1;
            triangles[(2 * verticesPart) + offset - 2] = i + 2;
            triangles[(2 * verticesPart) + offset - 1] = anzPoints + i + 1;

            //Front obere Dreiecke
            triangles[(3 * verticesPart) + offset - 3] = i + 2;
            triangles[(3 * verticesPart) + offset - 2] = anzPoints + i + 2;
            triangles[(3 * verticesPart) + offset - 1] = anzPoints + i + 1;
        }

        Array.Resize(ref triangles, triangles.Length + 6);
        triangles[triangles.Length - 6] = 0;
        triangles[triangles.Length - 5] = 1;
        triangles[triangles.Length - 4] = anzPoints + 1;
        triangles[triangles.Length - 3] = 0;
        triangles[triangles.Length - 2] = 2 * anzPoints;
        triangles[triangles.Length - 1] = anzPoints;



        mesh.vertices = vertices;
        mesh.triangles = triangles;
        meshFilter.mesh = mesh;

    }



    /// <summary>
	/// Rotiert einen Punkt um den angegebenen Schwerpunkt
	/// </summary>
	/// <param name="point">Punkt der rotiert werden soll</param>
	/// <param name="pivot">Schwerpunkt um den rotiert werden soll</param>
	/// <param name="rotation">Rotation</param>
	/// <returns>Den rotierten Punkt</returns>
	public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        Vector3 dir = point - pivot;
        dir = rotation * dir;
        point = dir + pivot;

        return point;
    }

    public Vector3 RotateZ(Vector3 point, float degree)
    {

        /*
         * Drehmatrix Um die Z-Achse
         * =========================
         * 
         (cosA  -sinA 0)
         (sinA  cosA  0)
         (0     0     1)
         */

        float angle = degree * Mathf.Deg2Rad;
        float sinA  = Mathf.Sin(angle);
        float cosA  = Mathf.Cos(angle);

        return new Vector3( cosA * point.x - sinA * point.y,
                            sinA * point.x + cosA * point.y,
                            1*point.z);
    }

    public Vector3 multiplyVector(Vector3 point, float faktor)
    {
        return new Vector3(point.x * faktor, point.y*faktor, point.z*faktor);
    }

    void RaycastSensor()
    {
        RaycastHit hit;
        for (int i = 0; i < vertices.Length; i++)
        {
            if (Physics.Raycast(vertices[0], vertices[i], out hit, range))
            {
                vertices[i] = hit.point;
            }
            else
            {
                vertices[i] = Vector3.Normalize(vertices[i]) * range;
            }
            

        }

    }

    void OnDrawGizmos()
    {
        if (vertices != null)
        {

            for (int i = 0; i < vertices.Length; i++)
            {

                Gizmos.color = Color.green;
                Gizmos.DrawSphere(vertices[i], 0.01f);
                Gizmos.DrawLine(vertices[0], vertices[i]);

         

            }

            for (int i = 0; i < vertices.Length; i++)
            {

                Gizmos.color = Color.red;
                Gizmos.DrawSphere(vertices[i] * 0.5f, 0.01f);
                Gizmos.DrawLine(vertices[0], vertices[i]);



            }

        }

    }
}
