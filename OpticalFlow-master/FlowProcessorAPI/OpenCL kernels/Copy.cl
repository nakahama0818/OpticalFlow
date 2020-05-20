void __kernel copy(read_only image2d_t input, write_only image2d_t output, int width, int height){ 
 
 int2 coordinates = (int2)(get_global_id(0), get_global_id(1));

 sampler_t sampler = CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST ;

 float4 pixel = read_imagef(input, sampler, coordinates);
 float outputpart = (((coordinates.x / width) + (coordinates.y / height)) / 2);
 float4 outputPixel = (float4)(0.0f, 0.0f, 1.0f, 0.0f);
 write_imagef(output, coordinates, outputPixel);
}