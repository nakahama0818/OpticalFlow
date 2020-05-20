__kernel void divP_Flow(__global float* rho_c,__global float* Ixd,__global float* Iyd,__global float* U1prev,__global float* U2prev,__global float* U1next,__global float* U2next, float theta, float lambda,
									__global float* grad2,__global float* P11,__global float* P12,__global float* P21,__global float* P22, float threshold, int width, int height)
{ 	
	int i = get_global_id(0);	
	
	float rho = rho_c[i] + Ixd[i] * U1prev[i] +Iyd[i]*U2prev[i];
		
	float V1 =0.0;
	float V2 =0.0;
	
	if(rho < -1*lambda*theta*grad2[i])
	{
		V1 = U1prev[i] + lambda*theta*Ixd[i];
		V2 = U2prev[i] + lambda*theta*Iyd[i];

	}
		
	else if(rho > lambda*theta*grad2[i])
	{
		V1 = U1prev[i] - lambda*theta*Ixd[i];
		V2 = U2prev[i] - lambda*theta*Iyd[i];	
	}
		
	else
	
	{
		V1 = U1prev[i] - rho *(Ixd[i]/grad2[i]);
		V2 = U2prev[i] - rho *(Iyd[i]/grad2[i]);
	}	
	
	float divP11 = 0.0;
	float divP12 = 0.0;
	float divP21 = 0.0;
	float divP22 = 0.0;
	
	float divP1 = 0.0;
	float divP2 = 0.0;
	
	if((i%width)==0)
	{
		
	divP11 = P11[i];
	divP21 = P21[i];
	}

	if((i%width > 0)&&(i%width <(width-1)))
	{
	
	divP11 = P11[i]-P11[i-1];
	divP21 = P21[i]-P21[i-1];
	}
	
	if((i%width ==width-1))
	{
	
	divP11 =-P11[i-1] ;
	divP21 =-P21[i-1] ;
	}
	
	//---------------------------------
	
	if(i <width)
	{
		
	divP12 = P12[i];
	divP22 = P22[i];
	}

	if((i > width-1) &&(i<(height*width)-(width)+1))
	{
	
	divP12 = P12[i]- P12[i-width];
	divP22 = P22[i]- P22[i-width];
	}
	
	if((i>(height*width)-width))
	{
	
	divP12 = -P12[i-width];
	divP22 = -P22[i-width];
	
	}	

	divP1 =divP11 + divP12;
	divP2 =divP21 + divP22;
	
	
	
	float U = V1 + theta * divP1;
	float V = V2 + theta * divP2;
	
	if(!(sqrt((U*U+V*V)<threshold)))
	{
	U1next[i] = U;
	U2next[i] = V;
	}
	
	else
	{
	U1next[i] = 0.0;
	U2next[i] = 0.0;
	
	
	}
	
	
	
	
		
}