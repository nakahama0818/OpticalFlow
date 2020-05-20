__kernel void gradRho(read_only image2d_t input0, read_only image2d_t input1d, __global float* U, __global float* V,
									 __global float* gradX, __global float* gradY, __global float* grad2,  __global float* rho_c, int width, int height)
{ 	
	int i = get_global_id(0);	
	int x = (i % width);
	int y = (i / width);

	sampler_t sampler = CLK_NORMALIZED_COORDS_FALSE | CLK_FILTER_LINEAR | CLK_ADDRESS_CLAMP_TO_EDGE;

	
float gradx = 0.25 * (read_imagei(input0, sampler, (int2)(x+1, y)).x - read_imagei(input0, sampler, (int2)(x, y)).x + read_imagei(input0, sampler, (int2)(x+1, y+1)).x
		-read_imagei(input0, sampler, (int2)(x, y+1)).x + read_imagei(input1d, sampler, (int2)(x+1, y)).x - read_imagei(input1d, sampler, (int2)(x, y)).x
		+read_imagei(input1d, sampler, (int2)(x+1, y+1)).x - read_imagei(input1d, sampler, (int2)(x, y+1)).x);
		
float grady = 0.25 * (read_imagei(input0, sampler, (int2)(x, y+1)).x - read_imagei(input0, sampler, (int2)(x, y)).x + read_imagei(input0, sampler, (int2)(x+1, y+1)).x
		-read_imagei(input0, sampler, (int2)(x+1, y)).x + read_imagei(input1d, sampler, (int2)(x, y+1)).x - read_imagei(input1d, sampler, (int2)(x, y)).x
		+read_imagei(input1d, sampler, (int2)(x+1, y+1)).x - read_imagei(input1d, sampler, (int2)(x+1, y)).x);
	
	gradX[i] = gradx;
	gradY[i] = grady;
	grad2[i] = (gradx *gradx) + (grady * grady);
	rho_c[i] = (read_imagei(input1d, sampler, (int2)(x, y)).x)-1*(gradx*U[i]+ grady*V[i]) - (read_imagei(input0, sampler, (int2)(x, y)).x);
	
	
	
	
	}
