using LevelSetData;
using System.IO;
using UnityEngine;

public class BrickType
{
	public string Name { get; set; }
	public BrickProperties Properties { get; set; }
	public Sprite[] Sprites { get; set; }

	public readonly float BrickUnityWidth;
	public readonly float BrickUnityHeight;

	internal class InvalidBrickTextureException : IOException
	{
		public InvalidBrickTextureException(string message) : base(message) { }
	}

	//FIXME Add brick folder existence check
	public BrickType(string brickname, string path = "Default Bricks")
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
				Sprites[i] = Sprite.Create(brickTexture, new Rect(0, p, brickTexture.width, singleSpriteHeight), new Vector2(0, 1), unityBrickTextureScaleFactor/** * (brickTexture.width / BrickProperties.PIXEL_WIDTH)*/, 1, SpriteMeshType.FullRect);
			}
		}
		else
			throw new InvalidBrickTextureException($"Single brick sprite width and height do not have ratio of 2. Actual ratio: {brickTextureWidthToHeightRatio}, brick name: {brickname}");
	}
}
