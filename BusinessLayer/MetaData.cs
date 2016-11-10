using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    class RoomMetaData
    {
        [Required]
        [Display(Name ="Hostel Block Number")]
        public int hostelBlockNumber { get; set; }

        [Required]
        [Display(Name ="Room Number")]
        [Range(0, double.MaxValue)]
        public int roomNumber { get; set; }

        [Required]
        [Display(Name ="Room Type")]
        public Nullable<int> roomType { get; set; }

        [Required]
        [Display(Name ="Maximum Occupancy")]
        [Range(1, 10)]
        public int maxOccupancy { get; set; }
    }

    [MetadataType(typeof(RoomMetaData))]
    public partial class Room
    {

    }
}
