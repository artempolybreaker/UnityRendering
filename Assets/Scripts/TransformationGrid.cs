using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;


public class TransformationGrid : MonoBehaviour
{
    public Transform Prefab;

    private Transform[] _grid;

    public int GridResolution = 10;

    private List<TransformationBase> _transformations;
    
    Matrix4x4 transformation;

    public float focalLength = 1;
    
    private void Awake()
    {
        _grid = new Transform[GridResolution * GridResolution * GridResolution];
        _transformations = new List<TransformationBase>();
        
        for (int i = 0, x = 0; x < GridResolution; x++)
        {
            for (int y = 0; y < GridResolution; y++)
            {
                for (int z = 0; z < GridResolution; z++, i++)
                {
                    _grid[i] = CreateGridPoint(x, y, z);
                }
            }
        }
    }

    void Update()
    {
        UpdateTransformation();
        for (int i = 0, x = 0; x < GridResolution; x++)
        {
            for (int y = 0; y < GridResolution; y++)
            {
                for (int z = 0; z < GridResolution; z++, i++)
                {
                    _grid[i].localPosition = TransformPoint(x, y, z);
                }
            }
        }
    }
    
    void UpdateTransformation () {
        GetComponents<TransformationBase>(_transformations);
        if (_transformations.Count > 0) {
            transformation = _transformations[0].Matrix;
            for (int i = 1; i < _transformations.Count; i++) {
                transformation = _transformations[i].Matrix * transformation;
            }
        }
    }

    private Vector3 TransformPoint(int x, int y, int z)
    {
        var coordinates = GetCoordinates(x, y, z);
        
        
        var matrix2D = new Matrix4x4();
        matrix2D.SetRow(0, new Vector4(focalLength, 0, 0, 0));
        matrix2D.SetRow(1, new Vector4(0, focalLength, 0, 0));
        matrix2D.SetRow(2, new Vector4(0, 0, 0, 0));
        matrix2D.SetRow(3, new Vector4(0, 0, 1, 0));

        
        var transformedPoint =  transformation.MultiplyPoint(coordinates);
        return matrix2D.MultiplyPoint(transformedPoint);
    }

    private Transform CreateGridPoint(int x, int y, int z)
    {
        var point = Instantiate<Transform>(Prefab);
        point.GetComponent<MeshRenderer>().material.color = new Color(
            (float) x / GridResolution,
            (float) y / GridResolution,
            (float) z / GridResolution
        );
        point.localPosition = GetCoordinates(x, y, z);
        return point;
    }

    private Vector3 GetCoordinates(int x, int y, int z)
    {
        return new Vector3(
            x - (GridResolution - 1) * 0.5f,
            y - (GridResolution - 1) * 0.5f,
            z - (GridResolution - 1) * 0.5f
        );
    }

    private void OnDisable()
    {
        for (var i = 0; i < _grid.Length; i++)
        {
            var element = _grid[i];
            DestroyImmediate(element);
        }
    }
}