//**************************************************************************************
//
// This header is an inseparable part of the source code.
// This source code is created by György Richárd Bogár.
// It is mandatory to refer to the source in all uses that continuously contain
// at least 10% of the code or any smaller code snippet which makes it identifiable.
//
//**************************************************************************************

__kernel void downSampleTwo(read_only image2d_t leftInput,
							read_only image2d_t rightInput,
							write_only image2d_t leftOutput,
							write_only image2d_t rightOutput){ 

	sampler_t sampler = CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST ;
	int x = get_global_id(0);
	int y = get_global_id(1);
	int2 coordinates = (int2)(x, y);

	int2 sampledCoordinates = (int2)((2*x)-1, (2*y)-1);
	int4 leftResult = read_imagei(leftInput, sampler, sampledCoordinates);
	int4 rightResult = read_imagei(rightInput, sampler, sampledCoordinates);

	write_imagei(leftOutput, coordinates, leftResult);
	write_imagei(rightOutput, coordinates, rightResult);

}