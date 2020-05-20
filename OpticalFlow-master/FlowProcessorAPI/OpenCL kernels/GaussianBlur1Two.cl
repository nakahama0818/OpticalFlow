//**************************************************************************************
//
// This header is an inseparable part of the source code.
// This source code is created by György Richárd Bogár.
// It is mandatory to refer to the source in all uses that continuously contain
// at least 10% of the code or any smaller code snippet which makes it identifiable.
//
//**************************************************************************************

__kernel void gaussianBlur1Two(read_only image2d_t leftInput,
							   read_only image2d_t rightInput,
							   write_only image2d_t leftOutput,
							   write_only image2d_t rightOutput,
							   int width,
							   int height){ 

	sampler_t sampler = CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST ;
	
	int2 coordinates = (int2)(get_global_id(0), get_global_id(1));
	int x = get_global_id(0);
	int y = get_global_id(1);

	float gaussWindow[5][5] = {{0.0039, 0.0156, 0.0234, 0.0156, 0.0039 },
							   {0.0156, 0.0625, 0.0937, 0.0625, 0.0156 },
							   {0.0234, 0.0937, 0.1406, 0.0937, 0.0234 },
							   {0.0156, 0.0625, 0.0937, 0.0625, 0.0156 },
							   {0.0039, 0.0156, 0.0234, 0.0156, 0.0039 }}; 

	int4 currentLeftPixel;
	int4 currentRightPixel;
	float resultLeftPixel = 0;
	float resultRightPixel = 0;

		for(int i = 0; i < 5; i++){ 
			for(int j = 0; j < 5; j++){ 
				currentLeftPixel = read_imagei(leftInput, sampler, (int2)((x-2+j), (y-2+i)));
				currentRightPixel = read_imagei(rightInput, sampler, (int2)((x-2+j), (y-2+i)));
				resultLeftPixel += ((float)(currentLeftPixel.x) * gaussWindow[i][j]);
				resultRightPixel += ((float)(currentRightPixel.x) * gaussWindow[i][j]);
			}
		}

	int4 leftResult = (int4)(resultLeftPixel, resultLeftPixel, resultLeftPixel, 255);
	int4 rightResult = (int4)(resultRightPixel, resultRightPixel, resultRightPixel, 255);
	write_imagei(leftOutput, coordinates, leftResult);
	write_imagei(rightOutput, coordinates, rightResult);

}