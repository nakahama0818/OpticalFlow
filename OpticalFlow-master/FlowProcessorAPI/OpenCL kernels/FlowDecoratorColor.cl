
__kernel void flowDecoratorColor(write_only image2d_t output,
							int width,
							int height,
							__global float* flowU,
							__global float* flowV, float interval, float treshold){

	sampler_t sampler = CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST ;	
	
	int index = get_global_id(0);	
	int x = (index % width);
	int y = (index / width);
	int2 coordinates = (int2)(x,y);

	float u = flowU[index];
	float v = flowV[index];

	float4 green = (float4)(0, 255, 0, 255);
	float4 red = (float4)(0, 0, 255, 255);
	float4 blue = (float4)(255, 0,0,255);
	float4 color = (float4)(0,0,0,255);
	float length = sqrt(u*u+v*v);

	
	
	if((length <(interval + treshold)+treshold)&&length>treshold)
	{

		color = green;
		write_imagef(output, (int2)(x, y), color);

	}
	
	if( ((length >(interval + treshold))&&(length < 2*interval +treshold)) || (length == (interval+treshold)))
	{

		color = blue;
		write_imagef(output, (int2)(x, y), color);

	}
	
	
	
	if(length >2*interval+treshold)
	{

		color = red;
		write_imagef(output, (int2)(x, y), color);

	}

		
		write_imagef(output, (int2)(x, y), color);
	

}