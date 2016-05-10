typedef union
{
    struct
    {
        unsigned abyte : 8;
    } as;
    struct
    {
        unsigned color_plane_enable        : 4,    
        video_status_mux                   : 2,    
        not_used                           : 2;
    } as_bfld;
} COLOR_PLANE_ENABLE;

static struct 
{
    union
    {
        struct
        {
            unsigned abyte : 8;
        } as;
        struct
        {
            unsigned index                  : 5,
            not_used                        : 3;
        } as_bfld;
    } address;

    union
    {
        struct
        {
            unsigned abyte : 8;
        } as;
        struct
        {
            unsigned blue: 1,    
            green                              : 1,    
            red                                : 1,    
            secondary_blue                     : 1,    
            secondary_green                    : 1,    
            secondary_red                      : 1,    
            not_used                           : 2;    
        } as_bfld;
    } palette[16];

	COLOR_PLANE_ENABLE color_plane_enable;
    union
    {
        struct
        {
            unsigned abyte : 8;
        } as;
        struct
        {
            unsigned horizontal_pel_panning             : 4,    
            not_used                           : 4;
        } as_bfld;
    } horizontal_pel_panning;
} attribute_controller;

typedef struct { void (*change_plane_mask) (int arg1); } FUNCS;

extern	struct	EGA_GLOBALS {
	int			actual_offset_per_line;	
	int			prim_font_index;	
	int			sec_font_index;		
	int			underline_start;	
	int			plane_mask;
} EGA_GRAPH;

void use_bitfield_union(FUNCS* funcs)
{
	unsigned tmp;
	struct
    {
        unsigned value : 8;
    } newish;

	if (attribute_controller.color_plane_enable.as_bfld.color_plane_enable !=
                ((COLOR_PLANE_ENABLE*)&newish)->as_bfld.color_plane_enable)
        // this line gets a false positive if anonymous CSUs are not properly handled.
		EGA_GRAPH.plane_mask = (((COLOR_PLANE_ENABLE*)&newish)->as_bfld.color_plane_enable); 
}

