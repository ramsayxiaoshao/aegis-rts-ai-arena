using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    private bool gameStarted = false;
    private Texture2D backgroundTexture;

    private void Awake()
    {
        backgroundTexture = new Texture2D(2, 2);
        backgroundTexture.SetPixel(0, 0, new Color(0.03f, 0.04f, 0.06f));
        backgroundTexture.SetPixel(1, 0, new Color(0.05f, 0.08f, 0.12f));
        backgroundTexture.SetPixel(0, 1, new Color(0.08f, 0.12f, 0.18f));
        backgroundTexture.SetPixel(1, 1, new Color(0.04f, 0.05f, 0.07f));
        backgroundTexture.Apply();

        Camera.main.orthographic = true;
        Camera.main.transform.position = new Vector3(0, 0, -10);
        Camera.main.orthographicSize = 10;
    }

    private void OnGUI()
    {
        if (!gameStarted)
        {
            DrawMainMenu();
        }
        else
        {
            DrawGameScreen();
        }
    }

    private void DrawMainMenu()
    {
        GUI.DrawTexture(
            new Rect(0, 0, Screen.width, Screen.height),
            backgroundTexture,
            ScaleMode.StretchToFill
        );

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 42;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = Color.white;

        GUI.Label(
            new Rect(0, Screen.height * 0.25f, Screen.width, 80),
            "Aegis RTS AI Arena",
            titleStyle
        );

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 28;

        Rect startButtonRect = new Rect(
            (Screen.width - 220) / 2f,
            Screen.height * 0.5f,
            220,
            70
        );

        if (GUI.Button(startButtonRect, "开始", buttonStyle))
        {
            gameStarted = true;
            Debug.Log("Game started.");
        }
    }

    private void DrawGameScreen()
    {
        GUI.Label(new Rect(20, 20, 400, 40), "游戏地图界面已进入");
    }
}