//**************************************************************************************
//
// This header is an inseparable part of the source code.
// This source code is created by György Richárd Bogár.
// It is mandatory to refer to the source in all uses that continuously contain
// at least 10% of the code or any smaller code snippet which makes it identifiable.
//
//**************************************************************************************

__kernel void flow(image2d_t read_only left,
				   image2d_t read_only right,
				   __global float* flowU,
				   __global float* flowV, 
				   int width, 
				   int height, 
				   float noiseReduction){ 

	sampler_t sampler = CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST ;
	
	int index = get_global_id(0);	
	int x = (index % width) * 5;
	int y = (index / width) * 5;
	int2 coordinates = {x, y};

	float sumXX = 0.0f;
	float sumYY = 0.0f;
	float sumXY = 0.0f;
	float sumXT = 0.0f;
	float sumYT = 0.0f;

	int gradX[5][5];
	int gradY[5][5];

	int gradT[5][5];
	
	for(int i = 0; i < 5; i++){ 
		for(int j = 0; j < 5; j++){ 
			gradX[i][j] = (read_imagei(left, sampler, (int2)(x+j+1, y+i)).x - read_imagei(left, sampler, (int2)(x+j-1, y+i)).x);
			gradX[i][j] /= 2;

			gradY[i][j] = (read_imagei(left, sampler, (int2)(x+j, y+i+1)).x - read_imagei(left, sampler, (int2)(x+j, y+i-1)).x);
			gradY[i][j] /= 2;

			gradT[i][j] = (read_imagei(right, sampler, (int2)(x+j, y+i)).x - read_imagei(left, sampler, (int2)(x+j, y+i)).x);

			sumXX += (float)gradX[i][j] * (float)gradX[i][j];
			sumYY += (float)gradY[i][j] * (float)gradY[i][j];
			sumXY += (float)gradX[i][j] * (float)gradY[i][j];
			sumXT += (float)gradX[i][j] * (float)gradT[i][j];
			sumYT += (float)gradY[i][j] * (float)gradT[i][j];
		}
	}

	float divisor = ( (sumXX * sumYY) - (sumXY * sumXY) );

	float resultU = ( ( (sumXY * sumYT) - (sumYY * sumXT) ) / divisor );
	float resultV = ( ( (sumXY * sumXT) - (sumXX * sumYT) ) / divisor );

	if(resultU != resultU || resultV != resultV || fabs(divisor) < noiseReduction){ 
		resultU = 0.0f;
		resultV = 0.0f;
	}

	flowU[index] = resultU;
	flowV[index] = resultV;
}