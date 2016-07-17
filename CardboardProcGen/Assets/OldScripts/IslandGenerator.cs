using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IslandGenerator : MonoBehaviour
{

    public MeshFilter landMF;

	/** Enum to determine the differnt types or terrain - can add more later */
    public enum TerrainType { None, Beach, Forest, Stone };
	/** The default terrain type - beach seemed correct as we are on an island */
    public static TerrainType baseTerrainType = TerrainType.Beach;

	/** The distance at which to render objects */
	public float renderDistance = 100;

	/** 
	 * Main tile structure, each tile is one unit. 
	 * Technically these are points and not tiles
	 * Should move to a new file TODO
	 */
    public class TerrainTile
    {
        private TerrainType m_terrainType;
        private int m_x;
        private int m_y;
        private int index;
        private float m_height;
        private TerrainType m_baseType;
		private List<GameObject> m_environmentObjects;

        public TerrainTile(TerrainType type, int x, int y, float h)
        {
            m_terrainType = type;
            m_x = x;
            m_y = y;
            m_height = h;
            m_baseType = baseTerrainType;
			m_environmentObjects = new List<GameObject>();
        }
		/** Getters and setters for the tiles*/
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
		public void addEnvironmentObject(GameObject go) { m_environmentObjects.Add (go); }
		public void cleanupEnvironmentObjects()
		{
			foreach (GameObject go in m_environmentObjects)
			{
				Destroy (go);
			}
		}
		public void SetActive(bool shouldBeActive)
		{
			foreach (GameObject go in m_environmentObjects)
			{
				go.SetActive (shouldBeActive);
			}
		}
    }

    // Height and width of the island
    public int width;
    public int height;
	public int worldScale = 1;
	
	// Scale for our perlin noise - basically selecting a section of a perlin map
	public float perlinScale = 5f;
	// How high to scale our points
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

	// Number we round the plataues to - this could be named better TODO
    public int roundNumber;

	// Maximum size the prefabs can be
	public int environmentScale = 1;

	// Prefabs we are using for our environments
	public GameObject[] treePrefabs;
	public GameObject[] stonePrefabs;

	// Our terrain tile map
    TerrainTile[,] map;

	// Determine a starting position for the player
    Vector3 startingPos;

    private float waterH;
    private int xPlayerStart;
    private int yPlayerStart;

	// Keep track of the last place the player was
	private Vector3 playerLastPos;
	// How far we have to move to change the render
	public float playerMoveRenderDist = 10f;

	// Have we generated the island once already - used for cleanup when we generate again
	private bool firstGenerate = true;

    private System.Random psuedoRandom;

    // Use this for initialization
    void Start()
    {
		// Create the island on start
        GenerateIsland();
    }

    // Update is called once per frame
    void Update()
    {
        // Make a new Island every time middle mouse is pressed for testing
        if (Input.GetMouseButtonDown(2))
        {
			
            GenerateIsland();
        }
		GameObject player = GameObject.Find("CardboardMain");
		if (player)
		{
			if (Vector3.Distance (player.transform.position, playerLastPos) > playerMoveRenderDist)
			{
				SetActiveFromPlayerPosition ();
			}
		}
	}

    Vector3 CoordToWorldPoint(int x, int y)
    {
		// Converting the tiles x,y coords to points in the game - the multiplication values should be made changeable TODO
        Vector3 pos = new Vector3(-width / 2 + (x * worldScale) + .5f, map[x, y].getHeight(), -height / 2 + (y * worldScale) + .5f);
        return pos;
    }

	public void SetActiveFromPlayerPosition()
	{
		GameObject player = GameObject.Find("CardboardMain");
		if (player)
		{
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{			
					if (Vector3.Distance (player.transform.position, CoordToWorldPoint (x, y)) > renderDistance)
					{
						map [x, y].SetActive (false);
					} 
					else
					{
						map [x, y].SetActive (true);
					}
				}
			}
		}
	}



    public void GenerateIsland()
	{
		if (!firstGenerate)
		{
			// Clear out the evironment prefabs
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					map [x, y].cleanupEnvironmentObjects ();
				}
			}
		}
		firstGenerate = false;

        // Scales the perlin noise
        // No divisioon by 0 / negative values
		perlinScale = Mathf.Max (perlinScale, 0.0001f);

        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }
        psuedoRandom = new System.Random(seed.GetHashCode());

        

        Debug.Log("-- Start Generate --");
        map = new TerrainTile[width, height];

		Debug.Log ("-- Random Heightmap --");
		RandomHeightMap(baseTerrainType);
		Debug.Log("-- End Random Heightmap --");

		// Adjust the water plane to a random water level
        waterH = ((float)psuedoRandom.Next(0, (int)hscale) * 0.1f);
        MovePlane(waterH);

		// TODO put this in a loop - iterate through enum!

        Debug.Log ("-- Random Fill --");
		RandomFillMap(TerrainType.Forest);
		Debug.Log ("-- End Random Fill --");


		Debug.Log ("-- Start Smooth --");

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

        // Post processing beach and stone height sharpening/flattening
		// TODO Put this into a function!
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float distToPoint = Vector3.Distance(new Vector3(x,y,0), new Vector3(width/2,height/2,0));
                //float distToCorner = Vector3.Distance(new Vector3(width/2,height/2,0), new Vector3(0,0,0));

				// Set the type to be beach when below the water.
				if (map [x, y].getHeight () <= waterH)
				{
					map [x, y].setTypeAndBase (TerrainType.Beach);
				}

				if (!(width/2 > distToPoint + borderSize))
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
                /*
                if(map[x,y].getType() == TerrainType.Stone)
                {
                    float newHScale = (((float)psuedoRandom.Next(100, 100))/100f);

                    map[x, y].setHeight(map[x, y].getHeight() * newHScale);
                }
                else
                {
                    map[x, y].setHeight(map[x, y].getHeight() * 1f);
                }*/
                map[x, y].setHeight(Mathf.Round(map[x, y].getHeight() / roundNumber) * roundNumber);
            }
        }

		Debug.Log ("-- Start Generating Mesh --");
        GenerateMesh();
		Debug.Log ("-- End Generating Mesh --");

		Debug.Log ("-- Start Generate environment prefabs --");
		GenerateEnvironment ();
		Debug.Log ("-- End Generate environment prefabs --");


		Debug.Log ("-- Start Find Player Position --");
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
		Debug.Log ("-- End Find Player Position --");

        Debug.Log("End Generate");
    }

	// Uses the cardboard as the player, and transforms them to the new position
    void SetPlayerPos(Vector3 pos)
    {
        Vector3 newPos = pos;
		// Add three to the position to account for the size of the player, this shouldn't be a constant TODO
        newPos.y = pos.y + 3;
        GameObject player = GameObject.Find("CardboardMain");
        if (player)
        {
            player.transform.position = newPos;
            Debug.Log("Player Pos: " + newPos);
        }
    }

	// Moves the Water plane to a more appropriate height
    private void MovePlane(float h)
    {
        GameObject plane = GameObject.Find("Water");
        Vector3 pos = CoordToWorldPoint(width / 2, height / 2);
        pos.y = h;
        
        plane.transform.position = pos;
    }


    // Sets the map's height values according to perlin noise generation in Unity
	public void RandomHeightMap(TerrainType defaultType){
        /*if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }
        System.Random psuedoRandom = new System.Random(seed.GetHashCode()); */
        for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
                float xCoord = (float)(x);
				float yCoord = (float)(y);
                float h = Mathf.PerlinNoise(xCoord / perlinScale, yCoord / perlinScale);
                //int tempBorder = borderSize + psuedoRandom.Next(borderSize / -2, borderSize / 2);
                float distToPoint = Vector3.Distance(new Vector3(x, y, 0), new Vector3(width / 2, height / 2, 0));

                if (width/2 - borderSize > distToPoint )
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
		
    // Randomly assigns values to be the toFill type based on global percentage of type
	public void RandomFillMap(TerrainType toFill)
    {
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
		
    // Smooths the map based on toSmooth - looks for surrounding points and if there is a majority - change this tile
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

	// Returns the number of terrain types around the posX/posY that have the same terrain as type
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

	// Generate the mesh for the island - Should go into its own class eventually?
    void GenerateMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List < Color > colors = new List<Color>();

		// Set up the colors for the different terrain types
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

		// Create the initial vertices and set their colors
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

		// Create the triangle arrays for the mesh
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

		// Unity shaders mean that we have to make individual triangles to get their color full
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

		// Add all of our arrays to our mesh
        mesh.vertices = verticesModified;
        mesh.triangles = trianglesModified;
        mesh.colors = colorsModified;
        mesh.RecalculateNormals();
        landMF.mesh = mesh;

		// Destroy the old land mesh - used when we regenerate!
        if ( landMF.gameObject.GetComponent<MeshCollider>() != null)
        {
            Destroy(landMF.gameObject.GetComponent<MeshCollider>());
        }
		// Add a colider for our mesh
        MeshCollider wallCollider = landMF.gameObject.AddComponent<MeshCollider>();
        wallCollider.sharedMesh = mesh;

    }

	// Using map coords x and y, place trees in this tile randomly
	void PlaceTrees (int x, int y)
	{
		if (treePrefabs.Length > 0)
		{
			GameObject tree = Instantiate(treePrefabs[psuedoRandom.Next(0, treePrefabs.Length)], CoordToWorldPoint(x, y), Quaternion.Euler(0, 0, 0)) as GameObject;
			tree.transform.localScale = new Vector3(psuedoRandom.Next(1, environmentScale), psuedoRandom.Next(1, environmentScale), psuedoRandom.Next(1, environmentScale));
			map [x, y].addEnvironmentObject(tree);
		}
	}

	void PlaceStones(int x, int y)
	{
		if (stonePrefabs.Length > 0)
		{
			Debug.Log ("-- RAND: " + psuedoRandom.Next(1,15));
			GameObject stone = Instantiate(stonePrefabs[psuedoRandom.Next(0, stonePrefabs.Length - 1)], CoordToWorldPoint(x, y), Quaternion.Euler(0, 0, 0)) as GameObject;
			stone.transform.localScale = new Vector3(psuedoRandom.Next(1, environmentScale), psuedoRandom.Next(1, environmentScale), psuedoRandom.Next(1, environmentScale));
			map [x, y].addEnvironmentObject(stone);
		}
	}

	void GenerateEnvironment()
	{
		for (int x = 0; x < width; x++) 
		{
			for (int y = 0; y < height; y++) 
			{
				switch (map[x, y].getType())
				{
				case TerrainType.Beach:
					// No terrain items right now
					break;
				case TerrainType.Forest:
					PlaceTrees (x, y);
					break;
				case TerrainType.Stone:
					PlaceStones (x, y);
					break;
				}
			}
		}
	}
}
