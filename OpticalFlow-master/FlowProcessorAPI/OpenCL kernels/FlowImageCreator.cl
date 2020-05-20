//**************************************************************************************
//
// This header is an inseparable part of the source code.
// This source code is created by György Richárd Bogár.
// It is mandatory to refer to the source in all uses that continuously contain
// at least 10% of the code or any smaller code snippet which makes it identifiable.
//
//**************************************************************************************

__kernel void flowImageCreator(__global uint16* output,
							   __global float* flow,
							   int flowWidth,
							   int flowHeight){ 

	int x = get_global_id(0);
	int y = get_global_id(1);
    int2 coordinates = (int2)(x, y);

    if(x % 3 == 0){
        output[x+y*flowWidth] = 255;
    }

}