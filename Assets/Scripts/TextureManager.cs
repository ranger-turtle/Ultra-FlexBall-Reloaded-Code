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

	private Dictionary<string, Sprite> loadedBackgrounds;
	private Sprite levelSetDefaultBackground;

	public void LoadLevelSetTextures(string levelSetDirectory, string levelSetFileName)
	{
		loadedBackgrounds = FileImporter.LoadBackgroundsFromLevelSet(levelSetFileName, levelSetDirectory)?.ToDictionary(k => k.Key, v => CreateBackgroundSprite(v.Value));
	}

	public Sprite CreateBackgroundSprite(Texture2D backgroundTexture)
	{
		return Sprite.Create(
			backgroundTexture,
			new Rect(0, 0, backgroundTexture.width, backgroundTexture.height),
			new Vector2(0, 1),
			48.0f,
			1,
			SpriteMeshType.FullRect
		);
	}

	public void UpdateLevelSetTextures(LevelSetData.LevelSet levelSet, List<string> errorList)
	{
		string backgroundName = levelSet.LevelSetProperties.DefaultBackgroundName;
		if (loadedBackgrounds != null)
		{
			if (backgroundName == "<none>")
				levelSetDefaultBackground = null;
			else
			{
				try
				{
					levelSetDefaultBackground = loadedBackgrounds[backgroundName];
				}
				catch (KeyNotFoundException)
				{
					errorList.Add($"Background image {backgroundName} not found.");
				}
			}
		}
		else if (backgroundName[0] != '<')
			errorList.Add($"Background image {backgroundName} not found.");
	}

	public void UpdateLevelTextures(LevelSetData.Level level, List<string> errorList)
	{
		string backgroundName = level.LevelProperties.BackgroundName;
		if (loadedBackgrounds != null)
		{
			if (backgroundName == "<level-set-default>")
			{
				backgroundRenderer.sprite = levelSetDefaultBackground;
			}
			else if (backgroundName == "<none>")
				backgroundRenderer.sprite = null;
			else
			{
				try
				{
					backgroundRenderer.sprite = loadedBackgrounds[backgroundName];
				}
				catch (KeyNotFoundException)
				{
					errorList.Add($"Background image {backgroundName} not found.");
				}
			}
		}
		else if (backgroundName[0] != '<')
			errorList.Add($"Background image {backgroundName} not found.");
		backgroundRenderer.size = new Vector2(12.50f, 10.0f);
	}
}
