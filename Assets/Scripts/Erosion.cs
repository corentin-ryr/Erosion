using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Erosion : MonoBehaviour
{
    float[] heightMap;
    int mapSizeX;
    float quadSize;

    public void erosion (float[] heightMap, int mapSizeX, float quadSize, int iteration) {
        this.heightMap = heightMap;
        this.mapSizeX = mapSizeX;
        this.quadSize = quadSize;

        for (int i = 0; i < iteration; i++)
        {
            simulateDrop();
        }
    }
    public void simulateDrop () {

        //Create a drop of water
        Vector2 pos;
        Vector2 dir; //Normalized
        float vel; //Speed of the drop
        float water; //amount of water in the drop 
        float sediment; //amount of sediment in the drop

        pos = new Vector2 (10, 10);
        positionGizmos = new Vector3(pos.x, 5, pos.y);

        dir = gradient(pos).normalized;
        directionGizmos = new Vector3(dir.x, 0, dir.y);

    }

    private Vector2 gradient (Vector2 position) {
        int posX = Mathf.FloorToInt(position.x / quadSize);
        int posZ = Mathf.FloorToInt(position.y / quadSize);

        float u = position.x - posX; //Distance from the point on the x axis
        float v = position.y - posZ; //Distance from the point on the z axis
        
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

        float gradXf =  (getHeightFromCoordinates(posX, posZ + 1) - getHeightFromCoordinates(posX, posZ)) * (1 - v) + 
                        (getHeightFromCoordinates(posX + 1, posZ + 1) - getHeightFromCoordinates(posX, posZ + 1)) * v;
        
        float gradZf =  (getHeightFromCoordinates(posX, posZ + 1) - getHeightFromCoordinates(posX, posZ)) * (1 - u) + 
                        (getHeightFromCoordinates(posX + 1, posZ + 1) - getHeightFromCoordinates(posX + 1, posZ)) * u;
                        
        return new Vector2(gradXf, gradZf);


    }

    private float getHeightFromCoordinates (int x, int y) {
        return heightMap[x * mapSizeX + y];
    }

    //Debug
    Vector3 positionGizmos;
    Vector3 directionGizmos;
    public void OnDrawGizmos () {
        Debug.Log("draw gizmos");
        Debug.DrawLine(positionGizmos, positionGizmos + directionGizmos);
    }

}
