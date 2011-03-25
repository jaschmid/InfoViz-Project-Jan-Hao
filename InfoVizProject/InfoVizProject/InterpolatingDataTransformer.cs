using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Gav.Data;
using Gav.Transformers;

namespace InfoVizProject
{
    class InterpolatingDataTransformer : DataTransformer
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
                    float lastValue = float.NaN;
                    int lastIndex = 0;
                    for (int k = 0; k < sizeZ; k++)
                    {
                        float val = inputData[i, j, k];
                        if (!float.IsNaN(val) && !float.IsInfinity(val))
                        {
                            // good value
                            lastValue = val;
                            lastIndex = k;
                            outputData[i, j, k] = val;
                        }
                        else
                        {
                            //try to interpolate
                            if (!float.IsNaN(lastValue))
                            {
                                //find next value and index
                                float nextValue = float.NaN;
                                int nextIndex = 0;
                                for (int k2 = k + 1; k2 < sizeZ; k2++)
                                {
                                    float valN = inputData[i, j, k2];
                                    if (!float.IsNaN(valN) && !float.IsInfinity(valN))
                                    {
                                        nextValue = valN;
                                        nextIndex = k2;
                                        break;
                                    }
                                }

                                if (!float.IsNaN(nextValue))
                                {
                                    //can interpolate
                                    float factor = ((float)(k - lastIndex)) / (float)(nextIndex - lastIndex);
                                    outputData[i, j, k] = factor * (nextValue - lastValue) + lastValue;
                                }
                                else
                                    outputData[i, j, k] = float.NaN;//can't interpolate
                            }
                            else
                                outputData[i, j, k] = float.NaN;//can't interpolate
                        }
                    }
                }

            }
            _dataCube.DataArray = outputData;
        }
    }
}
