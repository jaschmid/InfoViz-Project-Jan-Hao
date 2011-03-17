using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Gav.Data;
using Gav.Transformers;

namespace InfoVizProject
{
    class YearSliceDataTransformer : DataTransformer
    {
        private int currentSelectedYear;
        public int CurrentSelectedYear
        {
            get
            { return currentSelectedYear; }
            set
            { currentSelectedYear = value; }
        }
        protected override void ProcessData()
        {
            //throw new NotImplementedException();
            float[, ,] inputData = _input.GetDataCube().DataArray;
            int sizeX = inputData.GetLength(0);
            int sizeY = inputData.GetLength(1);
            float[, ,] outputData = new float[sizeX, sizeY, 1];
            for (int i = 0; i < sizeX; i++)
			{
                for (int j = 0; j < sizeY; j++)
			    {
			        outputData[i,j,0] = inputData[i,j,currentSelectedYear-1960];
			    }
			 
			}
            _dataCube.DataArray = outputData;
        }
    }
}
