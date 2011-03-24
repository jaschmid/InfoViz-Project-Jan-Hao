using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Gav.Data;
using Gav.Transformers;

namespace InfoVizProject
{
    class TransposDataTransformer: DataTransformer
    {
        private int selectedIndicator;
        public int SelectedIndicator
        {
            set { selectedIndicator = value; }
            get { return selectedIndicator; }
        }
        private List<int> selectedCountry;
        public List<int> SelectedCountry
        {
            set { selectedCountry = value;}
            get {return selectedCountry;}
        }
        
        protected override void ProcessData()
        {
            
            float[, ,] inputData = _input.GetDataCube().DataArray;
            int sizeX = inputData.GetLength(0);
            int sizeY = inputData.GetLength(1);
            int sizeZ = inputData.GetLength(2);
            int countryNum = selectedCountry.Count;
            float[, ,] outputData = new float[countryNum, sizeZ, 1];
           
                for(int j = 0;j<countryNum;j++)
                {
                    for (int k = 0; k < sizeZ; k++)
                    {
                        outputData[j, k, 0] = inputData[selectedIndicator,selectedCountry[j] ,k];
                    }
                }

                  
            _dataCube.DataArray = outputData;
            
        }
    }
}
