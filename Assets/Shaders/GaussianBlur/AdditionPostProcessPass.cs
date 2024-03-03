using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine;

namespace AD.Experimental.Rendering.Universal
{
    /// <summary>
    /// 对URP屏幕后处理扩展
    /// </summary>
    public class AdditionPostProcessPass : ScriptableRenderPass
    {
        /// <summary>
        /// 高斯模糊
        /// </summary>
        RenderTargetIdentifier m_ColorAttachment;
        RenderTargetIdentifier m_CameraDepthAttachment;
        RenderTargetHandle m_Destination;

        const string k_RenderPostProcessingTag = "Render AdditionalPostProcessing Effects";
        const string k_RenderFinalPostProcessingTag = "Render Final AdditionalPostProcessing Pass";

        //additonal effects settings
        GaussianBlur m_GaussianBlur;

        MaterialLibrary m_Materials;
        AdditionalPostProcessData m_Data;

        RenderTargetHandle m_TemporaryColorTexture01;

        RenderTargetHandle m_TemporaryColorTexture02;

        RenderTargetHandle m_TemporaryColorTexture03;

        public AdditionPostProcessPass()
        {
            m_TemporaryColorTexture01.Init("_TemporaryColorTexture1");
            m_TemporaryColorTexture02.Init("_TemporaryColorTexture2");
            m_TemporaryColorTexture03.Init("_TemporaryColorTexture3");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var stack = VolumeManager.instance.stack;
            m_GaussianBlur = stack.GetComponent<GaussianBlur>();
            var cmd = CommandBufferPool.Get(k_RenderPostProcessingTag);
            Render(cmd, ref renderingData);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Setup(RenderPassEvent @event, RenderTargetIdentifier source, RenderTargetIdentifier cameraDepth, RenderTargetHandle destination, AdditionalPostProcessData data)
        {
            m_Data = data;
            renderPassEvent = @event;
            m_ColorAttachment = source;
            m_CameraDepthAttachment = cameraDepth;
            m_Destination = destination;
            m_Materials = new MaterialLibrary(data);
        }

        public void Setup(RenderPassEvent @event, ScriptableRenderer renderer, RenderTargetHandle dest, AdditionalPostProcessData data)
        {
            Setup(@event, renderer.cameraColorTarget, renderer.cameraDepthTarget, dest, data);
        }

        void Render(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            if (m_GaussianBlur.IsActive() && !cameraData.isSceneViewCamera)
            {
                SetupGaussianBlur(cmd, ref renderingData, m_Materials.gaussianBlur);
            }
        }

        public void SetupGaussianBlur(CommandBuffer cmd, ref RenderingData renderingData, Material blurMaterial)
        {
            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.width = opaqueDesc.width >> m_GaussianBlur.downSample.value;
            opaqueDesc.height = opaqueDesc.height >> m_GaussianBlur.downSample.value;
            opaqueDesc.depthBufferBits = 0;
            cmd.GetTemporaryRT(m_TemporaryColorTexture01.id, opaqueDesc, m_GaussianBlur.filterMode.value);
            cmd.GetTemporaryRT(m_TemporaryColorTexture02.id, opaqueDesc, m_GaussianBlur.filterMode.value);
            cmd.GetTemporaryRT(m_TemporaryColorTexture03.id, opaqueDesc, m_GaussianBlur.filterMode.value);
            cmd.BeginSample("GaussianBlur");
            cmd.Blit(this.m_ColorAttachment, m_TemporaryColorTexture03.Identifier());
            for (int i = 0; i < m_GaussianBlur.blurCount.value; i++)
            {
                blurMaterial.SetVector("_offsets", new Vector4(0, m_GaussianBlur.indensity.value, 0, 0));
                cmd.Blit(m_TemporaryColorTexture03.Identifier(), m_TemporaryColorTexture01.Identifier(), blurMaterial);
                blurMaterial.SetVector("_offsets", new Vector4(m_GaussianBlur.indensity.value, 0, 0, 0));
                cmd.Blit(m_TemporaryColorTexture01.Identifier(), m_TemporaryColorTexture02.Identifier(), blurMaterial);
                cmd.Blit(m_TemporaryColorTexture02.Identifier(), m_ColorAttachment);
            }
            cmd.Blit(m_TemporaryColorTexture03.Identifier(), this.m_Destination.Identifier());
            cmd.EndSample("Blur");
        }
    }
}
