using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Gav.Data;
using Gav.Transformers;

namespace InfoVizProject
{
    class FilterDataTransformer : DataTransformer
    {
        public float[] MinValues
        {
            get;
            set;
        }
        public float[] MaxValues
        {
            get;
            set;
        }
        public int CurrentlySelectedYear
        {
            get;
            set;
        }

        protected override void ProcessData()
        {
            //throw new NotImplementedException();
            float[, ,] inputData = _input.GetDataCube().DataArray;
            int sizeX = inputData.GetLength(0);
            int sizeY = inputData.GetLength(1);
            int sizeZ = inputData.GetLength(2);
            float[, ,] outputData = new float[sizeX, sizeY, sizeZ];

            for (int j = 0; j < sizeY; j++)
            {

                bool filter_ok = true;
                if( MinValues != null && MaxValues != null)
                {
                    for (int i = 0; i < sizeX; i++)
                    {
                        float val =inputData[i, j, CurrentlySelectedYear - 1960];
                        if ((val <= MaxValues[i] && val >= MinValues[i] )|| float.IsNaN(val) || float.IsInfinity(val))
                            continue;
                        else
                        {
                            filter_ok = false;
                            break;
                        }
                    }
                }

                for (int k = 0; k < sizeZ; k++)
                {

                    for (int i = 0; i < sizeX; i++)
                    {
                        if (filter_ok)
                            outputData[i, j, k] = inputData[i, j, k];
                        else
                            outputData[i, j, k] = float.NaN;
                    }
                    
                }
            }

            _dataCube.DataArray = outputData;
        }
    }
}
