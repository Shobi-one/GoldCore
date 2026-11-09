using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace GoldCore.RetroFX
{
    /// <summary>
    /// Draws RetroFX on URP. Add it to the URP renderer (GoldCore > Setup does this), then put a
    /// <see cref="RetroCamera"/> with a preset on the camera, the same as on the built-in pipeline.
    /// Cameras without a RetroCamera are left alone.
    /// </summary>
    public class RetroFXFeature : ScriptableRendererFeature
    {
        [Tooltip("When the effect runs. Before post-processing lets URP's own bloom and tonemapping run on top.")]
        public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingPostProcessing;

        RetroFXPass pass;
        Material material;

        public override void Create()
        {
            pass = new RetroFXPass { renderPassEvent = passEvent };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            Camera camera = renderingData.cameraData.camera;
            if (renderingData.cameraData.cameraType == CameraType.Preview || renderingData.cameraData.cameraType == CameraType.Reflection) return;

            RetroCamera retro = RetroCamera.For(camera);
            if (retro == null && renderingData.cameraData.cameraType == CameraType.SceneView)
                retro = RetroCamera.For(Camera.main);
            if (retro == null || !retro.WantsPostProcess) return;

            if (material == null)
            {
                Shader shader = Shader.Find("Hidden/GoldCore/RetroPostURP");
                if (shader == null || !shader.isSupported) return;
                material = CoreUtils.CreateEngineMaterial(shader);
            }

            pass.renderPassEvent = passEvent;
            pass.Setup(retro, material);
            pass.ConfigureInput(retro.Preset.fog ? ScriptableRenderPassInput.Depth : ScriptableRenderPassInput.None);
            renderer.EnqueuePass(pass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(material);
            material = null;
        }

        class RetroFXPass : ScriptableRenderPass
        {
            static readonly int DepthTextureId = Shader.PropertyToID("_RetroDepthTexture");

            RetroCamera retro;
            Material material;

            class ProcessData
            {
                public TextureHandle source;
                public TextureHandle depth;
                public bool useDepth;
                public Material material;
            }

            class UpscaleData
            {
                public TextureHandle source;
                public bool pixelated;
            }

            public RetroFXPass()
            {
                // The effect reads the camera color and writes a new target, so it needs an intermediate texture.
                requiresIntermediateTexture = true;
            }

            public void Setup(RetroCamera retroCamera, Material postMaterial)
            {
                retro = retroCamera;
                material = postMaterial;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                UniversalResourceData resources = frameData.Get<UniversalResourceData>();
                if (resources.isActiveTargetBackBuffer || retro == null || material == null) return;

                TextureHandle source = resources.activeColorTexture;
                TextureDesc sourceDesc = renderGraph.GetTextureDesc(source);
                Vector2Int target = retro.TargetSize(sourceDesc.width, sourceDesc.height);
                retro.ApplyPostSettings(material, target);
                RetroPreset preset = retro.Preset;

                TextureDesc lowDesc = sourceDesc;
                lowDesc.name = "_RetroFXLow";
                lowDesc.width = target.x;
                lowDesc.height = target.y;
                lowDesc.clearBuffer = false;
                lowDesc.msaaSamples = MSAASamples.None;
                lowDesc.depthBufferBits = DepthBits.None;
                lowDesc.filterMode = preset.pixelatedUpscale ? FilterMode.Point : FilterMode.Bilinear;
                TextureHandle low = renderGraph.CreateTexture(lowDesc);

                using (var builder = renderGraph.AddRasterRenderPass<ProcessData>("RetroFX Process", out var data))
                {
                    data.source = source;
                    data.material = material;
                    data.useDepth = preset.fog && resources.cameraDepthTexture.IsValid();
                    builder.UseTexture(source);
                    if (data.useDepth)
                    {
                        data.depth = resources.cameraDepthTexture;
                        builder.UseTexture(data.depth);
                    }
                    builder.SetRenderAttachment(low, 0);
                    builder.SetRenderFunc((ProcessData d, RasterGraphContext ctx) =>
                    {
                        if (d.useDepth) d.material.SetTexture(DepthTextureId, (RTHandle)d.depth);
                        Blitter.BlitTexture(ctx.cmd, d.source, new Vector4(1, 1, 0, 0), d.material, 0);
                    });
                }

                TextureDesc outDesc = sourceDesc;
                outDesc.name = "_RetroFXOut";
                outDesc.clearBuffer = false;
                TextureHandle output = renderGraph.CreateTexture(outDesc);

                using (var builder = renderGraph.AddRasterRenderPass<UpscaleData>("RetroFX Upscale", out var data))
                {
                    data.source = low;
                    data.pixelated = preset.pixelatedUpscale;
                    builder.UseTexture(low);
                    builder.SetRenderAttachment(output, 0);
                    builder.SetRenderFunc((UpscaleData d, RasterGraphContext ctx) =>
                        Blitter.BlitTexture(ctx.cmd, d.source, new Vector4(1, 1, 0, 0), 0, !d.pixelated));
                }

                resources.cameraColor = output;
            }
        }
    }
}
