//**************************************************************************************
//
// This header is an inseparable part of the source code.
// This source code is created by György Richárd Bogár.
// It is mandatory to refer to the source in all uses that continuously contain
// at least 10% of the code or any smaller code snippet which makes it identifiable.
//
//**************************************************************************************

__kernel void gaussianBlur1(read_only image2d_t input,
							write_only image2d_t output,
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

	int4 currentPixel;
	float resultPixel = 0;

		for(int i = 0; i < 5; i++){ 
			for(int j = 0; j < 5; j++){ 
				currentPixel = read_imagei(input, sampler, (int2)((x-2+j), (y-2+i)));
				resultPixel += ((float)(currentPixel.x) * gaussWindow[i][j]);
			}
		}

	int4 result = (int4)(resultPixel, resultPixel, resultPixel, 255);
	write_imagei(output, coordinates, result);

}