﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MiniEngine.Configuration;
using MiniEngine.Editor.Configuration;
using MiniEngine.Editor.Controllers;
using MiniEngine.Editor.Scenes;
using MiniEngine.Editor.Workspaces;
using MiniEngine.Graphics;
using MiniEngine.Graphics.PostProcess;
using MiniEngine.Graphics.Utilities;
using MiniEngine.Systems.Pipeline;

namespace MiniEngine.Editor
{
    [Service]
    internal sealed class GameLoop
    {
        private readonly GraphicsDeviceManager Graphics;
        private readonly GraphicsDevice Device;
        private readonly OpaqueEffect Effect;
        private readonly FullScreenTriangle FullScreenTriangle;
        private readonly FrameService FrameService;
        private readonly WorkspaceManager WorkspaceManager;
        private readonly SceneManager SceneManager;
        private readonly GameWindow Window;
        private readonly GameTimer GameTimer;
        private readonly KeyboardController Keyboard;
        private readonly MouseController Mouse;
        private readonly CameraController CameraController;

        private readonly ParallelPipeline RenderPipeline;
        private readonly FrameCounter FrameCounter;
        private bool renderUi = true;

        public GameLoop(
            GraphicsDeviceManager graphics,
            GraphicsDevice device,
            OpaqueEffect effect,
            FullScreenTriangle fullScreenTriangle,
            FrameService frameService,
            WorkspaceManager workspaceManager,
            SceneManager sceneManager,
            GameWindow window,
            RenderPipelineBuilder renderPipelineBuilder,
            GameTimer gameTimer,
            KeyboardController keyboard,
            MouseController mouse,
            CameraController cameraController)
        {
            this.Graphics = graphics;
            this.Device = device;
            this.Effect = effect;
            this.FullScreenTriangle = fullScreenTriangle;
            this.FrameService = frameService;
            this.WorkspaceManager = workspaceManager;
            this.SceneManager = sceneManager;
            this.Window = window;
            this.GameTimer = gameTimer;
            this.Keyboard = keyboard;
            this.Mouse = mouse;
            this.CameraController = cameraController;

            this.RenderPipeline = renderPipelineBuilder.Build();
            this.FrameCounter = new FrameCounter();
        }

        internal bool Update(GameTime gameTime)
        {
            this.Keyboard.Update();
            this.Mouse.Update();
            if (this.Keyboard.Pressed(Keys.Escape))
            {
                return false;
            }

            if (this.Keyboard.Released(Keys.F11))
            {
                this.Graphics.SynchronizeWithVerticalRetrace = !this.Graphics.SynchronizeWithVerticalRetrace;
                this.GameTimer.IsFixedTimeStep = !this.GameTimer.IsFixedTimeStep;
                this.Graphics.ApplyChanges();
            }

            if (this.Keyboard.Released(Keys.F12))
            {
                this.renderUi = !this.renderUi;
            }

            var elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            this.CameraController.Update(this.FrameService.CamereComponent.Camera, elapsed);

            if (this.FrameCounter.Update(gameTime))
            {
                this.Window.Title = $"Editor :: {this.FrameCounter.MillisecondsPerFrame:F2}ms, {this.FrameCounter.FramesPerSecond} fps, {this.FrameService.GetBufferSize() * 0.000001f} MB";
            }

            this.SceneManager.Update(gameTime);
            return true;
        }

        internal void Draw(GameTime gameTime)
        {
            this.RunPipeline();
            this.RenderToViewport(this.FrameService.PBuffer.ToneMap);

            if (this.renderUi)
            {
                this.WorkspaceManager.Render(gameTime);
            }
        }

        private void RunPipeline()
        {
            this.RenderPipeline.Run();
            this.RenderPipeline.Wait();
        }

        private void RenderToViewport(RenderTarget2D renderTarget)
        {
            this.Device.SetRenderTarget(null);

            this.Device.SamplerStates[0] = SamplerState.LinearClamp;
            this.Device.BlendState = BlendState.Opaque;
            this.Device.DepthStencilState = DepthStencilState.None;
            this.Device.RasterizerState = RasterizerState.CullCounterClockwise;

            this.Effect.Texture = renderTarget;
            this.Effect.Apply();
            this.FullScreenTriangle.Render(this.Device);
        }

        public void Stop()
        {
            this.WorkspaceManager.Save();
            this.RenderPipeline.Stop();
        }
    }
}
