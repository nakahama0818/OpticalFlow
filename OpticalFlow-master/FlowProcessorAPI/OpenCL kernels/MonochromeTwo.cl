//**************************************************************************************
//
// This header is an inseparable part of the source code.
// This source code is created by György Richárd Bogár.
// It is mandatory to refer to the source in all uses that continuously contain
// at least 10% of the code or any smaller code snippet which makes it identifiable.
//
//**************************************************************************************

__kernel void monochromeTwo(read_only image2d_t leftInput,
							read_only image2d_t rightInput,
							write_only image2d_t leftOutput,
							write_only image2d_t rightOutput,
							int width,
							int height){ 

 sampler_t sampler = CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST ;
	 
 int2 coordinates = (int2)(get_global_id(0), get_global_id(1));


 int4 leftPixel = read_imagei(leftInput, sampler, coordinates);
 int4 rightPixel = read_imagei(rightInput, sampler, coordinates);

 int leftBrightness = (int)(0.2126*leftPixel.x + 0.7152*leftPixel.y + 0.0722*leftPixel.z);
 int rightBrightness = (int)(0.2126*rightPixel.x + 0.7152*rightPixel.y + 0.0722*rightPixel.z);
 
 leftPixel.x = leftPixel.y = leftPixel.z = leftBrightness;
 rightPixel.x = rightPixel.y = rightPixel.z = rightBrightness;

 write_imagei(leftOutput, coordinates, leftPixel);
 write_imagei(rightOutput, coordinates, rightPixel);

}