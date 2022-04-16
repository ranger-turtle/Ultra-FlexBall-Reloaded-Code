using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TextureManager : MonoBehaviour
{
	#region Singleton
	public static TextureManager Instance { get; private set; }

	void Awake()
	{
		if (Instance)
			Destroy(Instance);
		else
		{
			Instance = this;
		}
	}
	#endregion

	[SerializeField]
	private SpriteRenderer backgroundRenderer;
	[SerializeField]
	private SpriteRenderer leftWallRenderer;
	[SerializeField]
	private SpriteRenderer rightWallRenderer;

	private Dictionary<string, Sprite> loadedBackgrounds;
	private Dictionary<string, Sprite> loadedWalls;
	private Sprite levelSetDefaultBackground;
	private Sprite levelSetDefaultLeftWallSprite;
	private Sprite levelSetDefaultRightWallSprite;

	public void LoadLevelSetTextures(string levelSetDirectory, string levelSetFileName)
	{
		loadedBackgrounds = FileImporter.LoadBackgroundsFromLevelSet(levelSetFileName, levelSetDirectory)?.ToDictionary(k => k.Key, v => CreateBackgroundSprite(v.Value));
		loadedWalls = FileImporter.LoadWallTexturesFromLevelSet(levelSetFileName, levelSetDirectory)?.ToDictionary(k => k.Key, v => CreateWallSprite(v.Value));
	}

	public Sprite CreateBackgroundSprite(Texture2D backgroundTexture)
	{
		return Sprite.Create(
			backgroundTexture,
			new Rect(0, 0, backgroundTexture.width, backgroundTexture.height),
			Vector2.up,
			48.0f,
			1,
			SpriteMeshType.FullRect
		);
	}

	public Sprite CreateWallSprite(Texture2D wallTexture)
	{
		return Sprite.Create(
			wallTexture,
			//BONUS new Rect(0, 0, wallTexture.width, wallTexture.height),
			new Rect(0, 0, Mathf.Max(wallTexture.width, 19), Mathf.Max(wallTexture.height, 480)),
			new Vector2(0.5f, 0.5f),
			48.0f,
			1,
			SpriteMeshType.FullRect
		);
	}

	private void UpdateLevelSetTexture(ref Sprite levelSetDefaultTexture, string textureName, Dictionary<string, Sprite> loadedSprites, string textureTypeName, List<string> errorList)
	{
		if (loadedSprites != null)
		{
			if (textureName == "<none>")
				levelSetDefaultTexture = null;
			else
			{
				try
				{
					levelSetDefaultTexture = loadedSprites[textureName];
				}
				catch (KeyNotFoundException)
				{
					errorList.Add($"{textureTypeName} {textureName} not found.");
				}
			}
		}
		else if (textureName[0] != '<')
			errorList.Add($"{textureTypeName} {textureName} not found.");
	}

	public void UpdateLevelSetTextures(LevelSetData.LevelSet levelSet, List<string> errorList)
	{
		UpdateLevelSetTexture(ref levelSetDefaultBackground, levelSet.LevelSetProperties.DefaultBackgroundName, loadedBackgrounds, "Background image", errorList);
		UpdateLevelSetTexture(ref levelSetDefaultLeftWallSprite, levelSet.LevelSetProperties.DefaultLeftWallName, loadedWalls, "Left wall", errorList);
		UpdateLevelSetTexture(ref levelSetDefaultRightWallSprite, levelSet.LevelSetProperties.DefaultRightWallName, loadedWalls, "Right wall", errorList);
	}

	private void UpdateLevelTexture(SpriteRenderer spriteRenderer, Vector2 renderSize, Sprite levelSetDefaultSprite, string textureName, Dictionary<string, Sprite> loadedSprites, string textureTypeName, List<string> errorList)
	{
		if (loadedSprites != null)
		{
			if (textureName == "<level-set-default>")
			{
				spriteRenderer.sprite = levelSetDefaultSprite;
			}
			else if (textureName == "<none>")
				spriteRenderer.sprite = null;
			else
			{
				try
				{
					spriteRenderer.sprite = loadedSprites[textureName];
				}
				catch (KeyNotFoundException)
				{
					errorList.Add($"{textureTypeName} {textureName} not found.");
				}
			}
		}
		else if (textureName[0] != '<')
			errorList.Add($"{textureTypeName} {textureName} not found.");
		spriteRenderer.size = renderSize;
	}

	public void UpdateLevelTextures(LevelSetData.Level level, List<string> errorList)
	{
		UpdateLevelTexture(backgroundRenderer, new Vector2(12.50f, 10.0f), levelSetDefaultBackground, level.LevelProperties.BackgroundName, loadedBackgrounds, "Background image", errorList);
		UpdateLevelTexture(leftWallRenderer, new Vector2(0.3958333f, 10.0f), levelSetDefaultLeftWallSprite, level.LevelProperties.LeftWallName, loadedWalls, "Left wall", errorList);
		UpdateLevelTexture(rightWallRenderer, new Vector2(0.3958333f, 10.0f), levelSetDefaultRightWallSprite, level.LevelProperties.RightWallName, loadedWalls, "Right wall", errorList);
	}
}
