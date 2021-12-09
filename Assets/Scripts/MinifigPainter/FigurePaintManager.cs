using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FigurePaintManager : MonoBehaviour
{
    [SerializeField] private LayerMask paintLayerMask;

    private Vector4 mouseWorldPosition;
    
    public  Texture          baseTexture;                  // used to deterimne the dimensions of the runtime texture
    public  Material         meshMaterial;                 // used to bind the runtime texture as the albedo of the mesh
    public  GameObject       meshGameobject;
    public  Shader           uVShader;                     // the shader usedto draw in the texture of the mesh
    public  Mesh             meshToDraw;
    public  Shader           islandMarkerShader;
    public  Shader           fixIslandEdgesShader;
    public  Shader           combineMetalicSmoothnes;                         

    private Camera           mainCam;
    private int              clearTexture;
    private RenderTexture    markedIslands;
    private CommandBuffer    islandMarkCommandBuffer;
    private int              numberOfFrames;
    private Material         fixEdgesMaterial;
    private Material         createMetalicGlossMap;
    private RenderTexture    metallicGlossMapCombined;

    private PaintableTexture albedo;
    private PaintableTexture metallic;
    public  PaintableTexture smoothness;
    void Start()
    {
        markedIslands = new RenderTexture(baseTexture.width, baseTexture.height, 0, RenderTextureFormat.R8);
        albedo = new PaintableTexture(baseTexture.width, MaterialType.Albedo, uVShader, meshToDraw, fixIslandEdgesShader,markedIslands);
        metallic = new PaintableTexture(baseTexture.width, MaterialType.Metallic,uVShader, meshToDraw, fixIslandEdgesShader, markedIslands);
        smoothness = new PaintableTexture(baseTexture.width, MaterialType.Smoothness, uVShader, meshToDraw, fixIslandEdgesShader, markedIslands);
        metallicGlossMapCombined = new RenderTexture(metallic.runTimeTexture.descriptor)
        {
            format = RenderTextureFormat.ARGB32,
        };

        meshMaterial.SetTexture(albedo.textureId, albedo.runTimeTexture);
        meshMaterial.SetTexture(metallic.textureId, metallicGlossMapCombined);
        meshMaterial.EnableKeyword("_METALLICGLOSSMAP");

        createMetalicGlossMap = new Material(combineMetalicSmoothnes);

        islandMarkCommandBuffer = new CommandBuffer();
        islandMarkCommandBuffer.name = "markingIslands";
        islandMarkCommandBuffer.SetRenderTarget(markedIslands);
        Material islandMarkerMaterial  = new Material(islandMarkerShader);
        islandMarkCommandBuffer.DrawMesh(meshToDraw, Matrix4x4.identity, islandMarkerMaterial);
        
        mainCam = Camera.main;
        mainCam.AddCommandBuffer(CameraEvent.AfterDepthTexture, islandMarkCommandBuffer);

        albedo.SetActiveTexture(mainCam);
    }

    void Update()
    {
        if (numberOfFrames > 2) mainCam.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, islandMarkCommandBuffer);

        createMetalicGlossMap.SetTexture("_Smoothness", smoothness.runTimeTexture);
        createMetalicGlossMap.SetTexture("_MainTex", metallic.runTimeTexture);
        Graphics.Blit(metallic.runTimeTexture, metallicGlossMapCombined, createMetalicGlossMap);
        numberOfFrames++;

        albedo.UpdateShaderParameters(meshGameobject.transform.localToWorldMatrix);
        metallic.UpdateShaderParameters(meshGameobject.transform.localToWorldMatrix);
        smoothness.UpdateShaderParameters(meshGameobject.transform.localToWorldMatrix);
        
        //if (Input.GetMouseButton(0))
        //{
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out RaycastHit hit, paintLayerMask))
            {
                mouseWorldPosition = hit.point;
                mouseWorldPosition.w = 1;
            }
        //}
        //else
        //{
        //    mouseWorldPosition = Vector4.positiveInfinity;
        //    mouseWorldPosition.w = 0;
        //}

        //meshMaterial.SetVector("_MousePosition", mouseWorldPosition);
        //meshMaterial.SetFloat("_BrushOpacity", 1);
        Shader.SetGlobalVector("_MousePosition", mouseWorldPosition);
        Shader.SetGlobalVector("_BrushColor", new Vector4(1,0,0,0));
        Shader.SetGlobalFloat("_BrushOpacity", 1);
        Shader.SetGlobalFloat("_BrushSize", 1);
    }
}
