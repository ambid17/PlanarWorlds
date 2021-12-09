using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

public enum MaterialType
{
    Albedo, Metallic, Smoothness
}

[System.Serializable]
public class PaintableTexture
{
    public string textureId;
    public RenderTexture runTimeTexture;
    public RenderTexture paintedTexture;
    public CommandBuffer commandBuffer;

    private Material uvMaterial;
    private Material fixedEdgesMaterial;
    private RenderTexture fixedIslandsRenderTexture;
    private Color clearColor;

    public PaintableTexture(int size, MaterialType materialType, 
        Shader paintShader, Mesh meshToDraw, Shader fixIslandEdgesShader, RenderTexture markedIslands)
    {
        switch (materialType)
        {
            case MaterialType.Albedo:
                textureId = "_MainTex";
                clearColor = Color.white;
                break;
            case MaterialType.Metallic:
                textureId = "_MetallicGlossMap";
                clearColor = Color.white;
                break;
            case MaterialType.Smoothness:
                textureId = "_GlossMap";
                clearColor = Color.black;
                break;
        }

        runTimeTexture = CreateRenderTexture(size);
        paintedTexture = CreateRenderTexture(size);


        fixedIslandsRenderTexture = new RenderTexture(paintedTexture.descriptor);

        // Clear both textures, reset them to "clearColor"
        Graphics.SetRenderTarget(runTimeTexture);
        GL.Clear(false, true, clearColor);
        Graphics.SetRenderTarget(paintedTexture);
        GL.Clear(false, true, clearColor);


        uvMaterial = new Material(paintShader);
        if (!uvMaterial.SetPass(0))
        {
            Debug.LogError("Invalid Shader Pass: " );
        }
        uvMaterial.SetTexture("_MainTex", paintedTexture);

        fixedEdgesMaterial = new Material(fixIslandEdgesShader);
        fixedEdgesMaterial.SetTexture("_IslandMap", markedIslands);
        fixedEdgesMaterial.SetTexture("_MainTex", paintedTexture);

        commandBuffer = new CommandBuffer();
        commandBuffer.name = "TexturePainting"+ textureId;


        commandBuffer.SetRenderTarget(runTimeTexture);
        commandBuffer.DrawMesh(meshToDraw, Matrix4x4.identity, uvMaterial);

        commandBuffer.Blit(runTimeTexture, fixedIslandsRenderTexture, fixedEdgesMaterial);
        commandBuffer.Blit(fixedIslandsRenderTexture, runTimeTexture);
        commandBuffer.Blit(runTimeTexture, paintedTexture);
    }

    public void SetActiveTexture(Camera mainCam)
    {
        mainCam.AddCommandBuffer(CameraEvent.AfterDepthTexture, commandBuffer);
    }
    
    public void SetInactiveTexture(Camera mainCam)
    {
        mainCam.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, commandBuffer);
    }

    public void UpdateShaderParameters(Matrix4x4 localToWorld)
    {
        uvMaterial.SetMatrix("mesh_Object2World", localToWorld); // Mus be updated every time the mesh moves, and also at start
    }

    private RenderTexture CreateRenderTexture(int size)
    {
        return new RenderTexture(size, size, 0)
        {
            anisoLevel = 0,
            useMipMap  = false,
            filterMode = FilterMode.Bilinear
        };
    }
}