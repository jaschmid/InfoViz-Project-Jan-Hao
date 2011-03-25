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
                public CustomVertex.PositionColored[] Vertices
                {
                    private set;
                    get;
                }
                public int[] FillIndices;
                public int[] EdgeIndices;

                public Material Material
                {
                    set;
                    get;
                }

                private float zoom;
                public float Zoom
                {
                    set
                    {
                        zoom = value;
                        BuildLines();
                    }
                    get
                    {
                        return zoom;
                    }
                }

                private Viewport viewport;
                public Viewport Viewport
                {
                    set
                    {
                        viewport = value;
                        BuildLines();
                    }
                    get
                    {
                        return viewport;
                    }
                }

                private Color[] colors;

                public Line(float zoom,Viewport vp, Color[] colors)
                {
                    this.zoom = zoom;
                    this.viewport = vp;
                    this.colors = colors;
                }

                private void BuildLines()
                {
                    if (data == null || data.Length < 2)
                    {
                        Vertices = null;
                        return;
                    }

                    this.Vertices = new CustomVertex.PositionColored[data.Length * 2];
                    this.PositionData = new Vector2[data.Length];

                    // start values
                    int index = 0;
                    Vector4 v = data[index+1]-data[index];
                    float angle = (float)System.Math.Atan2(v.Y, v.X);

                    this.Vertices[index * 2 + 0].X = (float)this.data[index].X + (float)Math.Cos(angle - Math.PI / 2) * data[index].Z / Zoom / Viewport.Width;
                    this.Vertices[index * 2 + 0].Y = (float)this.data[index].Y + (float)Math.Sin(angle - Math.PI / 2) * data[index].Z / Zoom / Viewport.Height;
                    this.Vertices[index * 2 + 0].Z = 0.5f;
                    this.Vertices[index * 2 + 0].Color = this.colors[index].ToArgb();

                    this.Vertices[index * 2 + 1].X = (float)this.data[index].X + (float)Math.Cos(angle + Math.PI / 2) * data[index].Z / Zoom / Viewport.Width;
                    this.Vertices[index * 2 + 1].Y = (float)this.data[index].Y + (float)Math.Sin(angle + Math.PI / 2) * data[index].Z / Zoom / Viewport.Height;
                    this.Vertices[index * 2 + 1].Z = 0.5f;
                    this.Vertices[index * 2 + 1].Color = this.colors[index].ToArgb();

                    this.PositionData[index].X = (float)this.data[index].X;
                    this.PositionData[index].Y = (float)this.data[index].Y;

                    //intermediate values
                    for (index = 1; index < data.Length - 1; ++index)
                    {
                        v = data[index + 1] - data[index - 1];
                        angle = (float)System.Math.Atan2(v.Y, v.X);

                        this.Vertices[index * 2 + 0].X = (float)this.data[index].X + (float)Math.Cos(angle - Math.PI / 2) * data[index].Z / Zoom / Viewport.Width;
                        this.Vertices[index * 2 + 0].Y = (float)this.data[index].Y + (float)Math.Sin(angle - Math.PI / 2) * data[index].Z / Zoom / Viewport.Height;
                        this.Vertices[index * 2 + 0].Z = 0.5f;
                        this.Vertices[index * 2 + 0].Color = this.colors[index].ToArgb();

                        this.Vertices[index * 2 + 1].X = (float)this.data[index].X + (float)Math.Cos(angle + Math.PI / 2) * data[index].Z / Zoom / Viewport.Width;
                        this.Vertices[index * 2 + 1].Y = (float)this.data[index].Y + (float)Math.Sin(angle + Math.PI / 2) * data[index].Z / Zoom / Viewport.Height;
                        this.Vertices[index * 2 + 1].Z = 0.5f;
                        this.Vertices[index * 2 + 1].Color = this.colors[index].ToArgb();

                        this.PositionData[index].X = (float)this.data[index].X;
                        this.PositionData[index].Y = (float)this.data[index].Y;
                    }


                    // final values
                    v = data[index] - data[index - 1];
                    angle = (float)System.Math.Atan2(v.Y, v.X);

                    this.Vertices[index * 2 + 0].X = (float)this.data[index].X + (float)Math.Cos(angle - Math.PI / 2) * data[index].Z / Zoom / Viewport.Width;
                    this.Vertices[index * 2 + 0].Y = (float)this.data[index].Y + (float)Math.Sin(angle - Math.PI / 2) * data[index].Z / Zoom / Viewport.Height;
                    this.Vertices[index * 2 + 0].Z = 0.5f;
                    this.Vertices[index * 2 + 0].Color = this.colors[index].ToArgb();

                    this.Vertices[index * 2 + 1].X = (float)this.data[index].X + (float)Math.Cos(angle + Math.PI / 2) * data[index].Z / Zoom / Viewport.Width;
                    this.Vertices[index * 2 + 1].Y = (float)this.data[index].Y + (float)Math.Sin(angle + Math.PI / 2) * data[index].Z / Zoom / Viewport.Height;
                    this.Vertices[index * 2 + 1].Z = 0.5f;
                    this.Vertices[index * 2 + 1].Color = this.colors[index].ToArgb();

                    this.PositionData[index].X = (float)this.data[index].X;
                    this.PositionData[index].Y = (float)this.data[index].Y;

                    this.FillIndices = new int[this.Vertices.Length];
                    for (int i = 0; i < this.Vertices.Length; ++i)
                        this.FillIndices[i] = i;

                    this.EdgeIndices = new int[this.Vertices.Length + 1];
                    for (int i = 0; i < this.Vertices.Length / 2; ++i)
                    {
                        this.EdgeIndices[i] = 2 * i;
                        this.EdgeIndices[i + this.Vertices.Length / 2] = this.Vertices.Length - 1 - 2 * i;
                    }
                    this.EdgeIndices[this.Vertices.Length] = this.EdgeIndices[0];

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

                        if (!float.IsNaN(p1Dist) && !float.IsNaN(p2Dist) && !float.IsInfinity(p1Dist) && !float.IsInfinity(p2Dist))
                        {
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
                    }

                    return length;
                }

                public float Length()
                {
                    float result = 0.0f;
                    for (int i = 0; i < this.PositionData.Length - 1; ++i)
                    {
                        float d= (this.PositionData[i + 1] - this.PositionData[i]).Length();
                        if (!float.IsNaN(d) && !float.IsInfinity(d))
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
                    if (float.IsNaN(cx) || float.IsNaN(cy) || float.IsInfinity(cx) || float.IsInfinity(cy))
                        return float.NaN;

                    float minDistance = float.MaxValue;
                    for (int i = 0; i < this.PositionData.Length - 1; ++i)
                    {
                        float thisDistance = 0.0f;
                        float ax = this.PositionData[i].X;
                        float ay = this.PositionData[i].Y;
                        float bx = this.PositionData[i+1].X;
                        float by = this.PositionData[i+1].Y;

                        if (float.IsNaN(ax) || float.IsNaN(ay) || float.IsInfinity(ax) || float.IsInfinity(ay))
                            continue;
                        if (float.IsNaN(bx) || float.IsNaN(by) || float.IsInfinity(bx) || float.IsInfinity(by))
                            continue;

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
                    if (minDistance == float.MaxValue)
                        return float.NaN;
                    return minDistance;
                }
            }

            private ColorMap colorMap;
            private Color[] timeColors;
            override public ColorMap ColorMap 
            {
                get { return colorMap; }
                set
                {
                    this.colorMap =value;
                    if(this.Input != null)
                        this.timeColors = value.GetColors(this.Input.GetDataCube().GetAxisLength(this.TimeSourceAxis));
                }
            }

            // List containing line vector arrays
            private List<Line> lines;

            // D3D Device
            private Device device;

            // old data
            private Microsoft.DirectX.Direct3D.Line _d3dLine;

            // Settings
            private Axis lineSourceAxis;
            public Axis LineSourceAxis
            {
                get
                {
                    return lineSourceAxis;
                }
                set
                {
                    bLinesChanged = true;
                    lineSourceAxis = value;
                }
            }
            private Axis timeSourceAxis;
            public Axis TimeSourceAxis
            {
                get
                {
                    return timeSourceAxis;
                }
                set
                {
                    bLinesChanged = true;
                    timeSourceAxis = value;
                }
            }
            private Axis dataSourceAxis;
            public Axis DataSourceAxis
            {
                get
                {
                    return dataSourceAxis;
                }
                set
                {
                    bLinesChanged = true;
                    dataSourceAxis = value;
                }
            }
            private int dataLineXIndex;
            public int DataLineXIndex
            {
                get
                {
                    return dataLineXIndex;
                }
                set
                {
                    bLinesChanged = true;
                    dataLineXIndex = value;
                }
            }
            private int dataLineYIndex;
            public int DataLineYIndex
            {
                get
                {
                    return dataLineYIndex;
                }
                set
                {
                    bLinesChanged = true;
                    dataLineYIndex = value;
                }
            }
            private float dataLineThicknessScale;
            public float DataLineThicknessScale
            {
                get
                {
                    return dataLineThicknessScale;
                }
                set
                {
                    bLinesChanged = true;
                    dataLineThicknessScale = value;
                }
            }
            private int dataLineThicknessIndex;
            public int DataLineThicknessIndex
            {
                get
                {
                    return dataLineThicknessIndex;
                }
                set
                {
                    bLinesChanged = true;
                    dataLineThicknessIndex = value;
                }
            }
            private Color selectedIndexColor;
            public Color SelectedIndexColor
            {
                get
                {
                    return selectedIndexColor;
                }
                set
                {
                    bLinesChanged = true;
                    selectedIndexColor = value;
                }
            }
            private int yAxisSpacing;
            public int YAxisSpacing
            {
                get
                {
                    return yAxisSpacing;
                }
                set
                {
                    bLinesChanged = true;
                    yAxisSpacing = value;
                }
            }
            private int xAxisSpacing;
            public int XAxisSpacing
            {
                get
                {
                    return xAxisSpacing;
                }
                set
                {
                    bLinesChanged = true;
                    xAxisSpacing = value;
                }
            }

            public Vector2 Translation
            {
                get;
                set;
            }
            public float[] DataMin
            {
                get;
                set;
            }
            public float[] DataMax
            {
                get;
                set;
            }

            private float zoom;
            public float Zoom
            {
                get { return zoom; }
                set
                {
                    zoom = value;
                    foreach (Line l in this.lines)
                        l.Zoom = value;
                }
            }
            
            private List<int> selectedIndexes;

            public void SetSelectedIndexes(List<int> indexes)
            {
                selectedIndexes = indexes;
                Invalidate();
            }

            public LineLayer()
            {
                this.lines = new List<Line>();
                this.Zoom = 1.0f;
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
                //adjust for viewport

                x = x - (int)this.viewport.X;
                x = Math.Max(x, 0);

                y = (this.viewport.Height - (y - this.viewport.Y));
                y = Math.Max(y, 0);

                System.Diagnostics.Debug.Print("X/Y:" + x.ToString() + "/" + y.ToString());

                float fx = ((float)(x) / (float)(viewport.Width) - this.Translation.X / viewport.Width ) / this.Zoom;
                float fy = ((float)(y) / (float)(viewport.Height) - this.Translation.Y / viewport.Height) / this.Zoom;

                float minDistance = float.MaxValue;
                int minIndex = -1;
                for (int i = 0; i < this.lines.Count; ++i)
                {
                    float this_distance = this.lines[i].GetDistanceToPoint(fx, fy);
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

            private Viewport viewport;
            private Viewport old_viewport;

            bool bLinesChanged = false;

            protected override void Render(Device device)
            {

                if (!_inited) Initialize(device);
                if (bLinesChanged || this.old_viewport.GetHashCode() != device.Viewport.GetHashCode())
                {
                    this.old_viewport = device.Viewport;

                    this.viewport = this.old_viewport;
                    this.viewport.Width -= (int)this.XAxisSpacing;
                    this.viewport.Height -= (int)this.YAxisSpacing;
                    this.viewport.X += (int)this.XAxisSpacing;

                    CreateLines();
                    bLinesChanged = false;
                }
                
                this.device.RenderState.CullMode = Cull.None;

                Material mat = new Material();
                Material edge = new Material();
                edge.Emissive = Color.FromArgb(0x70,Color.Black);
                edge.Diffuse = Color.FromArgb(0x70, Color.Black);
                edge.Ambient = Color.FromArgb(0x70, Color.Black);

                int w = Control.AbsoluteSize.Width;
                int h = Control.AbsoluteSize.Height;

                Viewport oldView = this.device.Viewport;


                this.device.Viewport = this.viewport;
                this.device.VertexFormat = CustomVertex.PositionColored.Format;
                this.device.Transform.Projection = Matrix.OrthoLH(1.0f, 1.0f, 0.01f, 100.0f);
                this.device.Transform.View = Matrix.LookAtLH(new Vector3(1.0f / 2, 1.0f / 2, -10.0f),
                                                                new Vector3(1.0f / 2, 1.0f / 2, 0.0f),
                                                                new Vector3(0.0f, 1.0f, 0.0f));
                this.device.Transform.World = Matrix.Scaling(Zoom,Zoom,1.0f) * Matrix.Translation(this.Translation.X/this.viewport.Width,this.Translation.Y/this.viewport.Height,0.0f);
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
                        this.device.RenderState.DiffuseMaterialSource = ColorSource.Color1;
                        this.device.RenderState.EmissiveMaterialSource = ColorSource.Color1;
                    }
                    else
                    {
                        this.device.RenderState.DiffuseMaterialSource = ColorSource.Material;
                        this.device.RenderState.EmissiveMaterialSource = ColorSource.Material;
                        mat.Emissive = Color.Black;
                        mat.Diffuse = Color.Black;
                        mat.Ambient = Color.Black;
                    }
                    this.device.Material = mat;

                    // Draws the apropriate line in the color specified by the colormap
                    this.device.DrawIndexedUserPrimitives(PrimitiveType.TriangleStrip, 0, l.Vertices.Length, l.Vertices.Length - 2, l.FillIndices, false, l.Vertices);
                    this.device.RenderState.DiffuseMaterialSource = ColorSource.Material;
                    this.device.RenderState.EmissiveMaterialSource = ColorSource.Material;
                    this.device.Material = edge;
                    this.device.DrawIndexedUserPrimitives(PrimitiveType.LineStrip, 0, l.Vertices.Length, l.Vertices.Length, l.EdgeIndices, false, l.Vertices);
                }

                for (int i = 0; i < this.selectedIndexes.Count;++i )
                {
                    Line l = this.lines[this.selectedIndexes[i]];
                    mat.Emissive = this.SelectedIndexColor;
                    mat.Diffuse = this.SelectedIndexColor;
                    mat.Ambient = this.SelectedIndexColor;
                    this.device.Material = mat;
                    this.device.DrawIndexedUserPrimitives(PrimitiveType.TriangleStrip, 0, l.Vertices.Length, l.Vertices.Length - 2, l.FillIndices, false, l.Vertices);
                    this.device.Material = edge;
                    this.device.DrawIndexedUserPrimitives(PrimitiveType.LineStrip, 0, l.Vertices.Length, l.Vertices.Length, l.EdgeIndices, false, l.Vertices);
                }


                this.device.Viewport = oldView;
            }

            public void CreateLines()
            {
                // Check for problems with the input
                if (Input == null || Input.GetDataCube() == null || Input.GetDataCube().DataArray == null) return;

                // Clear the old lines
                lines.Clear();

                // Get the data
                float[, ,] data = Input.GetDataCube().DataArray;
                
                int[] loc = new int[3];

                // Get the maximum and minimum for all data fields

                float[] maxData = this.DataMax,
                        minData = this.DataMin;

                if (this.DataMin == null || this.DataMax == null)
                {
                    maxData = new float[data.GetLength((int)this.DataSourceAxis)];
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
                                if (float.IsNaN(val) || float.IsInfinity(val))
                                    continue;
                                if (val > fMaxData)
                                    fMaxData = val;
                                if (val < fMinData)
                                    fMinData = val;
                            }
                        }
                        maxData[iData] = fMaxData;
                        minData[iData] = fMinData;
                    }

                    this.DataMin = minData;
                    this.DataMax = maxData;
                }

                
                // Create the line vector arrays
                for (int iLine = 0; iLine < data.GetLength((int)this.LineSourceAxis); iLine++)
                {
                    loc[(int)this.LineSourceAxis] = iLine;
                    Line l = new Line(this.Zoom,this.viewport,this.timeColors);
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


                        if (this.DataLineYIndex >= 0)
                        {
                            loc[(int)this.DataSourceAxis] = this.DataLineYIndex;
                            float val = (float)data.GetValue(loc);
                            lData[iTime].Y = (val - minData[this.DataLineYIndex]) / (maxData[this.DataLineYIndex] - minData[this.DataLineYIndex]);
                        }
                        else lData[iTime].Y = 0.5f;

                        float realScale = DataLineThicknessScale;

                        if (this.DataLineThicknessIndex >= 0)
                        {
                            loc[(int)this.DataSourceAxis] = this.DataLineThicknessIndex;
                            float val = (float)data.GetValue(loc);
                            if (float.IsNaN(val) || float.IsInfinity(val))
                                lData[iTime].Z = 0.5f * realScale;
                            else
                            {
                                lData[iTime].Z = (val - minData[this.DataLineThicknessIndex]) / (maxData[this.DataLineThicknessIndex] - minData[this.DataLineThicknessIndex]);
                                lData[iTime].Z *= realScale;
                            }
                        }
                        else lData[iTime].Z = 0.5f*realScale;

                    }

                    l.Data = lData;
                    lines.Add(l);
                }

                for (int i = 0; i < this.selectedIndexes.Count; ++i)
                    if (this.selectedIndexes[i] > this.lines.Count)
                    {
                        this.selectedIndexes.RemoveAt(i);
                        i--;
                    }
            }
        }
    }
}
