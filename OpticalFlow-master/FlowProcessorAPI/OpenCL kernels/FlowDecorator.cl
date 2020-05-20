//**************************************************************************************
//
// This header is an inseparable part of the source code.
// This source code is created by György Richárd Bogár.
// It is mandatory to refer to the source in all uses that continuously contain
// at least 10% of the code or any smaller code snippet which makes it identifiable.
//
//**************************************************************************************

__kernel void flowDecorator(write_only image2d_t output,
							int width,
							int height,
							__global float* flowU,
							__global float* flowV){

	sampler_t sampler = CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST ;	
	
	int index = get_global_id(0);	
	int x = (index % width) * 5;
	int y = (index / width) * 5;
	int2 coordinates = (int2)(x,y);

	int u = (int)(flowU[index]);
	int v = (int)(flowV[index]);

	int4 green = (int4)(0, 255, 0, 255);
	int4 red = (int4)(0, 0, 255, 255);

	if((v != 0 || u != 0) && (x+u) < width*5 && (y+v) < height*5 && (x+u) > 0 && (y+v) > 0){
		int xMovement = 0;
		int yMovement = 0;
		if(v > 0){
			yMovement = 1;
		}
		else if(v < 0){ 
			yMovement = -1;
		}
		if(u > 0){ 
			xMovement = 1;
		}
		else if(u < 0){ 
			xMovement = -1;
		}
	
		bool xReady = false;
		bool yReady = false;
		int xMoves = 0;
		int yMoves = 0;

		while(!xReady || !yReady){ 
			write_imagei(output, (int2)(x+xMoves, y+yMoves), green);
			if(xMoves == u){
				xReady = true;
			}
			else{ 
				xMoves += xMovement;
			}
			if(yMoves == v){ 
				yReady = true;
			}
			else{ 
				yMoves += yMovement;
			}
		}


		write_imagei(output, (int2)(x, y), red);
		
	}

}