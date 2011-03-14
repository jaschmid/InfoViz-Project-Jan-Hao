using System;
using System.Collections.Generic;
using System.Text;

using Gav.Graphics;
using Gav.Data;

using Microsoft.DirectX;
using System.Drawing;
using Gav.Components.Internal;
using Gav.Components.GavLayers;
using Gav.Management;
using Microsoft.DirectX.Direct3D;

namespace InfoVizProject
{
    public partial class CustomComponent : GavComponent
    {
        private class Layer : ComponentLayer
        {
            // Bool to check initialization
            protected bool _inited;

            public IDataCubeProvider Input { get; set; }

            public ColorMap ColorMap { get; set; }
        }

        private List<Layer> layers;

        public IDataCubeProvider Input
        {
            get { return layers[0].Input; }
            set { foreach (Layer layer in layers) layer.Input = value; }
        }

        public ColorMap ColorMap
        {
            get { return layers[0].ColorMap; }
            set { foreach (Layer layer in layers) layer.ColorMap = value; }
        }


        public CustomComponent()
            : base()
        {
            layers = new List<Layer>();

            //InteractionLayer interaction1 = new InteractionLayer();
            //interaction1.Control.SetOuterMargins(20, GavControl.DistanceType.Absolute);
            //_sampleLayers.Add(interaction1);

            //InteractionLayer interaction2 = new InteractionLayer();
            //interaction2.Control.SetOuterMargins(30, GavControl.DistanceType.Absolute);
            //_sampleLayers.Add(interaction2);
            /*
            GlyphLayer glyphLayer = new GlyphLayer();
            glyphLayer.Control.SetOuterMargins(10, GavControl.DistanceType.Absolute);
            layers.Add(glyphLayer);*/

            LineLayer lineLayer = new LineLayer();
            lineLayer.Control.SetOuterMargins(10, GavControl.DistanceType.Absolute);
            layers.Add(lineLayer);
            
            TextLayer textLayer = new TextLayer();
            textLayer.Control.SetOuterMargins(10, GavControl.DistanceType.Absolute);
            layers.Add(textLayer);


            foreach (Layer layer in layers)
            {
                AddLayer(layer);
            }
        }

        /// <summary>
        /// Layer that draws one glyph per item in the dataset, x/y position is determined by the first two columns and size is from the value in the third.
        /// </summary>
        private class GlyphLayer : Layer
        {
            // Used to map from data values to 0-1
            private AxisMap _axisMapX, _axisMapY, _axisMapSize;
            // Stores the actual glyph positions and sizes
            private List<Vector2> _glyphPositions;
            private List<float> _glyphSizes;
            // Texture containing the glyph used in the plot.
            private Texture _glyphTexture;
            // Sprite object used to render glyphs.
            private Sprite _sprite;
            // The size from the actual texture source
            private int _glyphTextureSize;
            // Column indecies (attributes) in the input data to use on the tow axes andfor size
            int _xIndex = 0;
            int _yIndex = 1;
            int _sizeIndex = 2;

            public GlyphLayer()
            {
                _axisMapX = new AxisMap();
                _axisMapY = new AxisMap();
                _axisMapSize = new AxisMap();

                _glyphPositions = new List<Vector2>();
                _glyphSizes = new List<float>();
            }

            protected void Initialize(Device device)
            {
                // Sprite object used to draw the glyphs
                _sprite = new Sprite(device);
                // White circle texture loaded from a file (make sure the file is where it should be)
                Bitmap bitMap = new Bitmap("Graphics_Glyph.png");
                _glyphTexture = new Texture(device, bitMap, Usage.None, Pool.Managed);
                // Size of the texture
                _glyphTextureSize = bitMap.Width;
            }

            protected override void Render(Device device)
            {
                if (!_inited) Initialize(device);
                CreateGlyphs();

                _sprite.Begin(SpriteFlags.AlphaBlend);

                for (int i = 0; i < _glyphPositions.Count; i++)
                {
                    float scale;

                    scale = Math.Min(Control.AbsoluteSize.Width, Control.AbsoluteSize.Height) * (_glyphSizes[i] * 0.1f);
                    scale /= (float)_glyphTextureSize;


                    Point p = new Point();
                    p.X = (int)(_glyphPositions[i].X * Control.AbsoluteSize.Width);
                    p.Y = (int)((1.0f - _glyphPositions[i].Y) * Control.AbsoluteSize.Height);
                    p.X = (int)(p.X / scale);
                    p.Y = (int)(p.Y / scale);

                    _sprite.Transform = Matrix.Scaling(scale, scale, 1);
                    _sprite.Draw(_glyphTexture, new Vector3(_glyphTextureSize / 2, _glyphTextureSize / 2, 0), new Vector3(p.X, p.Y, 0), ColorMap.GetColor(i).ToArgb());
                }
                _sprite.End();
            }

            private void CreateGlyphs()
            {
                // Check for problems with the input
                if (Input == null || Input.GetDataCube() == null || Input.GetDataCube().DataArray == null) return;

                _axisMapX.Input = Input;
                _axisMapY.Input = Input;
                _axisMapSize.Input = Input;

                _axisMapX.Index = _xIndex;
                _axisMapY.Index = _yIndex;
                _axisMapSize.Index = _sizeIndex;

                _axisMapX.DoMapping();
                _axisMapY.DoMapping();
                _axisMapSize.DoMapping();

                _glyphPositions.Clear();
                _glyphSizes.Clear();
                // AxisMap.MappedValues contains the input values scaled between 0 and 1 according to each columns max and min
                for (int i = 0; i < Input.GetDataCube().DataArray.GetLength(1); i++)
                {
                    _glyphPositions.Add(new Vector2(_axisMapX.MappedValues[i], _axisMapY.MappedValues[i]));
                    _glyphSizes.Add(_axisMapSize.MappedValues[i]);
                }
            }
        }
        
        /// <summary>
        /// Layer that writes some text
        /// </summary>
        private class TextLayer : Layer
        {

            private Microsoft.DirectX.Direct3D.Font _d3dFont;
            private System.Drawing.Font _font;

            private Mesh _textMesh;
            private Material _material;

            public TextLayer()
            {
                _font = new System.Drawing.Font("Verdana", 10);
            }

            protected void Initialize(Device device)
            {
                // Use the GAV fontpool to avoid creating extra copies of fonts
                _d3dFont = FontPool.GetFont(_font, device);
                _textMesh = Mesh.Box(device, 0.5f, 0.5f, 0.5f);

                _material = new Material();
                _material.Ambient = Color.Red;
                _material.Diffuse = Color.Red;
                _material.Emissive = Color.Red;

                _inited = true;
            }

            protected override void Render(Device device)
            {
                if (!_inited) Initialize(device);

                // As d3dFont uses a sprite to draw, it draws in component absolute space, ignoring the margins of the layer,
                // therefore we need to convert from Layer Absolute to Component Absolute.
                /*Point textPosition = new Point((int)Control.ConvertLayerAbsoluteXToComponentAbsoluteX(0), (int)Control.ConvertLayerAbsoluteXToComponentAbsoluteX(1));
                _d3dFont.DrawText(null, "This text is drawn using DirectX Font class", textPosition, Color.Black);*/

                Material black = new Material();
                black.Emissive = Color.Red;
                black.Diffuse = Color.Red;
                black.Ambient = Color.Red;

                device.RenderState.Lighting = true;
                device.Material = black;
                device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, Control.AbsoluteSize.Width * 1.0f / Control.AbsoluteSize.Height, 0.1f, 10.0f);
                device.Transform.View = Matrix.LookAtLH(new Vector3(0, 0, -10), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
                device.Transform.World = Matrix.Scaling(0.5f, 1f, 1) * Matrix.Translation(-3f, 0, 0);

                Mesh m = Mesh.Box(device, 0.5f, 0.5f, 0.5f);
                m.DrawSubset(0);
            }
        }

        /// <summary>
        /// Layer that draws a "thing" that you can move and change color to. 
        /// Draws in object space but handles interaction in screen space, shows off the conversion between the two.
        /// </summary>
        private class InteractionLayer : Layer
        {
            private Vector3[] _vertices;
            private VertexBuffer _vb;
            private Material _material;

            private Vector2 _mouseDownDelta;
            private Vector3 _boxPosition;
            private float _scale;
            private bool _moveBox;

            public InteractionLayer()
            {
                _vertices = new Vector3[4];
                _vertices[0] = new Vector3(-1, -1, 0);
                _vertices[1] = new Vector3(-1, 1, 0);
                _vertices[2] = new Vector3(1, -1, 0);
                _vertices[3] = new Vector3(1, 1, 0);

            }

            protected void Initialize(Device device)
            {
                _inited = true;
                _vb = new VertexBuffer(typeof(CustomVertex.PositionOnly), 4, device, Usage.None, CustomVertex.PositionOnly.Format, Pool.Managed);
                _vb.SetData(_vertices, 0, LockFlags.None);
                _boxPosition = new Vector3(0.5f, 0.5f, 0);
                _scale = 0.25f;

                // Pick a random color from the ColorMap
                Random rand = new Random();
                Color boxColor = ColorMap.GetColor(rand.Next(Input.GetDataCube().DataArray.GetLength(1)));

                _material = new Material();
                _material.Ambient = boxColor;
                _material.Diffuse = boxColor;
                _material.Emissive = boxColor;
            }

            protected override void Render(Device device)
            {
                if (!_inited) Initialize(device);

                device.RenderState.Lighting = true;
                device.Material = _material;
                device.VertexFormat = CustomVertex.PositionOnly.Format;

                device.Transform.World = Matrix.Scaling(_scale, _scale, 1) * Matrix.Translation(_boxPosition);

                device.SetStreamSource(0, _vb, 0);
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }

            protected override bool MouseDown(Gav.Components.Events.LayerMouseButtonEventArgs e)
            {
                Vector2 pos = e.GetLayerRelativePosition();
                if (_boxPosition.X + _scale > pos.X && _boxPosition.X - _scale < pos.X &&
                    _boxPosition.Y + _scale > pos.Y && _boxPosition.Y - _scale < pos.Y)
                {
                    _moveBox = true;
                    _mouseDownDelta.X = _boxPosition.X - pos.X;
                    _mouseDownDelta.Y = _boxPosition.Y - pos.Y;
                    // Returnning true locks the mouse to this layer until false is returned
                    return true;
                }

                return false;
            }

            protected override bool MouseMove(Gav.Components.Events.LayerMouseMoveEventArgs e)
            {
                if (_moveBox)
                {
                    Vector2 pos = e.GetLayerRelativePosition();
                    _boxPosition = new Vector3(pos.X + _mouseDownDelta.X, pos.Y + _mouseDownDelta.Y, 0);
                    Render();
                    return true;
                }
                return false;
            }

            protected override bool MouseUp(Gav.Components.Events.LayerMouseButtonEventArgs e)
            {
                _moveBox = false;
                return false;
            }

            protected override bool MouseWheel(Gav.Components.Events.LayerMouseWheelEventArgs e)
            {
                _scale += e.WheelDelta / 1000.0f * Math.Abs(_scale);
                Render();
                // Return false to continue sending MouseWheel to the other layers,
                // return true to stop sending the event.
                return true;
            }
        }
    }


}

