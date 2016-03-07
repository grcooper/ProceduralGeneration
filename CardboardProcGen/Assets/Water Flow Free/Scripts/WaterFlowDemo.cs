// Water Flow FREE version: 1.0.2
// Author: Gold Experience Team (http://www.ge-team.com/pages)
// Support: geteamdev@gmail.com
// Please direct any bugs/comments/suggestions to geteamdev@gmail.com

#region Namespaces

using UnityEngine;
using System.Collections;

#endregion

// Water shader type
public enum eWaterShaderType
{
	None,
	WaterDiffuse,
	WaterHeightmap,
	WaterSimple
}

/***************
* WaterFlowDemo class
* This class handles switching demo water, displays and updates GUI of water shader.
**************/

public class WaterFlowDemo : MonoBehaviour
{
	
	#region Variables
	
	// Array of water in the scene
	Transform[]	m_WaterList;

	// Array of heightmap texture
	public Texture[] m_HeightMapTextures;
	
	// Current water index
	int					m_CurrentWaterIndex = 0;

	// Current shader type
	eWaterShaderType	m_CurrentWaterShaderType = eWaterShaderType.None;
	
	// Orbit camera
	WaterFlowOrbitCamera m_OrbitCamera = null;
	
	// Water Diffuse
	Color m_WaterDiffuse_Diffuse;
	float m_WaterDiffuse_Multiply;
	float m_WaterDiffuse_UMoveSpeed;
	float m_WaterDiffuse_VMoveSpeed;
	
	Color m_WaterDiffuseOld_Diffuse;
	float m_WaterDiffuseOld_Multiply;
	float m_WaterDiffuseOld_UMoveSpeed;
	float m_WaterDiffuseOld_VMoveSpeed;
	
		Color m_WaterDiffuseOriginal_Diffuse;
		float m_WaterDiffuseOriginal_Multiply;
		float m_WaterDiffuseOriginal_UMoveSpeed;
		float m_WaterDiffuseOriginal_VMoveSpeed;
	
	// Water Diffuse
	int m_WaterHeightMap1Index;
	int m_WaterHeightMap2Index;
	float m_WaterHeightmap_WaterTile;
	float m_WaterHeightmap_HeightmapTile;
	float m_WaterHeightmap_Refraction;
	float m_WaterHeightmap_Strength;
	float m_WaterHeightmap_Multiply;
	
	float m_WaterHeightmapOld_WaterTile;
	float m_WaterHeightmapOld_HeightmapTile;
	float m_WaterHeightmapOld_Refraction;
	float m_WaterHeightmapOld_Strength;
	float m_WaterHeightmapOld_Multiply;
	
		float m_WaterHeightmapOriginal_WaterTile;
		float m_WaterHeightmapOriginal_HeightmapTile;
		float m_WaterHeightmapOriginal_Refraction;
		float m_WaterHeightmapOriginal_Strength;
		float m_WaterHeightmapOriginal_Multiply;
	
	// Water Simple
	Color m_WaterSimple_Ambient;
	float m_WaterSimple_UMoveSpeed;
	float m_WaterSimple_VMoveSpeed;
	
	Color m_WaterSimpleOld_Ambient;
	float m_WaterSimpleOld_UMoveSpeed;
	float m_WaterSimpleOld_VMoveSpeed;
	
		Color m_WaterSimpleOriginal_Ambient;
		float m_WaterSimpleOriginal_UMoveSpeed;
		float m_WaterSimpleOriginal_VMoveSpeed;
	
	#endregion
	
	// ######################################################################
	// MonoBehaviour Functions
	// ######################################################################
	
	#region MonoBehaviour
	
	// Use this for initialization
	void Start ()
	{
		// Find orbit camera in the scene
		m_OrbitCamera = FindObjectOfType<WaterFlowOrbitCamera>();
		
		// Add water into m_WaterList array
		m_WaterList = new Transform[this.transform.childCount];
		int i=0;
		foreach(Transform child in transform)
		{
			// Add water into m_WaterList
			m_WaterList[i] = child;
			m_WaterList[i].gameObject.SetActive(false);

			// Keep original parameters of each shader
			switch(m_WaterList[i].gameObject.GetComponent<Renderer>().material.shader.name)
			{

				case "Water Flow/Water Diffuse":

					m_WaterDiffuseOriginal_Diffuse 		= m_WaterList[i].gameObject.GetComponent<Renderer>().material.GetColor("_MainTexColor");
					m_WaterDiffuseOriginal_Multiply 	= m_WaterList[i].gameObject.GetComponent<Renderer>().material.GetFloat("_MainTexMultiply");
					m_WaterDiffuseOriginal_UMoveSpeed 	= m_WaterList[i].gameObject.GetComponent<Renderer>().material.GetFloat("_MainTexMoveSpeedU");
					m_WaterDiffuseOriginal_VMoveSpeed 	= m_WaterList[i].gameObject.GetComponent<Renderer>().material.GetFloat("_MainTexMoveSpeedV");
					
					m_WaterDiffuseOld_Diffuse			= m_WaterDiffuse_Diffuse	= m_WaterDiffuseOriginal_Diffuse;
					m_WaterDiffuseOld_Multiply 			= m_WaterDiffuse_Multiply	= m_WaterDiffuseOriginal_Multiply;
					m_WaterDiffuseOld_UMoveSpeed		= m_WaterDiffuse_UMoveSpeed	= m_WaterDiffuseOriginal_UMoveSpeed;
					m_WaterDiffuseOld_VMoveSpeed		= m_WaterDiffuse_VMoveSpeed	= m_WaterDiffuseOriginal_VMoveSpeed;
					
					break;

				case "Water Flow/Water Heightmap":
					
					m_WaterHeightmapOriginal_WaterTile 			= m_WaterList[i].gameObject.GetComponent<Renderer>().material.GetFloat("_MainTexTile");
					m_WaterHeightmapOriginal_HeightmapTile 		= m_WaterList[i].gameObject.GetComponent<Renderer>().material.GetFloat("_HeightMapTile");
					m_WaterHeightmapOriginal_Refraction 		= m_WaterList[i].gameObject.GetComponent<Renderer>().material.GetFloat("_MainTexRefraction");
					m_WaterHeightmapOriginal_Strength 			= m_WaterList[i].gameObject.GetComponent<Renderer>().material.GetFloat("_HeightMapStrength");
					m_WaterHeightmapOriginal_Multiply 			= m_WaterList[i].gameObject.GetComponent<Renderer>().material.GetFloat("_HeightMapMultiply");
					
					m_WaterHeightmapOld_WaterTile		= m_WaterHeightmap_WaterTile		= m_WaterHeightmapOriginal_WaterTile;
					m_WaterHeightmapOld_HeightmapTile	= m_WaterHeightmap_HeightmapTile	= m_WaterHeightmapOriginal_HeightmapTile;
					m_WaterHeightmapOld_Refraction		= m_WaterHeightmap_Refraction		= m_WaterHeightmapOriginal_Refraction;
					m_WaterHeightmapOld_Strength		= m_WaterHeightmap_Strength			= m_WaterHeightmapOriginal_Strength;
					m_WaterHeightmapOld_Multiply		= m_WaterHeightmap_Multiply			= m_WaterHeightmapOriginal_Multiply;

					break;

				case "Water Flow/Water Simple":
					
					m_WaterSimpleOriginal_Ambient		= RenderSettings.ambientLight;
					m_WaterSimpleOriginal_UMoveSpeed 	= m_WaterList[i].gameObject.GetComponent<Renderer>().material.GetFloat("_MainTexMoveSpeedU");
					m_WaterSimpleOriginal_VMoveSpeed 	= m_WaterList[i].gameObject.GetComponent<Renderer>().material.GetFloat("_MainTexMoveSpeedV");
					
					m_WaterSimpleOld_Ambient			= m_WaterSimple_Ambient			= m_WaterSimpleOriginal_Ambient;
					m_WaterSimpleOld_UMoveSpeed			= m_WaterSimple_UMoveSpeed		= m_WaterSimpleOriginal_UMoveSpeed;
					m_WaterSimpleOld_VMoveSpeed			= m_WaterSimple_VMoveSpeed		= m_WaterSimpleOriginal_VMoveSpeed; 

					break;

				default:
					break;			
			}

			i++;
		}
		
		// set current water to zero
		m_CurrentWaterIndex = 0;		

		// update m_CurrentWaterShaderType
		UpdateCurrentShaderType();
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Restore orbit camera when user mouse button up
		if(Input.GetMouseButtonUp(0))
		{
			if(m_OrbitCamera.enabled==false)
			{
				m_OrbitCamera.enabled = true;	
			}
		}
	}
	
	// OnGUI is called for rendering and handling GUI events.
	void OnGUI ()
	{
		
		// Show version number
		GUI.Window(1, new Rect((Screen.width-260), 5, 250, 80), AppNameWindow, "Water Flow FREE 1.0.2");
		
		// Show Scene GUI window
		int DemoWaterWindowWidth = 10 + (195*transform.childCount);
		GUI.Window(2, new Rect((Screen.width-DemoWaterWindowWidth)/2, Screen.height-65, DemoWaterWindowWidth, 60), DemoWater, "Select Water");
		
		ShaderGUIWindow();
	}
	
	#endregion
	
	// ######################################################################
	// GUI window functions
	// ######################################################################
	
	#region GUI window functions
	
	// Info window
	void AppNameWindow(int id)
	{
		// www.ge-team.com button
		if(GUI.Button(new Rect(15, 25, 220, 20), "www.ge-team.com"))
		{
			Application.OpenURL("http://ge-team.com/pages/unity-3d/");
		}
		// Support button
		if(GUI.Button(new Rect(15, 50, 220, 20), "support: geteamdev@gmail.com"))
		{
			Application.OpenURL("mailto:geteamdev@gmail.com");
		}
	}
	
	// Water selection buttons window
	void DemoWater(int id)
	{
		if(m_CurrentWaterIndex>=0)
		{
			GUILayout.BeginHorizontal();
			
			// List all children of this object
			for(int i=0;i<transform.childCount;i++)
			{
				// Disable button if this child is selected water
				if(i==m_CurrentWaterIndex)
				{
					GUI.enabled=false;
					if(m_WaterList[i].gameObject.activeSelf==false)
						m_WaterList[i].gameObject.SetActive(true);
				}
				// Enable button if this is not selected water
				else
				{
					GUI.enabled=true;
					if(m_WaterList[i].gameObject.activeSelf==true)
						m_WaterList[i].gameObject.SetActive(false);
				}
				// Display water button
				if(GUI.Button(new Rect(10 + (195*i), 25, 185, 25), m_WaterList[i].name))
				{
					// Hide old selected water
					m_WaterList[m_CurrentWaterIndex].gameObject.SetActive(false);

					// Show current selected water
					m_WaterList[i].gameObject.SetActive(true);
					
					// update m_CurrentWaterIndex
					m_CurrentWaterIndex = i;
					
					// update m_CurrentWaterShaderType
					UpdateCurrentShaderType();
				}
			}
			
			GUILayout.EndHorizontal();
		}
		
	}
	
	// Shader GUI switcher
	void ShaderGUIWindow()
	{
		// Show Water Diffuse GUI window
		if(m_CurrentWaterShaderType == eWaterShaderType.WaterDiffuse)
		{
			GUI.Window(3, new Rect(0, 0, 300, 240), WaterDiffuseGUIWindow, m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.shader.name.Replace("Water Flow/","Shader: "));
		}
		// Show Water Heightmap GUI window
		else if(m_CurrentWaterShaderType == eWaterShaderType.WaterHeightmap)
		{
			GUI.Window(4, new Rect(0, 0, 300, 240), WaterHeightmapGUIWindow, m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.shader.name.Replace("Water Flow/","Shader: "));
		}
		// Show Water Simple GUI window
		else if(m_CurrentWaterShaderType == eWaterShaderType.WaterSimple)
		{
			GUI.Window(5, new Rect(0, 0, 300, 240), WaterSimpleGUIWindow, m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.shader.name.Replace("Water Flow/","Shader: "));
		}
	}
	
	// Show Water Diffuse GUI window
	void WaterDiffuseGUIWindow(int id)
	{
		// Diffuse Color sliders
		GUI.Label( new Rect(10, 25, 65, 22), "Diffuse R" );
		GUI.Label( new Rect(10, 45, 65, 22), "Diffuse G" );
		GUI.Label( new Rect(10, 65, 65, 22), "Diffuse B" );
		GUI.Label( new Rect(10, 85, 65, 22), "Diffuse A" );
		m_WaterDiffuse_Diffuse = new Color (GUI.HorizontalSlider( new Rect(125, 30, 150, 15), m_WaterDiffuse_Diffuse.r, 0f, 1f),
		                                    GUI.HorizontalSlider( new Rect(125, 50, 150, 15), m_WaterDiffuse_Diffuse.g, 0f, 1f),
		                                    GUI.HorizontalSlider( new Rect(125, 70, 150, 15), m_WaterDiffuse_Diffuse.b, 0f, 1f),
		                                    GUI.HorizontalSlider( new Rect(125, 90, 150, 15), m_WaterDiffuse_Diffuse.a, 0f, 1f));
		if(m_WaterDiffuseOld_Diffuse != m_WaterDiffuse_Diffuse)
		{
			PauseOrbitCamera(); // pause orbit camera
			m_WaterDiffuseOld_Diffuse = m_WaterDiffuse_Diffuse;
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetColor("_MainTexColor", m_WaterDiffuse_Diffuse);
		}
		
		// Multiply slider
		GUI.Label( new Rect(10, 125, 65, 22), "Multiply" );
		m_WaterDiffuse_Multiply = GUI.HorizontalSlider( new Rect(125, 130, 150, 15), m_WaterDiffuse_Multiply, 0f, 5f);
		if(m_WaterDiffuseOld_Multiply != m_WaterDiffuse_Multiply)
		{
			PauseOrbitCamera(); // pause orbit camera
			m_WaterDiffuseOld_Multiply = m_WaterDiffuse_Multiply;
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetFloat("_MainTexMultiply", m_WaterDiffuse_Multiply);
			
		}
		
		// U Move Speed slider
		GUI.Label( new Rect(10, 145, 65, 22), "U Speed" );
		m_WaterDiffuse_UMoveSpeed = GUI.HorizontalSlider( new Rect(125, 150, 150, 15), m_WaterDiffuse_UMoveSpeed, -6f, 6f);
		if(m_WaterDiffuseOld_UMoveSpeed != m_WaterDiffuse_UMoveSpeed)
		{
			PauseOrbitCamera(); // pause orbit camera
			m_WaterDiffuseOld_UMoveSpeed = m_WaterDiffuse_UMoveSpeed;
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetFloat("_MainTexMoveSpeedU", m_WaterDiffuse_UMoveSpeed);
		}
		
		// V Move Speed slider
		GUI.Label( new Rect(10, 165, 65, 22), "V Speed" );
		m_WaterDiffuse_VMoveSpeed = GUI.HorizontalSlider( new Rect(125, 170, 150, 15), m_WaterDiffuse_VMoveSpeed, -6f, 6f);
		if(m_WaterDiffuseOld_VMoveSpeed != m_WaterDiffuse_VMoveSpeed)
		{
			PauseOrbitCamera(); // pause orbit camera
			m_WaterDiffuseOld_VMoveSpeed = m_WaterDiffuse_VMoveSpeed;
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetFloat("_MainTexMoveSpeedV", m_WaterDiffuse_VMoveSpeed);
		}
		
		// Reset button
		if(GUI.Button( new Rect(125, 200, 150, 30), "Reset"))
		{
			// Load Original shader values
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetColor("_MainTexColor", m_WaterDiffuseOriginal_Diffuse);
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetFloat("_MainTexMultiply", m_WaterDiffuseOriginal_Multiply);
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetFloat("_MainTexMoveSpeedU", m_WaterDiffuseOriginal_UMoveSpeed);
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetFloat("_MainTexMoveSpeedV", m_WaterDiffuseOriginal_VMoveSpeed);
			
			// Update GUI objects
			m_WaterDiffuse_Diffuse = new Color (GUI.HorizontalSlider( new Rect(125, 30, 150, 15), m_WaterDiffuseOriginal_Diffuse.r, 0f, 1f),
			                                    GUI.HorizontalSlider( new Rect(125, 50, 150, 15), m_WaterDiffuseOriginal_Diffuse.g, 0f, 1f),
			                                    GUI.HorizontalSlider( new Rect(125, 70, 150, 15), m_WaterDiffuseOriginal_Diffuse.b, 0f, 1f),
			                                    GUI.HorizontalSlider( new Rect(125, 90, 150, 15), m_WaterDiffuseOriginal_Diffuse.a, 0f, 1f));
			m_WaterDiffuse_Multiply = GUI.HorizontalSlider( new Rect(125, 130, 150, 15), m_WaterDiffuseOriginal_Multiply, 0f, 5f);
			m_WaterDiffuse_UMoveSpeed = GUI.HorizontalSlider( new Rect(125, 150, 150, 15), m_WaterDiffuseOriginal_UMoveSpeed, -6f, 6f);
			m_WaterDiffuse_VMoveSpeed = GUI.HorizontalSlider( new Rect(125, 170, 150, 15), m_WaterDiffuseOriginal_VMoveSpeed, -6f, 6f);
		}
	}
	
	// Show Water Heightmap GUI window
	void WaterHeightmapGUIWindow(int id)
	{        
        // Main Texture tile slider
		GUI.Label( new Rect(10, 25, 90, 22), "Texture Tile" );
		m_WaterHeightmap_WaterTile = GUI.HorizontalSlider( new Rect(125, 30, 150, 15), m_WaterHeightmap_WaterTile, 0.25f, 5f);
		if(m_WaterHeightmapOld_WaterTile != m_WaterHeightmap_WaterTile)
		{
			PauseOrbitCamera(); // pause orbit camera
			m_WaterHeightmapOld_WaterTile = m_WaterHeightmap_WaterTile;
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetFloat("_MainTexTile", m_WaterHeightmap_WaterTile);            
		}
		
		// HeightMap1 buttons
		GUI.Label( new Rect(10, 55, 230, 22), "HeightMap 1: " + m_HeightMapTextures[m_WaterHeightMap1Index].name );
		if(GUI.Button( new Rect(245, 55, 20, 22), "<"))
		{
			m_WaterHeightMap1Index--;
			if(m_WaterHeightMap1Index<0)
			{
				m_WaterHeightMap1Index = m_HeightMapTextures.Length-1;
			}
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetTexture("_HeightMap1", m_HeightMapTextures[m_WaterHeightMap1Index]);
		}
		if(GUI.Button( new Rect(265, 55, 20, 22), ">"))
		{
			m_WaterHeightMap1Index++;
			if(m_WaterHeightMap1Index>=m_HeightMapTextures.Length-1)
			{
				m_WaterHeightMap1Index = 0;
			}
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetTexture("_HeightMap1", m_HeightMapTextures[m_WaterHeightMap1Index]);
		}
		
		// HeightMap2 buttons
		GUI.Label( new Rect(10, 75, 230, 22), "HeightMap 2: " + m_HeightMapTextures[m_WaterHeightMap2Index].name );
		if(GUI.Button( new Rect(245, 75, 20, 22), "<"))
		{
			m_WaterHeightMap2Index--;
			if(m_WaterHeightMap2Index<0)
			{
				m_WaterHeightMap2Index = m_HeightMapTextures.Length-1;
			}
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetTexture("_HeightMap2", m_HeightMapTextures[m_WaterHeightMap2Index]);
		}
		if(GUI.Button( new Rect(265, 75, 20, 22), ">"))
		{
			m_WaterHeightMap2Index++;
			if(m_WaterHeightMap2Index>=m_HeightMapTextures.Length-1)
			{
				m_WaterHeightMap2Index = 0;
			}
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetTexture("_HeightMap2", m_HeightMapTextures[m_WaterHeightMap2Index]);
		}
		
        // HeightMap Tile slider
		GUI.Label( new Rect(10, 95, 90, 22), "HeightMap Tile" );
		m_WaterHeightmap_HeightmapTile = GUI.HorizontalSlider( new Rect(125, 100, 150, 15), m_WaterHeightmap_HeightmapTile, 0.25f, 5f);
		if(m_WaterHeightmapOld_HeightmapTile != m_WaterHeightmap_HeightmapTile)
		{
			PauseOrbitCamera(); // pause orbit camera
			m_WaterHeightmapOld_HeightmapTile = m_WaterHeightmap_HeightmapTile;
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetFloat("_HeightMapTile", m_WaterHeightmap_HeightmapTile);
			
		}
		
		// Refraction slider
		GUI.Label( new Rect(10, 125, 90, 22), "Refraction" );
		m_WaterHeightmap_Refraction = GUI.HorizontalSlider( new Rect(125, 130, 150, 15), m_WaterHeightmap_Refraction, 0.1f, 5f);
		if(m_WaterHeightmapOld_Refraction != m_WaterHeightmap_Refraction)
		{
			PauseOrbitCamera(); // pause orbit camera
			m_WaterHeightmapOld_Refraction = m_WaterHeightmap_Refraction;
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetFloat("_MainTexRefraction", m_WaterHeightmap_Refraction);
			
		}
		
		// Strength slider
		GUI.Label( new Rect(10, 145, 90, 22), "Strength" );
		m_WaterHeightmap_Strength = GUI.HorizontalSlider( new Rect(125, 150, 150, 15), m_WaterHeightmap_Strength, 0f, 5f);
		if(m_WaterHeightmapOld_Strength != m_WaterHeightmap_Strength)
		{
			PauseOrbitCamera(); // pause orbit camera
			m_WaterHeightmapOld_Strength = m_WaterHeightmap_Strength;
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetFloat("_HeightMapStrength", m_WaterHeightmap_Strength);
		}
		
		// Multiply slider
		GUI.Label( new Rect(10, 165, 90, 22), "Multiply" );
		m_WaterHeightmap_Multiply = GUI.HorizontalSlider( new Rect(125, 170, 150, 15), m_WaterHeightmap_Multiply, 0.05f, 0.5f);
		if(m_WaterHeightmapOld_Multiply != m_WaterHeightmap_Multiply)
		{
			PauseOrbitCamera(); // pause orbit camera
			m_WaterHeightmapOld_Multiply = m_WaterHeightmap_Multiply;
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetFloat("_HeightMapMultiply", m_WaterHeightmap_Multiply);
		}
		
		// Reset button
		if(GUI.Button( new Rect(125, 200, 150, 30), "Reset"))
		{
			// Load Original shader values
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetFloat("_MainTexTile", m_WaterHeightmapOriginal_WaterTile);
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetFloat("_HeightMapTile", m_WaterHeightmapOriginal_HeightmapTile);
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetFloat("_MainTexRefraction", m_WaterHeightmapOriginal_Refraction);
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetFloat("_HeightMapStrength", m_WaterHeightmapOriginal_Strength);
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetFloat("_HeightMapMultiply", m_WaterHeightmapOriginal_Multiply);
			
			// Update GUI objects
			m_WaterHeightmap_WaterTile = GUI.HorizontalSlider( new Rect(125, 30, 150, 15), m_WaterHeightmapOriginal_WaterTile, 0.25f, 5f);
			m_WaterHeightmap_HeightmapTile = GUI.HorizontalSlider( new Rect(125, 100, 150, 15), m_WaterHeightmapOriginal_HeightmapTile, 0.25f, 5f);
			m_WaterHeightmap_Refraction = GUI.HorizontalSlider( new Rect(125, 130, 150, 15), m_WaterHeightmapOriginal_Refraction, 0.1f, 5f);
			m_WaterHeightmap_Strength = GUI.HorizontalSlider( new Rect(125, 150, 150, 15), m_WaterHeightmapOriginal_Strength, 0f, 5f);
			m_WaterHeightmap_Multiply = GUI.HorizontalSlider( new Rect(125, 170, 150, 15), m_WaterHeightmapOriginal_Multiply, 0.05f, 0.5f);
		}
	}
	
	// Show Water Simple GUI window
	void WaterSimpleGUIWindow(int id)
	{
		// Ambient Light sliders
		GUI.Label( new Rect(10, 25, 100, 22), "Ambient Red" );
		GUI.Label( new Rect(10, 45, 100, 22), "Ambient Green" );
		GUI.Label( new Rect(10, 65, 100, 22), "Ambient Blue" );
		GUI.Label( new Rect(10, 85, 100, 22), "Ambient Alpha" );
		m_WaterSimple_Ambient = new Color (GUI.HorizontalSlider( new Rect(125, 30, 150, 15), RenderSettings.ambientLight.r, 0f, 1f),
		                                   GUI.HorizontalSlider( new Rect(125, 50, 150, 15), RenderSettings.ambientLight.g, 0f, 1f),
		                                   GUI.HorizontalSlider( new Rect(125, 70, 150, 15), RenderSettings.ambientLight.b, 0f, 1f),
		                                   1f);
		if(m_WaterSimpleOld_Ambient != m_WaterSimple_Ambient)
		{
			PauseOrbitCamera(); // pause orbit camera
			m_WaterSimpleOld_Ambient = m_WaterSimple_Ambient;
			RenderSettings.ambientLight = m_WaterSimple_Ambient;
		}

		if (GUI.Button (new Rect (115, 87, 170, 18), "Transparancy and more"))
		{
			Application.OpenURL("https://www.assetstore.unity3d.com/#!/content/26430");
		}


		// U Move Speed slider
		GUI.Label( new Rect(10, 125, 65, 22), "U Speed" );
		m_WaterSimple_UMoveSpeed = GUI.HorizontalSlider( new Rect(125, 130, 150, 15), m_WaterSimple_UMoveSpeed, -6f, 6f);
		if(m_WaterSimpleOld_UMoveSpeed != m_WaterSimple_UMoveSpeed)
		{
			PauseOrbitCamera(); // pause orbit camera
			m_WaterSimpleOld_UMoveSpeed = m_WaterSimple_UMoveSpeed;
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetFloat("_MainTexMoveSpeedU", m_WaterSimple_UMoveSpeed);
		}
		
		// V Move Speed slider
		GUI.Label( new Rect(10, 145, 65, 22), "V Speed" );
		m_WaterSimple_VMoveSpeed = GUI.HorizontalSlider( new Rect(125, 150, 150, 15), m_WaterSimple_VMoveSpeed, -6f, 6f);
		if(m_WaterSimpleOld_VMoveSpeed != m_WaterSimple_VMoveSpeed)
		{
			PauseOrbitCamera(); // pause orbit camera
			m_WaterSimpleOld_VMoveSpeed = m_WaterSimple_VMoveSpeed;
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetFloat("_MainTexMoveSpeedV", m_WaterSimple_VMoveSpeed);
		}
		
		// Reset button
		if(GUI.Button( new Rect(125, 175, 150, 30), "Reset"))
		{
			// Load Original shader values
			RenderSettings.ambientLight = m_WaterSimpleOriginal_Ambient;
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetFloat("_MainTexMoveSpeedU", m_WaterSimpleOriginal_UMoveSpeed);
			m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.SetFloat("_MainTexMoveSpeedV", m_WaterSimpleOriginal_VMoveSpeed);

			// Update GUI objects
			m_WaterSimple_Ambient = new Color (GUI.HorizontalSlider( new Rect(125, 30, 150, 15), m_WaterSimpleOriginal_Ambient.r, 0f, 1f),
			                                   GUI.HorizontalSlider( new Rect(125, 50, 150, 15), m_WaterSimpleOriginal_Ambient.g, 0f, 1f),
			                                   GUI.HorizontalSlider( new Rect(125, 70, 150, 15), m_WaterSimpleOriginal_Ambient.b, 0f, 1f),
			                                   GUI.HorizontalSlider( new Rect(125, 90, 150, 15), m_WaterSimpleOriginal_Ambient.a, 0f, 1f));
			m_WaterSimple_UMoveSpeed = GUI.HorizontalSlider( new Rect(125, 130, 150, 15), m_WaterSimpleOriginal_UMoveSpeed, -6f, 6f);
			m_WaterSimple_VMoveSpeed = GUI.HorizontalSlider( new Rect(125, 150, 150, 15), m_WaterSimpleOriginal_VMoveSpeed, -6f, 6f);
        }
	}
	
	
	#endregion {GUI window functions}
	
	// ######################################################################
	//  Utilities functions
	// ######################################################################	
	
	#region Utilities functions

	// Update m_CurrentWaterShaderType according to current selected water
	void UpdateCurrentShaderType()
	{
		// Check if there is renderer component
		if(m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>()==null)
			return;
		if(m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material==null)
			return;

		switch(m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.shader.name)
		{
			// Water Diffuse shader
			case "Water Flow/Water Diffuse":

				m_CurrentWaterShaderType 			= eWaterShaderType.WaterDiffuse;
				
				m_WaterDiffuse_Diffuse 				= m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.GetColor("_MainTexColor");
				m_WaterDiffuse_Multiply 			= m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.GetFloat("_MainTexMultiply");
				m_WaterDiffuse_UMoveSpeed 			= m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.GetFloat("_MainTexMoveSpeedU");
				m_WaterDiffuse_VMoveSpeed 			= m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.GetFloat("_MainTexMoveSpeedV");
				
				m_WaterDiffuseOld_Diffuse			= m_WaterDiffuse_Diffuse;
				m_WaterDiffuseOld_Multiply 			= m_WaterDiffuse_Multiply;
				m_WaterDiffuseOld_UMoveSpeed		= m_WaterDiffuse_UMoveSpeed;
				m_WaterDiffuseOld_VMoveSpeed		= m_WaterDiffuse_VMoveSpeed;
				
				break;

			// Water Heightmap shader
			case "Water Flow/Water Heightmap":

				m_CurrentWaterShaderType			= eWaterShaderType.WaterHeightmap;
				
				Texture txHeightMap1 = m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.GetTexture("_HeightMap1");
				Texture txHeightMap2 = m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.GetTexture("_HeightMap2");
				for(int i=0;i<m_HeightMapTextures.Length;i++)
				{
					if(m_HeightMapTextures[i].name == txHeightMap1.name)
					{
						m_WaterHeightMap1Index = i;
					}
					if(m_HeightMapTextures[i].name == txHeightMap2.name)
					{
						m_WaterHeightMap2Index = i;
					}
				}
				
				m_WaterHeightmap_WaterTile 			= m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.GetFloat("_MainTexTile");
				m_WaterHeightmap_HeightmapTile 		= m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.GetFloat("_HeightMapTile");
				m_WaterHeightmap_Refraction 		= m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.GetFloat("_MainTexRefraction");
				m_WaterHeightmap_Strength 			= m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.GetFloat("_HeightMapStrength");
				m_WaterHeightmap_Multiply 			= m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.GetFloat("_HeightMapMultiply");
				
				m_WaterHeightmapOld_WaterTile		= m_WaterHeightmap_WaterTile;
				m_WaterHeightmapOld_HeightmapTile	= m_WaterHeightmap_HeightmapTile;
				m_WaterHeightmapOld_Refraction		= m_WaterHeightmap_Refraction;
				m_WaterHeightmapOld_Strength		= m_WaterHeightmap_Strength;
				m_WaterHeightmapOld_Multiply		= m_WaterHeightmap_Multiply;

				break;

			// Water Simple shader
			case "Water Flow/Water Simple":

				m_CurrentWaterShaderType			= eWaterShaderType.WaterSimple;
				
				m_WaterSimple_Ambient				= RenderSettings.ambientLight;
				m_WaterSimple_UMoveSpeed 			= m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.GetFloat("_MainTexMoveSpeedU");
				m_WaterSimple_VMoveSpeed 			= m_WaterList[m_CurrentWaterIndex].gameObject.GetComponent<Renderer>().material.GetFloat("_MainTexMoveSpeedV");
				
				m_WaterSimpleOld_Ambient			= m_WaterSimple_Ambient;
				m_WaterSimpleOld_UMoveSpeed			= m_WaterSimple_UMoveSpeed;
				m_WaterSimpleOld_VMoveSpeed			= m_WaterSimple_VMoveSpeed;  

				break;

			// Unknown shader
			default:

				m_CurrentWaterShaderType = eWaterShaderType.None;

				break;			
		}
	}
	
	// Pause orbit camera, it will be resume in Update function
	void PauseOrbitCamera()
	{
		if(m_OrbitCamera.enabled==true)
			m_OrbitCamera.enabled = false;
	}
	
	
	#endregion {Utilities functions}
}
