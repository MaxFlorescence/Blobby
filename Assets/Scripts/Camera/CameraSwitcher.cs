using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

public class CameraSwitcher : MonoBehaviour
{
    private PriorityCamera[] cameras;
    private int cameraCount;
    private int activeCamera = -1;

    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera.gameObject.layer == Utilities.INVENTORY_UI_LAYER) {
            GameInfo.ControlledBlob.EnableInventoryLight(true);
        }
    }

    void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera.gameObject.layer == Utilities.INVENTORY_UI_LAYER) {
            GameInfo.ControlledBlob.EnableInventoryLight(false);
        }
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

    void Awake()
    {
        cameras = Array.ConvertAll(Camera.allCameras, camera => camera.GetComponent<PriorityCamera>());
        cameraCount = cameras.Length;
        DeactivateAll();
    }

    void Update()
    {
        ActivateHighesetPriorityCamera();
    }

    private void DeactivateAll()
    {
        foreach (PriorityCamera camera in cameras)
        {
            camera.Deactivate();
        }
    }

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
        
        if (newActiveCamera == activeCamera) return;

        Assert.IsTrue(newActiveCamera >= 0);
        if (activeCamera >= 0)
        {
            cameras[activeCamera].Deactivate();
        }
        cameras[newActiveCamera].Activate();
        activeCamera = newActiveCamera;
    }
    
    public bool IsMainCameraActive()
    {
        return cameras[activeCamera].IsMain();
    }
}
