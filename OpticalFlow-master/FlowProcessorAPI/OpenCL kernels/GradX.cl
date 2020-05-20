//**************************************************************************************
//
// This header is an inseparable part of the source code.
// This source code is created by György Richárd Bogár.
// It is mandatory to refer to the source in all uses that continuously contain
// at least 10% of the code or any smaller code snippet which makes it identifiable.
//
//**************************************************************************************

void __kernel gradX(read_only image2d_t input,
					write_only image2d_t output,
					int width,
					int height){ 

	sampler_t sampler = CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST ;
	
	int2 coordinates = (int2)(get_global_id(0), get_global_id(1));
	int x = get_global_id(0);
	int y = get_global_id(1);

	int edge[3][3] = {{-1, 0, 1},
					  {-1, 0, 1},
					  {-1, 0, 1}};

	int4 result = (int4)(0,0,255,255);
	int monoResult = 0;

	if(y > 0 && x > 0 && x < width-1 && y < height-1)
	{
		monoResult = read_imagei(input, sampler, (int2)(x+1,y-1)).x - read_imagei(input, sampler, (int2)(x-1,y-1)).x;
		monoResult = read_imagei(input, sampler, (int2)(x+1,y)).x - read_imagei(input, sampler, (int2)(x-1,y)).x;
		monoResult = read_imagei(input, sampler, (int2)(x+1,y+1)).x - read_imagei(input, sampler, (int2)(x-1,y+1)).x;
		monoResult /= 2;
	}
	result = (int4)(monoResult, monoResult, monoResult, 255);
	
	write_imagei(output, coordinates, result);

 }