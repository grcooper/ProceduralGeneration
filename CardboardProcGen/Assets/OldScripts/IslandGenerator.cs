using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IslandGenerator : MonoBehaviour
{
    public MeshFilter landMF;

    public enum TerrainType { None, Beach, Forest, Stone };
    public static TerrainType baseTerrainType = TerrainType.Beach;

    public class TerrainTile
    {
        private TerrainType m_terrainType;
        private int m_x;
        private int m_y;
        private int index;
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
        public void setHeight( float h ) { m_height = h; }
        public int getIndex() { return index;  }
        public void setIndex(int ind) { index = ind; }
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

    private float waterH;
    private int xPlayerStart;
    private int yPlayerStart;

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

    Vector3 CoordToWorldPoint(int x, int y)
    {
        Vector3 pos = new Vector3(-width / 2 + (x * 32) + .5f, map[x, y].getHeight(), -height / 2 + (y * 32) + .5f);
        return pos;
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
        System.Random psuedoRandom = new System.Random(seed.GetHashCode());

        

        Debug.Log("Start Generate");
        map = new TerrainTile[width, height];

		Debug.Log ("Random Heightmap");
		RandomHeightMap(baseTerrainType);
		Debug.Log("End Random Heightmap");

        waterH = ((float)psuedoRandom.Next(0, (int)hscale) * 0.2f) + 0.1f;
        MovePlane(waterH);

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

        // Post processing beach and stone height sharpening
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float distToPoint = Vector3.Distance(new Vector3(x,y,0), new Vector3(width/2,height/2,0));
                float distToCorner = Vector3.Distance(new Vector3(width/2,height/2,0), new Vector3(0,0,0));

                if (width/2 > distToPoint + borderSize)
                {
                }
                else
                {
                    if (map[x, y].getType() == TerrainType.Forest)
                    {
                        float newRand = (float)psuedoRandom.Next(0, 4) * (Mathf.Min(height - y, y, width - x, x) / (float)borderSize);

                        if (newRand <= 2)
                        {
                            map[x, y].setType(TerrainType.Beach);
                        }
                    }
                }

                if(map[x,y].getType() == TerrainType.Stone)
                {
                    float newHScale = (((float)psuedoRandom.Next(90, 120))/100f);

                    map[x, y].setHeight(map[x, y].getHeight() * newHScale);
                }
                else
                {
                    map[x, y].setHeight(map[x, y].getHeight() * 0.7f);
                }
            }
        }

        GenerateMesh();
        bool foundStart = false;
        for(int x = 0; x < width && !foundStart; x++)
        {
            for(int y = 0; y < height && !foundStart; y++)
            {
                if (map[x,y].getHeight() > waterH + 1f)
                {
                    xPlayerStart = x;
                    yPlayerStart = y;
                    Debug.Log("XYPLZ: " + x + " " + y);
                    foundStart = true;
                }
            }
            
        }
        SetPlayerPos(CoordToWorldPoint(xPlayerStart, yPlayerStart));

        Debug.Log("End Generate");
    }

    void SetPlayerPos(Vector3 pos)
    {
        Vector3 newPos = pos;
        newPos.y = pos.y + 3;
        GameObject player = GameObject.Find("CardboardMain");
        if (player)
        {
            if (pos != null)
            {
                player.transform.position = newPos;
                Debug.Log("Player Pos: " + newPos);
            }
        }
    }

    private void MovePlane(float h)
    {
        GameObject plane = GameObject.Find("Water");
        Vector3 pos = CoordToWorldPoint(width / 2, height / 2);
        pos.y = h;
        
        plane.transform.position = pos;
    }

    /// <summary>
    /// Sets the map's height values according to perlin noise generation in Unity
    /// </summary>
    /// <param name="defaultType"></param>
	public void RandomHeightMap(TerrainType defaultType){
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }
        System.Random psuedoRandom = new System.Random(seed.GetHashCode());
        for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
                float xCoord = (float)(x);
				float yCoord = (float)(y);
                float h = Mathf.PerlinNoise(xCoord / perlinScale, yCoord / perlinScale);
                int tempBorder = borderSize + psuedoRandom.Next(borderSize / -2, borderSize / 2);
                float distToPoint = Vector3.Distance(new Vector3(x, y, 0), new Vector3(width / 2, height / 2, 0));

                if (width/2 - tempBorder > distToPoint )
                {
                    h = h * hscale;
                }
                else
                {
                    h = h * (((float)width / 2f - distToPoint)/ borderSize) * hscale;
                }
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

    void GenerateMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List < Color > colors = new List<Color>();

        const int NUMSANDCOLORS = 4;
        List<Color> sandColors = new List<Color>();
        sandColors.Add(new Color(194f/255f, 178f/255f, 128f/255f));
        sandColors.Add(new Color(193f/255f, 154f/255f, 107f/255f));
        sandColors.Add(new Color(150f/255f, 113f/255f, 23f/255f));
        sandColors.Add(new Color(225f/255f, 169f/255f, 95f/255f));

        const int NUMFORESTCOLORS = 4;
        List<Color> forestColors = new List<Color>();
        forestColors.Add(new Color(86f/255f, 130f/255f, 3f/255f));
        forestColors.Add(new Color(138f/255f, 154f/255f, 91f/255f));
        forestColors.Add(new Color(141f/255f, 182f/255f, 0f/255f));
        forestColors.Add(new Color(154f/255f, 205f/255f, 50f/255f));

        const int NUMSTONECOLORS = 4;
        List<Color> stoneColors = new List<Color>();
        stoneColors.Add(new Color(160f/255f, 160f/255f, 160f/255f));
        stoneColors.Add(new Color(130f/255f, 130f/255f, 130f/255f));
        stoneColors.Add(new Color(100f/255f, 100f/255f, 100f/255f));
        stoneColors.Add(new Color(70f/255f, 70f/255f, 70f/255f));


        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random psuedoRandom = new System.Random(seed.GetHashCode());

        int index = 0;
        for(int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                vertices.Add(CoordToWorldPoint(x, y));
                map[x, y].setIndex(index);
                index++;
                Color pointColor = new Color();
                int rand = psuedoRandom.Next(0, 3);
                switch (map[x, y].getType())
                {
                    case TerrainType.Beach:
                        pointColor = sandColors[rand];
                        break;
                    case TerrainType.Forest:
                        pointColor = forestColors[rand];
                        break;
                    case TerrainType.Stone:
                        pointColor = stoneColors[rand];
                        break;
                }
                colors.Add(pointColor);
            }
        }

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                if(x-1 >= 0 && y + 1 < height)
                {
                    triangles.Add(map[x - 1, y + 1].getIndex());
                    triangles.Add(map[x, y + 1].getIndex());
                    triangles.Add(map[x, y].getIndex());
                    
                }
                if(x+1 < width && y + 1 < height)
                {
                     triangles.Add(map[x, y].getIndex());
                     triangles.Add(map[x, y + 1].getIndex());
                     triangles.Add(map[x + 1, y].getIndex());
                }
            }
        }

        Vector3[] verticesModified = new Vector3[triangles.Count];
        int[] trianglesModified = new int[triangles.Count];
        Color[] colorsModified = new Color[triangles.Count];
        Color currentColor = new Color();
        for (int i = 0; i < trianglesModified.Length; i++)
        {
            //make every vertex unique
            verticesModified[i] = vertices[triangles[i]];
            trianglesModified[i] = i;
            // Assign a color coming from colors array
            if(i % 3 == 0)
            {
                currentColor = colors[triangles[i]];
            }
            colorsModified[i] = currentColor;
        }

        Mesh mesh = new Mesh();

        mesh.vertices = verticesModified;
        mesh.triangles = trianglesModified;
        mesh.colors = colorsModified;
        mesh.RecalculateNormals();
        landMF.mesh = mesh;

        if ( landMF.gameObject.GetComponent<MeshCollider>() != null)
        {
            Destroy(landMF.gameObject.GetComponent<MeshCollider>());
        }
        MeshCollider wallCollider = landMF.gameObject.AddComponent<MeshCollider>();
        wallCollider.sharedMesh = mesh;

    }
}
