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


        // Settings
        public Axis LineSourceAxis
        {
            get{ return this.lineLayer.LineSourceAxis;}
            set{ this.lineLayer.LineSourceAxis = value;}
        }
        public Axis TimeSourceAxis
        {
            get { return this.lineLayer.TimeSourceAxis; }
            set { this.lineLayer.TimeSourceAxis = value; }
        }
        public Axis DataSourceAxis
        {
            get { return this.lineLayer.DataSourceAxis; }
            set { this.lineLayer.DataSourceAxis = value; }
        }
        public int DataLineXIndex
        {
            get { return this.lineLayer.DataLineXIndex; }
            set { this.lineLayer.DataLineXIndex = value; }
        }
        public int DataLineYIndex
        {
            get { return this.lineLayer.DataLineYIndex; }
            set { this.lineLayer.DataLineYIndex = value; }
        }
        public int DataLineThicknessIndex
        {
            get { return this.lineLayer.DataLineThicknessIndex; }
            set { this.lineLayer.DataLineThicknessIndex = value; }
        }
        public Color SelectedIndexColor
        {
            get { return this.lineLayer.SelectedIndexColor; }
            set { this.lineLayer.SelectedIndexColor = value; }
        }
        public void SetSelectedIndexes(List<int> indexes)
        {
            this.lineLayer.SetSelectedIndexes(indexes);
        }
        public void SetAxisSize(float XAxisSize, float YAxisSize)
        {
            this.lineLayer.XAxisSpacing = XAxisSize;
            this.lineLayer.YAxisSpacing = YAxisSize;
        }
        public void SetAxisLabels(string XAxisLabel, string YAxisLabel)
        {
            this.axisLayer.XAxisLabel = XAxisLabel;
            this.axisLayer.YAxisLabel = YAxisLabel;
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

            this.lineLayer = new LineLayer();
            this.lineLayer.Control.SetOuterMargins(10, GavControl.DistanceType.Absolute);
            this.lineLayer.XAxisSpacing = 30.0f;
            this.lineLayer.YAxisSpacing = 30.0f;
            layers.Add(this.lineLayer);


            this.axisLayer = new AxisLayer();
            this.axisLayer.Control.SetOuterMargins(10, GavControl.DistanceType.Absolute);
            this.axisLayer.XAxisSpacing = 30.0f;
            this.axisLayer.YAxisSpacing = 30.0f;
            layers.Add(this.axisLayer);
            /*
            TextLayer textLayer = new TextLayer();
            textLayer.Control.SetOuterMargins(10, GavControl.DistanceType.Absolute);
            layers.Add(textLayer);
            */

            foreach (Layer layer in layers)
            {
                AddLayer(layer);
            }
        }

        private LineLayer lineLayer;
        private AxisLayer axisLayer;

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

