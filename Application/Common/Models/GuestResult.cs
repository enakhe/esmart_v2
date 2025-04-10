using ESMART.Domain.Entities.FrontDesk;

namespace ESMART.Application.Common.Models
{
    public class GuestResult
    {
        internal GuestResult(bool succeeded, IEnumerable<string> errors, Guest data)
        {
            Succeeded = succeeded;
            Errors = errors.ToArray();
            Response = data;
        }

        public bool Succeeded { get; init; }
        public string[] Errors { get; init; }
        public Guest Response { get; set; }

        public static GuestResult Success(Guest data)
        {
            return new GuestResult(true, Array.Empty<string>(), data);
        }

        public static GuestResult Failure(IEnumerable<string> errors)
        {
            return new GuestResult(false, errors, null);
        }
    }

    public class GuestIdenityResult
    {
        internal GuestIdenityResult(bool succeeded, IEnumerable<string> errors, GuestIdentity data)
        {
            Succeeded = succeeded;
            Errors = errors.ToArray();
            Response = data;
        }

        public bool Succeeded { get; init; }
        public string[] Errors { get; init; }
        public GuestIdentity Response { get; set; }

        public static GuestIdenityResult Success(GuestIdentity data)
        {
            return new GuestIdenityResult(true, Array.Empty<string>(), data);
        }

        public static GuestIdenityResult Failure(IEnumerable<string> errors)
        {
            return new GuestIdenityResult(false, errors, null);
        }
    }
}
