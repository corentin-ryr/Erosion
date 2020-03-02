// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.AI;

// [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
// public class Chunk : MonoBehaviour {

// 	Mesh gameMesh;
// 	MeshCollider collider;
// 	public MeshRenderer renderer;

// 	public Material material;
// 	public TerrainMesh terrainMesh;

// 	List<Vector3> vertices = new List<Vector3>();
// 	List<int> triangles = new List<int>();
// 	List<Vector2> uvs = new List<Vector2>();

// 	// Use this for initialization
// 	void Awake () {
// 		gameObject.AddComponent(typeof(MeshFilter));
// 		gameObject.AddComponent(typeof(MeshRenderer));
// 		gameObject.AddComponent(typeof(MeshCollider));

// 		gameMesh = GetComponent<MeshFilter>().mesh;
// 		collider = GetComponent<MeshCollider>();

// 		renderer = GetComponent<MeshRenderer>();

// 		GeneratePlane();
// 		RefreshMesh(vertices, triangles);
// 		gameMesh.uv = uvs.ToArray();
// 	}
	
// 	void GeneratePlane () {

// 		float quadSize = 1 / (float)TerrainMetrics.quadPerMeter;
		
// 		for (int i = 0; i < (TerrainMetrics.chunkSizeX * TerrainMetrics.quadPerMeter + 1); i++)
// 		{
// 			for (int j = 0; j < (TerrainMetrics.chunkSizeZ * TerrainMetrics.quadPerMeter + 1); j++)
// 			{
// 				vertices.Add(new Vector3(quadSize * i, 0f, quadSize * j));
// 				uvs.Add(new Vector2(quadSize * i / TerrainMetrics.chunkSizeX * TerrainMetrics.quadPerMeter, 
// 									quadSize * j / TerrainMetrics.chunkSizeX * TerrainMetrics.quadPerMeter)
// 				);
// 			}
// 		}

// 		for (int t = 0, i = 0; i < TerrainMetrics.chunkSizeX * TerrainMetrics.quadPerMeter; i++)
// 		{
// 			for (int j = 0; j < TerrainMetrics.chunkSizeZ * TerrainMetrics.quadPerMeter; j++)
// 			{
// 				triangles.Add(t);
// 				triangles.Add(t + 1);
// 				triangles.Add(t + TerrainMetrics.chunkSizeX * TerrainMetrics.quadPerMeter + 1);

// 				triangles.Add(t + TerrainMetrics.chunkSizeX * TerrainMetrics.quadPerMeter + 1);
// 				triangles.Add(t + 1);
// 				triangles.Add(t + TerrainMetrics.chunkSizeX * TerrainMetrics.quadPerMeter + 2);
// 				t++;
// 			}
// 			t++;
// 		}
// 	}



// 	//Modify the vertices =====================================================================================================

// 	public void RefreshHeight (List<float> heightMap) {
// 		RefreshHeight(heightMap.ToArray());	
// 	}
// 	public void RefreshHeight (float[,] heightMap) {
// 		float [] newMap = new float[heightMap.GetLength(0) * heightMap.GetLength(1)];
// 		for (int t = 0, i = 0; i < heightMap.GetLength(0); i++)
// 		{
// 			for (int j = 0; j < heightMap.GetLength(1); j++)
// 			{
// 				newMap[t] = heightMap[i, j];
// 				t++;
// 			}
// 		}
// 		RefreshHeight(newMap);	
// 	}

// 	public void RefreshHeight (float[] heightMap) {

// 		Vector3[] verticesModify = gameMesh.vertices;
		
// 		int[] triangles = gameMesh.triangles;
// 		Vector2[] uvs = gameMesh.uv;

// 		for (int i = 0; i < verticesModify.Length; i++)
// 		{
// 			verticesModify[i] = new Vector3(verticesModify[i].x, heightMap[i], verticesModify[i].z);
// 		}


// 		for (int i = 0; i < uvs.Length; i++)
// 		{
// 			uvs[i] = new Vector2 (verticesModify[i].x / TerrainMetrics.chunkSizeX, verticesModify[i].z / TerrainMetrics.chunkSizeZ);
// 		}

// 		RefreshMesh(verticesModify, triangles);
// 	}

// 	//Refresh the mesh =================================================================================================

// 	void RefreshMesh (List<Vector3> vertices, List<int> triangles) {
// 		RefreshMesh(vertices.ToArray(), triangles.ToArray());
// 	}

// 	void RefreshMesh (Vector3[] vertices, int[] triangles) {
// 		gameMesh.Clear();
// 		gameMesh.vertices = vertices;
// 		gameMesh.triangles = triangles;
// 		gameMesh.RecalculateNormals();

// 		collider.sharedMesh = gameMesh;

// 		if (terrainMesh != null)
// 		{
// 			terrainMesh.RefreshNavMesh();
// 		}
// 	}
// }
