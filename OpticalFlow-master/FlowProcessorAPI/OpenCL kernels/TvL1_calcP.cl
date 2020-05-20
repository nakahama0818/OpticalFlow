__kernel void calcP( __global float* U,  __global float* V ,  float tau,  float theta,
__global float* P11prev,__global float* P12prev,__global float* P21prev,__global float* P22prev,__global float* P11next,__global float* P12next,__global float* P21next,__global float* P22next, int width, int height)


{
int i = get_global_id(0);


	float gradxU = 0.0;
	float gradyU = 0.0;
	float gradxV = 0.0;
	float gradyV = 0.0;

	if((((i%width)==0))||((i%width > 0)&&(i%width <(width-1))))
	{
	
	gradxU = U[i+1]-U[i];
	gradxV = V[i+1]-V[i];
	
	}
	
	if((i%width == (width-1)))
	{
	
	gradxU = 0.0;
	gradxV = 0.0;
	}
	
	//---------------------------------
	
	if((i<width)||((i>(width-1))&&((height*width)-1-width)))
	{
	
	gradyV = V[i+width]-V[i];
	gradyU = U[i+width]-U[i];
	}
	
	if(i>((height*width)-width-1))
	{
	
	gradyV = 0.0;
	gradyU = 0.0;
	
	}
	
	float num11 = P11prev[i] +(tau/theta)* gradxU;
	float denum11 = 1 + sqrt(gradxU*gradxU + gradyU*gradyU);
	float num12 = P12prev[i] +(tau/theta)* gradyU;
	float denum12 = denum11;
	float num21 = P21prev[i] +(tau/theta)* gradxV;
	float denum21 = 1 + sqrt(gradxV*gradxV + gradyV*gradyV);
	float num22 = P22prev[i] +(tau/theta)* gradyV;
	float denum22 = denum21;

    P11next[i] = num11/denum11;
	P12next[i] = num12/denum12;
	P21next[i] = num21/denum21;
	P22next[i] = num22/denum22;
	

	
}