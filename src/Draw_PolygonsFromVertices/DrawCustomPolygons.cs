using SampleBase;
using System.Numerics;
using Yak2D;

namespace Draw_PolygonsFromVertices
{
    /// <summary>
    /// Drawing Polygons by Defining their Vertices and Indices (rather than using Helper Functions)
    /// </summary>
    public class DrawCustomPolygons : ApplicationBase
    {
        private IDrawStage _drawStage;
        private ICamera2D _camera;
        private ITexture _textureCity;
        private ITexture _textureGrass;
        private ITexture _textureMud;

        private DrawRequest _requestColouredTriangle;
        private DrawRequest _requestTexturedRectange;
        private DrawRequest _requestSkewedRectangleDualTextured;

        public override string ReturnWindowTitle() => "Drawing Custom 2D Polygons";

        public override void OnStartup() { }

        public override bool CreateResources(IServices yak)
        {
            _drawStage = yak.Stages.CreateDrawStage();

            _camera = yak.Cameras.CreateCamera2D(960, 540, 1.0f);

            _textureCity = yak.Surfaces.LoadTexture("city", AssetSourceEnum.Embedded);
            _textureGrass = yak.Surfaces.LoadTexture("grass", AssetSourceEnum.Embedded);
            _textureMud = yak.Surfaces.LoadTexture("mudrock", AssetSourceEnum.Embedded);

            CreateDrawRequests();

            return true;
        }

        private void CreateDrawRequests()
        {
            //Create Draw Requests - rely on texture items so recreated whenever CreateResources() fires

            _requestColouredTriangle = new DrawRequest
            {
                FillType = FillType.Coloured,
                Colour = Colour.White,
                CoordinateSpace = CoordinateSpace.Screen,
                Vertices = new Vertex2D[]
                   {
                        new Vertex2D { Colour = Colour.Red, Position = new Vector2(-200.0f, 200.0f), TexCoord0 = Vector2.Zero,  TexCoord1 = Vector2.Zero, TexWeighting = 1.0f },
                        new Vertex2D { Colour = Colour.Green, Position = new Vector2(0.0f, 0.0f), TexCoord0 = Vector2.Zero,  TexCoord1 = Vector2.Zero, TexWeighting = 1.0f },
                        new Vertex2D { Colour = Colour.Blue, Position = new Vector2(-400.0f, 0.0f), TexCoord0 = Vector2.Zero,  TexCoord1 = Vector2.Zero, TexWeighting = 1.0f }
                   },
                Indices = new int[]
                   {
                        0, 1, 2
                   },
                Depth = 0.5f,
                Layer = 0,
                Texture0 = null,
                Texture1 = null,
                TextureWrap0 = TextureCoordinateMode.None,
                TextureWrap1 = TextureCoordinateMode.None
            };

            _requestTexturedRectange = new DrawRequest
            {
                FillType = FillType.Textured,
                Colour = Colour.White,
                CoordinateSpace = CoordinateSpace.Screen,
                Vertices = new Vertex2D[]
                {
                        new Vertex2D { Colour = Colour.White, Position = new Vector2(-100.0f, 100.0f), TexCoord0 = new Vector2(0.0f, 0.0f),  TexCoord1 = Vector2.Zero, TexWeighting = 1.0f },
                        new Vertex2D { Colour = Colour.White, Position = new Vector2(200.0f, 100.0f), TexCoord0 =  new Vector2(1.0f, 0.0f),  TexCoord1 = Vector2.Zero, TexWeighting = 1.0f },
                        new Vertex2D { Colour = Colour.White, Position = new Vector2(200.0f, -200.0f), TexCoord0 =  new Vector2(1.0f, 1.0f),  TexCoord1 = Vector2.Zero, TexWeighting = 1.0f },
                        new Vertex2D { Colour = Colour.White, Position = new Vector2(-100.0f, -200.0f), TexCoord0 = new Vector2(0.0f, 1.0f),  TexCoord1 = Vector2.Zero, TexWeighting = 1.0f }
                },
                Indices = new int[]
                {
                        0, 1, 3, 1, 3, 2
                },
                Depth = 0.2f,
                Layer = 0,
                Texture0 = _textureCity,
                Texture1 = null,
                TextureWrap0 = TextureCoordinateMode.Mirror,
                TextureWrap1 = TextureCoordinateMode.Mirror
            };

            _requestSkewedRectangleDualTextured = new DrawRequest
            {
                FillType = FillType.DualTextured,
                Colour = new Colour(1.0f, 1.0f, 1.0f, 0.8f),
                CoordinateSpace = CoordinateSpace.Screen,
                Vertices = new Vertex2D[]
                {
                        new Vertex2D { Colour = Colour.White, Position = new Vector2(100.0f, 150.0f), TexCoord0 = new Vector2(0.0f, 0.0f),  TexCoord1= new Vector2(0.0f, 0.0f), TexWeighting = 1.0f },
                        new Vertex2D { Colour = Colour.White, Position = new Vector2(400.0f, 200.0f), TexCoord0 =  new Vector2(1.0f, 0.0f),  TexCoord1 = new Vector2(1.0f, 0.0f), TexWeighting = 0.0f },
                        new Vertex2D { Colour = Colour.White, Position = new Vector2(400.0f, -100.0f), TexCoord0 =  new Vector2(1.0f, 1.0f), TexCoord1= new Vector2(1.0f, 1.0f), TexWeighting = 0.0f },
                        new Vertex2D { Colour = Colour.White, Position = new Vector2(100.0f, -50.0f),  TexCoord0 = new Vector2(0.0f, 1.0f),  TexCoord1 = new Vector2(0.0f, 1.0f), TexWeighting = 1.0f }
                },
                Indices = new int[]
                {
                        0, 1, 3, 1, 3, 2
                },
                Depth = 0.9f,
                Layer = 1, //Note layer 1, so will appear on top of those in layer 0, despite having a higher depth value
                Texture0 = _textureGrass,
                Texture1 = _textureMud,
                TextureWrap0 = TextureCoordinateMode.Mirror,
                TextureWrap1 = TextureCoordinateMode.Mirror
            };
        }

        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds) => true;
        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        public override void Drawing(IDrawing drawing,
                                     IFps fps,
                                     IInput input,
                                     ICoordinateTransforms transforms,
                                     float timeSinceLastDrawSeconds,
                                     float timeSinceLastUpdateSeconds)
        {
            drawing.Draw(_drawStage, _requestColouredTriangle);
            drawing.Draw(_drawStage, _requestTexturedRectange);
            drawing.Draw(_drawStage, _requestSkewedRectangleDualTextured);
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
