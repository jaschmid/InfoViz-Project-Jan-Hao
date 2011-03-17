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
        class LineLayer : Layer
        {
            
            private class Line
            {
                
                private Vector4[] data;
                public CustomVertex.PositionOnly[] Vertices
                {
                    private set;
                    get;
                }

                private void BuildLines()
                {
                    if (data == null || data.Length < 2)
                    {
                        Vertices = null;
                        return;
                    }

                    this.Vertices = new CustomVertex.PositionOnly[data.Length * 2];
                    this.PositionData = new Vector2[data.Length];

                    // start values
                    int index = 0;
                    Vector4 v = data[index+1]-data[index];
                    float angle = (float)System.Math.Atan2(v.Y, v.X);

                    this.Vertices[index * 2 + 0].X = (float)this.data[index].X + (float)Math.Cos(angle - Math.PI / 2) * data[index].Z;
                    this.Vertices[index * 2 + 0].Y = (float)this.data[index].Y + (float)Math.Sin(angle - Math.PI / 2) * data[index].Z;
                    this.Vertices[index * 2 + 0].Z = 0.5f;

                    this.Vertices[index * 2 + 1].X = (float)this.data[index].X + (float)Math.Cos(angle + Math.PI / 2) * data[index].Z;
                    this.Vertices[index * 2 + 1].Y = (float)this.data[index].Y + (float)Math.Sin(angle + Math.PI / 2) * data[index].Z;
                    this.Vertices[index * 2 + 1].Z = 0.5f;

                    this.PositionData[index].X = (float)this.data[index].X;
                    this.PositionData[index].Y = (float)this.data[index].Y;

                    //intermediate values
                    for (index = 1; index < data.Length - 1; ++index)
                    {
                        v = data[index + 1] - data[index - 1];
                        angle = (float)System.Math.Atan2(v.Y, v.X);

                        this.Vertices[index * 2 + 0].X = (float)this.data[index].X + (float)Math.Cos(angle - Math.PI / 2) * data[index].Z;
                        this.Vertices[index * 2 + 0].Y = (float)this.data[index].Y + (float)Math.Sin(angle - Math.PI / 2) * data[index].Z;
                        this.Vertices[index * 2 + 0].Z = 0.5f;

                        this.Vertices[index * 2 + 1].X = (float)this.data[index].X + (float)Math.Cos(angle + Math.PI / 2) * data[index].Z;
                        this.Vertices[index * 2 + 1].Y = (float)this.data[index].Y + (float)Math.Sin(angle + Math.PI / 2) * data[index].Z;
                        this.Vertices[index * 2 + 1].Z = 0.5f;

                        this.PositionData[index].X = (float)this.data[index].X;
                        this.PositionData[index].Y = (float)this.data[index].Y;
                    }


                    // final values
                    v = data[index] - data[index - 1];
                    angle = (float)System.Math.Atan2(v.Y, v.X);

                    this.Vertices[index * 2 + 0].X = (float)this.data[index].X + (float)Math.Cos(angle - Math.PI / 2) * data[index].Z;
                    this.Vertices[index * 2 + 0].Y = (float)this.data[index].Y + (float)Math.Sin(angle - Math.PI / 2) * data[index].Z;
                    this.Vertices[index * 2 + 0].Z = 0.5f;

                    this.Vertices[index * 2 + 1].X = (float)this.data[index].X + (float)Math.Cos(angle + Math.PI / 2) * data[index].Z;
                    this.Vertices[index * 2 + 1].Y = (float)this.data[index].Y + (float)Math.Sin(angle + Math.PI / 2) * data[index].Z;
                    this.Vertices[index * 2 + 1].Z = 0.5f;

                    this.PositionData[index].X = (float)this.data[index].X;
                    this.PositionData[index].Y = (float)this.data[index].Y;
                }

                public Vector2[] PositionData
                {
                    get;
                    set;
                }

                public Vector4[] Data
                {
                    set 
                    {
                        data = value;
                        BuildLines();
                    }
                }
            }

            // List containing line vector arrays
            private List<Line> lines;
            // D3D Device
            private Device device;

            // old data
            private Microsoft.DirectX.Direct3D.Line _d3dLine;

            // Settings
            public Axis LineSourceAxis
            {
                get;
                set;
            }
            public Axis TimeSourceAxis
            {
                get;
                set;
            }
            public Axis DataSourceAxis
            {
                get;
                set;
            }
            public int DataLineXIndex
            {
                get;
                set;
            }
            public int DataLineYIndex
            {
                get;
                set;
            }
            public int DataLineThicknessIndex
            {
                get;
                set;
            }
            public int DataLineColorIndex
            {
                get;
                set;
            }

            public LineLayer()
            {
                this.TimeSourceAxis = Axis.Z;
                this.LineSourceAxis = Axis.Y;
                this.DataSourceAxis = Axis.X;
                this.DataLineXIndex = 0;
                this.DataLineYIndex = 1;
                this.DataLineThicknessIndex = -1;
                this.DataLineColorIndex = -1;
                this.lines = new List<Line>();
            }

            protected void Initialize(Device device)
            {
                this.device = device;
                _d3dLine = new Microsoft.DirectX.Direct3D.Line(this.device);
                this._inited = true;
            }

            protected override void Render(Device device)
            {
                if (!_inited) Initialize(device);
                CreateLines();
                
                //this.device.RenderState.CullMode = Cull.None;

                Material black = new Material();
                black.Emissive = Color.Black;
                black.Diffuse = Color.Black;
                black.Ambient = Color.Black;
                this.device.VertexFormat = CustomVertex.PositionOnly.Format;
                this.device.Transform.Projection = Matrix.OrthoLH(Control.AbsoluteSize.Width, Control.AbsoluteSize.Height, 0.01f, 100.0f);
                this.device.Transform.View = Matrix.LookAtLH(new Vector3(Control.AbsoluteSize.Width / 2, Control.AbsoluteSize.Height / 2, -10.0f),
                                                                new Vector3(Control.AbsoluteSize.Width / 2, Control.AbsoluteSize.Height / 2, 0.0f),
                                                                new Vector3(0.0f, 1.0f, 0.0f));
                this.device.Transform.World = Matrix.Identity;
                this.device.Material = black;
                this.device.RenderState.DiffuseMaterialSource = ColorSource.Material;
                this.device.RenderState.EmissiveMaterialSource = ColorSource.Material;
                this.device.RenderState.Lighting = true;
                
                foreach(Line l in this.lines)
                {
                    // Draws the apropriate line in the color specified by the colormap
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, l.Vertices.Length - 2, l.Vertices);
                    Vector4 v = new Vector4(l.Vertices[0].Position.X,l.Vertices[0].Position.Y,l.Vertices[0].Position.Z,1.0f);
                    v.Transform(this.device.Transform.World);
                    v.Transform(this.device.Transform.View);
                    v.Transform(this.device.Transform.Projection);
                }
            }

            private void CreateLines()
            {
                // Check for problems with the input
                if (Input == null || Input.GetDataCube() == null || Input.GetDataCube().DataArray == null) return;

                // Clear the old lines
                lines.Clear();

                // Get the data
                float[, ,] data = Input.GetDataCube().DataArray;
                int[] loc = new int[3];

                // Get the maximum and minimum of all columns
                List<float> columnMaxList, columnMinList;
                Input.GetDataCube().GetAllColumnMaxMin(out columnMaxList, out columnMinList);

                // Calculate column spacing
                float columnSpacing = Control.AbsoluteSize.Width / ((float)data.GetLength(0) - 1);

                // Create the line vector arrays
                for (int iLine = 0; iLine < data.GetLength((int)this.LineSourceAxis); iLine++)
                {
                    loc[(int)this.LineSourceAxis] = iLine;
                    Line l = new Line();
                    Vector4[] lData = new Vector4[data.GetLength((int)this.TimeSourceAxis)];
                    for (int iTime = 0; iTime < data.GetLength((int)this.TimeSourceAxis); iTime++)
                    {
                        loc[(int)this.TimeSourceAxis] = iTime;
                        if (this.DataLineXIndex >= 0)
                        {
                            loc[(int)this.DataSourceAxis] = this.DataLineXIndex;
                            lData[iTime].X = ((float)data.GetValue(loc)-columnMinList[this.DataLineXIndex])/(columnMaxList[this.DataLineXIndex]-columnMinList[this.DataLineXIndex]);
                        }
                        else lData[iTime].X = 0.5f;

                        lData[iTime].X *= Control.AbsoluteSize.Width;

                        if (this.DataLineYIndex >= 0)
                        {
                            loc[(int)this.DataSourceAxis] = this.DataLineYIndex;
                            lData[iTime].Y = ((float)data.GetValue(loc) - columnMinList[this.DataLineYIndex]) / (columnMaxList[this.DataLineYIndex] - columnMinList[this.DataLineYIndex]);
                        }
                        else lData[iTime].Y = 0.5f;

                        lData[iTime].Y *= Control.AbsoluteSize.Height;

                        if (this.DataLineThicknessIndex >= 0)
                        {
                            loc[(int)this.DataSourceAxis] = this.DataLineThicknessIndex;
                            lData[iTime].Z = ((float)data.GetValue(loc) - columnMinList[this.DataLineThicknessIndex]) / (columnMaxList[this.DataLineThicknessIndex] - columnMinList[this.DataLineThicknessIndex]);
                        }
                        else lData[iTime].Z = 1.0f;

                    }

                    l.Data = lData;
                    lines.Add(l);
                }
            }
        }
    }
}
