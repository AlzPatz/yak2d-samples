using SampleBase;
using System;
using System.Collections.Generic;
using System.Numerics;
using Yak2D;

namespace FrameworkItems_CreationAndDestruction
{
    /// <summary>
    /// 
    /// </summary>
    public class CreationAndDestruction : ApplicationBase
    {
        /*
            NOTE - if you destroy a framework item during an update() cycle which is being used in a current draw() cycle
            the asset may not be avaliable for the renderer; which will cause unexpected / erroneous behaviour or throw
            an exception. 

            Therefore, be sure you are not currently using an item when destroying it, OR destroy assets in PreDrawing or Drawing 
            Methods, as those are NOT executed while a render is inflight
         */

        private IDrawStage _drawStage;
        private ICamera2D _camera;

        private List<KeyCode> _keysHeld;

        private List<ITexture> _textures;
        private List<IRenderTarget> _renderTargets;
        private List<IRenderStage> _renderStages;
        private List<IFont> _fonts;
        private List<ICamera> _cameras;
        private List<IViewport> _viewports;

        private int _textureCount;
        private int _renderTargetCount;
        private int _viewportCount;
        private int _renderStageCount;
        private int _fontCount;
        private int _cameraCount;
        private Random _rnd;

        public override string ReturnWindowTitle() => "Asset Creation and Destruction";

        public override void OnStartup()
        {
            _textures = new List<ITexture>();
            _renderTargets = new List<IRenderTarget>();
            _viewports = new List<IViewport>();
            _renderStages = new List<IRenderStage>();
            _fonts = new List<IFont>();
            _cameras = new List<ICamera>();

            _rnd = new Random();

            _keysHeld = new List<KeyCode>();
        }

        public override bool CreateResources(IServices yak)
        {
            _textures.Clear();
            _renderTargets.Clear();
            _viewports.Clear();
            _renderStages.Clear();
            _fonts.Clear();
            _cameras.Clear();

            _drawStage = yak.Stages.CreateDrawStage();

            _camera = yak.Cameras.CreateCamera2D();

            _keysHeld.Clear();

            return true;
        }
        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds) => true;

        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds)
        {
            //PreDrawing is the best place to do realtime item destruction as a render (which could be using an asset) is not inflight

            //For purpose of this example I want to create and destroy framework items on key releases
            //As I want to do it in predrawing, where the input services (keys released this frame) are not 
            //reliable as they are update based and more than one update can happen between draws
            //The following code does some rudimentary input tracking using the reliable currently held input information
            var keysHeldNow = yak.Input.KeysHeldDown();

            var keysReleasedThisFrame = new List<KeyCode>();

            _keysHeld.ForEach(key =>
            {
                if (!keysHeldNow.Contains(key))
                {
                    keysReleasedThisFrame.Add(key);
                }
            });

            _keysHeld = keysHeldNow;

            //React to key releases

            //Add New (or delete)
            var add = !_keysHeld.Contains(KeyCode.ShiftLeft);
            //Deleting all
            var deleteAll = _keysHeld.Contains(KeyCode.ControlLeft);

            //Texture
            if (keysReleasedThisFrame.Contains(KeyCode.T))
            {
                if (add & !deleteAll)
                {
                    _textures.Add(yak.Surfaces.LoadTexture("stonewall", AssetSourceEnum.Embedded));
                }
                else
                {
                    if (deleteAll)
                    {
                        yak.Surfaces.DestroyAllUserTextures();
                        _textures.Clear();
                    }
                    else
                    {
                        if (_textures.Count > 0)
                        {
                            var texture = _textures[0];
                            yak.Surfaces.DestroySurface(texture);
                            _textures.RemoveAt(0);
                        }
                    }
                }
            }

            //Render Targets
            if (keysReleasedThisFrame.Contains(KeyCode.R))
            {
                if (add &!deleteAll)
                {
                    _renderTargets.Add(yak.Surfaces.CreateRenderTarget(128, 128));
                }
                else
                {
                    if (deleteAll)
                    {
                        yak.Surfaces.DestroyAllUserRenderTargets();
                        _renderTargets.Clear();
                    }
                    else
                    {
                        if (_renderTargets.Count > 0)
                        {
                            var renderTarget = _renderTargets[0];
                            yak.Surfaces.DestroySurface(renderTarget);
                            _renderTargets.RemoveAt(0);
                        }
                    }
                }
            }

            //Viewports
            if (keysReleasedThisFrame.Contains(KeyCode.V))
            {
                if (add & !deleteAll)
                {
                    _viewports.Add(yak.Stages.CreateViewport(0, 0, 128, 128));
                }
                else
                {
                    if (deleteAll)
                    {
                        yak.Stages.DestroyAllViewports();
                        _viewports.Clear();
                    }
                    else
                    {
                        if (_viewports.Count > 0)
                        {
                            var viewport = _viewports[0];
                            yak.Stages.DestroyViewport(viewport);
                            _viewports.RemoveAt(0);
                        }
                    }
                }
            }

            //Renderstages (adds one at random)
            if (keysReleasedThisFrame.Contains(KeyCode.S))
            {
                if (add & !deleteAll)
                {
                    var numStages = 10;
                    var n = (int)(_rnd.NextDouble() * numStages);
                    if (n == numStages)
                    {
                        n = 0;
                    }

                    var s = yak.Stages;
                    IRenderStage stage = null;

                    //No custom veldrid stage in here, just because can't be bothered to set up a fake
                    switch (n)
                    {
                        case 0:
                            stage = s.CreateDrawStage();
                            break;
                        case 1:
                            stage = s.CreateDistortionStage(128, 128, true);
                            break;
                        case 2:
                            stage = s.CreateBloomStage(128, 128);
                            break;
                        case 3:
                            stage = s.CreateBlur1DStage(128, 128);
                            break;
                        case 4:
                            stage = s.CreateBlurStage(128, 128);
                            break;
                        case 5:
                            stage = s.CreateColourEffectsStage();
                            break;
                        case 6:
                            stage = s.CreateCustomShaderStage("CustomBinaryFragment",
                            AssetSourceEnum.Embedded,
                               new ShaderUniformDescription[]
                               {
                                   new ShaderUniformDescription
                                   {
                                       Name = "Texture",
                                       UniformType = ShaderUniformType.Texture,
                                       SizeInBytes = 0
                                   },
                                   new ShaderUniformDescription
                                   {
                                       Name = "Threshold",
                                       UniformType = ShaderUniformType.Data,
                                       SizeInBytes = 32
                                   }
                               }, BlendState.Override);
                            break;
                        case 7:
                            stage = s.CreateMeshRenderStage();
                            break;
                        case 8:
                            stage = s.CreateMixStage();
                            break;
                        case 9:
                            stage = s.CreateStyleEffectsStage();
                            break;
                    }

                    _renderStages.Add(stage);
                }
                else
                {
                    if (deleteAll)
                    {
                        yak.Stages.DestroyAllStages();
                        _renderStages.Clear();
                        //We must recreate our draw stage!
                        _drawStage = yak.Stages.CreateDrawStage();
                    }
                    else
                    {
                        if (_renderStages.Count > 0)
                        {
                            var stage = _renderStages[0];
                            yak.Stages.DestroyStage(stage);
                            _renderStages.RemoveAt(0);
                        }
                    }
                }
            }

            //Fonts
            if (keysReleasedThisFrame.Contains(KeyCode.F))
            {
                if (add & !deleteAll)
                {
                    _fonts.Add(yak.Fonts.LoadFont("snappy_38", AssetSourceEnum.Embedded));
                }
                else
                {
                    if (deleteAll)
                    {
                        yak.Fonts.DestroyAllUserFonts();
                        _fonts.Clear();
                    }
                    else
                    {
                        if (_fonts.Count > 0)
                        {
                            var viewport = _fonts[0];
                            yak.Fonts.DestroyFont(viewport);
                            _fonts.RemoveAt(0);
                        }
                    }
                }
            }

            //Cameras
            if (keysReleasedThisFrame.Contains(KeyCode.C))
            {
                if (add & !deleteAll)
                {
                    _cameras.Add(_rnd.NextDouble() > 0.5f ?
                        (ICamera)yak.Cameras.CreateCamera2D() :
                        (ICamera)yak.Cameras.CreateCamera3D(Vector3.Zero, Vector3.Zero, Vector3.UnitY));
                }
                else
                {
                    if (deleteAll)
                    {
                        yak.Cameras.DestroyAllCameras();
                        _cameras.Clear();
                        //We must remake our draw camera!
                        _camera = yak.Cameras.CreateCamera2D();
                    }
                    else
                    {
                        if (_cameras.Count > 0)
                        {
                            var camera = _cameras[0];
                            yak.Cameras.DestroyCamera(camera);
                            _cameras.RemoveAt(0);
                        }
                    }
                }
            }

            //Destroy Everything:
            if (keysReleasedThisFrame.Contains(KeyCode.P))
            {
                yak.Surfaces.DestoryAllUserSurfaces(); //Deletes both Textures and RenderTargets
                _textures.Clear();
                _renderTargets.Clear();

                yak.Stages.DestroyAllViewports();
                _viewports.Clear();

                yak.Stages.DestroyAllStages();
                _renderStages.Clear();
                //We must remake our Draw Stage! :)
                _drawStage = yak.Stages.CreateDrawStage();

                yak.Fonts.DestroyAllUserFonts();
                _fonts.Clear();

                //yak.Cameras.DestroyAllCameras(); //Using the 2D and 3D seperate deletion functions instead
                yak.Cameras.DestroyAllCameras2D();
                yak.Cameras.DestroyAllCameras3D();
                _cameras.Clear();
                //We must remake our draw camera!
                _camera = yak.Cameras.CreateCamera2D();

            }

            //Update Counts
            _textureCount = yak.Surfaces.UserTextureCount;
            _renderTargetCount = yak.Surfaces.UserRenderTargetCount;
            _viewportCount = yak.Stages.CountViewports;
            _renderStageCount = yak.Stages.CountRenderStages;
            _fontCount = yak.Fonts.UserFontCount;
            _cameraCount = yak.Cameras.Camera2DCount + yak.Cameras.Camera3DCount;
        }

        public override void Drawing(IDrawing draw, IFps fps, IInput input, ICoordinateTransforms transform, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) 
        {
            var left = -460.0f;
            var fontSize = 24.0f;
            var ySpacing = 32.0f;
            var yCurrent = 200.0f;

            var strs = new List<string>
            {
                "Create and Destroy Framework Items",
                "Shift-Key for Delete One and Control-Key for Delete All",
                "P to delete all items",
                "",
                string.Concat("Textures (T): ", _textureCount),
                string.Concat("RenderTargets (R): ", _renderTargetCount),
                string.Concat("Viewports (V): ", _viewportCount),
                string.Concat("RenderStages (S): ", _renderStageCount),
                string.Concat("Fonts (F): ", _fontCount),
                string.Concat("Cameras (C): ", _cameraCount),
                "",
                "There will always be one RenderStage and one Camera, ", 
                "as those are used to render this text"
            };

            strs.ForEach(str =>
            {
                draw.DrawString(_drawStage,
                         CoordinateSpace.Screen,
                         str,
                         Colour.White,
                         fontSize,
                         new Vector2(left, yCurrent),
                         TextJustify.Left,
                         0.5f,
                         0);
                
                yCurrent -= ySpacing;
            });
        }

        public override void Rendering(IRenderQueue q, IRenderTarget windowRenderTarget)
        {
            q.ClearColour(windowRenderTarget, Colour.Clear);
            q.ClearDepth(windowRenderTarget);
            q.Draw(_drawStage, _camera, windowRenderTarget);
        }

        public override void Shutdown() { }
    }
}