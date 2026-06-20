using Core.Log;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Presentation.Postprocessing
{
	public class CRTFeature : ScriptableRendererFeature
	{
		private class CRTPass : ScriptableRenderPass
		{
			private static readonly int PropCRTSourceTex = Shader.PropertyToID("_CRTSourceTex");

			private readonly Material _material;
			private readonly MaterialPropertyBlock _propertyBlock;
			private RTHandle _tempRT;

			public CRTPass(Material material)
			{
				_material = material;
				renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
				_propertyBlock = new MaterialPropertyBlock();
			}

			public void Dispose()
			{
				_tempRT?.Release();
				_tempRT = null;
			}

			public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
			{
				ResetTarget();

				var descriptor = renderingData.cameraData.cameraTargetDescriptor;
				descriptor.msaaSamples = 1;
				descriptor.depthBufferBits = 0;
				descriptor.colorFormat = RenderTextureFormat.ARGB32; // 显式格式，避免与 cameraColor 不匹配
				RenderingUtils.ReAllocateIfNeeded(ref _tempRT, descriptor, name: "_CRTSourceTex");
			}

			public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
			{
				var cmd = CommandBufferPool.Get("CRT");
				var source = renderingData.cameraData.renderer.cameraColorTargetHandle;

				// 1. Copy pass：把屏幕当前颜色拷到临时 RT，规避读写同一张 RT 的限制
				cmd.SetRenderTarget(_tempRT);
				_propertyBlock.SetTexture(PropCRTSourceTex, source);
				cmd.DrawProcedural(Matrix4x4.identity, _material, 0, MeshTopology.Triangles, 3, 1, _propertyBlock);

				// 2. CRT pass：从临时 RT 采样、做变换、写回屏幕
				cmd.SetRenderTarget(source);
				_propertyBlock.SetTexture(PropCRTSourceTex, _tempRT);
				cmd.DrawProcedural(Matrix4x4.identity, _material, 1, MeshTopology.Triangles, 3, 1, _propertyBlock);

				context.ExecuteCommandBuffer(cmd);
				cmd.Clear();
				CommandBufferPool.Release(cmd);
			}
		}

		[SerializeField] private Material crtMaterial;

		private CRTPass _crtPass;

		private bool IsMaterialValid() =>
			crtMaterial && crtMaterial.shader && crtMaterial.shader.isSupported;

		public override void Create()
		{
			if (!IsMaterialValid()) return;
			_crtPass = new CRTPass(crtMaterial);
			this.Log("CRTFeature created.");
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			if (_crtPass == null) return;
			renderer.EnqueuePass(_crtPass);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			_crtPass?.Dispose();
		}
	}
}
