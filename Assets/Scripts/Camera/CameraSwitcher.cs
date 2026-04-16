using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

/// <summary>
///     A class for managing multiple cameras in a scene.
/// </summary>
public class CameraSwitcher : MonoBehaviour
{
    private PriorityCamera[] cameras;
    private int cameraCount;
    private int activeCameraIndex = -1;
    private const string IGNORE_CAMERA_TAG = "Independent Camera";

    /// <summary>
    ///     Make sure the inventory camera can only see the light for the inventory.
    /// </summary>
    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera.gameObject.layer == Utilities.INVENTORY_UI_LAYER) {
            GameInfo.ControlledBlob.SetLight(BlobLight.Inventory_Icon, true);
            GameInfo.ControlledBlob.SetLight(BlobLight.Material_Glow, false);
        }
    }

    /// <summary>
    ///     Reset changes made in <tt>OnBeginCameraRendering()</tt>.
    /// </summary>
    void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera.gameObject.layer == Utilities.INVENTORY_UI_LAYER) {
            GameInfo.ControlledBlob.ResetLight(BlobLight.Inventory_Icon);
            GameInfo.ControlledBlob.ResetLight(BlobLight.Material_Glow);
        }
    }

    /// <summary>
    ///     Collect all enabled priority cameras in the scene that aren't tagged with
    ///     <tt>IGNORE_CAMERA_TAG</tt>.
    /// </summary>
    void Awake()
    {
        cameras = Array.ConvertAll(
            Camera.allCameras.Where(camera => !camera.CompareTag(IGNORE_CAMERA_TAG)).ToArray(),
            camera => camera.GetComponent<PriorityCamera>()
        );
        cameraCount = cameras.Length;
        DeactivateAll();
    }

    void Start()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;

        ActivateHighesetPriorityCamera();
    }

    void OnDestroy()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    void Update()
    {
        ActivateHighesetPriorityCamera();
    }

    /// <summary>
    ///     Deactivate all cameras, regardless of priority.
    /// </summary>
    private void DeactivateAll()
    {
        foreach (PriorityCamera camera in cameras)
        {
            camera.Deactivate();
        }
    }

    /// <summary>
    ///     Search <tt>cameras[]</tt> for the one with the current highest priority, and activate
    ///     it while deactivating the rest.
    /// </summary>
    private void ActivateHighesetPriorityCamera()
    {
        int maxPriority = -1;
        int newActiveCamera = -1;

        for (int i = 0; i < cameraCount; i++)
        {
            if (maxPriority < cameras[i].GetPriority())
            {
                maxPriority = cameras[i].GetPriority();
                newActiveCamera = i;
            }
        }
        
        if (newActiveCamera == activeCameraIndex) return;

        Assert.IsTrue(newActiveCamera >= 0);
        if (activeCameraIndex >= 0)
        {
            cameras[activeCameraIndex].Deactivate();
        }
        cameras[newActiveCamera].Activate();
        activeCameraIndex = newActiveCamera;
    }
}
