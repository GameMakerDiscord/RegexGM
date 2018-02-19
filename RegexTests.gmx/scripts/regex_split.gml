///regex_split(regex_id, input, count = -1, start_at = 0)
var arr, split, count;

if(argument_count > 3)
    split = _regex_split_from_count(argument[0], argument[1], argument[2], argument[3]);
else if(argument_count > 2)
    split = _regex_split_count(argument[0], argument[1], argument[2]);
else
    split = _regex_split(argument[0], argument[1]);
    
if(split == -2)
    return noone;
    
count = _split_get_count(split);

if(count < 4) {
    for(var i = 0; i < count; i++)
        arr[i] = _split_get_index(split, i);
} else {
    var buffer = buffer_create(_split_get_size(split), buffer_fixed, 1);
    _split_fill_buffer(split, buffer_get_address(buffer));
    for(var i = 0; i < count; i++)
        arr[i] = buffer_read(buffer, buffer_string);
    buffer_delete(buffer);
}

regex_destroy_id(split);
return arr;
