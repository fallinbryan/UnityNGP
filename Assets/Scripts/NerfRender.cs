using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

using UnityEngine;

public class NerfRender : MonoBehaviour
{

  [Header("Basic NERF Settings")]
  [Tooltip("Basic settings for NERF")]
  public int width;
  public int height;
  public bool enableDlss;
  public string nerf_model = "D:\\DataSets\\Backyard\\transforms_base.ingp";
  public Camera SceneCamera;
  public float AABBScale = 0.5f;

  [Tooltip("Alpha threshold for NERF")]
  [Range(0, 100)]
  public int AlphaThresholdInt = 10;
  private float AlphaThreshold
  {
    get { return AlphaThresholdInt / 100f; }
  }

  Texture2D rgbTexture, depthTexture;
  static System.IntPtr rgbHandle = System.IntPtr.Zero;
  static System.IntPtr depthHandle = System.IntPtr.Zero;
  static bool isInitialized = false;
  static bool textureCreated = false;
  static bool graphicsInitialized = false;
   Material nerfMaterial;

  // camera view and transform management
  static Vector3 forwardPos;
  static Vector3 upPos;
  static Vector3 rightPos;
  static Vector3 positionPos;

  private Vector3 viewOffset;


  private const int INIT_EVENT = 0x0001;
  private const int DRAW_EVENT = 0x0002;
  private const int DESTROY_EVENT = 0x0003;
  private const int CREATE_TEXTURE_EVENT = 0x0004;
  private const int DESTROY_VULKAN_EVENT = 0x0005;

  private void Awake()
  {
    Vector3 objectPos = transform.position;
    Vector3 cameraPos = SceneCamera.transform.position;
    viewOffset = objectPos - cameraPos;

    nerfMaterial = GetComponent<Renderer>().material;

    textureCreated = false;
    isInitialized = false;


   

    if(File.Exists(nerf_model))
    {
      NefpluginWrapper.setup_initialization_params(nerf_model, enableDlss, width, height);
      GL.IssuePluginEvent(NefpluginWrapper.GetRenderEventFunc(), INIT_EVENT);
    }
    else
    {
      Debug.Log("Model does not exist");
#if UNITY_EDITOR
      UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
  }

  private void setBoundingBox()
  {
    
    float[] position = new float[3] { 0.0f, 0.0f, 0.0f };
    NefpluginWrapper.SetCropBox(AABBScale, position);
  }

  void OnApplicationQuit()
  {
    GL.IssuePluginEvent(NefpluginWrapper.GetRenderEventFunc(), DESTROY_VULKAN_EVENT);
    GL.IssuePluginEvent(NefpluginWrapper.GetRenderEventFunc(), DESTROY_EVENT);
    rgbHandle = System.IntPtr.Zero;
    depthHandle = System.IntPtr.Zero;
    isInitialized = false;
    textureCreated = false;
    graphicsInitialized = false;
  }

  static void CleanupOnEditorQuit()
  {
    GL.IssuePluginEvent(NefpluginWrapper.GetRenderEventFunc(), DESTROY_VULKAN_EVENT);
    GL.IssuePluginEvent(NefpluginWrapper.GetRenderEventFunc(), DESTROY_EVENT);
    rgbHandle = System.IntPtr.Zero;
    depthHandle = System.IntPtr.Zero;
    isInitialized = false;
    textureCreated = false;
    graphicsInitialized = false;
  }

  public void updateNeRFCamera()
  {
    float nerfWorldTranslationFactor = 0.1f;
    // Create a temporary GameObject to copy the camera's transform
    GameObject tempCamera = new GameObject("TempCamera");
    tempCamera.transform.position = SceneCamera.transform.position;
    tempCamera.transform.rotation = SceneCamera.transform.rotation;

    tempCamera.transform.Rotate(0, 45, 0, Space.World);

    forwardPos =   tempCamera.transform.TransformDirection(Vector3.forward);
    upPos =        tempCamera.transform.TransformDirection(Vector3.up);
    rightPos =     tempCamera.transform.TransformDirection(Vector3.right);
    positionPos =  tempCamera.transform.position * nerfWorldTranslationFactor;

    //destroy the temporary GameObject
    Destroy(tempCamera);
  }

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {
    
    graphicsInitialized = NefpluginWrapper.is_graphics_initialized();
    textureCreated = NefpluginWrapper.isTextureCreated();

    if(graphicsInitialized && !textureCreated)
    {
      GL.IssuePluginEvent(NefpluginWrapper.GetRenderEventFunc(), CREATE_TEXTURE_EVENT);
    }

    if(rgbHandle == System.IntPtr.Zero || depthHandle == System.IntPtr.Zero)
    {
      rgbHandle = NefpluginWrapper.get_color_buffer_handle();
      depthHandle = NefpluginWrapper.get_depth_buffer_handle();
    }

    if(rgbHandle != System.IntPtr.Zero && depthHandle != System.IntPtr.Zero && !isInitialized)
    {
      rgbTexture = Texture2D.CreateExternalTexture(width, height, TextureFormat.RGBAFloat, false, false, rgbHandle);

      depthTexture = Texture2D.CreateExternalTexture(width, height, TextureFormat.RFloat, false, false, depthHandle);

      nerfMaterial.SetTexture("_MainTex", rgbTexture);
      nerfMaterial.SetTexture("_DepthTex", depthTexture);
      nerfMaterial.SetFloat("_AlphaThreshold", AlphaThreshold);
      isInitialized = true;
    }

    if (rgbHandle != System.IntPtr.Zero && depthHandle != System.IntPtr.Zero && isInitialized)
    {
      updateNeRFCamera();
      setBoundingBox();
      float[] camer_matrix = new float[3*4]
      {
        rightPos.x,    rightPos.y,    rightPos.z,
        upPos.x,       upPos.y,       upPos.z,
        forwardPos.x,  forwardPos.y,  forwardPos.z,
        positionPos.x, positionPos.y, positionPos.z
      };

      NefpluginWrapper.UpdateViewMatrix(camer_matrix);
      GL.IssuePluginEvent(NefpluginWrapper.GetRenderEventFunc(), DRAW_EVENT);
      GL.InvalidateState();
    }


  }
}
