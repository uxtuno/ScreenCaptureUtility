using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class CaptureScreenShot : MonoBehaviour
{
	System.Action<byte[]> onCaptureCompleted;
	public RenderTexture renderTexture;
	int CaptureHeight;
	int CaptureWidth;

	public void capture(int width, int height, System.Action<byte[]> onCompleted)
	{
		if (onCaptureCompleted != null)
		{
			Debug.LogWarning("多重呼び出し禁止");
			return;
		}

		onCaptureCompleted = onCompleted;
		CaptureHeight = height;
		CaptureWidth = width;

		StartCoroutine(captureCoroutine());
	}

	IEnumerator captureCoroutine()
	{
		// レンダリング完了を待つ
		yield return null;

		renderTexture = new RenderTexture(CaptureWidth, CaptureHeight, 256, GraphicsFormat.R8G8B8A8_UNorm);

		var old = RenderTexture.active;
		RenderTexture.active = renderTexture;
		if (Camera.main != null)
		{
			foreach (var camera in Camera.allCameras)
			{
				camera.Render();
			}
		}

		yield return new WaitForEndOfFrame();

		// Texture2D.ReadPixels()によりアクティブなレンダーテクスチャのピクセル情報をテクスチャに格納する
		var texture = new Texture2D(CaptureWidth, CaptureHeight);
		texture.ReadPixels(new Rect(0, 0, CaptureWidth, CaptureHeight), 0, 0);
		texture.Apply();

		if (onCaptureCompleted != null)
		{
			onCaptureCompleted(texture.EncodeToPNG());
			onCaptureCompleted = null;
		}

		yield return new WaitForEndOfFrame();

		//RenderTexture.active = old;

		//ScreenCapture.CaptureScreenshotIntoRenderTexture(renderTexture);
		//AsyncGPUReadback.Request(renderTexture, 0, GraphicsFormat.R8G8B8A8_UNorm, onReadbackCompleted);

	}

	void onReadbackCompleted(AsyncGPUReadbackRequest request)
	{
		DestroyImmediate(renderTexture);
		var pngImage = ImageConversion.EncodeNativeArrayToPNG(request.GetData<byte>(), GraphicsFormat.R8G8B8A8_UNorm, (uint)CaptureWidth, (uint)CaptureHeight);

		if (onCaptureCompleted != null)
		{
			onCaptureCompleted(pngImage.ToArray());
			onCaptureCompleted = null;
		}
	}
}
