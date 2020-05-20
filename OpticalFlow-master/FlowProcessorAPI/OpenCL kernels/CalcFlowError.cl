

__kernel void calcFlowError(__global float* Uprev, __global float* Vprev,  __global float* Unext , __global float* Vnext, __global float* flowError, int width, int height)

{

	
	
	int i = get_global_id(0);
	

float err = 0.0;
	
	 err = (Unext[i]-Uprev[i])*(Unext[i]-Uprev[i]) + (Vnext[i]-Vprev[i])*(Vnext[i]-Vprev[i]);
	
	
	flowError[i] = sqrt(err);
	
	
	
	}