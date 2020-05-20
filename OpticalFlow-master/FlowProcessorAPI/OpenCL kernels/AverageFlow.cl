
__kernel void averageFlow(__global float*U, __global float*V,
						__global float* Uout, __global float* Vout,
						  int width, int height)
 
 

 {
	int index = get_global_id(0);	
	int i = (index)*5;
	
	float avgU = 0.0;
	float avgV = 0.0;

	
	if((i> 2*width+1) && (i<(width*height-1)) && (i%(width)!= width-1) &&( i%(width)!=width-2) &&
	(i%(width)!=0) && (i % (width) != 1))
	{
	
	avgU = 0.04 *(U[i-2] + U[i-1] +U[i]+U[i+1] + U[i+2]+ 
	U[i-width] + U[i-width-1] + U[i-width-2] +U[i-width+1] + U[i-width+2] + 
	U[i-2*width] +U[i-2*width-1] + U[i-2*width-2] +U[i-2*width+1] + U[i-2*width+2]+ 
	U[i+width] +U[i+width-1] + U[i+width-2] +U[i+width+1] + U[i+width+2] +
	U[i+2*width] + 	U[i+2*width-1] + U[i+2*width-2] +	U[i+2*width+1] + U[i+2*width+2]);
	
	
	avgV = 0.04 *(V[i-2] + V[i-1] +V[i]+V[i+1] + V[i+2]+ 
	V[i-width] + V[i-width-1] + V[i-width-2] +V[i-width+1] + V[i-width+2]+
	V[i-2*width] + V[i-2*width-1] + V[i-2*width-2] +V[i-2*width+1] + V[i-2*width+2]+ 
	V[i+width] + V[i+width-1] + V[i+width-2] +V[i+width+1] + V[i+width+2] + 
	V[i+2*width] + V[i+2*width-1] + V[i+2*width-2] +V[i+2*width+1] + V[i+2*width+2]);

	}
	
	Uout[index/5] = avgU;
	Vout[index/5] =avgV;
	}
	