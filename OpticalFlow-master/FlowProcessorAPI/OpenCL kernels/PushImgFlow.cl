__kernel void pushImgFlow(read_only image2d_t input, write_only image2d_t output, __global float* U, __global float* V, int width, int height)

{

	sampler_t sampler = CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST;

	int x = get_global_id(0);	
	int y = get_global_id(1);
	
	int u = U[x+y*width];
	int v = V[x+y*width];

int4 pixel = (int4)(0,0,0,0);

	if(((x+u) < width)&& ((y +v) < height))
	{ 
		
					pixel = read_imagei(input, sampler, (int2)(x, y));
					write_imagei(output, (int2)(x+u, y+v), pixel);
				
		
	}
			
}