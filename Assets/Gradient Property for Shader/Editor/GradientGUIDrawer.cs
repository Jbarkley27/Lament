using System;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Object = UnityEngine.Object;

#if UNITY_EDITOR
public class GradientGUIDrawer : MaterialPropertyDrawer
{
	/// <summary>
	/// The resolution used for the gradient texture.
	/// </summary>
	private readonly int resolution = 256;


	public override void OnGUI(Rect rect, MaterialProperty property, GUIContent label, MaterialEditor editor)
	{
		EditorGUI.BeginChangeCheck();

		string textureName = $"{property.name}_Texture";

		Gradient gradient = null;
		if (property.targets.Length == 1)
		{
			Object target = (Material)property.targets[0];
			string path = AssetDatabase.GetAssetPath(target);
			var textureAsset = LoadTexture(path, textureName);
			if (textureAsset != null)
				gradient = Decode(property, textureAsset.name, textureName);

			gradient ??= GetNewGradient();

			EditorGUI.showMixedValue = false;
		}
		else
			EditorGUI.showMixedValue = true;

		gradient = EditorGUILayout.GradientField(label, gradient);

		if (EditorGUI.EndChangeCheck())
		{
			string encodedGradient = Encode(gradient);
			string fullAssetName = textureName + encodedGradient;
			foreach (Object target in property.targets)
			{
				if (!AssetDatabase.Contains(target))
					continue;

				string path = AssetDatabase.GetAssetPath(target);
				FilterMode filterMode = gradient.mode == GradientMode.Blend ? FilterMode.Bilinear : FilterMode.Point;
				Texture2D textureAsset = GetTexture(path, textureName, filterMode);
				Undo.RecordObject(textureAsset, "Change Material Gradient");
				textureAsset.name = fullAssetName;
				BakeGradient(gradient, textureAsset);

				Material material = (Material)target;
				material.SetTexture(property.name, textureAsset);
				EditorUtility.SetDirty(material);
			}
		}

		EditorGUI.showMixedValue = false;
	}


	private Gradient GetNewGradient()
	{
		var colorKeys = new GradientColorKey[2];
		var alphaKeys = new GradientAlphaKey[2];
		colorKeys[0] = new GradientColorKey(Color.black, 0f);
		alphaKeys[0] = new GradientAlphaKey(1, 0f);
		colorKeys[1] = new GradientColorKey(Color.white, 1f);
		alphaKeys[1] = new GradientAlphaKey(1, 1f);

		return new Gradient { colorKeys = colorKeys, alphaKeys = alphaKeys };
	}


	private Texture2D GetTexture(string path, string name, FilterMode filterMode)
	{
		Texture2D textureAsset = LoadTexture(path, name);

		if (textureAsset == null)
			textureAsset = CreateTexture(path, name, filterMode);

		textureAsset.filterMode = filterMode;

		if (textureAsset.width != resolution)
		{
#if UNITY_2021_2_OR_NEWER
			textureAsset.Reinitialize(resolution, 1);
#else
            textureAsset.Resize(resolution, 1);
#endif
		}

		return textureAsset;
	}

	private Texture2D CreateTexture(string path, string name, FilterMode filterMode)
	{
		Texture2D textureAsset = new Texture2D(resolution, 1, TextureFormat.ARGB32, false)
		{
			name = name,
			wrapMode = TextureWrapMode.Clamp,
			filterMode = filterMode
		};
		AssetDatabase.AddObjectToAsset(textureAsset, path);
		AssetDatabase.SaveAssets();
		AssetDatabase.ImportAsset(path);

		return textureAsset;
	}

	private string Encode(Gradient gradient) => gradient == null ? null : JsonUtility.ToJson(new GradientRepresentation(gradient));

	private Gradient Decode(MaterialProperty prop, string name, string textureName)
	{
		if (prop == null)
			return null;

#pragma warning disable 0168
		string json = name.Substring(textureName.Length);
		try
		{
			GradientRepresentation gradientRepresentation = JsonUtility.FromJson<GradientRepresentation>(json);
			return gradientRepresentation?.ToGradient(new Gradient());
		}
		catch (Exception _)
		{
			return null;
		}
#pragma warning restore 0168
	}

	private Texture2D LoadTexture(string path, string name) => AssetDatabase.LoadAllAssetsAtPath(path).FirstOrDefault(asset => asset.name.StartsWith(name)) as Texture2D;

	private void BakeGradient(Gradient gradient, Texture2D texture)
	{
		if (gradient == null)
			return;

		for (int x = 0; x < texture.width; x++)
		{
			Color color = gradient.Evaluate((float)x / (texture.width - 1));
			for (int y = 0; y < texture.height; y++)
				texture.SetPixel(x, y, color);
		}

		texture.Apply();
	}


	[Serializable]
	class GradientRepresentation
	{
		public GradientMode mode;
		public ColorKey[] colorKeys;
		public AlphaKey[] alphaKeys;

		public GradientRepresentation()
		{

		}

		public GradientRepresentation(Gradient source)
		{
			FromGradient(source);
		}

		public void FromGradient(Gradient source)
		{
			mode = source.mode;
			colorKeys = source.colorKeys.Select(key => new ColorKey(key)).ToArray();
			alphaKeys = source.alphaKeys.Select(key => new AlphaKey(key)).ToArray();
		}

		public Gradient ToGradient(Gradient gradient)
		{
			gradient.mode = mode;
			gradient.colorKeys = colorKeys.Select(key => key.ToGradientKey()).ToArray();
			gradient.alphaKeys = alphaKeys.Select(key => key.ToGradientKey()).ToArray();

			return gradient;
		}

		[Serializable]
		public struct ColorKey
		{
			public Color color;
			public float time;

			public ColorKey(GradientColorKey source)
			{
				color = default;
				time = default;
				FromGradientKey(source);
			}

			public void FromGradientKey(GradientColorKey source)
			{
				color = source.color;
				time = source.time;
			}

			public GradientColorKey ToGradientKey()
			{
				GradientColorKey key;
				key.color = color;
				key.time = time;
				return key;
			}
		}

		[Serializable]
		public struct AlphaKey
		{
			public float alpha;
			public float time;

			public AlphaKey(GradientAlphaKey source)
			{
				alpha = default;
				time = default;
				FromGradientKey(source);
			}

			public void FromGradientKey(GradientAlphaKey source)
			{
				alpha = source.alpha;
				time = source.time;
			}

			public GradientAlphaKey ToGradientKey()
			{
				GradientAlphaKey key;
				key.alpha = alpha;
				key.time = time;
				return key;
			}
		}
	}
}
#endif