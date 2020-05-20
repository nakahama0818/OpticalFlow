__kernel void localGlobalFlow3(__global float* J00, __global float* J01,
__global float* J02, __global float* J11,	__global float* J12, __global float* J22,
__global float* Uprev, __global float* Vprev, __global float* Unext, __global float* Vnext, float alpha, float threshold, int width, int height)

{ 

	int i = get_global_id(0);

		float U =0.0;
		float V =0.0;
		
		
		if((i > width)&&(i < ((height*width-1) - width))&&(i%(width)!=(width-1))&&((i%width)!=0))
	
		{
		
	float avgU = (float)(0.1666 * (Uprev[i-1]+Uprev[i+width]+Uprev[i+1]+Uprev[i-width])
				+0.0833*(Uprev[i-width-1]+Uprev[i+width-1]+Uprev[i+width+1]+Uprev[i-width+1]));
		float avgV =(float) (0.1666 * (Vprev[i-1]+Vprev[i+width]+Vprev[i+1]+Vprev[i-width])
				+0.0833*(Vprev[i-width-1]+Vprev[i+width-1]+Vprev[i+width+1]+Vprev[i-width+1]));	
		
		float A1 = J00[i]* avgU + J01[i] * avgV + J02[i];
		float A2 = J01[i] * avgU + J11[i] * avgV + J12[i];
		float B = alpha * alpha + J00[i] + J11[i];


		
		U = avgU - (A1/B);
		V = avgV - (A2/B);
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