using UnityEngine;
using System.Collections;

public class IslandGenerator : MonoBehaviour
{

    public enum TerrainType { None, Beach, Forest, Stone };

    public class TerrainTile
    {
        private TerrainType m_terrainType;
        private int m_x;
        private int m_y;
        private float m_height;

        public TerrainTile(TerrainType type, int x, int y, float h)
        {
            m_terrainType = type;
            m_x = x;
            m_y = y;
            m_height = h;
        }

        public TerrainType getType() { return m_terrainType; }
        public void setType(TerrainType type) { m_terrainType = type; }
        public int getX() { return m_x; }
        public int getY() { return m_y; }
        public float getHeight() { return m_height; }
    }

    // Height and width of the island
    public int width;
    public int height;

	public float perlinScale = 5f;
	public float hscale;

    // Seed for generations
    public string seed;
    public bool useRandomSeed;

    // Number of iterations to use to smooth the island's shape
    public int smoothIterations = 5;

    // Border of the beach (may not use this)
    public int borderSize = 1;

    // chance for each type of terrain, must add up to 100%, the forest terrain will default fill the rest if under 100
    public float beachPercent = 20f;
    public float stonePercent = 20f;
    public float forestPercent = 60f;

    public float repeatPercent = 100f;

    TerrainTile[,] map;

    Vector3 startingPos;

    private System.Random psuedoRandom;

    // Use this for initialization
    void Start()
    {
        GenerateIsland();
    }

    // Update is called once per frame
    void Update()
    {
        // Make a new Island every time middle mouse is pressed
        if (Input.GetMouseButtonDown(2))
        {
            GenerateIsland();
            //OnDrawGizmos();
        }
    }

    public void GenerateIsland()
    {
		perlinScale = Mathf.Max (perlinScale, 0.0001f);

        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        Debug.Log("Start Generate");
        map = new TerrainTile[width, height];

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
				float xCoord = (float)(x);
				float yCoord = (float)(y);
                map[x, y] = new TerrainTile(TerrainType.Forest, x, y, Mathf.PerlinNoise(xCoord/perlinScale, yCoord/perlinScale) * hscale);
				if (x % 10 == 0) {
					Debug.Log ("X: " + xCoord / perlinScale + "Y: " + yCoord / perlinScale);
					Debug.Log (map [x, y].getHeight ());
				}
            }
        }
        //Debug.Log(map[0, 0] == null);

        //Debug.Log(map);
        //CrawlingGeneration(TerrainType.Beach, 0, 0);
        //Debug.Log(map);

        //RandomFillMap();

        /*for (int i = 0; i < smoothIterations; i++)
        {
            SmoothMap();
        }*/

        /*TerrainTile[,] borderedMap = new TerrainTile[width + borderSize * 2, height + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                int h = 0;
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                {
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                }
                else
                {
                    borderedMap[x, y] = new TerrainTile(TerrainType.Beach, x, y, h);
                }
            }
        }
        map = borderedMap;*/
        Debug.Log("End Generate");
    }

    public void CrawlingGeneration(TerrainType type, int indX, int indY)
    {
        if (map[indX, indY] != null)
        {
            return;
        }

        int rand = psuedoRandom.Next(0, 100);
        int h = 0;
        if ( rand < repeatPercent)
        {
            map[indX, indY] = new TerrainTile(type, indX, indY, h);
        }
        else
        {
            if (rand < beachPercent)
            {
                map[indX, indY] = new TerrainTile(TerrainType.Beach, indX, indY, h);
            }
            else if (rand < beachPercent + stonePercent)
            {
                map[indX, indY] = new TerrainTile(TerrainType.Stone, indX, indY, h);
            }
            else
            {
                map[indX, indY] = new TerrainTile(TerrainType.Forest, indX, indY, h);
            }
        }
        

        // Repeat on neighbours
        int startPosX = (indX - 1 < 0) ? indX : indX - 1;
        int startPosY = (indY - 1 < 0) ? indY : indY - 1;
        int endPosX = (indX + 1 > width - 1) ? indX : indX + 1;
        int endPosY = (indY + 1 > height - 1) ? indY : indY + 1;
        for(int x = startPosX; x <= endPosX; x++)
        {
            for( int y = startPosY; y <= endPosY; y++)
            {
                if (!(x == indX && y == indY))
                {
                    CrawlingGeneration(map[indX, indY].getType(), x, y);
                }
            }
        }
    }


    public void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random psuedoRandom = new System.Random(seed.GetHashCode());
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Generate Random Height (using 0 to start)
                int h = 0;
                int rand = psuedoRandom.Next(0, 100);
                if (rand < beachPercent)
                {
                    map[x, y] = new TerrainTile(TerrainType.Beach, x, y, h);
                }
                /*else if (rand < beachPercent + stonePercent)
                {
                    map[x, y] = new TerrainTile(TerrainType.Stone, x, y, h);
                }*/
                else
                {
                    map[x, y] = new TerrainTile(TerrainType.Forest, x, y, h);
                }
            }
        }
    }

    void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float neighboursBeach = GetSameTerrainNeighbours(TerrainType.Beach, x, y);
                float neighboursStone = GetSameTerrainNeighbours(TerrainType.Stone, x, y);
                float neighboursForest = GetSameTerrainNeighbours(TerrainType.Forest, x, y);
                //Debug.Log("beachn: " + neighboursBeach);
                //Debug.Log("Stonen: " + neighboursStone);
                //Debug.Log("Forestn: " + neighboursForest);

                float totalNeighbours = neighboursBeach + neighboursStone + neighboursForest;

                //Get same terrain checks within 2 blocks of neighbours (24 max)
                /*
                *   O O O O O
                *   O O O O O
                *   O O X O O
                *   O O O O O
                *   O O O O O
                */
                if (neighboursForest > 4f)//(totalNeighbours * ((100 - forestPercent )  / 100f)))
                {
                    //Debug.Log("Forest");
                    map[x, y].setType(TerrainType.Forest);
                }
                else if (neighboursBeach > 4f)//( totalNeighbours * ((100-beachPercent) / 100f )) )
                {
                    //Debug.Log("Beach");
                    map[x, y].setType(TerrainType.Beach);
                }
                /*else if (neighboursStone > 2f)//(totalNeighbours * ((100-stonePercent) / 100f )) )
                {
                    //Debug.Log("Stone");
                    map[x, y].setType(TerrainType.Stone);
                }*/
            }
        }
    }

    int GetSameTerrainNeighbours(TerrainType type, int posX, int posY)
    {
        int count = 0;
        /*
        int startPosX = (posX - 2 < 0) ? ((posX - 1 < 0) ? posX : posX - 1) : posX - 2;
        int startPosY = (posY - 2 < 0) ? ((posY - 1 < 0) ? posY : posY - 1) : posY - 2;
        int endPosX = (posX + 2 > width - 1) ? ((posX + 1 > width - 1) ? posX : posX + 1 ) : posX + 2;
        int endPosY = (posY + 2 > height - 1) ? ((posY + 1 > height - 1) ? posY : posY + 1) : posY + 2;
        //Debug.Log("StartPosX: " + startPosX);
        //Debug.Log("EndPosX: " + endPosX);
        //Debug.Log("StartPosY: " + startPosY);
        //Debug.Log("EndPosY: " + endPosY);
        */

        int startPosX = (posX - 1 < 0) ? posX : posX - 1;
        int startPosY = (posY - 1 < 0) ? posY : posY - 1;
        int endPosX = (posX + 1 > width - 1) ? posX : posX + 1;
        int endPosY = (posY + 1 > height - 1) ? posY : posY + 1;

        int loopCount = 0;

        for (int x = startPosX; x <= endPosX; x++)
        {
            for (int y = startPosY; y <= endPosY; y++)
            {
                if (posX != x && posY != y && map[x, y].getType() == type)
                {
                    count++;
                }
                loopCount++;
            }
        }
        //Debug.Log("LoopCount: " + loopCount);
        //Debug.Log("COUNT: " + count);
        return count;
    }

    void OnDrawGizmos()
    {
        if (map != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    switch (map[x, y].getType())
                    {
                        case TerrainType.Beach:
                            Gizmos.color = Color.yellow;
                            break;
                        case TerrainType.Stone:
                            Gizmos.color = Color.grey;
                            break;
                        case TerrainType.Forest:
                            Gizmos.color = Color.green;
                            break;
                        default:
                            Gizmos.color = Color.white;
                            break;

                    }

                    Vector3 pos = new Vector3(-width / 2 + x + .5f, map[x,y].getHeight() , -height / 2 + y + .5f);
                    
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }
}
