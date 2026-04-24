using System.Collections.Generic;
using Core.Log;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Presentation.Postprocessing
{
	public class WallOcclusionFeature : ScriptableRendererFeature
	{
		private class WallOcclusionPass : ScriptableRenderPass
		{
			private static readonly List<ShaderTagId> ShaderTags = new()
			{
				new ShaderTagId("Universal2D"),          // URP 2D sprite / URP Spine shader
				new ShaderTagId("SRPDefaultUnlit"),      // 兜底：旧版 Sprites-Default / 传统 Spine shader
				new ShaderTagId("UniversalForward"),
				new ShaderTagId("UniversalForwardOnly"),
			};

			private const uint UnitLayerMask = 1u << 2;
			private const uint WallLayerMask = 1u << 3;

			private static readonly int PropUnitColor = Shader.PropertyToID("_UnitColorTex");
			private static readonly int PropWallMask  = Shader.PropertyToID("_WallMaskTex");

			private readonly Material _composeMaterial;
			private readonly FilteringSettings _unitFilter;
			private readonly FilteringSettings _wallFilter;
			private readonly MaterialPropertyBlock _propertyBlock;

			private RTHandle _unitColorRT;
			private RTHandle _wallMaskRT;

			public WallOcclusionPass(Material composeMaterial)
			{
				_composeMaterial = composeMaterial;
				renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
				_unitFilter = new FilteringSettings(RenderQueueRange.all, renderingLayerMask: UnitLayerMask);
				_wallFilter = new FilteringSettings(RenderQueueRange.all, renderingLayerMask: WallLayerMask);
				_propertyBlock = new MaterialPropertyBlock();
			}

			public void Dispose()
			{
				_unitColorRT?.Release();
				_wallMaskRT?.Release();
				_unitColorRT = null;
				_wallMaskRT = null;
			}

			public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
			{
				ResetTarget();

				// _UnitColorRT: 需要保留完整颜色以便合成，ARGB32
				var unitDesc = renderingData.cameraData.cameraTargetDescriptor;
				unitDesc.msaaSamples = 1;
				unitDesc.depthBufferBits = 0;
				unitDesc.colorFormat = RenderTextureFormat.ARGB32;
				RenderingUtils.ReAllocateIfNeeded(ref _unitColorRT, unitDesc, name: "_UnitColorRT");

				// _WallMaskRT: 只需 alpha 通道判定覆盖，R8 节省带宽
				var wallDesc = renderingData.cameraData.cameraTargetDescriptor;
				wallDesc.msaaSamples = 1;
				wallDesc.depthBufferBits = 0;
				wallDesc.colorFormat = RenderTextureFormat.R8;
				RenderingUtils.ReAllocateIfNeeded(ref _wallMaskRT, wallDesc, name: "_WallMaskRT");
			}

			public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
			{
				var cmd = CommandBufferPool.Get("Wall Occlusion");

				cmd.SetRenderTarget(_unitColorRT);
				cmd.ClearRenderTarget(false, true, Color.clear);
				{
					var draw = CreateDrawingSettings(ShaderTags, ref renderingData, SortingCriteria.CommonTransparent);
					var listParams = new RendererListParams(renderingData.cullResults, draw, _unitFilter);
					cmd.DrawRendererList(context.CreateRendererList(ref listParams));
				}

				cmd.SetRenderTarget(_wallMaskRT);
				cmd.ClearRenderTarget(false, true, Color.clear);
				{
					var draw = CreateDrawingSettings(ShaderTags, ref renderingData, SortingCriteria.CommonTransparent);
					var listParams = new RendererListParams(renderingData.cullResults, draw, _wallFilter);
					cmd.DrawRendererList(context.CreateRendererList(ref listParams));
				}

				cmd.SetRenderTarget(renderingData.cameraData.renderer.cameraColorTargetHandle);
				_propertyBlock.SetTexture(PropUnitColor, _unitColorRT);
				_propertyBlock.SetTexture(PropWallMask,  _wallMaskRT);
				cmd.DrawProcedural(Matrix4x4.identity, _composeMaterial, 0, MeshTopology.Triangles, 3, 1, _propertyBlock);

				context.ExecuteCommandBuffer(cmd);
				cmd.Clear();
				CommandBufferPool.Release(cmd);
			}
		}

		[SerializeField] private Material composeMaterial;

		private WallOcclusionPass _wallOcclusionPass;

		private bool IsMaterialValid() =>
			composeMaterial && composeMaterial.shader && composeMaterial.shader.isSupported;

		public override void Create()
		{
			if (!IsMaterialValid()) return;

			_wallOcclusionPass = new WallOcclusionPass(composeMaterial);
			this.Log("WallOcclusionFeature created.");
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			if (_wallOcclusionPass == null) return;
			renderer.EnqueuePass(_wallOcclusionPass);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			_wallOcclusionPass?.Dispose();
		}
	}
}
