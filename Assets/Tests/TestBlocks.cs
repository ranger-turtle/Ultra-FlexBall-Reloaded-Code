using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestBlocks
{

    // A Test behaves as an ordinary method
    [Test]
    public void TestBlocksSimplePasses()
    {
        // Use the Assert class to test conditions
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator TestBrickReactionsOnCollisions()
    {
        yield return new WaitForSeconds(30);
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        GameObject ballObject =
			MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefabs/Ball"));

        GameManager gameManagerObject =
			GameObject.Find("Game").GetComponent<GameManager>();

        gameManagerObject.GetBrickByCoordinates(2, 6).Collision(ballObject);
        yield return null;
        Assert.IsTrue(gameManagerObject.NumberOfBricksRequiredToComplete == 9);

        gameManagerObject.GetBrickByCoordinates(4, 6).Collision(ballObject);
        yield return null;
        Assert.IsTrue(gameManagerObject.NumberOfBricksRequiredToComplete == 9);
    }
}
