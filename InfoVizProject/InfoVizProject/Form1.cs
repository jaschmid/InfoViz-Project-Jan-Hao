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
        private List<int> selectedCountry = new List<int>() { 100,200};

        List<int> countryIdList = new List<int>();


        private ExcelDataProvider excelDataProvider;
        private YearSliceDataTransformer yearSliceDataTransformer;
        private LogDataTransformer logDataTransformer;
        private TransposDataTransformer transposeDataTransformer;

        private ChoroplethMap choroplethMap;  // world map component
        private MapData mapData;
        private MapBorderLayer mapBorderLayer;
        private MapPolygonLayer mapPolygonLayer;
        private InteractiveColorLegend interactiveColorLegend;
        private GavToolTip gavToolTip;
        private StringIndexMapper stringIndexMapper;

        private ParallelCoordinatesPlot parallelCoordinatesPlot;
        

        private TableLens tablelens;

        private ViewManager viewManager;
        private CustomComponent component;
        private ColorMap colorMap;

        public System.EventHandler keydown;

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
            InitializeTableLens();
            InitializeparallelCoordinates();

            ControlComponentHandle(); // to handle things ohter than initializing the contorl compoent.


            InitializeViewManager();

            

            
        }

        private void InitializeTableLens()
        {
            //throw new NotImplementedException();
            tablelens = new TableLens();
            transposeDataTransformer.SelectedCountry = selectedCountry;
            tablelens.Input = transposeDataTransformer.GetDataCube();
            tablelens.ColorMap = colorMap;
            List<string> countrylist = new List<string>();
            for (int i = 0; i < selectedCountry.Count; i++)
            {
                countrylist.Add(excelDataProvider.RowIds[i]);
            }
            tablelens.HeadersList = countrylist;
            
            
        }

        private void InitializeparallelCoordinates()
        {
            //throw new NotImplementedException();
            parallelCoordinatesPlot = new ParallelCoordinatesPlot();
            
            parallelCoordinatesPlot.Input = excelDataProvider;
            parallelCoordinatesPlot.ColorMap = colorMap;

            
        }

        

        private void InitializeDataTransformer()
        {
            //throw new NotImplementedException();
            yearSliceDataTransformer = new YearSliceDataTransformer();
            yearSliceDataTransformer.Input = excelDataProvider;
            yearSliceDataTransformer.CurrentSelectedYear = 1960;
            transposeDataTransformer = new TransposDataTransformer();
            transposeDataTransformer.Input = excelDataProvider;
            transposeDataTransformer.SelectedCountry = selectedCountry;
            transposeDataTransformer.SelectedIndicator = choroplethMapSelectedIndex;
            transposeDataTransformer.GetDataCube();
            
            logDataTransformer = new LogDataTransformer();
            logDataTransformer.Input = excelDataProvider;
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
            gavToolTip.Show(new Point(0,0));
            gavToolTip.Hide();
            
            
        }

        private void InitializeViewManager()
        {
            //throw new NotImplementedException();
            viewManager = new ViewManager(this);
            viewManager.Add(choroplethMap, splitContainer2.Panel1);
            viewManager.Add(component, splitContainer2.Panel2);
            viewManager.Add(tablelens, splitContainer3.Panel1);
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
            
            colorMap.AddColorMapPart(new LinearRgbColorMapPart(Color.FromArgb(100,100,240),Color.FromArgb(200,200,250)));
            colorMap.AddColorMapPart(new LinearRgbColorMapPart(Color.FromArgb(255,200,200),Color.FromArgb(255,50,50)));
            colorMap.AddColorMapPart(new LinearRgbColorMapPart(Color.FromArgb(255, 0, 255), Color.FromArgb(200, 0, 200)));
            
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
        }
       

        void choroplethMap_VizComponentMouseDown(object sender, VizComponentMouseEventArgs e)
        {
            //throw new NotImplementedException();
            this.splitContainer2.Panel1.Focus();
            
            this.splitContainer2.Focus();
            Point p = e.MouseEventArgs.Location;
            Vector2 mapCoordinates = choroplethMap.ConvertScreenCoordinatesToMapCoordinates(p);
            int index = mapData.GetRegionId(mapCoordinates.X,mapCoordinates.Y);
            int mappedIndex = 0;
            stringIndexMapper.TryMapIndex(index,out mappedIndex);
            
            if (e.MouseEventArgs.Button == System.Windows.Forms.MouseButtons.Left &&index > 0 && mappedIndex != -1)
            {
                gavToolTip.Text = excelDataProvider.RowIds[mappedIndex];
                gavToolTip.Text += "\n";
                gavToolTip.Text += excelDataProvider.ColumnHeaders[choroplethMapSelectedIndex] + ":" + yearSliceDataTransformer.GetDataCube().DataArray[choroplethMapSelectedIndex, mappedIndex, 0];

                //gavToolTip.SetPosition(new Point(0, 0));

                gavToolTip.Show(p);




            }
            if (e.MouseEventArgs.Button == System.Windows.Forms.MouseButtons.Right && index > 0 && mappedIndex != -1)
            {
                gavToolTip.Hide();
                if (selectedCountry.Count == 2)
                {
                    int temp = selectedCountry[1];
                    selectedCountry.Clear();
                    selectedCountry.Add(temp);

                }
                if (countryIdList.Count == 2)
                {
                    int temp = countryIdList[1];
                    countryIdList.Clear();
                    countryIdList.Add(temp);
                }

                countryIdList.Add(index);
                selectedCountry.Add(mappedIndex);
                transposeDataTransformer.SelectedCountry = selectedCountry;
                transposeDataTransformer.CommitChanges();
                tablelens.Input = transposeDataTransformer.GetDataCube();
                List<string> countrylist = new List<string>();
                for (int i = 0; i < selectedCountry.Count; i++)
                {
                    countrylist.Add(excelDataProvider.RowIds[selectedCountry[i]]);
                }
                tablelens.HeadersList = countrylist;
                tablelens.Invalidate();


                mapPolygonLayer.SelectedPolygonColor = Color.Yellow;
                mapPolygonLayer.SetSelectedIndexes(countryIdList);



                mapBorderLayer.SetSelectedIndexes(selectedCountry);
                mapBorderLayer.Invalidate();
                choroplethMap.Invalidate();
            }
            
        }

        private void InitializeStringIndexMapper()
        {
            //throw new NotImplementedException();
            stringIndexMapper = new StringIndexMapper(mapData.RegionFullNames, excelDataProvider.RowIds);
        }

        private void customComponent_SelectionUpdatedEvent(object sender, CustomComponent.SelectionUpdatedEventArgs e)
        {
            List<int> selectedItems = new List<int>();
            foreach (int i in e.SelectedItems)
            {
                int mappedIndex = 0;
                stringIndexMapper.TryBackwardMapIndex(i, out mappedIndex);
                if(mappedIndex != -1)
                    selectedItems.Add(mappedIndex);
            }

            this.mapPolygonLayer.SetSelectedIndexes(selectedItems);
            this.mapPolygonLayer.Invalidate();
            this.choroplethMap.Invalidate();
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
            transposeDataTransformer.SelectedIndicator = choroplethMapSelectedIndex;
            transposeDataTransformer.CommitChanges();
            tablelens.Invalidate();
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

        private void splitContainer2_Panel1_MouseHover(object sender, EventArgs e)
        {
            
        }

    }
}