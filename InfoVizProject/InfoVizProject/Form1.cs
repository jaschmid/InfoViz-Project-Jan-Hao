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
        private ChoroplethMap choroplethMap;  // world map component
        private MapData mapData;
        private MapBorderLayer mapBorderLayer;
        private MapPolygonLayer mapPolygonLayer;
        private InteractiveColorLegend interactiveColorLegend;
        private StringIndexMapper stringIndexMapper;

        private ViewManager viewManager;
        private CustomComponent component;
        private ColorMap colorMap;
        
        public Form1()
        {
            InitializeComponent();

            InitializeData();
            InitializeColor();
            InitializeStringIndexMapper();
            InitializeInteractiveColorLegend(); // add color legend to choropleth map
            InitializeMap();
            InitializeCustomComponent();

            ControlComponentHandle(); // to handle things ohter than initializing the contorl compoent.


            InitializeViewManager();

            
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
            
        }

        private void InitializeInteractiveColorLegend()
        {
            //throw new NotImplementedException();
            float[] edgeValues={0} ;
            interactiveColorLegend = new InteractiveColorLegend();
            interactiveColorLegend.ColorMap = colorMap;
            interactiveColorLegend.EdgeValues = edgeValues;
            interactiveColorLegend.UseRelativePosition = true;
            interactiveColorLegend.SetPosition(0.1f,0.5f);
            interactiveColorLegend.UseRelativeSize = true;
            interactiveColorLegend.SetLegendSize(10,70);
            interactiveColorLegend.SetHeader(excelDataProvider.ColumnHeaders[choroplethMapSelectedIndex]);

            
            interactiveColorLegend.Enabled = true;
            
            
        }

        private void InitializeViewManager()
        {
            //throw new NotImplementedException();
            viewManager = new ViewManager(this);
            viewManager.Add(choroplethMap, splitContainer2.Panel1);
            viewManager.Add(component, splitContainer2.Panel2);
            

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
            colorMap.Input = excelDataProvider;
            colorMap.Index = choroplethMapSelectedIndex;
            //colorMap.AddColorMapPart(new LinearRgbColorMapPart(Color.CadetBlue,Color.GhostWhite));
            //colorMap.AddColorMapPart(new LinearRgbColorMapPart(Color.GhostWhite,Color.Red));
            colorMap.AddColorMapPart(new LinearHsvColorMapPart(0, 360));
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
            
            interactiveColorLegend.SetHeader(excelDataProvider.ColumnHeaders[choroplethMapSelectedIndex]);
            choroplethMap.Invalidate();
        }

    }
}