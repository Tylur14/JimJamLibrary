using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class JJU_GreenScreen : MonoBehaviour
{
//    ===========================================    
//    ================ VARIABLES ================
//    ===========================================

    [Header("Preview Settings")] 
    [SerializeField] private RectTransform previewPanel;
    [Range(1, 1920)]
    [SerializeField] private int snapshotPreviewX;
    [Range(1, 1080)]
    [SerializeField] private int snapshotPreviewY;
    [Range(0.0f,1.0f)]
    [SerializeField] float transparency = 0.5f;
    
    [Header("Camera Settings")]
    [SerializeField] private bool doesRotate;
    [SerializeField] private bool lookAtTarget;
    [SerializeField] private int rotationCount; // if > 0, rotate after each snapshot and decrement
    

    [Header("Export Settings")] 
    [SerializeField] private string exportDirPath;
    [SerializeField] private string exportName;

    [Header("Snapshot Settings")]
    [SerializeField] private Transform target;
    
    private Camera _cam;
    private bool _takeScreenshotOnNextFrame;
    private float _rotAmount;
    private int _rotIndex;

//    ===========================================
//    ================ FUNCTIONS ================
//    ===========================================
    public void Update()
    {
        SetPreviewOutline();
    }
    
    void SetPreviewOutline()
    {
        if (previewPanel)
        {
            previewPanel.sizeDelta = new Vector2(snapshotPreviewX, snapshotPreviewY);
            previewPanel.GetComponent<Image>().color = new Color(1, 1, 1, transparency);
        }
        
    }

    public void SnapScreenshot()
    {
        // ScreenCapture.CaptureScreenshot(@"C:\Users\Jim\Desktop\test.png");
        
        _rotAmount = (float)360 / (float)rotationCount;
        _takeScreenshotOnNextFrame = true;
    }

    private void GetTexture()
    {
        _cam.targetTexture = RenderTexture.GetTemporary(snapshotPreviewX,snapshotPreviewY,16);
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
        
    }

    private void OnPostRender()
    {
        if (_takeScreenshotOnNextFrame && _rotIndex < rotationCount)
        {
            _cam.targetTexture = RenderTexture.GetTemporary(snapshotPreviewX,snapshotPreviewY,16);
            _takeScreenshotOnNextFrame = false;
            
            RenderTexture rTex = _cam.targetTexture;
            RenderTexture.ReleaseTemporary(rTex);
            _cam.targetTexture = null;
            GetTexture();
            
            Nudge();
            _rotIndex++;

            _rotIndex++;
            if (_rotIndex < rotationCount)
                _takeScreenshotOnNextFrame = true;
            else _rotIndex = 0;
            
            #if UNITY_EDITOR
            AssetDatabase.Refresh();
            #endif
        }
    }

    
    
    private string GetFileName()
    {
        string fileName = "GreenScreenExportImage";         // set default file name
        if (exportName != String.Empty && exportName != "") // overwrite file name if applicable
            fileName = exportName;
        if (doesRotate) // if this is part of a series of images, number it accordingly
            fileName += _rotIndex.ToString();
        fileName += ".png"; // append file name to be a png type image
        return fileName;
    }
    
    private string GetFilePath()
    {
        string exPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        exPath += "/GreenScreenExports";
        if (!Directory.Exists(exPath))
            Directory.CreateDirectory(exPath);
        if(exportDirPath != String.Empty && exportDirPath != "")
            if (Directory.Exists(exportDirPath))
                exPath = exportDirPath;
        return exPath;
    }
    
    // Rotate Around Target Object
    public void Nudge()
    {
        _rotAmount = (float)360 / (float)rotationCount;
        
        // Rotates AROUND the object, pulled from the Transform base class since this function doesn't appear to work in edit mode
        Vector3 position = this.transform.position;
        Vector3 vector3 = Quaternion.AngleAxis(_rotAmount, Vector3.up) * (position - target.transform.position);
        this.transform.position = target.transform.position + vector3;
        
        transform.LookAt(target);
        if (!lookAtTarget)
            transform.rotation = Quaternion.Euler(new Vector3(0.0f, transform.eulerAngles.y, 0.0f));
        //var tarRot = transform.position * (20f * ((float) Math.PI / 180f));
        //this.transform.rotation = Quaternion.Euler(tarRot);

    }
    private void OnEnable()
    {
        _cam = GetComponent<Camera>();
    }
    // Check if File is Open -- may go unused?
    private bool IsFileLocked(FileInfo file)
    {
        FileStream stream = null;

        try
        {
            stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        catch (IOException)
        {
            //the file is unavailable because it is:
            //still being written to
            //or being processed by another thread
            //or does not exist (has already been processed)
            return true;
        }
        finally
        {
            if (stream != null)
                stream.Close();
        }

        //file is not locked
        return false;
    }
}

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
        GUILayout.Space(10);
        //GUILayout.BeginHorizontal();
        //GUILayout.FlexibleSpace();
        if (GUILayout.Button("Snap Screenshot",GUILayout.Width(200),GUILayout.Height(50)))
        {
            instance.SnapScreenshot();
        }
        if (GUILayout.Button("Nudge",GUILayout.Width(200),GUILayout.Height(50)))
        {
            instance.Nudge();
        }
        
        //GUILayout.FlexibleSpace();
        //GUILayout.EndHorizontal();
        //GUILayout.Space(10);
        //GUILayout.BeginHorizontal();
        //GUILayout.FlexibleSpace();
        //if (GUILayout.Button("Reset", GUILayout.Width(150), GUILayout.Height(30)))
        //{
        //    instance.ResetStuff();
        //}
        //GUILayout.FlexibleSpace();
        //GUILayout.EndHorizontal();
       

        //GUILayout.BeginHorizontal();
        //GUILayout.FlexibleSpace();
        //if (GUILayout.Button("Delete Screenshots", GUILayout.Width(150), GUILayout.Height(30)))
        //{
        //    instance.DeleteAssets();
        //}
        //GUILayout.FlexibleSpace();
        //GUILayout.EndHorizontal();
    }
}
