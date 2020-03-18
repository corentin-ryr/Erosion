using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainMesh : MonoBehaviour
{
    public int quadPerUnit = 3;

    private float[] heightMap;

    Mesh gameMesh;
	MeshCollider collider;
	MeshRenderer renderer;
	HeightMapGenerator heightMapGenerator;
	public Erosion erosion;

	//Holder for the mesh data
	Vector3[] vertices;
	int[] triangles;
	List<Vector2> uvs = new List<Vector2>();

    //Parameters :
    [Header("Grid parameters")]
    public int gridSizeX; //Size in unit of the game
    public int gridSizeZ;
	public Material material;

	//Heightmap parameters
	[Header("Heightmap parameters")]
	public float frequency = 1f;
    [Range(1, 8)]
    public int octaves = 4;
    public float lacunarity = 2f;
    public float persistence = 0.5f;
    public float amplitude = 1f;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.AddComponent(typeof(MeshFilter));
		gameObject.AddComponent(typeof(MeshRenderer));
		gameObject.AddComponent(typeof(MeshCollider));

		gameMesh = GetComponent<MeshFilter>().mesh;
		collider = GetComponent<MeshCollider>();
		renderer = GetComponent<MeshRenderer>();

		heightMapGenerator = new HeightMapGenerator();

		renderer.material = material;

		erosion.terrain = this;

        GenerateHeightMap();
        GeneratePlane();
        RefreshMesh();

        UpdateHeight();
        RefreshMesh();

		ErodeTerrain();
		UpdateHeight();
		RefreshMesh();
    }

    private void GenerateHeightMap()
    {
		
		heightMapGenerator.frequency = frequency;
		heightMapGenerator.octaves = octaves;
		heightMapGenerator.lacunarity = lacunarity;
		heightMapGenerator.persistence = persistence;
		heightMapGenerator.amplitude = amplitude;

        heightMap = heightMapGenerator.GenerateHeightMap(gridSizeX * quadPerUnit + 1, gridSizeZ * quadPerUnit + 1);
    }

    private void GeneratePlane()
    {
		if ((gridSizeX * quadPerUnit + 1) * (gridSizeZ * quadPerUnit + 1) >= 65534)
		{
			throw new Exception("Too much vertices. Impossible to build the mesh");
		}
        vertices = new Vector3[(gridSizeX * quadPerUnit + 1) * (gridSizeZ * quadPerUnit + 1)];
        float quadSize = 1 / (float)quadPerUnit;
		//Debug.Log(quadSize);
		
		for (int i = 0; i < (gridSizeZ * quadPerUnit + 1); i++)
		{
			for (int j = 0; j < (gridSizeX * quadPerUnit + 1); j++)
			{
				vertices[i * (gridSizeX * quadPerUnit + 1) + j] = new Vector3(quadSize * j, 0f, quadSize * i);
				float uvx = (i * gridSizeX) % quadSize;
				// uvs.Add(new Vector2(uvx, 
				// 					uvx)
				// );
			}
		}

        //triangles = new int[(gridSizeX * vertexPerUnit - 1) * (gridSizeZ * vertexPerUnit - 1) * 3];
        List<int> tempTriangles = new List<int>();

		for (int t = 0, i = 0; i < gridSizeZ * quadPerUnit; i++)
		{
			for (int j = 0; j < gridSizeX * quadPerUnit; j++)
			{
				
				tempTriangles.Add(t + gridSizeX * quadPerUnit + 1);
				tempTriangles.Add(t + 1);
				tempTriangles.Add(t);

				tempTriangles.Add(t + gridSizeX * quadPerUnit + 2);
				tempTriangles.Add(t + 1);
				tempTriangles.Add(t + gridSizeX * quadPerUnit + 1);
				t++;
			}
			t++;
		}

        triangles = tempTriangles.ToArray();
        tempTriangles.Clear(); //To free memory
    }

    private void UpdateHeight()
    {
		Vector2[] uvs = gameMesh.uv;

		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i] = new Vector3(vertices[i].x, heightMap[i], vertices[i].z);
		}


		for (int i = 0; i < uvs.Length; i++)
		{
			uvs[i] = new Vector2 (vertices[i].x / gridSizeX, vertices[i].z / gridSizeZ);
		}
    }

	private void ErodeTerrain()
    {
        //StartCoroutine(erosion.erosion(heightMap, gridSizeX * quadPerUnit + 1, 1 / (float)quadPerUnit, 20));
		erosion.erosion(heightMap, gridSizeX * quadPerUnit + 1, 1 / (float)quadPerUnit, 1);
    }

	//Action called by the button (can be after modifying the size of the map or the parameters of the noise)
	public void ReloadHeightmap () {
		GenerateHeightMap();

		GeneratePlane();
		UpdateHeight();

		RefreshMesh();
	}

    private void RefreshMesh()
    {
        gameMesh.Clear();
		gameMesh.vertices = vertices;
		gameMesh.triangles = triangles;
		gameMesh.uv = uvs.ToArray();
		gameMesh.RecalculateNormals();

		collider.sharedMesh = gameMesh;
    }

	//Debug
	public void UpdateMeshWithHeightMap () {
		heightMap = erosion.getHeightMap();
		UpdateHeight();
		RefreshMesh();
	}

}
