using System;
using System.IO;
using System.Collections;
using UnityEngine;

namespace FortySevenE
{
    public class Screenshot : MonoBehaviour
    {
        public string screenshotDirectory = "C:\\";
        public string screenshotFileName = "screenshot";
        public bool appendDate = true;
        public string dateFormat = "yy_MM_dd_T_HH_mm_ss";
        [SerializeField] private int _screenshotTextureW = 3840, _screenshotTextureH = 2160;

        [Header("CameraRender")] [SerializeField]
        private Camera _captureCamera;
        public Camera CaptureCamera => _captureCamera ? _captureCamera : Camera.main;
        public RenderTextureFormat renderFormat = RenderTextureFormat.ARGB32;

        public Texture2D ScreenshotTexture { get; private set; }

        public string FileName => appendDate ? $"{screenshotFileName}_{DateTime.Now.ToString(dateFormat)}.png" : $"{screenshotFileName}.png";
        public string FilePath => Path.Combine(screenshotDirectory, FileName);
        
        private void Update() 
        {
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current.rightBracketKey.wasPressedThisFrame)
#else
            if (Input.GetKeyDown(KeyCode.RightBracket))
#endif
            {
                CaptureCameraRenderTarget();
            }
            
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current.leftBracketKey.wasPressedThisFrame)
#else
            if (Input.GetKeyDown(KeyCode.LeftBracket))
#endif
            {
                ScreenCaptureSimple();
            }
        }

        [ContextMenu("CaptureCamTarget")]
        public void CaptureCameraRenderTarget()
        {
            ScreenshotTexture = new Texture2D(_screenshotTextureW, _screenshotTextureH, TextureFormat.RGBA32, false);
            StartCoroutine(c_CapctureCameraRenderTarget());
        }

        [ContextMenu("UnityScreenshot")]
        public void ScreenCaptureSimple()
        {
            ScreenCapture.CaptureScreenshot(FilePath);
            Debug.Log($"[{GetType().Name}] Simple screenshot saved at <b>{FilePath}</b>");
        }

        private IEnumerator c_CapctureCameraRenderTarget()
        {
            yield return new WaitForEndOfFrame();
            RenderTexture transformedRenderTexture = null;
            RenderTexture renderTexture = RenderTexture.GetTemporary(
                _screenshotTextureW,
                _screenshotTextureH,
                24,
                renderFormat,
                RenderTextureReadWrite.Default,
                1);
            try
            {
                CaptureCamera.targetTexture = renderTexture;
                CaptureCamera.Render();
                CaptureCamera.targetTexture = null;
                transformedRenderTexture = RenderTexture.GetTemporary(
                    ScreenshotTexture.width,
                    ScreenshotTexture.height,
                    24,
                    renderFormat,
                    RenderTextureReadWrite.Default,
                    1);
                Graphics.Blit(
                    renderTexture,
                    transformedRenderTexture,
                    new Vector2(1.0f, 1.0f),
                    new Vector2(0.0f, 0.0f));
                RenderTexture.active = transformedRenderTexture;
                ScreenshotTexture.ReadPixels(
                    new Rect(0, 0, ScreenshotTexture.width, ScreenshotTexture.height),
                    0, 0);

                var pixels = ScreenshotTexture.GetPixels();
            }
            catch (Exception e)
            {
                Debug.Log("Exception: " + e);
                yield break;
            }
            finally
            {
                RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(renderTexture);
                if (transformedRenderTexture != null)
                {
                    RenderTexture.ReleaseTemporary(transformedRenderTexture);
                }
            }

            ScreenshotTexture.Apply();

            byte[] bytes;
            bytes = ScreenshotTexture.EncodeToPNG();
            
            File.WriteAllBytes(FilePath, bytes);
            Debug.Log($"[{GetType().Name}] Camera texture saved at <b>{FilePath}</b>");
        }
    }
}