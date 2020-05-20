__kernel void angularError(__global float* Uest, __global float* Vest,  __global float* Ugt , __global float* Vgt, __global float* flowError, int width, int height)

{
	
	int i = get_global_id(0);
	

	float err = 0.0;
	float A = Uest[i]*Ugt[i] + Vest[i]*Vgt[i] +1;
	float B = sqrt(Uest[i]*Uest[i] +Vest[i]*Vest[i]+1)*sqrt(Ugt[i]*Ugt[i] +Vgt[i]*Vgt[i]+1);
	
	
	 err =A/B;
	 

	
	
	flowError[i] = acos(err);
	

	
	}