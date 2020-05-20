__kernel void localGlobalFlow_J(read_only image2d_t input1, read_only image2d_t input2, __global float* J00, __global float* J01,
__global float* J02, __global float* J11,	__global float* J12, __global float* J22, int width, int height)



{	sampler_t sampler = CLK_NORMALIZED_COORDS_FALSE | CLK_FILTER_NEAREST | CLK_ADDRESS_CLAMP_TO_EDGE;
		

		int i = get_global_id(0);
		int x = (i % width);
		int y = (i / width);
		
		float gradX =0.0f;
		float gradY =0.0f;
		float gradT =0.0f;



		if((i > width)&&(i < ((height*width-1) - width))&&(i%(width)!=(width-1))&&((i%width)!=0))
	
		{
		
		
		gradX = 0.25 * (read_imagei(input1, sampler, (int2)(x+1, y)).x - read_imagei(input1, sampler, (int2)(x, y)).x + read_imagei(input1, sampler, (int2)(x+1, y+1)).x
		-read_imagei(input1, sampler, (int2)(x, y+1)).x + read_imagei(input2, sampler, (int2)(x+1, y)).x - read_imagei(input2, sampler, (int2)(x, y)).x
		+read_imagei(input2, sampler, (int2)(x+1, y+1)).x - read_imagei(input2, sampler, (int2)(x, y+1)).x);
		
	gradY = 0.25 * (read_imagei(input1, sampler, (int2)(x, y+1)).x - read_imagei(input1, sampler, (int2)(x, y)).x + read_imagei(input1, sampler, (int2)(x+1, y+1)).x
		-read_imagei(input1, sampler, (int2)(x+1, y)).x + read_imagei(input2, sampler, (int2)(x, y+1)).x - read_imagei(input2, sampler, (int2)(x, y)).x
		+read_imagei(input2, sampler, (int2)(x+1, y+1)).x - read_imagei(input2, sampler, (int2)(x+1, y)).x);
		
	
		
		gradT = 0.25 * (read_imagei(input2, sampler, (int2)(x, y)).x - read_imagei(input1, sampler, (int2)(x, y)).x + read_imagei(input2, sampler, (int2)(x+1, y)).x
		-read_imagei(input1, sampler, (int2)(x+1, y)).x + read_imagei(input2, sampler, (int2)(x, y+1)).x - read_imagei(input1, sampler, (int2)(x, y+1)).x
		+read_imagei(input2, sampler, (int2)(x+1, y+1)).x - read_imagei(input1, sampler, (int2)(x+1, y+1)).x);	
		
		}
			
		J00[i] = gradX *gradX;
		J01[i] = gradX *gradY;
		J02[i] = gradX *gradT;
		
		J11[i] = gradY *gradY;
		J12[i] = gradY*gradT;
		
		
		J22[i]= gradT*gradT;
		
		
}