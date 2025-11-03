using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine.Rendering.HighDefinition;

public class GradientShaderEditor : ShaderGUI
{
	/// <summary>
	/// The string used to identify gradient texture properties in the shader.
	/// </summary>
	private const string GradientRegex = "GradientTexture";

	/// <summary>
	/// The string used to identify foldout sections in the shader property names.
	/// </summary>
	private const string FoldoutRegex = "Foldout";

	/// <summary>
	/// The character used as a separator in property names.
	/// </summary>
	private const char Separator = '_';

	/// <summary>
	/// Represents the darkness level applied to the background of foldout UI elements in the editor.
	/// </summary>
	private const float FoldoutBackgroundDarkness = 0.1f;


	private readonly GradientGUIDrawer gradientGUIDrawer = new GradientGUIDrawer();

	private readonly Dictionary<string, bool> foldouts;

	public GradientShaderEditor()
	{
		foldouts = new Dictionary<string, bool>();
	}

	private bool IncludesKey(string value, string key) => value.ToLower().Contains(key);
	private string GetWithout(string value, string without)
		=> Regex.Replace(value, without, "", RegexOptions.IgnoreCase);

	private bool IsGradient(MaterialProperty property) =>
		property.propertyType == ShaderPropertyType.Texture
		&& IncludesKey(property.name, GradientRegex.ToLower());


	private IEnumerable<Texture> GetTextures(Material target)
	{
		// Get all assets that are stored inside the material file
		string assetPath = AssetDatabase.GetAssetPath(target);
		if (string.IsNullOrEmpty(assetPath))
			yield break;

		Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

		foreach (Object obj in subAssets)
		{
			if (obj is Texture texture)
				yield return texture;
		}
	}

#if UNITY_EDITOR
	private void Cleanup(Material target, MaterialProperty[] properties)
	{
		var gradients = properties.Where(p => IsGradient(p));
		var allTextures = GetTextures(target).Where(t => IncludesKey(t.name, GradientRegex.ToLower()));
		var used = allTextures.Where(t => gradients.Any(g => target.GetTexture(g.name)?.name.Split()[0].Equals(t.name) ?? false));
		var unused = allTextures.Where(t => !used.Any(t2 => t2.name.Equals(t.name)));

		foreach (var texture in unused)
		{
			AssetDatabase.RemoveObjectFromAsset(texture);
			AssetDatabase.SaveAssets();
		}

		AssetDatabase.Refresh();
	}


	public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
	{
		EditorGUI.BeginChangeCheck(); // Track changes

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Expand All"))
			foldouts.ToList().ForEach(f => foldouts[f.Key] = true);
		if (GUILayout.Button("Close All"))
			foldouts.ToList().ForEach(f => foldouts[f.Key] = false);
		EditorGUILayout.Space();
		if (GUILayout.Button(new GUIContent("Cleanup", "Removes unused gradient textures.")))
			Cleanup((Material)editor.target, properties);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();
		EditorGUILayout.Space();

		var cache = GUI.backgroundColor;

		bool hasBegun = false;
		bool show = true;
		foreach (MaterialProperty property in properties)
		{
			string name = property.name;

			bool Includes(string key) => IncludesKey(name, key);

			bool hideInInspector = (property.propertyFlags & ShaderPropertyFlags.HideInInspector) != 0;
			if (hideInInspector)
				continue;

			string displayName = property.displayName;

			if (property.propertyType == ShaderPropertyType.Float && Includes(FoldoutRegex.ToLower()))
			{
				if (hasBegun)
					EditorGUILayout.EndFoldoutHeaderGroup();

				EditorGUILayout.Space();
				EditorGUILayout.Space();

				if (!foldouts.ContainsKey(name))
					foldouts.Add(name, true);
				GUI.backgroundColor = Color.Lerp(cache, Color.black, FoldoutBackgroundDarkness);
				foldouts[name] = EditorGUILayout.BeginFoldoutHeaderGroup(foldouts[name], new GUIContent(GetWithout(displayName, Separator + FoldoutRegex.ToLower())));
				GUI.backgroundColor = cache;
				hasBegun = true;

				show = foldouts[name];

				continue;
			}

			if (!show) continue;

			if (IsGradient(property))
			{
				gradientGUIDrawer.OnGUI(Rect.zero, property, new GUIContent(displayName, ""), editor);
			}
			else
			{
				editor.ShaderProperty(property, displayName);
				if (property.propertyType == ShaderPropertyType.Vector)
					EditorGUILayout.Space(-EditorGUIUtility.singleLineHeight);
			}
		}

		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		if (SupportedRenderingFeatures.active.editableMaterialRenderQueue)
			editor.RenderQueueField();
		editor.EnableInstancingField();
		editor.DoubleSidedGIField();

		// Apply changes if properties were modified
		if (EditorGUI.EndChangeCheck())
		{
			foreach (Object obj in editor.targets)
			{
				EditorUtility.SetDirty(obj); // Mark materials as modified
			}
		}
#endif
	}
}