using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			Instance = this;
	}
	#endregion

	[SerializeField]
#pragma warning disable CS0649 // Field 'TextureManager.backgroundRenderer' is never assigned to, and will always have its default value null
	private SpriteRenderer backgroundRenderer;
#pragma warning restore CS0649 // Field 'TextureManager.backgroundRenderer' is never assigned to, and will always have its default value null

	public void LoadGameTextures(string levelSetFileName, LevelSetData.Level level)
	{
		Texture2D backgroundTexture = FileImporter.GetBackgroundTexture(levelSetFileName, level.LevelProperties.BackgroundName);
		if (backgroundTexture != null)
		{
			Sprite sprite = Sprite.Create(
				backgroundTexture,
				new Rect(0, 0, backgroundTexture.width, backgroundTexture.height),
				new Vector2(0, 1),
				48.0f,
				1,
				SpriteMeshType.FullRect
			);
			backgroundRenderer.sprite = sprite;
			backgroundRenderer.size = new Vector2(12.50f, 10.0f);
		}
		else
		{
			backgroundRenderer.sprite = null;
		}
	}
}
