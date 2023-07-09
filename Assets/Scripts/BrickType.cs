using LevelSetData;
using System.IO;
using UnityEngine;

//BONUS make scripted importer
public class BrickType
{
	private const float unityTexturePixelsForUnit = 48.0f;
	private const float brickTextureWidthToHeightRatio = BrickProperties.PIXEL_WIDTH / BrickProperties.PIXEL_HEIGHT;
	public const float BrickUnityWidth = BrickProperties.PIXEL_WIDTH / unityTexturePixelsForUnit;
	public const float BrickUnityHeight = BrickProperties.PIXEL_HEIGHT / unityTexturePixelsForUnit;

	public string Name { get; private set; }
	public BrickProperties Properties { get; private set; }
	public Sprite[] Sprites { get; private set; }
	public Sprite HitSprite { get; private set; }
	public Sprite[] BallBreakAnimationSprites { get; private set; }
	public Sprite[] ExplosionBreakAnimationSprites { get; private set; }
	public Sprite[] BulletBreakAnimationSprites { get; private set; }

	public readonly AudioClip hitAudio;

	public Sprite FirstSprite => Sprites[0];
	public bool HasHitSprite => HitSprite;

	internal class InvalidBrickTextureException : IOException
	{
		public InvalidBrickTextureException(string message) : base(message) { }
	}

	internal class TextureNotFoundException : FileNotFoundException
	{
		public TextureNotFoundException(string textureName) : base($"{textureName} not found.") { }
	}

	public BrickType(string brickname, string path = "Default Bricks")
	{
		Name = brickname;
		Properties = FileImporter.LoadBrickProperties($"{path}/{brickname}");
		if (!Directory.Exists($"{path}/{brickname}"))
			throw new DirectoryNotFoundException($@"There is no folder ""{path}/{brickname}"". Please restore folder and add valid framesheet.");
		Sprites = ReadAnimation(brickname, $"{path}/{brickname}/frames");
		/*Texture2D brickTexture = FileImporter.LoadTexture($"{path}/{brickname}/frames");
		float brickTextureWidthToHeightRatio = BrickProperties.PIXEL_WIDTH / BrickProperties.PIXEL_HEIGHT;
		int singleSpriteHeight = Mathf.RoundToInt(brickTexture.width / brickTextureWidthToHeightRatio);
		if ((float)brickTexture.width / singleSpriteHeight == brickTextureWidthToHeightRatio)
		{
			int units = brickTexture.height / singleSpriteHeight;
			Sprites = new Sprite[units];
			brickTexture.filterMode = FilterMode.Bilinear;
			float unityBrickTextureScaleFactor = brickTexture.width / BrickProperties.PIXEL_WIDTH * 48.0f;
			BrickUnityWidth = brickTexture.width / unityBrickTextureScaleFactor;
			BrickUnityHeight = brickTexture.height / unityBrickTextureScaleFactor;
			for (int i = 0, p = 0; p < brickTexture.height; p += singleSpriteHeight, i++)
			{
				Sprites[i] = CreateSpriteFromTexture(brickTexture, singleSpriteHeight, unityBrickTextureScaleFactor, p);
			}
		}
		else
			throw new InvalidBrickTextureException($"Single brick sprite width and height do not have ratio of 2. Actual ratio: {brickTextureWidthToHeightRatio}, brick name: {brickname}");*/

		#region hitbrick
		if (File.Exists($"{path}/{brickname}/hit.png"))
		{
			Texture2D hitTexture = FileImporter.LoadTexture($"{path}/{brickname}/hit");
			float unityHitTextureScaleFactor = hitTexture.width / BrickProperties.PIXEL_WIDTH * 48.0f;
			int hitSpriteHeight = Mathf.RoundToInt(hitTexture.width / brickTextureWidthToHeightRatio);
			HitSprite = CreateSpriteFromTexture(hitTexture, hitSpriteHeight, unityHitTextureScaleFactor, 0);
		}
		#endregion

		#region Break Animations
		if (Properties.BallBreakAnimationType != BreakAnimationType.Fade)
		{
			if (Properties.BallBreakAnimationType == BreakAnimationType.Burn)
				BallBreakAnimationSprites = LoadedGameData.BurnAnimation;
			else if (File.Exists($"{path}/{brickname}/ballbreak.png"))
				BallBreakAnimationSprites = ReadAnimation(brickname, $"{path}/{brickname}/ballbreak");
			else
				throw new TextureNotFoundException($"{path}/{brickname}/ballbreak.png");
		}

		if (Properties.ExplosionBreakAnimationType != BreakAnimationType.Fade)
		{
			if (Properties.ExplosionBreakAnimationType == BreakAnimationType.Burn)
				ExplosionBreakAnimationSprites = LoadedGameData.BurnAnimation;
			else if (File.Exists($"{path}/{brickname}/explosionbreak.png"))
				ExplosionBreakAnimationSprites = ReadAnimation(brickname, $"{path}/{brickname}/explosionbreak");
			else
				throw new TextureNotFoundException($"{path}/{brickname}/explosionbreak.png");
		}

		if (Properties.BulletBreakAnimationType != BreakAnimationType.Fade)
		{
			if (Properties.BulletBreakAnimationType == BreakAnimationType.Burn)
				BulletBreakAnimationSprites = LoadedGameData.BurnAnimation;
			else if (File.Exists($"{path}/{brickname}/bulletbreak.png"))
				BulletBreakAnimationSprites = ReadAnimation(brickname, $"{path}/{brickname}/bulletbreak");
			else
				throw new TextureNotFoundException($"{path}/{brickname}/bulletbreak.png");
		}
		#endregion

		#region audio
		DefaultSoundLibrary defaultLevelSoundLibrary = DefaultSoundLibrary.Instance;
		hitAudio = Properties.HitSoundName switch
		{
			"<default>" => defaultLevelSoundLibrary.normalBrickBreak,
			"<indestructible>" => defaultLevelSoundLibrary.indestructibleBrickHit,
			"<bang>" => defaultLevelSoundLibrary.explosiveBrickHit,
			"<multi>" => defaultLevelSoundLibrary.changingBrickHit,
			"<plate>" => defaultLevelSoundLibrary.plateHit,
			"<none>" => hitAudio,
			_ => SoundManager.Instance.FromLoadedSoundFiles(Properties.HitSoundName)
		};
		#endregion
	}

	private Sprite[] ReadAnimation(string brickname, string framesheetPath)
	{
		Texture2D brickTexture = FileImporter.LoadTexture(framesheetPath);
		int singleSpriteHeight = Mathf.RoundToInt(brickTexture.width / brickTextureWidthToHeightRatio);
		if ((float)brickTexture.width / singleSpriteHeight == brickTextureWidthToHeightRatio)
		{
			int units = brickTexture.height / singleSpriteHeight;
			Sprite[] sprites = new Sprite[units];
			switch (Properties.GraphicType)
			{
				case GraphicType.Pixel:
					brickTexture.filterMode = FilterMode.Point;
					break;
				default:
					brickTexture.filterMode = FilterMode.Bilinear;
					break;
			}
			float unityBrickTextureScaleFactor = brickTexture.width / BrickProperties.PIXEL_WIDTH * 48.0f;
			for (int i = 0, p = brickTexture.height - singleSpriteHeight; p >= 0; p -= singleSpriteHeight, i++)
			{
				sprites[i] = CreateSpriteFromTexture(brickTexture, singleSpriteHeight, unityBrickTextureScaleFactor, p);
			}
			return sprites;
		}
		else
			throw new InvalidBrickTextureException($"Single brick sprite width and height do not have ratio of 2. Actual ratio: {brickTextureWidthToHeightRatio}, brick name: {brickname}");
	}

	private Sprite CreateSpriteFromTexture(Texture2D brickTexture, int singleSpriteHeight, float unityBrickTextureScaleFactor, int spriteNum)
	{
		return Sprite.Create(brickTexture, new Rect(0, spriteNum, brickTexture.width, singleSpriteHeight), new Vector2(0, 1), unityBrickTextureScaleFactor/** * (brickTexture.width / BrickProperties.PIXEL_WIDTH)*/, 1, SpriteMeshType.FullRect);
	}
}
