//**************************************************************************************
//
// This header is an inseparable part of the source code.
// This source code is created by György Richárd Bogár.
// It is mandatory to refer to the source in all uses that continuously contain
// at least 10% of the code or any smaller code snippet which makes it identifiable.
//
//**************************************************************************************

__kernel void advancedFlow( read_only image2d_t leftInput,
                            read_only image2d_t rightInput,
                            __global float* inputFlowU,
                            __global float* inputFlowV,
                            __global float* outputFlowU,
                            __global float* outputFlowV,
                            int inputFlowWidth,
                            int inputFlowHeight,
                            float noiseReduction,
                            int lastLevel){

    sampler_t sampler = CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST ;
	float NOISE_TRESHOLD = 100.0f;
    int index = get_global_id(0);	
	int x = (index % inputFlowWidth) * 5;
	int y = (index / inputFlowWidth) * 5;

    float sumXX = 0.0;
    float sumYY = 0.0;
    float sumXY = 0.0;
    float sumXT = 0.0;
    float sumYT = 0.0;
    float divisor;

	float gradX[5][5];
	float gradY[5][5];
    int gradT = 0;

    float resultU = inputFlowU[index];
    float resultV = inputFlowV[index];

    float gain = 1.0f;    

    for(int i = 0; i < 5; i++){
        for(int j = 0; j < 5; j++){         
            gradX[i][j] = (float)(read_imagei(leftInput, sampler, (int2)(x+j+1, y+i)).x - read_imagei(leftInput, sampler, (int2)(x+j-1, y+i)).x);
            gradX[i][j] /= 2.0f;
            gradY[i][j] = (float)(read_imagei(leftInput, sampler, (int2)(x+j, y+i+1)).x - read_imagei(leftInput, sampler, (int2)(x+j, y+i-1)).x);
            gradY[i][j] /= 2.0f;

            sumXX += gradX[i][j] * gradX[i][j];
		    sumYY += gradY[i][j] * gradY[i][j];
    		sumXY += gradX[i][j] * gradY[i][j];
        }
    }



    // Main iteration START
    float dU = 0.0f;
    float dV = 0.0f;
    for(int iteration = 0; iteration < 10; iteration++){
        sumXT = 0.0f;
        sumYT = 0.0f;

        for(int i = 0; i < 5; i++){
            for(int j = 0; j < 5; j++){
	    		gradT = (read_imagei(rightInput, sampler, (int2)(x+j+(int)(resultU), y+i+(int)(resultV))).x - read_imagei(leftInput, sampler, (int2)(x+j, y+i)).x);
	    		sumXT += gradX[i][j] * (float)gradT;
		    	sumYT += gradY[i][j] * (float)gradT;
            }
        }

	divisor = ( (sumXX*sumYY) - (sumXY*sumXY) );

    dU = ( ( (sumXY * sumYT) - (sumYY * sumXT) ) /divisor) * gain;
    dV = ( ( (sumXY * sumXT) - (sumXX * sumYT) ) /divisor) * gain;

    resultU += dU;
    resultV += dV;

    	if(resultU != resultU || resultV != resultV || fabs(resultU) > NOISE_TRESHOLD || fabs(resultV) > NOISE_TRESHOLD){
		    resultU = 0.0f;
			resultV = 0.0f;
            break;
	    }
        if(fabs(divisor) < noiseReduction){
            resultU = 0.0f;
            resultV = 0.0f;
        }

		// Early stopping
        if(length((float2)(dU, dV)) < 0.04f){ 
            break;
        }

    } // Main iteration END

	// Output write
    if(lastLevel == 1){
        outputFlowU[index] = resultU;
        outputFlowV[index] = resultV;
    }
    else{
        int flowX = x / 5;
        int flowY = y / 5;
        resultU *= 2;
        resultV *= 2;

        outputFlowU[(flowX*2)       + (flowY*2*2*inputFlowWidth)] = resultU;
        outputFlowU[(flowX*2 + 1)   + (flowY*2*2*inputFlowWidth)] = resultU;
        outputFlowU[(flowX*2)       + (flowY*2 + 1)*2*inputFlowWidth] = resultU;
        outputFlowU[(flowX*2 + 1)   + (flowY*2 + 1)*2*inputFlowWidth] = resultU;

        outputFlowV[(flowX*2)       + (flowY*4*inputFlowWidth)] = resultV;
        outputFlowV[(flowX*2 + 1)   + (flowY*2*2*inputFlowWidth)] = resultV;
        outputFlowV[(flowX*2)       + (flowY*2 + 1)*2*inputFlowWidth] = resultV;
        outputFlowV[(flowX*2 + 1)   + (flowY*2 + 1)*2*inputFlowWidth] = resultV;
    }
}