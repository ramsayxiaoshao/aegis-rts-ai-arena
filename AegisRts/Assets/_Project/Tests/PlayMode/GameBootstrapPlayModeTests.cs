using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed class GameBootstrapPlayModeTests
{
    [UnityTest]
    public IEnumerator MainScene_CreatesUguiAndStartsGame()
    {
        SceneManager.LoadScene("Main");
        yield return null;

        GameObject ui = GameObject.Find("RtsGameUI");
        Assert.IsNotNull(ui);
        Assert.IsNotNull(ui.GetComponent<Canvas>());

        GameObject startObject = GameObject.Find("Start");
        Assert.IsNotNull(startObject);
        startObject.GetComponent<Button>().onClick.Invoke();
        yield return null;

        Assert.IsNotNull(GameObject.Find("GridRoot"));
        Assert.IsNotNull(GameObject.Find("BuildingRoot"));
    }
}
