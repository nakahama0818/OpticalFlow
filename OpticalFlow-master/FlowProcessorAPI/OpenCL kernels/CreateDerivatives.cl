__kernel void createDerivatives(read_only image2d_t input, write_only image2d_t output, int width, int height){
    sampler_t sampler = CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST ;
    int x = get_global_id(0);
    int y = get_global_id(1);
    int2 coordinates = (int2)(x,y);

    int4 result;
    int gradX = 0;
    int gradY = 0;

    if(x > 0 && x < (width - 1)){
        gradX = ( read_imagei(input, sampler, (int2)(x+1, y)).x - read_imagei(input, sampler, (int2)(x-1, y)).x );
        gradX /= 2;
    }
    else if(x == 0){
        gradX = ( read_imagei(input, sampler, (int2)(x+1, y)).x - read_imagei(input, sampler, (int2)(x, y)).x );
    }
    else{
        gradX = ( read_imagei(input, sampler, (int2)(x, y)).x - read_imagei(input, sampler, (int2)(x-1, y)).x );
    }

    if(y > 0 && y < (height -1)){
        gradY = (read_imagei(input, sampler, (int2)(x, y+1)).x - read_imagei(input, sampler, (int2)(x, y-1)).y);
        gradY /= 2;
    }
    else if(y == 0){
        gradY = (read_imagei(input, sampler, (int2)(x, y+1)).x - read_imagei(input, sampler, (int2)(x, y)).y);
    }
    else{
        gradY = (read_imagei(input, sampler, (int2)(x, y)).x - read_imagei(input, sampler, (int2)(x, y-1)).y);
    }

    result = (int4)(gradX, gradY, 0, 255);
    write_imagei(output, coordinates, result);
}