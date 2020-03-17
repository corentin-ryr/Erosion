using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightMapGenerator
{
    public float frequency = 1f;
    [Range(1, 8)]
    public int octaves = 4;
    public float lacunarity = 2f;
    public float persistence = 0.5f;
    public float amplitude = 1f;

    //seed
    public bool randomizeSeed;
    public int seed;

    //Generer la heightMap ============================================================================================
    
    //Genere la heightmap a partir du perlinnoise et du gradient carré
    public float[] GenerateHeightMap (int sizeX, int sizeZ) {
        float[] noise = GenerateNoise(sizeX, sizeZ);
        float[] gradient = SquareGradient(sizeX, sizeZ);

        float[] result = new float[sizeZ * sizeX];

        for (int i = 0; i < sizeZ; i++)
        {
            for (int j = 0; j < sizeX; j++)
            {
                result[i * sizeX + j] = (noise[i * sizeX + j] - gradient[i * sizeX + j]) * amplitude;
            }
            
        }
        //renderer.material.mainTexture = GenerateTexture();
        return result;
    }

    //Genere le perlinNoise
    private float[] GenerateNoise (int width, int height) //Crée un tableau avec [lignes, colonnes]
    {

        seed = (randomizeSeed) ? Random.Range (-10000, 10000) : seed;
        var prng = new System.Random (seed);

        Vector2[] offsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++) {
            offsets[i] = new Vector2 (prng.Next (-1000, 1000), prng.Next (-1000, 1000));
        }


        float[] array = new float[height * width];
        for (int t = 0, i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                float x = (float)j / width;
                float y = (float)i / height;
                float sample = (perlinNoise.Sum(perlinNoise.valueMethod[1],new Vector3(x, y, 0f), frequency, octaves, lacunarity, persistence) + 0.5f) * 0.5f;
                array[i * width + j] = sample;
                t++;
            }
        }
        return array;
    }

    //Genere le gradient carré
    private float[] SquareGradient (int width, int height) {
        float[] array = new float[height * width];
        Vector2 middle = new Vector2((float)width/2f, (float)height/2f);

        for (int t = 0, i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                float x = Mathf.Abs((float)j - middle.x) / width;
                float y = Mathf.Abs((float)i - middle.y) / height;
                float coord = Mathf.Max(x, y);
                array[i * width + j] = Mathf.Lerp(0, 1, coord);
                t++;
            }
        }
        return array;
    }

    

    //Visualization function =============================================================================================

    /*Texture2D GenerateTexture()
    {
        texture = new Texture2D(Mathf.FloorToInt(Mathf.Sqrt(heightMap.Length)), Mathf.FloorToInt(Mathf.Sqrt(heightMap.Length)));

        Color[] colors = FloatToColor(heightMap);

        texture.SetPixels(colors);

        texture.Apply();
        return texture;
    }

    Color[] FloatToColor (float[,] array) {
        Color[] colors = new Color[array.GetLength(0) * array.GetLength(1)];
        for (int t = 0, i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                colors[t] = new Color(array[i, j], array[i, j], array[i, j]);
                t++;
            }
        }
        return colors;
    }

    void OnValidate () {
        if (heightMap != null)
        {
            GenerateHeightMap(heightMap.GetLength(0), heightMap.GetLength(1));
        }
    }*/
}
