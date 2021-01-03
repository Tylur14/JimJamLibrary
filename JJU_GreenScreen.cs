using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// JimJam Utilities - GreenScreen
///    !!! WARNING: CURRENTLY DOES NOT FUNCTION OUTSIDE OF THE UNITY EDITOR !!!
///     |-> if you wish to implement this outside of the unity editor
///     |-> please remove the '#if UNITY_EDITOR & #endif' and reapply those
///     |-> in areas that refresh the asset database, otherwise it will
///     |-> cause build errors.
/// </summary>

/// <summary>
/// TODO:
/// * Add default folder location options (i.e. save to assets, desktop, or other)
/// * Add option for opening to folder location after generating images
/// * Fix issue when trying to generate new folder at custom path
/// </summary>
    //   =================================================================================================================================
#if UNITY_EDITOR
[ExecuteAlways]
public class JJU_GreenScreen : MonoBehaviour
{
//    ===========================================    
//    ================ VARIABLES ================
//    ===========================================
    [Header("Preview Settings")]
    [SerializeField]
     private RectTransform previewPanel;
    [Range(1, 1920)] [SerializeField]
     private int snapshotWidth;
    [Range(1, 1080)] [SerializeField]
     private int snapshotHeight;
    [Range(0.0f,1.0f)] [SerializeField]
     private float previewTransparency = 0.5f; 
     
    //   =================================================================================================================================
    [Header("Camera Settings")]
    [SerializeField] private bool lookAtTarget;
    [SerializeField] private int rotationCount = 8; // if > 0, rotate after each snapshot and decrement
    
    //   =================================================================================================================================
    [Header("Export Settings")] 
    [Tooltip("Where to export snapshots. Leave blank to export into project.")]
    [SerializeField] private string exportDirectory;
    [Tooltip("Name of exported snapshot.")]
    [SerializeField] private string exportFileName;
    [Tooltip("Creates a folder to sort snapshots into. Leave blank to prevent making a new folder.")]
    [SerializeField] private string categoryFolderName;
    
    //   =================================================================================================================================
    [Header("Snapshot Settings")]
    [SerializeField] private Transform target;
    
    //   ============================================================
    // Private variables that shouldn't be displayed in the inspector
    private Camera _cam;
    private bool _takeScreenshotOnNextFrame;
    private float _rotAmount;
    private int _rotIndex;
    private string _dataPath;

    [HideInInspector] public Vector3 resetPosition; 
    [HideInInspector] public Vector3 resetRotation; 
    //   =================================================================================================================================

//    ===========================================
//    ================ FUNCTIONS ================
//    ===========================================
    public void Update()
    {
        SetPreviewOutline();
        // Shoutout to this thread for helping bring this project to fruition making it able to function entirely in edit mode
        // https://forum.unity.com/threads/solved-how-to-force-update-in-edit-mode.561436/
        EditorApplication.delayCall += EditorApplication.QueuePlayerLoopUpdate;
    }

    //   =================================================================================================================================
    void SetPreviewOutline()
    {
        if (previewPanel)
        {
            previewPanel.sizeDelta = new Vector2(snapshotWidth, snapshotHeight);
            previewPanel.GetComponent<Image>().color = new Color(1, 1, 1, previewTransparency);
        }
    }

    //   =================================================================================================================================
    public void SnapScreenshot()
    {
        _cam.targetTexture = RenderTexture.GetTemporary(snapshotWidth,snapshotHeight,16);
        _rotAmount = (float)360 / (float)rotationCount;
        _takeScreenshotOnNextFrame = true;
    }
    //   =================================================================================================================================
    private void OnPostRender()
    {
        if (!_takeScreenshotOnNextFrame)
        {
            return;
        }
        if (_takeScreenshotOnNextFrame)
        {
            _takeScreenshotOnNextFrame = false;
            
            // GET TEXTURE TO SAVE AS IMAGE
            RenderTexture rTex = _cam.targetTexture;
            Texture2D rResult = new Texture2D(rTex.width,rTex.height,TextureFormat.ARGB32,false);
            Rect rect = new Rect(0, 0, rTex.width,rTex.height);
            
            rResult.ReadPixels(rect,0,0);
            byte[] byteArray = rResult.EncodeToPNG();

            // NAME FILE
            string fileName = GetFileName();

            // SET FILE PATH TO SAVE TO
            string exPath = GetFilePath();
            // WRITE TO FILE
            File.WriteAllBytes(exPath + "/" + fileName , byteArray);

            Nudge();
            _rotIndex++;
            RenderTexture.ReleaseTemporary(rTex);
            _cam.targetTexture = null;
            #if UNITY_EDITOR
            AssetDatabase.Refresh();
            #endif
            
            // Increment
            if (_rotIndex < rotationCount)
                SnapScreenshot();
            else
            {
                _rotIndex = 0;
            }
        }
    }
    //   =================================================================================================================================
    private string GetFileName()
    {
        string fileName = "SnapshotSprite";         // set default file name
        if (exportFileName != String.Empty && exportFileName != "") // overwrite file name if applicable
            fileName = exportFileName;
        if (rotationCount > 0) // if this is part of a series of images, number it accordingly
            fileName += _rotIndex.ToString();
        fileName += ".png"; // append file name to be a png type image
        return fileName;
    }
    //   =================================================================================================================================
    private string GetFilePath()
    {
        //string exPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string exPath = _dataPath;
        exPath += "/Snapshots";
        if (categoryFolderName != String.Empty && categoryFolderName != "")
            exPath += "/" + categoryFolderName;
        if (!Directory.Exists(exPath))
            Directory.CreateDirectory(exPath);
        if(exportDirectory != String.Empty && exportDirectory != "")
            if (Directory.Exists(exportDirectory))
                exPath = exportDirectory;
        return exPath;
    }
    //   =================================================================================================================================
    // Rotate Around Target Object
    public void Nudge()
    {
        // Get the degrees of how much to rotate each sequence
        _rotAmount = (float)360 / (float)rotationCount;
        
        // Rotates AROUND the object, pulled from the Transform base class since this function doesn't appear to work in edit mode
        Vector3 position = transform.position;
        Vector3 vector3 = Quaternion.AngleAxis(_rotAmount, Vector3.up) * (position - target.transform.position);
        transform.position = target.transform.position + vector3;
        
        // Rotate camera to face the target object
        transform.LookAt(target);
        if (!lookAtTarget)
            transform.rotation = Quaternion.Euler(new Vector3(0.0f, transform.eulerAngles.y, 0.0f));
    }
    private void OnEnable()
    {
        _cam = GetComponent<Camera>();
        _dataPath = Application.dataPath;
    }

    public void ResetCamera()
    {
        transform.position = resetPosition;
        transform.rotation = Quaternion.Euler(resetRotation);
    }
}
    //   =================================================================================================================================



//    ============================================
//    ================ EDITOR GUI ================
//    ============================================

[CustomEditor(typeof(JJU_GreenScreen))]
public class GSE : Editor
{
    public override void OnInspectorGUI()
    {
        
        JJU_GreenScreen instance = (JJU_GreenScreen)target;
        DrawDefaultInspector();
        
        //    ===========================================
        
        GUILayout.Space(20);
        GUILayout.BeginHorizontal(); // START H
        if (GUILayout.Button("Take Snapshot",GUILayout.Width(125),GUILayout.Height(30)))
        {
            instance.SnapScreenshot();
        }
        if (GUILayout.Button("Nudge",GUILayout.Width(125),GUILayout.Height(30)))
        {
            instance.Nudge();
        }
        if (GUILayout.Button("Reset",GUILayout.Width(125),GUILayout.Height(30)))
        {
            instance.ResetCamera();
        }
        GUILayout.EndHorizontal(); // END H
        
        //    ===========================================
        
        GUILayout.BeginHorizontal(); // START H
        EditorGUILayout.Vector3Field("Starting Position:", instance.resetPosition);
        GUILayout.EndHorizontal(); // END H
        
        GUILayout.BeginHorizontal(); // START H
        EditorGUILayout.Vector3Field("Starting Rotation:", instance.resetRotation);
        GUILayout.EndHorizontal(); // END H
        
        //    ===========================================
        
        GUILayout.BeginHorizontal(); // START H
        if (GUILayout.Button("Capture Position & Rotation"))
        {
            instance.resetPosition = Selection.activeTransform.position;
            instance.resetRotation = Selection.activeTransform.rotation.eulerAngles;
        }
        GUILayout.EndHorizontal(); // END H
    }
    
}
#endif