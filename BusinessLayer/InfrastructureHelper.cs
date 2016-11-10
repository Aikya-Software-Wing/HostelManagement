using HostelManagement.Areas.Administration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    /// <summary>
    /// A class to manage college infrastructure
    /// </summary>
    public class InfrastructureHelper
    {
        HostelManagementEntities1 db = new HostelManagementEntities1();

        /// <summary>
        /// Method to add a new hostel block
        /// </summary>
        /// <param name="userInput">the form filled by user</param>
        /// <returns></returns>
        public string AddHostel(AddHostelViewModel userInput)
        {
            // if the hostel block number is already present in the database
            if (db.Hostels.Where(x => x.blockNumber == userInput.blockNumber).Count() > 0)
            {
                return "Can not add, hostel block number already exsists!";
            }

            // add the hostel 
            db.Hostels.Add(new Hostel
            {
                blockNumber = userInput.blockNumber,
                occupantType = userInput.occupantType,
                type = userInput.type
            });
            db.SaveChanges();

            return "Success!";
        }

        /// <summary>
        /// Method to get all hostel blocks
        /// </summary>
        /// <returns>a list of hostel blocks</returns>
        public List<Hostel> GetHostelBlocks()
        {
            return db.Hostels.ToList();
        }

        /// <summary>
        /// Method to get all room types
        /// </summary>
        /// <returns>a list of room types</returns>
        public List<RoomType> GetRoomTypes()
        {
            return db.RoomTypes.ToList();
        }

        /// <summary>
        /// Method to add a room to a hostel
        /// </summary>
        /// <param name="userInput">the form filled by the user</param>
        public void AddRoom(Room userInput)
        {
            db.Rooms.Add(userInput);
            db.SaveChanges();
        }
    }
}
