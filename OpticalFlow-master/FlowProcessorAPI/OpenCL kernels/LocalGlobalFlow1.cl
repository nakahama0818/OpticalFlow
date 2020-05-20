

__kernel void localGlobalFlow1(read_only image2d_t input1, read_only image2d_t input2,  __global float* Uprev, __global float* Vprev,
 __global float* Unext, __global float* Vnext, float alpha, float threshold,
													int width, int height)
{
		
		sampler_t sampler = CLK_NORMALIZED_COORDS_FALSE | CLK_FILTER_NEAREST | CLK_ADDRESS_CLAMP_TO_EDGE;
		

		int i = get_global_id(0);
		int x = (i % width);
		int y = (i / width);
		
		float gradX =0.0f;
		float gradY =0.0f;
		float gradT =0.0f;
		float U = 0.0f;
		float V = 0.0f;
		
		float J[3][3];
		 
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
		
		
		
		J[0][0] = gradX *gradX;
		J[0][1] = gradX *gradY;
		J[0][2] = gradX *gradT;
		J[1][0] = J[0][1];
		J[1][1] = gradY *gradY;
		J[1][2] = gradY*gradT;
		J[2][0] = J[0][2];
		J[2][1] = J[1][2];
		J[2][2]= gradT*gradT;
		
		

float avgU = (float)(0.1666 * (Uprev[i-1]+Uprev[i+width]+Uprev[i+1]+Uprev[i-width])
				+0.0833*(Uprev[i-width-1]+Uprev[i+width-1]+Uprev[i+width+1]+Uprev[i-width+1]));
		float avgV =(float) (0.1666 * (Vprev[i-1]+Vprev[i+width]+Vprev[i+1]+Vprev[i-width])
				+0.0833*(Vprev[i-width-1]+Vprev[i+width-1]+Vprev[i+width+1]+Vprev[i-width+1]));			
	
		
		
		
		float A = gradX * avgU + gradY * avgV + gradT;
		float B = alpha * alpha + gradX * gradX + gradY *gradY;

		
		
		
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