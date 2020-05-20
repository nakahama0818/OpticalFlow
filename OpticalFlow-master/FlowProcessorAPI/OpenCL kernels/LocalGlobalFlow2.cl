

__kernel void localGlobalFlow2( int kernelsize, float sigma, __global float* J00_prev, __global float* J01_prev,__global float* J02_prev, __global float* J11_prev,
	__global float* J12_prev, __global float* J22_prev,__global float* J00_next, __global float* J01_next,
	__global float* J02_next, __global float* J11_next,__global float* J12_next, __global float* J22_next, int width, int height)
{

		int i = get_global_id(0);
		int x = (i%width);
		int y = (i/width);	
		float U = 0.0f;
		float V = 0.0f;
		float currentJ00 = 0.0;
		float currentJ01 = 0.0;
		float currentJ02 = 0.0;
		float currentJ11 = 0.0;
		float currentJ12 = 0.0;
		float currentJ22 = 0.0;
		
		float maskValue =  0.0;
		

	if(kernelsize ==3)
		{
		for(int k = -1; k < 2 ; k++)
		{ 
			for(int j = -1; j < 2; j++)
			{ 
					
				maskValue = 1/(2*3.14*sigma*sigma) * exp((-(k*k+j*j))/(2*sigma*sigma));
						
				 currentJ00 += J00_prev[ (y+k)*width +x+j]*maskValue;
				 currentJ01 += J01_prev[ (y+k)*width +x+j]*maskValue;
				 currentJ02 += J02_prev[ (y+k)*width +x+j]*maskValue;
				 currentJ11 += J11_prev[ (y+k)*width +x+j]*maskValue;
				 currentJ12 += J12_prev[ (y+k)*width +x+j]*maskValue;
				 currentJ22 += J22_prev[ (y+k)*width +x+j]*maskValue;
				
		
			}
		
		
		  }
		}
		
		if(kernelsize ==5)
		{
		for(int k = -2; k < 3 ; k++)
		{ 
			for(int j = -2; j < 3; j++)
			{ 
					
				maskValue = 1/(2*3.14*sigma*sigma) * exp((-(k*k+j*j))/(2*sigma*sigma));
							
				 currentJ00 += J00_prev[ (y+k)*width +x+j]*maskValue;
				 currentJ01 += J01_prev[ (y+k)*width +x+j]*maskValue;
				 currentJ02 += J02_prev[ (y+k)*width +x+j]*maskValue;
				 currentJ11 += J11_prev[ (y+k)*width +x+j]*maskValue;
				 currentJ12 += J12_prev[ (y+k)*width +x+j]*maskValue;
				 currentJ22 += J22_prev[ (y+k)*width +x+j]*maskValue;
				
		
			}
		
		
		  }
		
		}
		
		if(kernelsize ==7)
		{
		for(int k = -3; k < 4 ; k++)
		{ 
			for(int j = -3; j < 4; j++)
			{ 
					
				maskValue = 1/(2*3.14*sigma*sigma) * exp((-(k*k+j*j))/(2*sigma*sigma));
							
				 currentJ00 += J00_prev[ (y+k)*width +x+j]*maskValue;
				 currentJ01 += J01_prev[ (y+k)*width +x+j]*maskValue;
				 currentJ02 += J02_prev[ (y+k)*width +x+j]*maskValue;
				 currentJ11 += J11_prev[ (y+k)*width +x+j]*maskValue;
				 currentJ12 += J12_prev[ (y+k)*width +x+j]*maskValue;
				 currentJ22 += J22_prev[ (y+k)*width +x+j]*maskValue;
				
		
			}
		
		
		  }
		}
		
		J00_next[i] = currentJ00;
		J01_next[i] = currentJ01;
		J02_next[i] = currentJ02;
		J11_next[i] = currentJ11;
		J12_next[i] = currentJ12;
		J22_next[i] = currentJ22;
		
		
		

}