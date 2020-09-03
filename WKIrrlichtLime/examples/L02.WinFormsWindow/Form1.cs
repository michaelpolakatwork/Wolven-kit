using System;
using System.ComponentModel;
using System.Windows.Forms;

using IrrlichtLime;
using IrrlichtLime.Core;
using IrrlichtLime.Video;
using IrrlichtLime.Scene;
using System.Diagnostics;
using System.Collections.Generic;

namespace L02.WinFormsWindow
{
	public partial class Form1 : Form
	{
		// this class will hold all settings that we need to pass to background worker,
		// which will create Irrlicht device, do all rendering and drop it when needed;
		// we are extending IrrlichtCreationParameters with our custom settings
		class DeviceSettings : IrrlichtCreationParameters
		{
			public Color BackColor; // "null" for skybox
			
			public DeviceSettings(IntPtr hh, DriverType dt, byte aa, Color bc, bool vs)
			{
				WindowID = hh;
				DriverType = dt;
				AntiAliasing = aa;
				BackColor = bc;
				VSync = vs;
			}
		}

		private bool userWantExit = false; // if "true", we shut down rendering thread and then exit app

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			// select "No AntiAliasing"
			comboBoxAntiAliasing.SelectedIndex = 0;

			// select "Skybox"
			comboBoxBackground.SelectedIndex = 0;

			// fill combobox with all available video drivers, except Null
			foreach (DriverType v in Enum.GetValues(typeof(DriverType)))
				if (v != DriverType.Null)
					comboBoxVideoDriver.Items.Add(v);
		}

		private void initializeIrrlichtDevice(object sender, EventArgs e)
		{
			if (comboBoxVideoDriver.SelectedItem == null)
				return;

			// if rendering in progress, we are sending cancel request and waiting for its finishing
			if (backgroundRendering.IsBusy)
			{
				backgroundRendering.CancelAsync();
				while (backgroundRendering.IsBusy)
					Application.DoEvents(); // this is not very correct way, but its very short, so we use it

				// redraw the panel, otherwise last rendered frame will stay as garbage
				panelRenderingWindow.Invalidate();
			}

			// collect settings and start background worker with these settings
			DeviceSettings s = new DeviceSettings(
				checkBoxUseSeparateWindow.Checked ? IntPtr.Zero : panelRenderingWindow.Handle,
				(DriverType)comboBoxVideoDriver.SelectedItem,
				(byte)(comboBoxAntiAliasing.SelectedIndex == 0 ? 0 : Math.Pow(2, comboBoxAntiAliasing.SelectedIndex)),
				comboBoxBackground.SelectedIndex == 0 ? null : new Color(comboBoxBackground.SelectedIndex == 1 ? 0xFF000000 : 0xFFFFFFFF),
				checkBoxUseVSync.Checked
			);

			backgroundRendering.RunWorkerAsync(s);

			labelRenderingStatus.Text = "Starting rendering...";
		}

		private void backgroundRendering_DoWork(object sender, DoWorkEventArgs e)
		{
			BackgroundWorker worker = sender as BackgroundWorker;
			DeviceSettings settings = e.Argument as DeviceSettings;

			// create irrlicht device using provided settings

			IrrlichtDevice dev = IrrlichtDevice.CreateDevice(settings);

			if (dev == null)
				throw new Exception("Failed to create Irrlicht device.");

			dev.Logger.LogLevel = LogLevel.Warning;
			VideoDriver drv = dev.VideoDriver;
			SceneManager smgr = dev.SceneManager;

			smgr.FileSystem.AddFileArchive("E:/WitcherMods/modTest/Test/files/Mod/Bundle");
			smgr.FileSystem.AddFileArchive("E:/WitcherMods/modTest/Test/files/Raw/Mod/TextureCache");

			smgr.Attributes.AddValue("TW_TW3_LOAD_SKEL", true);
			smgr.Attributes.AddValue("TW_TW3_LOAD_BEST_LOD_ONLY", true);

			// setup a simple 3d scene

			//CameraSceneNode cam = smgr.AddCameraSceneNode();
			//cam.Target = new Vector3Df(0);

			// added by vl
			AnimatedMesh mesh = smgr.GetMesh("E:/WitcherMods/modTest/Test/files/Mod/Bundle/characters/models/animals/cat/model/t_01__cat.w2mesh");
			if (mesh == null)
				throw new Exception("Failed to load mesh.");

			smgr.MeshManipulator.RecalculateNormals(mesh);

			List<String> animList = null;

			float scaleMul = 1.0f;
			AnimatedMeshSceneNode node = smgr.AddAnimatedMeshSceneNode(mesh);
			MeshLoaderHelper helper = smgr.GetMeshLoader(smgr.MeshLoaderCount - 1).getMeshLoaderHelper();
			if (node != null)
			{
				scaleMul = node.BoundingBox.Radius / 4;
				node.Scale = new Vector3Df(3.0f);
				node.SetMaterialFlag(MaterialFlag.Lighting, false);

				SkinnedMesh sm = helper.loadRig("E:/WitcherMods/modTest/Test/files/Mod/Bundle/characters/base_entities/cat_base/cat_base.w2rig", mesh);
				if(sm == null)
					throw new Exception("Failed to load rig.");

				animList = helper.loadAnimation("E:/WitcherMods/modTest/Test/files/Mod/Bundle/animations/animals/cat/cat_animation.w2anims", sm);
				if (animList.Count > 0)
				{
					AnimatedMesh am = helper.applyAnimation(animList[0], sm);
					node.Mesh = am;
				}
				//scaleSkeleton(sm, 3.0f);
				//sm.SkinMesh();
				
				mesh.Drop();
				sm.Drop();
				setMaterialsSettings(node);
				/*
				animList = helper.loadAnimation("E:/WitcherMods/modTest/Test/files/Mod/Bundle/animations/animals/cat/cat_animation.w2anims", node);
				if (animList.Count > 0)
				{
					helper.applyAnimation(animList[0]);
				}
				*/
				
			}

			var camera = smgr.AddCameraSceneNode(null,
					new Vector3Df(node.BoundingBox.Radius * 8, node.BoundingBox.Radius, 0),
					new Vector3Df(0, node.BoundingBox.Radius, 0)
				);

			camera.NearValue = 0.001f;
			camera.FOV = 45.0f * 3.14f / 180.0f;

			node.DebugDataVisible ^= DebugSceneType.BBox | DebugSceneType.Skeleton;

			node.Position = new Vector3Df(0.0f);

			/*
			SceneNodeAnimator anim = smgr.CreateFlyCircleAnimator(new Vector3Df(0, 15, 0), 30.0f);
			cam.AddAnimator(anim);
			anim.Drop();

			SceneNode cube = smgr.AddCubeSceneNode(20);
			cube.SetMaterialTexture(0, drv.GetTexture("../../media/wall.bmp"));
			cube.SetMaterialTexture(1, drv.GetTexture("../../media/water.jpg"));
			cube.SetMaterialFlag(MaterialFlag.Lighting, false);
			cube.SetMaterialType(MaterialType.Reflection2Layer);
			*/

			if (settings.BackColor == null)
			{
				smgr.AddSkyBoxSceneNode(
					"../../media/irrlicht2_up.jpg",
					"../../media/irrlicht2_dn.jpg",
					"../../media/irrlicht2_lf.jpg",
					"../../media/irrlicht2_rt.jpg",
					"../../media/irrlicht2_ft.jpg",
					"../../media/irrlicht2_bk.jpg");
			}

			dev.GUIEnvironment.AddImage(
				drv.GetTexture("../../media/lime_logo_alpha.png"),
				new Vector2Di(30, 0));

			// draw all

			int lastFPS = -1;

			while (dev.Run())
			{
				if (settings.BackColor == null)
					// indeed, we do not need to spend time on cleaning color buffer if we use skybox
					drv.BeginScene(ClearBufferFlag.Depth);
				else
					drv.BeginScene(ClearBufferFlag.Depth | ClearBufferFlag.Color, settings.BackColor);

				smgr.DrawAll();
				dev.GUIEnvironment.DrawAll();
				drv.EndScene();

				int fps = drv.FPS;
				if (lastFPS != fps)
				{
					// report progress using common BackgroundWorker' method
					// note: we cannot do just labelRenderingStatus.Text = "...",
					// because we are running another thread
					worker.ReportProgress(fps, drv.Name);
					lastFPS = fps;
				}

				// if we requested to stop, we close the device
				if (worker.CancellationPending)
					dev.Close();
			}

			// drop device
			dev.Drop();
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			// if background worker still running, we send request to stop
			if (backgroundRendering.IsBusy)
			{
				backgroundRendering.CancelAsync();
				e.Cancel = true;
				userWantExit = true;
			}
		}


		private void backgroundRendering_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			// process reported progress

			int f = e.ProgressPercentage;
			string d = e.UserState as string;

			labelRenderingStatus.Text = string.Format(
				"Rendering {1} fps using {0} driver",
				d, f);
		}

		private void backgroundRendering_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			// if exception occured in rendering thread -- we display the message
			if (e.Error != null)
				MessageBox.Show(e.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

			// if user want exit - we close main form
			// note: it is the only way to close form correctly -- only when device dropped,
			// so background worker not running
			if (userWantExit)
				Close();

			labelRenderingStatus.Text = "No rendering";
		}
		/*
		void scaleSkeleton(SkinnedMesh mesh, float factor)
		{
			int nbJoints = mesh.JointCount;
			for (int i = 0; i < nbJoints; ++i)
			{
				SJoint joint = mesh.GetAllJoints()[i];
				joint.Animatedposition *= factor;
			}
		}
		*/
		private void setMaterialsSettings(AnimatedMeshSceneNode node)
		{
			if (node != null)
			{
				// materials with normal maps are not handled
				for (int i = 0; i < node.MaterialCount; ++i)
				{
					Material material = node.GetMaterial(i);
					if (material.Type == MaterialType.NormalMapSolid
						|| material.Type == MaterialType.ParallaxMapSolid)
					{
						material.Type = MaterialType.Solid;
					}
					else if (material.Type == MaterialType.NormalMapTransparentAddColor
						|| material.Type == MaterialType.ParallaxMapTransparentAddColor)
					{
						material.Type = MaterialType.TransparentAddColor;
					}
					else if (material.Type == MaterialType.NormalMapTransparentVertexAlpha
						|| material.Type == MaterialType.ParallaxMapTransparentVertexAlpha)
					{
						material.Type = MaterialType.TransparentVertexAlpha;
					}
				}

				node.SetMaterialFlag(MaterialFlag.Lighting, false);
				node.SetMaterialFlag(MaterialFlag.BackFaceCulling, false);

				for (int i = 1; i < 8; ++i) // 8 = _IRR_MATERIAL_MAX_TEXTURES_
					node.SetMaterialTexture(i, null);
			}
		}
	}
}
