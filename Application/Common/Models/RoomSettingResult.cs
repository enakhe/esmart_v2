using ESMART.Domain.Entities.FrontDesk;
using ESMART.Domain.Entities.RoomSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Application.Common.Models
{
    public class RoomResult
    {
        internal RoomResult(bool succeeded, IEnumerable<string> errors, Room? data)
        {
            Succeeded = succeeded;
            Errors = [..errors];
            Response = data;
        }

        public bool Succeeded { get; init; }
        public string[] Errors { get; init; }
        public Room? Response { get; set; }

        public static RoomResult Success(Room data)
        {
            return new RoomResult(true, Array.Empty<string>(), data);
        }

        public static RoomResult Failure(IEnumerable<string> errors)
        {
            return new RoomResult(false, errors, null);
        }
    }

    public class RoomTypeResult
    {
        internal RoomTypeResult(bool succeeded, IEnumerable<string> errors, RoomType? data)
        {
            Succeeded = succeeded;
            Errors = [.. errors];
            Response = data;
        }

        public bool Succeeded { get; init; }
        public string[] Errors { get; init; }
        public RoomType? Response { get; set; }

        public static RoomTypeResult Success(RoomType data)
        {
            return new RoomTypeResult(true, Array.Empty<string>(), data);
        }

        public static RoomTypeResult Failure(IEnumerable<string> errors)
        {
            return new RoomTypeResult(false, errors, null);
        }
    }

    public class AreaResult
    {
        internal AreaResult(bool succeeded, IEnumerable<string> errors, Area? data)
        {
            Succeeded = succeeded;
            Errors = [.. errors];
            Response = data;
        }

        public bool Succeeded { get; init; }
        public string[] Errors { get; init; }
        public Area? Response { get; set; }

        public static AreaResult Success(Area data)
        {
            return new AreaResult(true, [], data);
        }

        public static AreaResult Failure(IEnumerable<string> errors)
        {
            return new AreaResult(false, errors, null);
        }
    }

    public class BuildingResult
    {
        internal BuildingResult(bool succeeded, IEnumerable<string> errors, Building? data)
        {
            Succeeded = succeeded;
            Errors = [.. errors];
            Response = data;
        }

        public bool Succeeded { get; init; }
        public string[] Errors { get; init; }
        public Building? Response { get; set; }

        public static BuildingResult Success(Building data)
        {
            return new BuildingResult(true, [], data);
        }

        public static BuildingResult Failure(IEnumerable<string> errors)
        {
            return new BuildingResult(false, errors, null);
        }
    }

    public class FloorResult
    {
        internal FloorResult(bool succeeded, IEnumerable<string> errors, Floor? data)
        {
            Succeeded = succeeded;
            Errors = [.. errors];
            Response = data;
        }

        public bool Succeeded { get; init; }
        public string[] Errors { get; init; }
        public Floor? Response { get; set; }

        public static FloorResult Success(Floor data)
        {
            return new FloorResult(true, [], data);
        }

        public static FloorResult Failure(IEnumerable<string> errors)
        {
            return new FloorResult(false, errors, null);
        }
    }
}
