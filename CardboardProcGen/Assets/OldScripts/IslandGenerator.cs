using UnityEngine;
using System.Collections;

public class IslandGenerator : MonoBehaviour
{
    public enum TerrainType { None, Beach, Forest, Stone };
    public static TerrainType baseTerrainType = TerrainType.Beach;

    public class TerrainTile
    {
        private TerrainType m_terrainType;
        private int m_x;
        private int m_y;
        private float m_height;
        private TerrainType m_baseType;

        public TerrainTile(TerrainType type, int x, int y, float h)
        {
            m_terrainType = type;
            m_x = x;
            m_y = y;
            m_height = h;
            m_baseType = baseTerrainType;
        }

        public TerrainType getType() { return m_terrainType; }
        public void setType( TerrainType type ) { m_terrainType = type;  }
        public void setTypeAndBase(TerrainType type) { m_baseType = m_terrainType;  m_terrainType = type; } // May need to implement as basestack later
        public TerrainType getBaseType() { return m_baseType; }
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
	public float[] typePercents;

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
        if (Input.GetMouseButtonDown(0))
        {
            GenerateIsland();
            //OnDrawGizmos();
        }
    }

    public void GenerateIsland()
    {
        // Scales the perlin noise
        // No divisioon by 0 / negative values
		perlinScale = Mathf.Max (perlinScale, 0.0001f);

        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        Debug.Log("Start Generate");
        map = new TerrainTile[width, height];

		Debug.Log ("Random Heightmap");
		RandomHeightMap(baseTerrainType);
		Debug.Log("End Random Heightmap");

		Debug.Log ("Random Fill");
		RandomFillMap(TerrainType.Forest);
		Debug.Log ("End Random Fill");


		Debug.Log ("Start Smooth");

        for (int i = 0; i < smoothIterations; i++)
        {
			SmoothMap(TerrainType.Forest);
        }
		Debug.Log ("End Smooth");

        Debug.Log("Random Fill");
        RandomFillMap(TerrainType.Stone);
        Debug.Log("End Random Fill");


        Debug.Log("Start Smooth");

        for (int i = 0; i < smoothIterations; i++)
        {
            SmoothMap(TerrainType.Stone);
        }
        Debug.Log("End Smooth");


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

    /// <summary>
    /// Sets the map's height values according to perlin noise generation in Unity
    /// </summary>
    /// <param name="defaultType"></param>
	public void RandomHeightMap(TerrainType defaultType){
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				
				float xCoord = (float)(x);
				float yCoord = (float)(y);
				float h = Mathf.PerlinNoise (xCoord / perlinScale, yCoord / perlinScale) * hscale;

				map[x, y] = new TerrainTile(defaultType, x, y, h);
			}
		}
	}

    /// <summary>
    /// Randomly assigns values to be the toFill type based on global percentage of type
    /// </summary>
    /// <param name="toFill"></param>
	public void RandomFillMap(TerrainType toFill)
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random psuedoRandom = new System.Random(seed.GetHashCode());
        for (int x = 0; x < width; x++)
        {
			for (int y = 0; y < height; y++){

                int rand = psuedoRandom.Next(0, 100);
                if (rand < typePercents[(int)toFill])
                {
					map [x, y].setTypeAndBase (toFill);
                }
            }
        }
    }

    /// <summary>
    /// Smooths the map based on 
    /// </summary>
    /// <param name="toSmooth"></param>
    void SmoothMap(TerrainType toSmooth)
    {
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				int neighbourSmoothTiles = GetSameTerrainNeighbours(map[x,y].getType(), x, y);
				if (neighbourSmoothTiles > 4) {
					// Keep the same type
				}
                else 
                {
                    int baseNeighbourSmoothTiles = GetSameTerrainNeighbours(map[x,y].getBaseType(), x, y);
                    if(baseNeighbourSmoothTiles > 4)
                    {
                        map[x, y].setType(map[x, y].getBaseType());
                    }
                    else
                    {
                        map[x, y].setType(toSmooth);
                    }
                }
			}
		}
    }

    int GetSameTerrainNeighbours(TerrainType type, int posX, int posY)
    {
        int count = 0;

        int startPosX = (posX - 1 < 0) ? posX : posX - 1;
        int startPosY = (posY - 1 < 0) ? posY : posY - 1;
        int endPosX = (posX + 1 > width - 1) ? posX : posX + 1;
        int endPosY = (posY + 1 > height - 1) ? posY : posY + 1;


        for (int x = startPosX; x <= endPosX; x++)
        {
            for (int y = startPosY; y <= endPosY; y++)
            {
				if (map[x, y].getType() == type)
                {
                    count++;
                }
            }
        }
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
