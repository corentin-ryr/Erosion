using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrayVisualizer : MonoBehaviour
{
    float quadSize;
    int mapSizeX;
    int mapSizeZ;
    float[] heightMap;

    Mesh gameMesh;
	MeshCollider collider;
	MeshRenderer renderer;

	//Holder for the mesh data
	Vector3[] vertices;
	int[] triangles;

	public Material material;


    public void CreateMesh (float[] heightMap, int mapSizeX, float quadSize) {

        gameMesh = GetComponent<MeshFilter>().mesh;
		renderer = GetComponent<MeshRenderer>();

		renderer.material = material;

        this.quadSize = quadSize;
        this.mapSizeX = mapSizeX;
        this.mapSizeZ = heightMap.Length / mapSizeX;
        this.heightMap = heightMap;

        GeneratePlane();
        UpdateHeight();
        RefreshMesh();
    }


    private void GeneratePlane()
    {
		if (mapSizeX * mapSizeZ >= 65534)
		{
			throw new Exception("Too much vertices. Impossible to build the mesh");
		}

        vertices = new Vector3[mapSizeX * mapSizeZ];
		
		for (int i = 0; i < mapSizeZ; i++)
		{
			for (int j = 0; j < mapSizeX; j++)
			{
                vertices[i * mapSizeX + j] = new Vector3(quadSize * j, 0f, quadSize * i);

			}
		}

        //triangles = new int[(gridSizeX * vertexPerUnit - 1) * (gridSizeZ * vertexPerUnit - 1) * 3];
        List<int> tempTriangles = new List<int>();

		for (int t = 0, i = 0; i < mapSizeZ - 1; i++)
		{
			for (int j = 0; j < mapSizeX - 1; j++)
			{
				
				tempTriangles.Add(t + mapSizeX);
				tempTriangles.Add(t + 1);
				tempTriangles.Add(t);

				tempTriangles.Add(t + mapSizeX + 1);
				tempTriangles.Add(t + 1);
				tempTriangles.Add(t + mapSizeX);
				t++;
			}
			t++;
		}

        triangles = tempTriangles.ToArray();
        tempTriangles.Clear(); //To free memory
    }

    private void UpdateHeight()
    {
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i] = new Vector3(vertices[i].x, heightMap[i], vertices[i].z);
		}
    }

    private void RefreshMesh()
    {
        gameMesh.Clear();
		gameMesh.vertices = vertices;
		gameMesh.triangles = triangles;
		gameMesh.RecalculateNormals();

    }

}
