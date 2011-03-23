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

                public Material Material
                {
                    set;
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

                private float GetLengthCloserThan(float fMaxDistance, Vector2[] line)
                {
                    float length = 0.0f;
                    for (int i = 0; i < line.Length-1; ++i)
                    {
                        float p1Dist = GetDistanceToPoint(line[i].X, line[i].Y);
                        float p2Dist = GetDistanceToPoint(line[i + 1].X, line[i + 1].Y);
                        float short_dist = Math.Min(p1Dist, p2Dist);
                        float long_dist = Math.Max(p1Dist, p2Dist);

                        if (long_dist < fMaxDistance)
                        {
                            //entire length is closer
                            length += (float)(line[i + 1] - line[i]).Length();
                        }
                        else if (short_dist > fMaxDistance)
                            continue; //none of it is closer
                        else
                        {
                            float pCloser = (fMaxDistance - short_dist) / (long_dist - short_dist);
                            length += pCloser * (float)(line[i + 1] - line[i]).Length();
                        }
                    }

                    return length;
                }

                public float Length()
                {
                    float result = 0.0f;
                    for (int i = 0; i < this.PositionData.Length - 1; ++i)
                    {
                        float d= (this.PositionData[i + 1] - this.PositionData[i]).Length();
                        if (!float.IsNaN(d))
                            result += d;
                    }
                    return result;
                }

                private const float measure_factor = 80.0f;

                public float GetInterLineMeasurement(Line l2)
                {
                    float cutoff_distance = (this.Length() + l2.Length()) / measure_factor;
                    return (l2.GetLengthCloserThan(cutoff_distance, this.PositionData) + this.GetLengthCloserThan(cutoff_distance, l2.PositionData)) / cutoff_distance * measure_factor;
                }

                public float GetDistanceToPoint(float cx, float cy)
                {
                    float minDistance = float.MaxValue;
                    for (int i = 0; i < this.PositionData.Length - 1; ++i)
                    {
                        float thisDistance = 0.0f;
                        float ax = this.PositionData[i].X;
                        float ay = this.PositionData[i].Y;
                        float bx = this.PositionData[i+1].X;
                        float by = this.PositionData[i+1].Y;

                        float r_numerator = (cx - ax) * (bx - ax) + (cy - ay) * (by - ay);
                        float r_denomenator = (bx - ax) * (bx - ax) + (by - ay) * (by - ay);
                        float r = r_numerator / r_denomenator;
                        //
                        float px = ax + r * (bx - ax);
                        float py = ay + r * (by - ay);
                        //     
                        float s = ((ay - cy) * (bx - ax) - (ax - cx) * (by - ay)) / r_denomenator;

                        float distanceLine = Math.Abs(s) * (float)Math.Sqrt((double)r_denomenator);

                        //
                        // (xx,yy) is the point on the lineSegment closest to (cx,cy)
                        //
                        double xx = px;
                        double yy = py;

                        if ((r >= 0) && (r <= 1))
                        {
                            thisDistance = distanceLine;
                        }
                        else
                        {

                            double dist1 = (cx - ax) * (cx - ax) + (cy - ay) * (cy - ay);
                            double dist2 = (cx - bx) * (cx - bx) + (cy - by) * (cy - by);
                            if (dist1 < dist2)
                            {
                                xx = ax;
                                yy = ay;
                                thisDistance = (float)Math.Sqrt((double)dist1);
                            }
                            else
                            {
                                xx = bx;
                                yy = by;
                                thisDistance = (float)Math.Sqrt((double)dist2);
                            }


                        }


                        if (thisDistance < minDistance)
                            minDistance = thisDistance;
                    }
                    return minDistance;
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
            public float DataLineThicknessScale
            {
                get;
                set;
            }
            public int DataLineThicknessIndex
            {
                get;
                set;
            }
            public Color SelectedIndexColor
            {
                get;
                set;
            }

            public float YAxisSpacing
            {
                get;
                set;
            }
            public float XAxisSpacing
            {
                get;
                set;
            }
            
            private List<int> selectedIndexes;

            public void SetSelectedIndexes(List<int> indexes)
            {
                selectedIndexes = indexes;
                Invalidate();
            }

            public LineLayer()
            {
                this.TimeSourceAxis = Axis.Z;
                this.LineSourceAxis = Axis.Y;
                this.DataSourceAxis = Axis.X;
                this.DataLineXIndex = 0;
                this.DataLineYIndex = 1;
                this.DataLineThicknessIndex = -1;
                this.DataLineThicknessScale = 1.5f;
                this.ColorMap = null;
                this.SelectedIndexColor = Color.Black;
                this.lines = new List<Line>();
            }

            public List<int> GetClosestLinesToLine(int index)
            {
                List<int> results = new List<int>();
                Line refLine = this.lines[index];
                SortedDictionary<float, List<int>> dict = new SortedDictionary<float, List<int>>();
                for(int i = 0; i < this.lines.Count ; ++i)
                    if (i != index)
                    {
                        float dist = this.lines[i].GetInterLineMeasurement(refLine);
                        if (dist != 0.0f && !float.IsNaN(dist))
                        {
                            List<int> value;
                            if (dict.TryGetValue(dist, out value))
                            {
                                value.Add(i);
                            }
                            else
                            {
                                value = new List<int>();
                                value.Add(i);
                                dict.Add(dist, value);
                            }
                        }
                    }

                foreach (KeyValuePair<float, List<int>> p in dict)
                    foreach(int i in p.Value)
                        results.Add(i);

                results.Reverse();

                return results;
            }

            public int GetClosestLine(int x, int y)
            {
                y = (this.Control.ComponentSize.Height + this.Control.AbsoluteSize.Height)/2 - y;
                x = (this.Control.AbsoluteSize.Height - this.Control.ComponentSize.Height) / 2 + x;
                float minDistance = float.MaxValue;
                int minIndex = -1;
                for (int i = 0; i < this.lines.Count; ++i)
                {
                    float this_distance = this.lines[i].GetDistanceToPoint((float)x, (float)y);
                    if (this_distance < minDistance)
                    {
                        minIndex = i;
                        minDistance = this_distance;
                    }
                }
                return minIndex;
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
                
                this.device.RenderState.CullMode = Cull.None;

                Material mat = new Material();

                int w = Control.AbsoluteSize.Width;
                int h = Control.AbsoluteSize.Height;

                this.device.VertexFormat = CustomVertex.PositionOnly.Format;
                this.device.Transform.Projection = Matrix.OrthoLH(w, h, 0.01f, 100.0f);
                this.device.Transform.View = Matrix.LookAtLH(new Vector3(w/2, h/2, -10.0f),
                                                                new Vector3(w/2, h/2, 0.0f),
                                                                new Vector3(0.0f, 1.0f, 0.0f));
                this.device.Transform.World = Matrix.Identity;
                this.device.RenderState.DiffuseMaterialSource = ColorSource.Material;
                this.device.RenderState.EmissiveMaterialSource = ColorSource.Material;
                this.device.RenderState.Lighting = true;
                
                for(int i = 0;i<this.lines.Count;++i)
                {
                    Line l = this.lines[i];

                    if (this.selectedIndexes!= null && this.selectedIndexes.Contains(i))
                    {
                        continue;
                    }
                    else if (this.ColorMap != null)
                    {
                        mat.Emissive = this.ColorMap.GetColor(i);
                        mat.Diffuse = this.ColorMap.GetColor(i);
                        mat.Ambient = this.ColorMap.GetColor(i);
                    }
                    else
                    {
                        mat.Emissive = Color.Black;
                        mat.Diffuse = Color.Black;
                        mat.Ambient = Color.Black;
                    }
                    this.device.Material = mat;

                    // Draws the apropriate line in the color specified by the colormap
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, l.Vertices.Length - 2, l.Vertices);
                }

                for (int i = 0; i < this.selectedIndexes.Count;++i )
                {
                    Line l = this.lines[this.selectedIndexes[i]];
                    mat.Emissive = this.SelectedIndexColor;
                    mat.Diffuse = this.SelectedIndexColor;
                    mat.Ambient = this.SelectedIndexColor;
                    this.device.Material = mat;
                    this.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, l.Vertices.Length - 2, l.Vertices);
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

                // Get the maximum and minimum for all data fields
                float[] maxData = new float[data.GetLength((int)this.DataSourceAxis)],
                    minData = new float[data.GetLength((int)this.DataSourceAxis)];
                for (int iData = 0; iData < data.GetLength((int)this.DataSourceAxis); ++iData)
                {
                    loc[(int)this.DataSourceAxis] = iData;
                    float fMaxData = float.MinValue, fMinData = float.MaxValue;
                    for (int iLine = 0; iLine < data.GetLength((int)this.LineSourceAxis); iLine++)
                    {
                        loc[(int)this.LineSourceAxis] = iLine;
                        for (int iTime = 0; iTime < data.GetLength((int)this.TimeSourceAxis); iTime++)
                        {
                            loc[(int)this.TimeSourceAxis] = iTime;
                            float val = (float)data.GetValue(loc);
                            if (val > fMaxData)
                                fMaxData = val;
                            if (val < fMinData)
                                fMinData = val;
                        }
                    }
                    maxData[iData] = fMaxData;
                    minData[iData] = fMinData;
                }
                
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
                            float val = (float)data.GetValue(loc);
                            lData[iTime].X = (val - minData[this.DataLineXIndex]) / (maxData[this.DataLineXIndex] - minData[this.DataLineXIndex]);
                        }
                        else lData[iTime].X = 0.5f;

                        lData[iTime].X *= Control.AbsoluteSize.Width;
                        lData[iTime].X += XAxisSpacing;

                        if (this.DataLineYIndex >= 0)
                        {
                            loc[(int)this.DataSourceAxis] = this.DataLineYIndex;
                            float val = (float)data.GetValue(loc);
                            lData[iTime].Y = (val - minData[this.DataLineYIndex]) / (maxData[this.DataLineYIndex] - minData[this.DataLineYIndex]);
                        }
                        else lData[iTime].Y = 0.5f;

                        lData[iTime].Y *= Control.AbsoluteSize.Height - 2*YAxisSpacing;
                        lData[iTime].Y += YAxisSpacing;

                        if (this.DataLineThicknessIndex >= 0)
                        {
                            loc[(int)this.DataSourceAxis] = this.DataLineThicknessIndex;
                            float val = (float)data.GetValue(loc);
                            lData[iTime].Z = (val - minData[this.DataLineThicknessIndex]) / (maxData[this.DataLineThicknessIndex] - minData[this.DataLineThicknessIndex]);
                            lData[iTime].Z *= this.DataLineThicknessScale;
                        }
                        else lData[iTime].Z = DataLineThicknessScale;

                    }

                    l.Data = lData;
                    lines.Add(l);
                }
            }
        }
    }
}
