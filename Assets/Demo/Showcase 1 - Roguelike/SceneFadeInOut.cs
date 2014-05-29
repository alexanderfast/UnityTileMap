using System;
using UnityEngine;

/// <summary>
/// Based on https://unity3d.com/learn/tutorials/projects/stealth/screen-fader
/// </summary>
public class SceneFadeInOut : MonoBehaviour
{
    public float fadeSpeed = 1.5f; // Speed that the screen fades to and from black.

    public float fadeMargin = 0.01f; // How close to target alpha has to be for the routine to stop.

    private State m_state = State.FadingToClear;

    // Called when fade is complete, either in or out
    public Action FadeCompleteCallback { get; set; }

    public bool IsFading
    {
        get { return m_state != State.Inactive; }
    }

    public void FadeToClear()
    {
        m_state = State.FadingToClear;
    }

    public void FadeToBlack()
    {
        m_state = State.FadingToBlack;
    }

    // Helper method.
    // Fades the scene to black, calls the callback and then fades in again.
    public void FadeOutThenIn(Action callback)
    {
        FadeCompleteCallback = () =>
            {
                FadeCompleteCallback = null;
                if (callback != null)
                    callback();
                FadeToClear();
            };
        FadeToBlack();
    }

    private void Awake()
    {
        // Set the texture so that it is the the size of the screen and covers it.
        guiTexture.pixelInset = new Rect(0f, 0f, Screen.width, Screen.height);

        // If the user hasn't specifically set a certain texture, create a black one.
        if (guiTexture.texture == null)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGB24, false);
            texture.SetPixel(0, 0, Color.black);
            guiTexture.texture = texture;
        }
    }

    private void Update()
    {
        switch (m_state)
        {
            case State.FadingToClear:
                FadingToClear();
                break;
            case State.FadingToBlack:
                FadingToBlack();
                break;
        }
    }

    private void FadingToClear()
    {
        // Fade the texture to clear.
        // Lerp the colour of the texture between itself and transparent.
        guiTexture.color = Color.Lerp(guiTexture.color, Color.clear, fadeSpeed * Time.deltaTime);

        // If the texture is almost clear...
        if (guiTexture.color.a <= fadeMargin)
        {
            // ... set the colour to clear and disable the GUITexture.
            guiTexture.color = Color.clear;
            guiTexture.enabled = false;

            // The scene is no longer starting.
            FadeComplete();
        }
    }

    private void FadingToBlack()
    {
        // Make sure the texture is enabled.
        guiTexture.enabled = true;

        // Lerp the colour of the texture between itself and black.
        guiTexture.color = Color.Lerp(guiTexture.color, Color.black, fadeSpeed * Time.deltaTime);

        // If the texture is almost black...
        if (guiTexture.color.a >= (1f - fadeMargin))
            FadeComplete();
    }

    private void FadeComplete()
    {
        m_state = State.Inactive;
        if (FadeCompleteCallback != null)
            FadeCompleteCallback();
    }

    private enum State
    {
        Inactive,
        FadingToClear,
        FadingToBlack
    }
}
