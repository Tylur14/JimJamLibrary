using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using Debug = UnityEngine.Debug;

/// <summary>
/// The Jim Jam Library Core
/// A simple controller for handling files you may want to use across several projects.
/// Say you have a door script that is simple and can be used on multiple projects, but on this other project you
/// |-> fix bugs or add a new feature that you want to use on the first project. So with this library you can add
/// |-> it to the library or update it if it's already in the project.
///
/// Features:
/// |- Manage a single location for resuable scripts
/// |- Get status of each file
/// |- Push updates to the library
/// |- Pull newer versions from the library
/// |- Add new files to library with context menu in project tab
/// </summary>
#if UNITY_EDITOR

public class Resource
{
    public enum ResourceStates
    {
        Same,
        Newer,
        Older,
        NoLocalCopy
    }

    public ResourceStates state;
    public string filePath;
    public string fileName;
}
public class JimJam_Library : EditorWindow
{
    private static string _libraryPath;
    private static List<String> _tasks = new List<string>();
    //private int _textMaxLength = 64;
    Vector2 _scrollPosition;
    List<Resource> _resources = new List<Resource>();
    private bool _updating;
    [MenuItem("JimJam/Library %g")]
    public static void ShowWindow()
    {
        var window = GetWindow<JimJam_Library>("Jim-Jam Library");
        window.minSize = new Vector2(380, 425);
    }

    private void OnGUI()
    {
        _scrollPosition = GUILayout.BeginScrollView(
            _scrollPosition, GUILayout.Height(350));
        
        
        if (GUILayout.Button("Check for Updates", GUILayout.Height(25),GUILayout.Width(175)))
            CheckForUpdates();
        if (GUILayout.Button("Open Directory", GUILayout.Height(25), GUILayout.Width(125)))
            Process.Start(_libraryPath);
        
        // Go through each resource and create a section for it
        if (_resources != null && !_updating)
            foreach (var file in _resources.ToList())
            {
                GUILayout.BeginHorizontal();
                string displayText = file.fileName;
                GUILayout.Label(displayText,GUILayout.Height(25));
                
                // Condition - The Library version of this resource is the SAME
                if (file.state == Resource.ResourceStates.Same)
                {
                    GUI.contentColor = Color.green;
                    GUILayout.Label("You have most recent file",GUILayout.Height(25), GUILayout.Width(155));
                }
                
                // Condition - The Library version of this resource is NEWER
                else if(file.state == Resource.ResourceStates.Newer)
                {
                    GUI.contentColor = Color.red;
                    GUILayout.Label("Needs to be updated",GUILayout.Height(25), GUILayout.Width(155));
                }
                
                // Condition - The Library version of this resource is OLDER 
                else if(file.state == Resource.ResourceStates.Older)
                {
                    GUI.contentColor = Color.yellow;
                    GUILayout.Label("You have a newer version",GUILayout.Height(25), GUILayout.Width(155));
                }
                
                // Condition - No Local Copy of Library Resource is found
                else if(file.state == Resource.ResourceStates.NoLocalCopy)
                {
                    GUI.contentColor = Color.gray;
                    GUILayout.Label("No local copy",GUILayout.Height(25), GUILayout.Width(155));
                }
                
                GUI.contentColor = Color.white;
                if (GUILayout.Button("Push", GUILayout.Height(25), GUILayout.Width(45)))
                {
                    PushToLibrary(file.filePath,file.fileName);
                    continue;
                }

                if (GUILayout.Button("Get", GUILayout.Height(25), GUILayout.Width(75)))
                {
                    PullFromLibrary(file.filePath,file.fileName);
                    continue;
                }
                GUILayout.EndHorizontal();
            }
        GUILayout.EndScrollView();
        if (GUILayout.Button("Backup", GUILayout.Height(30), GUILayout.Width(125)))
            PushToBackups();
        
    }

    void PushToBackups()
    {
        var files = Directory.GetFiles(_libraryPath).ToList();
        foreach (var f in files)
        {
            if (f.Contains(".cs"))
            {
                File.Copy(f,_libraryPath+"/JimJamLibrary/"+f.Substring(f.LastIndexOf('\\') + 1),true);
            }
        }
    }

    void PushToLibrary(string fp,string fn)
    {
        string dataPath = Application.dataPath;
        var localCopy = Directory.GetFiles(dataPath, fn, SearchOption.AllDirectories);
        File.Copy(localCopy[0],fp,true);
        AssetDatabase.Refresh();
        CheckForUpdates();
    }
    
    void PullFromLibrary(string fp,string fn)
    {
        string dataPath = Application.dataPath;
        var localCopy = Directory.GetFiles(dataPath, fn, SearchOption.AllDirectories);
        if (localCopy.Length > 0)
        {
            File.Copy(fp, localCopy[0],true);
            //File.Replace(fp, s[0],s[0]);    
        }
        else File.Copy(fp, dataPath+"/"+fn);
        AssetDatabase.Refresh();
        CheckForUpdates();
        
    }

    void CheckForUpdates()
    {
        _updating = true;
        _resources.Clear();
        var files = Directory.GetFiles(_libraryPath).ToList();
        foreach (var f in files)
        {
            if (f.Contains(".cs"))
            {
                var r = new Resource {filePath = f, fileName = f.Substring(f.LastIndexOf('\\') + 1)};
                r.state = CompareVersions(r.filePath, r.fileName);
                _resources.Add(r);
            }
        }
        _updating = false;
    }

    Resource.ResourceStates CompareVersions(string fp,string fn)
    {
        string dataPath = Application.dataPath;
        var localCopy = Directory.GetFiles(dataPath, fn, SearchOption.AllDirectories);
        if (localCopy.Length > 0)
        {
            DateTime lastModified = File.GetLastWriteTime(fp);
            
            if(lastModified > File.GetLastWriteTime(localCopy[0]))
                return Resource.ResourceStates.Newer;
            if(lastModified == File.GetLastWriteTime(localCopy[0]))
                return Resource.ResourceStates.Same;
            
            return Resource.ResourceStates.Older;    
        }
        return Resource.ResourceStates.NoLocalCopy;
        
    }
    

    /* https://stackoverflow.com/questions/20445426/how-to-show-an-ellipses-at-the-end-of-the-text-in-a-textarea
    public static string TruncateAtWord(string input, int length)
    {
        // verify that input is either empty or less than the max character count
        if (input == null || input.Length < length)
            return input;
        int iNextSpace = input.LastIndexOf(" ", length, StringComparison.Ordinal);

        return string.Format("{0}...", input.Substring(0, (iNextSpace > 0) ? iNextSpace : length).Trim());
    } */

    private void OnValidate()
    {
        _libraryPath = Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData) + @"\JimJam\LibraryPackages";
        if (!Directory.Exists(_libraryPath))
            Directory.CreateDirectory(_libraryPath);
        CheckForUpdates();
    }
    
    [MenuItem("Assets/JJL - Send to Library", false, 0)]
    static void SendFileToLibrary() {
        var obj = Selection.activeObject;
        var path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
        if (path.Length > 0 && path.Contains(".cs"))
        {
            Debug.Log(path);
            string newFileName = path.Substring(7);
            if (File.Exists(path))
            {
                File.Copy(path,_libraryPath+"/"+newFileName,true);
                AssetDatabase.Refresh();
            }
        }
        else
        {
            Debug.LogWarning("JimJamLibrary Warning! " + path + " is not a supported file type");
        }
        AssetDatabase.Refresh();
    }
}



#endif