using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

//***********************************************************************************
// Contributed by author jl-randazzo github.com/jl-randazzo
//***********************************************************************************
namespace NavMeshPlus.Components.Editors
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NavMeshModifierTilemap))]
    class NavMeshModifierTilemapEditor : Editor
    {
        SerializedProperty m_AffectedAgents;
        SerializedProperty m_TileModifiers;

        void OnEnable()
        {
            m_AffectedAgents = serializedObject.FindProperty("m_AffectedAgents");
            m_TileModifiers = serializedObject.FindProperty("m_TileModifiers");
        }

        public override void OnInspectorGUI()
        {
            NavMeshModifierTilemap modifierTilemap = target as NavMeshModifierTilemap;

            serializedObject.Update();

            NavMeshComponentsGUIUtility.AgentMaskPopup("Affected Agents", m_AffectedAgents);

            EditorGUILayout.PropertyField(m_TileModifiers);

            if (modifierTilemap.HasDuplicateTileModifiers())
            {
                EditorGUILayout.HelpBox("There are duplicate Tile entries in the tilemap modifiers! Only the first will be used.", MessageType.Warning);
            }

            EditorGUILayout.Space();

            Tilemap tilemap = modifierTilemap.GetComponent<Tilemap>();
            if (tilemap)
            {
                if (GUILayout.Button("Add Used Tiles"))
                {
                    AddUsedTiles(tilemap, modifierTilemap);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Missing required component 'Tilemap'", MessageType.Error);
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                modifierTilemap.CacheModifiers();
            }
        }

        private void AddUsedTiles(Tilemap tilemap, NavMeshModifierTilemap modifierTilemap)
        {
            Dictionary<TileBase, NavMeshModifierTilemap.TileModifier> tileModifiers = modifierTilemap.GetModifierMap();

            BoundsInt bounds = tilemap.cellBounds;
            for (int i = bounds.xMin; i <= bounds.xMax; i++)
            {
                for (int j = bounds.yMin; j <= bounds.yMax; j++)
                {
                    for (int k = bounds.zMin; k <= bounds.zMax; k++)
                    {
                        if (tilemap.GetTile(new Vector3Int(i, j, k)) is TileBase tileBase)
                        {
                            if (!tileModifiers.ContainsKey(tileBase))
                            {
                                tileModifiers.Add(tileBase, new NavMeshModifierTilemap.TileModifier());

                                int idx = m_TileModifiers.arraySize;
                                m_TileModifiers.InsertArrayElementAtIndex(idx);
                                var newElem = m_TileModifiers.GetArrayElementAtIndex(idx);
                                var tileProperty = newElem.FindPropertyRelative(nameof(NavMeshModifierTilemap.TileModifier.tile));
                                tileProperty.objectReferenceValue = tileBase;
                            }
                        }
                    }
                }
            }
        }

        [CustomPropertyDrawer(typeof(NavMeshModifierTilemap.TileModifier))]
        class TileModifierPropertyDrawer : PropertyDrawer
        {
            
            private static Dictionary<Object, Texture2D> Previews;
            
            private Rect ClaimAdvance(ref Rect position, float height)
            {
                Rect retVal = position;
                retVal.height = height;
                position.y += height;
                position.height -= height;
                return retVal;
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                Rect expandRect = ClaimAdvance(ref position, 20);
                property.isExpanded = EditorGUI.Foldout(expandRect, property.isExpanded, label);
                if (property.isExpanded)
                {
                    var tileProperty = property.FindPropertyRelative(nameof(NavMeshModifierTilemap.TileModifier.tile));
                    Rect tileRect = ClaimAdvance(ref position, 40);
                    tileRect.width -= 40;

                    Rect previewRect = tileRect;
                    previewRect.width = 40;
                    previewRect.x += tileRect.width;
                    tileRect.height /= 2;

                    // Adding the tile selector and a preview image.
                    EditorGUI.PropertyField(tileRect, tileProperty);
                    TileBase tileBase = tileProperty.objectReferenceValue as TileBase;
                    TileData tileData = new TileData();
                    Texture textureToDraw;
                    try
                    {
                        tileBase?.GetTileData(Vector3Int.zero, null, ref tileData);
                        textureToDraw = tileData.sprite?.texture;
                    }
                    catch
                    {
                        try
                        {
                            
                            textureToDraw = GetPreview(tileBase);
                        }
                        catch
                        {
                            textureToDraw = EditorGUIUtility.IconContent("console.erroricon.sml").image;
                        }
                    }

                    if (textureToDraw)
                    {
                        EditorGUI.DrawPreviewTexture(previewRect, textureToDraw, null, ScaleMode.ScaleToFit, 0);
                    }

                    Rect toggleRect = ClaimAdvance(ref position, 20);
                    var overrideAreaProperty = property.FindPropertyRelative(nameof(NavMeshModifierTilemap.TileModifier.overrideArea));
                    EditorGUI.PropertyField(toggleRect, overrideAreaProperty);

                    if (overrideAreaProperty.boolValue)
                    {
                        Rect areaRect = ClaimAdvance(ref position, 20);
                        var areaProperty = property.FindPropertyRelative(nameof(NavMeshModifierTilemap.TileModifier.area));
                        EditorGUI.indentLevel++;
                        EditorGUI.PropertyField(areaRect, areaProperty);
                        EditorGUI.indentLevel--;
                    }
                }
            }

            static Texture2D GetPreview(Object objectToPreview)
            {
                int maxResolution = 128;
                Previews ??= new();
                if (!Previews.TryGetValue(objectToPreview, out var preview) || preview == null)
                {
                    var path = AssetDatabase.GetAssetPath(objectToPreview);
                    if (objectToPreview)
                    {
                        var editor = CreateEditor(objectToPreview);
                        preview = editor.RenderStaticPreview(path, null, maxResolution, maxResolution);
                        preview.Apply();
                        DestroyImmediate(editor);
                        Previews[objectToPreview] = preview;
                    }
                }

                return preview;
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                if (property.isExpanded)
                {
                    var overrideAreaProperty = property.FindPropertyRelative(nameof(NavMeshModifierTilemap.TileModifier.overrideArea));
                    if (overrideAreaProperty.boolValue)
                    {
                        return 100;
                    }
                    return 80;
                }
                return 20;

            }
        }
    }
}
