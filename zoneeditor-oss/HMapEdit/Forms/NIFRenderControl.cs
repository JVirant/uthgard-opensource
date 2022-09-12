using HMapEdit.Properties;
using HMapEdit.Tools;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HMapEdit.Forms
{
	public partial class NIFRenderControl : UserControl
	{
		protected Device _device;
		protected Effect _shaderNif;
		protected LocalTextures _textures;

		protected Vector3 _camera = new Vector3(1, 1, 1) * 200;

		public NIFRenderControl()
		{
			InitializeComponent();
			Paint += OnPaint;
		}

		private void OnPaint(object sender, PaintEventArgs e)
		{
			if (Visible)
				_Render();
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			ParentForm.Shown += _Initialize;
			ParentForm.FormClosing += _Deinitialize;
		}

		private void _Initialize(object sender, EventArgs e)
		{
			string err;

			var hardware = Manager.GetDeviceCaps(0, DeviceType.Hardware);
			_device = new Device(0, DeviceType.Hardware, this, CreateFlags.HardwareVertexProcessing, GetPP());
			_device.DeviceReset += delegate
			{
				_device.RenderState.Lighting = false;
				_device.RenderState.CullMode = Cull.None;
				_device.RenderState.FillMode = Program.CONFIG.FillMode;
				_device.RenderState.AntiAliasedLineEnable = true;
				_device.RenderState.PointSize = 3.0f;

				if (Program.Arguments.AntiAlias)
					_device.RenderState.MultiSampleAntiAlias = true;

				//Alpha
				_device.RenderState.SourceBlend = Blend.SourceAlpha;
				_device.RenderState.DestinationBlend = Blend.InvSourceAlpha;
				_device.RenderState.AlphaBlendEnable = true;
				_device.RenderState.AlphaTestEnable = true;

				// Texture Blending
				_device.TextureState[0].AlphaOperation = TextureOperation.SelectArg1;
				_device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
			};

			_shaderNif = Effect.FromString(_device, Encoding.Default.GetString(Resources.nif), null, null, ShaderFlags.None, null, out err);
			if (_shaderNif == null || !string.IsNullOrEmpty(err))
			{
				MessageBox.Show(err ?? "shader nif.fx not found", "Shader Error");
				Environment.Exit(0);
			}

			_device.Reset(GetPP());

			var alph = new Bitmap(1, 1);
			alph.SetPixel(0, 0, Color.WhiteSmoke);
			var objsolid = new Texture(_device, alph, Usage.None, Pool.Managed);
			_textures = new LocalTextures(_device, objsolid);
		}

		private PresentParameters GetPP()
		{
			Format f = Manager.Adapters[0].CurrentDisplayMode.Format;
			var pp = new PresentParameters();
			pp.Windowed = true;
			pp.SwapEffect = SwapEffect.Discard;
			pp.EnableAutoDepthStencil = true;
			pp.AutoDepthStencilFormat = DepthFormat.D24S8;
			pp.BackBufferCount = 1;
			pp.BackBufferFormat = f;
			pp.BackBufferHeight = Height;
			pp.BackBufferWidth = Width;

			if (Program.Arguments.AntiAlias)
			{
				int res, h;
				if (!Manager.CheckDeviceMultiSampleType(0, DeviceType.Hardware, f, true, MultiSampleType.NonMaskable, out res, out h))
					Program.Arguments.AntiAlias = false;
				else
				{
					pp.MultiSample = MultiSampleType.NonMaskable;
					pp.MultiSampleQuality = Math.Min(2, h - 1);
				}
			}

			return pp;
		}

		private void _Deinitialize(object sender, FormClosingEventArgs e)
		{
			_shaderNif.Dispose();
			_device.Dispose();
			_device = null;
		}

		private NIFModel _nif;
		public NIFModel nif
		{
			get => _nif;
			set
			{
				if (_nif != null)
				{
					_nif.DxDeinit();
					_nif = null;
				}
				if (value != null)
				{
					var stream = GameData.Open(value.filename);
					_nif = new NIFModel(stream, value.filename, false);
					_nif.DxInit(_device);
				}

				Update();
			}
		}

		private void _Render(object sender = null, EventArgs e = null)
		{
			if (!Visible)
				return;

			var watch = Stopwatch.StartNew();

			var surface = _device.GetRenderTarget(0).Description;
			_device.Viewport = new Viewport
			{
				Width = surface.Width,
				Height = surface.Height,
				X = _device.Viewport.X,
				Y = _device.Viewport.Y,
				MinZ = _device.Viewport.MinZ,
				MaxZ = _device.Viewport.MaxZ
			};
			_device.Transform.View = Matrix.LookAtLH(_camera, Vector3.Empty, new Vector3(0, 0, 1));
			_device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 2, (float)surface.Width / surface.Height, 0.01f, 1000.0f);

			_device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
			if (nif == null)
				return;

			_device.BeginScene();

			_shaderNif.Begin(FX.None);
			_shaderNif.SetValue(EffectHandle.FromString("View"), _device.Transform.View);
			_shaderNif.SetValue(EffectHandle.FromString("Projection"), _device.Transform.Projection);

			var world = Matrix.Identity;
			_device.Transform.World = world;
			nif.Render(_textures, _shaderNif, ref world);

			_shaderNif.End();

			_device.EndScene();
			_device.Present();

			watch.Stop();

			
		}
	}
}
