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
    public partial class CustomComponent
    {
        class AxisLayer : Layer
        {
            private float yAxisSpacing;
            public float YAxisSpacing
            {
                get
                {
                    return yAxisSpacing;
                }
                set
                {
                    yAxisSpacing = value;
                    generateAxis();
                }
            }
            private float xAxisSpacing;
            public float XAxisSpacing
            {
                get
                {
                    return xAxisSpacing;
                }
                set
                {
                    xAxisSpacing = value;
                    generateAxis();
                }
            }
            private string xAxisLabel;
            public string XAxisLabel
            {
                get
                {
                    return xAxisLabel;
                }
                set
                {
                    this.xAxisLabel = value;
                    if (this.device != null)
                        xAxisMesh = Mesh.TextFromFont(this.device, _font, this.XAxisLabel, 0, 0.5f);
                }
            }
            private string yAxisLabel;
            public string YAxisLabel
            {
                get
                {
                    return yAxisLabel;
                }
                set
                {
                    this.yAxisLabel = value;
                    if (this.device != null)
                        yAxisMesh = Mesh.TextFromFont(this.device, _font, this.YAxisLabel, 0, 0.5f);
                }
            }

            private Microsoft.DirectX.Direct3D.Line _d3dLine;
            private System.Drawing.Font _font;

            private Mesh xAxisMesh;
            private Mesh yAxisMesh;
            private Device device;
            private Material _material;

            private Vector2[] xAxis;
            private Vector2[] yAxis;

            private void generateAxis()
            {

                int w = Control.AbsoluteSize.Width;
                int h = Control.AbsoluteSize.Height;

                xAxis[0].X = XAxisSpacing;
                xAxis[0].Y = h-YAxisSpacing / 8.0f;
                xAxis[1].X = w;
                xAxis[1].Y = h-YAxisSpacing / 8.0f;
                xAxis[2].X = w - YAxisSpacing / 8.0f;
                xAxis[2].Y = h-YAxisSpacing / 4.0f;
                xAxis[3] = xAxis[1];
                xAxis[4].X = w - YAxisSpacing / 8.0f;
                xAxis[4].Y = h;


                yAxis[0].X = XAxisSpacing / 8.0f;
                yAxis[0].Y = h-YAxisSpacing;
                yAxis[1].X = XAxisSpacing / 8.0f;
                yAxis[1].Y = 0.0f;
                yAxis[2].X = XAxisSpacing / 4.0f;
                yAxis[2].Y = XAxisSpacing / 8.0f;
                yAxis[3] = yAxis[1];
                yAxis[4].X = 0;
                yAxis[4].Y = XAxisSpacing / 8.0f;
            }

            public AxisLayer()
            {
                xAxis = new Vector2[5];
                yAxis = new Vector2[5];
                _font = new System.Drawing.Font("Arial", 10);
                this.XAxisLabel = "X Axis";
                this.YAxisLabel = "Y Axis";
                this.XAxisSpacing = 30.0f;
                this.YAxisSpacing = 30.0f;
            }

            protected  void Initialize(Device device)
            {
                // Use the GAV fontpool to avoid creating extra copies of fonts
                this.device = device;
                this._d3dLine = new Microsoft.DirectX.Direct3D.Line(device);
                this.xAxisMesh = Mesh.TextFromFont(this.device, _font, this.XAxisLabel, 0, 0.5f);
                this.yAxisMesh = Mesh.TextFromFont(this.device, _font, this.YAxisLabel, 0, 0.5f);

                this._material = new Material();
                this._material.Ambient = Color.Black;
                this._material.Diffuse = Color.Black;
                this._material.Emissive = Color.Black;

                this._inited = true;
            }

            protected override void Render(Device device)
            {
                if (!_inited) Initialize(device);
                
                device.RenderState.Lighting = true;
                device.Material = _material;

                int w = Control.AbsoluteSize.Width;
                int h = Control.AbsoluteSize.Height;

                device.Transform.Projection = Matrix.OrthoLH(w, Control.AbsoluteSize.Height, 0.01f, 100.0f);

                device.Transform.View = Matrix.LookAtLH(new Vector3(0, 0, -10), new Vector3(0, 0, 0), new Vector3(0, 1, 0));

                device.Transform.World = Matrix.Scaling(YAxisSpacing * 3.0f / 4.0f, YAxisSpacing * 3.0f / 4.0f, 1) * Matrix.Translation(-w / 2 + XAxisSpacing, -h / 2 + YAxisSpacing / 4, 0);
                xAxisMesh.DrawSubset(0);

                device.Transform.World = Matrix.RotationZ((float)Math.PI / 2.0f) * Matrix.Scaling(XAxisSpacing * 3.0f / 4.0f, XAxisSpacing * 3.0f / 4.0f, 1) * Matrix.Translation(-w / 2 + XAxisSpacing*3 / 4, -h / 2 + YAxisSpacing, 0);
                yAxisMesh.DrawSubset(0);

                generateAxis();

                _d3dLine.Draw(xAxis, Color.Black);
                _d3dLine.Draw(yAxis, Color.Black);
            }
        }
    }
}