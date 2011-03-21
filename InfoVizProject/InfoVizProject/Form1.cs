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
       


        private ExcelDataProvider excelDataProvider;
        private YearSliceDataTransformer yearSliceDataTransformer;
        private ChoroplethMap choroplethMap;  // world map component
        private MapData mapData;
        private MapBorderLayer mapBorderLayer;
        private MapPolygonLayer mapPolygonLayer;
        private InteractiveColorLegend interactiveColorLegend;
        private GavToolTip gavToolTip;
        private StringIndexMapper stringIndexMapper;

        private ViewManager viewManager;
        private CustomComponent component;
        private ColorMap colorMap;
        
        public Form1()
        {
            InitializeComponent();

            InitializeData();
            InitializeDataTransformer();
            InitializeColor();
            InitializeStringIndexMapper();
            InitializeInteractiveColorLegend(); // add color legend to choropleth map
            InitializeGavToolTip();
            InitializeMap();
            InitializeCustomComponent();

            ControlComponentHandle(); // to handle things ohter than initializing the contorl compoent.


            InitializeViewManager();

            
        }

        

        private void InitializeDataTransformer()
        {
            //throw new NotImplementedException();
            yearSliceDataTransformer = new YearSliceDataTransformer();
            yearSliceDataTransformer.Input = excelDataProvider;
            yearSliceDataTransformer.CurrentSelectedYear = 1960;
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
            this.trackBarYearSelecter.Maximum = 1975;
            
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
            float [] globalEdge = {0.3f,0.4f};
            List<float> postiton = new List<float>() ;
            postiton.Add(0.2f);
            interactiveColorLegend.SetValueSlider(postiton,InteractiveColorLegend.SliderLinePosition.Center,InteractiveColorLegend.TextPosition.RightOrBottom,true);
            interactiveColorLegend.MinTextPosition = InteractiveColorLegend.TextPosition.RightOrBottom;
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
        }

        void interactiveColorLegend_ColorEdgeValuesChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            choroplethMap.Invalidate();
        }

        void interactiveColorLegend_ValueSliderValuesChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            
            choroplethMap.Invalidate();
            
        }

        private void InitializeGavToolTip()
        {
            //throw new NotImplementedException();
            gavToolTip = new GavToolTip(this);
            gavToolTip.SetPosition(new Point(0,0));
            gavToolTip.ToolTipBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            
            
        }

        private void InitializeViewManager()
        {
            //throw new NotImplementedException();
            viewManager = new ViewManager(this);
            viewManager.Add(choroplethMap, splitContainer2.Panel1);
            viewManager.Add(component, splitContainer2.Panel2);
            viewManager.InvalidateAll();

        }

        private void InitializeCustomComponent()
        {
            //throw new NotImplementedException();
            component = new CustomComponent();
            component.Input = excelDataProvider;
            component.ColorMap = colorMap;
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
            colorMap.AddColorMapPart(new LinearRgbColorMapPart(Color.FromArgb(100,100,125),Color.FromArgb(255,255,255)));
            colorMap.AddColorMapPart(new LinearRgbColorMapPart(Color.FromArgb(255,255,255),Color.FromArgb(255,0,0)));
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
            

        }

        void choroplethMap_VizComponentMouseDown(object sender, VizComponentMouseEventArgs e)
        {
            //throw new NotImplementedException();
            Point p = e.MouseEventArgs.Location;
            Vector2 mapCoordinates = choroplethMap.ConvertScreenCoordinatesToMapCoordinates(p);
            int index = mapData.GetRegionId(mapCoordinates.X,mapCoordinates.Y);
            int mappedIndex = 0;
            stringIndexMapper.TryMapIndex(index,out mappedIndex);
            if (index > 0)
            {
                gavToolTip.Text = excelDataProvider.RowIds[mappedIndex];
                gavToolTip.Text += "\n";
                gavToolTip.Text += excelDataProvider.ColumnHeaders[choroplethMapSelectedIndex] + ":" + yearSliceDataTransformer.GetDataCube().DataArray[choroplethMapSelectedIndex, mappedIndex, 0];
                
                gavToolTip.SetPosition(new Point(0,0));

                gavToolTip.Show(p);
                
                
                
            }
            choroplethMap.Invalidate();
        }

        private void InitializeStringIndexMapper()
        {
            //throw new NotImplementedException();
            stringIndexMapper = new StringIndexMapper(mapData.RegionFullNames, excelDataProvider.RowIds);
        }

        private void comboBox_choropleth_SelectedIndexChanged(object sender, EventArgs e)
        {
            //string currentText = comboBox_choropleth.Text;
            choroplethMapSelectedIndex = comboBox_choropleth.SelectedIndex;
            colorMap.Index = choroplethMapSelectedIndex;
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
            yearSliceDataTransformer.CurrentSelectedYear = this.trackBarYearSelecter.Value;
            yearSliceDataTransformer.CommitChanges();
            colorMap.Input = yearSliceDataTransformer.GetDataCube();
            colorMap.Index = choroplethMapSelectedIndex;
            
            colorMap.Invalidate();
            interactiveColorLegend.ColorMap = colorMap;
            float min = 0;
            float max = 0;
            yearSliceDataTransformer.GetDataCube().GetColumnMaxMin(choroplethMapSelectedIndex, out max, out min);
            

            interactiveColorLegend.MaxValue = max;
            interactiveColorLegend.MinValue = min;

            viewManager.InvalidateAll();
        }

    }
}