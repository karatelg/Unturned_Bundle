using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

public class UnturnedWindow : EditorWindow
{
    string folderPostfix = "_Unturned";
    string createdPrefabName = "Object";
    string tag;
    int layer;

    bool needCreateNavAndClip = true;
    bool needAddMeshCollider = true;
    bool needAddBoxCollider = true;
    bool needAddLODGroup = true;

    // Add menu item named "My Window" to the Window menu
    [MenuItem("Unturned/Generation Prefab")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(UnturnedWindow));
    }

    [MenuItem("Unturned/Generation .Dat")]
    public static void GenerateDatFiles()
    {
        var obj = Selection.activeObject;
        if (obj == null)
        {
            return;
        }

        var pathToFolder = AssetDatabase.GetAssetPath(obj.GetInstanceID());
        if (pathToFolder.Length > 0)
        {
            if (Directory.Exists(pathToFolder))
            {
                string[] files = Directory.GetDirectories(pathToFolder, "*.*", SearchOption.TopDirectoryOnly);
                int idCounter = 10000;

                string[] prefabFiles = Directory.GetFiles(pathToFolder, "*.prefab", SearchOption.AllDirectories);

                Dictionary<string, bool> checkFolder = new Dictionary<string, bool>(); 
                
                foreach (string prefabFile in prefabFiles)
                {
                    //string assetPath = "Assets" + prefabFile.Replace(Application.dataPath, "").Replace('\\', '/');
                    string assetPath = prefabFile.Replace('\\', '/');

                    bool running = false;
                    string folderReverseName = "";
                    string pathToSave = assetPath;
                    for (int i = assetPath.Length - 1; i >= 0; i--)
                    {
                        if (assetPath[i].Equals('/'))
                        {
                            if (running)
                            {
                                break;
                            }

                            running = true;
                            continue;
                        }

                        if (running)
                        {
                            folderReverseName += assetPath[i];
                        }
                        else
                        {
                            pathToSave = pathToSave.Remove(pathToSave.Length - 1);
                        }
                    }

                    char[] charArray = folderReverseName.ToCharArray();
                    Array.Reverse(charArray);
                    string folderName = new string(charArray);

                    if (checkFolder.ContainsKey(folderName))
                    {
                        continue;
                    }
                    
                    checkFolder.Add(folderName, true);
                    
                    string fullPathToSave = pathToSave + folderName + ".dat";
                    
                    if (!File.Exists(fullPathToSave))
                    {
                        using (FileStream fs = File.Create(fullPathToSave))
                        {
                            byte[] info = new UTF8Encoding(true).GetBytes("");
                            fs.Write(info, 0, info.Length);
                        }
                    }
                    
                    
                    string[] textAllLines = File.ReadAllLines(fullPathToSave);

                    bool isIDAdded = false;
                    bool isTypeAdded = false;
                    string idLine = "ID " + idCounter;
                    idCounter++;
                    
                    for (int i = 0; i < textAllLines.Length; i++)
                    {
                        string word = "";
                        for (int j = 0; j < textAllLines[i].Length; j++)
                        {
                            if (textAllLines[i][j] == ' ')
                            {
                                break;
                            }

                            word += textAllLines[i][j];
                        }

                        if (word == "ID")
                        {
                            textAllLines[i] = idLine;
                            isIDAdded = true;
                        }
                        
                        if (word == "Type")
                        {
                            isTypeAdded = true;
                        }
                    }
                    
                    File.WriteAllLines(fullPathToSave, textAllLines);
                    
                    if (!isIDAdded)
                    {
                        File.AppendAllText(fullPathToSave, idLine + Environment.NewLine);
                    }
                    
                    if (!isTypeAdded)
                    {
                        File.AppendAllText(fullPathToSave, "Type Large");
                    }


                    fullPathToSave = pathToSave + "English.dat";

                    if (!File.Exists(fullPathToSave))
                    {
                        using (FileStream fs = File.Create(fullPathToSave))
                        {
                            byte[] info = new UTF8Encoding(true).GetBytes("");
                            fs.Write(info, 0, info.Length);
                        }
                    }
                    
                    
                    
                    textAllLines = File.ReadAllLines(fullPathToSave);
                    
                    bool isTextAdded = false;
                    
                    for (int i = 0; i < textAllLines.Length; i++)
                    {
                        string word = "";
                        for (int j = 0; j < textAllLines[i].Length; j++)
                        {
                            if (textAllLines[i][j] == ' ')
                            {
                                break;
                            }

                            word += textAllLines[i][j];
                        }

                        if (word == "Name")
                        {
                            isTextAdded = true;
                        }
                    }
                    
                    File.WriteAllLines(fullPathToSave, textAllLines);
                    
                    if (!isTextAdded)
                    {
                        File.AppendAllText(fullPathToSave, "Name " + folderName);
                    }
                }
            }
        }
    }

    void OnGUI()
    {
        //GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        folderPostfix = EditorGUILayout.TextField("Postfix Folder", folderPostfix);
        createdPrefabName = EditorGUILayout.TextField("Created Prefab Name", createdPrefabName);


        tag = EditorGUILayout.TagField(tag);
        layer = EditorGUILayout.LayerField(layer);

        needCreateNavAndClip = EditorGUILayout.Toggle("Create Nav and Clip", needCreateNavAndClip);
        needAddMeshCollider = EditorGUILayout.Toggle("Add MeshCollider", needAddMeshCollider);
        needAddBoxCollider = EditorGUILayout.Toggle("Add BoxCollider", needAddBoxCollider);
        needAddLODGroup = EditorGUILayout.Toggle("Add LOD", needAddLODGroup);

        GUI.enabled = IsButtonEnabled() && tag != null && folderPostfix != "" && createdPrefabName != "";

        if (GUILayout.Button("Generation"))
        {
            GenerationPrefab();
        }
    }

    bool IsButtonEnabled()
    {
        var obj = Selection.activeObject;
        if (obj == null)
        {
            return false;
        }

        var pathToFolder = AssetDatabase.GetAssetPath(obj.GetInstanceID());
        if (pathToFolder.Length == 0)
        {
            return false;
        }

        if (!Directory.Exists(pathToFolder))
        {
            return false;
        }

        string pathToFolderSave = pathToFolder + folderPostfix;
        return !Directory.Exists(pathToFolderSave);
    }

    void ChangePrefab(GameObject prefab)
    {
        prefab.tag = tag;
        prefab.layer = layer;

        if (prefab.GetComponent<LODGroup>() == null && needAddLODGroup)
        {
            prefab.AddComponent<LODGroup>();
        }

        if (prefab.GetComponent<BoxCollider>() == null && needAddBoxCollider)
        {
            prefab.AddComponent<BoxCollider>();
        }

        if (prefab.GetComponent<MeshCollider>() == null && needAddMeshCollider)
        {
            prefab.AddComponent<MeshCollider>();
        }

        var count = prefab.transform.childCount;
        for (int i = 0; i < count; i++)
        {
            ChangePrefab(prefab.transform.GetChild(i).gameObject);
        }
    }

    void GenerationPrefab()
    {
        var obj = Selection.activeObject;
        if (obj == null)
        {
            return;
        }

        var pathToFolder = AssetDatabase.GetAssetPath(obj.GetInstanceID());
        if (pathToFolder.Length > 0)
        {
            if (Directory.Exists(pathToFolder))
            {
                string pathToFolderSave = pathToFolder + folderPostfix;

                if (!Directory.Exists(pathToFolderSave))
                {
                    Directory.CreateDirectory(pathToFolderSave);
                }

                string[] prefabFiles = Directory.GetFiles(pathToFolder, "*.prefab", SearchOption.AllDirectories);
                foreach (string prefabFile in prefabFiles)
                {
                    //string assetPath = "Assets" + prefabFile.Replace(Application.dataPath, "").Replace('\\', '/');
                    string assetPath = prefabFile.Replace('\\', '/');

                    var prefab = PrefabUtility.LoadPrefabContents(assetPath);

                    ChangePrefab(prefab);

                    var assetPathToFolderSave =
                        assetPath.Replace(pathToFolder, pathToFolderSave).Replace(".prefab", "");

                    if (!Directory.Exists(assetPathToFolderSave))
                    {
                        Directory.CreateDirectory(assetPathToFolderSave);
                    }

                    string assetPathSave = assetPathToFolderSave + "/" + createdPrefabName + ".prefab";

                    PrefabUtility.SaveAsPrefabAsset(prefab, assetPathSave);


                    if (needCreateNavAndClip)
                    {
                        foreach (Transform child in prefab.transform)
                        {
                            DestroyImmediate(child.gameObject);
                        }

                        foreach (var comp in prefab.GetComponents<Component>())
                        {
                            if (comp is Transform)
                            {
                                continue;
                            }

                            if (comp is MeshCollider)
                            {
                                continue;
                            }

                            if (comp is BoxCollider)
                            {
                                continue;
                            }

                            DestroyImmediate(comp);
                        }

                        assetPathSave = assetPathToFolderSave + "/Nav.prefab";
                        prefab.tag = "Navmesh";
                        prefab.layer = 22;
                        PrefabUtility.SaveAsPrefabAsset(prefab, assetPathSave);


                        assetPathSave = assetPathToFolderSave + "/Clip.prefab";
                        prefab.tag = "Clip";
                        prefab.layer = 21;
                        PrefabUtility.SaveAsPrefabAsset(prefab, assetPathSave);
                    }


                    PrefabUtility.UnloadPrefabContents(prefab);

                    // {
                    //     var prefabRoot = editingScope.prefabContentsRoot;
                    //     prefabRoot.AddComponent<MeshCollider>();

                    // Removing GameObjects is supported
                    // Object.DestroyImmediate(prefabRoot.transform.GetChild(2).gameObject);
                    //
                    // // Reordering and reparenting are supported
                    // prefabRoot.transform.GetChild(1).parent = prefabRoot.transform.GetChild(0);
                    //
                    // // Adding GameObjects is supported
                    // var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    // cube.transform.parent = prefabRoot.transform;
                    // cube.name = "D";
                    //
                    // // Adding and removing components are supported
                    // prefabRoot.AddComponent<AudioSource>();
                    // }
                }
            }
            else
            {
                Debug.Log("File");
            }
        }
        else
        {
            Debug.Log("Not in assets folder");
        }
    }
}