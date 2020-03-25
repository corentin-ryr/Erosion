using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Erosion : MonoBehaviour
{
    //References and parameters of the map
    private int mapSizeX;
    private int mapSizeZ;
    private float quadSize;
    private float[] heightMap;

    public TerrainMesh terrain;
    

    //Parameters of the simulation
    [Header("Parameters of the simulation")]
    public float pinertia = 0.5f; //How much should the old direction be taken into acount
    public int iterationDrop = 75;
    public float pminCapacity = 0.01f;//TODO filler value
    public float pmaxCapacity = 20f;//TODO filler value
    public float pcapacity = 8; //TODO filler value
    public float pdeposition = 0.1f; //TODO filler value
    public float perosion = 0.1f; //TODO filler value
    public float pgravity = 9.81f; //TODO filler value
    public float pevaporation = 0.05f; //TODO filler value
    public float pradius = 2;

    public void OnEnable () {
        positionGizmos = new Vector3[iterationDrop];
        directionGizmos = new Vector3[iterationDrop];
    }

    public void erosion (float[] originalHeightMap, int mapSizeX, float quadSize, int iteration) {
        this.heightMap = originalHeightMap;
        this.mapSizeX = mapSizeX;
        this.mapSizeZ = originalHeightMap.Length / mapSizeX;
        this.quadSize = quadSize;
        

        for (int i = 0; i < iteration; i++)
        {
            simulateDrop();

            //Debug
            terrain.UpdateMeshWithHeightMap();
            //yield return new WaitForSeconds(0.1f);
        }
    }

    public float[] getHeightMap () {
        return heightMap;
    }
    private void simulateDrop () {

        //Create a drop of water
        Vector2 pos;
        Vector2 dir; //Normalized
        float vel = 1; //Speed of the drop
        float water = 1; //amount of water in the drop 
        float sediment = 0; //amount of sediment in the drop
        float capacity = 0;

        //Initialize variables
        float posX = Random.Range(0, (mapSizeX - 1) * quadSize);
        float posZ = Random.Range(0, (mapSizeZ - 1) * quadSize);
        pos = new Vector2 (posX, posZ);
        dir = new Vector2 (0, 0);

        for (int i = 0; i < iterationDrop; i++)
        {
            //Debug
            positionGizmos[i] = new Vector3(pos.x, getHeightFromCornerVertex(pos), pos.y);

            Vector2 g = gradient(pos);

            //Debug
            directionGizmos[i] = new Vector3(g.x, 0, g.y);

            dir = dir * pinertia - g * (1 - pinertia);

            if (dir == Vector2.zero)
            {
                dir = new Vector2 (UnityEngine.Random.Range(0, 1), UnityEngine.Random.Range(0, 1));
            }
            dir = dir.normalized * quadSize;

            float hold = getHeightFromCornerVertex(pos);
            
            Vector2 posOld = pos;
            //Update the position
            pos = pos + dir; //Always move by one unit
            if (!checkValidPosition(pos) || float.IsNaN(vel)) {
                break;
            }

            //Calculate height difference
            float hdif = getHeightFromCornerVertex(pos) - hold;
            capacity = Mathf.Min(Mathf.Max(-hdif * vel * water * pcapacity, pminCapacity), pmaxCapacity);

            float dSediment; //Amount to erode or the deposit

            if (hdif > 0)
            {
                float amountToDeposit = Mathf.Min(sediment, hdif);
                depositSediment(posOld, amountToDeposit);
                sediment -= amountToDeposit;
                vel = 0;
            }
            else
            {

                if (sediment > capacity)
                {
                    dSediment = Mathf.Min(-hdif, sediment - capacity) * pdeposition;
                    sediment -= dSediment;
                    depositSediment(posOld, dSediment);
                }
                else
                {
                    dSediment = Mathf.Min((capacity - sediment) * perosion, -hdif); //Le 0.99 ajouté du papier
                    sediment += dSediment;
                    erodeRadius(posOld, dSediment);

                    //hdif += dSediment;
                }
            }

            //Update of the speed and the amount of water in the drop
            vel = Mathf.Sqrt(vel * vel - (hdif + 0.15f) * pgravity);

            water = water * (1 - pevaporation);

            // Debug.Log("<color=red>End of a turn : </color>");
            // Debug.Log("hdif : " + hdif);
            // Debug.Log("vel : " + vel);
            // Debug.Log("capacity : " + capacity);
            // terrain.UpdateMeshWithHeightMap();
            // yield return waitForKeyPress(KeyCode.RightArrow);

        }
    }

    private IEnumerator waitForKeyPress(KeyCode key)
    {
        bool done = false;
        while(!done) // essentially a "while true", but with a bool to break out naturally
        {
            if(Input.GetKeyDown(key))
            {
                done = true; // breaks the loop
            }
            yield return null; // wait until next frame, then continue execution from here (loop continues)
        }
    
        // now this function returns
    }

    private bool checkValidPosition(Vector2 position)
    {
        if (position.x < 0 || position.x > (mapSizeX - 1) * quadSize ||
            position.y < 0 || position.y > (mapSizeZ - 1) * quadSize)
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

    //Erode in e circle around the drop :
    private void erodeRadius (Vector2 position, float amountToErode) {
        

        float[] weightArray = new float[heightMap.Length];

        bool oneCellInRadius = true;
        int directionIndex = 0;
        int nbStep = 1;
        float sum = 0; //Sum of all weights

        int index = getCornerIndex(position);
        sum += weightArray[index] = calculateWeight(position, index); //Computation of the first cell

        while (oneCellInRadius) //While we accessed (ie. one cell is within radius) at least one cell during last lap, we continue
        {
            bool allCellUnreachable = true;
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < nbStep; i++)
                {
                    index = moveWithDirection(directionIndex, index);
                    float weight = calculateWeight(position, index);
                    if (index < 0 || index > weightArray.Length)
                    {
                        sum += weight;
                        continue;
                    }
                    
                    sum += weightArray[index] = weight;
                    if (weight != 0)
                    {
                        allCellUnreachable = false;
                    }
                }

                //Change direction
                directionIndex = (directionIndex + 1) % 4;

                for (int i = 0; i < nbStep; i++)
                {
                    index = moveWithDirection(directionIndex, index);
                    float weight = calculateWeight(position, index);
                    if (index < 0 || index > weightArray.Length)
                    {
                        sum += weight;
                        continue;
                    }
                    
                    sum += weightArray[index] = weight;
                    if (weight != 0)
                    {
                        allCellUnreachable = false;
                    }
                }

                //Change direction (we did half of a turn)
                directionIndex = (directionIndex + 1) % 4;
                nbStep++; //After two sides of the spiral, the number of cell to move is inceased by one
            }

            if (allCellUnreachable)
            {
                oneCellInRadius = false;
            }
            
        }
        //createVisualizer(weightArray, mapSizeX, quadSize);

        //Apply erosion to the heightMap
        for (int i = 0; i < heightMap.Length; i++)
        {
            heightMap[i] -= weightArray[i] / sum * amountToErode;
        }
    }

    private float calculateWeight (Vector2 center, int index) {
        Vector2 worldCoord = getWorldCoord(index);
        float distanceToCenter = (getWorldCoord(index) - center).magnitude;
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

    //Deposit soil :
    private void depositSediment (Vector2 position, float amountToDeposit) {
        int posX = Mathf.FloorToInt(position.x / quadSize);
        int posZ = Mathf.FloorToInt(position.y / quadSize);

        float u = (position.x - posX * quadSize) / quadSize; //Distance from the point on the x axis
        float v = (position.y - posZ * quadSize) / quadSize; //Distance from the point on the z axis 

        float weight00 = (1 - u) * (1 - v);
        float weight01 = (1 - u) * v;
        float weight10 = u * (1 - v);
        float weight11 = u * v ;

        heightMap[posZ * mapSizeX + posX] += weight00 * amountToDeposit;
        heightMap[posZ * mapSizeX + posX + 1] += weight10 * amountToDeposit;
        heightMap[(posZ + 1) * mapSizeX + posX] += weight01 * amountToDeposit;
        heightMap[(posZ + 1) * mapSizeX + posX + 1] += weight11 * amountToDeposit;

    }

    //calculate height from position
    
    private float getHeightFromCoordinates (int x, int z) { 
        return heightMap[z * mapSizeX + x];
    }
        
    
    private float getHeightFromCornerVertex (float x, float z) {
        int posX = Mathf.FloorToInt(x / quadSize);
        int posZ = Mathf.FloorToInt(z / quadSize);

        float u = (x - posX * quadSize) / quadSize; //Distance from the point on the x axis
        float v = (z - posZ * quadSize) / quadSize; //Distance from the point on the z axis 

        if (posX < 0 || posX > mapSizeX || posZ < 0 || posZ > mapSizeZ)
        {
            return float.NaN;
        }
        int index = posZ * mapSizeX + posX;
        float height00 = heightMap[index];
        float height10 = heightMap[index + 1];
        float height01 = heightMap[index + mapSizeX];
        float height11 = heightMap[index + mapSizeX + 1];

        float height0 =  height00 * (1 - u) + height10 * u;
        float height1 =  height01 * (1 - u) + height11 * u;

        return height0 * (1 - v) + height1 * v;

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
            Gizmos.color = Color.Lerp(Color.blue, Color.red, (float)i / iterationDrop);
            Gizmos.DrawSphere(positionGizmos[i], 0.1f);
        }
        
    }
    #endregion

}
