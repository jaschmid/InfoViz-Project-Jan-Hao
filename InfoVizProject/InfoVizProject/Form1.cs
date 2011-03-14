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
        private ExcelDataProvider excelDataProvider;

        private ChoroplethMap choroplethMap;  // world map component
        private MapData mapData;
        private MapBorderLayer mapBorderLayer;
        private MapPolygonLayer mapPolygonLayer;

        private StringIndexMapper stringIndexMapper;

        private ViewManager viewManager;
        private SampleGavComponent component;
        private ColorMap colorMap;
        
        public Form1()
        {
            InitializeComponent();

            InitializeData();
            InitializeColor();
            InitializeStringIndexMapper();
            InitializeMap();
            InitializeCustomComponent();

            InitializeViewManager();
            
        }

        private void InitializeViewManager()
        {
            //throw new NotImplementedException();
            viewManager = new ViewManager(this);
            viewManager.Add(choroplethMap, splitContainer2.Panel1);
            //viewManager.Add(component, splitContainer2.Panel2);
            viewManager.InvalidateAll();

        }

        private void InitializeCustomComponent()
        {
            //throw new NotImplementedException();
            component = new SampleGavComponent();
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
            colorMap.Index = 1;
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

        }

        private void InitializeStringIndexMapper()
        {
            //throw new NotImplementedException();
            stringIndexMapper = new StringIndexMapper(mapData.RegionFullNames, excelDataProvider.RowIds);
        }

    }
}