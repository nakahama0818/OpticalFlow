//**************************************************************************************
//
// This header is an inseparable part of the source code.
// This source code is created by György Richárd Bogár.
// It is mandatory to refer to the source in all uses that continuously contain
// at least 10% of the code or any smaller code snippet which makes it identifiable.
//
//**************************************************************************************

__kernel void modifyFlowImage(read_only image2d_t input,
							  write_only image2d_t output,
							  __global float* flowU,
							  __global float*flowV,
							  int width,
							  int height){ 

	sampler_t sampler = CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST ;

	int index = get_global_id(0);	
	int x = (index % width) * 5;
	int y = (index / width) * 5;
	int2 coordinates = (int2)(x,y);

	int u = (int)flowU[index];
	int v = (int)flowV[index];

	int newX = x + u;
	int newY = y +v;

	if(newX < (width * 5) && newY < (height * 5)){ 
		for(int i = 0; i < 5; i++){ 
			for(int j = 0; j < 5; j++){ 
				if(u != 0 || v != 0){ 
					int4 pixel = read_imagei(input, sampler, (int2)(x+j, y+i));
					write_imagei(output, (int2)(x+j+u, y+i+v), pixel);
				}
			}
		}
	}

}