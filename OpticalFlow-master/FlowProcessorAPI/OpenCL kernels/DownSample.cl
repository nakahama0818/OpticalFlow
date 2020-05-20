//**************************************************************************************
//
// This header is an inseparable part of the source code.
// This source code is created by György Richárd Bogár.
// It is mandatory to refer to the source in all uses that continuously contain
// at least 10% of the code or any smaller code snippet which makes it identifiable.
//
//**************************************************************************************

__kernel void downSample(read_only image2d_t input,
						 write_only image2d_t output){ 

	sampler_t sampler = CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST ;
	int x = get_global_id(0);
	int y = get_global_id(1);
	int2 coordinates = (int2)(x, y);

	int4 result = read_imagei(input, sampler, (int2)((2*x)-1, (2*y)-1));

	write_imagei(output, coordinates, result);

}