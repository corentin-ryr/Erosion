using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Erosion : MonoBehaviour
{
    //References and parameters of the map
    float[] heightMap;
    int mapSizeX;
    float quadSize;

    //Parameters of the simulation
    [Header("Parameters of the simulation")]
    public float pinertia = 0.5f; //How much should the old direction be taken into acount
    public int iterationDrop = 30;

    public void OnEnable () {
        positionGizmos = new Vector3[iterationDrop];
        directionGizmos = new Vector3[iterationDrop];
    }

    public void erosion (float[] heightMap, int mapSizeX, float quadSize, int iteration) {
        this.heightMap = heightMap;
        this.mapSizeX = mapSizeX;
        this.quadSize = quadSize;

        for (int i = 0; i < iteration; i++)
        {
            StartCoroutine(simulateDrop());
        }
    }
    IEnumerator simulateDrop () {

        //Create a drop of water
        Vector2 pos;
        Vector2 dir; //Normalized
        float vel; //Speed of the drop
        float water; //amount of water in the drop 
        float sediment; //amount of sediment in the drop

        //Initialize variables
        pos = new Vector2 (57.1f, 30.3f);
        dir = new Vector2 (0, 0);

        for (int i = 0; i < iterationDrop; i++)
        {
            //Debug
            positionGizmos[i] = new Vector3(pos.x, getHeightFromCornerVertex(pos) + 0.5f , pos.y);

            Vector2 g = gradient(pos);

            //Debug
            directionGizmos[i] = new Vector3(g.x, 0, g.y);

            dir = dir * pinertia - g * (1 - pinertia);

            if (dir == Vector2.zero)
            {
                dir = new Vector2 (Random.Range(0, 1), Random.Range(0, 1));
            }
            dir = dir.normalized;

            float hold = getHeightFromCornerVertex(pos);
            //Update the position
            pos = pos + dir; //Always move by one unit

            //Calculate height difference
            float hdif = getHeightFromCornerVertex(pos) - hold;

            Debug.Log("position" + pos);
            yield return new WaitForSeconds(1);
        }
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

    //Debug
    Vector3[] positionGizmos;
    Vector3[] directionGizmos;
    public void OnDrawGizmos () {
        for (int i = 0; i < positionGizmos.Length; i++)
        {
            Debug.DrawLine(positionGizmos[i], positionGizmos[i] + directionGizmos[i]);
            Gizmos.color = Color.Lerp(Color.blue, Color.red, (float)i / 10f);
            Gizmos.DrawSphere(positionGizmos[i], 0.1f);
        }
        
    }

}
