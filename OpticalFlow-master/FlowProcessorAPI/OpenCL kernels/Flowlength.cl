
__kernel void flowlength(__global float*U, __global float*V,  __global float* length, int width, int height)
{

	
	
	int i = get_global_id(0);	


	float u = U[i];
	float v = V[i];


    length[i] = sqrt(u*u+v*v);

	
	

}