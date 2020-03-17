using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Erosion : MonoBehaviour
{
    //References and parameters of the map
    float[] heightMap;
    int mapSizeX;
    int mapSizeZ;
    float quadSize;

    //Parameters of the simulation
    [Header("Parameters of the simulation")]
    public float pinertia = 0.5f; //How much should the old direction be taken into acount
    public int iterationDrop = 30;
    public float pminslope = 0.5f;//TODO filler value
    public float pcapacity = 1; //TODO filler value
    public float pdeposition = 0.5f; //TODO filler value
    public float perosion = 0.5f; //TODO filler value
    public float pgravity = 9.81f; //TODO filler value
    public float pevaporation = 0.1f; //TODO filler value
    public float pradius = 2;

    public void OnEnable () {
        positionGizmos = new Vector3[iterationDrop];
        directionGizmos = new Vector3[iterationDrop];
    }

    public void erosion (float[] heightMap, int mapSizeX, float quadSize, int iteration) {
        this.heightMap = heightMap;
        this.mapSizeX = mapSizeX;
        this.mapSizeZ = heightMap.Length / mapSizeX;
        this.quadSize = quadSize;

        erodeRadius(new Vector2(57.1f, 30.3f), 10);

        for (int i = 0; i < iteration; i++)
        {
            StartCoroutine(simulateDrop());
        }
    }
    IEnumerator simulateDrop () {

        //Create a drop of water
        Vector2 pos;
        Vector2 dir; //Normalized
        float vel = 0; //Speed of the drop
        float water = 0; //amount of water in the drop 
        float sediment = 0; //amount of sediment in the drop
        float capacity = 0;

        //Initialize variables
        pos = new Vector2 (57.1f, 30.3f);
        dir = new Vector2 (0, 0);

        for (int i = 0; i < iterationDrop; i++)
        {
            //Debug
            positionGizmos[i] = new Vector3(pos.x, getHeightFromCornerVertex(pos) + 0.25f , pos.y);

            Vector2 g = gradient(pos);

            //Debug
            directionGizmos[i] = new Vector3(g.x, 0, g.y);

            dir = dir * pinertia - g * (1 - pinertia);

            if (dir == Vector2.zero)
            {
                dir = new Vector2 (UnityEngine.Random.Range(0, 1), UnityEngine.Random.Range(0, 1));
            }
            dir = dir.normalized;

            float hold = getHeightFromCornerVertex(pos);
            
            //Update the position
            pos = pos + dir; //Always move by one unit
            if (!checkValidPosition(pos)) {
                break;
            }

            //Calculate height difference
            float hdif = getHeightFromCornerVertex(pos) - hold;

            if (hdif > 0)
            {
                //TODO deposit sediment
            }
            else
            {
                capacity = Mathf.Max(-hdif, pminslope) * vel * water * pcapacity;

                if (sediment > capacity)
                {
                    float amountToDeposit = (sediment - capacity) * pdeposition;
                    //TODO deposit sediment
                }
                else
                {
                    float amountToErode = Mathf.Min((capacity - sediment) * perosion, -hdif);
                    //TODO deposit sediment
                }
            }

            //Update of the speed and the amount of water in the drop
            vel = Mathf.Sqrt(vel * vel + hdif * pgravity);

            water = water * (1 - pevaporation);

            Debug.Log("position" + pos);
            yield return new WaitForSeconds(1);
        }
    }

    private bool checkValidPosition(Vector2 position)
    {
        if (position.x < 0 || position.x > mapSizeX * quadSize ||
            position.y < 0 || position.y >mapSizeZ * quadSize)
        {
            return false;
        }

        return true;
    }

    private Vector2 gradient (Vector2 position) {
        int posX = Mathf.FloorToInt(position.x / quadSize);
        int posZ = Mathf.FloorToInt(position.y / quadSize);

        float u = position.x - (int)position.x; //Distance from the point on the x axis
        float v = position.y - (int)position.y; //Distance from the point on the z axis
        
        Debug.Log("u et v :" + u + " " + v);
        /*
        //Grad in x, z
        float gradX = getHeightFromCoordinates(posX + 1, posZ) - getHeightFromCoordinates(posX, posZ);
        float gradZ = getHeightFromCoordinates(posX, posZ + 1) - getHeightFromCoordinates(posX, posZ);

        Vector2 gradXZ = new Vector2(gradX, gradZ);
        //Grad in x + 1, z
        gradX = getHeightFromCoordinates(posX + 1, posZ) - getHeightFromCoordinates(posX, posZ);
        gradZ = getHeightFromCoordinates(posX + 1, posZ + 1) - getHeightFromCoordinates(posX + 1, posZ);

        Vector2 gradX1Z = new Vector2(gradX, gradZ);

        //Grad in x, z + 1
        gradX = getHeightFromCoordinates(posX + 1, posZ + 1) - getHeightFromCoordinates(posX, posZ + 1);
        gradZ = getHeightFromCoordinates(posX, posZ + 1) - getHeightFromCoordinates(posX, posZ);

        Vector2 gradXZ1 = new Vector2(gradX, gradZ);

        //Grad in x + 1, z + 1
        gradX = getHeightFromCoordinates(posX + 1, posZ + 1) - getHeightFromCoordinates(posX, posZ + 1);
        gradZ = getHeightFromCoordinates(posX + 1, posZ + 1) - getHeightFromCoordinates(posX + 1, posZ);

        Vector2 gradX1Z1 = new Vector2(gradX, gradZ);*/

        //TODO check if the value must be divide by the size of the cell
        float gradXf =  (getHeightFromCoordinates(posX + 1, posZ) - getHeightFromCoordinates(posX, posZ)) * (1 - v) + 
                        (getHeightFromCoordinates(posX + 1, posZ + 1) - getHeightFromCoordinates(posX, posZ + 1)) * v;
        
        float gradZf =  (getHeightFromCoordinates(posX, posZ + 1) - getHeightFromCoordinates(posX, posZ)) * (1 - u) + 
                        (getHeightFromCoordinates(posX + 1, posZ + 1) - getHeightFromCoordinates(posX + 1, posZ)) * u;
                        
        return new Vector2(gradXf, gradZf);

    }

    Vector2Int[] directions = new Vector2Int[] {
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(0, -1)
    };

    private void erodeRadius (Vector2 position, float amountToErode) {
        

        float[] weightArray = new float[heightMap.Length];

        bool oneCellInRadius = true;
        int directionIndex = 0;
        int nbStep = 1;
        float sum = 0; //Sum of all weights

        int index = getCornerIndex(position);
        weightArray[index] = calculateWeight(position, index); //Computation of the first cell

        while (oneCellInRadius) //While we accessed (ie. one cell is within radius) at least one cell during last lap, we continue
        {
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < nbStep; i++)
                {
                    index = moveWithDirection(directionIndex, index);
                    weightArray[index] = calculateWeight(position, index);
                }
                //Change direction
                directionIndex = (directionIndex + 1) / 3;

                for (int i = 0; i < nbStep; i++)
                {
                    index = moveWithDirection(directionIndex, index);
                    weightArray[index] = calculateWeight(position, index);
                }

                //Change direction (we did half of a turn)
                directionIndex = (directionIndex + 1) / 3;
                nbStep++; //After two sides of the spiral, the number of cell to move is inceased by one
            }

            if (nbStep > 4)
            {
                oneCellInRadius = false;
            }
            
        }

        createVisualizer(weightArray, mapSizeX, quadSize);
    }

    private float calculateWeight (Vector2 center, int index) {
        Vector2 worldCoord = getWorldCoord(index);
        float distanceToCenter = Mathf.Sqrt((getWorldCoord(index) - center).magnitude);
        return Mathf.Max(0, pradius - distanceToCenter);
    }

    private int moveWithDirection (int direction, int index) {
        Vector2 vectDir = directions[direction];

        if (vectDir.x == 1)
        {
            index++;
        }
        if (vectDir.y == 1)
        {
            index = index + mapSizeX;
        }

        if (vectDir.x == -1)
        {
            index--;
        }
        if (vectDir.y == -1)
        {
            index = index - mapSizeX;
        }
        //TODO when index is out of bounds
        return index;
    }

    //calculate height from position
    private float getHeightFromCoordinates (int x, int z) {
        return heightMap[z * mapSizeX + x];
    }
    private float getHeightFromCornerVertex (float x, float z) {
        int posX = Mathf.FloorToInt(x / quadSize);
        int posZ = Mathf.FloorToInt(z / quadSize);
        return getHeightFromCoordinates(posX, posZ);
    }
    private float getHeightFromCornerVertex (Vector2 position) {
        return getHeightFromCornerVertex(position.x, position.y);
    }

    private int getCornerIndex (Vector2 position) {
        int posX = Mathf.FloorToInt(position.x / quadSize);
        int posZ = Mathf.FloorToInt(position.y / quadSize);
        return posZ * mapSizeX + posX;
    }

    private Vector2 getWorldCoord (int index) {
        int posX = index % mapSizeX;
        int posZ = index / mapSizeX;
        return new Vector2 (posX * quadSize, posZ * quadSize);
    }

    #region Debug
    Vector3[] positionGizmos;
    Vector3[] directionGizmos;

    [Header("Debug")]
    public ArrayVisualizer visualizerPrefab;

    ArrayVisualizer visualizer;

    private void createVisualizer (float[] heightMap, int mapSizeX, float quadSize) {
        Debug.Log((1 / quadSize));
        
        if (visualizer != null)
        {
            Destroy(visualizer.gameObject);
        }
        visualizer = Instantiate(visualizerPrefab, new Vector3(0, 10, 0), Quaternion.identity);
        visualizer.transform.SetParent(transform);
        visualizer.CreateMesh(heightMap, mapSizeX, quadSize);
    }
    public void OnDrawGizmos () {
        if (positionGizmos == null || directionGizmos == null)
        {
            return;
        }

        for (int i = 0; i < positionGizmos.Length; i++)
        {
            Debug.DrawLine(positionGizmos[i], positionGizmos[i] + directionGizmos[i]);
            Gizmos.color = Color.Lerp(Color.blue, Color.red, (float)i / 10f);
            Gizmos.DrawSphere(positionGizmos[i], 0.1f);
        }
        
    }
    #endregion

}
