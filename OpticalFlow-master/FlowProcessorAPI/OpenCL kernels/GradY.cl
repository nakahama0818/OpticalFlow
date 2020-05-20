void __kernel gradY(read_only image2d_t input, write_only image2d_t output, int width, int height){ 
	sampler_t sampler = CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST ;
	
	int2 coordinates = (int2)(get_global_id(0), get_global_id(1));
	int x = get_global_id(0);
	int y = get_global_id(1);

	int edge[3][3] = {{-1, -1, -1},
					  {0,   0,  0},
					  {1,   1,  1}};

	int4 result = (int4)(0,0,0,0);

	int i;
	int j;
	if(x > 1 && y > 1 && x < (width-2) && y < (height -2)){
		for(i = -1; i <= 1; i++){
			for(j = -1; j <= 1; j++){
				result += edge[i+1][j+1] * read_imagei(input, sampler, (int2)(x+j, y+i));
			}
		}
	}
	result.w = 255;

	write_imagei(output, coordinates, result);

 }