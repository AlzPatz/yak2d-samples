using SampleBase;
using System;
using System.Collections.Generic;
using System.Numerics;
using Yak2D;

namespace Draw_PersistentDrawQueue
{
    /// <summary>
    /// Using a persistent draw queue - somewhat depreciated (?)
    /// </summary>
    public class PersistentDrawQueueExample : ApplicationBase
    {
        private IDrawStage _drawStage;
        private ICamera2D _camera;
        private IPersistentDrawQueue _persistent;

        private bool hasQueueBeenCreated = false;

        private const float DURATION = 2.0f;
        private float _count = 0.0f;
        private float _fraction = 0.0f;

        public override string ReturnWindowTitle() => "Persistent Draw Queue";

        public override void OnStartup() { }

        public override bool CreateResources(IServices yak)
        {
            _drawStage = yak.Stages.CreateDrawStage();

            _camera = yak.Cameras.CreateCamera2D(960, 540, 1.0f);

            return true;
        }

        public override bool Update_(IServices yak, float timeSinceLastUpdateSeconds)
        {
            //Generate a repeating 0 to 1 fraction loop

            _count += timeSinceLastUpdateSeconds;

            while (_count > DURATION)
            {
                _count -= DURATION;
            }

            _fraction = _count / DURATION;

            return true;
        }

        public override void PreDrawing(IServices yak, float timeSinceLastDrawSeconds, float timeSinceLastUpdateSeconds) { }

        /*
            A DrawStage holds a single dynamic draw queue (generally cleared each frame) and any number of persistent draw queues
            The purpose of which is to enable:
                1. Avoid creating requests each frame for unchanging draw calls
                2. Avoid sorting and transfering data to the GPU where possible
                3. Allows interleaving of persistent and dynamic drawrequests in terms of layer and depth

            When a drawstage dynamic queue is cleared, any persistent queues are not cleared (hence the name)

            At render, all queues within a drawstage are sorted, draw calls are batched and data is uploaded to the GPU

            However, using a persistent draw queue is perhaps depreciated as a better / simpler method could be to load a single drawstage dynamic queue
            with draw requests and not clear them each frame (currently at drawstage creation auto clear is set)
            Not clearing a dynamic queue, and not adding to it, will mean no resort or re-transfer to GPU will be done (achieving almost the same)
            Any draws that need to be re-submitted each frame, can then be done on another drawstage
            This method does not allow interleaving of these requests into layers and depths, but it is likely that an app layers these types of requests anyway

            The persistent queues do reduce sorting and data transfer overhead, BUT if dynamic draw requests are interleaved between them, the set is resorted
            and data will be reuploaded to the GPU anyway. Therefore the use cases for a drawstage with both persistent and dynamic
            queues is perhaps limited and a little complex.

            The functionality is left in to give the user an option to run all drawing off one drawstage and reduce some of the sort and data transfer overhead
         */

        public override void Drawing(IDrawing draw,
                                     IFps fps,
                                     IInput input,
                                     ICoordinateTransforms transforms,
                                     float timeSinceLastDrawSeconds,
                                     float timeSinceLastUpdateSeconds)
        {
            // Create a persistent draw queue for the drawstage once
            if (!hasQueueBeenCreated)
            {
                _persistent = draw.CreatePersistentDrawQueue(_drawStage, GenerateDrawRequests(draw.Helpers));
                hasQueueBeenCreated = true;
            }

            //Each frame render a single draw request to the dynamic portion of the queue
            var position = new Vector2(400.0f * (float)Math.Sin(_fraction * Math.PI * 2.0f), 0.0f);
            var rotation = _fraction * (float)Math.PI * 2.0f;
            draw.Helpers.DrawColouredQuad(_drawStage, CoordinateSpace.Screen, Colour.OrangeRed, position, 80.0f, 80.0f, 0.5f, 1, rotation);

            //You can remove queues too (press delete)
            if (input.WasKeyReleasedThisFrame(KeyCode.Delete))
            {
                draw.RemovePersistentDrawQueue(_drawStage, _persistent);
            }
        }

        private DrawRequest[] GenerateDrawRequests(IDrawingHelpers helper)
        {
            var requests = new List<DrawRequest>();

            //Generate the draw requests. Happen to use fluent interface for this to create a wee effect... uses code from the other example project

            var startPosition = new Vector2(-350.0f, 150.0f);
            var startColour = new Colour(0.22f, 0.0f, 0.35f, 1.0f);
            var startDepth = 1.0f;

            //Create original drawing object and generate draw request
            var d = helper.Construct().Coloured(startColour).Poly(startPosition, 5, 100.0f).Filled();
            requests.Add(d.GenerateDrawRequest(CoordinateSpace.Screen, startDepth, 0));

            var endPosition = new Vector2(350.0f, -150.0f);
            var endColour = new Colour(1.0f, 0.71f, 0.76f, 0.5f);
            var endDepth = 0.0f;

            var totalRotation = 2.0f * (float)Math.PI;
            var numberSteps = 24;

            var shiftAmount = (endPosition - startPosition) / (1.0f * numberSteps);
            var rotAmount = totalRotation / (1.0f * numberSteps);

            for (var n = 0; n < numberSteps; n++)
            {
                var frac = (1.0f + n) / (1.0f * numberSteps);
                var col = startColour + (frac * (endColour - startColour));
                var depth = startDepth + (frac * (endDepth - startDepth));

                //Modify draw object incrementally and draw each coonfiguration
                d = d.ShiftPosition(shiftAmount).Rotate(rotAmount).ChangeColour(col);
                requests.Add(d.GenerateDrawRequest(CoordinateSpace.Screen, depth, 0));
            }

            return requests.ToArray();
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