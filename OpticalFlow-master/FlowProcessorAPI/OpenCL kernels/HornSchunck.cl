

__constant sampler_t sampler = CLK_NORMALIZED_COORDS_FALSE | CLK_FILTER_NEAREST | CLK_ADDRESS_CLAMP_TO_EDGE;

__kernel void hornSchunck(__read_only image2d_t in1, __read_only image2d_t in2, __global float* Uprev, __global float* Vprev,
        __global float* Unext, __global float* Vnext, float alpha, float threshold, int width, int height)

 {
		int i = get_global_id(0);
		int x = (i % width);
		int y = (i / width);
			
		int2 coordinates = (int2)(x,y);
		float gradX = 0.0f;
		float gradY = 0.0f;
		float gradT = 0.0f;
		
		float A;
		float B;
		
		float avgU =0.0;
		float avgV =0.0;
		
		float U =0.0;
		float V =0.0;
if((i > width)&&(i < ((height*width-1) - width))&&(i%(width)!=(width-1))&&((i%width)!=0))
	
	{
	
		avgU = (float)(0.1666 * (Uprev[i-1]+Uprev[i+width]+Uprev[i+1]+Uprev[i-width])
				+0.0833*(Uprev[i-width-1]+Uprev[i+width-1]+Uprev[i+width+1]+Uprev[i-width+1]));
		avgV =(float) (0.1666 * (Vprev[i-1]+Vprev[i+width]+Vprev[i+1]+Vprev[i-width])
				+0.0833*(Vprev[i-width-1]+Vprev[i+width-1]+Vprev[i+width+1]+Vprev[i-width+1]));			
	
		
		
		gradX = 0.25 * (read_imagei(in1, sampler, (int2)(x+1, y)).x - read_imagei(in1, sampler, (int2)(x, y)).x + read_imagei(in1, sampler, (int2)(x+1, y+1)).x
		-read_imagei(in1, sampler, (int2)(x, y+1)).x + read_imagei(in2, sampler, (int2)(x+1, y)).x - read_imagei(in2, sampler, (int2)(x, y)).x
		+read_imagei(in2, sampler, (int2)(x+1, y+1)).x - read_imagei(in2, sampler, (int2)(x, y+1)).x);
		
		gradY = 0.25 * (read_imagei(in1, sampler, (int2)(x, y+1)).x - read_imagei(in1, sampler, (int2)(x, y)).x + read_imagei(in1, sampler, (int2)(x+1, y+1)).x
		-read_imagei(in1, sampler, (int2)(x+1, y)).x + read_imagei(in2, sampler, (int2)(x, y+1)).x - read_imagei(in2, sampler, (int2)(x, y)).x
		+read_imagei(in2, sampler, (int2)(x+1, y+1)).x - read_imagei(in2, sampler, (int2)(x+1, y)).x);
		
		
		gradT = 0.25 * (read_imagei(in2, sampler, (int2)(x, y)).x - read_imagei(in1, sampler, (int2)(x, y)).x + read_imagei(in2, sampler, (int2)(x+1, y)).x
		-read_imagei(in1, sampler, (int2)(x+1, y)).x + read_imagei(in2, sampler, (int2)(x, y+1)).x - read_imagei(in1, sampler, (int2)(x, y+1)).x
		+read_imagei(in2, sampler, (int2)(x+1, y+1)).x - read_imagei(in1, sampler, (int2)(x+1, y+1)).x);
		
		
		
		A = gradX * avgU + gradY * avgV + gradT;
		B = alpha * alpha + gradX * gradX + gradY *gradY;

		
		
		
		U = avgU - gradX*(A/B);
		V = avgV - gradY*(A/B);
		
		
		}
		
		
		
		if(!(sqrt((U*U+V*V)<threshold)))
		{
			Unext[i] = U;
			Vnext[i] = V;
		}
		else
		{
			Unext[i] = 0.0;
			Vnext[i] = 0.0;
		}
 
 }
 