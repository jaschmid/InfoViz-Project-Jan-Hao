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

        private ExcelDataProvider excelDataProvider;
        private ViewManager viewManager;
        private SampleGavComponent component;
        private ColorMap colorMap;

        public Form1()
        {

            InitializeComponent();

            Panel customPanel = panel1;

            // Init Data
            excelDataProvider = new ExcelDataProvider();
            excelDataProvider.HasIDColumn = true;
            excelDataProvider.Load("data.xls");

            // Init Color
            colorMap = new ColorMap();
            colorMap.Input = excelDataProvider;
            colorMap.AddColorMapPart(new LinearRgbColorMapPart(Color.LightGreen, Color.Yellow));
            colorMap.AddColorMapPart(new LinearRgbColorMapPart(Color.Yellow, Color.Red));

            // Init custom component
            component = new SampleGavComponent();
            component.Input = excelDataProvider;
            component.ColorMap = colorMap;

            // Init View Manager
            viewManager = new ViewManager(this);
            viewManager.Add(component, customPanel);

        }

    }
}