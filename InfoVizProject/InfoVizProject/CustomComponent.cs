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

            virtual public ColorMap ColorMap { get; set; }
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

        public System.Windows.Forms.ContextMenu UserMenu
        {
            get;
            set;
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
            set { 
                this.lineLayer.DataLineXIndex = value;

                if (DataLabels != null)
                {
                    if(value == -1)
                        this.axisLayer.XAxisLabel = "None";
                    else
                        this.axisLayer.XAxisLabel = this.DataLabels[value];
                }
                else
                    this.axisLayer.XAxisLabel = "X Axis";
            }
        }
        public float DataLineThicknessScale
        {
            get { return this.lineLayer.DataLineThicknessScale; }
            set { this.lineLayer.DataLineThicknessScale = value; }
        }
        public int DataLineYIndex
        {
            get { return this.lineLayer.DataLineYIndex; }
            set
            {
                this.lineLayer.DataLineYIndex = value;

                if (DataLabels != null)
                {
                    if (value == -1)
                        this.axisLayer.YAxisLabel = "None";
                    else
                        this.axisLayer.YAxisLabel = this.DataLabels[value];
                }
                else
                    this.axisLayer.YAxisLabel = "Y Axis";
            }
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
            this.interactionLayer.SelectedItems = indexes;
        }
        public void SetAxisSize(int XAxisSize, int YAxisSize)
        {
            this.lineLayer.XAxisSpacing = XAxisSize;
            this.lineLayer.YAxisSpacing = YAxisSize;
        }
        public bool AllowCustomizeXAxis { set { this.interactionLayer.AllowCustomizeXAxis = value; } get { return this.interactionLayer.AllowCustomizeXAxis; } }
        public bool AllowCustomizeYAxis { set { this.interactionLayer.AllowCustomizeYAxis = value; } get { return this.interactionLayer.AllowCustomizeYAxis; } }
        public bool AllowCustomizeThickness { set { this.interactionLayer.AllowCustomizeThickness = value; } get { return this.interactionLayer.AllowCustomizeThickness; } }

        private List<string> dataLabels;
        public List<string> DataLabels
        {
            get
            {
                return dataLabels;
            }
            set
            {
                dataLabels = value;
                if (value != null && this.lineLayer.DataLineYIndex != -1)
                    this.axisLayer.YAxisLabel = this.DataLabels[this.lineLayer.DataLineYIndex];
                if (value != null && this.lineLayer.DataLineXIndex != -1)
                    this.axisLayer.XAxisLabel = this.DataLabels[this.lineLayer.DataLineXIndex];
            }
        }

        public List<int> GetClosestLine(int sourceIndex)
        {
            return lineLayer.GetClosestLinesToLine(sourceIndex);
        }

        public class SelectionUpdatedEventArgs : System.EventArgs
        {
            public List<int> SelectedItems;
        };

        public delegate void SelectionUpdatedEventHandler(object sender, SelectionUpdatedEventArgs e);

        public event SelectionUpdatedEventHandler SelectionChanged;

        internal System.Windows.Forms.Control iRenderTarget
        {
            get
            {
                return this.RenderTarget;
            }
            set
            {
                this.RenderTarget = value;
            }
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
            this.lineLayer.XAxisSpacing = 30;
            this.lineLayer.YAxisSpacing = 30;
            layers.Add(this.lineLayer);


            this.axisLayer = new AxisLayer();
            this.axisLayer.Control.SetOuterMargins(10, GavControl.DistanceType.Absolute);
            this.axisLayer.XAxisSpacing = 30.0f;
            this.axisLayer.YAxisSpacing = 30.0f;
            layers.Add(this.axisLayer);


            this.interactionLayer = new InteractionLayer(this.lineLayer,this);
            this.interactionLayer.Control.SetOuterMargins(10, GavControl.DistanceType.Absolute);
            this.interactionLayer.SelectionChanged += new SelectionUpdatedEventHandler(InternalSelectionUpdatedEvent);
            layers.Add(this.interactionLayer);
            /*
            TextLayer textLayer = new TextLayer();
            textLayer.Control.SetOuterMargins(10, GavControl.DistanceType.Absolute);
            layers.Add(textLayer);
            */

            List<int> selected = new List<int>();
            selected.Add(201);
            selected.Add(202);
            selected.Add(203);

            // default settings
            this.TimeSourceAxis = Axis.Z;
            this.LineSourceAxis = Axis.Y;
            this.DataSourceAxis = Axis.X;
            this.DataLineXIndex = 0;
            this.DataLineYIndex = 1;
            this.DataLineThicknessIndex = -1;
            this.DataLineThicknessScale = 1.5f;
            this.ColorMap = null;
            this.SelectedIndexColor = Color.Red;
            this.AllowCustomizeXAxis = true;
            this.AllowCustomizeYAxis = true;
            this.AllowCustomizeThickness = true;

            this.SetSelectedIndexes(selected);

            foreach (Layer layer in layers)
            {
                AddLayer(layer);
            }
        }

        private void InternalSelectionUpdatedEvent(object sender, SelectionUpdatedEventArgs e)
        {
            this.SelectionChanged(this,e);
            //this.Invalidate();
        }

        private LineLayer lineLayer;
        private AxisLayer axisLayer;
        private InteractionLayer interactionLayer;
    }


}

