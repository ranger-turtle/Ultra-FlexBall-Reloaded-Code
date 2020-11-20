using LevelSetData;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//BONUS do brick enum called GraphicType which decides about graphic scaling algorithm (Pixel, Shaded)
public class BrickType
{
	public string Name { get; set; }
	public BrickProperties Properties { get; set; }
	public Sprite[] Sprites { get; set; }
	public Sprite HitSprite { get; set; }

	public readonly float BrickUnityWidth;
	public readonly float BrickUnityHeight;

	public readonly AudioClip hitAudio;

	public Sprite FirstSprite => Sprites[0];
	public bool HasHitSprite => HitSprite;

	internal class InvalidBrickTextureException : IOException
	{
		public InvalidBrickTextureException(string message) : base(message) { }
	}

	//FIXME Add brick folder existence check
	public BrickType(string brickname, HashSet<string> missingFileNames, string path = "Default Bricks")
	{
		Name = brickname;
		Properties = FileImporter.LoadBrickProperties($"{path}/{brickname}");
		if (!Directory.Exists($"{path}/{brickname}"))
			throw new DirectoryNotFoundException($@"There is no folder ""{path}/{brickname}"". Please restore folder and add valid framesheet.");
		Texture2D brickTexture = FileImporter.LoadTexture($"{path}/{brickname}/frames.png");
		float brickTextureWidthToHeightRatio = BrickProperties.PIXEL_WIDTH / BrickProperties.PIXEL_HEIGHT;
		int singleSpriteHeight = Mathf.RoundToInt(brickTexture.width / brickTextureWidthToHeightRatio);
		if ((float)brickTexture.width / singleSpriteHeight == brickTextureWidthToHeightRatio)
		{
			int units = brickTexture.height / singleSpriteHeight;
			Sprites = new Sprite[units];
			brickTexture.filterMode = FilterMode.Bilinear;
			float unityBrickTextureScaleFactor = brickTexture.width / (BrickProperties.PIXEL_WIDTH / 2) * 48.0f;
			BrickUnityWidth = brickTexture.width / unityBrickTextureScaleFactor;
			BrickUnityHeight = brickTexture.height / unityBrickTextureScaleFactor;
			for (int i = 0, p = 0; p < brickTexture.height; p += singleSpriteHeight, i++)
			{
				Sprites[i] = CreateSpriteFromTexture(brickTexture, singleSpriteHeight, unityBrickTextureScaleFactor, p);
			}
		}
		else
			throw new InvalidBrickTextureException($"Single brick sprite width and height do not have ratio of 2. Actual ratio: {brickTextureWidthToHeightRatio}, brick name: {brickname}");

		#region hitbrick
		if (File.Exists($"{path}/{brickname}/hit.png"))
		{
			Texture2D hitTexture = FileImporter.LoadTexture($"{path}/{brickname}/hit.png");
			float unityHitTextureScaleFactor = hitTexture.width / (BrickProperties.PIXEL_WIDTH / 2) * 48.0f;
			int hitSpriteHeight = Mathf.RoundToInt(hitTexture.width / brickTextureWidthToHeightRatio);
			HitSprite = CreateSpriteFromTexture(hitTexture, hitSpriteHeight, unityHitTextureScaleFactor, 0);
		}

		#endregion

		#region audio
		DefaultSoundLibrary defaultLevelSoundLibrary = DefaultSoundLibrary.Instance;
		//BONUS convert to switch expression when you upgrade to Visual Studio 2019
		if (Properties.HitSoundName == "<default>")
			hitAudio = defaultLevelSoundLibrary.normalBrickBreak;
		else if (Properties.HitSoundName == "<indestructible>")
			hitAudio = defaultLevelSoundLibrary.indestructibleBrickHit;
		else if (Properties.HitSoundName == "<bang>")
			hitAudio = defaultLevelSoundLibrary.explosiveBrickHit;
		else if (Properties.HitSoundName == "<multi>")
			hitAudio = defaultLevelSoundLibrary.changingBrickHit;
		else if (Properties.HitSoundName == "<plate>")
			hitAudio = defaultLevelSoundLibrary.plateHit;
		else if (Properties.HitSoundName != "<none>")
			hitAudio = SoundManager.Instance.FromLoadedSoundFiles(Properties.HitSoundName, missingFileNames);//UNDONE finish it
		#endregion
	}

	private Sprite CreateSpriteFromTexture(Texture2D brickTexture, int singleSpriteHeight, float unityBrickTextureScaleFactor, int spriteNum)
	{
		return Sprite.Create(brickTexture, new Rect(0, spriteNum, brickTexture.width, singleSpriteHeight), new Vector2(0, 1), unityBrickTextureScaleFactor/** * (brickTexture.width / BrickProperties.PIXEL_WIDTH)*/, 1, SpriteMeshType.FullRect);
	}
}
