//**************************************************************************************
//
// This header is an inseparable part of the source code.
// This source code is created by György Richárd Bogár.
// It is mandatory to refer to the source in all uses that continuously contain
// at least 10% of the code or any smaller code snippet which makes it identifiable.
//
//**************************************************************************************

void __kernel monochrome(read_only image2d_t input,
						 write_only image2d_t output,
						 int width,
						 int height){ 
 
 int2 coordinates = (int2)(get_global_id(0), get_global_id(1));

 sampler_t sampler = CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST ;

 int4 pixel = read_imagei(input, sampler, coordinates);

 int brightness = (int)(0.2126*pixel.z + 0.7152*pixel.y + 0.0722*pixel.x);
 pixel.x = pixel.y = pixel.z = brightness;

 write_imagei(output, coordinates, pixel);

}