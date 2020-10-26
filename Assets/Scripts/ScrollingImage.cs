using UnityEngine;

public class ScrollingImage : MonoBehaviour
{
	private Sprite sprite;

    // Start is called before the first frame update
    void Start()
    {
		//sprite = GetComponent<SpriteRenderer>().sprite;
    }

    // Update is called once per frame
    void Update()
    {
		GetComponent<MeshRenderer>().material.mainTextureOffset = new Vector2(-(Time.time * 0.05f), 0);
    }
}
