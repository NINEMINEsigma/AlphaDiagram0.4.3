using UnityEngine.Rendering.Universal;

namespace AD.Experimental.Rendering.Universal
{
    public class AdditionPostProcessRendererFeature : ScriptableRendererFeature
    {
        public RenderPassEvent evt = RenderPassEvent.AfterRenderingTransparents;
        public AdditionalPostProcessData postData; 
        AdditionPostProcessPass postPass;

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (postData == null) 
                return;
            postPass.Setup( evt, renderer, RenderTargetHandle.CameraTarget,postData);

            renderer.EnqueuePass(postPass);
        }

        public override void Create()
        {
            postPass = new AdditionPostProcessPass();
        }
    }
}
