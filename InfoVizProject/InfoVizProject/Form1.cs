using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Gav.Data;
using Gav.Graphics;
using Microsoft.DirectX;

using Gav.Components.MapLayers;
using Gav.Components;
using Gav.Management;
using Gav.Components.Internal;

namespace InfoVizProject
{
    public partial class Form1 : Form
    {
        /*private data and components*/

        private int choroplethMapSelectedIndex;
        private int choroplethMapCurrentYear = 1960;

        private int __mapSelectedIndex;
        private int __dataSelectedIndex = 0;
        private int mapSelectedIndex
        {
            get
            {
                return __mapSelectedIndex;
            }
            set
            {
                __mapSelectedIndex = value;
                this.stringIndexMapper.TryMapIndex(__mapSelectedIndex, out __dataSelectedIndex);
            }
        }
        private int dataSelectedIndex
        {
            get
            {
                return __dataSelectedIndex;
            }
            set
            {
                __dataSelectedIndex = value;
                this.stringIndexMapper.TryBackwardMapIndex(__dataSelectedIndex, out __mapSelectedIndex);
            }
        }


        private ExcelDataProvider excelDataProvider;
        private YearSliceDataTransformer yearSliceDataTransformer;
        private YearSliceDataTransformer unfilteredYearSliceDataTransformer;
        private LogDataTransformer logDataTransformer;
        private InterpolatingDataTransformer interpolatingDataTransformer;

        private ChoroplethMap choroplethMap;  // world map component
        private MapData mapData;
        private MapBorderLayer mapBorderLayer;
        private MapPolygonLayer mapPolygonLayer;
        private InteractiveColorLegend interactiveColorLegend;
        private GavToolTip gavToolTip;
        private StringIndexMapper stringIndexMapper;

        private ParallelCoordinatesPlot parallelCoordinatesPlot;

        private FilterDataTransformer dataFilterTransformer;

        private ContextMenu userMenu;

        private class TableLensEncapsulator
        {
            public TransposDataTransformer lensDataTransformer;
            public ColorMap colorMapForTableLens;
            public TableLens tablelens;

            private ExcelDataProvider excelDataProvider;
            private IDataCubeProvider data;

            private int selectedIndex;
            public int SelectedIndex
            {
                get
                {
                    return selectedIndex;
                }
                set
                {
                    selectedIndex = value;
                    this.lensDataTransformer.SelectedCountry = new List<int>() { selectedIndex };
                    tablelens.HeadersList = new List<string>() { excelDataProvider.RowIds[selectedIndex] };
                    this.lensDataTransformer.CommitChanges();
                    colorMapForTableLens.Invalidate();
                    this.tablelens.Invalidate();
                }
            }

            private int selectedindicator;
            public int SelectedIndicator
            {

                get
                {
                    return selectedindicator;
                }
                set
                {
                    selectedindicator = value;
                    this.lensDataTransformer.SelectedIndicator = selectedindicator;
                    this.lensDataTransformer.CommitChanges();
                    colorMapForTableLens.Invalidate();
                    this.tablelens.Invalidate();
                }
            }

            public TableLensEncapsulator(IDataCubeProvider data, ExcelDataProvider excelDataProvider)
            {
             
                this.excelDataProvider = excelDataProvider;
                this.data = data;

                // color map
                colorMapForTableLens = new ColorMap();
                colorMapForTableLens.AddColorMapPart(new LinearRgbColorMapPart(Color.Blue, Color.Red));

                // transpos
                List<int> selected = new List<int>();
                selected.Add(SelectedIndex);
                lensDataTransformer = new TransposDataTransformer();
                lensDataTransformer.Input = this.data;
                lensDataTransformer.SelectedCountry = selected;
                lensDataTransformer.SelectedIndicator = SelectedIndicator;
                lensDataTransformer.GetDataCube();

                // table lens
                tablelens = new TableLens();
                lensDataTransformer.SelectedCountry = new List<int>(){0};
                tablelens.Input = lensDataTransformer.GetDataCube();
                colorMapForTableLens.Input = tablelens.Input;

                tablelens.ColorMap = colorMapForTableLens;
                List<string> countrylist = new List<string>();
                countrylist.Add(excelDataProvider.RowIds[SelectedIndex]);
                tablelens.HeadersList = countrylist;

            }
        };

        private TableLensEncapsulator tableLensA;
        private TableLensEncapsulator tableLensB;

        private ViewManager viewManager;
        private CustomComponent component;
        private ColorMap colorMap;

        public System.EventHandler keydown;

        public Form1()
        {
            InitializeComponent();

            MenuItem addToA = new MenuItem("Set Selection As View A",
                delegate(object o, EventArgs e)
                {
                    this.setViewA();
                }
                );
            MenuItem addToB = new MenuItem("Set Selection As View B",
                delegate(object o, EventArgs e)
                {
                    this.setViewB();
                }
                );

            userMenu = new ContextMenu(new MenuItem[]{ addToA, addToB });


            InitializeData();
            InitializeDataTransformer();
            InitializeColor();
            InitializeStringIndexMapper();
            InitializeInteractiveColorLegend(); // add color legend to choropleth map
            InitializeGavToolTip();
            InitializeMap();
            InitializeCustomComponent();
            InitializeTableLens();
            InitializeparallelCoordinates();

            ControlComponentHandle(); // to handle things ohter than initializing the contorl compoent.


            InitializeViewManager();

        }

        private void setViewA()
        {
            this.tableLensA.SelectedIndex = this.dataSelectedIndex;
        }

        private void setViewB()
        {
            this.tableLensB.SelectedIndex = this.dataSelectedIndex;
        }

        private void InitializeTableLens()
        {
            //throw new NotImplementedException();

            this.tableLensA = new TableLensEncapsulator(this.interpolatingDataTransformer,this.excelDataProvider);
            this.tableLensB = new TableLensEncapsulator(this.interpolatingDataTransformer, this.excelDataProvider);
            
        }

        private void InitializeparallelCoordinates()
        {
            //throw new NotImplementedException();
            parallelCoordinatesPlot = new ParallelCoordinatesPlot();

            parallelCoordinatesPlot.Headers = excelDataProvider.ColumnHeaders;
            parallelCoordinatesPlot.Input = unfilteredYearSliceDataTransformer;
            parallelCoordinatesPlot.ColorMap = colorMap;

            parallelCoordinatesPlot.FilterChanged += new EventHandler(parallelCoordinatesPlot_FilterChanged);

            UpdateFilter();
            
        }

        private void UpdateFilter()
        {
            float[] min = new float[this.excelDataProvider.GetDataCube().GetAxisLength(Axis.X)];
            float[] max = new float[this.excelDataProvider.GetDataCube().GetAxisLength(Axis.X)];

            this.parallelCoordinatesPlot.ResetAxisMaxMin();
            
            float[] axis_min = new float[this.excelDataProvider.GetDataCube().GetAxisLength(Axis.X)];
            float[] axis_max = new float[this.excelDataProvider.GetDataCube().GetAxisLength(Axis.X)];

            List<float> lowerPos = parallelCoordinatesPlot.FilterLayer.GetLowerSliderPositions();
            List<float> upperPos = parallelCoordinatesPlot.FilterLayer.GetUpperSliderPositions();

            for (int i = 0; i < min.Length; ++i)
            {
                float min_axis, max_axis;
                parallelCoordinatesPlot.GetAxisMaxMin(i, out max_axis, out min_axis);
                axis_min[i] = (float)Math.Log(min_axis);
                axis_max[i] = (float)Math.Log(max_axis);

                if (lowerPos.Count == min.Length && upperPos.Count == max.Length)
                {
                    min[i] = lowerPos[i] * (max_axis - min_axis) + min_axis;
                    max[i] = upperPos[i] * (max_axis - min_axis) + min_axis;
                }
                else
                {
                    min[i] = min_axis;
                    max[i] = max_axis;
                }
            }

            this.dataFilterTransformer.MaxValues = max;
            this.dataFilterTransformer.MinValues = min;
            this.dataFilterTransformer.CommitChanges();
            this.yearSliceDataTransformer.CommitChanges();
            this.logDataTransformer.CommitChanges();

            this.choroplethMap.Invalidate();
            this.component.BuildLines();
            this.component.Invalidate();
        }

        void parallelCoordinatesPlot_FilterChanged(object sender, EventArgs e)
        {
            UpdateFilter();
        }



        private void InitializeDataTransformer()
        {
            //throw new NotImplementedException();

            this.interpolatingDataTransformer = new InterpolatingDataTransformer();
            this.interpolatingDataTransformer.Input = excelDataProvider;

            this.dataFilterTransformer = new FilterDataTransformer();
            this.dataFilterTransformer.Input = this.interpolatingDataTransformer;
            this.dataFilterTransformer.CurrentlySelectedYear = 1960;

            yearSliceDataTransformer = new YearSliceDataTransformer();
            yearSliceDataTransformer.Input = this.dataFilterTransformer;
            yearSliceDataTransformer.CurrentSelectedYear = 1960;

            unfilteredYearSliceDataTransformer = new YearSliceDataTransformer();
            unfilteredYearSliceDataTransformer.Input = this.interpolatingDataTransformer;
            unfilteredYearSliceDataTransformer.CurrentSelectedYear = 1960;
            
            logDataTransformer = new LogDataTransformer();
            logDataTransformer.Input = this.dataFilterTransformer;
        }

        private void ControlComponentHandle()
        {
            //throw new NotImplementedException();
            foreach (var i in excelDataProvider.ColumnHeaders)
            {
                this.comboBox_choropleth.Items.Add(i);
            }
            this.comboBox_choropleth.SelectedIndex = choroplethMapSelectedIndex;

            this.textBoxYear.Text = choroplethMapCurrentYear.ToString();

            this.trackBarYearSelecter.SmallChange = 1;
            this.trackBarYearSelecter.LargeChange = 10;
            this.trackBarYearSelecter.Minimum = 1960;
            this.trackBarYearSelecter.Maximum = 2008;
            this.startYearLabel.Text = this.trackBarYearSelecter.Minimum.ToString();
            this.endYearLabel.Text = this.trackBarYearSelecter.Maximum.ToString();
            
        }

        private void InitializeInteractiveColorLegend()
        {
            //throw new NotImplementedException();
            
            
            interactiveColorLegend = new InteractiveColorLegend();
            interactiveColorLegend.Name = "ColorLegend";
            interactiveColorLegend.ColorMap = colorMap;
            interactiveColorLegend.ShowColorEdgeSliders = true;
            interactiveColorLegend.UseRelativePosition = true;
            interactiveColorLegend.SetPosition(0.1f,0.5f);
            interactiveColorLegend.UseRelativeSize = true;
            interactiveColorLegend.SetLegendSize(10,70);
            interactiveColorLegend.SetHeader(excelDataProvider.ColumnHeaders[choroplethMapSelectedIndex]);
            float [] globalEdge = {2f,0.9f};
            List<float> edges = new List<float>();
            edges.Add(0.4f);
            edges.Add(0.4f);
            edges.Add(0.2f);
            interactiveColorLegend.EdgeValuesList = edges;
            float min = 0;
            float max = 0;
            yearSliceDataTransformer.GetDataCube().GetColumnMaxMin(choroplethMapSelectedIndex, out max, out min);
            interactiveColorLegend.MaxValue = max;
            interactiveColorLegend.MinValue = min;
            
            interactiveColorLegend.ShowMinMaxValues = true;
            
            interactiveColorLegend.Enabled = true;
            interactiveColorLegend.ValueSliderValuesChanged += new EventHandler(interactiveColorLegend_ValueSliderValuesChanged);
            interactiveColorLegend.ColorEdgeValuesChanged += new EventHandler(interactiveColorLegend_ColorEdgeValuesChanged);
            interactiveColorLegend.ThresholdValuesChanged += new EventHandler(interactiveColorLegend_ThresholdValuesChanged); 
        }

        void interactiveColorLegend_ThresholdValuesChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            choroplethMap.Invalidate();
            parallelCoordinatesPlot.Invalidate();
        }

        void interactiveColorLegend_ColorEdgeValuesChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            choroplethMap.Invalidate();
            parallelCoordinatesPlot.Invalidate();
        }

        void interactiveColorLegend_ValueSliderValuesChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            
            choroplethMap.Invalidate();
            parallelCoordinatesPlot.Invalidate();
            
        }

        private void InitializeGavToolTip()
        {
            //throw new NotImplementedException();
            gavToolTip = new GavToolTip(this);
            gavToolTip.SetPosition(new Point(0,0));
            gavToolTip.ToolTipBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            gavToolTip.Show(new Point(0,0));
            gavToolTip.Hide();
            
            
        }

        private void InitializeViewManager()
        {
            //throw new NotImplementedException();
            viewManager = new ViewManager(this);
            viewManager.Add(choroplethMap, splitContainer2.Panel1);
            viewManager.Add(component, splitContainer2.Panel2);
            viewManager.Add(this.tableLensA.tablelens, this.BarGraphContainer.Panel1);
            viewManager.Add(this.tableLensB.tablelens, this.BarGraphContainer.Panel2);
            viewManager.Add(this.parallelCoordinatesPlot, this.FilterContainer.Panel2);
            viewManager.InvalidateAll();

        }

        private void InitializeCustomComponent()
        {
            //throw new NotImplementedException();
            component = new CustomComponent();
            component.Input = logDataTransformer.GetDataCube();
            component.ColorMap = colorMap;
            this.component.SelectionChanged += new CustomComponent.SelectionUpdatedEventHandler(customComponent_SelectionUpdatedEvent);
            this.component.DataLabels = this.excelDataProvider.ColumnHeaders;
            this.component.SelectedIndexColor = Color.Yellow;
            List<int> selected = new List<int>();
            selected.Add(this.dataSelectedIndex);
            this.component.SetSelectedIndexes(selected);
            this.component.UserMenu = this.userMenu;
            this.component.DataLineThicknessScale = 2.0f;
        }

        private void InitializeData()
        {
            excelDataProvider = new ExcelDataProvider();
            excelDataProvider.HasIDColumn = true;

            //here, i can not use my data, if so ,it will crash. so i comment you component
            //temporarily
            excelDataProvider.Load("world data.xls");   


            mapData = new MapData();
            mapData = MapReader.Read("world5.map", "world.dbf");
            //int i = mapData.GetRegionId(0.3f,0.4f);

        }
        private void InitializeColor()
        {
            colorMap = new ColorMap();
            
            colorMap.Input = yearSliceDataTransformer.GetDataCube();
            colorMap.Index = choroplethMapSelectedIndex;
            //colorMap.AddColorMapPart(new LinearRgbColorMapPart(Color.CadetBlue,Color.GhostWhite));
            //colorMap.AddColorMapPart(new LinearRgbColorMapPart(Color.GhostWhite,Color.Red));
            
            colorMap.AddColorMapPart(new LinearRgbColorMapPart(Color.Blue,Color.LightYellow));
            colorMap.AddColorMapPart(new LinearRgbColorMapPart(Color.LightYellow,Color.Red));
            
            colorMap.NaNColor = Color.Black;
            //colorMap.AddColorMapPart(new LinearHsvColorMapPart(200,40,0.1f,0.5f));
            
        }

        private void InitializeMap()
        {
            //throw new NotImplementedException();
            choroplethMap = new ChoroplethMap();
            
            mapBorderLayer = new MapBorderLayer();
            mapBorderLayer.MapData = mapData;
            
            mapPolygonLayer = new MapPolygonLayer();
            mapPolygonLayer.MapData = mapData;
            mapPolygonLayer.IndexMapper = stringIndexMapper;
            mapPolygonLayer.ColorMap = colorMap;
            choroplethMap.Position = new Vector2(0.5f, 0.23f);
            choroplethMap.Zoom = -(0.75f);

            choroplethMap.AddLayer(mapPolygonLayer);
            
            choroplethMap.AddLayer(mapBorderLayer);
            
            choroplethMap.AddSubComponent(interactiveColorLegend);
            choroplethMap.VizComponentMouseDown += new EventHandler<VizComponentMouseEventArgs>(choroplethMap_VizComponentMouseDown);
            choroplethMap.VizComponentMouseUp += new EventHandler<VizComponentMouseEventArgs>(choroplethMap_VizComponentMouseUp);
            choroplethMap.VizComponentMouseMove += new EventHandler<VizComponentMouseEventArgs>(choroplethMap_VizComponentMouseMove);
            choroplethMap.PositionInternallyChanged += new EventHandler(choroplethMap_PositionInternallyChanged);
            choroplethMap.ZoomInternallyChanged += new EventHandler(choroplethMap_ZoomInternallyChanged);
        }



        void choroplethMap_VizComponentMouseMove(object sender, VizComponentMouseEventArgs e)
        {
            Point p = e.MouseEventArgs.Location;
            Vector2 mapCoordinates = choroplethMap.ConvertScreenCoordinatesToMapCoordinates(p);
            int index = mapData.GetRegionId(mapCoordinates.X, mapCoordinates.Y);
            int mappedIndex = 0;
            stringIndexMapper.TryMapIndex(index, out mappedIndex);
            if (mappedIndex != -1)
            {
                gavToolTip.Text = excelDataProvider.RowIds[mappedIndex];
                gavToolTip.Text += "\n";
                gavToolTip.Text += excelDataProvider.ColumnHeaders[choroplethMapSelectedIndex] + ":" + yearSliceDataTransformer.GetDataCube().DataArray[choroplethMapSelectedIndex, mappedIndex, 0];

                gavToolTip.Show(p);
            }
            else
                gavToolTip.Hide();
        }

        private bool drag_begin = false;
        private Vector2 drag_position = new Vector2();
        private bool zoom_begin = false;
        private float zoom_level = new float();

        void choroplethMap_VizComponentMouseUp(object sender, VizComponentMouseEventArgs e)
        {
            if (e.MouseEventArgs.Button == System.Windows.Forms.MouseButtons.Left && !drag_begin)
            { //throw new NotImplementedException();
                this.splitContainer2.Panel1.Focus();

                this.splitContainer2.Focus();
                Point p = e.MouseEventArgs.Location;
                Vector2 mapCoordinates = choroplethMap.ConvertScreenCoordinatesToMapCoordinates(p);
                int index = mapData.GetRegionId(mapCoordinates.X, mapCoordinates.Y);
                int mappedIndex = 0;
                stringIndexMapper.TryMapIndex(index, out mappedIndex);

                //gavToolTip.SetPosition(new Point(0, 0));


                if (index > 0 && mappedIndex != -1)
                {
                    gavToolTip.Hide();

                    this.dataSelectedIndex = mappedIndex;
                    this.mapSelectedIndex = index;

                    List<int> currentSelectedIndexes = mapPolygonLayer.GetSelectedIndexes();

                    List<int> mapSelected = new List<int>() { this.mapSelectedIndex };
                    mapPolygonLayer.SelectedPolygonColor = Color.Yellow;
                    mapPolygonLayer.SetSelectedIndexes(mapSelected);
                    choroplethMap.Invalidate();

                    List<int> dataSelected = new List<int>() { this.dataSelectedIndex };
                    this.component.SetSelectedIndexes(dataSelected);
                    this.component.Invalidate();
                }
            }
            if (e.MouseEventArgs.Button == System.Windows.Forms.MouseButtons.Right && !zoom_begin)
            {
                this.userMenu.Show(this, e.MouseEventArgs.Location);
            }
        }

        void choroplethMap_VizComponentMouseDown(object sender, VizComponentMouseEventArgs e)
        {
            if (e.MouseEventArgs.Button == System.Windows.Forms.MouseButtons.Left)
            {
                this.drag_begin = false;
                this.drag_position = choroplethMap.Position;
            }
            if (e.MouseEventArgs.Button == System.Windows.Forms.MouseButtons.Right)
            {
                this.zoom_begin = false;
                this.zoom_level = choroplethMap.Zoom;
            }
            
        }
        void choroplethMap_ZoomInternallyChanged(object sender, EventArgs e)
        {
            if (this.choroplethMap.Zoom != this.zoom_level)
                this.zoom_begin = true;
        }

        void choroplethMap_PositionInternallyChanged(object sender, EventArgs e)
        {
            if (this.choroplethMap.Position != this.drag_position)
                this.drag_begin = true;
        }

        private void InitializeStringIndexMapper()
        {
            //throw new NotImplementedException();
            stringIndexMapper = new StringIndexMapper(mapData.RegionFullNames, excelDataProvider.RowIds);
            this.stringIndexMapper.TryBackwardMapIndex(this.dataSelectedIndex, out this.__mapSelectedIndex);
        }

        private void customComponent_SelectionUpdatedEvent(object sender, CustomComponent.SelectionUpdatedEventArgs e)
        {
            
            foreach (int i in e.SelectedItems)
            {
                int mappedIndex;
                stringIndexMapper.TryBackwardMapIndex(i, out mappedIndex);
                if (mappedIndex != -1)
                {
                    this.dataSelectedIndex = i;
                    this.mapSelectedIndex = mappedIndex;
                }
            }

            mapPolygonLayer.SelectedPolygonColor = Color.Yellow;
            List<int> selected = new List<int>() { this.mapSelectedIndex };
            mapPolygonLayer.SetSelectedIndexes(selected);

            //this.mapPolygonLayer.Invalidate();
            this.choroplethMap.Invalidate();
        }

        private void comboBox_choropleth_SelectedIndexChanged(object sender, EventArgs e)
        {
            //string currentText = comboBox_choropleth.Text;
            choroplethMapSelectedIndex = comboBox_choropleth.SelectedIndex;
            colorMap.Index = choroplethMapSelectedIndex;
            this.tableLensA.SelectedIndicator = choroplethMapSelectedIndex;
            this.tableLensB.SelectedIndicator = choroplethMapSelectedIndex;

            //mapPolygonLayer.Invalidate();

            float min = 0;
            float max = 0;
            yearSliceDataTransformer.GetDataCube().GetColumnMaxMin(choroplethMapSelectedIndex, out max, out min);


            interactiveColorLegend.MaxValue = max;
            interactiveColorLegend.MinValue = min;

            interactiveColorLegend.SetHeader(excelDataProvider.ColumnHeaders[choroplethMapSelectedIndex]);
            choroplethMap.Invalidate();
        }

        void trackBarYearSelecter_ValueChanged(object sender, System.EventArgs e)
        {
            //throw new System.NotImplementedException();
            this.textBoxYear.Text = this.trackBarYearSelecter.Value.ToString();
            this.dataFilterTransformer.CurrentlySelectedYear = this.trackBarYearSelecter.Value;
            this.yearSliceDataTransformer.CurrentSelectedYear = this.trackBarYearSelecter.Value;
            this.unfilteredYearSliceDataTransformer.CurrentSelectedYear = this.trackBarYearSelecter.Value;
            this.unfilteredYearSliceDataTransformer.CommitChanges();
            this.parallelCoordinatesPlot.Invalidate();
            UpdateFilter();

            colorMap.Input = yearSliceDataTransformer.GetDataCube();
            colorMap.Index = choroplethMapSelectedIndex;

            float[] old_edges = interactiveColorLegend.EdgeValues;

            colorMap.Invalidate();
            interactiveColorLegend.ColorMap = colorMap;
            float min = 0;
            float max = 0;
            yearSliceDataTransformer.GetDataCube().GetColumnMaxMin(choroplethMapSelectedIndex, out max, out min);
            
            interactiveColorLegend.MaxValue = max;
            interactiveColorLegend.MinValue = min;

            interactiveColorLegend.EdgeValues = old_edges;

            viewManager.InvalidateAll();
        }

        private void splitContainer3_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void splitContainer4_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

    }
}