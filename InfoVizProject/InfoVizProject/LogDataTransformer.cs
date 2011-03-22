using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Gav.Data;
using Gav.Transformers;

namespace InfoVizProject
{
    class LogDataTransformer : DataTransformer
    {
        protected override void ProcessData()
        {
            //throw new NotImplementedException();
            float[, ,] inputData = _input.GetDataCube().DataArray;
            int sizeX = inputData.GetLength(0);
            int sizeY = inputData.GetLength(1);
            int sizeZ = inputData.GetLength(2);
            float[, ,] outputData = new float[sizeX, sizeY, sizeZ];
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    for (int k = 0; k < sizeZ; k++)
                        outputData[i, j, k] = (float)Math.Log((double)inputData[i, j, k]);
                }

            }
            _dataCube.DataArray = outputData;
        }
    }
}
